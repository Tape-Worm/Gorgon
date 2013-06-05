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
// Created: Tuesday, June 4, 2013 10:22:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A render target bound to a buffer.
	/// </summary>
	public class GorgonRenderTargetBuffer
		: GorgonBuffer
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default render target view for this render target.
		/// </summary>
		public GorgonRenderTargetView DefaultRenderTargetView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for this render target.
		/// </summary>
		public new GorgonRenderTargetBufferSettings Settings
		{
			get;
			private set;
		}

		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the render target.</param>
		/// <param name="settings">Settings to apply to the render target.</param>
		internal GorgonRenderTargetBuffer(GorgonGraphics graphics, string name, GorgonRenderTargetBufferSettings settings)
			: base(graphics, name, settings)
		{
			Settings = settings;
		}
		#endregion
	}
}
