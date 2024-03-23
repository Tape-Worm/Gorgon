
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: July 22, 2017 10:31:48 AM
// 

using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Memory;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a read/write (unordered access) view for a <see cref="GorgonTexture2D"/>
/// </summary>
/// <remarks>
/// <para>
/// This type of view allows for unordered access to a <see cref="GorgonTexture2D"/>. The texture must have been created with the <see cref="TextureBinding.ReadWriteView"/> flag in its 
/// <see cref="IGorgonTexture2DInfo.Binding"/> property
/// </para>
/// <para>
/// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
/// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>
/// </para>
/// <para>
/// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
/// <see cref="GorgonDrawCallCommon">draw call</see>
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// Unordered access views do not support <see cref="GorgonTexture2D"/> textures with <see cref="GorgonMultisampleInfo">multisampling</see> enabled
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsResource"/>
/// <seealso cref="GorgonTexture2D"/>
/// <seealso cref="GorgonComputeShader"/>
/// <seealso cref="GorgonPixelShader"/>
/// <seealso cref="GorgonDrawCallCommon"/>
/// <seealso cref="GorgonMultisampleInfo"/>
public sealed class GorgonTexture2DReadWriteView
    : GorgonReadWriteView, IGorgonTexture2DInfo, IGorgonImageInfo
{
    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    ImageDataType IGorgonImageInfo.ImageType => IsCubeMap ? ImageDataType.ImageCube : ImageDataType.Image2D;

    /// <summary>
    /// Property to return the depth of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 3D images only.  This parameter will be set to a value of 1 for a 1D or 2D image.
    /// </para>
    /// </remarks>
    int IGorgonImageInfo.Depth => 1;

    /// <summary>
    /// Property to return whether the image data is using premultiplied alpha.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value has no meaning for this type.
    /// </para>
    /// </remarks>
    bool IGorgonImageInfo.HasPreMultipliedAlpha => false;

    /// <summary>
    /// Property to return the number of mip map levels in the image.
    /// </summary>
    int IGorgonImageInfo.MipCount => Texture?.MipLevels ?? 0;

    /// <summary>
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
    /// </para>
    /// </remarks>
    int IGorgonImageInfo.ArrayCount => Texture?.ArrayCount ?? 0;

    /// <summary>
    /// Property to return whether the size of the texture is a power of 2 or not.
    /// </summary>
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                          && ((Height == 0) || (Height & (Height - 1)) == 0);

    /// <summary>
    /// Property to return the pixel format for an image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the value is set to <see cref="BufferFormat.Unknown"/>, then an exception will be thrown upon image creation.
    /// </para>
    /// </remarks>
    BufferFormat IGorgonImageInfo.Format => Texture?.Format ?? BufferFormat.Unknown;

    /// <summary>
    /// Property to return the format used to interpret this view.
    /// </summary>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return information about the <see cref="Format"/> used by this view.
    /// </summary>
    public GorgonFormatInfo FormatInformation
    {
        get;
    }

    /// <summary>
    /// Property to return the index of the first mip map in the resource to view.
    /// </summary>
    public int MipSlice
    {
        get;
    }

    /// <summary>
    /// Property to return the first array index to use in the view.
    /// </summary>
    public int ArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the number of array indices to use in the view.
    /// </summary>
    public int ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return whether the texture is a texture cube or not.
    /// </summary>
    public bool IsCubeMap => Texture?.IsCubeMap ?? false;

    /// <summary>
    /// Property to return the texture that is bound to this view.
    /// </summary>
    public GorgonTexture2D Texture
    {
        get; private set;
    }

    /// <summary>
    /// Property to return the bounding rectangle for the view.
    /// </summary>
    /// <remarks>
    /// This value is the full bounding rectangle of the first mip map level for the texture associated with the view.
    /// </remarks>
    public GorgonRectangle Bounds
    {
        get;
    }

    /// <summary>
    /// Property to return the width of the texture in pixels.
    /// </summary>
    /// <remarks>
    /// This value is the full width of the first mip map level for the texture associated with the view.
    /// </remarks>
    public int Width => Texture?.Width ?? 0;

    /// <summary>
    /// Property to return the height of the texture in pixels.
    /// </summary>
    /// <remarks>
    /// This value is the full height of the first mip map level for the texture associated with the view.
    /// </remarks>
    public int Height => Texture?.Height ?? 0;

    /// <summary>
    /// Property to return the name of the texture.
    /// </summary>
    string IGorgonNamedObject.Name => Texture?.Name ?? string.Empty;

    /// <summary>
    /// Property to return the number of mip-map levels for the texture.
    /// </summary>
    int IGorgonTexture2DInfo.MipLevels => Texture?.MipLevels ?? 0;

    /// <summary>
    /// Property to return the multisample quality and count for this texture.
    /// </summary>
    /// <remarks>
    /// This value is defaulted to <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.
    /// </remarks>
    GorgonMultisampleInfo IGorgonTexture2DInfo.MultisampleInfo => GorgonMultisampleInfo.NoMultiSampling;

    /// <summary>
    /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
    /// </summary>
    public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;

    /// <summary>
    /// Property to return whether the resource used by this view can be shared or not.
    /// </summary>
    public TextureSharingOptions Shared => Texture.Shared;

    /// <summary>Function to retrieve the necessary parameters to create the native view.</summary>
    /// <returns>The D3D11 UAV descriptor.</returns>        
    private protected override ref readonly D3D11.UnorderedAccessViewDescription1 OnGetUavParams()
    {
        UavDesc = Texture.ArrayCount == 1
                   ? new D3D11.UnorderedAccessViewDescription1
                   {
                       Format = (Format)Format,
                       Dimension = D3D11.UnorderedAccessViewDimension.Texture2D,
                       Texture2D =
                         {
                             MipSlice = MipSlice,
                             PlaneSlice = 0
                         }
                   }
                   : new D3D11.UnorderedAccessViewDescription1
                   {
                       Format = (Format)Format,
                       Dimension = D3D11.UnorderedAccessViewDimension.Texture2DArray,
                       Texture2DArray =
                         {
                             MipSlice = MipSlice,
                             FirstArraySlice = ArrayCount,
                             ArraySize = ArrayIndex,
                             PlaneSlice = 0
                         }
                   };

        return ref UavDesc;
    }

    /// <summary>
    /// Function to convert a rectangle of texel coordinates to pixel space.
    /// </summary>
    /// <param name="texelCoordinates">The texel coordinates to convert.</param>
    /// <param name="mipLevel">[Optional] The mip level to use.</param>
    /// <returns>A rectangle containing the pixel space coordinates.</returns>
    /// <remarks>
    /// <para>
    /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
    /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
    /// for the underlying <see cref="Texture"/> is used.
    /// </para>
    /// </remarks>
    public GorgonRectangle ToPixel(GorgonRectangleF texelCoordinates, int? mipLevel = null)
    {
        float width = Texture.Width;
        float height = Texture.Height;

        if (mipLevel is null)
        {
            return new GorgonRectangle
            {
                Left = (int)(texelCoordinates.X * width),
                Top = (int)(texelCoordinates.Y * height),
                Right = (int)(texelCoordinates.Right * width),
                Bottom = (int)(texelCoordinates.Bottom * height)
            };
        }

        width = GetMipWidth(mipLevel.Value);
        height = GetMipHeight(mipLevel.Value);

        return new GorgonRectangle
        {
            Left = (int)(texelCoordinates.X * width),
            Top = (int)(texelCoordinates.Y * height),
            Right = (int)(texelCoordinates.Right * width),
            Bottom = (int)(texelCoordinates.Bottom * height)
        };
    }

    /// <summary>
    /// Function to convert a rectangle of pixel coordinates to texel space.
    /// </summary>
    /// <param name="pixelCoordinates">The pixel coordinates to convert.</param>
    /// <param name="mipLevel">[Optional] The mip level to use.</param>
    /// <returns>A rectangle containing the texel space coordinates.</returns>
    /// <remarks>
    /// <para>
    /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
    /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
    /// for the underlying <see cref="Texture"/> is used.
    /// </para>
    /// </remarks>
    public GorgonRectangleF ToTexel(GorgonRectangle pixelCoordinates, int? mipLevel = null)
    {
        float width = Texture.Width;
        float height = Texture.Height;

        if (mipLevel is null)
        {
            return new GorgonRectangleF
            {
                Left = pixelCoordinates.Left / width,
                Top = pixelCoordinates.Top / height,
                Right = pixelCoordinates.Right / width,
                Bottom = pixelCoordinates.Bottom / height
            };
        }

        width = GetMipWidth(mipLevel.Value);
        height = GetMipHeight(mipLevel.Value);

        return new GorgonRectangleF
        {
            Left = pixelCoordinates.Left / width,
            Top = pixelCoordinates.Top / height,
            Right = pixelCoordinates.Right / width,
            Bottom = pixelCoordinates.Bottom / height
        };
    }

    /// <summary>
    /// Function to convert a size value from pixel coordinates to texel space.
    /// </summary>
    /// <param name="pixelSize">The pixel size to convert.</param>
    /// <param name="mipLevel">[Optional] The mip level to use.</param>
    /// <returns>A size value containing the texel space coordinates.</returns>
    /// <remarks>
    /// <para>
    /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
    /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
    /// for the underlying <see cref="Texture"/> is used.
    /// </para>
    /// </remarks>
    public Vector2 ToTexel(GorgonPoint pixelSize, int? mipLevel = null)
    {
        float width = Texture.Width;
        float height = Texture.Height;

        if (mipLevel is null)
        {
            return new Vector2(pixelSize.X / width, pixelSize.Y / height);
        }

        width = GetMipWidth(mipLevel.Value);
        height = GetMipHeight(mipLevel.Value);

        return new Vector2(pixelSize.X / width, pixelSize.Y / height);
    }

    /// <summary>
    /// Function to convert a size value from texel coordinates to pixel space.
    /// </summary>
    /// <param name="texelSize">The texel size to convert.</param>
    /// <param name="mipLevel">[Optional] The mip level to use.</param>
    /// <returns>A size value containing the texel space coordinates.</returns>
    /// <remarks>
    /// <para>
    /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
    /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
    /// for the underlying <see cref="Texture"/> is used.
    /// </para>
    /// </remarks>
    public GorgonPoint ToPixel(Vector2 texelSize, int? mipLevel = null)
    {
        float width = Texture.Width;
        float height = Texture.Height;

        if (mipLevel is null)
        {
            return new GorgonPoint((int)(texelSize.X * width), (int)(texelSize.Y * height));
        }

        width = GetMipWidth(mipLevel.Value);
        height = GetMipHeight(mipLevel.Value);

        return new GorgonPoint((int)(texelSize.X * width), (int)(texelSize.Y * height));
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        Texture = null;
        base.Dispose();
    }

    /// <summary>
    /// Function to return the width of the texture at the current <see cref="MipSlice"/> in pixels.
    /// </summary>
    /// <param name="mipLevel">The mip level to evaluate.</param>
    /// <returns>The width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the view.</returns>
    public int GetMipWidth(int mipLevel)
    {
        mipLevel = mipLevel.Min(Texture.MipLevels).Max(MipSlice);
        return (Width >> mipLevel).Max(1);
    }

    /// <summary>
    /// Function to return the height of the texture at the current <see cref="MipSlice"/> in pixels.
    /// </summary>
    /// <param name="mipLevel">The mip level to evaluate.</param>
    /// <returns>The height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the view.</returns>
    public int GetMipHeight(int mipLevel)
    {
        mipLevel = mipLevel.Min(Texture.MipLevels).Max(MipSlice);

        return (Height >> mipLevel).Max(1);
    }

    /// <summary>
    /// Function to clear the contents of the texture for this view.
    /// </summary>
    /// <param name="color">Color to use when clearing the texture unordered access view.</param>
    /// <param name="rectangles">[Optional] Specifies which regions on the view to clear.</param>
    /// <remarks>
    /// <para>
    /// This will clear the texture unordered access view to the specified <paramref name="color"/>.  If a specific region should be cleared, one or more <paramref name="rectangles"/> should be passed 
    /// to the method.
    /// </para>
    /// <para>
    /// If the <paramref name="rectangles"/> parameter is <b>null</b>, or has a zero length, the entirety of the view is cleared.
    /// </para>
    /// <para>
    /// If this method is called with a 3D texture bound to the view, and with regions specified, then the regions are ignored.
    /// </para>
    /// </remarks>
    public void Clear(GorgonColor color, ReadOnlySpan<GorgonRectangle> rectangles)
    {
        if (rectangles.IsEmpty)
        {
            Clear(color.Red, color.Green, color.Blue, color.Alpha);
            return;
        }

        RawRectangle[] clearRects = GorgonArrayPool<RawRectangle>.SharedTiny.Rent(rectangles.Length);

        try
        {
            for (int i = 0; i < rectangles.Length; ++i)
            {
                clearRects[i] = rectangles[i].ToSharpDXRawRectangle();
            }

            Resource.Graphics.D3DDeviceContext.ClearView(Native, color.ToRawColor4(), clearRects, rectangles.Length);
        }
        finally
        {
            GorgonArrayPool<RawRectangle>.SharedTiny.Return(clearRects, true);
        }
    }

    /// <summary>
    /// Function to create a new texture that is bindable to the GPU as an unordered access resource.
    /// </summary>
    /// <param name="graphics">The graphics interface to use when creating the target.</param>
    /// <param name="info">The information about the texture.</param>
    /// <param name="initialData">[Optional] Initial data used to populate the texture.</param>
    /// <returns>A new <see cref="GorgonTexture2DReadWriteView"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method that will create a <see cref="GorgonTexture2D"/> and a <see cref="GorgonTexture2DReadWriteView"/> as a single object that users can use to apply a texture as an unordered  
    /// access resource. This helps simplify creation of a texture by executing some prerequisite steps on behalf of the user.
    /// </para>
    /// <para>
    /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DReadWriteView"/> returned, disposal of either one will dispose of the other on your behalf. If 
    /// the user created a <see cref="GorgonTexture2DReadWriteView"/> from the <see cref="GorgonTexture2D.GetReadWriteView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
    /// what they are doing and will handle the disposal of the texture and view on their own.
    /// </para>
    /// <para>
    /// If an <paramref name="initialData"/> image is provided, and the width/height/depth is not the same as the values in the <paramref name="info"/> parameter, then the image data will be cropped to
    /// match the values in the <paramref name="info"/> parameter. Things like array count, and mip levels will still be taken from the <paramref name="initialData"/> image parameter.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture2D"/>
    public static GorgonTexture2DReadWriteView CreateTexture(GorgonGraphics graphics, IGorgonTexture2DInfo info, IGorgonImage initialData = null)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        GorgonTexture2DInfo newInfo = new(info)
        {
            Usage = info.Usage == ResourceUsage.Staging ? ResourceUsage.Default : info.Usage,
            Binding = (((info.Binding & TextureBinding.ReadWriteView) != TextureBinding.ReadWriteView)
                                         ? (info.Binding | TextureBinding.ReadWriteView)
                                         : info.Binding) & ~TextureBinding.DepthStencil
        };

        if (initialData is not null)
        {
            if ((initialData.Width > info.Width)
                || (initialData.Height > info.Height))
            {
                initialData = initialData.BeginUpdate()
                                         .Expand(info.Width, info.Height, 1)
                                         .EndUpdate();
            }

            if ((initialData.Width < info.Width)
                || (initialData.Height < info.Height))
            {
                initialData = initialData.BeginUpdate()
                                         .Crop(new GorgonRectangle(0, 0, info.Width, info.Height), 1)
                                         .EndUpdate();
            }
        }

        GorgonTexture2D texture = initialData is null
                                      ? new GorgonTexture2D(graphics, newInfo)
                                      : initialData.ToTexture2D(graphics,
                                                                new GorgonTexture2DLoadOptions
                                                                {
                                                                    Usage = newInfo.Usage,
                                                                    Binding = newInfo.Binding,
                                                                    MultisampleInfo = newInfo.MultisampleInfo,
                                                                    Name = newInfo.Name,
                                                                    IsTextureCube = newInfo.IsCubeMap
                                                                });

        GorgonTexture2DReadWriteView result = texture.GetReadWriteView();
        result.OwnsResource = true;

        return result;
    }

    /// <summary>
    /// Function to load a texture from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="graphics">The graphics interface that will own the texture.</param>
    /// <param name="stream">The stream containing the texture image data.</param>
    /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
    /// <param name="size">[Optional] The size of the image in the stream, in bytes.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture2DReadWriteView"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
    /// <remarks>
    /// <para>
    /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DReadWriteView"/>.
    /// </para>
    /// <para>
    /// If the <paramref name="size"/> option is specified, then the method will read from the stream up to that number of bytes, so it is up to the user to provide an accurate size. If it is omitted 
    /// then the <c>stream length - stream position</c> is used as the total size.
    /// </para>
    /// <para>
    /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
    /// <list type="bullet">
    ///		<item>
    ///			<term>Binding</term>
    ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
    ///         <see cref="TextureBinding.ShaderResource"/>.</description>
    ///		</item>
    ///		<item>
    ///			<term>Usage</term>
    ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
    ///		</item>
    ///		<item>
    ///			<term>Multisample info</term>
    ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// <para>
    /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DReadWriteView"/> returned, disposal of either one will dispose of the other on your behalf. If 
    /// the user created a <see cref="GorgonTexture2DReadWriteView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
    /// what they are doing and will handle the disposal of the texture and view on their own.
    /// </para>
    /// </remarks>
    public static GorgonTexture2DReadWriteView FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTexture2DLoadOptions options = null)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (!stream.CanRead)
        {
            throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
        }

        size ??= stream.Length - stream.Position;

        if ((stream.Length - stream.Position) < size)
        {
            throw new EndOfStreamException();
        }

        using IGorgonImage image = codec.FromStream(stream, size);
        GorgonTexture2D texture = image.ToTexture2D(graphics, options);
        GorgonTexture2DReadWriteView view = texture.GetReadWriteView();
        view.OwnsResource = true;
        return view;
    }

    /// <summary>
    /// Function to load a texture from a file.
    /// </summary>
    /// <param name="graphics">The graphics interface that will own the texture.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture2DReadWriteView"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DReadWriteView"/>.
    /// </para>
    /// <para>
    /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
    /// <list type="bullet">
    ///		<item>
    ///			<term>Binding</term>
    ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
    ///         <see cref="TextureBinding.ShaderResource"/>.</description>
    ///		</item>
    ///		<item>
    ///			<term>Usage</term>
    ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
    ///		</item>
    ///		<item>
    ///			<term>Multisample info</term>
    ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// <para>
    /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DReadWriteView"/> returned, disposal of either one will dispose of the other on your behalf. If 
    /// the user created a <see cref="GorgonTexture2DReadWriteView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
    /// what they are doing and will handle the disposal of the texture and view on their own.
    /// </para>
    /// </remarks>
    public static GorgonTexture2DReadWriteView FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTexture2DLoadOptions options = null)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        using IGorgonImage image = codec.FromFile(filePath);
        GorgonTexture2D texture = image.ToTexture2D(graphics, options);
        GorgonTexture2DReadWriteView view = texture.GetReadWriteView();
        view.OwnsResource = true;
        return view;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture2DReadWriteView"/> class.
    /// </summary>
    /// <param name="texture">The texture to view.</param>
    /// <param name="format">The format for the view.</param>
    /// <param name="formatInfo">Information about the format.</param>
    /// <param name="firstMipLevel">The first mip level to view.</param>
    /// <param name="arrayIndex">The first array index to view.</param>
    /// <param name="arrayCount">The number of array indices to view.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
    internal GorgonTexture2DReadWriteView(GorgonTexture2D texture,
                              BufferFormat format,
                              GorgonFormatInfo formatInfo,
                              int firstMipLevel,
                              int arrayIndex,
                              int arrayCount)
        : base(texture)
    {
        FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
        Format = format;
        Texture = texture;
        Bounds = new GorgonRectangle(0, 0, Width, Height);
        MipSlice = firstMipLevel;
        ArrayIndex = arrayIndex;
        ArrayCount = arrayCount;
    }
}
