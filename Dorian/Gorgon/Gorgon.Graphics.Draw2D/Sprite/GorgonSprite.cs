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
// Created: Thursday, February 16, 2012 9:19:30 AM
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
	/// A sprite object.
	/// </summary>
	public class GorgonSprite
		: GorgonNamedObject
	{
		#region Variables.
		private Gorgon2D.Vertex[] _vertices = new Gorgon2D.Vertex[4];					// Vertices for the sprite.
		private float _width = 0.0f;													// Width of the sprite
		private float _height = 0.0f;													// Height of the sprite.
		private Matrix _worldMatrix = Matrix.Identity;									// World matrix for the sprite.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the interface that owns this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the width of the sprite.
		/// </summary>
		public float Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		/// <summary>
		/// Property to set or return the height of the sprite.
		/// </summary>
		public float Height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		/// <summary>
		/// Property to set or return the texture for the sprite.
		/// </summary>
		public GorgonTexture Texture
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the left position of the sprite.
		/// </summary>
		public float Left
		{
			get
			{
				return _worldMatrix.M41;
			}
			set
			{
				Matrix.Translation(value, _worldMatrix.M42, _worldMatrix.M43, out _worldMatrix);
			}
		}

		/// <summary>
		/// Property to set or return the top position of the sprite.
		/// </summary>
		public float Top
		{
			get
			{
				return _worldMatrix.M42;
			}
			set
			{
				Matrix.Translation(_worldMatrix.M41, value, _worldMatrix.M43, out _worldMatrix);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the vertices for the sprite.
		/// </summary>
		private void CreateVertices()
		{
			_vertices[0] = new Gorgon2D.Vertex(new Vector4(0, 0, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), Vector2.Zero);
			_vertices[1] = new Gorgon2D.Vertex(new Vector4(Width, 0, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), Vector2.UnitX);
			_vertices[2] = new Gorgon2D.Vertex(new Vector4(0, Height, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), Vector2.UnitY);
			_vertices[3] = new Gorgon2D.Vertex(new Vector4(Width, Height, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector2(1));

			for (int i = 0; i < _vertices.Length; i++)
				_vertices[i].Position = Vector4.Transform(_vertices[i].Position, _worldMatrix);
		}

		/// <summary>
		/// Function to draw the sprite.
		/// </summary>
		public void Draw()
		{
			CreateVertices();
			if (Gorgon2D.Shaders.Current.Textures[0] != Texture)
			{
				if (Texture != null)
				{
					Gorgon2D.Shaders.Current = null;
					Gorgon2D.Shaders.Current.Textures[0] = Texture;
					Gorgon2D.Shaders.Current.Samplers[0] = GorgonTextureSamplerStates.DefaultStates;					
				}
				else
					Gorgon2D.Shaders.Current.Textures[0] = null;

				Gorgon2D.Shaders.UpdateShaders();
			}

			Gorgon2D.AddVertices(_vertices);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSprite"/> class.
		/// </summary>
		/// <param name="gorgon2D">The interface that owns this object.</param>
		/// <param name="name">The name of the sprite.</param>
		/// <param name="width">The width of the sprite.</param>
		/// <param name="height">The height of the sprite.</param>
		internal GorgonSprite(Gorgon2D gorgon2D, string name, float width, float height)
			: base(name)
		{
			Gorgon2D = gorgon2D;
			Width = width;
			Height = height;
		}
		#endregion
	}
}
