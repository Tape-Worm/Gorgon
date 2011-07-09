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
// Created: Friday, June 24, 2011 10:04:50 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using GorgonLibrary.Win32;

namespace GorgonLibrary.HID.RawInput
{
	/// <summary>
	/// Object for raw input data (platform agnostic).
	/// </summary>
	internal class RawInputData
		: IDisposable
	{
		#region Variables.
		private long _headerSize = 0;				// Size of the input data in bytes.
		private bool _isDisposed = false;			// Flag to indicate that the object has been disposed of.
		private IntPtr _rawData = IntPtr.Zero;		// Pointer to the raw input data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the raw input header.
		/// </summary>
		public RAWINPUTHEADER Header
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the keyboard raw input data.
		/// </summary>
		public RAWINPUTKEYBOARD Keyboard
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the mouse raw input data.
		/// </summary>
		public RAWINPUTMOUSE Mouse
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the HID raw input data.
		/// </summary>
		public RAWINPUTHID HID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the data bound to the HID device.
		/// </summary>
		public byte[] HIDData
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to marshal the raw input data back to the .NET world (x64).
		/// </summary>
		/// <param name="handle">Handle to the device we're getting data for.</param>
		/// <returns>A structure used to return the RAW input data.</returns>
		public void GetRawInputData(IntPtr handle)
		{
			int result = 0;
			int dataSize = 0;
			RAWINPUTx64 rawInputx64 = default(RAWINPUTx64);
			RAWINPUTx86 rawInputx86 = default(RAWINPUTx86);
						
			result = Win32API.GetRawInputData(handle, RawInputCommand.Input, IntPtr.Zero, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
			if (result == -1)
				throw new GorgonException(GorgonResult.CannotRead, "Error reading raw input data.");

			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
			{
				result = Win32API.GetRawInputData(handle, RawInputCommand.Input, _rawData, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
				if ((result == -1) || (result != dataSize))
					throw new GorgonException(GorgonResult.CannotRead, "Error reading raw input data.");

				rawInputx64 = new RAWINPUTx64();
				// Get header info.
				rawInputx64 = (RAWINPUTx64)Marshal.PtrToStructure(_rawData, typeof(RAWINPUTx64));
				Header = rawInputx64.Header;

				switch (Header.Type)
				{
					case RawInputType.HID:
						HIDData = new byte[rawInputx64.HID.Size * rawInputx64.HID.Count];
						HID = rawInputx64.HID;
						Marshal.Copy(new IntPtr(_rawData.ToInt32() + _headerSize + 8), HIDData, 0, HIDData.Length);
						break;
					case RawInputType.Keyboard:
						Keyboard = rawInputx64.Keyboard;
						break;
					case RawInputType.Mouse:
						Mouse = rawInputx64.Mouse;
						break;
				}
			}
			else
			{
				result = Win32API.GetRawInputData(handle, RawInputCommand.Input, _rawData, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
				if ((result == -1) || (result != dataSize))
					throw new GorgonException(GorgonResult.CannotRead, "Error reading raw input data.");

				rawInputx86 = new RAWINPUTx86();
				// Get header info.
				rawInputx86 = (RAWINPUTx86)Marshal.PtrToStructure(_rawData, typeof(RAWINPUTx86));
				Header = rawInputx86.Header;

				switch(Header.Type)
				{
					case RawInputType.HID:
						HIDData = new byte[rawInputx86.HID.Size * rawInputx86.HID.Count];
						HID = rawInputx86.HID;
						Marshal.Copy(new IntPtr(_rawData.ToInt32() + _headerSize + 8), HIDData, 0, HIDData.Length);
						break;
					case RawInputType.Keyboard:
						Keyboard = rawInputx86.Keyboard;
						break;
					case RawInputType.Mouse:
						Mouse = rawInputx86.Mouse;
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
			HIDData = new byte[0];
			Mouse = new RAWINPUTMOUSE();
			Keyboard = new RAWINPUTKEYBOARD();
			HID = new RAWINPUTHID();

			_headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				_rawData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RAWINPUTx64)));
			else
				_rawData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RAWINPUTx86)));
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					if (_rawData != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(_rawData);
						_rawData = IntPtr.Zero;
					}
				}
			}
			_isDisposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
