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
using GI = SharpDX.DXGI;
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

			Resource.Graphics.Output.RenderTargets.Unbind(this);

			Gorgon.Log.Print("Destroying render target view for {0}.",
							 LoggingLevel.Verbose,
							 Resource.Name);
			D3DView.Dispose();
	        D3DView = null;
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

	/// <summary>
	/// A view to allow texture based render targets to be bound to the pipeline.
	/// </summary>
	/// <remarks>A render target view is what allows a render target to be bound to the pipeline.  By default, every render target has a default view assigned which encompasses the entire resource.  
	/// This allows the resource to be bound to the pipeline directly via a cast operation.  However, in some instances, only a section of the resource may need to be assigned to the pipeline (e.g. 
	/// a particular array index in a 2D array texture).  In this case, the user can define a render target view to only use that one array index and bind that to the pipeline.</remarks>
	public class GorgonRenderTargetTextureView
		: GorgonRenderTargetView
	{
		#region Variables.
		private readonly GorgonRenderTarget1D _target;				// 1D render target to bind.
		#endregion

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
        /// <remarks>For a 1D/2D texture, this value indicates an array index.  For a 3D texture, this value indicates a depth slice.</remarks>
		public int ArrayOrDepthCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the first array index or depth slice to use in the view.
		/// </summary>
        /// <remarks>For a 1D/2D texture, this value indicates an array index.  For a 3D texture, this value indicates a depth slice.</remarks>
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
		private D3D.RenderTargetViewDescription GetDesc()
		{
			// Set up for arrayed and multisampled texture.
			if (_target.Settings.ArrayCount > 1)
			{
				return new D3D.RenderTargetViewDescription
				{
					Format = (GI.Format)Format,
					Dimension = D3D.RenderTargetViewDimension.Texture1DArray,
					Texture1DArray =
					{
						MipSlice = MipSlice,
						FirstArraySlice = FirstArrayIndex,
						ArraySize = ArrayCount
					}
				};
			}

			return new D3D.RenderTargetViewDescription
			{
				Format = (GI.Format)Format,
				Dimension = D3D.RenderTargetViewDimension.Texture1D,
				Texture1D =
				{
					MipSlice = MipSlice
				}
			};
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		protected override void OnInitialize()
		{
			var desc = GetDesc();

			D3DView = new D3D.RenderTargetView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
				{
					DebugName = string.Format("{0} '{1}' Render Target View", Resource.ResourceType, Resource.Name)
				};

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
		internal GorgonRenderTargetTextureView(GorgonRenderTarget1D target, BufferFormat format, int mipSlice, int arrayIndex, int arrayCount)
			: base(target, format)
		{
			_target = target;
			MipSlice = mipSlice;
			FirstArrayOrDepthIndex = arrayIndex;
			ArrayOrDepthCount = arrayCount;
		}
		#endregion
	}
}
