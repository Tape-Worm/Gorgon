
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
// Created: June 5, 2020 6:15:37 PM
// 

using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.AnimationEditor.Services;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;
using Gorgon.Plugins;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A Plugin used to build an importer for animation data
/// </summary>
internal class AnimationImporterPlugin
    : ContentImportPlugin
{

    // The image editor settings.
    private IImportSettings _settings;

    // The codecs registered with the plugin.
    private CodecRegistry _codecs;

    // The plugin cache for image codecs.
    private GorgonMefPluginCache _pluginCache;

    /// <summary>
    /// The file name for the file that stores the settings.
    /// </summary>
    public readonly static string SettingsFilename = typeof(AnimationImporterPlugin).FullName;

    /// <summary>
    /// Function to retrieve the codec used by the sprite.
    /// </summary>
    /// <param name="filePath">Path to the file containing the image content.</param>
    /// <returns>The codec used to read the file.</returns>
    public static IGorgonAnimationCodec GetCodec(string filePath, CodecRegistry codecs)
    {
        string fileExtension = Path.GetExtension(filePath);

        // Locate the file extension.
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            return null;
        }

        GorgonFileExtension extension = new(fileExtension);

        // Since all Gorgon's sprite files use the same extension, we'll have to be a little more aggressive when determining type.
        (GorgonFileExtension, IGorgonAnimationCodec codec)[] results = [.. codecs.CodecFileTypes.Where(item => item.extension == extension)];

        if (results.Length == 0)
        {
            return null;
        }

        using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return results.Select(item => item.codec).FirstOrDefault(item => item.IsReadable(stream));
    }

    /// <summary>Function to retrieve the settings interface for this plugin.</summary>
    /// <param name="injector">Objects to inject into the view model.</param>
    /// <returns>The settings interface view model.</returns>
    /// <remarks>
    ///   <para>
    /// Implementors who wish to supply customizable settings for their plugins from the main "Settings" area in the application can override this method and return a new view model based on
    /// the base <see cref="ISettingsCategoryViewModel"/> type.
    /// </para>
    ///   <para>
    /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{Control})"/> method in the
    /// <see cref="OnInitialize()"/> method or the settings will not display.
    /// </para>
    /// </remarks>
    protected override ISettingsCategory OnGetSettings() => _settings;

    /// <summary>Function to provide initialization for the Plugin.</summary>
    /// <param name="PluginService">The Plugin service used to access other Plugins.</param>
    /// <remarks>This method is only called when the Plugin is loaded at startup.</remarks>
    protected override void OnInitialize()
    {
        ViewFactory.Register<IImportSettings>(() => new AnimationCodecSettingsPanel());

        _pluginCache = new GorgonMefPluginCache(HostContentServices.Log);

        AnimationImportSettings settings = HostContentServices.ContentPluginService.ReadContentSettings<AnimationImportSettings>(SettingsFilename);

        settings ??= new AnimationImportSettings();

        _codecs = new CodecRegistry(_pluginCache, HostContentServices.GraphicsContext.Renderer2D, HostContentServices.Log);
        _codecs.LoadFromSettings(settings);

        ImportSettings settingsVm = new();
        settingsVm.Initialize(new ImportSettingsParameters(settings, _codecs, new FileOpenDialogService(), _pluginCache, HostContentServices));
        _settings = settingsVm;
    }

    /// <summary>Function to provide clean up for the Plugin.</summary>
    protected override void OnShutdown()
    {
        try
        {
            if ((_settings?.WriteSettingsCommand is not null) && (_settings.WriteSettingsCommand.CanExecute(null)))
            {
                // Persist any settings.
                _settings.WriteSettingsCommand.Execute(null);
            }

            ViewFactory.Unregister<IImportSettings>();

            _pluginCache?.Dispose();
        }
        catch (Exception ex)
        {
            // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
            HostContentServices.Log.PrintException(ex);
        }
    }

    /// <summary>Function to determine if the content Plugin can open the specified file.</summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns>
    ///   <b>true</b> if the Plugin can open the file, or <b>false</b> if not.</returns>
    /// <remarks>
    ///   <para>
    /// This method is used to determine if the file specified by the <paramref name="filePath" /> passed to the method can be opened by this plugin. If the method returns <b>true</b>, then the host
    /// application will convert the file using the importer produced by this plugin. Otherwise, if the method returns <b>false</b>, then the file is skipped.
    /// </para>
    ///   <para>
    /// The <paramref name="filePath" /> is a path to the file on the project virtual file system.
    /// </para>
    ///   <para>
    /// Implementors may use whatever method they desire to determine if the file can be opened (e.g. checking file extensions, examining file headers, etc...).
    /// </para>
    /// </remarks>
    protected override bool OnCanOpenContent(string filePath) => GetCodec(filePath, _codecs) is not null;

    /// <summary>Function to open a content object from this Plugin.</summary>
    /// <returns>A new <see cref="IEditorContentImporter"/> object.</returns>
    /// <remarks>This method creates an instance of the custom content importer. The application will use the object returned to perform the actual import process.</remarks>
    protected override IEditorContentImporter OnCreateImporter() => new GorgonAnimationImporter(ProjectFileSystem, TemporaryFileSystem, _codecs, HostContentServices.GraphicsContext.Renderer2D, HostContentServices.Log);

    /// <summary>Initializes a new instance of the <see cref="AnimationImporterPlugin"/> class.</summary>
    public AnimationImporterPlugin()
        : base(Resources.GORANM_IMPORT_DESC)
    {
    }
}
