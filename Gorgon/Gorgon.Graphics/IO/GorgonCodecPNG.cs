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
    /// Filter to apply for compression optimization.
    /// </summary>
    public enum PNGFilter
    {
        /// <summary>
        /// The system will chose the best filter based on the image data.
        /// </summary>
        DontCare = PngFilterOption.Unspecified,
        /// <summary>
        /// No filtering.
        /// </summary>
        None = PngFilterOption.None,
        /// <summary>
        /// Sub filtering.
        /// </summary>
        Sub = PngFilterOption.Sub,
        /// <summary>
        /// Up filtering.
        /// </summary>
        Up = PngFilterOption.Up,
        /// <summary>
        /// Average filtering.
        /// </summary>
        Average = PngFilterOption.Average,
        /// <summary>
        /// Paeth filtering.
        /// </summary>
        Paeth = PngFilterOption.Paeth,
        /// <summary>
        /// Adaptive filtering.  The system will choose the best filter based on a per-scanline basis.
        /// </summary>
        Adaptive = PngFilterOption.Adaptive
    }

    /// <summary>
    /// A codec to handle read/writing of PNG files.
    /// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>This format requires that the Windows Imaging Components are installed on the system.</para>
    /// </remarks>
    public sealed class GorgonCodecPNG
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
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the data formats for the image.
		/// </summary>
		public override IEnumerable<BufferFormat> SupportedFormats
		{
			get
			{
				return _supportedFormats;
			}
		}

		/// <summary>
        /// Property to set or return the filter to apply.
        /// </summary>
        /// <remarks>This property will control the filtering applied to the image for compression optimization.
        /// <para>This property only applies when encoding the image.</para>
        /// <para>The default value is None.</para>
        /// </remarks>
        public PNGFilter CompressionFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to use interlacing on the image data.
        /// </summary>
        /// <remarks>
        /// Use this property to control the output of vertical rows in the image.  
        /// <para>This property only applies when encoding the image.</para>
        /// <para>The default value is <b>false</b>.</para>
        /// </remarks>
        public bool UseInterlacing
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
            frame.Options.InterlaceOption = UseInterlacing;
            frame.Options.FilterOption = (PngFilterOption)CompressionFilter;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecPNG"/> class.
        /// </summary>
        public GorgonCodecPNG()
            : base("PNG", Resources.GORGFX_IMAGE_PNG_CODEC_DESC, new[] { "png" }, ContainerFormatGuids.Png)
        {
            UseInterlacing = false;
            CompressionFilter = PNGFilter.None;
        }
        #endregion
    }
}
