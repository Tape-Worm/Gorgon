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
using System.Windows.Forms;
using Forms = System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Internal.Native;

namespace GorgonLibrary.InputDevices
{
    /// <summary>
    /// Enumeration containing key state.
    /// </summary>
    public enum KeyState
    {
        /// <summary>
        /// Key is not pressed.
        /// </summary>
        Up = 0,
        /// <summary>
        /// Key is pressed.
        /// </summary>
        Down = 1
    }
    
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

        #region Classes.
        /// <summary>
        /// Object representing the keyboard mappings.
        /// </summary>
        public class KeyMapCollection
            : IEnumerable<KeyCharMap>
        {
            #region Variables.
            private SortedDictionary<KeyboardKeys, KeyCharMap> _keys = null;    // Keyboard mappings.
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the key mapping assigned to the key.
            /// </summary>
            /// <param name="key">Key to retrieve mapping for.</param>
            /// <returns>Character mapping for the key.</returns>
            public KeyCharMap this[KeyboardKeys key]
            {
                get
                {
                    if (!_keys.ContainsKey(key))
                        throw new KeyNotFoundException("Keyboard key '" + key.ToString() + "' has not been assigned to a mapping.");

                    return _keys[key];
                }
                set
                {
                    if (!_keys.ContainsKey(key))
                        throw new KeyNotFoundException("Keyboard key '" + key.ToString() + "' has not been assigned to a mapping.");

                    _keys[key] = value;
                }
            }
            #endregion

            #region Methods.
            /// <summary>
            /// Function to add a key mapping to the collection.
            /// </summary>
            /// <param name="key">Key to map.</param>
            /// <param name="value">Value to map to the key.</param>
            /// <param name="shiftedValue">Value to map if the shift key is held down.</param>
            public void Add(KeyboardKeys key, string value, string shiftedValue)
            {
                if (_keys.ContainsKey(key))
                    _keys[key] = new KeyCharMap(key, value, shiftedValue);
                else
                    _keys.Add(key, new KeyCharMap(key, value, shiftedValue));
            }

            /// <summary>
            /// Function to return whether a key exists in this collection or not.
            /// </summary>
            /// <param name="key">Key to check for.</param>
            /// <returns>TRUE if found, FALSE if not.</returns>
            public bool Contains(KeyboardKeys key)
            {
                return _keys.ContainsKey(key);
            }

            /// <summary>
            /// Function to remove the specified key from the mappings list.
            /// </summary>
            /// <param name="key">Key to remove.</param>
            public void Remove(KeyboardKeys key)
            {
                if (!_keys.ContainsKey(key))
                    throw new KeyNotFoundException("Keyboard key '" + key.ToString() + "' has not been assigned to a mapping.");

                _keys.Remove(key);
            }

            /// <summary>
            /// Function to clear the key mappings.
            /// </summary>
            public void Clear()
            {
                _keys.Clear();
            }
            #endregion

            #region Constructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyMapCollection"/> class.
            /// </summary>
            internal KeyMapCollection()
            {
                _keys = new SortedDictionary<KeyboardKeys, KeyCharMap>();
            }
            #endregion

            #region IEnumerable<KeyCharMap> Members

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<KeyCharMap> GetEnumerator()
            {
                foreach (KeyValuePair<KeyboardKeys, KeyCharMap> value in _keys)
                    yield return value.Value;
            }
            #endregion

            #region IEnumerable Members

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
            #endregion
        }

        /// <summary>
        /// Object representing the keyboard state.
        /// </summary>
        public class KeyStateCollection
        {
            #region Variables.
            private SortedDictionary<KeyboardKeys, KeyState> _keys = null;      // Keyboard key state.
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the state of a given key.
            /// </summary>
            /// <param name="key">Key to check.</param>
            /// <returns>The state of the key.</returns>
            public KeyState this[KeyboardKeys key]
            {
                get
                {
                    if (!_keys.ContainsKey(key))
                        _keys.Add(key, KeyState.Up);

                    return _keys[key];
                }
                set
                {
                    if (!_keys.ContainsKey(key))
                        _keys.Add(key, value);
                    else
                        _keys[key] = value;
                }
            }
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyStateCollection"/> class.
            /// </summary>
            public KeyStateCollection()
            {
                _keys = new SortedDictionary<KeyboardKeys, KeyState>();
            }
            #endregion
        }
        #endregion
        
