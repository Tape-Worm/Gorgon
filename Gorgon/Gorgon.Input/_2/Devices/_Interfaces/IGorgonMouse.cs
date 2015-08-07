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
// Created: Tuesday, July 28, 2015 9:23:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;

namespace Gorgon.Input
{
	/// <summary>
	/// Enumeration for mouse buttons.
	/// </summary>
	[Flags]
	public enum MouseButtons
	{
		/// <summary>
		/// No pointing device button pressed.
		/// </summary>
		None = 0,
		/// <summary>
		/// Left pointing device button pressed.
		/// </summary>
		Left = 1,
		/// <summary>
		/// Right pointing device button pressed.
		/// </summary>
		Right = 2,
		/// <summary>
		/// Middle pointing device button pressed.
		/// </summary>
		Middle = 4,
		/// <summary>
		/// Left pointing device button pressed (same as <see cref="MouseButtons.Left"/>).
		/// </summary>
		Button1 = 1,
		/// <summary>
		/// Right pointing device button pressed (same as <see cref="MouseButtons.Right"/>).
		/// </summary>
		Button2 = 2,
		/// <summary>
		/// Middle pointing device button pressed (same as <see cref="MouseButtons.Middle"/>).
		/// </summary>
		Button3 = 4,
		/// <summary>
		/// Fourth pointing device button pressed.
		/// </summary>
		Button4 = 8,
		/// <summary>
		/// Fifth pointing device button pressed.
		/// </summary>
		Button5 = 16
	}

