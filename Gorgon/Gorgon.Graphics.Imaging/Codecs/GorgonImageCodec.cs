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
// Created: Monday, June 27 2016 5:59:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// A codec to reading and/or writing image data.
	/// </summary>
	/// <typeparam name="TEncOpt">The type of the options object used to provide options when encoding an image. Must be a reference type and implement <see cref="IGorgonImageCodecEncodingOptions"/>.</typeparam>
	/// <typeparam name="TDecOpt">The type of the options object used to provide options when decoding an image. Must be a reference type and implement <see cref="IGorgonImageCodecDecodingOptions"/>.</typeparam>
	/// <remarks>
	/// <para>
	/// A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own image formats, or use one of the predefined image codecs available in 
	/// Gorgon.
	/// </para>
	/// <para>
	/// Currently, Gorgon supports the following codecs:
	/// <para>
	/// <list type="table">
	///		<listheader>
	///			<term>Format</term>
	///			<term>File extension(s)</term>
	///			<term>Read?</term>
	///			<term>Write?</term>
	///			<term>Limitations</term>
	///			<term>Features</term>
	///		</listheader>
	///		<item>
	///			<description><see cref="GorgonCodecJpeg">Jpeg</see></description>
	///			<description>*.jpg; *.jpeg; *.jpe; *.jif; *.jfif; *.jfi</description>
	///			<description>Yes</description>
	///			<description>Yes</description>
	///			<description>Only supports the first array index in an image array, the first mip slice in a mip map, and the first depth slice in a 3D image.</description>
	///			<description>None</description>
	///		</item>
	///		<item>
	///			<description><see cref="GorgonCodecPng">Png</see></description>
	///			<description>*.png</description>
	///			<description>Yes</description>
	///			<description>Yes</description>
	///			<description>Only supports the first array index in an image array, the first mip slice in a mip map, and the first depth slice in a 3D image.</description>
	///			<description>None</description>
	///		</item>
	///		<item>
	///			<description><see cref="GorgonCodecBmp">Bmp</see></description>
	///			<description>*.bmp; *.dib</description>
	///			<description>Yes</description>
	///			<description>Yes</description>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<description>24 bit only</description>
	///					</item>
	///					<item>
	///						<description>Only supports the first array index in an image array, the first mip slice in a mip map, and the first depth slice in a 3D image.</description>
	///					</item>
	///				</list>
	///			</description>
	///			<description>None</description>
	///		</item>
	///		<item>
	///			<description><see cref="GorgonCodecGif">Gif</see></description>
	///			<description>*.gif; *.dib</description>
	///			<description>Yes</description>
	///			<description>Yes</description>
	///			<description>
	///				<list type="bullet">					
	///					<item>
	///						<description>8 bit paletted images only. Color quantinization will reduce image quality.</description>
	///					</item>
	///					<item>
	///						<description>Only supports the first array index in an image array (for non-animated images), the first mip slice in a mip map, and the first depth slice in a 3D image.</description>
	///					</item>
	///				</list>
	///			</description>
	///			<description>Supports reading of animated gif files as an array of images.</description>
	///		</item>
	///		<item>
	///			<description><see cref="GorgonCodecTga">Tga</see></description>
	///			<description>*.tga; *.tpic</description>
	///			<description>Yes</description>
	///			<description>Yes</description>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<description>No color map support.</description>						
	///					</item>
	///					<item>
	///						<description>Interleaved files are not supported.</description>						
	///					</item>
	///					<item>
	///						<description>Only supports 8 bit grayscale, 16, 24 and 32 bit image formats.</description>						
	///					</item>
	///					<item>
	///						<description>Can only write uncompressed image data.</description>						
	///					</item>
	///					<item>
	///						<description>Only supports the first array index in an image array, the first mip slice in a mip map, and the first depth slice in a 3D image.</description>
	///					</item>
	///				</list>
	///			</description>
	///			<description>Can read RLE compressed and uncompressed formats.</description>
	///		</item>
	///		<item>
	///			<description><see cref="GorgonCodecDds">Dds</see></description>
	///			<description>*.dds</description>
	///			<description>Yes</description>
	///			<description>Yes</description>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<description>
	///							No support for typless formats or the following legacy Direct 3D 9 image formats:
	///							<para>
	///							<list type="bullet">
	///							<item>
	///								<description>BumpDuDv D3DFMT_V8U8</description>
	///							</item>
	///							<item>
	///								<description>D3DFMT_Q8W8V8U8</description>
	///							</item>
	///							<item>
	///								<description>D3DFMT_V16U16</description>
	///							</item>
	///							<item>
	///								<description>D3DFMT_A2W10V10U10</description>
	///							</item>
	///							<item>
	///								<description>BumpLuminance D3DFMT_L6V5U5</description>
	///							</item>
	///							<item>
	///								<description>D3DFMT_X8L8V8U8</description>
	///							</item>
	///							<item>
	///								<description>FourCC "UYVY" D3DFMT_UYVY</description>
	///							</item>
	///							<item>
	///								<description>FourCC "YUY2" D3DFMT_YUY2</description>
	///							</item>
	///							<item>
	///								<description>FourCC 117 D3DFMT_CxV8U8</description>
	///							</item>
	///							<item>
	///								<description>ZBuffer D3DFMT_D16_LOCKABLE</description>
	///							</item>
	///							<item>
	///								<description>FourCC 82 D3DFMT_D32F_LOCKABLE</description>
	///							</item>
	///							</list>
	///							</para>
	///						</description>
	///					</item>
	///					<item>
	///						<description>No support for writing block compressed formats (BC1-BC7).</description>
	///					</item>
	///				</list>
	///			</description>
	///			<description>
	///				Supports the full array of image options like arrays, mip maps, 3D images and all Direct 3D 11 pixel formats.
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// </para>
	/// <para>
	///	While many of the image formats supplied will be useful out of the box, the system can read/write images via a <see cref="GorgonImageCodecPlugIn"/> if the supplied formats are too limited or do not 
	/// support a necessary feature.
	/// </para>
	/// </remarks>
	public abstract class GorgonImageCodec<TEncOpt, TDecOpt>
	    : IGorgonImageCodec
	    where TEncOpt : class, IGorgonImageCodecEncodingOptions
        where TDecOpt : class, IGorgonImageCodecDecodingOptions
	{
		#region Properties.
        /// <summary>
        /// Property to return the encoding options for the codec.
        /// </summary>
	    protected TEncOpt EncodingOptions
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the decoding options for the codec.
        /// </summary>
	    protected TDecOpt DecodingOptions
	    {
	        get;
	    }

		/// <summary>
		/// Property to return whether the codec supports decoding/encoding multiple frames or not.
		/// </summary>
		public virtual bool SupportsMultipleFrames => false;

		/// <summary>
		/// Property to return whether the codec supports encoding of image data.
		/// </summary>
		/// <remarks>
		/// If this value is <b>false</b>, then the codec is effectively read only.
		/// </remarks>
		public virtual bool CanEncode => true;

		/// <summary>
		/// Property to return whether the codec supports decoding of image data.
		/// </summary>
		/// <remarks>
		/// If this value is <b>false</b>, then the codec is effectively write only.
		/// </remarks>
		public virtual bool CanDecode => true;

		/// <summary>
		/// Property to return the common file name extension(s) for a codec.
		/// </summary>
		public IReadOnlyList<string> CodecCommonExtensions
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public abstract string CodecDescription
		{
			get;
		}

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. PNG).
		/// </summary>
		public abstract string Codec
		{
			get;
		}

        /// <summary>
        /// Property to return the pixel formats supported by the codec.
        /// </summary>
	    public abstract IReadOnlyList<BufferFormat> SupportedPixelFormats
	    {
	        get;
	    }

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume (3D) images.
        /// </summary>
	    public abstract bool SupportsDepth
	    {
	        get;
	    }

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
	    public abstract bool SupportsMipMaps
	    {
	        get;
	    }

		/// <summary>
		/// Property to return whether the image codec supports block compression.
		/// </summary>
		public abstract bool SupportsBlockCompression
		{
			get;
		}

		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string IGorgonNamedObject.Name => Codec;
		#endregion

		#region Methods.
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

	    /// <summary>
	    /// Function to load an image from a stream.
	    /// </summary>
	    /// <param name="stream">The stream containing the image data to read.</param>
	    /// <param name="size">[Optional] The size of the image within the stream, in bytes.</param>
	    /// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
	    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is write only.</exception>
	    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
	    /// <exception cref="EndOfStreamException">Thrown when the amount of data requested exceeds the size of the stream minus its current position.</exception>
	    /// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
	    /// <remarks>
	    /// <para>
	    /// When the <paramref name="size"/> parameter is specified, the image data will be read from the stream up to the amount specified. If it is omitted, then image data will be read up to the end of 
	    /// the stream.
	    /// </para>
	    /// </remarks>
	    public IGorgonImage LoadFromStream(Stream stream, long? size = null)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanRead)
			{
				throw new ArgumentException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY, nameof(stream));
			}

			if (size + stream.Position > stream.Length)
			{
				throw new EndOfStreamException();
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size), Resources.GORIMG_ERR_IMAGE_BYTE_LENGTH_TOO_SHORT);
			}

			long basePosition = stream.Position;

		    if (size == null)
		    {
		        size = stream.Length;
		    }

            Stream externalStream = stream;

            try
            {
                if (!stream.CanSeek)
                {
                    externalStream = new DX.DataStream((int)(size ?? stream.Length), true, true);
                    stream.CopyTo(externalStream, (int)(size ?? stream.Length));
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
                if (externalStream != stream)
                {
                    externalStream?.Dispose();
                }
            }
		}

		/// <summary>
		/// Function to load an image from a file on the physical file system.
		/// </summary>
		/// <param name="filePath">Path to the file to load.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size of the file is less than 1 byte.</exception>
		/// <exception cref="GorgonException">Thrown when the image data in the file has a pixel format that is unsupported.</exception>
		public IGorgonImage LoadFromFile(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentEmptyException(nameof(filePath));
			}

			using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return LoadFromStream(stream);
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
		public abstract void SaveToStream(IGorgonImage imageData, Stream stream);

		/// <summary>
		/// Function to persist a <see cref="IGorgonImage"/> to a file on the physical file system.
		/// </summary>
		/// <param name="imageData">A <see cref="IGorgonImage"/> to persist to the stream.</param>
		/// <param name="filePath">The path to the file that will hold the image data.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="imageData"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> is empty..</exception>
		/// <exception cref="NotSupportedException">Thrown when the image data in the stream has a pixel format that is unsupported by the codec.</exception>
		/// <remarks>
		/// <para>
		/// When persisting image data via a codec, the image must have a format that the codec can recognize. This list of supported formats is provided by the <see cref="SupportedPixelFormats"/> 
		/// property. Applications may convert their image data a supported format before saving the data using a codec.
		/// </para>
		/// </remarks>
		public void SaveToFile(IGorgonImage imageData, string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentEmptyException(nameof(filePath));
			}

			using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				SaveToStream(imageData, stream);
			}
		}

		/// <summary>
		/// Function to determine if this codec can read the image data within the stream or not.
		/// </summary>
		/// <param name="stream">The stream that is used to read the image data.</param>
		/// <returns><b>true</b> if the codec can read the file, <b>false</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
		/// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only, or non-seekable.</exception>
		/// <remarks>
		/// <para>
		/// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so may cause 
		/// undesirable results or an exception. 
		/// </para>
		/// </remarks>
		public abstract bool IsReadable(Stream stream);

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
		public abstract IGorgonImageInfo GetMetaData(Stream stream);

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GORIMG_TOSTR_IMAGE_CODEC, Codec);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageCodec{TEncOpt, TDecOpt}" /> class.
        /// </summary>
        /// <param name="encodingOptions">Codec specific options to use when encoding image data.</param>
        /// <param name="decodingOptions">Codec specific options to use when decoding image data.</param>
        protected GorgonImageCodec(TEncOpt encodingOptions, TDecOpt decodingOptions)
		{
		    EncodingOptions = encodingOptions;
		    DecodingOptions = decodingOptions;
			CodecCommonExtensions = Array.Empty<string>();
		}
		#endregion
	}
}
