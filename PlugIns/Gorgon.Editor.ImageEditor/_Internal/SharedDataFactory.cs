
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
// Created: April 22, 2019 10:47:26 PM
// 

using Gorgon.Core;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Plugins;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// A factory used to build data that is shared between the importer and image editor Plugins
/// </summary>
internal static class SharedDataFactory
{

    // A weak reference to the common services in the application.
    private static WeakReference<IHostContentServices> _hostServices;
    // The service used to handling content plugins.
    // We will keep this alive and undisposed since it's meant to live for the lifetime of the application.
    private static readonly Lazy<GorgonMefPluginCache> _pluginCache;
    // The factory that creates/loads the settings.
    private static readonly Lazy<ImageEditorSettings> _settingsFactory;
    // The factory that creates/loads the codec registry.
    private static readonly Lazy<ICodecRegistry> _codecRegistryFactory;
    // The factory that creates/loads the settings view model.
    private static readonly Lazy<(Settings settings, SettingsPlugins PluginSettings)> _settingsViewModelFactory;

    /// <summary>
    /// Function to retrieve the common plugin cache.
    /// </summary>
    /// <returns>The plugin cache.</returns>
    private static GorgonMefPluginCache GetPluginCache() => !_hostServices.TryGetTarget(out IHostContentServices commonServices)
            ? throw new GorgonException(GorgonResult.CannotCreate)
            : new GorgonMefPluginCache(commonServices.Log);

    /// <summary>
    /// Function to load the settings for the image editor/importer.
    /// </summary>
    /// <returns>The settings for both plugins.</returns>
    private static ImageEditorSettings LoadSettings()
    {
        if (!_hostServices.TryGetTarget(out IHostContentServices commonServices))
        {
            throw new GorgonException(GorgonResult.CannotCreate);
        }

        ImageEditorSettings settings = commonServices.ContentPluginService.ReadContentSettings<ImageEditorSettings>(ImageEditorPlugin.SettingsName);

        settings ??= new ImageEditorSettings();

        return settings;
    }

    /// <summary>
    /// Function to retrieve the codec registry and load the initial codecs from the settings.
    /// </summary>
    /// <returns>The codec registry.</returns>
    private static ICodecRegistry GetCodecRegistry()
    {
        if (!_hostServices.TryGetTarget(out IHostContentServices commonServices))
        {
            throw new GorgonException(GorgonResult.CannotCreate);
        }

        CodecRegistry result = new(_pluginCache.Value, commonServices.Log);
        result.LoadFromSettings(_settingsFactory.Value);
        return result;
    }

    /// <summary>
    /// Function to retrieve the settings view model.
    /// </summary>
    /// <returns>The settings view model.</returns>
    private static (Settings settings, SettingsPlugins Plugins) GetSettingsViewModel()
    {
        if (!_hostServices.TryGetTarget(out IHostContentServices commonServices))
        {
            throw new GorgonException(GorgonResult.CannotCreate);
        }

        IFileDialogService dialog = new FileOpenDialogService
        {
            DialogTitle = Resources.GORIMG_CAPTION_SELECT_CODEC_DLL,
            FileFilter = Resources.GORIMG_FILTER_SELECT_CODEC
        };

        Settings settings = new();
        SettingsPlugins settingsPlugins = new();
        settings.Initialize(new SettingsParameters(_settingsFactory.Value, commonServices));
        settingsPlugins.Initialize(new SettingsPluginsParameters(_settingsFactory.Value, _codecRegistryFactory.Value, dialog, _pluginCache.Value, commonServices));
        return (settings, settingsPlugins);
    }

    /// <summary>
    /// Function to retrieve the shared data for the plugins in this assembly.
    /// </summary>
    /// <param name="hostServices">The services passed from the host application.</param>
    /// <returns>A tuple containing the shared codec registry and the settings view models.</returns>
    public static (ICodecRegistry codecRegisry, ISettings settingsViewModel, ISettingsPlugins PluginSettingsViewModel) GetSharedData(IHostContentServices hostServices)
    {
        Interlocked.CompareExchange(ref _hostServices, new WeakReference<IHostContentServices>(hostServices), null);
        return (_codecRegistryFactory.Value, _settingsViewModelFactory.Value.settings, _settingsViewModelFactory.Value.PluginSettings);
    }

    /// <summary>Initializes static members of the <see cref="SharedDataFactory"/> class.</summary>
    static SharedDataFactory()
    {
        _pluginCache = new Lazy<GorgonMefPluginCache>(GetPluginCache, LazyThreadSafetyMode.ExecutionAndPublication);
        _settingsFactory = new Lazy<ImageEditorSettings>(LoadSettings, LazyThreadSafetyMode.ExecutionAndPublication);
        _codecRegistryFactory = new Lazy<ICodecRegistry>(GetCodecRegistry, LazyThreadSafetyMode.ExecutionAndPublication);
        _settingsViewModelFactory = new Lazy<(Settings, SettingsPlugins)>(GetSettingsViewModel, LazyThreadSafetyMode.ExecutionAndPublication);
    }
}
