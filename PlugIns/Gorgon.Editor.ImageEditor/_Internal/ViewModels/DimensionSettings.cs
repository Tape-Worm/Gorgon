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
// Created: February 5, 2019 7:54:23 PM
// 
#endregion

using System;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
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
        private int _maxWidth = int.MaxValue;
        // The maximum height.
        private int _maxHeight = int.MaxValue;
        // The maximum depth slices or array indices.
        private int _maxDepthOrArray = int.MaxValue;
        // The video adapter used by the application.
        private IGorgonVideoAdapterInfo _videoAdapter;
        // The maximum number of mip map levels.
        private int _maxMipLevels = int.MaxValue;
        // Flag to indicate that the editor is active.
        private bool _isActive;
        // The current cropping alignment.
        private Alignment _cropAlignment = Alignment.Center;
        // The current resizing filter.
        private ImageFilter _resizeFilter = ImageFilter.Fant;
        // The current mode of operation.
        private CropResizeMode _mode = CropResizeMode.Crop;
        // The current stepping for the array index.
        private int _arrayIndexStep = 1;
        // The width of the image.
        private int _width = 1;
        // The height of the image.
        private int _height = 1;
        // The number of mip map levels in the image.
        private int _mipLevels = 1;
        // The number of depth slices or array indices in the image.
        private int _depthSlicesOrArrayIndices = 1;
        // The service used to display message notifications.
        private IMessageDisplayService _messageDisplay;
        private bool _mipSupport;
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

                NotifyPropertyChanged(nameof(MaxWidth));
                NotifyPropertyChanged(nameof(MaxHeight));
                NotifyPropertyChanged(nameof(MaxMipLevels));
                NotifyPropertyChanged(nameof(MaxDepthOrArrayIndices));
            }
        }

        /// <summary>Property to set or return the width of the image, in pixels.</summary>
        public int Width
        {
            get => _width;
            set
            {
                if (_width == value)
                {
                    return;
                }

                OnPropertyChanging();
                _width = value;
                OnPropertyChanged();

                UpdateMaxMips();
            }
        }

        /// <summary>Property to set or return the height of the image, in pixels.</summary>
        public int Height
        {
            get => _height;
            set
            {
                if (_height == value)
                {
                    return;
                }

                OnPropertyChanging();
                _height = value;
                OnPropertyChanged();

                UpdateMaxMips();
            }
        }

        /// <summary>Property to set or return the number of mip map levels.</summary>
        public int MipLevels
        {
            get => _mipLevels;
            set
            {
                if (_mipLevels == value)
                {
                    return;
                }

                OnPropertyChanging();
                _mipLevels = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the number of depth slices if the image is a volume texture, or array indices if not.</summary>
        public int DepthSlicesOrArrayIndices
        {
            get => _depthSlicesOrArrayIndices;
            set
            {
                if (_depthSlicesOrArrayIndices == value)
                {
                    return;
                }

                OnPropertyChanging();
                _depthSlicesOrArrayIndices = value;
                OnPropertyChanged();

                UpdateMaxMips();
            }
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

        /// <summary>
        /// Property to set or return whether mip maps are supported for the current image pixel format.
        /// </summary>
        public bool MipSupport
        {
            get => _mipSupport;
            set
            {
                if (_mipSupport == value)
                {
                    return;
                }

                OnPropertyChanging();
                _mipSupport = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command to execute for updating the current image information.
        /// </summary>
        public IEditorCommand<IGorgonImage> UpdateImageInfoCommand
        {
            get;
        }

        /// <summary>Property to set or return the command used to cancel the operation.</summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
        }

        /// <summary>Property to set or return the command used to apply the operation.</summary>
        public IEditorCommand<object> OkCommand
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the maximum mip map count for the current spatial dimensions.
        /// </summary>
        private void UpdateMaxMips()
        {
            try
            {
                int maxMips = GorgonImage.CalculateMaxMipCount(Width, Height, HasDepth ? DepthSlicesOrArrayIndices : 1);
                _mipLevels = _mipLevels.Min(maxMips).Max(1);
                NotifyPropertyChanged(nameof(MipLevels));
                MaxMipLevels = maxMips;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORIMG_ERR_IMAGE_INFO);
            }
        }

        /// <summary>
        /// Function called to update the information for the current image.
        /// </summary>
        /// <param name="image">The current image.</param>
        private void DoUpdateImageInfo(IGorgonImage image)
        {
            try
            {
                if (image == null)
                {
                    MaxDepthOrArrayIndices = MaxMipLevels = MaxHeight = MaxWidth = int.MaxValue;
                    ArrayIndexStep = DepthSlicesOrArrayIndices = MipLevels = Height = Width = 1;
                    return;
                }

                MaxDepthOrArrayIndices = image.ImageType == ImageType.Image3D ? _videoAdapter.MaxTexture3DDepth : _videoAdapter.MaxTextureArrayCount;
                MaxHeight = _videoAdapter.MaxTextureHeight;
                MaxWidth = _videoAdapter.MaxTextureWidth;

                _arrayIndexStep = image.ImageType == ImageType.ImageCube ? 6 : 1;                
                _depthSlicesOrArrayIndices = image.ImageType == ImageType.Image3D ? image.Depth : image.ArrayCount;
                _height = image.Height;
                _width = image.Width;
                _mipLevels = image.MipCount;

                NotifyPropertyChanged(nameof(ArrayIndexStep));
                NotifyPropertyChanged(nameof(Width));
                NotifyPropertyChanged(nameof(Height));
                NotifyPropertyChanged(nameof(DepthSlicesOrArrayIndices));

                HasDepth = image.ImageType == ImageType.Image3D;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORIMG_ERR_IMAGE_INFO);
            }
            finally
            {
                UpdateMaxMips();
            }
        }

        /// <summary>
        /// Function to cancel the dimension change operation.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                IsActive = false;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORIMG_ERR_CANCEL_OP);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(DimensionSettingsParameters injectionParameters)
        {
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(DimensionSettingsParameters.MessageDisplay), nameof(injectionParameters));
            _videoAdapter = injectionParameters.VideoAdapter ?? throw new ArgumentMissingException(nameof(DimensionSettingsParameters.VideoAdapter), nameof(injectionParameters));
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ViewModels.DimensionSettings"/> class.</summary>
        public DimensionSettings()
        {
            UpdateImageInfoCommand = new EditorCommand<IGorgonImage>(DoUpdateImageInfo);
            CancelCommand = new EditorCommand<object>(DoCancel);
        }
        #endregion
    }
}
