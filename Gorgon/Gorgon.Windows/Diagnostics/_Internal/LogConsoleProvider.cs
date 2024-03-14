
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 22, 2018 11:34:55 AM
// 


using System.Diagnostics;
using System.Text;
using Gorgon.Native;
using Gorgon.Windows.Properties;
using Microsoft.Win32.SafeHandles;

namespace Gorgon.Diagnostics.LogProviders;

/// <summary>
/// A provider used to store logging messages to a console window
/// </summary>
internal class LogConsoleProvider
    : IGorgonLogProvider
{

    /// <summary>
    /// The type of message to display.
    /// </summary>
    private enum MessageType
    {
        /// <summary>
        /// A normal message.
        /// </summary>
        Normal,
        /// <summary>
        /// An error message.
        /// </summary>
        Error,
        /// <summary>
        /// An exception.
        /// </summary>
        Exception,
        /// <summary>
        /// A warning message.
        /// </summary>
        Warning,
        /// <summary>
        /// Another type of warning message.
        /// </summary>
        Warning2
    }



    // Message strings used to determine the type of message.
    private readonly string _exceptionLine = $"\t{Resources.GOR_LOG_EXCEPTION.ToUpper()}!!";
    private readonly string _errorLine = $"] {Resources.GOR_LOG_ERROR}";
    private readonly string _warnLine1 = $"] {Resources.GOR_LOG_WARNING}";
    private readonly string _warnLine2 = $"] {Resources.GOR_LOG_WARNING2}";

    // Flag to indicate that the app has a console window.
    private int _hasConsole;
    // Flag to indicate that we own the console window.
    private bool _ownsConsole;



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

            nint filePtr = KernelApi.CreateFileW("CONOUT$",
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
    private static string[] GetLines(string message) => string.IsNullOrWhiteSpace(message)
            ? []
            : message.Split(new[]
                             {
                                 '\r',
                                 '\n'
                             },
                             StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Function to hilight a specific word in a line using a console color.
    /// </summary>
    /// <param name="line">The line of text to scan.</param>
    /// <param name="word">The word to look for.</param>
    /// <param name="color">The color to apply.</param>
    /// <returns></returns>
    private static string HilightWord(string line, string word, ConsoleColor color)
    {
        int start = line.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);

        if (start == -1)
        {
            return line;
        }

        Console.ForegroundColor = color;
        Console.Write(" " + word);
        Console.ResetColor();

        return line[(start + word.Length)..];
    }

    /// <summary>
    /// Function to format an individual line.
    /// </summary>
    /// <param name="line">The line to format.</param>
    /// <param name="messageType">The type of message.</param>
    private static void FormatLine(string line, MessageType messageType)
    {
        if (line.StartsWith("[", StringComparison.CurrentCultureIgnoreCase))
        {
            int end = line.IndexOf("]", StringComparison.CurrentCultureIgnoreCase);

            if (end != -1)
            {
                ++end;
                string dateTime = line[..end];

                line = (line.Length - end) > 0 ? line[end..] : string.Empty;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(dateTime);
                Console.ResetColor();
            }
        }

        switch (messageType)
        {
            case MessageType.Exception:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case MessageType.Error:
                line = HilightWord(line, Resources.GOR_LOG_ERROR, ConsoleColor.Red);
                break;
            case MessageType.Warning:
                line = HilightWord(line, Resources.GOR_LOG_WARNING, ConsoleColor.Yellow);
                break;
            case MessageType.Warning2:
                line = HilightWord(line, Resources.GOR_LOG_WARNING2, ConsoleColor.Yellow);
                break;
            default:
                Console.ResetColor();
                break;
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

        string[] lines;

        if (message.IndexOf('\n') != -1)
        {
            lines = GetLines(message);
        }
        else
        {
            lines = [
                        message
                    ];
        }

        MessageType messageType = MessageType.Normal;

        if (message.Contains(_exceptionLine))
        {
            messageType = MessageType.Exception;
        }
        else if (message.Contains(_errorLine))
        {
            messageType = MessageType.Error;
        }
        else if (message.Contains(_warnLine1))
        {
            messageType = MessageType.Warning;
        }
        else if (message.Contains(_warnLine2))
        {
            messageType = MessageType.Warning2;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            FormatLine(lines[i], messageType);
        }
    }

}
