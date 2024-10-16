﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 24, 2017 12:20:41 AM
// 

using Gorgon.Core;
using Gorgon.Graphics.Fonts.Properties;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts.Codecs;

/// <summary>
/// The base class used to define functionality to allow applications to write their own codecs for reading/writing font data
/// </summary>
/// <remarks>
/// <para>
/// Implementors that want to create their own font import/export functionality should do so by inheriting from this class. It contains functionality that will allow the fonts to be created and registered 
/// with a <seealso cref="GorgonFontFactory"/>
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonFontCodec"/> class
/// </remarks>
/// <param name="factory">The font factory that holds cached font information.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="factory"/> parameter is <b>null</b>.</exception>
public abstract class GorgonFontCodec(GorgonFontFactory factory)
        : IGorgonFontCodec
{
    /// <summary>
    /// Property to return the font factory containing cached font data.
    /// </summary>
    protected GorgonFontFactory Factory
    {
        get;
    } = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>
    /// Property to return the default filename extension for font files.
    /// </summary>
    public abstract string DefaultFileExtension
    {
        get;
    }

    /// <summary>
    /// Property to return whether the codec supports decoding of font data.
    /// </summary>
    /// <remarks>
    /// If this value is <b>false</b>, then the codec is effectively write only.
    /// </remarks>
    public abstract bool CanDecode
    {
        get;
    }

    /// <summary>
    /// Property to return whether the codec supports encoding of font data.
    /// </summary>
    /// <remarks>
    /// If this value is <b>false</b>, then the codec is effectively read only.
    /// </remarks>
    public abstract bool CanEncode
    {
        get;
    }

    /// <summary>
    /// Property to return whether the codec supports fonts with outlines.
    /// </summary>
    public abstract bool SupportsFontOutlines
    {
        get;
    }

    /// <summary>
    /// Property to return the common file name extension(s) for a codec.
    /// </summary>
    public IReadOnlyList<string> CodecCommonExtensions
    {
        get;
        protected set;
    } = [];

    /// <summary>
    /// Property to return the friendly description of the codec.
    /// </summary>
    public abstract string CodecDescription
    {
        get;
    }

    /// <summary>
    /// Property to return the abbreviated name of the codec (e.g. GorFont).
    /// </summary>
    public abstract string Codec
    {
        get;
    }

    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    /// <remarks>
    /// For best practises, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
    /// property.
    /// </remarks>
    string IGorgonNamedObject.Name => Codec;

    /// <summary>
    /// Function to write the font data to the stream.
    /// </summary>
    /// <param name="fontData">The font data to write.</param>
    /// <param name="stream">The stream to write into.</param>
    /// <remarks>
    /// <para>
    /// Implementors must override this method to write out the font data in the expected format.
    /// </para>
    /// </remarks>
    protected abstract void OnWriteFontData(GorgonFont fontData, Stream stream);

    /// <summary>
    /// Function to read the meta data for font data within a stream.
    /// </summary>
    /// <param name="stream">The stream containing the metadata to read.</param>
    /// <returns>
    /// The font meta data as a <see cref="GorgonFontInfo"/> value.
    /// </returns>
    protected abstract GorgonFontInfo OnGetMetaData(Stream stream);

    /// <summary>
    /// Function to load the font data, with the specified size, from a stream.
    /// </summary>
    /// <param name="name">The name to assign to the font.</param>
    /// <param name="stream">The stream containing the font data.</param>
    /// <returns>A new <seealso cref="GorgonFont"/>, or, an existing font from the <seealso cref="GorgonFontFactory"/> cache.</returns>
    protected abstract GorgonFont OnLoadFromStream(string name, Stream stream);

    /// <summary>
    /// Function to load the font data, with the specified size, from a stream.
    /// </summary>
    /// <param name="name">The name to assign to the font.</param>
    /// <param name="stream">The stream containing the font data.</param>
    /// <returns>A new <seealso cref="GorgonFont"/>, or, an existing font from the <seealso cref="GorgonFontFactory"/> cache.</returns>
    protected abstract Task<GorgonFont> OnLoadFromStreamAsync(string name, Stream stream);

    /// <summary>
    /// Function to read the meta data for font data within a stream.
    /// </summary>
    /// <param name="stream">The stream containing the metadata to read.</param>
    /// <returns>
    /// The font meta data as a <see cref="GorgonFontInfo"/> value.
    /// </returns>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
    /// <para>-or-</para>
    /// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    public GorgonFontInfo GetMetaData(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
        }

        if (!stream.CanSeek)
        {
            throw new IOException(Resources.GORGFX_ERR_STREAM_NO_SEEK);
        }

        if (stream.Length - stream.Position < sizeof(ulong))
        {
            throw new EndOfStreamException();
        }

        long streamPosition = stream.Position;

        try
        {
            return OnGetMetaData(stream);
        }
        finally
        {
            stream.Position = streamPosition;
        }
    }

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
    /// Implementors should ensure that the stream position is restored prior to exiting this method. Failure to do so may cause problems when reading the data from the stream.
    /// </para>
    /// </remarks>
    public abstract bool IsReadable(Stream stream);

    /// <summary>
    /// Function to asynchronously load a font from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the font data to read.</param>
    /// <param name="name">The name of the font.</param>
    /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the amount of data requested exceeds the size of the stream minus its current position.</exception>
    public async Task<GorgonFont> FromStreamAsync(Stream stream, string name)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY, nameof(stream));
        }

        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        Stream externalStream = stream;

        try
        {
            if (!stream.CanSeek)
            {
                externalStream = new DX.DataStream((int)stream.Length, true, true);
                stream.CopyTo(externalStream);
                externalStream.Position = 0;
            }

            GorgonFont result = await OnLoadFromStreamAsync(name, externalStream);

            return result;
        }
        finally
        {
            if (externalStream != stream)
            {
                externalStream.Dispose();
            }
        }
    }

    /// <summary>
    /// Function to load a font from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the font data to read.</param>
    /// <param name="name">The name of the font.</param>
    /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the amount of data requested exceeds the size of the stream minus its current position.</exception>
    public GorgonFont FromStream(Stream stream, string name)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY, nameof(stream));
        }

        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        Stream externalStream = stream;

        try
        {
            if (!stream.CanSeek)
            {
                externalStream = new DX.DataStream((int)stream.Length, true, true);
                stream.CopyTo(externalStream);
                externalStream.Position = 0;
            }

            return OnLoadFromStream(name, externalStream);
        }
        finally
        {
            if (externalStream != stream)
            {
                externalStream.Dispose();
            }
        }
    }

    /// <summary>
    /// Function to persist a <see cref="GorgonFont"/> to a file on the physical file system.
    /// </summary>
    /// <param name="fontData">A <see cref="GorgonFont"/> to persist to the stream.</param>
    /// <param name="filePath">The path to the file that will hold the font data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="fontData"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> is empty..</exception>
    /// <exception cref="GorgonException">Thrown when the font data in the stream has a pixel format that is unsupported.</exception>
    public void Save(GorgonFont fontData, string filePath)
    {
        FileStream stream = null;

        if (fontData is null)
        {
            throw new ArgumentNullException(nameof(fontData));
        }

        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        try
        {
            stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            OnWriteFontData(fontData, stream);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    /// <summary>
    /// Function to persist a <see cref="GorgonFont"/> to a stream.
    /// </summary>
    /// <param name="fontData">A <see cref="GorgonFont"/> to persist to the stream.</param>
    /// <param name="stream">The stream that will receive the font data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="fontData"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="GorgonException">Thrown when the font data in the stream has a pixel format that is unsupported.</exception>
    public void Save(GorgonFont fontData, Stream stream)
    {
        if (fontData is null)
        {
            throw new ArgumentNullException(nameof(fontData));
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanWrite)
        {
            throw new IOException(Resources.GORGFX_ERR_STREAM_READ_ONLY);
        }

        if (!stream.CanSeek)
        {
            throw new IOException(Resources.GORGFX_ERR_STREAM_NO_SEEK);
        }

        OnWriteFontData(fontData, stream);
    }

    /// <summary>
    /// Function to asynchronously load a font from a file on the physical file system.
    /// </summary>
    /// <param name="filePath">Path to the file to load.</param>
    /// <param name="name">[Optional] The name of the font.</param>
    /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    public async Task<GorgonFont> FromFileAsync(string filePath, string name = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = filePath;
        }

        using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await OnLoadFromStreamAsync(name, stream);
    }

    /// <summary>
    /// Function to load a font from a file on the physical file system.
    /// </summary>
    /// <param name="filePath">Path to the file to load.</param>
    /// <param name="name">[Optional] The name of the font.</param>
    /// <returns>A <see cref="GorgonFont"/> containing the font data from the stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    public GorgonFont FromFile(string filePath, string name = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = filePath;
        }

        using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return OnLoadFromStream(name, stream);
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GORGFX_TOSTR_FONT_CODEC, Codec);

}
