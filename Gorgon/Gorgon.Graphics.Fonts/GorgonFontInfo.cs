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
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts
{
    /// <summary>
    /// Provides information used to create a new <see cref="GorgonFont"/>.
    /// </summary>
    public class GorgonFontInfo
        : IGorgonFontInfo
    {
        #region Variables.
        // Texture size.
        private DX.Size2 _textureSize = new DX.Size2(256, 256);
        // The list of characters supported by the font.
        private char[] _characters;
        // Packing spacing.
        private int _packSpace = 1;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the font height is in pixels or in points.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the font uses points for its height, the user must be aware of DPI scaling issues that may arise.
        /// </para>
        /// <para>
        /// This will affect the <see cref="Size"/> value in that it will alter the meaning of the units.
        /// </para>
        /// <para>
        /// The default value is <see cref="Fonts.FontHeightMode.Pixels"/>.
        /// </para>
        /// </remarks>
        public FontHeightMode FontHeightMode
        {
            get;
        }

        /// <summary>
        /// Property to return the font family name to generate the font from.
        /// </summary>
        public string FontFamilyName
        {
            get;
        }

        /// <summary>
        /// Property to return the font size.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the height of the font, including ascent and descent.
        /// </para>
        /// <para>
        /// This is affected by the <see cref="Fonts.FontHeightMode"/>. If the <see cref="FontHeightMode"/> is set to <see cref="Fonts.FontHeightMode.Points"/>, then this unit is the height 
        /// size height for the font. Otherwise, this represents the font height in <see cref="Fonts.FontHeightMode.Pixels"/>.
        /// </para>
        /// </remarks>
        public float Size
        {
            get;
        }

        /// <summary>
        /// Property to set or return the width of the texture(s) used as the backing store for the bitmap font data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value will control the width of the textures created.  It can be used to decrease or increase the number of textures used for a font.  This is important because if the number of 
        /// requested glyphs cannot fit onto a single texture, a new texture will be created to store the remaining glyphs. Keeping as few textures as possible for a font is beneficial for performance.
        /// </para>
        /// <para>
        /// Font textures cannot be smaller than 256x256 and the maximum size is dependent upon <see cref="FeatureSet"/> for the <see cref="IGorgonVideoAdapterInfo"/>.
        /// </para>
        /// <para>
        /// The default width is 256.
        /// </para>
        /// </remarks>
        public int TextureWidth
        {
            get => _textureSize.Width;
            set
            {
                if (value < 256)
                {
                    value = 256;
                }
                if (value < 256)
                {
                    value = 256;
                }

                _textureSize = new DX.Size2(value, _textureSize.Height);
            }
        }

        /// <summary>
        /// Property to return the height of the texture(s) used as the backing store for the bitmap font data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value will control the width of the textures created.  It can be used to decrease or increase the number of textures used for a font.  This is important because if the number of 
        /// requested glyphs cannot fit onto a single texture, a new texture will be created to store the remaining glyphs. Keeping as few textures as possible for a font is beneficial for performance.
        /// </para>
        /// <para>
        /// Font textures cannot be smaller than 256x256 and the maximum size is dependent upon <see cref="FeatureSet"/> for the <see cref="IGorgonVideoAdapterInfo"/>.
        /// </para>
        /// <para>
        /// The default height is 256.
        /// </para>
        /// </remarks>
        public int TextureHeight
        {
            get => _textureSize.Height;
            set
            {
                if (value < 256)
                {
                    value = 256;
                }
                if (value < 256)
                {
                    value = 256;
                }

                _textureSize = new DX.Size2(_textureSize.Width, value);
            }
        }

        /// <summary>
        /// Property to return whether premultiplied textures are used when generating the glyphs for the font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines whether the textures used to store the glyphs of the font will use premultiplied alpha. Premultiplied alpha is used to give a more accurate representation of blending between 
        /// colors, and is accomplished by multiplying the RGB values by the Alpha component of a pixel.
        /// </para>
        /// <para>
        /// If this value is <b>true</b>, then applications should use the <see cref="GorgonBlendState.Premultiplied"/> blending state when rendering text.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool UsePremultipliedTextures
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the list of available characters to use as glyphs within the font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will define what characters the font can display when rendering.  Any character not defined in this property will use the default character (typically a space).
        /// </para>
        /// <para>
        /// The default encompasses characters from ASCII character code 32 to 255.
        /// </para>
        /// </remarks>
        public IEnumerable<char> Characters
        {
            get => _characters;
            set
            {
                if (value == null)
                {
                    _characters = new[]
                                  {
                                      ' '
                                  };
                    return;
                }

                _characters = value.ToArray();

                if (_characters.Length == 0)
                {
                    _characters = new[]
                                  {
                                      ' '
                                  };
                }
            }
        }

        /// <summary>
        /// Property to set or return the anti-aliasing mode for the font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the smoothness of font pixel edges.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// Gorgon does not support clear type at this time.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// The default value is <see cref="FontAntiAliasMode.AntiAlias"/>.
        /// </para>
        /// </remarks>
        public FontAntiAliasMode AntiAliasingMode
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size of an outline.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the thickness of an outline (in pixels) for each glyph in the font.  A value greater than 0 will draw an outline, 0 or less will turn outlining off.
        /// </para>
        /// <para>
        /// The default value is 0.
        /// </para>
        /// </remarks>
        public int OutlineSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the starting color of the outline.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the starting color for an outline around a font glyph. This can be used to give a gradient effect to the outline and allow for things like glowing effects and such.
        /// </para>
        /// <para>
        /// If the alpha channel is set to 0.0f and the <see cref="OutlineColor2"/> alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible. This will also be ignored 
        /// if the <see cref="OutlineSize"/> value is not greater than 0.
        /// </para> 
        /// <para>
        /// The default value is <see cref="GorgonColor.Transparent"/> (A=1.0f, R=0.0f, G=0.0f, B=0.0f).
        /// </para>
        /// </remarks>
        public GorgonColor OutlineColor1
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the ending color of the outline.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the ending color for an outline around a font glyph. This can be used to give a gradient effect to the outline and allow for things like glowing effects and such.
        /// </para>
        /// <para>
        /// If the alpha channel is set to 0.0f and the <see cref="OutlineColor1"/> alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible. This will also be ignored 
        /// if the <see cref="OutlineSize"/> value is not greater than 3.
        /// </para> 
        /// <para>
        /// The default value is <see cref="GorgonColor.Transparent"/> (A=1.0f, R=0.0f, G=0.0f, B=0.0f).
        /// </para>
        /// </remarks>
        public GorgonColor OutlineColor2
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return a <see cref="GorgonGlyphBrush"/> to use for special effects on the glyphs for the font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value can be used to define how a glyph is rendered when the <see cref="GorgonFont"/> is generated. Applications can use brushes for gradients, textures, etc... to render the glyphs to give 
        /// them a unique look.
        /// </para>
        /// <para>
        /// This default value is <b>null</b>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonGlyphHatchBrush"/>
        /// <seealso cref="GorgonGlyphLinearGradientBrush"/>
        /// <seealso cref="GorgonGlyphPathGradientBrush"/>
        /// <seealso cref="GorgonGlyphSolidBrush"/>
        /// <seealso cref="GorgonGlyphTextureBrush"/>
        public GorgonGlyphBrush Brush
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the style for the font.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="Fonts.FontStyle.Normal"/>.
        /// </remarks>
        public FontStyle FontStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return a default character to use in place of a character that cannot be found in the font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If some characters are unprintable (e.g. have no width/height), or they were not defined in the <see cref="Characters"/> property.  This character will be substituted in those cases.
        /// </para>
        /// <para>
        /// The default value is a space (' ').
        /// </para>
        /// </remarks>
        public char DefaultCharacter
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the spacing (in pixels) used between font glyphs on the backing texture.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines how much space to put between glyphs. Higher values will waste more space (and thus lead to more textures being created), but may resolve rendering issues.
        /// </para>
        /// <para>
        /// The valid values are between 0 and 8.
        /// </para>
        /// <para>
        /// The default value is 1 pixel.
        /// </para>
        /// </remarks>
        public int PackingSpacing
        {
            get => _packSpace;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 8)
                {
                    value = 8;
                }

                _packSpace = value;
            }
        }

        /// <summary>
        /// Property to set or return whether to include kerning pair information in the font.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kerning pairs are used to define spacing between 2 characters in a font.  When this value is set to <b>true</b> the kerning information of the font is retrieved and added to the <see cref="GorgonFont"/> 
        /// for use when rendering. This can be used to resolve rendering issues regarding horizontal font spacing.
        /// </para>
        /// <para>
        /// <note type="note">
        /// <para>
        /// Some fonts do not employ the use of kerning pairs, and consequently, this setting will be ignored if that is the case.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// The default value is <b>true</b>.
        /// </para>
        /// </remarks>
        public bool UseKerningPairs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the name of the font object.
        /// </summary>
	    public string Name
        {
            get;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontInfo"/> class.
        /// </summary>
        /// <param name="fontInfo">The font information to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fontInfo"/> parameter is <b>null</b>.</exception>
        public GorgonFontInfo(IGorgonFontInfo fontInfo)
        {
            FontHeightMode = fontInfo?.FontHeightMode ?? throw new ArgumentNullException(nameof(fontInfo));
            AntiAliasingMode = fontInfo.AntiAliasingMode;
            Brush = fontInfo.Brush;
            DefaultCharacter = fontInfo.DefaultCharacter;
            Characters = fontInfo.Characters;
            FontFamilyName = fontInfo.FontFamilyName;
            FontStyle = fontInfo.FontStyle;
            OutlineColor1 = fontInfo.OutlineColor1;
            OutlineColor2 = fontInfo.OutlineColor2;
            OutlineSize = fontInfo.OutlineSize;
            PackingSpacing = fontInfo.PackingSpacing;
            Size = fontInfo.Size;
            TextureWidth = fontInfo.TextureWidth;
            TextureHeight = fontInfo.TextureHeight;
            UsePremultipliedTextures = fontInfo.UsePremultipliedTextures;
            UseKerningPairs = fontInfo.UseKerningPairs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontInfo"/> class.
        /// </summary>
        /// <param name="fontFamily">The name of the font family to derive the font from.</param>
        /// <param name="size">The height of the font.</param>
        /// <param name="heightMode">[Optional] The type of units to express the font height in.</param>
        /// <param name="name">[Optional] The name of the font.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fontFamily"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fontFamily"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">The <paramref name="size"/> parameter is less than or equal to 0.</exception>
        public GorgonFontInfo(string fontFamily, float size, FontHeightMode heightMode = FontHeightMode.Pixels, string name = null)
        {
            if (fontFamily == null)
            {
                throw new ArgumentNullException(nameof(fontFamily));
            }

            if (string.IsNullOrWhiteSpace(fontFamily))
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(fontFamily));
            }

            if (size <= 0)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_SIZE_TOO_SMALL, size), nameof(size));
            }

            Name = name ?? $"{fontFamily} {size}{heightMode}";
            UseKerningPairs = true;
            _characters = Enumerable.Range(32, 224).
                                     Select(Convert.ToChar).
                                     Where(c => !char.IsControl(c))
                                     .ToArray();

            FontHeightMode = heightMode;
            OutlineColor1 = GorgonColor.Transparent;
            OutlineColor2 = GorgonColor.Transparent;
            OutlineSize = 0;
            FontStyle = FontStyle.Normal;
            DefaultCharacter = ' ';
            AntiAliasingMode = FontAntiAliasMode.AntiAlias;
            Size = size;
            FontFamilyName = fontFamily;
        }
        #endregion
    }
}
