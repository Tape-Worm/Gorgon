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

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a track of transform keys.
	/// </summary>
	public class TrackTransform
		: Track
	{
		#region Properties.
		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <param name="timeIndex">Frame time index to retrieve.</param>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override Key this[float timeIndex]
		{
            get
            {
                KeyTransform newKey = null;				// Key information.
                NearestKeys keyData;					// Nearest key information.

				// If we specify the exact key, then return it.
				if (Contains(timeIndex))
					return KeyList[timeIndex];
				
				// Get the nearest key information.
                keyData = FindNearest(timeIndex);

                // Get an instance of the key.
				newKey = new KeyTransform(this, keyData.KeyTimeDelta);

                // Apply the transformation.
				newKey.Apply(keyData.PreviousKeyIndex, keyData.PreviousKey, keyData.NextKey);

                return newKey;
            }
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a key to the track.
		/// </summary>
		/// <param name="key">Key to add.</param>
		public override void AddKey(Key key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (key.Owner != null)
				throw new AnimationKeyAssignedException(GetType());

			// Assign the key owner.
			key.AssignToTrack(this);
			if (!Contains(key.Time))
				KeyList.Add(key.Time, key);
			else
				KeyList[key.Time] = key;
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

			if (!Contains(timePosition))
				KeyList.Add(timePosition, newKey);
			else
				KeyList[timePosition] = newKey;

			Update();
			return newKey;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Animation that owns this track.</param>
		internal TrackTransform(Animation owner)
			: base(owner)
		{
		}
		#endregion
	}
}
