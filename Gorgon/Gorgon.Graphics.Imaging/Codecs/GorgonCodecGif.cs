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
// Created: Thursday, February 14, 2013 9:24:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// A codec to handle reading/writing GIF files.
/// </summary>
/// <remarks>
/// <para>
/// This codec will read and write lossless compressed files using the Graphics Interchange Format (GIF).
/// </para>
/// <para>
/// This codec only supports 1, 4, and 8 bit indexed pixel formats, and uses a palette to define the actual colors at the indices for each pixel. All data decoded will be encoded using the 
/// 32 bit <see cref="BufferFormat.R8G8B8A8_UNorm"/> pixel format. Data encoded with this codec will be downsampled to 8 bit indexed data.
/// </para>
/// </remarks>
public sealed class GorgonCodecGif
    : GorgonCodecWic<GorgonGifEncodingOptions, GorgonGifDecodingOptions>
{
    #region Variables.
    // Meta data names for the frame offsets.
    private static readonly string[] _frameOffsetItems =
    {
        "/imgdesc/Left",
        "/imgdesc/Top"
    };
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the list of names used to locate frame offsets in metadata.
    /// </summary>
    /// <remarks>
    /// Implementors must put the horizontal offset name first, and the vertical name second.  Failure to do so will lead to incorrect offsets.
    /// </remarks>
    protected override IReadOnlyList<string> FrameOffsetMetadataNames => _frameOffsetItems;

    /// <summary>
    /// Property to return whether the codec supports decoding/encoding multiple frames or not.
    /// </summary>
    public override bool SupportsMultipleFrames => true;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve custom metadata when encoding an image frame.
    /// </summary>
    /// <param name="frameIndex">The index of the frame being encoded.</param>
    /// <param name="settings">The settings for the image being encoded.</param>
    /// <returns>A dictionary containing the key/value pair describing the metadata to write to the frame, or <b>null</b> if the frame contains no metadata.</returns>
    protected override IReadOnlyDictionary<string, object> GetCustomEncodingMetadata(int frameIndex, IGorgonImageInfo settings)
    {
        var result = new Dictionary<string, object>();

        if (EncodingOptions?.Palette is not null)
        {
            for (int i = 0; i < EncodingOptions.Palette.Count; ++i)
            {
                if (!EncodingOptions.Palette[i].Alpha.EqualsEpsilon(0))
                {
                    continue;
                }

                result["/grctlext/TransparencyFlag"] = true;
                result["/grctlext/TransparentColorIndex"] = i;
            }
        }

        bool saveAllFrames = EncodingOptions?.SaveAllFrames ?? true;

        if ((settings is null)
            || (settings.ArrayCount < 2)
            || (!saveAllFrames))
        {
            return result.Count == 0 ? null : result;
        }

        // Write out frame delays.
        ushort delayValue = 0;

        if ((EncodingOptions?.FrameDelays is not null) && (frameIndex >= 0) && (frameIndex < EncodingOptions.FrameDelays.Count))
        {
            delayValue = (ushort)EncodingOptions.FrameDelays[frameIndex];
        }

        result["/grctlext/Delay"] = delayValue;
        result["/grctlext/Disposal"] = (ushort)1;

        return result;
    }

    /// <summary>
    /// Function to retrieve a list of frame delays for each frame in an animated GIF.
    /// </summary>
    /// <param name="filePath">Path to the animated GIF file.</param>
    /// <returns>An array of frame delays (1/100th of a second), or an empty array if the image is not an animated GIF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thown when the filePath parameter is empty.</exception>
    /// <exception cref="IOException">Thrown when the stream parameter is write-only.
    /// <para>-or-</para>
    /// <para>The data in the stream could not be decoded as GIF file.</para>
    /// <para>-or-</para>
    /// <para>The stream cannot perform seek operations.</para>
    /// </exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    /// <remarks>
    /// <para>
    /// This will return the delay (in 1/100ths of a second) between each frame in a multi-frame animated GIF. If the GIF file only has a single frame, then an empty array is returned.
    /// </para>
    /// </remarks>
    public IReadOnlyList<int> GetFrameDelays(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return GetFrameDelays(fileStream);
    }

    /// <summary>
    /// Function to retrieve a list of frame delays for each frame in an animated GIF.
    /// </summary>
    /// <param name="stream">Stream containing the animated GIF.</param>
    /// <returns>An array of frame delays (1/100th of a second), or an empty array if the image is not an animated GIF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the stream parameter is write-only.
    /// <para>-or-</para>
    /// <para>The data in the stream could not be decoded as GIF file.</para>
    /// <para>-or-</para>
    /// <para>The stream cannot perform seek operations.</para>
    /// </exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    /// <remarks>
    /// <para>
    /// This will return the delay (in 1/100ths of a second) between each frame in a multi-frame animated GIF. If the GIF file only has a single frame, then an empty array is returned.
    /// </para>
    /// </remarks>
    public IReadOnlyList<int> GetFrameDelays(Stream stream)
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

        long position = stream.Position;
        var wic = new WicUtilities();

        try
        {
            return wic.GetFrameDelays(stream, SupportedFileFormat, "/grctlext/Delay");
        }
        finally
        {
            stream.Position = position;
        }
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCodecGif" /> class.
    /// </summary>
    /// <param name="encodingOptions">[Optional] Options to use when encoding a GIF file.</param>
    /// <param name="decodingOptions">[Optional] Options to use when decoding a GIF file.</param>
    public GorgonCodecGif(GorgonGifEncodingOptions encodingOptions = null, GorgonGifDecodingOptions decodingOptions = null)
        : base("GIF", Resources.GORIMG_DESC_GIF_CODEC, new[] { "gif" }, ContainerFormatGuids.Gif, encodingOptions, decodingOptions)
    {
    }
    #endregion
}
