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
// Created: Thursday, August 30, 2012 5:35:30 AM
// 
#endregion

using System.Drawing;
using Gorgon.IO;
using Gorgon.Math;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A 2 dimensional bounding circle.
	/// </summary>
	/// <remarks>This is used to provide a bounding circle for a sprite, or other renderable that has a collider property.
	/// <para>The <see cref="P:GorgonLibrary.Renderers.Gorgon2DBoundingCircle.Center">Center</see> and <see cref="P:GorgonLibrary.Renderers.Gorgon2DBoundingCircle.Radius">Radius</see> properties are 
	/// used to make adjustments to the bounding circle, and are in the same coordinate space as the renderable (i.e. the coordinate 0, 0 is the upper-left of the renderable).</para>
	/// <para>To get screen space coordinates for the center and radius use the <see cref="P:GorgonLibrary.Renderers.Gorgon2DBoundingCircle.ColliderCenter">ColliderCenter</see> and 
	/// the <see cref="P:GorgonLibrary.Renderers.Gorgon2DBoundingCircle.ColliderRadius">ColliderRadius</see> properties.</para>
	/// <para>A <see cref="P:GorgonLibrary.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see> is not required to use this, and in such a case, the ColliderRadius and ColliderCenter properties will 
	/// be the same as the Center and Radius properties (i.e. in screen space).</para>
	/// </remarks>
	public class Gorgon2DBoundingCircle
		: Gorgon2DCollider
	{
		#region Variables.
		private Vector2 _center = Vector2.Zero;
		private float _radius = 1.0f;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the collider radius.
		/// </summary>
		/// <remarks>This returns the actual radius with the <see cref="Gorgon.Renderers.Gorgon2DBoundingCircle.Radius">Radius</see> applied to the <see cref="Gorgon.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see> bounds.</remarks>
		public float ColliderRadius
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the collider radius.
		/// </summary>
		/// <remarks>This returns the actual center with the <see cref="Gorgon.Renderers.Gorgon2DBoundingCircle.Center">Center</see> applied to the <see cref="Gorgon.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see> bounds.</remarks>
		public Vector2 ColliderCenter
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the center for the bounding circle.
		/// </summary>
		/// <remarks>Use this to adjust the center for the bounding circle, use <see cref="Gorgon.Renderers.Gorgon2DBoundingCircle.ColliderCenter">ColliderCenter</see> to get the actual center.  
		/// If this object is not attached to a <see cref="Gorgon.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see>, then the ColliderCenter will be the same as the Center.</remarks>
		public Vector2 Center
		{
			get
			{
				return _center;
			}
			set
			{
				if (_center == value)
				{
					return;
				}

				_center = value;

				OnPropertyUpdated();
			}
		}

		/// <summary>
		/// Property to set or return the radius for the bounding circle.
		/// </summary>
		/// <remarks>Use this to adjust the radius for the bounding circle, use <see cref="Gorgon.Renderers.Gorgon2DBoundingCircle.ColliderRadius">ColliderRadius</see> to get the actual radius.  
		/// If this object is not attached to a <see cref="Gorgon.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see>, then the ColliderRadius will be the same as the Radius.</remarks>
		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				if (_radius.EqualsEpsilon(value))
				{
					return;
				}

				_radius = value;

				OnPropertyUpdated();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when the center or radius properties are updated.
		/// </summary>
		private void OnPropertyUpdated()
		{
			if (CollisionObject == null)
			{
				ColliderBoundaries = new RectangleF(_center.X, _center.Y, _radius * 2.0f, _radius * 2.0f);
				ColliderRadius = _radius;
				ColliderCenter = _center;

				return;
			}

			UpdateFromCollisionObject();
		}

		/// <summary>
        /// Function to write the collider information into a chunk.
        /// </summary>
        /// <param name="writer">The writer for the chunk.</param>
        /// <remarks>
        /// This method must be implemented to write out collider information to a stream (e.g. saving a sprite with collider information).
        /// <para>The format is as follows:  Write the full type name of the collider, then any relevant information pertaining the collider (e.g. location, width, height, etc...).</para>
        /// <para>This method assumes the chunk writer has already started the collider chunk.</para>
        /// </remarks>
        protected internal override void WriteToChunk(GorgonChunkWriter writer)
        {
            writer.WriteString(GetType().FullName);
            writer.Write(Center);
            writer.WriteFloat(Radius);
        }

        /// <summary>
        /// Function to read in the information about a collider from a chunk.
        /// </summary>
        /// <param name="reader">The reader for the chunk.</param>
        /// <remarks>
        /// This method must be implemented to read in collider information to a stream (e.g. reading a sprite with collider information).
        /// <para>Unlike the <see cref="Gorgon.Renderers.Gorgon2DCollider.WriteToChunk">WriteToChunk</see> method, the reader only needs to read in any custom information
        /// about the collider (e.g. location, width, height, etc...).</para>
        /// <para>This method assumes the chunk writer has already positioned at the collider chunk.</para>
        /// </remarks>
        protected internal override void ReadFromChunk(GorgonChunkReader reader)
        {
            _center = reader.Read<Vector2>();
            _radius = reader.ReadFloat();

            OnPropertyUpdated();
        }

		/// <summary>
		/// Function to update the collider on the object to match the collision object transformation.
		/// </summary>
		/// <remarks>This function must be called to update the collider object boundaries from the collision object after transformation.</remarks>
		protected internal override void UpdateFromCollisionObject()
		{
			if ((CollisionObject == null) || (!Enabled))
			{
				return;
			}

			if ((CollisionObject.Vertices == null) || (CollisionObject.Vertices.Length == 0) || (CollisionObject.VertexCount == 0))
			{
				ColliderBoundaries = new RectangleF(_center.X, _center.Y, _radius * 2.0f, _radius * 2.0f);
				return;
			}

			// Define an infinite boundary.
			var min = new Vector2(float.MaxValue, float.MaxValue);
			var max = new Vector2(float.MinValue, float.MinValue);

			// Determine the minimum and maximum extents.
			for (int i = 0; i < CollisionObject.VertexCount; i++)
			{
				Vector4 position = CollisionObject.Vertices[i].Position;

				min.X = min.X.Min(position.X);
				min.Y = min.Y.Min(position.Y);
				max.X = max.X.Max(position.X);
				max.Y = max.Y.Max(position.Y);
			}

			var size = new Vector2((max.X - min.X).Abs() / 2.0f, (max.Y - min.Y).Abs() / 2.0f);
			var center = new Vector2(min.X + _center.X + size.X, _center.Y + min.Y + size.Y);
			Vector2.Multiply(ref size, _radius, out size);
			ColliderRadius = size.X.Max(size.Y);
			ColliderCenter = center;

			ColliderBoundaries = new RectangleF(center.X - ColliderRadius, center.Y - ColliderRadius, ColliderRadius * 2.0f, ColliderRadius * 2.0f);
		}
		#endregion
	}
}