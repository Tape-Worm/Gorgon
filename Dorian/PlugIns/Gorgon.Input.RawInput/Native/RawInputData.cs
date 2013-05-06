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
using GorgonLibrary.Native;
using GorgonLibrary.Input.Raw.Properties;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Object for raw input data (platform agnostic).
	/// </summary>
	internal class RawInputData
		: IDisposable
	{
		#region Variables.
		private readonly int _headerSize;				// Size of the input data in bytes.
		private bool _isDisposed;			            // Flag to indicate that the object has been disposed of.
		private IntPtr _rawData = IntPtr.Zero;		    // Pointer to the raw input data.
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
		    int dataSize = 0;

			// Get data size.			
			int result = Win32API.GetRawInputData(handle, RawInputCommand.Input, IntPtr.Zero, ref dataSize, _headerSize);

		    if (result == -1)
		    {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_CANNOT_READ_DATA);
		    }

			// Get actual data.
			result = Win32API.GetRawInputData(handle, RawInputCommand.Input, _rawData, ref dataSize, _headerSize);

		    if ((result == -1) || (result != dataSize))
		    {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_CANNOT_READ_DATA);
		    }

		    unsafe
			{
				// Get header info.
				var rawInputx64 = (RAWINPUT*)_rawData;
				Header = rawInputx64->Header;

				// Get device data.
				switch (Header.Type)
				{
					case RawInputType.HID:
						HIDData = new byte[rawInputx64->Union.HID.Size * rawInputx64->Union.HID.Count];
						HID = rawInputx64->Union.HID;							
						Marshal.Copy(_rawData + _headerSize + 8, HIDData, 0, HIDData.Length);
						break;
					case RawInputType.Keyboard:
						Keyboard = rawInputx64->Union.Keyboard;
						break;
					case RawInputType.Mouse:
						Mouse = rawInputx64->Union.Mouse;
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
			_rawData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RAWINPUT)));
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="RawInputData"/> is reclaimed by garbage collection.
		/// </summary>
		~RawInputData()
		{
			Dispose();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
            if (!_isDisposed)
            {
                if (_rawData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_rawData);
                    _rawData = IntPtr.Zero;
                }
            }
            _isDisposed = true;

            GC.SuppressFinalize(this);
		}
		#endregion
	}
}
