#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: March 22, 2018 11:34:55 AM
// 
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Gorgon.Native;
using Gorgon.Windows.Properties;
using Microsoft.Win32.SafeHandles;

namespace Gorgon.Diagnostics.LogProviders
{
    /// <summary>
    /// A provider used to store logging messages to a console window.
    /// </summary>
    public class GorgonLogConsoleProvider
        : IGorgonLogProvider
    {
        #region Variables.
        // Flag to indicate that the app has a console window.
        private int _hasConsole;
        // Flag to indicate that we own the console window.
        private bool _ownsConsole;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to close the data store for writing.
        /// </summary>
        /// <param name="closingMessage">[Optional] The message to write when closing.</param>
        public void Close(string closingMessage = null)
        {
            if (Interlocked.Exchange(ref _hasConsole, 0) == 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(closingMessage))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(closingMessage);
            }

            Console.ResetColor();

            if (!_ownsConsole)
            {
                return;
            }

            Debug.Assert(KernelApi.FreeConsole() != 0);
            _ownsConsole = false;
        }

        /// <summary>
        /// Function to open the data store for writing.
        /// </summary>
        /// <param name="initialMessage">[Optional] The initial message to write.</param>
        public void Open(string initialMessage = null)
        {
            if (Interlocked.Exchange(ref _hasConsole, 1) == 1)
            {
                return;
            }

            // Check for an existing console first.
            using (Stream stdIn = Console.OpenStandardInput())
            {
                // If it does not exist, then create one.
                if (stdIn == Stream.Null)
                {
                    if (!KernelApi.AllocConsole())
                    {
                        return;
                    }

                    _ownsConsole = true;
                }
            }

            // If we have Enable Native Debugging turned on, all output is redirected to the debug window.
            // This will reset that back to our logging window.
            if (Console.IsOutputRedirected)
            {
                const uint genericRead = 0x80000000;
                const uint genericWrite = 0x40000000;

                IntPtr filePtr = KernelApi.CreateFileW("CONOUT$",
                                                       (genericRead | genericWrite),
                                                       (uint)FileShare.ReadWrite,
                                                       IntPtr.Zero,
                                                       (uint)FileMode.Open,
                                                       0,
                                                       IntPtr.Zero);
                var handle = new SafeFileHandle(filePtr, true);
                if (handle.IsInvalid)
                {
                    handle.Dispose();
                    return;
                }

                KernelApi.SetStdHandle(KernelApi.StdOutputHandle, filePtr);

                var stream = new FileStream(handle, FileAccess.Write);
                var writer = new StreamWriter(stream, Encoding.Default)
                             {
                                 AutoFlush = true
                             };
                Console.SetOut(writer);

                if (KernelApi.GetConsoleMode(filePtr, out uint consoleMode))
                {
                    KernelApi.SetConsoleMode(filePtr, consoleMode | 0x200);
                }
            }

            if (string.IsNullOrWhiteSpace(initialMessage))
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(initialMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// Function to break the message up into individual lines.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>The array containing each line.</returns>
        private static string[] GetLines(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return new string[0];
            }

            return message.Split(new[]
                                 {
                                     '\r',
                                     '\n'
                                 },
                                 StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Function to format an individual line.
        /// </summary>
        /// <param name="line">The line to format.</param>
        /// <param name="isException"><b>true</b> if the line is part of an exception, <b>false</b> if not.</param>
        private static void FormatLine(string line, bool isException)
        {
            if (line.StartsWith("[", StringComparison.CurrentCultureIgnoreCase))
            {
                int end = line.IndexOf("]", StringComparison.CurrentCultureIgnoreCase);

                if (end != -1)
                {
                    ++end;
                    string dateTime = line.Substring(0, end);

                    line = (line.Length - end) > 0 ? line.Substring(end, line.Length - end) : string.Empty;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(dateTime);
                }
            }

            if (isException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ResetColor();
            }

            Console.WriteLine(line);

            Console.ResetColor();
        }

        /// <summary>
        /// Function to write a message to the data store.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void SendMessage(string message)
        {
            if (_hasConsole != 1)
            {
                return;
            }

            string exceptionLine = $"\t{Resources.GOR_LOG_EXCEPTION.ToUpper()}!!";
            string[] lines;

            if (message.IndexOf('\n') != -1)
            {
                lines = GetLines(message);
            }
            else
            {
                lines = new []
                        {
                            message
                        };
            }

            bool isException = message.Contains(exceptionLine);

            for (var i = 0; i < lines.Length; i++)
            {
                FormatLine(lines[i], isException);
            }
        }
        #endregion
    }
}
