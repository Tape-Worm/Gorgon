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
// Created: Tuesday, November 14, 2006 11:09:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics.Fonts
{
    /// <summary>
    /// Direction of the font shadow.
    /// </summary>
    public enum FontShadowDirection
    {
        /// <summary>Upper left corner.</summary>
        UpperLeft = 0,
        /// <summary>Middle left.</summary>
        MiddleLeft = 1,
        /// <summary>Lower left corner.</summary>
        LowerLeft = 2,
        /// <summary>Upper middle.</summary>
        UpperMiddle = 3,
        /// <summary>Lower middle.</summary>
        LowerMiddle = 4,
        /// <summary>Upper right corner.</summary>
        UpperRight = 5,
        /// <summary>Middle right.</summary>
        MiddleRight = 6,
        /// <summary>Lower right corner.</summary>
        LowerRight = 7
    }

}
