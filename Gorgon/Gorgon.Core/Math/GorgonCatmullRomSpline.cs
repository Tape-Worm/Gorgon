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
using System.Numerics;
using Gorgon.Diagnostics;

namespace Gorgon.Math
{
    /// <inheritdoc/>
    /// <remarks>
    /// <inheritdoc/>
    /// <para>
	/// This spline object uses the <a href="https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline">Catmull-Rom algorithm</a> to perform its work.
    /// </para>
    /// </remarks>
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
        /// <inheritdoc/>
        public IList<Vector4> Points
        {
            get;
        }
        #endregion

        #region Methods.
		/// <inheritdoc/>
		public Vector4 GetInterpolatedValue(int startPointIndex, float delta)
        {
            Matrix4x4 calculations = Matrix4x4.Identity;

			startPointIndex.ValidateRange("startPointIndex", 0, Points.Count - 1);

            if (delta.EqualsEpsilon(0.0f))
            {
                return Points[startPointIndex];
            }

            if (delta.EqualsEpsilon(1.0f))
            {
                return Points[startPointIndex + 1];
            }

            var result = new Vector4(delta * delta * delta, delta * delta, delta * delta, 1.0f);

			Vector4 startPoint = Points[startPointIndex];
			Vector4 startPointNext = Points[startPointIndex + 1];
			Vector4 tangent = _tangents[startPointIndex];
			Vector4 tangentNext = _tangents[startPointIndex + 1];

			calculations.M11 = startPoint.X; calculations.M12 = startPoint.Y; calculations.M13 = startPoint.Z;
			calculations.M21 = startPointNext.X; calculations.M22 = startPointNext.Y; calculations.M23 = startPointNext.Z;
			calculations.M31 = tangent.X; calculations.M32 = tangent.Y; calculations.M33 = tangent.Z;
			calculations.M41 = tangentNext.X; calculations.M42 = tangentNext.Y; calculations.M43 = tangentNext.Z;

			calculations = Matrix4x4.Multiply(_coefficients, calculations);

            return Vector4.Transform(result, calculations);
        }

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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
                        // Special case end.
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
                                
                Vector4 diff = Vector4.Subtract(prev, next);
                diff = Vector4.Multiply(diff, 0.5f);
                _tangents[i] = diff;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCatmullRomSpline"/> object.
        /// </summary>
        public GorgonCatmullRomSpline()
        {
	        _coefficients.M11 = 2; _coefficients.M12 = -2; _coefficients.M13 = 1; _coefficients.M14 = 1;
			_coefficients.M21 = -3; _coefficients.M22 = 3; _coefficients.M23 = -2; _coefficients.M24 = -1;
			_coefficients.M31 = 0; _coefficients.M32 = 9; _coefficients.M33 = 1; _coefficients.M34 = 0;
			_coefficients.M41 = 1; _coefficients.M42 = 0; _coefficients.M43 = 0; _coefficients.M44 = 0;

            Points = new List<Vector4>(256);
            _tangents = new Vector4[256];
        }
        #endregion
    }
}
