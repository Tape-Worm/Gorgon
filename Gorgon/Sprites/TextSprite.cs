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
using GorgonLibrary.Graphics.Animations;
using GorgonLibrary.Graphics.Fonts;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a sprite containing text.
	/// </summary>
	public class TextSprite
		: Renderable<VertexTypes.PositionDiffuse2DTexture1>
	{
		#region Variables.
		private Font _font;							// Font to use.
		private StringBuilder _text;				// Text to use.
		private StringBuilder _originalText;		// Original text.
		private int[] _vertexColors;				// Vertex colors.
		private bool _colorUpdated;					// Flag to indicate that the color was updated.
		private Alignment _alignment;				// Text alignment.
		private bool _wordWrap;						// Flag to enable or disable word wrapping.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		public override float UniformScale
		{
			get
			{
				return _scale.X;
			}
			set
			{
				if (value == 0.0f)
					return;

				// Set the uniform scale.
				_scale.X = _scale.Y = value;
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
				_wordWrap = value;
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
				_alignment = value;
			}
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		public override Vector2D Axis
		{
			get
			{
				return _axis;
			}
			set
			{
				_axis = value;
				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public override Vector2D Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				// Don't allow 0 scale.
				if ((value.X == 0.0f) && (value.Y == 0.0f))
					return;
                
                _needParentUpdate = true;

				if (value.X != 0.0f)
					_scale.X = value.X;
				if (value.Y != 0.0f)
					_scale.Y = value.Y;

                if (_children.Count > 0)
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
		public string Text
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
				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;

                if (_children.Count > 0)
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
				return new Vector2D(_AABB.Width, _AABB.Height);
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
				return _AABB.Width;
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
				return _AABB.Height;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return the font.
		/// </summary>
		public Font Font
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
				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;
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
		#endregion

		#region Methods.
		/// <summary>
		/// Property to return post-formatted text.
		/// </summary>
		/// <param name="text">Text to format.</param>
		private void FormatText(string text)
		{
			Drawing.RectangleF bounds = Drawing.RectangleF.Empty;		// Boundaries.

			if (ClippingViewport == null)
				bounds = Gorgon.Screen.DefaultView.Dimensions;
			else
				bounds = ClippingViewport.Dimensions;

			_text.Length = 0;
			if (_wordWrap)
				_text.Append(WordWrapText(text, bounds));
			else
				_text.Append(text);
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
			_vertices[charIndex].Color = _vertexColors[(int)VertexLocations.UpperLeft];
			_vertices[charIndex + 1].Color = _vertexColors[(int)VertexLocations.UpperRight];
			_vertices[charIndex + 2].Color = _vertexColors[(int)VertexLocations.LowerRight];
			_vertices[charIndex + 3].Color = _vertexColors[(int)VertexLocations.LowerLeft];
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

            // Get the parent transforms.
            GetParentTransform();

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
            posX1 = position.X + _axis.X;
            posX2 = position.X + characterInfo.B + _axis.X;
            posY1 = position.Y + _axis.Y;
            posY2 = position.Y + characterInfo.Height + _axis.Y;

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
					posY1 += (int)((bounds.Height / 2.0f) - (_AABB.Height / 2.0f));
					posY2 += (int)((bounds.Height / 2.0f) - (_AABB.Height / 2.0f));
					break;
				case Alignment.Right:
					posY1 += (int)((bounds.Height / 2.0f) - (_AABB.Height / 2.0f));
					posY2 += (int)((bounds.Height / 2.0f) - (_AABB.Height / 2.0f));
					posX1 += (int)(bounds.Width - lineLength);
					posX2 += (int)(bounds.Width - lineLength);
					break;
				case Alignment.Center:
					posY1 += (int)((bounds.Height / 2.0f) - (_AABB.Height / 2.0f));
					posY2 += (int)((bounds.Height / 2.0f) - (_AABB.Height / 2.0f));
					posX1 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posX2 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					break;
				case Alignment.LowerLeft:
					posY1 += (int)(bounds.Height - _AABB.Height);
					posY2 += (int)(bounds.Height - _AABB.Height);
					break;
				case Alignment.LowerCenter:
					posX1 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posX2 += (int)((bounds.Width / 2.0f) - (lineLength / 2.0f));
					posY1 += (int)(bounds.Height - _AABB.Height);
					posY2 += (int)(bounds.Height - _AABB.Height);
					break;
				case Alignment.LowerRight:
					posX1 += (int)(bounds.Width - lineLength);
					posX2 += (int)(bounds.Width - lineLength);
					posY1 += (int)(bounds.Height - _AABB.Height);
					posY2 += (int)(bounds.Height - _AABB.Height);
					break;
			}

			// Scale the points.
			if (_finalScale.X != 1.0f)
			{
                posX1 *= _finalScale.X;
                posX2 *= _finalScale.X;
			}
			if (_finalScale.Y != 1.0f)
			{
                posY1 *= _finalScale.Y;
                posY2 *= _finalScale.Y;
			}

			// Set the Z position, although we don't really need it, it may fuck something up if we don't.
			_vertices[charIndex].Position.Z = -0.5f;
			_vertices[charIndex+1].Position.Z = -0.5f;
			_vertices[charIndex+2].Position.Z = -0.5f;
			_vertices[charIndex+3].Position.Z = -0.5f;

			// Set rotation.
			if (_finalRotation != 0.0f)
			{
				float cosVal;		// Cached cosine.
				float sinVal;		// Cached sine.
				float angle;		// Angle in radians.

			    angle = MathUtility.Radians(_finalRotation);
				cosVal = MathUtility.Cos(angle);
				sinVal = MathUtility.Sin(angle);

				// Rotate the vertices.
                _vertices[charIndex].Position.X = (posX1 * cosVal - posY1 * sinVal);
                _vertices[charIndex].Position.Y = (posX1 * sinVal + posY1 * cosVal);
                _vertices[charIndex + 1].Position.X = (posX2 * cosVal - posY1 * sinVal);
                _vertices[charIndex + 1].Position.Y = (posX2 * sinVal + posY1 * cosVal);
                _vertices[charIndex + 2].Position.X = (posX2 * cosVal - posY2 * sinVal);
                _vertices[charIndex + 2].Position.Y = (posX2 * sinVal + posY2 * cosVal);
                _vertices[charIndex + 3].Position.X = (posX1 * cosVal - posY2 * sinVal);
                _vertices[charIndex + 3].Position.Y = (posX1 * sinVal + posY2 * cosVal);
			}
			else
			{
				// Else just keep the positions.
				_vertices[charIndex].Position.X = posX1;
                _vertices[charIndex].Position.Y = posY1;
                _vertices[charIndex + 1].Position.X = posX2;
                _vertices[charIndex + 1].Position.Y = posY1;
                _vertices[charIndex + 2].Position.X = posX2;
                _vertices[charIndex + 2].Position.Y = posY2;
                _vertices[charIndex + 3].Position.X = posX1;
                _vertices[charIndex + 3].Position.Y = posY2;
			}

			// Translate if necessary.
			if (_finalPosition.X != 0.0)
			{
                _vertices[charIndex].Position.X += _finalPosition.X;
                _vertices[charIndex + 1].Position.X += _finalPosition.X;
                _vertices[charIndex + 2].Position.X += _finalPosition.X;
                _vertices[charIndex + 3].Position.X += _finalPosition.X;
			}

			if (_finalPosition.Y != 0.0)
			{
                _vertices[charIndex].Position.Y += _finalPosition.Y;
                _vertices[charIndex + 1].Position.Y += _finalPosition.Y;
                _vertices[charIndex + 2].Position.Y += _finalPosition.Y;
                _vertices[charIndex + 3].Position.Y += _finalPosition.Y;
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
			_vertices[charIndex].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Left, characterInfo._textureCoordinates.Top);
			_vertices[charIndex + 1].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Right, characterInfo._textureCoordinates.Top);
			_vertices[charIndex + 2].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Right, characterInfo._textureCoordinates.Bottom);
			_vertices[charIndex + 3].TextureCoordinates = new Vector2D(characterInfo._textureCoordinates.Left, characterInfo._textureCoordinates.Bottom);
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

			if (ClippingViewport == null)
				bounds = Gorgon.Renderer.CurrentRenderTarget.DefaultView.Dimensions;
			else
				bounds = ClippingViewport.Dimensions;

			// Calculate the number of vertices.
			vertexCount = (_text.Length * 2) * 4;

			// If no vertices, do nothing.
			if (vertexCount == 0)
			{
				_vertices = null;
				return;
			}

			// Create a new set of vertices.
			if ((_vertices == null) || (vertexCount > _vertices.Length))
			{
				_vertices = new VertexTypes.PositionDiffuse2DTexture1[vertexCount];
				_imageCoordinatesChanged = true;
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
			_dimensionsChanged = false;			
		}

		/// <summary>
		/// Function to append text to the already existing text.
		/// </summary>
		/// <param name="text">Text to append.</param>
		public void AppendText(string text)
		{
			if (text.Length > 0)
			{
				_originalText.Append(text);

				FormatText(_originalText.ToString());
				UpdateAABB();

				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;

                if (_children.Count > 0)
					((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Function to insert text into the already existing text.
		/// </summary>
		/// <param name="position">Position to insert at.</param>
		/// <param name="text">Text to insert.</param>
		public void InsertText(int position, string text)
		{
			if ((position < 0) || ((position + text.Length) > _text.Length))
				return;

			if (text.Length > 0)
			{
				_originalText.Insert(position, text);

				FormatText(_originalText.ToString());
				UpdateAABB();

				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;
			}
		}

		/// <summary>
		/// Function to remove text from the already existing text.
		/// </summary>
		/// <param name="position">Position to start removing.</param>
		/// <param name="length">Number of characters to remove.</param>
		public void RemoveText(int position, int length)
		{
			if (((position + length) >= _text.Length) || (position < 0))
				return;

			_originalText.Remove(position, length);

			FormatText(_originalText.ToString());
			UpdateAABB();

			_dimensionsChanged = true;
			_imageCoordinatesChanged = true;
		}

		/// <summary>
		/// Function to return the optimal dimensions of the specified text.
		/// </summary>
		/// <param name="text">Text to examine.</param>
		/// <param name="desiredWidth">Ideal width of the rectangle, only applies of wrap is set to TRUE.</param>
		/// <param name="wrap">TRUE to wrap text at the desired rectangle width, FALSE to let it continue.</param>
		/// <returns>Rectangle that will contain the text.</returns>
		public Drawing.RectangleF MeasureText(string text, float desiredWidth, bool wrap)
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

			if (ClippingViewport == null)
				bounds = Gorgon.Screen.DefaultView.Dimensions;
			else
				bounds = ClippingViewport.Dimensions;

			_AABB = MeasureText(_text.ToString(), bounds.Width, _wordWrap);
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public override void Draw(bool flush)
		{
			StateManager manager = null;							// State manager.
			int start = Gorgon.StateManager.RenderData.VertexOffset;// Vertex starting point.
			int count = Gorgon.StateManager.RenderData.VertexCount;	// Vertex count.
			int vertexIndex = 0;									// Vertex index.
            IMoveable child = null;									// Child object.

			// Don't draw if we don't need to.
			if (_text.Length == 0)
				return;

			manager = Gorgon.StateManager;

            // Draw the children.
            if (_children.Count > 0)
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    child = _children[i].Child;
                    if (child != null)
                        child.Draw(flush);
                }
            }
            
            // If we're at the end of the buffer, wrap around.
			if (((manager.RenderData.VerticesWritten + 4 >= count) || (Gorgon.Renderer.GetImage(0) != Image) ||
				(manager.StateChanged(this))) && (manager.RenderData.VerticesWritten != 0))
			{
				Gorgon.Renderer.Render();
				start = 0;
			}

            // Apply animations.
            if (_animations.Count > 0)
                ((IAnimatable)this).ApplyAnimations();

			// Update the data.
			UpdateDimensions();

			// Don't draw if we don't need to.
			if ((_vertices == null) || (_text.Length == 0))
				return;

			// Set the state for this sprite.
			manager.SetStates(this);

			// Set the currently active image.
			Gorgon.Renderer.SetImage(0, _font.FontImage);

			// Send to the buffers.
			for (int i = 0; i < _text.Length; i++)
			{
				// Write the data to the buffer.			
				if ((_text[i] != '\r') && (_text[i] != '\n') && (_text[i] != '\t') && (_text[i] != ' '))
				{
					manager.RenderData.VertexCache.WriteData(vertexIndex, manager.RenderData.VertexOffset + manager.RenderData.VerticesWritten, 4, _vertices);

					// Recycle if necessary.
					if (manager.RenderData.VerticesWritten + 4 >= manager.RenderData.VertexCount)
						Gorgon.Renderer.Render();
					vertexIndex += 4;
				}
			}
			
			_colorUpdated = false;
			_imageCoordinatesChanged = false;

			// Flush the current contents, this will slow things down greatly, so use sparingly.
			if (flush)
				Gorgon.Renderer.Render();
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
		/// <param name="textColor">Color of the</param>
		public TextSprite(string name, string text, Font font, Vector2D position, Drawing.Color textColor)
			: base(name)
		{
			Image = font.FontImage;
			_text = new StringBuilder(80);
			_originalText = new StringBuilder(80);
			_font = font;
			_vertexColors = new int[4];			
			_position = position;
			_vertices = null;
			_scale = Vector2D.Unit;
			_axis = Vector2D.Zero;
			_alignment = Alignment.UpperLeft;            
			
			// Update the string buffer.
			if ((text != null) && (text.Length != 0))
			{
				_originalText.Append(text);
				FormatText(text);
			}

			_dimensionsChanged = true;
			_colorUpdated = true;
			// Set sprite colors.
			Color = textColor;
			UpdateAABB();
			_size.X = _AABB.Width;
			_size.Y = _AABB.Height;

            // Create animation list.			
            _animations = new AnimationList(this);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="position">Initial position of the text.</param>
		/// <param name="textColor">Color of the</param>
		public TextSprite(string name, string text, Vector2D position, Drawing.Color textColor)
			: this(name, text, FontManager.DefaultFont, position, textColor)
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
			: this(name, text, FontManager.DefaultFont, position, Drawing.Color.White)
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
			: this(name, text, FontManager.DefaultFont, Vector2D.Zero, Drawing.Color.White)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="color">Color of the text.</param>
		public TextSprite(string name, string text,Drawing.Color color)
			: this(name, text, FontManager.DefaultFont, Vector2D.Zero, color)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		public TextSprite(string name)
			: this(name, string.Empty, FontManager.DefaultFont, Vector2D.Zero, Drawing.Color.White)
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
			: this(name, text, FontManager.DefaultFont, new Vector2D(positionX, positionY), textColor)
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
			: this(name, text, FontManager.DefaultFont, new Vector2D(positionX, positionY), Drawing.Color.White)
		{
		}
		#endregion
    }
}
