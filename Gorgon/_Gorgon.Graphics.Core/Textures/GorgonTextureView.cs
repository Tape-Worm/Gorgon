#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 19, 2016 1:29:59 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using DXGI = SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A shader view for textures.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a texture shader view to allow a <see cref="GorgonTexture"/> to be bound to the GPU pipeline as a shader resource.
	/// </para>
	/// <para>
	/// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
	/// format within the same group.	
	/// </para>
	/// </remarks>
	public sealed class GorgonTextureView
		: GorgonShaderResourceView
    {
        #region Variables.
		// Log used for debugging.
		private readonly IGorgonLog _log;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the index of the first mip map in the resource to view.
		/// </summary>
		/// <remarks>
		/// If this view is for a 2D <see cref="GorgonTexture"/>, and that texture is multisampled, then this value will be set to 0.
		/// </remarks>
		public int MipSlice
		{
			get;
		    private set;
		}

		/// <summary>
		/// Property to return the number of mip maps in the resource to view.
		/// </summary> 
		/// <remarks>
		/// If this view is for a 2D <see cref="GorgonTexture"/>, and that texture is multisampled, then this value will be set to 1.
		/// </remarks>
		public int MipCount
		{
			get;
		    private set;
		}

		/// <summary>
		/// Property to return the first array index to use in the view.
		/// </summary>
		/// <remarks>
		/// For a 1D or 2D <see cref="GorgonTexture"/>, this value indicates an array index.  For a 3D texture, this value will always equal 0.
		/// </remarks>
		public int ArrayIndex
		{
			get;
		    private set;
		}

		/// <summary>
		/// Property to return the number of array indices to use in the view.
		/// </summary>
		/// <remarks>
		/// <para>
		/// For a 1D or 2D <see cref="GorgonTexture"/>, this value indicates an array index.  For a 3D texture, this value will always equal 1.
		/// </para>
		/// <para>
		/// For a 2D <see cref="GorgonTexture"/>, this value must be a multiple of 6 if the texture is a 2D texture cube.
		/// </para>
		/// </remarks>
		public int ArrayCount
		{
			get;
            private set;
		}

        /// <summary>
        /// Property to return whether the texture is a texture cube or not.
        /// </summary>
        /// <remarks>
        /// This value is always <b>false</b> for a 1D or 3D <see cref="GorgonTexture"/>.
        /// </remarks>
        public bool IsCubeMap => Texture.Info.IsCubeMap;

        /// <summary>
        /// Property to return the texture that is bound to this view.
        /// </summary>
        public GorgonTexture Texture
        {
            get;
        }

        /// <summary>
        /// Property to return the format used to interpret this view.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return information about the <see cref="Format"/> used by this view.
        /// </summary>
        public GorgonFormatInfo FormatInformation
        {
            get;
        }

        /// <summary>
        /// Property to return the bounding rectangle for the view.
        /// </summary>
        /// <remarks>
        /// This value is the full bounding rectangle of the first mip map level for the texture associated with the render target.
        /// </remarks>
        public DX.Rectangle Bounds
        {
            get;
        }

        /// <summary>
        /// Property to return the type of texture bound to this view.
        /// </summary>
        public TextureType TextureType => Texture.Info.TextureType;

        /// <summary>
        /// Property to return the width of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full width of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Width => Texture.Info.Width;

        /// <summary>
        /// Property to return the height of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full height of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Height => Texture.Info.Height;

        /// <summary>
        /// Property to return the depth of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full depth of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Depth => Texture.Info.Depth;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description for a 1D texture.
        /// </summary>
        /// <returns>The shader view description.</returns>
        private D3D11.ShaderResourceViewDescription GetDesc1D()
		{
			return new D3D11.ShaderResourceViewDescription
			       {
				       Format = (DXGI.Format)Format,
				       Dimension = Texture.Info.ArrayCount > 1
					                   ? D3D.ShaderResourceViewDimension.Texture1DArray
					                   : D3D.ShaderResourceViewDimension.Texture1D,
				       Texture1DArray =
				       {
					       MipLevels = MipCount,
					       MostDetailedMip = MipSlice,
					       ArraySize = ArrayCount,
					       FirstArraySlice = ArrayIndex
				       }
			       };
		}

		/// <summary>
		/// Function to retrieve the view description for a 2D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private D3D11.ShaderResourceViewDescription GetDesc2D()
		{
			bool isMultisampled = ((Texture.Info.MultisampleInfo.Count > 1)
			                       || (Texture.Info.MultisampleInfo.Quality > 0));

			if (!isMultisampled)
			{
				return new D3D11.ShaderResourceViewDescription
				       {
					       Format = (DXGI.Format)Format,
					       Dimension = Texture.Info.ArrayCount > 1
						                   ? D3D.ShaderResourceViewDimension.Texture2DArray
						                   : D3D.ShaderResourceViewDimension.Texture2D,
					       Texture2DArray =
					       {
						       MipLevels = MipCount,
						       MostDetailedMip = MipSlice,
						       FirstArraySlice = ArrayIndex,
						       ArraySize = ArrayCount
					       }
				       };
			}

		    MipSlice = 0;
		    MipCount = 1;

			return new D3D11.ShaderResourceViewDescription
			       {
				       Format = (DXGI.Format)Format,
				       Dimension = Texture.Info.ArrayCount > 1
					                   ? D3D.ShaderResourceViewDimension.Texture2DMultisampledArray
					                   : D3D.ShaderResourceViewDimension.Texture2DMultisampled,
				       Texture2DMSArray =
				       {
					       ArraySize = ArrayCount,
					       FirstArraySlice = ArrayIndex
				       }
			       };
		}

		/// <summary>
        /// Function to retrieve the view description for a 2D texture cube.
        /// </summary>
	    private D3D11.ShaderResourceViewDescription GetDesc2DCube()
	    {
            bool isMultisampled = ((Texture.Info.MultisampleInfo.Count > 1)
                                   || (Texture.Info.MultisampleInfo.Quality > 0));

	        if (isMultisampled)
	        {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CANNOT_MULTISAMPLE_CUBE);
            }

            if ((ArrayCount % 6) != 0)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CUBE_ARRAY_SIZE_INVALID);
			}

			int cubeArrayCount = ArrayCount / 6;

			return new D3D11.ShaderResourceViewDescription
			       {
				       Format = (DXGI.Format)Format,
				       Dimension =
					       cubeArrayCount > 1
						       ? D3D.ShaderResourceViewDimension.TextureCubeArray
						       : D3D.ShaderResourceViewDimension.TextureCube,
				       TextureCubeArray =
				       {
					       CubeCount = cubeArrayCount,
					       First2DArrayFace = ArrayIndex,
					       MipLevels = MipCount,
					       MostDetailedMip = MipSlice
				       }
			       };
	    }

		/// <summary>
		/// Function to retrieve the view description for a 3D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private D3D11.ShaderResourceViewDescription GetDesc3D()
		{
		    ArrayIndex = 0;
		    ArrayCount = 1;
			return new D3D11.ShaderResourceViewDescription
			       {
				       Format = (DXGI.Format)Format,
				       Dimension = D3D.ShaderResourceViewDimension.Texture3D,
				       Texture3D =
				       {
					       MostDetailedMip = MipSlice,
					       MipLevels = MipCount
				       }
			       };
		}

		/// <summary>
		/// Function to initialize the shader resource view.
		/// </summary>
		protected internal override void CreateNativeView()
		{
			D3D11.ShaderResourceViewDescription desc;
			
			_log.Print("Creating texture shader view for {0}.", LoggingLevel.Verbose, Texture.Name);

			// Build SRV description.
			switch (Texture.ResourceType)
			{
				case GraphicsResourceType.Texture1D:
					desc = GetDesc1D();
					break;
				case GraphicsResourceType.Texture2D:
					desc = IsCubeMap ? GetDesc2DCube() : GetDesc2D();
					break;
				case GraphicsResourceType.Texture3D:
					desc = GetDesc3D();
					break;
				default:
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_ERR_TEXTURE_TYPE_NOT_SUPPORTED, Texture.ResourceType));
			}

			try
			{
				_log.Print("Gorgon resource view: Creating D3D 11 shader resource view.", LoggingLevel.Verbose);

				// Create our SRV.
				NativeView = new D3D11.ShaderResourceView(Texture.Graphics.VideoDevice.D3DDevice(), Texture.D3DResource, desc)
				          {
					          DebugName = $"'{Texture.Name}': D3D 11 Shader resource view"
				          };
			}
			catch (DX.SharpDXException sDXEx)
			{
				if ((uint)sDXEx.ResultCode.Code == 0x80070057)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
															Texture.Info.Format,
															Format));
				}
			}
		}

        /// <summary>
        /// Function to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipWidth(int mipLevel)
        {
            mipLevel = mipLevel.Min(MipCount + MipSlice).Max(MipSlice);
            return Width << mipLevel;
        }

        /// <summary>
        /// Function to return the height of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipHeight(int mipLevel)
        {
            mipLevel = mipLevel.Min(MipCount + MipSlice).Max(MipSlice);

            return Height << mipLevel;
        }

        /// <summary>
        /// Function to return the depth of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The depth of the mip map level assigned to the <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipDepth(int mipLevel)
        {
            mipLevel = mipLevel.Min(MipCount + MipSlice).Max(MipSlice);

            return Depth << mipLevel;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
		{
			if (NativeView != null)
			{
				_log.Print($"Shader Resource View '{Texture.Name}': Releasing D3D11 Shader resource view.", LoggingLevel.Simple);
			}

			base.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextureView"/> class.
		/// </summary>
		/// <param name="texture">The <see cref="GorgonTexture"/> being viewed.</param>
		/// <param name="format">The format for the view..</param>
		/// <param name="firstMipLevel">The first mip level to view.</param>
		/// <param name="mipCount">The number of mip levels to view.</param>
		/// <param name="arrayIndex">For a 1D or 2D <see cref="GorgonTexture"/>, this will be the first array index to view.</param>
		/// <param name="arrayCount">For a 1D or 2D <see cref="GorgonTexture"/>, this will be the number of array indices to view.</param>
		/// <param name="log">The log used for debugging.</param>
		internal GorgonTextureView(GorgonTexture texture,
		                           BufferFormat format,
		                           int firstMipLevel,
		                           int mipCount,
		                           int arrayIndex,
		                           int arrayCount,
								   IGorgonLog log)
            : base(texture)
		{
            _log = log ?? GorgonLogDummy.DefaultInstance;
		    Texture = texture;

		    if (format == BufferFormat.Unknown)
		    {
		        format = texture.Info.Format;
		    }

            if ((texture.Info.Usage == ResourceUsage.Staging) 
				|| ((texture.Info.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_SHADER_RESOURCE, texture.Name), nameof(texture));
			}

		    if (format == BufferFormat.Unknown)
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, BufferFormat.Unknown), nameof(texture));
		    }

		    if (firstMipLevel + mipCount > texture.Info.MipLevels)
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_MIP_OUT_OF_RANGE, firstMipLevel, mipCount, texture.Info.MipLevels));
		    }

            // Arrays only apply to 1D/2D textures.
		    if ((texture.Info.TextureType != TextureType.Texture3D) && (arrayIndex + arrayCount > texture.Info.ArrayCount))
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_ARRAY_OUT_OF_RANGE, arrayIndex, arrayCount, texture.Info.ArrayCount));
            }

		    FormatInformation = new GorgonFormatInfo(Format);

		    if (FormatInformation.IsTypeless)
		    {
		        throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
		    }

		    Format = format;
		    Bounds = new DX.Rectangle(0, 0, Width, Height);
            MipSlice = firstMipLevel;
		    MipCount = mipCount;
		    ArrayIndex = arrayIndex;
		    ArrayCount = arrayCount;
		}
		#endregion
	}
}