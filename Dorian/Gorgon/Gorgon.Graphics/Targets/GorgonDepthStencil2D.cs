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
// Created: Saturday, June 8, 2013 9:17:21 PM
// 
#endregion

using System;
using System.ComponentModel;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A 2D depth/stencil buffer.
	/// </summary>
	/// <remarks>This is for setting a depth and/or stencil buffer along with a render target.  When pairing with a render target, the user must ensure that the depth/stencil buffer matches the dimensions and 
	/// the multisampling settings for the render target.</remarks>
	public class GorgonDepthStencil2D
		: GorgonTexture2D
	{
		#region Variables.
		private bool _isDisposed;							// Flag to indicate that the object was disposed of.		
		private GorgonDepthStencilView _defaultView;		// The default depth/stencil view.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the render target that has this depth/stencil buffer as its default.
		/// </summary>
		public GorgonRenderTarget2D RenderTarget
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the swap chain that owns this depth/stencil buffer.
		/// </summary>
		public GorgonSwapChain SwapChain
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the settings for a depth/stencil buffer.
		/// </summary>
		public new GorgonDepthStencil2DSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the resource objects.
		/// </summary>
		protected override void CreateDefaultResourceView()
		{
			// We do not create a default shader view for this resource.
		}

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			Gorgon.Log.Print("Destroying GorgonDepthStencil '{0}'...", LoggingLevel.Verbose, Name);
			GorgonRenderStatistics.DepthBufferCount--;
			GorgonRenderStatistics.DepthBufferSize -= SizeInBytes;

			// Unbind our depth/stencil buffer.
			if ((Graphics.Output.DepthStencilView != null) && (Graphics.Output.DepthStencilView.Resource == this))
			{
				Graphics.Output.DepthStencilView = null;
			}

			base.CleanUpResource();
		}

		/// <summary>
		/// Function to initialize the texture with optional initial data.
		/// </summary>
		/// <param name="initialData">Data used to populate the image.</param>
		protected override void OnInitialize(GorgonImageData initialData)
		{
			var shaderBind = Settings.AllowShaderView ? SharpDX.Direct3D11.BindFlags.ShaderResource : SharpDX.Direct3D11.BindFlags.None;

			var desc = new SharpDX.Direct3D11.Texture2DDescription
				{
					ArraySize = Settings.ArrayCount,
					BindFlags = GetBindFlags(true, false) | shaderBind,
					CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
					Format = Settings.AllowShaderView ? (SharpDX.DXGI.Format)Settings.TextureFormat : (SharpDX.DXGI.Format)Settings.Format,
					Height = Settings.Height,
					Width = Settings.Width,
					MipLevels = Settings.MipCount,
					OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
					SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling)
				};

			Gorgon.Log.Print("{0} {1}: Creating 2D depth/stencil texture...", LoggingLevel.Verbose, GetType().Name, Name);

			// Create the texture.
			D3DResource = initialData != null
				              ? new SharpDX.Direct3D11.Texture2D(Graphics.D3DDevice, desc, initialData.GetDataBoxes())
				              : new SharpDX.Direct3D11.Texture2D(Graphics.D3DDevice, desc);

			GorgonRenderStatistics.DepthBufferCount++;
			GorgonRenderStatistics.DepthBufferSize += SizeInBytes;

			_defaultView = CreateDepthStencilView(Settings.Format, 0, 0, 1, Settings.DefaultDepthStencilViewFlags);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <exception cref="System.NotSupportedException">This method is not supported for depth/stencil buffers.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void OnUpdateSubResource(ISubResourceData data, int subResource)
		{
			throw new NotSupportedException(Resources.GORGFX_DEPTH_OPERATION_NOT_SUPPORTED);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <exception cref="System.NotSupportedException">This method is not supported for depth/stencil buffers.</exception>
		/// <remarks>This method is not supported for depth/stencil buffers.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void UpdateSubResource(ISubResourceData data)
		{
			throw new NotSupportedException(Resources.GORGFX_DEPTH_OPERATION_NOT_SUPPORTED);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <exception cref="System.NotSupportedException">This method is not supported for depth/stencil buffers.</exception>
		/// <remarks>This method is not supported for depth/stencil buffers.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void UpdateSubResource(ISubResourceData data, int subResource)
		{
			throw new NotSupportedException(Resources.GORGFX_DEPTH_OPERATION_NOT_SUPPORTED);
		}

		/// <summary>
		/// Function called if this buffer is attached to a swap chain and it's been resized.
		/// </summary>
		/// <returns>TRUE if this depth buffer was bound to the pipeline, FALSE if not.</returns>
		internal bool OnDepthStencilResize()
		{
			bool result = (Graphics.Output.DepthStencilView != null) && (Graphics.Output.DepthStencilView.Resource == this);

			CleanUpResource();

			return result;
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="destRect">Destination region to copy into.</param>
		/// <remarks>This method is not supported for depth/stencil buffers.</remarks>
		/// <exception cref="System.NotSupportedException">This method is not supported by depth/stencil buffers.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void UpdateSubResource(ISubResourceData data, int subResource, System.Drawing.Rectangle destRect)
		{
			throw new NotSupportedException(Resources.GORGFX_DEPTH_OPERATION_NOT_SUPPORTED);
		}

        /// <summary>
        /// Function to create a new depth/stencil view object.
        /// </summary>
        /// <param name="format">The format of the depth/stencil view.</param>
        /// <param name="mipSlice">Starting mip map for the view.</param>
        /// <param name="arrayStart">Starting array index for the view.</param>
        /// <param name="arrayCount">Array index count for the view.</param>
        /// <param name="flags">Flags to determine how to treat the bound view.</param>
        /// <remarks>Use a depth/stencil view to bind a resource to the pipeline as a depth/stencil buffer.  A depth/stencil view can view a select portion of the texture, and the view <paramref name="format"/> can be used to 
        /// cast the format of the texture into another type (as long as the view format has the same bit depth as the depth/stencil format).  For example, a texture with a format of D32_Float could be cast 
        /// to R32_Uint, R32_Int or R32_Float formats.
        /// <para>The <paramref name="flags"/> parameter will allow the depth/stencil buffer to be read simultaneously from the depth/stencil view and from a shader view.  It is not normally possible to bind a view of a 
        /// resource to 2 parts of the pipeline at the same time.  However, using the flags provided, read-only access may be granted to a part of the resource (depth or stencil) or all of it for all parts of the pipline.  
        /// This would bind the depth/stencil as a read-only view and make it a read-only view accessible to shaders. If the flags are not set to None, then the depth/stencil buffer must allow shader access.</para>
        /// <para>Binding to simulatenous views require a video device with a feature level of SM5 or better.</para>
        /// </remarks>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture has a usage of staging.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="format"/> is not valid for the view.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="arrayStart"/> and the <paramref name="arrayCount"/> are less than 0 or 1 respectively, or greater than the number of array indices in the texture.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="mipSlice"/> is less than 0 or greater than the number of mip levels in the texture.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="flags"/> property is not set to None and the depth buffer does not allow shader access, or if the current video device feature level is not SM5 or better.</para>
        /// </exception>
        /// <returns>A texture shader view object.</returns>
        public GorgonDepthStencilView CreateDepthStencilView(BufferFormat format, int mipSlice, int arrayStart, int arrayCount, DepthStencilViewFlags flags)
        {
            if (flags != DepthStencilViewFlags.None)
            {
                if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
                }

                if (!Settings.AllowShaderView)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_VIEW_NO_SUPPORT, "GorgonShaderView"));
                }
            }

            return OnCreateDepthStencilView(format, mipSlice, arrayStart, arrayCount, flags);
        }

		/// <summary>
		/// Function to clear the depth portion of the depth/stencil buffer.
		/// </summary>
		/// <param name="depthValue">Value to fill the depth buffer with.</param>
		public void ClearDepth(float depthValue)
		{
			_defaultView.ClearDepth(depthValue);
		}

		/// <summary>
		/// Function to clear the stencil portion of the depth/stencil buffer.
		/// </summary>
		/// <param name="stencilValue">Value to fill the stencil buffer with.</param>
		public void ClearStencil(int stencilValue)
		{
			_defaultView.ClearStencil(stencilValue);
		}

		/// <summary>
		/// Function to retrieve the depth stencil  view for a depth stencil .
		/// </summary>
		/// <param name="target">Render target to evaluate.</param>
		/// <returns>The depth stencil  view for the swap chain.</returns>
		public static GorgonDepthStencilView ToDepthStencilView(GorgonDepthStencil2D target)
		{
			return target == null ? null : target._defaultView;
		}

		/// <summary>
		/// Implicit operator to return the depth stencil  view for a depth stencil 
		/// </summary>
		/// <param name="target">Render target to evaluate.</param>
		/// <returns>The depth stencil  view for the swap chain.</returns>
		public static implicit operator GorgonDepthStencilView(GorgonDepthStencil2D target)
		{
			return target == null ? null : target._defaultView;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencil2D"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this depth/stencil buffer.</param>
		/// <param name="name">The name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings for the depth buffer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonDepthStencil2D(GorgonGraphics graphics, string name, GorgonDepthStencil2DSettings settings)
			: base(graphics, name, settings)
		{
			Settings = settings;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			if (disposing)
			{
				if (_defaultView != null)
				{
					_defaultView.Dispose();
					_defaultView = null;
				}

				// Removing bindings.
				SwapChain = null;
				RenderTarget = null;
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}
		#endregion
	}
}