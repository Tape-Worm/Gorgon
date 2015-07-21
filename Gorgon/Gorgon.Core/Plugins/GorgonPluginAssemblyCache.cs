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
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Plugins
{
	/// <inheritdoc/>
	public sealed class GorgonPluginAssemblyCache
		: IGorgonPluginAssemblyCache
	{
		#region Constants.
		// Full name of the internal assembly builder for dynamic assemblies.
		private const string InternalAssemblyBuilder = "System.Reflection.Emit.InternalAssemblyBuilder";
		#endregion

		#region Variables.
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
		/// <inheritdoc/>
		public GorgonPluginPathCollection SearchPaths => _paths.Value;

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public IReadOnlyDictionary<string, Assembly> PluginAssemblies => _assemblies.Value;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to create any additional application domains we may need.
		/// </summary>
		/// <returns>The application domain.</returns>
		private AppDomain CreateAppDomain()
		{
			Evidence evidence = AppDomain.CurrentDomain.Evidence;
			AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;

			// Create our domain.
			_log.Print("Creating temporary application domain.", LoggingLevel.Intermediate);
			return AppDomain.CreateDomain("Gorgon.PlugIns.Discovery", evidence, setup);
		}

		/// <summary>
		/// Function to create the verification type from the application domain.
		/// </summary>
		/// <returns>The plugin verification object.</returns>
		private GorgonPluginVerifier GetVerifier()
		{
			Type verifierType = typeof(GorgonPluginVerifier);
			_log.Print("Creating plugin verifier...", LoggingLevel.Verbose);
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

			return resolver?.Invoke(AppDomain.CurrentDomain, args);
		}

		/// <summary>
		/// Function to either retrieve an existing assembly, or load one and add it to the cache.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly to load.</param>
		/// <param name="added"><b>true</b> if the assembly was added, <b>false</b> if not.</param>
		/// <returns>The assembly that was loaded, or <b>null</b> if that assembly does not contain any plugin types.</returns>
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
		/// <returns><b>true</b> if plugin types were found, <b>false</b> if not.</returns>
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
				throw new ArgumentNullException(nameof(pluginPath));
			}
			
			if (string.IsNullOrWhiteSpace(pluginPath))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(pluginPath));
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
				throw new FileNotFoundException(string.Format(Resources.GOR_ERR_PLUGIN_CANNOT_FIND_FILE,
					assemblyFile));
			}

			pathBuffer.Length = 0;
			pathBuffer.Append(pluginPath);
			pathBuffer.Append(assemblyFile);

			return AssemblyName.GetAssemblyName(pathBuffer.ToString());
		}

		/// <inheritdoc/>
		public AssemblySigningResult VerifyAssemblyStrongName(string assemblyPath, byte[] publicKey = null)
		{
			if ((string.IsNullOrWhiteSpace(assemblyPath)) || (!File.Exists(assemblyPath)))
			{
				return AssemblySigningResult.NotSigned;
			}

			Guid clrStrongNameClsId = new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92");
			Guid clrStrongNameriid = new Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D");

			var strongName = (IClrStrongName)RuntimeEnvironment.GetRuntimeInterfaceAsObject(clrStrongNameClsId, clrStrongNameriid);
			bool wasVerified;

			int result = strongName.StrongNameSignatureVerificationEx(assemblyPath, true, out wasVerified);

			if ((result != 0) || (!wasVerified))
			{
				return AssemblySigningResult.NotSigned;
			}

			if (publicKey == null)
			{
				return AssemblySigningResult.Signed;
			}

			AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
			byte[] compareToken = assemblyName.GetPublicKey();

			if ((compareToken == null) || (publicKey.Length != compareToken.Length) || (!publicKey.SequenceEqual(compareToken)))
			{
				return AssemblySigningResult.Signed | AssemblySigningResult.KeyMismatch;
			}

			return AssemblySigningResult.Signed;
		}

		/// <inheritdoc/>
		public IReadOnlyList<string> EnumeratePlugins(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException(nameof(assemblyName));
			}

			// Function to load a list of type names from an assembly.
			return _verifier.Value.GetPlugInTypes(assemblyName);
		}

		/// <inheritdoc/>
		public IReadOnlyList<string> EnumeratePlugins(string assemblyFile)
		{
			if (assemblyFile == null)
			{
				throw new ArgumentNullException(nameof(assemblyFile));
			}

			if (string.IsNullOrWhiteSpace(assemblyFile))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(assemblyFile));
			}

			return EnumeratePlugins(FindPluginAssembly(assemblyFile));
		}

		/// <inheritdoc/>
		public bool IsPluginAssembly(AssemblyName assemblyName)
		{
			bool result = false;

			if (assemblyName == null)
			{
				throw new ArgumentNullException(nameof(assemblyName));
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

		/// <inheritdoc/>
		public bool IsPluginAssembly(string assemblyPath)
		{
			if (assemblyPath == null)
			{
				throw new ArgumentNullException(nameof(assemblyPath));
			}

			if (string.IsNullOrEmpty(assemblyPath))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(assemblyPath));
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


		/// <inheritdoc/>
		public void Load(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException(nameof(assemblyName));
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
				throw new GorgonException(GorgonResult.InvalidFileFormat, string.Format(Resources.GOR_ERR_PLUGIN_NOT_PLUGIN_ASSEMBLY, assemblyName.FullName));
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

		/// <inheritdoc/>
		public AssemblyName Load(string assemblyPath)
		{
			if (assemblyPath == null)
			{
				throw new ArgumentNullException(nameof(assemblyPath));
			}

			if (string.IsNullOrWhiteSpace(assemblyPath))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(assemblyPath));
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
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This method must be called, or the application domain that is created to interrogate assembly types will live until the end of the application. This could lead to memory bloat or worse. 
		/// </para>
		/// <para>
		/// Because the application domain is unloaded on a separate thread, it may deadlock with the finalizer thread and thus we cannot count on the finalizer to clean evict the stale app domain on our 
		/// behalf.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Dispose()
		{
			if ((_discoveryDomain == null) || (_discoveryDomain.IsValueCreated))
			{
				return;
			}

			_log.Print("Unloading temporary application domain.", LoggingLevel.Intermediate);

			AssemblyResolver = null;
			_verifier = null;

			// App domains get unloaded on a separate thread. So we have to manually call dispose to unload the thread,
			// otherwise we'll get a deadlock if we try to do this in the finalizer thread.
			AppDomain.Unload(_discoveryDomain.Value);
			_discoveryDomain = null;
		}
		#endregion
	}
}
