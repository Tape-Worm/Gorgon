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
    /// A shader resource view.
    /// </summary>
    /// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
    /// the resource to be cast to any format within the same group.</remarks>
    public abstract class GorgonShaderView
    {
        #region Properties.
        /// <summary>
        /// Property to return the Direct3D shader resource view.
        /// </summary>
        internal D3D.ShaderResourceView D3DView
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

		/// <summary>
		/// Property to return whether this is a raw view or not.
		/// </summary>
		public bool IsRaw
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

            Resource.Graphics.Shaders.Unbind(this);

            Gorgon.Log.Print("Destroying shader resource view for {0}.",
                             LoggingLevel.Verbose,
                             Resource.Name);
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
            Gorgon.Log.Print("Creating shader resource view for {0}.", LoggingLevel.Verbose, Resource.Name);
            InitializeImpl();
        }

        #endregion

        #region Constructor/Destructor.

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderView"/> class.
        /// </summary>
        /// <param name="resource">The buffer to bind to the view.</param>
        /// <param name="format">The format of the view.</param>
        /// <param name="isRaw">TRUE if the view is raw, FALSE if not.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is NULL (Nothing in VB.Net).</exception>
        protected GorgonShaderView(GorgonResource resource, BufferFormat format, bool isRaw)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

	        IsRaw = isRaw;
            Resource = resource;
            Format = format;
            FormatInformation = GorgonBufferFormatInfo.GetInfo(Format);
        }
        #endregion
    }
}

