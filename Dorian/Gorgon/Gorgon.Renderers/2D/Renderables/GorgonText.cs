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
// Created: Friday, April 20, 2012 8:12:15 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A renderable object that renders a block of text.
	/// </summary>
	public class GorgonText
		: GorgonNamedObject, IRenderable
	{
		#region Variables.
		private int _vertexCount = 0;													// Number of vertices.
		private Gorgon2DVertex[] _vertices = null;										// Vertices.
		private GorgonFont _font = null;												// Font to apply to the text.
		private StringBuilder _text = null;												// Original text.
		private bool _isTextUpdated = false;											// Flag to indicate that the text was updated.
		private IDictionary<char, GorgonGlyph> _groupedGlyphs = null;					// A list of glyphs sorted by texture.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the 2D interface that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the text to display.
		/// </summary>
		public string Text
		{
			get
			{
				return _text.ToString();
			}
			set
			{
				if (value == null)
					value = string.Empty;

				if (string.Compare(value, _text.ToString(), false) != 0)
				{
					_text.Length = 0;
					if (value.Length > 0)
						_text.Append(value);
					UpdateText();
				}
			}
		}

		/// <summary>
		/// Property to return the texture being used by the renderable.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		public override GorgonTexture2D Texture
		{
			get
			{
				return null;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return the font to be applied to the text.
		/// </summary>
		/// <remarks>This property will ignore requests to set it to NULL (Nothing in VB.Net).</remarks>
		public GorgonFont Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == null)
					return;

				if (value != _font)
				{
					_font = value;
					UpdateGlyphSorting();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the glyph sorting.
		/// </summary>
		private void UpdateGlyphSorting()
		{
			if (_font.Textures.Count > 1)
			{
				// Sort all the glyphs for the font.
				var groupedGlyphs = (from glyph in _font.Glyphs
									 group glyph by glyph.Texture into groupedGlyph
									 select groupedGlyph);

				_groupedGlyphs = new Dictionary<char, GorgonGlyph>();
				foreach (var glyphGroup in groupedGlyphs)
				{
					foreach (var glyph in glyphGroup)
						_groupedGlyphs.Add(glyph.Character, glyph);
				}
			}
			else
				_groupedGlyphs = _font.Glyphs.ToDictionary(item => item.Character);
		}

		/// <summary>
		/// Function to perform an update on the text object.
		/// </summary>
		private void UpdateText()
		{
			int vertexCount = 0;

			if (_text.Length == 0)
			{
				_vertexCount = 0;
				_vertices = new Gorgon2DVertex[1024];
				return;
			}

			// Allocate the number of vertices, if the vertex count is less than the required amount.
			vertexCount = _text.Length * 4;

			if (vertexCount > _vertexCount)
			{
				_vertices = new Gorgon2DVertex[vertexCount];
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = Color;
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}
			}

			// Update the vertex count.
			_vertexCount = vertexCount;						
		}

		/// <summary>
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected override void InitializeCustomVertexInformation()
		{
			
		}		

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="M:GorgonLibrary.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public override void Draw()
		{
			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonText"/> class.
		/// </summary>
		/// <param name="gorgon2D">2D interface that created this object.</param>
		/// <param name="name">Name of the text object.</param>
		/// <param name="text">The text to display.</param>
		/// <param name="font">The font to use.</param>
		public GorgonText(Gorgon2D gorgon2D, string name, GorgonFont font)
			: base(name)
		{
			_text = new StringBuilder(256);
			_font = font;
			UpdateText();
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		/// <value></value>
		PrimitiveType IRenderable.PrimitiveType
		{
			get
			{
				return PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		/// <value></value>
		/// <remarks>This is only matters when the renderable uses an index buffer.</remarks>
		int IRenderable.IndexCount
		{
			get
			{
				return 6;
			}
		}

		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		/// <value></value>
		GorgonVertexBufferBinding IRenderable.VertexBufferBinding
		{
			get 
			{
				return Gorgon2D.DefaultVertexBufferBinding;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		/// <value></value>
		GorgonIndexBuffer IRenderable.IndexBuffer
		{
			get 
			{
				return Gorgon2D.DefaultIndexBuffer;
			}
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		/// <value></value>
		Gorgon2DVertex[] IRenderable.Vertices
		{
			get 
			{
				return _vertices;
			}
		}

		/// <summary>
		/// Property to return the number of vertices to add to the base starting index.
		/// </summary>
		/// <value></value>
		int IRenderable.BaseVertexCount
		{
			get 
			{
				return 0;
			}
		}

		public GorgonRenderable.DepthStencilStates DepthStencil
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public GorgonRenderable.BlendState Blending
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public GorgonRenderable.TextureSamplerState TextureSampler
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public SmoothingMode SmoothingMode
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public BlendingMode BlendingMode
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public CullingMode CullingMode
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public GorgonMinMaxF AlphaTestValues
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public float Opacity
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public GorgonColor Color
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}
}
