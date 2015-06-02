#region MIT.
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
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Plugins
{
	/// <summary>
	/// A cache to hold plugin assemblies.
	/// </summary>
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
		private Lazy<GorgonPlugInVerifier> _verifier;
		#endregion

		#region Properties.
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
		private GorgonPlugInVerifier GetVerifier()
		{
			Type verifierType = typeof(GorgonPlugInVerifier);
			return (GorgonPlugInVerifier)(_discoveryDomain.Value.CreateInstanceFrom(verifierType.Assembly.Location, verifierType.FullName).Unwrap());
		}

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
		#endregion

		#region Methods.
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
		/// <returns>The assembly that was loaded.</returns>
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
					return result;
				}

				try
				{
					if (Interlocked.Increment(ref _getOrAddSync) == 1)
					{
						_log.Print("Loading plugin assembly '{0}' from {1}", LoggingLevel.Simple, assemblyName.FullName, assemblyName.EscapedCodeBase);

						_assemblies.Value[assemblyName.FullName] = result = Assembly.Load(assemblyName);
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

					try
					{
						if (Interlocked.Increment(ref _enumSync) == 1)
						{
							_assemblies.Value.Add(assembly.FullName, assembly);
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
		/// <param name="assemblyFile">Path to the file containing the plugins.</param>
		/// <returns>A read-only list of plugin names.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyFile"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the assemblyFile parameter is an empty string.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the assembly file could not be found.</exception>
		/// <remarks>
		/// <para>
		/// If the file pointed at by <see cref="assemblyFile"/> contains <see cref="GorgonPlugIn"/> types, then this method will retrieve a list of plugin names from that assembly. This method 
		/// can and should be used to determine if the plugin assembly actually contains any plugins, or to retrieve a catalog of available plugins.
		/// </para>
		/// <para>
		/// The assembly being enumerated is not loaded into the current application domain, and as such, is not cached. 
		/// </para>
		/// <para>
		/// If the file pointed by the <see cref="assemblyFile"/> parameter is not a .NET assembly, then an exception will be raised.
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

			// Function to load a list of type names from an assembly.
			return _verifier.Value.GetPlugInTypes(FindPluginAssembly(assemblyFile));
		}

		/// <summary>
		/// Function to determine if an assembly is a plugin assembly.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns><c>true</c> if this is a plugin assembly, <c>false</c> if it is not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is NULL (Nothing in VB.Net).</exception>
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
		/// Function to load an assembly holding plugins.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <returns>The loaded assembly.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyName"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		public Assembly Load(AssemblyName assemblyName)
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

#error Finish this method.  Comment this file properly.  Add appropriate overload for this method.  We don't need a return type here.

			return assembly;
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
			_verifier = new Lazy<GorgonPlugInVerifier>(GetVerifier);
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