        #region Variables.
		private KeyMapCollection _keyMap = null;		                    // Key->character mapping.
        private KeyStateCollection _keyStates = null;                       // Key states.
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
		public KeyMapCollection KeyMappings
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
        /// Property to return the key states.
        /// </summary>
        public KeyStateCollection KeyStates
        {
            get
            {
                return _keyStates;
            }
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to fire the key down event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="modifiers">Modifier keys.</param>
		/// <param name="scan">Scan code data.</param>
		/// <param name="left">TRUE if the modifier is left, FALSE if not.</param>
		/// <param name="right">TRUE if the modifier is right, FALSE if not.</param>
		protected void OnKeyDown(KeyboardKeys key, KeyboardKeys modifiers, int scan, bool left, bool right)
		{
			if (KeyDown != null)
			{
				KeyCharMap character;			// Character representation of the key.

				if (this.KeyMappings.Contains(key))
					character = KeyMappings[key];
				else
					character = default(KeyCharMap);

				KeyboardInputEventArgs e = new KeyboardInputEventArgs(key, modifiers, character, scan, left, right);
				KeyDown(this, e);
			}
		}

		/// <summary>
		/// Function to fire the key up event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="modifiers">Modifier keys.</param>
		/// <param name="scan">Scan code data.</param>
		/// <param name="left">TRUE if the modifier is left, FALSE if not.</param>
		/// <param name="right">TRUE if the modifier is right, FALSE if not.</param>
		protected void OnKeyUp(KeyboardKeys key, KeyboardKeys modifiers, int scan, bool left, bool right)
		{
			if (KeyUp != null)
			{
				KeyCharMap character;			// Character representation of the key.

				if (this.KeyMappings.Contains(key))
					character = KeyMappings[key];
				else
					character = default(KeyCharMap);

				KeyboardInputEventArgs e = new KeyboardInputEventArgs(key, modifiers, character, scan, left, right);
				KeyUp(this, e);
			}
		}

		/// <summary>
		/// Function to retrieve character mappings for keys.
		/// </summary>
		public void GetDefaultKeyMapping()
		{
			// Add default key mapping.
			_keyMap.Clear();
			_keyMap.Add(KeyboardKeys.Tab, "\t", "\t");
			_keyMap.Add(KeyboardKeys.Return, "\n", "\n");
			_keyMap.Add(KeyboardKeys.Space, " ", " ");
			_keyMap.Add(KeyboardKeys.D0, "0", ")");
			_keyMap.Add(KeyboardKeys.D1, "1", "!");
			_keyMap.Add(KeyboardKeys.D2, "2", "@");
			_keyMap.Add(KeyboardKeys.D3, "3", "#");
			_keyMap.Add(KeyboardKeys.D4, "4", "$");
			_keyMap.Add(KeyboardKeys.D5, "5", "%");
			_keyMap.Add(KeyboardKeys.D6, "6", "^");
			_keyMap.Add(KeyboardKeys.D7, "7", "&");
			_keyMap.Add(KeyboardKeys.D8, "8", "*");
			_keyMap.Add(KeyboardKeys.D9, "9", "(");
			_keyMap.Add(KeyboardKeys.A, "a", "A");
			_keyMap.Add(KeyboardKeys.B, "b", "B");
			_keyMap.Add(KeyboardKeys.C, "c", "C");
			_keyMap.Add(KeyboardKeys.D, "d", "D");
			_keyMap.Add(KeyboardKeys.E, "e", "E");
			_keyMap.Add(KeyboardKeys.F, "f", "F");
			_keyMap.Add(KeyboardKeys.G, "g", "G");
			_keyMap.Add(KeyboardKeys.H, "h", "H");
			_keyMap.Add(KeyboardKeys.I, "i", "I");
			_keyMap.Add(KeyboardKeys.J, "j", "J");
			_keyMap.Add(KeyboardKeys.K, "k", "K");
			_keyMap.Add(KeyboardKeys.L, "l", "L");
			_keyMap.Add(KeyboardKeys.M, "m", "M");
			_keyMap.Add(KeyboardKeys.N, "n", "N");
			_keyMap.Add(KeyboardKeys.O, "o", "O");
			_keyMap.Add(KeyboardKeys.P, "p", "P");
			_keyMap.Add(KeyboardKeys.Q, "q", "Q");
			_keyMap.Add(KeyboardKeys.R, "r", "R");
			_keyMap.Add(KeyboardKeys.S, "s", "S");
			_keyMap.Add(KeyboardKeys.T, "t", "T");
			_keyMap.Add(KeyboardKeys.U, "u", "U");
			_keyMap.Add(KeyboardKeys.V, "v", "V");
			_keyMap.Add(KeyboardKeys.W, "w", "W");
			_keyMap.Add(KeyboardKeys.X, "x", "X");
			_keyMap.Add(KeyboardKeys.Y, "y", "Y");
			_keyMap.Add(KeyboardKeys.Z, "z", "Z");
			_keyMap.Add(KeyboardKeys.NumPad0, "0", "0");
			_keyMap.Add(KeyboardKeys.NumPad1, "1", "1");
			_keyMap.Add(KeyboardKeys.NumPad2, "2", "2");
			_keyMap.Add(KeyboardKeys.NumPad3, "3", "3");
			_keyMap.Add(KeyboardKeys.NumPad4, "4", "4");
			_keyMap.Add(KeyboardKeys.NumPad5, "5", "5");
			_keyMap.Add(KeyboardKeys.NumPad6, "6", "6");
			_keyMap.Add(KeyboardKeys.NumPad7, "7", "7");
			_keyMap.Add(KeyboardKeys.NumPad8, "8", "8");
			_keyMap.Add(KeyboardKeys.NumPad9, "9", "9");
			_keyMap.Add(KeyboardKeys.Multiply, "*", "*");
			_keyMap.Add(KeyboardKeys.Add, "+", "+");
			_keyMap.Add(KeyboardKeys.Subtract, "-", "-");
			_keyMap.Add(KeyboardKeys.Divide, "/", "/");
            _keyMap.Add(KeyboardKeys.OemPipe, @"\", "|");
			_keyMap.Add(KeyboardKeys.Oem1, ";", ":");
            _keyMap.Add(KeyboardKeys.OemSemicolon, ";", ":");
			_keyMap.Add(KeyboardKeys.Oemplus, "=", "+");
			_keyMap.Add(KeyboardKeys.Oemcomma, ",", "<");
			_keyMap.Add(KeyboardKeys.OemMinus, "-", "_");
			_keyMap.Add(KeyboardKeys.OemPeriod, ".", ">");
			_keyMap.Add(KeyboardKeys.OemQuestion, "/", "?");
			_keyMap.Add(KeyboardKeys.Oemtilde, "`", "~");
			_keyMap.Add(KeyboardKeys.OemOpenBrackets, "[", "{");
			_keyMap.Add(KeyboardKeys.Oem6, "]", "}");
			_keyMap.Add(KeyboardKeys.Oem7, "\'", "\"");
			_keyMap.Add(KeyboardKeys.OemBackslash, "\\", "|");
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		protected internal Keyboard(Input owner)
			: base(owner)
		{
            _keyMap = new KeyMapCollection();
            _keyStates = new KeyStateCollection();
			GetDefaultKeyMapping();
		}
		#endregion
	}
}
