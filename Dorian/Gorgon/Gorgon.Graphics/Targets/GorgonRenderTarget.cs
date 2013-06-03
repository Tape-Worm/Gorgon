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
	/// The base render target object.
	/// </summary>
	/// <remarks>This is the base class for all render target types in Gorgon.</remarks>
	public abstract class GorgonRenderTarget
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
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
		/// Property to return the settings for this render target.
		/// </summary>
		public IRenderTargetSettings Settings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default depth/stencil for this render target.
		/// </summary>
		public GorgonDepthStencil DepthStencilBuffer
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
		/// Function to clean up any internal resources.
		/// </summary>
		protected abstract void OnCleanUp();

		/// <summary>
		/// Function to create the resources for the render target.
		/// </summary>
		protected abstract void OnInitialize();

		/// <summary>
		/// Function to clean up any internal resources.
		/// </summary>
		internal void CleanUp()
		{
            Graphics.Output.RenderTargets.Unbind(this);
            OnCleanUp();
		}

		/// <summary>
		/// Function to initialize the target.
		/// </summary>
		internal void Initialize()
		{
			OnInitialize();
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

			if ((DepthStencilBuffer != null) && (DepthStencilBuffer.FormatInformation.HasDepth))
				DepthStencilBuffer.ClearDepth(depthValue);
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
			if ((DepthStencilBuffer != null) && (DepthStencilBuffer.FormatInformation.HasDepth) && (!DepthStencilBuffer.FormatInformation.HasStencil))
			{
				Clear(color, depthValue);
				return;
			}

			Clear(color);

			if ((DepthStencilBuffer != null) && (DepthStencilBuffer.FormatInformation.HasDepth) && (DepthStencilBuffer.FormatInformation.HasStencil))
			{
				DepthStencilBuffer.Clear(depthValue, stencilValue);
			}
		}
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the render target.</param>
		/// <param name="settings">Settings to apply to the render target.</param>
		protected GorgonRenderTarget(GorgonGraphics graphics, string name, IRenderTargetSettings settings)
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
		    if (_disposed)
		    {
		        return;
		    }

		    if (disposing)
		    {
		        CleanUp();

		        Graphics.RemoveTrackedObject(this);
		    }

            _disposed = true;
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
