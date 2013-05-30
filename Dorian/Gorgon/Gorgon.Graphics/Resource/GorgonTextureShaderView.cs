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

using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A shader view for textures.
	/// </summary>
	/// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public sealed class GorgonTextureShaderView
		: GorgonShaderView
	{
		#region Properties.
		/// <summary>
		/// Property to return the index of the first mip map in the resource to view.
		/// </summary>
		public int MipStart
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of mip maps in the resource to view.
		/// </summary>
		public int MipCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the starting array element to view.
		/// </summary>
		/// <remarks>This value has no meaning for a 3D texture and is ignored.</remarks>
		public int ArrayStart
		{
			get;
			private set;
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
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the view settings.
		/// </summary>
		private void ValidateViewSettings(GorgonTexture texture)
		{
			if (Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNKNOWN_FORMAT);
			}

			if (FormatInformation.IsTypeless)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_NO_TYPELESS);
			}

			if ((texture.Settings.Format != Format) && (texture.FormatInformation.Group != FormatInformation.Group))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_VIEW_FORMAT_GROUP_INVALID,
				                                        texture.Settings.Format,
				                                        Format));
			}

			// 3D textures don't use arrays.
			if (texture.ResourceType != ResourceType.Texture3D)
			{
				if ((ArrayCount > texture.Settings.ArrayCount)
				    || (ArrayCount + ArrayStart > texture.Settings.ArrayCount)
				    || (ArrayCount < 1))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_VIEW_ARRAY_COUNT_INVALID,
					                                        texture.Settings.ArrayCount));
				}

				if ((ArrayStart >= texture.Settings.ArrayCount)
				    || (ArrayStart < 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_VIEW_ARRAY_START_INVALID,
					                                        texture.Settings.ArrayCount));
				}
			}

			if ((MipCount > texture.Settings.MipCount) || (MipStart + MipCount > texture.Settings.MipCount)
			    || (MipCount < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_VIEW_MIP_COUNT_INVALID,
				                                        texture.Settings.MipCount));
			}

			if ((MipStart >= texture.Settings.MipCount)
			    || (MipStart < 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_VIEW_MIP_START_INVALID,
				                                        texture.Settings.MipCount));
			}

			if ((texture.Settings.IsTextureCube)
			    && ((ArrayCount % 6) != 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CUBE_ARRAY_SIZE_INVALID);
			}
		}

		/// <summary>
		/// Function to retrieve the view description for a 1D texture.
		/// </summary>
		/// <param name="texture">Texture to build a view description for.</param>
		/// <returns>The shader view description.</returns>
		private SharpDX.Direct3D11.ShaderResourceViewDescription GetDesc1D(GorgonTexture texture)
		{
			return new SharpDX.Direct3D11.ShaderResourceViewDescription
				{
					Format = (SharpDX.DXGI.Format)Format,
					Dimension = texture.Settings.ArrayCount > 1 ? SharpDX.Direct3D.ShaderResourceViewDimension.Texture1DArray 
						            : SharpDX.Direct3D.ShaderResourceViewDimension.Texture1D,
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
		private SharpDX.Direct3D11.ShaderResourceViewDescription GetDesc2D(GorgonTexture texture)
		{
			bool isMultiSampled = ((texture.Settings.Multisampling.Count > 1)
			                       || (texture.Settings.Multisampling.Quality > 0));
			int arrayCount = texture.Settings.IsTextureCube ? ArrayCount / 6 : ArrayCount;

			if (!isMultiSampled)
			{
				return new SharpDX.Direct3D11.ShaderResourceViewDescription
					{
						Format = (SharpDX.DXGI.Format)Format,
						Dimension = texture.Settings.ArrayCount > 1
							            ? texture.Settings.IsTextureCube ? SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray 
								              : SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray
							            : texture.Settings.IsTextureCube ? SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube
								              : SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
						Texture2DArray = 
							{
								MipLevels = MipCount,
								MostDetailedMip = MipStart,
								FirstArraySlice = ArrayStart,
								ArraySize = arrayCount
							}
					};
			}

			return new SharpDX.Direct3D11.ShaderResourceViewDescription
				{
					Format = (SharpDX.DXGI.Format)Format,
					Dimension = texture.Settings.ArrayCount > 1
						            ? SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampledArray
						            : SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled,
					Texture2DMSArray =
						{
							ArraySize = ArrayCount,
							FirstArraySlice = ArrayStart
						}
				};
		}

		/// <summary>
		/// Function to retrieve the view description for a 3D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private SharpDX.Direct3D11.ShaderResourceViewDescription GetDesc3D()
		{
			return new SharpDX.Direct3D11.ShaderResourceViewDescription
				{
					Format = (SharpDX.DXGI.Format)Format,
					Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D,
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
		protected override void InitializeImpl()
		{
			SharpDX.Direct3D11.ShaderResourceViewDescription desc;
			var texture = (GorgonTexture)Resource;

			ValidateViewSettings(texture);

			// Build SRV description.
			switch(Resource.ResourceType)
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
				Gorgon.Log.Print("Gorgon resource view: Creating D3D 11 shader resource view.", LoggingLevel.Verbose);

				// Create our SRV.
				D3DView = new SharpDX.Direct3D11.ShaderResourceView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
					{
						DebugName = Resource.ResourceType + " '" + texture.Name + "' Shader Resource View"
					};
			}
			catch (SharpDX.SharpDXException sDXEx)
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
			: base(texture, format, false)
		{
			// TODO: Update this to use raw views.
			MipStart = firstMipLevel;
			MipCount = mipCount;
			ArrayStart = arrayIndex;
			ArrayCount = arrayCount;
		}
		#endregion
	}
}