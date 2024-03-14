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

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.IO;

/// <summary>
/// A codec that can read version 1 sprite data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonV1SpriteBinaryCodec"/> class.
/// </remarks>
/// <param name="renderer">The renderer used for resource handling.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
public class GorgonV1SpriteBinaryCodec(Gorgon2D renderer)
        : GorgonSpriteCodecCommon(renderer, Resources.GOR2DIO_V1_CODEC, Resources.GOR2DIO_V1_CODEC_DESCRIPTION)
{
    #region Variables.
    // The animation codec for the sprite.
    private readonly GorgonV1AnimationCodec _animationCodec = new(renderer);
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
    } = new Version(1, 2);
    #endregion

    #region Methods.
    /// <summary>
    /// Function to convert Gorgon 1.x image smoothing to 2.x texture filtering values.
    /// </summary>
    /// <param name="smoothing">Smoothing value to convert.</param>
    /// <returns>Texture filtering value.</returns>
    private static SampleFilter ConvertSmoothingToFilter(int smoothing) => smoothing switch
    {
        0 => SampleFilter.MinMagMipPoint,
        2 => SampleFilter.MinPointMagMipLinear,
        3 => SampleFilter.MinLinearMagMipPoint,
        _ => SampleFilter.MinMagMipLinear,
    };

    /// <summary>
    /// Function to convert Gorgon 1.x image addressing values to 2.x texture addressing values.
    /// </summary>
    /// <param name="imageAddress">Image addressing values.</param>
    /// <returns>Texture addressing values.</returns>
    private static TextureWrap ConvertImageAddressToTextureAddress(int imageAddress) => imageAddress switch
    {
        0 => TextureWrap.Wrap,
        1 => TextureWrap.Mirror,
        2 => TextureWrap.MirrorOnce,
        4 => TextureWrap.Border,
        _ => TextureWrap.Clamp,
    };

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

        return filter switch
        {
            SampleFilter.MinMagMipLinear when (hWrap == TextureWrap.Clamp) && (vWrap == TextureWrap.Clamp) && (borderColor == GorgonColor.White) => null,
            SampleFilter.MinMagMipPoint when (hWrap == TextureWrap.Clamp) && (vWrap == TextureWrap.Clamp) && (borderColor == GorgonColor.White) => GorgonSamplerState.PointFiltering,
            SampleFilter.MinMagMipLinear when (hWrap == TextureWrap.Wrap) && (vWrap == TextureWrap.Wrap) && (borderColor == GorgonColor.White) => GorgonSamplerState.Wrapping,
            SampleFilter.MinMagMipPoint when (hWrap == TextureWrap.Wrap) && (vWrap == TextureWrap.Wrap) && (borderColor == GorgonColor.White) => GorgonSamplerState.PointFilteringWrapping,
            _ => builder.Wrapping(hWrap, vWrap, borderColor: borderColor)
.Filter(filter)
.Build(),
        };
    }

    /// <summary>
    /// Function to load a version 1.x Gorgon sprite.
    /// </summary>
    /// <param name="graphics">The graphics interface used to create states.</param>
    /// <param name="reader">Binary reader to use to read in the data.</param>
    /// <param name="overrideTexture">The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
    /// <returns>The sprite from the stream data.</returns>
    private static GorgonSprite LoadSprite(GorgonGraphics graphics, GorgonBinaryReader reader, GorgonTexture2DView overrideTexture)
    {
        string imageName = string.Empty;

        string headerVersion = reader.ReadString();
        if ((!headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase))
            || (headerVersion.Length < 7)
            || (headerVersion.Length > 9))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR2DIO_ERR_INVALID_HEADER);
        }

        var sprite = new GorgonSprite();
        Version version = headerVersion.ToUpperInvariant() switch
        {
            "GORSPR1" => new Version(1, 0),
            "GORSPR1.1" => new Version(1, 1),
            "GORSPR1.2" => new Version(1, 2),
            _ => throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_VERSION_MISMATCH, headerVersion)),
        };

        // We don't need the sprite name.
        reader.ReadString();

        // Find out if we have an image.
        if (reader.ReadBoolean())
        {
            bool isRenderTarget = reader.ReadBoolean();

            imageName = reader.ReadString();

            // We won't be supporting reading render targets from sprites in this version.
            if (isRenderTarget)
            {
                // Skip the target data.
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadBoolean();
                reader.ReadBoolean();
            }
        }

        // We don't use "inherited" values anymore.  But we need them because
        // the file doesn't include inherited data.
        bool InheritAlphaMaskFunction = reader.ReadBoolean();
        bool InheritAlphaMaskValue = reader.ReadBoolean();
        bool InheritBlending = reader.ReadBoolean();
        bool InheritHorizontalWrapping = reader.ReadBoolean();
        bool InheritSmoothing = reader.ReadBoolean();
        bool InheritStencilCompare = reader.ReadBoolean();
        bool InheritStencilEnabled = reader.ReadBoolean();
        bool InheritStencilFailOperation = reader.ReadBoolean();
        bool InheritStencilMask = reader.ReadBoolean();
        bool InheritStencilPassOperation = reader.ReadBoolean();
        bool InheritStencilReference = reader.ReadBoolean();
        bool InheritStencilZFailOperation = reader.ReadBoolean();
        bool InheritVerticalWrapping = reader.ReadBoolean();
        bool InheritDepthBias = true;
        bool InheritDepthTestFunction = true;
        bool InheritDepthWriteEnabled = true;

        // Get version 1.1 fields.
        if ((version.Major == 1) && (version.Minor >= 1))
        {
            InheritDepthBias = reader.ReadBoolean();
            InheritDepthTestFunction = reader.ReadBoolean();
            InheritDepthWriteEnabled = reader.ReadBoolean();
        }

        // Get the size of the sprite.
        sprite.Size = new DX.Size2F(reader.ReadSingle(), reader.ReadSingle());

        // Older versions of the sprite object used pixel space for their texture coordinates.  We will have to 
        // fix up these coordinates into texture space once we have a texture loaded.  At this point, there's no guarantee 
        // that the texture was loaded safely, so we'll have to defer it until later.
        // Also, older versions used the size the determine the area on the texture to cover.  So use the size to
        // get the texture bounds.
        var textureOffset = new Vector2(reader.ReadSingle(), reader.ReadSingle());

        // Read the anchor.
        // Gorgon v3 anchors are relative, so we need to convert them based on our sprite size.
        sprite.Anchor = new Vector2(reader.ReadSingle() / sprite.Size.Width, reader.ReadSingle() / sprite.Size.Height);

        // Get vertex offsets.
        sprite.CornerOffsets.UpperLeft = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);
        sprite.CornerOffsets.UpperRight = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);
        sprite.CornerOffsets.LowerRight = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);
        sprite.CornerOffsets.LowerLeft = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);

        // Get vertex colors.
        sprite.CornerColors.UpperLeft = new GorgonColor(reader.ReadInt32());
        sprite.CornerColors.UpperRight = new GorgonColor(reader.ReadInt32());
        sprite.CornerColors.LowerLeft = new GorgonColor(reader.ReadInt32());
        sprite.CornerColors.LowerRight = new GorgonColor(reader.ReadInt32());

        // Skip shader information.  Version 1.0 had shader information attached to the sprite.
        if ((version.Major == 1) && (version.Minor < 1))
        {
            if (reader.ReadBoolean())
            {
                reader.ReadString();
                reader.ReadBoolean();
                if (reader.ReadBoolean())
                {
                    reader.ReadString();
                }
            }
        }

        // We no longer have an alpha mask function.
        if (!InheritAlphaMaskFunction)
        {
            reader.ReadInt32();
        }

        if (!InheritAlphaMaskValue)
        {
            // Direct 3D 9 used a value from 0..255 for alpha masking, we use
            // a scalar value so convert to a scalar.
            sprite.AlphaTest = new GorgonRangeF(0.0f, reader.ReadInt32() / 255.0f);
        }

        // Set the blending mode.
        if (!InheritBlending)
        {
            // Skip the blending mode.  We don't use it per-sprite.
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
        }

        // Get alpha blending mode.
        if ((version.Major == 1) && (version.Minor >= 2))
        {
            // Skip the blending mode information.  We don't use it per-sprite.
            reader.ReadInt32();
            reader.ReadInt32();
        }

        TextureWrap hWrap = TextureWrap.Clamp;
        TextureWrap vWrap = TextureWrap.Clamp;
        SampleFilter filter = SampleFilter.MinMagMipLinear;
        GorgonColor samplerBorder = GorgonColor.White;

        // Get horizontal wrapping mode.
        if (!InheritHorizontalWrapping)
        {
            hWrap = ConvertImageAddressToTextureAddress(reader.ReadInt32());
        }

        // Get smoothing mode.
        if (!InheritSmoothing)
        {
            filter = ConvertSmoothingToFilter(reader.ReadInt32());
        }

        // Get stencil stuff.
        if (!InheritStencilCompare)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }
        if (!InheritStencilEnabled)
        {
            // We don't enable stencil in the same way anymore, so skip this value.
            reader.ReadBoolean();
        }
        if (!InheritStencilFailOperation)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }
        if (!InheritStencilMask)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }
        if (!InheritStencilPassOperation)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }
        if (!InheritStencilReference)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }
        if (!InheritStencilZFailOperation)
        {
            // We don't use depth/stencil info per sprite anymore.
            reader.ReadInt32();
        }

        // Get vertical wrapping mode.
        if (!InheritVerticalWrapping)
        {
            vWrap = ConvertImageAddressToTextureAddress(reader.ReadInt32());
        }

        // Get depth info.
        if ((version.Major == 1) && (version.Minor >= 1))
        {
            if (!InheritDepthBias)
            {
                // Depth bias values are quite different on D3D9 than they are on D3D11, so skip this.
                reader.ReadSingle();
            }
            if (!InheritDepthTestFunction)
            {
                // We don't use depth/stencil info per sprite anymore.
                reader.ReadInt32();
            }
            if (!InheritDepthWriteEnabled)
            {
                // We don't use depth/stencil info per sprite anymore.
                reader.ReadBoolean();
            }

            samplerBorder = new GorgonColor(reader.ReadInt32());

            // The border in the older version defaults to black.  To make it more performant, reverse this value to white.
            if (samplerBorder == GorgonColor.Black)
            {
                samplerBorder = GorgonColor.White;
            }
        }

        // Get flipped flags.
        sprite.HorizontalFlip = reader.ReadBoolean();
        sprite.VerticalFlip = reader.ReadBoolean();

        GorgonTexture2DView textureView;

        // Bind the texture (if we have one bound to this sprite) if it's already loaded, otherwise defer it.
        if ((!string.IsNullOrEmpty(imageName)) && (overrideTexture is null))
        {
            GorgonTexture2D texture = graphics.LocateResourcesByName<GorgonTexture2D>(imageName).FirstOrDefault();
            textureView = texture?.GetShaderResourceView();
        }
        else
        {
            textureView = overrideTexture;
        }

        // If we cannot load the image, then fall back to the standard coordinates.            
        if (textureView is null)
        {
            sprite.TextureRegion = new DX.RectangleF(0, 0, 1, 1);
        }
        else
        {
            sprite.TextureRegion = new DX.RectangleF(textureOffset.X / textureView.Width,
                                                     textureOffset.Y / textureView.Height,
                                                     sprite.Size.Width / textureView.Width,
                                                     sprite.Size.Height / textureView.Height);
            sprite.TextureSampler = CreateSamplerState(graphics, filter, samplerBorder, hWrap, vWrap);
        }

        sprite.Texture = textureView;

        return sprite;
    }

    /// <summary>
    /// Function to determine if the data in a stream is readable by this codec.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <returns><b>true</b> if the data can be read, or <b>false</b> if not.</returns>
    protected override bool OnIsReadable(Stream stream)
    {
        using var reader = new GorgonBinaryReader(stream, true);
        // If we don't have at least 10 bytes, then this file is not valid.
        if ((stream.Length - stream.Position) < 16)
        {
            return false;
        }

        string headerVersion = reader.ReadString();
        if ((!headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase))
            || (headerVersion.Length < 7)
            || (headerVersion.Length > 9))
        {
            return false;
        }

        // Get the version information.
        return headerVersion.ToUpperInvariant() switch
        {
            "GORSPR1" or "GORSPR1.1" or "GORSPR1.2" => true,
            _ => false,
        };
    }

    /// <summary>
    /// Function to retrieve the name of the associated texture.
    /// </summary>
    /// <param name="stream">The stream containing the texture data.</param>
    /// <returns>The name of the texture associated with the sprite, or <b>null</b> if no texture was found.</returns>
    protected override string OnGetAssociatedTextureName(Stream stream)
    {
        using var reader = new GorgonBinaryReader(stream, true);
        string headerVersion = reader.ReadString();
        if ((!headerVersion.StartsWith("GORSPR", StringComparison.OrdinalIgnoreCase))
            || (headerVersion.Length < 7)
            || (headerVersion.Length > 9))
        {
            return null;
        }

        // Get the version information.
        switch (headerVersion.ToUpperInvariant())
        {
            case "GORSPR1":
            case "GORSPR1.1":
            case "GORSPR1.2":
                break;
            default:
                return null;
        }

        // We don't need the sprite name.
        reader.ReadString();

        // Find out if we have an image.
        if (!reader.ReadBoolean())
        {
            return null;
        }

        reader.ReadBoolean();
        return reader.ReadString();
    }

    /// <summary>
    /// Function to read the sprite data from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the sprite.</param>
    /// <param name="byteCount">The number of bytes to read from the stream.</param>
    /// <param name="overrideTexture">[Optional] The texture to assign to the sprite instead of the texture associated with the name stored in the file.</param>
    /// <returns>A new <see cref="GorgonSprite"/>.</returns>
    protected override GorgonSprite OnReadFromStream(Stream stream, int byteCount, GorgonTexture2DView overrideTexture)
    {
        using var reader = new GorgonBinaryReader(stream, true);
        // We don't need the byte count here.
        return LoadSprite(Graphics, reader, overrideTexture);
    }

    /// <summary>
    /// Function to save the sprite data to a stream.
    /// </summary>
    /// <param name="sprite">The sprite to serialize into the stream.</param>
    /// <param name="stream">The stream that will contain the sprite.</param>
    protected override void OnSaveToStream(GorgonSprite sprite, Stream stream) => throw new NotSupportedException();

    /// <summary>
    /// Function to read all of the animations from the v1.x sprite.
    /// </summary>
    /// <param name="stream">The stream containing the sprite animation data.</param>
    /// <param name="size">[Optional] The size of the sprite, in bytes.</param>
    /// <returns>A list of <see cref="IGorgonAnimation"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
    public IReadOnlyList<IGorgonAnimation> LoadAnimationsFromStream(Stream stream, int? size = null) => _animationCodec.AllFromStream(stream, size);

    /// <summary>
    /// Function to read all of the animations from the v1.x sprite.
    /// </summary>
    /// <param name="filePath">The path to the v1.x sprite file that contains the animations.</param>
    /// <returns>A list of <see cref="IGorgonAnimation"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    public IReadOnlyList<IGorgonAnimation> LoadAnimationsFromFile(string filePath) => _animationCodec.AllFromFile(filePath);

    /// <summary>
    /// Function to determine the number of animations in the sprite.
    /// </summary>
    /// <param name="stream">The stream containing the sprite data.</param>
    /// <returns>The number of animations in the sprite.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the current <paramref name="stream"/> position, plus the size of the data exceeds the length of the stream.</exception>
    public int GetAnimationCount(Stream stream) => _animationCodec.GetAnimationCount(stream);

    #endregion

}
