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

using System;
using System.Runtime.InteropServices;
using Gorgon.Input.WinForms.Properties;

namespace Gorgon.Input.WinForms
{
	/// <inheritdoc/>
	sealed class WinFormsKeyboardInfo
		: IGorgonKeyboardInfo
	{
		#region Methods.
		/// <summary>
		/// Function to retrieve keyboard type information.
		/// </summary>
		/// <param name="nTypeFlag">The type of info.</param>
		/// <returns>The requested information.</returns>
		[DllImport("User32.dll", CharSet = CharSet.Ansi)]
		private static extern int GetKeyboardType(int nTypeFlag);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsKeyboardInfo"/> class.
		/// </summary>
		public WinFormsKeyboardInfo()
		{
			Name = Resources.GORINP_WINFORMS_KEYBOARD_DESC;
			UUID = Guid.Empty;
			ClassName = "Keyboard";
			HumanInterfaceDevicePath = "SystemKeyboard";

			int keyboardType = GetKeyboardType(0);

			switch (keyboardType)
			{
				case 1:
					KeyboardType = KeyboardType.XT;
					KeyCount = 83;
					IndicatorCount = 3;
					break;
				case 2:
					KeyboardType = KeyboardType.OlivettiICO;
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case 3:
					KeyboardType = KeyboardType.AT;
					KeyCount = 84;
					IndicatorCount = 3;
					break;
				case 4:
					KeyboardType = KeyboardType.Enhanced;
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case 5:
					KeyboardType = KeyboardType.Nokia1050;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case 6:
					KeyboardType = KeyboardType.Nokia9140;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case 7:
					KeyboardType = KeyboardType.Japanese;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case 81:
					KeyboardType = KeyboardType.USB;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				default:
					KeyboardType = KeyboardType.Unknown;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
			}

			FunctionKeyCount = GetKeyboardType(2);
		}
		#endregion

		#region IGorgonKeyboardInfo Members
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
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get;
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get;
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Keyboard;

		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public string Name
		{
			get;
		}
		#endregion
	}
}
