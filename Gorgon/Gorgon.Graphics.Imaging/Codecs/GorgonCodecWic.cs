#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 27, 2016 7:53:02 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using DX = SharpDX;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Base class for the WIC based file formats (PNG, JPG, and BMP).
/// </summary>
/// <typeparam name="TWicEncOpt">The type of the options object used to provide options when encoding an image. Must be a reference type and implement <see cref="IGorgonWicEncodingOptions"/>.</typeparam>
/// <typeparam name="TWicDecOpt">The type of the options object used to provide options when decoding an image. Must be a reference type and implement <see cref="IGorgonWicDecodingOptions"/>.</typeparam>
/// <remarks>
/// <para>
/// A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
/// image formats, or use one of the predefined image codecs available in Gorgon.
/// </para>
/// </remarks>
public abstract class GorgonCodecWic<TWicEncOpt, TWicDecOpt>
    : GorgonImageCodec<TWicEncOpt, TWicDecOpt>
    where TWicEncOpt : class, IGorgonWicEncodingOptions
    where TWicDecOpt : class, IGorgonWicDecodingOptions
{
    #region Variables.
    // Supported formats.
    private readonly BufferFormat[] _supportedFormats =
    [
        BufferFormat.R8G8B8A8_UNorm,
        BufferFormat.B8G8R8A8_UNorm,
        BufferFormat.B8G8R8X8_UNorm
    ];
    #endregion

    #region Properties
    /// <summary>
    /// Property to return the list of names used to locate frame offsets in metadata.
    /// </summary>
    /// <remarks>
    /// Implementors must put the horizontal offset name first, and the vertical name second.  Failure to do so will lead to incorrect offsets.
    /// </remarks>
    protected virtual IReadOnlyList<string> FrameOffsetMetadataNames => null;

    /// <summary>
    /// Property to set or return the file format that is supported by this codec.
    /// </summary>
    protected Guid SupportedFileFormat
    {
        get;
    }

    /// <summary>
    /// Property to return the abbreviated name of the codec (e.g. PNG).
    /// </summary>
    public override string Codec
    {
        get;
    }

    /// <summary>
    /// Property to return the friendly description of the format.
    /// </summary>
    public override string CodecDescription
    {
        get;
    }

    /// <summary>
    /// Property to return whether the image codec supports block compression.
    /// </summary>
    public override bool SupportsBlockCompression => false;

    /// <summary>
    /// Property to return whether the image codec supports mip maps.
    /// </summary>
    public override bool SupportsMipMaps => false;

    /// <summary>
    /// Property to return whether the image codec supports a depth component for volume textures.
    /// </summary>
    public override bool SupportsDepth => false;

    /// <summary>
    /// Property to return the supported pixel formats for this codec.
    /// </summary>
    public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _supportedFormats;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve custom metadata when encoding an image frame.
    /// </summary>
    /// <param name="frameIndex">The index of the frame being encoded.</param>
    /// <param name="settings">The settings for the image being encoded.</param>
    /// <returns>A dictionary containing the key/value pair describing the metadata to write to the frame, or <b>null</b> if the frame contains no metadata.</returns>
    protected virtual IReadOnlyDictionary<string, object> GetCustomEncodingMetadata(int frameIndex, IGorgonImageInfo settings) => null;

    /// <summary>
    /// Function to load an image from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the image data to read.</param>
    /// <param name="size">The size of the image within the stream, in bytes.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
    /// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
    protected override IGorgonImage OnDecodeFromStream(Stream stream, long size)
    {
        var wic = new WicUtilities();
        Stream streamAlias = stream;

        try
        {
            // If we have a stream position that does not begin exactly at the start of the stream, we have to wrap that stream in a 
            // dummy stream wrapper. This is to get around a problem in the underlying COM stream object used by WIC that throws an 
            // exception when the stream position is not exactly 0. 
            if (streamAlias.Position != 0)
            {
                streamAlias = new GorgonStreamWrapper(stream, 0, size);
            }

            IGorgonImage result = wic.DecodeImageData(streamAlias, size, SupportedFileFormat, DecodingOptions, FrameOffsetMetadataNames) ?? throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));

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

    /// <summary>
    /// Function to persist a <see cref="IGorgonImage"/> to a stream.
    /// </summary>
    /// <param name="imageData">A <see cref="IGorgonImage"/> to persist to the stream.</param>
    /// <param name="stream">The stream that will receive the image data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="imageData"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="NotSupportedException">Thrown when the image data in the stream has a pixel format that is unsupported by the codec.</exception>
    /// <remarks>
    /// <para>
    /// When persisting image data via a codec, the image must have a format that the codec can recognize. This list of supported formats is provided by the <see cref="SupportedPixelFormats"/> 
    /// property. Applications may convert their image data a supported format before saving the data using a codec.
    /// </para>
    /// </remarks>
    public override void Save(IGorgonImage imageData, Stream stream)
    {
        if (imageData is null)
        {
            throw new ArgumentNullException(nameof(imageData));
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanWrite)
        {
            throw new IOException(string.Format(Resources.GORIMG_ERR_STREAM_IS_READONLY));
        }

        var wic = new WicUtilities();

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

    /// <summary>
    /// Function to read the meta data for image data within a stream.
    /// </summary>
    /// <param name="stream">The stream containing the metadata to read.</param>
    /// <returns>
    /// The image meta data as a <see cref="GorgonImageInfo"/> value.
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
    public override IGorgonImageInfo GetMetaData(Stream stream) => GetMetaData(stream, null);

    /// <summary>
    /// Function to read file meta data.
    /// </summary>
    /// <param name="stream">Stream used to read the meta data.</param>
    /// <param name="options">Options used for decoding the meta data.</param>
    /// <returns>
    /// The image meta data as a <see cref="GorgonImageInfo"/> value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the stream is write-only.
    /// <para>-or-</para>
    /// <para>Thrown when the stream cannot perform seek operations.</para>
    /// </exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    public IGorgonImageInfo GetMetaData(Stream stream, IGorgonWicDecodingOptions options)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
        }

        if (!stream.CanSeek)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
        }

        var wic = new WicUtilities();

        try
        {
            // Get our WIC interface.				
            GorgonImageInfo result = wic.GetImageMetaDataFromStream(stream, SupportedFileFormat, options);

            return result.Format == BufferFormat.Unknown
                ? throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, result.Format))
                : result;
        }
        catch (DX.SharpDXException)
        {
            throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
        }
        finally
        {
            wic.Dispose();
        }
    }

    /// <summary>
    /// Function to retrieve the horizontal and vertical offsets for the frames in a multi-frame image.
    /// </summary>
    /// <param name="fileName">The path to the file to retrieve the offsets from.</param>
    /// <returns>A list of <c>Point</c> values that indicate the offset within the image for each frame.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileName"/> parameter is empty.</exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    /// <remarks>
    /// <para>
    /// For image codecs that support multiple frames, this reads a list of offset values for each frame so that the frame can be positioned correctly within the base image. If the image does not have 
    /// multiple frames, or the codec does not support multiple frames, then an empty list is returned.
    /// </para>
    /// </remarks>
    public IReadOnlyList<DX.Point> GetFrameOffsets(string fileName)
    {
        if (fileName is null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }

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
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
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
    public IReadOnlyList<DX.Point> GetFrameOffsets(Stream stream)
    {
        if (!SupportsMultipleFrames)
        {
            return [];
        }

        var wic = new WicUtilities();

        try
        {
            if ((FrameOffsetMetadataNames is null) || (FrameOffsetMetadataNames.Count == 0))
            {
                return [];
            }

            IReadOnlyList<DX.Point> result = wic.GetFrameOffsetMetadata(stream, SupportedFileFormat, FrameOffsetMetadataNames);

            return result ?? throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
        }
        catch (DX.SharpDXException)
        {
            throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
        }
        finally
        {
            wic.Dispose();
        }
    }

    /// <summary>
    /// Function to determine if this codec can read the image data within the stream or not.
    /// </summary>
    /// <param name="stream">The stream that is used to read the image data.</param>
    /// <returns><b>true</b> if the codec can read the file, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
    /// <remarks>
    /// <para>
    /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so may cause 
    /// undesirable results or an exception. 
    /// </para>
    /// </remarks>
    public override bool IsReadable(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
        }

        if (!stream.CanSeek)
        {
            throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
        }

        var wic = new WicUtilities();

        try
        {
            GorgonImageInfo info = wic.GetImageMetaDataFromStream(stream, SupportedFileFormat, null);

            return info is not null && info.Format != BufferFormat.Unknown;
        }
        catch (DX.SharpDXException)
        {
            return false;
        }
        finally
        {
            wic.Dispose();
        }
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCodecWic{TWicEncOpt, TWicDecOpt}" /> class.
    /// </summary>
    /// <param name="codec">Codec name.</param>
    /// <param name="description">Description for the codec.</param>
    /// <param name="extensions">Common extension(s) for the codec.</param>
    /// <param name="containerGUID">GUID for the container format.</param>
    /// <param name="encodingOptions">Options for encoding WIC images.</param>
    /// <param name="decodingOptions">Options for decoding WIC images.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
    protected GorgonCodecWic(string codec, string description, IReadOnlyList<string> extensions, Guid containerGUID, TWicEncOpt encodingOptions, TWicDecOpt decodingOptions)
        : base(encodingOptions, decodingOptions)
    {
        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentEmptyException(nameof(codec));
        }

        Codec = codec;
        CodecDescription = description ?? string.Empty;
        CodecCommonExtensions = extensions ?? [];
        SupportedFileFormat = containerGUID;
    }
    #endregion
}
