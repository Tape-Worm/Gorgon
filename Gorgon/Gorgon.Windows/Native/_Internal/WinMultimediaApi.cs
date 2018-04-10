﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, August 12, 2015 8:04:27 PM
// 
#endregion

using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native
{
	/// <summary>
	/// Native functions from the windows multimedia API.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	internal static class WinMultimediaApi
	{
		#region Methods.
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
		public static extern uint timeBeginPeriod(uint uPeriod);

		/// <summary>
		/// Function to end a timing session.
		/// </summary>
		/// <param name="uPeriod">Minimum resolution in milliseconds.</param>
		/// <returns>0 if successful, non 0 if not.</returns>
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern uint timeEndPeriod(uint uPeriod);

		/// <summary>
		/// Function to get time capabilities.
		/// </summary>
		/// <param name="timeCaps">Timer capabilities value.</param>
		/// <param name="size">Size of the value, in bytes.</param>
		/// <returns>0 if successful, non-0 if not.</returns>
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern int timeGetDevCaps(ref TIMECAPS timeCaps, int size);
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes static members of the <see cref="WinMultimediaApi"/> class.
		/// </summary>
		static WinMultimediaApi()
		{
			Marshal.PrelinkAll(typeof(WinMultimediaApi));
		}
		#endregion
	}
}
