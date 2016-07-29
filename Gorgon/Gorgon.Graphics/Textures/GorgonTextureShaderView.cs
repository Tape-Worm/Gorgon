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
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
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
	/// <para>
	/// The <see cref="GorgonShaderResourceView.Key"/> Gorgon will set the view parameters as a 64 bit unsigned integer value when the view is created. This key is composed of the following bits:
	/// <list type="table">
	///		<listheader>
	///			<term>Bits (inclusive)</term>
	///			<term>Value</term>		
	///		</listheader>
	///		<item>
	///			<term>0 - 3 (4 bits)</term>
	///			<term><see cref="MipSlice">Mip slice.</see></term>
	///		</item>
	///		<item>
	///			<term>4 - 7 (4 bits)</term>
	///			<term><see cref="MipCount">Mip count.</see></term>
	///		</item>
	///		<item>
	///			<term>8 - 18 (11 bits)</term>
	///			<term><see cref="ArrayIndex">Array Index.</see></term>
	///		</item>
	///		<item>
	///			<term>19 - 29 (11 bits)</term>
	///			<term><see cref="ArrayCount">Array Count.</see></term>
	///		</item>
	///		<item>
	///			<term>30 - 37 (8 bits)</term>
	///			<term><see cref="Format"/></term>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// For example, a <see cref="MipSlice"/> of 2, a <see cref="MipCount"/> of 1 and a <see cref="ArrayIndex"/> of 4, with an <see cref="ArrayCount"/> of 2 and a <see cref="Format"/> 
	/// of <c>R8G8B8A8_UNorm</c> (28) would yield a key of: <c>30065820690</c>. 
	/// <br/>
	/// Or, a <see cref="MipSlice"/> of 0, and <see cref="MipCount"/> of 1, with a <see cref="ArrayIndex"/> of 0, and a <see cref="ArrayCount"/> of 1, and the same buffer format would yield a key of: 
	/// <c>30065295376</c>.
	/// </para>
	/// </remarks>
	public sealed class GorgonTextureShaderView
		: GorgonShaderResourceView
    {
        #region Variables.
		// Log used for debugging.
		private readonly IGorgonLog _log;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the texture bound with this view.
		/// </summary>
		public GorgonTexture Texture
		{
			get;
		}

		/// <summary>
		/// Property to return the format for this view.
		/// </summary>
		public DXGI.Format Format
		{
			get;
		}

		/// <summary>
		/// Property to return the format information for the <see cref="Format"/> of this view.
		/// </summary>
		public GorgonFormatInfo FormatInformation
		{
			get;
		}

		/// <summary>
		/// Property to return the index of the first mip map in the resource to view.
		/// </summary>
		/// <remarks>
		/// If this view is for a 2D <see cref="GorgonTexture"/>, and that texture is multisampled, then this value will be set to 0.
		/// </remarks>
		public int MipSlice
		{
			get;
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
		}

		/// <summary>
		/// Property to return whether the texture is a texture cube or not.
		/// </summary>
		/// <remarks>
		/// This value is always <b>false</b> for a 1D or 3D <see cref="GorgonTexture"/>.
		/// </remarks>
		public bool IsCube
	    {
	        get;
	    }
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
				       Format = Format,
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
					       Format = Format,
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

			return new D3D11.ShaderResourceViewDescription
			       {
				       Format = Format,
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
				       Format = Format,
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
			return new D3D11.ShaderResourceViewDescription
			       {
				       Format = Format,
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
		private void Initialize()
		{
			D3D11.ShaderResourceViewDescription desc;
			
			_log.Print("Creating texture shader view for {0}.", LoggingLevel.Verbose, Texture.Name);

			// Build SRV description.
			switch (Texture.ResourceType)
			{
				case ResourceType.Texture1D:
					desc = GetDesc1D();
					break;
				case ResourceType.Texture2D:
					desc = IsCube ? GetDesc2DCube() : GetDesc2D();
					break;
				case ResourceType.Texture3D:
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
				D3DView = new D3D11.ShaderResourceView(Texture.Graphics.VideoDevice.D3DDevice(), Texture.D3DResource, desc)
				          {
					          DebugName = $"'{Texture.Name}': D3D 11 Shader resource view"
				          };
			}
			catch (DX.SharpDXException sDXEx)
			{
				if ((uint)sDXEx.ResultCode.Code == 0x80070057)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_CANNOT_CAST_FORMAT,
															Texture.Info.Format,
															Format));
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			if (D3DView != null)
			{
				_log.Print($"Shader Resource View '{Texture.Name}': Releasing D3D11 Shader resource view.", LoggingLevel.Simple);
			}

			base.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextureShaderView"/> class.
		/// </summary>
		/// <param name="texture">The <see cref="GorgonTexture"/> being viewed.</param>
		/// <param name="format">[Optional] The format for the view..</param>
		/// <param name="firstMipLevel">[Optional] The first mip level to view.</param>
		/// <param name="mipCount">[Optional] The number of mip levels to view.</param>
		/// <param name="arrayIndex">[Optional] For a 1D or 2D <see cref="GorgonTexture"/>, this will be the first array index to view.</param>
		/// <param name="arrayCount">[Optional] For a 1D or 2D <see cref="GorgonTexture"/>, this will be the number of array indices to view.</param>
		/// <param name="log">[Optional] The log used for debugging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="texture"/> does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ShaderResource"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="texture"/> has a usage of <c>Staging</c>.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="format"/> is typeless or cannot be determined from the <paramref name="texture"/>, or the <paramref name="format"/> is not in the same group as the <paramref name="texture"/> format.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the <paramref name="texture"/> is a 2D cube texture, but is multiple sampled, or the <paramref name="arrayCount"/> is not a multiple of 6.</exception>
		/// <remarks>
		/// <para>
		/// This will create a view that makes a texture accessible to shaders. This allows viewing of the texture data in a different format, or even a subsection of the texture from within the shader.
		/// </para>
		/// <para>
		/// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <c>Unknown</c>, then the format from the 
		/// <paramref name="texture"/> is used. The <paramref name="format"/> must be castable to the format supplied with <paramref name="texture"/>. If it is not, an exception will be thrown.
		/// </para>
		/// <para>
		/// The <paramref name="firstMipLevel"/> and <paramref name="mipCount"/> parameters define the starting mip level and the number of mip levels to allow access to within the shader. If these values fall 
		/// outside of the range of available mip levels, then they will be clipped to the upper and lower bounds of the mip chain. If these values are left at 0, then all mip levels will be accessible.
		/// </para>
		/// <para>
		/// The <paramref name="arrayIndex"/> and <paramref name="arrayCount"/> parameters define the starting array index and the number of array indices to allow access to within the shader. If these values 
		/// fall outside of the range of available array indices, then they will be clipped to the upper and lower bounds of the array. If these values are left at 0, then all array indices will be accessible.
		/// </para>
		/// </remarks>
		public GorgonTextureShaderView(GorgonTexture texture,
		                               DXGI.Format format = DXGI.Format.Unknown,
		                               int firstMipLevel = 0,
		                               int mipCount = 0,
		                               int arrayIndex = 0,
		                               int arrayCount = 0,
									   IGorgonLog log = null)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;

			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			Texture = texture;

			if ((Texture.Info.Usage == D3D11.ResourceUsage.Staging) 
				|| ((Texture.Info.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_SHADER_RESOURCE, texture.Name), nameof(texture));
			}

			Format = format == DXGI.Format.Unknown ? Texture.Info.Format : format;

			if (Format == DXGI.Format.Unknown)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, Format), nameof(texture));
			}

			FormatInformation = new GorgonFormatInfo(Format);

			if (FormatInformation.IsTypeless)
			{
				throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(texture));
			}


			if ((Texture.Info.Format != format) && (FormatInformation.Group != Texture.FormatInformation.Group))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_FORMAT_GROUP_INVALID,
				                                          Texture.Info.Format,
				                                          Format),
				                            nameof(texture));
			}

			MipSlice = Texture.Info.MipLevels <= 0 ? 0 : firstMipLevel.Max(0).Min(Texture.Info.MipLevels - 1);

			if (mipCount <= 0)
			{
				mipCount = Texture.Info.MipLevels;
			}

			MipCount = Texture.Info.MipLevels <= 0 ? 1 : mipCount.Max(1).Min(mipCount - MipSlice);

			ArrayIndex = arrayIndex.Max(0).Min(Texture.Info.ArrayCount - 1);

			if (arrayCount == 0)
			{
				arrayCount = Texture.Info.ArrayCount;
			}

			ArrayCount = arrayCount.Min(arrayCount - ArrayIndex).Max(1);

			IsCube = Texture.Info.IsCubeMap;

			Initialize();

			// The key for a texture shader view is broken up into the following layout.
			// Bits: [37 - 30]   [29 - 19]     [18 - 8]		[7 - 4]		[3 - 0]
			//       Format      Array/Depth   Array/Depth  Mip Count	Mip slice
			//                   Count         Index
			Key = (((uint)Format) & 0xff) << 30
			      | (((uint)ArrayCount) & 0x7ff) << 19
			      | (((uint)ArrayIndex) & 0x7ff) << 8
			      | (((uint)MipCount) & 0xf) << 4
			      | (uint)MipSlice;
		}
		#endregion
	}
}