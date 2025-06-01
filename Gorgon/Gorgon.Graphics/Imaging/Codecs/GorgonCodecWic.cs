
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
// Created: June 27, 2016 7:53:02 PM
// 

using Gorgon.Configuration;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Graphics.Imaging.Wic;
using Gorgon.IO;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Base class for the WIC based file formats (PNG, JPG, and BMP)
/// </summary>
/// <typeparam name="TWicEncOpt">The type of the options object used to provide options when encoding an image. Must be a reference type and implement <see cref="IGorgonWicEncodingOptions"/>.</typeparam>
/// <typeparam name="TWicDecOpt">The type of the options object used to provide options when decoding an image. Must be a reference type and implement <see cref="IGorgonWicDecodingOptions"/>.</typeparam>
/// <remarks>
/// <para>
/// A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
/// image formats, or use one of the predefined image codecs available in Gorgon
/// </para>
/// </remarks>
public abstract class GorgonCodecWic<TWicEncOpt, TWicDecOpt>
    : GorgonImageCodec<TWicEncOpt, TWicDecOpt>
    where TWicEncOpt : class, IGorgonWicEncodingOptions
    where TWicDecOpt : class, IGorgonWicDecodingOptions
{
    /// <summary>
    /// Default decoding options.
    /// </summary>
    private class EmptyDecodingOptions
        : IGorgonWicDecodingOptions
    {
        /// <inheritdoc/>
        public WICFlags Flags
        {
            get => WICFlags.None;
            set
            {
            }
        }

        /// <inheritdoc/>
        public ImageDithering Dithering
        {
            get => ImageDithering.None;
            set
            {
            }
        }

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
    /// Default encoding options.
    /// </summary>
    private class EmptyEncodingOptions
        : IGorgonWicEncodingOptions
    {
        /// <inheritdoc/>
        public ImageDithering Dithering
        {
            get => ImageDithering.None;
            set
            {
            }
        }

        /// <inheritdoc/>
        public double DpiX
        {
            get => 72.0;
            set
            {
            }
        }

        /// <inheritdoc/>
        public double DpiY
        {
            get => 72.0;
            set
            {
            }
        }

        /// <inheritdoc/>
        public bool SaveAllFrames
        {
            get => false;
            set
            {
            }
        }

        /// <inheritdoc/>
        public IGorgonOptionBag Options => throw new NotImplementedException();
    }

    /// <summary>
    /// Default, empty options for decoding image data.
    /// </summary>
    protected static readonly IGorgonWicDecodingOptions DefaultWicDecodingOptions = new EmptyDecodingOptions();

    /// <summary>
    /// Default, empty options for encoding image data.
    /// </summary>
    protected static readonly IGorgonWicEncodingOptions DefaultWicEncodingOptions = new EmptyEncodingOptions();

    // Supported formats.
    private readonly BufferFormat[] _supportedFormats =
    [
        BufferFormat.R8G8B8A8_UNorm,
        BufferFormat.B8G8R8A8_UNorm,
        BufferFormat.B8G8R8X8_UNorm
    ];

    /// <summary>
    /// Property to set or return the file format that is supported by this codec.
    /// </summary>
    protected Guid SupportedFileFormat
    {
        get;
    }

    /// <inheritdoc/>
    public override string Codec
    {
        get;
    }

    /// <inheritdoc/>
    public override string CodecDescription
    {
        get;
    }

    /// <inheritdoc/>
    public override bool SupportsBlockCompression => false;

    /// <inheritdoc/>
    public override bool SupportsMipMaps => false;

    /// <inheritdoc/>
    public override bool SupportsDepth => false;

    /// <inheritdoc/>
    public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _supportedFormats;

    /// <summary>
    /// Property to return the list of names used to locate frame offsets in metadata.
    /// </summary>
    /// <remarks>
    /// Implementors must put the horizontal offset name first, and the vertical name second.  Failure to do so will lead to incorrect offsets.
    /// </remarks>
    protected virtual (string xOffset, string yOffset) GetFrameOffsetMetadataNames() => (string.Empty, string.Empty);

    /// <summary>
    /// Function to retrieve custom metadata when encoding an image frame.
    /// </summary>
    /// <param name="frameIndex">The index of the frame being encoded.</param>
    /// <param name="settings">The settings for the image being encoded.</param>
    /// <returns>A dictionary containing the key/value pair describing the metadata to write to the frame, or <b>null</b> if the frame contains no metadata.</returns>
    protected virtual IReadOnlyDictionary<string, object> GetCustomEncodingMetadata(int frameIndex, IGorgonImageInfo settings) => new Dictionary<string, object>();

    /// <inheritdoc/>
    protected override IGorgonImage OnDecodeFromStream(Stream stream, long size)
    {
        WicUtilities wic = new();
        Stream streamAlias = stream;

        try
        {
            // If we have a stream position that does not begin exactly at the start of the stream, we have to wrap that stream in a 
            // dummy stream wrapper. This is to get around a problem in the underlying COM stream object used by WIC that throws an 
            // exception when the stream position is not exactly 0. 
            if (streamAlias.Position != 0)
            {
                streamAlias = new GorgonStreamSlice(stream, 0, size, stream.CanWrite);
            }

            (string xOffsetName, string yOffsetName) = GetFrameOffsetMetadataNames();

            IGorgonImage result = wic.DecodeImageData(streamAlias, size, SupportedFileFormat, DecodingOptions, xOffsetName, yOffsetName) ?? throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));

            if (stream.Position != streamAlias.Position)
            {
                stream.Position += streamAlias.Position;
            }

            return result;
        }
        finally
        {
            if (streamAlias != stream)
            {
                streamAlias?.Dispose();
            }

            wic.Dispose();
        }
    }

    /// <inheritdoc/>
    public override void Save(IGorgonImage imageData, Stream stream)
    {
        if (!stream.CanWrite)
        {
            throw new IOException(string.Format(Resources.GORIMG_ERR_STREAM_IS_READONLY));
        }

        WicUtilities wic = new();

        try
        {
            if (SupportedPixelFormats.All(item => item != imageData.Format))
            {
                throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
            }

            IReadOnlyDictionary<string, object> metaData = GetCustomEncodingMetadata(0, imageData);
            wic.EncodeImageData(imageData, stream, SupportedFileFormat, EncodingOptions, metaData);
        }
        finally
        {
            wic.Dispose();
        }
    }

    /// <inheritdoc/>
    public override GorgonImageInfo GetMetaData(Stream stream) => GetMetaData(stream, DecodingOptions);

    /// <inheritdoc/>
    public GorgonImageInfo GetMetaData(Stream stream, IGorgonWicDecodingOptions options)
    {
        if (!stream.CanRead)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
        }

        if (!stream.CanSeek)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
        }

        using WicUtilities wic = new();

        // Get our WIC interface.				
        GorgonImageInfo result = wic.GetImageMetaDataFromStream(stream, SupportedFileFormat, options);

        return result.Format == BufferFormat.Unknown
            ? throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, result.Format))
            : result;
    }

    /// <summary>
    /// Function to retrieve the horizontal and vertical offsets for the frames in a multi-frame image.
    /// </summary>
    /// <param name="fileName">The path to the file to retrieve the offsets from.</param>
    /// <returns>A list of <c>Point</c> values that indicate the offset within the image for each frame.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileName"/> parameter is empty.</exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    /// <remarks>
    /// <para>
    /// For image codecs that support multiple frames, this reads a list of offset values for each frame so that the frame can be positioned correctly within the base image. If the image does not have 
    /// multiple frames, or the codec does not support multiple frames, then an empty list is returned.
    /// </para>
    /// </remarks>
    public IReadOnlyList<GorgonPoint> GetFrameOffsets(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentEmptyException(nameof(fileName));
        }

        using FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return GetFrameOffsets(stream);
    }

    /// <summary>
    /// Function to retrieve the horizontal and vertical offsets for the frames in a multi-frame image.
    /// </summary>
    /// <param name="stream">The stream containing the image data.</param>
    /// <returns>A list of <c>Point</c> values that indicate the offset within the image for each frame.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.
    /// <para>-or-</para>
    /// <para>Thrown when the stream cannot perform seek operations.</para>
    /// </exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    /// <remarks>
    /// <para>
    /// For image codecs that support multiple frames, this reads a list of offset values for each frame so that the frame can be positioned correctly within the base image. If the image does not have 
    /// multiple frames, or the codec does not support multiple frames, then an empty list is returned.
    /// </para>
    /// </remarks>
    public IReadOnlyList<GorgonPoint> GetFrameOffsets(Stream stream)
    {
        if (!SupportsMultipleFrames)
        {
            return [];
        }

        using WicUtilities wic = new();

        (string xOffsetName, string yOffsetName) = GetFrameOffsetMetadataNames();

        if ((string.IsNullOrWhiteSpace(xOffsetName)) || (string.IsNullOrWhiteSpace(yOffsetName)))
        {
            return [];
        }

        IReadOnlyList<GorgonPoint> result = wic.GetFrameOffsetMetadata(stream, SupportedFileFormat, xOffsetName, yOffsetName);

        return result ?? throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
    }

    /// <inheritdoc/>
    public override bool IsReadable(Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
        }

        if (!stream.CanSeek)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
        }

        using WicUtilities wic = new();

        GorgonImageInfo info = wic.GetImageMetaDataFromStream(stream, SupportedFileFormat, DecodingOptions);

        return info is not null && info.Format != BufferFormat.Unknown;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCodecWic{TWicEncOpt, TWicDecOpt}" /> class.
    /// </summary>
    /// <param name="codec">Codec name.</param>
    /// <param name="description">Description for the codec.</param>
    /// <param name="extensions">Common extension(s) for the codec.</param>
    /// <param name="containerGUID">GUID for the container format.</param>
    /// <param name="encodingOptions">Options for encoding WIC images.</param>
    /// <param name="decodingOptions">Options for decoding WIC images.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
    protected GorgonCodecWic(string codec, string description, IReadOnlyList<string> extensions, Guid containerGUID, TWicEncOpt encodingOptions, TWicDecOpt decodingOptions)
        : base(encodingOptions, decodingOptions)
    {
        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentEmptyException(nameof(codec));
        }

        Codec = codec;
        CodecDescription = description ?? string.Empty;
        CodecCommonExtensions = extensions ?? [];
        SupportedFileFormat = containerGUID;
    }
}
