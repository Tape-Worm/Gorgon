
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
// Created: March 5, 2017 10:05:08 PM
// 

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Examples;

/// <summary>
/// Our TV image codec
/// </summary>
/// <remarks>
/// This codec will encode and decode image data as 1 channel/pixel to make the image look similar to the line patterns on a CRT tv screen
/// <para>
/// To create a codec, we must inherit the GorgonImageCodec object and implement functionality to load and save image data to and from a stream
/// </para>
/// </remarks>
internal class TvImageCodec
    : GorgonImageCodec<IGorgonImageCodecEncodingOptions, IGorgonImageCodecDecodingOptions>
{
    /// <summary>
    /// The header for our image format.
    /// </summary>
    /// <remarks>
    /// This is used to contain any metadata about the image such as its width, height, etc...
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TvHeader
    {
        /// <summary>
        /// Returns the size of this type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Unsafe.SizeOf<TvHeader>();

        /// <summary>
        /// The magic number that identifies the image data as our desired format.
        /// </summary>
        public long MagicValueData;
        /// <summary>
        /// The width of the image.
        /// </summary>
        public int Width;
        /// <summary>
        /// The height of the image.
        /// </summary>
        public int Height;
    }

    // The magic number to identify the file.
    private const long MagicValue = 0x3074724356543020;

    // Formats supported by the image.
    // We need to tell Gorgon which pixel formats this image codec stores its data as.  Otherwise, the image will not look right when it's loaded.
    private readonly BufferFormat[] _supportedFormats =
    [
        BufferFormat.R8G8B8A8_UNorm
    ];

    /// <summary>
    /// Property to return the friendly description of the format.
    /// </summary>
    public override string CodecDescription => "TV image";

    /// <summary>
    /// Property to return the abbreviated name of the codec (e.g. PNG).
    /// </summary>
    public override string Codec => "TV";

    /// <summary>
    /// Property to return the pixel formats supported by the codec.
    /// </summary>
    public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _supportedFormats;

    /// <summary>
    /// Property to return whether the image codec supports a depth component for volume textures.
    /// </summary>
    /// <remarks>This tv format doesn't support volume textures.</remarks>
    public override bool SupportsDepth => false;

    /// <summary>
    /// Property to return whether the codec supports decoding/encoding multiple frames or not.
    /// </summary>
    public override bool SupportsMultipleFrames => false;

    /// <summary>
    /// Property to return whether the image codec supports mip maps.
    /// </summary>
    /// <remarks>This tv format doesn't support mip mapping.</remarks>
    public override bool SupportsMipMaps => false;

    /// <summary>
    /// Property to return whether the image codec supports block compression.
    /// </summary>
    public override bool SupportsBlockCompression => false;

    /// <summary>
    /// Function to read the meta data for the image.
    /// </summary>
    /// <param name="stream">Stream containing the image data.</param>
    /// <returns>An image settings object containing information about the image.</returns>
    private static GorgonImageInfo ReadMetaData(Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(@"Stream is write only.", nameof(stream));
        }

        if (stream.Position + TvHeader.SizeInBytes >= stream.Length)
        {
            throw new EndOfStreamException();
        }

        // We only support 2D images with the tv format.
        TvHeader header;

        // Load the header for the image.
        using (GorgonBinaryReader reader = new(stream, true))
        {
            header = reader.ReadValue<TvHeader>();
        }

        // Ensure we've got the correct data.
        if (header.MagicValueData != MagicValue)
        {
            throw new ArgumentException(@"The image data is not a tv image.", nameof(stream));
        }

        // Ensure the width/height are valid.
        if ((header.Width < 0)
            || (header.Height < 0))
        {
            throw new ArgumentException(@"The image in this stream has an invalid width/height.", nameof(stream));
        }

        return new GorgonImageInfo(ImageDataType.Image2D, BufferFormat.R8G8B8A8_UNorm)
        {
            Width = header.Width,
            Height = header.Height
        };
    }

    /// <summary>
    /// Function to decode an image from a <see cref="Stream"/>.
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
    protected override IGorgonImage OnDecodeFromStream(Stream stream, long size)
    {
        // Read the image meta data so we'll know how large the data should be.
        GorgonImageInfo settings = ReadMetaData(stream);

        // Calculate the expected size of the image.
        int dataSize = settings.Width * settings.Height * sizeof(ushort);

        if ((size - TvHeader.SizeInBytes) != dataSize)
        {
            throw new GorgonException(GorgonResult.CannotRead, "The data in the stream is not the same size as the proposed image size.");
        }

        // Create our resulting image buffer.
        GorgonImage result = new(settings);

        using (GorgonBinaryReader reader = new(stream, true))
        {
            // Write each scanline.
            for (int y = 0; y < settings.Height; ++y)
            {
                // Ensure that we move to the next line by the row pitch and not the amount of pixels.
                // Some images put padding in for alignment reasons which can throw off the data offsets.
                int ptrPosition = (y * result.Buffers[0].PitchInformation.RowPitch);

                // Decode the pixels in the scan line for our resulting image.
                for (int x = 0; x < settings.Width; ++x)
                {
                    // Get our current pixel.
                    ushort pixel = reader.ReadUInt16();

                    // Since we encode 1 byte per color component for each pixel, we need to bump up the bit shift
                    // by 8 bits.  Once we get above 24 bits we'll start over since we're only working with 4 bytes 
                    // per pixel in the destination.

                    // We determine how many bits to shift the pixel based on horizontal positioning.
                    // We assume that the image is based on 4 bytes/pixel.  In most cases this value should be 
                    // determined by dividing the row pitch by the image width.

                    // Write the color by shifting the byte in the source data to the appropriate byte position.
                    uint color = (uint)(((pixel >> 8) & 0xff) << (8 * (x % 3)));
                    uint alpha = (uint)((pixel & 0xff) << 24);

                    ref uint imagePixel = ref result.ImageData.AsRef<uint>(ptrPosition);
                    imagePixel = color | alpha;
                    ptrPosition += sizeof(uint);
                }
            }
        }

        return result;
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
        if (imageData.Format != BufferFormat.R8G8B8A8_UNorm)
        {
            throw new ArgumentException(@"The image format must be R8G8B8A8_UNorm", nameof(imageData));
        }

        // First, we'll need to set up our header metadata.
        TvHeader header = new()
        {
            MagicValueData = MagicValue,
            Width = imageData.Width,
            Height = imageData.Height
        };

        // Write the metadata to the stream.
        using GorgonBinaryWriter writer = new(stream, true);
        writer.WriteValue(header);

        // Now, we need to encode the image data as 1 byte for every other color component per pixel. 
        // In essence, we'll be writing one channel as a byte and moving to the next pixel. 
        // Get the pointer to our image buffer.
        //IGorgonPointer imagePtr = imageData.ImageData;

        // For each scan line in the image we'll encode the data as described above.
        for (int y = 0; y < imageData.Height; ++y)
        {
            // Ensure that we move to the next line by the row pitch and not the amount of pixels.
            // Some images put padding in for alignment reasons which can throw off the data offsets.
            // Also, the width is not suitable as a pixel is often more than 1 byte.
            int pointerPos = (y * imageData.Buffers[0].PitchInformation.RowPitch);

            // Loop through the scan line until we're at its end.
            for (int x = 0; x < imageData.Width; ++x)
            {
                // We're assuming our image data is 4 bytes/pixel, but in real world scenarios this is dependent upon 
                // the format of the data.
                uint pixel = imageData.ImageData.AsRef<uint>(pointerPos);
                pointerPos += sizeof(uint);

                // Get the alpha channel for this pixel.
                byte alpha = (byte)((pixel >> 24) & 0xff);

                // Since we encode 1 byte per color component for each pixel, we need to bump up the bit shift
                // by 8 bits.  Once we get above 24 bits we'll start over since we're only working with 2 bytes 
                // per pixel in the destination.

                // We determine how many bits to shift the pixel based on horizontal positioning.
                // We assume that the image is based on 4 bytes/pixel.  In most cases this value should be 
                // determined by dividing the row pitch by the image width.

                // Get the color component for the pixel.
                byte color = (byte)((pixel >> (8 * (x % 3))) & 0xff);

                // Write it to the scanline.
                // We're encoding a pixel as a single color component with its alpha channel
                // value into an unsigned 16 bit number.
                writer.Write((ushort)((color << 8) | alpha));
            }
        }
    }

    /// <summary>
    /// Function to determine if this codec can read the file or not.
    /// </summary>
    /// <param name="stream">Stream used to read the file information.</param>
    /// <returns>
    /// <b>true</b> if the codec can read the file, <b>false</b> if not.
    /// </returns>
    /// <remarks>
    /// This is the method we'll use to determine if the data in the stream can actually be read by our codec.  Typically this is done by a "magic number" consisting of a set of bytes 
    /// that identify the data as the type we're expecting.  To retrieve the magic number we'll read in the meta data for the image, which may not seem efficient, but it gives us the
    /// ability to also check to ensure that the image stream contains enough information about the image to actually load it by comparing the size of the meta data in the stream with 
    /// the required meta data size.
    /// <para>
    /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so
    /// may cause undesirable results.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
    /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
    public override bool IsReadable(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new IOException("The stream is write only.");
        }

        if (!stream.CanSeek)
        {
            throw new IOException("Stream cannot perform seek operations.");
        }

        if (stream.Position + TvHeader.SizeInBytes >= stream.Length)
        {
            return false;
        }

        // Remember the stream position.
        // If we fail to do this then the stream will be offset when we return and could cause corruption.
        long position = stream.Position;
        GorgonBinaryReader reader = null;

        try
        {
            // Using the GorgonBinaryReader, we can pull in the data we need.
            reader = new GorgonBinaryReader(stream, true);

            // Retrieve our magic number.
            TvHeader header = reader.ReadValue<TvHeader>();

            // Ensure that the image size is valid and that the magic numbers match up.
            return header.Width > 0 && header.Height > 0 && header.MagicValueData == MagicValue;
        }
        finally
        {
            reader?.Dispose();

            // Restore the stream to its original placement.
            stream.Position = position;
        }
    }

    /// <summary>
    /// Function to read the meta data for image data within a stream.
    /// </summary>
    /// <param name="stream">The stream containing the metadata to read.</param>
    /// <returns>
    /// The image meta data as a <see cref="IGorgonImageInfo"/> value.
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
    public override IGorgonImageInfo GetMetaData(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new IOException("The stream is write only.");
        }

        if (!stream.CanSeek)
        {
            throw new IOException("Stream cannot perform seek operations.");
        }

        if (stream.Position + TvHeader.SizeInBytes >= stream.Length)
        {
            throw new EndOfStreamException();
        }

        // Remember the stream position.
        // If we fail to do this then the stream will be offset when we return and could cause corruption.
        long position = stream.Position;

        try
        {
            // Read the image meta data from the stream.
            return ReadMetaData(stream);
        }
        finally
        {
            // Restore the stream to its original placement.
            stream.Position = position;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvImageCodec"/> class.
    /// </summary>
    public TvImageCodec()
        : base(null, null) =>
        // Tell the codec which image file name extensions are commonly used to 
        // identify the image data type.  This is use by applications to determine 
        // which codec to use when loading an image.
        CodecCommonExtensions = ["tv"];

}
