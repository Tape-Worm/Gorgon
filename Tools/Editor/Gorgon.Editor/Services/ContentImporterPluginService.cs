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
// Created: December 17, 2018 10:17:43 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gorgon.Diagnostics;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.IO;
using Gorgon.Plugins;
using Newtonsoft.Json;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The service used for managing the content importer plugins.
    /// </summary>
    internal class ContentImporterPluginService
        : IContentImporterPluginManagerService, IDisposable
    {
        #region Variables.
        // The plugin list.
        private Dictionary<string, ContentImportPlugin> _plugins = new Dictionary<string, ContentImportPlugin>(StringComparer.OrdinalIgnoreCase);
        // The list of disabled content plug ins.
        private readonly Dictionary<string, IDisabledPlugin> _disabled = new Dictionary<string, IDisabledPlugin>(StringComparer.OrdinalIgnoreCase);
        // The directory that contains the settings for the plug ins.
        private DirectoryInfo _settingsDir;
        // The application graphics context for passing to content plug ins.
        private readonly IGraphicsContext _graphicsContext;
        #endregion

        #region Properties.
        /// <summary>Property to return the list of content plugins loaded in to the application.</summary>
        /// <value>The plugins.</value>
        public IReadOnlyDictionary<string, ContentImportPlugin> Plugins => _plugins;

        /// <summary>Property to return the list of disabled plug ins.</summary>
        public IReadOnlyDictionary<string, IDisabledPlugin> DisabledPlugins => _disabled;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the file for the content plug in settings.
        /// </summary>
        /// <param name="plugin">The content plug in that owns the settings file.</param>
        /// <returns>The file containing the plug in settings.</returns>
        private FileInfo GetContentImportPluginSettingsPath(ContentImportPlugin plugin) =>
#if DEBUG
            new FileInfo(Path.Combine(_settingsDir.FullName, plugin.Name) + ".DEBUG.json");
#else
            new FileInfo(Path.Combine(_settingsDir.FullName, Path.ChangeExtension(plugin.Name, "json")));
#endif


        /// <summary>Function to read the settings for a content importer plug in from a JSON file.</summary>
        /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
        /// <param name="plugin">The plug in that owns the settings being read.</param>
        /// <returns>The settings object for the plug in, or <b>null</b> if no settings file was found for the plug in.</returns>
        /// <exception cref="T:System.ArgumentNullException">plugin</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the <paramref name="plugin" /> parameter is <b>null</b>.</exception>
        /// <remarks>This will read in the settings for a content plug from the same location where the editor stores its application settings file.</remarks>
        public T ReadContentSettings<T>(ContentImportPlugin plugin)
            where T : class
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            FileInfo settingsFile = GetContentImportPluginSettingsPath(plugin);

            if (!settingsFile.Exists)
            {
                return null;
            }

            using (Stream stream = settingsFile.OpenRead())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
        }

        /// <summary>Function to write out the settings for a content importer plug in as a JSON file.</summary>
        /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
        /// <param name="plugin">The plug in that owns the settings being written.</param>
        /// <param name="contentSettings">The content settings to persist as JSON file.</param>
        /// <exception cref="T:System.ArgumentNullException">plugin
        /// or
        /// contentSettings</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the <paramref name="plugin" />, or the <paramref name="contentSettings" /> parameter is <b>null</b>.</exception>
        /// <remarks>This will write out the settings for a content plug in to the same location where the editor stores its application settings file.</remarks>
        public void WriteContentSettings<T>(ContentImportPlugin plugin, T contentSettings)
            where T : class
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (contentSettings == null)
            {
                throw new ArgumentNullException(nameof(contentSettings));
            }

            FileInfo settingsFile = GetContentImportPluginSettingsPath(plugin);
            using (Stream stream = settingsFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 80000, false))
                {
                    writer.Write(JsonConvert.SerializeObject(contentSettings));
                }
            }
        }

        /// <summary>Function to add a content importer plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="T:System.ArgumentNullException">plugin</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the <paramref name="plugin" /> parameter is <b>null</b>.</exception>
        public void AddContentImportPlugin(ContentImportPlugin plugin)
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

        /// <summary>Function to clear all of the content importer plugins.</summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ContentImportPlugin> plugin in _plugins)
            {
                plugin.Value.Shutdown(Program.Log);
            }

            _plugins.Clear();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => Clear();

        /// <summary>Function to retrieve a plug in that inherits or implements a specific type.</summary>
        /// <typeparam name="T">The type that is inherited/implemented. Must inherit GorgonPlugin</typeparam>
        /// <returns>A list of plugins that implement, inherit or matches a specific type.</returns>
        public IEnumerable<T> GetByType<T>() where T : ContentImportPlugin => throw new NotImplementedException();

        /// <summary>Loads the content import plugins.</summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>
        /// <exception cref="T:System.ArgumentNullException">pluginCache
        /// or
        /// pluginDir</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the <paramref name="pluginCache" />, or the <paramref name="pluginDir" /> parameter is <b>null</b>.</exception>
        public void LoadContentImporterPlugins(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException(nameof(pluginCache));
            }

            if (pluginDir == null)
            {
                throw new ArgumentNullException(nameof(pluginDir));
            }

            FileInfo[] assemblies = pluginDir.GetFiles("*.dll");

            foreach (FileInfo file in assemblies)
            {
                try
                {
                    Program.Log.Print($"Loading content importer plug in assembly '{file.FullName}'...", LoggingLevel.Simple);
                    pluginCache.LoadPluginAssemblies(pluginDir.FullName, file.Name);
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Cannot load content importer plug in assembly '{file.FullName}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);
                }
            }

            IGorgonPluginService plugins = new GorgonMefPluginService(pluginCache, Program.Log);            
            IReadOnlyList<ContentImportPlugin> pluginList = plugins.GetPlugins<ContentImportPlugin>();

            foreach (ContentImportPlugin plugin in pluginList)
            {
                try
                {
                    Program.Log.Print($"Creating content importer plug in '{plugin.Name}'...", LoggingLevel.Simple);
                    plugin.Initialize(this, _graphicsContext, Program.Log);

                    // Check to see if this plug in can continue.
                    IReadOnlyList<string> validation = plugin.IsPluginAvailable();                    

                    if (validation.Count > 0)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown(Program.Log);

                        Program.Log.Print($"WARNING: The content importer plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in validation)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }

                        _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\n", validation));

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                        continue;
                    }
                                        
                    AddContentImportPlugin(plugin);                    
                }
                catch (Exception ex)
                {
                    // Attempt to gracefully shut the plug in down if we error out.
                    plugin.Shutdown(Program.Log);

                    Program.Log.Print($"ERROR: Cannot create content importer plug in '{plugin.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_IMPORT_PLUGIN_EXCEPTION, ex.Message));
                }
            }
        }

        /// <summary>Function to remove a content importer plugin from the service.</summary>
        /// <param name="plugin">The plugin to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">plugin</exception>
        public void RemoveContentImportPlugin(ContentImportPlugin plugin)
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
            plugin.Shutdown(Program.Log);
        }


        /// <summary>
        /// Function to retrieve the appropriate content importer for the file specified.
        /// </summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <param name="fileSystem">The file system containing the file to evaluate.</param>
        /// <param name="metadataOnly"><b>true</b> to indicate that only metadata should be used to scan the content file, <b>false</b> to scan, in depth, per plugin (slow).</param>
        /// <returns>A <see cref="IEditorContentImporter"/>, or <b>null</b> if none was found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile"/> parameter is <b>null</b>.</exception>
        public IEditorContentImporter GetContentImporter(FileInfo file, IGorgonFileSystem fileSystem)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            ContentImportPlugin importPlugin = null;

            foreach (KeyValuePair<string, ContentImportPlugin> plugin in _plugins)
            {
                if (plugin.Value.CanOpenContent(file))
                {
                    importPlugin = plugin.Value;
                    break;
                }

                continue;
            }

            return importPlugin?.CreateImporter(file, fileSystem, Program.Log);
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the ContentImporterPluginService class.</summary>
        /// <param name="settingsDirectory">The directory that will contain settings for the content plug ins.</param>
        /// <param name="graphicsContext">The graphics context used to pass the application graphics context to plug ins.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="settingsDirectory"/>, or the <paramref name="graphicsContext"/> parameter is <b>null</b>.</exception>
        public ContentImporterPluginService(DirectoryInfo settingsDirectory, IGraphicsContext graphicsContext)
        {
            _settingsDir = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));
            _graphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));
        }
        #endregion
    }
}
