#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Tuesday, October 31, 2006 4:55:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Error codes for sprite exceptions.
	/// </summary>
	public enum SpriteErrors
	{
		/// <summary>Sprite size is invalid.</summary>
		InvalidSpriteSize = 0x7FFF0010,
	}

	/// <summary>
	/// Base exception for sprite exceptions.
	/// </summary>
	public abstract class SpriteException
		: ResourceException
	{
		#region "Constructor."
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public SpriteException(string message, SpriteErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Sprite size exception.
	/// </summary>
	public class SpriteSizeException
		: SpriteException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public SpriteSizeException(string description, Exception ex)
			: base(description, SpriteErrors.InvalidSpriteSize, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public SpriteSizeException(Exception ex)
			: this("A sprite cannot have a width or height of 0.", ex)
		{
		}
		#endregion
	}
}
