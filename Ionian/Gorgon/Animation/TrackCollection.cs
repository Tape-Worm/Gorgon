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
// Created: Wednesday, April 30, 2008 10:53:15 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of animation tracks.
	/// </summary>
	public class TrackCollection
		: Collection<Track>
	{
		#region Methods.
		/// <summary>
		/// Function to add a new track to the collection.
		/// </summary>
		/// <param name="track">Track to add.</param>
		public void Add(Track track)
		{
			if (track == null)
				throw new ArgumentNullException("track");
			if (Contains(track.Name))
				throw new AnimationTrackAlreadyExistsException(track.Name);

			AddItem(track.Name, track);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackCollection"/> class.
		/// </summary>
		internal TrackCollection()
			: base(8, false)
		{
			
		}
		#endregion
	}
}
