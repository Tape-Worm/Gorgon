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
// Created: Monday, September 3, 2012 8:29:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An animation clip for a renderable object.
	/// </summary>
	public class GorgonAnimationClip
		: GorgonNamedObject
	{
		#region Variables.
		private IRenderable _renderable = null;
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the renderable that owns this animation.
        /// </summary>
        public IRenderable Renderable
        {
            get
            {
                return _renderable;
            }
            internal set
            {
                if (_renderable == value)
                    return;

                _renderable = value;
                GetTracks();
            }
        }
        
        /// <summary>
		/// Property to return the list of tracks for the animation.
		/// </summary>
		public GorgonAnimationTrackCollection Tracks
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set up the track list.
		/// </summary>
		private void GetTracks()
		{
            if (Tracks == null)
                Tracks = new GorgonAnimationTrackCollection();
            else
                Tracks.Clear();

			if (_renderable == null)
				return;

			foreach (var property in _renderable.Animations.RenderableProperties)
				Tracks.AddTrack(new GorgonAnimationTrack(property.Key, property.Value));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationClip" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public GorgonAnimationClip(string name)
			: base(name)
		{
			Tracks = new GorgonAnimationTrackCollection();
		}
		#endregion		
	}
}
