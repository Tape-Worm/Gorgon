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
// Created: Sunday, April 15, 2012 9:19:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Anti-aliasing modes for the font.
	/// </summary>
	public enum FontAntiAliasMode
	{
		/// <summary>
		/// No anti-aliasing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Anti-aliasing, low quality.  
		/// </summary>
		AntiAlias = 1,
		/// <summary>
		/// Anti-aliasing, high quality.
		/// </summary>
		AntiAliasHQ = 2
	}

	/// <summary>
	/// Settings for a font.
	/// </summary>
	public class GorgonFontSettings
	{
		#region Variables.
		private Size _textureSize = new Size(256, 256);						// Texture size.
		private IEnumerable<char> _characters = string.Empty;				// The list of characters supported by the font.
		private int _contrast = 4;											// Text contrasting.
		private int _packSpace = 1;											// Packing spacing.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the font family name to generate the font from.
		/// </summary>
		public string FontFamilyName
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the point size.
		/// </summary>
		public float PointSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the contrast when anti-aliasing.
		/// </summary>
		/// <remarks>The default value is 4.  This value is has a range of 0 to 12.</remarks>
		public int TextContrast
		{
			get
			{
				return _contrast;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 12)
					value = 12;

				_contrast = value;
			}
		}			

		/// <summary>
		/// Property to set or return the size of the texture to use with the font.
		/// </summary>
		/// <remarks>Use this decrease or increase the number of textures used for a font.  If the number of glyphs cannot fit onto a single texture, a new texture will be created to 
		/// store the remaining glyphs.  This value will control the width and height of the textures created.
		/// <para>To retrieve the count of textures used, call the <see cref="P:GorgonLibrary.Graphics.GorgonFont.FontTextureCollection.Count">Count</see> property on the <see cref="P:GorgonLibrary.Graphics.GorgonFont.Textures">Textures</see> property.</para>
		/// <para>The default size is 256x256, the minimum size is 64x64 and the maximum size depends on the maximum texture size that's supported by the feature level.</para>
		/// </remarks>
		public Size TextureSize
		{
			get
			{
				return _textureSize;
			}
			set
			{
				if (value.Width < 64)
					value.Width = 64;
				if (value.Height < 64)
					value.Height = 64;

				_textureSize = value;
			}
		}

		/// <summary>
		/// Property to set or return the list of available characters in the font.
		/// </summary>
		/// <remarks>This will be a list of characters that can be displayed by the font.
		/// <para>This property will re-order the string from the lowest character value to the highest.</para>
		/// <para>The default encompasses characters from ASCII character code 32 to 255.</para>
		/// </remarks>
		public IEnumerable<char> Characters
		{
			get
			{
				return _characters;
			}
			set
			{
				// Always default to a space character.
				if ((value == null) || (value.Count() == 0))
					_characters = " ";

				// Reorder the string.
				_characters = value.OrderBy(c => Convert.ToInt32(c));
			}
		}

		/// <summary>
		/// Property to set or return whether the anti-aliasing mode.
		/// </summary>
		/// <remarks>This is normal anti-aliasing, and not ClearType.  ClearType is not supported by Gorgon.
		/// <para>The default value is AntiAliasHQ.</para>
		/// </remarks>
		public FontAntiAliasMode AntiAliasingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the base color for the font data on the textures.
		/// </summary>
		/// <remarks>The default value is White (A=1.0f, R=1.0f, G=1.0f, B=1.0f).</remarks>
		public GorgonColor BaseColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an outline.
		/// </summary>
		/// <remarks>The size of the outline is in pixels, and a value of 0 indicates that outlining is disabled.
		/// <para>The default value is 0.</para></remarks>
		public int OutlineSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color of the outline.
		/// </summary>
		/// <remarks>
		/// If the alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible.
		/// <para>The default value is Black (A=1.0f, R=0.0f, G=0.0f, B=0.0f).</para></remarks>
		public GorgonColor OutlineColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a brush to use for special effects on the font.
		/// </summary>
		/// <remarks>The default value is NULL (Nothing in VB.Net).</remarks>
		public Brush Brush
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the style for the font.
		/// </summary>
		/// <remarks>The default value is Regular.</remarks>
		public FontStyle FontStyle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a default character to use in place of a character that cannot be found in the font.
		/// </summary>
		/// <remarks>Some characters are unprintable, and thus have no width/height.  This character will be substituted in those cases.
		/// <para>The default value is a space ' '.</para>
		/// </remarks>
		public char DefaultCharacter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the spacing (in pixels) used between font glyphs on the packed texture.
		/// </summary>
		/// <remarks>Valid values are between 0 and 8.
		/// <para>The default value is 1 pixel.</para></remarks>
		public int PackingSpacing
		{
			get
			{
				return _packSpace;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 8)
					value = 8;
				_packSpace = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontSettings"/> class.
		/// </summary>
		public GorgonFontSettings()
		{
			Characters = Enumerable.Range(32, 224).
						 Select(i => Convert.ToChar(i)).
						 Where(c => !char.IsControl(c));

			OutlineColor = Color.Black;
			OutlineSize = 0;
			BaseColor = Color.White;			
			FontStyle = FontStyle.Regular;
			DefaultCharacter = ' ';
			AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ;
		}
		#endregion		
	}
}
