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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// A track that will animate properties with a 2D texture data type.
	/// </summary>
	class GorgonTrackTexture2D
		: GorgonAnimationTrack
	{
		#region Variables.
		private static Func<Object, GorgonTexture2D> _getTextureProperty = null;			// Get property method.
        private static Action<Object, GorgonTexture2D> _setTextureProperty = null;			// Set property method.
        private static Func<Object, RectangleF> _getTextureRegionProperty = null;			// Get property method.
        private static Action<Object, RectangleF> _setTextureRegionProperty = null;			// Set property method.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		public override TrackInterpolationMode SupportedInterpolation
		{
			get
			{
				return TrackInterpolationMode.None;
			}
		}		
		#endregion

		#region Methods.
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

            GorgonKeyTexture2D prev = (GorgonKeyTexture2D)keyValues.PreviousKey;
			key = prev;
		}

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected internal override void ApplyKey(ref IKeyFrame key)
		{
            GorgonKeyTexture2D value = (GorgonKeyTexture2D)key;
            GorgonTexture2D currentTexture = _getTextureProperty(Animation.AnimationController.AnimatedObject);

            if (currentTexture != value.Value)
			    _setTextureProperty(Animation.AnimationController.AnimatedObject, value.Value);
            _setTextureRegionProperty(Animation.AnimationController.AnimatedObject, value.TextureRegion);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonTrackTexture2D" /> class.
		/// </summary>
        /// <param name="textureProperty">Property to alter the texture.</param>
        /// <param name="regionProperty">Property to alter the region.</param>
		internal GorgonTrackTexture2D(GorgonAnimationTrackCollection.AnimatedProperty textureProperty, GorgonAnimationTrackCollection.AnimatedProperty regionProperty)
			: base(textureProperty)
		{
			if (_getTextureProperty == null)
                _getTextureProperty = BuildGetAccessor<GorgonTexture2D>();
			if (_setTextureProperty == null)
                _setTextureProperty = BuildSetAccessor<GorgonTexture2D>();
            if (_getTextureRegionProperty == null)
                _getTextureRegionProperty = BuildGetAccessor<RectangleF>(regionProperty.Property.GetGetMethod());
            if (_setTextureRegionProperty == null)
                _setTextureRegionProperty = BuildSetAccessor<RectangleF>(regionProperty.Property.GetSetMethod());

			InterpolationMode = TrackInterpolationMode.None;
		}
		#endregion
	}
}
