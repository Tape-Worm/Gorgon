
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, February 14, 2013 9:24:55 PM
// 

using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Graphics.Imaging.Wic;
using Gorgon.Math;
using Windows.Win32;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// A codec to handle reading/writing GIF files.
/// </summary>
/// <remarks>
/// <para>
/// This codec will read and write lossless compressed files using the Graphics Interchange Format (GIF).
/// </para>
/// <para>
/// This codec only supports 1, 4, and 8 bit indexed pixel formats, and uses a palette to define the actual colors at the indices for each pixel. All data is decoded into the 32 bit 
/// <see cref="BufferFormat.R8G8B8A8_UNorm"/> pixel format. Data encoded with this codec will be downsampled to 8 bit indexed data.
/// </para>
/// </remarks>
/// <param name="encodingOptions">[Optional] Options to use when encoding a GIF file.</param>
/// <param name="decodingOptions">[Optional] Options to use when decoding a GIF file.</param>
public sealed class GorgonCodecGif(GorgonGifEncodingOptions? encodingOptions = null, GorgonGifDecodingOptions? decodingOptions = null)
        : GorgonCodecWic<GorgonGifEncodingOptions, GorgonGifDecodingOptions>("GIF", Resources.GORIMG_DESC_GIF_CODEC, ["gif"], PInvoke.GUID_ContainerFormatGif,
                                                                             encodingOptions ?? GorgonGifEncodingOptions.Default, decodingOptions ?? GorgonGifDecodingOptions.Default)
{
    // Meta data names for the frame offsets.
    private const string OffsetXName = "/imgdesc/Left";
    private const string OffsetYName = "/imgdesc/Top";
    // Meta data names for the size of the canvas (the logical screen) that the frames are placed on.
    private const string CanvasWidthName = "/logscrdesc/Width";
    private const string CanvasHeightName = "/logscrdesc/Height";

    /// <inheritdoc/>
    public override bool SupportsMultipleFrames => true;

    /// <inheritdoc/>
    protected override (string xOffset, string yOffset) GetFrameOffsetMetadataNames() => (OffsetXName, OffsetYName);

    /// <inheritdoc/>
    protected override (string width, string height) GetCanvasSizeMetadataNames() => (CanvasWidthName, CanvasHeightName);

    /// <inheritdoc/>
    protected override IReadOnlyDictionary<string, object> GetCustomEncodingMetadata(int frameIndex, IGorgonImageInfo settings)
    {
        Dictionary<string, object> result = [];

        if (EncodingOptions?.Palette is not null)
        {
            for (int i = 0; i < EncodingOptions.Palette.Count; ++i)
            {
                if (!EncodingOptions.Palette[i].Alpha.EqualsEpsilon(0))
                {
                    continue;
                }

                result["/grctlext/TransparencyFlag"] = true;
                result["/grctlext/TransparentColorIndex"] = (byte)i;
            }
        }

        bool saveAllFrames = EncodingOptions?.SaveAllFrames ?? true;

        if ((settings is null)
            || (settings.ArrayCount < 2)
            || (!saveAllFrames))
        {
            return result.Count == 0 ? [] : result;
        }

        // Write out frame delays.
        ushort delayValue = 0;

        if ((EncodingOptions?.FrameDelays is not null) && (frameIndex >= 0) && (frameIndex < EncodingOptions.FrameDelays.Count))
        {
            delayValue = (ushort)EncodingOptions.FrameDelays[frameIndex];
        }

        result["/grctlext/Delay"] = delayValue;
        result["/grctlext/Disposal"] = (byte)1;

        return result;
    }

    /// <summary>
    /// Function to retrieve a list of frame delays for each frame in an animated GIF.
    /// </summary>
    /// <param name="filePath">Path to the animated GIF file.</param>
    /// <returns>A list of frame delays (in 1/100ths of a second), with one entry for each frame in the file.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    /// <remarks>
    /// <inheritdoc cref="GetFrameDelays(Stream)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    public IReadOnlyList<int> GetFrameDelays(string filePath)
    {
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
    /// <returns>A list of frame delays (in 1/100ths of a second), with one entry for each frame in the stream.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write-only, or cannot perform seek operations.</exception>
    /// <remarks>
    /// <para type="common">
    /// This will return the delay (in 1/100ths of a second) for each frame in a GIF file. A GIF file with a single frame returns a list with one entry.
    /// </para>
    /// <para>
    /// The position of the <paramref name="stream"/> is restored when this method returns.
    /// </para>
    /// </remarks>
    public IReadOnlyList<int> GetFrameDelays(Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK, nameof(stream));
        }

        long position = stream.Position;
        WicUtilities wic = new();

        try
        {
            return wic.GetFrameDelays(stream, SupportedFileFormat, "/grctlext/Delay");
        }
        finally
        {
            stream.Position = position;
        }
    }
}
