﻿#region MIT
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
// Created: August 4, 2016 11:22:47 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Graphics.Imaging.Properties;
using DXGI = SharpDX.DXGI;
using WIC = SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// A codec to handle read/writing of BMP files.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This codec will read and write lossless compressed files using the Windows Bitmap (BMP) format.
	/// </para>
	/// <para>
	/// This codec supports the following pixel formats:
	/// <list type="bullet">
	///		<item><c>B8G8R8X8_UNorm</c></item>
	///		<item><c>B5G6R5_UNorm</c></item>
	/// </list>
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// This codec requires the Windows Imaging Components (WIC) to be installed for the operating system.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	public sealed class GorgonCodecBmp
        : GorgonCodecWic
	{
		#region Variables.
		// Supported formats.
		private readonly DXGI.Format[] _supportedFormats =
		{
			DXGI.Format.B8G8R8X8_UNorm,
			DXGI.Format.B5G6R5_UNorm
		};
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported pixel formats for this codec.
		/// </summary>
		public override IReadOnlyList<DXGI.Format> SupportedPixelFormats => _supportedFormats;
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecBmp"/> class.
        /// </summary>
        public GorgonCodecBmp()
            : base("BMP", Resources.GORIMG_DESC_BMP_CODEC, new[] { "bmp", "dib" }, WIC.ContainerFormatGuids.Bmp)
        {
        }
        #endregion
    }
}
