﻿/*
* Copyright (c) 2007-2010 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections.Generic;

namespace SlimMath
{
	/// <summary>
	/// Represents an axis-aligned bounding box in three dimensional space.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BoundingBox : IEquatable<BoundingBox>, IFormattable
	{
		/// <summary>
		/// The minimum point of the box.
		/// </summary>
		public Vector3 Minimum;

		/// <summary>
		/// The maximum point of the box.
		/// </summary>
		public Vector3 Maximum;

		/// <summary>
		/// Initializes a new instance of the <see cref="SlimMath.BoundingBox"/> struct.
		/// </summary>
		/// <param name="minimum">The minimum vertex of the bounding box.</param>
		/// <param name="maximum">The maximum vertex of the bounding box.</param>
		public BoundingBox(Vector3 minimum, Vector3 maximum)
		{
			this.Minimum = minimum;
			this.Maximum = maximum;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SlimMath.BoundingBox"/> struct.
		/// </summary>
		/// <param name="minimumX">The minimum x-coordinate of the bounding box.</param>
		/// <param name="minimumY">The minimum y-coordinate of the bounding box.</param>
		/// <param name="minimumZ">The minimum z-coordinate of the bounding box.</param>
		/// <param name="maximumX">The maximum x-coordinate of the bounding box.</param>
		/// <param name="maximumY">The maximum y-coordinate of the bounding box.</param>
		/// <param name="maximumZ">The maximum z-coordinate of the bounding box.</param>
		public BoundingBox(float minimumX, float minimumY, float minimumZ, float maximumX, float maximumY, float maximumZ)
		{
			this.Minimum = new Vector3(minimumX, minimumY, minimumZ);
			this.Maximum = new Vector3(maximumX, maximumY, maximumZ);
		}

		/// <summary>
		/// Retrieves the eight corners of the bounding box.
		/// </summary>
		/// <returns>An array of points representing the eight corners of the bounding box.</returns>
		public Vector3[] GetCorners()
		{
			Vector3[] results = new Vector3[8];
			results[0] = new Vector3(Minimum.X, Maximum.Y, Maximum.Z);
			results[1] = new Vector3(Maximum.X, Maximum.Y, Maximum.Z);
			results[2] = new Vector3(Maximum.X, Minimum.Y, Maximum.Z);
			results[3] = new Vector3(Minimum.X, Minimum.Y, Maximum.Z);
			results[4] = new Vector3(Minimum.X, Maximum.Y, Minimum.Z);
			results[5] = new Vector3(Maximum.X, Maximum.Y, Minimum.Z);
			results[6] = new Vector3(Maximum.X, Minimum.Y, Minimum.Z);
			results[7] = new Vector3(Minimum.X, Minimum.Y, Minimum.Z);
			return results;
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a <see cref="SlimMath.Ray"/>.
		/// </summary>
		/// <param name="ray">The ray to test.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public bool Intersects(ref Ray ray)
		{
			float distance;
			return Collision.RayIntersectsBox(ref ray, ref this, out distance);
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a <see cref="SlimMath.Ray"/>.
		/// </summary>
		/// <param name="ray">The ray to test.</param>
		/// <param name="distance">When the method completes, contains the distance of the intersection,
		/// or 0 if there was no intersection.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public bool Intersects(ref Ray ray, out float distance)
		{
			return Collision.RayIntersectsBox(ref ray, ref this, out distance);
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a <see cref="SlimMath.Ray"/>.
		/// </summary>
		/// <param name="ray">The ray to test.</param>
		/// <param name="point">When the method completes, contains the point of intersection,
		/// or <see cref="SlimMath.Vector3.Zero"/> if there was no intersection.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public bool Intersects(ref Ray ray, out Vector3 point)
		{
			return Collision.RayIntersectsBox(ref ray, ref this, out point);
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a <see cref="SlimMath.Plane"/>.
		/// </summary>
		/// <param name="plane">The plane to test.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public PlaneIntersectionType Intersects(ref Plane plane)
		{
			return Collision.PlaneIntersectsBox(ref plane, ref this);
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a triangle.
		/// </summary>
		/// <param name="vertex1">The first vertex of the triangle to test.</param>
		/// <param name="vertex2">The second vertex of the triagnle to test.</param>
		/// <param name="vertex3">The third vertex of the triangle to test.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
		{
			return Collision.BoxIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a <see cref="SlimMath.BoundingBox"/>.
		/// </summary>
		/// <param name="box">The box to test.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public bool Intersects(ref BoundingBox box)
		{
			return Collision.BoxIntersectsBox(ref this, ref box);
		}

		/// <summary>
		/// Determines if there is an intersection between the current object and a <see cref="SlimMath.BoundingSphere"/>.
		/// </summary>
		/// <param name="sphere">The sphere to test.</param>
		/// <returns>Whether the two objects intersected.</returns>
		public bool Intersects(ref BoundingSphere sphere)
		{
			return Collision.BoxIntersectsSphere(ref this, ref sphere);
		}

		/// <summary>
		/// Determines whether the current objects contains a point.
		/// </summary>
		/// <param name="point">The point to test.</param>
		/// <returns>The type of containment the two objects have.</returns>
		public ContainmentType Contains(ref Vector3 point)
		{
			return Collision.BoxContainsPoint(ref this, ref point);
		}

		/// <summary>
		/// Determines whether the current objects contains a triangle.
		/// </summary>
		/// <param name="vertex1">The first vertex of the triangle to test.</param>
		/// <param name="vertex2">The second vertex of the triagnle to test.</param>
		/// <param name="vertex3">The third vertex of the triangle to test.</param>
		/// <returns>The type of containment the two objects have.</returns>
		public ContainmentType Contains(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
		{
			return Collision.BoxContainsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
		}

		/// <summary>
		/// Determines whether the current objects contains a <see cref="SlimMath.BoundingBox"/>.
		/// </summary>
		/// <param name="box">The box to test.</param>
		/// <returns>The type of containment the two objects have.</returns>
		public ContainmentType Contains(ref BoundingBox box)
		{
			return Collision.BoxContainsBox(ref this, ref box);
		}

		/// <summary>
		/// Determines whether the current objects contains a <see cref="SlimMath.BoundingSphere"/>.
		/// </summary>
		/// <param name="sphere">The sphere to test.</param>
		/// <returns>The type of containment the two objects have.</returns>
		public ContainmentType Contains(ref BoundingSphere sphere)
		{
			return Collision.BoxContainsSphere(ref this, ref sphere);
		}

		/// <summary>
		/// Generates a supporting point for this instance.
		/// </summary>
		/// <param name="direction">The direction for which to build the supporting point.</param>
		/// <param name="result">When the method completes, contains the supporting point.</param>
		public void SupportMapping(ref Vector3 direction, out Vector3 result)
		{
			Collision.SupportPoint(ref this, ref direction, out result);
		}

		/// <summary>
		/// Generates a support mapping for this instance.
		/// </summary>
		/// <param name="direction">The direction for which to build the support mapping.</param>
		/// <returns>The resulting support mapping.</returns>
		public Vector3 SupportMapping(Vector3 direction)
		{
			Vector3 result;
			SupportMapping(ref direction, out result);
			return result;
		}

		/// <summary>
		/// Constructs a <see cref="SlimMath.BoundingBox"/> that fully contains the given points.
		/// </summary>
		/// <param name="points">The points that will be contained by the box.</param>
		/// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
		public static void FromPoints(IList<Vector3> points, out BoundingBox result)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			Vector3 min = new Vector3(float.MaxValue);
			Vector3 max = new Vector3(float.MinValue);

			for (int i = 0; i < points.Count; ++i)
			{
				Vector3 point = points[i];
				Vector3.Min(ref min, ref point, out min);
				Vector3.Max(ref max, ref point, out max);
			}

			result = new BoundingBox(min, max);
		}

		/// <summary>
		/// Constructs a <see cref="SlimMath.BoundingBox"/> that fully contains the given points.
		/// </summary>
		/// <param name="points">The points that will be contained by the box.</param>
		/// <returns>The newly constructed bounding box.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
		public static BoundingBox FromPoints(IList<Vector3> points)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			Vector3 min = new Vector3(float.MaxValue);
			Vector3 max = new Vector3(float.MinValue);

			for (int i = 0; i < points.Count; ++i)
			{
				Vector3 point = points[i];
				Vector3.Min(ref min, ref point, out min);
				Vector3.Max(ref max, ref point, out max);
			}

			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Constructs a <see cref="SlimMath.BoundingBox"/> from a given sphere.
		/// </summary>
		/// <param name="sphere">The sphere that will designate the extents of the box.</param>
		/// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
		public static void FromSphere(ref BoundingSphere sphere, out BoundingBox result)
		{
			result.Minimum = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
			result.Maximum = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
		}

		/// <summary>
		/// Constructs a <see cref="SlimMath.BoundingBox"/> from a given sphere.
		/// </summary>
		/// <param name="sphere">The sphere that will designate the extents of the box.</param>
		/// <returns>The newly constructed bounding box.</returns>
		public static BoundingBox FromSphere(BoundingSphere sphere)
		{
			BoundingBox box;
			box.Minimum = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
			box.Maximum = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
			return box;
		}

		/// <summary>
		/// Constructs a <see cref="SlimMath.BoundingBox"/> that is as large as the total combined area of the two specified boxes.
		/// </summary>
		/// <param name="value1">The first box to merge.</param>
		/// <param name="value2">The second box to merge.</param>
		/// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
		public static void Merge(ref BoundingBox value1, ref BoundingBox value2, out BoundingBox result)
		{
			Vector3.Min(ref value1.Minimum, ref value2.Minimum, out result.Minimum);
			Vector3.Max(ref value1.Maximum, ref value2.Maximum, out result.Maximum);
		}

		/// <summary>
		/// Constructs a <see cref="SlimMath.BoundingBox"/> that is as large as the total combined area of the two specified boxes.
		/// </summary>
		/// <param name="value1">The first box to merge.</param>
		/// <param name="value2">The second box to merge.</param>
		/// <returns>The newly constructed bounding box.</returns>
		public static BoundingBox Merge(BoundingBox value1, BoundingBox value2)
		{
			BoundingBox box;
			Vector3.Min(ref value1.Minimum, ref value2.Minimum, out box.Minimum);
			Vector3.Max(ref value1.Maximum, ref value2.Maximum, out box.Maximum);
			return box;
		}

		/// <summary>
		/// Tests for equality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator ==(BoundingBox left, BoundingBox right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Tests for inequality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator !=(BoundingBox left, BoundingBox right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString());
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format)
		{
			if (format == null)
				return ToString();

			return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(format, CultureInfo.CurrentCulture),
				Maximum.ToString(format, CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(IFormatProvider formatProvider)
		{
			return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString());
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (format == null)
				return ToString(formatProvider);

			return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", Minimum.ToString(format, formatProvider),
				Maximum.ToString(format, formatProvider));
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Minimum.GetHashCode() + Maximum.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="SlimMath.Vector4"/> is equal to this instance.
		/// </summary>
		/// <param name="value">The <see cref="SlimMath.Vector4"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="SlimMath.Vector4"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(BoundingBox value)
		{
			return Minimum == value.Minimum && Maximum == value.Maximum;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType() != GetType())
				return false;

			return Equals((BoundingBox)obj);
		}

#if SlimDX1xInterop
		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.BoundingBox"/> to <see cref="SlimDX.BoundingBox"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator SlimDX.BoundingBox(BoundingBox value)
		{
			return new SlimDX.BoundingBox(value.Minimum, value.Maximum);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimDX.BoundingBox"/> to <see cref="SlimMath.BoundingBox"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator BoundingBox(SlimDX.BoundingBox value)
		{
			return new BoundingBox(value.Minimum, value.Maximum);
		}
#endif

#if XnaInterop
		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.BoundingBox"/> to <see cref="Microsoft.Xna.Framework.BoundingBox"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Microsoft.Xna.Framework.BoundingBox(BoundingBox value)
		{
			return new Microsoft.Xna.Framework.BoundingBox(value.Minimum, value.Maximum);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Microsoft.Xna.Framework.BoundingBox"/> to <see cref="SlimMath.BoundingBox"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator BoundingBox(Microsoft.Xna.Framework.BoundingBox value)
		{
			return new BoundingBox(value.Min, value.Max);
		}
#endif
	}
}
