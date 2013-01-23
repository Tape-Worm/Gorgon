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
using System.Linq.Expressions;
using System.Reflection;
using GorgonLibrary.Math;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// Interpolation mode for animating between key frames.
	/// </summary>    
	[Flags()]
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
	/// <see cref="P:GorgonLibrary.Animation.GorgonAnimation.Tracks">GorgonAnimation.Tracks</see> collection.  Please note that when a custom track is built a custom 
	/// <see cref="GorgonLibrary.Animation.IKeyFrame">key frame</see> type must be built to accompany the track.</remarks>
	/// <typeparam name="T">The type of object being animated.</typeparam>
	public abstract class GorgonAnimationTrack<T>
		: GorgonNamedObject
		where T : class
	{
		#region Value Types.
		/// <summary>
		/// Value type containing information about the nearest keys for a time position.
		/// </summary>
		public struct NearestKeys
		{
			#region Variables.
			private float _timePosition;            // Time position within the track that lies between the two keys.
			private IKeyFrame _previousKey;			// Keyframe prior to the time position.
			private IKeyFrame _nextKey;				// Keyframe after the time position.
			private float _keyTimeDelta;            // Distance between the previous and next keyframes, 0 if equal to the previous, 1 if equal to the next.
			private int _previousKeyIndex;          // Previous key index.
			private int _nextKeyIndex;              // Next key index.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the track time position requested.
			/// </summary>
			public float TrackTimePosition
			{
				get
				{
					return _timePosition;
				}
			}

			/// <summary>
			/// Property to return the previous key to the time position.
			/// </summary>
			public IKeyFrame PreviousKey
			{
				get
				{
					return _previousKey;
				}
			}

			/// <summary>
			/// Property to return the next key from the time position.
			/// </summary>
			public IKeyFrame NextKey
			{
				get
				{
					return _nextKey;
				}
			}

			/// <summary>
			/// Property to return the previous key index.
			/// </summary>
			public int PreviousKeyIndex
			{
				get
				{
					return _previousKeyIndex;
				}
			}

			/// <summary>
			/// Property to return the next key index.
			/// </summary>
			public int NextKeyIndex
			{
				get
				{
					return _nextKeyIndex;
				}
			}

			/// <summary>
			/// Property to return the delta between the previous and next frames.  0 if equal to the previous keyframe, 1.0 if equal to the next keyframe.
			/// </summary>
			public float KeyTimeDelta
			{
				get
				{
					return _keyTimeDelta;
				}
			}
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
				int i = 0;											// Loop.

#if DEBUG
				if (owner.KeyFrames.Count < 1)
					throw new ArgumentException("Track has no keys.", "owner");
#endif

				// Initialize.
				_previousKeyIndex = 0;
				_previousKey = owner.KeyFrames[0];
				_nextKeyIndex = -1;                
				_nextKey = null;
				_keyTimeDelta = 0;                

				// Wrap around.
				while (requestedTime > animationLength)
					requestedTime -= animationLength;

				// Find previous key.                
				if (owner.KeyFrames[0].Time <= requestedTime)
				{
					for (i = 0; i < owner.KeyFrames.Count; i++)
					{
						if ((owner.KeyFrames[i].Time > requestedTime) && (i > 0))
						{
							_previousKey = owner.KeyFrames[i - 1];
							_previousKeyIndex = i - 1;
							break;
						}
					}
				}
				else
					i = 1;

				// Wrap to the first key if we went through the entire list.
				if (i >= owner.KeyFrames.Count)
				{
					if (!owner.Animation.IsLooped)
					{
						_nextKey = owner.KeyFrames[owner.KeyFrames.Count - 1];
						_nextKeyIndex = owner.KeyFrames.Count - 1;
					}
					else
					{
						_nextKey = owner.KeyFrames[0];
						_nextKeyIndex = 0;
					}
					_keyTimeDelta = animationLength;
				}
				else
				{
					_nextKey = owner.KeyFrames[i];
					_nextKeyIndex = i;
					_keyTimeDelta = _nextKey.Time;
				}

				// Same frame.				
				if (_previousKey.Time.EqualsEpsilon(_keyTimeDelta, 0.001f))
					_keyTimeDelta = 0;
				else
					_keyTimeDelta = (requestedTime - _previousKey.Time) / (_keyTimeDelta - _previousKey.Time);

				// We can't have negative time.
				if (_keyTimeDelta < 0)
					_keyTimeDelta = 0;
				_timePosition = requestedTime;
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build a get accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <typeparam name="O">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Func<T, O> BuildGetAccessor<O>()
		{
			return BuildGetAccessor<O>(AnimatedProperty.Property.GetGetMethod());
		}

		/// <summary>
		/// Function to build a set accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <typeparam name="O">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Action<T, O> BuildSetAccessor<O>()
		{
			return BuildSetAccessor<O>(AnimatedProperty.Property.GetSetMethod());
		}
		
		/// <summary>
		/// Function to build a get accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <param name="getMethod">Method information.</param>
		/// <typeparam name="O">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Func<T, O> BuildGetAccessor<O>(MethodInfo getMethod)
		{
			var instance = Expression.Parameter(typeof(T), "Inst");

			Expression<Func<T, O>> expression =
				Expression.Lambda<Func<T, O>>(
					Expression.Call(instance, getMethod),
				instance);

			return expression.Compile();
		}

		/// <summary>
		/// Function to build a set accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <param name="setMethod">Method information.</param>
		/// <typeparam name="O">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected Action<T, O> BuildSetAccessor<O>(MethodInfo setMethod)
		{
			var instance = Expression.Parameter(typeof(T), "Inst");
			var value = Expression.Parameter(typeof(O));

			Expression<Action<T, O>> expression =
				Expression.Lambda<Action<T, O>>(
					Expression.Call(instance, setMethod, value),
				instance, value);

			return expression.Compile();
		}

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
		protected abstract IKeyFrame GetTweenKey(ref GorgonAnimationTrack<T>.NearestKeys keyValues, float keyTime, float unitTime);

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected abstract internal void ApplyKey(ref IKeyFrame key);

		/// <summary>
		/// Function to read the track data from a stream.
		/// </summary>
		/// <param name="reader">Reader used to read in the data from the stream.</param>
		internal void FromStream(GorgonLibrary.IO.GorgonBinaryReader reader)
		{
			InterpolationMode = (TrackInterpolationMode)reader.ReadInt32();

			int keyCount = reader.ReadInt32();

			for (int i = 0; i < keyCount; i++)
			{
				IKeyFrame key = MakeKey();
				key.FromStream(reader);
				KeyFrames.Add(key);
			}
		}

		/// <summary>
		/// Function to write the track data to a stream.
		/// </summary>
		/// <param name="writer">Writer used to write the data to the stream.</param>
		internal void ToStream(GorgonLibrary.IO.GorgonBinaryWriter writer)
		{
			writer.Write(Name);
			writer.Write((int)InterpolationMode);
			writer.Write(KeyFrames.Count);
			for (int i = 0; i < KeyFrames.Count; i++)
				KeyFrames[i].ToStream(writer);
		}

		/// <summary>
		/// Function to retrieve a key frame for a given time.
		/// </summary>
		/// <param name="time">Time to look up.</param>
		/// <returns>A keyframe at that time.  Note that this can return an interpolated key frame and therefore not actually exist in the <see cref="P:GorgonLibrary.Animation.GorgonAnimationTrack.KeyFrames">key frames collection</see>.</returns>
		public IKeyFrame GetKeyAtTime(float time)
		{
			NearestKeys keys = default(NearestKeys);

			if (KeyFrames.Times.ContainsKey(time))
				return KeyFrames.Times[time];

			if (time >= KeyFrames[KeyFrames.Count - 1].Time)
				return KeyFrames[KeyFrames.Count - 1];

			if (time <= 0)
				return KeyFrames[0];

			keys = new NearestKeys(this, time);

			if (keys.KeyTimeDelta == 0.0f)
				return keys.PreviousKey;

			return GetTweenKey(ref keys, time, keys.KeyTimeDelta);
		}

		/// <summary>
		/// Function to retrieve the nearest keys to the specified time interval.
		/// </summary>
		/// <param name="time">Time interval to look up.</param>
		/// <returns>The nearest keys to the interval.</returns>
		public GorgonAnimationTrack<T>.NearestKeys GetNearestKeys(float time)
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
