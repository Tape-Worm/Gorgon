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

using System.Collections.Generic;
using Gorgon.Diagnostics;
using SlimMath;

namespace Gorgon.Math
{
    /// <summary>
    /// Used to return spline interpolated values across a set of points.
    /// </summary>
    /// <remarks>When adding or removing <see cref="Gorgon.Math.GorgonSpline.Points">points</see> from the spline, remember to call <see cref="M:GorgonLibrary.Math.GorgonSpline.UpdateTangents">UpdateTangents</see> to recalculate the tangents.</remarks>
    public class GorgonSpline
    {
        #region Variables.
        private Matrix _coefficients = Matrix.Identity;         // Spline coefficients.
        private Vector4[] _tangents;                            // Tangents.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of points for the spline.
        /// </summary>
        /// <remarks>When adding or removing <see cref="P:GorgonLibrary.Math.GorgonSpline.Points">points</see> from the spline, remember to call <see cref="M:GorgonLibrary.Math.GorgonSpline.UpdateTangents">UpdateTangents</see> to recalculate the tangents.</remarks>
        public IList<Vector4> Points
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return an interpolated point from the spline.
        /// </summary>
        /// <param name="startPointIndex">Index in the point list to start from.</param>
        /// <param name="delta">Delta value to interpolate.</param>
        /// <returns>The interpolated value at <paramref name="delta"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startPointIndex"/> parameter is less than 0, or greater than/equal to the number of points in the spline minus 1.</exception>
        /// <remarks>The delta parameter is a unit value where 0 is the first point in the spline (referenced by startPointIndex) and 1 is the next point from the startPointIndex in the spline.</remarks>
		public Vector4 GetInterpolatedValue(int startPointIndex, float delta)
        {
            Matrix calculations = Matrix.Identity;

            GorgonDebug.AssertParamRange(startPointIndex, 0, Points.Count - 1, "startPointIndex");

            if (delta.EqualsEpsilon(0.0f))
            {
                return Points[startPointIndex];
            }

            if (delta.EqualsEpsilon(1.0f))
            {
                return Points[startPointIndex + 1];
            }

            var result = new Vector4(delta * delta * delta, delta * delta, delta * delta, 1.0f);

            calculations.Row1 = Points[startPointIndex];
            calculations.Row2 = Points[startPointIndex + 1];
            calculations.Row3 = _tangents[startPointIndex];
            calculations.Row4 = _tangents[startPointIndex + 1];

            Matrix.Multiply(ref _coefficients, ref calculations, out calculations);
            Vector4.Transform(ref result, ref calculations, out result);

            return result;
        }

        /// <summary>
        /// Function to return an interpolated point from the spline.
        /// </summary>
        /// <param name="delta">Delta value to interpolate.</param>
        /// <returns>The interpolated value at <paramref name="delta"/>.</returns>
        /// <remarks>The delta parameter is a unit value where 0 is the first point in the spline and 1 is the 2nd last point in the spline.</remarks>
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
        /// Function to calculate the tangent vectors using Catmull-Rom interpolation.
        /// </summary>
        public void UpdateTangents()
        {
            bool closed = false;            // Flag to indicate whether the spline is closed or not.            

            // Need 2 or more points.
            if (Points.Count < 2)
            {
                return;
            }

            // Closed or open?
            if (Points[0] == Points[Points.Count - 1])
                closed = true;

            if (_tangents.Length < Points.Count)
                _tangents = new Vector4[Points.Count * 2];

            // Calculate...
            for (int i = 0; i < Points.Count; i++)
            {
                Vector4 prev;
                Vector4 next;
                
                if (i == 0)
                {
                    prev = Points[1];

                    next = closed ? Points[Points.Count - 2] : Points[0];
                }
                else
                {
                    if (i == Points.Count - 1)
                    {
                        // Special case end
                        if (closed)
                        {
                            _tangents[i] = _tangents[0];
                            continue;
                        }

                        prev = Points[i];
                        next = Points[i - 1];
                    }
                    else
                    {
                        prev = Points[i + 1];
                        next = Points[i - 1];
                    }
                }
                                
                Vector4 diff;
                Vector4.Subtract(ref prev, ref next, out diff);
                Vector4.Multiply(ref diff, 0.5f, out diff);
                _tangents[i] = diff;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSpline"/> object.
        /// </summary>
        public GorgonSpline()
        {
            _coefficients.Row1 = new Vector4(2, -2, 1, 1);
            _coefficients.Row2 = new Vector4(-3, 3, -2, -1);
            _coefficients.Row3 = new Vector4(0, 0, 1, 0);
            _coefficients.Row4 = new Vector4(1, 0, 0, 0);

            Points = new List<Vector4>(256);
            _tangents = new Vector4[256];
        }
        #endregion
    }
}
