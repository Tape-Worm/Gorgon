#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, September 23, 2012 11:48:52 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Animation.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Animation
{
	/// <summary>
	/// A collection of animation key frames.
	/// </summary>
	/// <typeparam name="T">The type of object being animated.</typeparam>
	public class GorgonAnimationKeyFrameCollection<T>
		: IList<IKeyFrame>
		where T : class
	{
		#region Variables.
		private readonly List<IKeyFrame> _keyFrames;			// List of key frames.
		private readonly GorgonAnimationTrack<T> _track;		// Track that owns this collection.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of keys sorted by time.
		/// </summary>
		internal SortedList<float, IKeyFrame> Times
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether a key with the specified time exists in the collection.
		/// </summary>
		/// <param name="time">Time in the collection to look up.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool Contains(float time)
		{
			return Times.ContainsKey(time);
		}

		/// <summary>
		/// Function to add a list of keyframes to the collection.
		/// </summary>
		/// <param name="keyFrames">Keyframes to add.</param>
		/// <exception cref="System.ArgumentException">Thrown when a key frame already exists in the collection with the specified time index.</exception>
		public void AddRange(IEnumerable<IKeyFrame> keyFrames)
		{
		    IKeyFrame[] frames = keyFrames.ToArray();

		    if (frames.Length == 0)
		    {
		        return;
		    }

		    foreach (var item in frames)
		    {
		        Add(item);
		    }
		}

		/// <summary>
		/// Function to insert a list of key frames into the collection.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="keyFrames" /> should be inserted.</param>
		/// <param name="keyFrames">The key frames that should be inserted.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0, or greater than the number of elements in the collection.</exception>
		public void InsertRange(int index, IEnumerable<IKeyFrame> keyFrames)
		{
		    IKeyFrame[] frames = keyFrames.ToArray();

			GorgonDebug.AssertParamRange(index, 0, Count, "index");

			foreach (var key in frames)
			{
			    if (Contains(key.Time))
                {
			        throw new ArgumentException(string.Format(Resources.GORANM_KEY_EXISTS_AT_TIME, key.Time));
			    }

			    Times.Add(key.Time, key);
			}

			_keyFrames.InsertRange(index, frames);
			if (_track != null)
				_track.SetupSpline();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationKeyFrameCollection{T}" /> class.
		/// </summary>
		/// <param name="track">Track that owns this collection.</param>
		internal GorgonAnimationKeyFrameCollection(GorgonAnimationTrack<T> track)			
		{
			_keyFrames = new List<IKeyFrame>();
			Times = new SortedList<float, IKeyFrame>();
			_track = track;
		}
		#endregion

		#region IList<IKeyFrame> Members
		#region Properties.
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		public IKeyFrame this[int index]
		{
			get
			{
				return _keyFrames[index];
			}
			set
			{
				_keyFrames[index] = value;
				Times[value.Time] = value;
				if (_track != null)
					_track.SetupSpline();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <returns>
		/// The index of <paramref name="item" /> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(IKeyFrame item)
		{
			return _keyFrames.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="System.ArgumentException">Thrown when a key frame already exists in the collection with the specified time index.</exception>
		public void Insert(int index, IKeyFrame item)
		{
		    if (Contains(item.Time))
		    {
                throw new ArgumentException(string.Format(Resources.GORANM_KEY_EXISTS_AT_TIME, item.Time));
		    }

		    _keyFrames.Insert(index, item);
			Times.Add(item.Time, item);
			if (_track != null)
				_track.SetupSpline();
		}       

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public void RemoveAt(int index)
		{
			Times.Remove(_keyFrames[index].Time);
			_keyFrames.RemoveAt(index);
			if (_track != null)
				_track.SetupSpline();
		}
		#endregion
		#endregion

		#region ICollection<IKeyFrame> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count
		{
			get
			{
				return _keyFrames.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Property to add a key frame to the collection.
		/// </summary>
		/// <param name="item">Keyframe to add.</param>
		/// <exception cref="System.ArgumentException">Thrown when a key frame already exists in the collection with the specified time index.</exception>
		public void Add(IKeyFrame item)
		{
		    if (item.DataType != _track.DataType)
		    {
		        throw new InvalidCastException(string.Format(Resources.GORANM_KEY_TRACK_TYPE_MISMATCH, 
                                                    item.GetType().FullName, _track.GetType().FullName));
		    }

		    if (Contains(item.Time))
		    {
		        throw new ArgumentException(string.Format(Resources.GORANM_KEY_EXISTS_AT_TIME, item.Time));
		    }

		    _keyFrames.Add(item);
			Times.Add(item.Time, item);
			if (_track != null)
				_track.SetupSpline();
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			_keyFrames.Clear();
			Times.Clear();
			if (_track != null)
				_track.SetupSpline();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>		
		public bool Contains(IKeyFrame item)
		{
			return _keyFrames.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo(IKeyFrame[] array, int arrayIndex)
		{
			_keyFrames.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove(IKeyFrame item)
		{
			Times.Remove(item.Time);
			if (_track != null)
				_track.SetupSpline();
			return _keyFrames.Remove(item);
		}
		#endregion
		#endregion

		#region IEnumerable<IKeyFrame> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IKeyFrame> GetEnumerator()
		{
		    return _keyFrames.GetEnumerator();
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
}
