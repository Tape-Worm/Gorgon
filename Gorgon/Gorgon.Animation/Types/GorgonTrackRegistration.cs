#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: October 30, 2019 11:02:53 AM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Animation.Properties;
using Gorgon.Core;

namespace Gorgon.Animation
{
    /// <summary>
    /// The type of key frame data for a track.
    /// </summary>
    public enum AnimationTrackKeyType
    {
        /// <summary>
        /// Single floating point values.
        /// </summary>
        Single = 0,
        /// <summary>
        /// 2D vector values.
        /// </summary>
        Vector2 = 1,
        /// <summary>
        /// 3D vector values.
        /// </summary>
        Vector3 = 2,
        /// <summary>
        /// 4D vector values.
        /// </summary>
        Vector4 = 3,
        /// <summary>
        /// Rectangle values.
        /// </summary>
        Rectangle = 4,
        /// <summary>
        /// Color values.
        /// </summary>
        Color = 5,
        /// <summary>
        /// 2D texture, coordinate, and array index values.
        /// </summary>
        Texture2D = 6
    }

    /// <summary>
    /// Defines a registration for a track in the controller.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides metadata for an animation track and indicates which properties on an object can be manipulated while rendering the animation. 
    /// </para>
    /// <para>
    /// Applications defining their own animation controllers should define which tracks correspond to which properties by declaring a static field and registering that with the 
    /// <see cref="GorgonAnimationController{T}.RegisterTrack(GorgonTrackRegistration)"/> method in the static constructor of the custom controller.
    /// </para>
    /// </remarks>
    public class GorgonTrackRegistration
        : IEquatable<GorgonTrackRegistration>
    {
        #region Variables.
        // The counter for generating IDs.
        private static int _idCount;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the ID of the track.
        /// </summary>
        public int ID
        {
            get;
        }

        /// <summary>
        /// Property to return the name of the track.
        /// </summary>
        public string TrackName
        {
            get;
        }

        /// <summary>
        /// Property to return the friendly description for the track.
        /// </summary>
        public string Description
        {
            get;
        }

        /// <summary>
        /// Property to return the type of key frame data in the track.
        /// </summary>
        public AnimationTrackKeyType KeyType
        {
            get;
        }

        /// <summary>
        /// Property to return the type of interpolation supported by the track.
        /// </summary>
        public TrackInterpolationMode SupportedInterpolation
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GORANM_TOSTR_TRACKREG, TrackName, KeyType.ToString());

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <span class="keyword">
        ///     <span class="languageSpecificText">
        ///       <span class="cs">true</span>
        ///       <span class="vb">True</span>
        ///       <span class="cpp">true</span>
        ///     </span>
        ///   </span>
        ///   <span class="nu">
        ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
        /// </returns>
        public bool Equals(GorgonTrackRegistration other) => (string.Equals(TrackName, other.TrackName, StringComparison.OrdinalIgnoreCase)) && (KeyType == other.KeyType);
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="GorgonTrackRegistration"/> struct.</summary>
        /// <param name="trackName">The name of the track.</param>
        /// <param name="description">The friendly description of the track.</param>
        /// <param name="keyType">The type of key frame data stored in the track.</param>
        /// <param name="interpolationSupport">[Optional] The type of interpolation supported by the track.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="trackName"/>, or the <paramref name="keyType"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="trackName"/> parameter is empty.</exception>
        public GorgonTrackRegistration(string trackName, string description, AnimationTrackKeyType keyType, TrackInterpolationMode interpolationSupport = TrackInterpolationMode.Spline | TrackInterpolationMode.Linear)
        {
            if (trackName == null)
            {
                throw new ArgumentNullException(nameof(trackName));
            }

            if (string.IsNullOrWhiteSpace(trackName))
            {
                throw new ArgumentEmptyException(nameof(trackName));
            }

            TrackName = trackName;
            Description = string.IsNullOrWhiteSpace(description) ? trackName : description;
            KeyType = keyType;
            SupportedInterpolation = interpolationSupport;
            ID = Interlocked.Increment(ref _idCount);
        }
        #endregion
    }
}
