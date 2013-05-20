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
// Created: Saturday, May 11, 2013 1:56:53 PM
// 
#endregion

using System;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// An unordered access resource view.
    /// </summary>
    /// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
    /// <para>Unordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
    /// have a format that is the same bit-depth and in the same group as its bound resource.</para>
    /// <para>Unlike a <see cref="GorgonLibrary.Graphics.GorgonTextureShaderView">GorgonTextureShaderView</see> or <see cref="GorgonLibrary.Graphics.GorgonBufferShaderView">GorgonBufferShaderView</see>, 
    /// only one unordered access view may be applied to a resource.</para>
    /// <para>Unordered access views are only available on SM_5 or better video devices.</para>
    /// </remarks>
    public abstract class GorgonUnorderedAccessView
        : IDisposable
    {
        #region Variables.
        private bool _disposed;             // Flag to indicate whether this object was disposed or not.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct3D unordered access resource view.
        /// </summary>
        internal D3D.UnorderedAccessView D3DView
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the resource that the view is bound with.
        /// </summary>
        public GorgonResource Resource
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the format for the view.
        /// </summary>
        public BufferFormat Format
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return information about the view format.
        /// </summary>
        public GorgonBufferFormatInfo.GorgonFormatData FormatInformation
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clean up the resources used by the view.
        /// </summary>
        internal void CleanUp()
        {
            if (D3DView == null)
            {
                return;
            }

			// TODO: We'll have to do this for unordered view properties.
            //Resource.Graphics.Shaders.Unbind(this);

            Gorgon.Log.Print("Destroying unordered access resource view for {0}.",
                             LoggingLevel.Verbose,
                             Resource.GetType().FullName);
            D3DView.Dispose();
            D3DView = null;
        }

        /// <summary>
        /// Function to perform initialization of the shader view resource.
        /// </summary>
        protected abstract void InitializeImpl();

        /// <summary>
        /// Function to perform initialization of the shader view resource.
        /// </summary>
        internal void Initialize()
        {
			Gorgon.Log.Print("Creating unordered access resource view for {0}.", LoggingLevel.Verbose, Resource.GetType().FullName);
			InitializeImpl();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonUnorderedAccessView"/> class.
        /// </summary>
        /// <param name="resource">The buffer to bind to the view.</param>
        /// <param name="format">The format of the view.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonUnorderedAccessView(GorgonResource resource, BufferFormat format)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            Resource = resource;
            Format = format;
            FormatInformation = GorgonBufferFormatInfo.GetInfo(Format);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (D3DView != null)
                {
                    D3DView.Dispose();
                    D3DView = null;
                }
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

