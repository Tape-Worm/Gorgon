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
// Created: Monday, September 3, 2012 7:57:00 PM
// 
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Animation
{
	// TODO: Why is this marked as flags??

	/// <summary>
	/// Interpolation mode for animating between key frames.
	/// </summary>    
	[Flags]
	public enum TrackInterpolationMode
	{
		/// <summary>
		/// No interpolation.
		/// </summary>
		None = 0,
		/// <summary>
		/// Linear interpolation.
		/// </summary>
		Linear = 1,
		/// <summary>
		/// Spline interpolation.
		/// </summary>
		Spline = 2
	}

	/// <summary>
	/// A track for an animated property on an animated object.
	/// </summary>
	/// <remarks>Custom tracks may be built by the user by inheriting from this object.  When a custom track is built, the user will need to add that track to the 
    /// <see cref="Gorgon.Animation.GorgonAnimation{T}.Tracks">GorgonAnimation.Tracks</see> collection.  Please note that when a custom track is built a custom 
	/// <see cref="Gorgon.Animation.IKeyFrame">key frame</see> type must be built to accompany the track.</remarks>
	/// <typeparam name="T">The type of object being animated.</typeparam>
	[Obsolete("TODO: Use our property expression tree code in Gorgon.Reflection instead of what we've got here.")]
	public abstract class GorgonAnimationTrack<T>
		: GorgonNamedObject
		where T : class
	{
		#region Constants.
		// Key frames list
		private const string KeyFramesChunk = "KEFRAMES";
		// Track listing.
		private const string TracksChunk = "TRAKDATA";
		#endregion

		#region Value Types.
		/// <summary>
		/// Value type containing information about the nearest keys for a time position.
		/// </summary>
		public struct NearestKeys
		{
			#region Variables.
            /// <summary>
            /// Time position within the track that lies between the two keys.
            /// </summary>
			public readonly float TrackTimePosition;
            /// <summary>
            /// Keyframe prior to the time position.
            /// </summary>
			public readonly IKeyFrame PreviousKey;
            /// <summary>
            /// Keyframe after the time position.
            /// </summary>
			public readonly IKeyFrame NextKey;
            /// <summary>
            /// Distance between the previous and next keyframes, 0 if equal to the previous, 1 if equal to the next.
            /// </summary>
			public readonly float KeyTimeDelta;
            /// <summary>
            /// Previous key index.
            /// </summary>
			public readonly int PreviousKeyIndex;
            /// <summary>
            /// Next key index.
            /// </summary>
			public readonly int NextKeyIndex;
			#endregion

			#region Constructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="owner">Owning track.</param>
			/// <param name="requestedTime">Track time requested.</param>
			internal NearestKeys(GorgonAnimationTrack<T> owner, float requestedTime)
			{
				float animationLength = owner.Animation.Length;     // Animation length.
				int i;											    // Loop.

#if DEBUG
			    if (owner.KeyFrames.Count < 1)
			    {
			        throw new ArgumentException(Resources.GORANM_TRACK_HAS_NO_KEYS, "owner");
			    }
#endif

				// Initialize.
				PreviousKeyIndex = 0;
				PreviousKey = owner.KeyFrames[0];
				NextKeyIndex = -1;                
				NextKey = null;
				KeyTimeDelta = 0;                

				// Wrap around.
				while (requestedTime > animationLength)
					requestedTime -= animationLength;

				// Find previous key.                
				if (owner.KeyFrames[0].Time <= requestedTime)
				{
					for (i = 0; i < owner.KeyFrames.Count; i++)
					{
						if ((!(owner.KeyFrames[i].Time > requestedTime)) || (i <= 0))
						{
							continue;
						}

						PreviousKey = owner.KeyFrames[i - 1];
						PreviousKeyIndex = i - 1;
						break;
					}
				}
				else
					i = 1;

				// Wrap to the first key if we went through the entire list.
				if (i >= owner.KeyFrames.Count)
				{
					if (!owner.Animation.IsLooped)
					{
						NextKey = owner.KeyFrames[owner.KeyFrames.Count - 1];
						NextKeyIndex = owner.KeyFrames.Count - 1;
					}
					else
					{
						NextKey = owner.KeyFrames[0];
						NextKeyIndex = 0;
					}
					KeyTimeDelta = animationLength;
				}
				else
				{
					NextKey = owner.KeyFrames[i];
					NextKeyIndex = i;
					KeyTimeDelta = NextKey.Time;
				}

				// Same frame.				
				if (PreviousKey.Time.EqualsEpsilon(KeyTimeDelta, 0.001f))
					KeyTimeDelta = 0;
				else
					KeyTimeDelta = (requestedTime - PreviousKey.Time) / (KeyTimeDelta - PreviousKey.Time);

				// We can't have negative time.
				if (KeyTimeDelta < 0)
					KeyTimeDelta = 0;
				TrackTimePosition = requestedTime;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private TrackInterpolationMode _interpolation = TrackInterpolationMode.None;    // Interpolation mode for the track.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the property that's being animated.
		/// </summary>
		protected GorgonAnimatedProperty AnimatedProperty
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the spline control interface.
		/// </summary>
		protected GorgonSpline Spline
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		/// <remarks>This will return one or more of the interpolation modes supported.  The interpolation mode "None" is always supported.</remarks>
		public abstract TrackInterpolationMode SupportedInterpolation
		{
			get;
		}

		/// <summary>
		/// Property to set or return the interpolation mode for the track.
		/// </summary>
		/// <remarks>This value depends on which interpolation modes are supported for tweening.  If the track does not support interpolation, this value will have no effect.</remarks>
		public TrackInterpolationMode InterpolationMode
		{
			get
			{
				return _interpolation;
			}
			set
			{
				if ((SupportedInterpolation & value) == value)
					_interpolation = value;
				if ((_interpolation == TrackInterpolationMode.Spline) && (KeyFrames.Count > 0) && ((SupportedInterpolation & TrackInterpolationMode.Spline) == TrackInterpolationMode.Spline))
					SetupSpline();
			}
		}

		/// <summary>
		/// Property to return the list of key frames for this animation.
		/// </summary>
		public GorgonAnimationKeyFrameCollection<T> KeyFrames
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the animation that this track belongs to.
		/// </summary>
		public GorgonAnimation<T> Animation
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the type of data that is represented by the key frames in the track.
		/// </summary>
		public Type DataType
		{
			get;
			private set;
		}

        /// <summary>
        /// Property set or return the function to call when time has been updated for animation.
        /// </summary>
        /// <remarks>
        /// Assign a function to this property if you wish to override the keyframe animation code.  The override function should have 2 parameters: an object that matches the type 
        /// T for the animation, and a float value that represents the current time in the animation.  The return value for the function should either return <b>false</b> to indicate that 
        /// the animation updating for this object has been handled, or <b>true</b> if the animation should continue to update the object based on key frame information. 
        /// </remarks>
        public Func<T, float, bool> TimeUpdatedCallback
        {
            get;
            set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build a get accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <typeparam name="TO">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Func<T, TO> BuildGetAccessor<TO>()
		{
			return BuildGetAccessor<TO>(AnimatedProperty.Property.GetGetMethod());
		}

		/// <summary>
		/// Function to build a set accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <typeparam name="TO">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Action<T, TO> BuildSetAccessor<TO>()
		{
			return BuildSetAccessor<TO>(AnimatedProperty.Property.GetSetMethod());
		}
		
		/// <summary>
		/// Function to build a get accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <param name="getMethod">Method information.</param>
		/// <typeparam name="TO">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Func<T, TO> BuildGetAccessor<TO>(MethodInfo getMethod)
		{
			var instance = Expression.Parameter(typeof(T), "Inst");

			Expression<Func<T, TO>> expression =
				Expression.Lambda<Func<T, TO>>(
					Expression.Call(instance, getMethod),
				instance);

			return expression.Compile();
		}

		/// <summary>
		/// Function to build a set accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <param name="setMethod">Method information.</param>
		/// <typeparam name="TO">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		// ReSharper disable PossiblyMistakenUseOfParamsMethod
		protected Action<T, TO> BuildSetAccessor<TO>(MethodInfo setMethod)
		{
			var instance = Expression.Parameter(typeof(T), "Inst");
			var value = Expression.Parameter(typeof(TO));

			Expression<Action<T, TO>> expression =
				Expression.Lambda<Action<T, TO>>(
					Expression.Call(instance, setMethod, value),
				instance, value);

			return expression.Compile();
		}
		// ReSharper restore PossiblyMistakenUseOfParamsMethod

		/// <summary>
		/// Function to set up the spline for the animation.
		/// </summary>
		protected internal virtual void SetupSpline()
		{
			Spline.Points.Clear();
		}

		/// <summary>
		/// Function to create the a key with the proper type for this track.
		/// </summary>
		/// <returns>The key with the proper type for this track.</returns>
		protected abstract IKeyFrame MakeKey();

		/// <summary>
		/// Function to interpolate a new key frame from the nearest previous and next key frames.
		/// </summary>
		/// <param name="keyValues">Nearest previous and next key frames.</param>
		/// <param name="keyTime">The time to assign to the key.</param>
		/// <param name="unitTime">The time, expressed in unit time.</param>
		/// <returns>The interpolated key frame containing the interpolated values.</returns>
		protected abstract IKeyFrame GetTweenKey(ref NearestKeys keyValues, float keyTime, float unitTime);

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected abstract internal void ApplyKey(ref IKeyFrame key);

		/// <summary>
		/// Function to read the track data from a data chunk.
		/// </summary>
		/// <param name="animFile">Animation file to read from.</param>
		/// <param name="animation">Animation that owns the track.</param>
		internal static void FromChunk(IGorgonChunkFileReader animFile, GorgonAnimation<T> animation)
		{
			int trackCount = animFile.Chunks.Count(item => item.ID == TracksChunk.ChunkID());

			// Get all tracks for the animation.
			for (int i = 0; i < trackCount; ++i)
			{
				GorgonBinaryReader reader = animFile.OpenChunk(TracksChunk);
				string trackName = reader.ReadString();

				GorgonAnimationTrack<T> track;

				if (!animation.Tracks.TryGetValue(trackName, out track))
				{
					throw new GorgonException(GorgonResult.CannotRead,
					                          string.Format(Resources.GORANM_TRACK_TYPE_DOES_NOT_EXIST, trackName, animation.AnimationController.AnimatedObjectType.FullName));
				}

				track.InterpolationMode = reader.ReadValue<TrackInterpolationMode>();

				animFile.CloseChunk();

				// Get key frames for the track.
				reader = animFile.OpenChunk(KeyFramesChunk);
				int keyCount = reader.ReadInt32();

				for (int k = 0; k < keyCount; ++k)
				{
					IKeyFrame key = track.MakeKey();
					key.FromChunk(reader);
					track.KeyFrames.Add(key);
				}

				animFile.CloseChunk();
			}
		}

		/// <summary>
		/// Function to write the track data to a data chunk.
		/// </summary>
		/// <param name="animFile">Animation file to write into.</param>
		internal void ToChunk(IGorgonChunkFileWriter animFile)
		{
			GorgonBinaryWriter writer = animFile.OpenChunk(TracksChunk);
			writer.Write(Name);
			writer.WriteValue(InterpolationMode);
			animFile.CloseChunk();

			writer = animFile.OpenChunk(KeyFramesChunk);
			writer.Write(KeyFrames.Count);
			foreach (IKeyFrame keyFrame in KeyFrames)
			{
				keyFrame.ToChunk(writer);
			}
			animFile.CloseChunk();
		}

		/// <summary>
		/// Function to retrieve a key frame for a given time.
		/// </summary>
		/// <param name="time">Time to look up.</param>
		/// <returns>A key frame at that time.  Note that this can return an interpolated key frame and therefore not actually exist in the <see cref="Gorgon.Animation.GorgonAnimationTrack{T}.KeyFrames">key frames collection</see>.</returns>
		public IKeyFrame GetKeyAtTime(float time)
		{
		    IKeyFrame result;

		    if (KeyFrames.Times.TryGetValue(time, out result))
		    {
		        return result;
		    }

			if (time >= KeyFrames[KeyFrames.Count - 1].Time)
			{
				return KeyFrames[KeyFrames.Count - 1];
			}

			if (time <= 0)
			{
				return KeyFrames[0];
			}

			var keys = new NearestKeys(this, time);

		    return keys.KeyTimeDelta.EqualsEpsilon(0.0f) ? keys.PreviousKey : GetTweenKey(ref keys, time, keys.KeyTimeDelta);
		}

		/// <summary>
		/// Function to retrieve the nearest keys to the specified time interval.
		/// </summary>
		/// <param name="time">Time interval to look up.</param>
		/// <returns>The nearest keys to the interval.</returns>
		public NearestKeys GetNearestKeys(float time)
		{
			return new NearestKeys(this, time);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationTrack{T}" /> class.
		/// </summary>
		/// <param name="property">The property to animate.</param>
		protected GorgonAnimationTrack(GorgonAnimatedProperty property)
			: base(property.DisplayName)
		{
			Animation = null;
			DataType = property.DataType;

			// We use this to set/return the property values.  Unlike the previous version of Gorgon,
			// which used reflection to set/return the values, we're using an expression tree to do the work.
			// Profiling in a tight loop of 1,000,000 iterations:  The reflection took between 415ms - 469ms 
			// to set and return values, while the expression tree took 39 ms, a little over 10-12x the speed.
			// The animation controller in the previous version of Gorgon was horrifically slow, hopefully
			// this will remedy that.
			AnimatedProperty = property;
			KeyFrames = new GorgonAnimationKeyFrameCollection<T>(this);
			Spline = new GorgonSpline();
		}
		#endregion
	}
}
