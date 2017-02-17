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
// Created: Thursday, June 23, 2011 11:23:18 AM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.Reflection;

namespace Gorgon.Plugins
{
	/// <summary>
	/// A service to create, cache and return <see cref="GorgonPlugin"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This service object is meant to instantiate, and cache instances of <see cref="GorgonPlugin"/> objects contained within external assemblies loaded by the <see cref="GorgonPluginAssemblyCache"/>. 
	/// It also allows the user to unload plugin instances when necessary.
	/// </para>
	/// <para>
	/// A plugin can be any class within an assembly that inherits from the <see cref="GorgonPlugin"/> base object. When the service is created, it will retrieve a list of all known plugins types that exist 
	/// in previously loaded plugin assemblies (this list can also be updated with the <see cref="ScanPlugins"/> method). Plugins are not created until they are requested from the service via the 
	/// <see cref="GetPlugin{T}"/> or <see cref="GetPlugins{T}"/> methods. When these methods are called, they will instantiate the plugin type, and cache it for quick retrieval on subsequent calls to the 
	/// methods.
	/// </para>
	/// <note type="tip">
	/// A plugin assembly may contain many or one plugin type, otherwise it is not considered when enumerating plugin types.
	/// </note>
	/// <para>
	/// <para>
	/// <h3>Defining your own plugin</h3>
	/// While any class can be a plugin within an assembly, Gorgon uses the following strategy to define a plugin assembly.
	/// </para>
	/// <h3>In your host assembly (an application, DLL, etc...):</h3>
	/// <code language="csharp">
	/// <![CDATA[
	/// // This will go into your host assembly (e.g. an application, another DLL, etc...)
	/// // This defines the functionality that you wish to override in your plugin assembly.
	/// public abstract class FunctionalityBase
	/// {
	///		public abstract int DoSomething();
	/// }
	/// 
	/// // This too will go into the host assembly and be overridden in your plugin assembly.
	/// public abstract class FunctionalityPlugin
	///		: GorgonPlugin
	/// {
	///		public abstract FunctionalityBase GetNewFunctionality();
	/// 
	///		protected FunctionalityPlugin(string description)
	///		{
	///		}
	/// }
	///	]]>
	/// </code>
	/// <h3>In your plugin assembly:</h3>
	/// <note type="tip">
	/// Be sure to reference your host assembly in the plugin assembly project.
	/// </note>
	/// <code language="csharp">
	/// <![CDATA[
	/// // We put the namespace here because when loading the plugin in our example below, we need to give a fully qualified name for the type that we're loading.
	/// namespace Fully.Qualified.Name
	/// {
	///		// Typically Gorgon makes the extension classes internal, but they can have a public accessor if you wish.
	///		class ConcreteFunctionality
	///			: FunctionalityBase
	///		{
	///			public override int DoSomething()
	///			{
	///				return 42;
	///			}
	///		}
	/// 
	///		public class ConcreteFunctionalityPlugin
	///			: FunctionalityPlugin
	///		{
	///			public override FunctionalityBase GetNewFunctionality()
	///			{
	///				return new ConcreteFunctionality();
	///			}
	/// 
	///			public ConcreteFunctionalityPlugin()
	///				: base("What is the answer to life, the universe, and blah blah blah?")
	///			{
	///			}
	///		}
	/// }
	/// ]]>
	/// </code>  
	/// </para>
	/// </remarks>
	/// <example>
	/// This example shows how to load a plugin and get its plugin instance. It will use the <c>ConcreteFunctionalityPlugin</c> above:
	/// <code language="csharp"> 
	/// <![CDATA[
	/// // Our base functionality.
	/// private FunctionalityBase _functionality;
	/// 
	/// void LoadFunctionality()
	/// {
	///		using (IGorgonPluginAssemblyCache assemblies = new GorgonPluginAssemblyCache())
	///		{	
	///			// For brevity, we've omitted checking to see if the assembly is valid and such.
	///			// In the real world, you should always determine whether the assembly can be loaded 
	///			// before calling the Load method.
	///			assemblies.Load("ConcreteFunctionality.dll");
	/// 			
	///			IGorgonPluginService pluginService = new GorgonPluginService(assemblies);
	/// 
	///			_functionality = pluginService.GetPlugin<FunctionalityBase>("Fully.Qualified.Name.ConcreteFunctionalityPlugin"); 
	///		}
	/// }
	/// 
	/// void Main()
	/// {
	///		LoadFunctionality();
	///		
	///		Console.WriteLine("The ultimate answer and stuff: {0}", _functionality.DoSomething());
	/// }
	/// ]]>
	/// </code>
	/// </example>
	public sealed class GorgonPluginService 
		: IGorgonPluginService
	{
		#region Variables.
		// List of previously loaded plugins.
		private readonly Lazy<GorgonNamedObjectDictionary<GorgonPlugin>> _loadedPlugins;
		// List of plugin constructors.
		private readonly Lazy<ConcurrentDictionary<string, ObjectActivator<GorgonPlugin>>> _constructors;
		// List of constructors from a specific assembly.
		private readonly Lazy<ConcurrentDictionary<string, Lazy<ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>>>> _assemblyConstructors;
		// The application log file.
		private readonly IGorgonLog _log = GorgonLogDummy.DefaultInstance;
		// Thread synchronization for the plugin dictionary.
		private int _pluginSync;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the plugin assembly cache this service is using.
		/// </summary>
		public GorgonPluginAssemblyCache PluginAssemblyCache
		{
			get;
		}


		/// <summary>
		/// Property to return the number of plugins that are currently loaded in this service.
		/// </summary>
		public int LoadedPluginCount => !_loadedPlugins.IsValueCreated ? 0 : _loadedPlugins.Value.Count;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to create or retrieve a list of plugins.
		/// </summary>
		/// <typeparam name="T">Type of plugin.</typeparam>
		/// <param name="pluginConstructors">A list of plugin constructors.</param>
		/// <returns>An array of plugins created or retrieved.</returns>
		private T[] GetPluginsFromConstructors<T>(ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>> pluginConstructors)
		{
			var result = new ConcurrentBag<GorgonPlugin>();
			Type typeT = typeof(T);

			// Match the types with our generic type. The type in the plugin must be an exact match with T, or a subclass of T.
			foreach (KeyValuePair<Type, ObjectActivator<GorgonPlugin>> constructor in
				pluginConstructors.Where(item => typeT == item.Key || item.Key.IsSubclassOf(typeT)))
			{
				GorgonPlugin plugin;

				while (true)
				{
					if (_loadedPlugins.Value.TryGetValue(constructor.Key.FullName, out plugin))
					{
						_log.Print("Found existing plugin '{0}'.", LoggingLevel.Simple, constructor.Key.FullName);
						break;
					}

					try
					{
						if (Interlocked.Increment(ref _pluginSync) > 1)
						{
							continue;
						}

						_log.Print("Creating plugin '{0}'.", LoggingLevel.Simple, constructor.Key.FullName);

						plugin = constructor.Value();

						_loadedPlugins.Value.Add(plugin);

						_log.Print("Plugin '{0}' instance added to registry.", LoggingLevel.Simple, constructor.Key.FullName);
						break;
					}
					finally
					{
						Interlocked.Decrement(ref _pluginSync);
					}
				}

				if (plugin != null)
				{
					result.Add(plugin);
				}
			}

			return result.Cast<T>().ToArray();
		}

		/// <summary>
		/// Function to retrieve all the plugins of the specified type from the currently loaded plugin assemblies.
		/// </summary>
		/// <typeparam name="T">Type of plugin to filter.</typeparam>
		/// <returns>The list of all the plugins.</returns>
		private IReadOnlyList<T> GetAllPlugins<T>()
			where T : GorgonPlugin
		{
			if (_assemblyConstructors.Value.Count == 0)
			{
				return new T[0];
			}

			List<T> result = new List<T>();

			foreach (var item in _assemblyConstructors.Value)
			{
				result.AddRange(GetPluginsFromConstructors<T>(item.Value.Value));
			}

			return result;
		}

		/// <summary>
		/// Function to retrieve the list of plugins from a given assembly.
		/// </summary>
		/// <typeparam name="T">Type of plugin to retrieve. Must implement <see cref="GorgonPlugin"/>.</typeparam>
		/// <param name="assemblyName">[Optional] The name of the assembly associated with the plugins.</param>
		/// <returns>A list of plugins from the assembly.</returns>
		/// <remarks>
		/// This will retrieve all the plugins from the plugin service of the type <typeparamref name="T"/>. If the <paramref name="assemblyName"/> parameter is not <b>null</b>, then, 
		/// the only the assembly with that name will be scanned for the plugin type.
		/// </remarks>
		public IReadOnlyList<T> GetPlugins<T>(AssemblyName assemblyName = null)
			where T : GorgonPlugin
		{
			if (!_assemblyConstructors.IsValueCreated)
			{
				ScanPlugins();
			}

			if (assemblyName == null)
			{
				return GetAllPlugins<T>();
			}

			Lazy<ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>> pluginConstructors;

			return !_assemblyConstructors.Value.TryGetValue(assemblyName.FullName, out pluginConstructors)
				       ? new T[0]
				       : GetPluginsFromConstructors<T>(pluginConstructors.Value);
		}

		/// <summary>
		/// Function to retrieve a plugin by its fully qualified type name.
		/// </summary>
		/// <typeparam name="T">The base type of the plugin. Must implement <see cref="GorgonPlugin"/>.</typeparam>
		/// <param name="pluginName">Fully qualified type name of the plugin to find.</param>
		/// <returns>The plugin, if found, or <b>null</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginName"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="pluginName"/> is empty.</exception>
		public T GetPlugin<T>(string pluginName)
			where T : GorgonPlugin
		{
			if (pluginName == null)
			{
				throw new ArgumentNullException(nameof(pluginName));
			}

			if (string.IsNullOrWhiteSpace(pluginName))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(pluginName));
			}

