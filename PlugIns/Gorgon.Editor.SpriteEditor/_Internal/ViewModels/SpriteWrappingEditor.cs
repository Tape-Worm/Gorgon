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
// Created: April 17, 2019 9:11:50 AM
// 
#endregion

using System;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the wrapping editor panel.
    /// </summary>
    internal class SpriteWrappingEditor
        : ViewModelBase<SpriteWrappingEditorParameters>, ISpriteWrappingEditor
    {
        #region Variables.
        // Horiztonal wrapping state.
        private TextureWrap _hWrap = TextureWrap.Clamp;
        // Vertical wrapping state.
        private TextureWrap _vWrap = TextureWrap.Clamp;
        // The current color for the border.
        private GorgonColor _border = GorgonColor.White;
        // The original border color.
        private GorgonColor _originalBorder = GorgonColor.White;
        // The service used to display messages to the user.
        private IMessageDisplayService _messageDisplay;
        // Flag to indicate that the editor is active.
        private bool _isActive;
        // The command to execute when the OK button is clicked.
        private IEditorCommand<object> _okCommand;
        // The builder used to create samplers.
        private ISamplerBuildService _samplerBuilder;
        #endregion

        #region Properties.
        /// <summary>Property to set or return the current horizontal wrapping state.</summary>
        public TextureWrap HorizontalWrapping
        {
            get => _hWrap;
            set
            {
                if (_hWrap == value)
                {
                    return;
                }

                OnPropertyChanging();
                _hWrap = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the current vertical wrapping state.</summary>
        public TextureWrap VerticalWrapping
        {
            get => _vWrap;
            set
            {
                if (_vWrap == value)
                {
                    return;
                }

                OnPropertyChanging();
                _vWrap = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the current border color.</summary>
        public GorgonColor BorderColor
        {
            get => _border;
            set
            {
                if (_border.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _border = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the original border color.
        /// </summary>
        public GorgonColor OriginalBorderColor
        {
            get => _originalBorder;
            set
            {
                if (_originalBorder.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _originalBorder = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return whether the crop/resize settings is active or not.</summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isActive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return whether the panel is modal.</summary>
        public bool IsModal => true;

        /// <summary>Property to return the command used to cancel the operation.</summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
        }

        /// <summary>Property to set or return the command used to apply the operation.</summary>
        public IEditorCommand<object> OkCommand
        {
            get => _okCommand;
            set
            {
                if (_okCommand == value)
                {
                    return;
                }

                OnPropertyChanging();
                _okCommand = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the panel is cancelled by the user.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                IsActive = false;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(SpriteWrappingEditorParameters injectionParameters)
        {
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            _samplerBuilder = injectionParameters.SamplerStateBuilder ?? throw new ArgumentMissingException(nameof(injectionParameters.SamplerStateBuilder), nameof(injectionParameters));
        }

        /// <summary>
        /// Function to retrieve the sampler state specified by the settings on this view model.
        /// </summary>
        /// <param name="filter">The current texture filter.</param>
        /// <returns>The sampler state for the sprite.</returns>
        public GorgonSamplerState GetSampler(SampleFilter filter)
        {
            try
            {
                return _samplerBuilder.GetSampler(filter, HorizontalWrapping, VerticalWrapping, BorderColor);
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return null;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpriteWrappingEditor"/> class.</summary>
        public SpriteWrappingEditor() => CancelCommand = new EditorCommand<object>(DoCancel);
        #endregion
    }
}
