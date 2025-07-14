// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 12, 2025 7:33:51 PM
//

namespace Gorgon.UI.Win32;

/// <summary>
/// A data structure containing information for a Windows message.
/// </summary>
/// <param name="window">The window receiving the message.</param>    
/// <param name="message">The messaging being transmitted.</param>
/// <param name="wParam">A parameter for the message.</param>
/// <param name="lParam">Another parameter for the message.</param>
public readonly struct GorgonWindowMessage(GorgonWindow window, int message, nuint wParam, nint lParam)
{
    /// <summary>
    /// The window receiving the message.
    /// </summary>
    public readonly GorgonWindow Window = window;

    /// <summary>
    /// The window message being transmitted.
    /// </summary>
    /// <remarks>
    /// Users can compare this value with the <see cref="GorgonWindowMessage"/> class constants.
    /// </remarks>
    public readonly int Message = message;

    /// <summary>
    /// A parameter for the message.
    /// </summary>
    public readonly nuint WParam = wParam;

    /// <summary>
    /// Another parameter for the message.
    /// </summary>
    public readonly nint LParam = lParam;
}
