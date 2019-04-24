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
// Created: April 24, 2019 11:16:33 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model fro the importer plug in settings.
    /// </summary>
    internal class ImporterPlugInSettings
        : ViewModelBase<ImportPlugInSettingsParameters>, IImporterPlugInSettings
    {
        #region Variables.
		// The backing store for the settings.
        private SpriteImportSettings _settings;

		// The service used to display messages to the user.
        private IMessageDisplayService _messageDisplay;

		// The service used to indicate that the application is busy.
        private IBusyStateService _busyService;

		// The codecs for the plug in.
        private ICodecRegistry _codecs;

        // The dialog used to open a codec assembly.
        private IFileDialogService _openCodecDialog;

		// The content plug in service.
        private IContentPlugInService _plugInService;
        #endregion

        #region Properties.
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

        /// <summary>Property to return the ID for the panel.</summary>
        public Guid ID
        {
            get;
        }

        /// <summary>Property to return the name of this object.</summary>
        /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
        /// property.</remarks>
        public string Name => Resources.GORSPR_IMPORT_DESC;
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
                _plugInService.WriteContentSettings(typeof(SpriteImporterPlugIn).FullName, _settings);
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
                IReadOnlyList<GorgonSpriteCodecPlugIn> plugIns = selected.Select(item => item.PlugIn).ToArray();
                MessageResponse response = MessageResponse.None;

                if (plugIns.Count == 0)
                {
                    return;
                }

                foreach (GorgonSpriteCodecPlugIn plugIn in plugIns)
                {
                    if ((response != MessageResponse.YesToAll) && (response != MessageResponse.NoToAll))
                    {
                        response = _messageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_REMOVE_CODECS, Path.GetFileName(plugIn.PlugInPath)), toAll: plugIns.Count > 1);
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
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_CANNOT_UNLOAD_CODECS);
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

                _openCodecDialog.DialogTitle = Resources.GORSPR_CAPTION_SELECT_CODEC_DLL;
                _openCodecDialog.FileFilter = Resources.GORSPR_FILTER_SELECT_CODEC;
                _openCodecDialog.InitialDirectory = new DirectoryInfo(lastCodecPath);

                string path = _openCodecDialog.GetFilename();

                if ((string.IsNullOrWhiteSpace(path)) || (!File.Exists(path)))
                {
                    return;
                }

                _busyService.SetBusy();
                IReadOnlyList<GorgonSpriteCodecPlugIn> codecs = _codecs.AddCodecPlugIn(path, out IReadOnlyList<string> errors);

                if (errors.Count > 0)
                {
                    _messageDisplay.ShowError(Resources.GORSPR_ERR_CODEC_LOAD_ERRORS_PRESENT, details: string.Join("\n\n", errors.Select((item, index) => $"Error #{index + 1}\n--------------\n{item}")));

                    if (codecs.Count == 0)
                    {
                        return;
                    }
                }

                foreach (GorgonSpriteCodecPlugIn plugin in codecs)
                {
                    foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
                    {
                        IGorgonSpriteCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

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
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_CANNOT_LOAD_CODEC);
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
        protected override void OnInitialize(ImportPlugInSettingsParameters injectionParameters)
        {
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _plugInService = injectionParameters.ContentPlugInService ?? throw new ArgumentMissingException(nameof(injectionParameters.ContentPlugInService), nameof(injectionParameters));
            _codecs = injectionParameters.Codecs ?? throw new ArgumentMissingException(nameof(injectionParameters.Codecs), nameof(injectionParameters));
            _openCodecDialog = injectionParameters.OpenCodecDialog ?? throw new ArgumentMissingException(nameof(injectionParameters.OpenCodecDialog), nameof(injectionParameters));
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(injectionParameters.BusyService), nameof(injectionParameters));

            foreach (GorgonSpriteCodecPlugIn plugin in _codecs.CodecPlugIns)
            {
                foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
                {
                    IGorgonSpriteCodec codec = _codecs.Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

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
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.ImporterPlugInSettings"/> class.</summary>
        public ImporterPlugInSettings()
        {
            ID = Guid.NewGuid();
            WriteSettingsCommand = new EditorCommand<object>(DoWriteSettings);
            LoadPlugInAssemblyCommand = new EditorCommand<object>(DoLoadPlugInAssembly);
            UnloadPlugInAssembliesCommand = new EditorCommand<object>(DoUnloadPlugInAssemblies, CanUnloadPlugInAssemblies);
        }
        #endregion
    }
}
