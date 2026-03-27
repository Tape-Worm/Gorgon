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
// Created: July 12, 2025 12:48:51 AM
//

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.UI.Win32.Properties;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.UI.Win32;

/// <summary>
/// The state of the window.
/// </summary>
public enum WindowState
{
    /// <summary>
    /// The window is minimized.
    /// </summary>
    Minimized = SHOW_WINDOW_CMD.SW_MINIMIZE,
    /// <summary>
    /// The window is maximized.
    /// </summary>
    Maximized = SHOW_WINDOW_CMD.SW_MAXIMIZE,
    /// <summary>
    /// The window is neither maximized or minimized.
    /// </summary>
    Normal = SHOW_WINDOW_CMD.SW_RESTORE,
}

/// <summary>
/// The type of border on the window.
/// </summary>
public enum WindowBorder
{
    /// <summary>
    /// No border.
    /// </summary>
    None = 0,
    /// <summary>
    /// The border is resizable.
    /// </summary>
    Resizable = 1,
    /// <summary>
    /// The border is visible, but cannot be resized.
    /// </summary>
    Static = 2
}

/// <summary>
/// Flags for window decorations on the window.
/// </summary>
[Flags]
public enum WindowDecorations
{
    /// <summary>
    /// No decorations.
    /// </summary>
    None = 0,
    /// <summary>
    /// Window has a system menu.
    /// </summary>
    SystemMenu = 1,
    /// <summary>
    /// Window has a minimize button.
    /// </summary>
    MinimizeBox = 2,
    /// <summary>
    /// Window has a maximize button.
    /// </summary>
    Maximizebox = 4,
    /// <summary>
    /// All window decorations.
    /// </summary>
    All = SystemMenu | MinimizeBox | Maximizebox
}

/// <summary>
/// The window procedure used to handle messages to the window.
/// </summary>
/// <param name="messsage">The window message to send.</param>
/// <returns><b>true</b> if we've handled the message, or <b>false</b> if the system can continue processing the message.</returns>
/// <remarks>
/// <para>
/// The <see cref="GorgonWindowMessage.Message"/> parameter of the <paramref name="messsage"/> parameters can be compared with the <see cref="GorgonWindowMessages"/> class constants to determine which 
/// message is being processed.
/// </para>
/// <para>
/// Users should consult the Microsoft learning website to determine how to use the message (e.g. <a href="https://learn.microsoft.com/en-us/windows/win32/gdi/wm-paint">WM_PAINT</a>). Applications that 
/// handle a message should set the <see cref="GorgonWindowMessageResult.Handled"/> value to <b>true</b> when creating the result value, and return the required value in the 
/// <see cref="GorgonWindowMessageResult.Result"/> value (which often is just <c>0</c> - Always check the documentation for the window message). 
/// </para>
/// </remarks>
/// <seealso cref="GorgonWindowMessages"/>
public delegate GorgonWindowMessageResult GorgonWindowProcedure(ref readonly GorgonWindowMessage messsage);

