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
// Created: Saturday, April 19, 2008 12:55:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics.Tools
{
    /// <summary>
    /// A group of handy utility functions.
    /// </summary>
    static class Utilities
    {
        /// <summary>
        /// Function to determine if a function is numeric or not.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <returns>TRUE if the value is numeric, FALSE if not.</returns>
        public static bool IsNumeric(string value)
        {
            Decimal result;							// Result.

            return Decimal.TryParse(value, out result);
        }
    }
}
