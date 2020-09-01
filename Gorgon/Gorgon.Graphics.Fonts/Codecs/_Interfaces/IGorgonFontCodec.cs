#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 23, 2017 10:26:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gorgon.Core;

namespace Gorgon.Graphics.Fonts.Codecs
{
    /// <summary>
    /// A encoder/decoder for reading and writing font data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The font codec is used to encode and/or decode font data stored in a binary blob. By implementing this interface, applications can load and/or save font formats from applications (e.g. Bmfont). 
    /// </para>
    /// </remarks>
    public interface IGorgonFontCodec
        : IGorgonNamedObject
    {
        #region Properties.
        /// <summary>
        /// Property to return the common file name extension(s) for a codec.
        /// </summary>
        IReadOnlyList<string> CodecCommonExtensions
        {
            get;
        }

        /// <summary>
        /// Property to return the friendly description of the codec.
        /// </summary>
        string CodecDescription
        {
            get;
        }

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        string Codec
        {
            get;
        }

        /// <summary>
        /// Property to return the default filename extension for font files.
        /// </summary>
        string DefaultFileExtension
        {
            get;
        }

        /// <summary>
        /// Property to return whether the codec supports encoding of font data.
        /// </summary>
        /// <remarks>
        /// If this value is <b>false</b>, then the codec is effectively read only.
        /// </remarks>
        bool CanEncode
        {
            get;
        }

        /// <summary>
        /// Property to return whether the codec supports decoding of font data.
        /// </summary>
        /// <remarks>
        /// If this value is <b>false</b>, then the codec is effectively write only.
        /// </remarks>
        bool CanDecode
        {
            get;
        }

        /// <summary>
        /// Property to return whether the codec supports fonts with outlines.
        /// </summary>
        bool SupportsFontOutlines
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load an font from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the font data to read.</param>
        /// <param name="name">[Optional] The name of the font.</param>
        /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>
        [Obsolete("Use FromStream(Stream, string) instead.")]
        GorgonFont LoadFromStream(Stream stream, string name = null);

        /// <summary>
        /// Function to load an font from a file on the physical file system.
        /// </summary>
        /// <param name="filePath">Path to the file to load.</param>
        /// <param name="name">[Optional] The name of the font.</param>
        /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>
        [Obsolete("Use FromFile(string, string) instead.")]
        GorgonFont LoadFromFile(string filePath, string name = null);

        /// <summary>
        /// Function to persist a <see cref="GorgonFont"/> to a stream.
        /// </summary>
        /// <param name="fontData">A <see cref="GorgonFont"/> to persist to the stream.</param>
        /// <param name="stream">The stream that will receive the font data.</param>
        [Obsolete("Use Save(GorgonFont, Stream) instead.")]
        void SaveToStream(GorgonFont fontData, Stream stream);

        /// <summary>
        /// Function to persist a <see cref="GorgonFont"/> to a file on the physical file system.
        /// </summary>
        /// <param name="fontData">A <see cref="GorgonFont"/> to persist to the stream.</param>
        /// <param name="filePath">The path to the file that will hold the font data.</param>
        [Obsolete("Use Save(GorgonFont, string) instead.")]
        void SaveToFile(GorgonFont fontData, string filePath);

        /// <summary>
        /// Function to load an font from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the font data to read.</param>
        /// <param name="name">The name of the font.</param>
        /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>        
        GorgonFont FromStream(Stream stream, string name);

        /// <summary>
        /// Function to load an font from a file on the physical file system.
        /// </summary>
        /// <param name="filePath">Path to the file to load.</param>
        /// <param name="name">[Optional] The name of the font.</param>
        /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>        
        GorgonFont FromFile(string filePath, string name = null);

        /// <summary>
        /// Function to persist a <see cref="GorgonFont"/> to a stream.
        /// </summary>
        /// <param name="fontData">A <see cref="GorgonFont"/> to persist to the stream.</param>
        /// <param name="stream">The stream that will receive the font data.</param>        
        void Save(GorgonFont fontData, Stream stream);

        /// <summary>
        /// Function to persist a <see cref="GorgonFont"/> to a file on the physical file system.
        /// </summary>
        /// <param name="fontData">A <see cref="GorgonFont"/> to persist to the stream.</param>
        /// <param name="filePath">The path to the file that will hold the font data.</param>
        void Save(GorgonFont fontData, string filePath);

        /// <summary>
        /// Function to determine if this codec can read the font data within the stream or not.
        /// </summary>
        /// <param name="stream">The stream that is used to read the font data.</param>
        /// <returns><b>true</b> if the codec can read the file, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>
        /// <para>
        /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so may cause 
        /// undesirable results or an exception. 
        /// </para>
        /// </remarks>
        bool IsReadable(Stream stream);

        /// <summary>
        /// Function to read the meta data for font data within a stream.
        /// </summary>
        /// <param name="stream">The stream containing the metadata to read.</param>
        /// <returns>
        /// The font meta data as a <see cref="IGorgonFontInfo"/> value.
        /// </returns>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
        /// <para>-or-</para>
        /// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>
        /// <para>
        /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so 
        /// may cause undesirable results.
        /// </para> 
        /// </remarks>
        IGorgonFontInfo GetMetaData(Stream stream);
        #endregion
    }
}
