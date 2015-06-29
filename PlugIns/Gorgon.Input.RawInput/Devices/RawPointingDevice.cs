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
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Mouse interface.
	/// </summary>
	internal class RawPointingDevice
		: GorgonPointingDevice
	{
		#region Variables.
		private readonly MessageFilter _messageFilter;			// Window message filter.
		private RAWINPUTDEVICE _device;					        // Input device.
		private bool _outside;							        // Outside of window?
		private readonly IGorgonTimer _doubleClicker;		    // Double click timer.
		private int _clickCount;							    // Click counter.		
		private PointF _doubleClickPosition;				    // Double click position.
		private PointingDeviceButtons _doubleClickButton;	    // Button that was double clicked.
		private readonly IntPtr _deviceHandle;					// Device handle.
		private bool _isExclusive;								// Flag to indicate that the pointing device is placed in exclusive mode.
		private bool _isBound;									// Flag to indicate that the device is bound.
		private PointF _lastPosition;							// The last position of the cursor before it was put into exclusive mode.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the cursor on the screen if applicable.
		/// </summary>
		private void UpdateCursorPosition()
		{
		    // If the window is disposed, then do nothing.
		    if (BoundControl == null)
		    {
		        return;
		    }

		    // Move the windows cursor to match if not exclusive.
			Cursor.Position =
				BoundControl.PointToScreen(!Exclusive
					                           ? Point.Truncate(Position)
					                           : new Point(BoundControl.ClientSize.Width / 2, BoundControl.ClientSize.Height / 2));
		}

	    /// <summary>
		/// Function to begin a double click.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		private void BeginDoubleClick(PointingDeviceButtons button)
		{
		    if (_clickCount >= 1)
		    {
			    return;
		    }

		    _doubleClickPosition = Position;
		    _doubleClickButton = button;
		    _doubleClicker.Reset();
		}

		/// <summary>
		/// Function to return whether a pointing device click is a double click or not.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		/// <returns><b>true</b> if it is a double click, <b>false</b> if not.</returns>
		private bool IsDoubleClick(PointingDeviceButtons button)
		{
		    if ((button != _doubleClickButton)
				|| (_doubleClicker.Milliseconds > DoubleClickDelay))
		    {
		        return false;
		    }

			return (!((Position.X - _doubleClickPosition.X).Abs() > DoubleClickRange.X)) &&
			       (!((Position.Y - _doubleClickPosition.Y).Abs() > DoubleClickRange.Y));
		}

		/// <summary>
		/// Handles the MouseLeave event of the bound control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundControl_MouseLeave(object sender, EventArgs e)
		{
			// If we're not exclusive and we leave the control, we should reset
			// the button status or else the button(s) will remain in a pressed
			// state upon re-entry.  Regardless of whether a button is physically
			// pressed or not.
			ResetButtons();
		}

        /// <summary>
		/// Function to unbind the device from a window.
		/// </summary>
		protected override void UnbindWindow()
		{
			base.UnbindWindow();

			if ((BoundControl != null) && (!BoundControl.Disposing) && (!BoundControl.IsDisposed))
			{
				BoundControl.MouseLeave -= BoundControl_MouseLeave;
			}
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			if ((Exclusive) && (!_isExclusive))
			{
				_lastPosition = Cursor.Position;
				UpdateCursorPosition();
				_isExclusive = true;
			}

			if ((!Exclusive) && (_isExclusive))
			{
				_isExclusive = false;
				Position = BoundControl.PointToClient(Point.Truncate(_lastPosition));
			}

			if (_isBound)
			{
				return;
			}

            UnbindDevice();

			if (_messageFilter != null)
			{
				_messageFilter.RawInputPointingDeviceData += GetRawData;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Mouse;
            _device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = BoundControl.Handle;

			// Enable background access.
		    if (AllowBackground)
		    {
				_device.WindowHandle = BoundControl.Handle;
		        _device.Flags |= RawInputDeviceFlags.InputSink;
		    }

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_BIND_POINTING_DEVICE);
		    }

			if (!Exclusive)
			{
				BoundControl.MouseLeave += BoundControl_MouseLeave;
			}

			_isBound = true;
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			BoundControl.MouseLeave -= BoundControl_MouseLeave;

			if (_messageFilter != null)
			{
				_messageFilter.RawInputPointingDeviceData -= GetRawData;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Mouse;
			_device.Flags = RawInputDeviceFlags.Remove;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_UNBIND_POINTING_DEVICE);
		    }

			_isBound = false;
		}

		/// <summary>
		/// Function to retrieve and parse the raw pointing device data.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event data to examine.</param>
		private void GetRawData(object sender, RawInputPointingDeviceEventArgs e)
		{
		    if ((BoundControl == null) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    if ((_deviceHandle != IntPtr.Zero) && (_deviceHandle != e.Handle))
		    {
		        return;
		    }

		    if ((Exclusive) && (!Acquired))
			{
			    // Attempt to recapture.
			    if (BoundControl.Focused)
			    {
			        Acquired = true;
			    }
			    else
			    {
			        return;
			    }
			}

			// Do nothing if we're outside and we have exclusive mode turned off.
			if (!Exclusive)
			{
				Point clientPosition = BoundControl.PointToClient(Cursor.Position);

			    if (!WindowRectangle.Contains(clientPosition)) 
				{
					_outside = true;
					return;
				}

			    if (_outside) 
			    {
			        // If we're back inside place position at the entry point.
			        _outside = false;
			        Position = clientPosition;
			    }
			}

		    // Get wheel data.
		    if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.MouseWheel) != 0)
		    {
		        OnPointingDeviceWheelMove((short)e.PointingDeviceData.ButtonData);
		    }

		    // If we're outside of the delay, then restart double click cycle.
			if (_doubleClicker.Milliseconds > DoubleClickDelay)
			{
				_doubleClicker.Reset();
				_clickCount = 0;
			}

			// Get button data.
			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.LeftDown) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Left);
				OnPointingDeviceDown(PointingDeviceButtons.Left);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.RightDown) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Right);
				OnPointingDeviceDown(PointingDeviceButtons.Right);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.MiddleDown) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Middle);
				OnPointingDeviceDown(PointingDeviceButtons.Middle);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.Button4Down) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Button4);
				OnPointingDeviceDown(PointingDeviceButtons.Button4);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.Button5Down) != 0)
			{
				BeginDoubleClick(PointingDeviceButtons.Button5);
				OnPointingDeviceDown(PointingDeviceButtons.Button5);
			}

			// If we have an 'up' event on the buttons, remove the flag.
			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.LeftUp) != 0)
			{
			    if (IsDoubleClick(PointingDeviceButtons.Left))
			    {
			        _clickCount += 1;
			    }
			    else
			    {
			        _doubleClicker.Reset();
			        _clickCount = 0;
			    }

			    OnPointingDeviceUp(PointingDeviceButtons.Left, _clickCount);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.RightUp) != 0)
			{
			    if (IsDoubleClick(PointingDeviceButtons.Right))
			    {
			        _clickCount += 1;
			    }
			    else
			    {
			        _doubleClicker.Reset();
			        _clickCount = 0;
			    }

			    OnPointingDeviceUp(PointingDeviceButtons.Right, _clickCount);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.MiddleUp) != 0)
			{
			    if (IsDoubleClick(PointingDeviceButtons.Middle))
			    {
			        _clickCount += 1;
			    }
			    else
			    {
			        _doubleClicker.Reset();
			        _clickCount = 0;
			    }

			    OnPointingDeviceUp(PointingDeviceButtons.Middle, _clickCount);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.Button4Up) != 0)
			{
			    if (IsDoubleClick(PointingDeviceButtons.Button4))
			    {
			        _clickCount += 1;
			    }
			    else
			    {
			        _doubleClicker.Reset();
			        _clickCount = 0;
			    }

			    OnPointingDeviceUp(PointingDeviceButtons.Button4, _clickCount);
			}

			if ((e.PointingDeviceData.ButtonFlags & RawMouseButtons.Button5Up) != 0)
			{
			    if (IsDoubleClick(PointingDeviceButtons.Button5))
			    {
			        _clickCount += 1;
			    }
			    else
			    {
			        _doubleClicker.Reset();
			        _clickCount = 0;
			    }

			    OnPointingDeviceUp(PointingDeviceButtons.Button5, _clickCount);
			}

			// Fire events.
			RelativePosition = new PointF(RelativePosition.X + e.PointingDeviceData.LastX, RelativePosition.Y + e.PointingDeviceData.LastY);
			OnPointingDeviceMove(new PointF(Position.X + e.PointingDeviceData.LastX, Position.Y + e.PointingDeviceData.LastY),
				                    false);
			UpdateCursorPosition();
		}

        /// <summary>
        /// Function to reset the buttons.
        /// </summary>
	    public override void ResetButtons()
	    {
	        base.ResetButtons();

            // Ensure that the double clicking functionality is turned off.
            _doubleClickButton = PointingDeviceButtons.None;
            _doubleClicker.Reset();
            _clickCount = 0;
	    }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawPointingDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Device name.</param>
		/// <param name="handle">The handle to the device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		internal RawPointingDevice(GorgonRawInputService owner, string deviceName, IntPtr handle)
			: base(owner, deviceName)
		{
			GorgonApplication.Log.Print("Raw input pointing device interface created for handle 0x{0}.", LoggingLevel.Verbose, handle.FormatHex());
			_deviceHandle = handle;
			if (GorgonTimerQpc.SupportsQpc())
			{
				_doubleClicker = new GorgonTimerQpc();
			}
			else
			{
				_doubleClicker = new GorgonTimerMultimedia();
			}
			_doubleClicker.Reset();
			_messageFilter = owner.MessageFilter;
		}
		#endregion
	}
}
