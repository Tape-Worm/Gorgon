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
// Created: Friday, May 31, 2013 9:34:22 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A render target bound to a 2D texture.
    /// </summary>
    /// <remarks>
    /// A 2D render target is a texture that can be used to receive rendering data in the pipeline by binding it as a render target.  Because it is inherited from <see cref="GorgonLibrary.Graphics.GorgonTexture2D">GorgonTexture2D</see> 
    /// it can be cast to that type and used as a normal 2D texture.  Also, for convenience, it can also be cast to a <see cref="GorgonLibrary.Graphics.GorgonRenderTargetView">GorgonRenderTargetView</see> or 
	/// a <see cref="GorgonLibrary.Graphics.GorgonShaderView">GorgonTextureShaderView</see> to allow ease of <see cref="GorgonLibrary.Graphics.GorgonOutputMerger.SetRenderTarget">binding a render target</see> to the output merger stage in the pipeline or 
    /// to the <see cref="GorgonLibrary.Graphics.GorgonShaderState{T}.Resources">shader resource list</see>.
    /// </remarks>
    public sealed class GorgonRenderTarget2D
        : GorgonTexture2D
    {
		#region Variables.
        private GorgonRenderTargetView _defaultRenderTargetView;    // The default render target view for this render target.
        private bool _disposed;				                        // Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the swap chain that this texture is attached to.
		/// </summary>
		public GorgonSwapChain SwapChain
		{
			get;
			private set;
		}

	    /// <summary>
	    /// Property to return the settings for this render target.
	    /// </summary>
	    public new GorgonRenderTarget2DSettings Settings
	    {
		    get;
		    private set;
	    }

	    /// <summary>
		/// Property to return the default viewport for this render target.
		/// </summary>
		public GorgonViewport Viewport
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default depth/stencil buffer attached to this render target.
		/// </summary>
	    public GorgonDepthStencil2D DepthStencilBuffer
	    {
		    get;
		    private set;
	    }
	    #endregion

        #region Methods.
		/// <summary>
		/// Function to create the depth/stencil buffer for the target.
		/// </summary>
		private void CreateDepthStencilBuffer()
		{
			// Create the internal depth/stencil.
			if (Settings.DepthStencilFormat == BufferFormat.Unknown)
			{
				return;
			}

			Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating internal depth/stencil...", LoggingLevel.Verbose, Name);

		    if (DepthStencilBuffer == null)
		    {
				DepthStencilBuffer = new GorgonDepthStencil2D(Graphics,
		                                                    Name + "_Internal_DepthStencil_" + Guid.NewGuid(),
		                                                    new GorgonDepthStencil2DSettings
		                                                        {
		                                                            Format = Settings.DepthStencilFormat,
                                                                    Width = Settings.Width,
                                                                    Height = Settings.Height,
                                                                    Multisampling = Settings.Multisampling,
																	ArrayCount = Settings.ArrayCount,
																	MipCount = Settings.MipCount
		                                                        });
		    }
		    else
		    {
                DepthStencilBuffer.Settings.Format = Settings.DepthStencilFormat;
                DepthStencilBuffer.Settings.Width = Settings.Width;
                DepthStencilBuffer.Settings.Height = Settings.Height;
                DepthStencilBuffer.Settings.Multisampling = Settings.Multisampling;
		    }

#if DEBUG
			Graphics.Output.ValidateDepthStencilSettings(DepthStencilBuffer.Settings);
#endif
            
            DepthStencilBuffer.Initialize(null);
		    DepthStencilBuffer.RenderTarget = this;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// Remove us from the pipeline.
				Graphics.Output.Unbind(this, DepthStencilBuffer);

				// Remove link to a swap chain.
				SwapChain = null;
			}

			base.Dispose(disposing);
			_disposed = true;
		}

		/// <summary>
		/// Function to clean up any internal resources.
		/// </summary>
		protected override void CleanUpResource()
		{
			Gorgon.Log.Print("Destroying GorgonRenderTarget '{0}'...", LoggingLevel.Intermediate, Name);
		    GorgonRenderStatistics.RenderTargetCount--;
		    GorgonRenderStatistics.RenderTargetSize -= SizeInBytes * (SwapChain == null ? 1 : SwapChain.Settings.BufferCount);

		    if (DepthStencilBuffer != null)
		    {
                // If the swap chain is resized, we don't want to destroy these objects.
		        if (SwapChain == null)
		        {
			        Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing internal depth stencil...",
			                         LoggingLevel.Verbose,
			                         Name);
			        DepthStencilBuffer.Dispose();
			        DepthStencilBuffer = null;
		        }
		    }

			base.CleanUpResource();
		}
		
		/// <summary>
		/// Function to initialize the texture with optional initial data.
		/// </summary>
		/// <param name="initialData">Data used to populate the image.</param>
		protected override void OnInitialize(GorgonImageData initialData)
		{
			if ((Settings.Format != BufferFormat.Unknown) && (Settings.TextureFormat == BufferFormat.Unknown))
			{
				Settings.TextureFormat = Settings.Format;
			}

			var desc = new D3D.Texture2DDescription
			{
				ArraySize = Settings.ArrayCount,
				Format = (SharpDX.DXGI.Format)Settings.TextureFormat,
				Width = Settings.Width,
				Height = Settings.Height,
				MipLevels = Settings.MipCount,
				BindFlags = GetBindFlags(false, true),
				Usage = D3D.ResourceUsage.Default,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling)
			};

			Gorgon.Log.Print("{0} {1}: Creating 2D render target texture...", LoggingLevel.Verbose, GetType().Name, Name);

			// Create the texture.
			D3DResource = initialData != null
							  ? new D3D.Texture2D(Graphics.D3DDevice, desc, initialData.GetDataBoxes())
							  : new D3D.Texture2D(Graphics.D3DDevice, desc);

			// Create the default render target view.
			_defaultRenderTargetView = CreateRenderTargetView(Settings.Format, 0, 0, 1);

			GorgonRenderStatistics.RenderTargetCount++;
			GorgonRenderStatistics.RenderTargetSize += SizeInBytes;

			CreateDepthStencilBuffer();

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0.0f, 1.0f);
		}

		/// <summary>
		/// Function called when the attached swap chain is resized.
		/// </summary>
		/// <returns>TRUE if the swap chain was bound to slot 0, and needs to be rebound.</returns>
		internal bool OnSwapChainResize()
		{
			var currentTarget = Graphics.Output.GetRenderTarget(0);
			bool result = currentTarget != null && currentTarget.Resource == this;

			CleanUpResource();

			return result;
		}

		/// <summary>
		/// Function to initialize the texture from a swap chain.
		/// </summary>
		/// <param name="swapChain">The swap chain used to initialize the texture.</param>
		internal void InitializeSwapChain(GorgonSwapChain swapChain)
		{
			if (D3DResource != null)
			{
				CleanUpResource();
			}

			D3DResource = D3D.Resource.FromSwapChain<D3D.Texture2D>(swapChain.GISwapChain, 0);
			D3D.Texture2DDescription desc = ((D3D.Texture2D)D3DResource).Description;

			Settings.Width = desc.Width;
			Settings.Height = desc.Height;
			Settings.ArrayCount = desc.ArraySize;
			Settings.Format = (BufferFormat)desc.Format;
			Settings.MipCount = desc.MipLevels;
			Settings.ShaderViewFormat = (swapChain.Settings.Flags & SwapChainUsageFlags.ShaderInput) ==
			                            SwapChainUsageFlags.ShaderInput
				                            ? swapChain.Settings.Format
				                            : BufferFormat.Unknown;
			Settings.AllowUnorderedAccessViews = (desc.BindFlags & D3D.BindFlags.UnorderedAccess) == D3D.BindFlags.UnorderedAccess;
			Settings.Multisampling = new GorgonMultisampling(desc.SampleDescription.Count, desc.SampleDescription.Quality);
			Settings.IsTextureCube = (desc.OptionFlags & D3D.ResourceOptionFlags.TextureCube) ==
										  D3D.ResourceOptionFlags.TextureCube;
			Settings.DepthStencilFormat = swapChain.Settings.DepthStencilFormat;
			Settings.TextureFormat = swapChain.Settings.Format;

			SwapChain = swapChain;

#if DEBUG
			Graphics.Output.ValidateRenderTargetSettings(Settings);
#endif

            GorgonRenderStatistics.TextureCount++;
			GorgonRenderStatistics.TextureSize += SizeInBytes;
			GorgonRenderStatistics.RenderTargetCount++;
			GorgonRenderStatistics.RenderTargetSize += SizeInBytes * swapChain.Settings.BufferCount;

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0.0f, 1.0f);

            CreateDepthStencilBuffer();

			if (DepthStencilBuffer != null)
			{
				DepthStencilBuffer.SwapChain = swapChain;
			}

			// Create the default render target view.
			_defaultRenderTargetView = CreateRenderTargetView(Settings.Format, 0, 0, 1);

			if ((swapChain.Settings.Flags & SwapChainUsageFlags.ShaderInput) == SwapChainUsageFlags.ShaderInput)
			{
				CreateDefaultResourceView();
			}
		}

		/// <summary>
		/// Function to create a new render target view.
		/// </summary>
		/// <param name="format">Format of the new render target view.</param>
		/// <param name="mipSlice">Mip level index to use in the view.</param>
		/// <param name="arrayIndex">Array index to use in the view.</param>
		/// <param name="arrayCount">Number of array indices to use.</param>
		/// <returns>A render target view.</returns>
        /// <remarks>Use this to create a render target view that can bind a portion of the target to the pipeline as a render target.
        /// <para>The <paramref name="format"/> for the render target view does not have to be the same as the render target backing texture, and if the format is set to Unknown, then it will 
        /// use the format from the texture.</para>
        /// </remarks>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the render target view could not be created.</exception>
        public GorgonRenderTargetTextureView CreateRenderTargetView(BufferFormat format, int mipSlice, int arrayIndex,
															   int arrayCount)
		{
			return OnCreateRenderTargetView(format, mipSlice, arrayIndex, arrayCount);
		}

		/// <summary>
		/// Function to clear the render target.
		/// </summary>
		/// <param name="color">Color used to clear the render target.</param>
		/// <remarks>This will only clear the render target.  Only the default view will be cleared, any extra views will not be cleared. Any attached depth/stencil buffer will remain untouched.</remarks>
		public void Clear(GorgonColor color)
		{
            _defaultRenderTargetView.Clear(color);
		}

		/// <summary>
		/// Function to clear the render target and an attached depth buffer.
		/// </summary>
		/// <param name="color">Color used to clear the render target.</param>
		/// <param name="depthValue">Value used to clear the depth buffer.</param>
		/// <remarks>This will only clear the render target.  Only the default view will be cleared, any extra views will not be cleared. Any stencil buffer will remain untouched.</remarks>
		public void Clear(GorgonColor color, float depthValue)
		{
			Clear(color);

			if ((DepthStencilBuffer == null) || (!DepthStencilBuffer.FormatInformation.HasDepth))
			{
				return;
			}

			DepthStencilBuffer.ClearDepth(depthValue);
		}

		/// <summary>
		/// Function to clear the render target and an attached depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color used to clear the render target.</param>
		/// <param name="depthValue">Value used to clear the depth buffer.</param>
		/// <param name="stencilValue">Value used to clear the stencil buffer.</param>
		/// <remarks>This will only clear the render target.  Only the default view will be cleared, any extra views will not be cleared.</remarks>
		public void Clear(GorgonColor color, float depthValue, int stencilValue)
		{
			if ((DepthStencilBuffer != null) && (DepthStencilBuffer.FormatInformation.HasDepth) && (!DepthStencilBuffer.FormatInformation.HasStencil))
			{
				Clear(color, depthValue);
				return;
			}

			Clear(color);

			if ((DepthStencilBuffer == null) || (!DepthStencilBuffer.FormatInformation.HasDepth) ||
			    (!DepthStencilBuffer.FormatInformation.HasStencil))
			{
				return;
			}

			DepthStencilBuffer.ClearDepth(depthValue);
			DepthStencilBuffer.ClearStencil(stencilValue);
		}

		/// <summary>
		/// Function to retrieve the render target view for a render target.
		/// </summary>
		/// <param name="target">Render target to evaluate.</param>
		/// <returns>The render target view for the swap chain.</returns>
		public static GorgonRenderTargetView ToRenderTargetView(GorgonRenderTarget2D target)
		{
			return target == null ? null : target._defaultRenderTargetView;
		}

		/// <summary>
		/// Implicit operator to return the render target view for a render target
		/// </summary>
		/// <param name="target">Render target to evaluate.</param>
		/// <returns>The render target view for the swap chain.</returns>
		public static implicit operator GorgonRenderTargetView(GorgonRenderTarget2D target)
		{
			return target == null ? null : target._defaultRenderTargetView;
		}
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTarget2D"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that created this object.</param>
        /// <param name="name">The name of the render target.</param>
        /// <param name="settings">Settings to apply to the render target.</param>
        internal GorgonRenderTarget2D(GorgonGraphics graphics, string name, GorgonRenderTarget2DSettings settings)
            : base(graphics, name, settings)
        {
	        Settings = settings;
        }
        #endregion
    }
}
