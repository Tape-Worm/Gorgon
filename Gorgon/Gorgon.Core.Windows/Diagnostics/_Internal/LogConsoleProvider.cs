
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Core.Windows.Properties;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;

namespace Gorgon.Diagnostics.LogProviders;

/// <summary>
/// A provider used to store logging messages to a console window
/// </summary>
internal class LogConsoleProvider
    : IGorgonLogProvider
{
    // Line separator.
    private readonly char[] _lineSep = ['\r', '\n'];

    // Flag to indicate that the app has a console window.
    private int _hasConsole;
    // Flag to indicate that we own the console window.
    private bool _ownsConsole;
    // The handle to the console stream.
    private SafeFileHandle? _consoleHandle;

    /// <summary>
    /// Function to close the data store for writing.
    /// </summary>
    /// <param name="closingMessage">[Optional] The message to write when closing.</param>
    public void Close(string? closingMessage = null)
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

        // If we've created a custom output stream, then shut it down now.
        SafeFileHandle? handle = Interlocked.Exchange(ref _consoleHandle, null);
        handle?.Dispose();

        if (!_ownsConsole)
        {
            return;
        }

        Debug.Assert(PInvoke.FreeConsole(), "Console window was not freed.");
        _ownsConsole = false;
    }

    /// <summary>
    /// Function to open the data store for writing.
    /// </summary>
    /// <param name="initialMessage">[Optional] The initial message to write.</param>
    public void Open(string? initialMessage = null)
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
                if (!PInvoke.AllocConsole())
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
            HANDLE filePtr = PInvoke.CreateFile("CONOUT$",
                                                (uint)(GENERIC_ACCESS_RIGHTS.GENERIC_READ | GENERIC_ACCESS_RIGHTS.GENERIC_WRITE),
                                                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                                                null,
                                                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                                                0,
                                                HANDLE.Null);

            if (filePtr == HANDLE.INVALID_HANDLE_VALUE)
            {
                return;
            }

            PInvoke.SetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE, filePtr);

            unsafe
            {
                _consoleHandle = new((nint)filePtr.Value, true);
                FileStream stream = new(_consoleHandle, FileAccess.Write);
                StreamWriter writer = new(stream, Encoding.Default)
                {
                    AutoFlush = true
                };
                Console.SetOut(writer);

                if (PInvoke.GetConsoleMode(filePtr, out CONSOLE_MODE consoleMode))
                {
                    PInvoke.SetConsoleMode(filePtr, consoleMode | CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT);
                }
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
    /// Function to hilight a specific word in a line using a console color.
    /// </summary>
    /// <param name="line">The line of text to scan.</param>
    /// <param name="word">The word to look for.</param>
    /// <param name="color">The color to apply.</param>
    /// <returns>The formatted string.</returns>
    private static ReadOnlySpan<char> HilightWord(ReadOnlySpan<char> line, string word, ConsoleColor color)
    {
        int start = line.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);

        if (start == -1)
        {
            return line;
        }

        Console.ForegroundColor = color;
        Console.Write($" [{word}]:");
        Console.ResetColor();

        return line[(start + word.Length + 2)..];
    }

    /// <summary>
    /// Function to format an individual line.
    /// </summary>
    /// <param name="line">The line to format.</param>
    /// <param name="messageType">The type of message.</param>
    private static void FormatLine(ReadOnlySpan<char> line, MessageType messageType)
    {
        if (line.StartsWith("[", StringComparison.CurrentCultureIgnoreCase))
        {
            int end = line.IndexOf("]", StringComparison.CurrentCultureIgnoreCase);

            if (end != -1)
            {
                int nextBlock = end + 1;
                // Skip the thread ID.
                if ((nextBlock < line.Length) && (line[nextBlock] == '['))
                {
                    ReadOnlySpan<char> dateStartLine = line[(end + 1)..];
                    end = dateStartLine.IndexOf("]", StringComparison.CurrentCultureIgnoreCase) + nextBlock;
                }

                if (end != -1)
                {
                    ++end;
                    ReadOnlySpan<char> dateTime = line[..end];

                    line = (line.Length - end) > 0 ? line[end..] : string.Empty;

                    Console.ForegroundColor = messageType switch
                    {
                        MessageType.Warning => ConsoleColor.DarkYellow,
                        MessageType.Error or MessageType.Critical => ConsoleColor.DarkRed,
                        _ => ConsoleColor.DarkGreen,
                    };
                    Console.Write($"{dateTime}");
                    Console.ResetColor();
                }
            }
        }

        switch (messageType)
        {
            case MessageType.Critical:
                line = HilightWord(line, Resources.GOR_LOG_EXCEPTION, ConsoleColor.DarkRed);
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case MessageType.Error:
                line = HilightWord(line, Resources.GOR_LOG_ERROR, ConsoleColor.DarkRed);
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case MessageType.Warning:
                line = HilightWord(line, Resources.GOR_LOG_WARNING, ConsoleColor.DarkYellow);
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            default:
                Console.ResetColor();
                break;
        }

        Console.WriteLine(line.ToString());
        Console.ResetColor();
    }

    /// <inheritdoc/>
    public void SendMessage(string message, MessageType messageType)
    {
        if (_hasConsole != 1)
        {
            return;
        }

        ReadOnlySpan<char> messageSpan = message.AsSpan();
        GorgonSpanCharEnumerator spanEnum = messageSpan.Split(_lineSep);

        foreach (ReadOnlySpan<char> line in spanEnum)
        {
            FormatLine(line, messageType);
        }
    }
}
