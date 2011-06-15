#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
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
		: BaseCollection<Track>
	{
		#region Properties.
		/// <summary>
		/// Property to return a track by its name.
		/// </summary>
		public Track this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}

		/// <summary>
		/// Property to return a track by its index.
		/// </summary>
		public Track this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a new track to the collection.
		/// </summary>
		/// <param name="track">Track to add.</param>
		internal void Add(Track track)
		{
			if (track == null)
				throw new ArgumentNullException("track");

			AddItem(track.Name, track);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackCollection"/> class.
		/// </summary>
		internal TrackCollection()
			: base(32, false)
		{			
		}
		#endregion
	}
}
