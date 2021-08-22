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
        : SettingsCategoryBase<SettingsParameters>, ISettings
    {
        #region Variables.
        // The underlying settings for the plug in.
        private ImageEditorSettings _settings;
        // The range for the alpha setting funtionality.
        private GorgonRange _alphaRange;
        #endregion

        #region Properties.
        /// <summary>Gets the name.</summary>
        public override string Name => Resources.GORIMG_SETTINGS_DESC;

        /// <summary>
        /// Property to set or return the path to the image editor to use when editing the texture.
        /// </summary>
        public string ImageEditorApplicationPath
        {
            get => _settings.ImageEditorApplicationPath;
            private set
            {
                if (string.Equals(_settings.ImageEditorApplicationPath, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _settings.ImageEditorApplicationPath = value;
                OnPropertyChanged();
            }
        }

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

        /// <summary>
        /// Property to return the command used to update the path.
        /// </summary>
        public IEditorCommand<string> UpdatePathCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign the exe path.
        /// </summary>
        /// <param name="newPath">The path to assign.</param>
        private void DoSetPath(string newPath)
        {
            try
            {
                if (string.Equals(newPath, ImageEditorApplicationPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                ImageEditorApplicationPath = newPath;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_SETTING_EXE_PATH);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(SettingsParameters injectionParameters)            
        {
            _settings = injectionParameters.Settings;
            _alphaRange = new GorgonRange(_settings.AlphaRangeMin, _settings.AlphaRangeMax);            
        }
        #endregion

        #region Constructor
        /// <summary>Initializes a new instance of the <see cref="Settings" /> class.</summary>
        public Settings() => UpdatePathCommand = new EditorCommand<string>(DoSetPath);
        #endregion
    }
}
