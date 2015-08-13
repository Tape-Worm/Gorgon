#region MIT
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
// Created: Wednesday, August 12, 2015 11:29:45 PM
// 
#endregion

using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Gorgon.Input;

namespace Gorgon.Native
{
	/// <summary>
	/// Win32 native keyboard input functionality.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	static class Win32KeyboardApi
	{
		#region Properties.
		/// <summary>
		/// Property to return the number of function keys on the keyboard.
		/// </summary>
		public static int FunctionKeyCount => GetKeyboardType(2);

		/// <summary>
		/// Property to return the keyboard type.
		/// </summary>
		public static KeyboardType KeyboardType
		{
			get
			{
				int keyboardType = GetKeyboardType(0);

				switch (keyboardType)
				{
					case 1:
						return KeyboardType.XT;
					case 2:
						return KeyboardType.OlivettiICO;
					case 3:
						return KeyboardType.AT;
					case 4:
						return KeyboardType.Enhanced;
					case 5:
						return KeyboardType.Nokia1050;
					case 6:
						return KeyboardType.Nokia9140;
					case 7:
						return KeyboardType.Japanese;
					case 81:
						return KeyboardType.USB;
					default:
						return KeyboardType.Unknown;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve keyboard type information.
		/// </summary>
		/// <param name="nTypeFlag">The type of info.</param>
		/// <returns>The requested information.</returns>
		[DllImport("User32.dll", CharSet = CharSet.Ansi)]
		private static extern int GetKeyboardType(int nTypeFlag);

		/// <summary>
		/// Function to get the state of a key.
		/// </summary>
		/// <param name="nVirtKey">Virtual key code to retrieve.</param>
		/// <returns>A bit mask containing the state of the virtual key.</returns>
		[DllImport("User32.dll"), SuppressUnmanagedCodeSecurity]
		private static extern short GetKeyState(Keys nVirtKey);

		/// <summary>
		/// Function to retrieve the scan code for a virtual key.
		/// </summary>
		/// <param name="uCode">Virtual key code</param>
		/// <param name="uMapType">Mapping type.</param>
		/// <returns>The scan code.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto), SuppressUnmanagedCodeSecurity]
		private static extern int MapVirtualKey(Keys uCode, int uMapType);

		/// <summary>
		/// Function to return the scan code for a virtual key.
		/// </summary>
		/// <param name="virtualKey">The virtual key to evaluate.</param>
		/// <returns>The scan code for the virtual key.</returns>
		public static int GetScancode(Keys virtualKey)
		{
			return MapVirtualKey(virtualKey, 0);
		}

		/// <summary>
		/// Function to determine if the specified virtual key is pressed or not.
		/// </summary>
		/// <param name="virtualKey">The virtual key to evaluate.</param>
		/// <returns><b>true</b> if down, <b>false</b> if not.</returns>
		public static bool CheckKeyDown(Keys virtualKey)
		{
			return (GetKeyState(virtualKey) & 0x80) == 0x80;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes static members of the <see cref="Win32KeyboardApi"/> class.
		/// </summary>
		static Win32KeyboardApi()
		{
			Marshal.PrelinkAll(typeof(Win32KeyboardApi));
		}
		#endregion
	}
}
