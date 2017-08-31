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
// Created: Wednesday, August 12, 2015 8:07:36 PM
// 
#endregion

using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native
{
	/// <summary>
	/// Native windows kernal API functionality.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	internal static class KernelApi
	{
		#region Properties.
		/// <summary>
		/// Property to return the number of bytes of installed physical RAM.
		/// </summary>
		public static long TotalPhysicalRAM
		{
			get
			{
				MEMORYSTATUSEX memory = new MEMORYSTATUSEX
				{
					dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
				};

				if (GlobalMemoryStatusEx(ref memory))
				{
					return memory.ullTotalPhysical;
				}

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
				MEMORYSTATUSEX memory = new MEMORYSTATUSEX
				{
					dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
				};

				if (GlobalMemoryStatusEx(ref memory))
				{
					return memory.ullAvailablePhysical;
				}

				return -1;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the amount of memory available on the machine.
		/// </summary>
		/// <param name="stat">Memory status data.</param>
		/// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX stat);

		/// <summary>
		/// Function to return the frequency of the high precision timer.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="PerformanceFrequency">Frequency of timer.</param>
		/// <returns><b>true</b> if system supports high precision timing, <b>false</b> if not.</returns>
		[DllImport("kernel32", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool QueryPerformanceFrequency(out long PerformanceFrequency);

		/// <summary>
		/// Function to return the time from a high resolution timer.
		/// </summary>
		/// <remarks>See the MSDN documentation for a detailed description.</remarks>
		/// <param name="PerformanceCount">Time from the timer.</param>
		/// <returns><b>true</b> if system supports high precision timing, <b>false</b> if not.</returns>
		[DllImport("kernel32", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool QueryPerformanceCounter(out long PerformanceCount);
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes static members of the <see cref="KernelApi"/> class.
		/// </summary>
		static KernelApi()
		{
			Marshal.PrelinkAll(typeof(KernelApi));
		}
		#endregion
	}
}
