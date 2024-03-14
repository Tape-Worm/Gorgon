#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 14, 2020 12:33:11 PM
// 
#endregion

using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Data for the image picker import files.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ImagePickerImportData"/> class.</remarks>
/// <param name="originalFilePath">The original file path.</param>
/// <param name="fromFile">From file.</param>
/// <param name="thumbnail">The thumbnail.</param>
/// <param name="originalMetadata">The original size of the image.</param>
internal class ImagePickerImportData(string originalFilePath, IGorgonVirtualFile fromFile, IGorgonImage thumbnail, IGorgonImageInfo originalMetadata)
{
    #region Properties.
    /// <summary>
    /// Property to return the image containing the thumbnail.
    /// </summary>
    public IGorgonImage Thumbnail
    {
        get;
        private set;
    } = thumbnail;

    /// <summary>
    /// Property to return the file to work with.
    /// </summary>
    public IGorgonVirtualFile FromFile
    {
        get;
    } = fromFile;

    /// <summary>
    /// Property to return the source file that was imported.
    /// </summary>
    public string OriginalFilePath
    {
        get;
    } = originalFilePath;

    /// <summary>
    /// Property to return the original metadata of the image that is used to generate the thumbnail.
    /// </summary>
    public IGorgonImageInfo OriginalMetadata
    {
        get;
    } = originalMetadata;

    #endregion
    #region Constructor.
    #endregion
}
