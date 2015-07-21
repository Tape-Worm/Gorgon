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
using System.Drawing;
using System.Globalization;
using System.Text;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.UI;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A renderable object that renders a block of text.
	/// </summary>
	/// <remarks>Text in Gorgon is like any other renderable.  It can be used as a discrete object that can be translated, scaled, or rotated.  Additionally it can 
	/// wrap text at a boundary, change its horizontal alignment and line spacing.  Text objects require the use of <see cref="Gorgon.Graphics.GorgonFont">font</see> 
	/// objects in order to render the glyphs used in the text.</remarks>
	public class GorgonText
		: GorgonNamedObject, IRenderable, IMoveable, I2DCollisionObject
    {
        #region Value Types.
        /// <summary>
        /// Color code used for rendering multicolored text.
        /// </summary>
	    private struct ColorCode
	    {
            /// <summary>
            /// Starting index of the text to color.
            /// </summary>
	        public readonly int StartIndex;
            /// <summary>
            /// Ending index of the text to color.
            /// </summary>
	        public readonly int EndIndex;
            /// <summary>
            /// Color to use.
            /// </summary>
	        public readonly GorgonColor Color;

            /// <summary>
            /// Initializes a new instance of the <see cref="ColorCode"/> struct.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <param name="startIndex">The start index.</param>
            /// <param name="endIndex">The end index.</param>
            public ColorCode(Color color, int startIndex, int endIndex)
            {
                Color = color;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }
	    }
        #endregion

        #region Variables.
        private readonly StringBuilder _tabText;								// Tab spacing text.
	    private readonly Gorgon2DVertexCache _vertexCache;                      // The internal vertex cache used by the renderer.
		private Gorgon2DCollider _collider;                                     // Collision object.
		private int _colliderVertexCount;                                       // Collider vertex count.
		private RectangleF? _textRect;											// Text rectangle.
		private int _vertexCount;												// Number of vertices.
		private Gorgon2DVertex[] _vertices;										// Vertices.
		private GorgonFont _font;												// Font to apply to the text.
		private string _encodedText;											// The text with color embedded codes.
		private string _text;													// Original text.
		private readonly GorgonColor[] _colors = new GorgonColor[4];			// Vertex colors.
		private GorgonRenderable.DepthStencilStates _depthStencil;				// Depth/stencil states.
		private GorgonRenderable.BlendState _blend;								// Blending states.
		private GorgonRenderable.TextureSamplerState _sampler;					// Texture sampler states.
		private GorgonTexture2D _currentTexture;								// Currently active texture.
		private bool _needsVertexUpdate = true;									// Flag to indicate that we need a vertex update.
		private bool _needsColorUpdate = true;									// Flag to indicate that the color needs updating.
		private bool _needsShadowUpdate = true;									// Flag to indicate that the shadow alpha needs updating.
		private bool _needsTextUpdate = true;									// Flag to indicate that the text formatting needs to be updated.
		private readonly StringBuilder _formattedText;							// Formatted text.
		private readonly List<string> _lines;									// Lines in the text.
		private Vector2 _size = Vector2.Zero;									// Size of the text block.
		private Vector2 _position = Vector2.Zero;								// Position of the text.
		private Vector2 _scale = new Vector2(1);								// Scale for the text.
		private float _angle;													// Angle of rotation for the text.
		private Vector2 _anchor = Vector2.Zero;									// Anchor point for the text.
		private float _depth;													// Depth value.
		private int _tabSpace = 3;												// Tab spaces.
		private bool _wordWrap;													// Flag to indicate that the text should word wrap.
		private Alignment _alignment = Alignment.UpperLeft;						// Text alignment.
		private float _lineSpace = 1.0f;										// Line spacing.
		private readonly float[] _shadowAlpha = new float[4];					// Shadow vertex opacity.
		private Vector2 _shadowOffset = new Vector2(1);							// Shadow offset.
		private bool _shadowEnabled;											// Flag to indicate whether shadowing is enabled or not.
		private bool _useKerning = true;										// Flag to indicate that kerning should be used.
	    private readonly List<ColorCode> _colorCodes = new List<ColorCode>();   // List of color code points.
		private bool _allowColorCodes;											// Flag to indicate color codes are allowed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the clipping region.
		/// </summary>
		private RectangleF ClipRegion => _textRect == null ? (RectangleF)Gorgon2D.DefaultViewport : _textRect.Value;

		/// <summary>
		/// Property to set or return whether to allow the embedding of different colors in the <see cref="Text"/>.
		/// </summary>
		/// <remarks>When this value is set to <b>true</b>, Gorgon will parse the string to look for codes to change the color of the text.  The codes 
		/// should follow the format of [c=RRGGBBAA]text[/c].
		/// <para>For example, in the text "The quick brown fox", a code of "The quick [c=554400FF]brown[/c] fox" will change the word 'brown' to the color brown.</para>
		/// <para>Be aware that when this value is set to <b>true</b> there will be a slight performance penalty.</para>
		/// </remarks>
	    public bool AllowColorCodes
	    {
			get
			{
				return _allowColorCodes;
			}
			set
			{
				if (value == _allowColorCodes)
				{
					return;
				}

				_allowColorCodes = value;

				// Reset the text.
				if (_allowColorCodes)
				{
					_encodedText = _text;
					_text = ParseCodes(_encodedText);
				}
				else
				{
					_text = _encodedText;
				}

				FormatText();
				UpdateText();
				_needsColorUpdate = true;
				if (ShadowEnabled)
				{
					_needsShadowUpdate = true;
				}
				_needsTextUpdate = true;
				_needsVertexUpdate = true;
			}
	    }

		/// <summary>
		/// Property to set or return whether kerning should be used.
		/// </summary>
		public bool UseKerning
		{
			get
			{
				return _useKerning;
			}
			set
			{
				if (value == _useKerning)
				{
					return;
				}

				_useKerning = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset for the shadow.
		/// </summary>
		/// <remarks>This value only applies when <see cref="P:Gorgon.Renderers.GorgonText.ShadowEnabled">ShadowEnabled</see> is <b>true</b>.</remarks>
		[AnimatedProperty]
		public Vector2 ShadowOffset
		{
			get
			{
				return _shadowOffset;
			}
			set
			{
				if (_shadowOffset == value)
				{
					return;
				}

				_shadowOffset = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether shadowing is enabled or not.
		/// </summary>
		public bool ShadowEnabled
		{
			get
			{
				return _shadowEnabled;
			}
			set
			{
				if (_shadowEnabled == value)
				{
					return;
				}

				_shadowEnabled = value;
				_needsShadowUpdate = value;
				_needsVertexUpdate = true;
				_needsColorUpdate = true;
				UpdateText();
			}
		}

		/// <summary>
		/// Property to set or return the shadow opacity.
		/// </summary>
		/// <remarks>This value only applies when <see cref="P:Gorgon.Renderers.GorgonText.ShadowEnabled">ShadowEnabled</see> is <b>true</b>.</remarks>
		[AnimatedProperty]
		public float ShadowOpacity
		{
			get
			{
				return _shadowAlpha[0];
			}
			set
			{
				for (int i = 0; i < 4; i++)
				{
					_shadowAlpha[i] = value;
				}

				_needsShadowUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the spacing for the lines.
		/// </summary>
		[AnimatedProperty]
		public float LineSpacing
		{
			get
			{
				return _lineSpace;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_lineSpace == value)
				{
					return;
				}

				_lineSpace = value;
				_size = GetTextSize();
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the alignment of the text within the <see cref="P:Gorgon.Renderers.GorgonText.TextRectangle">TextRectangle.</see>/.
		/// </summary>
		/// <remarks>If the TextRectangle property is NULL (<i>Nothing</i> in VB.Net), then this value has no effect.</remarks>
		public Alignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				if (_alignment == value)
				{
					return;
				}

				_alignment = value;
				_needsTextUpdate = _needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether word wrapping is enabled.
		/// </summary>
		/// <remarks>
		/// This uses the <see cref="P:Gorgon.Renderers.GorgonText.TextRectangle">TextRectangle</see> to determine where the cutoff boundary is for word wrapping.  
		/// If a character is positioned outside of the region, then the previous space character is located and the region is broken at that point.  Note that the left position 
		/// of the rectangle is not taken into consideration when performing a word wrap, and consequently the user will be responsible for calculating the word wrap boundary.
		/// <para>Only the space character is considered when performing word wrapping.  Other whitespace or control characters are not considered break points in the string.</para>
		/// <para>If the TextRectangle property is NULL (<i>Nothing</i> in VB.Net), then this value has no effect.</para></remarks>
		public bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				if (value == _wordWrap)
				{
					return;
				}

				_wordWrap = value;
				_needsTextUpdate = _needsVertexUpdate = true;
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
				{
					value = 1;
				}

				_tabSpace = value;
				_tabText.Length = 0;
				_tabText.Append(' ', value);
			}
		}

		/// <summary>
		/// Property to set or return the rectangle used for clipping and/or alignment for the text.
		/// </summary>
		/// <remarks>
		/// This defines the range used when aligning text horizontally and/or vertically.  For example, if the alignment is set to center horizontally, then the width of this rectangle is 
		/// used to determine the horizontal center point.  Likewise for vertically aligned text.
		/// <para>If <see cref="P:Gorgon.Renderers.GorgonText.WordWrap">WordWrap</see> is set to <b>true</b>, then this determines how far, in pixels, a line of text can go before it will be wrapped.</para>
		/// <para>This property will clip the text to the rectangle if the <see cref="P:Gorgon.Renderers.GorgonText.ClipToRectangle">ClipToRectangle</see> property is set to <b>true</b>.</para>
		/// <para>Setting this value to NULL (<i>Nothing</i> in VB.Net) will disable alignment, word wrapping and clipping.</para>
		/// </remarks>
		public RectangleF? TextRectangle
		{
			get
			{
				return _textRect;
			}
			set
			{
				if (_textRect == value)
				{
					return;
				}

				_textRect = value;
				_needsTextUpdate = _needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to clip the text to the <see cref="P:Gorgon.Renderers.GorgonText.TextRectangle">TextRectangle</see>.
		/// </summary>
		/// <remarks>If the TextRectangle property is NULL (<i>Nothing</i> in VB.Net), this value will have no effect.</remarks>
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
		}

		/// <summary>
		/// Property to set or return the text to display.
		/// </summary>
		public string Text
		{
			get
			{
				return AllowColorCodes ? _encodedText : _text;
			}
			set
			{
				int prevLength = _text.Length;

				if (value == null)
				{
					value = string.Empty;
				}

				if (string.Compare(value, AllowColorCodes ? _text : _encodedText, StringComparison.CurrentCulture) != 0)
				{
					if (AllowColorCodes)
					{
						_encodedText = value;
						_text = ParseCodes(value);
					}
					else
					{
						_text = value;
					}

					UpdateText();
				}

				// Update the colors for the rest of the string if the text length is longer.
				if (prevLength >= _text.Length)
				{
					return;
				}

				_needsColorUpdate = true;
				if (ShadowEnabled)
				{
					_needsShadowUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the font to be applied to the text.
		/// </summary>
		/// <remarks>This property will ignore requests to set it to NULL (<i>Nothing</i> in VB.Net).</remarks>
		public GorgonFont Font
		{
			get
			{
				return _font;
			}
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    if (value == _font)
			    {
			        return;
			    }

                if (_font != null)
                {
                    _font.FontChanged -= FontChanged;
                }

			    _font = value;

                if (_font != null)
                {
                    _font.FontChanged += FontChanged;
                }
                
			    UpdateText();
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to parse the codes from the text.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <returns>The text without the codes.</returns>
	    private string ParseCodes(string text)
	    {
            _colorCodes.Clear();

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            int startIndex = text.IndexOf("[c=", StringComparison.OrdinalIgnoreCase);

            while ((startIndex > -1)
                   && ((startIndex + 3) < text.Length))
            {
                int codeStart = startIndex;

                // Find the closing bracket.
                int endBracket = text.IndexOf(']', startIndex);

                // Locate the closing tag.
                int endIndex = text.IndexOf("[/c]", startIndex + 1, StringComparison.OrdinalIgnoreCase);

				startIndex = text.IndexOf("[c=", startIndex + 1, StringComparison.OrdinalIgnoreCase);

	            if (startIndex < endIndex)
	            {
		            startIndex = -1;
	            }

                int colorLength = endBracket - (codeStart + 3);

                if ((endBracket == -1)
                    || (endIndex == -1)
                    || (colorLength <= 0))
                {
                    continue;
                }

                int colorValue;
                string colorText = text.Substring(codeStart + 3, colorLength);

                if (!int.TryParse(colorText,
                                  NumberStyles.HexNumber,
                                  CultureInfo.InvariantCulture,
                                  out colorValue))
                {
                    continue;
                }

	            int textRange = endIndex - 1 - endBracket;

	            if (textRange <= 0)
	            {
		            continue;
	            }

				// Remove the closing tag, we don't need it now.
				text = text.Remove(endIndex, 4);

                GorgonColor color = GorgonColor.FromRGBA(colorValue);

                // Remove the codes from the text so it doesn't get used in calculations 
                // or actual rendering.
				text = text.Remove(codeStart, (endBracket + 1 - codeStart));

                _colorCodes.Add(new ColorCode(color, codeStart, codeStart + textRange - 1));

				startIndex = text.IndexOf("[c=", codeStart, StringComparison.OrdinalIgnoreCase);
            }

	        _needsColorUpdate = true;

            return text;
	    }

        /// <summary>
        /// Function to handle the font changing event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event parameters.</param>
        private void FontChanged(object sender, EventArgs e)
        {
            if ((_font != null)
                && (_font.IsDisposed))
            {
                return;
            }

            UpdateText();

            _needsColorUpdate = true;
            _needsVertexUpdate = true;
        }

		/// <summary>
		/// Function to perform an update on the text object.
		/// </summary>
		private void UpdateText()
		{
			if ((_text.Length == 0) || (_font == null) || (_font.Glyphs.Count == 0))
			{
				_vertexCount = 0;
			    if (_vertices == null)
			    {
			        _vertices = new Gorgon2DVertex[1024];
			    }

			    for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}

				_currentTexture = null;
				_needsShadowUpdate = true;
				_needsVertexUpdate = true;
				_needsColorUpdate = true;
				return;
			}

			// Find out how many vertices we have.
			_vertexCount = _text.Length * (_shadowEnabled ? 8 : 4);

			// Don't do less than 512 vertices.
			if (_vertexCount < 512)
			{
				_vertexCount = 512;
			}

			// Allocate the number of vertices if the vertex count is less than the required amount.
			if (_vertexCount > _vertices.Length)
			{
				_vertices = new Gorgon2DVertex[_vertexCount * 2];
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}

				_needsShadowUpdate = true;
				_needsColorUpdate = true;
			}

			// Get the first texture.
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _text.Length; i++)
			{
			    GorgonGlyph glyph;

				if (!_font.Glyphs.TryGetValue(_text[i], out glyph))
				{
					continue;
				}

				_currentTexture = glyph.Texture;
				break;
			}

			_needsVertexUpdate = true;

			FormatText();
		}

		/// <summary>
		/// Function to update the transform coordinates by the alignment settings.
		/// </summary>
		/// <param name="leftTop">Left and top coordinates.</param>
		/// <param name="rightBottom">Right and bottom coordinates.</param>
		/// <param name="lineLength">Length of the line, in pixels.</param>
		private void GetAlignmentExtents(ref Vector2 leftTop, ref Vector2 rightBottom, float lineLength)
		{
			int calc;

			switch (_alignment)
			{
				case Alignment.UpperCenter:
					calc = (int)((ClipRegion.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					rightBottom.X += calc;
					break;
				case Alignment.UpperRight:
					calc = (int)(ClipRegion.Width - lineLength);
					leftTop.X += calc;
					rightBottom.X += calc;
					break;
				case Alignment.CenterLeft:
					calc = (int)(ClipRegion.Height / 2.0f - _size.Y / 2.0f);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case Alignment.Center:
					calc = (int)((ClipRegion.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height / 2.0f - _size.Y / 2.0f);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case Alignment.CenterRight:
					calc = (int)(ClipRegion.Width - lineLength);
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height / 2.0f - _size.Y / 2.0f);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case Alignment.LowerLeft:
					calc = (int)(ClipRegion.Height - _size.Y);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case Alignment.LowerCenter:
					calc = (int)((ClipRegion.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height - _size.Y);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case Alignment.LowerRight:
					calc = (int)(ClipRegion.Width - lineLength);
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height - _size.Y);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
			}
		}

		/// <summary>
		/// Function to update the transformation for the text.
		/// </summary>
		/// <param name="glyph">Glyph to transform.</param>
		/// <param name="vertexIndex">Index of the vertex to modify.</param>
		/// <param name="textOffset">Offset of the glyph within the text.</param>
		/// <param name="lineLength">The length of the line of text when alignment is active.</param>
		/// <param name="cosValue">Cosine value for rotation.</param>
		/// <param name="sinValue">Sin value for rotation.</param>
		private void UpdateTransform(GorgonGlyph glyph, int vertexIndex, ref Vector2 textOffset, float lineLength, float cosValue, float sinValue)
		{
			Vector2 ltCorner = Vector2.Subtract(textOffset, Anchor);
			Vector2 rbCorner = Vector2.Add(ltCorner, glyph.GlyphCoordinates.Size);

			if ((_alignment != Alignment.UpperLeft) && (_textRect != null))
			{
				GetAlignmentExtents(ref ltCorner, ref rbCorner, lineLength);
			}

			// Scale.
			// ReSharper disable CompareOfFloatsByEqualityOperator
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
				// Rotate the vertices.
				_vertices[vertexIndex].Position.X = (ltCorner.X * cosValue - ltCorner.Y * sinValue);
				_vertices[vertexIndex].Position.Y = (ltCorner.X * sinValue + ltCorner.Y * cosValue);
				_vertices[vertexIndex + 1].Position.X = (rbCorner.X * cosValue - ltCorner.Y * sinValue);
				_vertices[vertexIndex + 1].Position.Y = (rbCorner.X * sinValue + ltCorner.Y * cosValue);
				_vertices[vertexIndex + 2].Position.X = (ltCorner.X * cosValue - rbCorner.Y * sinValue);
				_vertices[vertexIndex + 2].Position.Y = (ltCorner.X * sinValue + rbCorner.Y * cosValue);
				_vertices[vertexIndex + 3].Position.X = (rbCorner.X * cosValue - rbCorner.Y * sinValue);
				_vertices[vertexIndex + 3].Position.Y = (rbCorner.X * sinValue + rbCorner.Y * cosValue);
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
			_vertices[vertexIndex + 3].Position.Z = _vertices[vertexIndex + 2].Position.Z = _vertices[vertexIndex + 1].Position.Z = _vertices[vertexIndex].Position.Z = -Depth;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

	    /// <summary>
		/// Function to perform a vertex update for the text.
		/// </summary>
		private void UpdateVertices()
		{
			int vertexIndex = 0;
			Vector2 pos = Vector2.Zero;
			float outlineOffset = 0;
			float angle = _angle.ToRadians();						// Angle in radians.
			float cosVal = angle.Cos();							// Cached cosine.
			float sinVal = angle.Sin();							// Cached sine.

		    if ((_font.Settings.OutlineSize > 0)
		        && (_font.Settings.OutlineColor1.Alpha > 0))
		    {
		        outlineOffset = _font.Settings.OutlineSize;
		    }

			_colliderVertexCount = 0;
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int line = 0; line < _lines.Count; line++)
			{
				float lineLength = 0;
				string currentLine = _lines[line];

				if ((_alignment != Alignment.UpperLeft) && (_textRect.HasValue))
				{
					lineLength = LineMeasure(_lines[line], outlineOffset);
				}

				for (int i = 0; i < currentLine.Length; ++i)
				{
					char c = currentLine[i];

				    GorgonGlyph glyph;

					if (!_font.Glyphs.TryGetValue(c, out glyph))
					{
						c = _font.Settings.DefaultCharacter;
                        _font.Glyphs.TryGetValue(c, out glyph);
					}

					if (c == ' ')
					{
						pos.X += glyph.GlyphCoordinates.Width - 1;
						continue;
					}

                    var vertexPosition = new Vector2(pos.X + glyph.Offset.X, pos.Y + glyph.Offset.Y);

				    // Add shadow character.
					if (_shadowEnabled)
					{
						Vector2 shadowPosition;

						Vector2.Add(ref vertexPosition, ref _shadowOffset, out shadowPosition);
						UpdateTransform(glyph, vertexIndex, ref shadowPosition, lineLength, cosVal, sinVal);
						vertexIndex += 4;
						_colliderVertexCount += 4;
					}

					UpdateTransform(glyph, vertexIndex, ref vertexPosition, lineLength, cosVal, sinVal);
					vertexIndex += 4;
					_colliderVertexCount += 4;

				    pos.X += outlineOffset;

					// Apply kerning pairs.
				    if (_useKerning)
				    {
				        pos.X += glyph.Advance;

				        if ((i == currentLine.Length - 1)
				            || (_font.KerningPairs.Count == 0))
				        {
				            continue;
				        }

				        var kerning = new GorgonKerningPair(c, currentLine[i + 1]);
				        int kernAmount;
				        if (_font.KerningPairs.TryGetValue(kerning, out kernAmount))
				        {
				            pos.X += kernAmount;
				        }
				    }
				    else
				    {
				        pos.X += glyph.GlyphCoordinates.Width;
				    }
				}

				// If we have texture filtering on, this is going to look weird because it's a sub-pixel.
				// In order to get the font to look right on a second line, we need to use point filtering.
				// We -could- use a Floor here, but then movement becomes very jerky.  We're better off turning
				// off texture filtering even just for an increase in speed.
				pos.Y += (_font.FontHeight + _font.Settings.OutlineSize) * _lineSpace;
				pos.X = 0;
			}

			if (_collider != null)
			{
				_collider.UpdateFromCollisionObject();
			}
		}

		/// <summary>
		/// Function to update shadow alpha.
		/// </summary>
		private void UpdateShadowAlpha()
		{
			for (int i = 0; i < _vertexCount; i += 8)
			{
				_vertices[i].Color = new GorgonColor(0, 0, 0, _shadowAlpha[0]);
				_vertices[i + 1].Color = new GorgonColor(0, 0, 0, _shadowAlpha[1]);
				_vertices[i + 2].Color = new GorgonColor(0, 0, 0, _shadowAlpha[2]);
				_vertices[i + 3].Color = new GorgonColor(0, 0, 0, _shadowAlpha[3]);
			}
		}

		/// <summary>
		/// Function to update the colors.
		/// </summary>
		private void UpdateColors()
		{
			int increment = _shadowEnabled ? 8 : 4;

			if ((AllowColorCodes)
				&& (_colorCodes.Count > 0))
			{
				int vertexIndex = _shadowEnabled ? 4 : 0;

				// For color codes we need an alternate path to determine which vertices to update.
				for (int i = 0; i < _text.Length; ++i)
				{
					char c = _text[i];

					// These characters don't have color.
					switch (c)
					{
						case '\n':
						case '\t':
						case ' ':
						case '\r':
							continue;
					}
					
					// Apply color codes.
					for (int code = 0; code < _colorCodes.Count; ++code)
					{
						if ((i < _colorCodes[code].StartIndex) || (i > _colorCodes[code].EndIndex))
						{
							// Use base color.
							_vertices[vertexIndex].Color = _colors[0];
							_vertices[vertexIndex + 1].Color = _colors[1];
							_vertices[vertexIndex + 2].Color = _colors[2];
							_vertices[vertexIndex + 3].Color = _colors[3];
							continue;
						}

						_vertices[vertexIndex + 3].Color =
							_vertices[vertexIndex + 2].Color = _vertices[vertexIndex + 1].Color = _vertices[vertexIndex].Color = _colorCodes[code].Color;
						break;
					}

					vertexIndex += increment;
				}

				return;
			}

			for (int i = (increment - 4); i < _vertexCount; i += increment)
			{
				_vertices[i].Color = _colors[0];
				_vertices[i + 1].Color = _colors[1];
				_vertices[i + 2].Color = _colors[2];
				_vertices[i + 3].Color = _colors[3];
			}
		}

		/// <summary>
		/// Function to word wrap at the text region border.
		/// </summary>
		private void WordWrapText()
		{
			int i = 0;
			float pos = 0;
			int outlineSize = 0;
			RectangleF region = ClipRegion;

			// If we don't have a font, then we have nothing to wrap.  Yet.
			if ((_font == null)
				|| (pos >= region.Width))
			{
				return;
			}

			if (_font.Settings.OutlineColor1.Alpha > 0)
			{
				outlineSize = _font.Settings.OutlineSize;
			}

			_formattedText.Append(_text);
			_formattedText.Replace("\n\r", "\n");
			_formattedText.Replace("\r\n", "\n");
			_formattedText.Replace("\r", "\n");
			_formattedText.Replace("\t", _tabText.ToString());

			while (i < _formattedText.Length)
			{
				char character = _formattedText[i];

				if (character == '\n')
				{
					pos = 0;
					i++;
					continue;
				}

			    GorgonGlyph glyph;

				if (!_font.Glyphs.TryGetValue(character, out glyph))
				{
					character = _font.Settings.DefaultCharacter;
                    _font.Glyphs.TryGetValue(character, out glyph);
				}

				// If we can't fit a single glyph into the boundaries, then just leave.  Else we'll have an infinite loop on our hands.
				if (glyph.GlyphCoordinates.Width > region.Width)
				{
					return;
				}

				switch (character)
				{
					case ' ':
						pos += glyph.GlyphCoordinates.Width - 1;
						break;
					default:
						if (_useKerning)
						{
                            pos += glyph.Advance + outlineSize;

							if ((i < _formattedText.Length - 1) && (_font.KerningPairs.Count > 0))
							{
								var kernPair = new GorgonKerningPair(character, _formattedText[i + 1]);
							    int kernAmount;

								if (_font.KerningPairs.TryGetValue(kernPair, out kernAmount))
								{
									pos += kernAmount;
								}
							}
						}
						else
						{
							pos += glyph.GlyphCoordinates.Width + outlineSize;
						}
						break;
				}

				if (pos > region.Width)
				{
					int j = i;

					// Try to find the previous space and replace it with a new line.
					// Otherwise just insert it at the previous character.
					while (j >= 0)
					{
						if ((_formattedText[j] == '\n') || (_formattedText[j] == '\r'))
						{
							j = -1;
							break;
						}

						if ((_formattedText[j] == ' ') || (_formattedText[j] == '\t'))
						{
							_formattedText[j] = '\n';
							i = j;
							break;
						}

						j--;
					}

					// If we reach end of line without finding a line break, add a line break.
					if ((i > 0) && (j < 0))
					{
						_formattedText.Insert(i, "\n");
					}

					pos = 0;
				}

				i++;
			}
		}

		/// <summary>
		/// Function to format the text for measuring and clipping.
		/// </summary>
		private void FormatText()
		{
			_formattedText.Length = 0;
			if ((!_textRect.HasValue) || (!_wordWrap))
			{
				_formattedText.Append(_text);
				_formattedText.Replace("\n\r", "\n");
				_formattedText.Replace("\r\n", "\n");
				_formattedText.Replace("\r", "\n");
				_formattedText.Replace("\t", _tabText.ToString());
			}
			else
			{
				WordWrapText();
			}

			_lines.Clear();
			_lines.AddRange(_formattedText.ToString().Split('\n'));
			_size = GetTextSize();
			_needsTextUpdate = false;
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
			bool firstChar = true;

			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
			    GorgonGlyph glyph;

				if (!_font.Glyphs.TryGetValue(c, out glyph))
				{
					c = _font.Settings.DefaultCharacter;
                    _font.Glyphs.TryGetValue(c, out glyph);
				}

				switch (c)
				{
					case ' ':
						size += glyph.GlyphCoordinates.Width - 1;
						continue;
					case '\r':
					case '\n':
						continue;
				}

				// Include the initial offset.
				if (firstChar)
				{
					size += glyph.Offset.X;
					firstChar = false;
				}

			    size += outlineOffset;

			    if (_useKerning)
			    {
			        size += glyph.Advance;

			        if ((i == line.Length - 1)
			            || (_font.KerningPairs.Count == 0))
			        {
			            continue;
			        }

			        var kerning = new GorgonKerningPair(c, line[i + 1]);
			        int kernAmount;

			        if (_font.KerningPairs.TryGetValue(kerning, out kernAmount))
			        {
			            size += kernAmount;
			        }
			    }
			    else
			    {
			        size += glyph.GlyphCoordinates.Width;
			    }
			}
			return size;
		}

		/// <summary>
		/// Function to measure the bounds of the text in the text object.
		/// </summary>
		/// <returns>The bounding rectangle for the text.</returns>
		private Vector2 GetTextSize()
		{
			Vector2 result = Vector2.Zero;
			float outlineSize = 0;

			if ((_text.Length == 0) || (_formattedText.Length == 0) || (_lines.Count == 0) || (_font == null))
			{
				return result;
			}

			if (_font.Settings.OutlineColor1.Alpha > 0)
			{
				outlineSize = _font.Settings.OutlineSize;
			}

			if (!_lineSpace.EqualsEpsilon(1.0f))
			{
				result.Y = (_lines.Count - 1) * (((_font.FontHeight + (outlineSize * 2.0f)) * _lineSpace)) +
				           (_font.FontHeight + (outlineSize * 2.0f));
			}
			else
			{
				result.Y = (_lines.Count * (_font.FontHeight + (outlineSize * 2.0f)));
			}

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _lines.Count; i++)
			{
				result.X = result.X.Max(LineMeasure(_lines[i], outlineSize));
			}

			return result;
		}

		/// <summary>
		/// Function to set the color for a specific corner on all characters in the string.
		/// </summary>
		/// <param name="corner">Corner of the characters to set.</param>
		/// <param name="color">Color to set.</param>
		public void SetCornerColor(RectangleCorner corner, GorgonColor color)
		{
			var index = (int)corner;

			if (color == _colors[index])
			{
				return;
			}

			_colors[index] = color;
			_needsColorUpdate = true;
		}

		/// <summary>
		/// Function to retrieve the color for a specific corner on all characters in the string.
		/// </summary>
		/// <param name="corner">Corner of the characters to retrieve the color from.</param>
		/// <returns>The color on the specified corner of the characters.</returns>
		public GorgonColor GetCornerColor(RectangleCorner corner)
		{
			return _colors[(int)corner];
		}

		/// <summary>
		/// Function to return the size, in pixels of the string assigned to the text object.
		/// </summary>
		/// <remarks>Unlike the overloaded MeasureText function, this one determines whether to use word wrapping based on the settings for the text object and uses the text that's already assigned to the text object.</remarks>
		public Vector2 MeasureText()
		{
			return MeasureText(Text);
		}

		/// <summary>
		/// Function to return the size, in pixels, of a string.
		/// </summary>
		/// <param name="text">Text to measure.</param>
		/// <returns>The size of the text, in pixels.</returns>
		/// <remarks>Unlike the overloaded MeasureText function, this one determines whether to use word wrapping based on the settings for the text object.</remarks>
		public Vector2 MeasureText(string text)
		{
			float size = float.MaxValue - 1;

			if (TextRectangle != null)
			{
				size = TextRectangle.Value.Width;
			}

			return MeasureText(text, WordWrap, size);
		}

		/// <summary>
		/// Function to return the size, in pixels, of a string.
		/// </summary>
		/// <param name="text">Text to measure.</param>
		/// <param name="useWordWrap"><b>true</b> to use word wrapping.</param>
		/// <param name="wrapBoundaryWidth">The boundary to word wrap on.</param>
		/// <returns>The size of the text, in pixels.</returns>
		/// <remarks>If the <paramref name="useWordWrap"/> is <b>true</b> and the <paramref name="wrapBoundaryWidth"/> is 0 or less, then word wrapping is disabled.</remarks>
		public Vector2 MeasureText(string text, bool useWordWrap, float wrapBoundaryWidth)
		{
			string previousString = Text;
			bool lastWordWrap = _wordWrap;
			RectangleF? lastRect = _textRect;

			try
			{
				if (string.IsNullOrEmpty(text))
				{
					return Vector2.Zero;
				}

				if (wrapBoundaryWidth <= 0.0f)
				{
					useWordWrap = false;
				}

				WordWrap = useWordWrap;
				if (WordWrap)
				{
					TextRectangle = new RectangleF(0, 0, wrapBoundaryWidth, 0);
				}

				// If the string is not the same as what we've stored, then temporarily
				// replace that string with the one being passed in and use that to
				// gauge size.  Otherwise, use the original size.
				if (!string.Equals(text, Text, StringComparison.CurrentCulture))
				{
					Text = text;
				}

				if (_needsTextUpdate)
				{
					FormatText();
				}

				return Size;
			}
			finally
			{
				_wordWrap = lastWordWrap;
				_textRect = lastRect;
				Text = previousString;
			}
		}

		/// <summary>
		/// Function to force an update to the renderable object.
		/// </summary>
		/// <remarks>
		/// Take care when calling this method repeatedly.  It will have a significant performance impact.
		/// </remarks>
		public void Refresh()
		{
			_needsTextUpdate = true;
			_needsVertexUpdate = true;
			_needsColorUpdate = true;
			if (_shadowEnabled)
			{
				_needsShadowUpdate = true;
			}
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="Gorgon.Renderers.Gorgon2D.Render">Render</see> is called.
		/// <para>
		/// If texture filtering is turned on, then the text will not look quite right because of sub-pixel accuracy.  In order to ensure the 
		/// vertical spacing is aligned with a pixel we need to ensure point filtering is turned on.
		/// </para>
		/// </remarks>
		public void Draw()
		{
			Rectangle clipRegion = Rectangle.Round(ClipRegion);
			Rectangle? lastClip = Gorgon2D.ClipRegion;
			int vertexIndex = 0;

			if (_needsTextUpdate)
			{
				FormatText();
			}

			// We don't need to draw anything if we don't have a font or text to draw.
			if ((_text.Length == 0) || (_font == null))
			{
				return;
			}

			if ((ClipToRectangle) &&
			    (((lastClip == null) && (_textRect != null)) || ((lastClip != null) && (lastClip.Value != ClipRegion))))
			{
				Gorgon2D.ClipRegion = clipRegion;
			}

			StateChange states = Gorgon2D.DefaultState.Compare(this);
			if (states != StateChange.None)
			{
				Gorgon2D.Flush();
				Gorgon2D.DefaultState.UpdateState(this, states);

                // Always set the cache as enabled when drawing from here, we know that
                // text always uses the cache.
			    _vertexCache.Enabled = true;
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

			if ((_shadowEnabled) && (_needsShadowUpdate))
			{
				UpdateShadowAlpha();
				_needsShadowUpdate = false;
			}

            GorgonGlyph glyph;

			if (_shadowEnabled)
			{
			    // ReSharper disable once ForCanBeConvertedToForeach
				for (int i = 0; i < _text.Length; i++)
				{
					char c = _text[i];

					switch (c)
					{
						case '\n':
						case '\t':
						case ' ':
						case '\r':
							continue;
					}

                    if (!_font.Glyphs.TryGetValue(c, out glyph))
                    {
                        c = _font.Settings.DefaultCharacter;
                        _font.Glyphs.TryGetValue(c, out glyph);
                    }

				    // Change to the current texture.
				    if (Gorgon2D.PixelShader.Resources[0] != GorgonTexture.ToShaderView(glyph.Texture))
				    {
				        Gorgon2D.Flush();
				        Gorgon2D.PixelShader.Resources[0] = glyph.Texture;
				    }

				    // Add shadowed characters.
                    _vertexCache.AddVertices(_vertices, 0, 6, vertexIndex, 4);
				    vertexIndex += 8;
				}

				vertexIndex = 4;
			}

		    // ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _text.Length; i++)
			{
				char c = _text[i];

				switch (c)
				{
					case '\n':
					case '\t':
					case ' ':
					case '\r':
						continue;
				}

                if (!_font.Glyphs.TryGetValue(c, out glyph))
                {
                    c = _font.Settings.DefaultCharacter;
                    _font.Glyphs.TryGetValue(c, out glyph);
                }

			    // Change to the current texture.
			    if (Gorgon2D.PixelShader.Resources[0] != GorgonTexture.ToShaderView(glyph.Texture))
			    {
			        Gorgon2D.Flush();
			        Gorgon2D.PixelShader.Resources[0] = glyph.Texture;
			    }

                _vertexCache.AddVertices(_vertices, 0, 6, vertexIndex, 4);
			    vertexIndex += _shadowEnabled ? 8 : 4;
			}

			if (ClipToRectangle)
			{
				Gorgon2D.ClipRegion = lastClip;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonText"/> class.
		/// </summary>
		/// <param name="gorgon2D">2D interface that created this object.</param>
		/// <param name="cache">The internal cache used by the renderer.</param>
		/// <param name="name">Name of the text object.</param>
		/// <param name="font">The font to use.</param>
		internal GorgonText(Gorgon2D gorgon2D, Gorgon2DVertexCache cache, string name, GorgonFont font)
			: base(name)
		{
			Gorgon2D = gorgon2D;

		    _vertexCache = cache;
			_text = string.Empty;
			_formattedText = new StringBuilder(256);
			_lines = new List<string>();
			if (font != null)
			{
				_font = font;
			    _font.FontChanged += FontChanged;
			}
			_shadowAlpha[3] = _shadowAlpha[2] = _shadowAlpha[1] = _shadowAlpha[0] = 0.25f;

			_depthStencil = new GorgonRenderable.DepthStencilStates();
			_blend = new GorgonRenderable.BlendState();
			_sampler = new GorgonRenderable.TextureSamplerState();
			_tabText = new StringBuilder("   ", 8);

			UpdateText();
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int IRenderable.VertexCount => _vertexCount;

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		/// <value></value>
		PrimitiveType IRenderable.PrimitiveType => PrimitiveType.TriangleList;

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		/// <value></value>
		/// <remarks>This is only matters when the renderable uses an index buffer.</remarks>
		int IRenderable.IndexCount => 6;

		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		/// <value></value>
		GorgonVertexBufferBinding IRenderable.VertexBufferBinding => Gorgon2D.DefaultVertexBufferBinding;

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		/// <value></value>
		GorgonIndexBuffer IRenderable.IndexBuffer => Gorgon2D.DefaultIndexBuffer;

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		/// <value></value>
		Gorgon2DVertex[] IRenderable.Vertices => _vertices;

		/// <summary>
		/// Property to return the number of vertices to add to the base starting index.
		/// </summary>
		/// <value></value>
		int IRenderable.BaseVertexCount => 0;

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
				{
					return;
				}

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
				{
					return;
				}

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
				{
					return;
				}

				_sampler = value;
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined smoothing states, to get more control over the smoothing, use the <see cref="GorgonRenderable.TextureSamplerState.TextureFilter">TextureFilter</see> 
		/// property exposed by the <see cref="GorgonRenderable.TextureSampler">TextureSampler</see> property.</remarks>
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
		/// <remarks>These modes are pre-defined blending states, to get more control over the blending, use the <see cref="GorgonRenderable.BlendState.SourceBlend">SourceBlend</see> 
		/// or the <see cref="GorgonRenderable.BlendState.DestinationBlend">DestinationBlend</see> property which are exposed by the 
		/// <see cref="P:Gorgon.Renderers.GorgonRenderable.Blending">Blending</see> property.</remarks>
		public BlendingMode BlendingMode
		{
			get
			{
				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.Zero))
				{
					return BlendingMode.None;
				}

				if (Blending.SourceBlend == BlendType.SourceAlpha)
				{
					if (Blending.DestinationBlend == BlendType.InverseSourceAlpha)
					{
						return BlendingMode.Modulate;
					}

					if (Blending.DestinationBlend == BlendType.One)
					{
						return BlendingMode.Additive;
					}
				}

				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
				{
					return BlendingMode.PreMultiplied;
				}

				if ((Blending.SourceBlend == BlendType.InverseDestinationColor) &&
				    (Blending.DestinationBlend == BlendType.InverseSourceColor))
				{
					return BlendingMode.Inverted;
				}

				return BlendingMode.Custom;
			}
			set
			{
				switch (value)
				{
					case BlendingMode.Additive:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.One;
						break;
					case BlendingMode.Inverted:
						Blending.SourceBlend = BlendType.InverseDestinationColor;
						Blending.DestinationBlend = BlendType.InverseSourceColor;
						break;
					case BlendingMode.Modulate:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case BlendingMode.PreMultiplied:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case BlendingMode.None:
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
		public GorgonRangeF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		[AnimatedProperty]
		public float Opacity
		{
			get
			{
				return Color.Alpha;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (value == Color.Alpha)
				{
					return;
				}

				_colors[3] = _colors[2] = _colors[1] = _colors[0] = new GorgonColor(Color, value);
				_needsColorUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		[AnimatedProperty]
		public GorgonColor Color
		{
			get
			{
				return _colors[0];
			}
			set
			{
				if (value == _colors[0])
				{
					return;
				}

				_colors[3] = _colors[2] = _colors[1] = _colors[0] = value;
				_needsColorUpdate = true;
			}
		}

		/// <summary>
		/// Property to return the texture being used by the renderable.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this property is made.</exception>
		GorgonTexture2D IRenderable.Texture
		{
			get
			{
				return _currentTexture;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the texture region.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this property is made.</exception>
		RectangleF IRenderable.TextureRegion
		{
			get
			{
				return _currentTexture == null ? RectangleF.Empty : new RectangleF(Vector2.Zero, _currentTexture.Settings.Size);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this property is made.</exception>
		Vector2 IRenderable.TextureOffset
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
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this property is made.</exception>
		Vector2 IRenderable.TextureSize
		{
			get
			{
				return _currentTexture != null ? _currentTexture.Settings.Size : Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region IMoveable Members
		/// <summary>
		/// Property to set or return the position of the renderable.
		/// </summary>
		[AnimatedProperty]
		public Vector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (_position == value)
				{
					return;
				}

				_position = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a renderable.
		/// </summary>
		[AnimatedProperty]
		public float Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_angle == value)
				{
					return;
				}

				_angle = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
		/// </summary>
		[AnimatedProperty]
		public Vector2 Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				if (_scale == value)
				{
					return;
				}

				_scale = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the renderable.
		/// </summary>
		[AnimatedProperty]
		public Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor == value)
				{
					return;
				}

				_anchor = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		[AnimatedProperty]
		public float Depth
		{
			get
			{
				return _depth;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_depth == value)
				{
					return;
				}

				_depth = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this property is made.</exception>
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

		#region I2DCollisionObject Members
		/// <summary>
		/// Property to set or return the collider that is assigned to the object.
		/// </summary>
		public Gorgon2DCollider Collider
		{
			get
			{
				return _collider;
			}
			set
			{
				if (value == _collider)
				{
					return;
				}

				if (value == null)
				{
					if (_collider != null)
					{
						_collider.CollisionObject = null;
					}

					return;
				}

				// Force a transform to get the latest vertices.
				value.CollisionObject = this;
				_collider = value;
				UpdateText();
				UpdateVertices();
			}
		}

		/// <summary>
		/// Property to return the number of vertices to process.
		/// </summary>
		int I2DCollisionObject.VertexCount => _colliderVertexCount;

		/// <summary>
		/// Property to return the list of vertices associated with the object.
		/// </summary>
		Gorgon2DVertex[] I2DCollisionObject.Vertices => _vertices;

		#endregion
	}
}