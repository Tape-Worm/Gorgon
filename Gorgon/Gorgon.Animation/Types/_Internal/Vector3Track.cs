
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


using System.Numerics;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Animation;

/// <summary>
/// A track that stores 3D vectors representing various properties in an animation
/// </summary>
internal class Vector3Track
    : GorgonNamedObject, IGorgonAnimationTrack<GorgonKeyVector3>
{

    // The interpolation mode for the track.
    private TrackInterpolationMode _interpolationMode = TrackInterpolationMode.Linear;
    // The spline controller for the track.
    private readonly GorgonCatmullRomSpline _splineController = new();



    /// <summary>Property to return the type of key frame data stored in this track.</summary>
    public AnimationTrackKeyType KeyFrameDataType => AnimationTrackKeyType.Vector3;

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
    public IReadOnlyList<GorgonKeyVector3> KeyFrames
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
    public GorgonKeyVector3 GetValueAtTime(float timeIndex)
    {
        if (KeyFrames.Count == 0)
        {
            return null;
        }

        if (KeyFrames.Count == 1)
        {
            return KeyFrames[0];
        }

        GorgonKeyVector3 result = KeyFrames.FirstOrDefault(item => item.Time == timeIndex);

        if (result is not null)
        {
            return result;
        }

        TrackKeyProcessor.TryUpdateVector3(this, timeIndex, out Vector3 vec);

        return new GorgonKeyVector3(timeIndex, vec);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Track"/> class.
    /// </summary>
    /// <param name="keyFrames">The list of key frames for the track.</param>
    /// <param name="name">The name of the track.</param>
    internal Vector3Track(IReadOnlyList<GorgonKeyVector3> keyFrames, string name)
        : base(name)
    {
        KeyFrames = keyFrames;

        // Build the spline for the track.
        for (int i = 0; i < keyFrames.Count; ++i)
        {
            _splineController.Points.Add(new Vector4(keyFrames[i].Value, 1.0f));
        }

        _splineController.UpdateTangents();
    }

}
