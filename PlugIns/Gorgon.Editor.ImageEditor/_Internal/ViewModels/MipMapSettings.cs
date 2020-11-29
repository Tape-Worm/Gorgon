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
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The view model for the image dimensions editor.
    /// </summary>
    internal class MipMapSettings
        : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IMipMapSettings
    {
        #region Variables.
        // The maximum number of mip map levels.
        private int _maxMipLevels = int.MaxValue;
        // The current resizing filter.
        private ImageFilter _resizeFilter = ImageFilter.Fant;
        // The number of mip map levels in the image.
        private int _mipLevels = 2;
        #endregion

        #region Properties.
        /// <summary>Property to return whether the panel is modal.</summary>
        public override bool IsModal => true;

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

        /// <summary>Property to set or return the image filter to use when resizing.</summary>
        public ImageFilter MipFilter
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
        /// Property to return the command to execute for updating the current image information.
        /// </summary>
        public IEditorCommand<IGorgonImage> UpdateImageInfoCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the maximum mip map count for the current spatial dimensions.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="depth">The depth slices in the image.</param>
        private void UpdateMaxMips(int width, int height, int depth)
        {
            try
            {
                int maxMips = GorgonImage.CalculateMaxMipCount(width, height, depth);
                _mipLevels = _mipLevels.Min(maxMips).Max(2);
                MaxMipLevels = maxMips;
                NotifyPropertyChanged(nameof(MipLevels));
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_IMAGE_INFO);
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
                    MaxMipLevels = int.MaxValue;
                    MipLevels = 2;
                    return;
                }

                _mipLevels = image.MipCount;
                UpdateMaxMips(image.Width, image.Height, image.Depth);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_IMAGE_INFO);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
        {
        
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="Gorgon.Editor.ImageEditor.ViewModels.DimensionSettings"/> class.</summary>
        public MipMapSettings() => UpdateImageInfoCommand = new EditorCommand<IGorgonImage>(DoUpdateImageInfo);
        #endregion
    }
}
