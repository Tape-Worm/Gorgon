using System;

namespace KRBTabControl.Win32
{
    partial class User32
    {
        #region Symbolic Constants

        internal const byte _AC_SRC_OVER = 0x00;
        internal const byte _AC_SRC_ALPHA = 0x01;
        internal const int _LWA_ALPHA = 0x00000002;
        internal const int _PAT_INVERT = 0x5A0049;
        internal const int _HT_CAPTION = 0x2;
        internal const int _HT_CLIENT = 1;
        internal const int _HT_TRANSPARENT = -1;
        internal const int _TCM_HITTEST = 0x130D;
        //internal const int _SRC_COPY = 0xCC0020;
        //internal const int _CS_DROPSHADOW = 0x20000;
        //internal const int _TCM_ADJUSTRECT = 0x1328;
        //internal const int _TCN_FIRST = -550;
        //internal const int _TCN_SELCHANGE = -551;
        //internal const int _TCN_SELCHANGING = -552;

        #endregion
        
        [Flags]
        internal enum TabControlHitTest
        {
            /// <summary>
            /// The position is not over a tab.
            /// </summary>
            TCHT_NOWHERE = 1,

            /// <summary>
            /// The position is over a tab's icon.
            /// </summary>
            TCHT_ONITEMICON = 2,

            /// <summary>
            /// The position is over a tab's text.
            /// </summary>
            TCHT_ONITEMLABEL = 4,

            /// <summary>
            /// The position is over a tab but not over its icon or its text. For owner-drawn tab controls, this value is specified if the position is anywhere over a tab.
            /// TCHT_ONITEM is a bitwise-OR operation on TCHT_ONITEMICON and TCHT_ONITEMLABEL.
            /// </summary>
            TCHT_ONITEM = TCHT_ONITEMICON | TCHT_ONITEMLABEL
        };

        /* Virtual Keys Enum for Tab Control Notifications[TCN_FIRST, TCN_SELCHANGE, TCN_SELCHANGING]
        
        /// <summary>
        /// Enumeration for virtual keys.
        /// </summary>
        public enum VirtualKeys
            : ushort
        {
            /// <summary>
            /// TAB key
            /// </summary>
            Tab = 0x09,

            /// <summary>
            /// ENTER key
            /// </summary>
            Return = 0x0D,
            
            /// <summary>
            /// ESCAPE key
            /// </summary>
            Escape = 0x1B,
            
            /// <summary>
            /// SHIFT key
            /// </summary>
            Shift = 0x10,

            /// <summary>
            /// CTRL key
            /// </summary>
            Control = 0x11,

            /// <summary>
            /// END key
            /// </summary>
            End = 0x23,

            /// <summary>
            /// HOME key
            /// </summary>
            Home = 0x24,

            /// <summary>
            /// LEFT ARROW key
            /// </summary>
            Left = 0x25,

            /// <summary>
            /// UP ARROW key
            /// </summary>
            Up = 0x26,

            /// <summary>
            /// RIGHT ARROW key
            /// </summary>
            Right = 0x27,

            /// <summary>
            /// DOWN ARROW key
            /// </summary>
            Down = 0x28,

            /// <summary>
            /// INS key
            /// </summary>
            Insert = 0x2D,

            /// <summary>
            /// DEL key
            /// </summary>
            Delete = 0x2E,

            /// <summary>
            /// F1 key
            /// </summary>
            F1 = 0x70
        };

        */

        /// <summary>
        /// Specifies values from SetWindowPosZ enumeration.
        /// </summary>
        internal enum SetWindowPosZ
        {
            /// <summary>
            /// Specified HWND_TOP enumeration value.
            /// </summary>
            HWND_TOP = 0,

            /// <summary>
            /// Specified HWND_BOTTOM enumeration value.
            /// </summary>
            HWND_BOTTOM = 1,

            /// <summary>
            /// Specified HWND_TOPMOST enumeration value.
            /// </summary>
            HWND_TOPMOST = -1,

            /// <summary>
            /// Specified HWND_NOTOPMOST enumeration value.
            /// </summary>
            HWND_NOTOPMOST = -2
        };

        //[CLSCompliant(false)]
        [Flags]
        internal enum FlagsSetWindowPos : uint
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
            SWP_NOREDRAW = 0x0008,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020,
            SWP_SHOWWINDOW = 0x0040,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_DRAWFRAME = 0x0020,
            SWP_NOREPOSITION = 0x0200,
            SWP_DEFERERASE = 0x2000,
            SWP_ASYNCWINDOWPOS = 0x4000
        };
        
        [Flags]
        internal enum RedrawWindowFlags : uint
        {
            /// <summary>
            /// Invalidates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_INVALIDATE invalidates the entire window.
            /// </summary>
            Invalidate = 0x1,

