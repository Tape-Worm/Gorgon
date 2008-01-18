#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, October 02, 2006 12:57:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Forms = System.Windows.Forms;
using SharpUtilities.Native.Win32;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Timing;

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
				_doubleClickPosition = _position;
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
			if (_doubleClicker.Milliseconds > _doubleClickDelay)
				return false;
			if ((Math.Abs(_position.X - _doubleClickPosition.X) > _doubleClickRange.X) || (Math.Abs(_position.Y - _doubleClickPosition.Y) > _doubleClickRange.Y))
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
			if ((_background) || (_exclusive))
				_device.Flags |= RawInputDeviceFlags.InputSink;

			// Enable exclusive access.
			if (_exclusive)
				_device.Flags |= RawInputDeviceFlags.CaptureMouse | RawInputDeviceFlags.NoLegacy;

			_device.WindowHandle = InputInterface.Window.Handle;
			
			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new InputCannotBindMouseException();

			_acquired = true;
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
				throw new InputCannotBindMouseException();

			_acquired = false;
		}

		/// <summary>
		/// Function to retrieve and parse the raw mouse data.
		/// </summary>
		/// <param name="mouseData">Data to examine.</param>
		public void GetRawData(RAWINPUTMOUSE mouseData)
		{
			// Do nothing if we're outside and we have exclusive mode turned off.
			if (!_exclusive)
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
						_position = InputInterface.Window.PointToClient(Forms.Cursor.Position);
					}
				}
			}

			_relativePosition.X = mouseData.LastX;
			_relativePosition.Y = mouseData.LastY;			

			// Get position.
			_position.X += mouseData.LastX;
			_position.Y += mouseData.LastY;

			// Get wheel data.
			if ((mouseData.ButtonFlags & RawMouseButtons.MouseWheel) != 0)
			{
				_relativeWheel = (int)((short)mouseData.ButtonData);
				_wheel += (short)mouseData.ButtonData;				
				OnMouseWheelMove();
			}

			// If we're outside of the delay, then restart double click cycle.
			if (_doubleClicker.Milliseconds > _doubleClickDelay)
			{
				_doubleClicker.Reset();
				_clickCount = 0;
			}

			// Get button data.
			if ((mouseData.ButtonFlags & RawMouseButtons.LeftDown) != 0)
			{
				BeginDoubleClick(MouseButtons.Left);
				OnMouseDown(MouseButtons.Left);
				_button |= MouseButtons.Left;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.RightDown) != 0)
			{
				BeginDoubleClick(MouseButtons.Right);
				OnMouseDown(MouseButtons.Right);
				_button |= MouseButtons.Right;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.MiddleDown) != 0)
			{
				BeginDoubleClick(MouseButtons.Middle);
				OnMouseDown(MouseButtons.Middle);
				_button |= MouseButtons.Middle;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.Button4Down) != 0)
			{
				BeginDoubleClick(MouseButtons.Button4);
				OnMouseDown(MouseButtons.Button4);
				_button |= MouseButtons.Button4;				
			}
			if ((mouseData.ButtonFlags & RawMouseButtons.Button5Down) != 0)
			{
				BeginDoubleClick(MouseButtons.Button5);
				OnMouseDown(MouseButtons.Button5);
				_button |= MouseButtons.Button5;				
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

				_button &= ~MouseButtons.Left;
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

				_button &= ~MouseButtons.Right;
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

				_button &= ~MouseButtons.Middle;
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

				_button &= ~MouseButtons.Button4;
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

				_button &= ~MouseButtons.Button5;
				OnMouseUp(MouseButtons.Button5, _clickCount);
			}

			// Fire events.
			if ((mouseData.LastX != 0) || (mouseData.LastY != 0))
				OnMouseMove();

			// Move the windows cursor to match if not exclusive.
			if (!_exclusive)
				Forms.Cursor.Position = InputInterface.Window.PointToScreen((Point)_position);
			else
				Forms.Cursor.Position = InputInterface.Window.PointToScreen((Point)Vector2D.Zero);
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
