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
// Created: Thursday, November 23, 2006 9:52:36 AM
// 
#endregion

using System;
using System.Collections.Generic;
using SharpUtilities.Collections;
using SharpUtilities.Mathematics;

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Object representing a track of transform keys.
	/// </summary>
	public class TrackTransform
		: Track<KeyTransform>
	{
		#region Properties.
		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <param name="timeIndex">Frame time index to retrieve.</param>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override KeyTransform this[float timeIndex]
		{
            get
            {
                KeyTransform newKey = null;				// Key information.
                NearestKeys keyData;					// Nearest key information.

                // Get the nearest key information.
                keyData = FindNearest(timeIndex);

                // Get an instance of the key.
				newKey = new KeyTransform(this, keyData.KeyTimeDelta);

                // Apply the transformation.
                ((IKey<KeyTransform>)newKey).Apply(keyData.PreviousKeyIndex, keyData.PreviousKey, keyData.NextKey);

                return newKey;
            }
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a key to the track.
		/// </summary>
		/// <param name="key">Key to add.</param>
		public override void AddKey(KeyTransform key)
		{
            if (key.Owner != null)
                throw new KeyAlreadyAssignedException(GetType(), null);

			// Assign the key owner.
			key.AssignKey(this);
			_keys.Add(key);
			SortKeys();
		}

		/// <summary>
		/// Function to create a transformation key.
		/// </summary>
        /// <param name="timePosition">Time index for the key.</param>
        /// <param name="position">Position for the object.</param>
        /// <param name="scale">Scale for the object.</param>
        /// <param name="rotation">Rotation for the object.</param>
		/// <returns>A new transformation key.</returns>
		public KeyTransform CreateKey(float timePosition, Vector2D position, Vector2D scale, float rotation)
		{
			KeyTransform newKey = null;         // New transform key.

			newKey = new KeyTransform(this, timePosition);
			newKey.Position = position;
			newKey.Scale = scale;
			newKey.Rotation = rotation;

			_keys.Add(newKey);
			SortKeys();

			_keysUpdated = true;
			return newKey;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Animation that owns this track.</param>
		/// <param name="name">Name of the track.</param>
		internal TrackTransform(Animation owner, string name)
			: base(owner, name)
		{
		}
		#endregion
	}
}
