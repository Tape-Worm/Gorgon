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
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Timing;

namespace Gorgon.Input
{
	/// <summary>
	/// A mouse interface.
	/// </summary>
	/// <remarks>A mouse can be any type of pointing device.  For instance a trackball will be considered a mouse by Gorgon.</remarks>
	public sealed class GorgonMouse
		: GorgonInputDevice2, IGorgonMouse, IGorgonDeviceRouting<GorgonMouseData>
	{
		#region Variables.
		// Is the pointing device cursor visible?
		private static int _cursorHidden;                                   
		// Range that a double click is valid within.
		private Size _doubleClickSize;
		// Mouse horizontal and vertical position.
		private Point _position;
		// Mouse wheel position.
		private int _wheel;                                                 
		// Constraints for the pointing device position.
		private Rectangle _positionConstraint;
		// Constraints for the pointing device wheel.
		private GorgonRange _wheelConstraint;
		// The last known cursor position before this object takes over control of it.
		private Point _lastCursorPosition = Cursor.Position;
		// The delay, in milliseconds, between clicks for a double click event.
		private int _doubleClickDelay;
		// The relative position of the mouse since it was last checked.
		private Point _relativePosition;
		// The relative position of the mouse wheel since it was last checked.
		private int _wheelDelta;
		// The cursor was outside of the window.
		private bool _wasOutside;
		// The number of times a button was fully clicked.
		private int _clickCount;
		// The recorded position for a double click.
		private Point _doubleClickPosition;
		// The button used for a double click.
		private MouseButtons _doubleClickButton;
		// The timer used for a double click.
		private readonly IGorgonTimer _doubleClickTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerQpc() : new GorgonTimerMultimedia();
		// Flag to indicate that the device has never been acquired.
		private bool _firstAcquisition = true;
		// The absolute position of the wheel.
		private int _wheelPosition;
		#endregion

		#region Events.
		/// <inheritdoc/>
		public event EventHandler<GorgonMouseEventArgs> MouseMove;

		/// <inheritdoc/>
		public event EventHandler<GorgonMouseEventArgs> MouseButtonDown;

		/// <inheritdoc/>
		public event EventHandler<GorgonMouseEventArgs> MouseButtonUp;

		/// <inheritdoc/>
		public event EventHandler<GorgonMouseEventArgs> MouseWheelMove;

		/// <inheritdoc/>
		public event EventHandler<GorgonMouseEventArgs> MouseDoubleClicked;
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public override bool IsPolled => false;
		
		/// <inheritdoc/>
		public int DoubleClickDelay
		{
			get
			{
				return _doubleClickDelay;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				_doubleClickDelay = value;
			}
		}

		/// <inheritdoc/>
		public bool CursorVisible
		{
			get
			{
				return _cursorHidden == 0;
			}
			set
			{
				int visibleValue = value ? 0 : -1;

				// Do nothing if the cursor was already shown or hidden.
				// This will keep the stack count at 1 when showing, or 0 when hiding.
				if (Interlocked.Exchange(ref _cursorHidden, visibleValue) == visibleValue)
				{
					return;
				}

				ShowMouseCursor(value);
			}
		}

		/// <inheritdoc/>
		public Rectangle PositionConstraint
		{
			get
			{
				return _positionConstraint;
			}
			set
			{
				_positionConstraint = value;
				ConstrainPositionData(ref _position);
			}
		}

		/// <inheritdoc/>
		public GorgonRange WheelConstraint
		{
			get
			{
				return _wheelConstraint;
			}
			set
			{
				_wheelConstraint = value;
				ConstrainWheelData();
			}
		}

		/// <inheritdoc/>
		public Size DoubleClickSize
		{
			get
			{
				return _doubleClickSize;
			}
			set
			{
				_doubleClickSize = new Size(value.Width.Abs(), value.Height.Abs());
			}
		}

		/// <inheritdoc/>
		public Point Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;

				ConstrainPositionData(ref _position);

				if (Window == null)
				{
					return;
				}

				Cursor.Position = IsExclusive ? Window.PointToScreen(new Point(Window.ClientSize.Width / 2, Window.ClientSize.Height / 2))
					: Cursor.Position = Window.PointToScreen(_position);
			}
		}

		/// <inheritdoc/>
		public int WheelPosition
		{
			get
			{
				return _wheelPosition;
			}
			set
			{
				_wheelPosition = value;
				ConstrainWheelData();
			}
		}

		/// <inheritdoc/>
		public MouseButtons Buttons
		{
			get;
			set;
		}

		/// <inheritdoc/>
		public IGorgonMouseInfo2 Info
		{
			get;
		}

		/// <inheritdoc/>
		InputDeviceType IGorgonDeviceRouting<GorgonMouseData>.DeviceType => InputDeviceType.Mouse;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to show the mouse cursor or hide it.
		/// </summary>
		/// <param name="show"><b>true</b> to show the mouse cursor, <b>false</b> to hide it.</param>
		private static void ShowMouseCursor(bool show)
		{
			CursorInfoFlags isVisible = Win32Api.IsCursorVisible();

			// If the cursor is suppressed, then we're using a touch interface.
			// So we'll not acknowledge requests to show the cursor in that case.
			if ((isVisible == CursorInfoFlags.Suppressed)
			    || ((isVisible == CursorInfoFlags.CursorShowing) && (show))
			    || ((isVisible == CursorInfoFlags.CursorHidden) && (!show)))
			{
				return;
			}

			if (show)
			{
				while (Win32Api.ShowCursor(true) < 0)
				{
				}
				return;
			}

			while (Win32Api.ShowCursor(false) > -1)
			{
			}
		}

		/// <summary>
		/// Function to constrain the mouse position data to the supplied ranges.
		/// </summary>
		private void ConstrainPositionData(ref Point position)
		{
			if (_positionConstraint.IsEmpty)
			{
				if (!Cursor.Clip.IsEmpty)
				{
					Cursor.Clip = Rectangle.Empty;
				}
				return;
			}

			// Limit positioning.
			if (position.X < _positionConstraint.X)
			{
				position.X = _positionConstraint.X;
			}

			if (position.Y < _positionConstraint.Y)
			{
				position.Y = _positionConstraint.Y;
			}

			if (position.X > _positionConstraint.Right)
			{
				position.X = _positionConstraint.Right;
			}

			if (position.Y > _positionConstraint.Bottom)
			{
				position.Y = _positionConstraint.Bottom;
			}

			if (Window == null)
			{
				Cursor.Clip = Rectangle.Empty;
				return;
			}

			if (IsExclusive)
			{
				if (Cursor.Clip.IsEmpty)
				{
					Cursor.Clip = Rectangle.Empty;
				}
				return;
			}

			Rectangle screenRect = Window.RectangleToScreen(_positionConstraint);

			if (Cursor.Clip != screenRect)
			{
				Cursor.Clip = screenRect;
			}
		}

		/// <summary>
		/// Function to constrain the mouse wheel data to the supplied range.
		/// </summary>
		private void ConstrainWheelData()
		{
			if (_wheelConstraint == GorgonRange.Empty)
			{
				return;
			}

			// Limit wheel.
			if (_wheel < _wheelConstraint.Minimum)
			{
				_wheel = _wheelConstraint.Minimum;
			}

			if (_wheel > _wheelConstraint.Maximum)
			{
				_wheel = _wheelConstraint.Maximum;
			}
		}

		/// <inheritdoc/>
		protected override void OnAcquiredStateChanged()
		{
			_relativePosition = Point.Empty;
			_wheelDelta = 0;
			_clickCount = 0;
			_doubleClickPosition = Point.Empty;
			_doubleClickTimer.Reset();
			Buttons = MouseButtons.None;

			if (!IsAcquired)
			{
				ShowMouseCursor(true);
				Cursor.Clip = Rectangle.Empty;

				if ((IsExclusive) && (_cursorHidden == 0))
				{
					Cursor.Position = _lastCursorPosition;
				}

				return;
			}

			if (!IsExclusive)
			{
				// If we've acquired the mouse, and the cursor visibility flag is on, then turn on the cursor and 
				// update the position to the current position.
				if (_cursorHidden == 0)
				{
					ShowMouseCursor(true);
				}

				Point clientPos = Window.PointToClient(Cursor.Position);

				if (!_firstAcquisition)
				{
					if (!Window.ClientRectangle.Contains(clientPos))
					{
						return;
					}

					ConstrainPositionData(ref _position);
					Cursor.Position = Window.PointToScreen(_position);
				}
				else
				{
					_firstAcquisition = false;
					Position = clientPos;
				}

				return;
			}

			// Set the position to empty when switching into exclusive mode.
			Position = Point.Empty;
			_lastCursorPosition = Cursor.Position;

			// Force the cursor to the upper left hand corner if we're hiding the cursor in exclusive mode.
			Cursor.Position = Window.PointToScreen(new Point(Window.ClientSize.Width / 2, Window.ClientSize.Height / 2));

			// If the cursor was not hidden while in exclusive mode, then keep it visible.
			if (_cursorHidden != 0)
			{
				return;
			}

			_cursorHidden = 0;
			ShowMouseCursor(false);
		}

		/// <inheritdoc/>
		public void ResetCursor()
		{
			// Turn off the cursor.
			ShowMouseCursor(false);

			// If we're in exclusive mode, then keep the cursor hidden, otherwise show it.
			if (!IsExclusive)
			{
				ShowMouseCursor(true);
				_cursorHidden = 0;
			}
			else
			{
				_cursorHidden = -1;
			}
		}

		/// <inheritdoc/>
		public Point GetRelativePosition(bool peek = false)
		{
			Point result = _relativePosition;

			if (!peek)
			{
				_relativePosition = Point.Empty;
			}

			return result;
		}

		/// <inheritdoc/>
		public int GetRelativeWheelPosition(bool peek = false)
		{
			int result = _wheelDelta;

			if (!peek)
			{
				_wheelDelta = 0;
			}

			return result;
		}

		/// <summary>
		/// Function to initiate a double click.
		/// </summary>
		/// <param name="button">The button that was clicked.</param>
		private void BeginDoubleClick(MouseButtons button)
		{
			if ((_doubleClickButton != MouseButtons.None) && (button != _doubleClickButton))
			{
				_doubleClickTimer.Reset();
				_clickCount = 0;
				return;
			}

			if (_clickCount > 0)
			{
				return;
			}
			
			_doubleClickTimer.Reset();
			_doubleClickPosition = Position;
			_doubleClickButton = button;
		}


		/// <summary>
		/// Function to handle mouse down button events.
		/// </summary>
		/// <param name="buttonState">Button state to evaluate.</param>
		/// <returns>The current button that was held down.</returns>
		private MouseButtons HandleButtonDownEvents(MouseButtonState buttonState)
		{
			MouseButtons button = MouseButtons.None;

			if (_doubleClickTimer.Milliseconds > DoubleClickDelay)
			{
				_doubleClickTimer.Reset();
				_clickCount = 0;
			}

			if ((buttonState & MouseButtonState.ButtonLeftDown) == MouseButtonState.ButtonLeftDown)
			{
				button = MouseButtons.Left;
				BeginDoubleClick(button);
			}

			if ((buttonState & MouseButtonState.ButtonRightDown) == MouseButtonState.ButtonRightDown)
			{
				button = MouseButtons.Right;
				BeginDoubleClick(button);
			}

			if ((buttonState & MouseButtonState.ButtonMiddleDown) == MouseButtonState.ButtonMiddleDown)
			{
				button = MouseButtons.Middle;
				BeginDoubleClick(button);
			}

			if ((buttonState & MouseButtonState.Button4Down) == MouseButtonState.Button4Down)
			{
				button = MouseButtons.Button4;
				BeginDoubleClick(button);
			}

			if ((buttonState & MouseButtonState.Button5Down) == MouseButtonState.Button5Down)
			{
				button = MouseButtons.Button5;
				BeginDoubleClick(button);
			}

			Buttons |= button;

			return button;
		}

		/// <summary>
		/// Function to handle mouse up button events.
		/// </summary>
		/// <param name="buttonState">Button state to evaluate.</param>
		/// <returns>The current button that was released.</returns>
		private MouseButtons HandleButtonUpEvents(MouseButtonState buttonState)
		{
			MouseButtons button = MouseButtons.None;

			if ((buttonState & MouseButtonState.ButtonLeftUp) == MouseButtonState.ButtonLeftUp)
			{
				button = MouseButtons.Left;
			}

			if ((buttonState & MouseButtonState.ButtonRightUp) == MouseButtonState.ButtonRightUp)
			{
				button = MouseButtons.Right;
			}

			if ((buttonState & MouseButtonState.ButtonMiddleUp) == MouseButtonState.ButtonMiddleUp)
			{
				button = MouseButtons.Middle;
			}

			if ((buttonState & MouseButtonState.Button4Up) == MouseButtonState.Button4Up)
			{
				button = MouseButtons.Button4;
			}

			if ((buttonState & MouseButtonState.Button5Up) == MouseButtonState.Button5Up)
			{
				button = MouseButtons.Button5;
			}

			// If no button was released, then exit.
			if (button == MouseButtons.None)
			{
				return button;
			}

			Rectangle doubleClickArea = new Rectangle(_doubleClickPosition.X - DoubleClickSize.Width / 2,
			                                          _doubleClickPosition.Y - DoubleClickSize.Height / 2,
			                                          DoubleClickSize.Width,
			                                          DoubleClickSize.Height);

			if ((!doubleClickArea.Contains(Position)) || (_doubleClickButton != button) || (_doubleClickTimer.Milliseconds > DoubleClickDelay))
			{
				_doubleClickTimer.Reset();
				_clickCount = 0;
			}
			else
			{
				++_clickCount;
			}

			Buttons &= ~button;

			return button;
		}

		/// <summary>
		/// Function to handle a mouse movement event.
		/// </summary>
		/// <param name="mouseWheelDelta">The delta indicating the direction and amount that the mouse wheel moved by.</param>
		private void HandleMouseWheelMove(short mouseWheelDelta)
		{
			if (mouseWheelDelta == 0)
			{
				return;
			}

			_wheelDelta += mouseWheelDelta;
			WheelPosition += mouseWheelDelta;
		}

		/// <summary>
		/// Function to handle the mouse movement event.
		/// </summary>
		/// <param name="x">The last relative horizontal position for the mouse.</param>
		/// <param name="y">The last relative vertical position for the mouse.</param>
		/// <param name="relative"><b>true</b> if the mouse movement is relative, <b>false</b> if absolute.</param>
		/// <returns><b>true</b> if the mouse moved, <b>false</b> if not.</returns>
		private bool HandleMouseMove(int x, int y, bool relative)
		{
			Point newPosition;

			if (relative)
			{
				_relativePosition = new Point(_relativePosition.X + x, _relativePosition.Y + y);
				newPosition = new Point(_position.X + x, _position.Y + y);
			}
			else
			{
				_relativePosition = new Point(_relativePosition.X + (x - _position.X), _relativePosition.Y + (y - _position.Y));
				newPosition = new Point(x, y);
			}

			ConstrainPositionData(ref newPosition);

			if ((newPosition.X == _position.X) && (newPosition.Y == _position.Y))
			{
				return false;
			}

			Position = newPosition;

			return true;
		}

		/// <inheritdoc/>
		bool IGorgonDeviceRouting<GorgonMouseData>.ParseData(ref GorgonMouseData data)
		{
			if ((!IsAcquired) || (Window == null) || (Window.Disposing) || (Window.IsDisposed))
			{
				return false;
			}

			// If the mouse cursor was outside of the window, and the mouse is not exclusive, then 
			// flag it as outside. Once it returns to the window, reset the position to match the
			// current cursor position so we don't get weirdness. This only applies to plug ins that 
			// can monitor the mouse position outside of the client area of the window (e.g. raw input).
			if (!IsExclusive)
			{
				Point clientPosition = Window.PointToClient(Cursor.Position);

				if (!Window.ClientRectangle.Contains(clientPosition))
				{
					_wasOutside = true;
					return true;
				}

				// If we were previously outside of the window, reposition at the point of entry.
				if (_wasOutside)
				{
					Position = clientPosition;
					_wasOutside = false;
				}
			}

			// Gather the event information.
			MouseButtons downButtons = MouseButtons.None;
			MouseButtons upButtons = MouseButtons.None;
			
			bool wasMoved = HandleMouseMove(data.Position.X, data.Position.Y, data.IsRelative);
			
			if ((Info.HasMouseWheel) && (data.MouseWheelDelta != 0))
			{
				HandleMouseWheelMove(data.MouseWheelDelta);
			}

			// If there's a button event, then process it.
			if (data.ButtonState != MouseButtonState.None)
			{
				downButtons = HandleButtonDownEvents(data.ButtonState);
				upButtons = HandleButtonUpEvents(data.ButtonState);
			}

			// Trigger button events.
			if (downButtons != MouseButtons.None)
			{
				MouseButtonDown?.Invoke(this, new GorgonMouseEventArgs(downButtons, Buttons, _position, _wheelPosition, _relativePosition, _wheelDelta, _clickCount));
			}

			if (upButtons != MouseButtons.None)
			{
				var e = new GorgonMouseEventArgs(upButtons, Buttons, _position, _wheelPosition, _relativePosition, _wheelDelta, _clickCount);

				MouseButtonUp?.Invoke(this, e);

				if ((_clickCount > 0) && ((_clickCount % 2) == 0))
				{
					MouseDoubleClicked?.Invoke(this, e);
				}
			}

			// Trigger move events.
			if (data.MouseWheelDelta != 0)
			{
				MouseWheelMove?.Invoke(this, new GorgonMouseEventArgs(Buttons, MouseButtons.None, _position, _wheelPosition, _relativePosition, _wheelDelta, 0));
			}

			if (wasMoved)
			{
				MouseMove?.Invoke(this, new GorgonMouseEventArgs(Buttons, MouseButtons.None, _position, _wheelPosition, _relativePosition, _wheelDelta, 0));
			}

			return true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMouse"/> class.
		/// </summary>
		/// <param name="service">The input service that this device is registered with.</param>
		/// <param name="mouseInfo">Information about which mouse to use.</param>
		/// <param name="log">[Optional] The logging interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="service"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		public GorgonMouse(IGorgonInputService service, IGorgonMouseInfo2 mouseInfo, IGorgonLog log = null)
			: base(service, mouseInfo, log)
		{
			Info = mouseInfo;
			_position = Point.Empty;
			Buttons = MouseButtons.None;
			PositionConstraint = Rectangle.Empty;
			WheelConstraint = GorgonRange.Empty;

			DoubleClickDelay = SystemInformation.DoubleClickTime;
			DoubleClickSize = SystemInformation.DoubleClickSize;
		}
		#endregion
	}
}
