#region MIT
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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using SharpDX.DXGI;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides an unordered access view for a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonTexture"/>. The texture must have been created with the <see cref="TextureBinding.UnorderedAccess"/> flag in its 
    /// <see cref="IGorgonTextureInfo.Binding"/> property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallBase">draw call</see>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Unordered access views do not support multisampled <see cref="GorgonTexture"/>s.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallBase"/>
    public sealed class GorgonTextureUav
        : GorgonUnorderedAccessView
    {
        #region Variables.
        // Log used for debugging.
        private readonly IGorgonLog _log;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the index of the first mip map in the resource to view.
        /// </summary>
        /// <remarks>
        /// If this view is for a 2D <see cref="GorgonTexture"/>, and that texture is multisampled, then this value will be set to 0.
        /// </remarks>
        public int MipSlice
        {
            get;
        }

        /// <summary>
        /// Property to return the first array index or depth slice to use in the view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a 1D or 2D <see cref="GorgonTexture"/>, this value indicates an array index.  
        /// </para>
        /// <para>
        /// For a 3D <see cref="GorgonTexture"/>, this value indicates the first depth slice to view.
        /// </para>
        /// </remarks>
        public int ArrayOrDepthIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the number of array indices or depth slices to use in the view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a 1D or 2D <see cref="GorgonTexture"/>, this value indicates an array index.
        /// </para>
        /// <para>
        /// For a 2D <see cref="GorgonTexture"/>, this value must be a multiple of 6 if the texture is a 2D texture cube.
        /// </para>
        /// <para>
        /// For a 3D <see cref="GorgonTexture"/>, this value will represent the number of depth slices to view.
        /// </para>
        /// </remarks>
        public int ArrayOrDepthCount
        {
            get;
        }

        /// <summary>
        /// Property to return whether the texture is a texture cube or not.
        /// </summary>
        /// <remarks>
        /// This value is always <b>false</b> for a 1D or 3D <see cref="GorgonTexture"/>.
        /// </remarks>
        public bool IsCubeMap => Texture.Info.IsCubeMap;

        /// <summary>
        /// Property to return the texture that is bound to this view.
        /// </summary>
        public GorgonTexture Texture
        {
            get;
        }

        /// <summary>
        /// Property to return the format used to interpret this view.
        /// </summary>
        public Format Format
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
        /// Property to return the bounding rectangle for the view.
        /// </summary>
        /// <remarks>
        /// This value is the full bounding rectangle of the first mip map level for the texture associated with the render target.
        /// </remarks>
        public DX.Rectangle Bounds
        {
            get;
        }

        /// <summary>
        /// Property to return the type of texture bound to this view.
        /// </summary>
        public TextureType TextureType => Texture.Info.TextureType;

        /// <summary>
        /// Property to return the width of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full width of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Width => Texture.Info.Width;

        /// <summary>
        /// Property to return the height of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full height of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Height => Texture.Info.Height;

        /// <summary>
        /// Property to return the depth of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full depth of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Depth => Texture.Info.Depth;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description for a 1D texture.
        /// </summary>
        /// <param name="texture">Texture to build a view description for.</param>
        /// <returns>The shader view description.</returns>
        private D3D11.UnorderedAccessViewDescription GetDesc1D(GorgonTexture texture)
        {
            return new D3D11.UnorderedAccessViewDescription
                   {
                       Format = Format,
                       Dimension = texture.Info.ArrayCount > 1
                                       ? D3D11.UnorderedAccessViewDimension.Texture1DArray
                                       : D3D11.UnorderedAccessViewDimension.Texture1D,
                       Texture1DArray =
                       {
                           MipSlice = MipSlice,
                           ArraySize = ArrayOrDepthCount,
                           FirstArraySlice = ArrayOrDepthIndex
                       }
                   };
        }

        /// <summary>
        /// Function to retrieve the view description for a 2D texture.
        /// </summary>
        /// <param name="texture">Texture to build a view description for.</param>
        /// <returns>The shader view description.</returns>
        private D3D11.UnorderedAccessViewDescription GetDesc2D(GorgonTexture texture)
        {
            return new D3D11.UnorderedAccessViewDescription
                   {
                       Format = Format,
                       Dimension = texture.Info.ArrayCount > 1
                                       ? D3D11.UnorderedAccessViewDimension.Texture2DArray
                                       : D3D11.UnorderedAccessViewDimension.Texture2D,
                       Texture2DArray =
                       {
                           MipSlice =  MipSlice,
                           FirstArraySlice = ArrayOrDepthIndex,
                           ArraySize = ArrayOrDepthCount
                       }
                   };
        }

        /// <summary>
        /// Function to retrieve the view description for a 3D texture.
        /// </summary>
        /// <returns>The shader view description.</returns>
        private D3D11.UnorderedAccessViewDescription GetDesc3D()
        {
            return new D3D11.UnorderedAccessViewDescription
                   {
                       Format = Format,
                       Dimension = D3D11.UnorderedAccessViewDimension.Texture3D,
                       Texture3D =
                       {
                           MipSlice = MipSlice,
                           FirstWSlice = ArrayOrDepthIndex,
                           WSize = ArrayOrDepthCount
                       }
                   };
        }

        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            D3D11.UnorderedAccessViewDescription desc;
            GorgonTexture texture = (GorgonTexture)Resource;

            _log.Print("Creating texture unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);

            // Build SRV description.
            switch (Resource.ResourceType)
            {
                case GraphicsResourceType.Texture1D:
                    desc = GetDesc1D(texture);
                    break;
                case GraphicsResourceType.Texture2D:
                    desc = GetDesc2D(texture);
                    break;
                case GraphicsResourceType.Texture3D:
                    desc = GetDesc3D();
                    break;
                default:
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_IMAGE_TYPE_INVALID, Resource.ResourceType));
            }

            try
            {
                _log.Print("Gorgon resource view: Creating D3D 11 unordered access resource view.", LoggingLevel.Verbose);

                // Create our SRV.
                NativeView = new D3D11.UnorderedAccessView(Resource.Graphics.VideoDevice.D3DDevice(), Resource.D3DResource, desc)
                          {
                              DebugName = $"'{Texture.Name}': D3D 11 Unordered access view"
                };
            }
            catch (DX.SharpDXException sDXEx)
            {
                if ((uint)sDXEx.ResultCode.Code == 0x80070057)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
                                                            texture.Info.Format,
                                                            Format));
                }
            }
        }

        /// <summary>
        /// Function to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipWidth(int mipLevel)
        {
            mipLevel = mipLevel.Min(Texture.Info.MipLevels + MipSlice).Max(MipSlice);
            return Width << mipLevel;
        }

        /// <summary>
        /// Function to return the height of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipHeight(int mipLevel)
        {
            mipLevel = mipLevel.Min(Texture.Info.MipLevels + MipSlice).Max(MipSlice);

            return Height << mipLevel;
        }

        /// <summary>
        /// Function to return the depth of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The depth of the mip map level assigned to the <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipDepth(int mipLevel)
        {
            mipLevel = mipLevel.Min(Texture.Info.MipLevels + MipSlice).Max(MipSlice);

            return Depth << mipLevel;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTextureUav"/> class.
        /// </summary>
        /// <param name="texture">The texture to view.</param>
        /// <param name="format">The format for the view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <param name="firstMipLevel">The first mip level to view.</param>
        /// <param name="arrayDepthIndex">For a 1D or 2D <see cref="GorgonTexture"/>, this will be the first array index to view, for a 3D <see cref="GorgonTexture"/>, this will be the first depth slice.</param>
        /// <param name="arrayDepthCount">For a 1D or 2D <see cref="GorgonTexture"/>, this will be the number of array indices to view, for a 3D <see cref="GorgonTexture"/>, this will be the number of depth slices.</param>
        /// <param name="log">The log used for debugging.</param>
        internal GorgonTextureUav(GorgonTexture texture,
                                  Format format,
                                  GorgonFormatInfo formatInfo,
                                  int firstMipLevel,
                                  int arrayDepthIndex,
                                  int arrayDepthCount,
                                  IGorgonLog log)
            : base(texture)
        {
            _log = log ?? GorgonLogDummy.DefaultInstance;
            Texture = texture;
            Format = format;

            FormatInformation = formatInfo;

            if (FormatInformation.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            Bounds = new DX.Rectangle(0, 0, Width, Height);
            MipSlice = firstMipLevel;
            ArrayOrDepthIndex = arrayDepthIndex;
            ArrayOrDepthCount = arrayDepthCount;
        }
        #endregion
    }
}
