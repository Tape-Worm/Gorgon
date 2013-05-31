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

using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An unordered access texture view.
	/// </summary>
	/// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
	/// <para>Ordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
	/// have a format that is the same bit-depth and in the same group as its bound resource.</para>
	/// <para>Unlike a <see cref="GorgonLibrary.Graphics.GorgonTextureShaderView">GorgonTextureShaderView</see>, only one unordered access view may be applied to a resource.</para>
	/// </remarks>
	public class GorgonTextureUnorderAccessView
		: GorgonUnorderedAccessView
	{
		#region Properties.
		/// <summary>
		/// Property to return the mip map index to view.
		/// </summary>
		public int MipIndex
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

			if ((texture.Settings.IsTextureCube) || (texture.Settings.Multisampling.Count > 1) || (texture.Settings.Multisampling.Quality > 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UAV_NOT_SUPPORTED);
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
			else
			{
				if ((ArrayCount > texture.Settings.Depth)
					|| (ArrayCount + ArrayStart > texture.Settings.Depth)
					|| (ArrayCount < 1))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_DEPTH_COUNT_INVALID,
															texture.Settings.Depth));
				}

				if ((ArrayStart >= texture.Settings.Depth)
					|| (ArrayStart < 0))
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
		private SharpDX.Direct3D11.UnorderedAccessViewDescription GetDesc1D(GorgonTexture texture)
		{
			return new SharpDX.Direct3D11.UnorderedAccessViewDescription
				{
					Format = (SharpDX.DXGI.Format)Format,
					Dimension = texture.Settings.ArrayCount > 1 ? SharpDX.Direct3D11.UnorderedAccessViewDimension.Texture1DArray
						            : SharpDX.Direct3D11.UnorderedAccessViewDimension.Texture1D,
					Texture1DArray =
						{
							MipSlice = MipIndex,
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
		private SharpDX.Direct3D11.UnorderedAccessViewDescription GetDesc2D(GorgonTexture texture)
		{
			return new SharpDX.Direct3D11.UnorderedAccessViewDescription
				{
					Format = (SharpDX.DXGI.Format)Format,
					Dimension = texture.Settings.ArrayCount > 1
						            ? SharpDX.Direct3D11.UnorderedAccessViewDimension.Texture2DArray
						            : SharpDX.Direct3D11.UnorderedAccessViewDimension.Texture2D,
					Texture2DArray =
						{
							MipSlice =  MipIndex,
							FirstArraySlice = ArrayStart,
							ArraySize = ArrayCount
						}
				};
		}

		/// <summary>
		/// Function to retrieve the view description for a 3D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private SharpDX.Direct3D11.UnorderedAccessViewDescription GetDesc3D()
		{
			return new SharpDX.Direct3D11.UnorderedAccessViewDescription
				{
					Format = (SharpDX.DXGI.Format)Format,
					Dimension = SharpDX.Direct3D11.UnorderedAccessViewDimension.Texture3D,
					Texture3D =
						{
							MipSlice = MipIndex,
							FirstWSlice = ArrayStart,
							WSize = ArrayCount
						}
				};
		}


		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void OnInitialize()
		{
			SharpDX.Direct3D11.UnorderedAccessViewDescription desc;
			var texture = (GorgonTexture)Resource;

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
				Gorgon.Log.Print("Gorgon resource view: Creating D3D 11 unordered access resource view.", LoggingLevel.Verbose);

				// Create our SRV.
				D3DView = new SharpDX.Direct3D11.UnorderedAccessView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
					{
						DebugName = Resource.ResourceType + " '" + texture.Name + "' Unordered Access Resource View"
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
		/// Initializes a new instance of the <see cref="GorgonTextureUnorderAccessView"/> class.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="format">The format.</param>
		/// <param name="mipIndex">The first mip level.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <param name="arrayCount">The array count.</param>
		internal GorgonTextureUnorderAccessView(GorgonResource resource, BufferFormat format, int mipIndex, 
		                                        int arrayIndex, int arrayCount)
			: base(resource, format)
		{
			MipIndex = mipIndex;
			ArrayStart = arrayIndex;
			ArrayCount = arrayCount;
		}
		#endregion
	}
}