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
// Created: Saturday, June 8, 2013 4:53:00 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using D3D = SharpDX.Direct3D11;
using GI = SharpDX.DXGI;


namespace Gorgon.Graphics
{
    /// <summary>
    /// Flags to determine how the view will be handled.
    /// </summary>
    [Flags]
    public enum DepthStencilViewFlags
    {
        /// <summary>
        /// No view flags.
        /// </summary>
        /// <remarks>This will make it so the depth/stencil view can't be bound to a shader view simultaneously.</remarks>
        None = D3D.DepthStencilViewFlags.None,
        /// <summary>
        /// Allow read-only access to the depth portion of the resource.
        /// </summary>
        /// <remarks>This will allow the depth/stencil buffer to be bound as a depth buffer and as a shader view at the same time.</remarks>
        DepthReadOnly = D3D.DepthStencilViewFlags.ReadOnlyDepth,
        /// <summary>
        /// Allow read-only access to the stencil porition of the resource.
        /// </summary>
        /// <remarks>This will allow the depth/stencil buffer to be bound as a stencil buffer and as a shader view at the same time.</remarks>
        StencilReadOnly = D3D.DepthStencilViewFlags.ReadOnlyStencil
    }
	/// <summary>
	/// A depth/stencil view to allow a texture to be bound to the pipeline as a depth/stencil buffer.
	/// </summary>
	public class GorgonDepthStencilView
		: GorgonView
    {
        #region Variables.
	    private GorgonDepthStencil1D _depth1D;             // 1D depth/stencil attached to this view.
	    private GorgonDepthStencil2D _depth2D;             // 2D depth/stencil attached to this view.
        #endregion

        #region Properties.
        /// <summary>
		/// Property to return the Direct3D depth/stencil view.
		/// </summary>
		internal D3D.DepthStencilView D3DView
		{
			get;
			set;
		}

        /// <summary>
        /// Property to return the flags for this view.
        /// </summary>
        /// <remarks>This will allow the depth/stencil buffer to be read simultaneously from the depth/stencil view and from a shader view.  It is not normally possible to bind a view of a resource to 2 parts of the 
        /// pipeline at the same time.  However, using the flags provided, read-only access may be granted to a part of the resource (depth or stencil) or all of it for all parts of the pipline.  This would bind 
        /// the depth/stencil as a read-only view and make it a read-only view accessible to shaders.
        /// <para>This is only valid if the resource allows shader access.</para>
        /// <para>This is only valid on video devices with a feature level of SM5 or better.</para>
        /// </remarks>
        public DepthStencilViewFlags Flags
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the mip slice to use for the view.
		/// </summary>
		public int MipSlice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of array indices to use in the view.
		/// </summary>
		public int ArrayCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the first array index or depth slice to use in the view.
		/// </summary>
		public int FirstArrayIndex
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the description for a 1D depth/stencil view.
		/// </summary>
		/// <returns>The direct 3D 1D depth/stencil view description.</returns>
		private D3D.DepthStencilViewDescription GetDesc1D()
		{
			var target1D = (GorgonDepthStencil1D)Resource;

			// Set up for arrayed and multisampled texture.
			if (target1D.Settings.ArrayCount > 1)
			{
				return new D3D.DepthStencilViewDescription
				{
					Format = (GI.Format)Format,
					Dimension = D3D.DepthStencilViewDimension.Texture1DArray,
					Texture1DArray =
					{
						MipSlice = MipSlice,
						FirstArraySlice = FirstArrayIndex,
						ArraySize = ArrayCount
					}
				};
			}

			return new D3D.DepthStencilViewDescription
			{
				Format = (GI.Format)Format,
				Dimension = D3D.DepthStencilViewDimension.Texture1D,
				Texture1D =
				{
					MipSlice = MipSlice
				}
			};
		}

		/// <summary>
		/// Function to retrieve the description for a 2D depth/stencil view.
		/// </summary>
		/// <returns>The direct 3D 2D depth/stencil view description.</returns>
		private D3D.DepthStencilViewDescription GetDesc2D()
		{
			var target2D = (GorgonDepthStencil2D)Resource;
			bool isMultiSampled = target2D.Settings.Multisampling != GorgonMultisampling.NoMultiSampling;

			// Set up for arrayed and multisampled texture.
			if (target2D.Settings.ArrayCount > 1)
			{
				return new D3D.DepthStencilViewDescription
				{
					Format = (GI.Format)Format,
					Dimension = isMultiSampled
									? D3D.DepthStencilViewDimension.Texture2DMultisampledArray
									: D3D.DepthStencilViewDimension.Texture2DArray,
					Texture2DArray =
					{
						MipSlice = isMultiSampled ? FirstArrayIndex : MipSlice,
						FirstArraySlice = isMultiSampled ? ArrayCount : FirstArrayIndex,
						ArraySize = isMultiSampled ? 0 : ArrayCount
					}
				};
			}

			return new D3D.DepthStencilViewDescription
			{
				Format = (GI.Format)Format,
				Dimension = isMultiSampled
								? D3D.DepthStencilViewDimension.Texture2DMultisampled
								: D3D.DepthStencilViewDimension.Texture2D,
				Texture2D =
				{
					MipSlice = isMultiSampled ? 0 : MipSlice
				}
			};
		}

		/// <summary>
		/// Function to perform clean up of the resources used by the view.
		/// </summary>
		protected override void OnCleanUp()
		{
			if (D3DView == null)
			{
				return;
			}

			// Unbind the depth/stencil view.
			if (Resource.Graphics.Output.DepthStencilView == this)
			{
				Resource.Graphics.Output.DepthStencilView = null;
			}

			GorgonApplication.Log.Print("Destroying depth/stencil view for {0}.",
							 LoggingLevel.Verbose,
							 Resource.Name);
			D3DView.Dispose();
			D3DView = null;
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		protected override void OnInitialize()
		{
			D3D.DepthStencilViewDescription desc = default(D3D.DepthStencilViewDescription);

			desc.Dimension = D3D.DepthStencilViewDimension.Unknown;

			switch (Resource.ResourceType)
			{
				case ResourceType.Texture1D:
					desc = GetDesc1D();
					break;
				case ResourceType.Texture2D:
					desc = GetDesc2D();
					break;
			}

			if (desc.Dimension == D3D.DepthStencilViewDimension.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
			}

			D3DView = new D3D.DepthStencilView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
			{
				DebugName = string.Format("{0} '{1}' Depth Stencil View", Resource.ResourceType, Resource.Name)
			};
		}

        /// <summary>
        /// Function to clear the depth portion of the depth/stencil buffer.
        /// </summary>
        /// <param name="depthValue">Value to fill the depth buffer with.</param>
        /// <param name="stencil">Value to fill the stencil buffer with.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the depth/stencil buffer.</param>
        /// <remarks>
        /// This method will clear both the stencil and depth buffer.
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the depth/stencil buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the depth/stencil buffer.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the depth/stencil buffer because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para></para>
        /// </remarks>
        public void Clear(float depthValue, byte stencil, GorgonGraphics deferred = null)
        {
            var flags = (D3D.DepthStencilClearFlags)0;

            if (FormatInformation.HasDepth)
            {
                flags |= D3D.DepthStencilClearFlags.Depth;
            }

            if (FormatInformation.HasStencil)
            {
                flags |= D3D.DepthStencilClearFlags.Stencil;
            }

            if (deferred != null)
            {
                deferred.Context.ClearDepthStencilView(D3DView, flags, depthValue, stencil);
                return;
            }

            Resource.Graphics.Context.ClearDepthStencilView(D3DView, flags, depthValue, stencil);
        }
        
		/// <summary>
		/// Function to clear the depth portion of the depth/stencil buffer.
		/// </summary>
		/// <param name="depthValue">Value to fill the depth buffer with.</param>
		/// <param name="deferred">[Optional] A deferred context to use when clearing the depth/stencil buffer.</param>
		/// <remarks>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the depth/stencil buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the depth/stencil buffer.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the depth/stencil buffer because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </remarks>
		public void ClearDepth(float depthValue, GorgonGraphics deferred = null)
		{
		    if (!FormatInformation.HasDepth)
		    {
		        return;
		    }

		    if (deferred != null)
		    {
		        deferred.Context.ClearDepthStencilView(D3DView, D3D.DepthStencilClearFlags.Depth, depthValue, 0);
		        return;
		    }

		    Resource.Graphics.Context.ClearDepthStencilView(D3DView, D3D.DepthStencilClearFlags.Depth, depthValue, 0);
		}

	    /// <summary>
		/// Function to clear the stencil portion of the depth/stencil buffer.
		/// </summary>
		/// <param name="stencilValue">Value to fill the stencil buffer with.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the depth/stencil buffer.</param>
        /// <remarks>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the depth/stencil buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the depth/stencil buffer.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the depth/stencil buffer because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </remarks>
        public void ClearStencil(byte stencilValue, GorgonGraphics deferred = null)
		{
	        if (!FormatInformation.HasStencil)
	        {
	            return;
	        }

	        if (deferred != null)
	        {
	            deferred.Context.ClearDepthStencilView(D3DView,
	                                                   D3D.DepthStencilClearFlags.Stencil,
	                                                   1.0f,
                                                       stencilValue);
	            return;
	        }

	        Resource.Graphics.Context.ClearDepthStencilView(D3DView,
	                                                        D3D.DepthStencilClearFlags.Stencil,
	                                                        1.0f,
	                                                        stencilValue);
		}

		/// <summary>
		/// Explicit operator to retrieve the 1D depth/stencil buffer associated with this view.
		/// </summary>
		/// <param name="view">The view to evaluate.</param>
		/// <returns>The 1D depth/stencil buffer associated with this view.</returns>
		public static explicit operator GorgonDepthStencil1D(GorgonDepthStencilView view)
		{
		    if (view._depth1D == null)
		    {
		        throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_DEPTHSTENCIL, "1D"));
		    }

		    return view._depth1D;
		}

