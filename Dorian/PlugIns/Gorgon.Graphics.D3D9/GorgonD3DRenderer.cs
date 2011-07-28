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
// Created: Tuesday, July 19, 2011 5:09:59 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using Microsoft.Win32;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// The interface for the Direct 3D9 renderer.
	/// </summary>
	class GorgonD3D9Renderer
		: GorgonRenderer
	{
		#region Constants.
		#endregion

		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface.
		/// </summary>
		public new GorgonD3D9Graphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonD3D9Renderer"/> class.
		/// </summary>
		internal GorgonD3D9Renderer(GorgonGraphics graphics)
			: base(graphics)
		{			
		}
		#endregion
	}
}
