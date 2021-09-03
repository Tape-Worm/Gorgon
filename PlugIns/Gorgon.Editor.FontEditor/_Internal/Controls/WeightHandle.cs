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
// Created: September 7, 2021 11:02:36 PM
// 
#endregion

using System.Drawing;
using Gorgon.Graphics;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// Represents the handle for a node on the gradient fill control.
    /// </summary>
    internal class WeightHandle
    {
        #region Properties.
        /// <summary>
        /// Property to return the index of the interpolation value.
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the color for the weight node.
        /// </summary>
        public GorgonColor Color
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the weight for the node.
        /// </summary>
        public float Weight
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="WeightHandle"/> class.
        /// </summary>
        /// <param name="weight">The weight of the node.</param>
        /// <param name="color">Color for the node.</param>
        /// <param name="index">The index of the corresponding interpolation value.</param>
        public WeightHandle(float weight, GorgonColor color, int index)
        {
            Weight = weight;
            Color = color;
            Index = index;
        }
        #endregion
    }
}
