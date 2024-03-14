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
// Created: February 12, 2021 1:07:51 AM
// 
#endregion

using System.Numerics;

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
public interface IGorgonSplineCalculation
{
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
    Vector4 GetInterpolatedValue(int startPointIndex, float delta);

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
    Vector4 GetInterpolatedValue(float delta);
}

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
public interface IGorgonSpline
    : IGorgonSplineCalculation
{
    #region Properties.
    /// <summary>
    /// Property to return the list of points for the spline.
    /// </summary>
    /// <remarks>
    /// When adding or removing points> from the spline, a call to the <see cref="UpdateTangents"/> method is required to recalculate the tangents. Otherwise, the spline interpolation will be incorrect.
    /// </remarks>
    IList<Vector4> Points
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to calculate the tangent vectors.
    /// </summary>
    /// <remarks>
    /// This function is used to calculate the tangent vectors from the points provided so that the object can interpolate a point in between the points given. Because this method requires the <see cref="Points"/>, 
    /// it must be called whenever a change to the <see cref="Points"/> property is made.
    /// </remarks>
    void UpdateTangents();
    #endregion
}
