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
// Created: Thursday, June 30, 2011 12:51:38 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Collections;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// An unknown input device.
	/// </summary>
	/// <remarks>Unknown devices don't have a class wrapper for them, but instead use specific functions to set/return the values for the device.</remarks>
	public abstract class GorgonCustomHID
		: GorgonInputDevice
	{
		#region Properties.
		/// <summary>
		/// Property to return the user organized data for the device.
		/// </summary>
		public GorgonCustomHIDPropertyCollection Data
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the properties and their values.
		/// </summary>
		protected void ClearData()
		{
			Data.Clear();
		}


		/// <summary>
		/// Function to set a value for a property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">Value to assign to the property.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="propertyName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the propertyName parameter is an empty string.</exception>
		protected void SetData(string propertyName, object value)
		{
			GorgonUtility.AssertParamString(propertyName, "propertyName");

			if (Data.Contains(propertyName))
				Data[propertyName].SetValue(value);
			else
				Data.Add(new GorgonCustomHIDProperty(propertyName, value));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCustomHID"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the input device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		protected GorgonCustomHID(GorgonInputDeviceFactory owner, string deviceName, Control boundWindow)
			: base(owner, deviceName, boundWindow)
		{
			Data = new GorgonCustomHIDPropertyCollection();
		}
		#endregion
	}
}
