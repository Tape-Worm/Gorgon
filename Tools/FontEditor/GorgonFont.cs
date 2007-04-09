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
// Created: Thursday, August 03, 2006 4:13:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using GorgonLibrary.Graphics.Fonts;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object representing a specialized gorgon font.
	/// </summary>
	class GorgonFont
		: Font
	{
		#region Events.
		/// <summary>
		/// Event fired when the border is updated.
		/// </summary>
		public event EventHandler BorderUpdated;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the size of the font image.
		/// </summary>
		public new Vector2D ImageSize
		{
			get
			{
				return _imageSize;
			}
			set
			{
				_imageSize = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clone a font.
		/// </summary>
		/// <param name="font">Font to clone.</param>
		public override void Copy(Font font)
		{
			_characters.Clear();
			_fontPath = font.Filename;
			_offset = font.ImageOffset;
			_fontImage = null;
			_objectName = font.Name;
			_sourceFont = font.SourceFont;
			_sourceFontSize = font.SourceFontSize;
			_bold = font.IsBold;
			_underline = font.IsUnderlined;
			_italics = font.IsItalics;
			_leftOffset = font.LeftBorderOffset;
			_rightOffset = font.RightBorderOffset;
			_topOffset = font.TopBorderOffset;
			_bottomOffset = font.BottomBorderOffset;
			_outlineColor = font.OutlineColor;
			_outlined = font.IsOutlined;
			_outlineSize = font.OutlineSize;
			_useAtlas = font.ImageIsAtlas;
			_imageSize = font.ImageSize;
			_baseColor = font.BaseColor;
			_antiAliased = font.IsAntiAliased;
			_shadowed = font.IsShadowed;
			_shadowColor = font.ShadowColor;
			_shadowDirection = font.ShadowDirection;
			_shadowOffset = font.ShadowOffset;

			// Copy the characters.
			foreach (Font.FontCharacter character in font)
				_characters.Add(character.Character, character);

			UpdateTextureCoordinates();
		}

		/// <summary>
		/// Function to update the characters in the font.
		/// </summary>
		/// <param name="characters">Array of characters to use.</param>
		public void UpdateCharacters(FontCharacter[] characters)
		{
			_characters.Clear();
			foreach (FontCharacter character in characters)
				_characters.Add(character.Character, character);
			UpdateTextureCoordinates();
		}

		/// <summary>
		/// Function to load a font.
		/// </summary>
		/// <param name="fontPath">Path and filename to the font.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		public bool LoadFont(string fontPath)
		{
		
			return false;
		}

		/// <summary>
		/// Function to set the font name.
		/// </summary>
		/// <param name="newName">Name of the sprite.</param>
		public void SetName(string newName)
		{
			_objectName = newName;
		}

		/// <summary>
		/// Function to set the font metadata.
		/// </summary>
		/// <param name="sourceFont">Name of the source font.</param>
		/// <param name="sourceFontSize">Size of the source font.</param>
		/// <param name="bold">TRUE if the font is bold, FALSE if not.</param>
		/// <param name="italics">TRUE if the font is in italics, FALSE if not.</param>
		/// <param name="underline">TRUE if the font is underlined, FALSE if not.</param>
		/// <param name="antialiased">TRUE if the font is anti-aliased, FALSE if not.</param>
		public void SetFontMetaData(string sourceFont, float sourceFontSize, bool bold, bool italics, bool underline, bool antialiased)
		{
			_sourceFont = sourceFont;
			_sourceFontSize = sourceFontSize;
			_bold = bold;
			_italics = italics;
			_underline = underline;
			_antiAliased = antialiased;
		}

		/// <summary>
		/// Function to set the border offsets.
		/// </summary>
		/// <param name="left">Left border offset.</param>
		/// <param name="top">Top border offset.</param>
		/// <param name="right">Right border offset.</param>
		/// <param name="bottom">Bottom border offset.</param>
		public void SetBorderOffsets(int left, int top, int right, int bottom)
		{
			_leftOffset = left;
			_topOffset = top;
			_rightOffset = right;
			_bottomOffset = bottom;
			if (BorderUpdated != null)
				BorderUpdated(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function to set the outline parameters.
		/// </summary>
		/// <param name="enabled">TRUE if there's an outline, FALSE if not.</param>
		/// <param name="size">Size of the outline.</param>
		/// <param name="color">Color of the outline.</param>
		public void SetOutline(bool enabled, int size, Drawing.Color color)
		{
			_outlined = enabled;
			_outlineSize = size;
			_outlineColor = color;
		}

		/// <summary>
		/// Function to set the shadow parameters.
		/// </summary>
		/// <param name="enabled">TRUE if the shadow is enabled, FALSE if not.</param>
		/// <param name="offset">Offset of the shadow from the character.</param>
		/// <param name="color">Color of the shadow.</param>
		/// <param name="direction">Direction of the shadow.</param>
		public void SetShadow(bool enabled, int offset, Drawing.Color color, FontShadowDirection direction)
		{
			_shadowed = enabled;
			_shadowOffset = offset;
			_shadowColor = color;
			_shadowDirection = direction;
		}

		/// <summary>
		/// Function to set the base font color.
		/// </summary>
		/// <param name="color">Color of the font.</param>
		public void SetFontColor(Drawing.Color color)
		{
			_baseColor = color;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public GorgonFont()
			: base("Unnamed")
		{
			_objectName = string.Empty;
		}
		#endregion
	}
}
