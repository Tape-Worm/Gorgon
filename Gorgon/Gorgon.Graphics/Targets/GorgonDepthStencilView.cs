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

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Gorgon.Graphics
{
#warning TODO: Clean this up to be like TextureShaderView.
	/// <summary>
	/// A depth/stencil view to allow a texture to be bound to the pipeline as a depth/stencil buffer.
	/// </summary>
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
		internal D3D11.DepthStencilView D3DView
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
		public DXGI.Format Format
		{
			get;
		}

        /// <summary>
        /// Property to return the flags for this view.
        /// </summary>
        /// <remarks>This will allow the depth/stencil buffer to be read simultaneously from the depth/stencil view and from a shader view.  It is not normally possible to bind a view of a resource to 2 parts of the 
        /// pipeline at the same time.  However, using the flags provided, read-only access may be granted to a part of the resource (depth or stencil) or all of it for all parts of the pipline.  This would bind 
        /// the depth/stencil as a read-only view and make it a read-only view accessible to shaders.
        /// <para>This is only valid if the resource allows shader access.</para>
        /// <para>This is only valid on video devices with a feature level of SM5 or better.</para>
        /// </remarks>
        public D3D11.DepthStencilViewFlags Flags
        {
            get;
            private set;
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
		/// Property to return the first array index or depth slice to use in the view.
		/// </summary>
		public int FirstArrayIndex
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
					Format = Format,
					Dimension = D3D11.DepthStencilViewDimension.Texture1DArray,
					Texture1DArray =
					{
						MipSlice = MipSlice,
						FirstArraySlice = FirstArrayIndex,
						ArraySize = ArrayCount
					}
				};
			}

			return new D3D11.DepthStencilViewDescription
			{
				Format = Format,
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
			bool isMultiSampled = Texture.Info.MultiSampleInfo != GorgonMultiSampleInfo.NoMultiSampling;

			// Set up for arrayed and multisampled texture.
			if (Texture.Info.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = Format,
					Dimension = isMultiSampled
									? D3D11.DepthStencilViewDimension.Texture2DMultisampledArray
									: D3D11.DepthStencilViewDimension.Texture2DArray,
					Texture2DArray =
					{
						MipSlice = isMultiSampled ? FirstArrayIndex : MipSlice,
						FirstArraySlice = isMultiSampled ? ArrayCount : FirstArrayIndex,
						ArraySize = isMultiSampled ? 0 : ArrayCount
					}
				};
			}

			return new D3D11.DepthStencilViewDescription
			{
				Format = Format,
				Dimension = isMultiSampled
								? D3D11.DepthStencilViewDimension.Texture2DMultisampled
								: D3D11.DepthStencilViewDimension.Texture2D,
				Texture2D =
				{
					MipSlice = isMultiSampled ? 0 : MipSlice
				}
			};
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		private void Initialize()
		{
			D3D11.DepthStencilViewDescription desc = default(D3D11.DepthStencilViewDescription);

			desc.Dimension = D3D11.DepthStencilViewDimension.Unknown;

			switch (Texture.ResourceType)
			{
				case ResourceType.Texture1D:
					desc = GetDesc1D();
					break;
				case ResourceType.Texture2D:
					desc = GetDesc2D();
					break;
			}

			if (desc.Dimension == D3D11.DepthStencilViewDimension.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
			}

			D3DView = new D3D11.DepthStencilView(Texture.Graphics.VideoDevice.D3DDevice, Texture.D3DResource, desc)
			          {
				          DebugName = $"'{Texture.Name}': D3D11 Depth/stencil view"
			          };
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_log.Print($"Destroying depth/stencil view for {Texture.Name}.", LoggingLevel.Verbose);
			D3DView?.Dispose();
			D3DView = null;
		}
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="mipSlice">The mip level to use for the view.</param>
		/// <param name="firstArrayIndex">The first array index to use for the view.</param>
		/// <param name="arrayCount">The number of array indices to use for the view.</param>
		/// <param name="flags">Depth/stencil view flags.</param>
		internal GorgonDepthStencilView(GorgonTexture resource, DXGI.Format format, int mipSlice, int firstArrayIndex, int arrayCount, D3D11.DepthStencilViewFlags flags)
		{
			Format = format;
			Texture = resource;
			MipSlice = mipSlice;
			FirstArrayIndex = firstArrayIndex;
			ArrayCount = arrayCount;
		    Flags = flags;
			Initialize();
		}
		#endregion
	}
}
