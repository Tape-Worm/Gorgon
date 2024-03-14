
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 5, 2019 7:46:30 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditor.ViewModels;

/// <summary>
/// The view model for the image dimensions settings view
/// </summary>
internal interface IDimensionSettings
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return whether mip maps are supported for the current image pixel format.
    /// </summary>
    bool MipSupport
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether the image has depth slices or not.
    /// </summary>
    /// <remarks>
    /// This will return <b>true</b> if the image is a volume texture, or <b>false</b> if it is not.  If this returns <b>false</b>, then the image uses array indices instead of depth slices.
    /// </remarks>
    bool HasDepth
    {
        get;
    }

    /// <summary>
    /// Property to set or return the width of the image, in pixels.
    /// </summary>
    int Width
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the height of the image, in pixels.
    /// </summary>
    int Height
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of mip map levels.
    /// </summary>
    int MipLevels
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of depth slices if the image is a volume texture, or array indices if not.
    /// </summary>
    int DepthSlicesOrArrayIndices
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the maximum width of an image.
    /// </summary>
    int MaxWidth
    {
        get;
    }

    /// <summary>
    /// Property to return the maximum height of an image.
    /// </summary>
    int MaxHeight
    {
        get;
    }

    /// <summary>
    /// Property to return the maximum number of depth slices for a volume texture, or array indices for a 2D image.
    /// </summary>
    int MaxDepthOrArrayIndices
    {
        get;
    }

    /// <summary>
    /// Property to return the maximum number of mip map levels based on the width, height and, if applicable, depth slices.
    /// </summary>
    int MaxMipLevels
    {
        get;
    }

    /// <summary>
    /// Property to return the number of array indices to increment or decrement by.
    /// </summary>
    /// <remarks>
    /// If the image is a cube map, then this value will be 6, otherwise it will be 1.
    /// </remarks>
    int ArrayIndexStep
    {
        get;
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
    /// Property to set or return the currently selected cropping alignment.
    /// </summary>
    Alignment CropAlignment
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

    /// <summary>
    /// Property to return the command to execute for updating the current image information.
    /// </summary>
    IEditorCommand<IGorgonImage> UpdateImageInfoCommand
    {
        get;
    }
}
