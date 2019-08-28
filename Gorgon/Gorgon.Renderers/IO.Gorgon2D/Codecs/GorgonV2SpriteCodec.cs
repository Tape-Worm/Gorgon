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
// Created: August 11, 2018 3:43:13 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using GorgonLibrary.IO;
using DX = SharpDX;

namespace Gorgon.IO
{
    /// <summary>
    /// A codec that can read version 2 sprite data.
    /// </summary>
    public class GorgonV2SpriteCodec
        : GorgonSpriteCodecCommon
    {
        #region Enums.
        /// <summary>
        /// Filtering to apply to a texture.
        /// </summary>
        [Flags]
        private enum TextureFilter
        {
            /// <summary>
            /// No filter.  This is equivalent to the Point value.
            /// </summary>
            None = 0,
            /// <summary>
            /// Point minification filtering.
            /// </summary>
            MinPoint = 1,
            /// <summary>
            /// Point magnifcation filtering.
            /// </summary>
            MagPoint = 2,
            /// <summary>
            /// Linear minification filtering.
            /// </summary>
            MinLinear = 4,
            /// <summary>
            /// Linear magnifcation filtering.
            /// </summary>
            MagLinear = 8,
            /// <summary>
            /// Mip map point sampling.
            /// </summary>
            MipPoint = 16,
            /// <summary>
            /// Mip map linear sampling.
            /// </summary>
            MipLinear = 32,
            /// <summary>
            /// Compare the result to the comparison value.
            /// </summary>
            Comparison = 64,
            /// <summary>
            /// Anisotropic filtering.
            /// </summary>
            /// <remarks>This flag is mutually exclusive and applies to minification, magnification and mip mapping.</remarks>
            Anisotropic = 65536,
            /// <summary>
            /// Anisotropic filtering with comparison.
            /// </summary>
            /// <remarks>This flag is mutually exclusive and applies to minification, magnification and mip mapping.</remarks>
            CompareAnisotropic = 131072,
        }
        #endregion

        #region Constants.
        // Sprite texture data chunk.
        private const string TextureDataChunk = "TXTRDATA";
        // Sprite render data chunk.
        private const string RenderDataChunk = "RNDRDATA";
        // Sprite data chunk.
        private const string SpriteDataChunk = "SPRTDATA";

        /// <summary>
        /// Header for the Gorgon sprite file.
        /// </summary>		
        public const string FileHeader = "GORSPR20";
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the codec can decode sprite data.
        /// </summary>
        public override bool CanDecode => true;

        /// <summary>
        /// Property to return whether or not the codec can encode sprite data.
        /// </summary>
        public override bool CanEncode => false;

        /// <summary>
        /// Property to return the version of sprite data that the codec supports.
        /// </summary>
        public override Version Version
        {
            get;
        } = new Version(2, 0);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to convert Gorgon 2.x texture filtering to 3.x texture filtering values.
        /// </summary>
        /// <param name="filter">Texture filtering value to convert.</param>
        /// <returns>Texture filtering value.</returns>
        private static SampleFilter ConvertV2TextureFilterToFilter(TextureFilter filter)
        {
            SampleFilter result = SampleFilter.MinMagMipPoint;

            switch (filter)
            {
                case TextureFilter.Anisotropic:
                    result = SampleFilter.Anisotropic;
                    break;
                case TextureFilter.CompareAnisotropic:
                    result = SampleFilter.ComparisonAnisotropic;
                    break;
            }

            // Sort out filter stateType.
            // Check comparison stateType.
            if ((filter & TextureFilter.Comparison) == TextureFilter.Comparison)
            {
                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.ComparisonMinMagMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.ComparisonMinMagMipPoint;
                }

                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.ComparisonMinMagLinearMipPoint;
                }

                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.ComparisonMinLinearMagMipPoint;
                }

                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.ComparisonMinLinearMagPointMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.ComparisonMinPointMagMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.ComparisonMinMagPointMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.ComparisonMinPointMagLinearMipPoint;
                }
            }
            else
            {
                if ((filter == TextureFilter.None)
                    || (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear)
                        && ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear)
                        && ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear)))
                {
                    result = SampleFilter.MinMagMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.MinMagMipPoint;
                }

                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.MinMagLinearMipPoint;
                }

                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.MinLinearMagMipPoint;
                }

                if (((filter & TextureFilter.MinLinear) == TextureFilter.MinLinear) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.MinLinearMagPointMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.MinPointMagMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagPoint) == TextureFilter.MagPoint) &&
                    ((filter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
                {
                    result = SampleFilter.MinMagPointMipLinear;
                }

                if (((filter & TextureFilter.MinPoint) == TextureFilter.MinPoint) &&
                    ((filter & TextureFilter.MagLinear) == TextureFilter.MagLinear) &&
                    ((filter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
                {
                    result = SampleFilter.MinPointMagLinearMipPoint;
                }
            }

            return result;
        }

        /// <summary>
		/// Function to convert Gorgon 1.x image addressing values to 2.x texture addressing values.
		/// </summary>
		/// <param name="imageAddress">Image addressing values.</param>
		/// <returns>Texture addressing values.</returns>
		private static TextureWrap ConvertV2TextureWrapToTextureAddress(int imageAddress)
        {
            switch (imageAddress)
            {
                case 1:
                    return TextureWrap.Wrap;
                case 2:
                    return TextureWrap.Mirror;
                case 5:
                    return TextureWrap.MirrorOnce;
                case 4:
                    return TextureWrap.Border;
                default:
                    return TextureWrap.Clamp;
            }
        }

        /// <summary>
        /// Function to build a sampler state from the information provided by the sprite data.
        /// </summary>
        /// <param name="graphics">The graphics interface to use.</param>
        /// <param name="filter">The filter to use.</param>
        /// <param name="borderColor">The color of the border to use.</param>
        /// <param name="hWrap">Horizontal wrapping mode.</param>
        /// <param name="vWrap">Vertical wrapping mode.</param>
        /// <returns>The sampler state.</returns>
	    private static GorgonSamplerState CreateSamplerState(GorgonGraphics graphics, SampleFilter filter, GorgonColor borderColor, TextureWrap hWrap, TextureWrap vWrap)
        {
            var builder = new GorgonSamplerStateBuilder(graphics);

            switch (filter)
            {
                case SampleFilter.MinMagMipLinear when (hWrap == TextureWrap.Clamp) && (vWrap == TextureWrap.Clamp) && (borderColor == GorgonColor.White):
                    return null;
                case SampleFilter.MinMagMipPoint when (hWrap == TextureWrap.Clamp) && (vWrap == TextureWrap.Clamp) && (borderColor == GorgonColor.White):
                    return GorgonSamplerState.PointFiltering;
                case SampleFilter.MinMagMipLinear when (hWrap == TextureWrap.Wrap) && (vWrap == TextureWrap.Wrap) && (borderColor == GorgonColor.White):
                    return GorgonSamplerState.Wrapping;
                case SampleFilter.MinMagMipPoint when (hWrap == TextureWrap.Wrap) && (vWrap == TextureWrap.Wrap) && (borderColor == GorgonColor.White):
                    return GorgonSamplerState.PointFilteringWrapping;
                default:
                    return builder.Wrapping(hWrap, vWrap, borderColor: borderColor)
                                  .Filter(filter)
                                  .Build();
            }
        }

        /// <summary>
        /// Function to load a version 1.x Gorgon sprite.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create states.</param>
        /// <param name="reader">Binary reader to use to read in the data.</param>
        /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
        /// <returns>The sprite from the stream data.</returns>
        private static GorgonSprite LoadSprite(GorgonGraphics graphics, GorgonChunkReader reader, GorgonTexture2DView overrideTexture)
        {
            var sprite = new GorgonSprite();

            if (!reader.HasChunk(FileHeader))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_INVALID_HEADER);
            }

            reader.Begin(FileHeader);
            reader.Begin(SpriteDataChunk);

            sprite.Anchor = reader.Read<DX.Vector2>();
            sprite.Size = reader.Read<DX.Size2F>();
            sprite.Anchor = new DX.Vector2(sprite.Anchor.X / sprite.Size.Width, sprite.Anchor.Y / sprite.Size.Height);

            sprite.HorizontalFlip = reader.ReadBoolean();
            sprite.VerticalFlip = reader.ReadBoolean();

            // Read vertex colors.
            sprite.CornerColors.UpperLeft = reader.Read<GorgonColor>();
            sprite.CornerColors.UpperRight = reader.Read<GorgonColor>();
            sprite.CornerColors.LowerLeft = reader.Read<GorgonColor>();
            sprite.CornerColors.LowerRight = reader.Read<GorgonColor>();

            // Write vertex offsets.
            sprite.CornerOffsets.UpperLeft = new DX.Vector3(reader.Read<DX.Vector2>(), 0);
            sprite.CornerOffsets.UpperRight = new DX.Vector3(reader.Read<DX.Vector2>(), 0);
            sprite.CornerOffsets.LowerLeft = new DX.Vector3(reader.Read<DX.Vector2>(), 0);
            sprite.CornerOffsets.LowerRight = new DX.Vector3(reader.Read<DX.Vector2>(), 0);

            reader.End();

            // Read rendering information.
            reader.Begin(RenderDataChunk);

            // Culling mode is not per-sprite anymore.
            reader.SkipBytes(Unsafe.SizeOf<CullingMode>());
            sprite.AlphaTest = reader.Read<GorgonRangeF>();

            // Blending values are not per-sprite anymore.
            // Depth/stencil values are not per-sprite anymore.
            reader.SkipBytes(91);
            reader.End();

            // Read texture information.
            reader.Begin(TextureDataChunk);
            GorgonColor borderColor = reader.Read<GorgonColor>();

            TextureWrap hWrap = ConvertV2TextureWrapToTextureAddress(reader.Read<int>());
            TextureWrap vWrap = ConvertV2TextureWrapToTextureAddress(reader.Read<int>());
            SampleFilter filter = ConvertV2TextureFilterToFilter(reader.Read<TextureFilter>());
            string textureName = reader.ReadString();
            GorgonTexture2DView textureView;

            // Bind the texture (if we have one bound to this sprite) if it's already loaded, otherwise defer it.
            if ((!string.IsNullOrEmpty(textureName)) && (overrideTexture == null))
            {
                GorgonTexture2D texture = graphics.LocateResourcesByName<GorgonTexture2D>(textureName).FirstOrDefault();

                // If we used the editor build to sprite, the path to the texture is stored in the name instead of just the name.
                // So let's try and strip out the path information and extension and try again.
                if (texture == null)
                {
                    textureName = Path.GetFileNameWithoutExtension(textureName);
                    texture = graphics.LocateResourcesByName<GorgonTexture2D>(textureName).FirstOrDefault();
                }

                textureView = texture?.GetShaderResourceView();
            }
            else
            {
                textureView = overrideTexture;
            }

            sprite.TextureRegion = reader.ReadRectangleF();

            if (textureView != null)
            {
                // V2 used black transparent by default, so convert it to our default so we can keep from creating unnecessary states.
                if (borderColor == GorgonColor.BlackTransparent)
                {
                    borderColor = GorgonColor.White;
                }

                sprite.Texture = textureView;
                sprite.TextureSampler = CreateSamplerState(graphics, filter, borderColor, hWrap, vWrap);
            }

            reader.End();
            reader.End();

            return sprite;
        }

        /// <summary>
        /// Function to determine if the data in a stream is readable by this codec.
        /// </summary>
        /// <param name="stream">The stream containing the data.</param>
        /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
        protected override bool OnIsReadable(Stream stream)
        {
            using (var reader = new GorgonBinaryReader(stream, true))
            {
                if ((stream.Length - stream.Position) < sizeof(ulong) * 2)
                {
                    return false;
                }

                ulong chunkHeader = reader.ReadUInt64();
                // Skip the size, we don't need it.
                reader.ReadUInt32();
                ulong chunkFileData = reader.ReadUInt64();

                return (chunkHeader == FileHeader.ChunkID()) && (chunkFileData == SpriteDataChunk.ChunkID());
            }
        }

        /// <summary>
        /// Function to retrieve the name of the associated texture.
        /// </summary>
        /// <param name="stream">The stream containing the texture data.</param>
        /// <returns>The name of the texture associated with the sprite, or <b>null</b> if no texture was found.</returns>
        protected override string OnGetAssociatedTextureName(Stream stream)
        {
            using (var reader = new GorgonChunkReader(stream))
            {
                if (!reader.HasChunk(FileHeader))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_INVALID_HEADER);
                }

                reader.Begin(FileHeader);
                reader.Begin(SpriteDataChunk);
                reader.SkipBytes(DX.Vector2.SizeInBytes
                                 + Unsafe.SizeOf<DX.Size2F>()
                                 + (sizeof(bool) * 2)
                                 + (GorgonColor.SizeInBytes * 4)
                                 + (DX.Vector2.SizeInBytes * 4));
                reader.End();

                // Read rendering information.
                reader.Begin(RenderDataChunk);
                reader.SkipBytes(Unsafe.SizeOf<CullingMode>() + Unsafe.SizeOf<GorgonRangeF>() + 91);
                reader.End();

                // Read texture information.
                reader.Begin(TextureDataChunk);
                reader.SkipBytes(GorgonColor.SizeInBytes + (sizeof(int) * 2) + Unsafe.SizeOf<TextureFilter>());
                return reader.ReadString();
            }
        }

        /// <summary>
        /// Function to read the sprite data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the sprite.</param>
        /// <param name="byteCount">The number of bytes to read from the stream.</param>
        /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
        /// <returns>A new <see cref="GorgonSprite"/>.</returns>
        protected override GorgonSprite OnReadFromStream(Stream stream, int byteCount, GorgonTexture2DView overrideTexture)
        {
            var reader = new GorgonChunkReader(stream);

            try
            {
                // We don't need byte count for this.
                return LoadSprite(Graphics, reader, overrideTexture);
            }
            finally
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Function to save the sprite data to a stream.
        /// </summary>
        /// <param name="sprite">The sprite to serialize into the stream.</param>
        /// <param name="stream">The stream that will contain the sprite.</param>
        protected override void OnSaveToStream(GorgonSprite sprite, Stream stream) => throw new NotSupportedException();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV2SpriteCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used for resource handling.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public GorgonV2SpriteCodec(Gorgon2D renderer)
            : base(renderer, Resources.GOR2DIO_V2_CODEC, Resources.GOR2DIO_V2_CODEC_DESCRIPTION)
        {
        }
        #endregion
    }
}
