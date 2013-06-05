#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, November 22, 2011 11:33:59 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A depth/stencil buffer.
	/// </summary>
	/// <remarks>This is for setting a depth and/or stencil buffer along with a render target.  When pairing with a render target, the user must ensure that the depth/stencil buffer matches the dimensions and 
	/// the multisampling settings for the render target.</remarks>
	public class GorgonDepthStencil
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _isDisposed;			// Flag to indicate that the object was disposed of.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D depth/stencil view for the depth/stencil buffer.
		/// </summary>
		internal D3D.DepthStencilView D3DDepthStencilView
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the render target that has this depth/stencil buffer as its default.
        /// </summary>
        public GorgonRenderTarget2D RenderTarget
        {
            get;
            internal set;
        }

		/// <summary>
		/// Property to return the texture for the depth buffer.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about the depth/stencil buffer format.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData FormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this depth/stencil.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for a depth/stencil buffer.
		/// </summary>
		public GorgonDepthStencilSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up any resources for the depth/stencil buffer.
		/// </summary>
		private void CleanUp()
		{
    		if (D3DDepthStencilView != null)
			{
				Gorgon.Log.Print("GorgonDepthStencil '{0}': Destroying D3D11 depth stencil view...", Diagnostics.LoggingLevel.Verbose, Name);
				D3DDepthStencilView.Dispose();
                D3DDepthStencilView = null;
			}

		    if (Texture == null)
		    {
		        return;
		    }

		    GorgonRenderStatistics.DepthBufferCount--;
		    GorgonRenderStatistics.DepthBufferSize -= Texture.SizeInBytes;
		    if (RenderTarget == null)
		    {
		        Gorgon.Log.Print("GorgonDepthStencil '{0}': Destroying depth stencil texture...",
		                         Diagnostics.LoggingLevel.Verbose,
		                         Name);
		        Texture.Dispose();
		        Texture = null;
		    }
		    else
		    {
		        // If we're bound, then just clean up the internal stuff.
		        GorgonRenderStatistics.TextureCount--;
		        GorgonRenderStatistics.TextureSize -= Texture.SizeInBytes;
		        Graphics.Shaders.Unbind(Texture);
		        Texture.D3DResource.Dispose();
		        Texture.D3DResource = null;
		    }
		}

		/// <summary>
		/// Function to validate the settings for this depth/stencil buffer.
		/// </summary>
		/// <param name="graphics">Graphics interface that will create this depth/stencil buffer.</param>
		/// <param name="settings">Settings to validate.</param>
		internal static void ValidateSettings(GorgonGraphics graphics, GorgonDepthStencilSettings settings)
		{
			if (settings.Format == BufferFormat.Unknown)
            {
				throw new ArgumentException("The format for the depth buffer ('" + settings.Format + "') is not valid.");
            }

		    if (!graphics.VideoDevice.SupportsDepthFormat(settings.Format))
		    {
		        throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '"
		                                    + settings.Format + "' as a depth/stencil buffer format.");
		    }

            // Make sure we can use the same multi-sampling with our depth buffer.
		    int quality = graphics.VideoDevice.GetMultiSampleQuality(settings.Format, settings.Multisampling.Count);

            // Ensure that the quality of the sampling does not exceed what the card can do.
            if ((settings.Multisampling.Quality >= quality)
                || (settings.Multisampling.Quality < 0))
            {
                throw new ArgumentException("Video device '" + graphics.VideoDevice.Name
                                            + "' does not support multisampling with a count of '"
                                            + settings.Multisampling.Count + "' and a quality of '"
                                            + settings.Multisampling.Quality + " with a format of '"
                                            + settings.Format + "'");
            }

		    if (!settings.AllowShaderView)
		    {
		        if (!graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
		        {
		            throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '"
		                                        + settings.Format + "' as texture format for the depth buffer.");
		        }
		    }
		    else
		    {
                if (settings.TextureFormat == BufferFormat.Unknown)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "The texture format must not be Unknown.");    
                }

		        var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.TextureFormat);

                if (!formatInfo.IsTypeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "The texture format must be typeless.");
                }

                if (!graphics.VideoDevice.Supports2DTextureFormat(settings.TextureFormat))
                {
                    throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '"
                                                + settings.TextureFormat + "' as texture format for the depth buffer.");
                }

                if (graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "Depth/stencil buffers cannot be bound to the shader pipeline with video devices that don't support SM4 or better.");
                }

                if ((graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4_1)
                    && ((settings.Multisampling.Count > 1) || (settings.Multisampling.Quality > 0)))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "Multisampled Depth/stencil buffers cannot be bound to the shader pipeline if the video device does not support SM4_1 or better.");
                }
            }
		}

		/// <summary>
		/// Function to initialize the depth/stencil buffer.
		/// </summary>
		/// <remarks>This will destroy and re-create the depth/stencil buffer according to the modified <see cref="GorgonLibrary.Graphics.GorgonDepthStencil.Settings">settings</see></remarks>
		internal void Initialize()
		{
			CleanUp();
			ValidateSettings(Graphics, Settings);

            Gorgon.Log.Print("GorgonDepthStencil '{0}': Creating depth stencil texture...", Diagnostics.LoggingLevel.Verbose, Name);

            var viewDesc = new D3D.DepthStencilViewDescription();

            if (Texture == null)
            {
                Texture = new GorgonTexture2D(Graphics,
                                              "Depth buffer '" + Name + "' texture.",
                                              new GorgonTexture2DSettings()
                                              {
                                                  Width = Settings.Width,
                                                  Height = Settings.Height,
                                                  MipCount = 1,
                                                  ArrayCount = 1,
                                                  Multisampling = Settings.Multisampling,
                                                  Format =
                                                      (Settings.AllowShaderView ? Settings.TextureFormat : Settings.Format),
                                                  Usage = BufferUsage.Default
                                              });
            }
            else
            {
                Texture.Settings.Width = Settings.Width;
                Texture.Settings.Height = Settings.Height;
                Texture.Settings.Multisampling = Settings.Multisampling;
                Texture.Settings.Format = (Settings.AllowShaderView ? Settings.TextureFormat : Settings.Format);
                Texture.Settings.Usage = BufferUsage.Default;
                // TODO: Determine if depth/stencil can support these properties.
                Texture.Settings.MipCount = 1;
                Texture.Settings.ArrayCount = 1;
            }

            Texture.InitializeDepth(this);

            // Create the view.
            Gorgon.Log.Print("GorgonDepthStencil '{0}': Creating D3D11 depth stencil view...", Diagnostics.LoggingLevel.Verbose, Name);

            // If we have multisampling enabled, then apply it to our view.
            if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 1))
                viewDesc.Dimension = D3D.DepthStencilViewDimension.Texture2DMultisampled;
            else
                viewDesc.Dimension = D3D.DepthStencilViewDimension.Texture2D;
            viewDesc.Texture2D.MipSlice = 0;
            viewDesc.Flags = D3D.DepthStencilViewFlags.None;
            viewDesc.Format = (GI.Format)Settings.Format;
            D3DDepthStencilView = new D3D.DepthStencilView(Graphics.D3DDevice, Texture.D3DResource, viewDesc);
            D3DDepthStencilView.DebugName = "Depth buffer '" + Name + "' view.";

            FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);

            GorgonRenderStatistics.DepthBufferCount++;
            GorgonRenderStatistics.DepthBufferSize += Texture.SizeInBytes;

            // Re-bind the depth/stencil with the new settings.
			if (Graphics.Output.RenderTargets.DepthStencilBuffer == this)
			{
				Graphics.Output.RenderTargets.DepthStencilBuffer = null;
				Graphics.Output.RenderTargets.DepthStencilBuffer = this;
			}
		}

		/// <summary>
		/// Function to clear the depth/stencil buffer.
		/// </summary>
		/// <param name="depthValue">Value to fill with depth portion with.</param>
		/// <param name="stencilValue">Value to fill the stencil portion with.</param>
		public void Clear(float depthValue, int stencilValue)
		{
			if ((FormatInformation.HasDepth) && (!FormatInformation.HasStencil))
			{
				ClearDepth(depthValue);
				return;
			}

			Graphics.Context.ClearDepthStencilView(D3DDepthStencilView, D3D.DepthStencilClearFlags.Depth | D3D.DepthStencilClearFlags.Stencil, depthValue, (byte)stencilValue);
		}

		/// <summary>
		/// Function to clear the depth portion of the depth/stencil buffer.
		/// </summary>
		/// <param name="depthValue">Value to fill the depth buffer with.</param>
		public void ClearDepth(float depthValue)
		{
			if (FormatInformation.HasDepth)
				Graphics.Context.ClearDepthStencilView(D3DDepthStencilView, D3D.DepthStencilClearFlags.Depth, depthValue, 0);
		}

		/// <summary>
		/// Function to clear the stencil portion of the depth/stencil buffer.
		/// </summary>
		/// <param name="stencilValue">Value to fill the stencil buffer with.</param>
		public void ClearStencil(int stencilValue)
		{			
			if (FormatInformation.HasStencil)
				Graphics.Context.ClearDepthStencilView(D3DDepthStencilView, D3D.DepthStencilClearFlags.Stencil, 1.0f, (byte)stencilValue);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencil"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this depth/stencil buffer.</param>
		/// <param name="name">The name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings for the depth buffer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonDepthStencil(GorgonGraphics graphics, string name, GorgonDepthStencilSettings settings)
			: base(name)
		{
			Settings = settings;
			Graphics = graphics;			
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
		    if (_isDisposed)
		    {
		        return;
		    }

		    if (disposing)
		    {
		        // Unbind this depth/stencil buffer.
		        if (Graphics.Output.RenderTargets.DepthStencilBuffer == this)
		        {
		            Graphics.Output.RenderTargets.DepthStencilBuffer = null;
		        }

		        CleanUp();

		        Graphics.RemoveTrackedObject(this);
		    }

		    _isDisposed = true;
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
