
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: February 18, 2020 7:14:37 PM
// 

using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The view model for picking a source image subresource
/// </summary>
internal interface ISourceImagePicker
    : IViewModel
{
    /// <summary>
    /// Property to set or return the name of the image.
    /// </summary>
    string ImageName
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the source image being imported.
    /// </summary>
    IGorgonImage SourceImage
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the number of array indices.
    /// </summary>
    int ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return the number of mip levels.
    /// </summary>
    int MipCount
    {
        get;
    }

    /// <summary>
    /// Property to return the width of the image at the current mip level.
    /// </summary>
    int MipWidth
    {
        get;
    }

    /// <summary>
    /// Property to return the height of the image at the current mip level.
    /// </summary>
    int MipHeight
    {
        get;
    }

    /// <summary>
    /// Property to return the depth of the 3D image at the current mip level.
    /// </summary>
    int MipDepth
    {
        get;
    }

    /// <summary>
    /// Property to return the current array index (for 2D images), or the current depth slice (for 3D images).
    /// </summary>
    int CurrentArrayIndexDepthSlice
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the current mip map level.
    /// </summary>
    int CurrentMipLevel
    {
        get;
        set;
    }
}
