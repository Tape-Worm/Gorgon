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
// Created: January 16, 2019 8:37:40 AM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The mode for the cropping/resizing of an image.
    /// </summary>
    [Flags]
    internal enum CropResizeMode
    {
        /// <summary>
        /// No mode selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// Image should be cropped.
        /// </summary>
        Crop = 1,
        /// <summary>
        /// Image should be resized.
        /// </summary>
        Resize = 2
    }

    /// <summary>
    /// The view model for the image cropping/resizing settings view.
    /// </summary>
    internal interface ICropResizeSettings
        : IHostedPanelViewModel
    {
        /// <summary>
        /// Property to set or return the file being imported.
        /// </summary>
        string ImportFile
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the directory containing the file being imported.
        /// </summary>
        string ImportFileDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the image being imported.
        /// </summary>
        IGorgonImage ImportImage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the width/height of the target image.
        /// </summary>
        DX.Size2 TargetImageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return which modes are allowed.
        /// </summary>
        CropResizeMode AllowedModes
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return which mode is active.
        /// </summary>
        CropResizeMode CurrentMode
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the currently selected alignment.
        /// </summary>
        Alignment CurrentAlignment
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to preserve the aspect ratio of the image being resized.
        /// </summary>
        bool PreserveAspect
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the image filter to use when resizing.
        /// </summary>
        ImageFilter ImageFilter
        {
            get;
            set;
        }
    }
}
