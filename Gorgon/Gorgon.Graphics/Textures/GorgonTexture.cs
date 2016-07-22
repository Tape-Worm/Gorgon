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
// Created: June 13, 2016 8:42:49 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A texture used to project an image onto a graphic primitive such as a triangle.
	/// </summary>
	public class GorgonTexture
		: GorgonResource
	{
		#region Variables.
		// The logging interface used for debug logging.
		private readonly IGorgonLog _log;
		// The texture lock cache.
		private readonly TextureLockCache _lockCache;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the information about the format of the texture.
		/// </summary>
		public GorgonFormatInfo FormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType 
		{
			get
			{
				switch (Info.TextureType)
				{
					case TextureType.Texture1D:
						return ResourceType.Texture1D;
					case TextureType.Texture2D:
						return ResourceType.Texture2D;
					case TextureType.Texture3D:
						return ResourceType.Texture3D;
					default:
						return ResourceType.Unknown;
				}
			}
		}

		/// <summary>
		/// Property to return the default shader view for this texture.
		/// </summary>
		public GorgonTextureShaderView DefaultView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the <see cref="GorgonTextureInfo"/> used to create this texture.
		/// </summary>
		public GorgonTextureInfo Info
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize a 2D image.
		/// </summary>
		/// <param name="image">The image data used to populate the texture.</param>
		private void Initialize2D(IGorgonImage image)
		{
			// TODO: Validate the settings.
			D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

			if (Info.Usage == D3D11.ResourceUsage.Staging)
			{
				cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
			}

			if (Info.Usage == D3D11.ResourceUsage.Dynamic)
			{
				cpuFlags = D3D11.CpuAccessFlags.Write;
			}

			var desc = new D3D11.Texture2DDescription
			           {
				           Format = Info.Format,
				           Width = Info.Width,
				           Height = Info.Height,
				           ArraySize = Info.ArrayCount,
				           Usage = Info.Usage,
				           BindFlags = (D3D11.BindFlags)Info.Binding,
				           CpuAccessFlags = cpuFlags,
				           OptionFlags = Info.IsCubeMap ? D3D11.ResourceOptionFlags.TextureCube : D3D11.ResourceOptionFlags.None,
						   SampleDescription = Info.MultiSampleInfo.ToSampleDesc(),
						   MipLevels = Info.MipLevels
			           };

			if (image == null)
			{
				D3DResource = new D3D11.Texture2D(Graphics.VideoDevice.D3DDevice, desc);
				return;
			}

			// Upload the data to the texture.
			DX.DataBox[] dataBoxes = new DX.DataBox[Info.ArrayCount * Info.MipLevels];

			int boxIndex = 0;
			for (int arrayIndex = 0; arrayIndex < Info.ArrayCount; ++arrayIndex)
			{
				for (int mipIndex = 0; mipIndex < Info.MipLevels; ++mipIndex)
				{
					IGorgonImageBuffer buffer = image.Buffers[mipIndex, arrayIndex];
					dataBoxes[boxIndex++] = new DX.DataBox(new IntPtr(buffer.Data.Address), buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
				}
			}

			D3DResource = new D3D11.Texture2D(Graphics.VideoDevice.D3DDevice, desc, dataBoxes);
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="image">The image used to initialize the texture.</param>
		private void Initialize(IGorgonImage image)
		{
			FormatInformation = new GorgonFormatInfo(Info.Format);

			switch (Info.TextureType)
			{
				case TextureType.Texture1D:
					break;
				case TextureType.Texture2D:
					Initialize2D(image);
					break;
				case TextureType.Texture3D:
					break;
			}

			// We cannot bind staging textures to the pipeline, so we can't have a shader resource view for it.
			if (Info.Usage == D3D11.ResourceUsage.Staging)
			{
				return;
			}

			DefaultView = new GorgonTextureShaderView(this);
		}

		/// <summary>
		/// Function to lock a CPU accessible texture sub resource for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used to lock.</param>
		/// <param name="arrayIndex">Array index of the sub resource to lock.</param>
		/// <param name="mipLevel">The mip-map level of the sub resource to lock.</param>
		/// <returns>A <see cref="GorgonTextureLockData"/> object representing the lock on the texture.</returns>
		/// <exception cref="ArgumentException">Thrown when the texture is not a dynamic or staging texture.</exception>
		/// <exception cref="NotSupportedException">Thrown when this texture is a depth/stencil texture.
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture has a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This method is used to lock down a sub resource in the texture for reading/writing (depending on <see cref="GorgonTextureInfo.Usage"/>). When locking a texture, the entire texture sub resource 
		/// is locked and returned.  There is no setting to return a portion of the texture subresource.
		/// </para>
		/// <para>
		/// This method is only works for textures with a <see cref="GorgonTextureInfo.Usage"/> of <c>Dynamic</c> or <c>Staging</c>. If the usage is not either of those values, then an exception will be thrown. 
		/// If the texture has a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>, then this method will throw an exception.
		/// </para>
		/// <para>
		/// When the texture usage is set to <c>Dynamic</c>, the lock will be write-only, but when the usage is set to <c>Staging</c>, then the lock will allow reading and writing of the texture data. This is 
		/// specified by the <paramref name="lockFlags"/> parameter. If the <c>WriteNoOverwrite</c> flag is supplied, it will be ignored and treated as a <c>Write</c> flag.
		/// </para> 
		/// <para>
		/// <note type="warning">
		/// <para>
		/// All exceptions raised by this method will only be done so when Gorgon is compiled in DEBUG mode.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <returns>This method will return a <see cref="GorgonTextureLockData"/> containing information about the locked sub resource and a pointer to the texture data in memory.</returns>
		public GorgonTextureLockData Lock(D3D11.MapMode lockFlags, int mipLevel = 0, int arrayIndex = 0)
		{
#if DEBUG
			if ((Info.Usage != D3D11.ResourceUsage.Staging) && (Info.Usage != D3D11.ResourceUsage.Dynamic))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_USAGE_CANT_LOCK, Info.Usage));
			}

			if ((Info.Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BINDING_TYPE_CANNOT_BE_USED, TextureBinding.DepthStencil));
			}

			if ((Info.Usage == D3D11.ResourceUsage.Dynamic) &&
			    ((lockFlags == D3D11.MapMode.Read)
			     || (lockFlags == D3D11.MapMode.ReadWrite)))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_LOCK_CANNOT_READ_NON_STAGING);
			}

			if (lockFlags == D3D11.MapMode.WriteNoOverwrite)
			{
				lockFlags = D3D11.MapMode.Write;
			}
