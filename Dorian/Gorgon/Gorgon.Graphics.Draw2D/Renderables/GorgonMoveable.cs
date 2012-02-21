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
using SlimMath;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// Defines a moveable and renderable object.
	/// </summary>
	public abstract class GorgonMoveable
		: GorgonRenderable
	{
		#region Variables.
		private string _textureName = string.Empty;															// Name of the texture for deferred loading.
		private Vector2 _textureOffset = Vector2.Zero;														// Texture offset.
		private Vector2 _textureScale = new Vector2(1);														// Texture scale.
		private Vector2 _size = Vector2.Zero;																// Size of the renderable.
		private Vector3 _angle = Vector3.Zero;																// Angle of rotation.
		private Vector3 _position = Vector3.Zero;															// Position of the sprite.
		private Vector2 _scale = new Vector2(1);															// Scale for the sprite.
		private Vector3 _anchor = Vector3.Zero;																// Anchor point.
		#endregion

		#region Properties.	
		/// <summary>
		/// Property to set or return whether the renderable needs translation.
		/// </summary>
		protected bool NeedsTranslate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether rotation needs updating or not.
		/// </summary>
		protected bool NeedsRotation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether scaling needs updating or not.
		/// </summary>
		protected bool NeedsScaling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the sprite.
		/// </summary>
		public Vector3 Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (_position != value)
				{
					_position = value;
					NeedsTranslate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a given axis.
		/// </summary>
		public Vector3 Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				if (_angle != value)
				{
					_angle = value;
					NeedsRotation = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				if (_scale != value)
				{
					_scale = value;
					NeedsScaling = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the sprite.
		/// </summary>
		public Vector3 Anchor
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
					NeedsTranslate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		public override GorgonTexture2D Texture
		{
			get
			{
				return base.Texture;
			}
			set
			{
				if (value != base.Texture)
				{
					base.Texture = value;

					// Assign the texture name.
					if (Texture != null)
						_textureName = Texture.Name;
					else
						_textureName = string.Empty;


					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		/// <remarks>You can use this property to scroll the texture in the sprite.</remarks>
		public virtual Vector2 TextureOffset
		{
			get
			{
				return _textureOffset;
			}
			set
			{
				if (_textureOffset != value)
				{
					_textureOffset = value;
					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the scaling of the texture width and height.
		/// </summary>
		public virtual Vector2 TextureScale
		{
			get
			{
				return _textureScale;
			}
			set
			{
				if (_textureScale != value)
				{
					// Lock the size.
					if (_textureScale.X < 0)
						_textureScale.X = 1e-6f;

					if (_textureScale.Y < 0)
						_textureScale.Y = 1e-6f;

					_textureScale = value;
					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
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
			UpdateTextureCoordinates();
			UpdateVertices();

			NeedsVertexUpdate = false;
			NeedsTextureUpdate = false;
			NeedsRotation = true;
			NeedsScaling = true;
			NeedsTranslate = true;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			if (NeedsTextureUpdate)
			{
				UpdateTextureCoordinates();
				NeedsTextureUpdate = false;
			}

			if (NeedsVertexUpdate)
			{
				UpdateVertices();
				NeedsVertexUpdate = false;
			}

			if ((NeedsTranslate) || (NeedsRotation) || (NeedsScaling))
			{
				TransformVertices();
				NeedsTranslate = false;
				NeedsRotation = false;
				NeedsScaling = false;
			}

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
		}
		#endregion
	}
}
