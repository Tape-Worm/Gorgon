#region MIT
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
// Created: August 18, 2018 10:51:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Math;
using Newtonsoft.Json;

namespace Gorgon.Animation
{
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
    /// A track for a <see cref="IGorgonAnimation"/>.
    /// </summary>
    /// <typeparam name="T">The type of key frame, must implement <see cref="IGorgonKeyFrame"/></typeparam>
    /// <remarks>
    /// <para>
    /// Tracks contain a list of values in time, called key frames. These key frames tell the animation what the value for a property should be at the specified time.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonKeyFrame"/>
    public interface IGorgonAnimationTrack<out T>
        : IGorgonNamedObject
        where T : IGorgonKeyFrame
    {
        #region Properties.
        /// <summary>
        /// Property to return the spline controller (if applicable) for the track.
        /// </summary>
        [JsonIgnore]
        IGorgonSplineCalculation SplineController
        {
            get;
        }

        /// <summary>
        /// Property to return the type of interpolation supported by the track.
        /// </summary>
        [JsonIgnore]
        TrackInterpolationMode SupportsInterpolation
        {
            get;
        }

        /// <summary>
        /// Property to set or return the interpolation mode.
        /// </summary>
        /// <remarks>
        /// If the value assigned is not supported by the track (use the <see cref="SupportsInterpolation"/> property), then the original value is kept.
        /// </remarks>
        TrackInterpolationMode InterpolationMode
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of key frame data stored in this track.
        /// </summary>
        [JsonIgnore]
        AnimationTrackKeyType KeyFrameDataType
        {
            get;
        }

        /// <summary>
        /// Property to return the key frames for the track.
        /// </summary>
        [JsonProperty("keyframes")]
        IReadOnlyList<T> KeyFrames
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the value at the specified time index.
        /// </summary>
        /// <param name="timeIndex">The time index within the track to retrieve the value from.</param>
        /// <returns>The value at the specified time index.</returns>
        /// <remarks>
        /// <para>
        /// The value returned by this method may or may not be interpolated based on the value in <see cref="InterpolationMode"/>.  
        /// </para>
        /// </remarks>
        T GetValueAtTime(float timeIndex);
        #endregion
    }
}