#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, September 3, 2012 8:25:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A collection of animation tracks.
	/// </summary>
	/// 
	public class GorgonAnimationTrackCollection
		: GorgonBaseNamedObjectDictionary<GorgonAnimationTrack>
	{
		#region Properties.
		/// <summary>
		/// Property to return the specified track from the collection.
		/// </summary>
		public GorgonAnimationTrack this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a track to the collection.
		/// </summary>
		/// <param name="track">Track to add.</param>
		internal void AddTrack(GorgonAnimationTrack track)
		{
			AddItem(track);
		}

		/// <summary>
		/// Function to remove all the tracks from the collection.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationTrackCollection" /> class.
		/// </summary>
		internal GorgonAnimationTrackCollection()
			: base(false)
		{
		}
		#endregion
	}
}
