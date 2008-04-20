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
// Created: Saturday, April 19, 2008 11:58:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary
{
    /// <summary>
    /// Cannot open log file.
    /// </summary>
    public class LogfileCannotOpenException
        : GorgonException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogfileCannotOpenException"/> class.
        /// </summary>
        /// <param name="fileName">Name of the log file.</param>
        /// <param name="ex">The inner exception.</param>
        public LogfileCannotOpenException(string fileName, Exception ex)
            : base("Unable to open the log file '" + fileName + "' for writing.", ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogfileCannotOpenException"/> class.
        /// </summary>
        /// <param name="fileName">Name of the log file.</param>
        public LogfileCannotOpenException(string fileName)
            : this(fileName, null)
        {
        }
    }

    /// <summary>
    /// Cannot write to the log file.
    /// </summary>
    public class LogfileCannotWriteException
        : GorgonException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogfileCannotOpenException"/> class.
        /// </summary>
        /// <param name="fileName">Name of the log file.</param>
        /// <param name="ex">The inner exception.</param>
        public LogfileCannotWriteException(string fileName, Exception ex)
            : base("Unable to write to the log file '" + fileName + "'.", ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogfileCannotOpenException"/> class.
        /// </summary>
        /// <param name="fileName">Name of the log file.</param>
        public LogfileCannotWriteException(string fileName)
            : this(fileName, null)
        {
        }
    }
}
