#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, November 15, 2008 12:33:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DX = SlimDX;
using D3D = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.SlimDXRenderer
{
	/// <summary>
	/// Renderer for SlimDX.
	/// </summary>
	public class SlimDXRenderer
		: Renderer
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		private D3D.Direct3DEx _direct3DEx = null;		// Direct 3D ex object.
		private D3D.Direct3D _direct3D = null;			// Direct 3D object.
		#endregion

		#region Properties.

		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SlimDXRenderer"/> class.
		/// </summary>
		/// <param name="plugIn">The plug in.</param>
		public SlimDXRenderer(SlimDXRendererPlugIn plugIn)
			: base(plugIn)
		{
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to remove all objects, FALSE to remove only unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					
				}

				_disposed = true;
			}
		}
		#endregion
	}
}
