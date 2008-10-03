#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 11:44:51 AM
// 
#endregion

using System;
using System.Collections.Generic;

namespace GorgonLibrary
{
    /// <summary>
    /// Object representing a spline.
    /// </summary>
    public class Spline
    {
        #region Variables.
        private Matrix _coefficients = Matrix.Identity;     // Coefficients.
        private List<Vector2D> _tangents = null;            // Tangents.
        private List<Vector2D> _points = null;              // Points.
        private bool _autoCalculate = true;                 // Flag to auto calculate the spline.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to autocalculate the spline upon changes.
        /// </summary>
        public bool AutoCalculate
        {
            get
            {
                return _autoCalculate;
            }
            set
            {
                _autoCalculate = value;
            }
        }

        /// <summary>
        /// Property to return a point in the spline.
        /// </summary>
        /// <param name="index">Index of the point in the spline.</param>
        /// <returns>Vector point in the spline.</returns>
        public Vector2D this[int index]
        {
            get
            {
                if ((index < 0) || (index >= _points.Count))
                    throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

                return _points[index];
            }
            set
            {
                if ((index < 0) || (index >= _points.Count))
                    throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

                _points[index] = value;
                if (_autoCalculate)
                    RecalculateTangents();
            }
        }

        /// <summary>
        /// Property to return an interpolated point from the spline.
        /// </summary>
        /// <param name="index">Index to start at.</param>
        /// <param name="time">Time index to use.</param>
        /// <returns>Interpolated point in the spline.</returns>
        public Vector2D this[int index, float time]
        {
            get
            {
                Vector4D power = Vector4D.Zero;     // Powers.
                Matrix result = Matrix.Identity;    // Result matrix.
                Vector2D point1, point2;            // First and second point.
                Vector2D tangent1, tangent2;        // First and second tangent.

                if ((index < 0) || (index >= _points.Count))
                    throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

                // Return last point since we can't blend beyond this point.
                if ((index + 1) == _points.Count)
                    return _points[index];

                // Return the first point or the next for speed purposes.
                if (time == 0.0f)
                    return _points[index];
                if (time == 1.0f)
                    return _points[index + 1];

                // Calculate powers.
                power = new Vector4D(time * time * time, time * time, time, 1.0f);

                // Get points and tangents.
                point1 = _points[index];
                point2 = _points[index + 1];
                tangent1 = _tangents[index];
                tangent2 = _tangents[index + 1];

				result[1,1] = point1.X;
                result[1,2] = point1.Y;
                result[1,3] = 1.0f;
                result[1,4] = 1.0f;

                result[2,1] = point2.X;
                result[2,2] = point2.Y;
                result[2,3] = 1.0f;
                result[2,4] = 1.0f;

                result[3,1] = tangent1.X;
                result[3,2] = tangent1.Y;
                result[3,3] = 1.0f;
                result[3,4] = 1.0f;

                result[4,1] = tangent2.X;
                result[4,2] = tangent2.Y;
                result[4,3] = 1.0f;
                result[4,4] = 1.0f;

                Vector4D value = (_coefficients * result) * power;
                
                return new Vector2D(value.X, value.Y);
            }
        }

        /// <summary>
        /// Property to return an interpolated point from the spline.
        /// </summary>
        /// <param name="time">Time index to use.</param>
        /// <returns>Interpolated point in the spline.</returns>
        public Vector2D this[float time]
        {
            get
            {
                float segment = time * (_points.Count - 1);     // Line segment.
                int segmentIndex = (int)segment;                // Segment index.

                return this[segmentIndex, segment - segmentIndex];
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to recalculate the spline tangents using Catmull-Rom.
        /// </summary>
        public void RecalculateTangents()
        {
            bool closed = false;            // Flag to indicate whether the spline is closed or not.

            // Need 2 or more points.
            if (_points.Count < 2)
                return;

            // Closed or open?
            if (_points[0] == _points[_points.Count - 1])
                closed = true;

            // Pre-create.
            _tangents.Clear();
            for (int i = 0; i < _points.Count; i++)
                _tangents.Add(Vector2D.Zero);

            // Calculate...
            for (int i = 0; i < _points.Count; i++)
            {
                if (i == 0)
                {
                    if (closed)
                        _tangents[i] = Vector2D.Multiply(Vector2D.Subtract(_points[1], _points[_points.Count - 2]), 0.5f);
                    else
                        _tangents[i] = Vector2D.Multiply(Vector2D.Subtract(_points[1], _points[0]), 0.5f);
                }
                else
                {
                    if (i == _points.Count - 1)
                    {
                        // Special case end
                        if (closed)
                            _tangents[i] = _tangents[0];
                        else
                            _tangents[i] = Vector2D.Multiply(Vector2D.Subtract(_points[i], _points[i - 1]), 0.5f);
                    }
                    else
                        _tangents[i] = Vector2D.Multiply(Vector2D.Subtract(_points[i + 1], _points[i - 1]), 0.5f);
                }
            }
        }

        /// <summary>
        /// Function to remove a point from the spline.
        /// </summary>
        /// <param name="index">Index of the point to remove.</param>
        public void Remove(int index)
        {
            if ((index < 0) || (index >= _points.Count))
                throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

            _points.RemoveAt(index);

            if (_autoCalculate)
                RecalculateTangents();
        }

        /// <summary>
        /// Function to clear the points in the spline.
        /// </summary>
        public void Clear()
        {
            _points.Clear();
            _tangents.Clear();
        }

        /// <summary>
        /// Function to add a new point to the spline.
        /// </summary>
        /// <param name="point">Point for the spline.</param>
        public void AddPoint(Vector2D point)
        {
            AddPoint(point.X, point.Y);            
        }

        /// <summary>
        /// Function to add a new point to the spline.
        /// </summary>
        /// <param name="x">Horizontal position.</param>
        /// <param name="y">Vertical position.</param>
        public void AddPoint(float x, float y)
        {
            _points.Add(new Vector2D(x, y));

            // Recalcualte the tangents.
            if (_autoCalculate)
                RecalculateTangents();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public Spline()
        {
            // Create coefficient lookups.
            _coefficients[1, 1] = 2;
            _coefficients[1, 2] = -2;
            _coefficients[1, 3] = 1;
            _coefficients[1, 4] = 1;

            _coefficients[2, 1] = -3;
            _coefficients[2, 2] = 3;
            _coefficients[2, 3] = -2;
            _coefficients[2, 4] = -1;

            _coefficients[3, 1] = 0;
            _coefficients[3, 2] = 0;
            _coefficients[3, 3] = 1;
            _coefficients[3, 4] = 0;

            _coefficients[4, 1] = 1;
            _coefficients[4, 2] = 0;
            _coefficients[4, 3] = 0;
            _coefficients[4, 4] = 0;

            _points = new List<Vector2D>();
            _tangents = new List<Vector2D>();
        }
        #endregion
    }
}
