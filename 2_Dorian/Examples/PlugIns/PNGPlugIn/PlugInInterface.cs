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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

// We HAVE to have this (can also go into assembly.cs as well), this way Gorgon will know if this is a valid plug-in assembly or not.
[assembly: PlugIn()]

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Object representing a sprite export plug-in.
	/// </summary>
	/// <remarks>Our plug-in will remarkably export a sprite to a PNG file.  Crazy!</remarks>
	public class SpriteExportPNGPlugIn
		: SpriteExportPlugIn
	{
		#region Methods.
		/// <summary>
		/// Function to create a new object from the plug-in.
		/// </summary>
		/// <param name="parameters">Parameters to pass.</param>
		/// <returns>The new object.</returns>
		protected override object CreateImplementation(object[] parameters)
		{
			return new SpritePNGExport();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteExportPlugIn"/> class.
		/// </summary>
		/// <param name="path">Path to the plug-in.</param>
		public SpriteExportPNGPlugIn(string path)
			: base("SpritePNGExport", path)
		{
		}
		#endregion
	}

	/// <summary>
	/// Object representing a PNG sprite exporter.
	/// </summary>
	public class SpritePNGExport
		: SpriteExporter
	{
		#region Methods.
		/// <summary>
		/// Function to save the image.
		/// </summary>
		/// <param name="image">Image to save.</param>
		/// <param name="path">Path for the image.</param>
		protected override void Save(Image image, string path)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			// Save to a PNG file.
			image.Save(path, ImageFileFormat.PNG);
		}
		#endregion
	}
}
