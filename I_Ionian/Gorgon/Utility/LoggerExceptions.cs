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
		: Exception
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
        : Exception
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
