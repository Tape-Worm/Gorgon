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
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A class that defines a rectangluar region to display a 2D image.
    /// </summary>
    public class GorgonSprite
    {
        #region Constants.
        // The property name for the header value.
        private const string JsonHeaderProp = "header";
        // The property name for the header value.
        private const string JsonVersionProp = "version";
        
        /// <summary>
        /// The header for a sprite file.
        /// </summary>
        public const string FileHeaderValue = "GORSPR30";
        #endregion

        #region Variables.
        // The angle of rotation, in degrees.
        private float _angle;
        // The renderable data for this sprite.
        // It is exposed as an internal variable (which goes against C# best practices) for performance reasons (property accesses add up over time).
        internal readonly BatchRenderable Renderable = new BatchRenderable();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the header for the sprite when serializing/deserializing.
        /// </summary>
        [JsonProperty(JsonHeaderProp)]
        private ulong Header => FileHeaderValue.ChunkID();

        /// <summary>
        /// Property to return the version number for serialization.
        /// </summary>
        [JsonProperty(JsonVersionProp)]
        public static Version SerializationVersion
        {
            get;
        } = new Version(3, 0);

        /// <summary>
        /// Property to return whether or not the sprite has had its position, size, texture information, or object space vertices updated since it was last drawn.
        /// </summary>
        [JsonIgnore]
        public bool IsUpdated => Renderable.HasTextureChanges 
                                 || Renderable.HasTransformChanges 
                                 || Renderable.HasVertexChanges 
                                 || Renderable.HasVertexColorChanges;

        /// <summary>
        /// Property to return the interface that allows colors to be assigned to each corner of the sprite.
        /// </summary>
        public GorgonRectangleColors CornerColors
        {
            get;
        }

        /// <summary>
        /// Property to set or return the color of the sprite.
        /// </summary>
        /// <remarks>
        /// This sets the color for the entire sprite.  To assign colors to each corner of the sprite, use the <see cref="CornerColors"/> property.
        /// </remarks>
        [JsonIgnore]
        public GorgonColor Color
        {
            get => Renderable.UpperLeftColor;
            set => CornerColors.SetAll(in value);
        }

        /// <summary>
        /// Property to return the interface that allows an offset to be applied to each corner of the sprite.
        /// </summary>
        public GorgonRectangleOffsets CornerOffsets
        {
            get;
        }

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get => Renderable.Texture;
            set
            {
                if (Renderable.Texture == value)
                {
                    return;
                }

                Renderable.Texture = value;
                Renderable.StateChanged = true;
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
                if (Renderable.TextureSampler == value)
                {
                    return;
                }

                Renderable.TextureSampler = value;
                Renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the boundaries of the sprite.
        /// </summary>
        [JsonIgnore]
        public DX.RectangleF Bounds
        {
            get => Renderable.Bounds;
            set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;

                if ((bounds.Left == value.Left) 
                    && (bounds.Right == value.Right)
                    && (bounds.Top == value.Top)
                    && (bounds.Bottom == value.Bottom))
                {
                    return;
                }

                bounds = value;
                Renderable.HasVertexChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the position of the sprite.
        /// </summary>
        [JsonIgnore]
        public DX.Vector2 Position
        {
            get => Renderable.Bounds.TopLeft;
            set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                if ((bounds.X == value.X)
                    && (bounds.Y == value.Y))
                {
                    return;
                }

                bounds.X = value.X;
                bounds.Y = value.Y;
                Renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the depth value for this sprite.
        /// </summary>
        [JsonIgnore]
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
        /// Property to set or return the size of the sprite.
        /// </summary>
        public DX.Size2F Size
        {
            get => Bounds.Size;
            set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                if ((bounds.Size.Width == value.Width)
                    && (bounds.Size.Height == value.Height))
                {
                    return;
                }

                bounds = new DX.RectangleF(Bounds.X, Bounds.Y, value.Width, value.Height);
                Renderable.HasVertexChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the region of the texture to use when drawing the sprite.
        /// </summary>
        /// <remarks>
        /// These values are in texel coordinates.
        /// </remarks>
        public DX.RectangleF TextureRegion
        {
            get => Renderable.TextureRegion;
            set
            {
                ref DX.RectangleF region = ref Renderable.TextureRegion;
                if ((region.Left == value.Left)
                    && (region.Top == value.Top)
                    && (region.Right == value.Right)
                    && (region.Bottom == value.Bottom))
                {
                    return;
                }

                region = value;
                Renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return which index within a texture array to use.
        /// </summary>
        public int TextureArrayIndex
        {
            get => Renderable.TextureArrayIndex;
            set
            {
                if (Renderable.TextureArrayIndex == value)
                {
                    return;
                }

                Renderable.TextureArrayIndex = value;
                Renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the size of the renderable after scaling has been applied.
        /// </summary>
        /// <remarks>
        /// This property will set or return the actual size of the renderable.  This means that if a <see cref="Scale"/> has been set, then this property will return the size of the renderable with
        /// multiplied by the scale.  When assigning a value, the scale be set on value derived from the current size of the renderable.
        /// </remarks>
        [JsonIgnore]
        public DX.Size2F ScaledSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                ref DX.Vector2 scale = ref Renderable.Scale;
                return new DX.Size2F(scale.X * bounds.Width, scale.Y * bounds.Height);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [JsonIgnore]
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
        [JsonIgnore]
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
                float rads = value.ToRadians();
                Renderable.AngleRads = rads;
                Renderable.AngleSin = rads.FastSin();
                Renderable.AngleCos = rads.FastCos();
                Renderable.HasTransformChanges = true;
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
                if (Renderable.AlphaTestData.IsEnabled == 0)
                {
                    return null;
                }

                return new GorgonRangeF(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha);
            }
            set
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (value == null)
                {
                    if (Renderable.AlphaTestData.IsEnabled == 0)
                    {
                        return;
                    }

                    Renderable.AlphaTestData = new AlphaTestData(false, new GorgonRangeF(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha));
                    Renderable.StateChanged = true;
                    return;
                }

                Renderable.AlphaTestData = new AlphaTestData(true, value.Value);
                Renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return whether the sprite texture is flipped horizontally.
        /// </summary>
        /// <remarks>
        /// This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.
        /// </remarks>
        public bool HorizontalFlip
        {
            get => Renderable.HorizontalFlip;
            set
            {
                if (value == Renderable.HorizontalFlip)
                {
                    return;
                }

                Renderable.HorizontalFlip = value;
                Renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return whether the sprite texture is flipped vertically.
        /// </summary>
        /// <remarks>
        /// This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.
        /// </remarks>
        public bool VerticalFlip
        {
            get => Renderable.VerticalFlip;
            set
            {
                if (value == VerticalFlip)
                {
                    return;
                }

                Renderable.VerticalFlip = value;
                Renderable.HasTextureChanges = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to convert a JSON string into a sprite object.
        /// </summary>
        /// <param name="renderer">The renderer for the sprite.</param>
        /// <param name="json">The JSON string containing the sprite data.</param>
        /// <returns>A new <see cref="GorgonSprite"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="json"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="json"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the JSON string does not contain sprite data, or there is a version mismatch.</exception>
        public static GorgonSprite FromJson(Gorgon2D renderer, string json)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentEmptyException(nameof(json));
            }

            // Set up serialization so we can convert our more complicated structures.
            var serializer = new JsonSerializer
                             {
                                 CheckAdditionalContent = false
                             };

            serializer.Converters.Add(new JsonVector2Converter());
            serializer.Converters.Add(new JsonVector3Converter());
            serializer.Converters.Add(new JsonSize2FConverter());
            serializer.Converters.Add(new JsonRectangleFConverter());
            serializer.Converters.Add(new JsonSamplerConverter(renderer.Graphics));
            serializer.Converters.Add(new JsonTexture2DConverter(renderer.Graphics));
            serializer.Converters.Add(new VersionConverter());

            // Parse the string so we can extract our header/version for comparison.
            JObject jobj = JObject.Parse(json);
            ulong jsonID = jobj[JsonHeaderProp].Value<ulong>();
            Version jsonVersion = jobj[JsonVersionProp].ToObject<Version>(serializer);

            if (jsonID != FileHeaderValue.ChunkID())
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2D_ERR_JSON_NOT_SPRITE);
            }

            if (!jsonVersion.Equals(SerializationVersion))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2D_ERR_SPRITE_VERSION_MISMATCH, SerializationVersion, jsonVersion));
            }

            return jobj.ToObject<GorgonSprite>(serializer);
        }

        /// <summary>
        /// Function to convert the data in the sprite to a JSON formatted string.
        /// </summary>
        /// <returns>The sprite data as a JSON string.</returns>
        public string ToJson(bool prettyFormat = false)
        {
            var settings = new JsonSerializerSettings
                           {
                               CheckAdditionalContent = false,
                               Formatting = prettyFormat ? Formatting.Indented : Formatting.None
                           };

            settings.Converters.Add(new JsonVector2Converter());
            settings.Converters.Add(new JsonVector3Converter());
            settings.Converters.Add(new JsonSize2FConverter());
            settings.Converters.Add(new JsonRectangleFConverter());
            settings.Converters.Add(new JsonSamplerConverter(null));
            settings.Converters.Add(new JsonTexture2DConverter(null));
            settings.Converters.Add(new VersionConverter());

            return JsonConvert.SerializeObject(this, settings);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSprite"/> class.
        /// </summary>
        public GorgonSprite()
        {
            CornerColors = new GorgonRectangleColors(GorgonColor.White, Renderable);
            CornerOffsets = new GorgonRectangleOffsets(Renderable);

            Renderable.Vertices = new Gorgon2DVertex[4];
            Renderable.ActualVertexCount = 4;
            Renderable.IndexCount = 6;

            for (int i = 0; i < Renderable.Vertices.Length; ++i)
            {
                Renderable.Vertices[i].Position.W = 1.0f;
            }
        }
        #endregion
    }
}
