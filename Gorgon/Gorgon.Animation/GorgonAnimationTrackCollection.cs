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
using Gorgon.Animation.Properties;
using Gorgon.Collections;

namespace Gorgon.Animation
{
	/// <summary>
	/// A collection of animation tracks.
	/// </summary>
	/// <typeparam name="T">Type of object that's being animated.</typeparam>
	public class GorgonAnimationTrackCollection<T>
		: GorgonBaseNamedObjectDictionary<GorgonAnimationTrack<T>>
		where T : class
	{
		#region Variables.
		private readonly GorgonAnimation<T> _animation;          // Animation that owns this collection.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the specified track from the collection.
		/// </summary>
		public GorgonAnimationTrack<T> this[string name]
		{
			get
			{
				return Items[name];
			}
			set
			{
				if (value == null)
				{
					if (Contains(name))
					{
						Remove(name);
					}

					return;
				}

				if (Contains(name))
				{
					UpdateItem(name, value);
					value.Animation = _animation;
				}
				else
				{
					Add(value);
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the track list for the animated object.
		/// </summary>
		internal void EnumerateTracks()
		{
			if (_animation.AnimationController == null)
			{
				return;
			}

			// Enumerate tracks from the owner object animated properties list.
			foreach (var item in _animation.AnimationController.AnimatedProperties)
			{
				if (Contains(item.Value.DisplayName)) // Don't add tracks that are already here.
				{
					continue;
				}

				switch (item.Value.DataType.FullName.ToUpperInvariant())
				{
					case "SYSTEM.BYTE":
						Add(new GorgonTrackByte<T>(item.Value));
						break;
					case "SYSTEM.SBYTE":
						Add(new GorgonTrackSByte<T>(item.Value));
						break;
					case "SYSTEM.INT16":
						Add(new GorgonTrackInt16<T>(item.Value));
						break;
					case "SYSTEM.UINT16":
						Add(new GorgonTrackUInt16<T>(item.Value));
						break;
					case "SYSTEM.INT32":
						Add(new GorgonTrackInt32<T>(item.Value));
						break;
					case "SYSTEM.UINT32":
						Add(new GorgonTrackUInt32<T>(item.Value));
						break;
					case "SYSTEM.INT64":
						Add(new GorgonTrackInt64<T>(item.Value));
						break;
					case "SYSTEM.UINT64":
						Add(new GorgonTrackUInt64<T>(item.Value));
						break;
					case "SYSTEM.SINGLE":
						Add(new GorgonTrackSingle<T>(item.Value));
						break;
					case "SLIMMATH.VECTOR2":
						Add(new GorgonTrackVector2<T>(item.Value));
						break;
					case "SLIMMATH.VECTOR3":
						Add(new GorgonTrackVector3<T>(item.Value));
						break;
					case "SLIMMATH.VECTOR4":
						Add(new GorgonTrackVector4<T>(item.Value));
						break;
					case "GORGON.GRAPHICS.GORGONTEXTURE2D":
						// We need grab an additional property for texture animation.
						var property = new GorgonAnimatedProperty(_animation.AnimationController.AnimatedObjectType.GetProperty("TextureRegion"));
						Add(new GorgonTrackTexture2D<T>(item.Value, property));
						break;
					case "GORGON.GRAPHICS.GORGONCOLOR":
						Add(new GorgonTrackGorgonColor<T>(item.Value));
						break;
				}
			}
		}

	    /// <summary>
	    /// Function to add a track to the collection.
	    /// </summary>
	    /// <param name="track">Track to add.</param>
	    /// <exception cref="ArgumentException"></exception>
	    /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="track"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
	    /// <exception cref="System.ArgumentException">Thrown when the track parameter already exists in the collection.</exception>
	    public void Add(GorgonAnimationTrack<T> track)
		{
		    if (track == null)
		    {
			    throw new ArgumentNullException("track");
		    }

		    if (Contains(track.Name))
		    {
		        throw new ArgumentException(string.Format(Resources.GORANM_TRACK_ALREADY_EXISTS, track.Name), "track");
		    }

			Items.Add(track.Name, track);
			track.Animation = _animation;
		}

		/// <summary>
		/// Function to remove a track from the collection.
		/// </summary>
		/// <param name="track">Track to remove from the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="track"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public void Remove(GorgonAnimationTrack<T> track)
		{
			if (track == null)
			{
				throw new ArgumentNullException("track");
			}

			track.Animation = null;
			Items.Remove(track.Name);
		}

		/// <summary>
		/// Function to remove a track by its name.
		/// </summary>
		/// <param name="name">Name of the track to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the track could not be found in the collection.</exception>
		public void Remove(string name)
		{
		    if (name == null)
		    {
		        throw new ArgumentNullException("name");
		    }

		    if (name.Length == 0)
		    {
		        throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "name");
		    }

			GorgonAnimationTrack<T> track;

			if (!Items.TryGetValue(name, out track))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORANM_TRACK_DOES_NOT_EXIST, name));
			}

			track.Animation = null;
		    Items.Remove(name);
		}

		/// <summary>
		/// Function to remove all the tracks from the collection.
		/// </summary>
		public void Clear()
		{
			Items.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationTrackCollection{T}" /> class.
		/// </summary>
		/// <param name="animation">The animation that owns this collection.</param>
		internal GorgonAnimationTrackCollection(GorgonAnimation<T> animation)
			: base(false)
		{
			_animation = animation;
		}
		#endregion
	}
}
