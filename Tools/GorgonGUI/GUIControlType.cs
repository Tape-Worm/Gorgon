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
// Created: Tuesday, December 08, 2006 10:39:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GUI
{
    /// <summary>
    /// Object representing a type of GUI control.
    /// </summary>
    public class GUIControlType
        : NamedObject
    {
        #region Variables.
        private GUISpriteList _sprites = null;          // Control types.
        private GUISkin _skin = null;                   // Skin for the GUI.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of control types.
        /// </summary>
        public GUISpriteList Sprites
        {
            get
            {
                return _sprites;
            }
        }

        /// <summary>
        /// Property to return the GUI skin.
        /// </summary>
        public GUISkin Skin
        {
            get
            {
                return _skin;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the control type.</param>
        /// <param name="skin">Skin for the GUI.</param>
        internal GUIControlType(string name, GUISkin skin)
            : base(name)
        {
            _sprites = new GUISpriteList(skin.Image);
            _skin = skin;
        }
        #endregion
    }
}
