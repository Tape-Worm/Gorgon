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
// Created: Saturday, April 19, 2008 12:41:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.InputDevices
{
    /// <summary>
    /// Joystick driver not present exception.
    /// </summary>
    /// <remarks>Exception thrown when there's no joystick driver present for the joystick ID passed.</remarks>
    public class JoystickDriverNotPresentException
        : GorgonException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public JoystickDriverNotPresentException()
            : base("Unable to find the joystick driver.")
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot retrieve joystick name exception.
    /// </summary>
    /// <remarks>Exception thrown when the function is unable to retrieve the joystick name.</remarks>
    public class JoystickCannotGetNameException
        : GorgonException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ex">Inner exception.</param>
        public JoystickCannotGetNameException(Exception ex)
            : base("Cannot retrieve the name for the joystick.", ex)
        {
        }
        #endregion
    }
}
