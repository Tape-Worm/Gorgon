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
// Created: October 29, 2018 1:19:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.IO;
using Gorgon.PlugIns;
using Newtonsoft.Json;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The service used for managing the tool plugins.
    /// </summary>
    internal class ToolPlugInService
        : IToolPlugInManagerService, IDisposable
    {
        #region Variables.
        // The plugin list.
        private readonly Dictionary<string, ToolPlugIn> _plugins = new Dictionary<string, ToolPlugIn>(StringComparer.OrdinalIgnoreCase);
        // The list of disabled tool plug ins.
        private readonly Dictionary<string, IDisabledPlugIn> _disabled = new Dictionary<string, IDisabledPlugIn>(StringComparer.OrdinalIgnoreCase);
		// The list of ribbon buttons for all tools.
        //private Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> _ribbonButtons = new Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);
        // The application graphics context for passing to tool plug ins.
        private readonly IGraphicsContext _graphicsContext;
		// Common application services.
        private readonly IViewModelInjection _commonServices;
        // The directory that contains the settings for the plug ins.
        private readonly DirectoryInfo _settingsDir;
		// The file system folder browser.
        private readonly IFileSystemFolderBrowseService _folderBrowser;
        #endregion

        #region Properties.
        /// <summary>Property to return the list of tool plugins loaded in to the application.</summary>
        /// <value>The plugins.</value>
        public IReadOnlyDictionary<string, ToolPlugIn> PlugIns => _plugins;

        /// <summary>Property to return the list of disabled plug ins.</summary>
        public IReadOnlyDictionary<string, IDisabledPlugIn> DisabledPlugIns => _disabled;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the file for the content plug in settings.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>The file containing the plug in settings.</returns>
        private FileInfo GetContentPlugInSettingsPath(string name) =>
#if DEBUG
            new FileInfo(Path.Combine(_settingsDir.FullName, name.FormatFileName()) + ".DEBUG.json");
#else
            new FileInfo(Path.Combine(_settingsDir.FullName, Path.ChangeExtension(name.FormatFileName(), "json")));
#endif

        /// <summary>Function to add a tool plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public void AddToolPlugIn(ToolPlugIn plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (_plugins.ContainsKey(plugin.Name))
            {
                return;
            }

            _plugins[plugin.Name] = plugin;
        }

        /// <summary>Function to clear all of the tool plugins.</summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ToolPlugIn> plugin in _plugins)
            {
                plugin.Value.Shutdown();				
            }

            _plugins.Clear();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => Clear();

        /// <summary>
        /// Function to load all of the tool plug ins into the service.
        /// </summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
        public void LoadToolPlugIns(GorgonMefPlugInCache pluginCache, DirectoryInfo pluginDir)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException(nameof(pluginCache));
            }

            if (pluginDir == null)
            {
                throw new ArgumentNullException(nameof(pluginDir));
            }

            IReadOnlyList<PlugInAssemblyState> assemblies = pluginCache.ValidateAndLoadAssemblies(pluginDir.GetFiles("*.dll"), Program.Log);

            if (assemblies.Count > 0)
            {
                foreach (PlugInAssemblyState record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
                {
                    _disabled[Path.GetFileName(record.Path)] = new DisabledPlugIn(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
                }
            }

            IGorgonPlugInService plugins = new GorgonMefPlugInService(pluginCache, Program.Log);            
            IReadOnlyList<ToolPlugIn> pluginList = plugins.GetPlugIns<ToolPlugIn>();

            foreach (ToolPlugIn plugin in pluginList)
            {
                try
                {
                    Program.Log.Print($"Creating tool plug in '{plugin.Name}'...", LoggingLevel.Simple);
                    plugin.AssignCommonServices(_commonServices);
                    plugin.Initialize(this, _graphicsContext, _folderBrowser);

                    // Check to see if this plug in can continue.
                    IReadOnlyList<string> validation = plugin.IsPlugInAvailable();                    

                    if (validation.Count > 0)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown();

                        Program.Log.Print($"WARNING: The tool plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in validation)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }

                        _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\n", validation), plugin.PlugInPath);

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                        continue;
                    }

                    AddToolPlugIn(plugin);                    
                }
                catch (Exception ex)
                {
                    // Attempt to gracefully shut the plug in down if we error out.
                    plugin.Shutdown();

                    Program.Log.Print($"ERROR: Cannot create tool plug in '{plugin.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), plugin.PlugInPath);
                }
            }
        }

        /// <summary>Function to remove a tool plugin from the service.</summary>
        /// <param name="plugin">The plugin to remove.</param>
        public void RemoveToolPlugIn(ToolPlugIn plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!_plugins.ContainsKey(plugin.Name))
            {
                return;
            }

            _plugins.Remove(plugin.Name);
            plugin.Shutdown();
        }

        /// <summary>Funcion to read the settings for a content plug in from a JSON file.</summary>
        /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
        /// <param name="name">The name of the file.</param>
        /// <param name="converters">A list of JSON data converters.</param>
        /// <returns>The settings object for the plug in, or <b>null</b> if no settings file was found for the plug in.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="plugin" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>This will read in the settings for a content plug from the same location where the editor stores its application settings file.</remarks>
        public T ReadContentSettings<T>(string name, params JsonConverter[] converters)
            where T : class
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            FileInfo settingsFile = GetContentPlugInSettingsPath(name);

            if (!settingsFile.Exists)
            {
                return null;
            }

            using (Stream stream = settingsFile.OpenRead())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), converters);
                }
            }
        }

        /// <summary>Function to write out the settings for a content plug in as a JSON file.</summary>
        /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
        /// <param name="name">The name of the file.</param>
        /// <param name="contentSettings">The content settings to persist as JSON file.</param>
        /// <param name="converters">A list of JSON converters.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="plugin" />, or the <paramref name="contentSettings" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>This will write out the settings for a content plug in to the same location where the editor stores its application settings file.</remarks>
        public void WriteContentSettings<T>(string name, T contentSettings, params JsonConverter[] converters)
            where T : class
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (contentSettings == null)
            {
                throw new ArgumentNullException(nameof(contentSettings));
            }

            FileInfo settingsFile = GetContentPlugInSettingsPath(name);
            using (Stream stream = settingsFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 80000, false))
                {
                    writer.Write(JsonConvert.SerializeObject(contentSettings, converters));
                }
            }
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the ToolPlugInService class.</summary>
        /// <param name="settingsDirectory">The directory that will contain settings for the content plug ins.</param>
        /// <param name="graphicsContext">The graphics context used to pass the application graphics context to plug ins.</param>
        /// <param name="folderBrowser">The folder browser used to browse the file system directory structure.</param>
        /// <param name="commonServices">Common application services.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public ToolPlugInService(DirectoryInfo settingsDirectory, IGraphicsContext graphicsContext, IFileSystemFolderBrowseService folderBrowser, IViewModelInjection commonServices)
        {
            _settingsDir = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));
            _graphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _folderBrowser = folderBrowser ?? throw new ArgumentNullException(nameof(folderBrowser));
        }
        #endregion
    }
}
