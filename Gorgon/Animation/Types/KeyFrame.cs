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
// Created: Tuesday, November 21, 2006 12:08:22 AM
// 
#endregion

using System;
using SharpUtilities.Mathematics;
using System.Drawing;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Object representing a frame switch key.
	/// </summary>
	public class KeyFrame
		: IKey<KeyFrame>
	{
		#region Variables.
		private Frame _frame;								// Frame for the animation.
		private TrackFrame _owner;						    // Owning track.
		private InterpolationMode _interpolation;			// Interpolation mode.
		private float _frameTime = 0;						// Time index for the keyframe.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the frame.
        /// </summary>
        public Frame Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                if (_owner != null)
                    _owner.Updated = true;
            }
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to assign this key to a track.
		/// </summary>
		/// <param name="track">Track to assign to.</param>
		internal void AssignKey(TrackFrame track)
		{
			_owner = track;
			_owner.Updated = true;
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">_owner of this key.</param>
		/// <param name="timePosition">Position in time for this keyframe.</param>
		public KeyFrame(TrackFrame owner, float timePosition)
		{
			_owner = owner;
			_interpolation = InterpolationMode.Linear;
			_frameTime = timePosition;

			// Get the size from the animation owner.
			if (_owner != null)
				_frame = new Frame(_owner.Owner.Owner.Image, _owner.Owner.Owner.ImageOffset, _owner.Owner.Owner.Size);
		}
		#endregion

		#region IKey<KeyFrame> Members
		#region Properties.
		/// <summary>
		/// Property to set or return the interpolation mode.
		/// </summary>
		/// <value></value>
		public InterpolationMode InterpolationMode
		{
			get
			{
				return _interpolation;
			}
			set
			{
				_interpolation = value;
				if (_owner != null)
					_owner.Updated = true;
			}
		}

		/// <summary>
		/// Property to set or return the time index (in milliseconds) for this keyframe.
		/// </summary>
		/// <value></value>
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
					_owner.Updated = true;
			}
		}

		/// <summary>
		/// Property to return the owning track for this key.
		/// </summary>
		/// <value></value>
		public Track<KeyFrame> Owner
		{
			get
			{
				return _owner;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		void IKey<KeyFrame>.Apply(int prevKeyIndex, KeyFrame previousKey, KeyFrame nextKey)
		{
            // Not necessary since we're not interpolating.
		}

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		void IKey<KeyFrame>.UpdateLayerObject(IAnimatable layerObject)
		{
			if (layerObject.Image != _frame.Image)
				layerObject.Image = _frame.Image;
			if (layerObject.ImageOffset != _frame.Offset)
				layerObject.ImageOffset = _frame.Offset;
			if (layerObject.Size != _frame.Size)
				layerObject.Size = _frame.Size;
		}

		/// <summary>
		/// Function to copy this key into a new time.
		/// </summary>
		/// <param name="newTime">Time index to place the copy into.</param>
		/// <returns>The copy of the this key.</returns>
		public KeyFrame CopyTo(float newTime)
		{
			KeyFrame copy = Clone();		// Create a copy.

			// Assign the time and owner.
			copy.Time = newTime;
			Owner.AddKey(copy);

			return copy;
		}

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		public KeyFrame Clone()
		{
			KeyFrame newKey = null;         // Cloned key.

			newKey = new KeyFrame(null, _frameTime);
			newKey.InterpolationMode = _interpolation;
			newKey.Frame = _frame;            

			return newKey;
		}
		#endregion
		#endregion
	}
}
