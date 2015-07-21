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
// Created: Wednesday, May 29, 2013 11:08:35 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A shader view for textures.
	/// </summary>
	/// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public sealed class GorgonTextureShaderView
		: GorgonShaderView
    {
        #region Variables.
	    private GorgonTexture1D _texture1D;             // 1D texture attached to this view.
	    private GorgonTexture2D _texture2D;             // 2D texture attached to this view.
        private GorgonTexture3D _texture3D;             // 3D texture attached to this view.
        #endregion

        #region Properties.
        /// <summary>
		/// Property to return the index of the first mip map in the resource to view.
		/// </summary>
		public int MipStart
		{
			get;
		}

		/// <summary>
		/// Property to return the number of mip maps in the resource to view.
		/// </summary>
		public int MipCount
		{
			get;
		}

		/// <summary>
		/// Property to return the starting array element to view.
		/// </summary>
		/// <remarks>This value has no meaning for a 3D texture and is ignored.</remarks>
		public int ArrayStart
		{
			get;
		}

		/// <summary>
		/// Property to return the number of array elements to view.
		/// </summary>
		/// <remarks>If the texture is a cube map, then this value must be a multiple of 6.
		/// <para>This value has no meaning for a 3D texture and is ignored.</para>
		/// </remarks>
		public int ArrayCount
		{
			get;
		}

        /// <summary>
        /// Property to return whether the texture is a texture cube or not.
        /// </summary>
	    public bool IsCube
	    {
	        get;
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the view description for a 1D texture.
		/// </summary>
		/// <param name="texture">Texture to build a view description for.</param>
		/// <returns>The shader view description.</returns>
		private D3D.ShaderResourceViewDescription GetDesc1D(GorgonTexture texture)
		{
			return new D3D.ShaderResourceViewDescription
				{
					Format = (Format)Format,
					Dimension = texture.Settings.ArrayCount > 1 ? ShaderResourceViewDimension.Texture1DArray 
						            : ShaderResourceViewDimension.Texture1D,
					Texture1DArray =
						{
							MipLevels = MipCount,
							MostDetailedMip = MipStart,
							ArraySize = ArrayCount,
							FirstArraySlice = ArrayStart
						}
				};
		}

		/// <summary>
		/// Function to retrieve the view description for a 2D texture.
		/// </summary>
		/// <param name="texture">Texture to build a view description for.</param>
		/// <returns>The shader view description.</returns>
		private D3D.ShaderResourceViewDescription GetDesc2D(GorgonTexture texture)
		{
			bool isMultiSampled = ((texture.Settings.Multisampling.Count > 1)
			                       || (texture.Settings.Multisampling.Quality > 0));

			if (!isMultiSampled)
			{
				return new D3D.ShaderResourceViewDescription
					{
						Format = (Format)Format,
						Dimension = texture.Settings.ArrayCount > 1
							            ? ShaderResourceViewDimension.Texture2DArray
							            : ShaderResourceViewDimension.Texture2D,
						Texture2DArray = 
							{
								MipLevels = MipCount,
								MostDetailedMip = MipStart,
								FirstArraySlice = ArrayStart,
								ArraySize = ArrayCount
							}
					};
			}

			return new D3D.ShaderResourceViewDescription
				{
					Format = (Format)Format,
					Dimension = texture.Settings.ArrayCount > 1
						            ? ShaderResourceViewDimension.Texture2DMultisampledArray
						            : ShaderResourceViewDimension.Texture2DMultisampled,
					Texture2DMSArray =
						{
							ArraySize = ArrayCount,
							FirstArraySlice = ArrayStart
						}
				};
		}

        /// <summary>
        /// Function to retrieve the view description for a 2D texture cube.
        /// </summary>
        /// <param name="texture">The texture to bind to the view.</param>
        /// <returns>The shader view description.</returns>
        /// <exception cref="GorgonException"></exception>
	    private D3D.ShaderResourceViewDescription GetDesc2DCube(GorgonTexture texture)
	    {
            bool isMultiSampled = ((texture.Settings.Multisampling.Count > 1)
                                   || (texture.Settings.Multisampling.Quality > 0));

	        if (isMultiSampled)
	        {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CANNOT_MULTISAMPLE_CUBE);
            }

            int cubeArrayCount = texture.Settings.ArrayCount / 6;

            return new D3D.ShaderResourceViewDescription
                   {
                       Format = (Format)Format,
                       Dimension =
                           cubeArrayCount > 1
                               ? ShaderResourceViewDimension.TextureCubeArray
                               : ShaderResourceViewDimension.TextureCube,
                       TextureCubeArray =
                       {
                           CubeCount = cubeArrayCount,
                           First2DArrayFace = ArrayStart,
                           MipLevels = MipCount,
                           MostDetailedMip = MipStart
                       }
                   };
	    }

		/// <summary>
		/// Function to retrieve the view description for a 3D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private D3D.ShaderResourceViewDescription GetDesc3D()
		{
			return new D3D.ShaderResourceViewDescription
				{
					Format = (Format)Format,
					Dimension = ShaderResourceViewDimension.Texture3D,
					Texture3D =
						{
							MostDetailedMip = MipStart,
							MipLevels = MipCount
						}
				};
		}


		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void OnInitialize()
		{
			D3D.ShaderResourceViewDescription desc;
			var texture = (GorgonTexture)Resource;

			GorgonApplication.Log.Print("Creating texture shader view for {0}.", LoggingLevel.Verbose, Resource.Name);

			// Build SRV description.
			switch(Resource.ResourceType)
			{
				case ResourceType.Texture1D:
					desc = GetDesc1D(texture);
					break;
				case ResourceType.Texture2D:
			        desc = IsCube ? GetDesc2DCube(texture) : GetDesc2D(texture);
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
				GorgonApplication.Log.Print("Gorgon resource view: Creating D3D 11 shader resource view.", LoggingLevel.Verbose);

				// Create our SRV.
				D3DView = new D3D.ShaderResourceView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
					{
						DebugName = Resource.ResourceType + " '" + texture.Name + "' Shader Resource View"
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
	    public static explicit operator GorgonTexture1D(GorgonTextureShaderView view)
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
        public static explicit operator GorgonTexture2D(GorgonTextureShaderView view)
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
        public static explicit operator GorgonTexture3D(GorgonTextureShaderView view)
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
        public static GorgonTexture1D ToTexture1D(GorgonTextureShaderView view)
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
        public static GorgonTexture2D ToTexture2D(GorgonTextureShaderView view)
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
        public static GorgonTexture3D ToTexture3D(GorgonTextureShaderView view)
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
		/// Initializes a new instance of the <see cref="GorgonTextureShaderView"/> class.
		/// </summary>
		/// <param name="texture">The texture to view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="firstMipLevel">The first mip level.</param>
		/// <param name="mipCount">The mip count.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <param name="arrayCount">The array count.</param>
		internal GorgonTextureShaderView(GorgonTexture texture,
		                                 BufferFormat format,
		                                 int firstMipLevel,
		                                 int mipCount,
		                                 int arrayIndex,
		                                 int arrayCount)
			: base(texture, format)
		{
			MipStart = firstMipLevel;
			MipCount = mipCount;
			ArrayStart = arrayIndex;
			ArrayCount = arrayCount;
		    IsCube = texture.Settings.ImageType == ImageType.ImageCube;

		    switch (texture.ResourceType)
		    {
		        case ResourceType.Texture1D:
		            _texture1D = (GorgonTexture1D)texture;
		            break;
                case ResourceType.Texture2D:
                    _texture2D = (GorgonTexture2D)texture;
		            break;
                case ResourceType.Texture3D:
                    _texture3D = (GorgonTexture3D)texture;
		            break;
		    }
		}
		#endregion
	}
}