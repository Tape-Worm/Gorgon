#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Monday, November 17, 2008 10:00:25 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.SlimDXRenderer
{
	/// <summary>
	/// Win 32 API functions.
	/// </summary>
	internal static class Win32
	{
		/// <summary>
		/// Function to retrieve a funciton address from a DLL.
		/// </summary>
		/// <param name="hModule">Handle to the DLL.</param>
		/// <param name="procName">Name of the function.</param>
		/// <returns>A pointer to the function.</returns>
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
		public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

		/// <summary>
		/// Function to unload a DLL.
		/// </summary>
		/// <param name="hModule">Handle to the DLL.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool FreeLibrary(IntPtr hModule);		
		
		/// <summary>
		/// Function to load a DLL.
		/// </summary>
		/// <param name="lpFileName">Path to the DLL.</param>
		/// <returns>A handle to the DLL.</returns>
		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr LoadLibrary(string lpFileName);
	}
}
