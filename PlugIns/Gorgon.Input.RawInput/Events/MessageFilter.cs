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
// Created: Friday, June 24, 2011 10:04:46 AM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Processor for raw input messages.
	/// </summary>
	class MessageFilter
		: IMessageFilter
	{
		#region Variables.
		// Size of the input data in bytes.
		private readonly int _headerSize = DirectAccess.SizeOf<RAWINPUTHEADER>();	
		#endregion

		#region Properties.
		/// <summary>
		/// Event fired when a raw input keyboard event occurs.
		/// </summary>
		public Action<RawInputKeyboardEventArgs> RawInputKeyboardData
		{
			get;
			set;
		}

		/// <summary>
		/// Event fired when a pointing device event occurs.
		/// </summary>
		public Action<RawInputPointingDeviceEventArgs> RawInputPointingDeviceData
		{
			get;
			set;
		}
		#endregion

		#region IMessageFilter Members
	    /// <summary>
	    /// Filters out a message before it is dispatched.
	    /// </summary>
	    /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
	    /// <returns>
	    /// true to filter the message and stop it from being dispatched; false to allow the message to continue to the next filter or control.
	    /// </returns>
	    public unsafe bool PreFilterMessage(ref Message m)
	    {
			int dataSize = 0;

	        // Get data size.			
	        int result = Win32API.GetRawInputData(m.LParam, RawInputCommand.Input, IntPtr.Zero, ref dataSize, _headerSize);

	        if (result == -1)
	        {
	            throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
	        }

	        // Get actual data.
	        var rawInputPtr = stackalloc byte[dataSize];
	        result = Win32API.GetRawInputData(m.LParam,
	                                          RawInputCommand.Input,
	                                          (IntPtr)rawInputPtr,
	                                          ref dataSize,
	                                          _headerSize);

	        if ((result == -1)
	            || (result != dataSize))
	        {
	            throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
	        }

	        var rawInput = *((RAWINPUT*)rawInputPtr);

	        switch (rawInput.Header.Type)
	        {
	            case RawInputType.Mouse:
			        RawInputPointingDeviceData?.Invoke(new RawInputPointingDeviceEventArgs(rawInput.Header.Device,
			                                                                               ref rawInput.Union.Mouse));
			        break;
	            case RawInputType.Keyboard:
			        RawInputKeyboardData?.Invoke(new RawInputKeyboardEventArgs(rawInput.Header.Device,
			                                                                   ref rawInput.Union.Keyboard));
			        break;
				default:
			        return true;
	        }

	        return false;
	    }
	    #endregion
	}
}
