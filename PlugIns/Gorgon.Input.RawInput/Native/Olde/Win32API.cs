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
// Created: Friday, June 24, 2011 10:04:54 AM
// 
#endregion

using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native
{
	/// <summary>
	/// Static class for Native Win32 methods and corresponding structures.
	/// </summary>
	/// <remarks>
	/// This is a grouping of any API calls regularly used by Gorgon.
	/// <para>
	/// This list is by no means complete.  The Win32 API is just massive and would probably take a lifetime to map.
	/// </para>
	/// 	<para>
	/// These calls are considered "unsafe" and thus should be used with care.  If you don't know how to use a function, or why you want it, you probably don't need to use them.
	/// </para>
	/// 	<para>
	/// Please note that a lot of the enumerators/structures have slightly different names than their Win32 counterparts.  This was done for the sake of readability.  This does NOT affect their results or their effect on the results of their related functionality.
	/// </para>
	/// </remarks>
	[SuppressUnmanagedCodeSecurity]
	static class Win32API
	{

		#region Properties.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the dead zone of the joystick.
		/// </summary>
		/// <param name="uJoyID">ID the joystick to query.</param>
		/// <param name="puThreshold">Threshold for the joystick.</param>
		/// <returns>0 if successful, non-zero if not.</returns>
		[DllImport("WinMM.dll")]
		public static extern int joyGetThreshold(int uJoyID, out int puThreshold);

		/// <summary>
		/// Function to signal that the joystick configuration has changed.
		/// </summary>
		/// <param name="doNotUse">Do not use.</param>
		/// <returns>0 if successful, non-zero if not.</returns>
		[DllImport("WinMM.dll")]
		public static extern int joyConfigChanged(int doNotUse);

		/// <summary>
		/// Function to return the number of joystick devices supported by the current driver.
		/// </summary>
		/// <returns>Number of joysticks supported, 0 if no driver is installed.</returns>
		[DllImport("WinMM.dll")]
		public static extern int joyGetNumDevs();

		/// <summary>
		/// Function to return the joystick device capabilities.
		/// </summary>
		/// <param name="uJoyID">ID of the joystick to return.  -1 will return registry key, whether a device exists or not.</param>
		/// <param name="pjc">Joystick capabilities.</param>
		/// <param name="cbjc">Size of the JOYCAPS structure in bytes.</param>
		/// <returns>0 if successful, non zero if not.</returns>
		[DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
		public static extern int joyGetDevCaps(int uJoyID, ref JOYCAPS pjc, int cbjc);

		/// <summary>
		/// Function to retrieve joystick position information.
		/// </summary>
		/// <param name="uJoyID">ID the of joystick to query.</param>
		/// <param name="pji">Position information.</param>
		/// <returns>0 if successful, non zero if not.  JOYERR_UNPLUGGED if not connected.</returns>
		[DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
		public static extern int joyGetPos(int uJoyID, ref JOYINFO pji);

		/// <summary>
		/// Function to retrieve joystick position information.
		/// </summary>
		/// <param name="uJoyID">ID the of joystick to query.</param>
		/// <param name="pji">Position information.</param>
		/// <returns>0 if successful, non zero if not.  JOYERR_UNPLUGGED if not connected.</returns>
		[DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
		public static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="Win32API"/> class.
		/// </summary>
		static Win32API()
		{
			Marshal.PrelinkAll(typeof(Win32API));
		}
		#endregion
	}
}
