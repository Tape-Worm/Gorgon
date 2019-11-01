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
// Created: August 18, 2018 10:39:26 AM
// 
#endregion

using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Animation
{
    /// <summary>
    /// A track that stores 2D vectors representing various properties in an animation.
    /// </summary>
    internal class Vector2Track
        : GorgonNamedObject, IGorgonAnimationTrack<GorgonKeyVector2>
    {
        #region Variables.
        // The interpolation mode for the track.
        private TrackInterpolationMode _interpolationMode = TrackInterpolationMode.Linear;
        // The spline controller for the track.
        private readonly GorgonCatmullRomSpline _splineController = new GorgonCatmullRomSpline();
        #endregion

        #region Properties.
        /// <summary>Property to return the type of key frame data stored in this track.</summary>
        public AnimationTrackKeyType KeyFrameDataType => AnimationTrackKeyType.Vector2;

        /// <summary>
        /// Property to return the type of interpolation supported by the track.
        /// </summary>
        public TrackInterpolationMode SupportsInterpolation => TrackInterpolationMode.Linear | TrackInterpolationMode.Spline;

        /// <summary>
        /// Property to return the spline controller (if applicable) for the track.
        /// </summary>
        public IGorgonSplineCalculation SplineController => _splineController;

        /// <summary>
        /// Property to set or return the interpolation mode.
        /// </summary>
        /// <remarks>
        /// If the value assigned is not supported by the track (use the <see cref="SupportsInterpolation"/> property), then the original value is kept.
        /// </remarks>
        public TrackInterpolationMode InterpolationMode
        {
            get => _interpolationMode;
            set
            {
                if ((SupportsInterpolation & value) != value)
                {
                    return;
                }

                _interpolationMode = value;
            }
        }

        /// <summary>
        /// Property to return the key frames for the track.
        /// </summary>
        public IReadOnlyList<GorgonKeyVector2> KeyFrames
        {
            get;
        }
        #endregion

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
        public GorgonKeyVector2 GetValueAtTime(float timeIndex)
        {
            if (KeyFrames.Count == 0)
            {
                return null;
            }

            if (KeyFrames.Count == 1)
            {
                return KeyFrames[0];
            }

            GorgonKeyVector2 result = KeyFrames.FirstOrDefault(item => item.Time == timeIndex);

            if (result != null)
            {
                return result;
            }

            float highestTime = KeyFrames.Max(item => item.Time);

            TrackKeyProcessor.TryUpdateVector2(highestTime, this, timeIndex, out DX.Vector2 vec);

            return new GorgonKeyVector2(timeIndex, vec);
        }

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Track"/> class.
        /// </summary>
        /// <param name="keyFrames">The list of key frames for the track.</param>
        /// <param name="name">The name of the track.</param>
        internal Vector2Track(IReadOnlyList<GorgonKeyVector2> keyFrames, string name)
            : base(name)
        {
            KeyFrames = keyFrames;

            // Build the spline for the track.
            for (int i = 0; i < keyFrames.Count; ++i)
            {
                _splineController.Points.Add(new DX.Vector4(keyFrames[i].Value, 0, 1.0f));
            }

            _splineController.UpdateTangents();
        }
        #endregion
    }
}
