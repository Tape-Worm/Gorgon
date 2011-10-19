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
// Created: Tuesday, October 18, 2011 9:12:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// Process management interface.
	/// </summary>
	public static class GorgonProcess
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Gorgon process that's currently executing.
		/// </summary>
		public static Process ApplicationProcess
		{
			get;
			private set;
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the foreground window handle.
		/// </summary>
		/// <returns>The handle to the foreground window.</returns>
		public static IntPtr GetForegroundWindow()
		{
			return Win32API.GetForegroundWindow();
		}

		/// <summary>
		/// Function to retrieve the process that is associated with the current foreground window.
		/// </summary>
		/// <returns>The process for the foreground window, or NULL if not found.</returns>
		public static Process GetActiveProcess()
		{
			uint processID = 0;
			IntPtr foregroundWindow = IntPtr.Zero;

			foregroundWindow = Win32API.GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
				return null;

			Win32API.GetWindowThreadProcessId(foregroundWindow, out processID);

			return Process.GetProcessById((int)processID);
		}

		/// <summary>
		/// Function to retrieve the process associated with a window handle.
		/// </summary>
		/// <param name="windowHandle">Handle to the window to retrieve the process from.</param>
		/// <returns>The process for the specified window, or NULL if not found.</returns>
		public static Process GetProcessByWindow(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero)
				return null;

			uint processID = 0;
			Win32API.GetWindowThreadProcessId(windowHandle, out processID);

			return Process.GetProcessById((int)processID);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="ApplicationProcess"/> class.
		/// </summary>
		static GorgonProcess()
		{
			ApplicationProcess = Process.GetCurrentProcess();
		}
		#endregion
	}
}
