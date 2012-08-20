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
// Created: Monday, February 13, 2012 7:22:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GI = SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A 1 dimension texture object.
	/// </summary>
	/// <remarks>A 1 dimensional texture only has a width.  This is useful as a buffer of linear data in texture format.
	/// <para>This texture type cannot be created by SM2_a_b video devices.</para></remarks>
	public class GorgonTexture1D
		: GorgonTexture
	{
		#region Properties.
		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public new GorgonTexture1DSettings Settings
		{
			get
			{
				return (GorgonTexture1DSettings)base.Settings;
			}
			private set
			{
				base.Settings = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read image data from an array of bytes.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		/// <param name="imageInfo">Information to pass to the image loading method.</param>
		protected override void InitializeImpl(byte[] imageData, D3D.ImageLoadInformation imageInfo)
		{
			// We don't need depth or height for this guy.
			imageInfo.Depth = 0;
			imageInfo.Height = 0;
						
			D3DTexture = D3D.Texture1D.FromMemory<D3D.Texture1D>(Graphics.D3DDevice, imageData, imageInfo);
		}

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		protected override void InitializeImpl(IEnumerable<ISubResourceData> initialData)
		{
			D3D.Texture1DDescription desc = new D3D.Texture1DDescription();

			desc.ArraySize = Settings.ArrayCount;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
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

			if ((initialData != null) && (initialData.Count() > 0))
			{
				DX.DataStream[] streams = GorgonTexture1DData.Convert(initialData);
				D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc, streams);

				// Clean up.
				foreach (var stream in streams)
					stream.Dispose();
			}
			else
				D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc);
		}

		/// <summary>
		/// Function to convert a texel space coordinate into a pixel space coordinate.
		/// </summary>
		/// <param name="texel">The texel coordinate to convert.</param>
		/// <returns>The pixel location of the texel on the texture.</returns>
		public float ToPixel(float texel)
		{
			return texel * Settings.Width;
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel space coordinate.
		/// </summary>
		/// <param name="pixel">The pixel coordinate to convert.</param>
		/// <returns>The texel space location of the pixel on the texture.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown when the texture width is equal to 0.</exception>
		public float ToTexel(float pixel)
		{
#if DEBUG
			if (Settings.Width == 0)
				throw new DivideByZeroException("The texture width is 0.");
#endif

			return pixel / Settings.Width;
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
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>A 1D dimensional texture can only be saved to DDS format.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is not DDS.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			if (format != ImageFileFormat.DDS)
				throw new ArgumentException("The file format for a 1D texture can only be DDS.", "format");

			D3D.Resource.ToStream<D3D.Texture1D>(Graphics.Context, (D3D.Texture1D)D3DTexture, D3D.ImageFileFormat.Dds, stream);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <param name="sourceRange">Width of the source texture to copy.</param>
		/// <param name="destination">Width of the destination area.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRange"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>The sourceRange uses absolute coorindates.  That is, Minimum is the Left coordinate, and Maximum is the Right coordinate.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="M:GorgonLibrary.Graphics.GorgonTexture1D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// <para>Pass NULL (Nothing in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture1D texture, int subResource, int destSubResource, GorgonMinMax? sourceRange, int destination)
		{
			GorgonDebug.AssertNull<GorgonTexture1D>(texture, "texture");

#if DEBUG
			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((string.Compare(texture.FormatInformation.Group, FormatInformation.Group, true) != 0) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((this == texture) && (subResource == destSubResource))
				throw new ArgumentException("Cannot copy to and from the same sub resource on the same texture.");
#endif

			// If we have multisampling enabled, then copy the entire sub resource.
			CopySubResourceProxy(texture, this, subResource, destSubResource, new D3D.ResourceRegion()
			{
				Back = 1,
				Front = 0,
				Top = 0,
				Left = sourceRange.Value.Minimum,
				Right = sourceRange.Value.Maximum,
				Bottom = 1
			}, destination, 0, 0);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="sourceRange">Region on the source texture to copy.</param>
		/// <param name="destination">Destination point to copy to.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRange"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>The sourceRange uses absolute coorindates.  That is, Minimum is the Left coordinate, and Maximum is the Right coordinate.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture1D texture, GorgonMinMax sourceRange, int destination)
		{
#if DEBUG
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");
#endif

			CopySubResource(texture, 0, 0, sourceRange, destination);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture1D texture)
		{
#if DEBUG
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");
#endif

			CopySubResource(texture, 0, 0, null, 0);
		}

		/// <summary>
		/// Function to copy a texture sub resource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The source texture must fit within the dimensions of this texture.  If it does not, then the copy will be clipped so that it fits.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="M:GorgonLibrary.Graphics.GorgonTexture1D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubResource(GorgonTexture1D texture, int subResource, int destSubResource)
		{
			CopySubResource(texture, subResource, destSubResource, null, 0);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="destRange">The destination range to write into.</param>
		/// <remarks>Use this to copy data to this texture.  If the texture is non CPU accessible texture then an exception is raised.
		/// <para>The destRange uses absolute coorindates.  That is, Minimum is the Left coordinate, and Maximum is the Right coordinate.</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// </exception>
		public void UpdateSubResource(ISubResourceData data, int subResource, GorgonMinMax destRange)
		{
#if DEBUG
			if ((Settings.Usage == BufferUsage.Dynamic) || (Settings.Usage == BufferUsage.Immutable))
				throw new InvalidOperationException("Cannot update a texture that is Dynamic or Immutable");
#endif

			if (destRange.Minimum < 0)
				destRange.Minimum = 0;
			if (destRange.Minimum >= Settings.Width) 
				destRange.Minimum = Settings.Width - 1;
			if (destRange.Maximum < 0)
				destRange.Maximum = 0;
			if (destRange.Maximum >= Settings.Width)
				destRange.Maximum = Settings.Width -1;

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
				Top = 0,
				Bottom = 1,
				Left = destRange.Minimum,
				Right = destRange.Maximum
			};

			Graphics.Context.UpdateSubresource(box, D3DTexture, subResource, region);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture1D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonTexture1D(GorgonGraphics graphics, string name, ITextureSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
