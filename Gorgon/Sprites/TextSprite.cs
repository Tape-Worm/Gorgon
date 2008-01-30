#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, July 24, 2006 5:26:35 PM
// 
#endregion

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a sprite containing text.
	/// </summary>
	public class TextSprite
		: Renderable
	{
		#region Variables.
		private Font _font;												// Font to use.
		private StringBuilder _text;									// Text to use.
		private StringBuilder _originalText;							// Original text.
		private StringCollection _lines;								// Lines of text.
		private int[] _vertexColors;									// Vertex colors.
		private bool _colorUpdated;										// Flag to indicate that the color was updated.
		private Alignment _alignment;									// Text alignment.
		private bool _wordWrap;											// Flag to enable or disable word wrapping.
		private Viewport _clipping;										// Clipping window.
		private bool _autoAdjustCRLF;									// Flag to indicate that CR/LF characters need to be translated to CR.
		private int _tabSpaces = 3;										// Number of spaces in a tab.
		private Vector2D _size = Vector2D.Zero;							// Size of the text sprite.
		private bool _shadowed = false;									// Flag to indicate that the text is shadowed.
		private Drawing.Color _shadowColor;								// Shadow color.
		private FontShadowDirection _shadowDir;							// Shadow direction.
		private Vector2D _shadowOffset;									// Shadow offset.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to show a shadow under the text.
		/// </summary>
		public bool Shadowed
		{
			get
			{
				return _shadowed;
			}
			set
			{
				_shadowed = value;				
			}
		}

		/// <summary>
		/// Property to set or return the color of the shadow.
		/// </summary>
		public Drawing.Color ShadowColor
		{
			get
			{
				return _shadowColor;
			}
			set
			{
				_shadowColor = value;
			}
		}

		/// <summary>
		/// Property to set or return the direction of the shadow.
		/// </summary>
		public FontShadowDirection ShadowDirection
		{
			get
			{
				return _shadowDir;
			}
			set
			{
				_shadowDir = value;
			}
		}

		/// <summary>
		/// Property to set or return the offset between the shadow and the text.
		/// </summary>
		public Vector2D ShadowOffset
		{
			get
			{
				return _shadowOffset;
			}
			set
			{
				if (value.X < 1.0f)
					value.X = 1.0f;
				if (value.Y < 1.0f)
					value.Y = 1.0f;

				_shadowOffset = value;
			}
		}

		/// <summary>
		/// Property to set or return the number of spaces in a tab.
		/// </summary>
		public int TabSpaces
		{
			get
			{
				return _tabSpaces;
			}
			set
			{
				_tabSpaces = value;

				IsSizeUpdated = true;
				IsImageUpdated = true;
				_colorUpdated = true;
				UpdateAABB();									
			}
		}

		/// <summary>
		/// Property to set or return whether this object will translate the CR/LF characters into a CR.
		/// </summary>
		public bool AutoAdjustCRLF
		{
			get
			{
				return _autoAdjustCRLF;
			}
			set
			{
				if (_autoAdjustCRLF != value)
				{
					_autoAdjustCRLF = value;
					FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);

					UpdateAABB();
					IsSizeUpdated = true;
					IsImageUpdated = true;

					if (Children.Count > 0)
						((IRenderable)this).UpdateChildren();
				}
			}
		}

		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		public override float UniformScale
		{
			get
			{
				return Scale.X;
			}
			set
			{
				if (value == 0.0f)
					return;

				// Set the uniform scale.
				Scale = new Vector2D(value, value);
			}
		}

		/// <summary>
		/// Property to set or return whether word wrapping is enabled or disabled.
		/// </summary>
		public bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				if (_wordWrap != value)
				{
					_wordWrap = value;
					FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);

					UpdateAABB();
					IsSizeUpdated = true;
					IsImageUpdated = true;

					if (Children.Count > 0)
						((IRenderable)this).UpdateChildren();
				}
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		public override PrimitiveStyle PrimitiveStyle
		{
			get
			{
				return PrimitiveStyle.TriangleList;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		public override bool UseIndices
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to set or return the horizontal alignment of the text.
		/// </summary>
		public Alignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				if (value != _alignment)
				{
					_alignment = value;

					FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);

					UpdateAABB();
					IsSizeUpdated = true;
					IsImageUpdated = true;

					if (Children.Count > 0)
						((IRenderable)this).UpdateChildren();
				}
			}
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		public override Vector2D Axis
		{
			get
			{
				return base.Axis;
			}
			set
			{
				base.Axis = value;
				IsSizeUpdated = true;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public override Vector2D Scale
		{
			get
			{
				return base.Scale;
			}
			set
			{
				NotifyParent();
				base.Scale = value;

                if (Children.Count > 0)
					((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return the sprite color.
		/// </summary>
		public override Drawing.Color Color
		{
			get
			{
				return Drawing.Color.FromArgb(_vertexColors[0]);
			}
			set
			{
				for (int i = 0; i < 4; i++)
					_vertexColors[i] = value.ToArgb();
				_colorUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		public override byte Opacity
		{
			get
			{
				return (byte)((_vertexColors[0] >> 24) & 0xFF);
			}
			set
			{
				_vertexColors[0] = ((int)value << 24) | (_vertexColors[0] & 0xFFFFFF);
				_vertexColors[1] = ((int)value << 24) | (_vertexColors[1] & 0xFFFFFF);
				_vertexColors[2] = ((int)value << 24) | (_vertexColors[2] & 0xFFFFFF);
				_vertexColors[3] = ((int)value << 24) | (_vertexColors[3] & 0xFFFFFF);
			}
		}


		/// <summary>
		/// Property to set or return the text for the buffer.
		/// </summary>
		public virtual string Text
		{
			get
			{				
				return _originalText.ToString();
			}
			set
			{
				_originalText.Length = 0;
				if ((value != null) && (value.Length != 0))
					_originalText.Append(value);

				FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);
				UpdateAABB();
				IsSizeUpdated = true;
				IsImageUpdated = true;

                if (Children.Count > 0)
					((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to return the size of the text.
		/// </summary>
		public override Vector2D Size
		{
			get
			{
				return _size;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to return the width of the text.
		/// </summary>
		public override float Width
		{
			get
			{
				// If we don't have a measurement, try and get one.
				if (MathUtility.EqualFloat(_size.X, 0.0f))
					UpdateAABB();
				return _size.X;
			}
			set
			{
			}
		}
		
		/// <summary>
		/// Property to set or return the height of the text.
		/// </summary>
		public override float Height
		{
			get
			{
				// If we don't have a measurement, try and get one.
				if (MathUtility.EqualFloat(_size.Y, 0.0f))
					UpdateAABB();
				return _size.Y;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return the font.
		/// </summary>
		public virtual Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();

				if (_font == value)
					return;

				// Re-assign the base image.
				if (value.NeedsUpdate)
					value.Refresh();
				base.Image = value.GetGlyph(' ').GlyphImage;
				_font = value;
				IsSizeUpdated = true;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		public override Image Image
		{
			get
			{
				return base.Image;
			}
			set
			{
				// Do nothing.  This is read-only.
				throw new InvalidOperationException("The text sprite image property is read-only.");
			}
		}

		/// <summary>
		/// Property to return the length of the text.
		/// </summary>
		public int TextLength
		{
			get
			{
				return _text.Length;
			}
		}

		/// <summary>
		/// Property to set or return the bounding rectangle for this object.
		/// </summary>
		public virtual Viewport Bounds
		{
			get
			{
				if ((_clipping == null) && (Gorgon.CurrentRenderTarget != null))
					return Gorgon.CurrentRenderTarget.DefaultView;

				return _clipping;
			}
			set
			{
				_clipping = value;
			}
		}

		/// <summary>
		/// Property to return whether bounds have been applied to the text sprite.
		/// </summary>
		public bool HasBounds
		{
			get
			{
				return !(_clipping == null);
			}
		}

		/// <summary>
		/// Property to return the number of lines for the text.
		/// </summary>
		public int LineCount
		{
			get
			{
				return _lines.Count;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to measure the length of the line in pixels.
        /// </summary>
        /// <param name="line">Line to read.</param>
        /// <returns>The length of the line in pixels.</returns>
        private float MeasureLine(string line)
        {
            float length = 0.0f;						// Length of the line.
            Glyph glyph;								// Glyph info.
            float startX = 0.0f;						// Start position.
            float endX = 0.0f;							// End position.

            if (_font.NeedsUpdate)
                _font.Refresh();

            // Calculate width.
            for (int i = 0; i < line.Length; i++)
            {
                glyph = _font.GetGlyph(line[i]);

                if (line[i] != '\t')
                {
                    endX = startX + glyph.GlyphDimensions.Width;
                    startX += glyph.Size.Width;
                }
                else
                {
                    endX = (startX + glyph.GlyphDimensions.Width) * _tabSpaces;
                    startX += (glyph.Size.Width * _tabSpaces);
                }

                length = endX;
            }

            return length;
        }
        
        /// <summary>
		/// Property to return post-formatted text.
		/// </summary>
		/// <param name="formattedOutput">String builder that will contain the formatted text.</param>
		/// <param name="lines">A collection of formatted lines from the text.</param>
		/// <param name="text">Text to format.</param>
		/// <param name="wordWrap">TRUE to enforce word wrapping, FALSE to leave as is.</param>
		/// <param name="bounds">Boundaries for word wrapping.</param>
		private void FormatText(StringBuilder formattedOutput, StringCollection lines, string text, bool wordWrap, Drawing.RectangleF bounds)
		{
			int previousLength = 0;										// Previous text length.

			// Reformat carriage returns and linefeeds.
			if (_autoAdjustCRLF)
			{
				text = text.Replace("\r\n", "\n");
				text = text.Replace("\n\r", "\n");
			}
						
			previousLength = formattedOutput.Length;
			
			formattedOutput.Length = 0;
			if (wordWrap)
				formattedOutput.Append(WordWrapText(text, bounds));
			else
				formattedOutput.Append(text);

			if (formattedOutput.Length > previousLength)
				_colorUpdated = true;

			// Get the lines.
			lines.Clear();
			lines.AddRange(formattedOutput.ToString().Split('\n'));
			for (int i = 0; i < lines.Count; i++)
			{
				lines[i] = lines[i].Replace("\r", string.Empty);
				lines[i] = lines[i].Replace("\n", string.Empty);
			}
		}

		/// <summary>
		/// Function to reformat the text for word wrapping.
		/// </summary>
		/// <param name="text">Text to wrap.</param>
		/// <param name="bounds">Boundaries to wrap at.</param>
		/// <returns>A string containing the re-formatted text.</returns>
		private string WordWrapText(string text, Drawing.RectangleF bounds)
		{
			StringBuilder newText = null;			// New text.
			Glyph charInfo;							// Character information.
			char character = ' ';					// Character.
			bool found = false;						// Found previous space.
			float pos = 0.0f;						// Position.
			int i = 0;								// Loop.
			

			if (text.Length < 1)
				return string.Empty;

			if (bounds.Width < 1)
				return text;

			// Re-word wrap.
			newText = new StringBuilder(text);

			while (i < newText.Length)
			{
				character = newText[i];
				charInfo = _font.GetGlyph(character);

				if ((character != '\r') && (character != '\n'))
				{
					if (character != '\t')
						pos += charInfo.Size.Width;
					else
						pos += charInfo.Size.Width * _tabSpaces;

					if (pos > bounds.Width)
					{
						// Try to find the previous space and replace it with a return.
						// Otherwise just insert it at the previous character.
						for (int j = i; j >= 0; j--)
						{
							if ((newText[j] == '\r') || (newText[j] == '\n'))
								break;

							if ((newText[j] == ' ') || (newText[j] == '\t'))
							{
								newText[j] = '\n';
								i = j;
								found = true;
								break;
							}
						}

						if (!found)
						{
							if (i > 0)
								newText.Insert(i,'\n');
						}

						pos = 0;
						found = false;
					}
				}
				else
					pos = 0;
				i++;
			}

			return newText.ToString();
		}

		/// <summary>
		/// Function to retrieve a line of text.
		/// </summary>
		/// <param name="line">Line index to for the text to retrieve.</param>
		/// <returns>The string at the line index.</returns>
		public string GetLine(int line)
		{
			if ((line < 0) || (line >= _lines.Count))
				throw new ArgumentOutOfRangeException("line");

			return _lines[line];
		}

		/// <summary>
		/// Function to measure the length of the line in pixels.
		/// </summary>
		/// <param name="line">Line index to read.</param>
		/// <returns>The length of the line in pixels.</returns>
		public float MeasureLine(int line)
		{
            return MeasureLine(GetLine(line));
		}

		/// <summary>
		/// Function to update the colors of the vertices.
		/// </summary>
		/// <param name="charIndex">Vertex index of the character quad to update.</param>
		private void UpdateColor(int charIndex)
		{
			Vertices[charIndex].Color = _vertexColors[(int)VertexLocations.UpperLeft];
			Vertices[charIndex + 1].Color = _vertexColors[(int)VertexLocations.UpperRight];
			Vertices[charIndex + 2].Color = _vertexColors[(int)VertexLocations.LowerRight];
			Vertices[charIndex + 3].Color = _vertexColors[(int)VertexLocations.LowerLeft];
		}

		/// <summary>
		/// Function to retrieve the aligned extents of the text sprite.
		/// </summary>
		/// <param name="x1">First horizontal source coordinate.</param>
		/// <param name="y1">First vertical source coordinate.</param>
		/// <param name="x2">Second horizontal source coordinate.</param>
		/// <param name="y2">Second vertical source coordinate.</param>
		/// <param name="bounds">Boundaries to respect.</param>
		/// <param name="maxLineSize">Largest line size.</param>
		/// <returns>New origin position for the sprite.</returns>
		private void GetAlignedExtents(ref float x1, ref float y1, ref float x2, ref float y2, Drawing.RectangleF bounds, float maxLineSize)
		{
			float calc = 0.0f;		// Calculation.

			switch (_alignment)
			{
				case Alignment.UpperCenter:
					calc = ((Bounds.Width / 2.0f) - (maxLineSize / 2.0f));
					x1 += calc;
					x2 += calc;
					break;
				case Alignment.UpperRight:
					calc = (Bounds.Width - maxLineSize);
					x1 += calc;
					x2 += calc;
					break;
				case Alignment.Left:
					calc = ((Bounds.Height / 2.0f) - (bounds.Height / 2.0f));
					y1 += calc;
					y2 += calc;
					break;
				case Alignment.Right:
					calc = (Bounds.Width - maxLineSize);
					x1 += calc;
					x2 += calc;
					calc = ((Bounds.Height / 2.0f) - (bounds.Height / 2.0f));
					y1 += calc;
					y2 += calc;
					break;
				case Alignment.Center:
					calc = ((Bounds.Width / 2.0f) - (maxLineSize / 2.0f));
					x1 += calc;
					x2 += calc;
					calc = ((Bounds.Height / 2.0f) - (bounds.Height / 2.0f));
					y1 += calc;
					y2 += calc;
					break;
				case Alignment.LowerLeft:
					calc = (Bounds.Height - bounds.Height);
					y1 += calc;
					y2 += calc;
					break;
				case Alignment.LowerCenter:
					calc = ((Bounds.Width / 2.0f) - (maxLineSize / 2.0f));
					x1 += calc;
					x2 += calc;
					calc = (Bounds.Height - bounds.Height);
					y1 += calc;
					y2 += calc;
					break;
				case Alignment.LowerRight:
					calc = (Bounds.Width - maxLineSize);
					x1 += calc;
					x2 += calc;
					calc = (Bounds.Height - bounds.Height);
					y1 += calc;
					y2 += calc;
					break;
			}
		}

		/// <summary>
		/// Function to update the transformation of the vertices.
		/// </summary>
		/// <param name="bounds">Boundaries for the text.</param>
		/// <param name="charIndex">Vertex index of the character quad to update.</param>		
		/// <param name="character">Character to use.</param>
		/// <param name="lineLength">Length of the current line.</param>
		/// <param name="characterInfo">Font information about the character.</param>
		/// <param name="calcAABB">TRUE to calculate AABB extents, FALSE to ignore.</param>
		/// <param name="position">Position of the character.</param>
		/// <param name="aabbUpdate">Updated AABB.</param>
		/// <returns>TRUE to advance the relative position pointer, FALSE to leave where it is.</returns>
		private void UpdateTransform(Drawing.RectangleF bounds, int charIndex, char character, float lineLength, Glyph characterInfo, bool calcAABB, ref Vector2D position, ref Drawing.RectangleF aabbUpdate)
		{
			float posX1;							// Horizontal position 1.
			float posX2;							// Horizontal position 2.
			float posY1;							// Vertical position 1.
			float posY2;							// Vertical position 2.

			switch (character)
			{
				case '\r':
				case '\n':
					position.X = 0;
					position.Y += characterInfo.Size.Height;
					return;
				case ' ':
					position.X += characterInfo.Size.Width;
					return;
				case '\t':
					position.X += characterInfo.Size.Width * _tabSpaces;
					return;
			}

			// Get dimensions.
            posX1 = position.X - Axis.X;
			posX2 = position.X + characterInfo.GlyphDimensions.Width - Axis.X;
			posY1 = position.Y - Axis.Y;
			posY2 = position.Y + characterInfo.GlyphDimensions.Height - Axis.Y;

			// Adjust for text alignment.
			GetAlignedExtents(ref posX1, ref posY1, ref posX2, ref posY2, bounds, lineLength);

			// Scale the points.
			if (FinalScale.X != 1.0f)
			{
                posX1 *= FinalScale.X;
                posX2 *= FinalScale.X;
			}
			if (FinalScale.Y != 1.0f)
			{
                posY1 *= FinalScale.Y;
                posY2 *= FinalScale.Y;
			}

			// Set the Z position, although we don't really need it, it may fuck something up if we don't.
			Vertices[charIndex].Position.Z = -0.5f;
			Vertices[charIndex+1].Position.Z = -0.5f;
			Vertices[charIndex+2].Position.Z = -0.5f;
			Vertices[charIndex+3].Position.Z = -0.5f;

			// Set rotation.
			if (FinalRotation != 0.0f)
			{
				float cosVal;		// Cached cosine.
				float sinVal;		// Cached sine.
				float angle;		// Angle in radians.

			    angle = MathUtility.Radians(FinalRotation);
				cosVal = MathUtility.Cos(angle);
				sinVal = MathUtility.Sin(angle);

				// Rotate the vertices.
                Vertices[charIndex].Position.X = (posX1 * cosVal - posY1 * sinVal);
                Vertices[charIndex].Position.Y = (posX1 * sinVal + posY1 * cosVal);
                Vertices[charIndex + 1].Position.X = (posX2 * cosVal - posY1 * sinVal);
                Vertices[charIndex + 1].Position.Y = (posX2 * sinVal + posY1 * cosVal);
                Vertices[charIndex + 2].Position.X = (posX2 * cosVal - posY2 * sinVal);
                Vertices[charIndex + 2].Position.Y = (posX2 * sinVal + posY2 * cosVal);
                Vertices[charIndex + 3].Position.X = (posX1 * cosVal - posY2 * sinVal);
                Vertices[charIndex + 3].Position.Y = (posX1 * sinVal + posY2 * cosVal);
			}
			else
			{
				// Else just keep the positions.
				Vertices[charIndex].Position.X = posX1;
                Vertices[charIndex].Position.Y = posY1;
                Vertices[charIndex + 1].Position.X = posX2;
                Vertices[charIndex + 1].Position.Y = posY1;
                Vertices[charIndex + 2].Position.X = posX2;
                Vertices[charIndex + 2].Position.Y = posY2;
                Vertices[charIndex + 3].Position.X = posX1;
                Vertices[charIndex + 3].Position.Y = posY2;
			}

			// Translate if necessary.
			if (FinalPosition.X != 0.0)
			{
                Vertices[charIndex].Position.X += FinalPosition.X;
                Vertices[charIndex + 1].Position.X += FinalPosition.X;
                Vertices[charIndex + 2].Position.X += FinalPosition.X;
                Vertices[charIndex + 3].Position.X += FinalPosition.X;
			}

			if (FinalPosition.Y != 0.0)
			{
                Vertices[charIndex].Position.Y += FinalPosition.Y;
                Vertices[charIndex + 1].Position.Y += FinalPosition.Y;
                Vertices[charIndex + 2].Position.Y += FinalPosition.Y;
                Vertices[charIndex + 3].Position.Y += FinalPosition.Y;
			}

			if (calcAABB)
			{
				float minX = aabbUpdate.Left;		// Min x.
				float minY = aabbUpdate.Top;		// Min y.
				float maxX = aabbUpdate.Right;		// Max x.
				float maxY = aabbUpdate.Bottom;		// Max y.

				// Calculate AABB extents.
				for (int i = 0; i < 4; i++)
				{
					minX = MathUtility.Min(Vertices[charIndex + i].Position.X, minX);
					minY = MathUtility.Min(Vertices[charIndex + i].Position.Y, minY);
					maxX = MathUtility.Max(Vertices[charIndex + i].Position.X, maxX);
					maxY = MathUtility.Max(Vertices[charIndex + i].Position.Y, maxY);
				}

				aabbUpdate = new Drawing.RectangleF(minX, minY, maxX - minX, maxY - minY);
			}
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		/// <param name="charIndex">Vertex index of the character quad to update.</param>
		/// <param name="glyphInfo">Glyph information.</param>
		private void UpdateTextureCoordinates(int charIndex, Glyph glyphInfo)
		{
			Vertices[charIndex].TextureCoordinates = new Vector2D(glyphInfo.ImageDimensions.Left, glyphInfo.ImageDimensions.Top);
			Vertices[charIndex + 1].TextureCoordinates = new Vector2D(glyphInfo.ImageDimensions.Right, glyphInfo.ImageDimensions.Top);
			Vertices[charIndex + 2].TextureCoordinates = new Vector2D(glyphInfo.ImageDimensions.Right, glyphInfo.ImageDimensions.Bottom);
			Vertices[charIndex + 3].TextureCoordinates = new Vector2D(glyphInfo.ImageDimensions.Left, glyphInfo.ImageDimensions.Bottom);
		}

		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			int vertexCount = 0;				// Number of vertices.
			string text = string.Empty;			// Text to update.
			char character = ' ';				// Character.
			Glyph charInfo;						// Font character information.
			Vector2D position = Vector2D.Zero;	// Position.
			int vertexIndex = 0;				// Vertex index.
			Drawing.RectangleF bounds;			// Boundaries.
			float adjustment = 0.0f;			// Adjustment position.
			Drawing.RectangleF aabbUpdate;		// Updated AABB.
			bool calcAABB;						// Flag to calculate AABB.
			
			if (_font.NeedsUpdate)
				_font.Refresh();

			// Get the untransformed AABB.
			if ((MathUtility.EqualFloat(_size.X, 0.0f)) || (MathUtility.EqualFloat(_size.Y, 0.0f)))
				UpdateAABB();

			bounds = new System.Drawing.RectangleF(0, 0, _size.X, _size.Y); 

			// Prepare to update the AABB.
			calcAABB = true;
			aabbUpdate = Drawing.RectangleF.Empty;

			// Calculate the number of vertices.
			vertexCount = _text.Length * 8;

			// If no vertices, do nothing.
			if (vertexCount == 0)
			{
				InitializeVertices(0);
				return;
			}

			// Create a new set of vertices.
			if ((Vertices == null) || (vertexCount > Vertices.Length))
			{
				InitializeVertices(vertexCount);
				IsImageUpdated = true;
				_colorUpdated = true;
			}

			position = Vector2D.Zero;
			vertexIndex = 0;

			// Don't calculate AABB unless we absolutely need it.
			if ((MathUtility.EqualFloat(FinalRotation, 0.0f)) && (MathUtility.EqualFloat(FinalScale.X, 1.0f)) && (MathUtility.EqualFloat(FinalScale.Y, 1.0f)))
			{
				aabbUpdate = new Drawing.RectangleF(FinalPosition.X, FinalPosition.Y, _size.X, _size.Y);
				calcAABB = false;
			}

			// Go through each line.
			for (int i = 0; i < _lines.Count; i++)
			{
				string line = _lines[i];		// Line.

				adjustment = MeasureLine(i);

				// Go through each character.
				for (int j = 0; j < line.Length; j++)
				{
					character = line[j];
					charInfo = _font.GetGlyph(character);

					// We need to space it out if we hit a tab.
					switch(character)
					{
						case '\t':
							position.X += charInfo.Size.Width * _tabSpaces;
							break;
						case ' ':
							position.X += charInfo.Size.Width;
							break;
						default:
							UpdateTransform(bounds, vertexIndex, character, adjustment, charInfo, calcAABB, ref position, ref aabbUpdate);
							position.X += charInfo.Size.Width;

							UpdateTextureCoordinates(vertexIndex, charInfo);

							if (_colorUpdated)
								UpdateColor(vertexIndex);


							vertexIndex += 4;
							break;
					}						
				}

				// Wrap to next line.
				position.Y += _font.GetGlyph(' ').Size.Height;
				position.X = 0;
			}

			// Set the new AABB.
			SetAABB(aabbUpdate);
			_colorUpdated = false;
			IsSizeUpdated = false;			
		}

		/// <summary>
		/// Function to append text to the already existing text.
		/// </summary>
		/// <param name="text">Text to append.</param>
		public virtual void AppendText(string text)
		{
			if (text.Length > 0)
			{
				_originalText.Append(text);

				FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);
				UpdateAABB();

				IsSizeUpdated = true;
				IsImageUpdated = true;
				_colorUpdated = true;

                if (Children.Count > 0)
					((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Function to insert text into the already existing text.
		/// </summary>
		/// <param name="position">Position to insert at.</param>
		/// <param name="text">Text to insert.</param>
		public virtual void InsertText(int position, string text)
		{
			if ((position < 0) || ((position + text.Length) > _text.Length))
				return;

			if (text.Length > 0)
			{
				_originalText.Insert(position, text);

				FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);
				UpdateAABB();

				IsSizeUpdated = true;
				IsImageUpdated = true;
				_colorUpdated = true;
			}
		}

		/// <summary>
		/// Function to get the color of a character vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <returns>Color of the sprite vertex.</returns>
		public Drawing.Color GetCharacterVertexColor(VertexLocations vertexPosition)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new ArgumentOutOfRangeException("vertexPosition", "Characters do not contain a vertex at the position " + ((int)vertexPosition).ToString());

			return Drawing.Color.FromArgb(_vertexColors[(int)vertexPosition]);
		}

		/// <summary>
		/// Function to set the color of a character vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="newColor">New color to set the vertex to.</param>
		public void SetCharacterVertexColor(VertexLocations vertexPosition, Drawing.Color newColor)
		{
			if ((vertexPosition < VertexLocations.UpperLeft) || (vertexPosition > VertexLocations.LowerLeft))
				throw new ArgumentOutOfRangeException("vertexPosition", "Characters do not contain a vertex at the position " + ((int)vertexPosition).ToString());

			_vertexColors[(int)vertexPosition] = newColor.ToArgb();
			_colorUpdated = true;
		}
		
		/// <summary>
		/// Function to remove text from the already existing text.
		/// </summary>
		/// <param name="position">Position to start removing.</param>
		/// <param name="length">Number of characters to remove.</param>
		public virtual void RemoveText(int position, int length)
		{
			if (((position + length) >= _text.Length) || (position < 0))
				return;

			_originalText.Remove(position, length);

			FormatText(_text, _lines, _originalText.ToString(), _wordWrap, Bounds.Dimensions);
			UpdateAABB();

			IsSizeUpdated = true;
			IsImageUpdated = true;
		}

		/// <summary>
		/// Function to return the optimal dimensions of the specified text.
		/// </summary>
		/// <param name="text">Text to examine.</param>
		/// <param name="desiredWidth">Ideal width of the rectangle, only applies of wrap is set to TRUE.</param>
		/// <param name="wordWrap">TRUE to word wrap text at the desired rectangle width, FALSE to let it continue.</param>
		/// <returns>Rectangle that will contain the text.</returns>
		public Drawing.RectangleF MeasureText(StringBuilder text, float desiredWidth, bool wordWrap)
		{
			Drawing.RectangleF result = Drawing.RectangleF.Empty;			// Resulting rectangle.
			float maxWidth = 0.0f;											// Maximum width.
			float maxHeight = 0.0f;											// Maximum height.
            StringCollection lines = null;                                  // Lines for the text.

			if ((_font == null) || (text.Length == 0))
				return Drawing.RectangleF.Empty;

			if (_font.NeedsUpdate)
				_font.Refresh();

			// Turn off wrapping if the desired rectangle is empty.
			if (desiredWidth < 0.0f)
				wordWrap = false;

			if (!wordWrap)
				desiredWidth = 0.0f;

			// Check for the maximum width and height.
            // Get the lines.            
            if (!text.Equals(_text))
            {
				lines = new StringCollection();
				FormatText(text, lines, text.ToString(), wordWrap, new Drawing.RectangleF(0, 0, desiredWidth, float.MaxValue - 1));
            }
            else
                lines = _lines;

			maxHeight = _font.CharacterHeight * lines.Count;
			for (int i = 0; i < lines.Count; i++)
			{
                if (text.Equals(_text))
				    maxWidth = MathUtility.Max(maxWidth, MeasureLine(i));
                else
                    maxWidth = MathUtility.Max(maxWidth, MeasureLine(lines[i]));
				if ((maxWidth > desiredWidth) && (desiredWidth != 0.0f))
					maxWidth = desiredWidth;
			}

			return new Drawing.RectangleF(0, 0, maxWidth, maxHeight);
		}

		/// <summary>
		/// Function to update the AABB for the text.
		/// </summary>
		public override void UpdateAABB()
		{
			base.UpdateAABB();
			_size = MeasureText(_text, Bounds.Dimensions.Width, _wordWrap).Size;			
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public override void Draw(bool flush)
		{
			int vertexIndex = 0;				// Current vertex index.
			Vector2D offset = Vector2D.Zero;	// Shadow offset.
			Glyph glyphData;					// Glyph data.

			BeginRendering(flush);

			// Update the data.
			UpdateDimensions();

			// Don't draw if we don't need to.
			if ((_text.Length == 0) || (Vertices == null))
				return;

			// Draw the shadow.
			if (_shadowed)
			{
				switch (_shadowDir)
				{
					case FontShadowDirection.LowerLeft:						
						offset = new Vector2D(-_shadowOffset.X, _shadowOffset.Y);
						break;
					case FontShadowDirection.LowerMiddle:
						offset = new Vector2D(0.0f, _shadowOffset.Y);
						break;
					case FontShadowDirection.LowerRight:
						offset = _shadowOffset;
						break;
					case FontShadowDirection.UpperLeft:
						offset = new Vector2D(-_shadowOffset.X, -_shadowOffset.Y);
						break;
					case FontShadowDirection.UpperMiddle:
						offset = new Vector2D(0.0f, -_shadowOffset.Y);
						break;
					case FontShadowDirection.UpperRight:
						offset = new Vector2D(_shadowOffset.X, -_shadowOffset.Y);
						break;
					case FontShadowDirection.MiddleLeft:
						offset = new Vector2D(-_shadowOffset.X, 0.0f);
						break;
					case FontShadowDirection.MiddleRight:
						offset = new Vector2D(_shadowOffset.X, 0.0f);
						break;
				}

				for (int i = 0; i < _text.Length; i++)
				{					
					// Write the data to the buffer.			
					if ((_text[i] != '\r') && (_text[i] != '\n') && (_text[i] != '\t') && (_text[i] != ' '))
					{
						glyphData = Font.GetGlyph(_text[i]);

						if (!IsImageCurrent(glyphData.GlyphImage))
						{
							FlushToRenderer();
							SetCurrentImage(glyphData.GlyphImage);
						}

						// Set shadow parameters.
						for (int j = 0; j < 4; j++)
						{
							Vertices[vertexIndex + j].Position.X += offset.X;
							Vertices[vertexIndex + j].Position.Y += offset.Y;
							Vertices[vertexIndex + j].Color = _shadowColor.ToArgb();
						}

						if (WriteVertexData(vertexIndex, 4) + 4 >= BufferSize)
							FlushToRenderer();

						// Reset.
						for (int j = 0; j < 4; j++)
						{
							Vertices[vertexIndex + j].Position.X -= offset.X;
							Vertices[vertexIndex + j].Position.Y -= offset.Y;
							Vertices[vertexIndex + j].Color = _vertexColors[j];
						}

						vertexIndex += 4;
					}
				}

				// Reset.
				vertexIndex = 0;
			}

			// Send to the buffers.			
			for (int i = 0; i < _text.Length; i++)
			{
				// Write the data to the buffer.			
				if ((_text[i] != '\r') && (_text[i] != '\n') && (_text[i] != '\t') && (_text[i] != ' '))
				{
					glyphData = Font.GetGlyph(_text[i]);
					if (!IsImageCurrent(glyphData.GlyphImage))
					{
						FlushToRenderer();
						SetCurrentImage(glyphData.GlyphImage);
					}

					if (WriteVertexData(vertexIndex, 4) + 4 >= BufferSize)
						FlushToRenderer();

					vertexIndex += 4;
				}
			}
			
			_colorUpdated = false;
			IsImageUpdated = false;

			EndRendering(flush);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override object Clone()
		{
			TextSprite clone = new TextSprite(_objectName + ".Clone", _text.ToString(), _font);		// Create clone.

			for (int i = 0; i < Vertices.Length; i++)
				clone.Vertices[i] = Vertices[i];

			((IRenderable)clone).SetParent(Parent);
			clone.Size = Size;
			clone.Position = Position;
			clone.Rotation = Rotation;
			clone.Scale = Scale;
			clone.Axis = Axis;
			clone.SetAABB(AABB);
			clone.ParentPosition = ParentPosition;
			clone.ParentRotation = ParentRotation;
			clone.ParentScale = ParentScale;

			if (!InheritSmoothing)
				clone.Smoothing = Smoothing;
			if (!InheritBlending)
			{
				clone.BlendingMode = BlendingMode;
				clone.SourceBlend = SourceBlend;
				clone.DestinationBlend = DestinationBlend;
			}
			if (!InheritAlphaMaskFunction)
				clone.AlphaMaskFunction = AlphaMaskFunction;
			if (!InheritAlphaMaskValue)
				clone.AlphaMaskValue = AlphaMaskValue;
			if (!InheritHorizontalWrapping)
				clone.HorizontalWrapMode = HorizontalWrapMode;
			if (!InheritVerticalWrapping)
				clone.VerticalWrapMode = VerticalWrapMode;
			if (!InheritStencilPassOperation)
				clone.StencilPassOperation = StencilPassOperation;
			if (!InheritStencilFailOperation)
				clone.StencilFailOperation = StencilFailOperation;
			if (!InheritStencilZFailOperation)
				clone.StencilZFailOperation = StencilZFailOperation;
			if (!InheritStencilCompare)
				clone.StencilCompare = StencilCompare;
			if (!InheritStencilEnabled)
				clone.StencilEnabled = StencilEnabled;
			if (!InheritStencilReference)
				clone.StencilReference = StencilReference;
			if (!InheritStencilMask)
				clone.StencilMask = StencilMask;

			// Clone the animations.
			Animations.CopyTo(clone.Animations);

			clone.Shader = Shader;
			clone.InheritScale = InheritScale;
			clone.InheritRotation = InheritRotation;
			clone.Refresh();

			return clone;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="position">Initial position of the text.</param>
		/// <param name="textColor">Color of the text.</param>
		public TextSprite(string name, string text, Font font, Vector2D position, Drawing.Color textColor)
			: base(name)
		{
			if (font == null)
				throw new ArgumentNullException("font");

			font.Refresh();
			base.Image = font.GetGlyph(' ').GlyphImage;
			_shadowed = false;
			_shadowDir = FontShadowDirection.LowerRight;
			_shadowColor = Drawing.Color.FromArgb(192, Drawing.Color.Black);
			_shadowOffset = new Vector2D(2.0f, 2.0f);
			_lines = new StringCollection();
			_clipping = null;
			_text = new StringBuilder(80);
			_originalText = new StringBuilder(80);
			_font = font;
			_vertexColors = new int[4];			
			Position = position;
			Scale = Vector2D.Unit;
			Axis = Vector2D.Zero;
			_alignment = Alignment.UpperLeft;
			if (_font.NeedsUpdate)
				_font.Refresh();
			
			// Update the string buffer.
			if ((text != null) && (text.Length != 0))
			{
				_originalText.Append(text);
				FormatText(_text, _lines, text, _wordWrap, Bounds.Dimensions);
			}

			IsSizeUpdated = true;
			_colorUpdated = true;

			// Set sprite colors.
			for (int i = 0; i < 4; i++)
				_vertexColors[i] = textColor.ToArgb();
			
			// Get size.
			base.UpdateAABB();
			SetAABB(MeasureText(_text, Gorgon.Screen.DefaultView.Dimensions.Width, _wordWrap));

			Size = new Vector2D(AABB.Width, AABB.Height);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="position">Initial position of the text.</param>
		public TextSprite(string name, string text, Font font, Vector2D position)
			: this(name, text, font, position, Drawing.Color.White)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		public TextSprite(string name, string text, Font font)
			: this(name, text, font, Vector2D.Zero, Drawing.Color.White)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="color">Color of the text.</param>
		public TextSprite(string name, string text, Font font, Drawing.Color color)
			: this(name, text, font, Vector2D.Zero, color)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="positionX">Horizontal position of the text.</param>
		/// <param name="positionY">Vertical position of the text.</param>
		/// <param name="textColor">Color of the</param>
		public TextSprite(string name, string text, Font font, float positionX, float positionY, Drawing.Color textColor)
			: this(name, text, font, new Vector2D(positionX, positionY), textColor)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="positionX">Horizontal position of the text.</param>
		/// <param name="positionY">Vertical position of the text.</param>		
		public TextSprite(string name, string text, Font font, float positionX, float positionY)
			: this(name, text, font, new Vector2D(positionX, positionY), Drawing.Color.White)
		{
		}
		#endregion
    }
}