#endif

			mipLevel = mipLevel.Min(Info.MipLevels - 1).Max(0);
			arrayIndex = Info.TextureType == TextureType.Texture3D ? 0 : arrayIndex.Min(Info.ArrayCount - 1).Max(0);

			return _lockCache.Lock(lockFlags, mipLevel, arrayIndex);
		}

		/// <summary>
		/// Function to copy another <see cref="GorgonTexture"/>, in its entirty, to this texture.
		/// </summary>
		/// <param name="sourceTexture">The <see cref="GorgonTexture"/> to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceTexture"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device has a feature level of <see cref="FeatureLevelSupport.Level_10_0"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="GorgonTextureInfo.MultiSampleInfo"/>.<see cref="GorgonMultiSampleInfo.Count"/> is not the same for the source <paramref name="sourceTexture"/> and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture sizes are not the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">Thrown when this texture, or the source <paramref name="sourceTexture"/> has a <see cref="GorgonTextureInfo.Usage"/> setting of <c>Immutable</c>.</exception>
		/// <remarks>
		/// <para>
		/// This method copies the contents of the <paramref name="sourceTexture"/> parameter into this texture. If a sub resource for the <paramref name="sourceTexture"/> must be copied, use the <see cref="CopySubResource"/> 
		/// method.
		/// </para>
		/// <para>
		/// This method does not perform stretching, filtering or clipping.
		/// </para>
		/// <para>
		/// The source <paramref name="sourceTexture"/> dimensions must be have the same dimensions, and <see cref="GorgonTextureInfo.MultiSampleInfo"/> as this texture. As well, both textures must not have a 
		/// <see cref="GorgonTextureInfo.Usage"/> of <c>Immutable.</c>. If these contraints are violated, then an exception will be thrown.
		/// </para>
		/// <para>
		/// If the current video device has a feature level better than <see cref="FeatureLevelSupport.Level_10_0"/>, then limited format conversion will be performed if the two textures are within the same bit 
		/// group (e.g. <c>R8G8B8A8_SInt</c> is convertible to <c>R8G8B8A8_UInt</c> and so on, since they are both R8G8B8A8). If the feature level is <see cref="FeatureLevelSupport.Level_10_0"/>, or the bit group 
		/// does not match, then an exception will be thrown.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="CopySubResource"/>
		public void CopyFrom(GorgonTexture sourceTexture)
		{
			sourceTexture.ValidateObject(nameof(sourceTexture));

#if DEBUG
			if (sourceTexture.ResourceType != ResourceType)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_SAME_TYPE, sourceTexture.Name, sourceTexture.ResourceType, ResourceType), nameof(sourceTexture));
			}

			if ((Info.Usage == D3D11.ResourceUsage.Immutable) || (sourceTexture.Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
			}

			if ((Info.MultiSampleInfo.Count != sourceTexture.Info.MultiSampleInfo.Count) || (Info.MultiSampleInfo.Quality != sourceTexture.Info.MultiSampleInfo.Quality))
			{
				throw new InvalidOperationException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLE_PARAMS_MISMATCH);
			}

			// If the format is different, then check to see if the format group is the same.
			if ((sourceTexture.Info.Format != Info.Format) && ((sourceTexture.FormatInformation.Group != FormatInformation.Group) 
				|| (Graphics.VideoDevice.RequestedFeatureLevel == FeatureLevelSupport.Level_10_0)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, sourceTexture.Info.Format, Info.Format), nameof(sourceTexture));
			}

			if ((sourceTexture.Info.Width != Info.Width) || (sourceTexture.Info.Height != Info.Height) || (sourceTexture.Info.Depth != Info.Depth))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_TEXTURE_MUST_BE_SAME_SIZE, nameof(sourceTexture));
			}
