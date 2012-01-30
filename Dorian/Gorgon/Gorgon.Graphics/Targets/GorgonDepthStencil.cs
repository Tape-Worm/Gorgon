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
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private bool _isDisposed = false;			// Flag to indicate that the object was disposed of.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D texture for the depth/stencil buffer.
		/// </summary>
		internal D3D.Texture2D D3DTexture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D depth/stencil view for the depth/stencil buffer.
		/// </summary>
		internal D3D.DepthStencilView D3DDepthStencilView
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
			}

			if (D3DTexture != null)
			{
				Gorgon.Log.Print("GorgonDepthStencil '{0}': Destroying D3D11 depth stencil texture...", Diagnostics.LoggingLevel.Verbose, Name);
				D3DTexture.Dispose();
			}

			D3DTexture = null;
			D3DDepthStencilView = null;
		}

		/// <summary>
		/// Function to create the resources for the depth/stencil buffer.
		/// </summary>
		private void CreateResources()
		{
			Gorgon.Log.Print("GorgonDepthStencil '{0}': Creating D3D11 depth stencil texture...", Diagnostics.LoggingLevel.Verbose, Name);

			D3D.Texture2DDescription desc = new D3D.Texture2DDescription();
			D3D.DepthStencilViewDescription viewDesc = new D3D.DepthStencilViewDescription();

			desc.ArraySize = 1;
			desc.BindFlags = D3D.BindFlags.DepthStencil;
			desc.Format = (GI.Format)Settings.Format;

			// Determine if we can bind this to a shader.
			if (Settings.TextureFormat != BufferFormat.Unknown)
			{
				desc.BindFlags |= D3D.BindFlags.ShaderResource;
				desc.Format = (GI.Format)Settings.TextureFormat;
			}

			desc.CpuAccessFlags = D3D.CpuAccessFlags.None;
			desc.Height = Settings.Height;
			desc.Width = Settings.Width;
			desc.MipLevels = 1;
			desc.OptionFlags = D3D.ResourceOptionFlags.None;
			desc.SampleDescription = GorgonMultiSampling.Convert(Settings.MultiSample);
			desc.Usage = D3D.ResourceUsage.Default;

			D3DTexture = new D3D.Texture2D(Graphics.VideoDevice.D3DDevice, desc);
			D3DTexture.DebugName = "Depth buffer '" + Name + "' texture.";

			// Create the view.
			Gorgon.Log.Print("GorgonDepthStencil '{0}': Creating D3D11 depth stencil view...", Diagnostics.LoggingLevel.Verbose, Name);

			// If we have multisampling enabled, then apply it to our view.
			if ((Settings.MultiSample.Count > 1) || (Settings.MultiSample.Quality > 1))
				viewDesc.Dimension = D3D.DepthStencilViewDimension.Texture2DMultisampled;
			else
				viewDesc.Dimension = D3D.DepthStencilViewDimension.Texture2D;
			viewDesc.Texture2D.MipSlice = 0;
			viewDesc.Flags = D3D.DepthStencilViewFlags.None;
			viewDesc.Format = (GI.Format)Settings.Format;
			D3DDepthStencilView = new D3D.DepthStencilView(Graphics.VideoDevice.D3DDevice, D3DTexture, viewDesc);
			D3DDepthStencilView.DebugName = "Depth buffer '" + Name + "' view.";

			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
		}

		/// <summary>
		/// Function to validate the settings for this depth/stencil buffer.
		/// </summary>
		/// <param name="graphics">Graphics interface that will create this depth/stencil buffer.</param>
		/// <param name="settings">Settings to validate.</param>
		internal static void ValidateSettings(GorgonGraphics graphics, GorgonDepthStencilSettings settings)
		{
			if (settings.Format == BufferFormat.Unknown)
				throw new ArgumentException("The format for the depth buffer ('" + settings.Format.ToString() + "') is not valid.");

			if (!graphics.VideoDevice.SupportsDepthFormat(settings.Format))
				throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '" + settings.Format + "' as a depth/stencil buffer format.");

			if (!graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
				throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '" + settings.Format + "' as texture format for the depth buffer.");

			// Make sure we can use the same multi-sampling with our depth buffer.
			int quality = 0;
			quality = graphics.VideoDevice.GetMultiSampleQuality(settings.Format, settings.MultiSample.Count);

			// Ensure that the quality of the sampling does not exceed what the card can do.
			if ((settings.MultiSample.Quality >= quality) || (settings.MultiSample.Quality < 0))
				throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support multisampling with a count of '" + settings.MultiSample.Count.ToString() + "' and a quality of '" + settings.MultiSample.Quality.ToString() + " with a format of '" + settings.Format + "'");

			// Validate the texture format if it's different.
			if ((settings.TextureFormat != BufferFormat.Unknown) && (settings.TextureFormat != settings.Format))
			{
				if (!graphics.VideoDevice.SupportsDepthFormat(settings.TextureFormat))
					throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '" + settings.TextureFormat + "' as a depth/stencil buffer format.");

				// Ensure that this format can be used to pass to a shader.
				if (!graphics.VideoDevice.Supports2DTextureFormat(settings.TextureFormat))
					throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support '" + settings.TextureFormat + "' as texture format for the depth buffer.");

				// Feature levels less than 4.1 can't read back multi sampled depth buffers in a shader.
				if ((graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM4_1) && (graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM5) &&
					((settings.MultiSample.Count > 1) || (settings.MultiSample.Quality > 0)))
					throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' cannot bind a multi sampled depth buffer to a shader if the feature level is less than SM_4_1");
			}
		}

		/// <summary>
		/// Function to update the settings for the depth/stencil buffer.
		/// </summary>
		/// <remarks>This will destroy and re-create the depth/stencil buffer according to the modified <see cref="P:GorgonLibrary.GorgonGraphics.GorgonDepthStencil.Settings">settings</see></remarks>
		public void UpdateSettings()
		{
			CleanUp();
			ValidateSettings(Graphics, Settings);
			CreateResources();
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
			if (!_isDisposed)
			{
				if (disposing)
				{
					CleanUp();
					if (Graphics != null)
						Graphics.RemoveTrackedObject(this);
				}

				Graphics = null;
				_isDisposed = true;
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
