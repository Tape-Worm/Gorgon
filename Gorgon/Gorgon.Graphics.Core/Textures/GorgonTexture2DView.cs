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
using System.IO;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
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
	/// This is a texture shader view to allow a <see cref="GorgonTexture2D"/> to be bound to the GPU pipeline as a shader resource.
	/// </para>
	/// <para>
	/// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
	/// format within the same group.	
	/// </para>
	/// </remarks>
	public sealed class GorgonTexture2DView
		: GorgonShaderResourceView, IGorgonTexture2DInfo
    {
        #region Variables.
        // The texture linked to this view.
        private GorgonTexture2D _texture;
        #endregion

        #region Properties.

		/// <summary>
		/// Property to return the index of the first mip map in the resource to view.
		/// </summary>
		/// <remarks>
		/// If the texture is multisampled, then this value will be set to 0.
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
		/// If the texture is multisampled, then this value will be set to 1.
		/// </remarks>
		public int MipCount
		{
			get;
		    private set;
		}

		/// <summary>
		/// Property to return the first array index to use in the view.
		/// </summary>
		public int ArrayIndex
		{
			get;
		}

		/// <summary>
		/// Property to return the number of array indices to use in the view.
		/// </summary>
		/// <remarks>
		/// If the texture is a 2D texture cube, then this value will be a multiple of 6.
		/// </remarks>
		public int ArrayCount
		{
			get;
		}

        /// <summary>
        /// Property to return whether the texture is a texture cube or not.
        /// </summary>
        public bool IsCubeMap => Texture?.IsCubeMap ?? false;

        /// <summary>
        /// Property to return the texture that is bound to this view.
        /// </summary>
        public GorgonTexture2D Texture => _texture;

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
        /// Property to return the width of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full width of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Width => Texture?.Width ?? 0;

        /// <summary>
        /// Property to return the height of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full height of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Height => Texture?.Height ?? 0;

        /// <summary>
        /// Property to return the name of the texture.
        /// </summary>
        string IGorgonNamedObject.Name => Texture?.Name ?? string.Empty;

        /// <summary>
        /// Property to return the number of mip-map levels for the texture.
        /// </summary>
        int IGorgonTexture2DInfo.MipLevels => Texture?.MipLevels ?? 0;

        /// <summary>
        /// Property to return the multisample quality and count for the texture.
        /// </summary>
        public GorgonMultisampleInfo MultisampleInfo => Texture?.MultisampleInfo ?? GorgonMultisampleInfo.NoMultiSampling;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;
        #endregion

        #region Methods.
		/// <summary>
		/// Function to retrieve the view description for a 2D texture.
		/// </summary>
		/// <returns>The shader view description.</returns>
		private D3D11.ShaderResourceViewDescription1 GetDesc2D()
		{
			bool isMultisampled = ((Texture.MultisampleInfo.Count > 1)
			                       || (Texture.MultisampleInfo.Quality > 0));

			if (!isMultisampled)
			{
				return new D3D11.ShaderResourceViewDescription1
				       {
					       Format = (DXGI.Format)Format,
					       Dimension = Texture.ArrayCount > 1
						                   ? D3D.ShaderResourceViewDimension.Texture2DArray
						                   : D3D.ShaderResourceViewDimension.Texture2D,
					       Texture2DArray =
					       {
						       MipLevels = MipCount,
						       MostDetailedMip = MipSlice,
						       FirstArraySlice = ArrayIndex,
						       ArraySize = ArrayCount,
                               PlaneSlice = 0
					       }
				       };
			}

		    MipSlice = 0;
		    MipCount = 1;

			return new D3D11.ShaderResourceViewDescription1
			       {
				       Format = (DXGI.Format)Format,
				       Dimension = Texture.ArrayCount > 1
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
	    private D3D11.ShaderResourceViewDescription1 GetDesc2DCube()
	    {
            bool isMultisampled = ((Texture.MultisampleInfo.Count > 1)
                                   || (Texture.MultisampleInfo.Quality > 0));

	        if (isMultisampled)
	        {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CANNOT_MULTISAMPLE_CUBE);
            }

            if ((ArrayCount % 6) != 0)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CUBE_ARRAY_SIZE_INVALID);
			}

			int cubeArrayCount = ArrayCount / 6;

			return new D3D11.ShaderResourceViewDescription1
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
		/// Function to initialize the shader resource view.
		/// </summary>
		protected internal override void CreateNativeView()
		{
			D3D11.ShaderResourceViewDescription1 desc = IsCubeMap ? GetDesc2DCube() : GetDesc2D();

			try
			{
			    Graphics.Log.Print($"'{Texture.Name}': Creating D3D11 shader resource view.", LoggingLevel.Simple);

				// Create our SRV.
			    NativeView = new D3D11.ShaderResourceView1(Texture.Graphics.D3DDevice, Texture.D3DResource, desc)
			                 {
			                     DebugName = $"'{Texture.Name}'_D3D11ShaderResourceView1_2D"
			                 };

			    Graphics.Log.Print($"Shader Resource View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}", LoggingLevel.Verbose);
                
			    this.RegisterDisposable(Texture.Graphics);
			}
			catch (DX.SharpDXException sDXEx)
			{
				if ((uint)sDXEx.ResultCode.Code == 0x80070057)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
															Texture.Format,
															Format));
				}
			}
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _texture = null;
            base.Dispose();
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
        /// Function to create a new texture that is bindable to the GPU.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="info">The information about the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonTexture2D"/> and a <see cref="GorgonTexture2DView"/> as a single object that users can use to apply a texture as a shader 
        /// resource. This helps simplify creation of a texture by executing some prerequisite steps on behalf of the user.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2D"/>
        public static GorgonTexture2DView CreateTexture(GorgonGraphics graphics, IGorgonTexture2DInfo info)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var newInfo = new GorgonTexture2DInfo(info)
                          {
                              Usage = info.Usage == ResourceUsage.Staging ? ResourceUsage.Default : info.Usage,
                              Binding = (info.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource
                                            ? (info.Binding & TextureBinding.ShaderResource)
                                            : info.Binding
                          };

            var texture = new GorgonTexture2D(graphics, newInfo);
            GorgonTexture2DView result = texture.GetShaderResourceView();
            result.OwnsResource = true;

            return result;
        }

        /// <summary>
        /// Function to load a texture from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="stream">The stream containing the texture image data.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="size">[Optional] The size of the image in the stream, in bytes.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DView"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DView"/>.
        /// </para>
        /// <para>
        /// If the <paramref name="size"/> option is specified, then the method will read from the stream up to that number of bytes, so it is up to the user to provide an accurate size. If it is omitted 
        /// then the <c>stream length - stream position</c> is used as the total size.
        /// </para>
        /// <para>
        /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
        /// <list type="bullet">
        ///		<item>
        ///			<term>Binding</term>
        ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
        ///         <see cref="TextureBinding.ShaderResource"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Usage</term>
        ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Multisample info</term>
        ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture2DView FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTextureLoadOptions options = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
            }

            if (size == null)
            {
                size = stream.Length - stream.Position;
            }

            if ((stream.Length - stream.Position) < size)
            {
                throw new EndOfStreamException();
            }

            using (IGorgonImage image = codec.LoadFromStream(stream, size))
            {
                GorgonTexture2D texture = image.ToTexture2D(graphics, options);
                GorgonTexture2DView view =  texture.GetShaderResourceView();
                view.OwnsResource = true;
                return view;
            }
        }

        /// <summary>
        /// Function to load a texture from a file.
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DView"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DView"/>.
        /// </para>
        /// <para>
        /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
        /// <list type="bullet">
        ///		<item>
        ///			<term>Binding</term>
        ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
        ///         <see cref="TextureBinding.ShaderResource"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Usage</term>
        ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Multisample info</term>
        ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DView"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DView"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture2DView FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTextureLoadOptions options = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            using (IGorgonImage image = codec.LoadFromFile(filePath))
            {
                GorgonTexture2D texture = image.ToTexture2D(graphics, options);
                GorgonTexture2DView view = texture.GetShaderResourceView();
                view.OwnsResource = true;
                return view;
            }
        }
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2DView"/> class.
        /// </summary>
        /// <param name="texture">The <see cref="GorgonTexture2D"/> being viewed.</param>
        /// <param name="format">The format for the view..</param>
        /// <param name="firstMipLevel">The first mip level to view.</param>
        /// <param name="mipCount">The number of mip levels to view.</param>
        /// <param name="arrayIndex">The first array index to view.</param>
        /// <param name="arrayCount">The number of array indices to view.</param>
        internal GorgonTexture2DView(GorgonTexture2D texture,
                                     BufferFormat format,
                                     int firstMipLevel,
                                     int mipCount,
                                     int arrayIndex,
                                     int arrayCount)
            : base(texture)
        {
            _texture = texture;

            if (format == BufferFormat.Unknown)
            {
                format = texture.Format;
            }

            if ((texture.Usage == ResourceUsage.Staging)
                || ((texture.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_SHADER_RESOURCE, texture.Name), nameof(texture));
            }

            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, BufferFormat.Unknown), nameof(texture));
            }

            if (firstMipLevel + mipCount > texture.MipLevels)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_MIP_OUT_OF_RANGE, firstMipLevel, mipCount, texture.MipLevels));
            }

            if (arrayIndex + arrayCount > texture.ArrayCount)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_ARRAY_OUT_OF_RANGE, arrayIndex, arrayCount, texture.ArrayCount));
            }

            FormatInformation = new GorgonFormatInfo(Format);

            if (FormatInformation.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            if ((!texture.FormatInformation.IsTypeless) && ((texture.Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil))
            {
                throw new ArgumentException(Resources.GORGFX_ERR_DEPTHSTENCIL_TYPED_SHADER_RESOURCE, nameof(texture));
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