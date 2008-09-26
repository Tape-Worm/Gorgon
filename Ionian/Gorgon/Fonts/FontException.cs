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
