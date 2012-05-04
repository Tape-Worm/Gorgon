using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KRBTabControl.Win32
{
    internal partial class User32
    {
        #region Struct

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public override string ToString()
            {
                return "{left=" + left.ToString() + ", " + "top=" + top.ToString() + ", " +
                    "right=" + right.ToString() + ", " + "bottom=" + bottom.ToString() + "}";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        #region Tab Control Notification Structures

        //[StructLayout(LayoutKind.Sequential)]
        //internal struct NMHDR
        //{
        //    public IntPtr HWND;
        //    public uint idFrom;
        //    public int code;
            
        //    public override string ToString()
        //    {
        //        return String.Format("Hwnd: {0}, ControlID: {1}, Code: {2}", HWND, idFrom, code);
        //    }
        //}

        ///// <summary>
        ///// From MSDN, Contains information about a key press in a tab control. It is used with the TCN_KEYDOWN notification message. This structure supersedes the TC_KEYDOWN structure.
        ///// </summary>
        //[StructLayout(LayoutKind.Sequential)]
        //internal struct NMTCKEYDOWN
        //{
        //    /// <summary>
        //    /// NMHDR structure that contains information about the notification message. 
        //    /// </summary>
        //    public NMHDR hdr;
            
        //    /// <summary>
        //    /// Virtual key code.
        //    /// </summary>
        //    public VirtualKeys wVKey;
            
        //    /// <summary>
        //    /// Value that is identical to the lParam parameter of the WM_KEYDOWN message.
        //    /// </summary>
        //    public uint flags;

        //    public override string ToString()
        //    {
        //        return String.Format("NMHDR: [{0}], Key codes: [{1}], the lParam parameter of the WM_KEYDOWN: [{2}]", hdr, wVKey, flags);
        //    }
        //}

        #endregion

        [StructLayout(LayoutKind.Sequential)]
        internal struct TCHITTESTINFO
        {
            public Point pt;
            public TabControlHitTest flags;

            public TCHITTESTINFO(TabControlHitTest hitTest)
                : this()
            {
                flags = hitTest;
            }

            public TCHITTESTINFO(Point point, TabControlHitTest hitTest)
                : this(hitTest)
            {
                pt = point;
            }

            public TCHITTESTINFO(int x, int y, TabControlHitTest hitTest)
                : this(hitTest)
            {
                pt = new Point(x, y);
            }
        }
        
        #endregion

        #region UnmanagedMethods

        [DllImport("gdi32")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        
        [DllImport("gdi32", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32")]
        internal static extern IntPtr CreateDC(
          String driverName,
          String deviceName,
          String output,
          IntPtr lpInitData);

        [DllImport("gdi32")]
        internal static extern bool DeleteDC(
          IntPtr dc);

        [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern Int32 SetWindowTheme(
            IntPtr hWnd,
            String textSubAppName,
            String textSubIdList);
        
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwndParent);

        /// <summary>
        /// The GetParent function retrieves a handle to the specified window's parent or owner.
        /// </summary>
        /// <param name="hwnd">Handle to the window whose parent window handle is to be retrieved.</param>
        /// <returns>If the window is a child window, the return value is a handle to the parent window. If the window is a top-level window, the return value is a handle to the owner window. If the window is a top-level unowned window or if the function fails, the return value is NULL.</returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(
            IntPtr hwnd,
            int wMsg,
            int wParam,
            int lParam);
        
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(
            IntPtr hwnd,
            int wMsg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(
            IntPtr hwnd,
            int tMsg,
            IntPtr wParam,
            ref TCHITTESTINFO lParam);
        
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern IntPtr LoadCursorFromFile(string filename);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern bool IsWindowVisible(IntPtr hwnd);

        /// <summary>
        /// The FindWindowEx function retrieves a handle to a window whose class name and window name match the specified strings. The function searches child windows, beginning with the one following the specified child window.
        /// </summary>
        /// <param name="hwndParent">Handle to the parent window whose child windows are to be searched.</param>
        /// <param name="hwndChildAfter">Handle to a child window.</param>
        /// <param name="lpszClass">Specifies class name.</param>
        /// <param name="lpszWindow">Pointer to a null-terminated string that specifies the window name (the window's title).</param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class and window names.If the function fails, the return value is NULL.</returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            [MarshalAs(UnmanagedType.LPTStr)]
			string lpszClass,
            [MarshalAs(UnmanagedType.LPTStr)]
			string lpszWindow);

        /// <summary>
        /// The InvalidateRect function adds a rectangle to the specified window's update region.
        /// </summary>
        /// <param name="hwnd">Handle to window.</param>
        /// <param name="rect">Rectangle coordinates.</param>
        /// <param name="bErase">Erase state.</param>
        /// <returns>If the function succeeds, the return value is true.If the function fails, the return value is false.</returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern bool InvalidateRect(
            IntPtr hwnd,
            ref Rectangle rect,
            bool bErase);

        /// <summary>
        /// The ValidateRect function validates the client area within a rectangle by removing the rectangle from the update region of the specified window.
        /// </summary>
        /// <param name="hwnd">Handle to window.</param>
        /// <param name="rect">Validation rectangle coordinates.</param>
        /// <returns>If the function succeeds, the return value is true.If the function fails, the return value is false.</returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern bool ValidateRect(
            IntPtr hwnd,
            ref Rectangle rect);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int GetWindowLong(
            IntPtr hWnd,
            int dwStyle);
        
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetDC(
            IntPtr hwnd);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int ReleaseDC(
            IntPtr hwnd,
            IntPtr hdc);
        
        //[DllImport("user32", SetLastError = false)]
        //internal static extern IntPtr GetDesktopWindow();

        //[DllImport("user32", CharSet = CharSet.Auto)]
        //internal static extern int GetScrollPos(
        //    IntPtr hwnd,
        //    int nBar);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int GetClientRect(
            IntPtr hwnd,
            ref RECT rc);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int GetClientRect(
            IntPtr hwnd,
            [In, Out] ref Rectangle rect);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern bool GetWindowRect(
            IntPtr hWnd,
            [In, Out] ref Rectangle rect);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int GetWindowRect(
            IntPtr hwnd,
            ref RECT rc);

        [DllImport("user32", ExactSpelling = true, SetLastError = true)]
        internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern uint SetWindowLong(
            IntPtr hWnd,
            int nIndex,
            int dwNewLong);

        /// <summary>
        /// Changes the size, position, and Z order of a child, pop-up, or top-level window.
        /// These windows are ordered according to their appearance on the screen.
        /// The topmost window receives the highest rank and is the first window in the Z order.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="hWndAfter">A handle to the window to precede the positioned window in the Z order. This parameter must be a window handle or one of the following values.</param>
        /*  
            HWND_BOTTOM : 1
            Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows. 
            HWND_NOTOPMOST : -2
            Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
            HWND_TOP : 0
            Places the window at the top of the Z order.
            HWND_TOPMOST : -1
            Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated. */
        /// <param name="X">Specifies the new position of the left side of the window, in client coordinates.</param>
        /// <param name="Y">Specifies the new position of the top of the window, in client coordinates.</param>
        /// <param name="Width">Specifies the new width of the window, in pixels.</param>
        /// <param name="Height">Specifies the new height of the window, in pixels.</param>
        /// <param name="flags">Specifies the window sizing and positioning flags. This parameter can be a combination of the following values.</param>
        /// <returns>If the function succeeds, the return value is nonzero, if the function fails, the return value is zero.</returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern int SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndAfter,
            int X,
            int Y,
            int Width,
            int Height,
            FlagsSetWindowPos flags);

        [DllImport("user32", CharSet = CharSet.Auto)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);

        #endregion
    }
}