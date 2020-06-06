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
// Created: August 15, 2018 11:01:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Animation
{
    /// <summary>
    /// A base class for a <see cref="IGorgonAnimation"/> implementation.
    /// </summary>
    public class AnimationData
        : GorgonNamedObject, IGorgonAnimation
    {
        #region Variables.
        // Number of loops for the animation.
        private int _loopCount;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the number of times to loop an animation.
        /// </summary>
        public int LoopCount
        {
            get => _loopCount;
            set => _loopCount = value.Max(0);
        }

        /// <summary>
        /// Property to set or return the speed of the animation.
        /// </summary>
        /// <remarks>Setting this value to a negative value will make the animation play backwards.</remarks>
        public float Speed
        {
            get;
            set;
        } = 1.0f;

        /// <summary>
		/// Property to return the length of the animation (in seconds).
		/// </summary>
		public float Length
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether this animation should be looping or not.
        /// </summary>
        public bool IsLooped
        {
            get;
            set;
        }

        /// <summary>Property to set or return the tracks used to update any values using a single floating point value.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeySingle>> SingleTracks
        {
            get;
            set;
        }

        /// <summary>Property to set or return the tracks used to update any values using a 2D vector.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyVector2>> Vector2Tracks
        {
            get;
            set;
        }

        /// <summary>Property to set or return the track used to update any values using a 3D vector.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyVector3>> Vector3Tracks
        {
            get;
            set;
        }

        /// <summary>Property to set or return the track used to update any values using a 4D vector.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyVector4>> Vector4Tracks
        {
            get;
            set;
        }

        /// <summary>Property to set or return the track used to update any values using a <see cref="GorgonColor"/>.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyGorgonColor>> ColorTracks
        {
            get;
            set;
        }

        /// <summary>Property to set or return the track used to update any values using a SharpDX <c>RectangleF</c>.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyRectangle>> RectangleTracks
        {
            get;
            set;
        }

        /// <summary>Property to set or return the tracks used for updating a 2D texture on an object.</summary>
        public IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> Texture2DTracks
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the frames per second for this animation.
        /// </summary>
        public float Fps
        {
            get;
        } = 60.0f;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the maximum number of key frames across all tracks.
        /// </summary>
        /// <returns>The maximum number of key frames across all tracks.</returns>
        public int GetMaxKeyFrameCount()
        {
            int result = SingleTracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0);
            result = result.Max(Vector2Tracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0));
            result = result.Max(Vector3Tracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0));
            result = result.Max(Vector4Tracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0));
            result = result.Max(ColorTracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0));
            result = result.Max(RectangleTracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0));
            return result.Max(Texture2DTracks.Select(item => item.Value).DefaultIfEmpty().Max(key => key?.KeyFrames.Count ?? 0));
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationData" /> class.
        /// </summary>
        /// <param name="name">The name of the track.</param>
        /// <param name="fps">The frames per second for the animation.</param>
        /// <param name="length">The length of the animation, in seconds.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public AnimationData(string name, float fps, float length)
            : base(name)
        {
            Length = length.Max(0);
            Fps = fps.Max(1);
        }
        #endregion
    }
}
