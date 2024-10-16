﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, October 3, 2012 9:13:59 PM
// 

using System.Numerics;

namespace Gorgon.Animation;

/// <summary>
/// An animation key frame for a SharpDX <c>Vector4</c> type
/// </summary>
/// <remarks>
/// <para>
/// A key frame represents a value for an object property at a given time. 
/// </para>
/// <para>
/// The track that the key frame is on is used to interpolate the value between key frames. This method makes it so that only a few key frames are required for an animation rather then setting a value
/// for every time index
/// </para>
/// </remarks>
/// <seealso cref="IGorgonAnimationTrack{T}"/>
public class GorgonKeyVector4
    : IGorgonKeyFrame
{

    // The value for the key.
    private Vector4 _value;

    /// <summary>
    /// Property to return the time at which the key frame is stored.
    /// </summary>
    public float Time
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to set or return the value for the key frame.
    /// </summary>
    public ref Vector4 Value => ref _value;

    /// <summary>
    /// Property to return the type of data for this key frame.
    /// </summary>
    public Type DataType
    {
        get;
    } = typeof(Vector4);

    /// <summary>
    /// Function to clone an object.
    /// </summary>
    /// <returns>The cloned object.</returns>
    public IGorgonKeyFrame Clone() => new GorgonKeyVector4(this);

    /// <summary>Initializes a new instance of the <see cref="GorgonKeyVector4"/> class.</summary>
    /// <param name="key">The key to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> parameter is <b>null</b>.</exception>
    public GorgonKeyVector4(GorgonKeyVector4 key)
    {
        Time = key?.Time ?? throw new ArgumentNullException(nameof(key));
        Value = key.Value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonKeyVector4" /> class.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="value">The value to apply to the key frame.</param>
    public GorgonKeyVector4(float time, Vector2 value)
    {
        Time = time;
        Value = new Vector4(value, 0, 0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonKeyVector4" /> class.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="value">The value to apply to the key frame.</param>
    public GorgonKeyVector4(float time, Vector3 value)
    {
        Time = time;
        Value = new Vector4(value, 0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonKeyVector4" /> class.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="value">The value to apply to the key frame.</param>
    public GorgonKeyVector4(float time, Vector4 value)
    {
        Time = time;
        Value = value;
    }
}
