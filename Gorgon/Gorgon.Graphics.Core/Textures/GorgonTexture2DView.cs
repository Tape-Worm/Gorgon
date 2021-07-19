#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 19, 2016 1:29:59 PM
// 
#endregion

using System;
using System.IO;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A shader view for textures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a texture shader view to allow a <see cref="GorgonTexture2D"/> to be bound to the GPU pipeline as a shader resource.
    /// </para>
    /// <para>
    /// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
    /// format within the same group.	
    /// </para>
    /// </remarks>
    public sealed class GorgonTexture2DView
        : GorgonShaderResourceView, IGorgonTexture2DInfo, IGorgonImageInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        ImageType IGorgonImageInfo.ImageType => IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;

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
        /// Property to return the format for the view.
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
        /// <remarks>
        /// If the texture is multisampled, then this value will be set to 0.
        /// </remarks>
        public int MipSlice
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of mip maps in the resource to view.
        /// </summary> 
        /// <remarks>
        /// If the texture is multisampled, then this value will be set to 1.
        /// </remarks>
        public int MipCount
        {
            get;
            private set;
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
        /// <remarks>
        /// If the texture is a 2D texture cube, then this value will be a multiple of 6.
        /// </remarks>
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
        public DX.Rectangle Bounds
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
        /// Property to return the multisample quality and count for the texture.
        /// </summary>
        public GorgonMultisampleInfo MultisampleInfo => Texture?.MultisampleInfo ?? GorgonMultisampleInfo.NoMultiSampling;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;

        /// <summary>
        /// Property to return whether the resource used by this view can be shared or not.
        /// </summary>
        public bool Shared => Texture.Shared;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description for a 2D texture.
        /// </summary>
        private void GetDesc2D()
        {
            bool isMultisampled = ((Texture.MultisampleInfo.Count > 1)
                                   || (Texture.MultisampleInfo.Quality > 0));

            if (!isMultisampled)
            {
                SrvDesc = new D3D11.ShaderResourceViewDescription1
                {
                    Format = (Format)Format,
                    Dimension = Texture.ArrayCount > 1
                                           ? D3D.ShaderResourceViewDimension.Texture2DArray
                                           : D3D.ShaderResourceViewDimension.Texture2D,
                    Texture2DArray =
                    {
                        MipLevels = MipCount,
                        MostDetailedMip = MipSlice,
                        FirstArraySlice = ArrayIndex,
                        ArraySize = ArrayCount,
                        PlaneSlice = 0
                    }
                };
                return;
            }

            MipSlice = 0;
            MipCount = 1;

            SrvDesc = new D3D11.ShaderResourceViewDescription1
            {
                Format = (Format)Format,
                Dimension = Texture.ArrayCount > 1
                                       ? D3D.ShaderResourceViewDimension.Texture2DMultisampledArray
                                       : D3D.ShaderResourceViewDimension.Texture2DMultisampled,
                Texture2DMSArray =
                {
                    ArraySize = ArrayCount,
                    FirstArraySlice = ArrayIndex
                }
            };
        }

        /// <summary>
        /// Function to retrieve the view description for a 2D texture cube.
        /// </summary>
        private void GetDesc2DCube()
        {
            bool isMultisampled = ((Texture.MultisampleInfo.Count > 1)
                                   || (Texture.MultisampleInfo.Quality > 0));

            if (isMultisampled)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CANNOT_MULTISAMPLE_CUBE);
            }

            if ((ArrayCount % 6) != 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CUBE_ARRAY_SIZE_INVALID);
            }

            int cubeArrayCount = ArrayCount / 6;

            SrvDesc = new D3D11.ShaderResourceViewDescription1
            {
                Format = (Format)Format,
                Dimension =
                           cubeArrayCount > 1
                               ? D3D.ShaderResourceViewDimension.TextureCubeArray
                               : D3D.ShaderResourceViewDimension.TextureCube,
                TextureCubeArray =
                       {
                           CubeCount = cubeArrayCount,
                           First2DArrayFace = ArrayIndex,
                           MipLevels = MipCount,
                           MostDetailedMip = MipSlice
                       }
            };
        }

        /// <summary>Function to retrieve the necessary parameters to create the native view.</summary>
        /// <returns>A shader resource view descriptor.</returns>
        private protected override ref readonly D3D11.ShaderResourceViewDescription1 OnGetSrvParams()
        {
            if (IsCubeMap)
            {
                GetDesc2DCube();
            }
            else
            {
                GetDesc2D();
            }
            
            return ref SrvDesc;
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
        /// Function to create a new <see cref="GorgonTexture2DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <returns>A <see cref="GorgonTexture2DView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="format"/> is a typeless format.</exception>
        /// <exception cref="GorgonException">
        /// Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ShaderResource"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the texture <see cref="Format"/> is not typeless, and the <see cref="Binding"/> is set to <see cref="TextureBinding.DepthStencil"/>.</para>
        /// <para></para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a view that makes a texture accessible to shaders. This allows viewing of the texture data in a different format, or even a subsection of the texture from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <see cref="BufferFormat.Unknown"/>, then 
        /// the format from the this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// </remarks>
        public GorgonTexture2DView GetShaderResourceView(BufferFormat format = BufferFormat.Unknown) => Texture.GetShaderResourceView(format, MipSlice, MipCount, ArrayIndex, ArrayCount);

        /// <summary>
        /// Function to convert a rectangle of texel coordinates to pixel space.
        /// </summary>
        /// <param name="texelCoordinates">The texel coordinates to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>A rectangle containing the pixel space coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public DX.Rectangle ToPixel(DX.RectangleF texelCoordinates, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new DX.Rectangle
                {
                    Left = (int)(texelCoordinates.X * width),
                    Top = (int)(texelCoordinates.Y * height),
                    Right = (int)(texelCoordinates.Right * width),
                    Bottom = (int)(texelCoordinates.Bottom * height)
                };
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new DX.Rectangle
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
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public DX.RectangleF ToTexel(DX.Rectangle pixelCoordinates, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new DX.RectangleF
                {
                    Left = pixelCoordinates.Left / width,
                    Top = pixelCoordinates.Top / height,
                    Right = pixelCoordinates.Right / width,
                    Bottom = pixelCoordinates.Bottom / height
                };
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new DX.RectangleF
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
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public DX.Size2F ToTexel(DX.Size2 pixelSize, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new DX.Size2F(pixelSize.Width / width, pixelSize.Height / height);
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new DX.Size2F(pixelSize.Width / width, pixelSize.Height / height);
        }

        /// <summary>
        /// Function to convert a size value from texel coordinates to pixel space.
        /// </summary>
        /// <param name="texelSize">The texel size to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>A size value containing the texel space coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public DX.Size2 ToPixel(DX.Size2F texelSize, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new DX.Size2((int)(texelSize.Width * width), (int)(texelSize.Height * height));
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new DX.Size2((int)(texelSize.Width * width), (int)(texelSize.Height * height));
        }

        /// <summary>
        /// Function to convert a 2D vector value from pixel coordinates to texel space.
        /// </summary>
        /// <param name="pixelVector">The pixel size to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>A 2D vector containing the texel space coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public Vector2 ToTexel(Vector2 pixelVector, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new Vector2(pixelVector.X / width, pixelVector.Y / height);
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new Vector2(pixelVector.X / width, pixelVector.Y / height);
        }

        /// <summary>
        /// Function to convert a 2D vector value from texel coordinates to pixel space.
        /// </summary>
        /// <param name="texelVector">The texel size to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>A 2D vector containing the pixel space coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public Vector2 ToPixel(Vector2 texelVector, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new Vector2(texelVector.X * width, texelVector.Y * height);
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new Vector2(texelVector.X * width, texelVector.Y * height);
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonRenderTarget2DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <returns>A <see cref="GorgonTexture2DView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless.</exception>
        /// <exception cref="GorgonException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.RenderTarget"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a view that allows a texture to become a render target. This allows rendering into texture data in a different format, or even a subsection of the texture.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <see cref="BufferFormat.Unknown"/>, then
        /// the format from the this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// </remarks>
        public GorgonRenderTarget2DView GetRenderTargetView(BufferFormat format = BufferFormat.Unknown) => Texture.GetRenderTargetView(format, MipSlice, ArrayIndex, ArrayCount);

        /// <summary>
        /// Function to convert a 2D point value from pixel coordinates to texel space.
        /// </summary>
        /// <param name="pixelPoint">The pixel size to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>A 2D vector containing the texel space coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> and <see cref="MipCount"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public Vector2 ToTexel(DX.Point pixelPoint, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new Vector2(pixelPoint.X / width, pixelPoint.Y / height);
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new Vector2(pixelPoint.X / width, pixelPoint.Y / height);
        }

        /// <summary>
        /// Function to return the width of the texture at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the texture.</returns>
        public int GetMipWidth(int mipLevel)
        {
            mipLevel = mipLevel.Min(MipCount + MipSlice).Max(MipSlice);
            return (Width >> mipLevel).Max(1);
        }

        /// <summary>
        /// Function to return the height of the texture at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the texture.</returns>
        public int GetMipHeight(int mipLevel)
        {
            mipLevel = mipLevel.Min(MipCount + MipSlice).Max(MipSlice);

            return (Height >> mipLevel).Max(1);
        }

        /// <summary>
        /// Function to create a new texture that is bindable to the GPU.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="info">The information about the texture.</param>
        /// <param name="initialData">[Optional] Initial data used to populate the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonTexture2D"/> and a <see cref="GorgonTexture2DView"/> as a single object that users can use to apply a texture as a shader 
        /// resource. This helps simplify creation of a texture by executing some prerequisite steps on behalf of the user.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// <para>
        /// If an <paramref name="initialData"/> image is provided, and the width/height/depth is not the same as the values in the <paramref name="info"/> parameter, then the image data will be cropped to
        /// match the values in the <paramref name="info"/> parameter. Things like array count, and mip levels will still be taken from the <paramref name="initialData"/> image parameter.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2D"/>
        public static GorgonTexture2DView CreateTexture(GorgonGraphics graphics, IGorgonTexture2DInfo info, IGorgonImage initialData = null)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (initialData is not null)
            {
                if ((initialData.Width < info.Width)
                    || (initialData.Height < info.Height))
                {
                    initialData.BeginUpdate()
                               .Expand(info.Width, info.Height, 1)
                               .EndUpdate();
                }

                if ((initialData.Width > info.Width)
                    || (initialData.Height > info.Height))
                {
                    initialData.BeginUpdate()
                               .Crop(new DX.Rectangle(0, 0, info.Width, info.Height), 1)
                               .EndUpdate();
                }
            }

            var newInfo = new GorgonTexture2DInfo(info)
            {
                Usage = info.Usage == ResourceUsage.Staging ? ResourceUsage.Default : info.Usage,
                Binding = (info.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource
                                            ? (info.Binding | TextureBinding.ShaderResource)
                                            : info.Binding
            };

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

            GorgonTexture2DView result = texture.GetShaderResourceView();
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
        /// <returns>A new <see cref="GorgonTexture2DView"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DView"/>.
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
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture2DView FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTexture2DLoadOptions options = null)
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

            if (size is null)
            {
                size = stream.Length - stream.Position;
            }

            if ((stream.Length - stream.Position) < size)
            {
                throw new EndOfStreamException();
            }

            using IGorgonImage image = codec.FromStream(stream, size);
            GorgonTexture2D texture = image.ToTexture2D(graphics, options);
            GorgonTexture2DView view = texture.GetShaderResourceView();
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
        /// <returns>A new <see cref="GorgonTexture2DView"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DView"/>.
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
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture2DView FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTexture2DLoadOptions options = null)
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
            if (options is null)
            {
                options = new GorgonTexture2DLoadOptions
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    Usage = ResourceUsage.Default,
                    Binding = TextureBinding.ShaderResource,
                    IsTextureCube = image.ImageType == ImageType.ImageCube
                };
            }
            GorgonTexture2D texture = image.ToTexture2D(graphics, options);
            GorgonTexture2DView view = texture.GetShaderResourceView();
            view.OwnsResource = true;
            return view;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2DView"/> class.
        /// </summary>
        /// <param name="texture">The <see cref="GorgonTexture2D"/> being viewed.</param>
        /// <param name="format">The format for the view.</param>
        /// <param name="formatInfo">The information about the view format.</param>
        /// <param name="firstMipLevel">The first mip level to view.</param>
        /// <param name="mipCount">The number of mip levels to view.</param>
        /// <param name="arrayIndex">The first array index to view.</param>
        /// <param name="arrayCount">The number of array indices to view.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
        internal GorgonTexture2DView(GorgonTexture2D texture,
                                     BufferFormat format,
                                     GorgonFormatInfo formatInfo,
                                     int firstMipLevel,
                                     int mipCount,
                                     int arrayIndex,
                                     int arrayCount)
            : base(texture)
        {
            FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
            Format = format;
            Texture = texture;
            Bounds = new DX.Rectangle(0, 0, Width, Height);
            MipSlice = firstMipLevel;
            MipCount = mipCount;
            ArrayIndex = arrayIndex;
            ArrayCount = arrayCount;
        }
        #endregion
    }
}