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
// Created: Thursday, August 6, 2015 12:17:06 AM
// 
#endregion

using System.Windows.Forms;
using Gorgon.Input.WinForms.Properties;

namespace Gorgon.Input.WinForms
{
	/// <inheritdoc/>
	sealed class WinFormsMouseInfo2
		: IGorgonMouseInfo2
	{
		#region Properties.
		/// <inheritdoc/>
		public int ButtonCount => SystemInformation.MouseButtons;

		/// <inheritdoc/>
		public string ClassName => "Mouse";

		/// <inheritdoc/>
		public string Description => string.Format(Resources.GORINP_WINFORMS_MOUSE_DESC, ButtonCount);

		/// <inheritdoc/>
		public bool HasHorizontalWheel => false;

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath => "SystemMouse";

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Mouse;

		/// <inheritdoc/>
		public int SamplingRate => -1;

		/// <inheritdoc/>
		public bool HasMouseWheel
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsMouseInfo2" /> class.
		/// </summary>
		public WinFormsMouseInfo2()
		{
			HasMouseWheel = SystemInformation.MouseWheelPresent;
		}
		#endregion
	}
}
