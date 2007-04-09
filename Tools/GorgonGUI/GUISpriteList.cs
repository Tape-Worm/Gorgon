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
// Created: Tuesday, December 08, 2006 10:47:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Drawing = System.Drawing;
using SharpUtilities.Collections;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GUI
{
    /// <summary>
    /// Object representing a list of GUI sprites.
    /// </summary>
    public class GUISpriteList
        : HashMap<Sprite>
    {
		#region Variables.
		private Image _skin = null;			// Skin for the GUI.
		#endregion

		#region Methods.
		/// <summary>
        /// Function to add a sprite to the list.
        /// </summary>
        /// <param name="name">Name of the sprite piece.</param>
		/// <param name="rect">Dimensions of the sprite piece.</param>
        public void Add(string name, Drawing.RectangleF rect)
        {
			Sprite newSprite;		// Sprite used.

            if (Contains(name))
                throw new DuplicateObjectException(name);

			// Create the sprite piece.
			newSprite = new Sprite(string.Empty, name, _skin, rect.X, rect.Y, rect.Width, rect.Height, 0, 0, 0, 0, 0, 1.0f, 1.0f); 

            // Add to the sprite list.
            _items.Add(name, newSprite);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
		/// <param name="skin">Skin to use for the GUI.</param>
        internal GUISpriteList(Image skin)
        {
			_skin = skin;
        }
        #endregion
    }
}
