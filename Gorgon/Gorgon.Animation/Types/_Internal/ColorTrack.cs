﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 18, 2018 10:39:26 AM
// 

using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Animation;

/// <summary>
/// A track that stores <see cref="GorgonColor"/> values representing color in an animation
/// </summary>
internal class ColorTrack
    : IGorgonNamedObject, IGorgonAnimationTrack<GorgonKeyGorgonColor>
{
    // The interpolation mode for the track.
    private TrackInterpolationMode _interpolationMode = TrackInterpolationMode.Linear;
    // The spline controller for the track.
    private readonly GorgonCatmullRomSpline _splineController = new();

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>Property to return the type of key frame data stored in this track.</summary>
    public AnimationTrackKeyType KeyFrameDataType => AnimationTrackKeyType.Color;

    /// <summary>
    /// Property to return the type of interpolation supported by the track.
    /// </summary>
    public TrackInterpolationMode SupportsInterpolation => TrackInterpolationMode.Linear | TrackInterpolationMode.Spline;

    /// <summary>
    /// Property to return the spline controller (if applicable) for the track.
    /// </summary>
    public IGorgonSpline SplineController => _splineController;

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
    public IReadOnlyList<GorgonKeyGorgonColor> KeyFrames
    {
        get;
    }

    /// <summary>Property to set or return whether this track is enabled during animation.</summary>
    public bool IsEnabled
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Function to retrieve the value at the specified time index.
    /// </summary>
    /// <param name="timeIndex">The time index, in seconds, within the track to retrieve the value from.</param>
    /// <returns>The value at the specified time index.</returns>
    /// <remarks>
    /// <para>
    /// The value returned by this method may or may not be interpolated based on the value in <see cref="InterpolationMode"/>.  
    /// </para>
    /// </remarks>
    public GorgonKeyGorgonColor GetValueAtTime(float timeIndex)
    {
        if (KeyFrames.Count == 0)
        {
            return null;
        }

        if (KeyFrames.Count == 1)
        {
            return KeyFrames[0];
        }

        GorgonKeyGorgonColor result = KeyFrames.FirstOrDefault(item => item.Time == timeIndex);

        if (result is not null)
        {
            return result;
        }

        TrackKeyProcessor.TryUpdateColor(this, timeIndex, out GorgonColor color);

        return new GorgonKeyGorgonColor(timeIndex, color);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorTrack"/> class.
    /// </summary>
    /// <param name="keyFrames">The list of key frames for the track.</param>
    /// <param name="name">The name of the track.</param>
    internal ColorTrack(IReadOnlyList<GorgonKeyGorgonColor> keyFrames, string name)
    {
        Name = name;
        KeyFrames = keyFrames;

        // Build the spline for the track.
        for (int i = 0; i < keyFrames.Count; ++i)
        {
            _splineController.Points.Add(keyFrames[i].Value);
        }

        _splineController.UpdateTangents();
    }
}
