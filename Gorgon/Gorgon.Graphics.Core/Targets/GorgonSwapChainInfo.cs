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
// Created: Tuesday, June 4, 2013 8:25:47 PM
// 
#endregion

using System;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
	/// <summary> 
	/// Settings for defining the set up for a swap chain.
	/// </summary>
	public class GorgonSwapChainInfo 
		: IGorgonSwapChainInfo
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether the back buffer contents will be stretched to fit the size of the presentation target area (typically the client area of the window).
		/// </summary>
		/// <remarks>
		/// The default value for this value is <b>true</b>.
		/// </remarks>
		public bool StretchBackBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use flip mode rather than a bitblt mode to present the back buffer to the presentation target.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Setting this value to <b>true</b> will use the more performant flip model for displaying data from the swap chain back buffer, and setting it to <b>false</b> will use the bitblt model to copy the 
		/// back buffer data to the presentation target.
		/// </para>
		/// <para>
		/// When this value is set to <b>true</b>, the back buffer contents are kept after the back buffer is sent to the presentation target, when it is set to <b>false</b>, the contents may be erased (this 
		/// ultimately depends on the driver implementation).
		/// </para>
		/// <para>
		/// This value will default to <b>false</b>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// Setting this value on Windows 7 will have no effect, it is only supported on Windows 8 or better.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public bool UseFlipMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the swap chain back buffer.
		/// </summary>
		/// <remarks>
		/// The default value is <c>R8G8B8A8_UNorm</c>.
		/// </remarks>
		public DXGI.Format Format
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the format for a depth buffer that will be associated with the swap chain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value must be set to one of the depth formats (<c>D16_UNorm</c>, <c>D24_UNorm_S8_UInt</c>, <c>D32_Float</c>, or <c>D32_Float_S8X24_UInt</c>), or <c>Unknown</c>. Any other value will cause 
        /// an exception when the swap chain is created. 
        /// </para>
        /// <para>
        /// If this value is set to <c>Unknown</c>, then no depth buffer will be created for this swap chain.
        /// </para>
        /// <para>
        /// The default value <c>Unknown</c>.
        /// </para>
        /// </remarks>
	    public DXGI.Format DepthStencilFormat
	    {
	        get;
	        set;
	    }
	
		/// <summary>
		/// Property to set or return the width of the swap chain back buffer.
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the swap chain back buffer.
		/// </summary>
		public int Height
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonSwapChainInfo"/> to copy the settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonSwapChainInfo(IGorgonSwapChainInfo info)
		{
		    Format = info?.Format ?? throw new ArgumentNullException(nameof(info));
			Height = info.Height;
			StretchBackBuffer = info.StretchBackBuffer;
			UseFlipMode = info.UseFlipMode;
			Width = info.Width;
		    DepthStencilFormat = info.DepthStencilFormat;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
		/// </summary>
		public GorgonSwapChainInfo()
		{
			StretchBackBuffer = true;
			Format = DXGI.Format.R8G8B8A8_UNorm;
		    DepthStencilFormat = DXGI.Format.Unknown;
		}
		#endregion
	}
}