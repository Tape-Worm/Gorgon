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
// Created: Tuesday, September 25, 2007 11:02:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Class to represent a cache for fonts.
	/// </summary>
	public static class FontCache
	{
		#region Variables.
		private static FontList _fonts = null;			// List of fonts.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the loaded fonts.
		/// </summary>
		public static FontList Fonts
		{
			get
			{
				return _fonts;
			}
		}
		#endregion

		#region Methods.		
		/// <summary>
		/// Function to destroy all the loaded fonts.
		/// </summary>
		public static void DestroyAll()
		{
			// Destroy all the fonts.
			while (_fonts.Count > 0)
				_fonts[0].Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static FontCache()
		{
			_fonts = new FontList();
		}
		#endregion
	}
}
