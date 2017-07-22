﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 21, 2016 11:05:38 AM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Base class for shader resource views.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This base class is used to define shader resource views for strongly typed resources like textures and buffers.
	/// </para>
	/// </remarks>
	public abstract class GorgonShaderResourceView
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the native Direct 3D 11 view.
		/// </summary>
		protected internal D3D11.ShaderResourceView NativeView
		{
			get;
			protected set;
		}

        /// <summary>
        /// Property to return the resource bound to the view.
        /// </summary>
	    public GorgonResource Resource
	    {
	        get;
	    }
        #endregion

        #region Methods.
	    /// <summary>
	    /// Function to initialize the buffer view.
	    /// </summary>
	    protected internal abstract void CreateNativeView();

		/// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "I have no finalizer. This is completely overridable. Idiot.")]
		public virtual void Dispose()
		{
			NativeView?.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderResourceView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		protected GorgonShaderResourceView(GorgonResource resource)
		{
		    Resource = resource;
		}
		#endregion
	}
}
