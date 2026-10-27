
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
// Created: April 24, 2019 11:16:33 AM
// 

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model for the importer plugin settings
/// </summary>
internal class ImportSettings
    : PluginsCategory<ImportSettingsParameters>, IImportSettings
{

    // The backing store for the settings.
    private SpriteImportSettings _settings;

    // The codecs for the plugin.
    private CodecRegistry _codecs;

    /// <summary>Property to return the file name that will hold the plugins.</summary>
    protected override string SettingsFileName => SpriteImporterPlugin.SettingsFilename;

    /// <summary>
    /// Property to return the list of selected codecs.
    /// </summary>
    public ObservableCollection<CodecSetting> SelectedCodecs
    {
        get;
    } = [];

    /// <summary>
    /// Propery to return the paths to the codec plugins.
    /// </summary>
    public ObservableCollection<CodecSetting> CodecPluginPaths
    {
        get;
    } = [];

    /// <summary>Property to return the name of this object.</summary>
    public override string Name => Resources.GORSPR_IMPORT_DESC;

    /// <summary>Handles the CollectionChanged event of the CodecPluginPaths control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void CodecPluginPaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (CodecSetting codec in e.NewItems.OfType<CodecSetting>())
                {
                    _settings.CodecPluginPaths[codec.Name] = codec.Plugin.PluginPath;
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (CodecSetting codec in e.OldItems.OfType<CodecSetting>())
                {
                    if (!_settings.CodecPluginPaths.ContainsKey(codec.Name))
                    {
                        continue;
                    }

                    _settings.CodecPluginPaths.Remove(codec.Name);
                }
                break;
        }
    }

    /// <summary>
    /// Function to unload the selected plugin assemblies.
    /// </summary>
    protected override bool OnUnloadPlugins()
    {
        IReadOnlyList<CodecSetting> selected = [.. SelectedCodecs];
        IReadOnlyList<GorgonSpriteCodecPlugin> Plugins = [.. selected.Select(item => item.Plugin)];
        MessageResponse response = MessageResponse.None;

        if (Plugins.Count == 0)
        {
            return true;
        }

        foreach (GorgonSpriteCodecPlugin Plugin in Plugins)
        {
            if (response is not MessageResponse.YesToAll and not MessageResponse.NoToAll)
            {
                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_REMOVE_CODECS, Path.GetFileName(Plugin.PluginPath)), toAll: Plugins.Count > 1);
            }

            if (response == MessageResponse.NoToAll)
            {
                return false;
            }

            HostServices.BusyService.SetBusy();

            if (response == MessageResponse.No)
            {
                continue;
            }

            _codecs.RemoveCodecPlugin(Plugin);

            foreach (CodecSetting setting in selected)
            {
                SelectedCodecs.Remove(setting);
                CodecPluginPaths.Remove(setting);
            }

            HostServices.BusyService.SetIdle();
        }

        return true;
    }

    /// <summary>
    /// Function to load in a plugin assembly.
    /// </summary>
    protected override bool OnLoadPlugins()
    {
        string lastCodecPath = _settings.LastCodecPluginPath.FormatDirectory(Path.DirectorySeparatorChar);

        if ((string.IsNullOrWhiteSpace(lastCodecPath)) || (!Directory.Exists(lastCodecPath)))
        {
            lastCodecPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        OpenCodecDialog.DialogTitle = Resources.GORSPR_CAPTION_SELECT_CODEC_DLL;
        OpenCodecDialog.FileFilter = Resources.GORSPR_FILTER_SELECT_CODEC;
        OpenCodecDialog.InitialDirectory = new DirectoryInfo(lastCodecPath);

        string path = OpenCodecDialog.GetFilename();

        if ((string.IsNullOrWhiteSpace(path)) || (!File.Exists(path)))
        {
            return false;
        }

        HostServices.BusyService.SetBusy();
        IReadOnlyList<GorgonSpriteCodecPlugin> codecs = _codecs.AddCodecPlugin(path, out IReadOnlyList<string> errors);

        if (errors.Count > 0)
        {
            HostServices.MessageDisplay.ShowError(Resources.GORSPR_ERR_CODEC_LOAD_ERRORS_PRESENT, details: string.Join("\n\n", errors.Select((item, index) => $"Error #{index + 1}\n--------------\n{item}")));

            if (codecs.Count == 0)
            {
                return false;
            }
        }

        foreach (GorgonSpriteCodecPlugin Plugin in codecs)
        {
            foreach (GorgonSpriteCodecDescription desc in Plugin.Codecs)
            {
                IGorgonSpriteCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                if (codec is null)
                {
                    continue;
                }

                CodecPluginPaths.Add(new CodecSetting(codec.CodecDescription, Plugin, desc));
            }
        }

        _settings.LastCodecPluginPath = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);

        // Store the settings now.
        return true;
    }

    /// <summary>
    /// Function to determine if the selected plugin assemblies can be unloaded.
    /// </summary>
    /// <returns><b>true</b> if the plugin assemblies can be removed, <b>false</b> if not.</returns>
    protected override bool CanUnloadPluginAssemblies() => SelectedCodecs.Count > 0;

    /// <summary>Function to retrieve the underlying object used to hold the settings.</summary>
    /// <returns>The object that holds the settings.</returns>
    protected override object OnGetSettings() => _settings;

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(ImportSettingsParameters injectionParameters)
    {
        base.OnInitialize(injectionParameters);

        _settings = injectionParameters.Settings;
        _codecs = injectionParameters.Codecs;

        foreach (GorgonSpriteCodecPlugin Plugin in _codecs.CodecPlugins)
        {
            foreach (GorgonSpriteCodecDescription desc in Plugin.Codecs)
            {
                IGorgonSpriteCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                if (codec is null)
                {
                    continue;
                }

                CodecPluginPaths.Add(new CodecSetting(codec.CodecDescription, Plugin, desc));
            }
        }
    }

    /// <summary>
    /// Function called when the associated view is loaded.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        CodecPluginPaths.CollectionChanged += CodecPluginPaths_CollectionChanged;
    }

    /// <summary>
    /// Function called when the associated view is unloaded.
    /// </summary>
    protected override void OnUnload()
    {
        CodecPluginPaths.CollectionChanged -= CodecPluginPaths_CollectionChanged;

        base.OnUnload();
    }
}