            /// <summary>Causes the OS to post a WM_PAINT message to the window regardless of whether a portion of the window is invalid.</summary>
            InternalPaint = 0x2,

            /// <summary>
            /// Causes the window to receive a WM_ERASEBKGND message when the window is repainted.
            /// Specify this value in combination with the RDW_INVALIDATE value; otherwise, RDW_ERASE has no effect.
            /// </summary>
            Erase = 0x4,

            /// <summary>
            /// Validates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_VALIDATE validates the entire window.
            /// This value does not affect internal WM_PAINT messages.
            /// </summary>
            Validate = 0x8,

            NoInternalPaint = 0x10,

            /// <summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
            NoErase = 0x20,

            /// <summary>Excludes child windows, if any, from the repainting operation.</summary>
            NoChildren = 0x40,

            /// <summary>Includes child windows, if any, in the repainting operation.</summary>
            AllChildren = 0x80,

            /// <summary>Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND and WM_PAINT messages before the RedrawWindow returns, if necessary.</summary>
            UpdateNow = 0x100,

            /// <summary>
            /// Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND messages before RedrawWindow returns, if necessary.
            /// The affected windows receive WM_PAINT messages at the ordinary time.
            /// </summary>
            EraseNow = 0x200,

            Frame = 0x400,

            NoFrame = 0x800
        };

