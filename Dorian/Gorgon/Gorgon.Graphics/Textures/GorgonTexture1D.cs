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
using GorgonLibrary.Diagnostics;
using GI = SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

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
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType
		{
			get
			{
				return ResourceType.Texture1D;
			}
		}

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
			return new GorgonTexture1DData(dataStream);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		protected override void UpdateSubResourceImpl(ISubResourceData data, int subResource)
		{
			var box = new SharpDX.DataBox()
			{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			var region = new D3D.ResourceRegion();

			region.Front = 0;
			region.Back = 1;
			region.Left = 0;
			region.Right = Settings.Width;
			region.Top = 0;
			region.Bottom = 1;

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

			var settings1D = new GorgonTexture1DSettings()
			{
				ArrayCount = Settings.ArrayCount,
				Format = Settings.Format,
				Width = Settings.Width,
				MipCount = Settings.MipCount,
				Usage = BufferUsage.Staging
			};

			staging = Graphics.Textures.CreateTexture<GorgonTexture1D>(Name + ".Staging", settings1D);
			staging.Copy(this);

			return staging;
		}

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>
		/// The <paramref name="initialData" /> can be NULL (Nothing in VB.Net) IF the texture is not created with an Immutable usage flag.
		/// <para>To initialize the texture, create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object and fill it with image information.</para>
		/// </remarks>
		protected override void InitializeImpl(GorgonImageData initialData)
		{
			var desc = new D3D.Texture1DDescription
				{
					ArraySize = Settings.ArrayCount,
					BindFlags = GetBindFlags(false, false),
					Format = (SharpDX.DXGI.Format)Settings.Format,
					Width = Settings.Width,
					MipLevels = Settings.MipCount,
					OptionFlags = D3D.ResourceOptionFlags.None,
					Usage = (D3D.ResourceUsage)Settings.Usage
				};
			
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

			if ((initialData != null) && (initialData.Count > 0))
			{
				D3DResource = new D3D.Texture1D(Graphics.D3DDevice, desc, initialData.GetDataBoxes());
			}
			else
			{
				D3DResource = new D3D.Texture1D(Graphics.D3DDevice, desc);
			}
		}

		/// <summary>
		/// Function to create an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="mipStart">First mip map level to map to the view.</param>
		/// <param name="arrayStart">The first array index to map to the view.</param>
		/// <param name="arrayCount">The number of array indices to map to the view.</param>
		/// <returns>A new unordered access view for the texture.</returns>
		/// <remarks>Use this to create an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the video device feature level is not SM_5 or better.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="mipStart"/>, <paramref name="arrayStart"/> or <paramref name="arrayCount"/> parameters are less than 0 or greater than or equal to the 
		/// number of mip levels and/or array levels in the texture.
		/// <para>-or-</para>
		/// <para>Thrown if the bit count of the <paramref name="format"/> and the texture format are different, or if format is not in the R32 group and is not in the same group as the texture format.</para>
		/// </exception>
		public GorgonTextureUnorderAccessView CreateUnorderedAccessView(BufferFormat format, int mipStart, int arrayStart,
		                                                                     int arrayCount)
		{
			return OnCreateUnorderedAccessView(format, mipStart, arrayStart, arrayCount);
		}

		/// <summary>
		/// Function to create an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="mipStart">First mip map level to map to the view.</param>
		/// <returns>A new unordered access view for the texture.</returns>
		/// <remarks>Use this to create an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the video device feature level is not SM_5 or better.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="mipStart"/> parameters is less than 0 or greater than or equal to the 
		/// number of mip levels in the texture.
		/// <para>-or-</para>
		/// <para>Thrown if the bit count of the <paramref name="format"/> and the texture format are different, or if format is not in the R32 group and is not in the same group as the texture format.</para>
		/// </exception>
		public GorgonTextureUnorderAccessView CreateUnorderedAccessView(BufferFormat format, int mipStart)
		{
			return OnCreateUnorderedAccessView(format, mipStart, 0, Settings.ArrayCount);
		}

		/// <summary>
		/// Function to create an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <returns>A new unordered access view for the texture.</returns>
		/// <remarks>Use this to create an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the video device feature level is not SM_5 or better.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown if the bit count of the <paramref name="format"/> and the texture format are different, or if format is not in the R32 group and is not in the same group as the texture format.
		/// </exception>
		public GorgonTextureUnorderAccessView CreateUnorderedAccessView(BufferFormat format)
		{
			return OnCreateUnorderedAccessView(format, 0, 0, Settings.ArrayCount);
		}

        /// <summary>
        /// Function to create a new shader view for the texture.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="mipStart">Starting mip map for the view.</param>
        /// <param name="mipCount">Mip map count for the view.</param>
        /// <param name="arrayIndex">Starting array index for the view.</param>
        /// <param name="arrayCount">Array index count for the view.</param>
        /// <returns>A new shader view for the texture.</returns>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <remarks>Use this to create additional shader views for the texture.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonTextureShaderView CreateShaderView(BufferFormat format, int mipStart, int mipCount, int arrayIndex, int arrayCount)
        {
            return OnCreateShaderView(format, mipStart, mipCount, arrayIndex, arrayCount);
        }

        /// <summary>
        /// Function to create a new shader view for the texture.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="mipStart">Starting mip map for the view.</param>
        /// <param name="mipCount">Mip map count for the view.</param>
        /// <returns>A new shader view for the texture.</returns>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <remarks>Use this to create additional shader views for the texture.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonTextureShaderView CreateShaderView(BufferFormat format, int mipStart, int mipCount)
        {
            return OnCreateShaderView(format, mipStart, mipCount, 0, Settings.ArrayCount);
        }

        /// <summary>
        /// Function to create a new shader view for the texture.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <returns>A new shader view for the texture.</returns>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <remarks>Use this to create additional shader views for the texture.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonTextureShaderView CreateShaderView(BufferFormat format)
        {
            return OnCreateShaderView(format, 0, Settings.MipCount, 0, Settings.ArrayCount);
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
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
		public void CopySubResource(GorgonTexture1D texture, int subResource, int destSubResource, GorgonRange? sourceRange, int destination)
		{
			GorgonDebug.AssertNull<GorgonTexture1D>(texture, "texture");

#if DEBUG
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && ((Settings.Usage != BufferUsage.Staging) || (texture.Settings.Usage != BufferUsage.Staging)))
            {
                throw new NotSupportedException("Feature level SM2_a_b video devices cannot copy 1D non-staging textures.");
            }

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((texture.FormatInformation.Group != FormatInformation.Group) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((this == texture) && (subResource == destSubResource))
				throw new ArgumentException("Cannot copy to and from the same sub resource on the same texture.");
#endif

            if (sourceRange != null)
            {
                Graphics.Context.CopySubresourceRegion(texture.D3DResource, subResource, new D3D.ResourceRegion()
                    {
                        Back = 1,
                        Front = 0,
                        Top = 0,
                        Left = sourceRange.Value.Minimum,
                        Right = sourceRange.Value.Maximum,
                        Bottom = 1
                    }, this.D3DResource, destSubResource, destination, 0, 0);
            }
            else
            {
                Graphics.Context.CopySubresourceRegion(texture.D3DResource, subResource, null, this.D3DResource, destSubResource, 0, 0, 0);
            }
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
		public void CopySubResource(GorgonTexture1D texture, GorgonRange sourceRange, int destination)
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
		public void UpdateSubResource(ISubResourceData data, int subResource, GorgonRange destRange)
		{
#if DEBUG
			if ((Settings.Usage == BufferUsage.Dynamic) || (Settings.Usage == BufferUsage.Immutable))
				throw new InvalidOperationException("Cannot update a texture that is Dynamic or Immutable");
#endif

			if (destRange.Minimum < 0)
			{
				destRange = new GorgonRange(0, destRange.Maximum);
			}

			if (destRange.Minimum >= Settings.Width)
			{
				destRange = new GorgonRange(Settings.Width - 1, destRange.Maximum);
			}

			if (destRange.Maximum < 0)
			{
				destRange = new GorgonRange(destRange.Minimum, 0);
			}

			if (destRange.Maximum >= Settings.Width)
			{
				destRange = new GorgonRange(destRange.Minimum, Settings.Width - 1);
			}

			var box = new SharpDX.DataBox()
			{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			var region = new D3D.ResourceRegion()
			{
				Front = 0,
				Back = 1,
				Top = 0,
				Bottom = 1,
				Left = destRange.Minimum,
				Right = destRange.Maximum
			};

			Graphics.Context.UpdateSubresource(box, D3DResource, subResource, region);
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
