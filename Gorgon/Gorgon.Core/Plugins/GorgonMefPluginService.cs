#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: March 25, 2018 3:01:41 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Properties;

namespace Gorgon.PlugIns
{
    /// <summary>
    /// A service to create, cache and return <see cref="GorgonPlugIn"/> instances by using the built in Microsoft Extensibility Framework as its provider.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service object is meant to instantiate, and cache instances of <see cref="GorgonPlugIn"/> objects contained within external assemblies loaded by the <see cref="GorgonMefPlugInService"/>. 
    /// It also allows the user to unload plugin instances when necessary.
    /// </para>
    /// <para>
    /// A plugin can be any class within an assembly that inherits from the <see cref="GorgonPlugIn"/> base object. When the service is created, it will retrieve a list of all known plugins types that exist 
    /// in previously loaded plugin assemblies (this list can also be updated with the <see cref="ScanPlugIns"/> method). PlugIns are not created until they are requested from the service via the 
    /// <see cref="GetPlugIn{T}"/> or <see cref="GetPlugIns{T}"/> methods. When these methods are called, they will instantiate the plugin type, and cache it for quick retrieval on subsequent calls to the 
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
    /// public abstract class FunctionalityPlugIn
    ///		: GorgonPlugIn
    /// {
    ///		public abstract FunctionalityBase GetNewFunctionality();
    /// 
    ///		protected FunctionalityPlugIn(string description)
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
    ///		public class ConcreteFunctionalityPlugIn
    ///			: FunctionalityPlugIn
    ///		{
    ///			public override FunctionalityBase GetNewFunctionality()
    ///			{
    ///				return new ConcreteFunctionality();
    ///			}
    /// 
    ///			public ConcreteFunctionalityPlugIn()
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
    /// This example shows how to load a plugin and get its plugin instance. It will use the <c>ConcreteFunctionalityPlugIn</c> above:
    /// <code language="csharp"> 
    /// <![CDATA[
    /// // Our base functionality.
    /// private FunctionalityBase _functionality;
    /// private GorgonMefPlugInCache _assemblies;
    /// 
    /// void LoadFunctionality()
    /// {
    ///		assemblies = new GorgonMefPlugInCache();
    ///		
    ///		// For brevity, we've omitted checking to see if the assembly is valid and such.
    ///		// In the real world, you should always determine whether the assembly can be loaded 
    ///		// before calling the Load method.
    ///		_assemblies.LoadPlugInAssemblies("Your\Directory\Here", "file search pattern");  // You can pass a wild card like *.dll, *.exe, etc..., or an absolute file name like "MyPlugin.dll".
    /// 			
    ///		IGorgonPlugInService pluginService = new GorgonMefPlugInService(_assemblies);
    /// 
    ///		_functionality = pluginService.GetPlugIn<FunctionalityBase>("Fully.Qualified.Name.ConcreteFunctionalityPlugIn"); 
    /// }
    /// 
    /// void Main()
    /// {
    ///		LoadFunctionality();
    ///		
    ///		Console.WriteLine("The ultimate answer and stuff: {0}", _functionality.DoSomething());
    ///		
    ///     _assemblies?.Dispose();
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public sealed class GorgonMefPlugInService
        : IGorgonPlugInService
    {
        #region Variables.
        // The MEF plugin assembly cache.
        private readonly GorgonMefPlugInCache _cache;
        // The application log file.
        private readonly IGorgonLog _log;
        // List of previously loaded plugins.
        private readonly ConcurrentDictionary<string, Lazy<GorgonPlugIn, IDictionary<string, object>>> _loadedPlugIns = new ConcurrentDictionary<string, Lazy<GorgonPlugIn, IDictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);
        // Flag to indicate whether or not the plugins have been scanned.
        private int _scanned;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the number of plugins that are currently loaded in this service.
        /// </summary>
        public int LoadedPlugInCount => _loadedPlugIns.Count;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to unload a plugin by its name.
        /// </summary>
        /// <param name="plugin">The plugin to remove.</param>
        private void DisposePlugIn(GorgonPlugIn plugin)
        {
            if (!(plugin is IDisposable disposable))
            {
                return;
            }

            disposable.Dispose();
            _log.Print($"PlugIn '{plugin.Name}' disposed.", LoggingLevel.Verbose);
        }