#endif

			Graphics.D3DDeviceContext.CopyResource(sourceTexture.D3DResource, D3DResource);
		}

		/// <summary>
		/// Function to copy a texture subresource from another <see cref="GorgonTexture"/> into this texture.
		/// </summary>
		/// <param name="sourceTexture">The texture to copy.</param>
		/// <param name="sourceBox">[Optional] The dimensions of the source area to copy.</param>
		/// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy (for 1D/2D textures only).</param>
		/// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
		/// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
		/// <param name="destY">[Optional] Vertical offset into the destination texture to place the copied data (for 2D/3D textures only).</param>
		/// <param name="destZ">[Optional] Depth offset into the destination texture to place the copied data (for 3D textures only).</param>
		/// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into (for 1D/2D textures only).</param>
		/// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
		/// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
		/// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group or the current video device has a feature level of <see cref="FeatureLevelSupport.Level_10_0"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="sourceTexture"/> is the same as this texture, and the <paramref name="sourceArrayIndex"/>, <paramref name="destArrayIndex"/>, <paramref name="sourceMipLevel"/> and the <paramref name="destMipLevel"/> 
		/// specified are pointing to the same subresource.</para>
		/// <para>-or-</para>
		/// <para>Thrown when this texture or the <paramref name="sourceTexture"/> have a <see cref="GorgonTextureInfo.Usage"/> of <c>Immutable</c>.</para>
		/// </exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <remarks>
		/// <para>
		/// Use this method to copy a specific sub resource of a <see cref="GorgonTexture"/> to another sub resource of this <see cref="GorgonTexture"/>, or to a different sub resource of the same texture.  
		/// The <paramref name="sourceBox"/> coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is 
		/// supported by this method. If the entire texture needs to be copied, then use the <see cref="CopyFrom"/> method.
		/// </para>
		/// <para>
		/// If the current video device has a feature level better than <see cref="FeatureLevelSupport.Level_10_0"/>, then limited format conversion will be performed if the two textures are within the same bit 
		/// group (e.g. <c>R8G8B8A8_SInt</c> is convertible to <c>R8G8B8A8_UInt</c> and so on, since they are both R8G8B8A8). If the feature level is <see cref="FeatureLevelSupport.Level_10_0"/>, or the bit group 
		/// does not match, then an exception will be thrown.
		/// </para>
		/// <para>
		/// When copying sub resources (e.g. mip levels, array indices, etc...), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="CopyFrom"/>
		public void CopySubResource(GorgonTexture sourceTexture, GorgonBox? sourceBox = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destZ = 0, int destArrayIndex = 0, int destMipLevel = 0)
		{
			sourceTexture.ValidateObject(nameof(sourceTexture));

			// If we're trying to place the image data outside of this texture, then leave.
			if ((destX >= Info.Width)
				|| (destY >= Info.Height)
				|| (destZ >= Info.Depth))
			{
				return;
			}

			GorgonBox box;

			// If we didn't specify a box to copy from, then create one.
			if (sourceBox == null)
			{
				box = new GorgonBox
				            {
					            X = 0,
					            Y = 0,
					            Z = 0,
					            Width = (sourceTexture.Info.Width.Max(1)).Min(sourceTexture.Info.Width),
					            Height = (sourceTexture.Info.Height.Max(1)).Min(sourceTexture.Info.Height),
					            Depth = (sourceTexture.Info.Depth.Max(1)).Min(sourceTexture.Info.Depth)
				            };
			}
			else
			{
				box = new GorgonBox
				      {
					      Left = (sourceBox.Value.Left.Min(Info.Height - 1).Max(0)).Min(sourceTexture.Info.Width - 1),
					      Top = (sourceBox.Value.Top.Min(Info.Depth - 1).Max(0)).Min(sourceTexture.Info.Height - 1),
					      Front = (sourceBox.Value.Front.Min(Info.Width - 1).Max(0)).Min(sourceTexture.Info.Depth - 1),
					      Width = (sourceBox.Value.Width.Min(Info.Width).Max(1)).Min(sourceTexture.Info.Width),
					      Height = (sourceBox.Value.Height.Min(Info.Height).Max(1)).Min(sourceTexture.Info.Height),
					      Depth = (sourceBox.Value.Depth.Min(Info.Depth).Max(1)).Min(sourceTexture.Info.Depth)
				      };
			}

			// Ensure the indices are clipped to our settings.
			sourceArrayIndex = sourceTexture.Info.TextureType == TextureType.Texture3D ? 0 : sourceArrayIndex.Min(sourceTexture.Info.ArrayCount - 1).Max(0);
			sourceMipLevel = sourceMipLevel.Min(sourceTexture.Info.MipLevels - 1).Max(0);
			destArrayIndex = Info.TextureType == TextureType.Texture3D ? 0 : destArrayIndex.Min(Info.ArrayCount - 1).Max(0);
			destMipLevel = destMipLevel.Min(Info.MipLevels - 1).Max(0);

			int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, sourceArrayIndex, Info.MipLevels);
			int destResource = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, Info.MipLevels);

