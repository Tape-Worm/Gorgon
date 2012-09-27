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
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An animation clip for an animated object.
	/// </summary>
	public class GorgonAnimationClip
		: GorgonNamedObject
	{
		#region Variables.
		private IAnimated _owner = null;
		private float _length = 0;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this animation is currently playing or not.
		/// </summary>
		public bool IsPlaying
		{
			get
			{
				if (_owner == null)
					return false;

				return _owner.Animations.CurrentAnimation == this;
			}
		}

		/// <summary>
		/// Property to set or return the length of the animation (in milliseconds).
		/// </summary>
		public float Length
		{
			get
			{
				return _length;
			}
			set
			{
				if (value < 0)
					value = 0;

				_length = value;
			}
		}

		/// <summary>
		/// Property to return the object that owns this animation.
		/// </summary>
		public IAnimated Owner
		{
			get
			{
				return _owner;
			}
			internal set
			{
				if (_owner == value)
					return;

				_owner = value;
				GetTracks(_owner);
			}
		}

		/// <summary>
		/// Property to set or return whether this animation should be looping or not.
		/// </summary>
		public bool IsLooped
		{
			get;
			set;
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
		/// Function to build the track list for the owner object.
		/// </summary>
		/// <param name="owner">The object that contains the properties to animate.</param>
		internal void GetTracks(IAnimated owner)
		{
			if (owner == null)
				return;

			// Enumerate tracks from the owner object animated properties list.
			foreach (var item in owner.Animations.AnimatedProperties)
			{
				if (Tracks.Contains(item.Value.DisplayName))		// Don't add tracks that are already here.
					continue;

				switch (item.Value.DataType.FullName.ToLower())
				{
					case "slimmmath.vector2":						
						Tracks.AddTrack(new GorgonTrackVector2(this, item.Value));
						break;
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationClip" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="length">The length of the animation, in milliseconds.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonAnimationClip(string name, float length)
			: base(name)
		{
			GorgonDebug.AssertParamString(name, "name");

			Tracks = new GorgonAnimationTrackCollection();
			Length = length;
		}
		#endregion		
	}
}
