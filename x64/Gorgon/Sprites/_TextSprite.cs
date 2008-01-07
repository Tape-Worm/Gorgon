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
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Mathematics;
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
		private Font _font;							// Font to use.
		private StringBuilder _text;				// Text to use.
		private StringBuilder _originalText;		// Original text.
		private int[] _vertexColors;				// Vertex colors.
		private bool _colorUpdated;					// Flag to indicate that the color was updated.
		private Alignment _alignment;				// Text alignment.
		private bool _wordWrap;						// Flag to enable or disable word wrapping.
		private Viewport _clipping;					// Clipping window.
		private bool _autoAdjustCRLF;				// Flag to indicate that CR/LF characters need to be translated to CR.
		#endregion

		#region Properties.
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
					FormatText(_originalText.ToString());

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
					FormatText(_originalText.ToString());

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

					FormatText(_originalText.ToString());

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

				FormatText(_originalText.ToString());
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
				return new Vector2D(AABB.Width, AABB.Height);
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
				return AABB.Width;
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
				return AABB.Height;
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
				if (_font == value)
					return;
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
				return _font.FontImage;
			}
			set
			{
				// Do nothing.  This is read-only.
				throw new InvalidOperationException("The font image property is read-only.");
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
		#endregion

		#region Methods.
		/// <summary>
		/// Property to return post-formatted text.
		/// </summary>
		/// <param name="text">Text to format.</param>
		private void FormatText(string text)
		{
			Drawing.RectangleF bounds = Drawing.RectangleF.Empty;		// Boundaries.
			int previousLength = 0;										// Previous text length.

			bounds = Bounds.Dimensions;

			// Reformat carriage returns and linefeeds.
			if (_autoAdjustCRLF)
			{
				text = text.Replace("\r\n", "\n");
				text = text.Replace("\n\r", "\n");
			}

			previousLength = _text.Length;

			_text.Length = 0;
			if (_wordWrap)
				_text.Append(WordWrapText(text, bounds));
			else
				_text.Append(text);

			if (_text.Length > previousLength)
				_colorUpdated = true;
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
			Font.FontCharacter charInfo;			// Character information.
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
				charInfo = GetFontInfo(character);

				if ((character != '\r') && (character != '\n'))
				{
					if (character != '\t')
						pos += (int)charInfo.B + charInfo.C;
					else
						pos += (charInfo.A + (int)charInfo.B + charInfo.C) * 3.0f;

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
		/// Function to return the information for a given character from the font.
		/// </summary>
		/// <param name="character">Character to retrieve information for.</param>
		/// <returns>Font information for a character, or information for a space character.</returns>
		private Font.FontCharacter GetFontInfo(char character)
		{
			if (_font.Contains(character))
				return _font[character];
			else
				return _font[' '];
		}

		/// <summary>
		/// Function to perform a measurement on the text.
		/// </summary>
		/// <returns>Length of line in pixels.</returns>
		private float MeasureLine(int startIndex)
		{
			char character;						// Character.
			Font.FontCharacter charInfo;		// Character info.
			float length = 0;					// Length.

			if (startIndex >= _text.Length)
				return 0;

			// If a carriage return is the first character, then exit.
			if (((_text[startIndex] == '\r') || (_text[startIndex] == '\n')) && (startIndex == _text.Length - 1))
				return 0;

			// Loop through the line until a carriage return is hit, or the text ends.
			for (int i = startIndex; i < _text.Length; i++)
			{
				character = _text[i];
				if (_font.Contains(character))
					charInfo = _font[character];
				else
					charInfo = _font[' '];

				switch (character)
				{
					case '\r':
					case '\n':
						return length;
					case '\t':
						length += (float)(charInfo.A + (int)charInfo.B + charInfo.C) * 3.0f;
						break;
					default:
						length += (int)charInfo.B + charInfo.C;
						break;
				}
			}

			return length;
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
		/// Function to update the transformation of the vertices.
		/// </summary>
		/// <param name="bounds">Boundaries for the text.</param>
		/// <param name="charIndex">Vertex index of the character quad to update.</param>		
		/// <param name="character">Character to use.</param>
		/// <param name="lineLength">Length of the current line.</param>
		/// <param name="characterInfo">Font information about the character.</param>
		/// <param name="position">Position of the character.</param>
		/// <returns>TRUE to advance the relative position pointer, FALSE to leave where it is.</returns>
		private bool UpdateTransform(Drawing.RectangleF bounds, int charIndex, char character, float lineLength, Font.FontCharacter characterInfo, ref Vector2D position)
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
					position.Y += characterInfo.Height;

					if (_font.IsShadowed)
					{
						switch (_font.ShadowDirection)
						{
							case FontShadowDirection.UpperMiddle:
							case FontShadowDirection.UpperRight:
							case FontShadowDirection.UpperLeft:
							case FontShadowDirection.LowerMiddle:
							case FontShadowDirection.LowerRight:
							case FontShadowDirection.LowerLeft:
								position.Y -= _font.ShadowOffset;
								break;	
						}			
					}

					return false;
				case ' ':
					return true;
				case '\t':
					position.X += (characterInfo.A + (int)characterInfo.B + characterInfo.C) * 3.0f;
					return false;
			}

			// Get dimensions.
            posX1 = position.X + Axis.X;
			posX2 = position.X + characterInfo.B + Axis.X;
			posY1 = position.Y + Axis.Y;
			posY2 = position.Y + characterInfo.Height + Axis.Y;

			// Adjust for text alignment.
			switch (_alignment)
			{
				case Alignment.UpperCenter:
					posX1 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posX2 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					break;
				case Alignment.UpperRight:
					posX1 += (int)(bounds.Width - lineLength);
					posX2 += (int)(bounds.Width - lineLength);
					break;
				case Alignment.Left:
					posY1 += (int)((bounds.Height / 2.0f) - (AABB.Height / 2.0f));
					posY2 += (int)((bounds.Height / 2.0f) - (AABB.Height / 2.0f));
					break;
				case Alignment.Right:
					posY1 += (int)((bounds.Height / 2.0f) - (AABB.Height / 2.0f));
					posY2 += (int)((bounds.Height / 2.0f) - (AABB.Height / 2.0f));
					posX1 += (int)(bounds.Width - lineLength);
					posX2 += (int)(bounds.Width - lineLength);
					break;
				case Alignment.Center:
					posY1 += (int)((bounds.Height / 2.0f) - (AABB.Height / 2.0f));
					posY2 += (int)((bounds.Height / 2.0f) - (AABB.Height / 2.0f));
					posX1 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posX2 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					break;
				case Alignment.LowerLeft:
					posY1 += (int)(bounds.Height - AABB.Height);
					posY2 += (int)(bounds.Height - AABB.Height);
					break;
				case Alignment.LowerCenter:
					posX1 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posX2 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posY1 += (int)(bounds.Height - AABB.Height);
					posY2 += (int)(bounds.Height - AABB.Height);
					break;
				case Alignment.LowerRight:
					posX1 += (int)(bounds.Width - lineLength);
					posX2 += (int)(bounds.Width - lineLength);
					posY1 += (int)(bounds.Height - AABB.Height);
					posY2 += (int)(bounds.Height - AABB.Height);
					break;
			}

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
			return true;
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		/// <param name="charIndex">Vertex index of the character quad to update.</param>
		/// <param name="characterInfo">Font character information.</param>
		private void UpdateTextureCoordinates(int charIndex,Font.FontCharacter characterInfo)
		{
			Vertices[charIndex].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Left, characterInfo._textureCoordinates.Top);
			Vertices[charIndex + 1].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Right, characterInfo._textureCoordinates.Top);
			Vertices[charIndex + 2].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Right, characterInfo._textureCoordinates.Bottom);
			Vertices[charIndex + 3].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Left, characterInfo._textureCoordinates.Bottom);
		}

		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			int vertexCount = 0;				// Number of vertices.
			string text = string.Empty;			// Text to update.
			char character = ' ';				// Character.
			Font.FontCharacter charInfo;		// Font character information.
			Vector2D position = Vector2D.Zero;	// Position.
			int vertexIndex = 0;				// Vertex index.
			Drawing.RectangleF bounds;			// Boundaries.
			float adjustment = 0.0f;			// Adjustment position.

			bounds = Bounds.Dimensions;

			// Calculate the number of vertices.
			vertexCount = (_text.Length * 2) * 4;

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

			if (_alignment != Alignment.UpperLeft)
				adjustment = MeasureLine(0);

			// Go through each character.
			for (int i = 0; i < _text.Length; i++)
			{
				character = _text[i];
				if (_font.Contains(character))
					charInfo = _font[character];
				else
					charInfo = _font[' '];

                // Update the character.				
				if (UpdateTransform(bounds, vertexIndex, character, adjustment, charInfo, ref position))
					position.X += (int)charInfo.B + charInfo.C;
				else
				{
					if (_alignment != Alignment.UpperLeft)
						adjustment = MeasureLine(i + 1);
				}
                
				UpdateTextureCoordinates(vertexIndex, charInfo);
				if (_colorUpdated)
					UpdateColor(vertexIndex);

				if ((character != '\r') && (character != '\n') && (character != '\t') && (character != ' '))
					vertexIndex += 4;
			}

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

				FormatText(_originalText.ToString());
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

				FormatText(_originalText.ToString());
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

			FormatText(_originalText.ToString());
			UpdateAABB();

			IsSizeUpdated = true;
			IsImageUpdated = true;
		}

		/// <summary>
		/// Function to return the optimal dimensions of the specified text.
		/// </summary>
		/// <param name="text">Text to examine.</param>
		/// <param name="desiredWidth">Ideal width of the rectangle, only applies of wrap is set to TRUE.</param>
		/// <param name="wrap">TRUE to wrap text at the desired rectangle width, FALSE to let it continue.</param>
		/// <returns>Rectangle that will contain the text.</returns>
		public Drawing.RectangleF MeasureText(StringBuilder text, float desiredWidth, bool wrap)
		{
			Drawing.RectangleF result = Drawing.RectangleF.Empty;			// Resulting rectangle.
			Font.FontCharacter characterInfo;								// Character info.
			char character = ' ';											// Character.
			float xPos = 0.0f;												// Horizontal position.			

			if ((_font == null) || (_font.Count == 0) || (text.Length == 0))
				return Drawing.RectangleF.Empty;

			// Turn off wrapping if the desired rectangle is empty.
			if (desiredWidth < 0.0f)
				wrap = false;

			if (!wrap)
				desiredWidth = 0.0f;
		
			// Start at the beginning.
			result.X = 0;
			result.Width = desiredWidth;
			result.Y = 0;
			result.Height = 0;

			// Calculate each character.
			for (int i = 0; i < text.Length; i++)
			{
				character = text[i];	
				if (_font.Contains(character))
					characterInfo = _font[character];
				else
					characterInfo = _font[' '];

				// Default to the first character height.
				if (result.Height == 0)
					result.Height = characterInfo.Height;

				// Wrap if we've encountered a break.
				if ((character == '\r') || (character == '\n'))
				{
					result.Height += characterInfo.Height;
					xPos = result.X;
				}
				else
				{
					xPos += (int)characterInfo.B + characterInfo.C;
					if (xPos > result.Width)
					{
						if (!wrap)
							result.Width = xPos;
						else
						{
							xPos = 0.0f;
							result.Height += characterInfo.Height;
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to update the AABB for the text.
		/// </summary>
		public override void UpdateAABB()
		{
			Drawing.RectangleF bounds = Drawing.RectangleF.Empty;		// Boundaries.

			base.UpdateAABB();

			bounds = Bounds.Dimensions;

			SetAABB(MeasureText(_text, bounds.Width, _wordWrap));
		}

		/// <summary>
		/// Function to update the current text in the buffer.
		/// </summary>
		/// <param name="text">Text to use.</param>
		public void UpdateTextBuffer(StringBuilder text)
		{
			// Just copy the reference.
			_text = text;
		}		

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public override void Draw(bool flush)
		{
			int vertexIndex = 0;			// Current vertex index.

			BeginRendering(flush);

			// Update the data.
			UpdateDimensions();

			// Don't draw if we don't need to.
			if ((_text.Length == 0) || (Vertices == null))
				return;

			// Send to the buffers.
			for (int i = 0; i < _text.Length; i++)
			{
				// Write the data to the buffer.			
				if ((_text[i] != '\r') && (_text[i] != '\n') && (_text[i] != '\t') && (_text[i] != ' '))
				{

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
			base.Image = font.FontImage;
			_clipping = null;
			_text = new StringBuilder(80);
			_originalText = new StringBuilder(80);
			_font = font;
			_vertexColors = new int[4];			
			Position = position;
			Scale = Vector2D.Unit;
			Axis = Vector2D.Zero;
			_alignment = Alignment.UpperLeft;            
			
			// Update the string buffer.
			if ((text != null) && (text.Length != 0))
			{
				_originalText.Append(text);
				FormatText(text);
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
		/// <param name="position">Initial position of the text.</param>
		/// <param name="textColor">Color of the text</param>
		public TextSprite(string name, string text, Vector2D position, Drawing.Color textColor)
			: this(name, text, Font.DefaultFont, position, textColor)
		{
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
		/// <param name="position">Initial position of the text.</param>
		public TextSprite(string name, string text, Vector2D position)
			: this(name, text, Font.DefaultFont, position, Drawing.Color.White)
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
		public TextSprite(string name, string text)
			: this(name, text, Font.DefaultFont, Vector2D.Zero, Drawing.Color.White)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="color">Color of the text.</param>
		public TextSprite(string name, string text,Drawing.Color color)
			: this(name, text, Font.DefaultFont, Vector2D.Zero, color)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		public TextSprite(string name)
			: this(name, string.Empty, Font.DefaultFont, Vector2D.Zero, Drawing.Color.White)
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
		/// <param name="positionX">Horizontal position of the text.</param>
		/// <param name="positionY">Vertical position of the text.</param>
		/// <param name="textColor">Color of the</param>
		public TextSprite(string name, string text, float positionX, float positionY, Drawing.Color textColor)
			: this(name, text, Font.DefaultFont, new Vector2D(positionX, positionY), textColor)
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

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>		
		/// <param name="positionX">Horizontal position of the text.</param>
		/// <param name="positionY">Vertical position of the text.</param>		
		public TextSprite(string name, string text, float positionX, float positionY)
			: this(name, text, Font.DefaultFont, new Vector2D(positionX, positionY), Drawing.Color.White)
		{
		}
		#endregion
    }
}
