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

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a track of frame switch keys.
	/// </summary>
	public class TrackFrame
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
                Key newKey = null;			// Key information.
                Key currentKey = null;      // Current key.

				// If we specify the exact key, then return it.
				if (Contains(timeIndex))
					return KeyList[timeIndex];
				
				// Get first frame.
                newKey = GetKeyAtIndex(0);

                // Find the key that matches the time index.
				for (int i = 0; i < KeyList.Count; i++)
                {
                    currentKey = GetKeyAtIndex(i);

                    if (currentKey.Time <= timeIndex)                    
                    {
						newKey = GetKeyAtIndex(i);
                        // If we have a key that matches the time index, then return it.
                        if (currentKey.Time == timeIndex)
                            return newKey;
                    }
                }

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
		/// Function to create a frame key.
		/// </summary>
		/// <param name="timePosition">Time index for the key.</param>
		/// <param name="image">Image bound to the frame.</param>
		/// <param name="offset">Offset within the bound image.</param>
		/// <param name="size">Size of the frame.</param>
		/// <returns>A new frame key.</returns>
		public KeyFrame CreateKey(float timePosition, Image image, Vector2D offset, Vector2D size)
		{
			KeyFrame newKey = null;         // New transform key.

			newKey = new KeyFrame(this, timePosition);
			newKey.Frame = new Frame(image, offset, size);

			if (!Contains(timePosition))
				KeyList.Add(timePosition, newKey);
			else
				KeyList[timePosition] = newKey;

			Update();
			return newKey;
		}

		/// <summary>
		/// Function to create a frame key.
		/// </summary>
		/// <param name="timePosition">Time index for the key.</param>
		/// <param name="frame">Frame information.</param>
		/// <returns></returns>
		public KeyFrame CreateKey(float timePosition, Frame frame)
		{
			return CreateKey(timePosition, frame.Image, frame.Offset, frame.Size);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Animation that owns this track.</param>
		internal TrackFrame(Animation owner)
			: base(owner)
		{
		}
		#endregion
	}
}
