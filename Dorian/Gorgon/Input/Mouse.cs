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
// Created: Monday, October 02, 2006 12:32:57 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Forms = System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Internal.Native;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object that will represent mouse data.
	/// </summary>
	public abstract class Mouse
		: InputDevice
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private bool _cursorVisible = true;									// Is the mouse cursor visible?
		private Vector2D _doubleClickRange = new Vector2D(2.0f, 2.0f);		// Range that a double click is valid within.
		private Vector2D _position;											// Mouse horizontal and vertical position.
		private int _wheel;													// Mouse wheel position.
		private Vector2D _relativePosition = Vector2D.Zero;					// Mouse relative position.
		private RectangleF _positionConstraint;								// Constraints for the mouse position.
		private Point _wheelConstraint;										// Constraints for the mouse wheel.
		#endregion

		#region Events.
		/// <summary>
		/// Mouse moved event.
		/// </summary>
		public event MouseInputEvent MouseMove;

		/// <summary>
		/// Mouse button down event.
		/// </summary>
		public event MouseInputEvent MouseDown;

		/// <summary>
		/// Mouse button up event.
		/// </summary>
		public event MouseInputEvent MouseUp;

		/// <summary>
		/// Mouse wheel move event.
		/// </summary>
		public event MouseInputEvent MouseWheelMove;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the owner window client rectangle.
		/// </summary>
		protected virtual Rectangle WindowRectangle
		{
			get
			{
				return InputInterface.Window.ClientRectangle;
			}
		}

		/// <summary>
		/// Property to set or return the range in which a double click is valid (pixels).
		/// </summary>
		public Vector2D DoubleClickRange
		{
			get
			{
				return _doubleClickRange;
			}
			set
			{
				_doubleClickRange.X = Math.Abs(value.X);
				_doubleClickRange.Y = Math.Abs(value.Y);
			}
		}

		/// <summary>
		/// Property to set or return the double click delay in milliseconds.
		/// </summary>
		public int DoubleClickDelay
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the windows cursor is visible or not.
		/// </summary>
		public bool CursorVisible
		{
			get
			{
				return _cursorVisible;
			}
			set
			{
				if (value)
				{
					if (!_cursorVisible)
						Forms.Cursor.Show();
				}
				else
				{
					if (_cursorVisible)
						Forms.Cursor.Hide();
				}

				_cursorVisible = value;
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
		/// Property to set or return the mouse wheel range.
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
		/// Property to return the relative horizontal amount moved.
		/// </summary>
		public Vector2D RelativePosition
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
		/// Property to set or return the position of the mouse.
		/// </summary>
		public Vector2D Position
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
		/// Property to set or return the mouse wheel position.
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
		/// Property to return the mouse button(s) that are currently down.
		/// </summary>
		public MouseButtons Button
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function that will hide the cursor and rewind the cursor visibility stack.
		/// </summary>
		private void HideCursor()
		{
			int count = 0;

			count = Win32API.ShowCursor(false);

			if (count < 0)
				return;

			// Turn off the cursor.
			while (count >= 0)
				count = Win32API.ShowCursor(false);
		}

		/// <summary>
		/// Function to fire the mouse wheel move event.
		/// </summary>
		protected void OnMouseWheelMove()
		{
			if (MouseWheelMove != null)
			{
				ConstrainData();
				MouseInputEventArgs e = new MouseInputEventArgs(Button, MouseButtons.None, _position, _wheel, RelativePosition, WheelDelta, 0);
				MouseWheelMove(this, e);
			}
		}

		/// <summary>
		/// Function to fire the mouse move event.
		/// </summary>
		protected void OnMouseMove()
		{
			if (MouseMove != null)
			{
				ConstrainData();
				MouseInputEventArgs e = new MouseInputEventArgs(Button, MouseButtons.None, _position, _wheel, RelativePosition, WheelDelta, 0);
				MouseMove(this, e);
			}
		}

		/// <summary>
		/// Function to fire the mouse button down event.
		/// </summary>
		/// <param name="button">Button that's being pressed.</param>
		protected void OnMouseDown(MouseButtons button)
		{
			if (MouseDown != null)
			{
				ConstrainData();
				MouseInputEventArgs e = new MouseInputEventArgs(button, Button, _position, _wheel, RelativePosition, WheelDelta, 0);
				MouseDown(this, e);
			}
		}

		/// <summary>
		/// Function to fire the mouse button up event.
		/// </summary>
		/// <param name="button">Button that's being pressed.</param>
		/// <param name="clickCount">Number of full clicks in a timed period.</param>
		protected void OnMouseUp(MouseButtons button, int clickCount)
		{
			if (MouseUp != null)
			{
				ConstrainData();
				MouseInputEventArgs e = new MouseInputEventArgs(button, Button, _position, _wheel, RelativePosition, WheelDelta, clickCount);
				MouseUp(this, e);
			}
		}

		/// <summary>
		/// Function to perform clean up on the object.
		/// </summary>
		protected internal override void Dispose()
		{
			if (!_disposed)
			{
				if (!CursorVisible)
					System.Windows.Forms.Cursor.Show();
				_disposed = true;
			}

			base.Dispose();
		}

		/// <summary>
		/// Function to return whether a mouse click is a double click or not.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		/// <returns>TRUE if it is a double click, FALSE if not.</returns>
		protected abstract bool IsDoubleClick(MouseButtons button);

		/// <summary>
		/// Function to constrain the mouse data to the supplied ranges.
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
		/// Function to reset the buttons.
		/// </summary>
		public void ResetButtons()
		{
			Button = MouseButtons.None;
		}

		/// <summary>
		/// Function to reset the last mouse wheel delta value to zero.
		/// </summary>
		/// <remarks>This function is used to manually reset the last mouse wheel delta value.  This may be required if manually polling using the <see cref="GorgonLibrary.InputDevices.Mouse.WheelDelta">WheelDelta</see> property.</remarks>
		public void ResetWheelDelta()
		{
			WheelDelta = 0;
		}

		/// <summary>
		/// Function to set the mouse position.
		/// </summary>
		/// <param name="x">Horizontal coordinate.</param>
		/// <param name="y">Vertical coordinate.</param>
		public void SetPosition(float x, float y)
		{
			Position = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the double click validity range.
		/// </summary>
		/// <param name="x">Number of horizontal pixels.</param>
		/// <param name="y">Number of vertial pixels.</param>
		public void SetDoubleClickRange(float x, float y)
		{
			_doubleClickRange.X = Math.Abs(x);
			_doubleClickRange.Y = Math.Abs(y);
		}

		/// <summary>
		/// Function to set the mouse position range limits.
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
		/// Function to set the mouse wheel range limits.
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
		/// Constructor.
		/// </summary>
		/// <param name="owner">Input interface that owns this device.</param>
		protected internal Mouse(Input owner)
			: base(owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			DoubleClickDelay = 600;

			_position = new Vector2D(owner.Window.ClientRectangle.Width / 2, owner.Window.ClientRectangle.Height / 2);
			Button = MouseButtons.None;
			_positionConstraint = RectangleF.Empty;
			_wheelConstraint = Point.Empty;

			// Force the cursor visibility stack to 0 so we can force the mouse cursor to be visible.
			HideCursor();

			// Show the cursor.
			Forms.Cursor.Show();

			// Center the cursor.
			Forms.Cursor.Position = owner.Window.PointToScreen((Point)_position);
		}
		#endregion
	}
}
