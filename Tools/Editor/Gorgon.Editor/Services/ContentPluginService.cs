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
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
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
    /// The state for the plugin when associated with an included item.
    /// </summary>
    internal enum MetadataPlugInState
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
    internal class ContentPlugInService
        : IContentPlugInManagerService, IDisposable
    {
        #region Variables.
        // The plugin list.
        private readonly Dictionary<string, ContentPlugIn> _plugins = new Dictionary<string, ContentPlugIn>(StringComparer.OrdinalIgnoreCase);
        // The plugin list.
        private readonly Dictionary<string, ContentImportPlugIn> _importers = new Dictionary<string, ContentImportPlugIn>(StringComparer.OrdinalIgnoreCase);
        // The list of disabled content plug ins.
        private readonly Dictionary<string, IDisabledPlugIn> _disabled = new Dictionary<string, IDisabledPlugIn>(StringComparer.OrdinalIgnoreCase);
        // The directory that contains the settings for the plug ins.
        private readonly DirectoryInfo _settingsDir;
        // The application graphics context for passing to content plug ins.
        private readonly IGraphicsContext _graphicsContext;
		// The message display service.
        private readonly IViewModelInjection _commonServices;
        #endregion

        #region Properties.
        /// <summary>Property to return the list of content plugins loaded in to the application.</summary>
        /// <value>The plugins.</value>
        public IReadOnlyDictionary<string, ContentPlugIn> PlugIns => _plugins;

		/// <summary>
        /// Property to return the list of content importer plug ins loaded into the application.
        /// </summary>
        public IReadOnlyDictionary<string, ContentImportPlugIn> Importers => _importers;

        /// <summary>Property to return the list of disabled plug ins.</summary>
        public IReadOnlyDictionary<string, IDisabledPlugIn> DisabledPlugIns => _disabled;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the actual plug in based on the name associated with the project metadata item.
        /// </summary>
        /// <param name="metadata">The metadata item to evaluate.</param>
        /// <returns>The plug in, and the <see cref="MetadataPlugInState"/> used to evaluate whether a deep inspection is required.</returns>
        private (ContentPlugIn plugin, MetadataPlugInState state) GetContentPlugIn(ProjectItemMetadata metadata)
        {
            // If the name is null, then we've never assigned the content plugin.  So look it up.
            if (metadata.PlugInName == null)
            {
                return (null, MetadataPlugInState.Unassigned);
            }

            // If we have an empty string
            if (string.IsNullOrWhiteSpace(metadata.PlugInName))
            {
                return (null, MetadataPlugInState.NotFound);
            }

            #pragma warning disable IDE0046 // Convert to conditional expression
            if (!PlugIns.TryGetValue(metadata.PlugInName, out ContentPlugIn plugin))
            {
                return (null, MetadataPlugInState.NotFound);
            }

            return (plugin, MetadataPlugInState.Assigned);
            #pragma warning restore IDE0046 // Convert to conditional expression
        }

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

        /// <summary>Function to add a content import plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public void AddContentImportPlugIn(ContentImportPlugIn plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (_importers.ContainsKey(plugin.Name))
            {
                return;
            }

            _importers[plugin.Name] = plugin;
        }

        /// <summary>Function to add a content plugin to the service.</summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public void AddContentPlugIn(ContentPlugIn plugin)
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
        public void AssignContentPlugIn(IContentFile contentFile, IContentFileManager fileManager, IContentPlugInMetadata plugin)
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
        public bool AssignContentPlugIn(IContentFile contentFile, IContentFileManager fileManager, bool metadataOnly)
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
            (ContentPlugIn plugin, MetadataPlugInState state) = GetContentPlugIn(contentFile.Metadata);

            switch (state)
            {
                case MetadataPlugInState.NotFound:
                    contentFile.Metadata.ContentMetadata = null;
                    contentFile.Metadata.PlugInName = string.Empty;
                    return true;
                case MetadataPlugInState.Assigned:
                    contentFile.Metadata.ContentMetadata = plugin as IContentPlugInMetadata;
                    return true;
            }

            if (metadataOnly)
            {
                return true;
            }

            // Assume that no plugin is available for the node.
            contentFile.Metadata.PlugInName = string.Empty;

            // Attempt to associate a content plug in with the node.            
            foreach (KeyValuePair<string, ContentPlugIn> servicePlugIn in PlugIns)
            {
                if ((!(servicePlugIn.Value is IContentPlugInMetadata pluginMetadata))
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
            foreach (KeyValuePair<string, ContentPlugIn> plugin in _plugins)
            {
                plugin.Value.Shutdown();
            }

            _plugins.Clear();

            foreach (KeyValuePair<string, ContentImportPlugIn> plugin in _importers)
            {
                plugin.Value.Shutdown();
            }

            _importers.Clear();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => Clear();

        /// <summary>
        /// Function to load all of the content plug ins into the service.
        /// </summary>
        /// <param name="pluginCache">The plug in assembly cache.</param>
        /// <param name="pluginDir">The directory that contains the plug ins.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
        public void LoadContentPlugIns(GorgonMefPlugInCache pluginCache, DirectoryInfo pluginDir)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException(nameof(pluginCache));
            }

            if (pluginDir == null)
            {
                throw new ArgumentNullException(nameof(pluginDir));
            }

            IReadOnlyList<PlugInRecord> assemblies = pluginCache.ValidateAndLoadAssemblies(pluginDir.GetFiles("*.dll"), Program.Log);

            if (assemblies.Count > 0)
            {
                foreach (PlugInRecord record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
                {
                    _disabled[Path.GetFileName(record.Path)] = new DisabledPlugIn(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
                }
            }

            IGorgonPlugInService plugins = new GorgonMefPlugInService(pluginCache, Program.Log);

            // Before we load, pull in any importers so they'll be initialized and ready for content plug ins (if they're needed).
            IReadOnlyList<ContentImportPlugIn> importers = plugins.GetPlugIns<ContentImportPlugIn>();

            foreach (ContentImportPlugIn plugin in importers)
            {
                try
                {
                    Program.Log.Print($"Creating content importer plug in '{plugin.Name}'...", LoggingLevel.Simple);
                    plugin.AssignCommonServices(_commonServices);
                    plugin.Initialize(this, _graphicsContext);

                    // Check to see if this plug in can continue.
                    IReadOnlyList<string> validation = plugin.IsPlugInAvailable();

                    if (validation.Count > 0)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown();

                        Program.Log.Print($"WARNING: The content plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in validation)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }

                        _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\n", validation), plugin.PlugInPath);

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                        continue;
                    }

                    AddContentImportPlugIn(plugin);
                }
                catch (Exception ex)
                {
                    // Attempt to gracefully shut the plug in down if we error out.
                    plugin.Shutdown();

                    Program.Log.Print($"ERROR: Cannot create content plug in '{plugin.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), plugin.PlugInPath);
                }
            }

            IReadOnlyList<ContentPlugIn> pluginList = plugins.GetPlugIns<ContentPlugIn>();

            foreach (ContentPlugIn plugin in pluginList)
            {
                try
                {
                    Program.Log.Print($"Creating content plug in '{plugin.Name}'...", LoggingLevel.Simple);
                    plugin.AssignCommonServices(_commonServices);
                    plugin.Initialize(this, _graphicsContext);

                    // Check to see if this plug in can continue.
                    IReadOnlyList<string> validation = plugin.IsPlugInAvailable();                    

                    if (validation.Count > 0)
                    {
                        // Shut the plug in down.
                        plugin.Shutdown();

                        Program.Log.Print($"WARNING: The content plug in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                        foreach (string reason in validation)
                        {
                            Program.Log.Print($"WARNING: {reason}", LoggingLevel.Verbose);
                        }

                        _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\n", validation), plugin.PlugInPath);

                        // Remove this plug in.
                        plugins.Unload(plugin.Name);
                        continue;
                    }
                                        
                    AddContentPlugIn(plugin);                    
                }
                catch (Exception ex)
                {
                    // Attempt to gracefully shut the plug in down if we error out.
                    plugin.Shutdown();

                    Program.Log.Print($"ERROR: Cannot create content plug in '{plugin.Name}'.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);

                    _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), plugin.PlugInPath);
                }
            }
        }

        /// <summary>
        /// Function to remove a content import plugin from the service.
        /// </summary>
        /// <param name="plugin">The plugin to remove.</param>
        public void RemoveContentImportPlugIn(ContentImportPlugIn plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!_importers.ContainsKey(plugin.Name))
            {
                return;
            }

            _importers.Remove(plugin.Name);
            plugin.Shutdown();
        }

        /// <summary>Function to remove a content plugin from the service.</summary>
        /// <param name="plugin">The plugin to remove.</param>
        public void RemoveContentPlugIn(ContentPlugIn plugin)
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

            ContentImportPlugIn importPlugIn = null;

            foreach (KeyValuePair<string, ContentImportPlugIn> plugin in _importers)
            {
                if (plugin.Value.CanOpenContent(file))
                {
                    importPlugIn = plugin.Value;
                    break;
                }

                continue;
            }

            return importPlugIn?.CreateImporter(file, fileSystem);
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the ContentPlugInService class.</summary>
        /// <param name="settingsDirectory">The directory that will contain settings for the content plug ins.</param>
        /// <param name="graphicsContext">The graphics context used to pass the application graphics context to plug ins.</param>
        /// <param name="commonServices">Common services for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="settingsDirectory"/>, <paramref name="graphicsContext"/>, or the <paramref name="messageDisplay"/> parameter is <b>null</b>.</exception>
        public ContentPlugInService(DirectoryInfo settingsDirectory, IGraphicsContext graphicsContext, IViewModelInjection commonServices)
        {
            _settingsDir = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));
            _graphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        }
        #endregion
    }
}
