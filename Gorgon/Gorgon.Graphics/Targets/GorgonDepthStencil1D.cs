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
// Created: Saturday, June 8, 2013 9:17:04 PM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A 1D depth/stencil buffer.
	/// </summary>
	/// <remarks>This is for setting a depth and/or stencil buffer along with a render target.  When pairing with a render target, the user must ensure that the depth/stencil buffer matches the dimensions and 
	/// the multisampling settings for the render target.</remarks>
	public class GorgonDepthStencil1D
		: GorgonTexture1D
	{
		#region Variables.
		private bool _isDisposed;							// Flag to indicate that the object was disposed of.		
        private GorgonDepthStencilView _defaultView;        // The default view for the depth buffer.
		#endregion

		#region Properties.
        /// <summary>
		/// Property to return the render target that has this depth/stencil buffer as its default.
		/// </summary>
		public GorgonRenderTarget1D RenderTarget
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the settings for a depth/stencil buffer.
		/// </summary>
		public new GorgonDepthStencil1DSettings Settings
		{
			get;
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
			GorgonApplication.Log.Print("Destroying GorgonDepthStencil '{0}'...", LoggingLevel.Verbose, Name);
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
		/// Function called if this buffer is attached to a swap chain and it's been resized.
		/// </summary>
		/// <returns><b>true</b> if this depth buffer was bound to the pipeline, <b>false</b> if not.</returns>
		internal bool OnDepthStencilResize()
		{
			bool result = (Graphics.Output.DepthStencilView != null) && (Graphics.Output.DepthStencilView.Resource == this);

			CleanUpResource();

			return result;
		}

		/// <summary>
		/// Function to initialize the texture with optional initial data.
		/// </summary>
		/// <param name="initialData">Data used to populate the image.</param>
		protected override void OnInitialize(GorgonImageData initialData)
		{
			var shaderBind = Settings.AllowShaderView ? BindFlags.ShaderResource : BindFlags.None;

			var desc = new Texture1DDescription
				{
					ArraySize = Settings.ArrayCount,
					BindFlags = GetBindFlags(true, false) | shaderBind,
					CpuAccessFlags = CpuAccessFlags.None,
					Format = Settings.AllowShaderView ? (Format)Settings.TextureFormat : (Format)Settings.Format,
					Width = Settings.Width,
					MipLevels = Settings.MipCount,
					OptionFlags = ResourceOptionFlags.None
				};

			GorgonApplication.Log.Print("{0} {1}: Creating 1D depth/stencil texture...", LoggingLevel.Verbose, GetType().Name, Name);

			// Create the texture.
			D3DResource = initialData != null
				              ? new Texture1D(Graphics.D3DDevice, desc, initialData.Buffers.DataBoxes)
				              : new Texture1D(Graphics.D3DDevice, desc);

			GorgonRenderStatistics.DepthBufferCount++;
			GorgonRenderStatistics.DepthBufferSize += SizeInBytes;

			_defaultView = GetDepthStencilView(Settings.Format, 0, 0, 1, Settings.DefaultDepthStencilViewFlags);
		}

        /// <summary>
        /// Function to copy data from the CPU to the texture on the GPU.
        /// </summary>
        /// <param name="buffer">A buffer containing the image data to copy.</param>
        /// <param name="destRange">A start and end range value that will specify the region that will receive the data.</param>
        /// <param name="destArrayIndex">[Optional] The array index that will receive the data.</param>
        /// <param name="destMipLevel">[Optional] The mip map level that will receive the data.</param>
        /// <param name="deferred">[Optional] A deferred graphics context used to copy the data.</param>
        /// <exception cref="System.NotSupportedException">This method is not supported for depth/stencil buffers.</exception>
        /// <remarks>
        /// This method is not supported for depth/stencil buffers.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void UpdateSubResource(GorgonImageBuffer buffer,
	        GorgonRange destRange,
	        int destArrayIndex = 0,
	        int destMipLevel = 0,
	        GorgonGraphics deferred = null)
	    {
            throw new NotSupportedException(Resources.GORGFX_DEPTH_OPERATION_NOT_SUPPORTED);
	    }

        /// <summary>
        /// Function to lock a CPU accessible texture sub resource for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used to lock.</param>
        /// <param name="arrayIndex">[Optional] Array index of the sub resource to lock.</param>
        /// <param name="mipLevel">[Optional] The mip-map level of the sub resource to lock.</param>
        /// <param name="deferred">[Optional] The deferred graphics context used to lock the texture.</param>
        /// <returns>
        /// A stream used to write to the texture.
        /// </returns>
        /// <exception cref="System.NotSupportedException">This method is not supported for depth/stencil buffers.</exception>
        /// <remarks>
        /// This method is not supported for depth/stencil buffers.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
	    public override GorgonTextureLockData Lock(BufferLockFlags lockFlags,
	        int arrayIndex = 0,
	        int mipLevel = 0,
	        GorgonGraphics deferred = null)
	    {
            throw new NotSupportedException(Resources.GORGFX_DEPTH_OPERATION_NOT_SUPPORTED);
	    }

        /// <summary>
        /// Function to retrieve a depth/stencil view object.
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
        /// <exception cref="GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
        /// <returns>A texture shader view object.</returns>
        public GorgonDepthStencilView GetDepthStencilView(BufferFormat format, int mipSlice, int arrayStart, int arrayCount, DepthStencilViewFlags flags)
		{
	        if (flags == DepthStencilViewFlags.None)
	        {
		        return OnGetDepthStencilView(format, mipSlice, arrayStart, arrayCount, flags);
	        }

	        if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.Sm5)
	        {
		        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.Sm5));
	        }

	        if (!Settings.AllowShaderView)
	        {
		        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_VIEW_NO_SUPPORT, "GorgonShaderView"));
	        }

	        return OnGetDepthStencilView(format, mipSlice, arrayStart, arrayCount, flags);
		}

        /// <summary>
        /// Function to clear the depth and stencil buffers.
        /// </summary>
        /// <param name="depthValue">[Optional] The value to fill the depth portion with.</param>
        /// <param name="stencilValue">[Optional] The value to fill the stencil portion with.</param>
        /// <param name="deferred">[Optional] The deferred context to use when clearing.</param>
        /// <remarks>
        /// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the depth/stencil buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the depth/stencil buffer.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the depth/stencil because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </remarks>
        public void Clear(float depthValue, byte stencilValue, GorgonGraphics deferred = null)
        {
            _defaultView.Clear(depthValue, stencilValue, deferred);
        }

        /// <summary>
        /// Function to clear the depth portion of the buffer.
        /// </summary>
        /// <param name="depthValue">The value to fill the depth portion with.</param>
        /// <param name="deferred">[Optional] The deferred context to use when clearing.</param>
        /// <remarks>
        /// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the depth/stencil buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the depth/stencil buffer.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the depth/stencil because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </remarks>
        public void ClearDepth(float depthValue, GorgonGraphics deferred = null)
        {
            _defaultView.ClearDepth(depthValue, deferred);
        }

        /// <summary>
        /// Function to clear the stencil portion of the buffer.
        /// </summary>
        /// <param name="stencilValue">The value to fill the stencil portion with.</param>
        /// <param name="deferred">[Optional] The deferred context to use when clearing.</param>
        /// <remarks>
        /// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to clear the depth/stencil buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the depth/stencil buffer.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the depth/stencil because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </remarks>
        public void ClearStencil(byte stencilValue, GorgonGraphics deferred = null)
        {
            _defaultView.ClearStencil(stencilValue, deferred);
        }
        
        /// <summary>
		/// Function to retrieve the depth stencil  view for a depth stencil .
		/// </summary>
		/// <param name="target">Render target to evaluate.</param>
		/// <returns>The depth stencil  view for the swap chain.</returns>
		public static GorgonDepthStencilView ToDepthStencilView(GorgonDepthStencil1D target)
		{
			return target == null ? null : target._defaultView;
		}

		/// <summary>
		/// Implicit operator to return the depth stencil  view for a depth stencil 
		/// </summary>
		/// <param name="target">Render target to evaluate.</param>
		/// <returns>The depth stencil  view for the swap chain.</returns>
		public static implicit operator GorgonDepthStencilView(GorgonDepthStencil1D target)
		{
			return target == null ? null : target._defaultView;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencil1D"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this depth/stencil buffer.</param>
		/// <param name="name">The name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings for the depth buffer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonDepthStencil1D(GorgonGraphics graphics, string name, GorgonDepthStencil1DSettings settings)
			: base(graphics, name, settings)
		{
			Settings = (GorgonDepthStencil1DSettings)settings.Clone();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			if (disposing)
			{
				// Removing bindings.
				RenderTarget = null;
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}
		#endregion
	}
}