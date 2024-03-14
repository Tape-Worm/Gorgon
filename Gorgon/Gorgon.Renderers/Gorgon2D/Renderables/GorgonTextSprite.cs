
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: June 7, 2018 3:13:51 PM
// 


using System.Numerics;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.UI;
using DX = SharpDX;


namespace Gorgon.Renderers;

/// <summary>
/// Determines how text should be rendered
/// </summary>
public enum TextDrawMode
{
    /// <summary>
    /// Draw the glyphs only.
    /// </summary>
    GlyphsOnly = 0,
    /// <summary>
    /// <para>
    /// Draw the outlines only.
    /// </para>
    /// <para>
    /// This is only supported when the <see cref="GorgonFont"/> has an outline.
    /// </para>
    /// </summary>
    OutlineOnly = 1,
    /// <summary>
    /// <para>
    /// Draw both the glyphs and the outlines.
    /// </para> 
    /// <para>
    /// This is only supported when the <see cref="GorgonFont"/> has an outline.
    /// </para>
    /// </summary>
    OutlinedGlyphs = 2
}

/// <summary>
/// A class that defines a sprite that is used to display text
/// </summary>
public class GorgonTextSprite
{

    // The text to render.
    private string _text;
    // Text with embedded codes.
    private string _encodedText;
    // The formatted text.
    private readonly StringBuilder _formattedText = new(256);
    // The area for used for text layout.
    private DX.Size2F? _layoutArea;
    // Flag to allow or disallow control codes in the text.
    private bool _allowCodes;
    // The parser used to parse out the codes from text assigned to this object.
    private readonly TextCodeParser _parser = new();

    /// <summary>
    /// The renderable data for this sprite.
    /// It is exposed an internal variable (which goes against C# best practices) for performance reasons (property accesses add up over time).
    /// </summary>
    internal readonly TextRenderable Renderable = new();



    /// <summary>
    /// Property to return whether or not the sprite has had its position, size, texture information, or object space vertices updated since it was last drawn.
    /// </summary>
    public bool IsUpdated => Renderable.VertexCountChanged || Renderable.HasTransformChanges || Renderable.HasVertexChanges || Renderable.HasVertexColorChanges || Renderable.HasTextureChanges;

    /// <summary>
    /// Property to set or return whether color codes are allowed or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The text sprite allows a developer to insert a special code into the <see cref="Text"/> string to enable coloring of individual letters or words. This code, which is similar to a bbcode is in 
    /// the format of [c #AARRGGBB] and is closed with a tag of [/c].  For example, to change a block of text to red, pass <c>[c #FFFF0000]Red[/c] text</c> to the <see cref="Text"/> property.
    /// </para>
    /// <para>
    /// Because enabling this value incurs more overhead for text rendering, it is defaulted to <b>false</b>.
    /// </para>
    /// </remarks>
    public bool AllowColorCodes
    {
        get => _allowCodes;
        set
        {
            if (_allowCodes == value)
            {
                return;
            }

            _allowCodes = value;
            Renderable.HasVertexColorChanges = true;

            if (Renderable.ColorBlocks.Count > 0)
            {
                Renderable.ColorBlocks.Clear();
            }

            if (_allowCodes)
            {
                _encodedText = _text;
                (string decodedText, List<ColorBlock> colorBlocks) = _parser.ParseColorCodes(_encodedText);
                Renderable.ColorBlocks.AddRange(colorBlocks);
                _text = decodedText;
            }
            else
            {
                _encodedText = string.Empty;
            }

            Renderable.TextLength = _text.Length;
            _formattedText.Length = 0;
            _formattedText.Append(_text);

            FormatText();
        }
    }