/// <summary>
/// A basic Win32 window for use in applications that don't need or can't use Windows Forms functionality.
/// </summary>
/// <remarks>
/// <para>
/// This is a very simple wrapper around Win32, and as such, it is not suitable for creating a full on application with multiple windows, controls, etc... This is meant for applications that need to bring 
/// up a window and will be doing all UI themselves within the client area.
/// </para>
/// <para>
/// For applications that can't use Windows Forms because of Native AOT (at least as of this writing), this is the ideal means of getting your application up and running with AOT compilation and a custom 
/// user interface.
/// </para>
/// <para>
/// <h3>Regarding DPI</h3>
/// The window will not be DPI aware unless an application manifest is applied to the application, stating that the application is DPI aware. 
/// </para>
/// </remarks>
public sealed class GorgonWindow
    : IGorgonNamedObject, IDisposable
{
    private readonly static Dictionary<nint, (GorgonWindow window, GorgonWindowProcedure proc)> _windowProcs = [];

    private nint _hwnd;
    private int _isDisposing;
    private HINSTANCE _instance;
    private bool _visible = true;
    private WindowState _state = WindowState.Normal;
    private string _caption = string.Empty;
    private nint _icon;
    private GorgonRectangle _bounds;
    private GorgonRectangle? _desiredBounds = null;
    private WindowBorder _border = WindowBorder.Resizable;
    private WindowDecorations _decorations = WindowDecorations.All;
    private bool _topMost;
    private GorgonPoint _clientSize;
    private GorgonWindowProcedure _wndProc;

    /// <summary>
    /// Property to return the window handle.
    /// </summary>
    public nint Handle => _hwnd;

    /// <summary>
    /// Property to return whether the window is visible or not.
    /// </summary>
    public bool IsVisible => _visible;

    /// <summary>
    /// Property to return the window caption.
    /// </summary>
    public string Caption => _caption;

    /// <summary>
    /// Property to return the unique name for this window.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the state of the window.
    /// </summary>
    /// <seealso cref="Minimize"/>
    /// <seealso cref="Maximize"/>
    /// <seealso cref="Restore"/>
    public WindowState State => _state;

    /// <summary>
    /// Property to return the bounds of the window.
    /// </summary>
    /// <seealso cref="SetBounds"/>
    public GorgonRectangle Bounds => _bounds;

    /// <summary>
    /// Property to return the full size of the window.
    /// </summary>
    /// <seealso cref="SetBounds"/>
    /// <seealso cref="Resize"/>
    public GorgonPoint Size => _bounds.Size;

    /// <summary>
    /// Property to return the location of the window on the desktop.
    /// </summary>
    /// <seealso cref="SetBounds"/>
    /// <seealso cref="MoveTo"/>
    public GorgonPoint Location => _bounds.Location;

    /// <summary>
    /// Property to return the style of border on the window.
    /// </summary>
    /// <seealso cref="SetBorder"/>
    public WindowBorder BorderStyle => _border;

    /// <summary>
    /// Property to return the window decorations for the window.
    /// </summary>
    /// <see cref="SetDecorations"/>
    public WindowDecorations Decorations => _decorations;

    /// <summary>
    /// Property to return whether the window is always at the top of the window Z-order for non-topmost windows.
    /// </summary>
    /// <seealso cref="SetTopMost"/>
    public bool IsTopMost => _topMost;

    /// <summary>
    /// Property to return whether this window contains keyboard focus.
    /// </summary>
    /// <seealso cref="BringToFront"/>
    public bool HasFocus => PInvoke.GetFocus() == _hwnd;

    /// <summary>
    /// Property to return the client area size for the window.
    /// </summary>
    /// <seealso cref="SetClientSize"/>
    public GorgonPoint ClientSize => _clientSize;

    /// <summary>
    /// Function to calculate the window styling based on object parameters.
    /// </summary>
    /// <param name="isVisible"><b>true</b> if the window is visible, <b>false</b> if not.</param>
    /// <param name="border">The border style for the window.</param>
    /// <param name="decorations">The decorations on the window.</param>
    /// <returns>The window style.</returns>
    private static WINDOW_STYLE CalculateStyle(bool isVisible, WindowBorder border, WindowDecorations decorations)
    {
        WINDOW_STYLE style = WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_CLIPSIBLINGS;

        switch (border)
        {
            case WindowBorder.Resizable:
                style = WINDOW_STYLE.WS_OVERLAPPED | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SIZEBOX | WINDOW_STYLE.WS_CLIPSIBLINGS;
                break;
            case WindowBorder.Static:
                style = WINDOW_STYLE.WS_OVERLAPPED | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_CLIPSIBLINGS;
                break;
            case WindowBorder.None:
                return style;
        }

        if (isVisible)
        {
            style |= WINDOW_STYLE.WS_VISIBLE;
        }

        if ((decorations & WindowDecorations.SystemMenu) == WindowDecorations.SystemMenu)
        {
            style |= WINDOW_STYLE.WS_SYSMENU;

            // These styles need a system menu.
            if ((decorations & WindowDecorations.MinimizeBox) == WindowDecorations.MinimizeBox)
            {
                style |= WINDOW_STYLE.WS_MINIMIZEBOX;
            }

            if (((decorations & WindowDecorations.Maximizebox) == WindowDecorations.Maximizebox) && (border == WindowBorder.Resizable))
            {
                style |= WINDOW_STYLE.WS_MAXIMIZEBOX;
            }
        }

        return style;
    }

    /// <summary>
    /// Function to create the actual window.
    /// </summary>
    private unsafe void CreateWindow()
    {
        if ((_hwnd != IntPtr.Zero) || (_isDisposing != 0))
        {
            return;
        }

        unsafe
        {
            _instance = PInvoke.GetModuleHandle(new PCWSTR());

            if (_instance.Value == null)
            {
                throw new Win32Exception(Resources.GOR_ERR_CANNOT_GET_INSTANCE);
            }

            fixed (char* classNamePtr = Name)
            {
                HICON icon = _icon == IntPtr.Zero ? PInvoke.LoadIcon(HINSTANCE.Null, PInvoke.IDI_APPLICATION) : new HICON(_icon);

                WNDCLASSEXW wndClass = new()
                {
                    cbSize = (uint)sizeof(WNDCLASSEXW),
                    hInstance = _instance,
                    lpszClassName = new PCWSTR(classNamePtr),
                    style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW,
                    hCursor = PInvoke.LoadCursor(HINSTANCE.Null, PInvoke.IDC_ARROW),
                    hbrBackground = new HBRUSH((nint)PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH)),
                    lpfnWndProc = &WndProc,
                    hIcon = icon,
                    hIconSm = icon
                };

                ushort atom = PInvoke.RegisterClassEx(in wndClass);

                if (atom == 0)
                {
                    int errCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(string.Format(Resources.GOR_ERR_CANNOT_REGISTER_WINDOW, errCode));
                }

                WINDOW_STYLE style = CalculateStyle(_visible, _border, _decorations);
                WINDOW_EX_STYLE exStyle = _topMost ? WINDOW_EX_STYLE.WS_EX_TOPMOST : 0;
                RECT rect = new(0, 0, _clientSize.X, _clientSize.Y);

                if (_desiredBounds is not null)
                {
                    _bounds = _desiredBounds.Value;
                }
                else if (PInvoke.AdjustWindowRectEx(ref rect, style & ~WINDOW_STYLE.WS_OVERLAPPED, false, _topMost ? WINDOW_EX_STYLE.WS_EX_TOPMOST : 0))
                {
                    _bounds = new GorgonRectangle(_bounds.X, _bounds.Y, rect.Width, rect.Height);
                }

                _desiredBounds = null;

                _hwnd = PInvoke.CreateWindowEx(exStyle, Name, _caption, style, _bounds.Left, _bounds.Top, _bounds.Width.Max(0), _bounds.Height.Max(0), HWND.Null, HMENU.Null, _instance, null);

                if (_hwnd == IntPtr.Zero)
                {
                    int errCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(string.Format(Resources.GOR_ERR_CANNOT_CREATE_WINDOW, Name, errCode));
                }

                _windowProcs[_hwnd] = (this, _wndProc);

                PInvoke.GetClientRect(new HWND(_hwnd), out RECT r);
                _clientSize = new GorgonPoint(r.Width, r.Height);

                if (_visible)
                {
                    PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_SHOWNORMAL);
                }

                switch (_state)
                {
                    case WindowState.Maximized:
                        SetState(WindowState.Maximized);
                        break;
                    case WindowState.Minimized:
                        SetState(WindowState.Minimized);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Function to handle window messages for our window.
    /// </summary>
    /// <param name="hwnd">The window handle.</param>
    /// <param name="msg">The message for the window.</param>
    /// <param name="wParam">Message parameter.</param>
    /// <param name="lParam">Message parameter.</param>
    /// <returns>The result of the call.</returns>
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private unsafe static LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        if (!_windowProcs.TryGetValue(hwnd, out (GorgonWindow Window, GorgonWindowProcedure WndProc) proc))
        {
            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        RECT windowRect = default;

        switch (msg)
        {
            // Handle minimize by keeping our previous size, but setting the state only.
            // This will keep apps from tripping up on weird client sizes/positions.
            case GorgonWindowMessages.WM_SIZE when wParam.Value == 1:
                proc.Window._state = WindowState.Minimized;
                break;
            case GorgonWindowMessages.WM_SIZE:
                // Handle minimized scenario.
                switch (wParam.Value)
                {
                    // Restore.
                    case 0:
                        proc.Window._state = WindowState.Normal;
                        break;
                    // Maximized.
                    case 2:
                        proc.Window._state = WindowState.Maximized;
                        break;
                }

                int width = (int)lParam.Value & 0xffff;
                int height = ((int)lParam.Value >> 16) & 0xffff;

                PInvoke.GetWindowRect(hwnd, &windowRect);

                proc.Window._clientSize = new GorgonPoint(width, height);
                proc.Window._bounds = new GorgonRectangle(windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);
                break;
            case GorgonWindowMessages.WM_MOVE:
                PInvoke.GetWindowRect(hwnd, &windowRect);
                proc.Window._bounds = new GorgonRectangle(windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);
                break;
        }

        GorgonWindowMessage message = new(proc.Window, (int)msg, wParam, lParam);
        GorgonWindowMessageResult result = proc.WndProc(in message);

        if (result.Handled)
        {
            return new LRESULT(result.Result);
        }

        return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Function to set the state for the window.
    /// </summary>
    /// <param name="state">The state of the window.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetState(WindowState state)
    {
        _state = state;

        if (_hwnd != IntPtr.Zero)
        {
            PInvoke.ShowWindow(new HWND(_hwnd), (SHOW_WINDOW_CMD)state);
        }
    }

    /// <summary>
    /// Function to update the window's geometry.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateWindow()
    {
        if (_hwnd == IntPtr.Zero)
        {
            return;
        }

        HWND hwnd = new(_hwnd);
        PInvoke.SetWindowPos(hwnd, HWND.HWND_TOP, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
        PInvoke.GetClientRect(hwnd, out RECT r);
        _clientSize = new GorgonPoint(r.Width, r.Height);
    }

    /// <summary>
    /// Function called when the frame of the window needs updating.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateFrame() => PInvoke.SetWindowPos(new HWND(_hwnd), HWND.HWND_TOP, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);

    /// <summary>
    /// Function to calculate the actual size, pixels, of the window from its expected client area.
    /// </summary>
    /// <param name="clientSize">The desired size, in pixels, of the window client area.</param>
    /// <param name="border">The type of border on the window.</param>
    /// <param name="decorations">The decorations on the window.</param>
    /// <returns>The size of the full window.</returns>
    /// <remarks>
    /// <para>
    /// Use this to determine the full size of the window based on a desired client size. These values can be different due to the size of the window border, the caption, etc... Applications can then call 
    /// the <see cref="Resize"/> method to size the window to the desired size.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint CalculateWindowSize(GorgonPoint clientSize, WindowBorder border, WindowDecorations decorations)
    {
        RECT r = new(0, 0, clientSize.X, clientSize.Y);

        PInvoke.AdjustWindowRectEx(ref r, CalculateStyle(true, border, decorations) & ~WINDOW_STYLE.WS_OVERLAPPED, false, 0);

        return new(r.X, r.Y);
    }

    /// <summary>
    /// Function to retrieve a monitor handle that has the largest area of intersection with the specified window handle.
    /// </summary>
    /// <param name="windowHandle">The handle to the window.</param>
    /// <returns>The handle for the monitor.</returns>
    /// <remarks>
    /// <para>
    /// If the window does not intersect any monitor (e.g. it is off screen), then the primary monitor handle is returned.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint MonitorHandleFromWindow(nint windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        HWND hwnd = new(windowHandle);
        return PInvoke.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
    }

    /// <summary>
    /// Function to retrieve a monitor handle that has the largest area of intersection with the specified window.
    /// </summary>
    /// <returns>The handle for the monitor.</returns>
    /// <remarks>
    /// <para>
    /// If the window does not intersect any monitor (e.g. it is off screen), then the primary monitor handle is returned.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint MonitorHandleFromWindow()
    {
        CreateWindow();

        if (_hwnd == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        return MonitorHandleFromWindow(Handle);
    }

    /// <summary>
    /// Function to set the size of the client area for the window.
    /// </summary>
    /// <param name="newSize">The new size for the window.</param>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// This is the client size of the window, not the <see cref="Size"/>. To set the full window size, call the <see cref="Resize"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="Resize"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SetClientSize(GorgonPoint newSize)
    {
        _clientSize = newSize;
        _desiredBounds = null;

        RECT rect = new(0, 0, newSize.X, newSize.Y);
        if (PInvoke.AdjustWindowRectEx(ref rect, CalculateStyle(_visible, _border, _decorations) & ~WINDOW_STYLE.WS_OVERLAPPED, false, _topMost ? WINDOW_EX_STYLE.WS_EX_TOPMOST : 0))
        {
            _bounds = new GorgonRectangle(_bounds.X, _bounds.Y, rect.Width, rect.Height);
        }

        UpdateWindow();

        return this;
    }

    /// <summary>
    /// Function to set the border style on the window.
    /// </summary>
    /// <param name="border">The style of border to use.</param>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// If the border style is set to <see cref="WindowBorder.None"/>, then no window decorations will be applied.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SetBorder(WindowBorder border)
    {
        _border = border;

        if (_hwnd != IntPtr.Zero)
        {
            WINDOW_STYLE style = CalculateStyle(_visible, _border, _decorations);
            User32Api.SetWindowAttribute(_hwnd, GwlIndex.GwlStyle, (nint)style);
            UpdateFrame();
        }

        return this;
    }

    /// <summary>
    /// Function to change the window decorations (System Menu, Maximize, and Minimize buttons).
    /// </summary>
    /// <param name="decorations">The decorations, OR'd together.</param>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// If the <see cref="BorderStyle"/> is set to <see cref="WindowBorder.None"/>, then no window decorations will be applied.
    /// </para>
    /// </remarks>
    /// <seealso cref="BorderStyle"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SetDecorations(WindowDecorations decorations)
    {
        _decorations = decorations;

        if (_hwnd != IntPtr.Zero)
        {
            WINDOW_STYLE style = CalculateStyle(_visible, _border, _decorations);
            User32Api.SetWindowAttribute(_hwnd, GwlIndex.GwlStyle, (nint)style);
            UpdateFrame();
        }

        return this;
    }

    /// <summary>
    /// Function to minimize the window.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow Minimize()
    {
        SetState(WindowState.Minimized);
        return this;
    }

    /// <summary>
    /// Function to maximize the window.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow Maximize()
    {
        SetState(WindowState.Maximized);
        return this;
    }

    /// <summary>
    /// Function to restore the window from a minimized or maximized state.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow Restore()
    {
        SetState(WindowState.Normal);
        return this;
    }

    /// <summary>
    /// Function to set the window dimensions and position all at once.
    /// </summary>
    /// <param name="bounds">The rectangle containing the bounds and position of the window.</param>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// This method changes the full size of the window, not the <see cref="ClientSize"/>. To set the client size, call the <see cref="SetClientSize"/> method.
    /// </para>
    /// </remarks>
    /// <see cref="SetClientSize"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SetBounds(GorgonRectangle bounds)
    {
        if (_hwnd == IntPtr.Zero)
        {
            _desiredBounds = _bounds = bounds;
            return this;
        }

        _bounds = bounds;
        _desiredBounds = null;
        UpdateWindow();

        return this;
    }

    /// <summary>
    /// Function to move the window to a new position on the desktop.
    /// </summary>
    /// <param name="position">The new position for the window.</param>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow MoveTo(GorgonPoint position)
    {
        _bounds = new GorgonRectangle(position.X, position.Y, _bounds.Width, _bounds.Height);

        if (_hwnd == IntPtr.Zero)
        {
            if (_desiredBounds is not null)
            {
                _desiredBounds = new GorgonRectangle(position.X, position.Y, _desiredBounds.Value.Width, _desiredBounds.Value.Height);
            }

            return this;
        }

        UpdateWindow();

        return this;
    }

    /// <summary>
    /// Function to move the window to a new position on the desktop.
    /// </summary>
    /// <param name="size">The new size for the window.</param>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// This is the full size of the window, not the <see cref="ClientSize"/>. To set the client size, call the <see cref="SetClientSize"/> method.
    /// </para>
    /// </remarks>
    /// <see cref="SetClientSize"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow Resize(GorgonPoint size) => SetBounds(new GorgonRectangle(_bounds.X, _bounds.Y, size.X, size.Y));

    /// <summary>
    /// Function to set the caption text for the window.
    /// </summary>
    /// <param name="caption">The caption to apply to the window.</param>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SetCaption(string caption)
    {
        _caption ??= string.Empty;

        if (_hwnd != IntPtr.Zero)
        {
            PInvoke.SetWindowText(new HWND(_hwnd), caption);
        }

        return this;
    }

    /// <summary>
    /// Function to show the window.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// This method is also responsible for creating the window handle and should be called as soon as possible after object creation.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow Show()
    {
        CreateWindow();

        if (_hwnd == IntPtr.Zero)
        {
            _visible = true;
            return this;
        }

        PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_SHOW);

        _visible = true;

        return this;
    }

    /// <summary>
    /// Function to hide the window.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow Hide()
    {
        if (_hwnd != IntPtr.Zero)
        {
            PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_HIDE);
        }

        _visible = false;

        return this;
    }

    /// <summary>
    /// Function to set whether the window is always at the top of the window Z-order for non-top most windows.
    /// </summary>
    /// <param name="topMost"><b>true</b> to set the window as top most, <b>false</b> if not.</param>
    /// <returns>This window as a fluent interface.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SetTopMost(bool topMost)
    {
        _topMost = topMost;

        if (_hwnd != IntPtr.Zero)
        {
            PInvoke.SetWindowPos(new HWND(_hwnd), _topMost ? HWND.HWND_TOPMOST : HWND.HWND_NOTOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
        }

        return this;
    }

    /// <summary>
    /// Function to assign a new icon to the window.
    /// </summary>
    /// <param name="icon">The handle to the icon to assign.</param>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// This method takes a Win32 icon handle (HICON) and assigns it to the system menu and window icon. 
    /// </para>
    /// <para>
    /// The <paramref name="icon"/>, if it is not <see cref="IntPtr.Zero"/>, is copied to the window, leaving ownership of the icon to the caller. This means that the icon passed to this method can be 
    /// destroyed without affecting the icon on this window.
    /// </para>
    /// <para>
    /// If the <paramref name="icon"/> is set to <see cref="IntPtr.Zero"/>, then an internal windows icon is applied to the window.
    /// </para>
    /// </remarks>
    public GorgonWindow SetIcon(nint icon)
    {
        if (icon == _icon)
        {
            return this;
        }

        HICON newIcon = icon == IntPtr.Zero ? PInvoke.LoadIcon(HINSTANCE.Null, PInvoke.IDI_APPLICATION) : PInvoke.CopyIcon(new HICON(icon));

        if (_hwnd != IntPtr.Zero)
        {
            HWND hwnd = new(_hwnd);
            PInvoke.SendMessage(hwnd, PInvoke.WM_SETICON, new WPARAM(PInvoke.ICON_BIG), new LPARAM(newIcon));
            PInvoke.SendMessage(hwnd, PInvoke.WM_SETICON, new WPARAM(PInvoke.ICON_SMALL), new LPARAM(newIcon));
        }

        if (_icon != IntPtr.Zero)
        {
            PInvoke.DestroyIcon(new HICON(_icon));
        }

        _icon = newIcon;

        return this;
    }

    /// <summary>
    /// Function to send the window to the bottom of the z-order.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// To move the window to the top of the Z order stack, call the <see cref="BringToFront"/> method.
    /// </para>
    /// <para>
    /// If the <see cref="IsTopMost"/> is <b>true</b>, then this method does nothing.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow SendToBack()
    {
        if ((_hwnd == IntPtr.Zero) || (_topMost))
        {
            return this;
        }

        PInvoke.SetWindowPos(new HWND(_hwnd), HWND.HWND_BOTTOM, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);

        return this;
    }

    /// <summary>
    /// Function to bring the window to the top of the z-order.
    /// </summary>
    /// <returns>This window as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// To move the window to the bottom of the Z order stack, call the <see cref="SendToBack"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="SendToBack"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonWindow BringToFront()
    {
        if (_hwnd == IntPtr.Zero)
        {
            return this;
        }

        HWND hwnd = new(_hwnd);

        PInvoke.SetWindowPos(hwnd, HWND.HWND_TOP, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
        PInvoke.SetFocus(hwnd);

        return this;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposing, 1) != 0)
        {
            return;
        }

        Interlocked.Exchange(ref _wndProc!, null);
        nint hwnd = Interlocked.Exchange(ref _hwnd, IntPtr.Zero);
        nint hicon = Interlocked.Exchange(ref _icon, IntPtr.Zero);

        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        _windowProcs.Remove(hwnd);

        if (hicon != IntPtr.Zero)
        {
            PInvoke.DestroyIcon(new HICON(hicon));
        }

        if (hwnd != IntPtr.Zero)
        {
            PInvoke.DestroyWindow(new HWND(_hwnd));
        }

        if (!string.IsNullOrWhiteSpace(Name))
        {
            PInvoke.UnregisterClass(Name, _instance);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonWindow"/> class.
    /// </summary>
    /// <param name="name">The identifier name of the window.</param>
    /// <param name="caption">The caption for the window.</param>
    /// <param name="position">The position of the window on the desktop.</param>
    /// <param name="clientSize">The client size for the window.</param>
    /// <param name="wndProc">The window procedure used to handle window messages</param>
    /// <param name="icon">[Optional] The handle for the icon to assign to the window.</param>
    /// <remarks>
    /// <para>
    /// This will create a new window object with the specified objects. However, it will not create the actual window until the <see cref="Show"/> method is called. 
    /// </para>
    /// <para>
    /// The <paramref name="clientSize"/> value is the size of the <b>client area</b> of the window. Which is not the same as the entire window size. Use the total <see cref="CalculateWindowSize"/> method 
    /// static method for this class.
    /// </para>
    /// <para>
    /// The <paramref name="wndProc"/> method is a custom method that users provide in order to capture window messages and handle them as they see fit. It should conform to the signature provided by the 
    /// <see cref="GorgonWindowProcedure"/> delegate.
    /// </para>
    /// <para>
    /// The <paramref name="icon"/> is a handle to an icon (<c>HICON</c> in Win32). And if the user provides a custom icon handle, then that icon is copied so that ownership of the icon handle remains with 
    /// the caller. This means the icon passed in is not shared with the window and can be destroyed without issue. If this value is not supplied, then the default windows application icon is used.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonWindowProcedure"/>
    public GorgonWindow(string name, string caption, GorgonPoint position, GorgonPoint clientSize, GorgonWindowProcedure wndProc, nint? icon = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(name);

        _caption = caption ?? string.Empty;
        _clientSize = clientSize;
        _wndProc = wndProc;
        Name = name;

        RECT adjustedBounds = new(0, 0, clientSize.X, clientSize.Y);
        PInvoke.AdjustWindowRectEx(ref adjustedBounds, CalculateStyle(_visible, _border, _decorations) & ~WINDOW_STYLE.WS_OVERLAPPED, false, 0);
        _bounds = new GorgonRectangle(position.X, position.Y, adjustedBounds.Width, adjustedBounds.Height);

        if (icon is null)
        {
            _icon = IntPtr.Zero;
            return;
        }

        // Duplicate the icon, so we get ownership.
        _icon = PInvoke.CopyIcon(new HICON(icon.Value));
    }
}