			if (!_constructors.IsValueCreated)
			{
				ScanPlugins();
			}
			
			while (true)
			{
				GorgonPlugin plugin;

				// We haven't created this plugin yet, so create it.
				if (_loadedPlugins.Value.TryGetValue(pluginName, out plugin))
				{
					_log.Print("Found existing plugin '{0}'.", LoggingLevel.Simple, pluginName);
					return (T)plugin;
				}

				try
				{
					if (Interlocked.Increment(ref _pluginSync) > 1)
					{
						continue;
					}

					_log.Print("Creating plugin '{0}'.", LoggingLevel.Simple, pluginName);

					ObjectActivator<GorgonPlugin> constructor;

					if (!_constructors.Value.TryGetValue(pluginName, out constructor))
					{
						_log.Print("Plugin '{0}' does not exist.", LoggingLevel.Simple, pluginName);
						return null;
					}
					
					T typedPlugin = (T)constructor();
					
					_loadedPlugins.Value.Add(typedPlugin);

					_log.Print("Plugin '{0}' instance added to registry.", LoggingLevel.Simple, pluginName);
					return typedPlugin;
				}
				finally
				{
					Interlocked.Decrement(ref _pluginSync);
				}
			}
		}

		/// <summary>
		/// Function to retrieve a list of names for available plugins.
		/// </summary>
		/// <param name="assemblyName">[Optional] Name of the assembly containing the plugins.</param>
		/// <returns>A list of names for the available plugins.</returns>
		/// <remarks>
		/// <para>
		/// This method will retrieve a list of fully qualified type names for plugins contained within the <see cref="GorgonPluginAssemblyCache"/> passed to this object. This list is 
		/// not indicative of whether the type has been created or not.
		/// </para>
		/// <para>
		/// The <paramref name="assemblyName"/> parameter, when not <b>null</b>, will return only plugin names belonging to that assembly. 
		/// If the assembly is not loaded, then an exception is thrown.
		/// </para>
		/// </remarks>
		public IReadOnlyList<string> GetPluginNames(AssemblyName assemblyName = null)
		{
			if ((!_constructors.IsValueCreated) || (!_assemblyConstructors.IsValueCreated))
			{
				ScanPlugins();
			}

			if (assemblyName == null)
			{
				return _constructors.Value.Keys.ToArray();
			}

			Lazy<ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>> pluginConstructors;

			if (!_assemblyConstructors.Value.TryGetValue(assemblyName.FullName, out pluginConstructors))
			{
				throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_PLUGIN_ASSEMBLY_NOT_FOUND, assemblyName.FullName));
			}

			return pluginConstructors.Value.Keys.Select(item => item.FullName).ToArray();
		}

		/// <summary>
		/// Function to scan for plugins in the loaded plugin assemblies that are cached in the <see cref="GorgonPluginAssemblyCache"/> passed to this object.
		/// </summary>
		/// <remarks>
		/// This method will unload any active plugins, and, if implemented, call the dispose method for any plugin.
		/// </remarks>
		public void ScanPlugins()
		{
			// Get rid of any plugins that are instanced.
			UnloadAll();

			// Clear out our type constructors.
			_constructors.Value.Clear();
			_assemblyConstructors.Value.Clear();

			_log.Print("Scanning for plugins...", LoggingLevel.Simple);

			if (PluginAssemblyCache.PluginAssemblies.Count == 0)
			{
				_log.Print("0 plugins found in the assembly cache.", LoggingLevel.Simple);
				return;
			}

			foreach (KeyValuePair<string, Assembly> assemblyItem in PluginAssemblyCache.PluginAssemblies)
			{
				try
				{
					IEnumerable<Type> types = from type in assemblyItem.Value.GetTypes()
					                          where type.IsSubclassOf(typeof(GorgonPlugin))
					                                && !type.IsAbstract && !type.IsPrimitive && !type.IsValueType
													&& type.GetCustomAttribute<ExcludeAsPluginAttribute>() == null
					                          select type;

					// Build another view of the constructor list that allows us to segregate by assembly.
					var assemblyConstructors = _assemblyConstructors.Value.GetOrAdd(assemblyItem.Value.GetName().FullName,
					                                                                new Lazy<ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>>(
						                                                                () => new ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>(), true));

					foreach (Type pluginType in types)
					{
						ObjectActivator<GorgonPlugin> activator = pluginType.CreateActivator<GorgonPlugin>(null);

						if (activator == null)
						{
							throw new GorgonException(GorgonResult.CannotCreate,
							                          string.Format(Resources.GOR_ERR_PLUGIN_CANNOT_CREATE, pluginType.FullName, pluginType.Assembly.FullName, typeof(GorgonPlugin).FullName));
						}

						_log.Print("Found plugin '{0}' in the assembly '{1}'.", LoggingLevel.Verbose, pluginType.FullName, assemblyItem.Value.FullName);
						_constructors.Value.TryAdd(pluginType.FullName, activator);
						assemblyConstructors.Value.TryAdd(pluginType, activator);
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					var errorMessage = new StringBuilder(512);

					foreach (Exception loadEx in ex.LoaderExceptions)
					{
						if (errorMessage.Length > 0)
						{
							errorMessage.Append("\n\r");
						}

						errorMessage.Append(loadEx.Message);
					}

					throw new GorgonException(GorgonResult.CannotEnumerate,
					                          string.Format(Resources.GOR_ERR_PLUGIN_TYPE_LOAD_FAILURE,
					                                        assemblyItem.Key,
					                                        errorMessage));
				}
			}

			_log.Print("{0} plugins found in the assembly cache.", LoggingLevel.Simple, _constructors.Value.Count);
		}

		/// <summary>
		/// Function to unload all the plugins.
		/// </summary>
		public void UnloadAll()
		{
			if ((!_loadedPlugins.IsValueCreated) || (_loadedPlugins.Value.Count == 0))
			{
				return;
			}

			_log.Print("Unloading all plugins.", LoggingLevel.Simple);

			IEnumerable<IDisposable> disposable = from plugin in _loadedPlugins.Value
			                                      // ReSharper disable once SuspiciousTypeConversion.Global
			                                      let disposer = plugin as IDisposable
			                                      where disposer != null
			                                      select disposer;

			foreach (IDisposable disposedPlugin in disposable)
			{
				disposedPlugin.Dispose();	
			}

			_loadedPlugins.Value.Clear();
		}

		/// <summary>
		/// Function to unload a plugin by its name.
		/// </summary>
		/// <param name="name">Fully qualified type name of the plugin to remove.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="name"/> parameter was <b>null</b>.</exception>
		/// <exception cref="ArgumentException">The <paramref name="name "/> parameter was an empty string.</exception>
		/// <returns><b>true</b> if the plugin was unloaded successfully, <b>false</b> if it did not exist in the collection, or failed to unload.</returns>
		public bool Unload(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			while (true)
			{
				GorgonPlugin plugin;

				if (!_loadedPlugins.Value.TryGetValue(name, out plugin))
				{
					_log.Print("Plugin '{0}' was not found, it may not have been created yet.", LoggingLevel.Simple, name);
					return false;
				}

				try
				{
					if (Interlocked.Increment(ref _pluginSync) > 1)
					{
						continue;
					}

					// ReSharper disable once SuspiciousTypeConversion.Global
					var disposer = plugin as IDisposable;

					disposer?.Dispose();

					_loadedPlugins.Value.Remove(plugin);

					_log.Print("Plugin '{0}' removed from the active plugin registry.", LoggingLevel.Simple, name);
					return true;
				}
				finally
				{
					Interlocked.Decrement(ref _pluginSync);
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPluginService"/> class.
		/// </summary>
		/// <param name="assemblyCache">A <see cref="GorgonPluginAssemblyCache"/> that will contain assemblies with plugin types.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblyCache"/> is <b>null</b>.</exception>
		public GorgonPluginService(GorgonPluginAssemblyCache assemblyCache, IGorgonLog log = null)
		{
			if (assemblyCache == null)
			{
				throw new ArgumentNullException(nameof(assemblyCache));
			}

			PluginAssemblyCache = assemblyCache;

			if (log != null)
			{
				_log = log;
			}

			_loadedPlugins = new Lazy<GorgonNamedObjectDictionary<GorgonPlugin>>(() => new GorgonNamedObjectDictionary<GorgonPlugin>(false), true);
			_constructors = new Lazy<ConcurrentDictionary<string, ObjectActivator<GorgonPlugin>>>(
				() => new ConcurrentDictionary<string, ObjectActivator<GorgonPlugin>>(StringComparer.OrdinalIgnoreCase), true);
			_assemblyConstructors = new Lazy<ConcurrentDictionary<string, Lazy<ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>>>>(
				() => new ConcurrentDictionary<string, Lazy<ConcurrentDictionary<Type, ObjectActivator<GorgonPlugin>>>>(), true);
		}
		#endregion
	}
}
