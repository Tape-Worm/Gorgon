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

using Gorgon.Diagnostics;
using Gorgon.UI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
    /// A shader resource view.
    /// </summary>
    /// <remarks>Use a shader resource view to allow a resource (or sub resource) to be bound to a shader.  When the resource is created with a typeless format, this will allow 
    /// the resource to be cast to any format within the same group.</remarks>
    public abstract class GorgonShaderView
		: GorgonView
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
        #endregion

        #region Methods.
		/// <summary>
		/// Function to clean up the resources used by the view.
		/// </summary>
		protected override void OnCleanUp()
		{
			if (D3DView == null)
			{
				return;
			}

			Resource.Graphics.Shaders.Unbind(this);

			GorgonApplication.Log.Print("Destroying shader resource view for {0}.",
							 LoggingLevel.Verbose,
							 Resource.Name);
			D3DView.Dispose();
			D3DView = null;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderView"/> class.
        /// </summary>
        /// <param name="resource">The buffer to bind to the view.</param>
        /// <param name="format">The format of the view.</param>
        protected GorgonShaderView(GorgonResource resource, BufferFormat format)
			: base(resource, format)
        {
        }
        #endregion
    }
}

