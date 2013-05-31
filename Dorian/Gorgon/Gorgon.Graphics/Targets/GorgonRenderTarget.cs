#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, March 07, 2012 9:11:50 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Render target resized event arguments.
	/// </summary>
	public class GorgonRenderTargetResizedEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the new width of the target.
		/// </summary>
		public int Width
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the new height of the target.
		/// </summary>
		public int Height
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetResizedEventArgs" /> class.
		/// </summary>
		/// <param name="target">Render target that was resized.</param>
		public GorgonRenderTargetResizedEventArgs(GorgonRenderTarget target)
		{
			Width = target.Settings.Width;
			Height = target.Settings.Height;
		}
		#endregion
	}

	/// <summary>
	/// A texture render target.
	/// </summary>
	/// <remarks>Use this to render graphics data to a texture.</remarks>
	public class GorgonRenderTarget
		: GorgonNamedObject, IDisposable
	{
		#region Events.
		/// <summary>
		/// Event called after the render target has been resized.
		/// </summary>
		public event EventHandler<GorgonRenderTargetResizedEventArgs> Resized;
		#endregion

		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
		private GorgonDepthStencil _depthStencil;					// Depth/stencil buffer attached to this render target.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D render target interface.
		/// </summary>
		internal D3D.RenderTargetView D3DRenderTarget
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the internal depth/stencil for the render target.
		/// </summary>
		protected GorgonDepthStencil InternalDepthStencil
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the settings for this render target.
		/// </summary>
		public GorgonRenderTargetSettings Settings
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to set or return the depth/stencil for this render target.
		/// </summary>
		/// <remarks>
		/// Setting this value to NULL will reset this value to the internal depth/stencil buffer if one was created when the render target was created.  Use <see cref="M:GorgonLibrary.GorgonGraphics.GorgonRenderTarget.UpdateSettings">UpdateSettings</see> to 
		/// change the internal depth/stencil buffer.
		/// <para>Care should be taken with the lifetime of the depth/stencil that is attached to this render target.  If a user creates the render target with a depth buffer, its 
		/// lifetime will be managed by the render target (i.e. it will be disposed when the render target is disposed).  If a user sets this value to an external depth buffer, then the render target will -NOT- manage the lifetime of the external depth/stencil.</para>
		/// </remarks>
		public GorgonDepthStencil DepthStencil
		{
			get
			{
				return _depthStencil ?? InternalDepthStencil;
			}
			set
			{
				_depthStencil = value;
			}
		}

		/// <summary>
		/// Property to return the texture for the render target.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the graphics interface that created this render target.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default viewport associated with this render target.
		/// </summary>
		public GorgonViewport Viewport
		{
			get;
			protected set;
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when the target is resized.
		/// </summary>
		/// <param name="args">Resize event arguments.</param>
		protected virtual void OnTargetResize(GorgonRenderTargetResizedEventArgs args)
		{
			if (Resized != null)
			{
				Resized(this, args);
			}
		}

		/// <summary>
		/// Function to clean up any internal resources.
		/// </summary>
		protected virtual void CleanUp()
		{
			if (Texture != null)
			{
				GorgonRenderStatistics.RenderTargetCount--;
				GorgonRenderStatistics.RenderTargetSize -= Texture.SizeInBytes;

				Texture.Dispose();
			}

			if (InternalDepthStencil != null)
			{
				Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing internal depth stencil...", LoggingLevel.Verbose, Name);
				InternalDepthStencil.Dispose();				
			}

			Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing D3D11 render target view...", Diagnostics.LoggingLevel.Intermediate, Name);
			if (D3DRenderTarget != null)
				D3DRenderTarget.Dispose();

			InternalDepthStencil = null;
			D3DRenderTarget = null;
			Texture = null;
		}

		/// <summary>
		/// Function to create the resources for the render target.
		/// </summary>
		protected virtual void CreateResources()
		{
			if (D3DRenderTarget != null)
				CleanUp();

			// Create the internal depth/stencil.
			if (Settings.DepthStencilFormat != BufferFormat.Unknown)
			{
				Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating internal depth/stencil...", Diagnostics.LoggingLevel.Verbose, Name);

				var settings = new GorgonDepthStencilSettings
				{
					Format = Settings.DepthStencilFormat,
					Width = Settings.Width,
					Height = Settings.Height,
					MultiSample = Settings.MultiSample
				};

				GorgonDepthStencil.ValidateSettings(Graphics, settings);

				InternalDepthStencil = new GorgonDepthStencil(Graphics, Name + "_Internal_DepthStencil_" + Guid.NewGuid().ToString(), settings);
				InternalDepthStencil.UpdateSettings();
			}

			// Create the render target texture.
			Texture = new GorgonTexture2D(Graphics, Name + "_Internal_Texture_" + Guid.NewGuid().ToString(), new GorgonTexture2DSettings()
			{
				ArrayCount = 1,
				Format = Settings.Format,
				Height = Settings.Height,
				Width =  Settings.Width,
				MipCount = 1,
				Multisampling = Settings.MultiSample,
				Usage = BufferUsage.Default
			});
			Texture.RenderTarget = this;
			Texture.InitializeRenderTarget();

			Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating D3D11 render target view...", Diagnostics.LoggingLevel.Intermediate, Name);
			UpdateResourceView();

			GorgonRenderStatistics.RenderTargetCount++;
			GorgonRenderStatistics.RenderTargetSize += Texture.SizeInBytes;

			// Set default viewport.
			Viewport = new GorgonViewport(0, 0, Settings.Width, Settings.Height, 0.0f, 1.0f);
		}

		/// <summary>
		/// Function to initialize the target.
		/// </summary>
		protected internal virtual void Initialize()
		{
			CreateResources();
		}

		/// <summary>
		/// Function called by a texture to update the resource view.
		/// </summary>
		internal void UpdateResourceView()
		{
			if (D3DRenderTarget != null)
			{
				D3DRenderTarget.Dispose();
				D3DRenderTarget = null;
			}

			// Modify the render target.
			D3DRenderTarget = new D3D.RenderTargetView(Graphics.D3DDevice, Texture.D3DResource);
			D3DRenderTarget.DebugName = "RenderTarget '" + Name + "' Render Target View";

			Graphics.Output.RenderTargets.ReSeat(this);
		}

		/// <summary>
		/// Function to validate the settings for a render target.
		/// </summary>
		internal static void ValidateRenderTargetSettings(GorgonGraphics graphics, GorgonRenderTargetSettings settings)
		{
			D3D.Device d3dDevice = null;

			if (graphics.VideoDevice == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the render target, no video device was selected.");

			// Get the Direct 3D device instance.
			d3dDevice = graphics.D3DDevice;

			// Ensure width and height are valid.
			if ((settings.Width <= 0) || (settings.Width >= graphics.Textures.MaxWidth))
				throw new GorgonException(GorgonResult.CannotCreate, "Render target must have a width greater than 0 or less than " + graphics.Textures.MaxWidth.ToString() + ".");

			if ((settings.Height <= 0) || (settings.Height >= graphics.Textures.MaxHeight))
				throw new GorgonException(GorgonResult.CannotCreate, "Render target must have a height greater than 0 or less than " + graphics.Textures.MaxHeight.ToString() + ".");

			if (settings.Format == BufferFormat.Unknown)
				throw new GorgonException(GorgonResult.CannotCreate, "Render target must have a known buffer format.");

			int quality = graphics.VideoDevice.GetMultiSampleQuality(settings.Format, settings.MultiSample.Count);

			// Ensure that the quality of the sampling does not exceed what the card can do.
			if ((settings.MultiSample.Quality >= quality) || (settings.MultiSample.Quality < 0))
				throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support multisampling with a count of '" + settings.MultiSample.Count.ToString() + "' and a quality of '" + settings.MultiSample.Quality.ToString() + " with a format of '" + settings.Format + "'");

			// Ensure that the selected video format can be used.
			if (!graphics.VideoDevice.SupportsRenderTargetFormat(settings.Format, (settings.MultiSample.Quality > 0) || (settings.MultiSample.Count > 1)))
				throw new ArgumentException("Cannot use the format '" + settings.Format.ToString() + "' for a render target on the video device '" + graphics.VideoDevice.Name + "'.");
		}

		/// <summary>
		/// Function to clear the swap chain and any depth buffer attached to it.
		/// </summary>
		/// <param name="color">Color used to clear the swap chain.</param>
		/// <remarks>This will only clear the swap chain.  Any attached depth/stencil buffer will remain untouched.</remarks>
		public void Clear(GorgonColor color)
		{
			Graphics.Context.ClearRenderTargetView(D3DRenderTarget, color.SharpDXColor4);
		}

		/// <summary>
		/// Function to clear the swap chain and an associated depth buffer.
		/// </summary>
		/// <param name="color">Color used to clear the swap chain.</param>
		/// <param name="depthValue">Value used to fill the depth buffer.</param>
		/// <remarks>This will clear the swap chain and depth buffer, but depth buffers with a stencil component will remain untouched.</remarks>
		public void Clear(GorgonColor color, float depthValue)
		{
			Clear(color);

			if ((DepthStencil != null) && (DepthStencil.FormatInformation.HasDepth))
				DepthStencil.ClearDepth(depthValue);
		}

		/// <summary>
		/// Function to clear the swap chain and an associated depth buffer with a stencil component.
		/// </summary>
		/// <param name="color">Color used to clear the swap chain.</param>
		/// <param name="depthValue">Value used to fill the depth buffer.</param>
		/// <param name="stencilValue">Value used to fill the stencil component of the depth buffer.</param>
		/// <remarks>This will clear the swap chain, depth buffer and stencil component of the depth buffer.</remarks>
		public void Clear(GorgonColor color, float depthValue, int stencilValue)
		{
			if ((DepthStencil != null) && (DepthStencil.FormatInformation.HasDepth) && (!DepthStencil.FormatInformation.HasStencil))
			{
				Clear(color, depthValue);
				return;
			}

			Clear(color);

			if ((DepthStencil != null) && (DepthStencil.FormatInformation.HasDepth) && (DepthStencil.FormatInformation.HasStencil))
			{
				DepthStencil.Clear(depthValue, stencilValue);
				return;
			}
		}

		/// <summary>
		/// Function to update the settings for the render target.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="depthStencilFormat">The format of the internal depth/stencil buffer.</param>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonRenderTargetSettings.VideoMode.Format</see> property cannot be used by the render target.
		/// <para>-or-</para>
		/// <para>The width and height are not valid for the render target.</para>
		/// </exception>
		public virtual void UpdateSettings(GorgonVideoMode mode, BufferFormat depthStencilFormat)
		{
			bool sizeChanged = (mode.Size != Settings.VideoMode.Size);

			// Assign the new settings.	
			Settings.VideoMode = mode;
			Settings.DepthStencilFormat = depthStencilFormat;

			// Validate and modify the settings as appropriate.
			ValidateRenderTargetSettings(Graphics, Settings);

			// Recreate the render target.
			CleanUp();
			CreateResources();

			// Re-seat our target.
			Graphics.Output.RenderTargets.ReSeat(this);

			if (sizeChanged)
				OnTargetResize(new GorgonRenderTargetResizedEventArgs(this));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the render target.</param>
		/// <param name="settings">Settings to apply to the render target.</param>
		internal GorgonRenderTarget(GorgonGraphics graphics, string name, GorgonRenderTargetSettings settings)
			: base(name)
		{
			Graphics = graphics;
			Settings = settings;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					int targetIndex = Graphics.Output.RenderTargets.IndexOf(this);

					if (targetIndex > -1)
						Graphics.Output.RenderTargets[targetIndex] = null;

					CleanUp();
				}

				Graphics = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
