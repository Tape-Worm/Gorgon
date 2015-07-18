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
// Created: Saturday, July 11, 2015 9:32:45 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Collections;

namespace Gorgon.Input
{
	/// <summary>
	/// Provides access to various input devices attached to the system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An input service provides access to the various input devices (such as a keyboard, mouse, or gaming device) attached to the system. It provides data from the native input device data sources (e.g. 
	/// Raw input) and converts it into a form that Gorgon can interpret and use.
	/// </para>
	/// <para>
	/// Because this service does transformation of the input data, it will provide a consistent interface to accessing that data whether the input devices be accessed through regular windows events (WinForms), 
	/// Raw Input, XInput or another input wrapper. The interface to these devices will be seamless and as such, it won't be known, or even matter which input capture method is being used.
	/// </para>
	/// <para>
	/// Input services can pick and choose which device types they support. This means that one input service may support XBox controllers through XInput, and another will use the Windows Multimedia interface 
	/// to use other joystick types. And yet another may provide mouse/keyboard support only. 
	/// </para>
	/// <para>
	/// Users may enumerate the input devices present on the system via provided enumeration methods. These methods will return <see cref="IGorgonNamedObjectReadOnlyDictionary{T}"/> types that will contain 
	/// <see cref="IGorgonKeyboardInfo"/>, <see cref="IGorgonMouseInfo"/> or <see cref="IGorgonJoystickInfo"/> types giving information about each device. These values may be passed to the constructors of the  
	/// various input object types to allow selection of a specific device. This includes mice and keyboards. However, not all input services will allow the use of individual mice and keyboards and may only 
	/// allow use of the system devices (i.e. all input is directed from multiple devices into a single interface). If this is the case, the enumeration method will return only one item in its list.
	/// </para>
	/// <para>
	/// <note type="warning">
	/// <para>
	/// No input service is guaranteed to be thread safe, and none of the services provided by Gorgon are thread safe. Accessing a single <see cref="IGorgonInputService"/> instance via multiple threads is 
	/// highly discouraged and not supported.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// <note type="caution">
	/// <para>
	/// While multiple input services can co-exist, care should be taken not to instantiate the same type of input service more than once. Doing so may end up in undefined behaviour due to internal implementation 
	/// details that may conflict. For example, creating a <c>GorgonRawInputService</c> and a <c>GorgonXInputService</c> would be fine, but a <c>GorgonInputService</c> and a <c>GorgonInputService</c> would 
	/// cause problems due to how RAW Input works.
	/// </para> 
	/// </note>
	/// </para>
	/// </remarks>
	public interface IGorgonInputService
	{
		/// <summary>
		/// Function to enumerate the keyboards attached to the computer.
		/// </summary>
		/// <returns>A <see cref="IGorgonNamedObjectReadOnlyDictionary{T}"/> type containing <see cref="IGorgonKeyboardInfo"/> values with information about each keyboard.</returns>
		/// <remarks>
		/// <para>
		/// This will return information about each keyboard attached to the system. The <see cref="IGorgonKeyboardInfo"/> values contained within the returned list are used to create 
		/// <see cref="IGorgonKeyboard"/> devices.
		/// </para>
		/// <para>
		/// Not all input services will allow the use of the individual keyboard devices attached to the computer. Some will only allow the use of the system keyboard (i.e. all input from all keyboards 
		/// is directed into a single device object). If this is the case, this method will return a list with only one item.
		/// </para>
		/// </remarks>
		IReadOnlyList<IGorgonKeyboardInfo2> EnumerateKeyboards();

		/// <summary>
		/// Function to enumerate the mice (or other pointing devices) attached to the computer.
		/// </summary>
		/// <returns>A <see cref="IGorgonNamedObjectReadOnlyDictionary{T}"/> type containing <see cref="IGorgonMouseInfo"/> values with information about each mouse.</returns>
		/// <remarks>
		/// <para>
		/// This will return information about each mouse attached to the system. The <see cref="IGorgonMouseInfo"/> values contained within the returned list are used to create <see cref="IGorgonMouse"/> 
		/// devices.
		/// </para>
		/// <para>
		/// Not all input services will allow the use of the individual mice attached to the computer. Some will only allow the use of the system mouse (i.e. all input from all mice is directed into a 
		/// single device object). If this is the case, this method will return a list with only one item.
		/// </para>
		/// </remarks>
		IReadOnlyList<IGorgonMouseInfo2> EnumerateMice();

		/// <summary>
		/// Function to enumerate the joysticks (or other gaming devices such as game pads) attached to the computer.
		/// </summary>
		/// <returns>A <see cref="IGorgonNamedObjectReadOnlyDictionary{T}"/> type containing <see cref="IGorgonJoystickInfo"/> values with information about each joystick.</returns>
		/// <remarks>
		/// <para>
		/// This will return information about each joystick attached to the system. The <see cref="IGorgonJoystickInfo"/> values contained within the returned list are used to create <see cref="IGorgonJoystick"/> 
		/// devices.
		/// </para>
		/// </remarks>
		IReadOnlyList<IGorgonJoystickInfo2> EnumerateJoysticks();
	}
}
