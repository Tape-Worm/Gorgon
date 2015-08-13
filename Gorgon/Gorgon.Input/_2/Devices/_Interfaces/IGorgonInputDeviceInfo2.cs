﻿#region MIT
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
// Created: Friday, July 3, 2015 11:27:21 AM
// 
#endregion

using System;

namespace Gorgon.Input
{
	/// <summary>
	/// The types of available input devices.
	/// </summary>
	public enum InputDeviceType
	{
		/// <summary>
		/// Keyboard.
		/// </summary>
		Keyboard = 0,
		/// <summary>
		/// Pointing input device such as a mouse or trackball.
		/// </summary>
		Mouse = 1,
		/// <summary>
		/// Gaming input device such as a joystick or game pad.
		/// </summary>
		Joystick = 2,
		/// <summary>
		/// Human Interface Device.
		/// </summary>
		[Obsolete("We're not supplying this any more.")]
		HumanInterfaceDevice = 3
	}

	/// <summary>
	/// Contains information about an input device attached to or registered with the system.
	/// </summary>
	public interface IGorgonInputDeviceInfo2
	{
		/// <summary>
		/// Property to return a human readable description of the device.
		/// </summary>
		string Description
		{
			get;
		}

		/// <summary>
		/// Property to return the Human Interface Device path.
		/// </summary>
		/// <remarks>
		/// This serves as the unique identifier for the device.
		/// </remarks>
		string HumanInterfaceDevicePath
		{
			get;
		}

		/// <summary>
		/// Property to return the class name of the device.
		/// </summary>
		string ClassName
		{
			get;
		}

		/// <summary>
		/// Property to return the type of input device.
		/// </summary>
		InputDeviceType InputDeviceType
		{
			get;
		}
	}
}