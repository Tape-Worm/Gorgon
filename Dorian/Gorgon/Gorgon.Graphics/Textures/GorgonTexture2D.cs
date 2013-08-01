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
using System.Drawing;
using GorgonLibrary.Diagnostics;
using SlimMath;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

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
	    /// Property to return the settings for this texture.
	    /// </summary>
	    public new GorgonTexture2DSettings Settings
	    {
	        get;
	        private set;
	    }
	    #endregion

		#region Methods.
		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="context">The graphics context to use when updating the texture.</param>
		/// <remarks>
		/// The <paramref name="context" /> allows a separate thread to access the resource at the same time as another thread.
		/// </remarks>
		protected override void OnUpdateSubResource(ISubResourceData data, int subResource, GorgonGraphics context)
		{
			var box = new SharpDX.DataBox
				{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			var region = new D3D.ResourceRegion
				{
					Front = 0,
					Back = 1,
					Left = 0,
					Right = Settings.Width,
					Top = 0,
					Bottom = Settings.Height
				};
			
			context.Context.UpdateSubresourceSafe(box, D3DResource, FormatInformation.SizeInBytes, subResource, region, FormatInformation.IsCompressed);			
		}

		/// <summary>
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>
		/// The new staging texture.
		/// </returns>
		protected override GorgonTexture OnGetStagingTexture()
		{
			GorgonTexture staging = null;

			var settings2D = new GorgonTexture2DSettings
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

			staging = Graphics.ImmediateContext.Textures.CreateTexture(Name + ".Staging", settings2D);

			staging.Copy(this);

			return staging;
		}

		/// <summary>
		/// Function to initialize the texture with optional initial data.
		/// </summary>
		/// <param name="initialData">Data used to populate the image.</param>
		protected override void OnInitialize(GorgonImageData initialData)
		{
			var desc = new D3D.Texture2DDescription
			    {
			        ArraySize = Settings.ArrayCount,
			        Format = (SharpDX.DXGI.Format)Settings.Format,
			        Width = Settings.Width,
			        Height = Settings.Height,
			        MipLevels = Settings.MipCount,
			        BindFlags = GetBindFlags(false, false),
			        Usage = (D3D.ResourceUsage)Settings.Usage,
			        OptionFlags = Settings.IsTextureCube ? D3D.ResourceOptionFlags.TextureCube : D3D.ResourceOptionFlags.None,
			        SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling)
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

		    D3DResource = initialData != null
		                      ? new D3D.Texture2D(Graphics.D3DDevice, desc, initialData.GetDataBoxes())
		                      : new D3D.Texture2D(Graphics.D3DDevice, desc);
		}

        /// <summary>
        /// Function to lock a CPU accessible texture sub resource for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used to lock.</param>
        /// <param name="arrayIndex">[Optional] Array index of the sub resource to lock.</param>
        /// <param name="mipLevel">[Optional] The mip-map level of the sub resource to lock.</param>
        /// <param name="deferred">[Optional] The deferred graphics context used to lock the texture.</param>
        /// <returns>A stream used to write to the texture.</returns>
        /// <remarks>This method is used to lock down a sub resource in the texture for reading/writing. When locking a texture, the entire texture sub resource is locked and returned.  There is no setting to return a portion of the texture subresource.
        /// <para>This method is only available to textures created with a staging or dynamic usage setting.  Otherwise an exception will be raised.</para>
        /// <para>Only the Write, Discard (with the Write flag) and Read flags may be used in the <paramref name="lockFlags"/> parameter.  The Read flag can only be used with staging textures and is mutually exclusive.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), then the immediate context is used.  Use a deferred context to allow multiple threads to lock the 
        /// texture at the same time.</para>
        /// </remarks>
        /// <returns>This method will return a <see cref="GorgonLibrary.Graphics.GorgonTextureLockData">GorgonTextureLockData</see> object containing information about the locked sub resource as well as 
        /// a <see cref="GorgonLibrary.IO.GorgonDataStream">GorgonDataStream</see> that is used to access the locked sub resource data.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the texture is not a dynamic or staging texture.
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> or the <paramref name="mipLevel"/> parameters are less than 0, or larger than their respective counts in the texture settings.</exception>
        public GorgonTextureLockData Lock(BufferLockFlags lockFlags,
            int arrayIndex = 0,
            int mipLevel = 0,
            GorgonGraphics deferred = null)
        {
            return OnLock(lockFlags, arrayIndex, mipLevel, deferred);
        }

        /// <summary>
        /// Function to retrieve a shader resource view object.
        /// </summary>
        /// <param name="format">The format of the resource view.</param>
        /// <param name="mipStart">[Optional] Starting mip map for the view.</param>
        /// <param name="mipCount">[Optional] Mip map count for the view.</param>
        /// <param name="arrayIndex">[Optional] Starting array index for the view.</param>
        /// <param name="arrayCount">[Optional] Array index count for the view.</param>
        /// <remarks>Use a shader view to access a texture from a shader.  A shader view can view a select portion of the texture, and the view <paramref name="format"/> can be used to 
        /// cast the format of the texture into another type (as long as the view format is in the same group as the texture format).  For example, a texture with a format of R8G8B8A8 could be cast 
        /// to R8G8B8A8_UInt_Normal, or R8G8B8A8_UInt or any of the other R8G8B8A8 formats.
        /// <para>Multiple views of the texture can be bound to different parts of the shader pipeline.</para>
        /// <para>Textures that have a usage of staging cannot create shader views.</para>
        /// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
        /// <returns>A texture shader view object.</returns>
        public GorgonTextureShaderView GetShaderView(BufferFormat format, int mipStart = 0, int mipCount = 1, int arrayIndex = 0, int arrayCount = 1)
        {
            return OnGetShaderView(format, mipStart, mipCount, arrayIndex, arrayCount);
        }

		/// <summary>
		/// Function to retrieve an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="mipStart">[Optional] First mip map level to map to the view.</param>
		/// <param name="arrayStart">[Optional] The first array index to map to the view.</param>
		/// <param name="arrayCount">[Optional] The number of array indices to map to the view.</param>
		/// <returns>An unordered access view for the texture.</returns>
		/// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// <para>Textures that have a usage of staging cannot create unordered views.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
		public GorgonTextureUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int mipStart = 0, int arrayStart = 0,
																			 int arrayCount = 1)
		{
			return OnGetUnorderedAccessView(format, mipStart, arrayStart, arrayCount);
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
		/// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
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
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
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
		public void CopySubResource(GorgonTexture2D texture, int subResource, int destSubResource, Rectangle? sourceRegion, Vector2 destination, GorgonGraphics deferred = null)
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
			if (deferred == null)
			{
				deferred = Graphics;
			}

			// If we have multisampling enabled, then copy the entire sub resource.            
            if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0) || (sourceRegion == null))
            {
                deferred.Context.CopySubresourceRegion(texture.D3DResource, subResource, null, D3DResource, destSubResource, 0, 0, 0);
            }
            else
            {
                deferred.Context.CopySubresourceRegion(texture.D3DResource, subResource, new D3D.ResourceRegion
	                {
                        Back = 1,
                        Front = 0,
                        Top = sourceRegion.Value.Top,
                        Left = sourceRegion.Value.Left,
                        Right = sourceRegion.Value.Right,
                        Bottom = sourceRegion.Value.Bottom
                    }, D3DResource, destSubResource, (int)destination.X, (int)destination.Y, 0);
            }
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="sourceRegion">Region on the source texture to copy.</param>
		/// <param name="destination">Destination point to copy to.</param>
		/// <param name="deferred">[Optional] The deferred context used to copy the sub resource.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRegion"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
		/// accessing the sub resource.</para>
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
		public void CopySubResource(GorgonTexture2D texture, Rectangle sourceRegion, Vector2 destination, GorgonGraphics deferred = null)
		{
#if DEBUG
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");
#endif

			CopySubResource(texture, 0, 0, sourceRegion, destination, deferred);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="deferred">[Optional] The deferred context used to copy the sub resource.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
		/// accessing the sub resource.</para>
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
		public void CopySubResource(GorgonTexture2D texture, GorgonGraphics deferred = null)
		{
#if DEBUG
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");
#endif

			CopySubResource(texture, 0, 0, null, Vector2.Zero, deferred);
		}

		/// <summary>
		/// Function to copy a texture sub resource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <param name="deferred">[Optional] The deferred context used to copy the sub resource.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The source texture must fit within the dimensions of this texture.  If it does not, then the copy will be clipped so that it fits.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="M:GorgonLibrary.Graphics.GorgonTexture2D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
		/// accessing the sub resource.</para>
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
		public void CopySubResource(GorgonTexture2D texture, int subResource, int destSubResource, GorgonGraphics deferred = null)
		{
			GorgonDebug.AssertNull<GorgonTexture>(texture, "texture");

			CopySubResource(texture, subResource, destSubResource, null, Vector2.Zero, deferred);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="destRect">Destination region to copy into.</param>
		/// <param name="deferred">[Optional] The deferred context used to update the sub resource.</param>
		/// <remarks>
		/// Use this to copy data to this texture.  If the texture is non CPU accessible texture then an exception is raised.
		/// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
		/// accessing the sub resource.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="destRect"/> parameter is less than 0 or larger than this texture.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has multisampling applied.</para>
		/// </exception>
		public virtual void UpdateSubResource(ISubResourceData data, int subResource, Rectangle destRect, GorgonGraphics deferred = null)
		{
#if DEBUG
			if ((Settings.Usage == BufferUsage.Dynamic) || (Settings.Usage == BufferUsage.Immutable))
				throw new InvalidOperationException("Cannot update a texture that is Dynamic or Immutable");

			if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0))
				throw new InvalidOperationException("Cannot update a texture that is multisampled.");
#endif
			var textureSize = new Rectangle(0, 0, Settings.Width, Settings.Height);
			if (!textureSize.Contains(destRect))
				destRect = Rectangle.Intersect(destRect, textureSize);

			var box = new SharpDX.DataBox
				{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			var region = new D3D.ResourceRegion
				{
				Front = 0,
				Back = 1,
				Top = destRect.Top,
				Left = destRect.Left,
				Bottom = destRect.Bottom,
				Right = destRect.Right,
			};

			if (deferred == null)
			{
				deferred = Graphics;
			}

			deferred.Context.UpdateSubresource(box, D3DResource, subResource, region);
		}
		#endregion

		#region Constructor/Destructor.
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
            // TODO: This is really kind of a hack.  I need to find a better way to make specific settings show up for their respective types.
		    Settings = settings as GorgonTexture2DSettings ?? new GorgonTexture2DSettings
		        {
		            Width = settings.Width,
		            Height = settings.Height,
		            AllowUnorderedAccessViews = settings.AllowUnorderedAccessViews,
		            ArrayCount = settings.ArrayCount,
		            Format = settings.Format,
		            IsTextureCube = settings.IsTextureCube,
		            MipCount = settings.MipCount,
		            Multisampling = settings.Multisampling,
		            ShaderViewFormat = settings.ShaderViewFormat,
		            Usage = settings.Usage
		        };
		}
		#endregion
	}
}