    /// <summary>
    /// Property to set or return the alignment for the text.
    /// </summary>
    /// <remarks>
    /// If the <see cref="LayoutArea"/> is defined, then it will be used to determine the layout of the text when aligning. Otherwise, the <see cref="Size"/> is used.
    /// </remarks>
    public Alignment Alignment
    {
        get => Renderable.Alignment;
        set
        {
            if (Renderable.Alignment == value)
            {
                return;
            }

            Renderable.Alignment = value;
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the layout area for the <see cref="Alignment"/> property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines a custom area to layout the text when changing its <see cref="Alignment"/>. By default, Gorgon will use the <see cref="Bounds"/> to determine the layout region for aligning text,
    /// but, when this property is defined, the layout region can be larger or smaller than the sprite <see cref="Bounds"/>.
    /// </para>
    /// </remarks>
    public DX.Size2F? LayoutArea
    {
        get => _layoutArea;
        set
        {
            if (_layoutArea == value)
            {
                return;
            }

            _layoutArea = value;
            UpdateBounds();
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return a mulitplier used to indicate how much space to put between each vertical line.
    /// </summary>
    public float LineSpace
    {
        get => Renderable.LineSpaceMultiplier;
        set
        {
            if (Renderable.LineSpaceMultiplier.EqualsEpsilon(value))
            {
                return;
            }

            Renderable.LineSpaceMultiplier = value;
            Renderable.HasTransformChanges = true;
            UpdateBounds();
        }
    }

    /// <summary>
    /// Property to set or return how to draw the text when rendering.
    /// </summary>
    /// <remarks>
    /// This parameter will not have any effect if the <see cref="Font"/> was not generated with an <see cref="IGorgonFontInfo.OutlineSize"/> greater than 0, and non transparent colors in
    /// <see cref="IGorgonFontInfo.OutlineColor1"/> and <see cref="IGorgonFontInfo.OutlineColor1"/>.
    /// </remarks>
    /// <seealso cref="IGorgonFontInfo"/>
    public TextDrawMode DrawMode
    {
        get => Renderable.DrawMode;
        set
        {
            if (Renderable.DrawMode == value)
            {
                return;
            }

            Renderable.DrawMode = value;
            UpdateBounds();
            // This will increase the amount of geometry required by a factor of 2 in the worst case.
            Renderable.VertexCountChanged = true;
        }
    }

    /// <summary>
    /// Property to set or return the number of spaces to substitute when rendering a tab (\t) control character.
    /// </summary>
    /// <remarks>
    /// The default value is 4.
    /// </remarks>
    public int TabSpaceCount
    {
        get => Renderable.TabSpaceCount;
        set
        {
            if (Renderable.TabSpaceCount == value)
            {
                return;
            }

            Renderable.TabSpaceCount = value;
            UpdateBounds();
        }
    }

    /// <summary>
    /// Property to return the individual lines of text within the <see cref="Text"/> property.
    /// </summary>
    public IReadOnlyList<string> Lines => Renderable.Lines;

    /// <summary>
    /// Property to set or return the text to render.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This sets or returns the text to render.
    /// </para>
    /// <para>
    /// This property will also allow the use of a color code (if the <see cref="AllowColorCodes"/> property is <b>true</b>). This code, which is similar to a bbcode is in the format of [c #AARRGGBB]
    /// and is closed with a tag of [/c].  For example, to change a block of text to red, pass <c>[c #FFFF0000]Red[/c] text</c> to the this property.
    /// </para>
    /// <para>
    /// Because the color codes add more overhead when rendering text, it is best to only use it sparingly for performance intensive scenarios.
    /// </para>
    /// </remarks>
    public string Text
    {
        get => _allowCodes ? _encodedText : _text;
        set
        {
            value ??= string.Empty;

            if (string.Equals(_allowCodes ? _encodedText : _text, value))
            {
                return;
            }

            if (Renderable.ColorBlocks.Count > 0)
            {
                Renderable.ColorBlocks.Clear();
            }

            if (_allowCodes)
            {
                _encodedText = value;
                (string decodedText, List<ColorBlock> colorBlocks) = _parser.ParseColorCodes(_encodedText);
                Renderable.ColorBlocks.AddRange(colorBlocks);
                _text = decodedText;
            }
            else
            {
                _encodedText = string.Empty;
                _text = value;
            }

            Renderable.TextLength = _text.Length;
            _formattedText.Length = 0;
            _formattedText.Append(_text);

            FormatText();
        }
    }

    /// <summary>
    /// Property to return the interface that allows colors to be assigned to each corner of an individual font glyph.
    /// </summary>
    public GorgonGlyphColors GlyphCornerColors
    {
        get;
    }

    /// <summary>
    /// Property to set or return the color of the sprite.
    /// </summary>
    /// <remarks>
    /// This sets the color for the entire sprite.  To assign colors to each corner of the sprite, use the <see cref="GlyphCornerColors"/> property.
    /// </remarks>
    public GorgonColor Color
    {
        get => GlyphCornerColors.UpperLeft;
        set => GlyphCornerColors.SetAll(in value);
    }

    /// <summary>
    /// Property to set or return the tint color for an outline.
    /// </summary>
    /// <remarks>
    /// This sets a tint color value for a text sprite with an outline. This parameter requires that the <see cref="DrawMode"/> property be set to <b>true</b>, and the <see cref="Font"/> has a
    /// <see cref="IGorgonFontInfo.OutlineSize"/> that is greater than 0, and a non transparent <see cref="IGorgonFontInfo.OutlineColor1"/>, and/or <see cref="IGorgonFontInfo.OutlineColor2"/>.
    /// </remarks>
    /// <seealso cref="IGorgonFontInfo"/>
    public GorgonColor OutlineTint
    {
        get => Renderable.OutlineTint;
        set
        {
            if (GorgonColor.Equals(in Renderable.OutlineTint, in value))
            {
                return;
            }

            Renderable.OutlineTint = value;
            Renderable.HasVertexColorChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the texture sampler to use when rendering.
    /// </summary>
    public GorgonSamplerState TextureSampler
    {
        get => Renderable.TextureSampler;
        set
        {
            BatchRenderable renderable = Renderable;
            if (renderable.TextureSampler == value)
            {
                return;
            }

            renderable.TextureSampler = value;
            renderable.StateChanged = true;
        }
    }

    /// <summary>
    /// Property to set or return the font to used for rendering text.
    /// </summary>
    public GorgonFont Font
    {
        get => Renderable.Font;
        set
        {
            if ((Renderable.Font == value)
                || (value is null))
            {
                return;
            }

            Renderable.Font = value;
            // Default to the first glyph texture.
            Renderable.Texture = value.Glyphs.FirstOrDefault(item => item.TextureView is not null)?.TextureView;
            Renderable.HasVertexChanges = true;
            Renderable.HasTextureChanges = true;
            Renderable.HasVertexColorChanges = true;
            Renderable.StateChanged = true;

            FormatText();
        }
    }

    /// <summary>
    /// Property to return the boundaries of the sprite.
    /// </summary>
    public DX.RectangleF Bounds => Renderable.Bounds;

    /// <summary>
    /// Property to set or return the position of the sprite.
    /// </summary>
    public Vector2 Position
    {
        get => new(Renderable.Bounds.Left, Renderable.Bounds.Top);
        set
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;

            if ((bounds.Left == value.X)
                && (bounds.Top == value.Y))
            {
                return;
            }

            bounds = new DX.RectangleF(value.X, value.Y, bounds.Width, bounds.Height);
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the depth value for this sprite.
    /// </summary>
    public float Depth
    {
        get => Renderable.Depth;
        set
        {
            if (Renderable.Depth.EqualsEpsilon(value))
            {
                return;
            }

            Renderable.Depth = value;
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the point around which the sprite will pivot when rotated.
    /// </summary>
    /// <remarks>
    /// This value is a relative value where 0, 0 means the upper left of the sprite, and 1, 1 means the lower right.
    /// </remarks>
    public Vector2 Anchor
    {
        get => Renderable.Anchor;
        set
        {
            ref Vector2 anchor = ref Renderable.Anchor;
            if ((anchor.X == value.X)
                && (anchor.Y == value.Y))
            {
                return;
            }

            anchor = value;
            Renderable.HasVertexChanges = true;
        }
    }

    /// <summary>
    /// Property to return the size of the sprite.
    /// </summary>
    public DX.Size2F Size => Renderable.Bounds.Size;

    /// <summary>
    /// Property to set or return the size of the renderable after scaling has been applied.
    /// </summary>
    /// <remarks>
    /// This property will set or return the actual size of the renderable.  This means that if a <see cref="Scale"/> has been set, then this property will return the size of the renderable with
    /// multiplied by the scale.  When assigning a value, the scale be set on value derived from the current size of the renderable.
    /// </remarks>
    public DX.Size2F ScaledSize
    {
        get
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;
            ref Vector2 scale = ref Renderable.Scale;
            return new DX.Size2F(scale.X * bounds.Width, scale.Y * bounds.Height);
        }
        set
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;
            ref Vector2 scale = ref Renderable.Scale;
            scale = new Vector2(value.Width / bounds.Width, value.Height / bounds.Height);
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the scale factor to apply to the sprite.
    /// </summary>
    public Vector2 Scale
    {
        get => Renderable.Scale;
        set
        {
            ref Vector2 scale = ref Renderable.Scale;
            if ((scale.X == value.X)
                && (scale.Y == value.Y))
            {
                return;
            }

            scale = value;
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the angle of rotation, in degrees.
    /// </summary>
    public float Angle
    {
        get => Renderable.AngleDegs;
        set
        {
            if (Renderable.AngleDegs == value)
            {
                return;
            }

            Renderable.AngleDegs = value;
        }
    }

    /// <summary>
    /// Property to set or return the alpha testing values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Alpha testing will skip rendering pixels based on the current alpha value for the pixel if it falls into the range specified by this property.
    /// </para>
    /// <para>
    /// To disable alpha testing outright, set this property to <b>null</b>.
    /// </para>
    /// </remarks>
    public GorgonRangeF? AlphaTest
    {
        get
        {
            BatchRenderable renderable = Renderable;
            return renderable.AlphaTestData.IsEnabled == 0
                ? null
                : new GorgonRangeF(renderable.AlphaTestData.LowerAlpha, renderable.AlphaTestData.UpperAlpha);
        }
        set
        {
            BatchRenderable renderable = Renderable;
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (value is null)
            {
                if (renderable.AlphaTestData.IsEnabled == 0)
                {
                    return;
                }

                renderable.AlphaTestData = new AlphaTestData(false, new GorgonRangeF(renderable.AlphaTestData.LowerAlpha, renderable.AlphaTestData.UpperAlpha));
                renderable.StateChanged = true;
                return;
            }

            renderable.AlphaTestData = new AlphaTestData(true, value.Value);
            renderable.StateChanged = true;
        }
    }



    /// <summary>
    /// Function to update the boundaries on the text sprite.
    /// </summary>
    private void UpdateBounds()
    {
        DX.Size2F size = _formattedText.ToString().MeasureText(Renderable.Font,
                                                     DrawMode != TextDrawMode.GlyphsOnly,
                                                     Renderable.TabSpaceCount,
                                                     Renderable.LineSpaceMultiplier);

        Renderable.Bounds = new DX.RectangleF(Renderable.Bounds.Left, Renderable.Bounds.Top, size.Width, size.Height);
        Renderable.LayoutArea = _layoutArea ?? size;
    }

    /// <summary>
    /// Function to format the text applied.
    /// </summary>
    private void FormatText()
    {
        if (_formattedText.Length == 0)
        {
            Renderable.Lines = [];
            return;
        }

        _formattedText.GetLines(ref Renderable.Lines);
        UpdateBounds();

        int estimatedVertexCount = _formattedText.Length * (Renderable.DrawMode == TextDrawMode.OutlinedGlyphs ? 8 : 4);

        Renderable.HasVertexChanges = true;
        Renderable.VertexCountChanged = (Renderable.Vertices is null) || (estimatedVertexCount > Renderable.Vertices.Length);
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextSprite"/> class.
    /// </summary>
    /// <param name="font">The font used to render the text.</param>
    /// <param name="text">[Optional] The text to render.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
    public GorgonTextSprite(GorgonFont font, string text = null)
    {
        Font = font ?? throw new ArgumentNullException(nameof(font));
        Text = text ?? string.Empty;
        GlyphCornerColors = new GorgonGlyphColors(GorgonColor.White, Renderable);
    }

}
