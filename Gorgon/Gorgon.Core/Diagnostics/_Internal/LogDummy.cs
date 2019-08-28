#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Friday, May 23, 2015 3:09:45 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Diagnostics.LogProviders;

namespace Gorgon.Diagnostics
{
    /// <summary>
    /// An implementation of the <see cref="IGorgonThreadedLog"/> type that does nothing.
    /// </summary>
    internal class LogDummy
        : IGorgonThreadedLog
    {
        #region Properties.
        /// <summary>
        /// Property to return the ID of the thread that created the log object.
        /// </summary>
        public int ThreadID
        {
            get;
        }

        /// <summary>
        /// Property to set or return the filtering level of this log.
        /// </summary>
        public LoggingLevel LogFilterLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the name of the application that is being logged.
        /// </summary>
        public string LogApplication => string.Empty;

        /// <summary>
        /// Property to return the provider for this log.
        /// </summary>
        public IGorgonLogProvider Provider => null;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to send an exception to the log.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <remarks>
        /// <para>
        /// If the <see cref="GorgonLog.LogFilterLevel"/> is set to <c>LoggingLevel.NoLogging</c>, then the exception will not be logged. If the filter is set to any other setting, it will be logged 
        /// regardless of filter level.
        /// </para>
        /// </remarks>
        public void LogException(Exception ex)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Function to print a formatted line of text to the log.
        /// </summary>
        /// <param name="formatSpecifier">Format specifier for the line.</param>
        /// <param name="level">Level that this message falls under.</param>
        /// <param name="arguments">List of optional arguments.</param>
        public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Function to perform any one time inital logging.
        /// </summary>
        public void LogStart()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Function to perform any one time final logging.
        /// </summary>
        public void LogEnd()
        {
            // Intentionally left blank.
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="LogDummy"/> class.
        /// </summary>
        public LogDummy() => ThreadID = Thread.CurrentThread.ManagedThreadId;
        #endregion
    }
}
