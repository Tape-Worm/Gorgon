#region MIT
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
using System.Threading;
using Gorgon.Diagnostics;
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
		: IDisposable, IGorgonGraphicsObject
	{
        #region Variables.
        // The D3D11 shader resource view.
	    private D3D11.ShaderResourceView1 _view;
        // The resource to bind to the GPU.
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
	    /// Property to return the native Direct 3D 11 view.
	    /// </summary>
	    protected internal D3D11.ShaderResourceView1 Native
		{
			get => _view;
			protected set => _view = value;
		}

	    /// <summary>
	    /// Property to return whether or not the object is disposed.
	    /// </summary>
	    public bool IsDisposed => _resource == null;

	    /// <summary>
	    /// Property to return the resource bound to the view.
	    /// </summary>
	    public GorgonGraphicsResource Resource => _resource;

        /// <summary>
        /// Property to return the usage flag(s) for the resource.
        /// </summary>
	    public ResourceUsage Usage => _resource?.Usage ?? ResourceUsage.Default;

        /// <summary>
        /// Property to return the format for the view.
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
	    /// Property to return the graphics interface that built this object.
	    /// </summary>
	    public GorgonGraphics Graphics => Resource?.Graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the buffer view.
        /// </summary>
        protected internal abstract void CreateNativeView();

		/// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
		{
		    D3D11.ShaderResourceView1 view = Interlocked.Exchange(ref _view, null);

		    if (view == null)
		    {
		        return;
		    }

            this.UnregisterDisposable(Resource.Graphics);

		    if (OwnsResource)
		    {
		        Graphics.Log.Print($"Shader Resource View '{Resource.Name}': Releasing D3D11 Resource {Resource.ResourceType} because it owns it.", LoggingLevel.Simple);
		        GorgonGraphicsResource resource = Interlocked.Exchange(ref _resource, null);
                resource?.Dispose();
		    }
		    
		    Graphics.Log.Print($"Shader Resource View '{Resource.Name}': Releasing D3D11 shader resource view.", LoggingLevel.Simple);
            view.Dispose();

            GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderResourceView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="formatInfo">Information about the format.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is <b>null</b>.</exception>
		protected GorgonShaderResourceView(GorgonGraphicsResource resource, BufferFormat format, GorgonFormatInfo formatInfo)
		{
		    _resource = resource ?? throw new ArgumentNullException(nameof(resource));
		    Format = format;
		    FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
		}
		#endregion
	}
}
