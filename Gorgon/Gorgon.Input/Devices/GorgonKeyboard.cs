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
using System.Linq;
using Gorgon.Core;
using Gorgon.Input.Properties;
using Gorgon.Math;

namespace Gorgon.Input
{
	/// <summary>
	/// The base class used to create keyboard interfaces.
	/// </summary>
	public abstract class GorgonKeyboard
		: GorgonInputDevice
	{
		#region Value types.
		/// <summary>
		/// A code and textual mapping for a character on the keyboard.
		/// </summary>
		public struct KeyCharMap
            : IEquatable<KeyCharMap>
		{
			#region Variables.
			/// <summary>
			/// Key that the character represents.
			/// </summary>
			public readonly KeyboardKey Key;
			/// <summary>
			/// Character representation of the <see cref="KeyboardKey"/>.
			/// </summary>
            public readonly char Character;
			/// <summary>
			/// Character representation when applying a shift modifier (e.g. the Shift key is held down).
			/// </summary>
            public readonly char Shifted;
			#endregion

            #region Methods.
			/// <inheritdoc/>
            public override int GetHashCode()
            {
                return 281.GenerateHash(Key);
            }

			/// <inheritdoc/>
            public override bool Equals(object obj)
            {
                if (obj is KeyCharMap)
                {
                    return ((KeyCharMap)obj).Key == Key;
                }

                return base.Equals(obj);
            }

            /// <summary>
            /// Function to determine if 2 instances are equal.
            /// </summary>
            /// <param name="left">The left instance to compare.</param>
            /// <param name="right">The right instance to compare.</param>
            /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
            public static bool Equals(ref KeyCharMap left, ref KeyCharMap right)
            {
                return left.Key == right.Key;
            }

			/// <inheritdoc/>
            public override string ToString()
            {
                return string.Format(Resources.GORINP_KEYCHARMAP_TOSTR, Key, Character, Shifted);
            }

            /// <summary>
            /// Operator to compare 2 instances for equality.
            /// </summary>
            /// <param name="left">Left instance to compare.</param>
            /// <param name="right">Right instance to compare.</param>
            /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
            public static bool operator ==(KeyCharMap left, KeyCharMap right)
            {
                return Equals(ref left, ref right);
            }

            /// <summary>
            /// Operator to compare 2 instances for inequality.
            /// </summary>
            /// <param name="left">Left instance to compare.</param>
            /// <param name="right">Right instance to compare.</param>
            /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
            public static bool operator !=(KeyCharMap left, KeyCharMap right)
            {
                return !Equals(ref left, ref right);
            }
            #endregion

            #region Constructor.
            /// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="key">Key that the character represents.</param>
			/// <param name="character">Character representation.</param>
			/// <param name="shifted">Character representation with shift modifier.</param>
			public KeyCharMap(KeyboardKey key, char character, char shifted)
			{
				Key = key;
				Character = character;
				Shifted = shifted;
			}
			#endregion

            #region IEquatable<KeyCharMap> Members
            /// <inheritdoc/>
            public bool Equals(KeyCharMap other)
            {
                return Equals(ref this, ref other);
            }
            #endregion
        }
		#endregion

		#region Classes.
		/// <summary>
		/// An equality comparer for the <see cref="KeyboardKey"/> enumeration.
		/// </summary>
		private class KeyboardKeysEqualityComparer
			: IEqualityComparer<KeyboardKey>
		{
			#region IEqualityComparer<KeyboardKeys> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <see cref="KeyboardKey"/> to compare.</param>
			/// <param name="y">The second object of type <see cref="KeyboardKey"/> to compare.</param>
			/// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
			public bool Equals(KeyboardKey x, KeyboardKey y)
			{
				return x == y;
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The <see cref="KeyboardKey" /> for which a hash code is to be returned.</param>
			/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
			public int GetHashCode(KeyboardKey obj)
			{
				return obj.GetHashCode();
			}
			#endregion
		}

		/// <summary>
		/// A list that contains keyboard key to character mappings.
		/// </summary>
		public class KeyMapCollection
			: ICollection<KeyCharMap>
		{
			#region Variables.
			// Keyboard mappings.
			private readonly Dictionary<KeyboardKey, KeyCharMap> _keys = new Dictionary<KeyboardKey, KeyCharMap>(new KeyboardKeysEqualityComparer());
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the key mapping assigned to the key.
			/// </summary>
			/// <param name="key">Key to retrieve mapping for.</param>
			/// <returns>Character mapping for the key.</returns>
			public KeyCharMap this[KeyboardKey key]
			{
				get
				{
				    KeyCharMap result;

				    if (!_keys.TryGetValue(key, out result))
				    {
                        throw new KeyNotFoundException(string.Format(Resources.GORINP_KEYBOARD_KEY_NO_MAPPING, key));
				    }

				    return result;
				}
				set
				{
				    if (!_keys.ContainsKey(key))
				    {
                        throw new KeyNotFoundException(string.Format(Resources.GORINP_KEYBOARD_KEY_NO_MAPPING, key));
				    }

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
			public void Add(KeyboardKey key, char value, char shiftedValue)
			{
                Add(new KeyCharMap(key, value, shiftedValue));
			}

			/// <summary>
			/// Function to return whether a key exists in this collection or not.
			/// </summary>
			/// <param name="key">Key to check for.</param>
			/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
			public bool Contains(KeyboardKey key)
			{
				return _keys.ContainsKey(key);
			}

			/// <summary>
			/// Function to remove the specified key from the mappings list.
			/// </summary>
			/// <param name="key">Key to remove.</param>
			public void Remove(KeyboardKey key)
			{
			    if (!_keys.ContainsKey(key))
			    {
			        throw new KeyNotFoundException(string.Format(Resources.GORINP_KEY_NOT_FOUND, key));
			    }

			    _keys.Remove(key);
			}

			/// <summary>
			/// Function to clear the key mappings.
			/// </summary>
			public void Clear()
			{
				_keys.Clear();
			}

            /// <summary>
            /// Function to attempt to retrieve a key character mapping from the list.
            /// </summary>
            /// <param name="key">Key to look up.</param>
            /// <param name="value">Value in the list.</param>
            /// <returns><b>true</b> if the value was found, <b>false</b> if not.</returns>
		    public bool TryGetValue(KeyboardKey key, out KeyCharMap value)
		    {
		        return _keys.TryGetValue(key, out value);
		    }
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="KeyMapCollection"/> class.
			/// </summary>
			internal KeyMapCollection()
			{
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
			    // ReSharper disable once LoopCanBeConvertedToQuery
			    foreach (KeyValuePair<KeyboardKey, KeyCharMap> value in _keys)
			    {
			        yield return value.Value;
			    }
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion

            #region ICollection<KeyCharMap> Members
            #region Properties.
            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
            public int Count
            {
                get
                {
                    return _keys.Count;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
            #endregion

            #region Methods.
            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            public void Add(KeyCharMap item)
            {
                _keys[item.Key] = item;
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
            /// </returns>
            public bool Contains(KeyCharMap item)
            {
                return _keys.ContainsValue(item);
            }

            /// <summary>
            /// Copies to.
            /// </summary>
            /// <param name="array">The array.</param>
            /// <param name="arrayIndex">Index of the array.</param>
            public void CopyTo(KeyCharMap[] array, int arrayIndex)
            {
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}

				if ((arrayIndex < 0) || (arrayIndex >= array.Length))
				{
					throw new ArgumentOutOfRangeException("arrayIndex");	
				}

	            int count = array.Length.Min(Count);

				for (int i = arrayIndex; i < count; i++)
				{
					array[i] = this.ElementAt(i);
				}
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </returns>
            /// <exception cref="System.NotSupportedException"></exception>
            bool ICollection<KeyCharMap>.Remove(KeyCharMap item)
            {
                if (!_keys.ContainsKey(item.Key))
                {
                    return false;
                }

                Remove(item.Key);

                return true;
            }
            #endregion
            #endregion
        }

		/// <summary>
		/// A list containing the current <see cref="KeyState"/> for each <see cref="KeyboardKey"/>.
		/// </summary>
		public class KeyStateCollection
            : ICollection<KeyState>
		{
			#region Variables.
			// Keyboard key state.
			private readonly Dictionary<KeyboardKey, KeyState> _keys = new Dictionary<KeyboardKey, KeyState>(new KeyboardKeysEqualityComparer());      
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the state of a given key.
			/// </summary>
			/// <param name="key">Key to check.</param>
			/// <returns>The state of the key.</returns>
			public KeyState this[KeyboardKey key]
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
				var keys = (KeyboardKey[])Enum.GetValues(typeof(KeyboardKey));

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
				this[KeyboardKey.Menu] = KeyState.Up;
				this[KeyboardKey.RMenu] = KeyState.Up;
				this[KeyboardKey.LMenu] = KeyState.Up;
				this[KeyboardKey.ShiftKey] = KeyState.Up;
				this[KeyboardKey.LShiftKey] = KeyState.Up;
				this[KeyboardKey.RShiftKey] = KeyState.Up;
				this[KeyboardKey.ControlKey] = KeyState.Up;
				this[KeyboardKey.RControlKey] = KeyState.Up;
				this[KeyboardKey.LControlKey] = KeyState.Up;
				this[KeyboardKey.Alt] = KeyState.Up;
				this[KeyboardKey.Control] = KeyState.Up;
				this[KeyboardKey.Shift] = KeyState.Up;
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
            public int Count
            {
                get
                {
                    return _keys.Count;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
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
                foreach (KeyValuePair<KeyboardKey, KeyState> state in _keys)
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
		/// Key down event.
		/// </summary>
		public event EventHandler<KeyboardEventArgs> KeyDown;

		/// <summary>
		/// Key up event.
		/// </summary>
		public event EventHandler<KeyboardEventArgs> KeyUp;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return information about the keyboard device.
		/// </summary>
		public IGorgonKeyboardInfo Info
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return if the key states should be reset when focus is lost.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When the <see cref="GorgonInputDevice.BoundControl"/> loses focus, the <see cref="KeyStates"/> may keep the previous key state, leading to undesirable results.  This setting will allow the user to 
		/// control how the key states are preserved after a loss of focus.  
		/// </para>
		/// <para>
		/// The user may also manually reset the key states by setting the individual key state to Up, or use the <see cref="GorgonKeyboard.KeyStateCollection.Reset"/> (or the 
		/// <see cref="GorgonKeyboard.KeyStateCollection.ResetModifiers"/>) method.</para>
		/// <para>The key reset settings are as follows:
		/// <list type="table">
		/// <listheader><term>Mode</term><description>Description</description></listheader>
		/// <item>
		///		<term><see cref="Input.KeyStateResetMode.None"/></term>
		///		<description>Preserve all previous key states.</description>
		/// </item>
		/// <item>
		///		<term><see cref="Input.KeyStateResetMode.ResetModifiers"/></term>
		///		<description>Reset only the modifier keys (Ctrl, Alt, and Shift) to the Up key state.</description>
		/// </item>
		/// <item>
		///		<term><see cref="Input.KeyStateResetMode.ResetAll"/></term>
		///		<description>Reset all keys to the Up key state.</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// By default, this property value is set to <see cref="Input.KeyStateResetMode.ResetAll"/>.
		/// </para>
		/// </remarks>
		public KeyStateResetMode KeyStateResetMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the keyboard to character mappings.
		/// </summary>
		public KeyMapCollection KeyMappings
		{
			get;
            private set;
		}

		/// <summary>
		/// Property to return the key states.
		/// </summary>
		public KeyStateCollection KeyStates
		{
			get;
            private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the modifier keys.
		/// </summary>
		/// <returns>The modifier keys.</returns>
		private KeyboardKey GetModifiers()
		{
			var result = KeyboardKey.None;

			if (KeyStates[KeyboardKey.ControlKey] == KeyState.Down)
			{
			    if (KeyStates[KeyboardKey.LControlKey] == KeyState.Down)
			    {
			        result |= KeyboardKey.Control | KeyboardKey.LeftVersion;
			    }

			    if (KeyStates[KeyboardKey.RControlKey] == KeyState.Down)
			    {
			        result |= KeyboardKey.Control | KeyboardKey.RightVersion;
			    }
			}
			if (KeyStates[KeyboardKey.Menu] == KeyState.Down)
			{
			    if (KeyStates[KeyboardKey.LMenu] == KeyState.Down)
			    {
			        result |= KeyboardKey.Alt | KeyboardKey.LeftVersion;
			    }

			    if (KeyStates[KeyboardKey.RMenu] == KeyState.Down)
			    {
			        result |= KeyboardKey.Alt | KeyboardKey.RightVersion;
			    }

			}
			if (KeyStates[KeyboardKey.ShiftKey] != KeyState.Down)
			{
				return result;
			}

			if (KeyStates[KeyboardKey.LShiftKey] == KeyState.Down)
			{
				result |= KeyboardKey.Shift | KeyboardKey.LeftVersion;
			}

			if (KeyStates[KeyboardKey.RShiftKey] == KeyState.Down)
			{
				result |= KeyboardKey.Shift | KeyboardKey.RightVersion;
			}

			return result;
		}

		/// <summary>
		/// Function to fire the key down event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="scan">Scan code data.</param>
		protected void OnKeyDown(KeyboardKey key, int scan)
		{
		    if (KeyDown == null)
		    {
		        return;
		    }

		    KeyCharMap character;
            
            KeyMappings.TryGetValue(key, out character);
		    var e = new KeyboardEventArgs(key, GetModifiers(), character, scan);

		    KeyDown(this, e);
		}

		/// <summary>
		/// Function to fire the key up event.
		/// </summary>
		/// <param name="key">Key that's pressed.</param>
		/// <param name="scan">Scan code data.</param>
		protected void OnKeyUp(KeyboardKey key, int scan)
		{
		    if (KeyUp == null)
		    {
		        return;
		    }

		    KeyCharMap character;
                
            KeyMappings.TryGetValue(key, out character);
		    var e = new KeyboardEventArgs(key, GetModifiers(), character, scan);

		    KeyUp(this, e);
		}

		/// <summary>
		/// Function called if the bound window loses focus.
		/// </summary>
		protected override void OnBoundWindowUnfocused()
		{
			base.OnBoundWindowUnfocused();

			// If we lose focus, then remove any modifiers.
			switch(KeyStateResetMode)
			{
				case KeyStateResetMode.ResetAll:
					KeyStates.Reset();
					break;
				case KeyStateResetMode.ResetModifiers:
					KeyStates.ResetModifiers();
					break;
			}
		}
	
		/// <summary>
		/// Function to retrieve character mappings for keys.
		/// </summary>
		public void GetDefaultKeyMapping()
		{
			// Add default key mapping.
			KeyMappings.Clear();
			KeyMappings.Add(KeyboardKey.Tab, '\t', '\t');
			KeyMappings.Add(KeyboardKey.Return, '\n', '\n');
			KeyMappings.Add(KeyboardKey.Space, ' ', ' ');
			KeyMappings.Add(KeyboardKey.D0, '0', ')');
			KeyMappings.Add(KeyboardKey.D1, '1', '!');
			KeyMappings.Add(KeyboardKey.D2, '2', '@');
			KeyMappings.Add(KeyboardKey.D3, '3', '#');
			KeyMappings.Add(KeyboardKey.D4, '4', '$');
			KeyMappings.Add(KeyboardKey.D5, '5', '%');
			KeyMappings.Add(KeyboardKey.D6, '6', '^');
			KeyMappings.Add(KeyboardKey.D7, '7', '&');
			KeyMappings.Add(KeyboardKey.D8, '8', '*');
			KeyMappings.Add(KeyboardKey.D9, '9', '(');
			KeyMappings.Add(KeyboardKey.A, 'a', 'A');
			KeyMappings.Add(KeyboardKey.B, 'b', 'B');
			KeyMappings.Add(KeyboardKey.C, 'c', 'C');
			KeyMappings.Add(KeyboardKey.D, 'd', 'D');
			KeyMappings.Add(KeyboardKey.E, 'e', 'E');
			KeyMappings.Add(KeyboardKey.F, 'f', 'F');
			KeyMappings.Add(KeyboardKey.G, 'g', 'G');
			KeyMappings.Add(KeyboardKey.H, 'h', 'H');
			KeyMappings.Add(KeyboardKey.I, 'i', 'I');
			KeyMappings.Add(KeyboardKey.J, 'j', 'J');
			KeyMappings.Add(KeyboardKey.K, 'k', 'K');
			KeyMappings.Add(KeyboardKey.L, 'l', 'L');
			KeyMappings.Add(KeyboardKey.M, 'm', 'M');
			KeyMappings.Add(KeyboardKey.N, 'n', 'N');
			KeyMappings.Add(KeyboardKey.O, 'o', 'O');
			KeyMappings.Add(KeyboardKey.P, 'p', 'P');
			KeyMappings.Add(KeyboardKey.Q, 'q', 'Q');
			KeyMappings.Add(KeyboardKey.R, 'r', 'R');
			KeyMappings.Add(KeyboardKey.S, 's', 'S');
			KeyMappings.Add(KeyboardKey.T, 't', 'T');
			KeyMappings.Add(KeyboardKey.U, 'u', 'U');
			KeyMappings.Add(KeyboardKey.V, 'v', 'V');
			KeyMappings.Add(KeyboardKey.W, 'w', 'W');
			KeyMappings.Add(KeyboardKey.X, 'x', 'X');
			KeyMappings.Add(KeyboardKey.Y, 'y', 'Y');
			KeyMappings.Add(KeyboardKey.Z, 'z', 'Z');
			KeyMappings.Add(KeyboardKey.NumPad0, '0', '0');
			KeyMappings.Add(KeyboardKey.NumPad1, '1', '1');
			KeyMappings.Add(KeyboardKey.NumPad2, '2', '2');
			KeyMappings.Add(KeyboardKey.NumPad3, '3', '3');
			KeyMappings.Add(KeyboardKey.NumPad4, '4', '4');
			KeyMappings.Add(KeyboardKey.NumPad5, '5', '5');
			KeyMappings.Add(KeyboardKey.NumPad6, '6', '6');
			KeyMappings.Add(KeyboardKey.NumPad7, '7', '7');
			KeyMappings.Add(KeyboardKey.NumPad8, '8', '8');
			KeyMappings.Add(KeyboardKey.NumPad9, '9', '9');
			KeyMappings.Add(KeyboardKey.Multiply, '*', '*');
			KeyMappings.Add(KeyboardKey.Add, '+', '+');
			KeyMappings.Add(KeyboardKey.Subtract, '-', '-');
			KeyMappings.Add(KeyboardKey.Divide, '/', '/');
			KeyMappings.Add(KeyboardKey.OemPipe, '\\', '|');
			KeyMappings.Add(KeyboardKey.Oem1, ';', ':');
			KeyMappings.Add(KeyboardKey.OemSemicolon, ';', ':');
			KeyMappings.Add(KeyboardKey.Oemplus, '=', '+');
			KeyMappings.Add(KeyboardKey.Oemcomma, ',', '<');
			KeyMappings.Add(KeyboardKey.OemMinus, '-', '_');
			KeyMappings.Add(KeyboardKey.OemPeriod, '.', '>');
			KeyMappings.Add(KeyboardKey.OemQuestion, '/', '?');
			KeyMappings.Add(KeyboardKey.Oemtilde, '`', '~');
			KeyMappings.Add(KeyboardKey.OemOpenBrackets, '[', '{');
			KeyMappings.Add(KeyboardKey.Oem6, ']', '}');
			KeyMappings.Add(KeyboardKey.Oem7, '\'', '\"');
			KeyMappings.Add(KeyboardKey.OemBackslash, '\\', '|');
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="keyboardInfo">Information about the keyboard.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="owner"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		protected GorgonKeyboard(GorgonInputService owner, IGorgonKeyboardInfo keyboardInfo)
			: base(owner, keyboardInfo)
		{
			Info = keyboardInfo;
            KeyMappings = new KeyMapCollection();
            KeyStates = new KeyStateCollection();
			KeyStateResetMode = KeyStateResetMode.ResetAll;
		}
		#endregion
	}
}
