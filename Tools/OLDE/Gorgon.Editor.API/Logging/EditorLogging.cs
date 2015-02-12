#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Friday, May 02, 2014 10:03:18 PM
// 
#endregion

using System;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Interface for the editor logging.
    /// </summary>
    public class EditorLogging
    {
        #region Variables.
        // The log file.
        private static GorgonLogFile _logFile;
        #endregion

        #region Properties.
        
        #endregion

        #region Methods.
        /// <summary>
        /// Function to handle an exception with this instance of the logger.
        /// </summary>
        /// <param name="ex">Exception to catch.</param>
        /// <param name="handler">Exception handler to call.</param>
        public static Exception CatchException(Exception ex, GorgonExceptionHandler handler = null)
        {
            if ((_logFile == null)
                || (_logFile.IsClosed))
            {
                return ex;
            }

            GorgonLogFile oldLogger = GorgonException.Logs;

            try
            {
                GorgonException.Logs = _logFile;
                return GorgonException.Catch(ex, handler);
            }
            finally
            {
                GorgonException.Logs = oldLogger;
            }
        }

        /// <summary>
        /// Function to print a message to the log.
        /// </summary>
        /// <param name="format">Format specifier for the logging.</param>
        /// <param name="values">Optional values to supply to the logger.</param>
        public static void Print(string format, params object[] values)
        {
            if ((_logFile == null)
                || (_logFile.IsClosed))
            {
                return;
            }

            _logFile.Print(format, LoggingLevel.All, values);
        }

        /// <summary>
        /// Function to close the logger.
        /// </summary>
        public static void Close()
        {
            if ((_logFile == null)
                || (_logFile.IsClosed))
            {
                return;
            }

            _logFile.Close();
            _logFile = null;
        }

        /// <summary>
        /// Function to open the log file for writing.
        /// </summary>
        public static void Open()
        {
            _logFile = new GorgonLogFile("Gorgon.Editor", "Tape_Worm");

            try
            {
                _logFile.Open();
            }
#if DEBUG
            catch (Exception ex)
            {
                // If we can't open the log file in debug mode, let us know about it.
                GorgonDialogs.ErrorBox(null, ex);
            }
#else
			catch
			{
				// Purposely left empty.  We don't care about the log file in release mode.
			}
#endif
        }
        #endregion
    }
}
