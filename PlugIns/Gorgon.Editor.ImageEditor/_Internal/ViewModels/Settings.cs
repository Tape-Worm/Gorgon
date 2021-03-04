#region MIT
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
// Created: April 20, 2019 2:22:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Settings view model for image codecs.
    /// </summary>
    internal class Settings
        : PlugInsCategory<SettingsParameters>, ISettings
    {
        #region Variables.
        // The underlying settings for the plug in.
        private ImageEditorSettings _settings;
        // The registry for image codecs.
        private ICodecRegistry _codecs;
        // The range for the alpha setting funtionality.
        private GorgonRange _alphaRange;
        #endregion

        #region Properties.
        /// <summary>Property to return the file name that will hold the plug ins.</summary>
        protected override string SettingsFileName => typeof(ImageEditorPlugIn).FullName;

        /// <summary>Gets the name.</summary>
        public override string Name => Resources.GORIMG_IMPORT_DESC;

        /// <summary>
        /// Property to return the last used alpha value when setting the alpha channel on an image.
        /// </summary>
        public int LastAlphaValue
        {
            get => _settings.AlphaValue;
            set
            {
                if (_settings.AlphaValue == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.AlphaValue = value;
                OnPropertyChanged();
            }
        }
            

        /// <summary>
        /// Property to return the last used alpha value when setting the alpha channel on an image.
        /// </summary>
        public GorgonRange LastAlphaRange
        {
            get => _alphaRange;
            set
            {
                if (_alphaRange.Equals(value))
                {
                    return;
                }

                OnPropertyChanging();
                 _alphaRange = new GorgonRange(value.Minimum, value.Maximum);
                _settings.AlphaRangeMin = value.Minimum;
                _settings.AlphaRangeMax = value.Maximum;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the to the directory that was last used for importing/exporting.
        /// </summary>
        public string LastImportExportPath
        {
            get => _settings.LastImportExportPath;
            set
            {
                if (string.Equals(_settings.LastImportExportPath, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _settings.LastImportExportPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the list of selected codecs.
        /// </summary>
        public ObservableCollection<CodecSetting> SelectedCodecs
        {
            get;
        } = new ObservableCollection<CodecSetting>();

        /// <summary>
        /// Propery to return the paths to the codec plug ins.
        /// </summary>
        public ObservableCollection<CodecSetting> CodecPlugInPaths
        {
            get;
        } = new ObservableCollection<CodecSetting>();

        /// <summary>Property to set or return the width of the picker window.</summary>
        public int PickerWidth
        {
            get => _settings.PickerWidth;
            set
            {
                if (_settings.PickerWidth == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.PickerWidth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the height of the picker window.</summary>
        public int PickerHeight
        {
            get => _settings.PickerHeight;
            set
            {
                if (_settings.PickerHeight == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.PickerHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the state of the picker window.</summary>
        public int PickerWindowState
        {
            get => _settings.PickerWindowState;
            set
            {
                if (_settings.PickerWindowState == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.PickerWindowState = value;
                OnPropertyChanged();
            }
        }        
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
        /// Function to determine if the selected plug in assemblies can be unloaded.
        /// </summary>
        /// <returns><b>true</b> if the plug in assemblies can be removed, <b>false</b> if not.</returns>
        protected override bool CanUnloadPlugInAssemblies() => SelectedCodecs.Count > 0;

        /// <summary>Function to retrieve the underlying object used to hold the settings.</summary>
        /// <returns>The object that holds the settings.</returns>
        protected override object OnGetSettings() => _settings;

        /// <summary>Function to unload previously loaded plug ins.</summary>
        /// <returns>
        ///   <b>true</b> to indicate that the operation succeeded, or <b>false</b> if it was cancelled.</returns>
        protected override bool OnUnloadPlugIns()
        {
            IReadOnlyList<CodecSetting> selected = SelectedCodecs.ToArray();
            IReadOnlyList<GorgonImageCodecPlugIn> plugIns = selected.Select(item => item.PlugIn).ToArray();
            MessageResponse response = MessageResponse.None;

            if (plugIns.Count == 0)
            {
                return true;
            }

            foreach (GorgonImageCodecPlugIn plugIn in plugIns)
            {
                if ((response != MessageResponse.YesToAll) && (response != MessageResponse.NoToAll))
                {
                    response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORIMG_CONFIRM_REMOVE_CODECS, Path.GetFileName(plugIn.PlugInPath)), toAll: plugIns.Count > 1);
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

        /// <summary>Function to load plugins from selected assemblies.</summary>
        /// <returns>
        ///   <b>true</b> to indicate that the operation succeeded, or <b>false</b> if it was cancelled.</returns>
        protected override bool OnLoadPlugIns()
        {
            string lastCodecPath = _settings.LastCodecPlugInPath.FormatDirectory(Path.DirectorySeparatorChar);

            if ((string.IsNullOrWhiteSpace(lastCodecPath)) || (!Directory.Exists(lastCodecPath)))
            {
                lastCodecPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            OpenCodecDialog.DialogTitle = Resources.GORIMG_CAPTION_SELECT_CODEC_DLL;
            OpenCodecDialog.FileFilter = Resources.GORIMG_FILTER_SELECT_CODEC;
            OpenCodecDialog.InitialDirectory = new DirectoryInfo(lastCodecPath);

            string path = OpenCodecDialog.GetFilename();

            if ((string.IsNullOrWhiteSpace(path)) || (!File.Exists(path)))
            {
                return false;
            }

            HostServices.BusyService.SetBusy();
            IReadOnlyList<GorgonImageCodecPlugIn> codecs = _codecs.AddCodecPlugIn(path, out IReadOnlyList<string> errors);

            if (errors.Count > 0)
            {
                HostServices.MessageDisplay.ShowError(Resources.GORIMG_ERR_CODEC_LOAD_ERRORS_PRESENT, details: string.Join("\n\n", errors.Select((item, index) => $"Error #{index + 1}\n--------------\n{item}")));

                if (codecs.Count == 0)
                {
                    return false;
                }
            }

            foreach (GorgonImageCodecPlugIn plugin in codecs)
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    IGorgonImageCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                    if (codec is null)
                    {
                        continue;
                    }

                    CodecPlugInPaths.Add(new CodecSetting(codec.CodecDescription, plugin, desc));
                }
            }

            _settings.LastCodecPlugInPath = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);

            return true;
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(SettingsParameters injectionParameters)            
        {
            base.OnInitialize(injectionParameters);
            _settings = injectionParameters.Settings;
            _codecs = injectionParameters.Codecs;
            _alphaRange = new GorgonRange(_settings.AlphaRangeMin, _settings.AlphaRangeMax);
            foreach (GorgonImageCodecPlugIn plugin in _codecs.CodecPlugIns)
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    IGorgonImageCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

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
        public override void OnLoad()
        {
            base.OnLoad();

            CodecPlugInPaths.CollectionChanged += CodecPlugInPaths_CollectionChanged;
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload()
        {
            CodecPlugInPaths.CollectionChanged -= CodecPlugInPaths_CollectionChanged;

            base.OnUnload();
        }
        #endregion
    }
}
