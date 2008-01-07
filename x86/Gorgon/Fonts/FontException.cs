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
// Created: Thursday, April 12, 2007 5:04:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Font already loaded.
	/// </summary>
	public class FontAlreadyLoadedException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the font.</param>
		/// <param name="ex">Source exception.</param>
		public FontAlreadyLoadedException(string name, Exception ex)
			: base("The font '" + name + "' is already loaded.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the font.</param>
		public FontAlreadyLoadedException(string name)
			: this(name, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Font is invalid.
	/// </summary>
	public class FontNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public FontNotValidException(Exception ex)
			: base("This is not a valid font.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FontNotValidException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Font backing image format is invalid.
	/// </summary>
	public class FontImageFormatNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public FontImageFormatNotValidException(Exception ex)
			: base("Font backing image requires an alpha channel.  No suitable alpha capable format found.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FontImageFormatNotValidException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Font glyph not found.
	/// </summary>
	public class FontGlyphNotFoundException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="glyph">Character representing the font glyph.</param>
		/// <param name="ex">Source exception.</param>
		public FontGlyphNotFoundException(char glyph, Exception ex)
			: base("The glyph '" + glyph.ToString() + "' is not loaded into this font.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="glyph">Character representing the font glyph.</param>
		public FontGlyphNotFoundException(char glyph)
			: this(glyph, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Font glyph too large for backing image.
	/// </summary>
	public class FontGlyphTooLargeException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public FontGlyphTooLargeException(Exception ex)
			: base("A glyph is too large to fit within the backing image store.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FontGlyphTooLargeException()
			: this(null)
		{
		}
		#endregion
	}
}
