#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 17, 2018 8:45:32 AM
// 
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using Gorgon.Diagnostics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A view to allow 2D texture based render targets to be bound to the pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A render target view allows a render target (such as a <see cref="GorgonSwapChain"/> or a texture to be bound to the GPU pipeline as a render target resource.
    /// </para>
    /// <para>
    /// The view can bind the entire resource, or a sub section of the resource as required. It will also allow for casting of the format to allow for reinterpreting the data stored within the the render 
    /// target. 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonSwapChain"/>
    /// <seealso cref="GorgonTexture2D"/>
    /// <seealso cref="GorgonTexture3D"/>
    public abstract class GorgonRenderTargetView
        : IDisposable, IGorgonGraphicsObject
    {
		#region Variables.
        // The D3D 11 render target view.
        private D3D11.RenderTargetView1 _view;
        // The texture bound to this view.
        private GorgonGraphicsResource _resource;
        #endregion

		#region Properties.
        /// <summary>
        /// Property to set or return whether the view owns the attached resource or not.
        /// </summary>
        protected bool OwnsResource
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the render target view.
        /// </summary>
        internal D3D11.RenderTargetView1 Native
        {
            get => _view;
            set => _view = value;
        }

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public abstract TextureBinding Binding
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the object is disposed.
        /// </summary>
        public bool IsDisposed => _resource == null;

        /// <summary>
        /// Property to return the resource bound to this render target view.
        /// </summary>
        public GorgonGraphicsResource Resource => _resource;

        /// <summary>
        /// Property to return the format used to interpret this view.
        /// </summary>
        public BufferFormat Format
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
        /// Property to return the intended usage flags for the texture.
        /// </summary>
        public ResourceUsage Usage => _resource?.Usage ?? ResourceUsage.Default;

        /// <summary>
        /// Property to return the graphics interface that built the texture.
        /// </summary>
        public GorgonGraphics Graphics => _resource?.Graphics;

        /// <summary>
        /// Property to return the width of the render target view.
        /// </summary>
        public abstract int Width
        {
            get;
        }

        /// <summary>
        /// Property to return the height of the render target view.
        /// </summary>
        public abstract int Height
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a native version of the render target view.
        /// </summary>
        private protected abstract void OnCreateNativeView();

        /// <summary>
        /// Function to create a native version of the render target view.
        /// </summary>
        internal void CreateNativeView()
        {
            OnCreateNativeView();

            Debug.Assert(Native != null, "No view was created.");
            
            this.RegisterDisposable(Graphics);
        }

        /// <summary>
        /// Function to clear the contents of the render target for this view.
        /// </summary>
        /// <param name="color">Color to use when clearing the render target view.</param>
        /// <remarks>
        /// <para>
        /// This will clear the render target view to the specified <paramref name="color"/>.  
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color)
        {
            Graphics.D3DDeviceContext.ClearRenderTargetView(Native, color.ToRawColor4());
        }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
		    D3D11.RenderTargetView1 view = Interlocked.Exchange(ref _view, null);

		    if (view == null)
		    {
                GC.SuppressFinalize(this);
		        return;
		    }

            this.UnregisterDisposable(Resource.Graphics);

		    if (OwnsResource)
		    {
		        Graphics.Log.Print($"Render Target View '{Resource.Name}': Releasing D3D11 resource because it owns it.", LoggingLevel.Simple);

		        GorgonGraphicsResource resource = Interlocked.Exchange(ref _resource, null);
                resource?.Dispose();
		    }

		    Graphics.Log.Print($"Render Target View '{Resource.Name}': Releasing D3D11 render target view.", LoggingLevel.Simple);
		    view.Dispose();

            GC.SuppressFinalize(this);
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTarget2DView"/> class.
        /// </summary>
        /// <param name="resource">The resource to bind.</param>
        /// <param name="format">The format of the render target view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
        protected GorgonRenderTargetView(GorgonGraphicsResource resource, BufferFormat format, GorgonFormatInfo formatInfo)
        {
            _resource = resource ?? throw new ArgumentNullException(nameof(formatInfo));
            Format = format;
            FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
        }
        #endregion
    }
}
