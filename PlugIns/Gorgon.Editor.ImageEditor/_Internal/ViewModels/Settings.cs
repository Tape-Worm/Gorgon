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
using System.Linq;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Settings view model for image codecs.
    /// </summary>
    internal class Settings
        : ViewModelBase<SettingsParameters>, ISettings
    {
        #region Variables.
		// The message display service.
        private IMessageDisplayService _messageDisplay;
		// The underlying settings for the plug in.
        private ImageEditorSettings _settings;
		// The plug in service used to manage content and content importer plug ins.
        private IContentPluginService _pluginService;
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
        /// Propery to return the paths to the codec plug ins.
        /// </summary>
        public ObservableCollection<(string name, string path)> CodecPluginPaths
        {
            get;
            private set;
        } = new ObservableCollection<(string name, string path)>();

        /// <summary>
        /// Property to return the command for writing setting data.
        /// </summary>
        public IEditorCommand<object> WriteSettingsCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the CollectionChanged event of the CodecPluginPaths control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void CodecPluginPaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach ((string key, string value) in e.NewItems.OfType<(string, string)>())
                    {
                        _settings.CodecPluginPaths[key] = value;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach ((string key, string value) in e.OldItems.OfType<(string, string)>())
                    {
                        if (!_settings.CodecPluginPaths.ContainsKey(key))
                        {
                            continue;
                        }

                        _settings.CodecPluginPaths.Remove(key);
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
                _pluginService.WriteContentSettings(ImageEditorPlugin.SettingsName, _settings);
            }
            catch (Exception ex)
            {
                // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
                Log.LogException(ex);
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
            _pluginService = injectionParameters.PluginService ?? throw new ArgumentMissingException(nameof(injectionParameters.PluginService), nameof(injectionParameters));

            foreach (KeyValuePair<string, string> codecPath in _settings.CodecPluginPaths)
            {
                CodecPluginPaths.Add((codecPath.Key, codecPath.Value));
            }            
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public override void OnLoad()
        {
            base.OnLoad();
            CodecPluginPaths.CollectionChanged += CodecPluginPaths_CollectionChanged;
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload()
        {
            CodecPluginPaths.CollectionChanged -= CodecPluginPaths_CollectionChanged;            
            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ImageCodecSettings"/> class.</summary>
        public Settings()
        {
            ID = Guid.NewGuid();
            WriteSettingsCommand = new EditorCommand<object>(DoWriteSettings);
        }
        #endregion
    }
}
