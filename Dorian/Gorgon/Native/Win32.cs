#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, June 18, 2011 10:36:53 AM
// 
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GorgonLibrary
{
	#region Enums.
	/// <summary>
	/// Types of messages that passed to a window.
	/// </summary>
	/// <remarks>See the MSDN documentation for more detail.</remarks>
	internal enum WindowMessages
	{
		/// <summary>System command (WM_SYSCOMMAND)</summary>
		SysCommand = 0x0112,
		/// <summary>Quit command (WM_QUIT)</summary>
		Quit = 0x0012,
		/// <summary>Window has been resized. (WM_SIZE)</summary>
		Size = 0x0005,
		/// <summary>Query the drag icon. (WM_QUERYDRAGICON)</summary>
		QueryDragIcon = 0x0037,
		/// <summary>Get the window icon. (WM_GETICON)</summary>
		GetIcon = 0x007F,
		/// <summary>Set the window icon. (WM_SETICON)</summary>
		SetIcon = 0x0080,
		/// <summary></summary>
		NULL = 0,
		/// <summary></summary>
		Create = 0x0001,
		/// <summary></summary>
		Destroy = 0x0002,
		/// <summary></summary>
		Move = 0x0003,
		/// <summary></summary>
		Activate = 0x0006,
		/// <summary></summary>
		SetFocus = 0x0007,
		/// <summary></summary>
		KillFocus = 0x0008,
		/// <summary></summary>
		Enable = 0x000A,
		/// <summary></summary>
		SetRedraw = 0x000B,
		/// <summary></summary>
		SetText = 0x000C,
		/// <summary></summary>
		GetText = 0x000D,
		/// <summary></summary>
		GetTextLength = 0x000E,
		/// <summary></summary>
		Paint = 0x000F,
		/// <summary></summary>
		Close = 0x0010,
		/// <summary></summary>
		QueryEndSession = 0x0011,
		/// <summary></summary>
		QueryOpen = 0x0013,
		/// <summary></summary>
		EraseBackground = 0x0014,
		/// <summary></summary>
		SystemColorChange = 0x0015,
		/// <summary></summary>
		EndSession = 0x0016,
		/// <summary></summary>
		ShowWindow = 0x0018,
		/// <summary></summary>
		ControlColor = 0x0019,
		/// <summary></summary>
		WinINIChange = 0x001A,
		/// <summary></summary>
		SettingChange = 0x001A,
		/// <summary></summary>
		DeviceModeChange = 0x001B,
		/// <summary></summary>
		ActivateApplication = 0x001C,
		/// <summary></summary>
		FontChange = 0x001D,
		/// <summary></summary>
		TimeChange = 0x001E,
		/// <summary></summary>
		CancelMode = 0x001F,
		/// <summary></summary>
		SetCursor = 0x0020,
		/// <summary></summary>
		MouseActivate = 0x0021,
		/// <summary></summary>
		ChildActivate = 0x0022,
		/// <summary></summary>
		QueueSync = 0x0023,
		/// <summary></summary>
		GetMinMaxInformation = 0x0024,
		/// <summary></summary>
		PaintIcon = 0x0026,
		/// <summary></summary>
		IconEraseBackground = 0x0027,
		/// <summary></summary>
		NextDialogControl = 0x0028,
		/// <summary></summary>
		SpoolerStatus = 0x002A,
		/// <summary></summary>
		DrawItem = 0x002B,
		/// <summary></summary>
		MeasureItem = 0x002C,
		/// <summary></summary>
		DeleteItem = 0x002D,
		/// <summary></summary>
		VKeyToItem = 0x002E,
		/// <summary></summary>
		CharToItem = 0x002F,
		/// <summary></summary>
		SetFont = 0x0030,
		/// <summary></summary>
		GetFont = 0x0031,
		/// <summary></summary>
		SetHotKey = 0x0032,
		/// <summary></summary>
		GetHotKey = 0x0033,
		/// <summary></summary>
		CompareItem = 0x0039,
		/// <summary></summary>
		GetObject = 0x003D,
		/// <summary></summary>
		Compacting = 0x0041,
		/// <summary></summary>
		COMMNotify = 0x0044,
		/// <summary></summary>
		WindowPositionChanging = 0x0046,
		/// <summary></summary>
		WindowPositionChanged = 0x0047,
		/// <summary></summary>
		Power = 0x0048,
		/// <summary></summary>
		CopyData = 0x004A,
		/// <summary></summary>
		CancelJournal = 0x004B,
		/// <summary></summary>
		Notify = 0x004E,
		/// <summary></summary>
		InputLanguageChangeRequest = 0x0050,
		/// <summary></summary>
		InputLanguageChange = 0x0051,
		/// <summary></summary>
		TCard = 0x0052,
		/// <summary></summary>
		Help = 0x0053,
		/// <summary></summary>
		UserChanged = 0x0054,
		/// <summary></summary>
		NotifyFormat = 0x0055,
		/// <summary></summary>
		ContextMenu = 0x007B,
		/// <summary></summary>
		StyleChanging = 0x007C,
		/// <summary></summary>
		StyleChanged = 0x007D,
		/// <summary></summary>
		DisplayChange = 0x007E,
		/// <summary></summary>
		NCCreate = 0x0081,
		/// <summary></summary>
		NCDestroy = 0x0082,
		/// <summary></summary>
		NCCalcSize = 0x0083,
		/// <summary></summary>
		NCHitTest = 0x0084,
		/// <summary></summary>
		NCPaint = 0x0085,
		/// <summary></summary>
		NCActivate = 0x0086,
		/// <summary></summary>
		GetDialogCode = 0x0087,
		/// <summary></summary>
		SynchronizePaint = 0x0088,
		/// <summary></summary>
		NCMouseMove = 0x00A0,
		/// <summary></summary>
		NCLeftButtonDown = 0x00A1,
		/// <summary></summary>
		NCLeftButtonUp = 0x00A2,
		/// <summary></summary>
		NCLeftButtonDoubleClick = 0x00A3,
		/// <summary></summary>
		NCRightButtonDown = 0x00A4,
		/// <summary></summary>
		NCRightButtonUp = 0x00A5,
		/// <summary></summary>
		NCRightButtonDoubleClick = 0x00A6,
		/// <summary></summary>
		NCMiddleButtonDown = 0x00A7,
		/// <summary></summary>
		NCMiddleButtonUp = 0x00A8,
		/// <summary></summary>
		NCMiddleButtonDoubleClick = 0x00A9,
		/// <summary></summary>
		KeyDown = 0x0100,
		/// <summary></summary>
		KeyUp = 0x0101,
		/// <summary></summary>
		Char = 0x0102,
		/// <summary></summary>
		DeadChar = 0x0103,
		/// <summary></summary>
		SysKeyDown = 0x0104,
		/// <summary></summary>
		SysKeyUp = 0x0105,
		/// <summary></summary>
		SysChar = 0x0106,
		/// <summary></summary>
		SysDeadChar = 0x0107,
		/// <summary></summary>
		KeyLast = 0x0180,
		/// <summary></summary>
		IMEStartComposition = 0x010D,
		/// <summary></summary>
		IMEEndComposition = 0x010E,
		/// <summary></summary>
		IMEComposition = 0x010F,
		/// <summary></summary>
		IMEKeyLast = 0x010F,
		/// <summary></summary>
		InitializeDialog = 0x0110,
		/// <summary></summary>
		Command = 0x0111,
		/// <summary></summary>
		Timer = 0x0113,
		/// <summary></summary>
		HorizontalScroll = 0x0114,
		/// <summary></summary>
		VerticalScroll = 0x0115,
		/// <summary></summary>
		InitializeMenu = 0x0116,
		/// <summary></summary>
		InitializeMenuPopup = 0x0117,
		/// <summary></summary>
		MenuSelect = 0x011F,
		/// <summary></summary>
		MenuChar = 0x0120,
		/// <summary></summary>
		EnterIdle = 0x0121,
		/// <summary></summary>
		MenuRightButtonUp = 0x0122,
		/// <summary></summary>
		MenuDrag = 0x0123,
		/// <summary></summary>
		MenuGetObject = 0x0124,
		/// <summary></summary>
		UnInitializeMenuPopup = 0x0125,
		/// <summary></summary>
		MenuCommand = 0x0126,
		/// <summary></summary>
		ControlColorMessageBox = 0x0132,
		/// <summary></summary>
		ControlColorEdit = 0x0133,
		/// <summary></summary>
		ControlColorListBox = 0x0134,
		/// <summary></summary>
		ControlColorButton = 0x0135,
		/// <summary></summary>
		ControlColorDialog = 0x0136,
		/// <summary></summary>
		ControlColorScrollbar = 0x0137,
		/// <summary></summary>
		ControlColorStatic = 0x0138,
		/// <summary></summary>
		MouseMove = 0x0200,
		/// <summary></summary>
		LeftButtonDown = 0x0201,
		/// <summary></summary>
		LeftButtonUp = 0x0202,
		/// <summary></summary>
		LeftButtonDoubleClick = 0x0203,
		/// <summary></summary>
		RightButtonDown = 0x0204,
		/// <summary></summary>
		RightButtonUp = 0x0205,
		/// <summary></summary>
		RightButtonDoubleClick = 0x0206,
		/// <summary></summary>
		MiddleButtonDown = 0x0207,
		/// <summary></summary>
		MiddleButtonUp = 0x0208,
		/// <summary></summary>
		MiddleButtonDoubleClick = 0x0209,
		/// <summary></summary>
		MouseWheel = 0x020A,
		/// <summary></summary>
		ParentNotify = 0x0210,
		/// <summary></summary>
		EnterMenuLoop = 0x0211,
		/// <summary></summary>
		ExitMenuLoop = 0x0212,
		/// <summary></summary>
		NextMenu = 0x0213,
		/// <summary></summary>
		Sizing = 0x0214,
		/// <summary></summary>
		CaptureChanged = 0x0215,
		/// <summary></summary>
		Moving = 0x0216,
		/// <summary></summary>
		DeviceChange = 0x0219,
		/// <summary></summary>
		MDICreate = 0x0220,
		/// <summary></summary>
		MDIDestroy = 0x0221,
		/// <summary></summary>
		MDIActivate = 0x0222,
		/// <summary></summary>
		MDIRestore = 0x0223,
		/// <summary></summary>
		MDINext = 0x0224,
		/// <summary></summary>
		MDIMaximize = 0x0225,
		/// <summary></summary>
		MDITile = 0x0226,
		/// <summary></summary>
		MDICacade = 0x0227,
		/// <summary></summary>
		MDIIconArrange = 0x0228,
		/// <summary></summary>
		MDIGetActive = 0x0229,
		/// <summary></summary>
		MDISetMenu = 0x0230,
		/// <summary></summary>
		EnterSizeMove = 0x0231,
		/// <summary></summary>
		ExitSizeMove = 0x0232,
		/// <summary></summary>
		DropFiles = 0x0233,
		/// <summary></summary>
		MDIRefreshMenu = 0x0234,
		/// <summary></summary>
		IMESetContext = 0x0281,
		/// <summary></summary>
		IMENotify = 0x0282,
		/// <summary></summary>
		IMEControl = 0x0283,
		/// <summary></summary>
		IMECompositionFull = 0x0284,
		/// <summary></summary>
		IMESelect = 0x0285,
		/// <summary></summary>
		IMEChar = 0x0286,
		/// <summary></summary>
		IMERequest = 0x0288,
		/// <summary></summary>
		IMEKeyDown = 0x0290,
		/// <summary></summary>
		IMEKeyUp = 0x0291,
		/// <summary></summary>
		MouseHover = 0x02A1,
		/// <summary></summary>
		MouseLeave = 0x02A3,
		/// <summary></summary>
		Cut = 0x0300,
		/// <summary></summary>
		Copy = 0x0301,
		/// <summary></summary>
		Paste = 0x0302,
		/// <summary></summary>
		Clear = 0x0303,
		/// <summary></summary>
		Undo = 0x0304,
		/// <summary></summary>
		RenderFormat = 0x0305,
		/// <summary></summary>
		RenderAllFormats = 0x0306,
		/// <summary></summary>
		DestroyClipboard = 0x0307,
		/// <summary></summary>
		DrawClipboard = 0x0308,
		/// <summary></summary>
		Paintclipboard = 0x0309,
		/// <summary></summary>
		VerticalScrollClipboard = 0x030A,
		/// <summary></summary>
		SizeClipboard = 0x030B,
		/// <summary></summary>
		AskClipboardFormatName = 0x030C,
		/// <summary></summary>
		ChangeClipboardChain = 0x030D,
		/// <summary></summary>
		HorizontalScrollClipboard = 0x030E,
		/// <summary></summary>
		QueryNewPalette = 0x030F,
		/// <summary></summary>
		PaletteIsChanging = 0x0310,
		/// <summary></summary>
		PaletteChanged = 0x0311,
		/// <summary></summary>
		HotKey = 0x0312,
		/// <summary></summary>
		Print = 0x0317,
		/// <summary></summary>
		PrintClient = 0x0318,
		/// <summary></summary>
		HandheldFirst = 0x0358,
		/// <summary></summary>
		HandheldLast = 0x035F,
		/// <summary></summary>
		AFXFirst = 0x0360,
		/// <summary></summary>
		AFXLast = 0x037F,
		/// <summary></summary>
		PenWindowFirst = 0x0380,
		/// <summary></summary>
		PenWindowLast = 0x038F,
		/// <summary></summary>
		Application = 0x8000,
		/// <summary></summary>
		User = 0x0400,
		/// <summary></summary>
		RawInput = 0x00FF
	}

	/// <summary>
	/// Types of System commands.
	/// </summary>
	/// <remarks>
	/// See the MSDN documentation for more detail.
	/// <para>
	/// These are often used with the WM_SYSCOMMAND message.
	/// </para>
	/// </remarks>
	internal enum SysCommands
	{
		/// <summary>Screen saver. (SC_SCREENSAVE)</summary>
		ScreenSave = 0xF140,
		/// <summary>Monitor power saving. (SC_MONITORPOWER)</summary>
		MonitorPower = 0xF170
	}

	/// <summary>
	/// Flags for PeekMessage function.
	/// </summary>
	/// <remarks>See the MSDN documentation for more detail.</remarks>
	internal enum PeekMessageFlags
	{
		/// <summary>Keep message on the message queue.</summary>
		NoRemove = 0,
		/// <summary>Remove message from the queue.</summary>
		Remove = 1,
		/// <summary>Do not yield execution to waiting threads.</summary>
		NoYield = 2
	}
	#endregion

	/// <summary>
	/// Contains various necessary Win32 related functions.
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurity()]
	internal static class Win32
	{
		#region Value Types.
		/// <summary>
		/// Value type representing a Window message.
		/// </summary>
		/// <remarks>
		/// See the MSDN documentation for more detail.
		/// <para>
		/// Used to pass various messages back and forth between the OS and the app.
		/// </para>
		/// </remarks>
		[StructLayout(LayoutKind.Sequential)]
		internal struct MSG
		{
			/// <summary>Window handle.</summary>
			public IntPtr hwnd;
			/// <summary>Message to process.</summary>
			public WindowMessages Message;
			/// <summary>Window message parameter 1.</summary>
			public uint wParam;
			/// <summary>Window message parameter 2.</summary>
			public uint lParam;
			/// <summary>Time message was sent?</summary>
			public uint time;
			/// <summary>Mouse pointer position.</summary>
			public Point pt;
		}

		/// <summary>
		/// Used with GlobalMemoryStatusEx.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct MemoryStatusEx
		{
			/// <summary/>
			public uint dwLength;
			/// <summary/>
			public uint dwMemoryLoad;
			/// <summary/>
			public long ullTotalPhysical;
			/// <summary/>
			public long ullAvailablePhysical;
			/// <summary/>
			public long ullTotalPageFile;
			/// <summary/>
			public long ullAvailablePageFile;
			/// <summary/>
			public long ullTotalVirtual;
			/// <summary/>
			public long ullAvailableVirtual;
			/// <summary/>
			public long ullAvailableExtendedVirtual;
		}
		#endregion

		#region Imports.
		[DllImport("mscoree.dll", CharSet=CharSet.Unicode)]
		private static extern bool StrongNameSignatureVerificationEx(string wszFilePath, bool fForceVerification, ref bool  pfWasVer

		/// <summary/>
		[DllImport("kernel32.dll")] 
		private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx stat);

		/// <summary>
		/// Function to return the frequency of the high precision timer.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="PerformanceFrequency">Frequency of timer.</param>
		/// <returns>TRUE if system supports high precision timing, FALSE if not.</returns>
		[DllImport("kernel32", CharSet = CharSet.Auto)]
		public static extern bool QueryPerformanceFrequency(out long PerformanceFrequency);

		/// <summary>
		/// Function to return the time from a high resolution timer.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="PerformanceCount">Time from the timer.</param>
		/// <returns>TRUE if system supports high precision timing, FALSE if not.</returns>
		[DllImport("kernel32", CharSet = CharSet.Auto), System.Security.SuppressUnmanagedCodeSecurity()]
		public static extern bool QueryPerformanceCounter(out long PerformanceCount);

		/// <summary>
		/// Function to process window messages.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="msg">Message block to retrieve.</param>
		/// <param name="hwnd">Window to retrieve messages from, FALSE for all.</param>
		/// <param name="wFilterMin">Minimum message.</param>
		/// <param name="wFilterMax">Maximum message.</param>
		/// <param name="flags">Flags for the function.</param>
		/// <returns>TRUE if messages are ready for processing, FALSE if not.</returns>
		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool PeekMessage(ref MSG msg, IntPtr hwnd, int wFilterMin, int wFilterMax, PeekMessageFlags flags);

		/// <summary>
		/// Dispatches a message to a window procedure. It is typically used to dispatch a message retrieved by the Peek/GetMessage function.
		/// </summary>
		/// <param name="msg">Message to dispatch.</param>
		/// <returns>The return value specifies the value returned by the window procedure. Although its meaning depends on the message being dispatched, the return value generally is ignored.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr DispatchMessage([In] ref MSG msg);

		/// <summary>
		/// Translates virtual-key messages into character messages. The character messages are posted to the calling thread's message queue, to be read the next time the thread calls the GetMessage or PeekMessage function.
		/// </summary>
		/// <param name="msg">Value type that contains message information retrieved from the calling thread's message queue by using the GetMessage or PeekMessage function.</param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool TranslateMessage([In] ref MSG msg);

		/// <summary>
		/// Function to return time from a medium precision timer.
		/// </summary>
		/// <remarks>
		/// See the MSDN documentation for a detailed description.
		/// <para>
		/// This timer is of lower precision than the high precision timers, do not use unless
		/// your system does not support high resolution timers.
		/// </para>
		/// </remarks>
		/// <returns>Time in milliseconds.</returns>
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern int timeGetTime();

		/// <summary>
		/// Function to start a timing session.
		/// </summary>
		/// <param name="uPeriod">Minimum resolution in milliseconds.</param>
		/// <returns>0 if successful, non 0 if not.</returns>
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern int timeBeginPeriod(uint uPeriod);

		/// <summary>
		/// Function to end a timing session.
		/// </summary>
		/// <param name="uPeriod">Minimum resolution in milliseconds.</param>
		/// <returns>0 if successful, non 0 if not.</returns>
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern int timeEndPeriod(uint uPeriod);

		/// <summary>
		/// Function to set the visibility of the mouse cursor.
		/// </summary>
		/// <param name="bShow">TRUE to show, FALSE to hide.</param>
		/// <returns>-1 if no mouse is installed, 0 or greater for the number of times this function has been called with TRUE.</returns>
		[DllImport("User32.dll")]
		public static extern int ShowCursor(bool bShow);
		#endregion

		#region Methods.
		/// <summary>
		/// Property to return the number of bytes of installed physical RAM.
		/// </summary>
		public static long TotalPhysicalRAM
		{
			get
			{
				MemoryStatusEx memory = new MemoryStatusEx();

				memory.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
				if (GlobalMemoryStatusEx(ref memory))
					return memory.ullTotalPhysical;

				return -1;
			}
		}

		/// <summary>
		/// Property to return the number of bytes of free available RAM.
		/// </summary>
		public static long AvailablePhysicalRAM
		{
			get
			{
				MemoryStatusEx memory = new MemoryStatusEx();

				memory.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
				if (GlobalMemoryStatusEx(ref memory))
					return memory.ullAvailablePhysical;

				return -1;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="Win32"/> class.
		/// </summary>
		static Win32()
		{
			Marshal.PrelinkAll(typeof(Win32));
		}
		#endregion
	}
}
