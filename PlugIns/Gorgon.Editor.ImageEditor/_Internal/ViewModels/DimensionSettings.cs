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
// Created: February 5, 2019 7:54:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The view model for the image dimensions editor.
    /// </summary>
    internal class DimensionSettings
        : ViewModelBase<DimensionSettingsParameters>, IDimensionSettings
    {
        #region Variables.
        // Flag to indicate that the image has depth slices.
        private bool _hasDepth;
        // The maximum width.
        private int _maxWidth;
        // The maximum height.
        private int _maxHeight;
        // The maximum depth slices or array indices.
        private int _maxDepthOrArray;
        // The video adapter used by the application.
        private IGorgonVideoAdapterInfo _videoAdapter;
        // The maximum number of mip map levels.
        private int _maxMipLevels;
        // Flag to indicate that the editor is active.
        private bool _isActive;
        // The current cropping alignment.
        private Alignment _cropAlignment = Alignment.Center;
        // The current resizing filter.
        private ImageFilter _resizeFilter = ImageFilter.Fant;
        // The current mode of operation.
        private CropResizeMode _mode;
        // The current stepping for the array index.
        private int _arrayIndexStep = 1;
        #endregion

        #region Properties.
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

        /// <summary>Property to return whether the image has depth slices or not.</summary>
        /// <remarks>
        /// This will return <b>true</b> if the image is a volume texture, or <b>false</b> if it is not.  If this returns <b>false</b>, then the image uses array indices instead of depth slices.
        /// </remarks>
        public bool HasDepth
        {
            get => _hasDepth;
            private set
            {
                if (_hasDepth == value)
                {
                    return;
                }

                OnPropertyChanging();
                _hasDepth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the width of the image, in pixels.</summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>Property to set or return the height of the image, in pixels.</summary>
        public int Height
        {
            get;
            set;
        }

        /// <summary>Property to set or return the number of mip map levels.</summary>
        public int MipLevels
        {
            get;
            set;
        }

        /// <summary>Property to set or return the number of depth slices if the image is a volume texture, or array indices if not.</summary>
        public int DepthSlicesOrArrayIndices
        {
            get;
            set;
        }

        /// <summary>Property to return the maximum width of an image.</summary>
        public int MaxWidth
        {
            get => _maxWidth;
            private set
            {
                if (_maxWidth == value)
                {
                    return;
                }

                OnPropertyChanging();
                _maxWidth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the maximum height of an image.</summary>
        public int MaxHeight
        {
            get => _maxHeight;
            private set
            {
                if (_maxHeight == value)
                {
                    return;
                }

                OnPropertyChanging();
                _maxHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the maximum number of depth slices for a volume texture, or array indices for a 2D image.</summary>
        public int MaxDepthOrArrayIndices
        {
            get => _maxDepthOrArray;
            private set
            {
                if (_maxDepthOrArray == value)
                {
                    return;
                }

                OnPropertyChanging();
                _maxDepthOrArray = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the maximum number of mip map levels based on the width, height and, if applicable, depth slices.</summary>
        public int MaxMipLevels
        {
            get => _maxMipLevels;
            private set
            {
                if (_maxMipLevels == value)
                {
                    return;
                }

                OnPropertyChanging();
                _maxMipLevels = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the number of array indices to increment or decrement by.</summary>
        /// <remarks>If the image is a cube map, then this value will be 6, otherwise it will be 1.</remarks>
        public int ArrayIndexStep
        {
            get => _arrayIndexStep;
            private set
            {
                if (_arrayIndexStep == value)
                {
                    return;
                }

                OnPropertyChanging();
                _arrayIndexStep = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return which mode is active.</summary>
        public CropResizeMode CurrentMode
        {
            get => _mode;
            set
            {
                if (_mode == value)
                {
                    return;
                }

                OnPropertyChanging();
                _mode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the currently selected cropping alignment.</summary>
        public Alignment CropAlignment
        {
            get => _cropAlignment;
            set
            {
                if (_cropAlignment == value)
                {
                    return;
                }

                OnPropertyChanging();
                _cropAlignment = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the image filter to use when resizing.</summary>
        public ImageFilter ImageFilter
        {
            get => _resizeFilter;
            set
            {
                if (_resizeFilter == value)
                {
                    return;
                }

                OnPropertyChanging();
                _resizeFilter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the command used to cancel the operation.</summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
            set;
        }

        /// <summary>Property to set or return the command used to apply the operation.</summary>
        public IEditorCommand<object> OkCommand
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(DimensionSettingsParameters injectionParameters) => 
            _videoAdapter = injectionParameters.VideoAdapter ?? throw new ArgumentMissingException(nameof(DimensionSettingsParameters.VideoAdapter), nameof(injectionParameters));
        #endregion
    }
}