        /// <summary>
        /// Function to retrieve a plugin by its fully qualified type name.
        /// </summary>
        /// <typeparam name="T">The base type of the plugin. Must implement <see cref="GorgonPlugIn"/>.</typeparam>
        /// <param name="pluginName">Fully qualified type name of the plugin to find.</param>
        /// <returns>The plugin, if found, or <b>null</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="pluginName"/> is empty.</exception>
        public T GetPlugIn<T>(string pluginName) where T : GorgonPlugIn
        {
            if (pluginName == null)
            {
                throw new ArgumentNullException(nameof(pluginName));
            }

            if (string.IsNullOrWhiteSpace(pluginName))
            {
                throw new ArgumentEmptyException(nameof(pluginName));
            }

            if (_scanned == 0)
            {
                ScanPlugIns();
            }

            return !_loadedPlugIns.TryGetValue(pluginName, out Lazy<GorgonPlugIn, IDictionary<string, object>> plugin) ? null : plugin.Value as T;
        }

        /// <summary>
        /// Function to retrieve a list of names for available plugins.
        /// </summary>
        /// <param name="assemblyName">[Optional] Name of the assembly containing the plugins.</param>
        /// <returns>A list of names for the available plugins.</returns>
        /// <remarks>
        /// <para>
        /// This method will retrieve a list of fully qualified type names for plugins contained within the <see cref="GorgonMefPlugInCache"/> passed to this object. This list is 
        /// not indicative of whether the type has been created or not.
        /// </para>
        /// <para>
        /// The <paramref name="assemblyName"/> parameter, when not <b>null</b>, will return only plugin names belonging to that assembly. 
        /// If the assembly is not loaded, then an exception is thrown.
        /// </para>
        /// </remarks>
        public IReadOnlyList<string> GetPlugInNames(AssemblyName assemblyName = null)
        {
            if (_scanned == 0)
            {
                ScanPlugIns();
            }

            if (assemblyName == null)
            {
                return _loadedPlugIns.Keys.ToArray();
            }

            return _loadedPlugIns.Where(item =>
                                        {
                                            Debug.Assert(item.Value.Metadata.ContainsKey("Assembly"), "Assembly info not found.");

                                            var name = item.Value.Metadata["Assembly"] as AssemblyName;

                                            Debug.Assert(name != null, "Assembly name is null.");

                                            return AssemblyName.ReferenceMatchesDefinition(name, assemblyName);
                                        })
                                 .Select(item => item.Key)
                                 .ToArray();
        }

        /// <summary>
        /// Function to retrieve the list of plugins from a given assembly.
        /// </summary>
        /// <typeparam name="T">Type of plugin to retrieve. Must implement <see cref="GorgonPlugIn"/>.</typeparam>
        /// <param name="assemblyName">[Optional] The name of the assembly associated with the plugins.</param>
        /// <returns>A list of plugins from the assembly.</returns>
        /// <remarks>
        /// This will retrieve all the plugins from the plugin service of the type <typeparamref name="T"/>. If the <paramref name="assemblyName"/> parameter is not <b>null</b>, then, 
        /// the only the assembly with that name will be scanned for the plugin type.
        /// </remarks>
        public IReadOnlyList<T> GetPlugIns<T>(AssemblyName assemblyName = null)
            where T : GorgonPlugIn
        {
            if (_scanned == 0)
            {
                ScanPlugIns();
            }

            if (assemblyName == null)
            {
                return _loadedPlugIns.Values.Select(item => item.Value).OfType<T>().ToArray();
            }

            return _loadedPlugIns.Where(item =>
                                        {
                                            Debug.Assert(item.Value.Metadata.ContainsKey("Assembly"), "Assembly info not found.");

                                            var name = item.Value.Metadata["Assembly"] as AssemblyName;

                                            Debug.Assert(name != null, "Assembly name is null.");

                                            return AssemblyName.ReferenceMatchesDefinition(name, assemblyName);
                                        })
                                 .Select(item => item.Value.Value)
                                 .OfType<T>()
                                 .ToArray();
        }

