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
using System.Drawing;

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Object representing a track of color keys.
	/// </summary>
	public class TrackColor
		: Track<KeyColor>
	{
		#region Properties.
		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <param name="timeIndex">Frame time index to retrieve.</param>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override KeyColor this[float timeIndex]
		{
            get
            {
                KeyColor newKey = null;				// Key information.
                NearestKeys keyData;					// Nearest key information.

                // Get the nearest key information.
                keyData = FindNearest(timeIndex);

                // Get an instance of the key.
				newKey = new KeyColor(this, keyData.KeyTimeDelta);

                // Apply the color.
                ((IKey<KeyColor>)newKey).Apply(keyData.PreviousKeyIndex, keyData.PreviousKey, keyData.NextKey);

                return newKey;
            }
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a key to the track.
		/// </summary>
		/// <param name="key">Key to add.</param>
		public override void AddKey(KeyColor key)
		{
            if (key.Owner != null)
                throw new KeyAlreadyAssignedException(GetType(), null);

			// Assign the key owner.
			key.AssignKey(this);
			_keys.Add(key);
			SortKeys();
		}

		/// <summary>
		/// Function to create a color key.
		/// </summary>
		/// <returns>A new color key.</returns>
		public KeyColor CreateKey(float timePosition, Color color)
		{
			KeyColor newKey = null;         // New transform key.

			newKey = new KeyColor(this, timePosition);
            newKey.Color = color;

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
		internal TrackColor(Animation owner, string name)
			: base(owner, name)
		{
		}
		#endregion
	}
}
