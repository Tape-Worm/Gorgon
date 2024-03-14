
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Sunday, April 15, 2012 9:19:40 AM
// 


using Gorgon.Graphics.Core;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// Provides information used to create a new <see cref="GorgonFont"/>
/// </summary>
/// <param name="FontFamilyName">The name of the font family to derive the font from.</param>
/// <param name="Size">The height of the font.</param>
/// <param name="FontHeightMode">The type of units to express the font height in.</param>
/// <remarks>
/// <para>
/// The <paramref cref="Size"/> represents the height of the font, including ascent and descent. This is affected by the <paramref cref="FontHeightMode"/>. If the <param cref="FontHeightMode"/> is set 
/// to <see cref="GorgonFontHeightMode.Points"/>, then this unit is the height size height for the font. Otherwise, this represents the font height in <see cref="GorgonFontHeightMode.Pixels"/>
/// </para>
/// </remarks>
public record GorgonFontInfo(string FontFamilyName, float Size, GorgonFontHeightMode FontHeightMode)
    : IGorgonFontInfo
{

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFontInfo"/> class.
    /// </summary>
    /// <param name="fontInfo">The font information to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fontInfo"/> parameter is <b>null</b>.</exception>
    public GorgonFontInfo(IGorgonFontInfo fontInfo)
        : this(fontInfo?.FontFamilyName ?? throw new ArgumentNullException(nameof(fontInfo)), fontInfo.Size, fontInfo.FontHeightMode)
    {
        AntiAliasingMode = fontInfo.AntiAliasingMode;
        Brush = fontInfo.Brush;
        DefaultCharacter = fontInfo.DefaultCharacter;
        Characters = fontInfo.Characters;
        FontStyle = fontInfo.FontStyle;
        OutlineColor1 = fontInfo.OutlineColor1;
        OutlineColor2 = fontInfo.OutlineColor2;
        OutlineSize = fontInfo.OutlineSize;
        PackingSpacing = fontInfo.PackingSpacing;
        TextureWidth = fontInfo.TextureWidth;
        TextureHeight = fontInfo.TextureHeight;
        UsePremultipliedTextures = fontInfo.UsePremultipliedTextures;
        UseKerningPairs = fontInfo.UseKerningPairs;
    }



    // Texture size.
    private DX.Size2 _textureSize = new(256, 256);
    // The list of characters supported by the font.
    private char[] _characters = Enumerable.Range(32, 224).Select(Convert.ToChar).Where(c => !char.IsControl(c)).ToArray();
    // Packing spacing.
    private int _packSpace = 1;



    /// <summary>
    /// Property to return the width of the texture(s) used as the backing store for the bitmap font data.
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
        init
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
        init
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
        init;
    }

    /// <summary>
    /// Property to return the list of available characters to use as glyphs within the font.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will define what characters the font can display when rendering.  Any character not defined in this property will use the default character (typically a space).
    /// </para>
    /// <para>
    /// The default encompasses characters from ASCII character code 32 to 224.
    /// </para>
    /// </remarks>
    public IEnumerable<char> Characters
    {
        get => _characters;
        init
        {
            if (value is null)
            {
                _characters =
                              [
                                  ' '
                              ];
                return;
            }

            _characters = value.ToArray();

            if (_characters.Length == 0)
            {
                _characters =
                              [
                                  ' '
                              ];
            }
        }
    }

    /// <summary>
    /// Property to return the anti-aliasing mode for the font.
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
    /// The default value is <see cref="GorgonFontAntiAliasMode.AntiAlias"/>.
    /// </para>
    /// </remarks>
    public GorgonFontAntiAliasMode AntiAliasingMode
    {
        get;
        init;
    } = GorgonFontAntiAliasMode.AntiAlias;

    /// <summary>
    /// Property to return the size of an outline.
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
        init;
    }

    /// <summary>
    /// Property to return the starting color of the outline.
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
    /// The default value is <see cref="GorgonColor.BlackTransparent"/> (A=0.0f, R=0.0f, G=0.0f, B=0.0f).
    /// </para>
    /// </remarks>
    public GorgonColor OutlineColor1
    {
        get;
        init;
    } = GorgonColor.BlackTransparent;

    /// <summary>
    /// Property to return the ending color of the outline.
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
    /// The default value is <see cref="GorgonColor.BlackTransparent"/> (A=0.0f, R=0.0f, G=0.0f, B=0.0f).
    /// </para>
    /// </remarks>
    public GorgonColor OutlineColor2
    {
        get;
        init;
    } = GorgonColor.BlackTransparent;

    /// <summary>
    /// Property to return a <see cref="GorgonGlyphBrush"/> to use for special effects on the glyphs for the font.
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
        init;
    }

    /// <summary>
    /// Property to return the style for the font.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="GorgonFontStyle.Normal"/>.
    /// </remarks>
    public GorgonFontStyle FontStyle
    {
        get;
        init;
    } = GorgonFontStyle.Normal;

    /// <summary>
    /// Property to return a default character to use in place of a character that cannot be found in the font.
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
        init;
    } = ' ';

    /// <summary>
    /// Property to return the spacing (in pixels) used between font glyphs on the backing texture.
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
        init
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
    /// Property to return whether to include kerning pair information in the font.
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
        init;
    } = true;

    /// <summary>
    /// Property to return the name of the font object.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = $"{FontFamilyName} {Size} {FontHeightMode}";

}