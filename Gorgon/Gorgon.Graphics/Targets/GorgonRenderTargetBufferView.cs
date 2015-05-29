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
// Created: Friday, July 19, 2013 10:13:11 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Properties;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A view to allow buffer based render targets to be bound to the pipeline.
    /// </summary>
    /// <remarks>A render target view is what allows a render target to be bound to the pipeline.  By default, every render target has a default view assigned which encompasses the entire resource.  
    /// This allows the resource to be bound to the pipeline directly via a cast operation.  However, in some instances, only a section of the resource may need to be assigned to the pipeline (e.g. 
    /// a particular array index in a 2D array texture).  In this case, the user can define a render target view to only use that one array index and bind that to the pipeline.</remarks>
    public class GorgonRenderTargetBufferView
        : GorgonRenderTargetView
    {
        #region Properties.
        /// <summary>
        /// Property to return the first element in the buffer to use in the view.
        /// </summary>
        public int FirstElement
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of elements in the buffer to use in the view.
        /// </summary>
        public int ElementCount
        {
            get;
            private set;
        }
        #endregion

        #region Methods.

        /// <summary>
        /// Function to perform initialization of the view.
        /// </summary>
        protected override void OnInitialize()
        {
            if (Resource.ResourceType != ResourceType.Buffer)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
            }

            var desc = new D3D.RenderTargetViewDescription
            {
                Dimension = D3D.RenderTargetViewDimension.Buffer,
                Format = (GI.Format)Format,
                Buffer = new D3D.RenderTargetViewDescription.BufferResource
                {
                    FirstElement = FirstElement,
                    ElementCount = ElementCount
                }
            };

            D3DView = new D3D.RenderTargetView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
            {
                DebugName = string.Format("{0} '{1}' Render Target View", Resource.ResourceType, Resource.Name)
            };
        }

        /// <summary>
        /// Function to retrieve the render target buffer associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The render target buffer associated with this view.</returns>
        public static GorgonRenderTargetBuffer ToRenderTargetBuffer(GorgonRenderTargetBufferView view)
        {
            return view == null ? null : (GorgonRenderTargetBuffer)view.Resource;
        }

        /// <summary>
        /// Implicit operator to retrieve the render target buffer associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The render target buffer associated with this view.</returns>
        public static implicit operator GorgonRenderTargetBuffer(GorgonRenderTargetBufferView view)
        {
            return view == null ? null : (GorgonRenderTargetBuffer)view.Resource;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTargetBufferView"/> class.
        /// </summary>
        /// <param name="target">The render target to bind.</param>
        /// <param name="format">The format of the render target view.</param>
        /// <param name="firstElement">The first element in the buffer to map to the view.</param>
        /// <param name="elementCount">The number of elements in the buffer to map to the view.</param>
        internal GorgonRenderTargetBufferView(GorgonResource target, BufferFormat format, int firstElement, int elementCount)
            : base(target, format)
        {
            FirstElement = firstElement;
            ElementCount = elementCount;
        }
        #endregion
    }
}