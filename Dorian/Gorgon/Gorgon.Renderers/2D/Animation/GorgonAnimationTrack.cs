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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A track for an animated property on an animated object.
	/// </summary>
	public abstract class GorgonAnimationTrack
		: GorgonNamedObject
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
			internal NearestKeys(GorgonAnimationTrack owner, float requestedTime)
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
		private MethodInfo _getMethod = null;			// Property get accessor information.
		private MethodInfo _setMethod = null;			// Property set accessor information.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of key frames for this animation.
		/// </summary>
		public GorgonAnimationKeyFrameCollection KeyFrames
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the animation that this track belongs to.
		/// </summary>
		public GorgonAnimationClip Animation
		{
			get;
			private set;
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
		/// <typeparam name="T">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected internal Func<IAnimated, T> BuildGetAccessor<T>()
		{
			var instance = Expression.Parameter(typeof(IAnimated), "Inst");

			Expression<Func<IAnimated, T>> expression =
				Expression.Lambda<Func<IAnimated, T>>(
					Expression.Call(Expression.Convert(instance, _getMethod.DeclaringType), _getMethod),
				instance);

			return expression.Compile();
		}

		/// <summary>
		/// Function to build a set accessor for the property that will be manipulated by the track.
		/// </summary>
		/// <typeparam name="T">Type of output.</typeparam>
		/// <returns>The get accessor method.</returns>
		protected internal Action<IAnimated, T> BuildSetAccessor<T>()
		{
			var instance = Expression.Parameter(typeof(IAnimated), "Inst");
			var value = Expression.Parameter(typeof(T));

			Expression<Action<IAnimated, T>> expression =
				Expression.Lambda<Action<IAnimated, T>>(
					Expression.Call(Expression.Convert(instance, _setMethod.DeclaringType), _setMethod, value),
				instance, value);

			return expression.Compile();
		}

		/// <summary>
		/// Function to update the property value assigned to the track.
		/// </summary>
		/// <param name="keyValues">Values to use when updating.</param>
		/// <param name="key">The key to work on.</param>
		/// <param name="time">Time to reference.</param>
		protected abstract void GetTweenKey(ref GorgonAnimationTrack.NearestKeys keyValues, out IKeyFrame key, float time);

        /// <summary>
        /// Function to apply the key value to the object properties.
        /// </summary>
        /// <param name="key">Key to apply to the properties.</param>
        protected abstract internal void ApplyKey(ref IKeyFrame key);

		/// <summary>
		/// Function to retrieve a key frame for a given time.
		/// </summary>
		/// <param name="time">Time to look up.</param>
		/// <returns>A keyframe at that time.  Note that this can be a key frame that was interpolated because it doesn't actually exist in the collection.</returns>
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

            IKeyFrame key = default(IKeyFrame);
			GetTweenKey(ref keys, out key, time);

			return key;
		}

		/// <summary>
		/// Function to retrieve the nearest keys to the specified time interval.
		/// </summary>
		/// <param name="time">Time interval to look up.</param>
		/// <returns>The nearest keys to the interval.</returns>
		public GorgonAnimationTrack.NearestKeys GetNearestKeys(float time)
		{
			return new NearestKeys(this, time);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationTrack" /> class.
		/// </summary>
		/// <param name="animation">The animation that owns this track.</param>
		/// <param name="property">The property to animate.</param>
		protected GorgonAnimationTrack(GorgonAnimationClip animation, GorgonAnimationCollection.AnimatedProperty property)
			: base(property.DisplayName)
		{
			Animation = animation;
			DataType = property.DataType;

			// We use this to set/return the property values.  Unlike the previous version of Gorgon,
			// which used reflection to set/return the values, we're using an expression tree to do the work.
			// Profiling in a tight loop of 1,000,000 iterations:  The reflection took between 415ms - 469ms 
			// to set and return values, while the expression tree took 39 ms, a little over 10-12x the speed.
			// The animation controller in the previous version of Gorgon was horrifically slow, hopefully
			// this will remedy that.
			_getMethod = property.Property.GetGetMethod();
			_setMethod = property.Property.GetSetMethod();

			KeyFrames = new GorgonAnimationKeyFrameCollection(this);
		}
		#endregion
	}
}
