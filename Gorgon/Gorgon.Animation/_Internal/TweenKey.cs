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
// Created: August 18, 2018 1:17:00 PM
// 
#endregion

using System;
using Gorgon.Math;

namespace Gorgon.Animation
{
    /// <summary>
    /// Value type containing information about the nearest keys for a time position.
    /// </summary>
    public static class TweenKey
    {
        #region Methods.
        /// <summary>
        /// Function to search for the previous and next key for a specific time code.
        /// </summary>
        /// <typeparam name="T">The type of key.</typeparam>
        /// <param name="requestedTime">The time code to look up.</param>
        /// <param name="track">The track containing the keys to enumerate.</param>
        /// <param name="startIndex">The starting index to search from.</param>
        /// <param name="endIndex">The end index to search up to.</param>
        /// <returns>A tuple containing the previous and next key frame value, and the index of the previous key.</returns>
        private static (T prev, T next, int prevKeyIndex) PrevNextSearch<T>(float requestedTime, IGorgonAnimationTrack<T> track, int startIndex, int endIndex)
            where T : class, IGorgonKeyFrame
        {
            while (true)
            {
                T firstKey = track.KeyFrames[startIndex];
                T lastKey = track.KeyFrames[endIndex];

                switch (track.KeyFrames.Count)
                {
                    case 1:
                        return (firstKey, firstKey, startIndex);
                    case 2:
                        return (firstKey, lastKey, startIndex);
                }

                // If we're actually on the key frame, then there's no need to continue.
                if (requestedTime.EqualsEpsilon(firstKey.Time))
                {
                    return (startIndex < track.KeyFrames.Count - 1)
                               ? (firstKey, track.KeyFrames[startIndex + 1], startIndex)
                               : (track.KeyFrames[startIndex - 1], firstKey, startIndex - 1);
                }

                if (requestedTime.EqualsEpsilon(lastKey.Time))
                {
                    return (endIndex > 0) ? (track.KeyFrames[endIndex - 1], lastKey, endIndex - 1) : (lastKey, track.KeyFrames[endIndex + 1], endIndex);
                }

                int midIndex = (int)((endIndex - startIndex) / 2.0f) + startIndex;
                T midKey = track.KeyFrames[midIndex];

                // The time is in the first part of the collection.
                if ((requestedTime >= firstKey.Time) && (requestedTime < midKey.Time))
                {
                    endIndex = midIndex;

                    // The indices should have a distance of 1 if we want to locate our begin and end.
                    if ((endIndex - startIndex) <= 1)
                    {
                        return (firstKey, midKey, startIndex);
                    }

                    continue;
                }

                startIndex = midIndex;

                // The indices should have a distance of 1 if we want to locate our begin and end.
                if ((endIndex - startIndex) <= 1)
                {
                    return (midKey, lastKey, midIndex);
                }
            }
        }

        /// <summary>
        /// Function to return the nearest keys to the requested time.
        /// </summary>
        /// <typeparam name="T">The type of key in the track. Must implement <see cref="IGorgonKeyFrame"/> and be a reference type.</typeparam>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="requestedTime">Track time requested.</param>
        /// <param name="animationLength">The total animation time.</param>
        /// <returns>A tuple containing the previous and next key that falls on or outside of the requested time, the index of the first key and the delta time between the start frame and the requested time.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="track"/> parameter is <b>null</b>.</exception>
        public static (T previous, T next, int prevKeyIndex, float timeDelta) GetNearestKeys<T>(
            IGorgonAnimationTrack<T> track,
            float requestedTime,
            float animationLength)
            where T : class, IGorgonKeyFrame
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            // Wrap around for time so that we don't overextend.
            while (requestedTime > animationLength)
            {
                requestedTime -= animationLength;
            }

            (T prevKey, T nextKey, int prevKeyIndex) = PrevNextSearch(requestedTime, track, 0, track.KeyFrames.Count - 1);

            // Ensure that we clip to the animation length.
            float keyDelta = (nextKey.Time > animationLength ? animationLength : nextKey.Time) - prevKey.Time;

            // Calculate the delta time.
            return prevKey.Time.EqualsEpsilon(requestedTime)
                       ? (prevKey, nextKey, prevKeyIndex, 0.0f)
                       : (prevKey, nextKey, prevKeyIndex, ((requestedTime - prevKey.Time) / keyDelta).Min(1.0f).Max(0));
        }
        #endregion
    }
}
