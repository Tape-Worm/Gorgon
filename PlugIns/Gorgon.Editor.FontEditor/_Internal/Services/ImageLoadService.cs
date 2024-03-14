#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: September 10, 2021 8:45:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A service for loading images.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ImageLoadService" /> class.</remarks>
/// <param name="codec">The codec.</param>
/// <param name="fileManager">The file manager.</param>
internal class ImageLoadService(IGorgonImageCodec codec, IContentFileManager fileManager)
{
    #region Variables.
    // The file manager for the application.
    private readonly IContentFileManager _fileManager = fileManager;
    // The codec for the images.
    private readonly IGorgonImageCodec _codec = codec;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to return whether the file in the path is an image file or not.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns><b>true</b> if the file is an image, <b>false</b> if not.</returns>
    public bool IsImageFile(string filePath)
    {
        if (!_fileManager.FileExists(filePath))
        {
            return false;
        }

        // Is this an image file?
        IContentFile imageFile = _fileManager.GetFile(filePath);

        return ((imageFile.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                && (string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Function to determine if the file can be used as a texture for a texture glyph brush.
    /// </summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns><b>true</b> if the file is suitable for use, <b>false</b> if not.</returns>
    public bool CanBeUsedForGlyphBrush(string filePath)
    {
        if (!IsImageFile(filePath))
        {
            return false;
        }

        using Stream stream = _fileManager.OpenStream(filePath, FileMode.Open);

        if (!_codec.IsReadable(stream))
        {
            return false;
        }

        IGorgonImageInfo metadata = _codec.GetMetaData(stream);

        stream.Close();

        GorgonFormatInfo formatInfo = new(metadata.Format);

        return ((metadata.Format == BufferFormat.R8G8B8A8_UNorm_SRgb)
            || (metadata.Format == BufferFormat.R8G8B8A8_UNorm)
            || (metadata.Format == BufferFormat.B8G8R8A8_UNorm)
            || (metadata.Format == BufferFormat.B8G8R8A8_UNorm_SRgb)
            || (formatInfo.IsCompressed));
    }

    /// <summary>
    /// Function to load an image.
    /// </summary>
    /// <param name="filePath">The path to the image to load.</param>
    /// <returns>The image data.</returns>
    public Task<IGorgonImage> LoadImageAsync(string filePath)
    {
        IGorgonImage LoadImage()
        {
            using Stream stream = _fileManager.OpenStream(filePath, FileMode.Open);
            IGorgonImage result = _codec.FromStream(stream);

            // Convert from compressed should we be using compression on this image.
            if (result.FormatInfo.IsCompressed)
            {
                result.Decompress(true);
            }

            return result;
        }

        return Task.Run(LoadImage);
    }

    #endregion
}
