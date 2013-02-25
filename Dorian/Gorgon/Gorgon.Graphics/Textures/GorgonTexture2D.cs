#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, February 08, 2012 3:04:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using SlimMath;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A 2 dimensional texture object.
	/// </summary>
	public class GorgonTexture2D
		: GorgonTexture
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType
		{
			get 
			{
				return ResourceType.Texture2D;
			}
		}

		/// <summary>
		/// Property to return whether this texture is for a depth/stencil buffer.
		/// </summary>
		public bool IsDepthStencil
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public new GorgonTexture2DSettings Settings
		{
			get
			{
				return (GorgonTexture2DSettings)base.Settings;
			}
			private set
			{
				base.Settings = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return sub resource data for a lock operation.
		/// </summary>
		/// <param name="dataStream">Stream containing the data.</param>
		/// <param name="rowPitch">The number of bytes per row of the texture.</param>
		/// <param name="slicePitch">The number of bytes per depth slice of the texture.</param>
		/// <returns>
		/// The sub resource data.
		/// </returns>
		protected override ISubResourceData GetLockSubResourceData(IO.GorgonDataStream dataStream, int rowPitch, int slicePitch)
		{
			return new GorgonTexture2DData(dataStream, rowPitch);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		protected override void UpdateSubResourceImpl(ISubResourceData data, int subResource)
		{
#if DEBUG
			if (IsDepthStencil)
			{
				throw new InvalidOperationException("Cannot update a texture used as a depth/stencil buffer.");
			}
#endif

			SharpDX.DataBox box = new SharpDX.DataBox()
			{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			D3D.ResourceRegion region = new D3D.ResourceRegion();

			region.Front = 0;
			region.Back = 1;
			region.Left = 0;
			region.Right = Settings.Width;
			region.Top = 0;
			region.Bottom = Settings.Height;

			Graphics.Context.UpdateSubresourceSafe(box, D3DResource, FormatInformation.SizeInBytes, subResource, region, FormatInformation.IsCompressed);			
		}

		/// <summary>
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>
		/// The new staging texture.
		/// </returns>
		protected override GorgonTexture GetStagingTextureImpl()
		{
			GorgonTexture staging = null;

			GorgonTexture2DSettings settings2D = new GorgonTexture2DSettings()
			{
				ArrayCount = Settings.ArrayCount,
				Format = Settings.Format,
				Width = Settings.Width,
				Height = Settings.Height,
				IsTextureCube = Settings.IsTextureCube,
				Multisampling = Settings.Multisampling,
				MipCount = Settings.MipCount,
				Usage = BufferUsage.Staging
			};

			staging = Graphics.Textures.CreateTexture<GorgonTexture2D>(Name + ".Staging", settings2D);

			staging.Copy(this);

			return staging;
		}

		/// <summary>
		/// Function to retrieve information about an existing texture.
		/// </summary>
		/// <returns>
		/// New settings for the texture.
		/// </returns>
		protected override ITextureSettings GetTextureInformation()
		{
			ITextureSettings newSettings = null;
			BufferFormat viewFormat = BufferFormat.Unknown;

			if (Settings != null)
				viewFormat = Settings.ViewFormat;

			D3D.Texture2DDescription desc = ((D3D.Texture2D)this.D3DResource).Description;
			newSettings = new GorgonTexture2DSettings();
			newSettings.Width = desc.Width;
			newSettings.Height = desc.Height;
			newSettings.ArrayCount = desc.ArraySize;
			newSettings.Depth = 1;
			newSettings.Format = (BufferFormat)desc.Format;
			newSettings.MipCount = desc.MipLevels;
			newSettings.Usage = (BufferUsage)desc.Usage;
			newSettings.ViewFormat = BufferFormat.Unknown;
			newSettings.Multisampling = new GorgonMultisampling(desc.SampleDescription.Count, desc.SampleDescription.Quality);

			// Preserve any custom view format.
			newSettings.ViewFormat = viewFormat;

			return newSettings;
		}

		/// <summary>
		/// Function to initialize a render target texture.
		/// </summary>
		internal void InitializeRenderTarget()
		{
			D3D.Texture2DDescription desc = new D3D.Texture2DDescription();

			desc.ArraySize = 1;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.Height = Settings.Height;
			desc.MipLevels = 1;
			desc.BindFlags = D3D.BindFlags.RenderTarget | D3D.BindFlags.ShaderResource;
			desc.Usage = D3D.ResourceUsage.Default;
			desc.CpuAccessFlags = D3D.CpuAccessFlags.None;
			desc.OptionFlags = D3D.ResourceOptionFlags.None;
			desc.SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling);

			Gorgon.Log.Print("{0} {1}: Creating 2D D3D 11 render target texture...", Diagnostics.LoggingLevel.Verbose, GetType().Name, Name);
			D3DResource = new D3D.Texture2D(Graphics.D3DDevice, desc);

			CreateDefaultResourceView();
		}

		/// <summary>
		/// Function to initialize a depth/stencil texture.
		/// </summary>
		/// <param name="isShaderBound">TRUE if the texture should be used in a shader, FALSE if not.</param>
		internal void InitializeDepth(bool isShaderBound)
		{
			D3D.Texture2DDescription desc = new D3D.Texture2DDescription();

			desc.ArraySize = 1;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.Height = Settings.Height;
			desc.MipLevels = Settings.MipCount;
			desc.BindFlags = D3D.BindFlags.DepthStencil;
			if (isShaderBound)
				desc.BindFlags |= D3D.BindFlags.ShaderResource;
			desc.Usage = D3D.ResourceUsage.Default;
			desc.CpuAccessFlags = D3D.CpuAccessFlags.None;
			desc.OptionFlags = D3D.ResourceOptionFlags.None;
			desc.SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling);

			Gorgon.Log.Print("{0} {1}: Creating D3D 11 depth/stencil texture...", Diagnostics.LoggingLevel.Verbose, GetType().Name, Name);
			D3DResource = new D3D.Texture2D(Graphics.D3DDevice, desc);
			IsDepthStencil = true;

			if (isShaderBound)
				CreateDefaultResourceView();
		}

		/// <summary>
		/// Function to initialize the texture with optional initial data.
		/// </summary>
		/// <param name="initialData">Data used to populate the image.</param>
		protected override void InitializeImpl(GorgonImageData initialData)
		{
			D3D.Texture2DDescription desc = new D3D.Texture2DDescription();

			desc.ArraySize = Settings.ArrayCount;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.Height = Settings.Height;
			desc.MipLevels = Settings.MipCount;
			if (Settings.Usage != BufferUsage.Staging)
				desc.BindFlags = D3D.BindFlags.ShaderResource;
			else
				desc.BindFlags = D3D.BindFlags.None;
			desc.Usage = (D3D.ResourceUsage)Settings.Usage;
			switch (Settings.Usage)
			{
				case BufferUsage.Staging:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write;
					break;
				case BufferUsage.Dynamic:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.Write;
					break;
				default:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.None;
					break;
			}
			desc.OptionFlags = D3D.ResourceOptionFlags.None;
			desc.SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling);

			if (initialData != null)
			{
				D3DResource = new D3D.Texture2D(Graphics.D3DDevice, desc, initialData.GetDataBoxes());
			}
			else
			{
				D3DResource = new D3D.Texture2D(Graphics.D3DDevice, desc);
			}
		}

		/// <summary>
		/// Function to convert a texel space coordinate into a pixel space coordinate.
		/// </summary>
		/// <param name="texel">The texel coordinate to convert.</param>
		/// <returns>The pixel location of the texel on the texture.</returns>
		public Vector2 ToPixel(Vector2 texel)
		{
			return new Vector2(texel.X * Settings.Width, texel.Y * Settings.Height);
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel space coordinate.
		/// </summary>
		/// <param name="pixel">The pixel coordinate to convert.</param>
		/// <returns>The texel space location of the pixel on the texture.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown when the texture width or height is equal to 0.</exception>
		public Vector2 ToTexel(Vector2 pixel)
		{
#if DEBUG
			if (Settings.Width == 0)
				throw new DivideByZeroException("The texture width is 0.");
			if (Settings.Height == 0)
				throw new DivideByZeroException("The texture height is 0.");
#endif

			return new Vector2(pixel.X / Settings.Width, pixel.Y / Settings.Height);
		}

		/// <summary>
		/// Function to convert a texel space coordinate into a pixel space coordinate.
		/// </summary>
		/// <param name="texel">The texel coordinate to convert.</param>
		/// <param name="result">The texel converted to pixel space.</param>
		public void ToPixel(ref Vector2 texel, out Vector2 result)
		{
			result = new Vector2(texel.X * Settings.Width, texel.Y * Settings.Height);
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel space coordinate.
		/// </summary>
		/// <param name="pixel">The pixel coordinate to convert.</param>
		/// <param name="result">The texel space location of the pixel on the texture.</param>
		/// <exception cref="System.DivideByZeroException">Thrown when the texture width or height is equal to 0.</exception>
		public void ToTexel(ref Vector2 pixel, out Vector2 result)
		{
#if DEBUG
			if (Settings.Width == 0)
				throw new DivideByZeroException("The texture width is 0.");
			if (Settings.Height == 0)
				throw new DivideByZeroException("The texture height is 0.");
#endif
			result = new Vector2(pixel.X / Settings.Width, pixel.Y / Settings.Height);
		}

		/// <summary>
		/// Function to return the index of a sub resource (mip level, array item, etc...) in a texture.
		/// </summary>
		/// <param name="mipLevel">Mip level to look up.</param>
		/// <param name="arrayIndex">Array index to look up.</param>
		/// <param name="mipCount">Number of mip map levels in the texture.</param>
		/// <param name="arrayCount">Number of array indices in the texture.</param>
		/// <returns>The sub resource index.</returns>
		public static int GetSubResourceIndex(int mipLevel, int arrayIndex, int mipCount, int arrayCount)
		{
			if (arrayCount < 1)
				arrayCount = 1;
			if (arrayCount >= 2048)
				arrayCount = 2048;

			// Constrain to settings.
			if (mipLevel < 0)
				mipLevel = 0;
			if (arrayIndex < 0)
				arrayIndex = 0;
			if (mipLevel >= mipCount)
				mipLevel = mipCount - 1;
			if (arrayIndex >= arrayCount)
				arrayIndex = arrayCount - 1;

			return D3D.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, mipCount);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <param name="sourceRegion">Region on the source texture to copy.</param>
		/// <param name="destination">Destination point to copy to.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRegion"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="M:GorgonLibrary.Graphics.GorgonTexture2D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// <para>Pass NULL (Nothing in VB.Net) to the sourceRegion parameter to copy the entire sub resource.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
        /// <para>Video devices with a feature level of SM2_a_b cannot copy textures from GPU memory into staging textures, doing so will throw an exception.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture is a staging texture and the source texture is not a staging texture.</exception>
		public void CopySubResource(GorgonTexture2D texture, int subResource, int destSubResource, Rectangle? sourceRegion, Vector2 destination)
		{
			GorgonDebug.AssertNull<GorgonTexture>(texture, "texture");

#if DEBUG
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (Settings.Usage == BufferUsage.Staging) && (texture.Settings.Usage != BufferUsage.Staging))
            {
                throw new NotSupportedException("Feature level SM2_a_b video devices cannot copy sub resources from non staging textures to staging textures.");
            }

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count) || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
				throw new ArgumentException("Cannot copy textures with different multisampling parameters.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((texture.FormatInformation.Group != FormatInformation.Group) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((this == texture) && (subResource == destSubResource))
				throw new ArgumentException("Cannot copy to and from the same sub resource on the same texture.");
#endif

			// If we have multisampling enabled, then copy the entire sub resource.            
            if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0) || (sourceRegion == null))
            {
                Graphics.Context.CopySubresourceRegion(texture.D3DResource, subResource, null, this.D3DResource, destSubResource, 0, 0, 0);
            }
            else
            {
                Graphics.Context.CopySubresourceRegion(texture.D3DResource, subResource, new D3D.ResourceRegion()
                    {
                        Back = 1,
                        Front = 0,
                        Top = sourceRegion.Value.Top,
                        Left = sourceRegion.Value.Left,
                        Right = sourceRegion.Value.Right,
                        Bottom = sourceRegion.Value.Bottom
                    }, this.D3DResource, destSubResource, (int)destination.X, (int)destination.Y, 0);
            }
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="sourceRegion">Region on the source texture to copy.</param>
		/// <param name="destination">Destination point to copy to.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRegion"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture2D texture, Rectangle sourceRegion, Vector2 destination)
		{
#if DEBUG
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");
#endif

			CopySubResource(texture, 0, 0, sourceRegion, destination);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture2D texture)
		{
#if DEBUG
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");
#endif

			CopySubResource(texture, 0, 0, null, Vector2.Zero);
		}

		/// <summary>
		/// Function to copy a texture sub resource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The source texture must fit within the dimensions of this texture.  If it does not, then the copy will be clipped so that it fits.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="M:GorgonLibrary.Graphics.GorgonTexture2D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture2D texture, int subResource, int destSubResource)
		{
			GorgonDebug.AssertNull<GorgonTexture>(texture, "texture");

			CopySubResource(texture, subResource, destSubResource, null, Vector2.Zero);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="destRect">Destination region to copy into.</param>
		/// <remarks>Use this to copy data to this texture.  If the texture is non CPU accessible texture then an exception is raised.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="destRect"/> parameter is less than 0 or larger than this texture.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has multisampling applied.</para>
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a depth/stencil buffer texture.</para>
		/// </exception>
		public void UpdateSubResource(ISubResourceData data, int subResource, Rectangle destRect)
		{
#if DEBUG
			if ((Settings.Usage == BufferUsage.Dynamic) || (Settings.Usage == BufferUsage.Immutable))
				throw new InvalidOperationException("Cannot update a texture that is Dynamic or Immutable");

			if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0))
				throw new InvalidOperationException("Cannot update a texture that is multisampled.");

			if (IsDepthStencil)
				throw new InvalidOperationException("Cannot update a texture used as a depth/stencil buffer.");

#endif
			Rectangle textureSize = new Rectangle(0, 0, Settings.Width, Settings.Height);
			if (!textureSize.Contains(destRect))
				destRect = Rectangle.Intersect(destRect, textureSize);

			SharpDX.DataBox box = new SharpDX.DataBox()
			{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			D3D.ResourceRegion region = new D3D.ResourceRegion()
			{
				Front = 0,
				Back = 1,
				Top = destRect.Top,
				Left = destRect.Left,
				Bottom = destRect.Bottom,
				Right = destRect.Right,
			};

			Graphics.Context.UpdateSubresource(box, D3DResource, subResource, region);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Prevents a default instance of the <see cref="GorgonTexture2D"/> class from being created.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="texture">The source texture.</param>
		internal GorgonTexture2D(GorgonGraphics graphics, string name, D3D.Texture2D texture)
			: base(graphics, name, null)
		{
			D3DResource = texture;
			base.Settings = GetTextureInformation();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain to get texture information from.</param>
		internal GorgonTexture2D(GorgonSwapChain swapChain)
			: base(swapChain.Graphics, swapChain.Name + "_Internal_Texture_" + Guid.NewGuid().ToString(), null)
		{
			D3DResource = D3D.Texture2D.FromSwapChain<D3D.Texture2D>(swapChain.GISwapChain, 0);

			base.Settings = GetTextureInformation();

			RenderTarget = swapChain;

			if ((swapChain.Settings.Flags & SwapChainUsageFlags.ShaderInput) == SwapChainUsageFlags.ShaderInput)
				CreateDefaultResourceView();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonTexture2D(GorgonGraphics graphics, string name, ITextureSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}