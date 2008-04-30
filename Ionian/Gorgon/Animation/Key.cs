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
	public abstract class Key
		: ICloneable<Key>
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
					_owner.Update();
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
					_owner.Update();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to assign this key to a track.
		/// </summary>
		/// <param name="trackOwner">Track that will own this key.</param>
		protected virtual internal void AssignToTrack(Track trackOwner)
		{
			if (trackOwner == null)
				throw new ArgumentNullException("trackOwner");

			_owner = trackOwner;
			_owner.Update();
		}

		/// <summary>
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		protected internal abstract void Apply(int prevKeyIndex, Key previousKey, Key nextKey);

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		protected internal abstract void UpdateLayerObject(Renderable layerObject);

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// Function to copy this key into a new time.
		/// </summary>
		/// <param name="newTime">Time index to place the copy into.</param>
		/// <returns>The copy of the this key.</returns>
		public virtual Key CopyTo(float newTime)
		{
			Key copy = (Key)Clone();		// Create a copy.

			// Assign the time and owner.
			copy.Time = newTime;
			Owner.AddKey(copy);

			return copy;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="trackOwner">Track that owns this key.</param>
		/// <param name="time">Time at which this key will reside.</param>
		protected Key(Track trackOwner, float time)
		{
			_owner = trackOwner;
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
		public abstract Key Clone();
		#endregion
	}
}
