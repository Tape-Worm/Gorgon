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
// Created: Tuesday, December 08, 2006 10:46:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using SharpUtilities.Collections;

namespace GorgonLibrary.GUI
{
    /// <summary>
    /// Object representing a list of control types for the GUI.
    /// </summary>
    public class GUIControlTypeList
        : Collection<GUIControlType>
    {
        #region Methods.
        /// <summary>
        /// Function to add a new control type.
        /// </summary>
        /// <param name="controlType">Type of control to add.</param>
        /// <returns>A new control type.</returns>
        internal void Add(GUIControlType controlType)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            if (Contains(controlType.Name))
                throw new DuplicateObjectException(controlType.Name);
            
            _items.Add(controlType.Name, controlType);            
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        internal GUIControlTypeList()
        {
        }
        #endregion
    }
}
