#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 10:50:41 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Animation track used to animate byte properties.
	/// </summary>
	public class TrackImage
		: Track
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the interpolation mode for the track.
		/// </summary>
		/// <remarks>Image tracks don't use interpolation, and thus only use InterpolationMode.None.</remarks>
		public override InterpolationMode InterpolationMode
		{
			get
			{
				return InterpolationMode.None;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override KeyFrame this[float timeIndex]
		{
			get
			{
				KeyFrame newKey = null;				// Key information.
				KeyFrame currentKey = null;			// Current key.

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
		/// Function to create a keyframe.
		/// </summary>
		/// <returns>The new keyframe in the correct context.</returns>
		protected internal override KeyFrame CreateKey()
		{
			return new KeyImage(0, null);
		}
		#endregion
		
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackImage"/> class.
		/// </summary>
		/// <param name="property">Property that is bound to the track.</param>
		internal TrackImage(PropertyInfo property)
			: base(property, typeof(Image))
		{
		}
		#endregion
	}
}