        internal enum ShowWindowStyles
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        };

        #region Windows Messages
        
        /// <summary>
        /// Specifies values from Msgs enumeration.
        /// </summary>
        internal enum Msgs
        {
            /// <summary>
            /// Specified WM_NULL enumeration value.
            /// </summary>
            WM_NULL = 0x0000,

            /// <summary>
            /// Specified WM_CREATE enumeration value.
            /// </summary>
            WM_CREATE = 0x0001,

            /// <summary>
            /// Specified WM_DESTROY enumeration value.
            /// </summary>
            WM_DESTROY = 0x0002,

            /// <summary>
            /// Specified WM_MOVE enumeration value.
            /// </summary>
            WM_MOVE = 0x0003,

            /// <summary>
            /// Specified WM_SIZE enumeration value.
            /// </summary>
            WM_SIZE = 0x0005,

            /// <summary>
            /// Specified WM_ACTIVATE enumeration value.
            /// </summary>
            WM_ACTIVATE = 0x0006,

            /// <summary>
            /// Specified WM_SETFOCUS enumeration value.
            /// </summary>
            WM_SETFOCUS = 0x0007,

            /// <summary>
            /// Specified WM_KILLFOCUS enumeration value.
            /// </summary>
            WM_KILLFOCUS = 0x0008,

            /// <summary>
            /// Specified WM_ENABLE enumeration value.
            /// </summary>
            WM_ENABLE = 0x000A,

            /// <summary>
            /// Specified WM_SETREDRAW enumeration value.
            /// </summary>
            WM_SETREDRAW = 0x000B,

            /// <summary>
            /// Specified WM_SETTEXT enumeration value.
            /// </summary>
            WM_SETTEXT = 0x000C,

            /// <summary>
            /// Specified WM_GETTEXT enumeration value.
            /// </summary>
            WM_GETTEXT = 0x000D,

            /// <summary>
            /// Specified WM_GETTEXTLENGTH enumeration value.
            /// </summary>
            WM_GETTEXTLENGTH = 0x000E,

            /// <summary>
            /// Specified WM_PAINT enumeration value.
            /// </summary>
            WM_PAINT = 0x000F,

            /// <summary>
            /// Specified WM_CLOSE enumeration value.
            /// </summary>
            WM_CLOSE = 0x0010,

            /// <summary>
            /// Specified WM_QUERYENDSESSION enumeration value.
            /// </summary>
            WM_QUERYENDSESSION = 0x0011,

            /// <summary>
            /// Specified WM_QUIT enumeration value.
            /// </summary>
            WM_QUIT = 0x0012,

            /// <summary>
            /// Specified WM_QUERYOPEN enumeration value.
            /// </summary>
            WM_QUERYOPEN = 0x0013,

            /// <summary>
            /// Specified WM_ERASEBKGND enumeration value.
            /// </summary>
            WM_ERASEBKGND = 0x0014,

            /// <summary>
            /// Specified WM_SYSCOLORCHANGE enumeration value.
            /// </summary>
            WM_SYSCOLORCHANGE = 0x0015,

            /// <summary>
            /// Specified WM_ENDSESSION enumeration value.
            /// </summary>
            WM_ENDSESSION = 0x0016,

            /// <summary>
            /// Specified WM_SHOWWINDOW enumeration value.
            /// </summary>
            WM_SHOWWINDOW = 0x0018,

            /// <summary>
            /// Specified WM_WININICHANGE enumeration value.
            /// </summary>
            WM_WININICHANGE = 0x001A,

            /// <summary>
            /// Specified WM_SETTINGCHANGE enumeration value.
            /// </summary>
            WM_SETTINGCHANGE = 0x001A,

            /// <summary>
            /// Specified WM_DEVMODECHANGE enumeration value.
            /// </summary>
            WM_DEVMODECHANGE = 0x001B,

            /// <summary>
            /// Specified WM_ACTIVATEAPP enumeration value.
            /// </summary>
            WM_ACTIVATEAPP = 0x001C,

            /// <summary>
            /// Specified WM_FONTCHANGE enumeration value.
            /// </summary>
            WM_FONTCHANGE = 0x001D,

            /// <summary>
            /// Specified WM_TIMECHANGE enumeration value.
            /// </summary>
            WM_TIMECHANGE = 0x001E,

            /// <summary>
            /// Specified WM_CANCELMODE enumeration value.
            /// </summary>
            WM_CANCELMODE = 0x001F,

            /// <summary>
            /// Specified WM_SETCURSOR enumeration value.
            /// </summary>
            WM_SETCURSOR = 0x0020,

            /// <summary>
            /// Specified WM_MOUSEACTIVATE enumeration value.
            /// </summary>
            WM_MOUSEACTIVATE = 0x0021,

            /// <summary>
            /// Specified WM_CHILDACTIVATE enumeration value.
            /// </summary>
            WM_CHILDACTIVATE = 0x0022,

            /// <summary>
            /// Specified WM_QUEUESYNC enumeration value.
            /// </summary>
            WM_QUEUESYNC = 0x0023,

            /// <summary>
            /// Specified WM_GETMINMAXINFO enumeration value.
            /// </summary>
            WM_GETMINMAXINFO = 0x0024,

            /// <summary>
            /// Specified WM_PAINTICON enumeration value.
            /// </summary>
            WM_PAINTICON = 0x0026,

            /// <summary>
            /// Specified WM_ICONERASEBKGND enumeration value.
            /// </summary>
            WM_ICONERASEBKGND = 0x0027,

            /// <summary>
            /// Specified WM_NEXTDLGCTL enumeration value.
            /// </summary>
            WM_NEXTDLGCTL = 0x0028,

            /// <summary>
            /// Specified WM_SPOOLERSTATUS enumeration value.
            /// </summary>
            WM_SPOOLERSTATUS = 0x002A,

            /// <summary>
            /// Specified WM_DRAWITEM enumeration value.
            /// </summary>
            WM_DRAWITEM = 0x002B,

            /// <summary>
            /// Specified WM_MEASUREITEM enumeration value.
            /// </summary>
            WM_MEASUREITEM = 0x002C,

            /// <summary>
            /// Specified WM_DELETEITEM enumeration value.
            /// </summary>
            WM_DELETEITEM = 0x002D,

            /// <summary>
            /// Specified WM_VKEYTOITEM enumeration value.
            /// </summary>
            WM_VKEYTOITEM = 0x002E,

            /// <summary>
            /// Specified WM_CHARTOITEM enumeration value.
            /// </summary>
            WM_CHARTOITEM = 0x002F,

            /// <summary>
            /// Specified WM_SETFONT enumeration value.
            /// </summary>
            WM_SETFONT = 0x0030,

            /// <summary>
            /// Specified WM_GETFONT enumeration value.
            /// </summary>
            WM_GETFONT = 0x0031,

            /// <summary>
            /// Specified WM_SETHOTKEY enumeration value.
            /// </summary>
            WM_SETHOTKEY = 0x0032,

            /// <summary>
            /// Specified WM_GETHOTKEY enumeration value.
            /// </summary>
            WM_GETHOTKEY = 0x0033,

            /// <summary>
            /// Specified WM_QUERYDRAGICON enumeration value.
            /// </summary>
            WM_QUERYDRAGICON = 0x0037,

            /// <summary>
            /// Specified WM_COMPAREITEM enumeration value.
            /// </summary>
            WM_COMPAREITEM = 0x0039,

            /// <summary>
            /// Specified WM_GETOBJECT enumeration value.
            /// </summary>
            WM_GETOBJECT = 0x003D,

            /// <summary>
            /// Specified WM_COMPACTING enumeration value.
            /// </summary>
            WM_COMPACTING = 0x0041,

            /// <summary>
            /// Specified WM_COMMNOTIFY enumeration value.
            /// </summary>
            WM_COMMNOTIFY = 0x0044,

            /// <summary>
            /// Specified WM_WINDOWPOSCHANGING enumeration value.
            /// </summary>
            WM_WINDOWPOSCHANGING = 0x0046,

            /// <summary>
            /// Specified WM_WINDOWPOSCHANGED enumeration value.
            /// </summary>
            WM_WINDOWPOSCHANGED = 0x0047,

            /// <summary>
            /// Specified WM_POWER enumeration value.
            /// </summary>
            WM_POWER = 0x0048,

            /// <summary>
            /// Specified WM_COPYDATA enumeration value.
            /// </summary>
            WM_COPYDATA = 0x004A,

            /// <summary>
            /// Specified WM_CANCELJOURNAL enumeration value.
            /// </summary>
            WM_CANCELJOURNAL = 0x004B,

            /// <summary>
            /// Specified WM_NOTIFY enumeration value.
            /// </summary>
            WM_NOTIFY = 0x004E,

            /// <summary>
            /// Specified WM_INPUTLANGCHANGEREQUEST enumeration value.
            /// </summary>
            WM_INPUTLANGCHANGEREQUEST = 0x0050,

            /// <summary>
            /// Specified WM_INPUTLANGCHANGE enumeration value.
            /// </summary>
            WM_INPUTLANGCHANGE = 0x0051,

            /// <summary>
            /// Specified WM_TCARD enumeration value.
            /// </summary>
            WM_TCARD = 0x0052,

            /// <summary>
            /// Specified WM_HELP enumeration value.
            /// </summary>
            WM_HELP = 0x0053,

            /// <summary>
            /// Specified WM_USERCHANGED enumeration value.
            /// </summary>
            WM_USERCHANGED = 0x0054,

            /// <summary>
            /// Specified WM_NOTIFYFORMAT enumeration value.
            /// </summary>
            WM_NOTIFYFORMAT = 0x0055,

            /// <summary>
            /// Specified WM_CONTEXTMENU enumeration value.
            /// </summary>
            WM_CONTEXTMENU = 0x007B,

            /// <summary>
            /// Specified WM_STYLECHANGING enumeration value.
            /// </summary>
            WM_STYLECHANGING = 0x007C,

            /// <summary>
            /// Specified WM_STYLECHANGED enumeration value.
            /// </summary>
            WM_STYLECHANGED = 0x007D,

            /// <summary>
            /// Specified WM_DISPLAYCHANGE enumeration value.
            /// </summary>
            WM_DISPLAYCHANGE = 0x007E,

            /// <summary>
            /// Specified WM_GETICON enumeration value.
            /// </summary>
            WM_GETICON = 0x007F,

            /// <summary>
            /// Specified WM_SETICON enumeration value.
            /// </summary>
            WM_SETICON = 0x0080,

            /// <summary>
            /// Specified WM_NCCREATE enumeration value.
            /// </summary>
            WM_NCCREATE = 0x0081,

            /// <summary>
            /// Specified VK_RMENU enumeration value.
            /// </summary>
            WM_NCDESTROY = 0x0082,

            /// <summary>
            /// Specified WM_NCCALCSIZE enumeration value.
            /// </summary>
            WM_NCCALCSIZE = 0x0083,

            /// <summary>
            /// Specified WM_NCHITTEST enumeration value.
            /// </summary>
            WM_NCHITTEST = 0x0084,

            /// <summary>
            /// Specified WM_NCPAINT enumeration value.
            /// </summary>
            WM_NCPAINT = 0x0085,

            /// <summary>
            /// Specified WM_NCACTIVATE enumeration value.
            /// </summary>
            WM_NCACTIVATE = 0x0086,

            /// <summary>
            /// Specified WM_GETDLGCODE enumeration value.
            /// </summary>
            WM_GETDLGCODE = 0x0087,

            /// <summary>
            /// Specified WM_SYNCPAINT enumeration value.
            /// </summary>
            WM_SYNCPAINT = 0x0088,

            /// <summary>
            /// Specified WM_NCMOUSEMOVE enumeration value.
            /// </summary>
            WM_NCMOUSEMOVE = 0x00A0,

            /// <summary>
            /// Specified WM_NCLBUTTONDOWN enumeration value.
            /// </summary>
            WM_NCLBUTTONDOWN = 0x00A1,

            /// <summary>
            /// Specified WM_NCLBUTTONUP enumeration value.
            /// </summary>
            WM_NCLBUTTONUP = 0x00A2,

            /// <summary>
            /// Specified WM_NCLBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_NCLBUTTONDBLCLK = 0x00A3,

            /// <summary>
            /// Specified WM_NCRBUTTONDOWN enumeration value.
            /// </summary>
            WM_NCRBUTTONDOWN = 0x00A4,

            /// <summary>
            /// Specified WM_NCRBUTTONUP enumeration value.
            /// </summary>
            WM_NCRBUTTONUP = 0x00A5,

            /// <summary>
            /// Specified WM_NCRBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_NCRBUTTONDBLCLK = 0x00A6,

            /// <summary>
            /// Specified WM_NCMBUTTONDOWN enumeration value.
            /// </summary>
            WM_NCMBUTTONDOWN = 0x00A7,

            /// <summary>
            /// Specified WM_NCMBUTTONUP enumeration value.
            /// </summary>
            WM_NCMBUTTONUP = 0x00A8,

            /// <summary>
            /// Specified WM_NCMBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_NCMBUTTONDBLCLK = 0x00A9,

            /// <summary>
            /// Specified WM_NCXBUTTONDOWN enumeration value.
            /// </summary>
            WM_NCXBUTTONDOWN = 0x00AB,

            /// <summary>
            /// Specified WM_NCXBUTTONUP enumeration value.
            /// </summary>
            WM_NCXBUTTONUP = 0x00AC,

            /// <summary>
            /// Specified WM_KEYDOWN enumeration value.
            /// </summary>
            WM_KEYDOWN = 0x0100,

            /// <summary>
            /// Specified WM_KEYUP enumeration value.
            /// </summary>
            WM_KEYUP = 0x0101,

            /// <summary>
            /// Specified WM_CHAR enumeration value.
            /// </summary>
            WM_CHAR = 0x0102,

            /// <summary>
            /// Specified WM_DEADCHAR enumeration value.
            /// </summary>
            WM_DEADCHAR = 0x0103,

            /// <summary>
            /// Specified WM_SYSKEYDOWN enumeration value.
            /// </summary>
            WM_SYSKEYDOWN = 0x0104,

            /// <summary>
            /// Specified WM_SYSKEYUP enumeration value.
            /// </summary>
            WM_SYSKEYUP = 0x0105,

            /// <summary>
            /// Specified WM_SYSCHAR enumeration value.
            /// </summary>
            WM_SYSCHAR = 0x0106,

            /// <summary>
            /// Specified WM_SYSDEADCHAR enumeration value.
            /// </summary>
            WM_SYSDEADCHAR = 0x0107,

            /// <summary>
            /// Specified WM_KEYLAST enumeration value.
            /// </summary>
            WM_KEYLAST = 0x0108,

            /// <summary>
            /// Specified WM_IME_STARTCOMPOSITION enumeration value.
            /// </summary>
            WM_IME_STARTCOMPOSITION = 0x010D,

            /// <summary>
            /// Specified WM_IME_ENDCOMPOSITION enumeration value.
            /// </summary>
            WM_IME_ENDCOMPOSITION = 0x010E,

            /// <summary>
            /// Specified WM_IME_COMPOSITION enumeration value.
            /// </summary>
            WM_IME_COMPOSITION = 0x010F,

            /// <summary>
            /// Specified WM_IME_KEYLAST enumeration value.
            /// </summary>
            WM_IME_KEYLAST = 0x010F,

            /// <summary>
            /// Specified WM_INITDIALOG enumeration value.
            /// </summary>
            WM_INITDIALOG = 0x0110,

            /// <summary>
            /// Specified WM_COMMAND enumeration value.
            /// </summary>
            WM_COMMAND = 0x0111,

            /// <summary>
            /// Specified WM_SYSCOMMAND enumeration value.
            /// </summary>
            WM_SYSCOMMAND = 0x0112,

            /// <summary>
            /// Specified WM_TIMER enumeration value.
            /// </summary>
            WM_TIMER = 0x0113,

            /// <summary>
            /// Specified WM_HSCROLL enumeration value.
            /// </summary>
            WM_HSCROLL = 0x0114,

            /// <summary>
            /// Specified WM_VSCROLL enumeration value.
            /// </summary>
            WM_VSCROLL = 0x0115,

            /// <summary>
            /// Specified WM_INITMENU enumeration value.
            /// </summary>
            WM_INITMENU = 0x0116,

            /// <summary>
            /// Specified WM_INITMENUPOPUP enumeration value.
            /// </summary>
            WM_INITMENUPOPUP = 0x0117,

            /// <summary>
            /// Specified WM_MENUSELECT enumeration value.
            /// </summary>
            WM_MENUSELECT = 0x011F,

            /// <summary>
            /// Specified WM_MENUCHAR enumeration value.
            /// </summary>
            WM_MENUCHAR = 0x0120,

            /// <summary>
            /// Specified WM_ENTERIDLE enumeration value.
            /// </summary>
            WM_ENTERIDLE = 0x0121,

            /// <summary>
            /// Specified WM_MENURBUTTONUP enumeration value.
            /// </summary>
            WM_MENURBUTTONUP = 0x0122,

            /// <summary>
            /// Specified WM_MENUDRAG enumeration value.
            /// </summary>
            WM_MENUDRAG = 0x0123,

            /// <summary>
            /// Specified WM_MENUGETOBJECT enumeration value.
            /// </summary>
            WM_MENUGETOBJECT = 0x0124,

            /// <summary>
            /// Specified WM_UNINITMENUPOPUP enumeration value.
            /// </summary>
            WM_UNINITMENUPOPUP = 0x0125,

            /// <summary>
            /// Specified WM_MENUCOMMAND enumeration value.
            /// </summary>
            WM_MENUCOMMAND = 0x0126,

            /// <summary>
            /// Specified WM_CTLCOLORMSGBOX enumeration value.
            /// </summary>
            WM_CTLCOLORMSGBOX = 0x0132,

            /// <summary>
            /// Specified WM_CTLCOLOREDIT enumeration value.
            /// </summary>
            WM_CTLCOLOREDIT = 0x0133,

            /// <summary>
            /// Specified WM_CTLCOLORLISTBOX enumeration value.
            /// </summary>
            WM_CTLCOLORLISTBOX = 0x0134,

            /// <summary>
            /// Specified WM_CTLCOLORBTN enumeration value.
            /// </summary>
            WM_CTLCOLORBTN = 0x0135,

            /// <summary>
            /// Specified WM_CTLCOLORDLG enumeration value.
            /// </summary>
            WM_CTLCOLORDLG = 0x0136,

            /// <summary>
            /// Specified WM_CTLCOLORSCROLLBAR enumeration value.
            /// </summary>
            WM_CTLCOLORSCROLLBAR = 0x0137,

            /// <summary>
            /// Specified WM_CTLCOLORSTATIC enumeration value.
            /// </summary>
            WM_CTLCOLORSTATIC = 0x0138,

            /// <summary>
            /// Specified WM_MOUSEMOVE enumeration value.
            /// </summary>
            WM_MOUSEMOVE = 0x0200,

            /// <summary>
            /// Specified WM_LBUTTONDOWN enumeration value.
            /// </summary>
            WM_LBUTTONDOWN = 0x0201,

            /// <summary>
            /// Specified WM_LBUTTONUP enumeration value.
            /// </summary>
            WM_LBUTTONUP = 0x0202,

            /// <summary>
            /// Specified WM_LBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_LBUTTONDBLCLK = 0x0203,

            /// <summary>
            /// Specified WM_RBUTTONDOWN enumeration value.
            /// </summary>
            WM_RBUTTONDOWN = 0x0204,

            /// <summary>
            /// Specified WM_RBUTTONUP enumeration value.
            /// </summary>
            WM_RBUTTONUP = 0x0205,

            /// <summary>
            /// Specified WM_RBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_RBUTTONDBLCLK = 0x0206,

            /// <summary>
            /// Specified WM_MBUTTONDOWN enumeration value.
            /// </summary>
            WM_MBUTTONDOWN = 0x0207,

            /// <summary>
            /// Specified WM_MBUTTONUP enumeration value.
            /// </summary>
            WM_MBUTTONUP = 0x0208,

            /// <summary>
            /// Specified WM_MBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_MBUTTONDBLCLK = 0x0209,

            /// <summary>
            /// Specified WM_MOUSEWHEEL enumeration value.
            /// </summary>
            WM_MOUSEWHEEL = 0x020A,

            /// <summary>
            /// Specified WM_XBUTTONDOWN enumeration value.
            /// </summary>
            WM_XBUTTONDOWN = 0x020B,

            /// <summary>
            /// Specified WM_XBUTTONUP enumeration value.
            /// </summary>
            WM_XBUTTONUP = 0x020C,

            /// <summary>
            /// Specified WM_XBUTTONDBLCLK enumeration value.
            /// </summary>
            WM_XBUTTONDBLCLK = 0x020D,

            /// <summary>
            /// Specified WM_PARENTNOTIFY enumeration value.
            /// </summary>
            WM_PARENTNOTIFY = 0x0210,

            /// <summary>
            /// Specified WM_ENTERMENULOOP enumeration value.
            /// </summary>
            WM_ENTERMENULOOP = 0x0211,

            /// <summary>
            /// Specified WM_EXITMENULOOP enumeration value.
            /// </summary>
            WM_EXITMENULOOP = 0x0212,

            /// <summary>
            /// Specified WM_NEXTMENU enumeration value.
            /// </summary>
            WM_NEXTMENU = 0x0213,

            /// <summary>
            /// Specified WM_SIZING enumeration value.
            /// </summary>
            WM_SIZING = 0x0214,

            /// <summary>
            /// Specified WM_CAPTURECHANGED enumeration value.
            /// </summary>
            WM_CAPTURECHANGED = 0x0215,

            /// <summary>
            /// Specified WM_MOVING enumeration value.
            /// </summary>
            WM_MOVING = 0x0216,

            /// <summary>
            /// Specified WM_DEVICECHANGE enumeration value.
            /// </summary>
            WM_DEVICECHANGE = 0x0219,

            /// <summary>
            /// Specified WM_MDICREATE enumeration value.
            /// </summary>
            WM_MDICREATE = 0x0220,

            /// <summary>
            /// Specified WM_MDIDESTROY enumeration value.
            /// </summary>
            WM_MDIDESTROY = 0x0221,

            /// <summary>
            /// Specified WM_MDIACTIVATE enumeration value.
            /// </summary>
            WM_MDIACTIVATE = 0x0222,

            /// <summary>
            /// Specified WM_MDIRESTORE enumeration value.
            /// </summary>
            WM_MDIRESTORE = 0x0223,

            /// <summary>
            /// Specified WM_MDINEXT enumeration value.
            /// </summary>
            WM_MDINEXT = 0x0224,

            /// <summary>
            /// Specified WM_MDIMAXIMIZE enumeration value.
            /// </summary>
            WM_MDIMAXIMIZE = 0x0225,

            /// <summary>
            /// Specified WM_MDITILE enumeration value.
            /// </summary>
            WM_MDITILE = 0x0226,

            /// <summary>
            /// Specified WM_MDICASCADE enumeration value.
            /// </summary>
            WM_MDICASCADE = 0x0227,

            /// <summary>
            /// Specified WM_MDIICONARRANGE enumeration value.
            /// </summary>
            WM_MDIICONARRANGE = 0x0228,

            /// <summary>
            /// Specified WM_MDIGETACTIVE enumeration value.
            /// </summary>
            WM_MDIGETACTIVE = 0x0229,

            /// <summary>
            /// Specified WM_MDISETMENU enumeration value.
            /// </summary>
            WM_MDISETMENU = 0x0230,

            /// <summary>
            /// Specified WM_ENTERSIZEMOVE enumeration value.
            /// </summary>
            WM_ENTERSIZEMOVE = 0x0231,

            /// <summary>
            /// Specified WM_EXITSIZEMOVE enumeration value.
            /// </summary>
            WM_EXITSIZEMOVE = 0x0232,

            /// <summary>
            /// Specified WM_DROPFILES enumeration value.
            /// </summary>
            WM_DROPFILES = 0x0233,

            /// <summary>
            /// Specified WM_MDIREFRESHMENU enumeration value.
            /// </summary>
            WM_MDIREFRESHMENU = 0x0234,

            /// <summary>
            /// Specified WM_IME_SETCONTEXT enumeration value.
            /// </summary>
            WM_IME_SETCONTEXT = 0x0281,

            /// <summary>
            /// Specified WM_IME_NOTIFY enumeration value.
            /// </summary>
            WM_IME_NOTIFY = 0x0282,

            /// <summary>
            /// Specified WM_IME_CONTROL enumeration value.
            /// </summary>
            WM_IME_CONTROL = 0x0283,

            /// <summary>
            /// Specified WM_IME_COMPOSITIONFULL enumeration value.
            /// </summary>
            WM_IME_COMPOSITIONFULL = 0x0284,

            /// <summary>
            /// Specified WM_IME_SELECT enumeration value.
            /// </summary>
            WM_IME_SELECT = 0x0285,

            /// <summary>
            /// Specified WM_IME_CHAR enumeration value.
            /// </summary>
            WM_IME_CHAR = 0x0286,

            /// <summary>
            /// Specified WM_IME_REQUEST enumeration value.
            /// </summary>
            WM_IME_REQUEST = 0x0288,

            /// <summary>
            /// Specified WM_IME_KEYDOWN enumeration value.
            /// </summary>
            WM_IME_KEYDOWN = 0x0290,

            /// <summary>
            /// Specified WM_IME_KEYUP enumeration value.
            /// </summary>
            WM_IME_KEYUP = 0x0291,

            /// <summary>
            /// Specified WM_MOUSEHOVER enumeration value.
            /// </summary>
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,

            /// <summary>
            /// Specified WM_UNDO enumeration value.
            /// </summary>
            WM_UNDO = 0x0304,

            /// <summary>
            /// Specified WM_RENDERFORMAT enumeration value.
            /// </summary>
            WM_RENDERFORMAT = 0x0305,

            /// <summary>
            /// Specified WM_RENDERALLFORMATS enumeration value.
            /// </summary>
            WM_RENDERALLFORMATS = 0x0306,

            /// <summary>
            /// Specified WM_DESTROYCLIPBOARD enumeration value.
            /// </summary>
            WM_DESTROYCLIPBOARD = 0x0307,

            /// <summary>
            /// Specified WM_DRAWCLIPBOARD enumeration value.
            /// </summary>
            WM_DRAWCLIPBOARD = 0x0308,

            /// <summary>
            /// Specified WM_PAINTCLIPBOARD enumeration value.
            /// </summary>
            WM_PAINTCLIPBOARD = 0x0309,

            /// <summary>
            /// Specified WM_VSCROLLCLIPBOARD enumeration value.
            /// </summary>
            WM_VSCROLLCLIPBOARD = 0x030A,

            /// <summary>
            /// Specified WM_SIZECLIPBOARD enumeration value.
            /// </summary>
            WM_SIZECLIPBOARD = 0x030B,

            /// <summary>
            /// Specified WM_ASKCBFORMATNAME enumeration value.
            /// </summary>
            WM_ASKCBFORMATNAME = 0x030C,

            /// <summary>
            /// Specified WM_CHANGECBCHAIN enumeration value.
            /// </summary>
            WM_CHANGECBCHAIN = 0x030D,

            /// <summary>
            /// Specified WM_HSCROLLCLIPBOARD enumeration value.
            /// </summary>
            WM_HSCROLLCLIPBOARD = 0x030E,

            /// <summary>
            /// Specified WM_QUERYNEWPALETTE enumeration value.
            /// </summary>
            WM_QUERYNEWPALETTE = 0x030F,

            /// <summary>
            /// Specified WM_PALETTEISCHANGING enumeration value.
            /// </summary>
            WM_PALETTEISCHANGING = 0x0310,

            /// <summary>
            /// Specified WM_PALETTECHANGED enumeration value.
            /// </summary>
            WM_PALETTECHANGED = 0x0311,

            /// <summary>
            /// Specified WM_HOTKEY enumeration value.
            /// </summary>
            WM_HOTKEY = 0x0312,

            /// <summary>
            /// Specified WM_PRINT enumeration value.
            /// </summary>
            WM_PRINT = 0x0317,

            /// <summary>
            /// Specified WM_PRINTCLIENT enumeration value.
            /// </summary>
            WM_PRINTCLIENT = 0x0318,

            /// <summary>
            /// Specified WM_HANDHELDFIRST enumeration value.
            /// </summary>
            WM_HANDHELDFIRST = 0x0358,

            /// <summary>
            /// Specified WM_HANDHELDLAST enumeration value.
            /// </summary>
            WM_HANDHELDLAST = 0x035F,

            /// <summary>
            /// Specified WM_AFXFIRST enumeration value.
            /// </summary>
            WM_AFXFIRST = 0x0360,

            /// <summary>
            /// Specified WM_AFXLAST enumeration value.
            /// </summary>
            WM_AFXLAST = 0x037F,

            /// <summary>
            /// Specified WM_PENWINFIRST enumeration value.
            /// </summary>
            WM_PENWINFIRST = 0x0380,

            /// <summary>
            /// Specified WM_PENWINLAST enumeration value.
            /// </summary>
            WM_PENWINLAST = 0x038F,

            /// <summary>
            /// Specified WM_APP enumeration value.
            /// </summary>
            WM_APP = 0x8000,

            /// <summary>
            /// Specified WM_USER enumeration value.
            /// </summary>
            WM_USER = 0x0400,

            /// <summary>
            /// Specified WM_REFLECT enumeration value.
            /// </summary>
            WM_REFLECT = WM_USER + 0x1C00,

            /// <summary>
            /// Specified WM_THEMECHANGED enumeration value.
            /// </summary>
            WM_THEMECHANGED = 0x031A,
        };

        #endregion
        
        #region Windows-Styles-Const

        internal enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_CAPTION = 0x00C00000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_TILED = 0x00000000,
            WS_ICONIC = 0x20000000,
            WS_SIZEBOX = 0x00040000,
            WS_POPUPWINDOW = 0x80880000,
            WS_OVERLAPPEDWINDOW = 0x00CF0000,
            WS_TILEDWINDOW = 0x00CF0000,
            WS_CHILDWINDOW = 0x40000000
        };

        internal enum WindowExStyles
        {
            GWL_STYLE = -16,
            GWL_EXSTYLE = (-20),    //GetWindowLong
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_RIGHT = 0x00001000,   //Gives a window generic right-aligned properties.This depends on the window class.
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,  //Displays the window text using right-to-left reading order properties.
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,   //Places a vertical scroll bar to the left of the client area.
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_OVERLAPPEDWINDOW = 0x00000300,
            WS_EX_PALETTEWINDOW = 0x00000188,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_NOACTIVATE = 0x08000000
        };

        #endregion
    }
}