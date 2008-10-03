#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
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
