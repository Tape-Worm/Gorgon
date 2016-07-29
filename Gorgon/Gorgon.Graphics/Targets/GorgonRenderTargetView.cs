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
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A view to allow texture based render targets to be bound to the pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A render target view allows a render target (such as a <see cref="GorgonSwapChain"/>) to be bound to the GPU pipeline as a render target resource.
	/// </para>
	/// <para>
	/// The view can bind the entire resource, or a sub section of the resource as required. It will also allow for casting of the format to allow for reinterpreting the data stored within the the render 
	/// target. 
	/// </para>
	/// </remarks>
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
		internal D3D.RenderTargetView D3DRenderTargetView
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the key for the resource view.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This key can be used to sort, or define a unique resource view for use in caching. Users may set this key however they see fit to meet their caching/sorting needs. However, it is recommended 
		/// that this key be left alone, and never altered after it's been applied to a cache since it should be a unique value.
		/// </para>
		/// <para>
		/// By default, Gorgon will set the view parameters as a 64 bit unsigned integer value when the view is created. This key is composed of the following bits:
		/// <para>
		/// <list type="table">
		///		<listheader>
		///			<term>Bits</term>
		///			<term>Value</term>		
		///		</listheader>
		///		<item>
		///			<term>0 - 3 (4 bits)</term>
		///			<term><see cref="MipSlice">Mip slice.</see></term>
		///		</item>
		///		<item>
		///			<term>4 - 14 (11 bits)</term>
		///			<term><see cref="FirstArrayOrDepthIndex">Array/Depth Index.</see></term>
		///		</item>
		///		<item>
		///			<term>15 - 25 (11 bits)</term>
		///			<term><see cref="ArrayOrDepthCount">Array/Depth Count.</see></term>
		///		</item>
		///		<item>
		///			<term>25 - 33 (8 bits)</term>
		///			<term><see cref="Format"/></term>
		///		</item>
		/// </list>
		/// </para>
		/// </para>
		/// <para>
		/// For example, a <see cref="MipSlice"/> of 2, and a <see cref="FirstArrayOrDepthIndex"/> of 4, with an <see cref="ArrayOrDepthCount"/> of 2 and a <see cref="Format"/> 
		/// of <c>R8G8B8A8_UNorm</c> (28) would yield a key of: <c>1879113794</c>. 
		/// <br/>
		/// Or, a <see cref="MipSlice"/> of 0, with a <see cref="FirstArrayOrDepthIndex"/> of 0, and a <see cref="ArrayOrDepthCount"/> of 1, and the same buffer format would yield a key of: <c>1879048208</c>.
		/// </para>
		/// </remarks>
		public ulong Key
	    {
		    get;
		    set;
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
	    public DXGI.Format Format
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
        public int FirstArrayOrDepthIndex
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <returns>The view description.</returns>
        private D3D.RenderTargetViewDescription GetDesc1D()
        {
            // Set up for arrayed and multisampled texture.
            if (ArrayOrDepthCount > 1)
            {
                return new D3D.RenderTargetViewDescription
                {
                    Format = Format,
                    Dimension = D3D.RenderTargetViewDimension.Texture1DArray,
                    Texture1DArray =
                    {
                        MipSlice = MipSlice,
                        FirstArraySlice = FirstArrayOrDepthIndex,
                        ArraySize = ArrayOrDepthCount
                    }
                };
            }

            return new D3D.RenderTargetViewDescription
            {
                Format = Format,
                Dimension = D3D.RenderTargetViewDimension.Texture1D,
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
        private D3D.RenderTargetViewDescription GetDesc2D(bool isMultisampled)
        {
            // Set up for arrayed and multisampled texture.
            if (ArrayOrDepthCount > 1)
            {
                return new D3D.RenderTargetViewDescription
                {
                    Format = Format,
                    Dimension = isMultisampled
                        ? D3D.RenderTargetViewDimension.Texture2DMultisampledArray
                        : D3D.RenderTargetViewDimension.Texture2DArray,
                    Texture2DArray =
                    {
                        MipSlice = isMultisampled ? FirstArrayOrDepthIndex : MipSlice,
                        FirstArraySlice = isMultisampled ? ArrayOrDepthCount : FirstArrayOrDepthIndex,
                        ArraySize = isMultisampled ? 0 : ArrayOrDepthCount
                    }
                };
            }

            return new D3D.RenderTargetViewDescription
            {
                Format = Format,
                Dimension = isMultisampled
                    ? D3D.RenderTargetViewDimension.Texture2DMultisampled
                    : D3D.RenderTargetViewDimension.Texture2D,
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
        private D3D.RenderTargetViewDescription GetDesc3D()
        {
            return new D3D.RenderTargetViewDescription
            {
                Format = Format,
                Dimension = D3D.RenderTargetViewDimension.Texture3D,
                Texture3D =
                {
                    MipSlice = MipSlice,
                    FirstDepthSlice = FirstArrayOrDepthIndex,
                    DepthSliceCount = ArrayOrDepthCount
                }
            };
        }

        /// <summary>
        /// Function to perform initialization of the view.
        /// </summary>
        /// <param name="isMultisampled"><b>true</b> if the texture is multisampled, <b>false</b> if not.</param>
        private void Initialize(bool isMultisampled)
        {
            D3D.RenderTargetViewDescription desc = default(D3D.RenderTargetViewDescription);

			_log.Print($"Render Target View '{Texture.Name}': Creating D3D11 render target view.", LoggingLevel.Simple);

			desc.Dimension = D3D.RenderTargetViewDimension.Unknown;

            switch (Texture.ResourceType)
            {
                case ResourceType.Texture1D:
                    desc = GetDesc1D();
                    break;
                case ResourceType.Texture2D:
                    desc = GetDesc2D(isMultisampled);
                    break;
                case ResourceType.Texture3D:
                    desc = GetDesc3D();
                    break;
            }

			if (desc.Dimension == D3D.RenderTargetViewDimension.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
            }

	        _log.Print($"Render Target View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array/Depth Index: {FirstArrayOrDepthIndex}, Array/Depth Count: {ArrayOrDepthCount}",
	                   LoggingLevel.Verbose);

	        D3DRenderTargetView = new D3D.RenderTargetView(Texture.Graphics.VideoDevice.D3DDevice(), Texture.D3DResource, desc)
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
        /// <param name="format">[Optional] The format of the render target view.</param>
        /// <param name="mipSlice">[Optional] The mip slice to use in the view.</param>
        /// <param name="arrayOrDepthIndex">[Optional] The first array index to use in the view.</param>
        /// <param name="arrayOrDepthCount">[Optional] The number of array indices to use in the view.</param>
        /// <param name="log">[Optional] Logging interface for debugging.</param>
        public GorgonRenderTargetView(GorgonTexture texture, DXGI.Format format = DXGI.Format.Unknown, int mipSlice = 0, int arrayOrDepthIndex = 0, int arrayOrDepthCount = 0, IGorgonLog log = null)
        {
	        _log = log ?? GorgonLogDummy.DefaultInstance;

			if (texture == null)
	        {
		        throw new ArgumentNullException(nameof(texture));
	        }

	        if ((texture.Info.Binding & TextureBinding.RenderTarget) != TextureBinding.RenderTarget)
	        {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RESOURCE_IS_NOT_RENDERTARGET, texture.Name), nameof(texture));
	        }
			
	        Texture = texture;

	        if (format == DXGI.Format.Unknown)
	        {
		        format = texture.Info.Format;
	        }
			
			FormatInformation = new GorgonFormatInfo(Format);

	        if (FormatInformation.IsTypeless)
	        {
				throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
			}

	        Format = format;
			MipSlice = texture.Info.MipLevels <= 0 ? 0 : mipSlice.Max(0).Min(texture.Info.MipLevels - 1);
            FirstArrayOrDepthIndex = arrayOrDepthIndex.Max(0).Min(texture.Info.ArrayCount - 1);

	        if (arrayOrDepthCount == 0)
	        {
		        arrayOrDepthCount = texture.Info.TextureType == TextureType.Texture3D ? texture.Info.Depth : texture.Info.ArrayCount;
	        }
			
            ArrayOrDepthCount = arrayOrDepthCount.Min(arrayOrDepthCount - FirstArrayOrDepthIndex).Max(1);

			Initialize(!texture.Info.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling));

			// The key for a render target view is broken up into the following layout.
			// Bits: [33 - 26]   [25 - 15]     [14 - 4]      [3 - 0]
			//       Format      Array/Depth   Array/Depth   Mip slice
			//                   Count         Index
	        Key = (((uint)Format) & 0xff) << 26
	              | (((uint)ArrayOrDepthCount) & 0x7ff) << 15
	              | (((uint)FirstArrayOrDepthIndex) & 0x7ff) << 4
	              | (uint)MipSlice;
        }
        #endregion
    }
}