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
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A view to allow texture based render targets to be bound to the pipeline.
    /// </summary>
    /// <remarks>A render target view is what allows a render target to be bound to the pipeline.  By default, every render target has a default view assigned which encompasses the entire resource.  
    /// This allows the resource to be bound to the pipeline directly via a cast operation.  However, in some instances, only a section of the resource may need to be assigned to the pipeline (e.g. 
    /// a particular array index in a 2D array texture).  In this case, the user can define a render target view to only use that one array index and bind that to the pipeline.</remarks>
    public class GorgonRenderTargetTextureView
        : GorgonRenderTargetView
    {
        #region Properties.
        /// <summary>
        /// Property to return the mip slice to use for the view.
        /// </summary>
        public int MipSlice
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of array indices or depth slices to use in the view.
        /// </summary>
        /// <remarks>For a 1D/2D render target, this value indicates an array index.  For a 3D render target, this value indicates a depth slice.</remarks>
        public int ArrayOrDepthCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the first array index or depth slice to use in the view.
        /// </summary>
        /// <remarks>For a 1D/2D render target, this value indicates an array index.  For a 3D render target, this value indicates a depth slice.</remarks>
        public int FirstArrayOrDepthIndex
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <returns>The view description.</returns>
        private D3D.RenderTargetViewDescription GetDesc1D()
        {
            var target1D = (GorgonRenderTarget1D)Resource;

            // Set up for arrayed and multisampled texture.
            if (target1D.Settings.ArrayCount > 1)
            {
                return new D3D.RenderTargetViewDescription
                {
                    Format = (Format)Format,
                    Dimension = D3D.RenderTargetViewDimension.Texture1DArray,
                    Texture1DArray =
                    {
                        MipSlice = MipSlice,
                        FirstArraySlice = FirstArrayOrDepthIndex,
                        ArraySize = ArrayOrDepthCount
                    }
                };
            }

            return new D3D.RenderTargetViewDescription
            {
                Format = (Format)Format,
                Dimension = D3D.RenderTargetViewDimension.Texture1D,
                Texture1D =
                {
                    MipSlice = MipSlice
                }
            };
        }

        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <returns>The view description.</returns>
        private D3D.RenderTargetViewDescription GetDesc2D()
        {
            var target2D = (GorgonRenderTarget2D)Resource;
            bool isMultiSampled = target2D.Settings.Multisampling != GorgonMultisampling.NoMultiSampling;

            // Set up for arrayed and multisampled texture.
            if (target2D.Settings.ArrayCount > 1)
            {
                return new D3D.RenderTargetViewDescription
                {
                    Format = (Format)Format,
                    Dimension = isMultiSampled
                        ? D3D.RenderTargetViewDimension.Texture2DMultisampledArray
                        : D3D.RenderTargetViewDimension.Texture2DArray,
                    Texture2DArray =
                    {
                        MipSlice = isMultiSampled ? FirstArrayOrDepthIndex : MipSlice,
                        FirstArraySlice = isMultiSampled ? ArrayOrDepthCount : FirstArrayOrDepthIndex,
                        ArraySize = isMultiSampled ? 0 : ArrayOrDepthCount
                    }
                };
            }

            return new D3D.RenderTargetViewDescription
            {
                Format = (Format)Format,
                Dimension = isMultiSampled
                    ? D3D.RenderTargetViewDimension.Texture2DMultisampled
                    : D3D.RenderTargetViewDimension.Texture2D,
                Texture2D =
                {
                    MipSlice = isMultiSampled ? 0 : MipSlice
                }
            };
        }

        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <returns>The view description.</returns>
        private D3D.RenderTargetViewDescription GetDesc3D()
        {
            return new D3D.RenderTargetViewDescription
            {
                Format = (Format)Format,
                Dimension = D3D.RenderTargetViewDimension.Texture3D,
                Texture3D =
                {
                    MipSlice = MipSlice,
                    FirstDepthSlice = FirstArrayOrDepthIndex,
                    DepthSliceCount = ArrayOrDepthCount
                }
            };
        }

        /// <summary>
        /// Function to perform initialization of the view.
        /// </summary>
        protected override void OnInitialize()
        {
            D3D.RenderTargetViewDescription desc = default(D3D.RenderTargetViewDescription);

            desc.Dimension = D3D.RenderTargetViewDimension.Unknown;

            switch (Resource.ResourceType)
            {
                case ResourceType.Texture1D:
                    desc = GetDesc1D();
                    break;
                case ResourceType.Texture2D:
                    desc = GetDesc2D();
                    break;
                case ResourceType.Texture3D:
                    desc = GetDesc3D();
                    break;
            }

            if (desc.Dimension == D3D.RenderTargetViewDimension.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
            }

            D3DView = new D3D.RenderTargetView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
            {
                DebugName = string.Format("{0} '{1}' Render Target View", Resource.ResourceType, Resource.Name)
            };
        }

        /// <summary>
        /// Function to retrieve the 1D render target associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 1D render target associated with this view.</returns>
        public static GorgonRenderTarget1D ToRenderTarget1D(GorgonRenderTargetTextureView view)
        {
            return view == null ? null : (GorgonRenderTarget1D)view.Resource;
        }

        /// <summary>
        /// Implicit operator to retrieve the 1D render target associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 1D render target associated with this view.</returns>
        public static implicit operator GorgonRenderTarget1D(GorgonRenderTargetTextureView view)
        {
            return view == null ? null : (GorgonRenderTarget1D)view.Resource;
        }

        /// <summary>
        /// Function to retrieve the 2D render target associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 2D render target associated with this view.</returns>
        public static GorgonRenderTarget2D ToRenderTarget2D(GorgonRenderTargetTextureView view)
        {
            return view == null ? null : (GorgonRenderTarget2D)view.Resource;
        }

        /// <summary>
        /// Implicit operator to retrieve the 2D render target associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 2D render target associated with this view.</returns>
        public static implicit operator GorgonRenderTarget2D(GorgonRenderTargetTextureView view)
        {
            return view == null ? null : (GorgonRenderTarget2D)view.Resource;
        }

        /// <summary>
        /// Function to retrieve the 3D render target associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 3D render target associated with this view.</returns>
        public static GorgonRenderTarget3D ToRenderTarget3D(GorgonRenderTargetTextureView view)
        {
            return view == null ? null : (GorgonRenderTarget3D)view.Resource;
        }

        /// <summary>
        /// Implicit operator to retrieve the 3D render target associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 3D render target associated with this view.</returns>
        public static implicit operator GorgonRenderTarget3D(GorgonRenderTargetTextureView view)
        {
            return view == null ? null : (GorgonRenderTarget3D)view.Resource;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTargetTextureView"/> class.
        /// </summary>
        /// <param name="target">The render target to bind.</param>
        /// <param name="format">The format of the render target view.</param>
        /// <param name="mipSlice">The mip slice to use in the view.</param>
        /// <param name="arrayIndex">The first array index to use in the view.</param>
        /// <param name="arrayCount">The number of array indices to use in the view.</param>
        internal GorgonRenderTargetTextureView(GorgonResource target, BufferFormat format, int mipSlice, int arrayIndex, int arrayCount)
            : base(target, format)
        {
            MipSlice = mipSlice;
            FirstArrayOrDepthIndex = arrayIndex;
            ArrayOrDepthCount = arrayCount;
        }
        #endregion
    }
}