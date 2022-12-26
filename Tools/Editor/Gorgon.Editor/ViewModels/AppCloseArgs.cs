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
// Created: October 28, 2018 2:52:31 PM
// 
#endregion

using System.ComponentModel;
using DX = SharpDX;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Arguments for the <see cref="IMain.AppClosingAsyncCommand"/>.
/// </summary>
internal class AppCloseArgs
    : CancelEventArgs
{
    /// <summary>
    /// Property to return the dimensions for the window when closing.
    /// </summary>
    public DX.Rectangle WindowDimensions
    {
        get;
    }

    /// <summary>
    /// Property to return the state for the window when closing.
    /// </summary>
    public int WindowState
    {
        get;
    }

    /// <summary>Initializes a new instance of the AppCloseArgs class.</summary>
    /// <param name="windowDimensions">  The size of the window at shutdown.</param>
    /// <param name="windowState">  The current state of the window (e.g. maximized, normal, etc...)</param>
    public AppCloseArgs(DX.Rectangle windowDimensions, int windowState)
    {
        WindowDimensions = windowDimensions;
        WindowState = windowState;
    }
}
