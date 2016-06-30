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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A render target bound to a 2D texture.
    /// </summary>
    /// <remarks>
    /// A 2D render target is a texture that can be used to receive rendering data in the pipeline by binding it as a render target.  Because it is inherited from <see cref="Gorgon.Graphics.GorgonTexture2D">GorgonTexture2D</see> 
    /// it can be cast to that type and used as a normal 2D texture.  Also, for convenience, it can also be cast to a <see cref="Gorgon.Graphics.GorgonRenderTargetView">GorgonRenderTargetView</see> or 
	/// a <see cref="Gorgon.Graphics.GorgonShaderView">GorgonTextureShaderView</see> to allow ease of <see cref="Gorgon.Graphics.GorgonOutputMerger.SetRenderTarget">binding a render target</see> to the output merger stage in the pipeline or 
    /// to the <see cref="Gorgon.Graphics.GorgonShaderState{T}.Resources">shader resource list</see>.
    /// </remarks>
    public sealed class GorgonRenderTarget2D
        : GorgonTexture2D
    {
		#region Variables.
        private GorgonRenderTargetView _defaultRenderTargetView;    // The default render target view for this render target.
        private bool _disposed;				                        // Flag to indicate that the object was disposed.
		// TODO: Dump this guy.
        private GorgonSwapChain _swapChain;                         // The swap chain that this target is attached to.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this target is attached to a swap chain or not.
		/// </summary>
		public bool IsSwapChain => _swapChain != null;

	    /// <summary>
	    /// Property to return the settings for this render target.
	    /// </summary>
	    public new GorgonRenderTarget2DSettings Settings
	    {
		    get;
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

			GorgonApplication.Log.Print("GorgonRenderTarget '{0}': Creating internal depth/stencil...", LoggingLevel.Verbose, Name);

			/*
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
			*/
		}

	    /// <summary>
	    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
	    /// </para>
	    /// </remarks>
	    public override void Dispose()
		{
			// Remove link to a swap chain.
			_swapChain = null;

			base.Dispose();
		}

		/// <summary>
		/// Function to clean up any internal resources.
		/// </summary>
		protected override void CleanUpResource()
		{
			GorgonApplication.Log.Print("Destroying GorgonRenderTarget '{0}'...", LoggingLevel.Intermediate, Name);
		    GorgonRenderStatistics.RenderTargetCount--;
		    GorgonRenderStatistics.RenderTargetSize -= SizeInBytes * (_swapChain == null ? 1 : 3);

		    if (DepthStencilBuffer != null)
		    {
                // If the swap chain is resized, we don't want to destroy these objects.
		        if (_swapChain == null)
		        {
			        GorgonApplication.Log.Print("GorgonRenderTarget '{0}': Releasing internal depth stencil...",
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
		protected override void OnInitialize(GorgonImage initialData)
		{
			if ((Settings.Format != BufferFormat.Unknown) && (Settings.TextureFormat == BufferFormat.Unknown))
			{
				Settings.TextureFormat = Settings.Format;
			}

			var desc = new D3D.Texture2DDescription
			{
				ArraySize = Settings.ArrayCount,
				Format = (GI.Format)Settings.TextureFormat,
				Width = Settings.Width,
				Height = Settings.Height,
				MipLevels = Settings.MipCount,
				BindFlags = GetBindFlags(false, true),
				Usage = D3D.ResourceUsage.Default,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SampleDescription = Settings.Multisampling.ToSampleDesc()
			};

			GorgonApplication.Log.Print("{0} {1}: Creating 2D render target texture...", LoggingLevel.Verbose, GetType().Name, Name);

			// Create the texture.
			D3DResource = initialData != null
#warning We need to ensure we can get this information to the initialization for textures in some fashion.
							  ? new D3D.Texture2D(VideoDevice.D3DDevice(), desc, new DataBox[0])
							  : new D3D.Texture2D(VideoDevice.D3DDevice(), desc);

			// Create the default render target view.
			_defaultRenderTargetView = GetRenderTargetView(Settings.Format, 0, 0, 1);

			GorgonRenderStatistics.RenderTargetCount++;
			GorgonRenderStatistics.RenderTargetSize += SizeInBytes;

			CreateDepthStencilBuffer();

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0.0f, 1.0f);
		}

		/// <summary>
		/// Function called when the attached swap chain is resized.
		/// </summary>
		/// <returns><b>true</b> if the swap chain was bound to slot 0, and needs to be rebound.</returns>
		internal bool OnSwapChainResize()
		{
			ReleaseResourceViews();

			// Destroy the resource data.
			D3DResource.Dispose();
			D3DResource = null;

			return false;
		}

		/// <summary>
		/// Function to initialize the texture from a swap chain.
		/// </summary>
		/// <param name="swapChain">The swap chain used to initialize the texture.</param>
		[Obsolete("Get rid of this when GorgonSwapChain is replaced.")]
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
			Settings.ShaderViewFormat = BufferFormat.Unknown;
			Settings.AllowUnorderedAccessViews = (desc.BindFlags & D3D.BindFlags.UnorderedAccess) == D3D.BindFlags.UnorderedAccess;
			Settings.Multisampling = new GorgonMultiSampleInfo(desc.SampleDescription);
			Settings.IsTextureCube = (desc.OptionFlags & D3D.ResourceOptionFlags.TextureCube) ==
										  D3D.ResourceOptionFlags.TextureCube;
			Settings.DepthStencilFormat = BufferFormat.Unknown;
			Settings.TextureFormat = (BufferFormat)swapChain.Info.Format;

			_swapChain = swapChain;

            GorgonRenderStatistics.TextureCount++;
			GorgonRenderStatistics.TextureSize += SizeInBytes;
			GorgonRenderStatistics.RenderTargetCount++;
			GorgonRenderStatistics.RenderTargetSize += SizeInBytes * 3;

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0.0f, 1.0f);

			// Re-initialize any released resource views.
			InitializeResourceViews();

            CreateDepthStencilBuffer();

			if (DepthStencilBuffer != null)
			{
				DepthStencilBuffer.SwapChain = swapChain;
			}

			// Create the default render target view.
			_defaultRenderTargetView = GetRenderTargetView(Settings.Format, 0, 0, 1);
		}

		/// <summary>
		/// Function to retrieve a render target view.
		/// </summary>
		/// <param name="format">Format of the new render target view.</param>
		/// <param name="mipSlice">Mip level index to use in the view.</param>
		/// <param name="arrayIndex">Array index to use in the view.</param>
		/// <param name="arrayCount">Number of array indices to use.</param>
		/// <returns>A render target view.</returns>
		/// <remarks>Use this to create/retrieve a render target view that can bind a portion of the target to the pipeline as a render target.
		/// <para>The <paramref name="format"/> for the render target view does not have to be the same as the render target backing texture, and if the format is set to Unknown, then it will 
		/// use the format from the texture.</para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
		public GorgonRenderTargetView GetRenderTargetView(BufferFormat format, int mipSlice, int arrayIndex,
															   int arrayCount)
		{
			return OnGetRenderTargetView(format, mipSlice, arrayIndex, arrayCount);
		}

        /// <summary>
        /// Function to clear the render target and an attached depth/stencil buffer.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
        /// <remarks>
        /// This will only clear the default view for the render target.
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
		[Obsolete("Get rid of this")]
        public void Clear(GorgonColor color, GorgonGraphics deferred = null)
        {
#warning FIX THIS: Not used anymore.
		}

		/// <summary>
		/// Function to clear the render target and an attached stencil buffer.
		/// </summary>
		/// <param name="color">Color used to clear the render target.</param>
		/// <param name="stencilValue">Value used to clear the stencil buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
		/// <remarks>
		/// This will only clear the default view for the render target.
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// </remarks>
		[Obsolete("Get rid of this")]
		public void Clear(GorgonColor color, byte stencilValue, GorgonGraphics deferred = null)
        {
#warning FIX THIS: Not used anymore.
        }

		/// <summary>
		/// Function to clear the render target and an attached depth buffer.
		/// </summary>
		/// <param name="color">Color used to clear the render target.</param>
		/// <param name="depthValue">Value used to clear the depth buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
		/// <remarks>
		/// This will only clear the default view for the render target.
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// </remarks>
		[Obsolete("Get rid of this")]
		public void Clear(GorgonColor color, float depthValue, GorgonGraphics deferred = null)
        {
#warning FIX THIS: Not used anymore.
		}


		/// <summary>
		/// Function to clear the render target and an attached depth/stencil buffer.
		/// </summary>
		/// <param name="color">Color used to clear the render target.</param>
		/// <param name="depthValue">Value used to clear the depth buffer.</param>
		/// <param name="stencilValue">Value used to clear the stencil buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
		/// <remarks>
		/// This will only clear the default view for the render target.
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// </remarks>
		[Obsolete("Get rid of this")]
		public void Clear(GorgonColor color, float depthValue, byte stencilValue, GorgonGraphics deferred = null)
		{
#warning FIX THIS: Not used anymore.
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

        /// <summary>
        /// Implicit operator to return the swap chain for this render target.
        /// </summary>
        /// <param name="target">Render target to convert.</param>
        /// <returns>The swap chain for the render target.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the render target is not a swap chain.</exception>
        public static explicit operator GorgonSwapChain(GorgonRenderTarget2D target)
        {
            if (target._swapChain != null)
            {
                return target._swapChain;
            }

            throw new InvalidCastException(Resources.GORGFX_TARGET_IS_NOT_SWAPCHAIN);
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
            Settings = (GorgonRenderTarget2DSettings)settings.Clone();
        }
        #endregion
    }
}
