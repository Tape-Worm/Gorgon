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
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Plugins;
using Newtonsoft.Json;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The state for the plugin when associated with an included item.
    /// </summary>
    internal enum MetadataPluginState
    {
        /// <summary>
        /// No plugin was ever assigned.
        /// </summary>
        Unassigned,
        /// <summary>
        /// The plugin was assigned.
        /// </summary>
        Assigned,
        /// <summary>
        /// The plugin was not found.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// The service used for managing the content plugins.
    /// </summary>
    internal class ContentPluginService
        : IContentPluginManagerService, IDisposable
    {
        #region Variables.
        // The plugin list.
        private Dictionary<string, ContentPlugin> _plugins = new Dictionary<string, ContentPlugin>(StringComparer.OrdinalIgnoreCase);
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
        public IReadOnlyDictionary<string, ContentPlugin> Plugins => _plugins;

        /// <summary>Property to return the list of disabled plug ins.</summary>
        public IReadOnlyDictionary<string, IDisabledPlugin> DisabledPlugins => _disabled;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the actual plug in based on the name associated with the project metadata item.
        /// </summary>
        /// <param name="metadata">The metadata item to evaluate.</param>
        /// <returns>The plug in, and the <see cref="MetadataPluginState"/> used to evaluate whether a deep inspection is required.</returns>
        private (ContentPlugin plugin, MetadataPluginState state) GetContentPlugin(ProjectItemMetadata metadata)
        {
            // If the name is null, then we've never assigned the content plugin.  So look it up.
            if (metadata.PluginName == null)
            {
                return (null, MetadataPluginState.Unassigned);
            }

            // If we have an empty string
            if (string.IsNullOrWhiteSpace(metadata.PluginName))
            {
                return (null, MetadataPluginState.NotFound);
            }

            #pragma warning disable IDE0046 // Convert to conditional expression
            if (!Plugins.TryGetValue(metadata.PluginName, out ContentPlugin plugin))
            {
                return (null, MetadataPluginState.NotFound);
            }

            return (plugin, MetadataPluginState.Assigned);
            #pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to return the file for the content plug in settings.
        /// </summary>
        /// <param name="plugin">The content plug in that owns the settings file.</param>
        /// <returns>The file containing the plug in settings.</returns>
        private FileInfo GetContentPluginSettingsPath(ContentPlugin plugin) =>
#if DEBUG
            new FileInfo(Path.Combine(_settingsDir.FullName, plugin.Name) + ".DEBUG.json");
#else
            new FileInfo(Path.Combine(_settingsDir.FullName, Path.ChangeExtension(plugin.Name, "json")));
#endif


        /// <summary>
        /// Funcion to read the settings for a content plug in from a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
        /// <param name="plugin">The plug in that owns the settings being read.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        /// <returns>The settings object for the plug in, or <b>null</b> if no settings file was found for the plug in.</returns>
        /// <remarks>
        /// <para>
        /// This will read in the settings for a content plug from the same location where the editor stores its application settings file.
        /// </para>
        /// </remarks>
        public T ReadContentSettings<T>(ContentPlugin plugin)
            where T : class
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            FileInfo settingsFile = GetContentPluginSettingsPath(plugin);

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

        /// <summary>
        /// Function to write out the settings for a content plug in as a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
        /// <param name="plugin">The plug in that owns the settings being written.</param>
        /// <param name="contentSettings">The content settings to persist as JSON file.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/>, or the <paramref name="contentSettings"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will write out the settings for a content plug in to the same location where the editor stores its application settings file.
        /// </para>
        /// </remarks>
        public void WriteContentSettings<T>(ContentPlugin plugin, T contentSettings)
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

            FileInfo settingsFile = GetContentPluginSettingsPath(plugin);
            using (Stream stream = settingsFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 80000, false))
                {
                    writer.Write(JsonConvert.SerializeObject(contentSettings));
                }
            }
        }

        /// <summary>Function to add a content plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public void AddContentPlugin(ContentPlugin plugin)
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

        /// <summary>
        /// Function to assign a plugin to a content file.
        /// </summary>
        /// <param name="contentFile">The content file to update.</param>
        /// <param name="fileManager">The file manager used to manage content files.</param>
        /// <param name="plugin">The plugin to assign.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="plugin"/> is unable to read the contents of <paramref name="contentFile"/>.</exception>
        public void AssignContentPlugin(IContentFile contentFile, IContentFileManager fileManager, IContentPluginMetadata plugin)
        {
            if (contentFile == null)
            {
                throw new ArgumentNullException(nameof(contentFile));
            }

            if ((!plugin.CanOpenContent(contentFile, fileManager)) || (contentFile.Metadata == null))
            {
                throw new GorgonException(GorgonResult.CannotRead);
            }

            contentFile.Metadata.ContentMetadata = plugin;            
            contentFile.RefreshMetadata();
        }

        /// <summary>
        /// Function to set up the content plug in association for a content file.
        /// </summary>
        /// <param name="contentFile">The content file to evaluate.</param>
        /// <param name="fileManager">The file manager used to manage content files.</param>
        /// <param name="metadataOnly"><b>true</b> to indicate that only metadata should be used to scan the content file, <b>false</b> to scan, in depth, per plugin (slow).</param>
        /// <returns><b>true</b> if a content plug in was associated, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile"/> parameter is <b>null</b>.</exception>
        public bool AssignContentPlugin(IContentFile contentFile, IContentFileManager fileManager, bool metadataOnly)
        {
            if (contentFile == null)
            {
                throw new ArgumentNullException(nameof(contentFile));
            }

            // Do not query association data for excluded file paths.
            if (contentFile.Metadata == null)
            {
                return false;
            }

            // This node is already associated.
            if (contentFile.Metadata.ContentMetadata != null)
            {
                return false;
            }

            // Check the metadata for the plugin type associated with the node.            
            (ContentPlugin plugin, MetadataPluginState state) = GetContentPlugin(contentFile.Metadata);

            switch (state)
            {
                case MetadataPluginState.NotFound:
                    contentFile.Metadata.ContentMetadata = null;
                    contentFile.Metadata.PluginName = string.Empty;
                    return true;
                case MetadataPluginState.Assigned:
                    contentFile.Metadata.ContentMetadata = plugin as IContentPluginMetadata;
                    return true;
            }

            if (metadataOnly)
            {
                return true;
            }

            // Assume that no plugin is available for the node.
            contentFile.Metadata.PluginName = string.Empty;

            // Attempt to associate a content plug in with the node.            
            foreach (KeyValuePair<string, ContentPlugin> servicePlugin in Plugins)
            {
                if ((!(servicePlugin.Value is IContentPluginMetadata pluginMetadata))
                    || (!pluginMetadata.CanOpenContent(contentFile, fileManager)))
                {
                    continue;
                }

                contentFile.Metadata.ContentMetadata = pluginMetadata;
                break;
            }

            return true;
        }

        /// <summary>Function to clear all of the content plugins.</summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ContentPlugin> plugin in _plugins)
            {
                plugin.Value.Shutdown();
            }

            _plugins.Clear();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => Clear();

        /// <summary>
        /// Function to load all of the content plug ins into the service.
        /// </summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
        public void LoadContentPlugins(GorgonMefPluginCache pluginCache, DirectoryInfo pluginDir)
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
                    Program.Log.Print($"Loading content plug in assembly '{file.FullName}'...", LoggingLevel.Simple);
                    pluginCache.LoadPluginAssemblies(pluginDir.FullName, "*.dll");
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Cannot load content plug in assembly '{file.FullName}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);
                }
            }

            IGorgonPluginService plugins = new GorgonMefPluginService(pluginCache, Program.Log);            
            IReadOnlyList<ContentPlugin> pluginList = plugins.GetPlugins<ContentPlugin>();

            foreach (ContentPlugin plugin in pluginList)
            {
                try
                {
                    Program.Log.Print($"Creating content plug in '{plugin.Name}'...", LoggingLevel.Simple);
                    plugin.Initialize(this, _graphicsContext, Program.Log);

                    // Check to see if this plug in can continue.
                    IReadOnlyList<string> validation = plugin.IsPluginAvailable();                    

                    if (validation.Count > 0)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown();

                        Program.Log.Print($"WARNING: The content plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in validation)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }

                        _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\n", validation));

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                        continue;
                    }
                                        
                    AddContentPlugin(plugin);                    
                }
                catch (Exception ex)
                {
                    // Attempt to gracefully shut the plug in down if we error out.
                    plugin.Shutdown();

                    Program.Log.Print($"ERROR: Cannot create content plug in '{plugin.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[plugin.Name] = new DisabledPlugin(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message));
                }
            }
        }

        /// <summary>Function to remove a content plugin from the service.</summary>
        /// <param name="plugin">The plugin to remove.</param>
        public void RemoveContentPlugin(ContentPlugin plugin)
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
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the ContentPluginService class.</summary>
        /// <param name="settingsDirectory">The directory that will contain settings for the content plug ins.</param>
        /// <param name="graphicsContext">The graphics context used to pass the application graphics context to plug ins.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="settingsDirectory"/>, or the <paramref name="graphicsContext"/> parameter is <b>null</b>.</exception>
        public ContentPluginService(DirectoryInfo settingsDirectory, IGraphicsContext graphicsContext)
        {
            _settingsDir = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));
            _graphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));
        }
        #endregion
    }
}
