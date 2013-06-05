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
	[Obsolete("I'm deleting this.")]
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
		public IRenderTargetTextureSettings Settings
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
            OnCleanUp();
		}

		/// <summary>
		/// Function to initialize the target.
		/// </summary>
		internal void Initialize()
		{
			OnInitialize();
		}
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the render target.</param>
		/// <param name="settings">Settings to apply to the render target.</param>
		protected GorgonRenderTarget(GorgonGraphics graphics, string name, IRenderTargetTextureSettings settings)
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
