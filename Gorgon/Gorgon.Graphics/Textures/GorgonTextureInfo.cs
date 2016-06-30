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
// Created: June 13, 2016 8:45:14 PM
// 
#endregion

using System;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// The type of texture.
	/// </summary>
	public enum TextureType
	{
		/// <summary>
		/// The texture will only have a single dimension.
		/// </summary>
		Texture1D = 0,
		/// <summary>
		/// The texture will have 2 dimensions.
		/// </summary>
		Texture2D = 1,
		/// <summary>
		/// The texture will have 3 dimensions.
		/// </summary>
		Texture3D = 2
	}

	/// <summary>
	/// Defines the flags that describe how the texture should be used.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This values can be OR'd together for use in different stages of the pipeline. For example, <c>ShaderResource | RenderTarget</c> allows the texture to be used as a render target and a shader input 
	/// (although, not at the same time) through either a render target view, or shader resource view.
	/// </para>
	/// </remarks>
	[Flags]
	public enum TextureBinding
	{
		/// <summary>
		/// <para>
		/// No binding will be done with this texture.
		/// </para>
		/// <para>
		/// This flag is mutually exclusive, and supercedes any other flags.
		/// </para>
		/// <para>
		/// If this flag is set, then this texture cannot be bound with the pipeline. And this is the only binding flag allowed with the texture has a <see cref="GorgonTextureInfo.Usage"/> 
		/// of <see cref="BufferUsage.Staging"/>.
		/// </para>
		/// </summary>
		None = D3D11.BindFlags.None,
		/// <summary>
		/// The texture is meant to be bound as an input to a shader.
		/// </summary>
		ShaderResource = D3D11.BindFlags.ShaderResource,
		/// <summary>
		/// The texture is meant to be used as a render target.
		/// </summary>
		RenderTarget = D3D11.BindFlags.RenderTarget,
		/// <summary>
		/// <para>
		/// The texture is meant to be used as a depth stencil buffer.
		/// </para>
		/// <para>
		/// To use a depth/stencil buffer as a shader input, the <see cref="GorgonTextureInfo.Format"/> must be set to <see cref="BufferFormat.R32_Typeless"/>. Failure to do so when specifying this flag 
		/// will result in an exception.
		/// </para>
		/// </summary>
		DepthStencil = D3D11.BindFlags.DepthStencil,
		/// <summary>
		/// <para>
		/// The texture is meant to be used with an unordered access view.
		/// </para>
		/// <para>
		/// Textures that are multi-sampled cannot use this flag and can only be bound to a pixel shader and/or compute shader.
		/// </para>
		/// <para>
		/// This value is only supported on devices with a feature level of <see cref="FeatureLevelSupport.Level_11_0"/> or better. Any attempt to use this on a lesser device will throw an exception.
		/// </para>
		/// </summary>
		UnorderedAccess = D3D11.BindFlags.UnorderedAccess
	}

	/// <summary>
	/// Information used to create a texture object.
	/// </summary>
	public class GorgonTextureInfo
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the type of texture to use.
		/// </summary>
		public TextureType TextureType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of the texture, in pixels.
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the texture, in pixels.
		/// </summary>
		/// <remarks>
		/// For textures that have a <see cref="TextureType"/> of <see cref="Graphics.TextureType.Texture1D"/>, this value is ignored.
		/// </remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the texture, in pixels.
		/// </summary>
		/// <remarks>
		/// For textures that have a <see cref="TextureType"/> of <see cref="Graphics.TextureType.Texture1D"/>, or <see cref="Graphics.TextureType.Texture2D"/>, this value is ignored.
		/// </remarks>
		public int Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of array levels for a 1D or 2D texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When this value is greater than 0, the texture will be used as a texture array. If the texture is supposed to be a cube map, then this value should be a multiple of 6 (1 for each face in the cube).
		/// </para>
		/// <para>
		/// For textures that have a <see cref="TextureType"/> of <see cref="Graphics.TextureType.Texture3D"/>, this value is ignored.
		/// </para>
		/// <para>
		/// For video devices with a feature level of <see cref="FeatureLevelSupport.Level_10_0"/>, there can only be a single cube map and thus this value must be set to 6 when creating a cube map texture.
		/// </para>
		/// <para>
		/// This value is defaulted to 1.
		/// </para>
		/// </remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to define this 2D texture as a cube map.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When this value is set to <b>true</b>, then the texture is defined as a cube map using the <see cref="ArrayCount"/> as the number of faces. Because of this, the <see cref="ArrayCount"/> value 
		/// must be a multiple of 6. If it is not, then the array count will be adjusted to meet the requirement.
		/// </para>
		/// <para>
		/// For video devices with a feature level of <see cref="FeatureLevelSupport.Level_10_0"/>, there can only be a single cube map and thus <see cref="ArrayCount"/> must be set to 6 when creating a cube map 
		/// texture.
		/// </para>
		/// <para>
		/// For textures that have a <see cref="TextureType"/> of <see cref="Graphics.TextureType.Texture1D"/> or <see cref="Graphics.TextureType.Texture3D"/>, this value is ignored.
		/// </para>
		/// <para>
		/// This value is defaulted to <b>false</b>.
		/// </para>
		/// </remarks>
		public bool IsCubeMap
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the texture.
		/// </summary>
		public DXGI.Format Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip-map levels for the texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the texture is multi-sampled, this value must be set to 1.
		/// </para>
		/// <para>
		/// This value is defaulted to 1.
		/// </para>
		/// </remarks>
		public int MipLevels
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multi-sample quality and count for this texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <see cref="GorgonMultiSampleInfo.NoMultiSampling"/>.
		/// </remarks>
		public GorgonMultiSampleInfo MultiSampleInfo
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the intended usage flags for this texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <see cref="BufferUsage.Default"/>.
		/// </remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the flags to determine how the texture will be bound with the pipeline when rendering.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the <see cref="Usage"/> property is set to <see cref="BufferUsage.Staging"/>, then the texture will be created with a value of <see cref="TextureBinding.None"/> as staging textures do not 
		/// support bindings of any kind.
		/// </para>
		/// <para>
		/// This value is defaulted to <see cref="TextureBinding.ShaderResource"/>. 
		/// </para>
		/// </remarks>
		public TextureBinding Binding
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextureInfo"/> class.
		/// </summary>
		public GorgonTextureInfo()
		{
			Binding = TextureBinding.ShaderResource;
			Usage = BufferUsage.Default;
			MultiSampleInfo = GorgonMultiSampleInfo.NoMultiSampling;
			MipLevels = 1;
			ArrayCount = 1;
		}
		#endregion
	}
}
