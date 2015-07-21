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
// Created: Tuesday, February 19, 2013 9:13:28 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Graphics;
using Gorgon.Graphics.Properties;
using SharpDX.WIC;

namespace Gorgon.IO
{
    /// <summary>
    /// Types of compression for the TIFF codec.
    /// </summary>
    public enum TIFFCompressionType
    {
        /// <summary>
        /// The system will pick a compression format based on the format of the image.
        /// </summary>
        DontCare = TiffCompressionOption.DontCare,
        /// <summary>
        /// No compression.
        /// </summary>
        None = TiffCompressionOption.None,
        /// <summary>
        /// LZW compression algorithm.
        /// </summary>
        LZW = TiffCompressionOption.LZW,
        /// <summary>
        /// ZIP compression algorithm.
        /// </summary>
        ZIP = TiffCompressionOption.ZIP,
        /// <summary>
        /// LZW differencing compression algorithm.
        /// </summary>
        LZWDifferencing = TiffCompressionOption.LZWHDifferencing
    }

    /// <summary>
    /// A codec to handle read/writing of TIFF files.
    /// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>This format requires that the Windows Imaging Components are installed on the system.</para>
    /// </remarks>
    public sealed class GorgonCodecTIFF
        : GorgonCodecWIC
    {
        #region Variables.
		// Supported formats.
		private readonly BufferFormat[] _supportedFormats = {
																BufferFormat.R8G8B8A8_UIntNormal,
																BufferFormat.B8G8R8A8_UIntNormal,
																BufferFormat.B8G8R8X8_UIntNormal,
																BufferFormat.R16G16B16A16_UIntNormal
	                                                        };

		private float _compressionQuality = 1.0f;       // Compression quality.        
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the data formats for the image.
		/// </summary>
	    public override IEnumerable<BufferFormat> SupportedFormats => _supportedFormats;

	    /// <summary>
        /// Property to set or return the amount of compression to use.
        /// </summary>
        /// <remarks>
        /// This property controls how much compression to apply to the image.  A value of 0.0f will use the least amount of compression and 
        /// will result in larger files, a value of 1.0f will use the best compression and produce smaller files.
        /// <para>This property only applies when encoding an image.</para>
        /// <para>The default value is 1.0f</para>
        /// </remarks>
        public float CompressionQuality
        {
            get
            {
                return _compressionQuality;
            }
            set
            {
                if (value < 0.0f)
                {
                    value = 0.0f;
                }

                if (value > 1.0f)
                {
                    value = 1.0f;
                }

                _compressionQuality = value;
            }
        }

        /// <summary>
        /// Property to set or return the type of compression to apply to the image.
        /// </summary>
        /// <remarks>
        /// This property controls the type of compression for the image.
        /// <para>This property only applies when encoding an image.</para>
        /// <para>The default value is None.</para>
        /// </remarks>
        public TIFFCompressionType CompressionType
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set custom encoding options.
        /// </summary>
        /// <param name="frame">Frame encoder to use.</param>
        internal override void SetFrameOptions(BitmapFrameEncode frame)
        {
            frame.Options.CompressionQuality = _compressionQuality;
            frame.Options.TiffCompressionMethod = (TiffCompressionOption)CompressionType;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecTIFF"/> class.
        /// </summary>
        public GorgonCodecTIFF()
            : base("TIFF", Resources.GORGFX_IMAGE_TIF_CODEC_DESC, new[] { "tif", "tiff" }, ContainerFormatGuids.Tiff)
        {
            CompressionType = TIFFCompressionType.None;
        }
        #endregion
    }
}
