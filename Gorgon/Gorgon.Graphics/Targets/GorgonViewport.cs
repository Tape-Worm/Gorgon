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
// Created: Tuesday, November 22, 2011 9:10:21 AM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using SharpDX;
using D3D = SharpDX.Direct3D11;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A viewport rectangle to define extents for screen space rendering.
	/// </summary>
	public struct GorgonViewport
		: IEquatable<GorgonViewport>
	{
		#region Variables.
        /// <summary>
        /// An empty viewport.
        /// </summary>
	    public static readonly GorgonViewport Empty = new GorgonViewport(true);
        
	    private readonly bool _isEmpty;      // Flag to indicate that the viewport is empty.

        /// <summary>
        /// The horizontal position of the viewport.
        /// </summary>
	    public readonly float Left;
        /// <summary>
        /// The vertical position of the viewport.
        /// </summary>
	    public readonly float Top;
        /// <summary>
        /// The width of the viewport.
        /// </summary>
        /// <remarks>This value must be greater than or equal to 0.</remarks>
	    public readonly float Width;
        /// <summary>
        /// The height of the viewport.
        /// </summary>
        /// <remarks>This value must be greater than or equal to 0.</remarks>
	    public readonly float Height;
		/// <summary>
		/// The minimum depth for the viewport.
		/// </summary>
		/// <remarks>This must be between 0 and 1.</remarks>
		public readonly float MinimumZ;
		/// <summary>
		/// The maximum depth for the viewport.
		/// </summary>
		/// <remarks>This must be between 0 and 1.</remarks>
		public readonly float MaximumZ;
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the viewport is empty or not.
        /// </summary>
	    public bool IsEmpty
	    {
	        get
	        {
	            return _isEmpty;
	        }
	    }

        /// <summary>
        /// Property to return the right coordinate of the viewport.
        /// </summary>
        /// <remarks>This is the sum of the left coordinate and the width.</remarks>
        public float Right
        {
            get
            {
                return Width + Left;
            }
        }

        /// <summary>
        /// Property to return the bottom coordinate of the viewport.
        /// </summary>
        /// <remarks>This is the sum of the top coordinate and the height.</remarks>
	    public float Bottom
	    {
	        get
	        {
	            return Height + Top;
	        }
	    }
        #endregion

        #region Methods.
        /// <summary>
		/// Function to convert this viewport rectangle into a Direct3D viewport.
		/// </summary>
		/// <returns>The Direct3D viewport value type.</returns>		
		internal ViewportF Convert()
		{
			return new ViewportF(Left, Top, Width, Height, MinimumZ, MaximumZ);
		}

		/// <summary>
		/// Function to determine if the two values are equal.
		/// </summary>
		/// <param name="left">Left value to compare.</param>
		/// <param name="right">Right value to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public static bool Equals(ref GorgonViewport left, ref GorgonViewport right)
		{
		    return ((left.Left.EqualsEpsilon(right.Left)) && (left.Top.EqualsEpsilon(right.Top))
		            && (left.Width.EqualsEpsilon(right.Width)) && (left.Height.EqualsEpsilon(right.Height))
		            && (left.MinimumZ.EqualsEpsilon(right.MinimumZ)) && (left.MaximumZ.EqualsEpsilon(right.MaximumZ))
                    && (left._isEmpty == right._isEmpty));
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonViewport left, GorgonViewport right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonViewport left, GorgonViewport right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
		    if (obj is GorgonViewport)
		    {
		        return Equals((GorgonViewport)obj);
		    }

		    return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return 281.GenerateHash(Left).GenerateHash(Left).GenerateHash(Width).GenerateHash(Height).
						GenerateHash(MinimumZ).GenerateHash(MaximumZ).GenerateHash(_isEmpty);
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_VIEWPORT_TOSTR, Left, Top, Right, Bottom, Width, Height, MinimumZ, MaximumZ);
		}

        /// <summary>
        /// Implicit operator to convert this viewport into a System.Drawing.RectangleF.
        /// </summary>
        /// <param name="viewport">Viewport to convert.</param>
        /// <returns>The viewport converted into a rectangle.</returns>
        public static explicit operator RectangleF(GorgonViewport viewport)
        {
            return new RectangleF(viewport.Left, viewport.Top, viewport.Width, viewport.Height);
        }

        /// <summary>
        /// Implicit operator to convert this viewport into a System.Drawing.Rectangle.
        /// </summary>
        /// <param name="viewport">Viewport to convert.</param>
        /// <returns>The viewport converted into a rectangle.</returns>
        /// <remarks>This will round any fractional coordinates to the nearest whole number.</remarks>
        public static explicit operator Rectangle(GorgonViewport viewport)
        {
            return Rectangle.Round(new RectangleF(viewport.Left, viewport.Top, viewport.Width, viewport.Height));
        }
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
        /// </summary>
        /// <param name="isEmpty">Flag to indicate that the viewport is empty.</param>
        internal GorgonViewport(bool isEmpty)
        {
            _isEmpty = isEmpty;
            Top = 0;
            Left = 0;
            Width = 0;
            Height = 0;
            MinimumZ = 0;
            MaximumZ = 0;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
		/// </summary>
		/// <param name="region">The position and size for the screen space region.</param>
		/// <param name="minZ">Minimum depth.</param>
		/// <param name="maxZ">Maximum depth.</param>
		/// <remarks>The <paramref name="minZ"/> and <paramref name="maxZ"/> parameters must be between 0 and 1.  The width and height of the <paramref name="region"/> parameter must be greater than or equal to 0.</remarks>
		public GorgonViewport(RectangleF region, float minZ, float maxZ)
            : this(region.X, region.Y, region.Width, region.Height, minZ, maxZ)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
		/// </summary>
		/// <param name="region">The position and size for the screen space region.</param>
		/// <remarks>The width and height of the <paramref name="region"/> parameter must be greater than or equal to 0.</remarks>
		public GorgonViewport(RectangleF region)
			: this(region.X, region.Y, region.Width, region.Height, 0.0f, 1.0f)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
		/// </summary>
		/// <param name="x">The horizontal position of the view.</param>
		/// <param name="y">The vertical position of the view.</param>
		/// <param name="width">The width of the view.</param>
		/// <param name="height">The height of the view.</param>
		/// <param name="minZ">Minimum depth.</param>
		/// <param name="maxZ">Maximum depth.</param>
		/// <remarks>The <paramref name="minZ"/> and <paramref name="maxZ"/> parameters must be between 0 and 1.  The <paramref name="width"/> and <paramref name="height"/> parameters must be greater than or equal to 0.</remarks>
		public GorgonViewport(float x, float y, float width, float height, float minZ, float maxZ)
		{
		    Left = x;
		    Top = y;
		    Width = width;
		    Height = height;
		    MinimumZ = minZ;
		    MaximumZ = maxZ;
		    _isEmpty = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
		/// </summary>
		/// <param name="x">The horizontal position of the view.</param>
		/// <param name="y">The vertical position of the view.</param>
		/// <param name="width">The width of the view.</param>
		/// <param name="height">The height of the view.</param>
		/// <remarks>The <paramref name="width"/> and <paramref name="height"/> parameters must be greater than or equal to 0.</remarks>
		public GorgonViewport(float x, float y, float width, float height)
			: this(x, y, width, height, 0.0f, 1.0f)
		{
		}
		#endregion

		#region IEquatable<GorgonViewport> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonViewport other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
