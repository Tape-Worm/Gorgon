// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: March 26, 2024 12:05:11 AM
//

using Gorgon.Diagnostics.LogProviders;

namespace Gorgon.Diagnostics;

/// <summary>
/// A dummy provider for the dummy log.
/// </summary>
internal class DummyLogProvider
    : IGorgonLogProvider
{
    /// <summary>
    /// An instance of a provider that does nothing.
    /// </summary>
    public static readonly DummyLogProvider NullProvider = new();

    /// <inheritdoc/>
    public void Close(string? closingMessage = null)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void Open(string? initialMessage = null)
    {
        // Intentionally left blank.
    }

    /// <inheritdoc/>
    public void SendMessage(string message)
    {
        // Intentionally left blank.
    }
}
