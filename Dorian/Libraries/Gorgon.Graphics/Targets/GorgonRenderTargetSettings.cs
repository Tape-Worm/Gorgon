#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Sunday, August 28, 2011 4:13:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings used to create a render target.
	/// </summary>
	public class GorgonRenderTargetSettings
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the multi-sample/anti-alias quality and level.
		/// </summary>
		/// <remarks>The default value is no multi-sampling.</remarks>
		public GorgonMSAAQualityLevel MSAAQualityLevel
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the target.
		/// </summary>
		public GorgonBufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth/stencil buffer format.
		/// </summary>
		/// <remarks>Set this to GorgonBufferFormat.Unknown to skip the creation of a depth/stencil buffer.</remarks>
		public GorgonBufferFormat DepthStencilFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width and height of the target.
		/// </summary>
		public Size Dimensions
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of the target.
		/// </summary>
		public int Width
		{
			get
			{
				return Dimensions.Width;
			}
			set
			{
				Dimensions = new Size(value, Dimensions.Height);
			}
		}

		/// <summary>
		/// Property to set or return the height of the target.
		/// </summary>
		public int Height
		{
			get
			{
				return Dimensions.Height;
			}
			set
			{
				Dimensions = new Size(Dimensions.Width, value);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetSettings"/> class.
		/// </summary>
		public GorgonRenderTargetSettings()
		{
		}
		#endregion
	}
}
