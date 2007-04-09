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
// Created: Thursday, November 23, 2006 9:43:46 AM
// 
#endregion

using System;
using System.Collections.Generic;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Interface for key objects.
	/// </summary>
	/// <typeparam name="K">Type of key.</typeparam>
	public interface IKey<K>
		where K : IKey<K>
	{
		#region Properties.
        /// <summary>
        /// Property to set or return the interpolation mode.
        /// </summary>
		InterpolationMode InterpolationMode
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the time index (in milliseconds) for this keyframe.
        /// </summary>
		float Time
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the owning track for this key.
		/// </summary>
		Track<K> Owner
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		void Apply(int prevKeyIndex, K previousKey, K nextKey);

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		void UpdateLayerObject(IAnimatable layerObject);

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		K Clone();

		/// <summary>
		/// Function to copy this key into a new time.
		/// </summary>
		/// <param name="newTime">Time index to place the copy into.</param>
		/// <returns>The copy of the this key.</returns>
		K CopyTo(float newTime);
		#endregion
	}
}
