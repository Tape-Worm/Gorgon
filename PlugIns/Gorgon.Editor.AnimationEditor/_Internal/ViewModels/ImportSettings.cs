﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 24, 2019 11:16:33 AM
// 
#endregion

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The view model for the importer plug in settings.
/// </summary>
internal class ImportSettings
    : PlugInsCategory<ImportSettingsParameters>, IImportSettings
{
    #region Variables.
    // The backing store for the settings.
    private AnimationImportSettings _settings;

    // The codecs for the plug in.
    private CodecRegistry _codecs;
    #endregion

    #region Properties.
    /// <summary>Property to return the file name that will hold the plug ins.</summary>
    protected override string SettingsFileName => AnimationImporterPlugIn.SettingsFilename;

    /// <summary>
    /// Property to return the list of selected codecs.
    /// </summary>
    public ObservableCollection<CodecSetting> SelectedCodecs
    {
        get;
    } = [];

    /// <summary>
    /// Propery to return the paths to the codec plug ins.
    /// </summary>
    public ObservableCollection<CodecSetting> CodecPlugInPaths
    {
        get;
    } = [];

    /// <summary>Property to return the name of this object.</summary>
    public override string Name => Resources.GORANM_IMPORT_DESC;
    #endregion

    #region Methods.
    /// <summary>Handles the CollectionChanged event of the CodecPlugInPaths control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void CodecPlugInPaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (CodecSetting codec in e.NewItems.OfType<CodecSetting>())
                {
                    _settings.CodecPlugInPaths[codec.Name] = codec.PlugIn.PlugInPath;
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (CodecSetting codec in e.OldItems.OfType<CodecSetting>())
                {
                    if (!_settings.CodecPlugInPaths.ContainsKey(codec.Name))
                    {
                        continue;
                    }

                    _settings.CodecPlugInPaths.Remove(codec.Name);
                }
                break;
        }
    }

    /// <summary>
    /// Function to unload the selected plug in assemblies.
    /// </summary>
    protected override bool OnUnloadPlugIns()
    {
        IReadOnlyList<CodecSetting> selected = [.. SelectedCodecs];
        IReadOnlyList<GorgonAnimationCodecPlugIn> plugIns = selected.Select(item => item.PlugIn).ToArray();
        MessageResponse response = MessageResponse.None;

        if (plugIns.Count == 0)
        {
            return true;
        }

        foreach (GorgonAnimationCodecPlugIn plugIn in plugIns)
        {
            if (response is not MessageResponse.YesToAll and not MessageResponse.NoToAll)
            {
                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORANM_CONFIRM_REMOVE_CODECS, Path.GetFileName(plugIn.PlugInPath)), toAll: plugIns.Count > 1);
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

            _codecs.RemoveCodecPlugIn(plugIn);

            foreach (CodecSetting setting in selected)
            {
                SelectedCodecs.Remove(setting);
                CodecPlugInPaths.Remove(setting);
            }

            HostServices.BusyService.SetIdle();
        }

        return true;
    }

    /// <summary>
    /// Function to load in a plug in assembly.
    /// </summary>
    protected override bool OnLoadPlugIns()
    {
        string lastCodecPath = _settings.LastCodecPlugInPath.FormatDirectory(Path.DirectorySeparatorChar);

        if ((string.IsNullOrWhiteSpace(lastCodecPath)) || (!Directory.Exists(lastCodecPath)))
        {
            lastCodecPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        OpenCodecDialog.DialogTitle = Resources.GORANM_CAPTION_SELECT_CODEC_DLL;
        OpenCodecDialog.FileFilter = Resources.GORANM_FILTER_SELECT_CODEC;
        OpenCodecDialog.InitialDirectory = new DirectoryInfo(lastCodecPath);

        string path = OpenCodecDialog.GetFilename();

        if ((string.IsNullOrWhiteSpace(path)) || (!File.Exists(path)))
        {
            return false;
        }

        HostServices.BusyService.SetBusy();
        IReadOnlyList<GorgonAnimationCodecPlugIn> codecs = _codecs.AddCodecPlugIn(path, out IReadOnlyList<string> errors);

        if (errors.Count > 0)
        {
            HostServices.MessageDisplay.ShowError(Resources.GORANM_ERR_CODEC_LOAD_ERRORS_PRESENT, details: string.Join("\n\n", errors.Select((item, index) => $"Error #{index + 1}\n--------------\n{item}")));

            if (codecs.Count == 0)
            {
                return false;
            }
        }

        foreach (GorgonAnimationCodecPlugIn plugin in codecs)
        {
            foreach (GorgonAnimationCodecDescription desc in plugin.Codecs)
            {
                IGorgonAnimationCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                if (codec is null)
                {
                    continue;
                }

                CodecPlugInPaths.Add(new CodecSetting(codec.CodecDescription, plugin, desc));
            }
        }

        _settings.LastCodecPlugInPath = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);

        // Store the settings now.
        return true;
    }

    /// <summary>
    /// Function to determine if the selected plug in assemblies can be unloaded.
    /// </summary>
    /// <returns><b>true</b> if the plug in assemblies can be removed, <b>false</b> if not.</returns>
    protected override bool CanUnloadPlugInAssemblies() => SelectedCodecs.Count > 0;

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

        foreach (GorgonAnimationCodecPlugIn plugin in _codecs.CodecPlugIns)
        {
            foreach (GorgonAnimationCodecDescription desc in plugin.Codecs)
            {
                IGorgonAnimationCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                if (codec is null)
                {
                    continue;
                }

                CodecPlugInPaths.Add(new CodecSetting(codec.CodecDescription, plugin, desc));
            }
        }
    }

    /// <summary>
    /// Function called when the associated view is loaded.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        CodecPlugInPaths.CollectionChanged += CodecPlugInPaths_CollectionChanged;
    }

    /// <summary>
    /// Function called when the associated view is unloaded.
    /// </summary>
    protected override void OnUnload()
    {
        CodecPlugInPaths.CollectionChanged -= CodecPlugInPaths_CollectionChanged;

        base.OnUnload();
    }
    #endregion
}
