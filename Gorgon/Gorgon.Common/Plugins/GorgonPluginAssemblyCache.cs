﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, June 23, 2011 11:22:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Plugins
{
	/// <summary>
	/// The return values for the <see cref="GorgonPlugInFactory.IsAssemblySigned(System.Reflection.AssemblyName,byte[])">IsAssemblySigned</see> method.
	/// </summary>
	[Flags]
	public enum PlugInSigningResult
	{
		/// <summary>
		/// Assembly is not signed.  This flag is mutally exclusive.
		/// </summary>
		NotSigned = 1,
		/// <summary>
		/// Assembly is signed, and if it was requested, the key matches.
		/// </summary>
		Signed = 2,
		/// <summary>
		/// This flag is combined with the Signed flag to indicate that it was signed, but the keys did not match.
		/// </summary>
		KeyMismatch = 4
	}

	/// <summary>
	/// A cache to hold plugin assemblies.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This assembly cache is meant to load/hold a list of plugin assemblies that contain types that implement the <see cref="GorgonPlugin"/> type and is 
	/// meant to be used in conjunction with the <see cref="GorgonPluginService"/> type.
	/// </para>
	/// <para>
	/// The cache attempts to ensure that the application only loads an assembly once during the lifetime of the application in order to cut down on 
	/// overhead and potential errors that can come up when multiple assemblies with the same qualified name are loaded into the same context.
	/// </para>
	/// <para>
	/// This object may use a separate app domain to interrogate a potential plugin assembly, and because of this, it is essential to call the <see cref="Dispose()"/> 
	/// method when shutting down this object.
	/// </para>
	/// <para>
	/// In some cases, a plugin assembly may have issues when loading an assembly. Such as a type not being found, or a type in the assembly refusing to instantiate. 
	/// In these cases use the <see cref="AssemblyResolver"/> property to assign a method that will attempt to resolve any dependency assemblies.
	/// </para>
	/// </remarks>
	public class GorgonPluginAssemblyCache
		: IDisposable
	{
		#region Constants.
		// Full name of the internal assembly builder for dynamic assemblies.
		private const string InternalAssemblyBuilder = "System.Reflection.Emit.InternalAssemblyBuilder";
		#endregion

		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Search paths for the plugin assemblies.
		private readonly Lazy<GorgonPluginPathCollection> _paths = new Lazy<GorgonPluginPathCollection>(() =>
		                                                                                                {
			                                                                                                var result = new GorgonPluginPathCollection();
			                                                                                                result.GetDefaultPaths();

			                                                                                                return result;
		                                                                                                });
		// List of loaded assemblies.
		private readonly static Lazy<Dictionary<string, Assembly>> _assemblies =
			new Lazy<Dictionary<string, Assembly>>(() => new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase));
		// Synchronization primitive for get-or-add method.
		private static int _getOrAddSync;
		// Enumeration synchronization.
		private static int _enumSync;
		// Application log file.
		private readonly IGorgonLog _log = new GorgonLogDummy();
		// A resolver function for errant assemblies.
		private Func<AppDomain, ResolveEventArgs, Assembly> _resolver;
		// An application domain used for plugin information discovery.
		private Lazy<AppDomain> _discoveryDomain;
		// Plug-in verifier.
		private Lazy<GorgonPluginVerifier> _verifier;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of search paths to use.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If an assembly cannot be found, then these paths are used to search for the assembly.
		/// </para>
		/// <para>By default, the plugin factory checks (in order):
		/// <list type="number">
		/// <item><description>The directory of the executable.</description></item>
		/// <item><description>The working directory of the executable.</description></item>
		/// <item><description>The system directory.</description></item>
		/// <item><description>The directories listed in the PATH environment variable.</description></item>
		/// </list>
		/// </para>
		/// <para>
		/// While the assembly cache attempts to be as thread-safe as possible, this collection is not.
		/// </para>
		/// </remarks>
		public GorgonPluginPathCollection SearchPaths
		{
			get
			{
				return _paths.Value;
			}
		}

		/// <summary>
		/// Property to set or return a fucntion that will be used to resolve plugin assembly dependencies.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property will intercept an event on the current application domain to resolve assembly dependencies as they are loaded.  This is necessary to handle issues where 
		/// types won't load or instantiate in an assembly at run time.  
		/// </para>
		/// <para>
		/// For example, if a custom type converter attribute is specified in a plugin assembly, it may not instantiate unless some assemblies are resolved at load time.  Setting 
		/// this property with a method that will look up assemblies in the current application domain will correct the issue.
		/// </para>
		/// </remarks>
		public Func<AppDomain, ResolveEventArgs, Assembly> AssemblyResolver
		{
			get
			{
				return _resolver;
			}
			set
			{
				AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolveEvent;

				_resolver = value;

				if (_resolver == null)
				{
					return;
				}

				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolveEvent;
			}
		}

		/// <summary>
		/// Property to return the list of cached plugin assemblies.
		/// </summary>
		public IReadOnlyDictionary<string, Assembly> PluginAssemblies
		{
			get
			{
				return _assemblies.Value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create any additional application domains we may need.
		/// </summary>
		/// <returns>The application domain.</returns>
		private static AppDomain CreateAppDomain()
		{
			Evidence evidence = AppDomain.CurrentDomain.Evidence;
			AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;

			// Create our domain.
			return AppDomain.CreateDomain("GorgonLibrary.PlugIns.Discovery", evidence, setup);
		}

		/// <summary>
		/// Function to create the verification type from the application domain.
		/// </summary>
		/// <returns>The plugin verification object.</returns>
		private GorgonPluginVerifier GetVerifier()
		{
			Type verifierType = typeof(GorgonPluginVerifier);
			return (GorgonPluginVerifier)(_discoveryDomain.Value.CreateInstanceFrom(verifierType.Assembly.Location, verifierType.FullName).Unwrap());
		}

		/// <summary>
		/// Handles the AssemblyResolveEvent event of the CurrentDomain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
		/// <returns>The assembly to use.</returns>
		private Assembly CurrentDomain_AssemblyResolveEvent(object sender, ResolveEventArgs args)
		{
			var resolver = _resolver;

			return resolver != null ? resolver(AppDomain.CurrentDomain, args) : null;
		}

		/// <summary>
		/// Function to either retrieve an existing assembly, or load one and add it to the cache.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <param name="added"><c>true</c> if the assembly was added, <c>false</c> if not.</param>
		/// <returns>The assembly that was loaded, or <c>null</c> if that assembly does not contain any plugin types.</returns>
		private Assembly GetOrAddAssembly(AssemblyName assemblyName, out bool added)
		{
			added = false;

			// We're doing this in place of using a concurrent dictionary since there's seemingly no good way to ensure that the factory method 
			// on GetOrAdd gets executed only once without using Lazy<T> (and we can't use that since we populate the dictionary in GetAssemblies 
			// with existing instances - Lazy<T> won't allow that). The problem is that if we call the factory method more than once, we could 
			// load the same assembly multiple times - which is a waste. We only want the assembly to load once, and only once.
			while (true)
			{
				Assembly result;

				if (_assemblies.Value.TryGetValue(assemblyName.FullName, out result))
				{
					return !HasPluginTypes(result) ? null : result;
				}

				try
				{
					if (Interlocked.Increment(ref _getOrAddSync) == 1)
					{
						_log.Print("Loading plugin assembly '{0}' from {1}", LoggingLevel.Simple, assemblyName.FullName, assemblyName.EscapedCodeBase);

						result = Assembly.Load(assemblyName);

						if (!HasPluginTypes(result))
						{
							return null;
						}

						_assemblies.Value.Add(assemblyName.FullName, result);
						added = true;

						return result;
					}
				}
				finally
				{
					Interlocked.Decrement(ref _getOrAddSync);
				}
			}
		}

		/// <summary>
		/// Function to determine if an assembly has any plugin types available.
		/// </summary>
		/// <param name="assembly">The assembly to evaluate.</param>
		/// <returns><c>true</c> if plugin types were found, <c>false</c> if not.</returns>
		private static bool HasPluginTypes(Assembly assembly)
		{
			Type plugInType = typeof(GorgonPlugin);
			Type[] types = assembly.GetTypes();

			return (from type in types
					where !type.IsPrimitive && !type.IsValueType && !type.IsAbstract && type.IsSubclassOf(plugInType)
					select type.FullName).Any();
		}

		/// <summary>
		/// Function to retrieve the currently loaded assemblies in the current app domain.
		/// </summary>
		private static void GetLoadedAssemblies()
		{
			// Weed out already cached assemblies and those that are involved in the creation of dynamic assemblies (they throw an exception).
			IEnumerable<Assembly> assemblies = from assembly in AppDomain.CurrentDomain.GetAssemblies()
											   where !(assembly is AssemblyBuilder)
													 && (!string.Equals(assembly.GetType().FullName,
																		InternalAssemblyBuilder,
																		StringComparison.OrdinalIgnoreCase))
											   select assembly;

			var checkedAssembly = new HashSet<string>(StringComparer.OrdinalIgnoreCase);  

			foreach (Assembly assembly in assemblies)
			{
				// Ensure that multiple threads do not trample our static dictionary cache.
				// We use a spin here to ensure that another thread will be able to wait and determine if its current assembly 
				// exists in the collection. We have set this up to atomically increment a value every time the thread needs to 
				// add a new value. This keeps another thread from adding a value until the dictionary is released by the previous 
				// thread. While not being entirely lock-free, this is close enough to work and should be safe.
				while (true)
				{
					if (_assemblies.Value.ContainsKey(assembly.FullName))
					{
						break;
					}

					if (checkedAssembly.Contains(assembly.FullName))
					{
						break;
					}

					try
					{
						if (Interlocked.Increment(ref _enumSync) == 1)
						{
							// We've already scanned this guy, so we're done.
							if (!checkedAssembly.Contains(assembly.FullName))
							{
								checkedAssembly.Add(assembly.FullName);
							}

							if (HasPluginTypes(assembly))
							{
								_assemblies.Value.Add(assembly.FullName, assembly);
							}
							break;
						}
					}
					finally
					{
						Interlocked.Decrement(ref _enumSync);
					}
				}
			}
		}

		/// <summary>
		/// Function to find a plugin assembly on a given path.
		/// </summary>
		/// <param name="pluginPath">Initial path to the plugin</param>
		/// <returns>The assembly name for the plugin assembly.</returns>
		private AssemblyName FindPluginAssembly(string pluginPath)
		{
			if (pluginPath == null)
			{
				throw new ArgumentNullException("pluginPath");
			}
			
			if (string.IsNullOrWhiteSpace(pluginPath))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "pluginPath");
			}

			pluginPath = Path.GetFullPath(pluginPath);

			if (string.IsNullOrWhiteSpace(pluginPath))
			{
				throw new FileNotFoundException();
			}

			// We've found the file, so retrieve its assembly name.
			if (File.Exists(pluginPath))
			{
				return AssemblyName.GetAssemblyName(pluginPath);
			}

			// We couldn't find the file, start looking through the paths.
			var assemblyFile = Path.GetFileName(pluginPath);

			var pathBuffer = new StringBuilder();

			pluginPath = SearchPaths.FirstOrDefault(path =>
			                                        {
				                                        pathBuffer.Length = 0;
				                                        pathBuffer.Append(path);
				                                        pathBuffer.Append(assemblyFile);

				                                        return File.Exists(pathBuffer.ToString());
			                                        });

			if (string.IsNullOrWhiteSpace(pluginPath))
			{
				throw new FileNotFoundException(string.Format(Resources.GOR_PLUGIN_CANNOT_FIND_FILE,
					assemblyFile));
			}

			pathBuffer.Length = 0;
			pathBuffer.Append(pluginPath);
			pathBuffer.Append(assemblyFile);

			return AssemblyName.GetAssemblyName(pathBuffer.ToString());
		}

		/// <summary>
		/// Function to enumerate all the plugin names from an assembly.
		/// </summary>
		/// <param name="assemblyName">The name of the assembly to enumerate from.</param>
		/// <returns>A read-only list of plugin names.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// If the file pointed at by <paramref name="assemblyName"/> contains <see cref="GorgonPlugin"/> types, then this method will retrieve a list of plugin names from that assembly. This method 
		/// can and should be used to determine if the plugin assembly actually contains any plugins, or to retrieve a catalog of available plugins.
		/// </para>
		/// <para>
		/// The assembly being enumerated is not loaded into the current application domain, and as such, is not cached. 
		/// </para>
		/// <para>
		/// Users should call <see cref="IsPluginAssembly(System.Reflection.AssemblyName)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		public IReadOnlyList<string> EnumeratePlugins(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}

			// Function to load a list of type names from an assembly.
			return _verifier.Value.GetPlugInTypes(assemblyName);
		}

		/// <summary>
		/// Function to enumerate all the plugin names from an assembly.
		/// </summary>
		/// <param name="assemblyFile">Path to the file containing the plugins.</param>
		/// <returns>A read-only list of plugin names.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyFile"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyFile parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the assembly file could not be found.</exception>
		/// <exception cref="BadImageFormatException">Thrown when the assembly pointed to by <paramref name="assemblyFile"/> is not a valid .NET assembly.</exception>
		/// <remarks>
		/// <para>
		/// If the file pointed at by <paramref name="assemblyFile"/> contains <see cref="GorgonPlugin"/> types, then this method will retrieve a list of plugin names from that assembly. This method 
		/// can and should be used to determine if the plugin assembly actually contains any plugins, or to retrieve a catalog of available plugins.
		/// </para>
		/// <para>
		/// The assembly being enumerated is not loaded into the current application domain, and as such, is not cached. 
		/// </para>
		/// <para>
		/// If the file pointed by the <paramref name="assemblyFile"/> parameter is not a .NET assembly, then an exception will be raised.
		/// </para>
		/// <para>
		/// Since this method takes a path to an assembly, the <see cref="SearchPaths"/> property will be used if the assembly could not be found 
		/// on the path specified.
		/// </para>
		/// <para>
		/// Users should call <see cref="IsPluginAssembly(System.String)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		public IReadOnlyList<string> EnumeratePlugins(string assemblyFile)
		{
			if (assemblyFile == null)
			{
				throw new ArgumentNullException("assemblyFile");
			}

			if (string.IsNullOrWhiteSpace(assemblyFile))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "assemblyFile");
			}

			return EnumeratePlugins(FindPluginAssembly(assemblyFile));
		}

		/// <summary>
		/// Function to determine if an assembly is a plugin assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns><c>true</c> if this is a plugin assembly, <c>false</c> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// This method will load the assembly into a separate <see cref="AppDomain"/> and will determine if it contains any types that inherit from 
		/// <see cref="GorgonPlugin"/>. If the assembly does not contain any plugin types, then this method returns <c>false</c>, otherwise it will 
		/// return <c>true</c>.
		/// </para>
		/// <para>
		/// Users should call this method before calling <see cref="Load(System.Reflection.AssemblyName)"/> to determine whether or not a plugin assembly should be loaded into the 
		/// application.
		/// </para>
		/// <para>
		/// Because this method loads the assembly into a separate application domain, the assembly will not be cached.
		/// </para>
		/// </remarks>
		public bool IsPluginAssembly(AssemblyName assemblyName)
		{
			bool result = false;

			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}

			try
			{
				result = _verifier.Value.IsPlugInAssembly(assemblyName);
			}
			catch (ReflectionTypeLoadException rex)
			{
				// In this case, we'll just return false and log the message.				
				_log.Print("Exception while determining if assembly is a plugin assembly:", LoggingLevel.Verbose);
				foreach (Exception loaderEx in rex.LoaderExceptions)
				{
					_log.Print("{0}", LoggingLevel.Verbose, loaderEx.Message);
				}
			}

			return result;
		}

		/// <summary>
		/// Function to determine if an assembly is a plugin assembly.
		/// </summary>
		/// <param name="assemblyPath">Path to the assembly file.</param>
		/// <returns><c>true</c> if this is a plugin assembly, <c>false</c> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyPath parameter is an empty string.</exception>
		/// <remarks>
		/// <para>
		/// This method will load the assembly into a separate <see cref="AppDomain"/> and will determine if it contains any types that inherit from 
		/// <see cref="GorgonPlugin"/>. If the assembly does not contain any plugin types, then this method returns <c>false</c>, otherwise it will 
		/// return <c>true</c>. 
		/// </para>
		/// <para>
		/// This method will also check to determine if the assembly is a valid .NET assembly. If it is not, then the method will return <c>false</c>, 
		/// otherwise it will return <c>true</c>.
		/// </para>
		/// <para>
		/// Users should call this method before calling <see cref="Load(System.String)"/> to determine whether or not a plugin assembly should be loaded into the 
		/// application.
		/// </para>
		/// <para>
		/// Because this method loads the assembly into a separate application domain, the assembly will not be cached.
		/// </para>
		/// <para>
		/// Since this method takes a path to an assembly, the <see cref="SearchPaths"/> property will be used if the assembly could not be found 
		/// on the path specified.
		/// </para>
		/// </remarks>
		public bool IsPluginAssembly(string assemblyPath)
		{
			if (assemblyPath == null)
			{
				throw new ArgumentNullException("assemblyPath");
			}

			if (string.IsNullOrEmpty(assemblyPath))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "assemblyPath");
			}

			AssemblyName assemblyName;

			try
			{
				assemblyName = FindPluginAssembly(assemblyPath);
			}
			catch (BadImageFormatException)
			{
				// If a DLL/EXE is not a .NET assembly, then it will throw BadImageFormatException.
				// Catch it here and assume it's not going to load.
				return false;
			}

			return IsPluginAssembly(assemblyName);
		}


		/// <summary>
		/// Function to load an assembly that contains <see cref="GorgonPlugin"/> types.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the assembly does not contain any types that inherit from <see cref="GorgonPlugin"/>.</exception>
		/// <remarks>
		/// <para>
		/// This method will load an assembly that contains <see cref="GorgonPlugin"/> types. If the assembly does not contain any types that inherit from 
		/// <see cref="GorgonPlugin"/>, then an exception will be raised.
		/// </para>
		/// <para>
		/// Users should call <see cref="IsPluginAssembly(System.Reflection.AssemblyName)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		public void Load(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}

			// Initialize the cached assemblies.
			if (!_assemblies.IsValueCreated)
			{
				GetLoadedAssemblies();
			}

			bool newAssembly;
			Assembly assembly = GetOrAddAssembly(assemblyName, out newAssembly);

			if (assembly == null)
			{
				throw new GorgonException(GorgonResult.InvalidFileFormat, string.Format(Resources.GOR_PLUGIN_NOT_PLUGIN_ASSEMBLY, assemblyName.FullName));
			}

			if (!newAssembly)
			{
				_log.Print("Plug-in assembly '{0}' from {1} is already loaded.",
				           LoggingLevel.Simple,
				           assemblyName.FullName,
				           assemblyName.EscapedCodeBase);
			}
			else
			{
				_log.Print("Plug-in assembly '{0}' loaded successfully.",
				           LoggingLevel.Simple,
				           assemblyName.FullName);
			}
		}

		/// <summary>
		/// Function to load an assembly that contains <see cref="GorgonPlugin"/> types.
		/// </summary>
		/// <param name="assemblyPath">Name of the assembly to load.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyPath"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="assemblyPath"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the assembly does not contain any types that inherit from <see cref="GorgonPlugin"/>.</exception>
		/// <exception cref="BadImageFormatException">Thrown when the assembly pointed to by <paramref name="assemblyPath"/> is not a valid .NET assembly.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the assembly file could not be found.</exception>
		/// <returns>A <see cref="System.Reflection.AssemblyName"/> type containing the qualified name of the assembly that was loaded.</returns>
		/// <remarks>
		/// <para>
		/// This method will load an assembly that contains <see cref="GorgonPlugin"/> types. If the assembly does not contain any types that inherit from 
		/// <see cref="GorgonPlugin"/>, then an exception will be raised.
		/// </para>
		/// <para>
		/// Users should call <see cref="IsPluginAssembly(System.String)"/> prior to this method in order to determine whether the assembly 
		/// can be loaded or not.
		/// </para>
		/// </remarks>
		public AssemblyName Load(string assemblyPath)
		{
			if (assemblyPath == null)
			{
				throw new ArgumentNullException("assemblyPath");
			}

			if (string.IsNullOrWhiteSpace(assemblyPath))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "assemblyPath");
			}

			AssemblyName assemblyName = FindPluginAssembly(assemblyPath);

			Load(assemblyName);

			return assemblyName;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPluginAssemblyCache"/> class.
		/// </summary>
		/// <param name="log">[Optional] The application log file to use.</param>
		public GorgonPluginAssemblyCache(IGorgonLog log = null)
		{
			if (log != null)
			{
				_log = log;
			}

			_discoveryDomain = new Lazy<AppDomain>(CreateAppDomain);
			_verifier = new Lazy<GorgonPluginVerifier>(GetVerifier);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonPluginAssemblyCache"/> class.
		/// </summary>
		~GorgonPluginAssemblyCache()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_verifier = null;

			if (disposing)
			{
				AssemblyResolver = null;

				if ((_discoveryDomain != null) && (_discoveryDomain.IsValueCreated))
				{
					AppDomain.Unload(_discoveryDomain.Value);
				}
			}
			
			_discoveryDomain = null;
			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}