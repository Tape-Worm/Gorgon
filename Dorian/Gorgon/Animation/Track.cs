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
// Created: Monday, November 20, 2006 1:32:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Object representing a track in an animation.
    /// </summary>
    public abstract class Track
        : NamedObject, IEnumerable<KeyFrame>
    {
        #region Value Types.
        /// <summary>
        /// Value type containing information about the nearest keys for a time position.
        /// </summary>
        public struct NearestKeys
        {
            #region Variables.
            private float _timePosition;            // Time position within the track that lies between the two keys.
            private KeyFrame _previousKey;				// Keyframe prior to the time position.
            private KeyFrame _nextKey;					// Keyframe after the time position.
            private float _keyTimeDelta;            // Distance between the previous and next keyframes, 0 if equal to the previous, 1 if equal to the next.
            private int _previousKeyIndex;          // Previous key index.
            private int _nextKeyIndex;              // Next key index.
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the track time position requested.
            /// </summary>
            public float TrackTimePosition
            {
                get
                {
                    return _timePosition;
                }
            }

            /// <summary>
            /// Property to return the previous key to the time position.
            /// </summary>
            public KeyFrame PreviousKey
            {
                get
                {
                    return _previousKey;
                }
            }

            /// <summary>
            /// Property to return the next key from the time position.
            /// </summary>
			public KeyFrame NextKey
            {
                get
                {
                    return _nextKey;
                }
            }

            /// <summary>
            /// Property to return the previous key index.
            /// </summary>
            public int PreviousKeyIndex
            {
                get
                {
                    return _previousKeyIndex;
                }
            }

            /// <summary>
            /// Property to return the next key index.
            /// </summary>
            public int NextKeyIndex
            {
                get
                {
                    return _nextKeyIndex;
                }
            }

            /// <summary>
            /// Property to return the delta between the previous and next frames.  0 if equal to the previous keyframe, 1.0 if equal to the next keyframe.
            /// </summary>
            public float KeyTimeDelta
            {
                get
                {
                    return _keyTimeDelta;
                }
            }
            #endregion

            #region Constructor.
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="owner">Owning track.</param>
            /// <param name="requestedTime">Track time requested.</param>
            internal NearestKeys(Track owner, float requestedTime)
            {
                float animationLength = owner.Owner.Length;     // Animation length.
				int i = 0;										// Loop.

				if (owner.KeyCount < 1)
					throw new ArgumentException("Track has no keys.", "owner");

                // Initialize.
                _previousKeyIndex = 0;
                _previousKey = owner.GetKeyAtIndex(0);
                _nextKeyIndex = -1;                
                _nextKey = null;
                _keyTimeDelta = 0;                

                // Wrap around.
                while (requestedTime > animationLength)
                    requestedTime -= animationLength;

                // Find previous key.                
				if (owner.GetKeyAtIndex(0).Time <= requestedTime)
				{
					for (i = 0; i < owner.KeyCount; i++)
					{
						if ((owner.GetKeyAtIndex(i).Time > requestedTime) && (i > 0))
						{
							_previousKey = owner.GetKeyAtIndex(i - 1);
							_previousKeyIndex = i - 1;
							break;
						}
					}
				}
				else
					i = 1;

                // Wrap to the first key if we went through the entire list.
                if (i >= owner.KeyCount)
                {
                    if (!owner.Owner.Looped)
                    {
                        _nextKey = owner.GetKeyAtIndex(owner.KeyCount - 1);
                        _nextKeyIndex = owner.KeyCount - 1;
                    }
                    else
                    {
                        _nextKey = owner.GetKeyAtIndex(0);
                        _nextKeyIndex = 0;
                    }
                    _keyTimeDelta = animationLength;
                }
                else
                {
                    _nextKey = owner.GetKeyAtIndex(i);
                    _nextKeyIndex = i;
                    _keyTimeDelta = _nextKey.Time;
                }

                // Same frame.
                if (MathUtility.EqualFloat(_previousKey.Time, _keyTimeDelta, 0.001f))
                    _keyTimeDelta = 0;
                else
                    _keyTimeDelta = (requestedTime - _previousKey.Time) / (_keyTimeDelta - _previousKey.Time);

				// We can't have negative time.
				if (_keyTimeDelta < 0)
					_keyTimeDelta = 0;
                _timePosition = requestedTime;
            }
            #endregion
        }
        #endregion

		#region Classes.
		/// <summary>
		/// Object representing a sorter for frame times.
		/// </summary>
		private class FrameTimeSort
			: IComparer<float>
		{
			#region IComparer<float> Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
			/// </returns>
			public int Compare(float x, float y)
			{
				if (MathUtility.EqualFloat(x, y, 0.001f))
					return 0;

				if (x < y)
					return -1;

				return 1;
			}
			#endregion
		}
		#endregion

        #region Variables.
		private FrameTimeSort _sorter = null;				// Frame time sorter.
		private Animation _owner = null;					// Animation that owns the track.
		private SortedList<float, KeyFrame> _keys = null;	// List of key frames.
		private bool _keysUpdated = true;					// Flag to indicate that the keys had been updated.
		private PropertyInfo _property = null;				// Property on the owner object to update.
		private InterpolationMode _interpolation;			// Interpolation mode.
		private Type _dataType = null;						// Data type represented by the track.
		private MinMaxRangeF _range = MinMaxRangeF.Empty;	// Min/max value for the data type in the track.
		private bool _roundValues = false;					// Flag to indicate that the values need to be rounded.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the key list.
		/// </summary>
		protected SortedList<float, KeyFrame> KeyList
		{
			get
			{
				return _keys;
			}
		}

		/// <summary>
		/// Property to set or return the bound property.
		/// </summary>
		internal PropertyInfo BoundProperty
		{
			get
			{
				return _property;
			}
			set
			{
				_property = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the editor can drag the values.
		/// </summary>
		public EditorDragType EditCanDragValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to round the values for the track.
		/// </summary>
		public bool RoundValues
		{
			get
			{
				return _roundValues;
			}
			set
			{
				_roundValues = value;
			}
		}

		/// <summary>
		/// Property to set or return the range of values for the data represented by the track.
		/// </summary>
		public MinMaxRangeF DataRange
		{
			get
			{
				return _range;
			}
			set
			{
				_range = value;
			}
		}

		/// <summary>
		/// Property to return the type used by the track.
		/// </summary>
		public Type DataType
		{
			get
			{
				return _dataType;
			}
		}

		/// <summary>
		/// Property to set or return the interpolation mode for the track.
		/// </summary>
		public virtual InterpolationMode InterpolationMode
		{
			get
			{
				return _interpolation;
			}
			set
			{
				_interpolation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the track needs updating.
		/// </summary>
		public bool NeedsUpdate
		{
			get
			{
				return _keysUpdated;
			}
			set
			{
				_keysUpdated = value;
			}
		}

		/// <summary>
		/// Property to return the number of assigned keys in this track.
		/// </summary>
		public int KeyCount
		{
			get
			{
				return _keys.Count;
			}
		}
		
		/// <summary>
        /// Property to return the owner of the track.
        /// </summary>
        public Animation Owner
        {
            get
            {
                return _owner;
            }
        }

        /// <summary>
        /// Property to return the key for a given frame time index.
        /// </summary>
        /// <param name="timeIndex">Frame time index to retrieve.</param>
        /// <returns>A key containing interpolated keyframe data.</returns>
		public abstract KeyFrame this[float timeIndex]
		{
			get;
		}
        #endregion

        #region Methods.
		/// <summary>
		/// Function to create a keyframe.
		/// </summary>
		/// <returns>The new keyframe in the correct context.</returns>
		protected internal abstract KeyFrame CreateKey();

		/// <summary>
		/// Function to set an animation as the owner of this track.
		/// </summary>
		/// <param name="owner">Owner of the animation.</param>
		internal void SetAnimationOwner(Animation owner)
		{
			_owner = owner;
		}

		/// <summary>
		/// Function to scale the keys by a time value.
		/// </summary>
		/// <param name="scaleValue">Value to scale by.</param>
		public void ScaleKeys(float scaleValue)
		{
			KeyFrame[] keys;		// Key values.

			if (scaleValue < 0.0001f)
				return;

			if (MathUtility.EqualFloat(scaleValue, 1.0f))
				return;

			// Create key array.
			keys = new KeyFrame[_keys.Count];

			// Copy.
			for (int i = 0; i < _keys.Count; i++)
			{
				keys[i] = _keys[_keys.Keys[i]];
				keys[i].Time *= scaleValue;
			}

			_keys.Clear();

			// Re-add to collection.
			foreach (KeyFrame key in keys)
				_keys.Add(key.Time, key);
			_keysUpdated = true;
		}

		/// <summary>
		/// Function to return the key at the index.
		/// </summary>
		/// <param name="index">Index of the key to retrieve.</param>
		public KeyFrame GetKeyAtIndex(int index)
		{
			if ((index < 0) || (index >= _keys.Count))
                throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");
			return _keys[_keys.Keys[index]];
		}

		/// <summary>
		/// Function to clear the keys.
		/// </summary>
		public void ClearKeys()
		{
			_keys.Clear();
			_keysUpdated = true;
		}

		/// <summary>
		/// Function to remove a key by index from the track.
		/// </summary>
		/// <param name="index">Index of the key to remove.</param>
		public void RemoveKeyAtIndex(int index)
		{
			if ((index < 0) || (index >= _keys.Count))
                throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

			// Remove the key.
			_keys.RemoveAt(index);
		}

		/// <summary>
		/// Function to remove a key at a given time interval.
		/// </summary>
		/// <param name="time">Time position to remove key from.</param>
		public void Remove(float time)
		{
			if (!Contains(time))
				throw new ArgumentOutOfRangeException("There is no key at time index " + time.ToString("0.000") + ".", "time");

			_keys.Remove(time);
		}

		/// <summary>
		/// Function to determine if a key exists at a given time or not.
		/// </summary>
		/// <param name="time">Time to check.</param>
		/// <returns>TRUE if a key exists, FALSE if not.</returns>
		public bool Contains(float time)
		{
			for (int i = 0; i < _keys.Keys.Count; i++)
			{
				if (MathUtility.EqualFloat(_keys.Keys[i], time, 0.001f))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Function to return the index of a given key time.
		/// </summary>
		/// <param name="time">Time to check.</param>
		/// <returns>The index of the key, or -1 if not found.</returns>
		public int IndexOfKey(float time)
		{
			for (int i = 0; i < _keys.Keys.Count; i++)
			{
				if (MathUtility.EqualFloat(_keys.Keys[i], time, 0.001f))
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Function to add a key to the track.
		/// </summary>
		/// <param name="key">Key to add.</param>
		public void AddKey(KeyFrame key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (key.DataType != _dataType)
				throw new ArgumentException("The key type '" + key.DataType.FullName + "' is not the expected type '" + _dataType.FullName + "'.", "key");

			key.Owner = this;
			if (!Contains(key.Time))
				KeyList.Add(key.Time, key);
			else
				KeyList[key.Time] = key;
		}

		/// <summary>
		/// Function to find the nearest keys given frame time index.
        /// </summary>
        /// <param name="timeIndex">Time index to check.</param>
        /// <returns>A structure containing data on the point in time within the track.</returns>
        public NearestKeys FindNearest(float timeIndex)
        {
            return new NearestKeys(this, timeIndex);
        }

		/// <summary>
		/// Function to copy keys from one track to another.
		/// </summary>
		/// <param name="track">Track to copy from.</param>
		public void CopyKeysFrom(Track track)
		{
			if (track == null)
				throw new ArgumentNullException("track");

			if (track.KeyCount < 1)
				throw new ArgumentException("The source track contains 0 keys.", "track");

			if (track.GetKeyAtIndex(0).DataType != _dataType)
				throw new ArgumentException("The key type '" + track.GetKeyAtIndex(0).DataType.FullName + "' is not the expected type '" + _dataType.FullName + "'.", "key");

			ClearKeys();

			foreach (KeyFrame key in track)
			{
				if (key.Time <= _owner.Length)
				{
					KeyFrame newKey = key.Clone();
					AddKey(key);
				}
			}
		}
        #endregion

        #region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Track"/> class.
		/// </summary>
		/// <param name="name">Name of the track.</param>
		/// <param name="type">Type represented by the track.</param>
		protected Track(string name, Type type)
			: base(name)
		{
			EditCanDragValues = EditorDragType.None;
			_owner = null;
			_dataType = type;
			_sorter = new FrameTimeSort();
			_keys = new SortedList<float, KeyFrame>(_sorter);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Track"/> class.
		/// </summary>
		/// <param name="property">Property that is bound to the track.</param>
		/// <param name="type">Type represented by the track.</param>
        internal Track(PropertyInfo property, Type type)
			: this(property.Name, type)
        {
			_property = property;
		}
        #endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyFrame> GetEnumerator()
		{
			foreach (KeyValuePair<float, KeyFrame> key in _keys)
				yield return key.Value;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _keys.GetEnumerator();
		}
		#endregion
	}
}

