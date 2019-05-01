#region MIT
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, September 10, 2015 10:11:39 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gorgon.Input
{
	/// <summary>
	/// A list containing the current <see cref="KeyState"/> for each key in the <see cref="Keys"/> enumeration. 
	/// </summary>
	public class GorgonKeyStateCollection
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

				if (!_keys.TryGetValue(key, out KeyState result))
				{
					_keys.Add(key, KeyState.Up);
				}

				return result;
			}
			set => _keys[key] = value;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the key states.
		/// </summary>
		public void Reset()
		{
			var keys = (Keys[])Enum.GetValues(typeof(Keys));

			foreach (Keys key in keys)
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
		/// Initializes a new instance of the <see cref="GorgonKeyStateCollection"/> class.
		/// </summary>
		internal GorgonKeyStateCollection()
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
        /// <exception cref="NotSupportedException"></exception>
        void ICollection<KeyState>.Add(KeyState item) => throw new NotSupportedException();

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        void ICollection<KeyState>.Clear() => Reset();

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        bool ICollection<KeyState>.Contains(KeyState item) => _keys.ContainsValue(item);

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="NotSupportedException"></exception>
        void ICollection<KeyState>.CopyTo(KeyState[] array, int arrayIndex) => throw new NotSupportedException();

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="NotSupportedException"></exception>
        bool ICollection<KeyState>.Remove(KeyState item) => false;
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
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}