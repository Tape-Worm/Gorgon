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
// Created: Wednesday, October 3, 2012 9:14:34 PM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Graphics;
using SlimMath;

namespace Gorgon.Animation
{
	/// <summary>
	/// A track that will animate properties with a 2D texture data type.
	/// </summary>
	/// <typeparam name="T">Type of object to be animated.</typeparam>
	class GorgonTrackTexture2D<T>
		: GorgonAnimationTrack<T>
		where T : class
	{
		#region Variables.
		private readonly Func<T, GorgonTexture2D> _getTextureProperty;		// Get property method.
		private readonly Action<T, GorgonTexture2D> _setTextureProperty;	// Set property method.
	    private readonly Action<T, RectangleF> _setTextureRegionProperty;	// Set property method.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		public override TrackInterpolationMode SupportedInterpolation
		{
			get
			{
				return TrackInterpolationMode.Linear | TrackInterpolationMode.None;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the a key with the proper type for this track.
		/// </summary>
		/// <returns>The key with the proper type for this track.</returns>
		protected override IKeyFrame MakeKey()
		{
			return new GorgonKeyTexture2D(0, null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to interpolate a new key frame from the nearest previous and next key frames.
		/// </summary>
		/// <param name="keyValues">Nearest previous and next key frames.</param>
		/// <param name="keyTime">The time to assign to the key.</param>
		/// <param name="unitTime">The time, expressed in unit time.</param>
		/// <returns>
		/// The interpolated key frame containing the interpolated values.
		/// </returns>
		protected override IKeyFrame GetTweenKey(ref NearestKeys keyValues, float keyTime, float unitTime)
		{
			var prev = (GorgonKeyTexture2D)keyValues.PreviousKey;

			if (InterpolationMode != TrackInterpolationMode.Linear)
			{
				return prev;
			}

			var next = (GorgonKeyTexture2D)keyValues.NextKey;
			var regionStart = new Vector4(prev.TextureRegion.X, prev.TextureRegion.Y, prev.TextureRegion.Width, prev.TextureRegion.Height);
			var regionEnd = new Vector4(next.TextureRegion.X, next.TextureRegion.Y, next.TextureRegion.Width, next.TextureRegion.Height);
			Vector4 result;

			Vector4.Lerp(ref regionStart, ref regionEnd, unitTime, out result);
			return new GorgonKeyTexture2D(keyTime, prev.Value, new RectangleF(result.X, result.Y, result.Z, result.W));
		}

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected internal override void ApplyKey(ref IKeyFrame key)
		{
			var value = (GorgonKeyTexture2D)key;
			GorgonTexture2D currentTexture = _getTextureProperty(Animation.AnimationController.AnimatedObject);

			// If there's no texture on this key, then try to find it.
			if ((value.Value == null) && (!value.GetTexture()))
			{
                value = new GorgonKeyTexture2D(value.Time, currentTexture, value.TextureRegion);
            }

		    if (currentTexture != value.Value)
		    {
		        _setTextureProperty(Animation.AnimationController.AnimatedObject, value.Value);
		    }

		    _setTextureRegionProperty(Animation.AnimationController.AnimatedObject, value.TextureRegion);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTrackTexture2D{T}" /> class.
		/// </summary>
		/// <param name="textureProperty">Property to alter the texture.</param>
		/// <param name="regionProperty">Property to alter the region.</param>
		internal GorgonTrackTexture2D(GorgonAnimatedProperty textureProperty, GorgonAnimatedProperty regionProperty)
			: base(textureProperty)
		{
			_getTextureProperty = BuildGetAccessor<GorgonTexture2D>();
			_setTextureProperty = BuildSetAccessor<GorgonTexture2D>();
			_setTextureRegionProperty = BuildSetAccessor<RectangleF>(regionProperty.Property.GetSetMethod());

			InterpolationMode = TrackInterpolationMode.None;
		}
		#endregion
	}
}