	/// <summary>
	/// Provides an interface to access a mouse (or other pointer) device.
	/// </summary>
	public interface IGorgonMouse
		: IGorgonInputDevice
	{
		#region Events.
		/// <summary>
		/// Event triggered when the mouse is moved over the client area of the <see cref="IGorgonInputDevice.Window"/>.
		/// </summary>
		event EventHandler<GorgonMouseEventArgs> MouseMove;

		/// <summary>
		/// Event triggered when a mouse button is held down on the client area of the <see cref="IGorgonInputDevice.Window"/>.
		/// </summary>
		event EventHandler<GorgonMouseEventArgs> MouseButtonDown;

		/// <summary>
		/// Event triggered when a mouse button is release from the client area of the <see cref="IGorgonInputDevice.Window"/>.
		/// </summary>
		event EventHandler<GorgonMouseEventArgs> MouseButtonUp;

		/// <summary>
		/// Event triggered when a mouse wheel (if present) is moved while the mouse is within the client area of the <see cref="IGorgonInputDevice.Window"/>.
		/// </summary>
		event EventHandler<GorgonMouseEventArgs> MouseWheelMove;

		/// <summary>
		/// Event triggered when a double click is performed on a mouse button.
		/// </summary>
		event EventHandler<GorgonMouseEventArgs> MouseDoubleClicked;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return information about this mouse.
		/// </summary>
		IGorgonMouseInfo2 Info
		{
			get;
		}

		/// <summary>
		/// Property to set or return the <see cref="Rectangle"/> used to constrain the mouse <see cref="Position"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will constrain the value of the <see cref="Position"/> within the specified <see cref="Rectangle"/>. This means that a cursor positioned at 320x200 with a region located at 330x210 with a width 
		/// and height of 160x160 will make the <see cref="Position"/> property return 330x210. If the cursor was positioned at 500x400, the <see cref="Position"/> property would return 480x360.
		/// </para>
		/// <para>
		/// Passing <see cref="Rectangle.Empty"/> to this property will remove the constraint on the position.
		/// </para>
		/// </remarks>
		Rectangle PositionConstraint
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the mouse.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the device has its <see cref="IGorgonInputDevice.IsExclusive"/> property set to <b>false</b>, then this method will move the mouse cursor to the specified position (relative to the client 
		/// area of the <see cref="IGorgonInputDevice.Window"/>). 
		/// </para>
		/// <para>
		/// The value returned by this property is affected by the <see cref="PositionConstraint"/> value.
		/// </para>
		/// </remarks>
		Point Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the pointing device wheel position.
		/// </summary>
		/// <remarks>
		/// The value returned by this property is affected by the <see cref="WheelConstraint"/> value.
		/// </remarks>
		int WheelPosition
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the <see cref="GorgonRange"/> used to constrain the mouse <see cref="WheelPosition"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If a mouse wheel exists on the device, this will constrain the value of the <see cref="WheelPosition"/> within the specified <see cref="GorgonRange"/>. This means that a wheel with a position of  
		/// 160, with a constraint of 180-190 will make the <see cref="WheelPosition"/> property return 180.
		/// </para>
		/// <para>
		/// Passing <see cref="GorgonRange.Empty"/> to this property will remove the constraint on the position.
		/// </para>
		/// </remarks>
		GorgonRange WheelConstraint
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the <see cref="Size"/> of the area, in pixels, surrounding the cursor that represents a valid double click area.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When this value is set, and a mouse button is double clicked, this value is checked to see if the mouse <see cref="Position"/> falls within -<c>value.</c><see cref="Size.Width"/> to <c>value.</c><see cref="Size.Width"/>, 
		/// and -<c>value.</c><see cref="Size.Height"/> to <c>value.</c><see cref="Size.Height"/> on the second click. If the <see cref="Position"/> is within this area, then the double click event will be triggered. Otherwise, it will 
		/// not.
		/// </para>
		/// <para>
		/// Passing <see cref="Size.Empty"/> to this property will disable double clicking.
		/// </para>
		/// </remarks>
		Size DoubleClickSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the delay between button clicks, in milliseconds, for a double click event.
		/// </summary>
		int DoubleClickDelay
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the windows cursor is visible or not.
		/// </summary>
		/// <remarks>
		/// This will change the visibility for the windows cursor for all mouse device objects.
		/// </remarks>
		bool CursorVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the pointing device button(s) that are currently down.
		/// </summary>
		MouseButtons Buttons
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function that will hide the cursor and rewind the cursor visibility stack.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will reset the cursor visibility to an initial state. This initial state is visible when the <see cref="IGorgonInputDevice.IsExclusive"/> property is <b>false</b>, or invisible when the 
		/// property is returns <b>true</b>.
		/// </para>
		/// <para>
		/// Because the <see cref="Cursor.Show"/> method does not return any value to indicate how deep the cursor stack might be, this can lead to problems when hiding the cursor (i.e. it won't be hidden). 
		/// This method will ensure that the cursor stack is reset to the proper stack level and will show/hide properly.
		/// </para>
		/// </remarks>
		void ResetCursor();

		/// <summary>
		/// Function to retrieve the relative position of the mouse.
		/// </summary>
		/// <param name="peek">[Optional] <b>true</b> to read the value without resetting it, <b>false</b> to reset the value after reading.</param>
		/// <returns>A <see cref="Point"/> containing the relative movement of the mouse since the last call of this method.</returns>
		/// <remarks>
		/// This will return the relative position of the mouse since the last time this method was called. When this method is called, and the <paramref name="peek"/> parameter is <b>false</b>, then it will 
		/// reset the relative position to 0, 0 and accumulate until this method is called again.
		/// </remarks>
		Point GetRelativePosition(bool peek = false);

		/// <summary>
		/// Function to retrieve the relative position of the mouse wheel.
		/// </summary>
		/// <param name="peek">[Optional] <b>true</b> to read the value without resetting it, <b>false</b> to reset the value after reading.</param>
		/// <returns>An <see cref="int"/> containing the relative movement of the mouse wheel since the last call of this method.</returns>
		/// <remarks>
		/// This will return the relative position of the mouse wheel, if one is present, since the last time this method was called. When this method is called, and the <paramref name="peek"/> parameter is 
		/// <b>false</b>, then it will reset the relative position to 0 and accumulate until this method is called again.
		/// </remarks>
		int GetRelativeWheelPosition(bool peek = false);
		#endregion
	}
}
