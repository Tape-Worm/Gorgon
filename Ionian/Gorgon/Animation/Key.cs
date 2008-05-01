#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Monday, August 06, 2007 9:19:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Abstract object representing an animation key.
	/// </summary>
	public abstract class KeyFrame
		: ICloneable<KeyFrame>
	{
		#region Variables.
		private Track _owner = null;				// Track that owns this key.
		private float _frameTime;					// Frame time.
		private InterpolationMode _interpolation;	// Interpolation mode.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the interpolation mode.
		/// </summary>
		/// <value></value>
		public virtual InterpolationMode InterpolationMode
		{
			get
			{
				return _interpolation;
			}
			set
			{
				_interpolation = value;
				if (_owner != null)
					_owner.NeedsUpdate = true;
			}
		}

		/// <summary>
		/// Property to return the track that owns this key.
		/// </summary>
		public Track Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				if (_owner != null)
					_owner.NeedsUpdate = true;
				_owner = value;
				if (_owner != null)
					_owner.NeedsUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the time index (in milliseconds) for this keyframe.
		/// </summary>
		public float Time
		{
			get
			{
				return _frameTime;
			}
			set
			{
				_frameTime = value;
				if (_owner != null)
					_owner.NeedsUpdate = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the data within the key frame.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		protected internal abstract void UpdateKeyData(int prevKeyIndex, KeyFrame previousKey, KeyFrame nextKey);

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// Function to copy this key into a new time.
		/// </summary>
		/// <param name="newTime">Time index to place the copy into.</param>
		/// <returns>The copy of the this key.</returns>
		public virtual KeyFrame CopyTo(float newTime)
		{
			KeyFrame copy = Clone();		// Create a copy.

			// Assign the time and owner.
			copy.Time = newTime;

			if (Owner != null)
				Owner.AddKey(copy);

			return copy;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="time">Time at which this key will reside.</param>
		protected KeyFrame(float time)
		{
			_owner = null;
			Time = time;
			_interpolation = InterpolationMode.None;
		}
		#endregion

		#region ICloneable<T> Members
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public abstract KeyFrame Clone();
		#endregion
	}
}
