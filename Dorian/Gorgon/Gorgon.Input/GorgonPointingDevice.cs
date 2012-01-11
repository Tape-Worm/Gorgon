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
// Created: Friday, June 24, 2011 10:04:22 AM
// 
#endregion

using System;
using System.Drawing;
using Forms = System.Windows.Forms;
using GorgonLibrary.Math;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Enumeration for pointing device buttons.
	/// </summary>
	[Flags()]
	public enum PointingDeviceButtons
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
		/// Left pointing device button pressed (same as PointingDeviceButtons.Left).
		/// </summary>
		Button1 = 1,
		/// <summary>
		/// Right pointing device button pressed (same as PointingDeviceButtons.Right).
		/// </summary>
		Button2 = 2,
		/// <summary>
		/// Middle pointing device button pressed (same as PointingDeviceButtons.Middle).
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
	/// A pointing device interface.
	/// </summary>
	/// <remarks>A pointing device can be any type of device.  For instance a trackball may be considered a pointing device.</remarks>
	public abstract class GorgonPointingDevice
		: GorgonInputDevice
	{
		#region Variables.
		private bool _disposed = false;												// Flag to indicate that the object was disposed.
		private bool _cursorHidden = false;											// Is the pointing device cursor visible?
		private GorgonVector2 _doubleClickRange = new GorgonVector2(2.0f, 2.0f);				// Range that a double click is valid within.
		private GorgonVector2 _position;													// Mouse horizontal and vertical position.
		private int _wheel;															// Mouse wheel position.
		private GorgonVector2 _relativePosition = GorgonVector2.Zero;							// Mouse relative position.
		private RectangleF _positionConstraint;										// Constraints for the pointing device position.
		private Point _wheelConstraint;												// Constraints for the pointing device wheel.
		private int _doubleClickDelay = 0;											// Double click delay in milliseconds.
		#endregion

		#region Events.
		/// <summary>
		/// Pointing device moved event.
		/// </summary>
		public event EventHandler<PointingDeviceEventArgs> PointingDeviceMove;

		/// <summary>
		/// Pointing device button down event.
		/// </summary>
		public event EventHandler<PointingDeviceEventArgs> PointingDeviceDown;

		/// <summary>
		/// Pointing device button up event.
		/// </summary>
		public event EventHandler<PointingDeviceEventArgs> PointingDeviceUp;

		/// <summary>
		/// Pointing device wheel move event.
		/// </summary>
		public event EventHandler<PointingDeviceEventArgs> PointingDeviceWheelMove;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the owner window client rectangle.
		/// </summary>
		protected virtual Rectangle WindowRectangle
		{
			get
			{
				if (BoundControl == null)
					return Rectangle.Empty;

				return new Rectangle(Point.Empty, BoundControl.ClientSize);
			}
		}

		/// <summary>
		/// Property to set or return the range in which a double click is valid (pixels).
		/// </summary>
		public GorgonVector2 DoubleClickRange
		{
			get
			{
				return _doubleClickRange;
			}
			set
			{
				SetDoubleClickRange(System.Math.Abs(value.X), System.Math.Abs(value.Y));
			}
		}

		/// <summary>
		/// Property to set or return the double click delay in milliseconds.
		/// </summary>
		public int DoubleClickDelay
		{
			get
			{
				return _doubleClickDelay;
			}
			set
			{
				_doubleClickDelay = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the windows cursor is visible or not.
		/// </summary>
		public bool CursorVisible
		{
			get
			{
				return !_cursorHidden;
			}
			set
			{
				if (value)
					ShowCursor();
				else
					HideCursor();
			}
		}

		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.
		/// </summary>
		public override bool Exclusive
		{
			get
			{
				return base.Exclusive;
			}
			set
			{
				base.Exclusive = value;
				if (value)
					CursorVisible = false;
			}
		}

		/// <summary>
		/// Property to set or return the position range.
		/// </summary>
		public RectangleF PositionRange
		{
			get
			{
				return _positionConstraint;
			}
			set
			{
				_positionConstraint = value;
				ConstrainData();
			}
		}

		/// <summary>
		/// Property to set or return the pointing device wheel range.
		/// </summary>
		public Point WheelRange
		{
			get
			{
				return _wheelConstraint;
			}
			set
			{
				_wheelConstraint = value;
				ConstrainData();
			}
		}

		/// <summary>
		/// Property to return the relative wheel delta.
		/// </summary>
		public int WheelDelta
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the relative amount moved.
		/// </summary>
		public GorgonVector2 RelativePosition
		{
			get
			{
				ConstrainData();
				return _relativePosition;
			}
			protected set
			{
				_relativePosition = value;
			}
		}

		/// <summary>
		/// Property to set or return the position of the pointing device.
		/// </summary>
		public GorgonVector2 Position
		{
			get
			{
				ConstrainData();
				return _position;
			}
			set
			{
				_position = value;
			}
		}

		/// <summary>
		/// Property to set or return the pointing device wheel position.
		/// </summary>
		public int Wheel
		{
			get
			{
				return _wheel;
			}
			set
			{
				_wheel = value;
				ConstrainData();
			}
		}

		/// <summary>
		/// Property to return the pointing device button(s) that are currently down.
		/// </summary>
		public PointingDeviceButtons Button
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function that will hide the cursor and rewind the cursor visibility stack.
		/// </summary>
		protected abstract void ResetCursor();

		/// <summary>
		/// Function called when the device is bound to a window.
		/// </summary>
		/// <param name="window">Window that was bound.</param>
		protected override void OnWindowBound(Forms.Control window)
		{
			window.MouseLeave += new EventHandler(Owner_MouseLeave);
		}

		/// <summary>
		/// Handles the MouseLeave event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Owner_MouseLeave(object sender, EventArgs e)
		{
			// If we're not exclusive and we leave the control, we should reset
			// the button status or else the button(s) will remain in a pressed
			// state upon re-entry.  Regardless of whether a button is physically
			// pressed or not.
			if (!Exclusive)
				ResetButtons();
		}

		/// <summary>
		/// Function to fire the pointing device wheel move event.
		/// </summary>
		protected void OnPointingDeviceWheelMove(int newDelta)
		{
			if (newDelta != WheelDelta)
			{
				WheelDelta = newDelta;
				Wheel += newDelta;
			}
			else
			{
				WheelDelta = 0;
				return;
			}

			if (PointingDeviceWheelMove != null)
			{
				ConstrainData();
				PointingDeviceEventArgs e = new PointingDeviceEventArgs(Button, PointingDeviceButtons.None, _position, _wheel, RelativePosition, WheelDelta, 0);
				PointingDeviceWheelMove(this, e);
			}
		}

		/// <summary>
		/// Function to fire the pointing device move event.
		/// </summary>
		/// <param name="newPosition">New position for the pointing device.</param>
		/// <param name="setRelative">TRUE to calculate the relative motion of the device, FALSE to have the plug-in set it.</param>
		/// <remarks>Some plug-ins provide their own relative position data, which is likely to be more accurate, so we can tell the library to not calculate in that instance.</remarks>
		protected void OnPointingDeviceMove(GorgonVector2 newPosition, bool setRelative)
		{
			RelativePosition = GorgonVector2.Zero;

			if (newPosition != _position)
			{
				if (setRelative)
					RelativePosition = GorgonVector2.Subtract(newPosition, _position);
				_position = newPosition;
			}
			else
			{
				if (setRelative)
					RelativePosition = GorgonVector2.Zero;
				return;
			}

			if (PointingDeviceMove != null)
			{
				ConstrainData();
				PointingDeviceEventArgs e = new PointingDeviceEventArgs(Button, PointingDeviceButtons.None, _position, _wheel, RelativePosition, WheelDelta, 0);
				PointingDeviceMove(this, e);
			}
		}

		/// <summary>
		/// Function to fire the pointing device button down event.
		/// </summary>
		/// <param name="button">Button that's being pressed.</param>
		protected void OnPointingDeviceDown(PointingDeviceButtons button)
		{
			if (PointingDeviceDown != null)
			{
				ConstrainData();
				PointingDeviceEventArgs e = new PointingDeviceEventArgs(button, Button, _position, _wheel, RelativePosition, WheelDelta, 0);
				Button |= button;
				PointingDeviceDown(this, e);			
			}
		}

		/// <summary>
		/// Function to fire the pointing device button up event.
		/// </summary>
		/// <param name="button">Button that's being pressed.</param>
		/// <param name="clickCount">Number of full clicks in a timed period.</param>
		protected void OnPointingDeviceUp(PointingDeviceButtons button, int clickCount)
		{
			if (PointingDeviceUp != null)
			{
				ConstrainData();
				Button &= ~button;
				PointingDeviceEventArgs e = new PointingDeviceEventArgs(button, Button, _position, _wheel, RelativePosition, WheelDelta, clickCount);
				PointingDeviceUp(this, e);
			}
		}

		/// <summary>
		/// Function to unbind the device from a window.
		/// </summary>
		protected override void UnbindWindow()
		{
			base.UnbindWindow();
			if ((BoundControl != null) && (!BoundControl.Disposing) && (!BoundControl.IsDisposed))
				BoundControl.MouseLeave -= new EventHandler(Owner_MouseLeave);
		}

		/// <summary>
		/// Function to perform clean up on the object.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (!CursorVisible)
						ShowCursor();
				}
				_disposed = true;
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to constrain the pointing device data to the supplied ranges.
		/// </summary>
		protected virtual void ConstrainData()
		{
			if (_positionConstraint != RectangleF.Empty)
			{
				// Limit positioning.
				if (_position.X < _positionConstraint.X)
					_position.X = _positionConstraint.X;
				if (_position.Y < _positionConstraint.Y)
					_position.Y = _positionConstraint.Y;
				if (_position.X > _positionConstraint.Right - 1)
					_position.X = _positionConstraint.Right - 1;
				if (_position.Y > _positionConstraint.Bottom - 1)
					_position.Y = _positionConstraint.Bottom - 1;
			}

			if (_wheelConstraint != Point.Empty)
			{
				// Limit wheel.
				if (_wheel < _wheelConstraint.X)
					_wheel = _wheelConstraint.X;
				if (_wheel > _wheelConstraint.Y - 1)
					_wheel = _wheelConstraint.Y - 1;
			}
		}

		/// <summary>
		/// Function to show the pointing device cursor.
		/// </summary>
		public void ShowCursor()
		{
			if (_cursorHidden)
				Forms.Cursor.Show();
			_cursorHidden = false;
		}

		/// <summary>
		/// Function to hide the pointing device cursor.
		/// </summary>
		public void HideCursor()
		{
			if (!_cursorHidden)
				Forms.Cursor.Hide();
			_cursorHidden = true;
		}

		/// <summary>
		/// Function to reset the buttons.
		/// </summary>
		public void ResetButtons()
		{
			Button = PointingDeviceButtons.None;
		}

		/// <summary>
		/// Function to set the pointing device position.
		/// </summary>
		/// <param name="x">Horizontal coordinate.</param>
		/// <param name="y">Vertical coordinate.</param>
		public void SetPosition(float x, float y)
		{
			Position = new PointF(x, y);
		}

		/// <summary>
		/// Function to set the double click validity range.
		/// </summary>
		/// <param name="x">Number of horizontal pixels.</param>
		/// <param name="y">Number of vertial pixels.</param>
		public void SetDoubleClickRange(float x, float y)
		{
			_doubleClickRange.X = System.Math.Abs(x);
			_doubleClickRange.Y = System.Math.Abs(y);
		}

		/// <summary>
		/// Function to set the pointing device position range limits.
		/// </summary>
		/// <param name="left">Left limit.</param>
		/// <param name="top">Top limit.</param>
		/// <param name="width">Width limit.</param>
		/// <param name="height">Height limit.</param>
		public void SetPositionRange(float left, float top, float width, float height)
		{
			if ((width == 0) || (height == 0))
				PositionRange = RectangleF.Empty;
			else
				PositionRange = new RectangleF(left, top, width, height);
		}

		/// <summary>
		/// Function to set the pointing device wheel range limits.
		/// </summary>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		public void SetWheelRange(int min, int max)
		{
			if (min == max)
				WheelRange = Point.Empty;
			else
			{
				// Swap values if necessary.
				if (min > max)
				{
					int temp = min;
					min = max;
					max = temp;
				}
				WheelRange = new Point(min, max);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointingDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the input device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		protected GorgonPointingDevice(GorgonInputFactory owner, string deviceName, Forms.Control boundWindow)
			: base(owner, deviceName, boundWindow)
		{
			_position = new PointF(BoundControl.ClientSize.Width / 2, BoundControl.ClientSize.Height / 2);
			Button = PointingDeviceButtons.None;
			_positionConstraint = RectangleF.Empty;
			_wheelConstraint = Point.Empty;

			ResetCursor();
			ShowCursor();

			Forms.Cursor.Position = Point.Truncate(_position);

			DoubleClickDelay = Forms.SystemInformation.DoubleClickTime;
			DoubleClickRange = new PointF(Forms.SystemInformation.DoubleClickSize.Width, Forms.SystemInformation.DoubleClickSize.Height);
		}
		#endregion
	}
}
