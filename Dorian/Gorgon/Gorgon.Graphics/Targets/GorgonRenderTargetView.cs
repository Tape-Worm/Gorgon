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
// Created: Tuesday, June 04, 2013 7:51:42 AM
// 
#endregion

using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A view to allow render targets to be bound to the pipeline.
    /// </summary>
    /// <remarks>A render target view is what allows a render target to be bound to the pipeline.  By default, every render target has a default view assigned which encompasses the entire resource.  
    /// This allows the resource to be bound to the pipeline directly via a cast operation.  However, in some instances, only a section of the resource may need to be assigned to the pipeline (e.g. 
    /// a particular array index in a 2D array texture).  In this case, the user can define a render target view to only use that one array index and bind that to the pipeline.</remarks>
    public abstract class GorgonRenderTargetView
		: GorgonView
    {
        #region Properties.
        /// <summary>
        /// Property to return the render target view.
        /// </summary>
        internal D3D.RenderTargetView D3DView
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clean up the render target view.
        /// </summary>
        protected override void OnCleanUp()
        {
            if (D3DView == null)
            {
                return;
            }

			Resource.Graphics.Output.Unbind(this);

			Gorgon.Log.Print("Destroying render target view for {0}.",
							 LoggingLevel.Verbose,
							 Resource.Name);
			D3DView.Dispose();
	        D3DView = null;
        }

        /// <summary>
        /// Function to clear the render target view.
        /// </summary>
        /// <param name="color">Color used to clear the render target view.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the depth/stencil buffer.</param>
        /// <remarks>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </remarks>
        public void Clear(GorgonColor color, GorgonGraphics deferred = null)
        {
            if (deferred != null)
            {
                deferred.Context.ClearRenderTargetView(D3DView, color.SharpDXColor4);
                return;
            }

            Resource.Graphics.Context.ClearRenderTargetView(D3DView, color.SharpDXColor4);
        }
        #endregion

        #region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		protected GorgonRenderTargetView(GorgonResource resource, BufferFormat format)
			: base(resource, format)
		{
		}
        #endregion
    }
}
