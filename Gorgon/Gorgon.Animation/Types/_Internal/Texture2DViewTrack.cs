﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 18, 2018 10:39:26 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Animation
{
    /// <summary>
    /// A track that stores 2D texture values to apply to an object in an animation.
    /// </summary>
    internal class Texture2DViewTrack
        : GorgonNamedObject, IGorgonTrack<GorgonKeyTexture2D>
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of interpolation supported by the track.
        /// </summary>
        public TrackInterpolationMode SupportsInterpolation => TrackInterpolationMode.None;

        /// <summary>
        /// Property to return the spline controller (if applicable) for the track.
        /// </summary>
        public IGorgonSplineCalculation SplineController => null;

        /// <summary>
        /// Property to set or return the interpolation mode.
        /// </summary>
        /// <remarks>
        /// If the value assigned is not supported by the track (use the <see cref="SupportsInterpolation"/> property), then the original value is kept.
        /// </remarks>
        public TrackInterpolationMode InterpolationMode
        {
            get => TrackInterpolationMode.None;
            set 
            {
                // Do nothing, we don't support changing modes here.
            }
        }

        /// <summary>
        /// Property to return the key frames for the track.
        /// </summary>
        public IReadOnlyList<GorgonKeyTexture2D> KeyFrames
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DViewTrack"/> class.
        /// </summary>
        /// <param name="keyFrames">The list of key frames for the track.</param>
        internal Texture2DViewTrack(IReadOnlyList<GorgonKeyTexture2D> keyFrames)
            : base(Resources.GORANM_NAME_TEXTURE_2D) => KeyFrames = keyFrames;
        #endregion
    }
}
