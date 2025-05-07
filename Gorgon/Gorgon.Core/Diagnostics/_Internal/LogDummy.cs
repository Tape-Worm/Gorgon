
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
// Created: Friday, May 23, 2015 3:09:45 PM
// 

using System.Diagnostics;
using Gorgon.Diagnostics.LogProviders;

namespace Gorgon.Diagnostics;

/// <summary>
/// An implementation of the <see cref="IGorgonLog"/> type that does nothing.
/// </summary>
[DebuggerStepThrough()]
internal class LogDummy
    : IGorgonLog
{
    /// <inheritdoc/>
    public int ThreadID
    {
        get;
    }

    /// <inheritdoc/>
    public LoggingLevel LogFilterLevel
    {
        get;
        set;
    }

    /// <inheritdoc/>
    public string LogApplication => string.Empty;

    /// <inheritdoc/>
    public IGorgonLogProvider Provider
    {
        get;
    } = new DummyLogProvider();

    /// <inheritdoc/>
    public void PrintException(Exception ex)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void Print(string message, LoggingLevel level)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void PrintError(string message, LoggingLevel level)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void PrintWarning(string message, LoggingLevel level)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void LogStart(IGorgonComputerInfo? computerInfo = null)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void LogEnd()
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogDummy"/> class.
    /// </summary>
    public LogDummy() => ThreadID = Environment.CurrentManagedThreadId;
}
