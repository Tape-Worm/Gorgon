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
// Created: July 20, 2016 10:40:09 PM
// 
#endregion

using System;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Extension methods used to create textures from images.
	/// </summary>
	public static class GorgonImageTextureExtensions
	{
		/// <summary>
		/// Function to transfer texture data into an image buffer.
		/// </summary>
		/// <param name="texture">The texture to copy from.</param>
		/// <param name="arrayIndex">The index of the array to copy from.</param>
		/// <param name="mipLevel">The mip level to copy from.</param>
		/// <param name="buffer">The buffer to copy into.</param>
		private static unsafe void GetTextureData(GorgonTexture texture, int arrayIndex, int mipLevel, IGorgonImageBuffer buffer)
		{
			int depthCount = 1.Max(buffer.Depth);
			int height = 1.Max(buffer.Height);
			int rowStride = buffer.PitchInformation.RowPitch;
			int sliceStride = buffer.PitchInformation.SlicePitch;
			D3D11.MapMode flags = D3D11.MapMode.ReadWrite;

			// If this image is compressed, then use the block height information.
			if (buffer.PitchInformation.VerticalBlockCount > 0)
			{
				height = buffer.PitchInformation.HorizontalBlockCount;
			}

			// Copy the texture data into the buffer.
			GorgonTextureLockData textureLock;
			switch (texture.Info.TextureType)
			{
				case TextureType.Texture1D:
				case TextureType.Texture2D:
					textureLock = texture.Lock(flags, mipLevel, arrayIndex);
					break;
				case TextureType.Texture3D:
					textureLock = texture.Lock(flags, mipLevel);
					break;
				default:
					throw new ArgumentException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, texture.Info.TextureType), nameof(texture));
			}

			var bufferPtr = (byte*)buffer.Data.Address;

			using (textureLock)
			{
				// If the strides don't match, then the texture is using padding, so copy one scanline at a time for each depth index.
				if ((textureLock.PitchInformation.RowPitch != rowStride)
					|| (textureLock.PitchInformation.SlicePitch != sliceStride))
				{
					byte* destData = bufferPtr;
					var sourceData = (byte*)textureLock.Data.Address;

					for (int depth = 0; depth < depthCount; depth++)
					{
						// Restart at the padded slice size.
						byte* sourceStart = sourceData;

						for (int row = 0; row < height; row++)
						{
							DirectAccess.MemoryCopy(destData, sourceStart, rowStride);
							sourceStart += textureLock.PitchInformation.RowPitch;
							destData += rowStride;
						}

						sourceData += textureLock.PitchInformation.SlicePitch;
					}
				}
				else
				{
					// Since we have the same row and slice stride, copy everything in one shot.
					DirectAccess.MemoryCopy(bufferPtr, (byte*)textureLock.Data.Address, sliceStride);
				}
			}
		}

		/// <summary>
		/// Function to create a <see cref="GorgonTexture"/> from a <see cref="GorgonImage"/>.
		/// </summary>
		/// <param name="image">The image used to create the texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="graphics">The graphics interface used to create the texture.</param>
		/// <param name="info">[Optional] Defines parameters for creating the <see cref="GorgonTexture"/>.</param>
		/// <param name="log">[Optional] The log interface used for debugging.</param>
		/// <returns>A new <see cref="GorgonTexture"/> containing the data from the <paramref name="image"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, <paramref name="graphics"/> or the <paramref name="name"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <remarks>
		/// <para>
		/// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
		/// <see cref="GorgonImage"/> into a <see cref="GorgonTexture"/>. 
		/// </para>
		/// <para>
		/// The resulting <see cref="GorgonTexture"/> will inherit the <see cref="ImageType"/> (converted to the appropriate <see cref="TextureType"/>), width, height (for 2D/3D images), depth (for 3D images), 
		/// mip map count, array count (for 1D/2D images), and depth count (for 3D images). If the <see cref="GorgonImage"/> being converted has an <see cref="ImageType"/> of <see cref="ImageType.ImageCube"/> 
		/// then the resulting texture will be set to a <see cref="TextureType.Texture2D"/>, and it will have its <see cref="GorgonTextureInfo.IsCubeMap"/> flag set to <b>true</b>.
		/// </para>
		/// <para>
		/// The <paramref name="info"/> parameter, when defined, will allow users to control how the texture is bound to the GPU pipeline, and what its intended usage is going to be, as well as any multisample 
		/// information required to create the texture as a multisample texture. If this parameter is omitted, then the following defaults will be used:
		/// <list type="bullet">
		///		<item>
		///			<term>Binding</term>
		///			<description><see cref="TextureBinding.ShaderResource"/></description>
		///		</item>
		///		<item>
		///			<term>Usage</term>
		///			<description><c>Default</c></description>
		///		</item>
		///		<item>
		///			<term>Multisample info</term>
		///			<description><see cref="GorgonMultiSampleInfo.NoMultiSampling"/></description>
		///		</item>
		/// </list>
		/// </para>
		/// </remarks>
		public static GorgonTexture ToTexture(this IGorgonImage image,
		                                      string name,
											  GorgonGraphics graphics,
		                                      GorgonImageTextureInfo info = null,
											  IGorgonLog log = null)
		{
			if (image == null)
			{
				throw new ArgumentNullException(nameof(image));
			}

			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			return new GorgonTexture(name, graphics, image, info ?? new GorgonImageTextureInfo(), log);
		}

		/// <summary>
		/// Function to convert a texture to an image.
		/// </summary>
		/// <param name="texture">The texture to convert to an image.</param>
		/// <returns>A new <see cref="GorgonImage"/> containing the texture data.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="texture"/> has a <see cref="GorgonTextureInfo.Usage"/> set to <c>Immutable</c>.
		/// <para>-or-</para>
		/// <para>Thrown when the type of texture is not supported.</para>
		/// </exception>
		public static IGorgonImage ToImage(this GorgonTexture texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			if (texture.Info.Usage == D3D11.ResourceUsage.Immutable)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE), nameof(texture));
			}

			GorgonTexture stagingTexture = texture;
			GorgonImage image = null;

			try
			{
				if (texture.Info.Usage != D3D11.ResourceUsage.Staging)
				{
					stagingTexture = texture.GetStagingTexture();
				}

				ImageType imageType;
				switch (stagingTexture.Info.TextureType)
				{
					case TextureType.Texture1D:
						imageType = ImageType.Image1D;
						break;
					case TextureType.Texture2D:
						imageType = stagingTexture.Info.IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;
						break;
					case TextureType.Texture3D:
						imageType = ImageType.Image3D;
						break;
					default:
						throw new ArgumentException(string.Format(Resources.GORGFX_ERR_IMAGE_TYPE_UNSUPPORTED, stagingTexture.Info.TextureType));
				}

				image = new GorgonImage(new GorgonImageInfo(imageType, stagingTexture.Info.Format)
				                        {
					                        Width = texture.Info.Width,
					                        Height = texture.Info.Height,
					                        Depth = texture.Info.Depth,
					                        ArrayCount = texture.Info.ArrayCount,
					                        MipCount = texture.Info.MipLevels
				                        });

				for (int array = 0; array < stagingTexture.Info.ArrayCount; array++)
				{
					for (int mipLevel = 0; mipLevel < stagingTexture.Info.MipLevels; mipLevel++)
					{
						// Get the buffer for the array and mip level.
						var buffer = image.Buffers[mipLevel, array];

						// Copy the data from the texture.
						GetTextureData(stagingTexture, array, mipLevel, buffer);
					}
				}

				return image;
			}
			catch
			{
				image?.Dispose();
				throw;
			}
			finally
			{
				if (stagingTexture != texture)
				{
					stagingTexture?.Dispose();
				}
			}
		}
	}
}
