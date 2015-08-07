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
// Created: Friday, July 3, 2015 3:25:20 PM
// 
#endregion

using Gorgon.Input.WinForms.Properties;
using Gorgon.Native;

namespace Gorgon.Input.WinForms
{
	/// <inheritdoc/>
	sealed class WinFormsKeyboardInfo
		: IGorgonKeyboardInfo2
	{
		#region Properties.
		/// <inheritdoc/>
		public int KeyCount
		{
			get;
		}

		/// <inheritdoc/>
		public int IndicatorCount
		{
			get;
		}

		/// <inheritdoc/>
		public int FunctionKeyCount
		{
			get;
		}

		/// <inheritdoc/>
		public KeyboardType KeyboardType
		{
			get;
		}

		/// <inheritdoc/>
		public string Description => string.Format(Resources.GORINP_WINFORMS_KEYBOARD_DESC, KeyboardType);

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath => "SystemKeyboard";

		/// <inheritdoc/>
		public string ClassName => "Keyboard";

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Keyboard;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsKeyboardInfo"/> class.
		/// </summary>
		public WinFormsKeyboardInfo()
		{
			KeyboardType = Win32API.KeyboardType;

			switch (Win32API.KeyboardType)
			{
				case KeyboardType.XT:
					KeyCount = 83;
					IndicatorCount = 3;
					break;
				case KeyboardType.OlivettiICO:
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case KeyboardType.AT:
					KeyCount = 84;
					IndicatorCount = 3;
					break;
				case KeyboardType.Enhanced:
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case KeyboardType.Nokia1050:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case KeyboardType.Nokia9140:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case KeyboardType.Japanese:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case KeyboardType.USB:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				default:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
			}

			FunctionKeyCount = Win32API.FunctionKeyCount;
		}
		#endregion
	}
}
