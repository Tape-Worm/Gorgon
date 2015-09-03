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
// Created: Friday, June 24, 2011 10:04:17 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <summary>
	/// Key states.
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
	/// The base class used to create keyboard interfaces.
	/// </summary>
	public sealed class GorgonKeyboard2
		: GorgonInputDevice2, IGorgonInputEventDrivenDevice<GorgonKeyboardData>
	{
		#region Classes.
		/// <summary>
		/// A list containing the current <see cref="KeyState"/> for each <see cref="KeyboardKey"/>.
		/// </summary>
		public class KeyStateCollection
            : ICollection<KeyState>
		{
			#region Variables.
			// Keyboard key state.
			private readonly Dictionary<Keys, KeyState> _keys = new Dictionary<Keys, KeyState>(new GorgonKeysEqualityComparer());      
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the state of a given key.
			/// </summary>
			/// <param name="key">Key to check.</param>
			/// <returns>The state of the key.</returns>
			public KeyState this[Keys key]
			{
				get
				{
				    KeyState result;

				    if (!_keys.TryGetValue(key, out result))
				    {
				        _keys.Add(key, KeyState.Up);
				    }

				    return result;
				}
				set
				{
				    _keys[key] = value;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to reset the key states.
			/// </summary>
			public void Reset()
			{
				var keys = (Keys[])Enum.GetValues(typeof(KeyboardKey));

			    foreach (var key in keys)
			    {
			        this[key] = KeyState.Up;
			    }
			}

			/// <summary>
			/// Function to reset any modifier keys.
			/// </summary>
			public void ResetModifiers()
			{
				this[Keys.Menu] = KeyState.Up;
				this[Keys.RMenu] = KeyState.Up;
				this[Keys.LMenu] = KeyState.Up;
				this[Keys.ShiftKey] = KeyState.Up;
				this[Keys.LShiftKey] = KeyState.Up;
				this[Keys.RShiftKey] = KeyState.Up;
				this[Keys.ControlKey] = KeyState.Up;
				this[Keys.RControlKey] = KeyState.Up;
				this[Keys.LControlKey] = KeyState.Up;
				this[Keys.Alt] = KeyState.Up;
				this[Keys.Control] = KeyState.Up;
				this[Keys.Shift] = KeyState.Up;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="KeyStateCollection"/> class.
			/// </summary>
			internal KeyStateCollection()
			{
			}
			#endregion

            #region ICollection<KeyState> Members
            #region Properties.
            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
            public int Count => _keys.Count;

			/// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
            public bool IsReadOnly => true;

			#endregion

            #region Methods.
            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            void ICollection<KeyState>.Add(KeyState item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            void ICollection<KeyState>.Clear()
            {
                Reset();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
            /// </returns>
            bool ICollection<KeyState>.Contains(KeyState item)
            {
                return _keys.ContainsValue(item);
            }

            /// <summary>
            /// Copies to.
            /// </summary>
            /// <param name="array">The array.</param>
            /// <param name="arrayIndex">Index of the array.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            void ICollection<KeyState>.CopyTo(KeyState[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </returns>
            /// <exception cref="System.NotSupportedException"></exception>
            bool ICollection<KeyState>.Remove(KeyState item)
            {
                return false;
            }
            #endregion
            #endregion

            #region IEnumerable<KeyState> Members
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<KeyState> GetEnumerator()
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (KeyValuePair<Keys, KeyState> state in _keys)
                {
                    yield return state.Value;
                }
            }
            #endregion

            #region IEnumerable Members
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when a key is pressed on the keyboard.
		/// </summary>
		public event EventHandler<GorgonKeyboardEventArgs> KeyDown;

		/// <summary>
		/// Event fired when a key is released on the keyboard.
		/// </summary>
		public event EventHandler<GorgonKeyboardEventArgs> KeyUp;
		#endregion

		#region Variables.
		// The character buffer to hold the characters represented by a key press.
		private static readonly char[] _characterBuffer = new char[1];
		// The state values for the keys on the keyboard when translating a key press to a character.
		private static readonly byte[] _charStates = new byte[256];
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public override bool IsPolled => false;

		/// <summary>
		/// Property to return information about this keyboard.
		/// </summary>

		public IGorgonKeyboardInfo2 Info
		{
			get;
		}

		/// <summary>
		/// Property to return the key states.
		/// </summary>
		public KeyStateCollection KeyStates
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the modifier keys.
		/// </summary>
		/// <returns>The modifier keys.</returns>
		private Keys GetModifiers()
		{
			var result = Keys.None;

			if (KeyStates[Keys.ControlKey] == KeyState.Down)
			{
			    if (KeyStates[Keys.LControlKey] == KeyState.Down)
			    {
			        result |= Keys.Control | Keys.LControlKey;
			    }

			    if (KeyStates[Keys.RControlKey] == KeyState.Down)
			    {
			        result |= Keys.Control | Keys.RControlKey;
			    }
			}

			if (KeyStates[Keys.Menu] == KeyState.Down)
			{
			    if (KeyStates[Keys.LMenu] == KeyState.Down)
			    {
			        result |= Keys.Alt | Keys.LMenu;
			    }

			    if (KeyStates[Keys.RMenu] == KeyState.Down)
			    {
			        result |= Keys.Alt | Keys.RMenu;
			    }

			}

			if (KeyStates[Keys.ShiftKey] != KeyState.Down)
			{
				return result;
			}

			if (KeyStates[Keys.LShiftKey] == KeyState.Down)
			{
				result |= Keys.Shift | Keys.LShiftKey;
			}

			if (KeyStates[Keys.RShiftKey] == KeyState.Down)
			{
				result |= Keys.Shift | Keys.RShiftKey;
			}

			return result;
		}

		/// <summary>
		/// Function to fire the key down event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="scan">Scan code data.</param>
		private void OnKeyDown(Keys key, int scan)
		{
			KeyDown?.Invoke(this, new GorgonKeyboardEventArgs(key, GetModifiers(), scan));
		}

		/// <summary>
		/// Function to fire the key up event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="scan">Scan code data.</param>
		private void OnKeyUp(Keys key, int scan)
		{
			KeyUp?.Invoke(this, new GorgonKeyboardEventArgs(key, GetModifiers(), scan));
		}

		/// <inheritdoc/>
		protected override void OnAcquiredStateChanged()
		{
			KeyStates.Reset();
		}

		/// <summary>
		/// Function to convert a keyboard key into a character (if applicable).
		/// </summary>
		/// <param name="key">The key to convert into a character.</param>
		/// <param name="modifier">The modifier for that key.</param>
		/// <returns>The character representation for the key. If no representation is available, an empty string is returned.</returns>
		/// <remarks>
		/// <para>
		/// Use this to retrieve the character associated with a keyboard key. For example, if <see cref="Keys.A"/> is pressed, then 'a' will be returned. A <paramref name="modifier"/> can be 
		/// passed with the <see cref="Keys.ShiftKey"/> to return 'A'. 
		/// </para>
		/// <para>
		/// This method also supports the AltGr key which is represented by a combination of the <see cref="Keys.ControlKey"/> | <see cref="Keys.Menu"/> keys.
		/// </para>
		/// <para>
		/// This method only returns characters for the currently active keyboard layout (i.e. the system keyboard layout). If this keyboard interface represents another keyboard attached to the computer 
		/// then it will default to using the system keyboard to retrieve the character.
		/// </para>
		/// <para>
		/// This method is not thread safe. Invalid data will be returned if multiple thread access this method.
		/// </para>
		/// <para>
		/// This method was derived from the answer at <a href="http://stackoverflow.com/questions/6929275/how-to-convert-a-virtual-key-code-to-a-character-according-to-the-current-keyboa"/>.
		/// </para>
		/// </remarks>
		public string KeyToCharacter(Keys key, Keys modifier)
		{
			const int shiftKey = (int)Keys.ShiftKey;
			const int ctrlKey = (int)Keys.ControlKey;
			const int menuKey = (int)Keys.Menu;

			// Shift key modifier
			if ((modifier & Keys.Shift) == Keys.Shift)
			{
				_charStates[shiftKey] = 0xff;
			}
			else
			{
				_charStates[shiftKey] = 0;
			}

			// AltGr modifiers
			if ((modifier & (Keys.Control | Keys.Menu)) == (Keys.Control | Keys.Menu))
			{
				_charStates[ctrlKey] = 0xff;
				_charStates[menuKey] = 0xff;
			}
			else
			{
				_charStates[ctrlKey] = 0;
				_charStates[menuKey] = 0;
			}

			int result = UserApi.ToUnicode((uint)key, 0, _charStates, _characterBuffer, _characterBuffer.Length, 0);

			switch (result)
			{
				case -1:
				case 0:
					return string.Empty;
				case 1:
					return new string(_characterBuffer, 0, 1);
				default:
					return string.Empty;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKeyboard"/> class.
		/// </summary>
		/// <param name="service">The input service that this device is registered with.</param>
		/// <param name="keyboardInfo">Information about which keyboard to use.</param>
		/// <param name="log">[Optional] The logging interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="service"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		public GorgonKeyboard2(GorgonInputService2 service, IGorgonKeyboardInfo2 keyboardInfo, IGorgonLog log = null)
			: base(service, keyboardInfo, log)
		{
			Info = keyboardInfo;
            KeyStates = new KeyStateCollection();
		}
		#endregion

		#region GorgonKeyboard2 Members.
		#region Methods.
		#endregion
		#endregion

		#region IGorgonDeviceRouting<GorgonKeyboardData> Members
		/// <inheritdoc/>
		InputDeviceType IGorgonInputEventDrivenDevice<GorgonKeyboardData>.DeviceType => InputDeviceType.Keyboard;

		/// <inheritdoc/>
		bool IGorgonInputEventDrivenDevice<GorgonKeyboardData>.ParseData(ref GorgonKeyboardData data)
		{
			if ((!IsAcquired) || (Window == null) || (Window.Disposing) || (Window.IsDisposed))
			{
				return false;
			}

			// Get the key code.
			Keys keyCode = data.Key;

			KeyState state = ((data.Flags & KeyboardDataFlags.KeyUp) == KeyboardDataFlags.KeyUp) ? KeyState.Up : KeyState.Down;

			// Determine right or left, and unifier key.
			switch (keyCode)
			{
				case Keys.ControlKey:	// CTRL.
					keyCode = ((data.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? Keys.LControlKey : Keys.RControlKey;
					KeyStates[Keys.ControlKey] = state;
					break;
				case Keys.Menu:			// ALT.
					keyCode = ((data.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? Keys.LMenu : Keys.RMenu;
					KeyStates[Keys.Menu] = state;
					break;
				case Keys.ShiftKey:		// Shift.
					keyCode = ((data.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? Keys.LShiftKey : Keys.RShiftKey;
					KeyStates[Keys.ShiftKey] = state;
					break;
			}

			// Dispatch the key.
			KeyStates[keyCode] = state;

			if (state == KeyState.Down)
			{
				OnKeyDown(keyCode, data.ScanCode);
			}
			else
			{
				OnKeyUp(keyCode, data.ScanCode);
			}

			return true;
		}
		#endregion
	}
}
