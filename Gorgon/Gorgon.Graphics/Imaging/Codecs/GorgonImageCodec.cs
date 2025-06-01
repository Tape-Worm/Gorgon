
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: Monday, June 27 2016 5:59:03 PM
// 

using Gorgon.Configuration;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <inheritdoc cref="IGorgonImageCodec"/>
public abstract class GorgonImageCodec<TEncOpt, TDecOpt>(TEncOpt encodingOptions, TDecOpt decodingOptions)
    : IGorgonImageCodec
    where TEncOpt : class, IGorgonImageCodecEncodingOptions
    where TDecOpt : class, IGorgonImageCodecDecodingOptions
{
    /// <summary>
    /// Default, empty decoding options.
    /// </summary>
    private class EmptyDecodingOptions
        : IGorgonImageCodecDecodingOptions
    {
        /// <inheritdoc/>
        public bool ReadAllFrames
        {
            get => false;
            set
            {
            }
        }

        /// <inheritdoc/>
        public IGorgonOptionBag Options => new GorgonOptionBag([]);
    }

    /// <summary>
    /// Default, empty encoding options.
    /// </summary>
    private class EmptyEncodingOptions
        : IGorgonImageCodecEncodingOptions
    {
        /// <inheritdoc/>
        public bool SaveAllFrames
        {
            get => false;
            set
            {
            }
        }

        /// <inheritdoc/>
        public IGorgonOptionBag Options => new GorgonOptionBag([]);
    }

    /// <summary>
    /// Default, empty options for decoding image data.
    /// </summary>
    protected static readonly IGorgonImageCodecDecodingOptions DefaultDecodingOptions = new EmptyDecodingOptions();

    /// <summary>
    /// Default, empty options for encoding image data.
    /// </summary>
    protected static readonly IGorgonImageCodecEncodingOptions DefaultEncodingOptions = new EmptyEncodingOptions();

    /// <summary>
    /// Property to return the encoding options for the codec.
    /// </summary>
    protected TEncOpt EncodingOptions
    {
        get;
    } = encodingOptions;

    /// <summary>
    /// Property to return the decoding options for the codec.
    /// </summary>
    protected TDecOpt DecodingOptions
    {
        get;
    } = decodingOptions;

    /// <inheritdoc/>
    public virtual bool SupportsMultipleFrames => false;

    /// <inheritdoc/>
    public virtual bool CanEncode => true;

    /// <inheritdoc/>
    public virtual bool CanDecode => true;

    /// <inheritdoc/>
    public IReadOnlyList<string> CodecCommonExtensions
    {
        get;
        protected set;
    } = [];

    /// <inheritdoc/>
    public abstract string CodecDescription
    {
        get;
    }

    /// <inheritdoc/>
    public abstract string Codec
    {
        get;
    }

    /// <inheritdoc/>
    public abstract IReadOnlyList<BufferFormat> SupportedPixelFormats
    {
        get;
    }

    /// <inheritdoc/>
    public abstract bool SupportsDepth
    {
        get;
    }

    /// <inheritdoc/>
    public abstract bool SupportsMipMaps
    {
        get;
    }

    /// <inheritdoc/>
    public abstract bool SupportsBlockCompression
    {
        get;
    }

    /// <inheritdoc/>
    string IGorgonNamedObject.Name => Codec;

    /// <summary>
    /// Function to decode an image from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the image data to read.</param>
    /// <param name="size">The size of the image within the stream, in bytes.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
    /// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
    /// <remarks>
    /// <para>
    /// A codec must implement this method in order to decode the image data. 
    /// </para>
    /// </remarks>
    protected abstract IGorgonImage OnDecodeFromStream(Stream stream, long size);

    /// <inheritdoc/>
    public IGorgonImage FromStream(Stream stream, long? size = null)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        size ??= stream.Length;

        if (size + stream.Position > stream.Length)
        {
            throw new EndOfStreamException();
        }

        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), Resources.GORIMG_ERR_IMAGE_BYTE_LENGTH_TOO_SHORT);
        }

        long basePosition = stream.Position;

        Stream externalStream = stream;
        GorgonNativeBuffer<byte>? tempBuffer = null;

        try
        {
            if (!stream.CanSeek)
            {
                tempBuffer = new GorgonNativeBuffer<byte>(size.Value);
                externalStream = new GorgonMemoryStream<byte>(tempBuffer);
                stream.CopyTo(externalStream, (int)size);
                externalStream.Position = 0;
            }

            IGorgonImage result = OnDecodeFromStream(externalStream, size.Value);

            // Move the base stream to the number of bytes written (this is already done if we've copied the stream into memory above).
            if (stream.Position < basePosition + size)
            {
                stream.Position = basePosition + size.Value;
            }

            return result;
        }
        finally
        {
            tempBuffer?.Dispose();

            if (externalStream != stream)
            {
                externalStream?.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    public IGorgonImage FromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return FromStream(stream);
    }

    /// <inheritdoc/>
    public abstract void Save(IGorgonImage imageData, Stream stream);

    /// <inheritdoc/>
    public void Save(IGorgonImage imageData, string filePath)
    {

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        Save(imageData, stream);
    }

    /// <inheritdoc/>
    public abstract bool IsReadable(Stream stream);

    /// <inheritdoc/>
    public abstract GorgonImageInfo GetMetaData(Stream stream);

    /// <inheritdoc/>
    public override string ToString() => string.Format(Resources.GORIMG_TOSTR_IMAGE_CODEC, Codec);
}
