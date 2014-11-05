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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.Example
{
    /// <summary>
    /// Our useless image codec.
    /// </summary>
    /// <remarks>
    /// This codec will encode and decode image data as 1 channel/pixel.
    /// <para>
    /// To create a codec, we must inherit the GorgonImageCodec object and implement functionality to load and save image data to and from a stream.
    /// </para>
    /// </remarks>
    class UselessImageCodec
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
        private struct UselessHeader
        {
            /// <summary>
            /// Returns the size of this type, in bytes.
            /// </summary>
            public static readonly int SizeInBytes = DirectAccess.SizeOf<UselessHeader>();

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
        private const long MagicValue = 0x7553654C65537331;

        // Formats supported by the image.
        // We need to tell Gorgon which pixel formats this image codec stores its data as.  Otherwise, the image will not look right when it's loaded.
        private BufferFormat[] _supportFormats =
        {
            BufferFormat.R8G8B8A8_UIntNormal
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the friendly description of the format.
        /// </summary>
        public override string CodecDescription
        {
            get
            {
                return "Useless image";
            }
        }

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        public override string Codec
        {
            get
            {
                return "Useless";
            }
        }

        /// <summary>
        /// Property to return the data formats supported by the codec.
        /// </summary>
        public override IEnumerable<BufferFormat> SupportedFormats
        {
            get
            {
                return _supportFormats;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        /// <remarks>This useless format doesn't support volume textures.</remarks>
        public override bool SupportsDepth
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
        /// <remarks>This useless format doesn't support texture arrays.</remarks>
        public override bool SupportsArray
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
        /// <remarks>This useless format doesn't support mip mapping.</remarks>
        public override bool SupportsMipMaps
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports block compression.
        /// </summary>
        public override bool SupportsBlockCompression
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to read the meta data for the image.
        /// </summary>
        /// <param name="stream">Stream containing the image data.</param>
        /// <returns>An image settings object containing information about the image.</returns>
        private IImageSettings ReadMetaData(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException(@"Stream is write only.", "stream");
            }

            if (stream.Position + UselessHeader.SizeInBytes >= stream.Length)
            {
                throw new EndOfStreamException();
            }

            // We only support 2D images with this useless format.
            var settings = new GorgonTexture2DSettings();
            UselessHeader header;

            // Load the header for the image.
            using(var reader = new GorgonBinaryReader(stream, true))
            {
                header = reader.ReadValue<UselessHeader>();
            }

            // Ensure we've got the correct data.
            if (header.MagicValueData != MagicValue)
            {
                throw new ArgumentException(@"The image data is not a useless image.", "stream");
            }

            // Ensure the width/height are valid.
            if ((header.Width < 0)
                || (header.Height < 0))
            {
                throw new ArgumentException(@"The image in this stream has an invalid width/height.", "stream");
            }

            settings.Width = header.Width;
            settings.Height = header.Height;

            return settings;
        }


        /// <summary>
        /// Function to load an image from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the data to load.</param>
        /// <param name="size">Size of the data to read, in bytes.</param>
        /// <returns>
        /// The image data that was in the stream.
        /// </returns>
        protected override GorgonImageData LoadFromStream(GorgonDataStream stream, int size)
        {
                
        }

        /// <summary>
        /// Function to persist image data to a stream.
        /// </summary>
        /// <param name="imageData"><see cref="GorgonLibrary.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
        /// <param name="stream">Stream that will contain the data.</param>
        protected override void SaveToStream(GorgonImageData imageData, Stream stream)
        {
            
        }

        /// <summary>
        /// Function to determine if this codec can read the file or not.
        /// </summary>
        /// <param name="stream">Stream used to read the file information.</param>
        /// <returns>
        /// TRUE if the codec can read the file, FALSE if not.
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
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        public override bool IsReadable(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new IOException("The stream is write only.");
            }

            if (!stream.CanSeek)
            {
                throw new IOException("Stream cannot perform seek operations.");
            }

            if (stream.Position + UselessHeader.SizeInBytes >= stream.Length)
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
                reader = new GorgonBinaryReader(stream, true))
                
                // Retrieve our magic number.
                var header = reader.ReadValue<UselessHeader>();

                // Ensure that the image size is valid and that the magic numbers match up.
                return header.Width > 0 && header.Height > 0 && header.MagicValueData == MagicValue;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }

                // Restore the stream to its original placement.
                stream.Position = position;
            }
        }

        /// <summary>
        /// Function to read file meta data.
        /// </summary>
        /// <param name="stream">Stream used to read the metadata.</param>
        /// <returns>
        /// The image meta data as a <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> value.
        /// </returns>
        /// <remarks>
        /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so
        /// may cause undesirable results.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        public override IImageSettings GetMetaData(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new IOException("The stream is write only.");
            }

            if (!stream.CanSeek)
            {
                throw new IOException("Stream cannot perform seek operations.");
            }

            if (stream.Position + UselessHeader.SizeInBytes >= stream.Length)
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
    }
}
