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
// Created: Monday, November 20, 2006 7:06:56 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Animations
{
    /// <summary>
    /// Enumeration for interpolation modes.
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// No interpolation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Linear interpolation.
        /// </summary>
        Linear = 1,
        /// <summary>
        /// Spline interpolation.
        /// </summary>
        Spline = 2
    }
}