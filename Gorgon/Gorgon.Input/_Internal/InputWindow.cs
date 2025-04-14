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

using System.ComponentModel;
using System.Runtime.InteropServices;
using Gorgon.Input.Properties;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.Input;

/// <summary>
/// The window that will receive input data during polling.
/// </summary>
/// <remarks>
/// <para>
/// This is a simple wrapper for a standard message window. Only 1 window at a time can be created.
/// </para>
/// </remarks>
internal sealed class InputWindow
    : IDisposable
{
    // The name of the window class.
    private readonly string _className = $"Gorgon.Input.Window_{{{Guid.NewGuid()}}}";

    // Callbacks used for device change events.
    private Action<HANDLE>? _deviceAttached;
    private Action<HANDLE>? _deviceDetached;
    // These values are static so the finalizer can get to them if we don't dispose correctly.
    // The window handle.
    private static HWND _windowHandle;
    // The atom associated with the window class.
    private static ushort _classAtom;
    // The window procedure, tied to our object lifetime so the GC won't collect it.
    private static WNDPROC? _wndProc;

    /// <summary>
    /// Property to return the handle for the window.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public HWND Handle => _windowHandle;
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Function to release managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> if called from <see cref="Dispose()"/>, or <b>false</b> if called from the finalizer.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _deviceAttached = null;
            _deviceDetached = null;
        }

        HWND windowHandle = _windowHandle;
        ushort atom = _classAtom;

        _wndProc = null;
        _classAtom = 0;
        _windowHandle = HWND.Null;

        if (windowHandle != HWND.Null)
        {
            if (!PInvoke.DestroyWindow(windowHandle))
            {
                // If we fail to destroy the window here, try again by sending a message.
                PInvoke.PostMessage(windowHandle, PInvoke.WM_CLOSE, 0, 0);
            }
        }

        if (atom != 0)
        {
            PInvoke.UnregisterClass(_className, PInvoke.GetModuleHandle(new PCWSTR()));
        }
    }

    /// <summary>
    /// Function to handle messages sent to the window.
    /// </summary>
    /// <param name="hwnd">The handle of the window.</param>
    /// <param name="msg">The window message.</param>
    /// <param name="wParam">The word parameter.</param>
    /// <param name="lParam">The long parameter.</param>
    /// <returns>The window proc result.</returns>
    private LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case PInvoke.WM_CLOSE:
                if (hwnd != HWND.Null)
                {
                    PInvoke.DestroyWindow(hwnd);
                }
                return (LRESULT)0;
            case PInvoke.WM_INPUT_DEVICE_CHANGE:
                switch ((uint)wParam)
                {
                    case PInvoke.GIDC_ARRIVAL:
                        _deviceAttached?.Invoke(new HANDLE(lParam.Value));
                        return (LRESULT)0;
                    case PInvoke.GIDC_REMOVAL:
                        _deviceDetached?.Invoke(new HANDLE(lParam.Value));
                        return (LRESULT)0;
                }

                break;
        }

        return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Finalizer for the class.
    /// </summary>
    ~InputWindow() => Dispose(false);

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
    /// <param name="deviceAttachedCallback">The method to call when a device is attached.</param>
    /// <param name="deviceDetachedCallback">The method to call when a device is detached.</param>
    public InputWindow(Action<HANDLE> deviceAttachedCallback, Action<HANDLE> deviceDetachedCallback)
    {
        _deviceAttached = deviceAttachedCallback;
        _deviceDetached = deviceDetachedCallback;

        unsafe
        {
            fixed (char* strPtr = _className)
            {
                HINSTANCE instance = PInvoke.GetModuleHandle(new PCWSTR());

                _wndProc = WndProc;

                WNDCLASSW wndClass = new()
                {
                    lpszClassName = new PCWSTR(strPtr),
                    hInstance = instance,
                    lpfnWndProc = _wndProc
                };

                _classAtom = PInvoke.RegisterClass(in wndClass);
                if (_classAtom == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), Resources.GORINP_ERR_CANNOT_REGISTER_INPUT_WINDOW);
                }

                _windowHandle = PInvoke.CreateWindowEx(0, _className, "gorgon_input_window", WINDOW_STYLE.WS_POPUP, 0, 0, 0, 0, HWND.HWND_MESSAGE, HMENU.Null, instance, null);

                if (_windowHandle == HWND.Null)
                {
                    PInvoke.UnregisterClass(_className, instance);

                    throw new Win32Exception(Marshal.GetLastWin32Error(), Resources.GORINP_ERR_CANNOT_CREATE_INPUT_WINDOW);
                }
            }
        }
    }
}
