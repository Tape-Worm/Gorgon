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
    /// The type of keyframe data for a track.
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
    public struct GorgonTrackRegistration
        : IEquatable<GorgonTrackRegistration>
    {
        #region Variables.
        // The counter for generating IDs.
        private static int _idCount;

        /// <summary>
        /// The ID of the track.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// The name of the track.
        /// </summary>
        public readonly string TrackName;

        /// <summary>
        /// The type of key frame data in the track.
        /// </summary>
        public readonly AnimationTrackKeyType KeyType;
        #endregion

        #region Methods.
        /// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GORANM_TOSTR_TRACKREG, TrackName, KeyType.ToString());

        /// <summary>Determines whether the specified <see cref="object"/> is equal to this instance.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj is GorgonTrackRegistration ? ((GorgonTrackRegistration)obj).Equals(this) : base.Equals(obj);

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => (TrackName == null) ? 0 : 281.GenerateHash(TrackName).GenerateHash(KeyType);

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

        #region Operators.
        /// <summary>
        /// Operator to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonTrackRegistration left, GorgonTrackRegistration right) => left.Equals(right);

        /// <summary>
        /// Operator to determine if two instances are not equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonTrackRegistration left, GorgonTrackRegistration right) => !left.Equals(right);
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="GorgonTrackRegistration"/> struct.</summary>
        /// <param name="trackName">The name of the track.</param>
        /// <param name="keyType">The type of key frame data stored in the track.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="trackName"/>, or the <paramref name="keyType"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="trackName"/> parameter is empty.</exception>
        public GorgonTrackRegistration(string trackName, AnimationTrackKeyType keyType)
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
            KeyType = keyType;
            ID = Interlocked.Increment(ref _idCount);
        }
        #endregion
    }
}
