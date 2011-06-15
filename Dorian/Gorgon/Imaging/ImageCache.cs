#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, September 25, 2007 11:02:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Class to represent a cache for images.
	/// </summary>
	public static class ImageCache
	{
		#region Variables.
		private static ImageList _images = null;			// List of images.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the loaded images.
		/// </summary>
		public static ImageList Images
		{
			get
			{
				return _images;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to destroy all the loaded images.
		/// </summary>
		public static void DestroyAll()
		{
			// Destroy all the images.
			while (_images.Count > 0)
				_images[0].Dispose();			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static ImageCache()
		{
			_images = new ImageList();
		}
		#endregion
	}
}
