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
using System.IO;
using System.Linq;
using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Animation
{
	/// <summary>
	/// An animation clip for an animated object.
	/// </summary>
	public class GorgonAnimation<T>
		: GorgonNamedObject, IGorgonCloneable<GorgonAnimation<T>>
		where T : class
	{        
		#region Variables.
		private float _length;                                  // Length of the animation, in milliseconds.
		private float _time;                                    // Current time for the animation, in milliseconds.
		private int _loopCount;                                 // Number of loops for the animation.
		private int _looped;                                    // Number of times the animation has currently looped.
		private GorgonAnimationController<T> _controller;       // Animation controller.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the animation controller that owns this clip.
		/// </summary>
		public GorgonAnimationController<T> AnimationController
		{
			get
			{
				return _controller;
			}
			internal set
			{
				if (value == _controller)
					return;

				_controller = value;
				Tracks.EnumerateTracks();
			}
		}

		/// <summary>
		/// Property to set or return the number of times to loop an animation.
		/// </summary>
		/// <remarks></remarks>
		public int LoopCount
		{
			get
			{
				return _loopCount;
			}
			set
			{
				if (value < 0)
					value = 0;
				_loopCount = value;
			}
		}

		/// <summary>
		/// Property to set or return the speed of the animation.
		/// </summary>
		/// <remarks>Setting this value to a negative value will make the animation play backwards.</remarks>
		public float Speed
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether this animation is currently playing or not.
		/// </summary>
		public bool IsPlaying
		{
			get
			{
				if (AnimationController == null)
					return false;
								
				return AnimationController.CurrentAnimation == this;
			}
		}

		/// <summary>
		/// Property to set or return the length of the animation (in seconds).
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
		/// Property to set or return whether this animation should be looping or not.
		/// </summary>
		public bool IsLooped
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current time, in seconds, for this animation.
		/// </summary>
		public float Time
		{
			get
			{
				return _time;
			}
			set
			{
				if ((_time.EqualsEpsilon(value)) || (_length <= 0))
				{
					return;
				}

				_time = value;

				if ((IsLooped) && ((_time > _length) || (_time < 0)))
				{
					// Loop the animation.
					if ((_loopCount != 0) && (_looped == _loopCount))
					{
						return;
					}
					
					_looped++;
					_time = _time % _length;

					if (_time < 0)
					{
						_time += _length;
					}

                    UpdateObject();
					return;
				}

				if (_time < 0)
				{
					_time = 0;
				}

				if (_time > _length)
				{
					_time = _length;
				}

                UpdateObject();
			}
		}
		
		/// <summary>
		/// Property to return the list of tracks for the animation.
		/// </summary>
		public GorgonAnimationTrackCollection<T> Tracks
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the owner of the animation.
		/// </summary>
		internal void UpdateObject()
		{
			if (AnimationController == null)
				return;

			// Notify each track to update their animation to the current time.
			foreach (var track in Tracks)
			{
			    if ((track.KeyFrames.Count <= 0) 
                    || ((track.TimeUpdatedCallback != null) 
                    && (!track.TimeUpdatedCallback(_controller.AnimatedObject, Time))))
			    {
			        continue;
			    }

				IKeyFrame key = track.GetKeyAtTime(_time);
				track.ApplyKey(ref key);
			}
		}

		/// <summary>
		/// Function to save the animation to a stream.
		/// </summary>
		/// <param name="stream">Stream to write the animation into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read-only.</exception>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GORANM_ERR_STREAM_READ_ONLY);
			}

			using (var chunk = new GorgonChunkWriter(stream))
			{
				chunk.Begin(GorgonAnimationController<T>.AnimationVersion);

				// Write out animation header data.
				chunk.Begin("ANIMDATA");
				chunk.WriteString(AnimationController.AnimatedObjectType.FullName);
				chunk.WriteString(Name);
				chunk.WriteFloat(Length);
				chunk.WriteBoolean(IsLooped);
				chunk.End();

				// Put out the tracks with the most keys first.
				var activeTracks = from GorgonAnimationTrack<T> track in Tracks
								   where track.KeyFrames.Count > 0
								   orderby track.KeyFrames.Count
								   select track;

				foreach (var track in activeTracks)
				{
					if (track.KeyFrames.Count <= 0)
					{
						continue;
					}

					chunk.Begin("TRCKDATA");
					track.ToChunk(chunk);
					chunk.End();
				}
			}
		}

		/// <summary>
		/// Function to save the animation to a file.
		/// </summary>
		/// <param name="fileName">Path and file name of the file to write.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.</exception>
		public void Save(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "fileName");
			}

		    using (FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
		    {
		        Save(stream);
		    }
		}
		
		/// <summary>
		/// Function to reset the animation state.
		/// </summary>
		public void Reset()
		{
			_time = 0;
			_looped = 0;
			UpdateObject();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimation{T}" /> class.
		/// </summary>
		/// <param name="controller">Animation controller that owns this animation.</param>
		/// <param name="name">The name.</param>
		/// <param name="length">The length of the animation, in milliseconds.</param>
		internal GorgonAnimation(GorgonAnimationController<T> controller, string name, float length)
			: base(name)
		{
			Tracks = new GorgonAnimationTrackCollection<T>(this);
			Length = length;
			Speed = 1.0f;

			AnimationController = controller;
		}
		#endregion		
	
		#region ICloneable<GorgonAnimation> Members
		/// <summary>
		/// Function to clone the animation.
		/// </summary>
		/// <returns>A clone of the animation.</returns>		
		public GorgonAnimation<T> Clone()
		{
			var clone = new GorgonAnimation<T>(AnimationController, Name, Length)
			    {
			        IsLooped = IsLooped,
			        Length = Length,
			        LoopCount = LoopCount,
			        Speed = Speed,
			        Time = Time
			    };

		    foreach (var track in Tracks)
		    {
			    if (!clone.Tracks.Contains(track.Name))
			    {
				    continue;
			    }

			    foreach (var key in track.KeyFrames)
			    {
				    clone.Tracks[track.Name].KeyFrames.Add(key.Clone());
			    }
		    }

			return clone;
		}
		#endregion
	}
}
