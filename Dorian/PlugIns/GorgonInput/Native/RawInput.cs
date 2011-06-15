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
// Created: Thursday, November 20, 2008 10:31:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.InputDevices.Internal
{
	/// <summary>
	/// Object for raw input data (platform agnostic).
	/// </summary>
	internal class RawInputData
	{
		#region Variables.
		private int _dataSize = 0;				// Size of the input data in bytes.
		private RAWINPUTx86 _x86Data;			// x86 specific data.
		private RAWINPUTx64 _x64Data;			// x64 specific data.
		#endregion

		#region Properties.
		/// <summary>
		/// Header for the data.
		/// </summary>
		public RAWINPUTHEADER Header
		{
			get;
			private set;
		}

		/// <summary>
		/// Mouse specific data.
		/// </summary>
		public RAWINPUTMOUSE Mouse
		{
			get;
			private set;
		}

		/// <summary>
		/// Keyboard specific data.
		/// </summary>
		public RAWINPUTKEYBOARD Keyboard
		{
			get;
			private set;
		}

		/// <summary>
		/// Human input device data.
		/// </summary>
		public RAWINPUTHID HID
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the raw input data.
		/// </summary>
		/// <param name="inputHandle">Handle to raw input data.</param>
		public void GetRawInputData(IntPtr inputHandle)
		{
			int result = -1;				// Result code.
			int dataSize = 0;				// Size of the input data.

			result = Win32API.GetRawInputData(inputHandle, RawInputCommand.Input, null, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
			if (result == -1)
				throw new GorgonException(GorgonErrors.CannotReadData, "Error reading raw input data.");

			if (IntPtr.Size == 4)
			{
				result = Win32API.GetRawInputData(inputHandle, RawInputCommand.Input, out _x86Data, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
				if ((result == -1) || (result != dataSize))
					throw new GorgonException(GorgonErrors.CannotReadData, "Error reading raw input data.");

				Header = _x86Data.Header;
				switch(Header.Type)
				{
					case RawInputType.Mouse:
						Mouse = _x86Data.Mouse;
						break;
					case RawInputType.Keyboard:
						Keyboard = _x86Data.Keyboard;
						break;
					case RawInputType.HID:
						HID = _x86Data.HID;
						break;
				}				
			}
			else
			{
				result = Win32API.GetRawInputData(inputHandle, RawInputCommand.Input, out _x64Data, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
				if ((result == -1) || (result != dataSize))
					throw new GorgonException(GorgonErrors.CannotReadData, "Error reading raw input data.");

				Header = _x64Data.Header;
				switch (Header.Type)
				{
					case RawInputType.Mouse:
						Mouse = _x64Data.Mouse;
						break;
					case RawInputType.Keyboard:
						Keyboard = _x64Data.Keyboard;
						break;
					case RawInputType.HID:
						HID = _x64Data.HID;
						break;
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputData"/> class.
		/// </summary>
		public RawInputData()
		{
			if (IntPtr.Size == 4)
			{
				_dataSize = Marshal.SizeOf(typeof(RAWINPUTx86));
				_x86Data = new RAWINPUTx86();
			}
			else
			{
				_dataSize = Marshal.SizeOf(typeof(RAWINPUTx64));
				_x64Data = new RAWINPUTx64();
			}			
		}
		#endregion
	}
}
