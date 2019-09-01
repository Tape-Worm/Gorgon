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
        : ViewModelBase<SettingsParameters>, ISettings
    {
        #region Variables.
        // The busy state service.
        private IBusyStateService _busyService;
        // The message display service.
        private IMessageDisplayService _messageDisplay;
        // The underlying settings for the plug in.
        private ImageEditorSettings _settings;
        // The plug in service used to manage content and content importer plug ins.
        private IContentPlugInService _pluginService;
        // The registry for image codecs.
        private ICodecRegistry _codecs;
        // The dialog used to open a codec assembly.
        private IFileDialogService _openCodecDialog;
        // The range for the alpha setting funtionality.
        private GorgonRange _alphaRange;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the ID for this panel.
        /// </summary>
        public Guid ID
        {
            get;
        }

        /// <summary>Gets the name.</summary>
        public string Name => Resources.GORIMG_IMPORT_DESC;

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

        /// <summary>
        /// Property to return the command for writing setting data.
        /// </summary>
        public IEditorCommand<object> WriteSettingsCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command for loading a plug in assembly.
        /// </summary>
        public IEditorCommand<object> LoadPlugInAssemblyCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to unloading a plug in assembly.
        /// </summary>
        public IEditorCommand<object> UnloadPlugInAssembliesCommand
        {
            get;
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
        /// Function to write out the settings.
        /// </summary>
        private void DoWriteSettings()
        {
            try
            {
                _pluginService.WriteContentSettings(ImageEditorPlugIn.SettingsName, _settings);
            }
            catch (Exception ex)
            {
                // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to determine if the selected plug in assemblies can be unloaded.
        /// </summary>
        /// <returns><b>true</b> if the plug in assemblies can be removed, <b>false</b> if not.</returns>
        private bool CanUnloadPlugInAssemblies() => SelectedCodecs.Count > 0;

        /// <summary>
        /// Function to unload the selected plug in assemblies.
        /// </summary>
        private void DoUnloadPlugInAssemblies()
        {
            try
            {
                IReadOnlyList<CodecSetting> selected = SelectedCodecs.ToArray();
                IReadOnlyList<GorgonImageCodecPlugIn> plugIns = selected.Select(item => item.PlugIn).ToArray();
                MessageResponse response = MessageResponse.None;

                if (plugIns.Count == 0)
                {
                    return;
                }

                foreach (GorgonImageCodecPlugIn plugIn in plugIns)
                {
                    if ((response != MessageResponse.YesToAll) && (response != MessageResponse.NoToAll))
                    {
                        response = _messageDisplay.ShowConfirmation(string.Format(Resources.GORIMG_CONFIRM_REMOVE_CODECS, Path.GetFileName(plugIn.PlugInPath)), toAll: plugIns.Count > 1);
                    }

                    if (response == MessageResponse.NoToAll)
                    {
                        return;
                    }

                    _busyService.SetBusy();

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

                    _busyService.SetIdle();
                }

                // Store the settings now.
                DoWriteSettings();
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORIMG_ERR_CANNOT_UNLOAD_CODECS);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to load in a plug in assembly.
        /// </summary>
        private void DoLoadPlugInAssembly()
        {
            try
            {
                string lastCodecPath = _settings.LastCodecPlugInPath.FormatDirectory(Path.DirectorySeparatorChar);

                if ((string.IsNullOrWhiteSpace(lastCodecPath)) || (!Directory.Exists(lastCodecPath)))
                {
                    lastCodecPath = AppDomain.CurrentDomain.BaseDirectory;
                }

                _openCodecDialog.DialogTitle = Resources.GORIMG_CAPTION_SELECT_CODEC_DLL;
                _openCodecDialog.FileFilter = Resources.GORIMG_FILTER_SELECT_CODEC;
                _openCodecDialog.InitialDirectory = new DirectoryInfo(lastCodecPath);

                string path = _openCodecDialog.GetFilename();

                if ((string.IsNullOrWhiteSpace(path)) || (!File.Exists(path)))
                {
                    return;
                }

                _busyService.SetBusy();
                IReadOnlyList<GorgonImageCodecPlugIn> codecs = _codecs.AddCodecPlugIn(path, out IReadOnlyList<string> errors);

                if (errors.Count > 0)
                {
                    _messageDisplay.ShowError(Resources.GORIMG_ERR_CODEC_LOAD_ERRORS_PRESENT, details: string.Join("\n\n", errors.Select((item, index) => $"Error #{index + 1}\n--------------\n{item}")));

                    if (codecs.Count == 0)
                    {
                        return;
                    }
                }

                foreach (GorgonImageCodecPlugIn plugin in codecs)
                {
                    foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                    {
                        IGorgonImageCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                        if (codec == null)
                        {
                            continue;
                        }

                        CodecPlugInPaths.Add(new CodecSetting(codec.CodecDescription, plugin, desc));
                    }
                }

                _settings.LastCodecPlugInPath = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);

                // Store the settings now.
                DoWriteSettings();
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORIMG_ERR_CANNOT_LOAD_CODEC);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(SettingsParameters injectionParameters)
        {
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _pluginService = injectionParameters.PlugInService ?? throw new ArgumentMissingException(nameof(injectionParameters.PlugInService), nameof(injectionParameters));
            _codecs = injectionParameters.Codecs ?? throw new ArgumentMissingException(nameof(injectionParameters.Codecs), nameof(injectionParameters));
            _openCodecDialog = injectionParameters.CodecFileDialog ?? throw new ArgumentMissingException(nameof(injectionParameters.CodecFileDialog), nameof(injectionParameters));
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(injectionParameters.BusyService), nameof(injectionParameters));
            _alphaRange = new GorgonRange(_settings.AlphaRangeMin, _settings.AlphaRangeMax);
            foreach (GorgonImageCodecPlugIn plugin in _codecs.CodecPlugIns)
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    IGorgonImageCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                    if (codec == null)
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

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ImageCodecSettings"/> class.</summary>
        public Settings()
        {
            ID = Guid.NewGuid();
            WriteSettingsCommand = new EditorCommand<object>(DoWriteSettings);
            LoadPlugInAssemblyCommand = new EditorCommand<object>(DoLoadPlugInAssembly);
            UnloadPlugInAssembliesCommand = new EditorCommand<object>(DoUnloadPlugInAssemblies, CanUnloadPlugInAssemblies);
        }
        #endregion
    }
}
