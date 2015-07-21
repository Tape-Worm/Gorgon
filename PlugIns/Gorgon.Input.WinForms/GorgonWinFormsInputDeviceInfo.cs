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
// Created: Thursday, June 30, 2011 6:35:52 AM
// 
#endregion

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// The Raw Input implementation of a device name.
	/// </summary>
	internal class GorgonWinFormsInputDeviceInfo
		: GorgonInputDeviceInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return whether the device is connected or not.
        /// </summary>
        public override bool IsConnected => true;

		#endregion

        #region Constructor/Destructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="GorgonWinFormsInputDeviceInfo"/> class.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <param name="deviceType">Type of device.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <exception cref="System.ArgumentException">The handle is set to 0.</exception>
		/// <exception cref="System.ArgumentNullException">Either the name, className or hidPath are NULL or empty.</exception>
		public GorgonWinFormsInputDeviceInfo(string name, InputDeviceType deviceType, string className, string hidPath)
			: base(name, deviceType, className, hidPath)
		{
		}
		#endregion
	}
}
