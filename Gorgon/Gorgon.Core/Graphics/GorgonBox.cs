#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, March 15, 2012 7:34:32 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A structure with a width, height and depth.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value type merely represents the dimensions of a box, it does not draw a box. 
    /// </para>
    /// </remarks>
    public struct GorgonBox
        : IGorgonEquatableByRef<GorgonBox>
    {
        #region Variables.
        /// <summary>
        /// An empty box.
        /// </summary>
	    public static readonly GorgonBox Empty = new GorgonBox
        {
            X = 0,
            Y = 0,
            Z = 0,
            Width = 0,
            Height = 0,
            Depth = 0
        };

        /// <summary>
		/// Horizontal position.
		/// </summary>
		public int X;
        /// <summary>
        /// Vertical position
        /// </summary>
        public int Y;
        /// <summary>
        /// Depth position.
        /// </summary>
        public int Z;
        /// <summary>
        /// Width of the box.
        /// </summary>
        public int Width;
        /// <summary>
        /// Height of the box.
        /// </summary>
        public int Height;
        /// <summary>
        /// Depth of the box.
        /// </summary>
        public int Depth;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to determine if the box is empty.
        /// </summary>
	    public bool IsEmpty => Width == 0 && Height == 0 && Depth == 0 && X == 0 && Y == 0 && Z == 0;

        /// <summary>
        /// Property to set or return the left value for the box.
        /// </summary>
        public int Left
        {
            get => X;
            set => X = value;
        }

        /// <summary>
        /// Property to set or return the top value for the box.
        /// </summary>
        public int Top
        {
            get => Y;
            set => Y = value;
        }

        /// <summary>
        /// Property to set or return the front value for the box.
        /// </summary>
        public int Front
        {
            get => Z;
            set => Z = value;
        }

        /// <summary>
        /// Property to return the right value of the box.
        /// </summary>
        public int Right => X + Width;

        /// <summary>
        /// Property to return the bottom value of the box.
        /// </summary>
        public int Bottom => Y + Height;

        /// <summary>
        /// Property to return the back value of the box.
        /// </summary>
        public int Back => Z + Depth;

        #endregion

        #region Methods.
        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GOR_TOSTR_BOX, X, Y, Z, Right, Bottom, Back, Width, Height, Depth);

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Function to return a box from top, left, front, right, bottom and back coordinates.
        /// </summary>
        /// <param name="top">Top coordinate.</param>
        /// <param name="left">Left coordinate</param>
        /// <param name="front">Front coordinate.</param>
        /// <param name="right">Right coordinate.</param>
        /// <param name="bottom">Bottom coordinate.</param>
        /// <param name="back">Back coordinate.</param>
        /// <returns>A new box with the specified coordinates.</returns>
	    public static GorgonBox FromTLFRBB(int top, int left, int front, int right, int bottom, int back) => new GorgonBox
        {
            X = left,
            Y = top,
            Z = front,
            Width = right - left,
            Height = bottom - top,
            Depth = back - front
        };

        /// <summary>
        /// Function to determine the intersection between 2 boxes.
        /// </summary>
        /// <param name="box1">First box to intersect.</param>
        /// <param name="box2">Second box to intersect.</param>
        /// <param name="result">The resulting intersected box.</param>
	    public static void Intersect(in GorgonBox box1, in GorgonBox box2, out GorgonBox result)
        {
            int left = box2.Left.Max(box1.Left);
            int top = box2.Top.Max(box1.Top);
            int front = box2.Front.Max(box1.Front);

            int right = box2.Right.Min(box1.Right);
            int bottom = box2.Bottom.Min(box1.Bottom);
            int back = box2.Back.Min(box1.Back);

            if ((right < left) || (bottom < top) || (back < front))
            {
                result = Empty;
                return;
            }

            result = FromTLFRBB(left, top, front, right, bottom, back);
        }

        /// <summary>
        /// Function to determine the intersection between 2 boxes.
        /// </summary>
        /// <param name="box1">First box to intersect.</param>
        /// <param name="box2">Second box to intersect.</param>
        /// <returns>The intersected box.</returns>
        public static GorgonBox Intersect(GorgonBox box1, GorgonBox box2)
        {

            Intersect(in box1, in box2, out GorgonBox result);

            return result;
        }

        /// <summary>
        /// Funciton to determine the intersection between this and another box.
        /// </summary>
        /// <param name="box">Box to intersect.</param>
        /// <returns>The intersection between this box and the other box.</returns>
	    public GorgonBox Intersect(GorgonBox box)
        {

            Intersect(in this, in box, out GorgonBox result);

            return result;
        }

        /// <summary>
        /// Function to return whether two instances are equal or not.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
	    public static bool Equals(in GorgonBox left, in GorgonBox right) => ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z)
                    && (left.Width == right.Width) && (left.Height == right.Height) && (left.Depth == right.Depth));

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
	    public override bool Equals(object obj) => obj is GorgonBox box ? box.Equals(in this) : base.Equals(obj);

        /// <summary>
        /// Operator to determine if 2 instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
	    public static bool operator ==(GorgonBox left, GorgonBox right) => Equals(in left, in right);

        /// <summary>
        /// Operator to determine if 2 instances are not equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonBox left, GorgonBox right) => !Equals(in left, in right);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
	    public override int GetHashCode() => 281.GenerateHash(Back)
                    .GenerateHash(Right)
                    .GenerateHash(Bottom)
                    .GenerateHash(Left)
                    .GenerateHash(Top)
                    .GenerateHash(Front);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(GorgonBox other) => Equals(in this, in other);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(in GorgonBox other) => Equals(in this, in other);
        #endregion
    }
}
