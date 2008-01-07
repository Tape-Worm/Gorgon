#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Tuesday, December 11, 2007 9:56:58 AM
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
