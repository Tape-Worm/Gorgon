
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
// Created: February 14, 2020 6:29:02 PM
// 


using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Arguments for the <see cref="IImagePicker.ActivateCommand"/>
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ActivateImagePickerArgs"/> class.</remarks>
/// <param name="files">The files to import.</param>
/// <param name="imageData">The image data to update.</param>
internal class ActivateImagePickerArgs(IReadOnlyList<ImagePickerImportData> files, IGorgonImage imageData)
{
    /// <summary>
    /// Property to set or return whether the picker cancelled or not.
    /// </summary>
    public bool Cancel
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of files being imported.
    /// </summary>
    public IReadOnlyList<ImagePickerImportData> FilesToImport
    {
        get;
    } = files;

    /// <summary>
    /// Property to set or return the current image data to update.
    /// </summary>
    public IGorgonImage ImageData
    {
        get;
    } = imageData;

    /// <summary>
    /// Property to set or return the current array index or depth slice.
    /// </summary>
    public int CurrentArrayIndexDepthSlice
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the current mip map level.
    /// </summary>
    public int MipLevel
    {
        get;
        set;
    }
}
