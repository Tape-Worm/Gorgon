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
		: GorgonNamedObject, IRenderable, IMoveable
	{
		#region Variables.
		private RectangleF? _textRect = null;											// Text rectangle.
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
		private Vector2 _size = Vector2.Zero;											// Size of the text block.
		private Vector2 _position = Vector2.Zero;										// Position of the text.
		private Vector2 _scale = new Vector2(1);										// Scale for the text.
		private float _angle = 0;														// Angle of rotation for the text.
		private Vector2 _anchor = Vector2.Zero;											// Anchor point for the text.
		private float _depth = 0;														// Depth value.
		private int _tabSpace = 3;														// Tab spaces.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the clipping region.
		/// </summary>
		private RectangleF ClipRegion
		{
			get
			{
				if (_textRect == null)
					return Gorgon2D.Target.Viewport.Region;
				else
					return _textRect.Value;
			}
		}

		/// <summary>
		/// Property to set or return the number of spaces to use for a tab character.
		/// </summary>
		/// <remarks>The default value is 3.</remarks>
		public int TabSpaces
		{
			get
			{
				return _tabSpace;
			}
			set
			{
				if (value < 1)
					value = 1;

				_tabSpace = value;
			}
		}

		/// <summary>
		/// Property to set or return the rectangle used for clipping and/or alignment for the text.
		/// </summary>
		/// <remarks>This property will clip the text to the rectangle if the <see cref="GorgonLibrary.Renderers.GorgonText.ClipToRectangle">ClipToRectangle</see> property is set to TRUE.</remarks>
		public RectangleF? TextRectangle
		{
			get
			{
				return _textRect;
			}
			set
			{
				if (_textRect != value)
				{
					_textRect = value;
					_needsVertexUpdate = true;					
					FormatText();
				}
			}
		}

		/// <summary>
		/// Property to set or return whether to clip the text to the <see cref="P:GorgonLibrary.Renderers.GorgonText.TextRectangle">TextRectangle</see>.
		/// </summary>
		public bool ClipToRectangle
		{
			get;
			set;
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
				if (_vertices == null)
					_vertices = new Gorgon2DVertex[1024];
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}

				_currentTexture = null;
				_needsVertexUpdate = true;
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
		/// Function to update the transformation for the text.
		/// </summary>
		/// <param name="glyph">Glyph to transform.</param>
		/// <param name="vertexIndex">Index of the vertex to modify.</param>
		/// <param name="textOffset">Offset of the glyph within the text.</param>
		private void UpdateTransform(GorgonGlyph glyph, int vertexIndex, Vector2 textOffset)
		{
			Vector2 ltCorner = Vector2.Subtract(textOffset, Anchor);
			Vector2 rbCorner = Vector2.Add(ltCorner, glyph.GlyphCoordinates.Size);

			// Scale.
			if (_scale.X != 1.0f)
			{
				ltCorner.X *= Scale.X;
				rbCorner.X *= Scale.X;
			}

			if (_scale.Y != 1.0f)
			{
				ltCorner.Y *= Scale.Y;
				rbCorner.Y *= Scale.Y;
			}

			if (Angle != 0.0f)
			{
				float angle = GorgonMathUtility.Radians(_angle);	// Angle in radians.
				float cosVal = GorgonMathUtility.Cos(angle);		// Cached cosine.
				float sinVal = GorgonMathUtility.Sin(angle);		// Cached sine.

				// Rotate the vertices.
				_vertices[vertexIndex].Position.X = (ltCorner.X * cosVal - ltCorner.Y * sinVal);
				_vertices[vertexIndex].Position.Y = (ltCorner.X * sinVal + ltCorner.Y * cosVal);
				_vertices[vertexIndex + 1].Position.X = (rbCorner.X * cosVal - ltCorner.Y * sinVal);
				_vertices[vertexIndex + 1].Position.Y = (rbCorner.X * sinVal + ltCorner.Y * cosVal);
				_vertices[vertexIndex + 2].Position.X = (ltCorner.X * cosVal - rbCorner.Y * sinVal);
				_vertices[vertexIndex + 2].Position.Y = (ltCorner.X * sinVal + rbCorner.Y * cosVal);
				_vertices[vertexIndex + 3].Position.X = (rbCorner.X * cosVal - rbCorner.Y * sinVal);
				_vertices[vertexIndex + 3].Position.Y = (rbCorner.X * sinVal + rbCorner.Y * cosVal);
			}
			else
			{
				// Else just keep the positions.
				_vertices[vertexIndex].Position = new Vector4(ltCorner, 0, 1);
				_vertices[vertexIndex + 1].Position = new Vector4(rbCorner.X, ltCorner.Y, 0, 1);
				_vertices[vertexIndex + 2].Position = new Vector4(ltCorner.X, rbCorner.Y, 0, 1);
				_vertices[vertexIndex + 3].Position = new Vector4(rbCorner, 0, 1);
			}

			// Translate.
			_vertices[vertexIndex].Position.X += _position.X;
			_vertices[vertexIndex + 1].Position.X += _position.X;
			_vertices[vertexIndex + 2].Position.X += _position.X;
			_vertices[vertexIndex + 3].Position.X += _position.X;

			_vertices[vertexIndex].Position.Y += _position.Y;
			_vertices[vertexIndex + 1].Position.Y += _position.Y;
			_vertices[vertexIndex + 2].Position.Y += _position.Y;
			_vertices[vertexIndex + 3].Position.Y += _position.Y;

			// Update texture coordinates.
			_vertices[vertexIndex].UV = glyph.TextureCoordinates.Location;
			_vertices[vertexIndex + 1].UV = new Vector2(glyph.TextureCoordinates.Right, glyph.TextureCoordinates.Top);
			_vertices[vertexIndex + 2].UV = new Vector2(glyph.TextureCoordinates.Left, glyph.TextureCoordinates.Bottom);
			_vertices[vertexIndex + 3].UV = new Vector2(glyph.TextureCoordinates.Right, glyph.TextureCoordinates.Bottom);

			// Set depth.
			if (_depth != 0.0f)
				_vertices[vertexIndex + 3].Position.Z = _vertices[vertexIndex + 2].Position.Z = _vertices[vertexIndex + 1].Position.Z = _vertices[vertexIndex].Position.Z = -Depth;
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

			for (int line = 0; line < _lines.Count; line++)
			{
				for (int i = 0; i < _lines[line].Length; i++)
				{
					char c = _lines[line][i];

					switch (c)
					{
						case ' ':
							glyph = _font.Glyphs[c];
							pos.X += glyph.GlyphCoordinates.Width - 1;
							continue;
						case '\t':
							glyph = _font.Glyphs[' '];
							pos.X += (glyph.GlyphCoordinates.Width - 1) * TabSpaces;
							continue;
						case '\r':
							pos.X = 0;
							continue;
					}

					if (_font.Glyphs.Contains(c))
						glyph = _font.Glyphs[c];
					else
						glyph = _font.Glyphs[_font.Settings.DefaultCharacter];

					UpdateTransform(glyph, vertexIndex, Vector2.Add(pos, glyph.Offset));

					vertexIndex += 4;

					pos.X += glyph.Advance.X + glyph.Advance.Y + outlineOffset.X;

					// Apply kerning pairs.
					if ((i < _text.Length - 1) && (_font.KerningPairs.Count > 0))
					{
						GorgonKerningPair kerning = new GorgonKerningPair(c, _text[i + 1]);
						if (_font.KerningPairs.ContainsKey(kerning))
							pos.X += _font.KerningPairs[kerning];
						else
							pos.X += glyph.Advance.Z;
					}
					else
						pos.X += glyph.Advance.Z;
				}

				pos.Y += _font.FontHeight + outlineOffset.Y;
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
			_size = MeasureText();
		}

		/// <summary>
		/// Function to measure the width of a single line of text 
		/// </summary>
		/// <param name="line">Line to measure.</param>
		/// <param name="outlineOffset">Outline offset.</param>
		/// <returns>The width, pixels, of a single line of text.</returns>
		private float LineMeasure(string line, float outlineOffset)
		{
			float size = 0;

			for (int i = 0; i < line.Length; i++)
			{
				GorgonGlyph glyph = null;
				char c = line[i];

				if (_font.Glyphs.Contains(c))
					glyph = _font.Glyphs[c];
				else
					glyph = _font.Glyphs[_font.Settings.DefaultCharacter];
				
				switch (c)
				{
					case ' ':
						size += glyph.GlyphCoordinates.Width - 1;
						continue;
					case '\t':
						size += (glyph.GlyphCoordinates.Width - 1) * TabSpaces;
						continue;
					case '\r':
					case '\n':
						continue;
				}

				size += glyph.Advance.X + glyph.Advance.Y + outlineOffset;

				if ((i < line.Length - 1) && (_font.KerningPairs.Count > 0))
				{
					GorgonKerningPair kernPair = new GorgonKerningPair(c, line[i + 1]);

					if (_font.KerningPairs.ContainsKey(kernPair))
						size += _font.KerningPairs[kernPair];
					else
						size += glyph.Advance.Z;
				}
				else
					size += glyph.Advance.Z;
			}
			return size;
		}

		/// <summary>
		/// Function to measure the bounds of the text in the text object.
		/// </summary>
		/// <returns>The bounding rectangle for the text.</returns>
		private SizeF MeasureText()
		{
			SizeF result = SizeF.Empty;
			float outlineSize = 0;

			if ((_text.Length == 0) || (_formattedText.Length == 0) || (_lines.Count == 0))
				return result;

			if ((_font.Settings.OutlineColor.Alpha > 0) && (_font.Settings.OutlineSize > 0))
				outlineSize = _font.Settings.OutlineSize;

			result.Height = _lines.Count * (_font.FontHeight + outlineSize);

			for (int i = 0; i < _lines.Count; i++)
				result.Width = GorgonMathUtility.Max(result.Width, LineMeasure(_lines[i], outlineSize));

			return result;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="M:GorgonLibrary.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public void Draw()
		{
			Rectangle clipRegion = Rectangle.Round(ClipRegion);
			Rectangle? lastClip = Gorgon2D.ClipRegion;
			StateChange states = StateChange.None;
			int vertexIndex = 0;

			// We don't need to draw anything.
			if (_text.Length == 0)
				return;
			
			if ((ClipToRectangle) && (((lastClip == null) && (_textRect != null)) || ((lastClip != null) && (lastClip.Value != ClipRegion))))
				Gorgon2D.ClipRegion = clipRegion;

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

			if (ClipToRectangle)
				Gorgon2D.ClipRegion = lastClip;
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

		#region IMoveable Members
		/// <summary>
		/// Property to set or return the texture region.
		/// </summary>
		RectangleF IMoveable.TextureRegion
		{
			get
			{
				if (_currentTexture == null)
					return RectangleF.Empty;

				return new RectangleF(Vector2.Zero, _currentTexture.Settings.Size);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the position of the renderable.
		/// </summary>
		public Vector2 Position
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
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a renderable.
		/// </summary>
		public float Angle
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
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
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
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the renderable.
		/// </summary>
		public Vector2 Anchor
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
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		public float Depth
		{
			get
			{
				return _depth;
			}
			set
			{
				if (_depth != value) 
				{
					if (_vertices != null)
					{
						for (int i = 0; i < _vertices.Length; i++)
							_vertices[i].Position.Z = value;
					}

					_depth = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		Vector2 IMoveable.TextureOffset
		{
			get
			{
				return Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the scaling of the texture width and height.
		/// </summary>
		Vector2 IMoveable.TextureSize
		{
			get
			{
				if (_currentTexture != null)
					return _currentTexture.Settings.Size;
				else
					return Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		public Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion
	}
}
