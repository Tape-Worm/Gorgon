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
	/// Object that will represent keyboard data.
	/// </summary>
	public abstract class Keyboard
		: InputDevice
	{
		#region Value types.
		/// <summary>
		/// Value type containing the code and text character mapping.
		/// </summary>
		public struct KeyCharMap
		{
			#region Variables.
			/// <summary>Key that the character represents.</summary>
			KeyboardKeys Key;
			/// <summary>Character representation.</summary>
			public char Character;
			/// <summary>Character representation with shift modifier.</summary>
			public char Shifted;
			#endregion

			#region Constructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="key">Key that the character represents.</param>
			/// <param name="character">Character representation.</param>
			/// <param name="shifted">Character representation with shift modifier.</param>
			public KeyCharMap(KeyboardKeys key, string character, string shifted)
			{
				Key = key;
				Character = Convert.ToChar(character);
				Shifted = Convert.ToChar(shifted);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private SortedList<KeyboardKeys, KeyCharMap> _keyMap = null;		// Key->character mapping.

		/// <summary>Array to hold all the key presses.</summary>
		protected bool[] _keys = new bool[1024];
		#endregion

		#region Events.
		/// <summary>
		/// Key down event.
		/// </summary>
		public event KeyboardInputEvent KeyDown;

		/// <summary>
		/// Key up event.
		/// </summary>
		public event KeyboardInputEvent KeyUp;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the keyboard to character mappings.
		/// </summary>
		public SortedList<KeyboardKeys, KeyCharMap> KeyMappings
		{
			get
			{
				return _keyMap;
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
			}
		}

		/// <summary>
		/// Property to return whether a key has been pressed or not.
		/// </summary>
		/// <param name="key">Key that has been pressed.</param>
		public virtual bool this[KeyboardKeys key]
		{
			get
			{
				return _keys[(int)key];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve character mappings for keys.
		/// </summary>
		public void GetDefaultKeyMapping()
		{
			// Add default key mapping.
			_keyMap.Clear();
			_keyMap.Add(KeyboardKeys.Tab, new KeyCharMap(KeyboardKeys.Tab, "\t", "\t"));
			_keyMap.Add(KeyboardKeys.Return, new KeyCharMap(KeyboardKeys.Return, "\n", "\n"));
			_keyMap.Add(KeyboardKeys.Space, new KeyCharMap(KeyboardKeys.Space, " ", " "));
			_keyMap.Add(KeyboardKeys.D0, new KeyCharMap(KeyboardKeys.D0, "0", ")"));
			_keyMap.Add(KeyboardKeys.D1, new KeyCharMap(KeyboardKeys.D1, "1", "!"));
			_keyMap.Add(KeyboardKeys.D2, new KeyCharMap(KeyboardKeys.D2, "2", "@"));
			_keyMap.Add(KeyboardKeys.D3, new KeyCharMap(KeyboardKeys.D3, "3", "#"));
			_keyMap.Add(KeyboardKeys.D4, new KeyCharMap(KeyboardKeys.D4, "4", "$"));
			_keyMap.Add(KeyboardKeys.D5, new KeyCharMap(KeyboardKeys.D5, "5", "%"));
			_keyMap.Add(KeyboardKeys.D6, new KeyCharMap(KeyboardKeys.D6, "6", "^"));
			_keyMap.Add(KeyboardKeys.D7, new KeyCharMap(KeyboardKeys.D7, "7", "&"));
			_keyMap.Add(KeyboardKeys.D8, new KeyCharMap(KeyboardKeys.D8, "8", "*"));
			_keyMap.Add(KeyboardKeys.D9, new KeyCharMap(KeyboardKeys.D9, "9", "("));
			_keyMap.Add(KeyboardKeys.A, new KeyCharMap(KeyboardKeys.A, "a", "A"));
			_keyMap.Add(KeyboardKeys.B, new KeyCharMap(KeyboardKeys.B, "b", "B"));
			_keyMap.Add(KeyboardKeys.C, new KeyCharMap(KeyboardKeys.C, "c", "C"));
			_keyMap.Add(KeyboardKeys.D, new KeyCharMap(KeyboardKeys.D, "d", "D"));
			_keyMap.Add(KeyboardKeys.E, new KeyCharMap(KeyboardKeys.E, "e", "E"));
			_keyMap.Add(KeyboardKeys.F, new KeyCharMap(KeyboardKeys.F, "f", "F"));
			_keyMap.Add(KeyboardKeys.G, new KeyCharMap(KeyboardKeys.G, "g", "G"));
			_keyMap.Add(KeyboardKeys.H, new KeyCharMap(KeyboardKeys.H, "h", "H"));
			_keyMap.Add(KeyboardKeys.I, new KeyCharMap(KeyboardKeys.I, "i", "I"));
			_keyMap.Add(KeyboardKeys.J, new KeyCharMap(KeyboardKeys.J, "j", "J"));
			_keyMap.Add(KeyboardKeys.K, new KeyCharMap(KeyboardKeys.K, "k", "K"));
			_keyMap.Add(KeyboardKeys.L, new KeyCharMap(KeyboardKeys.L, "l", "L"));
			_keyMap.Add(KeyboardKeys.M, new KeyCharMap(KeyboardKeys.M, "m", "M"));
			_keyMap.Add(KeyboardKeys.N, new KeyCharMap(KeyboardKeys.N, "n", "N"));
			_keyMap.Add(KeyboardKeys.O, new KeyCharMap(KeyboardKeys.O, "o", "O"));
			_keyMap.Add(KeyboardKeys.P, new KeyCharMap(KeyboardKeys.P, "p", "P"));
			_keyMap.Add(KeyboardKeys.Q, new KeyCharMap(KeyboardKeys.Q, "q", "Q"));
			_keyMap.Add(KeyboardKeys.R, new KeyCharMap(KeyboardKeys.R, "r", "R"));
			_keyMap.Add(KeyboardKeys.S, new KeyCharMap(KeyboardKeys.S, "s", "S"));
			_keyMap.Add(KeyboardKeys.T, new KeyCharMap(KeyboardKeys.T, "t", "T"));
			_keyMap.Add(KeyboardKeys.U, new KeyCharMap(KeyboardKeys.U, "u", "U"));
			_keyMap.Add(KeyboardKeys.V, new KeyCharMap(KeyboardKeys.V, "v", "V"));
			_keyMap.Add(KeyboardKeys.W, new KeyCharMap(KeyboardKeys.W, "w", "W"));
			_keyMap.Add(KeyboardKeys.X, new KeyCharMap(KeyboardKeys.X, "x", "X"));
			_keyMap.Add(KeyboardKeys.Y, new KeyCharMap(KeyboardKeys.Y, "y", "Y"));
			_keyMap.Add(KeyboardKeys.Z, new KeyCharMap(KeyboardKeys.Z, "z", "Z"));
			_keyMap.Add(KeyboardKeys.NumPad0, new KeyCharMap(KeyboardKeys.NumPad0, "0", "0"));
			_keyMap.Add(KeyboardKeys.NumPad1, new KeyCharMap(KeyboardKeys.NumPad1, "1", "1"));
			_keyMap.Add(KeyboardKeys.NumPad2, new KeyCharMap(KeyboardKeys.NumPad2, "2", "2"));
			_keyMap.Add(KeyboardKeys.NumPad3, new KeyCharMap(KeyboardKeys.NumPad3, "3", "3"));
			_keyMap.Add(KeyboardKeys.NumPad4, new KeyCharMap(KeyboardKeys.NumPad4, "4", "4"));
			_keyMap.Add(KeyboardKeys.NumPad5, new KeyCharMap(KeyboardKeys.NumPad5, "5", "5"));
			_keyMap.Add(KeyboardKeys.NumPad6, new KeyCharMap(KeyboardKeys.NumPad6, "6", "6"));
			_keyMap.Add(KeyboardKeys.NumPad7, new KeyCharMap(KeyboardKeys.NumPad7, "7", "7"));
			_keyMap.Add(KeyboardKeys.NumPad8, new KeyCharMap(KeyboardKeys.NumPad8, "8", "8"));
			_keyMap.Add(KeyboardKeys.NumPad9, new KeyCharMap(KeyboardKeys.NumPad9, "9", "9"));
			_keyMap.Add(KeyboardKeys.Multiply, new KeyCharMap(KeyboardKeys.Multiply, "*", "*"));
			_keyMap.Add(KeyboardKeys.Add, new KeyCharMap(KeyboardKeys.Add, "+", "+"));
			_keyMap.Add(KeyboardKeys.Subtract, new KeyCharMap(KeyboardKeys.Subtract, "-", "-"));
			_keyMap.Add(KeyboardKeys.Divide, new KeyCharMap(KeyboardKeys.Divide, "/", "/"));
			_keyMap.Add(KeyboardKeys.Oem1, new KeyCharMap(KeyboardKeys.Oem1, ":", ":"));
			_keyMap.Add(KeyboardKeys.Oemplus, new KeyCharMap(KeyboardKeys.Oemplus, "=", "+"));
			_keyMap.Add(KeyboardKeys.Oemcomma, new KeyCharMap(KeyboardKeys.Oemcomma, ",", "<"));
			_keyMap.Add(KeyboardKeys.OemMinus, new KeyCharMap(KeyboardKeys.OemMinus, "-", "_"));
			_keyMap.Add(KeyboardKeys.OemPeriod, new KeyCharMap(KeyboardKeys.OemPeriod, ".", ">"));
			_keyMap.Add(KeyboardKeys.OemQuestion, new KeyCharMap(KeyboardKeys.OemQuestion, "/", "?"));
			_keyMap.Add(KeyboardKeys.Oemtilde, new KeyCharMap(KeyboardKeys.Oemtilde, "`", "~"));
			_keyMap.Add(KeyboardKeys.OemOpenBrackets, new KeyCharMap(KeyboardKeys.OemOpenBrackets, "[", "{"));
			_keyMap.Add(KeyboardKeys.Oem6, new KeyCharMap(KeyboardKeys.Oem6, "]", "}"));
			_keyMap.Add(KeyboardKeys.Oem7, new KeyCharMap(KeyboardKeys.Oem7, "\'", "\""));
			_keyMap.Add(KeyboardKeys.OemBackslash, new KeyCharMap(KeyboardKeys.OemBackslash, "\\", "|"));
		}

		/// <summary>
		/// Function to fire the key down event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="modifiers">Modifier keys.</param>
		/// <param name="scan">Scan code data.</param>
		protected void OnKeyDown(KeyboardKeys key, KeyboardKeys modifiers, int scan)
		{
			if (KeyDown != null)
			{
				KeyCharMap character;			// Character representation of the key.

				if (this.KeyMappings.ContainsKey(key))
					character = KeyMappings[key];
				else
					character = default(KeyCharMap);

				KeyboardInputEventArgs e = new KeyboardInputEventArgs(key, modifiers, character, scan);				
				KeyDown(this, e);
			}
		}

		/// <summary>
		/// Function to fire the key up event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="modifiers">Modifier keys.</param>
		/// <param name="scan">Scan code data.</param>
		protected void OnKeyUp(KeyboardKeys key, KeyboardKeys modifiers, int scan)
		{
			if (KeyUp != null)
			{
				KeyCharMap character;			// Character representation of the key.

				if (this.KeyMappings.ContainsKey(key))
					character = KeyMappings[key];
				else
					character = default(KeyCharMap);

				KeyboardInputEventArgs e = new KeyboardInputEventArgs(key, modifiers, character, scan);
				KeyUp(this, e);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Input interface that owns this device.</param>
		protected internal Keyboard(InputDevices owner)
			: base(owner)
		{
			_keyMap = new SortedList<KeyboardKeys, KeyCharMap>();
			GetDefaultKeyMapping();
		}
		#endregion
	}
}