#if DEBUG
			// If the format is different, then check to see if the format group is the same.
			if ((sourceTexture.Info.Format != Info.Format)
				&& ((sourceTexture.FormatInformation.Group != FormatInformation.Group)
					|| (Graphics.VideoDevice.RequestedFeatureLevel == FeatureLevelSupport.Level_10_0)))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, sourceTexture.Info.Format, Info.Format));
			}

			if ((Info.Usage == D3D11.ResourceUsage.Immutable) || (sourceTexture.Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
			}


			if ((this == sourceTexture) && (sourceResource == destResource))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE);
			}
#endif

			// Clip off any overlap if the destination is outside of the destination texture.
			if (destX < 0)
			{
				box.X -= destX;
				box.Width += destX;
			}

			if (destY < 0)
			{
				box.Y -= destY;
				box.Height += destY;
			}

			if (destZ < 0)
			{
				box.Z -= destZ;
				box.Depth += destZ;
			}

			// Clip source box.
			int left = box.Left.Min(sourceTexture.Info.Width - 1).Max(0);
			int top = box.Top.Min(sourceTexture.Info.Height - 1).Max(0);
			int front = box.Front.Min(sourceTexture.Info.Depth - 1).Max(0);
			int right = box.Right.Min(sourceTexture.Info.Width + left).Max(1);
			int bottom = box.Bottom.Min(sourceTexture.Info.Height + top).Max(1);
			int back = box.Back.Min(sourceTexture.Info.Depth + front).Max(1);

			box = GorgonBox.FromTLFRBB(left, top, front, right, bottom, back);

			// Adjust source box to fit within our destination.
			destX = destX.Min(Info.Width - 1).Max(0);
			destY = destY.Min(Info.Height - 1).Max(0);
			destZ = destZ.Min(Info.Depth - 1).Max(0);

			box.Width = (destX + box.Width).Min(Info.Width - destX).Max(1);
			box.Height = (destY + box.Height).Min(Info.Height - destY).Max(1);
			box.Depth = (destZ + box.Depth).Min(Info.Depth - destZ).Max(1);

			// Nothing to copy, so get out.
			if ((box.Width == 0)
				|| (box.Height == 0)
				|| (box.Depth == 0))
			{
				return;
			}

			Graphics.D3DDeviceContext.CopySubresourceRegion(sourceTexture.D3DResource,
			                                                sourceResource,
			                                                box.ToResourceRegion(),
			                                                D3DResource,
			                                                destResource,
			                                                destX,
			                                                destY,
			                                                destZ);
		}

		/// <summary>
		/// Function to resolve a multisampled 2D <see cref="GorgonTexture"/> into a non-multisampled <see cref="GorgonTexture"/>.
		/// </summary>
		/// <param name="destination">The <see cref="GorgonTexture"/> that will receive the resolved texture.</param>
		/// <param name="resolveFormat">[Optional] A format that will determine how to resolve the multisampled texture into a non-multisampled texture.</param>
		/// <param name="destArrayIndex">[Optional] Index in the array that will receive the resolved texture data (for 1D/2D textures only).</param>
		/// <param name="destMipLevel">[Optional] The mip map level that will receive the resolved texture data.</param>
		/// <param name="srcArrayIndex">[Optional] The array index in the source to resolve (for 1D/2D textures only).</param>
		/// <param name="srcMipLevel">[Optional] The source mip level to resolve.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the format of this texture, and the format <paramref name="destination"/> texture are not typeless, and are not the same format.
		/// <para>-or-</para>
		/// <para>Thrown when the format of both this texture and the <paramref name="destination"/> texture are typeless, but the resolve format is not set to a bit group compatible format, or the textures do not have bit group compatible formats.</para>
		/// <para>-or-</para>
		/// <para>Thrown when either the format of this texture or the <paramref name="destination"/> texture is typless, and the other is not, and the resolve format is not set to a bit group compatible format, or the textures do not have bit group compatible formats.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">Thrown when the source texture is not multisampled or the destination texture is multisampled or has a non default usage.</exception>
		/// <remarks>Use this method to resolve a multisampled texture into a non multisampled texture.  This is most useful when transferring a multisampled render target pass as an input to 
		/// a secondary pass.
		/// <para>The <paramref name="resolveFormat"/> parameter is used to determine how to interpret the data in the texture.  There are 3 ways this data may be interpreted:  
		/// <list type="number">
		/// <item><description>If both textures have a typed format, then the resolve format must be the same as the format of the textures.  Both textures must have the same format.</description></item>
		/// <item><description>If one of the textures have a typeless format and one has a typed format, then the resolve format must be in the same group as the typed format.</description></item>
		/// <item><description>If the textures both have a typeless format, then the resolve format must be in the same group as the typeless format.</description></item>
		/// </list>
		/// Leaving the resolve format as Unknown will automatically use the format of the source texture.
		/// </para>
		/// </remarks>
		public void ResolveTo(GorgonTexture destination, DXGI.Format resolveFormat = DXGI.Format.Unknown, int destArrayIndex = 0, int destMipLevel = 0, int srcArrayIndex = 0, int srcMipLevel = 0)
		{
			destination.ValidateObject(nameof(destination));

			destArrayIndex = destArrayIndex.Min(destination.Info.ArrayCount - 1).Max(0);
			destMipLevel = destMipLevel.Min(destination.Info.MipLevels - 1).Max(0);
			srcArrayIndex = srcArrayIndex.Min(Info.ArrayCount - 1).Max(0);
			srcMipLevel = srcMipLevel.Min(Info.MipLevels - 1).Max(0);

			// If the formats for the textures are identical, and we've not specified a format, then we need to 
			// tell the resolve function that we have to use the format of the textures.
			if ((resolveFormat == DXGI.Format.Unknown) && (destination.Info.Format == Info.Format))
			{
				resolveFormat = Info.Format;
			}

#if DEBUG
			if (Info.MultiSampleInfo.Equals(GorgonMultiSampleInfo.NoMultiSampling))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_MULTISAMPLED, Name));
			}

			if (destination.Info.Usage != D3D11.ResourceUsage.Default)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_DEST_NOT_DEFAULT, destination.Name));
			}

			var resolveFormatInfo = new GorgonFormatInfo(resolveFormat);

			// If we have typed formats, and they're not the same, then that's an error according to the D3D docs.
			if ((!FormatInformation.IsTypeless) && (!destination.FormatInformation.IsTypeless))
			{
				if (Info.Format != destination.Info.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMATS_NOT_SAME, Info.Format), nameof(destination));
				}
			}

			// If both formats are typeless, then both formats must be the same and the resolve format must be set to a compatible format.
			if ((FormatInformation.IsTypeless) && (destination.FormatInformation.IsTypeless))
			{
				if (Info.Format != destination.Info.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMATS_NOT_SAME, Info.Format), nameof(destination));
				}

				if (resolveFormatInfo.Group != FormatInformation.Group)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMAT_NOT_SAME_GROUP, Info.Format), nameof(resolveFormat));
				}
			}

			// If one format is typeless, and the other is not, then the formats must be compatible and the resolve format must be specified.
			if ((FormatInformation.IsTypeless) || (destination.FormatInformation.IsTypeless))
			{
				if (resolveFormatInfo.IsTypeless)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMAT_CANNOT_BE_TYPELESS), nameof(resolveFormat));
				}

				if ((FormatInformation.Group != destination.FormatInformation.Group) 
					|| ((resolveFormatInfo.Group != FormatInformation.Group) && (resolveFormatInfo.Group != destination.FormatInformation.Group)))
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_TEXTURE_RESOLVE_SRC_DEST_NOT_SAME_GROUP, Info.Format, destination.Info.Format),
					                            nameof(destination));
				}
			}
