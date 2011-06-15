#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Monday, October 02, 2006 12:57:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Forms = System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.InputDevices.Internal;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing mouse data.
	/// </summary>
	public class GorgonMouse
		: Mouse
	{
		#region Variables.
		private RAWINPUTDEVICE _device;			// Input device.
		private bool _outside = false;			// Outside of window?
		private PreciseTimer _doubleClicker;	// Double click timer.
		private int _clickCount = 0;			// Click counter.		
		private PointF _doubleClickPosition;	// Double click position.
		private MouseButtons _doubleClickButton;	// Button that was double clicked.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to begin a double click.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		private void BeginDoubleClick(MouseButtons button)
		{
			if (_clickCount < 1)
			{
				_doubleClickPosition = Position;
				_doubleClickButton = button;
				_doubleClicker.Reset();
			}
		}

		/// <summary>
		/// Function to return whether a mouse click is a double click or not.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		/// <returns>TRUE if it is a double click, FALSE if not.</returns>
		protected override bool IsDoubleClick(MouseButtons button)
		{
			if (button != _doubleClickButton)
				return false;
			if (_doubleClicker.Milliseconds > DoubleClickDelay)
				return false;
			if ((Math.Abs(Position.X - _doubleClickPosition.X) > DoubleClickRange.X) || (Math.Abs(Position.Y - _doubleClickPosition.Y) > DoubleClickRange.Y))
				return false;

			return true;
		}		

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = HIDUsage.Mouse;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
			if ((AllowBackground) || (Exclusive))
				_device.Flags |= RawInputDeviceFlags.InputSink;

			// Enable exclusive access.
			if (Exclusive)
				_device.Flags |= RawInputDeviceFlags.CaptureMouse | RawInputDeviceFlags.NoLegacy;

			_device.WindowHandle = InputInterface.Window.Handle;
			
			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonErrors.CannotBindInputDevice, "Failed to bind the mouse device.");
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = HIDUsage.Mouse;
			_device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonErrors.CannotBindInputDevice, "Failed to bind the mouse device.");
		}

		/// <summary>
		/// Function to retrieve and parse the raw mouse data.
		/// </summary>
		/// <param name="mouseData">Data to examine.</param>
		internal void GetRawData(RAWINPUTMOUSE mouseData)
		{
			// Do nothing if we're outside and we have exclusive mode turned off.
			if (!Exclusive)
			{
				if (!WindowRectangle.Contains(InputInterface.Window.PointToClient(Forms.Cursor.Position))) 
				{
					_outside = true;
					return;
				}
				else
				{
					if (_outside) 
					{
						// If we're back inside place position at the entry point.
						_outside = false;
						Position = InputInterface.Window.PointToClient(Forms.Cursor.Position);
					}
				}
			}

			RelativePosition = new Vector2D(mouseData.LastX, mouseData.LastY);

			// Get position.
			Position = Vector2D.Add(Position, new Vector2D(mouseData.LastX, mouseData.LastY));

			// Reset the last mouse wheel delta value.
			ResetWheelDelta();

			// Get wheel data.			
			if ((mouseData.ButtonFlags & RawMouseButtons.MouseWheel) != 0)
			{
				WheelDelta = (int)((short)mouseData.ButtonData);
				Wheel += (short)mouseData.ButtonData;				
				OnMouseWheelMove();
			}

			// If we're outside of the delay, then restart double click cycle.
			if (_doubleClicker.Milliseconds > DoubleClickDelay)
			{
				_doubleClicker.Reset();
				_clickCount = 0;
			}

			// Get button data.
			if ((mouseData.ButtonFlags & RawMouseButtons.LeftDown) != 0)
			{
				BeginDoubleClick(MouseButtons.Left);
				OnMouseDown(MouseButtons.Left);
				Button |= MouseButtons.Left;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.RightDown) != 0)
			{
				BeginDoubleClick(MouseButtons.Right);
				OnMouseDown(MouseButtons.Right);
				Button |= MouseButtons.Right;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.MiddleDown) != 0)
			{
				BeginDoubleClick(MouseButtons.Middle);
				OnMouseDown(MouseButtons.Middle);
				Button |= MouseButtons.Middle;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.Button4Down) != 0)
			{
				BeginDoubleClick(MouseButtons.Button4);
				OnMouseDown(MouseButtons.Button4);
				Button |= MouseButtons.Button4;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.Button5Down) != 0)
			{
				BeginDoubleClick(MouseButtons.Button5);
				OnMouseDown(MouseButtons.Button5);
				Button |= MouseButtons.Button5;				
			}

			// If we have an 'up' event on the buttons, remove the flag.
			if ((mouseData.ButtonFlags & RawMouseButtons.LeftUp) != 0)
			{
				if (IsDoubleClick(MouseButtons.Left))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				Button &= ~MouseButtons.Left;
				OnMouseUp(MouseButtons.Left, _clickCount);
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.RightUp) != 0)
			{
				if (IsDoubleClick(MouseButtons.Right))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				Button &= ~MouseButtons.Right;
				OnMouseUp(MouseButtons.Right, _clickCount);
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.MiddleUp) != 0)
			{
				if (IsDoubleClick(MouseButtons.Middle))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				Button &= ~MouseButtons.Middle;
				OnMouseUp(MouseButtons.Middle, _clickCount);
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.Button4Up) != 0)
			{
				if (IsDoubleClick(MouseButtons.Button4))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				Button &= ~MouseButtons.Button4;
				OnMouseUp(MouseButtons.Button4, _clickCount);
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.Button5Up) != 0)
			{
				if (IsDoubleClick(MouseButtons.Button5))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				Button &= ~MouseButtons.Button5;
				OnMouseUp(MouseButtons.Button5, _clickCount);
			}

			// Fire events.
			if ((mouseData.LastX != 0) || (mouseData.LastY != 0))
				OnMouseMove();

			// If the window is disposed, then do nothing.
			if (InputInterface.Window != null)
			{
				// Move the windows cursor to match if not exclusive.
				if (!Exclusive)
					Forms.Cursor.Position = InputInterface.Window.PointToScreen((Point)Position);
				else
					Forms.Cursor.Position = InputInterface.Window.PointToScreen((Point)Vector2D.Zero);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Input device interface that owns this object.</param>
		internal GorgonMouse(Input owner)
			: base(owner)
		{
			_doubleClicker = new PreciseTimer();
			_doubleClicker.Reset();
		}
		#endregion
	}
}
