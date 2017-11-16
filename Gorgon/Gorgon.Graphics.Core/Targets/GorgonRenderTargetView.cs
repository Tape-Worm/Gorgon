#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Friday, July 19, 2013 10:13:11 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A view to allow texture based render targets to be bound to the pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A render target view allows a render target (such as a <see cref="GorgonSwapChain"/> or <see cref="GorgonTexture"/>) to be bound to the GPU pipeline as a render target resource.
	/// </para>
	/// <para>
	/// The view can bind the entire resource, or a sub section of the resource as required. It will also allow for casting of the format to allow for reinterpreting the data stored within the the render 
	/// target. 
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonSwapChain"/>
	/// <seealso cref="GorgonTexture"/>
	public sealed class GorgonRenderTargetView
		: IDisposable
    {
		#region Variables.
		// Log interface for debugging.
	    private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the render target view.
		/// </summary>
		internal D3D11.RenderTargetView D3DRenderTargetView
		{
			get;
			set;
		}

        /// <summary>
        /// Property to return the depth/stencil view associated with this view.
        /// </summary>
        public GorgonDepthStencilView DepthStencilView
        {
            get;
        }

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
		/// Property to return the mip slice to use for the view.
		/// </summary>
		public int MipSlice
        {
            get;
        }

        /// <summary>
        /// Property to return the number of array indices or depth slices to use in the view.
        /// </summary>
        /// <remarks>For a 1D/2D render target, this value indicates an array index.  For a 3D render target, this value indicates a depth slice.</remarks>
        public int ArrayOrDepthCount
        {
            get;
        }

        /// <summary>
        /// Property to return the first array index or depth slice to use in the view.
        /// </summary>
        /// <remarks>For a 1D/2D render target, this value indicates an array index.  For a 3D render target, this value indicates a depth slice.</remarks>
        public int ArrayOrDepthIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the type of texture bound to this view.
        /// </summary>
        public TextureType TextureType => Texture.Info.TextureType;

        /// <summary>
        /// Property to return the width of the render target in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full width of the first mip map level for the texture associated with the render target.
        /// </remarks>
        public int Width => Texture.Info.Width;

        /// <summary>
        /// Property to return the height of the render target in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full width of the first mip map level for the texture associated with the render target.
        /// </remarks>
        public int Height => Texture.Info.Height;

        /// <summary>
        /// Property to return the depth for the render target.
        /// </summary>
        public int Depth => Texture.Info.Depth;

        /// <summary>
        /// Property to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.
        /// </remarks>
        public int MipWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the height of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.
        /// </remarks>
        public int MipHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the depth of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the depth of the mip map level assigned to the <see cref="MipSlice"/> for the texture associated with the render target.
        /// </remarks>
        public int MipDepth
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the bounding rectangle for the render target view.
        /// </summary>
        /// <remarks>
        /// This value is the full bounding rectangle of the first mip map level for the texture associated with the render target.
        /// </remarks>
        public DX.Rectangle Bounds
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <returns>The view description.</returns>
        private D3D11.RenderTargetViewDescription GetDesc1D()
        {
            // Set up for arrayed and multisampled texture.
            if (ArrayOrDepthCount > 1)
            {
                return new D3D11.RenderTargetViewDescription
                {
                    Format = (DXGI.Format)Format,
                    Dimension = D3D11.RenderTargetViewDimension.Texture1DArray,
                    Texture1DArray =
                    {
                        MipSlice = MipSlice,
                        FirstArraySlice = ArrayOrDepthIndex,
                        ArraySize = ArrayOrDepthCount
                    }
                };
            }

            return new D3D11.RenderTargetViewDescription
            {
                Format = (DXGI.Format)Format,
                Dimension = D3D11.RenderTargetViewDimension.Texture1D,
                Texture1D =
                {
                    MipSlice = MipSlice
                }
            };
        }

        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <param name="isMultisampled"><b>true</b> if the texture is multisampled, <b>false</b> if not.</param>
        /// <returns>The view description.</returns>
        private D3D11.RenderTargetViewDescription GetDesc2D(bool isMultisampled)
        {
            // Set up for arrayed and multisampled texture.
            if (ArrayOrDepthCount > 1)
            {
                return new D3D11.RenderTargetViewDescription
                {
                    Format = (DXGI.Format)Format,
                    Dimension = isMultisampled
                        ? D3D11.RenderTargetViewDimension.Texture2DMultisampledArray
                        : D3D11.RenderTargetViewDimension.Texture2DArray,
                    Texture2DArray =
                    {
                        MipSlice = isMultisampled ? ArrayOrDepthIndex : MipSlice,
                        FirstArraySlice = isMultisampled ? ArrayOrDepthCount : ArrayOrDepthIndex,
                        ArraySize = isMultisampled ? 0 : ArrayOrDepthCount
                    }
                };
            }

            return new D3D11.RenderTargetViewDescription
            {
                Format = (DXGI.Format)Format,
                Dimension = isMultisampled
                    ? D3D11.RenderTargetViewDimension.Texture2DMultisampled
                    : D3D11.RenderTargetViewDimension.Texture2D,
                Texture2D =
                {
                    MipSlice = isMultisampled ? 0 : MipSlice
                }
            };
        }

        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <returns>The view description.</returns>
        private D3D11.RenderTargetViewDescription GetDesc3D()
        {
            return new D3D11.RenderTargetViewDescription
            {
                Format = (DXGI.Format)Format,
                Dimension = D3D11.RenderTargetViewDimension.Texture3D,
                Texture3D =
                {
                    MipSlice = MipSlice,
                    FirstDepthSlice = ArrayOrDepthIndex,
                    DepthSliceCount = ArrayOrDepthCount
                }
            };
        }

        /// <summary>
        /// Function to perform initialization of the view.
        /// </summary>
        internal void CreateNativeView()
        {
            D3D11.RenderTargetViewDescription desc = default(D3D11.RenderTargetViewDescription);

			_log.Print($"Render Target View '{Texture.Name}': Creating D3D11 render target view.", LoggingLevel.Simple);

			desc.Dimension = D3D11.RenderTargetViewDimension.Unknown;

            switch (Texture.ResourceType)
            {
                case GraphicsResourceType.Texture1D:
                    desc = GetDesc1D();
                    break;
                case GraphicsResourceType.Texture2D:
                    desc = GetDesc2D(!Texture.Info.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling));
                    break;
                case GraphicsResourceType.Texture3D:
                    desc = GetDesc3D();
                    break;
            }

			if (desc.Dimension == D3D11.RenderTargetViewDimension.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
            }
            
            MipWidth = (Width >> MipSlice).Max(1);
            MipHeight = (Height >> MipSlice).Max(1);
            MipDepth = (Depth >> MipSlice).Max(1);
            
            Bounds = new DX.Rectangle(0, 0, Width, Height);

	        _log.Print($"Render Target View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array/Depth Index: {ArrayOrDepthIndex}, Array/Depth Count: {ArrayOrDepthCount}",
	                   LoggingLevel.Verbose);

	        D3DRenderTargetView = new D3D11.RenderTargetView(Texture.Graphics.VideoDevice.D3DDevice(), Texture.D3DResource, desc)
	                              {
		                              DebugName = $"'{Texture.Name}': D3D 11 Render target view"
	                              };
        }

		/// <summary>
		/// Function to clear the contents of the render target for this view.
		/// </summary>
		/// <param name="color">Color to use when clearing the render target view.</param>
		public void Clear(GorgonColor color)
		{
			Texture.Graphics.D3DDeviceContext.ClearRenderTargetView(D3DRenderTargetView, color.ToRawColor4());
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (D3DRenderTargetView != null)
			{
				_log.Print($"Render Target View '{Texture.Name}': Releasing D3D11 render target view.", LoggingLevel.Simple);
			}

			D3DRenderTargetView?.Dispose();
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTargetView"/> class.
        /// </summary>
        /// <param name="texture">The render target texture to bind.</param>
        /// <param name="format">The format of the render target view.</param>
        /// <param name="mipSlice">The mip slice to use in the view.</param>
        /// <param name="arrayOrDepthIndex">The first array index to use in the view.</param>
        /// <param name="arrayOrDepthCount">The number of array indices to use in the view.</param>
        /// <param name="log">Logging interface for debugging.</param>
        internal GorgonRenderTargetView(GorgonTexture texture, BufferFormat format, int mipSlice, int arrayOrDepthIndex, int arrayOrDepthCount, IGorgonLog log)
        {
	        _log = log ?? GorgonLogDummy.DefaultInstance;
            Texture = texture;

            if (format == BufferFormat.Unknown)
            {
                format = texture.Info.Format;
            }

            if ((texture.Info.Binding & TextureBinding.RenderTarget) != TextureBinding.RenderTarget)
	        {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RESOURCE_IS_NOT_RENDERTARGET, texture.Name), nameof(texture));
	        }

            // Arrays only apply to 1D/2D textures.
            if (texture.Info.TextureType != TextureType.Texture3D)
            {
                if (arrayOrDepthIndex + arrayOrDepthCount > texture.Info.ArrayCount)
                {
                    throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_ARRAY_OUT_OF_RANGE,
                                                              arrayOrDepthIndex,
                                                              arrayOrDepthCount,
                                                              texture.Info.ArrayCount));
                }
            }
            else
            {
                if (arrayOrDepthIndex + arrayOrDepthCount > texture.Info.Depth)
                {
                    throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_DEPTH_OUT_OF_RANGE,
                                                              arrayOrDepthIndex,
                                                              arrayOrDepthCount,
                                                              texture.Info.Depth));
                }
            }
           

            FormatInformation = new GorgonFormatInfo(Format);

	        if (FormatInformation.IsTypeless)
	        {
				throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
			}

            DepthStencilView = texture.AssociatedDepthStencil?.DefaultDepthStencilView;
            Format = format;
            MipSlice = mipSlice;
            ArrayOrDepthIndex = arrayOrDepthIndex;
            ArrayOrDepthCount = arrayOrDepthCount;
            MipWidth = (Width << MipSlice).Max(1);
            MipHeight = (Height << MipSlice).Max(1);
            MipDepth = (Depth << MipSlice).Max(1);
            Bounds = new DX.Rectangle(0, 0, Width, Height);
        }
        #endregion
    }
}