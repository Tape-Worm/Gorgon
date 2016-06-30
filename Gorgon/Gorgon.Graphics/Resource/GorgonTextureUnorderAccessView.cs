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
// Created: Wednesday, May 29, 2013 11:08:40 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An unordered access texture view.
	/// </summary>
	/// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
	/// <para>Ordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
	/// have a format that is the same bit-depth and in the same group as its bound resource.</para>
	/// <para>Unlike a <see cref="Gorgon.Graphics.GorgonTextureShaderView">GorgonTextureShaderView</see>, only one unordered access view may be applied to a resource.</para>
	/// </remarks>
	public class GorgonTextureUnorderedAccessView
		: GorgonUnorderedAccessView
	{
        #region Variables.
        private GorgonTexture1D _texture1D;             // 1D texture attached to this view.
        private GorgonTexture2D _texture2D;             // 2D texture attached to this view.
        private GorgonTexture3D _texture3D;             // 3D texture attached to this view.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the mip map index to view.
		/// </summary>
		public int MipIndex
		{
			get;
		}

		/// <summary>
		/// Property to return the starting array element or depth slice to view.
		/// </summary>
		/// <remarks>For a 1D/2D texture, this value indicates an array index.  For a 3D texture, this value indicates a depth slice.</remarks>
		public int ArrayOrDepthStart
		{
			get;
		}

		/// <summary>
		/// Property to return the number of array indices or depth slices to view.
		/// </summary>
		/// <remarks>If the texture is a cube map, then this value must be a multiple of 6.
        /// <para>For a 1D/2D texture, this value indicates an array index.  For a 3D texture, this value indicates a depth slice.</para>
		/// </remarks>
		public int ArrayOrDepthCount
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the view settings.
		/// </summary>
		private void ValidateViewSettings(GorgonTexture_OLDEN texture)
		{
			if (Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNKNOWN_FORMAT);
			}

			if (FormatInformation.IsTypeless)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_NO_TYPELESS);
			}

			if ((texture.Settings.IsTextureCube) || (texture.Settings.Multisampling.Count > 1) || (texture.Settings.Multisampling.Quality > 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UAV_NOT_SUPPORTED);
			}

			// 3D textures don't use arrays.
			if (texture.ResourceType != ResourceType.Texture3D)
			{
				if ((ArrayOrDepthCount > texture.Settings.ArrayCount)
				    || (ArrayOrDepthCount + ArrayOrDepthStart > texture.Settings.ArrayCount)
				    || (ArrayOrDepthCount < 1))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_VIEW_ARRAY_COUNT_INVALID,
					                                        texture.Settings.ArrayCount));
				}

				if ((ArrayOrDepthStart >= texture.Settings.ArrayCount)
				    || (ArrayOrDepthStart < 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_VIEW_ARRAY_START_INVALID,
					                                        texture.Settings.ArrayCount));
				}
			}
			else
			{
				if ((ArrayOrDepthCount > texture.Settings.Depth)
					|| (ArrayOrDepthCount + ArrayOrDepthStart > texture.Settings.Depth)
					|| (ArrayOrDepthCount < 1))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_DEPTH_COUNT_INVALID,
															texture.Settings.Depth));
				}

				if ((ArrayOrDepthStart >= texture.Settings.Depth)
					|| (ArrayOrDepthStart < 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_DEPTH_START_INVALID,
															texture.Settings.Depth));
				}
			}

			if ((MipIndex > texture.Settings.MipCount) || (MipIndex < 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_VIEW_MIP_COUNT_INVALID,
				                                        texture.Settings.MipCount));
			}

			if ((MipIndex >= texture.Settings.MipCount)
			    || (MipIndex < 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_VIEW_MIP_START_INVALID,
				                                        texture.Settings.MipCount));
			}
		}

		/// <summary>
		/// Function to retrieve the view description for a 1D texture.
		/// </summary>
		/// <param name="texture">Texture to build a view description for.</param>
		/// <returns>The shader view description.</returns>
		private UnorderedAccessViewDescription GetDesc1D(GorgonTexture_OLDEN texture)
		{
			return new UnorderedAccessViewDescription
				{
					Format = (Format)Format,
					Dimension = texture.Settings.ArrayCount > 1 ? UnorderedAccessViewDimension.Texture1DArray
						            : UnorderedAccessViewDimension.Texture1D,
					Texture1DArray =
						{
							MipSlice = MipIndex,
							ArraySize = ArrayOrDepthCount,
							FirstArraySlice = ArrayOrDepthStart
						}
				};
		}

		/// <summary>
		/// Function to retrieve the view description for a 2D texture.
		/// </summary>
		/// <param name="texture">Texture to build a view description for.</param>
		/// <returns>The shader view description.</returns>
		private UnorderedAccessViewDescription GetDesc2D(GorgonTexture_OLDEN texture)
		{
			return new UnorderedAccessViewDescription
				{
					Format = (Format)Format,
					Dimension = texture.Settings.ArrayCount > 1
						            ? UnorderedAccessViewDimension.Texture2DArray
						            : UnorderedAccessViewDimension.Texture2D,
					Texture2DArray =
						{
							MipSlice =  MipIndex,
							FirstArraySlice = ArrayOrDepthStart,
							ArraySize = ArrayOrDepthCount
						}
				};
		}

		/// <summary>
		/// Function to retrieve the view description for a 3D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private UnorderedAccessViewDescription GetDesc3D()
		{
			return new UnorderedAccessViewDescription
				{
					Format = (Format)Format,
					Dimension = UnorderedAccessViewDimension.Texture3D,
					Texture3D =
						{
							MipSlice = MipIndex,
							FirstWSlice = ArrayOrDepthStart,
							WSize = ArrayOrDepthCount
						}
				};
		}


		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void OnInitialize()
		{
			UnorderedAccessViewDescription desc;
			var texture = (GorgonTexture_OLDEN)Resource;

			GorgonApplication.Log.Print("Creating texture unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);

			ValidateViewSettings(texture);

			// Build SRV description.
			switch (Resource.ResourceType)
			{
				case ResourceType.Texture1D:
					desc = GetDesc1D(texture);
					break;
				case ResourceType.Texture2D:
					desc = GetDesc2D(texture);
					break;
				case ResourceType.Texture3D:
					desc = GetDesc3D();
					break;
				default:
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, Resource.ResourceType));
			}

			try
			{
				GorgonApplication.Log.Print("Gorgon resource view: Creating D3D 11 unordered access resource view.", LoggingLevel.Verbose);

				// Create our SRV.
				D3DView = new UnorderedAccessView(Resource.VideoDevice.D3DDevice(), Resource.D3DResource, desc)
					{
						DebugName = Resource.ResourceType + " '" + texture.Name + "' Unordered Access Resource View"
					};
			}
			catch (SharpDXException sDXEx)
			{
				if ((uint)sDXEx.ResultCode.Code == 0x80070057)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_VIEW_CANNOT_CAST_FORMAT,
					                                        texture.Settings.Format,
					                                        Format));
				}
			}
		}

        /// <summary>
        /// Explicit operator to convert this view into its attached 1D texture.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The texture attached to the view.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the view is not attached to a 1D texture.</exception>
        public static explicit operator GorgonTexture1D(GorgonTextureUnorderedAccessView view)
        {
            if (view._texture1D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TEXTURE, "1D"));
            }

            return view._texture1D;
        }

        /// <summary>
        /// Explicit operator to convert this view into its attached 2D texture.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The texture attached to the view.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the view is not attached to a 2D texture.</exception>
        public static explicit operator GorgonTexture2D(GorgonTextureUnorderedAccessView view)
        {
            if (view._texture2D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TEXTURE, "2D"));
            }

            return view._texture2D;
        }

        /// <summary>
        /// Explicit operator to convert this view into its attached 3D texture.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The texture attached to the view.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the view is not attached to a 3D texture.</exception>
        public static explicit operator GorgonTexture3D(GorgonTextureUnorderedAccessView view)
        {
            if (view._texture3D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TEXTURE, "3D"));
            }

            return view._texture3D;
        }

        /// <summary>
        /// Function to convert this view into its attached 1D texture.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The texture attached to the view.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the view is not attached to a 1D texture.</exception>
        public static GorgonTexture1D ToTexture1D(GorgonTextureUnorderedAccessView view)
        {
            if (view._texture1D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TEXTURE, "1D"));
            }

            return view._texture1D;
        }

        /// <summary>
        /// Function to convert this view into its attached 2D texture.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The texture attached to the view.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the view is not attached to a 2D texture.</exception>
        public static GorgonTexture2D ToTexture2D(GorgonTextureUnorderedAccessView view)
        {
            if (view._texture2D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TEXTURE, "2D"));
            }

            return view._texture2D;
        }

        /// <summary>
        /// Function to convert this view into its attached 3D texture.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The texture attached to the view.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the view is not attached to a 3D texture.</exception>
        public static GorgonTexture3D ToTexture3D(GorgonTextureUnorderedAccessView view)
        {
            if (view._texture3D == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TEXTURE, "3D"));
            }

            return view._texture3D;
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextureUnorderedAccessView"/> class.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="format">The format.</param>
		/// <param name="mipIndex">The first mip level.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <param name="arrayCount">The array count.</param>
		internal GorgonTextureUnorderedAccessView(GorgonResource resource, BufferFormat format, int mipIndex, 
		                                        int arrayIndex, int arrayCount)
			: base(resource, format)
		{
			MipIndex = mipIndex;
			ArrayOrDepthStart = arrayIndex;
			ArrayOrDepthCount = arrayCount;

            switch (resource.ResourceType)
            {
                case ResourceType.Texture1D:
                    _texture1D = (GorgonTexture1D)resource;
                    break;
                case ResourceType.Texture2D:
                    _texture2D = (GorgonTexture2D)resource;
                    break;
                case ResourceType.Texture3D:
                    _texture3D = (GorgonTexture3D)resource;
                    break;
            }
		}
		#endregion
	}
}