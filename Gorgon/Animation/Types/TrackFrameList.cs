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
// Created: Thursday, November 23, 2006 11:00:01 AM
// 
#endregion

using System;
using System.Collections.Generic;
using SharpUtilities.Collections;

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Object representing a list of frame switch tracks.
	/// </summary>
	public class TrackFrameList
		: TrackList<TrackFrame>
	{
		#region Method.
		/// <summary>
		/// Function to create a track.
		/// </summary>
		/// <param name="name">Name of the track.</param>
		/// <returns>A new track.</returns>
		public override TrackFrame Create(string name)
		{
			TrackFrame newTrack = null;       // New track.

			if (Contains(name))
				throw new DuplicateObjectException(name);

			newTrack = new TrackFrame(Owner, name);
			_items.Add(name, newTrack);
			return newTrack;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Animation that owns this object.</param>
		internal TrackFrameList(Animation owner)
			: base(owner)
		{
		}
		#endregion
	}
}
