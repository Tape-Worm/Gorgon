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
using System.Drawing;
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
		private RectangleF? _clipRect = null;											// Clipping rectangle.
		private int _vertexCount = 0;													// Number of vertices.
		private Gorgon2DVertex[] _vertices = null;										// Vertices.
		private GorgonFont _font = null;												// Font to apply to the text.
		private StringBuilder _text = null;												// Original text.
		private GorgonColor[] _colors = null;											// Vertex colors.
		private GorgonRenderable.DepthStencilStates _depthStencil = null;				// Depth/stencil states.
		private GorgonRenderable.BlendState _blend = null;								// Blending states.
		private GorgonRenderable.TextureSamplerState _sampler = null;					// Texture sampler states.
		private GorgonTexture2D _currentTexture = null;									// Currently active texture.
		private bool _needsVertexUpdate = true;											// Flag to indicate that we need a vertex update.
		private bool _needsColorUpdate = true;											// Flag to indicate that the color needs updating.
		private StringBuilder _formattedText = null;									// Formatted text.
		private List<string> _lines = null;												// Lines in the text.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the real clipping bounds.
		/// </summary>
		private RectangleF ClipBounds
		{
			get
			{
				if (_clipRect == null)
					return Gorgon2D.Target.Viewport.Region;
				else
					return _clipRect.Value;
			}
		}

		/// <summary>
		/// Property to set or return the clipping rectangle for the text.
		/// </summary>
		public RectangleF? ClippingRectangle
		{
			get
			{
				return _clipRect;
			}
			set
			{
				if (_clipRect != value)
				{
					_clipRect = value;
					_needsVertexUpdate = true;
					FormatText();
				}
			}
		}

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
					UpdateText();
					_needsVertexUpdate = true;
				}
			}
		}
		#endregion

		#region Methods.
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
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}

				_currentTexture = null;
				return;
			}

			// Allocate the number of vertices, if the vertex count is less than the required amount.
			vertexCount = _text.Length * 4;
			// TODO: When shadowing, make this length * 8.

			if (vertexCount > _vertexCount)
			{
				_vertices = new Gorgon2DVertex[vertexCount];
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}
			}

			// Update the vertex count.
			_vertexCount = vertexCount;

			// Get the first texture.
			for (int i = 0; i < _text.Length; i++)
			{
				if (_font.Glyphs.Contains(_text[i]))
				{
					_currentTexture = _font.Glyphs[_text[i]].Texture;
					break;
				}
			}

			_needsVertexUpdate = true;
			FormatText();
		}

		/// <summary>
		/// Function to perform a vertex update for the text.
		/// </summary>
		private void UpdateVertices()
		{
			int vertexIndex = 0;
			Vector2 pos = Vector2.Zero;
			Vector2 outlineOffset = Vector2.Zero;
			GorgonGlyph glyph = null;

			if ((_font.Settings.OutlineColor.Alpha > 0) && (_font.Settings.OutlineSize > 0))
				outlineOffset = new Vector2(_font.Settings.OutlineSize, _font.Settings.OutlineSize);

			for (int i = 0; i < _formattedText.Length; i++)
			{
				char c = _text[i];
			
				switch (c)
				{
					case ' ':
						glyph = _font.Glyphs[_text[i]];
						pos.X += glyph.GlyphCoordinates.Width - 1;
						continue;
					case '\t':
						// TODO:  Allow variable tab length.
						glyph = _font.Glyphs[_text[i]];
						pos.X += (glyph.GlyphCoordinates.Width - 1) * 3;
						continue;
					case '\n':
						pos.Y += _font.FontHeight + outlineOffset.Y;
						continue;
					case '\r':						
						// TODO: Reset to our anchor X position.
						pos.X = 0;
						continue;
				}

				if (_font.Glyphs.Contains(c))
					glyph = _font.Glyphs[c];
				else
					glyph = _font.Glyphs[_font.Settings.DefaultCharacter];

				_vertices[vertexIndex].Position = new Vector4(pos.X + glyph.Offset.X, pos.Y + glyph.Offset.Y, 0, 1.0f);
				_vertices[vertexIndex].Color = _colors[0];
				_vertices[vertexIndex].UV = glyph.TextureCoordinates.Location;

				_vertices[vertexIndex + 1].Position = new Vector4(pos.X + glyph.GlyphCoordinates.Width + glyph.Offset.X, pos.Y + glyph.Offset.Y, 0, 1.0f);
				_vertices[vertexIndex + 1].Color = _colors[1];
				_vertices[vertexIndex + 1].UV = new Vector2(glyph.TextureCoordinates.Right, glyph.TextureCoordinates.Top);

				_vertices[vertexIndex + 2].Position = new Vector4(pos.X + glyph.Offset.X, pos.Y + glyph.GlyphCoordinates.Height + glyph.Offset.Y, 0, 1.0f);
				_vertices[vertexIndex + 2].Color = _colors[2];
				_vertices[vertexIndex + 2].UV = new Vector2(glyph.TextureCoordinates.Left, glyph.TextureCoordinates.Bottom);

				_vertices[vertexIndex + 3].Position = new Vector4(pos.X + glyph.GlyphCoordinates.Width + glyph.Offset.X, pos.Y + glyph.GlyphCoordinates.Height + glyph.Offset.Y, 0, 1.0f);
				_vertices[vertexIndex + 3].Color = _colors[3];
				_vertices[vertexIndex + 3].UV = new Vector2(glyph.TextureCoordinates.Right, glyph.TextureCoordinates.Bottom);

				vertexIndex += 4;

				pos.X += glyph.Advance.X + glyph.Advance.Y + outlineOffset.X;

				if (i < _text.Length - 1)
				{
					GorgonKerningPair kerning = new GorgonKerningPair(_text[i], _text[i + 1]);
					if (_font.KerningPairs.ContainsKey(kerning))
						pos.X += _font.KerningPairs[kerning];
					else
						pos.X += glyph.Advance.Z;					
				}
			}
		}

		/// <summary>
		/// Function to update the colors.
		/// </summary>
		private void UpdateColors()
		{
			for (int i = 0; i < _vertices.Length; i+=4)
			{
				_vertices[i].Color = _colors[0];
				_vertices[i + 1].Color = _colors[1];
				_vertices[i + 2].Color = _colors[2];
				_vertices[i + 3].Color = _colors[3];
			}
		}

		/// <summary>
		/// Function to format the text for measuring and clipping.
		/// </summary>
		private void FormatText()
		{
			_formattedText.Length = 0;
			// TODO: Do word wrapping.
			_formattedText.Append(_text);
			_formattedText.Replace("\n\r", "\r\n");
			_lines.Clear();
			_lines.AddRange(_formattedText.ToString().Split('\n'));
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="M:GorgonLibrary.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public void Draw()
		{
			StateChange states = StateChange.None;
			int vertexIndex = 0;

			// We don't need to draw anything.
			if (_text.Length == 0)
				return;

			states = Gorgon2D.StateManager.CheckState(this);
			if (states != StateChange.None)
			{
				Gorgon2D.RenderObjects();
				Gorgon2D.StateManager.ApplyState(this, states);
			}

			if (_needsVertexUpdate)
			{
				UpdateVertices();
				_needsVertexUpdate = false;
			}

			if (_needsColorUpdate)
			{
				UpdateColors();
				_needsColorUpdate = false;
			}

			for (int i = 0; i < _formattedText.Length; i++)
			{
				char c = _text[i];
				if ((c != '\r') && (c != '\n') && (c != '\t') && (c != ' '))
				{
					GorgonGlyph glyph = null;

					if (_font.Glyphs.Contains(c))
						glyph = _font.Glyphs[c];
					else
						glyph = _font.Glyphs[_font.Settings.DefaultCharacter];
						
					// Change to the current texture.
					if (Gorgon2D.PixelShader.Textures[0] != glyph.Texture)
					{
						Gorgon2D.RenderObjects();
						Gorgon2D.PixelShader.Textures[0] = glyph.Texture;
					}

					Gorgon2D.AddVertices(_vertices, 0, 6, vertexIndex, 4);
					vertexIndex += 4;
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonText"/> class.
		/// </summary>
		/// <param name="gorgon2D">2D interface that created this object.</param>
		/// <param name="name">Name of the text object.</param>
		/// <param name="font">The font to use.</param>
		public GorgonText(Gorgon2D gorgon2D, string name, GorgonFont font)
			: base(name)
		{
			Gorgon2D = gorgon2D;

			_text = new StringBuilder(256);
			_formattedText = new StringBuilder(256);
			_lines = new List<string>();
			_font = font;
			_colors = new GorgonColor[4];

			_depthStencil = new GorgonRenderable.DepthStencilStates();
			_blend = new GorgonRenderable.BlendState();
			_sampler = new GorgonRenderable.TextureSamplerState();
			Color = new GorgonColor(1, 1, 1, 1);
			CullingMode = Graphics.CullingMode.Back;
			AlphaTestValues = GorgonMinMaxF.Empty;
			BlendingMode = BlendingMode.Modulate;

			UpdateText();
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int IRenderable.VertexCount
		{
			get
			{
				return _vertexCount;
			}
		}

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

		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		public virtual GorgonRenderable.DepthStencilStates DepthStencil
		{
			get
			{
				return _depthStencil;
			}
			set
			{
				if (value == null)
					return;

				_depthStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		public virtual GorgonRenderable.BlendState Blending
		{
			get
			{
				return _blend;
			}
			set
			{
				if (value == null)
					return;

				_blend = value;
			}
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		public virtual GorgonRenderable.TextureSamplerState TextureSampler
		{
			get
			{
				return _sampler;
			}
			set
			{
				if (value == null)
					return;

				_sampler = value;
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined smoothing states, to get more control over the smoothing, use the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.TextureSamplerState.TextureFilter.">TextureFilter</see> 
		/// property exposed by the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.TextureSampler.">TextureSampler</see> property.</remarks>
		public virtual SmoothingMode SmoothingMode
		{
			get
			{
				switch (TextureSampler.TextureFilter)
				{
					case TextureFilter.Point:
						return SmoothingMode.None;
					case TextureFilter.Linear:
						return SmoothingMode.Smooth;
					case TextureFilter.MinLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMinify;
					case TextureFilter.MagLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMagnify;
					default:
						return SmoothingMode.Custom;
				}
			}
			set
			{
				switch (value)
				{
					case SmoothingMode.None:
						TextureSampler.TextureFilter = TextureFilter.Point;
						break;
					case SmoothingMode.Smooth:
						TextureSampler.TextureFilter = TextureFilter.Linear;
						break;
					case SmoothingMode.SmoothMinify:
						TextureSampler.TextureFilter = TextureFilter.MinLinear | TextureFilter.MipLinear;
						break;
					case SmoothingMode.SmoothMagnify:
						TextureSampler.TextureFilter = TextureFilter.MagLinear | TextureFilter.MipLinear;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined blending states, to get more control over the blending, use the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.BlendingState.SourceBlend">SourceBlend</see> 
		/// or the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.Blending.DestinationBlend">DestinationBlend</see> property which are exposed by the 
		/// <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.Blending">Blending</see> property.</remarks>
		public virtual BlendingMode BlendingMode
		{
			get
			{
				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.Zero))
					return Renderers.BlendingMode.None;

				if (Blending.SourceBlend == BlendType.SourceAlpha)
				{
					if (Blending.DestinationBlend == BlendType.InverseSourceAlpha)
						return Renderers.BlendingMode.Modulate;
					if (Blending.DestinationBlend == BlendType.One)
						return Renderers.BlendingMode.Additive;
				}

				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
					return Renderers.BlendingMode.PreMultiplied;

				if ((Blending.SourceBlend == BlendType.InverseDestinationColor) && (Blending.DestinationBlend == BlendType.InverseSourceColor))
					return Renderers.BlendingMode.Inverted;

				return Renderers.BlendingMode.Custom;
			}
			set
			{
				switch (value)
				{
					case Renderers.BlendingMode.Additive:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.One;
						break;
					case Renderers.BlendingMode.Inverted:
						Blending.SourceBlend = BlendType.InverseDestinationColor;
						Blending.DestinationBlend = BlendType.InverseSourceColor;
						break;
					case Renderers.BlendingMode.Modulate:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case Renderers.BlendingMode.PreMultiplied:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case Renderers.BlendingMode.None:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.Zero;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		public CullingMode CullingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the range of alpha values to reject on this renderable.
		/// </summary>
		public GorgonMinMaxF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		public float Opacity
		{
			get
			{
				return Color.Alpha;
			}
			set
			{
				if (value != Color.Alpha)
					Color = new GorgonColor(Color.Red, Color.Green, Color.Blue, value);
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		public GorgonColor Color
		{
			get
			{
				return _colors[0];
			}
			set
			{
				if (value != _colors[0])
				{
					_colors[3] = _colors[2] = _colors[1] = _colors[0] = value;
					_needsColorUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to return the texture being used by the renderable.
		/// </summary>
		GorgonTexture2D IRenderable.Texture
		{
			get
			{
				return _currentTexture;
			}
			set
			{
			}
		}
		#endregion
	}
}
