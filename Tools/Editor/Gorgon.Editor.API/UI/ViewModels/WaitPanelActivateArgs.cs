
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
// Created: August 31, 2018 2:35:59 PM
// 

namespace Gorgon.Editor.UI;

/// <summary>
/// Event arguments for the <see cref="IViewModel.WaitPanelActivated"/> event
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitPanelActivateArgs"/> class
/// </remarks>
/// <param name="message">The message to display on the wait panel overlay.</param>
/// <param name="title">The title for the for wait panel overlay.</param>
public class WaitPanelActivateArgs(string message, string title)
        : EventArgs
{
    /// <summary>
    /// Property to return the message for the wait panel overlay.
    /// </summary>
    public string Message
    {
        get;
    } = message ?? string.Empty;

    /// <summary>
    /// Property to return the title for the wait panel overlay.
    /// </summary>
    public string Title
    {
        get;
    } = title ?? string.Empty;

}
