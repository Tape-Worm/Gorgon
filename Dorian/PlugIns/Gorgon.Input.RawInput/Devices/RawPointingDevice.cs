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
// Created: Friday, June 24, 2011 10:04:33 AM
// 
#endregion

using System;
using System.Drawing;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
using GorgonLibrary.Math;
using Forms = System.Windows.Forms;
using GorgonLibrary.Input;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Mouse interface.
	/// </summary>
	internal class RawPointingDevice
		: GorgonPointingDevice
	{
		#region Variables.
		private MessageFilter _messageFilter = null;			// Window message filter.
		private RAWINPUTDEVICE _device;							// Input device.
		private bool _outside = false;							// Outside of window?
		private GorgonTimer _doubleClicker;						// Double click timer.
		private int _clickCount = 0;							// Click counter.		
		private PointF _doubleClickPosition;					// Double click position.
		private PointingDeviceButtons _doubleClickButton;		// Button that was double clicked.
		private IntPtr _deviceHandle = IntPtr.Zero;				// Device handle.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the cursor on the screen if applicable.
		/// </summary>
		/// <remarks>This method is optional depending on whether you wish to update the actual cursor on the screen.  Plug-ins wishing to skip this behaviour should override this method and put in an empty stub.</remarks>
		private void UpdateCursorPosition()
		{
			// If the window is disposed, then do nothing.
			if (BoundControl != null)
			{
				// Move the windows cursor to match if not exclusive.
				if (!Exclusive)
					Forms.Cursor.Position = BoundControl.PointToScreen(Point.Truncate(Position));
				else
					Forms.Cursor.Position = BoundControl.PointToScreen(new Point(0, 0));
			}
		}
		
		/// <summary>
		/// Function to begin a double click.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		private void BeginDoubleClick(PointingDeviceButtons button)
		{
			if (_clickCount < 1)
			{
				_doubleClickPosition = Position;
				_doubleClickButton = button;
				_doubleClicker.Reset();
			}
		}

		/// <summary>
		/// Function to return whether a pointing device click is a double click or not.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		/// <returns>TRUE if it is a double click, FALSE if not.</returns>
		private bool IsDoubleClick(PointingDeviceButtons button)
		{
			if (button != _doubleClickButton)
				return false;
			if (_doubleClicker.Milliseconds > DoubleClickDelay)
				return false;
			if ((System.Math.Abs(Position.X - _doubleClickPosition.X) > DoubleClickRange.X) || (System.Math.Abs(Position.Y - _doubleClickPosition.Y) > DoubleClickRange.Y))
				return false;

			return true;
		}
		
		/// <summary>
		/// Function that will hide the cursor and rewind the cursor visibility stack.
		/// </summary>
		protected override void ResetCursor()
		{
			int count = 0;

			count = Win32API.ShowCursor(false);

			// Turn off the cursor.
			while (count >= 0)
				count = Win32API.ShowCursor(false);

			Win32API.ShowCursor(true);			
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= new EventHandler<RawInputEventArgs>(GetRawData);
				System.Windows.Forms.Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
			}

			_messageFilter = new MessageFilter();
			_messageFilter.RawInputData += new EventHandler<RawInputEventArgs>(GetRawData);
			System.Windows.Forms.Application.AddMessageFilter(_messageFilter);

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Mouse;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
			if ((AllowBackground) || (Exclusive))
				_device.Flags |= RawInputDeviceFlags.InputSink;

			// Enable exclusive access.
			if (Exclusive)
				_device.Flags |= RawInputDeviceFlags.CaptureMouse | RawInputDeviceFlags.NoLegacy;

			_device.WindowHandle = BoundControl.Handle;
						
			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonResult.DriverError, "Failed to bind the pointing device.");
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= new EventHandler<RawInputEventArgs>(GetRawData);
				System.Windows.Forms.Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
				_messageFilter = null;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Mouse;
			_device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonResult.DriverError, "Failed to unbind the pointing device.");
		}

		/// <summary>
		/// Function to retrieve and parse the raw pointing device data.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event data to examine.</param>
		private void GetRawData(object sender, RawInputEventArgs e)
		{
			if ((BoundControl == null) || (BoundControl.Disposing))
				return;
			
			if ((e.Data.Header.Type != RawInputType.Mouse) || ((_deviceHandle != IntPtr.Zero) && (_deviceHandle != e.Handle)))
				return;

			if ((Exclusive) && (!Acquired))
			{
			    // Attempt to recapture.
			    if (BoundControl.Focused)
			        Acquired = true;
			    else
			        return;
			}

			// Do nothing if we're outside and we have exclusive mode turned off.
			if (!Exclusive)
			{
				if (!WindowRectangle.Contains(BoundControl.PointToClient(Forms.Cursor.Position))) 
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
						Position = BoundControl.PointToClient(Forms.Cursor.Position);
					}
				}
			}

			// Get wheel data.
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.MouseWheel) != 0)
				OnPointingDeviceWheelMove((int)((short)e.Data.Mouse.ButtonData));

			// If we're outside of the delay, then restart double click cycle.
			if (_doubleClicker.Milliseconds > DoubleClickDelay)
			{
				_doubleClicker.Reset();
				_clickCount = 0;
			}

			// Get button data.
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.LeftDown) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Left);
				OnPointingDeviceDown(PointingDeviceButtons.Left);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.RightDown) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Right);
				OnPointingDeviceDown(PointingDeviceButtons.Right);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.MiddleDown) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Middle);
				OnPointingDeviceDown(PointingDeviceButtons.Middle);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.Button4Down) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Button4);
				OnPointingDeviceDown(PointingDeviceButtons.Button4);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.Button5Down) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Button5);
				OnPointingDeviceDown(PointingDeviceButtons.Button5);
			}

			// If we have an 'up' event on the buttons, remove the flag.
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.LeftUp) != 0)
			{
				if (IsDoubleClick(PointingDeviceButtons.Left))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				OnPointingDeviceUp(PointingDeviceButtons.Left, _clickCount);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.RightUp) != 0)
			{
				if (IsDoubleClick(PointingDeviceButtons.Right))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				OnPointingDeviceUp(PointingDeviceButtons.Right, _clickCount);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.MiddleUp) != 0)
			{
				if (IsDoubleClick(PointingDeviceButtons.Middle))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				OnPointingDeviceUp(PointingDeviceButtons.Middle, _clickCount);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.Button4Up) != 0)
			{
				if (IsDoubleClick(PointingDeviceButtons.Button4))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				OnPointingDeviceUp(PointingDeviceButtons.Button4, _clickCount);
			}
			if ((e.Data.Mouse.ButtonFlags & RawMouseButtons.Button5Up) != 0)
			{
				if (IsDoubleClick(PointingDeviceButtons.Button5))
					_clickCount += 1;
				else
				{
					_doubleClicker.Reset();
					_clickCount = 0;
				}

				OnPointingDeviceUp(PointingDeviceButtons.Button5, _clickCount);
			}

			// Fire events.
			RelativePosition = new PointF(e.Data.Mouse.LastX, e.Data.Mouse.LastY);
			OnPointingDeviceMove(GorgonVector2.Add(Position, new GorgonVector2(e.Data.Mouse.LastX, e.Data.Mouse.LastY)), false);
			UpdateCursorPosition();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawPointingDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="handle">The handle to the device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		internal RawPointingDevice(GorgonRawInputFactory owner, IntPtr handle, Forms.Control boundWindow)
			: base(owner, "Raw Input Mouse", boundWindow)
		{
			Gorgon.Log.Print("Raw input pointing device interface created for handle 0x{0}.", GorgonLoggingLevel.Verbose, handle.FormatHex());
			_deviceHandle = handle;
			_doubleClicker = new GorgonTimer();
			_doubleClicker.Reset();
		}
		#endregion
	}
}