		/// <summary>
		/// Explicit operator to retrieve the 2D depth/stencil buffer associated with this view.
		/// </summary>
		/// <param name="view">The view to evaluate.</param>
		/// <returns>The 2D depth/stencil buffer associated with this view.</returns>
		public static explicit operator GorgonDepthStencil2D(GorgonDepthStencilView view)
		{
            if (view._depth2D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_DEPTHSTENCIL, "2D"));
            }

            return view._depth2D;
        }

        /// <summary>
        /// Function to retrieve the 1D depth/stencil buffer associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 1D depth/stencil buffer associated with this view.</returns>
        public static GorgonDepthStencil1D ToDepthStencil1D(GorgonDepthStencilView view)
        {
            if (view._depth1D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_DEPTHSTENCIL, "1D"));
            }

            return view._depth1D;
        }

        /// <summary>
        /// Function to retrieve the 2D depth/stencil buffer associated with this view.
        /// </summary>
        /// <param name="view">The view to evaluate.</param>
        /// <returns>The 2D depth/stencil buffer associated with this view.</returns>
        public static GorgonDepthStencil2D ToDepthStencil2D(GorgonDepthStencilView view)
        {
            if (view._depth2D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_DEPTHSTENCIL, "2D"));
            }

            return view._depth2D;
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="mipSlice">The mip level to use for the view.</param>
		/// <param name="firstArrayIndex">The first array index to use for the view.</param>
		/// <param name="arrayCount">The number of array indices to use for the view.</param>
		/// <param name="flags">Depth/stencil view flags.</param>
		internal GorgonDepthStencilView(GorgonResource resource, BufferFormat format, int mipSlice, int firstArrayIndex, int arrayCount, DepthStencilViewFlags flags)
			: base(resource, format)
		{
			MipSlice = mipSlice;
			FirstArrayIndex = firstArrayIndex;
			ArrayCount = arrayCount;
		    Flags = flags;

		    switch (resource.ResourceType)
		    {
		        case ResourceType.Texture1D:
		            _depth1D = (GorgonDepthStencil1D)resource;
		            break;
                case ResourceType.Texture2D:
		            _depth2D = (GorgonDepthStencil2D)resource;
		            break;
		    }
		}
		#endregion
	}
}
