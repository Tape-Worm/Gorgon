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
// Created: Monday, October 02, 2006 12:32:57 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Forms = System.Windows.Forms;
using GorgonLibrary;
using SharpUtilities.Native.Win32;
using SharpUtilities.Mathematics;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Object that will represent mouse data.
	/// </summary>
	public abstract class Mouse
		: InputDevice
	{
		#region Variables.
		private bool _cursorVisible = true;		// Is the mouse cursor visible?

		/// <summary>Double click delay in milliseconds.</summary>
		protected int _doubleClickDelay = 600;
		/// <summary>Range that a double click is valid within.</summary>
		protected Vector2D _doubleClickRange = new Vector2D(2.0f, 2.0f);
		/// <summary>Mouse horizontal and vertical position.</summary>
		protected Vector2D _position;
		/// <summary>Mouse wheel position.</summary>
		protected int _wheel;
		/// <summary>Mouse button flags.</summary>
		protected MouseButton _button;
		/// <summary>Mouse relative position.</summary>
		protected Vector2D _relativePosition;
		/// <summary>Mouse wheel delta.</summary>
		protected int _relativeWheel;
		/// <summary>Constraints for the mouse position.</summary>
		protected RectangleF _positionConstraint;
		/// <summary>Constraints for the mouse wheel.</summary>
		protected Point _wheelConstraint;
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
		/// Function to fire the mouse wheel move event.
		/// </summary>
		protected void OnMouseWheelMove()
		{
			if (MouseWheelMove != null)
			{
				MouseInputEventArgs e = new MouseInputEventArgs(MouseButton.None, _button, _position, _wheel, _relativePosition, _relativeWheel, 0);
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
				MouseInputEventArgs e = new MouseInputEventArgs(MouseButton.None, _button, _position, _wheel, _relativePosition, _relativeWheel, 0);
				MouseMove(this, e);
			}
		}

		/// <summary>
		/// Function to fire the mouse button down event.
		/// </summary>
		/// <param name="button">Button that's being pressed.</param>
		protected void OnMouseDown(MouseButton button)
		{
			if (MouseDown != null)
			{
				MouseInputEventArgs e = new MouseInputEventArgs(button, _button, _position, _wheel, _relativePosition, _relativeWheel, 0);
				MouseDown(this, e);
			}
		}
		
		/// <summary>
		/// Function to fire the mouse button up event.
		/// </summary>
		/// <param name="button">Button that's being pressed.</param>
		/// <param name="clickCount">Number of full clicks in a timed period.</param>
		protected void OnMouseUp(MouseButton button, int clickCount)
		{
			if (MouseUp != null)
			{
				MouseInputEventArgs e = new MouseInputEventArgs(button, _button, _position, _wheel, _relativePosition, _relativeWheel, clickCount);
				MouseUp(this, e);
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
					Forms.Cursor.Hide();

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
				if (value)
					CursorVisible = false;
				
				base.Exclusive = value;
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
			get
			{
				return _relativeWheel;
			}
		}

		/// <summary>
		/// Property to return the relative horizontal amount moved.
		/// </summary>
		public Vector2D RelativePosition
		{
			get
			{
				return _relativePosition;
			}
		}

		/// <summary>
		/// Property to return the position of the mouse.
		/// </summary>
		public Vector2D Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				ConstrainData();
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
		public MouseButton Button
		{
			get
			{
				return _button;
			}
			set
			{
				_button = value;
			}
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
		/// Function to return whether a mouse click is a double click or not.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		/// <returns>TRUE if it is a double click, FALSE if not.</returns>
		protected abstract bool IsDoubleClick(MouseButton button);

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
				if (_position.X > _positionConstraint.Right)
					_position.X = _positionConstraint.Right;
				if (_position.Y > _positionConstraint.Bottom)
					_position.Y = _positionConstraint.Bottom;
			}

			if (_wheelConstraint != Point.Empty)
			{
				// Limit wheel.
				if (_wheel < _wheelConstraint.X)
					_wheel = _wheelConstraint.X;
				if (_wheel > _wheelConstraint.Y)
					_wheel = _wheelConstraint.Y;
			}
		}

		/// <summary>
		/// Function to set the mouse position.
		/// </summary>
		/// <param name="x">Horizontal coordinate.</param>
		/// <param name="y">Vertical coordinate.</param>
		public void SetPosition(float x, float y)
		{
			_position.X = x;
			_position.Y = y;
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
		protected internal Mouse(InputDevices owner)
			: base(owner)
		{
			_position = new Vector2D(WindowRectangle.Width / 2, WindowRectangle.Height / 2);
			_wheel = 0;
			_button = MouseButton.None;
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
