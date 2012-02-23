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
// Created: Wednesday, February 22, 2012 4:30:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Design
{
	/// <summary>
	/// An interface to allow access to the gorgon object tracker for external components.
	/// </summary>
	public static class GorgonExternalComponentExtension
	{
		#region Methods.
		/// <summary>
		/// Function to add a new object to the object tracker.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		/// <param name="trackedObject">Object to track.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="graphics"/> or the <paramref name="trackedObject"/> parameters are NULL (Nothing in VB.Net).</exception>
		public static void AddTrackedObject(this GorgonGraphics graphics, IDisposable trackedObject)
		{
			GorgonDebug.AssertNull<GorgonGraphics>(graphics, "graphics");
			GorgonDebug.AssertNull<IDisposable>(trackedObject, "trackedObject");
						
			graphics.TrackedObjects.Add(trackedObject);
		}
		#endregion
	}
}
