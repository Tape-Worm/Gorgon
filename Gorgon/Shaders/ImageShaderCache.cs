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
// Created: Wednesday, September 26, 2007 12:24:09 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Class representing a cache for image shaders.
	/// </summary>
	public static class ImageShaderCache
	{
		#region Variables.
		private static ImageShaderList _imageShaders = null;			// List of image shaders.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the image shaders in this factory.
		/// </summary>
		public static ImageShaderList ImageShaders
		{
			get
			{
				return _imageShaders;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to destroy all the image shaders.
		/// </summary>
		public static void DestroyAll()
		{
			while (_imageShaders.Count > 0)
				_imageShaders[0].Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static ImageShaderCache()
		{
			_imageShaders = new ImageShaderList();
		}
		#endregion
	}
}
