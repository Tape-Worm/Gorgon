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
// Created: Friday, February 17, 2012 6:11:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A renderable object.
	/// </summary>
	public abstract class GorgonRenderable
		: GorgonNamedObject, ICloneable<GorgonRenderable>
	{
		#region Variables.
		private string _textureName = string.Empty;								// Name of the texture for deferred loading.
		private GorgonTexture2D _texture = null;								// Texture to use for the renderable.
		private Vector2 _textureOffset = Vector2.Zero;							// Texture offset.
		private Vector2 _textureScale = new Vector2(1);							// Texture scale.
		private Vector2 _size = Vector2.Zero;									// Size of the renderable.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the texture information needs updating.
		/// </summary>
		protected bool NeedsTextureUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the renderable needs to adjust its dimensions.
		/// </summary>
		protected bool NeedsVertexUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return a list of transformed vertices.
		/// </summary>
		protected internal Gorgon2D.Vertex[] TransformedVertices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		protected Gorgon2D.Vertex[] Vertices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return an index buffer.
		/// </summary>
		protected internal abstract bool UseIndexBuffer
		{
			get;
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		/// <remarks>This will only return the color for the first vertex of the renderable and consequently will set all the vertices to the same color.</remarks>
		public virtual GorgonColor Color
		{
			get
			{
				return Vertices[0].Color;
			}
			set
			{
				GorgonColor current = Vertices[0].Color;

				if (value != current)
				{
					for (int i = 0; i < Vertices.Length; i++)
						Vertices[i].Color = value;
				}
			}
		}

		/// <summary>
		/// Property to return the name of the texture bound to this image.
		/// </summary>
		/// <remarks>This is used to defer the texture assignment until it is loaded.</remarks>
		public string DeferredTextureName
		{
			get
			{
				return _textureName;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				if (string.Compare(_textureName, value, true) != 0)
				{
					_textureName = value;
					GetDeferredTexture();
				}
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		public virtual GorgonTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (_texture != value)
				{
					_texture = value;

					// Assign the texture name.
					if (_texture != null)
						_textureName = _texture.Name;
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
		/// Property to return the Gorgon 2D interface that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
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
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected virtual void InitializeCustomVertexInformation()
		{
		}

		/// <summary>
		/// Function to initialize the vertex 
		/// </summary>
		/// <param name="vertexCount">Number of vertices in this renderable object.</param>
		protected void InitializeVertices(int vertexCount)
		{
			Vertices = new Gorgon2D.Vertex[vertexCount];
			TransformedVertices = new Gorgon2D.Vertex[vertexCount];

			for (int i = 0; i < Vertices.Length; i++)
				Vertices[i].Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);

			UpdateTextureCoordinates();
			UpdateVertices();
			InitializeCustomVertexInformation();

			NeedsVertexUpdate = false;
			NeedsTextureUpdate = false;
		}		

		/// <summary>
		/// Function to assign a deferred texture.
		/// </summary>
		/// <remarks>If there are multiple textures with the same name, then the first texture will be chosen.</remarks>
		public virtual void GetDeferredTexture()
		{
			if (string.IsNullOrEmpty(_textureName))
			{
				_texture = null;
				return;
			}

			// Look through the tracked objects in the graphics object.
			// FYI, LINQ is fucking awesome (if a little slow)...
			_texture = (from texture in Gorgon2D.Graphics.GetGraphicsObjectOfType<GorgonTexture2D>()
						where (texture != null) && (string.Compare(texture.Name, _textureName, true) == 0)
						select texture).FirstOrDefault();

			NeedsTextureUpdate = true;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be 
		/// drawn when <see cref="M:GorgonLibrary.Graphics.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public virtual void Draw()
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

			Gorgon2D.AddRenderable(this);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderable"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon 2D interface.</param>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="gorgon2D"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonRenderable(Gorgon2D gorgon2D, string name)
			: base(name)
		{
			GorgonDebug.AssertNull<Gorgon2D>(gorgon2D, "gorgon2D");
						
			Gorgon2D = gorgon2D;
		}
		#endregion

		#region ICloneable<GorgonRenderable> Members
		/// <summary>
		/// Function to clone this renderable object.
		/// </summary>
		/// <returns>The clone of the renderable object.</returns>
		public abstract GorgonRenderable Clone();
		#endregion
	}
}
