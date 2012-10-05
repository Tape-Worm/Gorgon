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
// Created: Monday, February 20, 2012 2:14:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Animation;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Defines a moveable and renderable object.
	/// </summary>
	public abstract class GorgonMoveable
		: GorgonRenderable, IMoveable
	{
		#region Variables.
		private Vector2 _size = Vector2.Zero;																// Size of the renderable.
		private Vector2 _anchor = Vector2.Zero;																// Anchor.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the position of the renderable.
		/// </summary>
        [AnimatedProperty()]
		public virtual Vector2 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a renderable.
		/// </summary>
        [AnimatedProperty()]
		public virtual float Angle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
		/// </summary>
		/// <remarks>This property uses scalar values to provide a relative scale.  To set an absolute scale (i.e. pixel coordinates), use the <see cref="P:GorgonLibrary.Renderers.GorgonMoveable.Size">Size</see> property.
		/// <para>Setting this value to a 0 vector will cause undefined behaviour and is not recommended.</para>
		/// </remarks>
        [AnimatedProperty()]
		public virtual Vector2 Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the anchor point of the renderable.
		/// </summary>
        [AnimatedProperty()]
		public virtual Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor != value)
				{
					_anchor = value;
					NeedsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
        [AnimatedProperty()]
		public virtual float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
        [AnimatedProperty()]
		public virtual Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size != value)
				{
					_size = value;					
					NeedsVertexUpdate = true;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected abstract void UpdateTextureCoordinates();

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected abstract void UpdateVertices();

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected abstract void TransformVertices();

		/// <summary>
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected override void InitializeCustomVertexInformation()
		{
			UpdateVertices();
			UpdateTextureCoordinates();

			NeedsVertexUpdate = false;
			NeedsTextureUpdate = false;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			if (NeedsVertexUpdate)
			{
				UpdateVertices();
				NeedsVertexUpdate = false;
			}

			if (NeedsTextureUpdate)
			{
				UpdateTextureCoordinates();
				NeedsTextureUpdate = false;
			}

			TransformVertices();

			base.Draw();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMoveable"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		/// <param name="name">The name of the renderable.</param>
		protected GorgonMoveable(Gorgon2D gorgon2D, string name)
			: base(gorgon2D, name)
		{
			Position = Vector2.Zero;
			Scale = new Vector2(1.0f);
			Angle = 0;
			Anchor = Vector2.Zero;
		}
		#endregion
	}
}
