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
// Created: Tuesday, July 12, 2011 1:36:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XI = SlimDX.XInput;

namespace GorgonLibrary.HID.XInput
{
	/// <summary>
	/// The XInput implementation of a device name.
	/// </summary>
	internal class GorgonXInputDeviceName
		: GorgonInputDeviceName
	{
		#region Properties.
		/// <summary>
		/// Property to return the handle to the device.
		/// </summary>
		public XI.Controller Controller
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the index of the device.
		/// </summary>
		public int Index
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputDeviceName"/> class.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="controller">Controller interface.</param>
		/// <param name="index">Index of the controller.</param>
		/// <exception cref="System.ArgumentNullException">Either the name, className or hidPath are NULL or empty.</exception>
		public GorgonXInputDeviceName(string name, string className, string hidPath, XI.Controller controller, int index)
			: base(name, className, hidPath)
		{
			Controller = controller;
			Index = index;
		}
		#endregion
	}
}
