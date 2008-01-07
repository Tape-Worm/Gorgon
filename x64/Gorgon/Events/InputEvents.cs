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
// Created: Thursday, October 05, 2006 3:57:58 PM
// 
#endregion

using System;
using SharpUtilities.Mathematics;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing mouse input event arguments.
	/// </summary>
	public class MouseInputEventArgs
		: EventArgs
	{
		#region Variables.
		private MouseButtons _button;			// Buttons that are pressed while the mouse is being moved.
		private MouseButtons _shiftButtons;		// Other buttons being held down.
		private Vector2D _position;				// Mouse position.
		private int _wheelPosition;				// Wheel position.
		private Vector2D _relative;				// Relative mouse position.
		private int _wheelDelta;				// Wheel delta.
		private int _clickCount;				// Number of clicks in a timed period.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return buttons that were pressed during mouse movement.
		/// </summary>
		public MouseButtons Buttons
		{
			get
			{
				return _button;
			}
		}

		/// <summary>
		/// Property to return the buttons that are being held down during the event.
		/// </summary>
		public MouseButtons ShiftButtons
		{
			get
			{
				return _shiftButtons;
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
		}

		/// <summary>
		/// Property to return the wheel position.
		/// </summary>
		public int WheelPosition
		{
			get
			{
				return _wheelPosition;
			}
		}

		/// <summary>
		/// Property to return the amount that the mouse has moved since it last moved.
		/// </summary>
		public Vector2D RelativePosition
		{
			get
			{
				return _relative;
			}
		}

		/// <summary>
		/// Property to return the amount that the wheel has moved since the last update.
		/// </summary>
		public int WheelDelta
		{
			get
			{
				return _wheelDelta;
			}
		}

		/// <summary>
		/// Property to return if we've double clicked.
		/// </summary>
		public bool DoubleClick
		{
			get
			{
				return (_clickCount > 1);
			}
		}

		/// <summary>
		/// Property to return the number of full clicks.
		/// </summary>
		public int ClickCount
		{
			get
			{
				return _clickCount;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="buttons">Buttons that are pressed during mouse event.</param>
		/// <param name="shiftButtons">Buttons that are held down during the mouse evnet.</param>
		/// <param name="position">Position of the mouse.</param>
		/// <param name="wheelPosition">Position of the wheel.</param>
		/// <param name="relativePosition">Relative position of the mouse.</param>
		/// <param name="wheelDelta">Relative position of the wheel.</param>
		/// <param name="clickCount">Number of clicks in a timed period.</param>
		public MouseInputEventArgs(MouseButtons buttons, MouseButtons shiftButtons, Vector2D position, int wheelPosition, Vector2D relativePosition, int wheelDelta, int clickCount)
		{
			_button = buttons;
			_shiftButtons = shiftButtons;
			_position = position;
			_wheelPosition = wheelPosition;
			_relative = relativePosition;
			_wheelDelta = wheelDelta;
			_clickCount = clickCount;
		}
		#endregion
	}

	/// <summary>
	/// Delegate used for sending a mouse input event.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void MouseInputEvent(object sender, MouseInputEventArgs e);

	/// <summary>
	/// Object representing keyboard input event arguments.
	/// </summary>
	public class KeyboardInputEventArgs
		: EventArgs
	{
		#region Variables.
		private KeyboardKeys _key;						// Key that is pressed.
		private KeyboardKeys _modifierKey;				// Other keys being held down.
		private int _scan = 0;							// Scan code information.
		private Keyboard.KeyCharMap _character;			// Character that the key represents.
		private bool _left = false;						// Indicates whether the key is the left key or not.
		private bool _right = false;					// Indicates whether the key is the right key or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the character that the key represents.
		/// </summary>
		public Keyboard.KeyCharMap CharacterMapping
		{
			get
			{
				return _character;
			}
		}

		/// <summary>
		/// Property to return key that is pressed.
		/// </summary>
		public KeyboardKeys Key
		{
			get
			{
				return _key;
			}
		}

		/// <summary>
		/// Property to return the keys that are being held down during the event.
		/// </summary>
		public KeyboardKeys ModifierKeys
		{
			get
			{
				return _modifierKey;
			}
		}

		/// <summary>
		/// Property to return if ALT is pressed or not.
		/// </summary>
		public bool Alt
		{
			get
			{
				return (_modifierKey & KeyboardKeys.Alt) != 0;
			}
		}

		/// <summary>
		/// Property to return if Ctrl is pressed or not.
		/// </summary>
		public bool Ctrl
		{
			get
			{
				return (_modifierKey & KeyboardKeys.Control) != 0;
			}
		}

		/// <summary>
		/// Property to return if Shift is pressed or not.
		/// </summary>
		public bool Shift
		{
			get
			{
				return (_modifierKey & KeyboardKeys.Shift) != 0;
			}
		}

		/// <summary>
		/// Property to return the scan code data.
		/// </summary>
		public int ScanCodeData
		{
			get
			{
				return _scan;
			}
		}

		/// <summary>
		/// Property to return whether a modifier key is on the left or not.
		/// </summary>
		public bool Left
		{
			get
			{
				return _left;
			}
		}

		/// <summary>
		/// Property to return whether a modifier key is on the right or not.
		/// </summary>
		public bool Right
		{
			get
			{
				return _right;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="key">Key that is pressed.</param>
		/// <param name="modifierKey">Keys that are held down during the event.</param>
		/// <param name="character">Character that the key represents.</param>
		/// <param name="scanData">Scan code data.</param>
		/// <param name="left">TRUE if the modifier is left, FALSE if not.</param>
		/// <param name="right">TRUE if the modifier is right, FALSE if not.</param>
		public KeyboardInputEventArgs(KeyboardKeys key, KeyboardKeys modifierKey, Keyboard.KeyCharMap character, int scanData, bool left, bool right)
		{
			_key = key;
			_modifierKey = modifierKey;
			_character = character;
			_scan = scanData;
			_left = left;
			_right = right;
		}
		#endregion
	}

	/// <summary>
	/// Delegate used for sending a keyboard input event.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void KeyboardInputEvent(object sender, KeyboardInputEventArgs e);
}