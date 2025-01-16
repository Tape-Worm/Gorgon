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
// Created: January 9, 2025 1:56:23 AM
//

using System.Diagnostics;
using Gorgon.Native;

namespace Gorgon.Windows.Input;

/// <summary>
/// The window that will receive input data during polling.
/// </summary>
internal sealed class InputWindow
    : NativeWindow, IDisposable
{
    // WS POPUP style.
    private const int WS_POPUP = unchecked((int)0x80000000);
    // Raw Input window message.
    private const int WM_INPUT = 0x00FF;

    /// <summary>
    /// Property to return the callback used when a WM_INPUT message is received.
    /// </summary>
    public Action<nint> WmInputCallback
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to release managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> if called from <see cref="Dispose()"/>, or <b>false</b> if called from the finalizer.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Handle != IntPtr.Zero)
            {
                DestroyHandle();
            }
        }
    }

    /// <summary>
    /// Function to handle window messages.
    /// </summary>
    /// <param name="m">The message to evaluate.</param>
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_INPUT)
        {
            WmInputCallback(m.LParam);
            m.Result = IntPtr.Zero;
        }

        base.WndProc(ref m);
    }

    /// <summary>
    /// Function to dispose of resources used by this object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputWindow"/> class.
    /// </summary>
    /// <param name="wmInputCallback">The method used to process WM_INPUT messages.</param>
    public InputWindow(Action<nint> wmInputCallback)
    {
        WmInputCallback = wmInputCallback;

        CreateHandle(new CreateParams
        {
            X = 0,
            Y = 0,
            Width = 0,
            Height = 0,
            Style = WS_POPUP
        });
    }
}
