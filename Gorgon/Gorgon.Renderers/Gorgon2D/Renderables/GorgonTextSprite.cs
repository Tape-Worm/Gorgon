#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 7, 2018 3:13:51 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Determines how text should be rendered.
    /// </summary>
    public enum TextDrawMode
    {
        /// <summary>
        /// <para>
        /// Draw both the glyphs and the outlines.
        /// </para> 
        /// <para>
        /// This is only supported when the <see cref="GorgonFont"/> has an outline.
        /// </para>
        /// </summary>
        OutlinedGlyphs = 0,
        /// <summary>
        /// Draw the glyphs only.
        /// </summary>
        GlyphsOnly = 1,
        /// <summary>
        /// <para>
        /// Draw the outlines only.
        /// </para>
        /// <para>
        /// This is only supported when the <see cref="GorgonFont"/> has an outline.
        /// </para>
        /// </summary>
        OutlineOnly = 2
    }
    
    /// <summary>
    /// A class that defines a sprite that is used to display text.
    /// </summary>
    public class GorgonTextSprite
    {
        #region Variables.
        // The angle of rotation, in degrees.
        private float _angle;
        // The font used to render the text.
        private GorgonFont _font;
        // The text to render.
        private string _text;
        // The formatted text.
        private string _formattedText;
        // Flag to indicate that we are using an outline from the font.
        private TextDrawMode _drawMode;
        // The tint color for the outline.
        private GorgonColor _outlineTint = GorgonColor.White;
        // The number of spaces to use when a tab character is encountered.
        private int _tabSpaceCount = 4;
        // The multiplier for spacing between lines.
        private float _lineSpacing = 1.0f;
        // Flag to indicate that word wrapping is enabled.
        private bool _useWordWrap;
        // The area for used for text layout.
        private DX.Size2F? _layoutArea;
        // The alignment of the text.
        private Alignment _alignment = Alignment.UpperLeft;
        // The lines of text in the sprite text property.
        private string[] _lines;

        /// <summary>
        /// The renderable data for this sprite.
        /// It is exposed an internal variable (which goes against C# best practices) for performance reasons (property accesses add up over time).
        /// </summary>
        internal readonly BatchRenderable Renderable = new BatchRenderable();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the alignment for the text.
        /// </summary>
        /// <remarks>
        /// This property requires that the <see cref="LayoutArea"/> be assigned with a value. Otherwise, it will be ignored.
        /// </remarks>
        public Alignment Alignment
        {
            get => _alignment;
            set
            {
                if (_alignment == value)
                {
                    return;
                }

                _alignment = value;
                UpdateBounds();
                Renderable.HasVertexChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return whether or not word wrapping is enabled.
        /// </summary>
        /// <remarks>
        /// If this property is set to <b>true</b>, then the <see cref="LayoutArea"/> must be assigned, otherwise there will be no cut off point for breaking the line.
        /// </remarks>
        public bool UseWordWrapping
        {
            get => _useWordWrap;
            set
            {
                if (_useWordWrap == value)
                {
                    return;
                }

                _useWordWrap = value;
                UpdateBounds();
                Renderable.HasVertexChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the layout area for <see cref="UseWordWrapping"/>, and/or the <see cref="Alignment"/> property.
        /// </summary>
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
                Renderable.HasVertexChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return a mulitplier used to indicate how much space to put between each vertical line.
        /// </summary>
        public float LineSpace
        {
            get => _lineSpacing;
            set
            {
                if (_lineSpacing.EqualsEpsilon(value))
                {
                    return;
                }

                _lineSpacing = value;
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
            get => _drawMode;
            set
            {
                if (_drawMode == value)
                {
                    return;
                }

                _drawMode = value;
                UpdateBounds();
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
            get => _tabSpaceCount;
            set
            {
                if (_tabSpaceCount == value)
                {
                    return;
                }

                _tabSpaceCount = value;
                FormatText();
            }
        }

        /// <summary>
        /// Property to return the individual lines of text within the <see cref="Text"/> property.
        /// </summary>
        public IReadOnlyList<string> Lines => _lines;

        /// <summary>
        /// Property to set or return the text to render.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (string.Equals(_text, value))
                {
                    return;
                }
                
                _text = value;
                Renderable.VertexCountChanged = (Renderable.Vertices == null) || ((value.Length * (DrawMode == TextDrawMode.OutlinedGlyphs ? 8 : 4)) >= Renderable.Vertices.Length);
                Renderable.HasVertexChanges = true;

                FormatText();
            }
        }

        /// <summary>
        /// Property to return whether or not the sprite has had its position, size, texture information, or object space vertices updated since it was last drawn.
        /// </summary>
        public bool IsUpdated => Renderable.VertexCountChanged || Renderable.HasTransformChanges || Renderable.HasVertexChanges || Renderable.HasVertexColorChanges || Renderable.HasTextureChanges;

        /// <summary>
        /// Property to return the interface that allows colors to be assigned to each corner of an individual font glyph.
        /// </summary>
        public GorgonRectangleColors GlyphCornerColors
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
            get => _outlineTint;
            set
            {
                if (GorgonColor.Equals(in value, in _outlineTint))
                {
                    return;
                }

                _outlineTint = value;
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
            get => _font;
            set
            {
                // TODO: Perhaps have a default font and NULL will be used to represent that.
                if ((_font == value)
                    || (value == null))
                {
                    return;
                }

                _font = value;
                Renderable.HasVertexChanges = true;
                Renderable.HasTextureChanges = true;
                Renderable.HasVertexColorChanges = true;

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
        public DX.Vector2 Position
        {
            get => Renderable.Bounds.TopLeft;
            set
            {
                if ((Renderable.Bounds.X == value.X)
                    && (Renderable.Bounds.Y == value.Y))
                {
                    return;
                }

                Renderable.Bounds.X = value.X;
                Renderable.Bounds.Y = value.Y;
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
        public DX.Vector2 Anchor
        {
            get => Renderable.Anchor;
            set
            {
                ref DX.Vector2 anchor = ref Renderable.Anchor;
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
                ref DX.Vector2 scale = ref Renderable.Scale;
                return new DX.Size2F(scale.X * bounds.Width, scale.Y * bounds.Height);
            }
            set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                ref DX.Vector2 scale = ref Renderable.Scale;
                scale = new DX.Vector2(value.Width / bounds.Width, value.Height / bounds.Height);
                Renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the scale factor to apply to the sprite.
        /// </summary>
        public DX.Vector2 Scale
        {
            get => Renderable.Scale;
            set
            {
                ref DX.Vector2 scale = ref Renderable.Scale;
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
            get => _angle;
            set
            {
                if (_angle == value)
                {
                    return;
                }

                _angle = value;
                Renderable.AngleRads = _angle.ToRadians();
                Renderable.AngleSin = Renderable.AngleRads.Sin();
                Renderable.AngleCos = Renderable.AngleRads.Cos();
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
                if (renderable.AlphaTestData.IsEnabled == 0)
                {
                    return null;
                }

                return new GorgonRangeF(renderable.AlphaTestData.LowerAlpha, renderable.AlphaTestData.UpperAlpha);
            }
            set
            {
                BatchRenderable renderable = Renderable;
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (value == null)
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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the boundaries on the text sprite.
        /// </summary>
        private void UpdateBounds()
        {
            float? layoutWidth = UseWordWrapping ? _layoutArea?.Width : null;
            DX.Size2F size = _font.MeasureText(_formattedText, DrawMode != TextDrawMode.GlyphsOnly, _tabSpaceCount, _lineSpacing, layoutWidth);
            Renderable.Bounds = new DX.RectangleF(Renderable.Bounds.Left, Renderable.Bounds.Top, size.Width, size.Height);
        }

        /// <summary>
        /// Function to format the text applied.
        /// </summary>
        private void FormatText()
        {
            if (_text.Length == 0)
            {
                _lines = new string[0];
                return;
            }

            string text = _text;
            text = text.FormatStringForRendering(_tabSpaceCount);

            if ((_useWordWrap) && (_layoutArea != null))
            {
                text = _font.WordWrap(text, _layoutArea.Value.Width);
            }

            _formattedText = text;
            _lines = text.GetLines();

            UpdateBounds();

            Renderable.HasVertexChanges = true;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTextSprite"/> class.
        /// </summary>
        /// <param name="font">The font used to render the text.</param>
        /// <param name="text">[Optional] The text to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
        public GorgonTextSprite(GorgonFont font, string text = null)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            Text = text ?? string.Empty;
            GlyphCornerColors = new GorgonRectangleColors(GorgonColor.White, Renderable);
        }
        #endregion
    }
}
