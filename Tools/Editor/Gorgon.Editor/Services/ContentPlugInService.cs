
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: October 29, 2018 1:19:30 PM
// 

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.IO.FileSystem;
using Gorgon.PlugIns;

namespace Gorgon.Editor.Services;

/// <summary>
/// The service used for managing the content plug-ins
/// </summary>
internal class ContentPlugInService
    : IContentPlugInService, IDisposable
{

    // The plug-in list.
    private readonly Dictionary<string, ContentPlugIn> _plugIns = new(StringComparer.OrdinalIgnoreCase);
    // The plug-in list.
    private readonly Dictionary<string, ContentImportPlugIn> _importers = new(StringComparer.OrdinalIgnoreCase);
    // The list of disabled content plug-ins.
    private readonly Dictionary<string, IDisabledPlugIn> _disabled = new(StringComparer.OrdinalIgnoreCase);
    // The directory that contains the settings for the plug-ins.
    private readonly string _settingsDir;
    // The services passed from the host to the content plug-ins.
    private readonly IHostContentServices _hostServices;

    /// <summary>Property to return the list of content plug-ins loaded in to the application.</summary>
    /// <value>The plug-ins.</value>
    public IReadOnlyDictionary<string, ContentPlugIn> PlugIns => _plugIns;

    /// <summary>
    /// Property to return the list of content importer plug-ins loaded into the application.
    /// </summary>
    public IReadOnlyDictionary<string, ContentImportPlugIn> Importers => _importers;

    /// <summary>Property to return the list of disabled plug-ins.</summary>
    public IReadOnlyDictionary<string, IDisabledPlugIn> DisabledPlugIns => _disabled;

    /// <summary>
    /// Property to set or return the currently active content file manager to pass to any plug-ins.
    /// </summary>
    public IContentFileManager ContentFileManager
    {
        get;
        set;
    }

    /// <summary>
    /// Function to retrieve the actual plug-in based on the name associated with the project metadata item.
    /// </summary>
    /// <param name="metadata">The metadata item to evaluate.</param>
    /// <returns>The plug-in, and the <see cref="MetadataPlugInState"/> used to evaluate whether a deep inspection is required.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/> parameter is <b>null</b>.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public (ContentPlugIn PlugIn, MetadataPlugInState state) GetContentPlugIn(ProjectItemMetadata metadata)
    {
        if (metadata is null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        // If the name is null, then we've never assigned the content plug-in.  So look it up.
        if (metadata.PlugInName is null)
        {
            return (null, MetadataPlugInState.Unassigned);
        }

        if ((string.IsNullOrWhiteSpace(metadata.PlugInName))
            || (!PlugIns.TryGetValue(metadata.PlugInName, out ContentPlugIn PlugIn)))
        {
            return (null, MetadataPlugInState.NotFound);
        }

        return (PlugIn, MetadataPlugInState.Assigned);
    }

    /// <summary>
    /// Function to return the file for the content plug-in settings.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>The file containing the plug-in settings.</returns>
    private string GetContentPlugInSettingsPath(string name) =>
#if DEBUG
        Path.Combine(_settingsDir, name.FormatFileName()) + ".DEBUG.json";
#else
        Path.Combine(_settingsDir, Path.ChangeExtension(name.FormatFileName(), "json"));
#endif

    /// <summary>
    /// Function to load plug-ins for content editors.
    /// </summary>
    /// <param name="plugIns">The plug-in service to use when loading the plug-ins.</param>
    private void LoadContentEditors(IGorgonPlugInService plugIns)
    {
        IReadOnlyList<ContentPlugIn> PlugInList = plugIns.GetPlugIns<ContentPlugIn>();

        foreach (ContentPlugIn PlugIn in PlugInList)
        {
            try
            {
                _hostServices.Log.Print($"Creating content plug-in '{PlugIn.Name}'...", LoggingLevel.Simple);
                PlugIn.Initialize(_hostServices);

                // Check to see if this plug-in can continue.
                IReadOnlyList<string> validation = PlugIn.IsPlugInAvailable();

                if (validation.Count > 0)
                {
                    // Shut the plug-in down.
                    PlugIn.Shutdown();

                    _hostServices.Log.PrintWarning($"The content plug-in '{PlugIn.Name}' is disabled:", LoggingLevel.Simple);
                    foreach (string reason in validation)
                    {
                        _hostServices.Log.PrintWarning($"{reason}", LoggingLevel.Verbose);
                    }

                    _disabled[PlugIn.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, PlugIn.Name, string.Join("\r\n", validation), PlugIn.PlugInPath);

                    // Remove this plug-in.
                    plugIns.Unload(PlugIn.Name);
                    continue;
                }

                AddContentPlugIn(PlugIn);
            }
            catch (Exception ex)
            {
                // Attempt to gracefully shut the plug-in down if we error out.
                PlugIn.Shutdown();

                _hostServices.Log.PrintError($"Cannot create content plug-in '{PlugIn.Name}'.", LoggingLevel.Simple);
                _hostServices.Log.PrintException(ex);

                _disabled[PlugIn.Name] = new DisabledPlugIn(DisabledReasonCode.Error, PlugIn.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), PlugIn.PlugInPath);
            }
        }
    }

    /// <summary>
    /// Function to load plug-ins for content importers.
    /// </summary>
    /// <param name="plugIns">The plug-in service to use when loading the plug-ins.</param>
    private void LoadImporters(IGorgonPlugInService plugIns)
    {
        // Before we load, pull in any importers so they'll be initialized and ready for content plug-ins (if they're needed).
        IReadOnlyList<ContentImportPlugIn> importers = plugIns.GetPlugIns<ContentImportPlugIn>();

        foreach (ContentImportPlugIn PlugIn in importers)
        {
            try
            {
                _hostServices.Log.Print($"Creating content importer plug-in '{PlugIn.Name}'...", LoggingLevel.Simple);
                PlugIn.Initialize(_hostServices);

                // Check to see if this plug-in can continue.
                IReadOnlyList<string> validation = PlugIn.IsPlugInAvailable();

                if (validation.Count > 0)
                {
                    // Shut the plug-in down.
                    PlugIn.Shutdown();

                    _hostServices.Log.PrintWarning($"The importer plug-in '{PlugIn.Name}' is disabled:", LoggingLevel.Simple);
                    foreach (string reason in validation)
                    {
                        _hostServices.Log.PrintWarning($"{reason}", LoggingLevel.Verbose);
                    }

                    _disabled[PlugIn.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, PlugIn.Name, string.Join("\r\n", validation), PlugIn.PlugInPath);

                    // Remove this plug-in.
                    plugIns.Unload(PlugIn.Name);
                    continue;
                }

                AddContentImportPlugIn(PlugIn);
            }
            catch (Exception ex)
            {
                // Attempt to gracefully shut the plug-in down if we error out.
                PlugIn.Shutdown();

                _hostServices.Log.PrintError($"Cannot create importer plug-in '{PlugIn.Name}'.", LoggingLevel.Simple);
                _hostServices.Log.PrintException(ex);

                _disabled[PlugIn.Name] = new DisabledPlugIn(DisabledReasonCode.Error, PlugIn.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), PlugIn.PlugInPath);
            }
        }
    }

    /// <summary>Funcion to read the settings for a content plug-in from a JSON file.</summary>
    /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="converters">A list of JSON data converters.</param>
    /// <returns>The settings object for the plug-in, or <b>null</b> if no settings file was found for the plug-in.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="PlugIn" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>This will read in the settings for a content plug from the same location where the editor stores its application settings file.</remarks>
    public T ReadContentSettings<T>(string name, params JsonConverter[] converters)
        where T : class
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        string settingsFile = GetContentPlugInSettingsPath(name);

        if (!File.Exists(settingsFile))
        {
            return null;
        }

        using Stream stream = File.Open(settingsFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(stream, Encoding.UTF8);

        JsonSerializerOptions options = new();

        if (converters is not null)
        {
            foreach (JsonConverter converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), options);
    }

    /// <summary>Function to write out the settings for a content plug-in as a JSON file.</summary>
    /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="contentSettings">The content settings to persist as JSON file.</param>
    /// <param name="converters">A list of JSON converters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="PlugIn" />, or the <paramref name="contentSettings" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>This will write out the settings for a content plug-in to the same location where the editor stores its application settings file.</remarks>
    public void WriteContentSettings<T>(string name, T contentSettings, params JsonConverter[] converters)
        where T : class
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (contentSettings is null)
        {
            throw new ArgumentNullException(nameof(contentSettings));
        }

        string settingsFile = GetContentPlugInSettingsPath(name);
        using Stream stream = File.Open(settingsFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new(stream, Encoding.UTF8, 80000, false);

        JsonSerializerOptions options = new();

        if (converters is not null)
        {
            foreach (JsonConverter converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        writer.Write(JsonSerializer.Serialize(contentSettings, options));
    }

    /// <summary>Function to add a content import plug-in to the service.</summary>
    /// <param name="plugIn">The plug-in to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugIn"/> parameter is <b>null</b>.</exception>
    public void AddContentImportPlugIn(ContentImportPlugIn plugIn)
    {
        if (plugIn is null)
        {
            throw new ArgumentNullException(nameof(plugIn));
        }

        if (_importers.ContainsKey(plugIn.Name))
        {
            return;
        }

        _importers[plugIn.Name] = plugIn;
    }

    /// <summary>Function to add a content plug-in to the service.</summary>
    /// <param name="plugIn">The plug-in to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugIn"/> parameter is <b>null</b>.</exception>
    public void AddContentPlugIn(ContentPlugIn plugIn)
    {
        if (plugIn is null)
        {
            throw new ArgumentNullException(nameof(plugIn));
        }

        if (_plugIns.ContainsKey(plugIn.Name))
        {
            return;
        }

        _plugIns[plugIn.Name] = plugIn;
    }

    /// <summary>Function to clear all of the content plug-ins.</summary>
    public void Clear()
    {

        foreach (KeyValuePair<string, ContentPlugIn> plugIn in _plugIns)
        {
            plugIn.Value.Shutdown();
        }

        _plugIns.Clear();

        foreach (KeyValuePair<string, ContentImportPlugIn> plugIn in _importers)
        {
            plugIn.Value.Shutdown();
        }

        _importers.Clear();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => Clear();

    /// <summary>
    /// Function to load all of the content plug-ins into the service.
    /// </summary>
    /// <param name="plugInCache">The plug-in assembly cache.</param>
    /// <param name="plugInDir">The directory that contains the plug-ins.</param>        
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugInCache"/>, or the <paramref name="plugInDir"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="plugInDir"/> parameter is empty.</exception>
    public void LoadContentPlugIns(GorgonMefPlugInCache plugInCache, string plugInDir)
    {
        if (plugInCache is null)
        {
            throw new ArgumentNullException(nameof(plugInCache));
        }

        if (plugInDir is null)
        {
            throw new ArgumentNullException(nameof(plugInDir));
        }

        if (string.IsNullOrWhiteSpace(plugInDir))
        {
            throw new ArgumentEmptyException(nameof(plugInDir));
        }

        IReadOnlyList<string> files = Directory.GetFiles(plugInDir, "*.dll");
        IReadOnlyList<PlugInAssemblyState> assemblies = plugInCache.ValidateAndLoadAssemblies(files, _hostServices.Log);

        if (assemblies.Count > 0)
        {
            foreach (PlugInAssemblyState record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
            {
                _disabled[Path.GetFileName(record.Path)] = new DisabledPlugIn(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
            }
        }

        IGorgonPlugInService PlugIns = new GorgonMefPlugInService(plugInCache);

        LoadImporters(PlugIns);
        LoadContentEditors(PlugIns);
    }

    /// <summary>
    /// Function to remove a content import plug-in from the service.
    /// </summary>
    /// <param name="plugIn">The plug-in to remove.</param>
    public void RemoveContentImportPlugIn(ContentImportPlugIn plugIn)
    {
        if (plugIn is null)
        {
            throw new ArgumentNullException(nameof(plugIn));
        }

        if (!_importers.ContainsKey(plugIn.Name))
        {
            return;
        }

        _importers.Remove(plugIn.Name);
        plugIn.Shutdown();
    }

    /// <summary>Function to remove a content plug-in from the service.</summary>
    /// <param name="plugIn">The plug-in to remove.</param>
    public void RemoveContentPlugIn(ContentPlugIn plugIn)
    {
        if (plugIn is null)
        {
            throw new ArgumentNullException(nameof(plugIn));
        }

        if (!_plugIns.ContainsKey(plugIn.Name))
        {
            return;
        }

        _plugIns.Remove(plugIn.Name);
        plugIn.Shutdown();
    }

    /// <summary>
    /// Function to retrieve the appropriate content importer for the file specified.
    /// </summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns>A <see cref="IEditorContentImporter"/>, or <b>null</b> if none was found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// Since the content importers are meant for importing into the project virtual file system, the <paramref name="filePath"/> must point to a file on the physical file system. 
    /// </para>
    /// </remarks>
    public IEditorContentImporter GetContentImporter(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        ContentImportPlugIn importPlugIn = null;

        foreach (KeyValuePair<string, ContentImportPlugIn> PlugIn in _importers)
        {
            if (PlugIn.Value.CanOpenContent(filePath))
            {
                importPlugIn = PlugIn.Value;
                break;
            }

            continue;
        }

        return importPlugIn?.GetImporter();
    }

    /// <summary>
    /// Function called when a project is loaded/created.
    /// </summary>
    /// <param name="projectFileSystem">The read only file system used by the project.</param>
    /// <param name="fileManager">The content file manager for the project.</param>
    /// <param name="temporaryFileSystem">The file system used to hold temporary working data.</param>
    public void ProjectActivated(IGorgonFileSystem projectFileSystem, IContentFileManager fileManager, IGorgonFileSystem temporaryFileSystem)
    {
        foreach (ContentPlugIn PlugIn in _plugIns.Values)
        {
            PlugIn.ProjectOpened(fileManager, temporaryFileSystem);
        }

        foreach (ContentImportPlugIn PlugIn in _importers.Values)
        {
            PlugIn.ProjectOpened(projectFileSystem, temporaryFileSystem);
        }
    }

    /// <summary>
    /// Function called when a project is unloaded.
    /// </summary>        
    public void ProjectDeactivated()
    {
        foreach (ContentImportPlugIn PlugIn in _importers.Values)
        {
            PlugIn.ProjectClosed();
        }

        foreach (ContentPlugIn PlugIn in _plugIns.Values)
        {
            PlugIn.ProjectClosed();
        }
    }

    /// <summary>Initializes a new instance of the ContentPlugInService class.</summary>
    /// <param name="settingsDirectory">The directory that will contain settings for the content plug-ins.</param>
    /// <param name="hostServices">The services to pass from the host application to the content plug-ins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="settingsDirectory"/>, or the <paramref name="hostServices"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="settingsDirectory"/> parameter is empty.</exception>
    /// <example
    public ContentPlugInService(string settingsDirectory, IHostContentServices hostServices)
    {
        _settingsDir = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));

        if (string.IsNullOrWhiteSpace(settingsDirectory))
        {
            throw new ArgumentEmptyException(nameof(settingsDirectory));
        }

        _hostServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
    }
}
