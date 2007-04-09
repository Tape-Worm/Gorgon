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
// Created: Saturday, January 06, 2007 2:10:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using SharpUtilities;
using SharpUtilities.Mathematics;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a manager for text sprite objects.
	/// </summary>
	public class TextSpriteManager
		: Manager<TextSprite>
	{
		#region Methods.
		/// <summary>
		/// Function to add a text sprite to the manager.
		/// </summary>
		/// <param name="textSprite">Text sprite object to add.</param>
		public void Add(TextSprite textSprite)
		{
			AddObject(textSprite);
		}

		/// <summary>
		/// Function to clear the text sprites.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to remove a text sprite by index.
		/// </summary>
		/// <param name="index">Index of the text sprite.</param>
		public void Remove(int index)
		{
			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a text sprite by its key name.
		/// </summary>
		/// <param name="key">Name of the text sprite.</param>
		public void Remove(string key)
		{
			RemoveItem(key);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public TextSpriteManager()
			: base(true)
		{
		}
		#endregion
	}
}
