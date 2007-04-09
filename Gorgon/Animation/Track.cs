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
// Created: Monday, November 20, 2006 1:32:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using SharpUtilities;
using SharpUtilities.Collections;

namespace GorgonLibrary.Graphics.Animations
{
    /// <summary>
    /// Object representing a track in an animation.
    /// </summary>
    /// <typeparam name="T">Type of key within the track.</typeparam>
    public abstract class Track<T>
        : NamedObject
		where T : IKey<T>
    {
        #region Value Types.
        /// <summary>
        /// Value type containing information about the nearest keys for a time position.
        /// </summary>
        public struct NearestKeys
        {
            #region Variables.
            private float _timePosition;            // Time position within the track that lies between the two keys.
            private T _previousKey;                 // Keyframe prior to the time position.
            private T _nextKey;                     // Keyframe after the time position.
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
            public T PreviousKey
            {
                get
                {
                    return _previousKey;
                }
            }

            /// <summary>
            /// Property to return the next key from the time position.
            /// </summary>
            public T NextKey
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
            internal NearestKeys(Track<T> owner, float requestedTime)
            {
                float animationLength = owner.Owner.Length;     // Animation length.
				int i = 0;										// Loop.

                if (owner.KeyCount < 1)
                    throw new NoKeysInTrackException(null);

                // Initialize.
                _previousKeyIndex = 0;
                _previousKey = owner.GetAssignedKey(0);
                _nextKeyIndex = -1;                
                _nextKey = default(T);
                _keyTimeDelta = 0;                

                // Wrap around.
                while (requestedTime > animationLength)
                    requestedTime -= animationLength;

                // Find previous key.                
				if (owner.GetAssignedKey(0).Time <= requestedTime)
				{
					for (i = 0; i < owner.KeyCount; i++)
					{
						if ((owner.GetAssignedKey(i).Time > requestedTime) && (i > 0))
						{
							_previousKey = owner.GetAssignedKey(i - 1);
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
                        _nextKey = owner.GetAssignedKey(owner.KeyCount - 1);
                        _nextKeyIndex = owner.KeyCount - 1;
                    }
                    else
                    {
                        _nextKey = owner.GetAssignedKey(0);
                        _nextKeyIndex = 0;
                    }
                    _keyTimeDelta = animationLength;
                }
                else
                {
                    _nextKey = owner.GetAssignedKey(i);
                    _nextKeyIndex = i;
                    _keyTimeDelta = _nextKey.Time;
                }

                // Same frame.
                if (_previousKey.Time == _keyTimeDelta)
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
			: IComparer<T>
		{
			#region IComparer<KeyTransform> Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
			/// </returns>
			public int Compare(T x, T y)
			{
				if (x.Time < y.Time)
					return -1;

				return 1;
			}
			#endregion
		}
		#endregion

        #region Variables.
		private FrameTimeSort _sorter = null;   // Frame time sorter.

		/// <summary>Animation that owns this track.</summary>
		protected Animation _owner = null;
		/// <summary>List of key frames.</summary>
		protected List<T> _keys = null;
		/// <summary>Flag to indicate that keys have been updated.</summary>
		protected bool _keysUpdated = true;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to set or return whether the keys have been updated.
		/// </summary>
		internal bool Updated
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
		public abstract T this[float timeIndex]
		{
			get;
		}
        #endregion

        #region Methods.
		/// <summary>
		/// Function to return the key at the index.
		/// </summary>
		/// <param name="index">Index of the key to retrieve.</param>
		public T GetAssignedKey(int index)
		{
			if ((index < 0) || (index >= _keys.Count))
				throw new IndexOutOfBoundsException(index);
			return _keys[index];
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
		/// Function to remove a key from the track.
		/// </summary>
		/// <param name="index">Index of the key to remove.</param>
		public void RemoveKey(int index)
		{
			if ((index < 0) || (index >= _keys.Count))
				throw new IndexOutOfBoundsException(index);

			// Remove the key.
			_keys.RemoveAt(index);
			SortKeys();
		}

		/// <summary>
		/// Function to sort the keys by frame time.
		/// </summary>
		public virtual void SortKeys()
		{
			_keys.Sort(_sorter);
			_keysUpdated = true;
		}


		/// <summary>
		/// Function to add a key to the track.
		/// </summary>
		/// <param name="key">Key to add.</param>
		public abstract void AddKey(T key);

		/// <summary>
		/// Function to find the nearest keys given frame time index.
        /// </summary>
        /// <param name="timeIndex">Time index to check.</param>
        /// <returns>A structure containing data on the point in time within the track.</returns>
        public NearestKeys FindNearest(float timeIndex)
        {
            return new NearestKeys(this, timeIndex);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        internal Track(Animation owner, string name)
            : base(name)
        {
			_owner = owner;
			_keys = new List<T>();
			_sorter = new FrameTimeSort();
        }
        #endregion
    }
}
