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
// Created: Thursday, December 15, 2011 1:24:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Shader state object.
	/// </summary>
	public class GorgonShaderState
	{
		#region Variables.
		private GorgonVertexShader _vertexShader = null;			// Current vertex shader.
		private GorgonPixelShader _pixelShader = null;				// Current pixel shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public GorgonVertexShader VertexShader
		{
			get
			{
				return _vertexShader;
			}
			set
			{
				if (value != _vertexShader)
				{
					_vertexShader = value;
					if (_vertexShader != null)
						_vertexShader.Assign();
					else
						Graphics.Context.VertexShader.Set(null);
				}
			}
		}

		/// <summary>
		/// Property to set or return the current pixel shader.
		/// </summary>
		public GorgonPixelShader PixelShader
		{
			get
			{
				return _pixelShader;
			}
			set
			{
				if (_pixelShader != value)
				{
					_pixelShader = value;
					if (_pixelShader != null)
						_pixelShader.Assign();
					else
						Graphics.Context.PixelShader.Set(null);
				}
			}
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonShaderState(GorgonGraphics graphics)
		{
			Graphics = graphics;
		}
		#endregion
	}
}
