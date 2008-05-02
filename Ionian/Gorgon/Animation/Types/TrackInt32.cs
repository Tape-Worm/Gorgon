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
	/// Animation track used to animate floating point properties.
	/// </summary>
	public class TrackInt32
		: Track
	{
		#region Properties.
		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override KeyFrame this[float timeIndex]
		{
			get
			{
				KeyInt32 newKey = null;					// Key information.
				NearestKeys keyData;					// Nearest key information.

				// If we specify the exact key, then return it.
				if (Contains(timeIndex))
					return KeyList[timeIndex];

				// If we're at the last key, then don't progress any further.
				if (timeIndex > GetKeyAtIndex(KeyList.Count - 1).Time)
					return GetKeyAtIndex(KeyList.Count - 1);

				// Get the nearest key information.
				keyData = FindNearest(timeIndex);

				// Get an instance of the key.
				newKey = new KeyInt32(keyData.KeyTimeDelta, 0);
				newKey.Owner = this;				

				// Apply the transformation.
				newKey.UpdateKeyData(keyData);

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
			return new KeyInt32(0.0f, 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackVector2D"/> class.
		/// </summary>
		/// <param name="property">Property that is bound to the track.</param>
		internal TrackInt32(PropertyInfo property)
			: base(property, typeof(Int32))
		{
		}
		#endregion
	}
}
