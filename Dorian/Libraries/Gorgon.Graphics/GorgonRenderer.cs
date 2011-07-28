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
// Created: Tuesday, July 19, 2011 8:41:23 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The renderer interface.
	/// </summary>
	public abstract class GorgonRenderer
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that is bound to this renderer.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderer"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns the renderer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonRenderer(GorgonGraphics graphics)			
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");
		}
		#endregion
	}
}
