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
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using GorgonLibrary.IO;

namespace Gorgon.IO.Codecs
{
    /// <summary>
    /// A codec that can read version 2 sprite data.
    /// </summary>
    public class GorgonV2SpriteCodec
        : IGorgonSpriteCodec
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
        /// Property to return the renderer used to create objects.
        /// </summary>
        public Gorgon2D Renderer
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the codec can decode sprite data.
        /// </summary>
        public bool CanDecode => true;

        /// <summary>
        /// Property to return whether or not the codec can encode sprite data.
        /// </summary>
        public bool CanEncode => false;

        /// <summary>
        /// Property to return the version of sprite data that the codec supports.
        /// </summary>
        public Version Version
        {
            get;
        } = new Version(2, 0);

        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics => Renderer.Graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to convert Gorgon 2.x texture filtering to 3.x texture filtering values.
        /// </summary>
        /// <param name="filter">Texture filtering value to convert.</param>
        /// <returns>Texture filtering value.</returns>
        private static SampleFilter ConvertV2TextureFilterToFilter(TextureFilter filter)
        {
            var result = SampleFilter.MinMagMipPoint;

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
		/// <returns>The sprite from the stream data.</returns>
		private static GorgonSprite LoadSprite(GorgonGraphics graphics, GorgonChunkReader reader)
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
		    GorgonTexture2DView textureView = null;

		    // Bind the texture (if we have one bound to this sprite) if it's already loaded, otherwise defer it.
		    if (!string.IsNullOrEmpty(textureName))
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

		    if (textureView == null)
		    {
                sprite.TextureRegion = new DX.RectangleF(0, 0, 1, 1);
		    }
		    else
		    {
		        // V2 used black transparent by default, so convert it to our default so we can keep from creating unnecessary states.
		        if (borderColor == GorgonColor.BlackTransparent)
		        {
		            borderColor = GorgonColor.White;
		        }

		        sprite.TextureRegion = reader.ReadRectangleF();
		        sprite.Texture = textureView;
		        sprite.TextureSampler = CreateSamplerState(graphics, filter, borderColor, hWrap, vWrap);
		    }

		    reader.End();
            reader.End();

		    return sprite;
		}

        /// <summary>
        /// Function to read the sprite data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the sprite.</param>
        /// <returns>A new <see cref="GorgonSprite"/>.</returns>
        public GorgonSprite FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GO2DIO_ERR_STREAM_IS_WRITE_ONLY);
            }

            if (stream.Position >= stream.Length)
            {
                throw new EndOfStreamException();
            }

            var reader = new GorgonChunkReader(stream);

            try
            {
                return LoadSprite(Graphics, reader);
            }
            finally
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Function to save the sprite data to a stream.
        /// </summary>
        /// <param name="stream">The stream that will contain the sprite.</param>
        /// <exception cref="NotSupportedException">This method is not supported by this codec.</exception>
        public void Save(Stream stream)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV2SpriteCodec"/> class.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <exception cref="ArgumentNullException">renderer</exception>
        public GorgonV2SpriteCodec(Gorgon2D renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }
        #endregion

    }
}
