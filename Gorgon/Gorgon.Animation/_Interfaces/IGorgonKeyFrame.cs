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
// Created: Sunday, September 23, 2012 11:35:35 AM
// 
#endregion

using System;
using Gorgon.Core;
using Newtonsoft.Json;

namespace Gorgon.Animation
{
    /// <summary>
    /// An animation key frame.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A key frame represents a value for an object property at a given time. 
    /// </para>
    /// <para>
    /// The track that the key frame is on is used to interpolate the value between key frames. This method makes it so that only a few keyframes are required for an animation rather then setting a value
    /// for every time index.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonAnimationTrack{T}"/>
    public interface IGorgonKeyFrame
        : IGorgonCloneable<IGorgonKeyFrame>
    {
        /// <summary>
        /// Property to return the time at which the key frame is stored.
        /// </summary>
        float Time
        {
            get;
        }

        /// <summary>
        /// Property to return the type of data for this key frame.
        /// </summary>
        [JsonIgnore]
        Type DataType
        {
            get;
        }
    }
}
