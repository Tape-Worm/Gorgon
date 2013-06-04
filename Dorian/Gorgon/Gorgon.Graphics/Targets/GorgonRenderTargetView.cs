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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A view to allow render targets to be bound to the pipeline.
    /// </summary>
    /// <remarks>A render target view is what allows a render target to be bound to the pipeline.  By default, every render target has a default view assigned which encompasses the entire resource.  
    /// This allows the resource to be bound to the pipeline directly via a cast operation.  However, in some instances, only a section of the resource may need to be assigned to the pipeline (e.g. 
    /// a particular array index in a 2D array texture).  In this case, the user can define a render target view to only use that one array index and bind that to the pipeline.</remarks>
    public class GorgonRenderTargetView
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the render target view.
        /// </summary>
        internal D3D.RenderTargetView D3DView
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the resource that is bound with this view.
        /// </summary>
        public GorgonResource Resource
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the format of the render target view.
        /// </summary>
        public BufferFormat Format
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clean up the render target view.
        /// </summary>
        internal void CleanUp()
        {
            if (D3DView == null)
            {
                return;
            }

            
        }
        #endregion

        #region Constructor/Destructor.

        #endregion
    }
}
