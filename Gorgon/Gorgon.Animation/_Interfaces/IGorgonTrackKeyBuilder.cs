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
// Created: August 18, 2018 7:18:17 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Animation
{
    /// <summary>
    /// A builder for building keys on a <see cref="IGorgonAnimationTrack{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of keyframe, must implement <see cref="IGorgonKeyFrame"/> and be a reference type.</typeparam>
    /// <seealso cref="IGorgonKeyFrame"/>
    public interface IGorgonTrackKeyBuilder<in T>
        where T : class, IGorgonKeyFrame
    {
        /// <summary>
        /// Function to clear the key list for the track being edited.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        IGorgonTrackKeyBuilder<T> Clear();

        /// <summary>
        /// Function to set a key to the track.  
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="key"/> is already in the key list.</exception>
        /// <remarks>
        /// <para>
        /// This will add, or replace, using the <see cref="IGorgonKeyFrame.Time"/> index, a key frame on the track.
        /// </para>
        /// </remarks>
        IGorgonTrackKeyBuilder<T> SetKey(T key);

        /// <summary>
        /// Function to assign a set of keys to the track.
        /// </summary>
        /// <param name="keys">The list of keys to assign to the track.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="keys"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if a key within the <paramref name="keys"/> parameter is already in the key list.</exception>
        /// <returns>The fluent interface for this builder.</returns>
        /// <remarks>
        /// <para>
        /// This will add, or replace, using the <see cref="IGorgonKeyFrame.Time"/> index, a key frame on the track.
        /// </para>
        /// </remarks>
        IGorgonTrackKeyBuilder<T> SetKeys(IEnumerable<T> keys);

        /// <summary>
        /// Function to delete a key at the specified time index.
        /// </summary>
        /// <param name="time">The time index for the key.</param>
        /// <returns>The fluent interface for this builder.</returns>
        IGorgonTrackKeyBuilder<T> DeleteKeyAtTime(float time);

        /// <summary>
        /// Function to delete a key at the specified time index.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is <b>null</b>.</exception>
        IGorgonTrackKeyBuilder<T> DeleteKey(T key);

        /// <summary>
        /// Function to set the interpolation mode for this track.
        /// </summary>
        /// <param name="mode">The interpolation mode to assign to the track.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <remarks>
        /// <para>
        /// Not all track types provide interpolation modes, in those cases, this value will be ignored.
        /// </para>
        /// </remarks>
        IGorgonTrackKeyBuilder<T> SetInterpolationMode(TrackInterpolationMode mode);

        /// <summary>
        /// Function to end editing of the track.
        /// </summary>
        /// <returns>The <see cref="GorgonAnimationBuilder"/> for the animation containing the track being edited.</returns>
        /// <seealso cref="GorgonAnimationBuilder"/>
        GorgonAnimationBuilder EndEdit();
    }
}