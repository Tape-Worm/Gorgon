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
// Created: Wednesday, August 22, 2012 9:10:37 PM
// 
#endregion

using System.Drawing;
using Gorgon.IO;
using Gorgon.Math;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A 2 dimensional axis aligned bounding box collider.
	/// </summary>
	/// <remarks>This is used to provide an axis aligned bounding box for a sprite, or other renderable that has a collider property.
	/// <para>The <see cref="P:GorgonLibrary.Renderers.Gorgon2DAABB.Location">Location</see> and <see cref="P:GorgonLibrary.Renderers.Gorgon2DAABB.Size">Size</see> properties are 
	/// used to make adjustments to the bounding box, and are in the same coordinate space as the renderable (i.e. the coordinate 0, 0 is the upper-left of the renderable).</para>
	/// <para>To get screen space coordinates for the location and size use the <see cref="P:GorgonLibrary.Renderers.Gorgon2DCollider.ColliderBoundaries">ColliderBoundaries</see> property.</para>
	/// <para>A <see cref="P:GorgonLibrary.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see> is not required to use this, and in such a case, the ColliderBoundaries property will 
	/// be the same as the Location and Size properties (i.e. in screen space).</para>
	/// </remarks>
	// ReSharper disable once InconsistentNaming
	public class Gorgon2DAABB
		: Gorgon2DCollider
	{
		#region Variables.
		private Vector2 _location = Vector2.Zero;           // Location.
		private Vector2 _size = new Vector2(1);             // Size of the AABB.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the upper-left coordinates for the bounding box.
		/// </summary>
		/// <remarks>Use this to adjust the location for the AABB, use <see cref="Gorgon.Renderers.Gorgon2DCollider.ColliderBoundaries">ColliderBoundaries</see> to get the actual location.  
		/// If this object is not attached to a <see cref="Gorgon.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see>, then the ColliderBoundaries location will be the same as the Location.</remarks>
		public Vector2 Location
		{
			get
			{
				return _location;
			}
			set
			{
				if (_location == value)
				{
					return;
				}

				_location = value;
				OnPropertyUpdated();
			}
		}

		/// <summary>
		/// Property to set or return the center of the AAAB.
		/// </summary>
		public Vector2 Center
		{
			get
			{
				return new Vector2(_location.X + _size.X / 2.0f, _location.Y + _size.Y / 2.0f);
			}
			set
			{
				Location = new Vector2(value.X - (_size.X / 2.0f), value.Y - (_size.Y / 2.0f));
			}
		}

		/// <summary>
		/// Property to set or return the size for the AABB.
		/// </summary>
		/// <remarks>Use this to adjust the size for the AABB, use <see cref="Gorgon.Renderers.Gorgon2DCollider.ColliderBoundaries">ColliderBoundaries</see> to get the actual size.  
		/// If this object is not attached to a <see cref="Gorgon.Renderers.Gorgon2DCollider.CollisionObject">CollisionObject</see>, then the ColliderBoundaries size will be the same as the Size.</remarks>
		public Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size == value)
				{
					return;
				}

				_size = value;
				OnPropertyUpdated();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when the size or position properties are updated.
		/// </summary>
		private void OnPropertyUpdated()
		{
			if (CollisionObject == null)
				ColliderBoundaries = new RectangleF(_location, _size);
			else
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
            writer.Write(Location);
            writer.Write(Size);
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
            _location = reader.Read<Vector2>();
            _size = reader.Read<Vector2>();

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
				ColliderBoundaries = new RectangleF(_location, _size);
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
			var location = new Vector2(_location.X + min.X + size.X, _location.Y + min.Y + size.Y);

			Vector2.Modulate(ref size, ref _size, out size);
			ColliderBoundaries = new RectangleF(location.X - size.X, location.Y - size.Y, size.X * 2.0f, size.Y * 2.0f);
		}
		#endregion
	}
}