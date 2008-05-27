#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, May 27, 2008 12:32:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary;

namespace GorgonLibrary.Extras.GUI
{
	/// <summary>
	/// Exception thrown when an input system is not bound to the GUI.
	/// </summary>
	public class GUIInputInvalidException
		: GorgonException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIInputInvalidException"/> class.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="inner">The inner exception that caused this exception.</param>
		public GUIInputInvalidException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GUIInputInvalidException"/> class.
		/// </summary>
		/// <param name="message">Error message.</param>
		public GUIInputInvalidException(string message)
			: this(message, null)
		{
		}
	}
}
