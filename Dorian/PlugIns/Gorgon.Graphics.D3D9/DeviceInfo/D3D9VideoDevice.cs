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
// Created: Thursday, July 21, 2011 3:41:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A D3D9 video device.
	/// </summary>
	public class D3D9VideoDevice
		: GorgonVideoDevice
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a description of the device.
		/// </summary>
		public override string Description
		{
			get
			{
				return "Fill me in.";
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the device capabilities.
		/// </summary>
		/// <returns>
		/// An enumerable list of driver capabilities.
		/// </returns>
		protected override IEnumerable<KeyValuePair<string, string>> GetDeviceCapabilities()
		{
			IDictionary<string, string> _deviceCaps = null;

			return _deviceCaps;			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoDevice"/> class.
		/// </summary>
		/// <param name="index">Index of the driver in the collection.</param>
		internal D3D9VideoDevice(int index)
			: base(index)
		{
			
		}
		#endregion
	}
}
