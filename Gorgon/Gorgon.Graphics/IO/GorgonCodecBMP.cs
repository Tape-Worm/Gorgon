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
    /// A codec to handle read/writing of BMP files.
    /// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>This format requires that the Windows Imaging Components are installed on the system.</para>
    /// </remarks>
    public sealed class GorgonCodecBMP
        : GorgonCodecWIC
	{
		#region Variables.
		// Supported formats.
		private readonly BufferFormat[] _supportedFormats = {
																BufferFormat.B8G8R8X8_UIntNormal,
																BufferFormat.B5G6R5_UIntNormal
	                                                        };
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the data formats for the image.
		/// </summary>
		public override IEnumerable<BufferFormat> SupportedFormats => _supportedFormats;

	    #endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecBMP"/> class.
        /// </summary>
        public GorgonCodecBMP()
            : base("BMP", Resources.GORGFX_IMAGE_BMP_CODEC_DESC, new[] { "bmp", "dib" }, ContainerFormatGuids.Bmp)
        {
        }
        #endregion
    }
}
