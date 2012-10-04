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
// Created: Wednesday, October 3, 2012 9:10:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An animation track for 8 bit integer values.
	/// </summary>
	internal class GorgonTrackSByte
		: GorgonAnimationTrack
	{
		#region Variables.
		private static Func<IAnimated, SByte> _getProperty = null;			// Get property method.
		private static Action<IAnimated, SByte> _setProperty = null;		// Set property method.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		public override TrackInterpolationMode SupportedInterpolation
		{
			get
			{
				return TrackInterpolationMode.Spline | TrackInterpolationMode.Linear;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set up the spline for the animation.
		/// </summary>
		protected internal override void SetupSpline()
		{
			base.SetupSpline();

			for (int i = 0; i < KeyFrames.Count; i++)
			{
				GorgonKeySByte key = (GorgonKeySByte)KeyFrames[i];
				Spline.Points.Add(new Vector4(key.Value, 0.0f, 0.0f, 1.0f));
			}

			Spline.UpdateTangents();
		}

		/// <summary>
		/// Function to update the property value assigned to the track.
		/// </summary>
		/// <param name="keyValues">Values to use when updating.</param>
		/// <param name="key">The key to work on.</param>
		/// <param name="time">Time to reference, in milliseconds.</param>
		protected override void GetTweenKey(ref GorgonAnimationTrack.NearestKeys keyValues, out IKeyFrame key, float time)
		{
			// Just use the previous key if we're at 0.
			if (time == 0)
			{
				key = keyValues.PreviousKey;
				return;
			}

			GorgonKeySByte next = (GorgonKeySByte)keyValues.NextKey;
			GorgonKeySByte prev = (GorgonKeySByte)keyValues.PreviousKey;

			key = prev;

			switch (InterpolationMode)
			{
				case TrackInterpolationMode.Linear:
					key = new GorgonKeySByte(time, (SByte)((float)prev.Value + (float)(next.Value - prev.Value) * time));
					break;
				case TrackInterpolationMode.Spline:
					key = new GorgonKeySByte(time, (SByte)Spline.GetInterpolatedValue(keyValues.PreviousKeyIndex, time).X);
					break;
			}
		}

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected internal override void ApplyKey(ref IKeyFrame key)
		{
			GorgonKeySByte value = (GorgonKeySByte)key;
			_setProperty(Animation.Owner, value.Value);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTrackSByte" /> class.
		/// </summary>
		/// <param name="animation">The animation that owns this track.</param>
		/// <param name="property">Property information.</param>
		internal GorgonTrackSByte(GorgonAnimation animation, GorgonAnimationCollection.AnimatedProperty property)
			: base(animation, property)
		{
			if (_getProperty == null)
				_getProperty = BuildGetAccessor<SByte>();
			if (_setProperty == null)
				_setProperty = BuildSetAccessor<SByte>();

			InterpolationMode = TrackInterpolationMode.Linear;
		}
		#endregion
	}
}
