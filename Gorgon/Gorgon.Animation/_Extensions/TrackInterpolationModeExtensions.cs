#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: March 4, 2021 12:45:19 AM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Animation.Properties;

namespace Gorgon.Animation;

/// <summary>
/// Extension methods for the <see cref="TrackInterpolationMode"/> values.
/// </summary>
public static class TrackInterpolationModeExtensions
{
    // The declared values.
    private static readonly TrackInterpolationMode[] _values = (TrackInterpolationMode[])Enum.GetValues(typeof(TrackInterpolationMode));

    /// <summary>
    /// Function to get a culture sensitive string representing a <see cref="TrackInterpolationMode"/> value.
    /// </summary>
    /// <param name="mode">The mode to evaluate.</param>
    /// <returns>The string representation.</returns>
    public static string GetDescription(this TrackInterpolationMode mode) => mode switch
    {
        TrackInterpolationMode.Linear => Resources.GORANM_DESC_INTERP_LINEAR,
        TrackInterpolationMode.Spline => Resources.GORANM_DESC_INTERP_SPLINE,
        _ => Resources.GORANM_DESC_INTERP_NONE,
    };

    /// <summary>
    /// Function to retrieve a list of explicit values in the <see cref="TrackInterpolationMode"/> passed in.
    /// </summary>
    /// <param name="mode">The interpolation mode(s) to evaluate.</param>
    /// <returns>The list of explicitly declared values.</returns>
    public static IEnumerable<TrackInterpolationMode> GetExplicitValues(this TrackInterpolationMode mode)
    {
        for (int i = 0; i < _values.Length; ++i)
        {
            TrackInterpolationMode modeItem = _values[i];

            if ((mode & modeItem) == modeItem)
            {
                yield return modeItem;
            }
        }
    }


    /// <summary>
    /// Function to get a the <see cref="TrackInterpolationMode"/> from a culture sensitive string.
    /// </summary>
    /// <param name="mode">The string value to evaluate.</param>
    /// <returns>The enum value.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public static TrackInterpolationMode GetTrackInterpolationMode(this string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return TrackInterpolationMode.None;
        }

        if (string.Equals(mode, Resources.GORANM_DESC_INTERP_LINEAR, StringComparison.CurrentCultureIgnoreCase))
        {
            return TrackInterpolationMode.Linear;
        }

        if (string.Equals(mode, Resources.GORANM_DESC_INTERP_SPLINE, StringComparison.CurrentCultureIgnoreCase))
        {
            return TrackInterpolationMode.Spline;
        }

        return TrackInterpolationMode.None;
    }
}
