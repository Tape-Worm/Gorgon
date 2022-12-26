#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Wednesday, October 02, 2012 07:17:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Numerics;
using Gorgon.Diagnostics;

namespace Gorgon.Math;

/// <summary>
/// Returns spline interpolated values across a set of points.
/// </summary>
/// <remarks>
/// <para>
/// This allows spline interpolation when iterating between points over time. This allows for a nice smoothing effect as a value approaches a node point on the spline. 
/// </para>
/// <para>
/// Because this class provides smoothing between the nodes on the a spline, the result is very different than that of a linear interpolation. A linear interpolation will go in a straight line until 
/// the end point is reached. This can give a jagged looking effect when a point moves between several points that are in vastly different places. But the spline will smooth the transition for the 
/// value travelling to the destination points, thus giving a curved appearance when a point traverses the spline.
/// </para>
/// <para>
/// When adding or removing <see cref="Points"/> from the spline, remember to call <see cref="UpdateTangents"/> to recalculate the tangents.
/// </para>
/// <para>
/// This spline object uses the <a href="https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline">Catmull-Rom algorithm</a> to perform its work.
/// </para>
/// </remarks>
/// <example>
/// An example on how to use the spline object:
/// <code language="csharp">
/// <![CDATA[
/// IGorgonSpline spline = new GorgonCatmullRomSpline();
/// 
/// spline.Points.Add(new Vector2(0, 0));
/// spline.Points.Add(new Vector2(1, 4.5f));
/// spline.Points.Add(new Vector2(7, -2.3f));
/// spline.Points.Add(new Vector2(10.2f, 0));
/// 
/// spline.UpdateTangents();
/// 
/// 
/// float startTime = GorgonTiming.SecondsSinceStart;
/// float endTime = GorgonTiming.SecondsSinceStart + 5;
/// float currentTime = 0;
/// 
/// while (currentTime < 1.0f)
/// {
///		Vector4 result = spline.GetInterpolatedValue(currentTime);
/// 
///		// Do something with the result... like plot a pixel:
///		// e.g PutPixel(result.X, result.Y, Color.Blue); or something.
///		// and over 5 seconds, a curved series of points should be plotted.
/// 
///		currentTime = GorgonTiming.SecondsSinceStart / (endTime - startTime);
/// } 
/// ]]>
/// </code>
/// </example>
public class GorgonCatmullRomSpline
    : IGorgonSpline
{
    #region Variables.
    // Spline coefficients.
    private Matrix4x4 _coefficients = Matrix4x4.Identity;
    // Tangents.
    private Vector4[] _tangents;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the list of points for the spline.
    /// </summary>
    /// <remarks>
    /// When adding or removing points from the spline, a call to the <see cref="IGorgonSpline.UpdateTangents"/> method is required to recalculate the tangents. Otherwise, the spline interpolation will be incorrect.
    /// </remarks>
    public IList<Vector4> Points
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to return an interpolated point from the spline.
    /// </summary>
    /// <param name="startPointIndex">Index in the point list to start from.</param>
    /// <param name="delta">Delta value to interpolate.</param>
    /// <returns>The interpolated value at <paramref name="delta"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startPointIndex"/> parameter is less than 0, or greater than/equal to the number of points in the spline minus 1.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="delta"/> parameter is a unit value where 0 is the first point in the spline (relative to <paramref name="startPointIndex"/>) and 1 is the next point from the <paramref name="startPointIndex"/> in the spline.
    /// </para>
    /// <para>
    /// If the <paramref name="delta"/> is less than 0, or greater than 1, the value will be wrapped to fit within the 0..1 range.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><c>[Debug only]</c> Thrown when the <paramref name="startPointIndex"/> is less than 0, or greater than/equal to the number of points - 1 in the <see cref="IGorgonSpline.Points"/> parameter.</exception>        
		public Vector4 GetInterpolatedValue(int startPointIndex, float delta)
    {
        Matrix4x4 calculations = Matrix4x4.Identity;

        startPointIndex.ValidateRange(nameof(startPointIndex), 0, Points.Count - 1);

        if (delta.EqualsEpsilon(0.0f))
        {
            return Points[startPointIndex];
        }

        if (delta.EqualsEpsilon(1.0f))
        {
            return Points[startPointIndex + 1];
        }

        var deltaCubeSquare = new Vector4(delta * delta * delta, delta * delta, delta, 1.0f);
                    
        calculations.SetRow(0, Points[startPointIndex]);
        calculations.SetRow(1, Points[startPointIndex + 1]);
        calculations.SetRow(2, _tangents[startPointIndex]);
        calculations.SetRow(3, _tangents[startPointIndex + 1]);

        var calcResult = Matrix4x4.Multiply(_coefficients, calculations);
        return Vector4.Transform(deltaCubeSquare, calcResult);
    }

    /// <summary>
    /// Function to return an interpolated point from the spline.
    /// </summary>
    /// <param name="delta">Delta value to interpolate.</param>
    /// <returns>The interpolated value at <paramref name="delta"/>.</returns>
    /// <remarks>
    /// The <paramref name="delta"/> parameter is a unit value where 0 is the first point in the spline and 1.0 is the last point in the spline.
    /// <para>
    /// If the <paramref name="delta"/> is less than 0, or greater than 1, the value will be wrapped to fit within the 0..1 range.
    /// </para>
    /// </remarks>
    public Vector4 GetInterpolatedValue(float delta)
    {
        // Wrap to 0 and 1.
        while (delta < 0.0f)
        {
            delta += 1.0f;
        }

        while (delta > 1.0f)
        {
            delta -= 1.0f;
        }

        float segment = delta * (Points.Count - 1);
        float index = (int)segment;

        return GetInterpolatedValue((int)(delta * (Points.Count - 1)), segment - index);
    }

    /// <summary>
    /// Function to calculate the tangent vectors.
    /// </summary>
    /// <remarks>
    /// This function is used to calculate the tangent vectors from the points provided so that the object can interpolate a point in between the points given. Because this method requires the <see cref="IGorgonSpline.Points"/>, 
    /// it must be called whenever a change to the <see cref="IGorgonSpline.Points"/> property is made.
    /// </remarks>
    public void UpdateTangents()
    {
        // Need 2 or more points.
        if (Points.Count < 2)
        {
            return;
        }

        // Closed or open?
        bool closed = Points[0] == Points[Points.Count - 1];

        if (_tangents.Length < Points.Count)
        {
            _tangents = new Vector4[Points.Count * 2];
        }

        // Calculate...
        for (int i = 0; i < Points.Count; i++)
        {
            Vector4 next;
            Vector4 prev;

            if (i == 0)
            {
                next = Points[1];
                prev = closed ? Points[Points.Count - 2] : Points[0];
            }
            else
            {
                if (i == Points.Count - 1)
                {
                    // Special case end.
                    if (closed)
                    {
                        _tangents[i] = _tangents[0];
                        continue;
                    }

                    next = Points[i];
                    prev = Points[i - 1];
                }
                else
                {
                    next = Points[i + 1];
                    prev = Points[i - 1];
                }
            }

            var diff = Vector4.Subtract(next, prev);
            _tangents[i] = Vector4.Multiply(diff, 0.5f);
        }
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCatmullRomSpline"/> object.
    /// </summary>
    public GorgonCatmullRomSpline()
    {
        _coefficients.SetRow(0, new Vector4(2, -2, 1, 1));
        _coefficients.SetRow(1, new Vector4(-3, 3, -2, -1));
        _coefficients.SetRow(2, new Vector4(0, 0, 1, 0));
        _coefficients.SetRow(3, new Vector4(1, 0, 0, 0));

        Points = new List<Vector4>(256);
        _tangents = new Vector4[256];
    }
    #endregion
}
