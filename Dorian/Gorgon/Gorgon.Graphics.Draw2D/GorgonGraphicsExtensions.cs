#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, February 22, 2012 4:36:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Design;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// Extensions to the main graphics interface.
	/// </summary>
	public static class GorgonGraphicsExtensions
	{
		#region Methods.
		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="target">Default target for the renderer.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.</exception>
		public static Gorgon2D Create2DRenderer(this GorgonGraphics graphics, GorgonSwapChain target)
		{
			Gorgon2D result = null;

			GorgonDebug.AssertNull<GorgonSwapChain>(target, "target");

#if DEBUG
			if (target.Graphics != graphics)
				throw new ArgumentException("The target '" + target.Name + "' was not created by the same graphics interface as this one.", "target");
#endif

			result = new Gorgon2D(target);
			graphics.AddTrackedObject(result);

			return result;
		}
		#endregion
	}
}
