﻿#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 22, 2017 10:31:48 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides a read/write (unordered access) view for a <see cref="GorgonTexture3D"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonTexture3D"/>. The texture must have been created with the <see cref="TextureBinding.ReadWriteView"/> flag in its 
    /// <see cref="IGorgonTexture3DInfo.Binding"/> property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallCommon">draw call</see>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonTexture3D"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallCommon"/>
    /// <seealso cref="GorgonMultisampleInfo"/>
    public sealed class GorgonTexture3DReadWriteView
        : GorgonReadWriteView, IGorgonTexture3DInfo, IGorgonImageInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        ImageType IGorgonImageInfo.ImageType => ImageType.Image3D;

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
        int IGorgonImageInfo.MipCount => Texture?.Depth ?? 0;

        /// <summary>
        /// Property to return the total number of images there are in an image array.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
        /// </para>
        /// </remarks>
        int IGorgonImageInfo.ArrayCount => 1;

        /// <summary>
        /// Property to return whether the size of the texture is a power of 2 or not.
        /// </summary>
        bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                              && ((Height == 0) || (Height & (Height - 1)) == 0)
                                              && ((Depth == 0) || (Depth & (Depth - 1)) == 0);

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
        /// Property to return the first depth slice to use in the view.
        /// </summary>
        public int StartDepthSlice
        {
            get;
        }

        /// <summary>
        /// Property to return the number of array indices to use in the view.
        /// </summary>
        public int DepthSliceCount
        {
            get;
        }

        /// <summary>
        /// Property to return the texture that is bound to this view.
        /// </summary>
        public GorgonTexture3D Texture
        {
            get;
            private set;
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
        /// Property to return the depth of the texture, in slices.
        /// </summary>
        public int Depth => Texture?.Depth ?? 0;

        /// <summary>
        /// Property to return the name of the texture.
        /// </summary>
        string IGorgonNamedObject.Name => Texture?.Name ?? string.Empty;

        /// <summary>
        /// Property to return the number of mip-map levels for the texture.
        /// </summary>
        int IGorgonTexture3DInfo.MipLevels => Texture?.MipLevels ?? 0;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;
        #endregion

        #region Methods.
        /// <summary>Function to retrieve the necessary parameters to create the native view.</summary>
        /// <returns>The D3D11 UAV descriptor.</returns>
        private protected override ref readonly D3D11.UnorderedAccessViewDescription1 OnGetUavParams()
        {
            UavDesc = new D3D11.UnorderedAccessViewDescription1
            {
                Format = (DXGI.Format)Format,
                Dimension = D3D11.UnorderedAccessViewDimension.Texture3D,
                Texture3D =
                                    {
                                        MipSlice = MipSlice,
                                        FirstWSlice = StartDepthSlice,
                                        WSize = DepthSliceCount
                                    }
            };

            return ref UavDesc;
        }

        /// <summary>
        /// Function to convert a texel coordinate into a pixel coordinate and a depth slice.
        /// </summary>
        /// <param name="texelCoordinates">The texel coordinates to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>The pixel coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public (DX.Point, int) ToPixel(DX.Vector3 texelCoordinates, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return (new DX.Point((int)(texelCoordinates.X * width), (int)(texelCoordinates.Y * height)), (int)(texelCoordinates.Z * DepthSliceCount));
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return (new DX.Point((int)(texelCoordinates.X * width), (int)(texelCoordinates.Y * height)), (int)(texelCoordinates.Z * DepthSliceCount));
        }

        /// <summary>
        /// Function to convert a pixel coordinate into a texel coordinate.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <param name="depthSlice">[Optional] The depth slice to use.</param>
        /// <returns>The texel coordinates.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// <para>
        /// If specified, the <paramref name="depthSlice"/> only applies to the <see cref="StartDepthSlice"/> and <see cref="DepthSliceCount"/> for this view, it will be constrained if it falls outside of that
        /// range. Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first depth slice
        /// is used.
        /// </para>
        /// </remarks>
        public DX.Vector3 ToTexel(DX.Point pixelCoordinates, int? mipLevel = null, int? depthSlice = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;
            float depth = depthSlice ?? 0;

            depth = depth.Min(DepthSliceCount + StartDepthSlice).Max(StartDepthSlice);

            if (mipLevel is null)
            {
                return new DX.Vector3(pixelCoordinates.X / width, pixelCoordinates.Y / height, depth / DepthSliceCount);
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);

            return new DX.Vector3(pixelCoordinates.X / width, pixelCoordinates.Y / height, depth / DepthSliceCount);
        }

        /// <summary>
        /// Function to convert a texel size into a pixel size.
        /// </summary>
        /// <param name="texelCoordinates">The texel size to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>The pixel size.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public DX.Size2 ToPixel(DX.Size2F texelCoordinates, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new DX.Size2((int)(texelCoordinates.Width * width), (int)(texelCoordinates.Height * height));
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);
            return new DX.Size2((int)(texelCoordinates.Width * width), (int)(texelCoordinates.Height * height));
        }

        /// <summary>
        /// Function to convert a pixel size into a texel size.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel size to convert.</param>
        /// <param name="mipLevel">[Optional] The mip level to use.</param>
        /// <returns>The texel size.</returns>
        /// <remarks>
        /// <para>
        /// If specified, the <paramref name="mipLevel"/> only applies to the <see cref="MipSlice"/> for this view, it will be constrained if it falls outside of that range.
        /// Because of this, the coordinates returned may not be the exact size of the texture bound to the view at mip level 0. If the <paramref name="mipLevel"/> is omitted, then the first mip level
        /// for the underlying <see cref="Texture"/> is used.
        /// </para>
        /// </remarks>
        public DX.Size2F ToTexel(DX.Size2 pixelCoordinates, int? mipLevel = null)
        {
            float width = Texture.Width;
            float height = Texture.Height;

            if (mipLevel is null)
            {
                return new DX.Size2F(pixelCoordinates.Width / width, pixelCoordinates.Height / height);
            }

            width = GetMipWidth(mipLevel.Value);
            height = GetMipHeight(mipLevel.Value);
            return new DX.Size2F(pixelCoordinates.Width / width, pixelCoordinates.Height / height);
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
        /// Function to return the depth of the texture at the current <see cref="MipSlice"/> in slices.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the texture.</returns>
        public int GetMipDepth(int mipLevel)
        {
            mipLevel = mipLevel.Min(Texture.MipLevels + MipSlice).Max(MipSlice);

            return (Depth >> mipLevel).Max(1);
        }

        /// <summary>
        /// Function to create a new texture that is bindable to the GPU as an unordered access resource.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="info">The information about the texture.</param>
        /// <param name="initialData">[Optional] Initial data used to populate the texture.</param>
        /// <returns>A new <see cref="GorgonTexture3DReadWriteView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonTexture3D"/> and a <see cref="GorgonTexture3DReadWriteView"/> as a single object that users can use to apply a texture as an unordered  
        /// access resource. This helps simplify creation of a texture by executing some prerequisite steps on behalf of the user.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture3D"/> created by this method is linked to the <see cref="GorgonTexture3DReadWriteView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture3DReadWriteView"/> from the <see cref="GorgonTexture3D.GetReadWriteView"/> method on the <see cref="GorgonTexture3D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// <para>
        /// If an <paramref name="initialData"/> image is provided, and the width/height/depth is not the same as the values in the <paramref name="info"/> parameter, then the image data will be cropped to
        /// match the values in the <paramref name="info"/> parameter. Things like array count, and mip levels will still be taken from the <paramref name="initialData"/> image parameter.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture3D"/>
        public static GorgonTexture3DReadWriteView CreateTexture(GorgonGraphics graphics, IGorgonTexture3DInfo info, IGorgonImage initialData = null)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var newInfo = new GorgonTexture3DInfo(info)
            {
                Usage = info.Usage == ResourceUsage.Staging ? ResourceUsage.Default : info.Usage,
                Binding = (((info.Binding & TextureBinding.ReadWriteView) != TextureBinding.ReadWriteView)
                                             ? (info.Binding | TextureBinding.ReadWriteView)
                                             : info.Binding) & ~TextureBinding.DepthStencil // There's now way we can build a depth/stencil from this method.
            };

            if (initialData is not null)
            {
                if ((initialData.Width > info.Width)
                    || (initialData.Height > info.Height)
                    || (initialData.Depth > info.Depth))
                {
                    initialData = initialData.BeginUpdate()
                                             .Expand(info.Width, info.Height, info.Depth)
                                             .EndUpdate();
                }

                if ((initialData.Width < info.Width)
                    || (initialData.Height < info.Height)
                    || (initialData.Depth < info.Depth))
                {
                    initialData = initialData.BeginUpdate()
                                             .Crop(new DX.Rectangle(0, 0, info.Width, info.Height), info.Depth)
                                             .EndUpdate();
                }
            }

            GorgonTexture3D texture = initialData is null
                                          ? new GorgonTexture3D(graphics, newInfo)
                                          : initialData.ToTexture3D(graphics,
                                                                    new GorgonTextureLoadOptions
                                                                    {
                                                                        Usage = newInfo.Usage,
                                                                        Binding = newInfo.Binding,
                                                                        Name = newInfo.Name
                                                                    });

            GorgonTexture3DReadWriteView result = texture.GetReadWriteView();
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
        /// <returns>A new <see cref="GorgonTexture3DReadWriteView"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture3D"/> object and return a <see cref="GorgonTexture3DReadWriteView"/>.
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
        /// Since the <see cref="GorgonTexture3D"/> created by this method is linked to the <see cref="GorgonTexture3DReadWriteView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture3DReadWriteView"/> from the <see cref="GorgonTexture3D.GetShaderResourceView"/> method on the <see cref="GorgonTexture3D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture3DReadWriteView FromStream(GorgonGraphics graphics,
                                                    Stream stream,
                                                    IGorgonImageCodec codec,
                                                    long? size = null,
                                                    GorgonTextureLoadOptions options = null)
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
            GorgonTexture3D texture = image.ToTexture3D(graphics, options);
            GorgonTexture3DReadWriteView view = texture.GetReadWriteView();
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
        /// <returns>A new <see cref="GorgonTexture3DReadWriteView"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture3D"/> object and return a <see cref="GorgonTexture3DReadWriteView"/>.
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
        /// Since the <see cref="GorgonTexture3D"/> created by this method is linked to the <see cref="GorgonTexture3DReadWriteView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture3DReadWriteView"/> from the <see cref="GorgonTexture3D.GetShaderResourceView"/> method on the <see cref="GorgonTexture3D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture3DReadWriteView FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTextureLoadOptions options = null)
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
            GorgonTexture3D texture = image.ToTexture3D(graphics, options);
            GorgonTexture3DReadWriteView view = texture.GetReadWriteView();
            view.OwnsResource = true;
            return view;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture3DReadWriteView"/> class.
        /// </summary>
        /// <param name="texture">The texture to view.</param>
        /// <param name="format">The format for the view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <param name="firstMipLevel">The first mip level to view.</param>
        /// <param name="startDepthSlice">The first depth slice to view.</param>
        /// <param name="depthSliceCount">The number of depth slices to view.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
        internal GorgonTexture3DReadWriteView(GorgonTexture3D texture,
                                    BufferFormat format,
                                    GorgonFormatInfo formatInfo,
                                    int firstMipLevel,
                                    int startDepthSlice,
                                    int depthSliceCount)
            : base(texture)
        {
            FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
            Format = format;
            Texture = texture;
            Bounds = new DX.Rectangle(0, 0, Width, Height);
            MipSlice = firstMipLevel;
            StartDepthSlice = startDepthSlice;
            DepthSliceCount = depthSliceCount;
        }
        #endregion
    }
}
