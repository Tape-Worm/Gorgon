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
// Created: Friday, May 18, 2012 6:43:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Shared resources.
	/// </summary>
	static class SharedResources
	{
		#region Variables.
		private static bool _initialized = false;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the texture used for font tools.
		/// </summary>
		public static GorgonTexture2D FontTools
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the shared resources.
		/// </summary>
		public static void Initialize()
		{
			if (_initialized)
				return;

			FontTools = Program.Graphics.Textures.FromGDIBitmap("Texture.FontTools", Properties.Resources.IBar);

			_initialized = true;
		}

		/// <summary>
		/// Function to terminate the shared resources.
		/// </summary>
		public static void Terminate()
		{
			if (!_initialized)
				return;

			if (FontTools != null)
				FontTools.Dispose();
			FontTools = null;
		}
		#endregion
	}
}