#endif

			int sourceIndex = D3D11.Resource.CalculateSubResourceIndex(srcMipLevel, srcArrayIndex, Info.MipLevels);
			int destIndex = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, destination.Info.MipLevels);

			Graphics.D3DDeviceContext.ResolveSubresource(D3DResource, sourceIndex, destination.D3DResource, destIndex, resolveFormat);
		}

		/// <summary>
		/// Function to get a staging texture from this texture.
		/// </summary>
		/// <returns>A new <see cref="GorgonTexture"/> containing a copy of the data in this texture, with a usage of <c>Staging</c>.</returns>
		/// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonTextureInfo.Usage"/> of <c>Immutable</c>.</exception>
		/// <remarks>
		/// <para>
		/// This allows an application to make a copy of the texture for editing on the CPU. The resulting staging texture, once edited, can then be reuploaded to the same texture, or another texture.
		/// </para>
		/// <para>
		/// The staging texture returned from this texture is tracked by this texture, and will be destroyed when this texture is destroyed. This also means that the instance returned will be the same instance 
		/// as the original staging teunless the result staging texture is disposed.
		/// </para>
		/// </remarks>
		public GorgonTexture GetStagingTexture()
		{
			if (Info.Usage == D3D11.ResourceUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE));
			}

			GorgonTextureInfo info = Info.Clone();
			info.Usage = D3D11.ResourceUsage.Staging;
			info.Binding = TextureBinding.None;
			var staging = new GorgonTexture(Name + " [Staging]", Graphics, info);

			// Copy the data from this texture into the new staging texture.
			staging.CopyFrom(this);

			return staging;
		}

		/// <summary>
		/// Function to copy data from the CPU using a <see cref="IGorgonImageBuffer"/> to this texture on the GPU.
		/// </summary>
		/// <param name="buffer">A <see cref="IGorgonImageBuffer"/> containing the image data to copy.</param>
		/// <param name="destBox">[Optional] A <see cref="GorgonBox"/> that will specify the region that will receive the data.</param>
		/// <param name="destArrayIndex">[Optional] The array index that will receive the data (1D/2D textures only).</param>
		/// <param name="destMipLevel">[Optional] The mip map level that will receive the data.</param>
		/// <exception cref="NotSupportedException">Thrown when this texture has a <see cref="GorgonTextureInfo.Usage"/> of <c>Dynamic</c> or <c>Immutable</c>.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has <see cref="GorgonTextureInfo.MultiSampleInfo"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown when this texture has a <see cref="GorgonTextureInfo.Binding"/> with the <see cref="TextureBinding.DepthStencil"/> flag set.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Use this to copy data into a texture with a <see cref="GorgonTextureInfo.Usage"/> of <c>Staging</c> or <c>Default</c>.  If the texture does not have a <c>Staging</c> or <c>Default</c> usage, then an 
		/// exception will be thrown.
		/// </para>
		/// <para>
		/// If the <paramref name="destBox"/> is specified, then a portion of the texture will be written into.  If the dimensions are larger than that of this texture, then the dimensions will be clipped to the 
		/// dimensions of the texture if they are larger. If this parameter is omitted, then the entire texture will be used (or up to the size of the <paramref name="buffer"/>).
		/// </para>
		/// <para>
		/// If the <paramref name="destArrayIndex"/>, or the <paramref name="destMipLevel"/> values are specified, then the data will be written into the array index if the texture is a 1D or 2D texture, otherwise 
		/// this value is ignored.
		/// </para>
		/// <para>
		/// This method will not work with textures that have a <see cref="GorgonTextureInfo.Binding"/> that includes the <see cref="TextureBinding.DepthStencil"/> flag. If this texture has the 
		/// <see cref="TextureBinding.DepthStencil"/> flag, then an exception will be thrown.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void UpdateSubResource(IGorgonImageBuffer buffer, GorgonBox? destBox = null, int destArrayIndex = 0, int destMipLevel = 0)
		{
			buffer.ValidateObject(nameof(buffer));

#if DEBUG
			if ((Info.Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BINDING_TYPE_CANNOT_BE_USED, TextureBinding.DepthStencil));
			}

			if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IS_DYNAMIC_OR_IMMUTABLE);
			}

			if ((Info.MultiSampleInfo.Count > 1) || (Info.MultiSampleInfo.Quality > 0))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
			}
