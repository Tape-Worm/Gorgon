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
using Gorgon.Core;
using Gorgon.Graphics;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;

namespace Gorgon.Renderers
{
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

        /// <summary>
        /// The renderable data for this sprite.
        /// It is exposed an internal variable (which goes against C# best practices) for performance reasons (property accesses add up over time).
        /// </summary>
        internal readonly BatchRenderable Renderable = new BatchRenderable();
        #endregion

        #region Properties.
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
                Renderable.VertexCountChanged = (Renderable.Vertices == null) || ((value.Length * 4) >= Renderable.Vertices.Length);
                Renderable.HasTransformChanges = true;
                Renderable.HasTextureChanges = true;
                Renderable.HasVertexChanges = true;
                Renderable.HasVertexColorChanges = true;

                DX.Size2F size = _font.MeasureText(_text, false, 4, 1, null);
                Renderable.Bounds = new DX.RectangleF(Renderable.Bounds.Left, Renderable.Bounds.Top, size.Width, size.Height);
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
