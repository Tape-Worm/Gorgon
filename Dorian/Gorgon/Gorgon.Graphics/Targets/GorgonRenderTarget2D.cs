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
    public class GorgonRenderTarget2D
        : GorgonRenderTarget
    {
        #region Variables.
	    private bool _disposed;									// Flag to indicate that the target was disposed.
	    private GorgonTexture2D _texture;						// The texture bound to this render target.
	    private GorgonSwapChain _swapChain;						// The swap chain bound to this render target.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the texture bound to the target (if applicable).
		/// </summary>
		protected override GorgonTexture TargetTexture
		{
			get
			{
				return _texture;
			}
		}

        /// <summary>
        /// Property to return the settings for this render target.
        /// </summary>
        public new GorgonRenderTarget2DSettings Settings
        {
            get
            {
                return (GorgonRenderTarget2DSettings)base.Settings;
            }
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

			var settings = new GorgonDepthStencilSettings
				{
					Format = Settings.DepthStencilFormat,
					Width = Settings.Width,
					Height = Settings.Height,
					MultiSample = Settings.MultiSample
				};

			GorgonDepthStencil.ValidateSettings(Graphics, settings);

			InternalDepthStencil = new GorgonDepthStencil(Graphics, Name + "_Internal_DepthStencil_" + Guid.NewGuid(), settings);
			InternalDepthStencil.UpdateSettings();
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_texture != null)
					{
						_texture.Dispose();
						_texture = null;
					}
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to clean up any internal resources.
		/// </summary>
		protected override void OnCleanUp()
		{
			if (D3DRenderTarget != null)
			{
				Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing D3D11 render target view...", LoggingLevel.Intermediate, Name);
				D3DRenderTarget.Dispose();
			}

			if (_texture != null)
			{
				GorgonRenderStatistics.RenderTargetCount--;
				GorgonRenderStatistics.RenderTargetSize -= TargetTexture.SizeInBytes * (_swapChain == null ? 1 : _swapChain.Settings.BufferCount);

				Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing D3D11 render target 2D texture...", LoggingLevel.Intermediate, Name);
				_texture.Dispose();
			}

			if (InternalDepthStencil != null)
			{
				Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing internal depth stencil...", LoggingLevel.Verbose, Name);
				InternalDepthStencil.Dispose();				
			}

			InternalDepthStencil = null;
			D3DRenderTarget = null;
			_texture = null;
		}

		/// <summary>
		/// Function to create the resources for the render target.
		/// </summary>
		protected override void OnInitialize()
		{
			// Create the internal depth/stencil.
			CreateDepthStencilBuffer();

			// Create the render target texture.
			Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating D3D11 render target 2D texture...", LoggingLevel.Intermediate, Name);
			_texture = new GorgonTexture2D(Graphics, Name + "_Internal_Texture_" + Guid.NewGuid(), new GorgonTexture2DSettings
				{
				ArrayCount = 1,
				Format = Settings.Format,
				Height = Settings.Height,
				Width =  Settings.Width,
				MipCount = 1,
				Multisampling = Settings.MultiSample,
				Usage = BufferUsage.Default
			});
			_texture.InitializeRenderTarget(this);

			Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating D3D11 render target view...", LoggingLevel.Intermediate, Name);

			// Modify the render target.
			D3DRenderTarget = new D3D.RenderTargetView(Graphics.D3DDevice, TargetTexture.D3DResource, new D3D.RenderTargetViewDescription
										{
											Texture2D =
												{
													MipSlice = 0
												},
												Dimension = D3D.RenderTargetViewDimension.Texture2D,
												Format = (GI.Format)_texture.Settings.Format
										})
				{
					DebugName = "GorgonRenderTarget2D '" + Name + "' Render Target View"
				};

			Graphics.Output.RenderTargets.ReSeat(this);

			GorgonRenderStatistics.RenderTargetCount++;
			GorgonRenderStatistics.RenderTargetSize += _texture.SizeInBytes;

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0.0f, 1.0f);
		}

	    /// <summary>
		/// Function to initialize this render target from a swap chain.
		/// </summary>
		/// <param name="swapChain">The swap chain used to initialize the render target.</param>
		internal void InitializeSwapChain(GorgonSwapChain swapChain)
		{
			// TODO: Allow the texture to be updated instead of destroyed when a swap chain is resized.
			_swapChain = swapChain;

			// TODO: Allow the depth/stencil to be updated instead of destroyed when a swap chain is resized.
			CreateDepthStencilBuffer();

			Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating D3D11 render target view...", LoggingLevel.Intermediate, Name);
			// Retrieve the texture for the swap chain.
			_texture = new GorgonTexture2D(Graphics, Name + "_Internal_Texture_" + Guid.NewGuid(), new GorgonTexture2DSettings());
			_texture.InitializeSwapChain(swapChain);

			// Modify the render target.
			D3DRenderTarget = new D3D.RenderTargetView(Graphics.D3DDevice, _texture.D3DResource)
			{
				DebugName = "GorgonRenderTarget2D '" + Name + "' Render Target View"
			};

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0, 1.0f);
		}

		/// <summary>
		/// Function to convert a render target to a texture.
		/// </summary>
		/// <param name="target">Target to convert to a texture.</param>
		/// <returns>The texture bound to the render target.</returns>
		public static GorgonTexture2D ToTexture(GorgonRenderTarget2D target)
		{
			return target == null ? null : target._texture;
		}

		/// <summary>
		/// Implicit operator to convert a render target to a texture.
		/// </summary>
		/// <param name="target">Render target to convert.</param>
		/// <returns>The texture attached to the render target.</returns>
		public static implicit operator GorgonTexture2D(GorgonRenderTarget2D target)
		{
			return target == null ? null : target._texture;
		}

		/// <summary>
		/// Explicit operator to convert a render target to a swap chain.
		/// </summary>
		/// <param name="target">The target to convert to a swap chain.</param>
		/// <returns>The swap chain bound to this render target.</returns>
		public static explicit operator GorgonSwapChain(GorgonRenderTarget2D target)
		{
			return target == null ? null : target._swapChain;
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
        }
        #endregion
    }
}
