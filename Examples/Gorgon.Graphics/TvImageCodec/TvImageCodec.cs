#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, November 03, 2014 9:34:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gorgon.IO;
using Gorgon.Native;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// Our TV image codec.
    /// </summary>
    /// <remarks>
    /// This codec will encode and decode image data as 1 channel/pixel to make the image look similar to the line patterns on a CRT tv screen.
    /// <para>
    /// To create a codec, we must inherit the GorgonImageCodec object and implement functionality to load and save image data to and from a stream.
    /// </para>
    /// </remarks>
    class TvImageCodec
        : GorgonImageCodec
    {
        #region Value Types.
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
            public static readonly int SizeInBytes = DirectAccess.SizeOf<TvHeader>();

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
        #endregion

        #region Variables.
        // The magic number to identify the file.
        private const long MagicValue = 0x3074724356543020;

        // Formats supported by the image.
        // We need to tell Gorgon which pixel formats this image codec stores its data as.  Otherwise, the image will not look right when it's loaded.
        private readonly BufferFormat[] _supportedFormats =
        {
            BufferFormat.R8G8B8A8_UNorm
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the friendly description of the format.
        /// </summary>
        public override string CodecDescription => "TV image";

	    /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        public override string Codec => "TV";

	    /// <summary>
        /// Property to return the data formats supported by the codec.
        /// </summary>
        public override IEnumerable<BufferFormat> SupportedFormats => _supportedFormats;

	    /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        /// <remarks>This tv format doesn't support volume textures.</remarks>
        public override bool SupportsDepth => false;

	    /// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
        /// <remarks>This tv format doesn't support texture arrays.</remarks>
        public override bool SupportsArray => false;

	    /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
        /// <remarks>This tv format doesn't support mip mapping.</remarks>
        public override bool SupportsMipMaps => false;

	    /// <summary>
        /// Property to return whether the image codec supports block compression.
        /// </summary>
        public override bool SupportsBlockCompression => false;

	    #endregion

        #region Methods.
        /// <summary>
        /// Function to read the meta data for the image.
        /// </summary>
        /// <param name="stream">Stream containing the image data.</param>
        /// <returns>An image settings object containing information about the image.</returns>
		private static IImageSettings ReadMetaData(Stream stream)
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
            var settings = new GorgonTexture2DSettings();
            TvHeader header;

            // Load the header for the image.
            using(var reader = new GorgonBinaryReader(stream, true))
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

            settings.Width = header.Width;
            settings.Height = header.Height;
	        settings.Format = BufferFormat.R8G8B8A8_UNorm;

            return settings;
        }


        /// <summary>
        /// Function to load an image from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the data to load.</param>
        /// <param name="size">The size of the stream, in bytes.</param>
        /// <returns>
        /// The image data that was in the stream.
        /// </returns>
        protected override GorgonImageData LoadFromStream(GorgonDataStream stream, int size)
        {
			// Read the image meta data so we'll know how large the data should be.
	        IImageSettings settings = ReadMetaData(stream);

			// Calculate the expected size of the image.
	        int dataSize = settings.Width * settings.Height * 2;

	        if ((size - TvHeader.SizeInBytes) != dataSize)
	        {
		        throw new ArgumentException("The data in the stream is not the same size as the proposed image size.");
	        }

			// We'll be getting into unsafe territory here for performance reasons.
			// If you're not comfortable with pointers, the stream object provides other ways of retrieving 
			// the data and copying it.
	        unsafe
	        {
		        // Create our resulting image buffer.
		        var result = new GorgonImageData(settings);

				// Get pointers to our data buffers.
		        var imagePtr = (byte*)result.UnsafePointer;
		        var srcPtr = (ushort*)stream.BasePointer;

		        // Write each scanline.
		        for (int y = 0; y < settings.Height; ++y)
		        {
			        var destPtr = (uint*)imagePtr;

					// Decode the pixels in the scan line for our resulting image.
			        for (int x = 0; x < settings.Width; ++x)
			        {
                        // Get our current pixel.
			            ushort pixel = *(srcPtr++);

                        // Since we encode 1 byte per color component for each pixel, we need to bump up the bit shift
                        // by 8 bits.  Once we get above 24 bits we'll start over since we're only working with 4 bytes 
                        // per pixel in the destination.

                        // We determine how many bits to shift the pixel based on horizontal positioning.
                        // We assume that the image is based on 4 bytes/pixel.  In most cases this value should be 
                        // determined by dividing the row pitch by the image width.

						// Write the color by shifting the byte in the source data to the appropriate byte position.
				        var color = (uint)(((pixel >> 8) & 0xff) << (8 * (x % 3)));
			            var alpha = (uint)((pixel & 0xff) << 24);

				        *(destPtr++) = color | alpha;
			        }

					// Ensure that we move to the next line by the row pitch and not the amount of pixels.
					// Some images put padding in for alignment reasons which can throw off the data offsets.
			        imagePtr += result.Buffers[0].PitchInformation.RowPitch;
		        }

				// Move to the end of the stream.
		        stream.Position += dataSize;

				return result;
	        }
        }

        /// <summary>
        /// Function to persist image data to a stream.
        /// </summary>
        /// <param name="imageData"><see cref="Gorgon.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
        /// <param name="stream">Stream that will contain the data.</param>
        protected override void SaveToStream(GorgonImageData imageData, Stream stream)
        {
	        if (imageData.Settings.Format != BufferFormat.R8G8B8A8_UNorm)
	        {
				throw new ArgumentException(@"The image format must be R8G8B8A8_UNorm", nameof(imageData));    
	        }

			// First, we'll need to set up our header metadata.
	        var header = new TvHeader
	                     {
		                     MagicValueData = MagicValue,
		                     Width = imageData.Settings.Width,
		                     Height = imageData.Settings.Height
	                     };

			// Write the metadata to the stream.
	        using (var writer = new GorgonBinaryWriter(stream, true))
	        {
		        writer.WriteValue(header);

				// Now, we need to encode the image data as 1 byte for every other color component per pixel. 
				// In essence, we'll be writing one channel as a byte and moving to the next pixel. 
		        unsafe
		        {
					// We're in unsafe land now.  Again, if you're uncomfortable with pointers, the GorgonDataStream object 
					// provides safe methods to read image data.

					// Get the pointer to our image buffer.
			        var imagePtr = (byte*)imageData.UnsafePointer;
					// Allocate a buffer to store our scanline before dumping to the file.
			        var scanLineBuffer = new byte[imageData.Settings.Width * 2];

					// For each scan line in the image we'll encode the data as described above.
			        for (int y = 0; y < imageData.Settings.Height; ++y)
			        {
						// Read 4 bytes at a time.
				        var colorPtr = (uint*)imagePtr;

						// Reset to the beginning of the scanline buffer.
				        fixed (byte* scanLinePtr = scanLineBuffer)
				        {
							// Need to alias the pointer because the result value in the fixed 
							// block can't be changed.
					        var scanLine = (ushort *)scanLinePtr;

							// Loop through the scan line until we're at its end.
					        for (int x = 0; x < imageData.Settings.Width; ++x)
					        {
                                // We're assuming our image data is 4 bytes/pixel, but in real world scenarios this is dependent upon 
                                // the format of the data.
					            var pixel = *(colorPtr++);

                                // Get the alpha channel for this pixel.
                                var alpha = (byte)((pixel >> 24) & 0xff);

                                // Since we encode 1 byte per color component for each pixel, we need to bump up the bit shift
                                // by 8 bits.  Once we get above 24 bits we'll start over since we're only working with 4 bytes 
                                // per pixel in the destination.

                                // We determine how many bits to shift the pixel based on horizontal positioning.
                                // We assume that the image is based on 4 bytes/pixel.  In most cases this value should be 
                                // determined by dividing the row pitch by the image width.
                                
                                // Get the color component for the pixel.
						        var color = (byte)((pixel >> (8 * (x % 3))) & 0xff);

						        // Write it to the scanline.
                                // We're encoding a pixel as a single color component with its alpha channel
                                // value into an unsigned 16 bit number.
						        *(scanLine++) = (ushort)((color << 8) | alpha);
					        }
				        }

				        // Ensure that we move to the next line by the row pitch and not the amount of pixels.
						// Some images put padding in for alignment reasons which can throw off the data offsets.
						// Also, the width is not suitable as a pixel is often more than 1 byte.
						imagePtr += imageData.Buffers[0].PitchInformation.RowPitch;

						// Send the scanline to the file.
						writer.Write(scanLineBuffer);
			        }
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
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        public override bool IsReadable(Stream stream)
        {
            if (stream == null)
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
            GorgonBinaryReader reader = null;

            try
            {
                // Using the GorgonBinaryReader, we can pull in the data we need.
	            reader = new GorgonBinaryReader(stream, true);
                
                // Retrieve our magic number.
                var header = reader.ReadValue<TvHeader>();

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
        /// Function to read file meta data.
        /// </summary>
        /// <param name="stream">Stream used to read the metadata.</param>
        /// <returns>
        /// The image meta data as a <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> value.
        /// </returns>
        /// <remarks>
        /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so
        /// may cause undesirable results.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        public override IImageSettings GetMetaData(Stream stream)
        {
            if (stream == null)
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
        #endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="TvImageCodec"/> class.
		/// </summary>
	    public TvImageCodec()
		{
			// Tell the codec which image file name extensions are commonly used to 
			// identify the image data type.  This is use by applications to determine 
			// which codec to use when loading an image.
			CodecCommonExtensions = new[]
			                        {
										"tv"
			                        };
		}
		#endregion
	}
}
