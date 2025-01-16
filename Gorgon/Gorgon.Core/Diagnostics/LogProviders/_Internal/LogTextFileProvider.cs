
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
// Created: March 22, 2018 11:32:20 AM
// 

using System.Diagnostics;
using Gorgon.Core;
using Gorgon.IO;

namespace Gorgon.Diagnostics.LogProviders;

/// <summary>
/// A provider used to store logging messages to a text file
/// </summary>
internal class LogTextFileProvider
    : IGorgonLogProvider
{
    // The file information for the log file.
    private readonly FileInfo _filePath;

    /// <summary>
    /// Function to open the data store for writing.
    /// </summary>
    /// <param name="initialMessage">[Optional] The initial message to write.</param>
    public void Open(string? initialMessage = null)
    {
        Debug.Assert(_filePath.Directory is not null, $"Directory not found for '{_filePath.FullName}'");

        // Create the directory if it doesn't exist.
        if (!_filePath.Directory.Exists)
        {
            _filePath.Directory.Create();
            _filePath.Directory.Refresh();
        }

        // Open the stream.
        using StreamWriter stream = new(File.Open(_filePath.FullName, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        if (!string.IsNullOrWhiteSpace(initialMessage))
        {
            stream.WriteLine(initialMessage);
        }
        stream.Flush();
    }

    /// <summary>
    /// Function to write a message to the data store.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void SendMessage(string message)
    {
        if (!_filePath.Exists)
        {
            return;
        }

        message ??= string.Empty;

        using StreamWriter stream = new(File.Open(_filePath.FullName, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        stream.WriteLine(message);
        stream.Flush();
    }

    /// <summary>
    /// Function to close the data store for writing.
    /// </summary>
    /// <param name="closingMessage">[Optional] The message to write when closing.</param>
    public void Close(string? closingMessage)
    {
        if ((!_filePath.Exists)
            || (string.IsNullOrWhiteSpace(closingMessage)))
        {
            return;
        }

        using StreamWriter stream = new(File.Open(_filePath.FullName, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        stream.WriteLine(closingMessage);
        stream.Flush();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogTextFileProvider"/> class.
    /// </summary>
    /// <param name="filePath">The path to the file to write into.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    public LogTextFileProvider(string filePath)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(filePath);

        _filePath = new FileInfo(filePath.FormatPath(Path.DirectorySeparatorChar));
    }
}
