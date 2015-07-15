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
// Created: Friday, July 3, 2015 3:42:52 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Extra information for the raw input data info structures.
	/// </summary>
	interface IRawInputMouseInfo
		: IGorgonMouseInfo
	{
		/// <summary>
		/// Property to return the handle to the device.
		/// </summary>
		IntPtr Handle
		{
			get;
		}

		/// <summary>
		/// Property to return the HID usage.
		/// </summary>
		HIDUsage Usage
		{
			get;
		}

		/// <summary>
		/// Property to return the HID usage page.
		/// </summary>
		HIDUsagePage UsagePage
		{
			get;
		}
	}
}
