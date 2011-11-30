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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A viewport rectangle to define extents for screen space rendering.
	/// </summary>
	public struct GorgonViewport
	{
		#region Variables.
		/// <summary>
		/// The rectangular region used for screen space clipping/scaling.
		/// </summary>
		/// <remarks>The width and height must be greater than or equal to 0.</remarks>
		public RectangleF Region;
		/// <summary>
		/// The minimum depth for the viewport.
		/// </summary>
		/// <remarks>This must be between 0 and 1.</remarks>
		public float MinimumZ;
		/// <summary>
		/// The maximum depth for the viewport.
		/// </summary>
		/// <remarks>This must be between 0 and 1.</remarks>
		public float MaximumZ;
		/// <summary>
		/// Flag to indicate that the viewport is enabled.
		/// </summary>
		/// <remarks>The default value is TRUE.</remarks>
		public bool IsEnabled;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert this viewport rectangle into a thingy.
		/// </summary>
		/// <returns></returns>
		internal D3D.Viewport Convert()
		{
			return new D3D.Viewport(Region.X, Region.Y, Region.Width, Region.Height, MinimumZ, MaximumZ);
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
			return ((left.Region == right.Region) && (left.MinimumZ == right.MinimumZ) && (left.MaximumZ == right.MaximumZ));
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
			return !(left == right);
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
				GorgonViewport view = (GorgonViewport)obj;
				return ((view.Region == Region) && (view.MinimumZ == MinimumZ) && (view.MaximumZ == MaximumZ));
			}

			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Region.GetHashCode() ^ MaximumZ.GetHashCode() ^ MinimumZ.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Gorgon Viewport:\n{0}x{1}-{2}x{3} (Width: {4}, Height: {5})\nMinimum depth: {6}\nMaximum depth: {7}", Region.X, Region.Y, Region.Right, Region.Bottom, Region.Width, Region.Height, MinimumZ, MaximumZ);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
		/// </summary>
		/// <param name="region">The position and size for the screen space region.</param>
		/// <param name="minZ">Minimum depth.</param>
		/// <param name="maxZ">Maximum depth.</param>
		/// <remarks>The <paramref name="minZ"/> and <paramref name="maxZ"/> parameters must be between 0 and 1.  The width and height of the <paramref name="region"/> parameter must be greater than or equal to 0.</remarks>
		public GorgonViewport(RectangleF region, float minZ, float maxZ)
		{
			Region = region;
			MinimumZ = minZ;
			MaximumZ = maxZ;
			IsEnabled = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewport"/> struct.
		/// </summary>
		/// <param name="region">The position and size for the screen space region.</param>
		/// <remarks>The width and height of the <paramref name="region"/> parameter must be greater than or equal to 0.</remarks>
		public GorgonViewport(RectangleF region)
			: this(region, 0.0f, 1.0f)
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
			: this(new RectangleF(x, y, width, height), minZ, maxZ)
		{
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
			: this(new RectangleF(x, y, width, height), 0.0f, 1.0f)
		{
		}
		#endregion
	}
}
