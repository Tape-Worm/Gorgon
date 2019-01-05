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
// Created: January 4, 2019 10:10:47 PM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using Gorgon.Editor.Content;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides I/O functionality for reading/writing image data.
    /// </summary>
    internal interface IImageIOService
    {
        #region Properties.
        /// <summary>
        /// Property to return whether or not block compression is supported.
        /// </summary>
        bool CanHandleBlockCompression
        {
            get;
        }

        /// <summary>
        /// Property to return the list of installed image codecs.
        /// </summary>
        IReadOnlyList<IGorgonImageCodec> InstalledCodecs
        {
            get;
        }

        /// <summary>
        /// Property to return the default plugin codec.
        /// </summary>
        IGorgonImageCodec DefaultCodec
        {
            get;
        }

        /// <summary>
        /// Property to return the file system writer used to write to the temporary area.
        /// </summary>
        IGorgonFileSystemWriter<Stream> ScratchArea
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to save the specified image file.
        /// </summary>
        /// <param name="name">The name of the file to write into.</param>
        /// <param name="image">The image to save.</param>
        /// <param name="pixelFormat">The pixel format for the image.</param>
        /// <returns>The updated working file.</returns>
        IGorgonVirtualFile SaveImageFile(string name, IGorgonImage image, BufferFormat pixelFormat);

        /// <summary>
        /// Function to perform an export of an image.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="image">The image data to export.</param>
        /// <param name="codec">The codec to use when encoding the image.</param>
        /// <returns>The path to the exported file.</returns>
        FileInfo ExportImage(IContentFile file, IGorgonImage image, IGorgonImageCodec codec);

        /// <summary>
        /// Function to load the image file into memory.
        /// </summary>
        /// <param name="file">The stream for the file to load.</param>
        /// <param name="name">The name of the file.</param>
        /// <returns>The image data, the virtual file entry for the working file and the original pixel format of the file.</returns>
        (IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) LoadImageFile(Stream file, string name);
        #endregion
    }
}