#endif

			destMipLevel = destMipLevel.Min(Info.MipLevels - 1).Max(0);
			destArrayIndex = Info.TextureType == TextureType.Texture3D ? 0 : destArrayIndex.Min(Info.ArrayCount - 1).Max(0);

			GorgonBox box;

			if (destBox == null)
			{
				box = new GorgonBox
				      {
					      Front = 0,
					      Top = 0,
					      Left = 0,
					      Width = (Info.Width.Max(1)).Min(buffer.Width),
					      Height = (Info.Height.Max(1)).Min(buffer.Height),
					      Depth = (Info.Depth.Max(1)).Min(buffer.Depth)
				      };
			}
			else
			{
				// Ensure the box fits the source.
				box = new GorgonBox
				      {
					      Front = (destBox.Value.Front.Min(Info.Depth - 1).Max(0)).Min(buffer.Depth - 1),
					      Left = (destBox.Value.Left.Min(Info.Width - 1).Max(0)).Min(buffer.Width - 1),
					      Top = (destBox.Value.Top.Min(Info.Height - 1).Max(0)).Min(buffer.Height - 1),
					      Depth = (destBox.Value.Depth.Min(Info.Depth).Max(1)).Min(buffer.Depth),
					      Width = (destBox.Value.Width.Min(Info.Width).Max(1)).Min(buffer.Width),
					      Height = (destBox.Value.Height.Min(Info.Height).Max(1)).Min(buffer.Height)
				      };
			}

			var boxPtr = new DX.DataBox
			{
				DataPointer = new IntPtr(buffer.Data.Address),
				RowPitch = buffer.PitchInformation.RowPitch,
				SlicePitch = buffer.PitchInformation.SlicePitch
			};

			Graphics.D3DDeviceContext.UpdateSubresource(boxPtr,
			                                            D3DResource,
			                                            D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, Info.MipLevels),
			                                            box.ToResourceRegion());
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			DefaultView?.Dispose();

			if (D3DResource == null)
			{
				return;
			}

			_log.Print($"'{Name}': Destroying D3D11 Texture.", LoggingLevel.Simple);

			base.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain that holds the back buffers to retrieve.</param>
		/// <param name="index">The index of the back buffer to retrieve.</param>
		/// <param name="log">The log used for debug output.</param>
		/// <remarks>
		/// <para>
		/// This constructor is used internally to create a render target texture from a swap chain.
		/// </para>
		/// </remarks>
		internal GorgonTexture(GorgonSwapChain swapChain, int index, IGorgonLog log)
			: base(swapChain.Graphics, $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.")
		{
			_log = log;

			_log.Print($"Swap Chain '{swapChain.Name}': Creating texture from back buffer index {index}.", LoggingLevel.Simple);
			
			D3D11.Texture2D texture;
			
			// Get the resource from the swap chain.
			D3DResource = texture = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(swapChain.GISwapChain, index);
			D3DResource.DebugName = $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.";

			// Get the info from the back buffer texture.
			Info = new GorgonTextureInfo
			       {
				       Format = texture.Description.Format,
				       Width = texture.Description.Width,
				       Height = texture.Description.Height,
				       TextureType = TextureType.Texture2D,
				       Usage = texture.Description.Usage,
				       ArrayCount = texture.Description.ArraySize,
				       MipLevels = texture.Description.MipLevels,
				       Depth = 0,
				       IsCubeMap = false,
				       MultiSampleInfo = GorgonMultiSampleInfo.NoMultiSampling,
				       Binding = (TextureBinding)texture.Description.BindFlags
			       };

			FormatInformation = new GorgonFormatInfo(Info.Format);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="name">The name of the texture.</param>
		/// <param name="graphics">The graphics interface used to create this texture.</param>
		/// <param name="image">The image to copy into the texture.</param>
		/// <param name="info">The information used to define how the texture should be created.</param>
		/// <param name="log">The log interface used for debugging.</param>
		/// <remarks>
		/// <para>
		/// This constructor is used when converting an image to a texture.
		/// </para>
		/// </remarks>
		internal GorgonTexture(string name, GorgonGraphics graphics, IGorgonImage image, GorgonImageTextureInfo info, IGorgonLog log)
			: base(graphics, name)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;

			TextureType type;

			switch (image.Info.ImageType)
			{
				case ImageType.Image1D:
					type = TextureType.Texture1D;
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					type = TextureType.Texture2D;
					break;
				case ImageType.Image3D:
					type = TextureType.Texture3D;
					break;
				default:
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_IMAGE_TYPE_UNSUPPORTED, image.Info.ImageType), nameof(image));
			}

			Info = new GorgonTextureInfo
			       {
				       Format = image.Info.Format,
				       Width = image.Info.Width,
				       Height = image.Info.Height,
				       TextureType = type,
				       Usage = info.Usage,
				       ArrayCount = image.Info.ArrayCount,
				       Binding = info.Binding,
				       Depth = image.Info.Depth,
				       IsCubeMap = image.Info.ImageType == ImageType.ImageCube,
				       MipLevels = image.Info.MipCount,
				       MultiSampleInfo = info.MultiSampleInfo
			       };

			Initialize(image);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="name">The name of the texture.</param>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> interface that created this texture.</param>
		/// <param name="textureInfo">A <see cref="GorgonTextureInfo"/> object describing the properties of this texture.</param>
		/// <param name="log">[Optional] The logging interface used for debugging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="graphics"/>, or the <paramref name="textureInfo"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="GorgonTextureInfo.Usage"/> is set to <c>Immutable</c>.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture could not be created due to misconfiguration.</exception>
		/// <remarks>
		/// <para>
		/// This constructor creates an empty texture. Data may be uploaded to the texture at a later time if its <see cref="GorgonTextureInfo.Usage"/> is not set to <c>Immutable</c>. If the 
		/// <see cref="GorgonTextureInfo.Usage"/> is set to <c>immutable</c> with this constructor, then an exception will be thrown. To use an immutable texture, use the 
		/// <see cref="GorgonImageTextureExtensions.ToTexture"/> extension method on the <see cref="IGorgonImage"/> type.
		/// </para>
		/// </remarks>
		public GorgonTexture(string name, GorgonGraphics graphics, GorgonTextureInfo textureInfo, IGorgonLog log = null)
			: base(graphics, name)
		{
			if (textureInfo == null)
			{
				throw new ArgumentNullException(nameof(textureInfo));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			
			Info = textureInfo.Clone();

			Initialize(null);

			_lockCache = new TextureLockCache(this);
		}
		#endregion
	}
}
