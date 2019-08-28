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
        : GorgonResourceView
    {
        #region Properties.
        /// <summary>
        /// Property to return the native D3D depth/stencil view.
        /// </summary>
        internal D3D11.RenderTargetView1 Native
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public abstract TextureBinding Binding
        {
            get;
        }

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
        /// Function to clear the contents of the render target for this view.
        /// </summary>
        /// <param name="color">Color to use when clearing the render target view.</param>
        /// <remarks>
        /// <para>
        /// This will clear the render target view to the specified <paramref name="color"/>.  
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color) => Graphics.D3DDeviceContext.ClearRenderTargetView(Native, color.ToRawColor4());
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
            : base(resource)
        {
            FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
            Format = format;
        }
        #endregion
    }
}