        /// <summary>
        /// Function to scan for plugins in the loaded plugin assemblies that are cached in the <see cref="GorgonMefPlugInCache"/> passed to this object.
        /// </summary>
        /// <remarks>
        /// This method will unload any active plugins, and, if implemented, call the dispose method for any plugin.
        /// </remarks>
        public void ScanPlugIns()
        {
            try
            {
                while (true)
                {
                    if (Interlocked.Exchange(ref _scanned, 1) == 1)
                    {
                        var wait = new SpinWait();
                        wait.SpinOnce();
                        continue;
                    }

                    UnloadAll();
                    _log.Print("Scanning cached assemblies for available plug ins...", LoggingLevel.Intermediate);

                    _cache.Refresh();
                    IEnumerable<Lazy<GorgonPlugIn, IDictionary<string, object>>> plugins = _cache.EnumeratePlugIns();

                    foreach (Lazy<GorgonPlugIn, IDictionary<string, object>> plugin in plugins)
                    {
                        Debug.Assert(plugin.Metadata.ContainsKey("Name"), "Name metadata not found.");

                        string name = plugin.Metadata["Name"]?.ToString();

                        Debug.Assert(!string.IsNullOrWhiteSpace(name), "Name is null or whitespace.");

                        if (!_loadedPlugIns.TryAdd(name, plugin))
                        {
                            if (!_loadedPlugIns.TryUpdate(name, plugin, plugin))
                            {
                                continue;
                            }
                        }

                        _log.Print("Created plugin '{0}'.", LoggingLevel.Verbose, name);
                    }

                    break;
                }
            }
            catch (ReflectionTypeLoadException rex)
            {
                var errorMessage = new StringBuilder(512);

                foreach (Exception loadEx in rex.LoaderExceptions)
                {
                    if (errorMessage.Length > 0)
                    {
                        errorMessage.Append("\n\r");
                    }

                    errorMessage.Append(loadEx.Message);
                }

                throw new GorgonException(GorgonResult.CannotEnumerate,
                                          string.Format(Resources.GOR_ERR_PLUGIN_TYPE_LOAD_FAILURE, errorMessage));
            }
            finally
            {
                Interlocked.Exchange(ref _scanned, 2);
            }

            _log.Print($"{_loadedPlugIns.Count} plugins found in the assembly cache.", LoggingLevel.Simple);
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
                throw new ArgumentEmptyException(nameof(name));
            }

            if (!_loadedPlugIns.TryRemove(name, out Lazy<GorgonPlugIn, IDictionary<string, object>> plugin))
            {
                _log.Print($"PlugIn '{name}' was not found, it may not have been created yet.", LoggingLevel.Simple);
                return false;
            }

            if (!plugin.IsValueCreated)
            {
                return true;
            }

            DisposePlugIn(plugin.Value);
            return true;
        }

        /// <summary>
        /// Function to unload all the plugins.
        /// </summary>
        public void UnloadAll()
        {
            _log.Print("Unloading all plug ins.", LoggingLevel.Simple);

            GorgonPlugIn[] plugins = _loadedPlugIns.Where(item => item.Value.IsValueCreated).Select(item => item.Value.Value).ToArray();
            _loadedPlugIns.Clear();

            foreach (GorgonPlugIn plugin in plugins)
            {
                DisposePlugIn(plugin);
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonMefPlugInService"/> class.
        /// </summary>
        /// <param name="mefCache">The cache of MEF plugin assemblies.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="mefCache"/> parameter is <b>null</b>.</exception>
        public GorgonMefPlugInService(GorgonMefPlugInCache mefCache)
        {
            _cache = mefCache ?? throw new ArgumentNullException(nameof(mefCache));
            _log = mefCache.Log ?? GorgonLog.NullLog;
        }
        #endregion
    }
}
