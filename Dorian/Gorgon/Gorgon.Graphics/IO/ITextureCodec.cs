#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, January 31, 2013 8:17:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
{
    /// <summary>
    /// Defines for loading/saving textures to a stream.
    /// </summary>
    /// <remarks>A codec is used to decode and encode data to and from a data store (such as a <see cref="System.IO.Stream">Stream</see>).  
    /// Using these codecs, we will be able to read/write any image data source as long as there's an implementation for the image format.
    /// <para>Gorgon will have several built-in codecs for Png, Bmp, TGA, dds, Jpg, and Wmp.</para>
    /// </remarks>
    public interface ITextureCodec
    {
        #region Methods.
        /// <summary>
        /// Function to load a texture from a stream.
        /// </summary>
        /// <typeparam name="T">Type of texture to load.  Must be a GorgonTexture.</typeparam>
        /// <param name="stream">Stream containing the data to load.</param>
        /// <param name="sizeInBytes">The size of the texture, in bytes.</param>
        /// <returns>The texture that was in the stream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="sizeInBytes"/> parameter is less than 1 byte.</exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream parameter is write-only.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt is made to read beyond the end of the stream.</exception>
        T LoadFromStream<T>(Stream stream, int sizeInBytes) where T : GorgonTexture;

        /// <summary>
        /// Function to persist a texture to a stream.
        /// </summary>
        /// <typeparam name="T">Type of texture to load.  Must be a GorgonTexture.</typeparam>
        /// <param name="texture">Texture to persist to the stream.</param>
        /// <param name="stream">Stream that will contain the data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="stream"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream parameter is read-only.</exception>
        void SaveToStream<T>(T texture, Stream stream) where T : GorgonTexture;
        #endregion
    }
}
