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
// Created: Saturday, June 8, 2013 4:53:00 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A depth/stencil view for textures.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a depth/stencil view to allow a <see cref="GorgonTexture"/> to be bound to the GPU pipeline as a depth/stencil resource.
	/// </para>
	/// <para>
	/// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
	/// format within the same group.	
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonTexture"/>
	public class GorgonDepthStencilView
    {
		#region Variables.
		// A log for debug logging.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D depth/stencil view.
		/// </summary>
		internal D3D11.DepthStencilView Native
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the texture bound to this view.
		/// </summary>
		public GorgonTexture Texture
		{
			get;
		}

		/// <summary>
		/// Property to return the format for this view.
		/// </summary>
		public BufferFormat Format
		{
			get;
		}

		/// <summary>
		/// Property to return the flags for this view.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will allow the depth/stencil buffer to be read simultaneously from the depth/stencil view and from a shader view.  It is not normally possible to bind a view of a resource to 2 parts of the 
		/// pipeline at the same time.  However, with the flags provided, read-only access may be granted to a part of the resource (depth or stencil) or all of it for all parts of the pipline.  This would bind 
		/// the depth/stencil as a read-only view and make it a read-only view accessible to shaders.
		/// </para>
		/// <para>
		/// This is only valid if the resource allows shader access.
		/// </para>
		/// <para>
		/// This is only valid on video adapters with a feature level of 11.0 or better.
		/// </para>
		/// </remarks>
		public D3D11.DepthStencilViewFlags Flags
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
		/// Property to return the number of array indices to use in the view.
		/// </summary>
		public int ArrayCount
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
		/// Property to return the format information for the <see cref="Format"/> of this view.
		/// </summary>
		public GorgonFormatInfo FormatInformation
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
        /// Property to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.
        /// </remarks>
        public int MipWidth
        {
            get;
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
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the description for a 1D depth/stencil view.
        /// </summary>
        /// <returns>The direct 3D 1D depth/stencil view description.</returns>
        private D3D11.DepthStencilViewDescription GetDesc1D()
		{
			// Set up for arrayed and multisampled texture.
			if (Texture.Info.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = (DXGI.Format)Format,
					Dimension = D3D11.DepthStencilViewDimension.Texture1DArray,
					Texture1DArray =
					{
						MipSlice = MipSlice,
						FirstArraySlice = ArrayIndex,
						ArraySize = ArrayCount
					}
				};
			}

			return new D3D11.DepthStencilViewDescription
			{
				Format = (DXGI.Format)Format,
				Dimension = D3D11.DepthStencilViewDimension.Texture1D,
				Texture1D =
				{
					MipSlice = MipSlice
				}
			};
		}

		/// <summary>
		/// Function to retrieve the description for a 2D depth/stencil view.
		/// </summary>
		/// <returns>The direct 3D 2D depth/stencil view description.</returns>
		private D3D11.DepthStencilViewDescription GetDesc2D()
		{
			bool isMultisampled = Texture.Info.MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling;

			// Set up for arrayed and multisampled texture.
			if (Texture.Info.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = (DXGI.Format)Format,
					Dimension = isMultisampled
									? D3D11.DepthStencilViewDimension.Texture2DMultisampledArray
									: D3D11.DepthStencilViewDimension.Texture2DArray,
					Texture2DArray =
					{
						MipSlice = isMultisampled ? ArrayIndex : MipSlice,
						FirstArraySlice = isMultisampled ? ArrayCount : ArrayIndex,
						ArraySize = isMultisampled ? 0 : ArrayCount
					}
				};
			}

			return new D3D11.DepthStencilViewDescription
			{
				Format = (DXGI.Format)Format,
				Dimension = isMultisampled
								? D3D11.DepthStencilViewDimension.Texture2DMultisampled
								: D3D11.DepthStencilViewDimension.Texture2D,
				Texture2D =
				{
					MipSlice = isMultisampled ? 0 : MipSlice
				}
			};
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		internal void CreateNativeView()
		{
			D3D11.DepthStencilViewDescription desc = default(D3D11.DepthStencilViewDescription);

			_log.Print($"'{Texture.Name}': Creating D3D11 depth/stencil view.", LoggingLevel.Simple);

			desc.Dimension = D3D11.DepthStencilViewDimension.Unknown;

			switch (Texture.ResourceType)
			{
				case GraphicsResourceType.Texture1D:
					desc = GetDesc1D();
					break;
				case GraphicsResourceType.Texture2D:
					desc = GetDesc2D();
					break;
			}

			if (desc.Dimension == D3D11.DepthStencilViewDimension.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
			}

            _log.Print($"Depth/Stencil View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}",
					   LoggingLevel.Verbose);

			Native = new D3D11.DepthStencilView(Texture.Graphics.VideoDevice.D3DDevice(), Texture.D3DResource, desc)
			          {
				          DebugName = $"'{Texture.Name}': D3D11 depth/stencil view"
			          };
		}

		/// <summary>
		/// Function to clear the depth and stencil portion of the buffer for this view.
		/// </summary>
		/// <param name="depthValue">The depth value to write to the depth portion of the buffer.</param>
		/// <param name="stencilValue">The stencil value to write to the stencil portion of the buffer.</param>
		/// <remarks>
		/// <para>
		/// If the view <see cref="Format"/> does not have a stencil component, then the <paramref name="stencilValue"/> will be ignored. Likewise, if the <see cref="Format"/> lacks a depth component, 
		/// then the <paramref name="depthValue"/> will be ignored.
		/// </para>
		/// </remarks>
		public void Clear(float depthValue, byte stencilValue)
		{
			D3D11.DepthStencilClearFlags clearFlags = 0;

			if (FormatInformation.HasDepth)
			{
				clearFlags = D3D11.DepthStencilClearFlags.Depth;
			}

			if (FormatInformation.HasStencil)
			{
				clearFlags |= D3D11.DepthStencilClearFlags.Stencil;
			}

			Texture.Graphics.D3DDeviceContext.ClearDepthStencilView(Native, clearFlags, depthValue, stencilValue);
		}

		/// <summary>
		/// Function to clear the depth portion of the buffer for this view.
		/// </summary> 
		/// <param name="depthValue">The depth value to write to the buffer.</param>
		/// <remarks>
		/// <para>
		/// If the view <see cref="Format"/> does not have a depth component, then this method will do nothing.
		/// </para>
		/// </remarks>
		public void ClearDepth(float depthValue)
		{
			if (!FormatInformation.HasDepth)
			{
				return;
			}

			Texture.Graphics.D3DDeviceContext.ClearDepthStencilView(Native, D3D11.DepthStencilClearFlags.Depth, depthValue, 0);
		}

		/// <summary>
		/// Function to clear the stencil portion of the buffer for this view.
		/// </summary>
		/// <param name="stencilValue">The stencil value to write to the buffer.</param>
		/// <remarks>
		/// <para>
		/// If the view <see cref="Format"/> does not have a stencil component, then this method will do nothing.
		/// </para>
		/// </remarks>
		public void ClearStencil(byte stencilValue)
		{
			if (!FormatInformation.HasStencil)
			{
				return;
			}

			Texture.Graphics.D3DDeviceContext.ClearDepthStencilView(Native, D3D11.DepthStencilClearFlags.Stencil, 1.0f, stencilValue);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (Native != null)
			{
				_log.Print($"Destroying depth/stencil view for {Texture.Name}.", LoggingLevel.Verbose);
			}

			Native?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilView"/> class.
		/// </summary>
		/// <param name="texture">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="mipSlice">The mip level to use for the view.</param>
		/// <param name="firstArrayIndex">The first array index to use for the view.</param>
		/// <param name="arrayCount">The number of array indices to use for the view.</param>
		/// <param name="flags">Depth/stencil view flags.</param>
		/// <param name="log">[Optional] The log used for debugging.</param>
		internal GorgonDepthStencilView(GorgonTexture texture,
		                              BufferFormat format,
		                              int mipSlice,
		                              int firstArrayIndex,
		                              int arrayCount,
		                              D3D11.DepthStencilViewFlags flags,
									  IGorgonLog log)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;
			Texture = texture ?? throw new ArgumentNullException(nameof(texture));

		    if ((texture.Info.Binding & TextureBinding.DepthStencil) != TextureBinding.DepthStencil)
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_RESOURCE_NOT_DEPTHSTENCIL, texture.Name), nameof(texture));
		    }

		    FormatInformation = new GorgonFormatInfo(format);

		    if (FormatInformation.IsTypeless)
		    {
		        throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
		    }

		    // Arrays only apply to 1D/2D textures.
		    if (firstArrayIndex + arrayCount > texture.Info.ArrayCount)
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_ARRAY_OUT_OF_RANGE,
		                                                  firstArrayIndex,
		                                                  arrayCount,
		                                                  texture.Info.ArrayCount));
		    }

		    Format = format;


		    MipSlice = texture.Info.MipLevels <= 0 ? 0 : mipSlice.Max(0).Min(texture.Info.MipLevels - 1);
            ArrayIndex = firstArrayIndex;
		    ArrayCount = arrayCount;
			Flags = flags;
		    MipWidth = (Width >> MipSlice).Max(1);
		    MipHeight = (Height >> MipSlice).Max(1);

		    Bounds = new DX.Rectangle(0, 0, Width, Height);
		}
		#endregion
	}
}
