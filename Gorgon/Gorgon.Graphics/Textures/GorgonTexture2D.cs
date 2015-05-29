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
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using SlimMath;
using GI = SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
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
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>
		/// The new staging texture.
		/// </returns>
		protected override GorgonTexture OnGetStagingTexture()
		{
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

			GorgonTexture staging = Graphics.ImmediateContext.Textures.CreateTexture(Name + ".Staging", settings2D);

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
			        Format = (GI.Format)Settings.Format,
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
							  ? new D3D.Texture2D(Graphics.D3DDevice, desc, initialData.Buffers.DataBoxes)
		                      : new D3D.Texture2D(Graphics.D3DDevice, desc);
		}


		/// <summary>
		/// Function to resolve a multisampled texture resource into another non-multisampled resource.
		/// </summary>
		/// <param name="destination">The texture that will receive the resolved texture.</param>
		/// <param name="destArrayIndex">[Optional] Index in the array that will receive the resolved texture data.</param>
		/// <param name="destMipLevel">[Optional] The mip map level that will receive the resolved texture data.</param>
		/// <param name="srcArrayIndex">[Optional] The array index in the source to resolve.</param>
		/// <param name="srcMipLevel">[Optional] The source mip level to resolve.</param>
		/// <param name="resolveFormat">[Optional] A format that will determine how to resolve the multisampled texture into a non-multisampled texture.</param>
		/// <param name="context">[Optional] A deferred graphics context used to copy the data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="destArrayIndex"/>, <paramref name="destMipLevel"/>, <paramref name="srcArrayIndex"/>, or the 
		/// <paramref name="srcMipLevel"/> parameters are not within the the range of the array indices or mip levels of the respective textures.</exception>
		/// <exception cref="System.NotSupportedException">Thrown when the source texture is not multisampled or the destination texture is multisampled or has a non default usage.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="resolveFormat"/> is not compatible with the formats of the source or destination texture.</exception>
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
		/// <para>If the <paramref name="context"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
		/// accessing the sub resource.</para>
		/// </remarks>
		public void ResolveTo(GorgonTexture destination,
							  int destArrayIndex = 0,
							  int destMipLevel = 0,
							  int srcArrayIndex = 0,
							  int srcMipLevel = 0,
							  BufferFormat resolveFormat = BufferFormat.Unknown,
							  GorgonGraphics context = null)
		{
			if (resolveFormat == BufferFormat.Unknown)
			{
				resolveFormat = Settings.Format;
			}

#if DEBUG
			if (destination == null)
			{
				throw new ArgumentNullException("destination");	
			}

			if ((destArrayIndex >= destination.Settings.ArrayCount)
				&& (destArrayIndex < 0))
			{
				throw new ArgumentOutOfRangeException("destArrayIndex",
					                                    string.Format(Resources.GORGFX_ARG_OUT_OF_RANGE,
					                                                0,
					                                                destination.Settings.ArrayCount));
			}

			if ((destMipLevel < 0) || (destMipLevel >= destination.Settings.MipCount))
			{
				throw new ArgumentOutOfRangeException("destMipLevel", string.Format(Resources.GORGFX_ARG_OUT_OF_RANGE, 0,
					                                                destination.Settings.MipCount));
			}

			if ((srcArrayIndex >= Settings.ArrayCount)
				&& (srcArrayIndex < 0))
			{
				throw new ArgumentOutOfRangeException("srcArrayIndex",
					                                    string.Format(Resources.GORGFX_ARG_OUT_OF_RANGE,
					                                                0,
					                                                Settings.ArrayCount));
			}

			if ((srcMipLevel < 0) || (srcMipLevel >= Settings.MipCount))
			{
				throw new ArgumentOutOfRangeException("srcMipLevel", string.Format(Resources.GORGFX_ARG_OUT_OF_RANGE, 0,
					                                                Settings.MipCount));
			}

			if (Settings.Multisampling.Equals(GorgonMultisampling.NoMultiSampling))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_TEXTURE_SRC_NOT_MULTISAMPLED, Name));
			}

			if (destination.Settings.Usage != BufferUsage.Default)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_TEXTURE_RESOLVE_DEST_NOT_DEFAULT, destination.Name));
			}

			var srcFormatInfo = GorgonBufferFormatInfo.GetInfo(Settings.Format);
			var destFormatInfo = GorgonBufferFormatInfo.GetInfo(destination.Settings.Format);
			var resolveFormatInfo = GorgonBufferFormatInfo.GetInfo(resolveFormat);

			// Ensure that the resource formats and resolve format all match up.
			if ((!srcFormatInfo.IsTypeless) && (!destFormatInfo.IsTypeless))
			{
				if (Settings.Format != destination.Settings.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_TEXTURE_RESOLVE_FORMATS_NOT_SAME, Settings.Format),
					                            "destination");
				}

				if (resolveFormat != Settings.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_TEXTURE_RESOLVE_FORMAT_MUST_BE_UNKNOWN), "resolveFormat");
				}
			}
			else if ((srcFormatInfo.IsTypeless) && (destFormatInfo.IsTypeless))
			{
				if (Settings.Format != destination.Settings.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_TEXTURE_RESOLVE_FORMATS_NOT_SAME, Settings.Format),
												"destination");
				}

				if ((resolveFormatInfo.Group != srcFormatInfo.Group)
				    || (resolveFormatInfo.Group != destFormatInfo.Group))
				{
					throw new ArgumentException(
						string.Format(Resources.GORGFX_TEXTURE_RESOLVE_FORMAT_NOT_SAME_GROUP, Settings.Format),
						"resolveFormat");
				}
			} 
			else if ((srcFormatInfo.IsTypeless) || (destFormatInfo.IsTypeless))
			{
				if (resolveFormatInfo.IsTypeless)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_TEXTURE_RESOLVE_FORMAT_CANNOT_BE_TYPELESS),
					                            "resolveFormat");
				}

				if (srcFormatInfo.Group != destFormatInfo.Group)
				{
					throw new ArgumentException(
						string.Format(Resources.GORGFX_TEXTURE_RESOLVE_SRC_DEST_NOT_SAME_GROUP,
						              Settings.Format,
						              destination.Settings.Format),
						"destination");
				}
			}
#endif

			int sourceIndex = D3D.Resource.CalculateSubResourceIndex(srcMipLevel, srcArrayIndex, Settings.MipCount);
			int destIndex = D3D.Resource.CalculateSubResourceIndex(destMipLevel,
			                                                       destArrayIndex,
			                                                       destination.Settings.MipCount);

			if (context == null)
			{
				context = Graphics;
			}

			context.Context.ResolveSubresource(D3DResource, sourceIndex, destination.D3DResource, destIndex, (GI.Format)resolveFormat);
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
        /// <returns>This method will return a <see cref="Gorgon.Graphics.GorgonTextureLockData">GorgonTextureLockData</see> object containing information about the locked sub resource as well as 
        /// a <see cref="Gorgon.IO.GorgonDataStream">GorgonDataStream</see> that is used to access the locked sub resource data.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the texture is not a dynamic or staging texture.
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> or the <paramref name="mipLevel"/> parameters are less than 0, or larger than their respective counts in the texture settings.</exception>
        public virtual GorgonTextureLockData Lock(BufferLockFlags lockFlags,
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
		/// <exception cref="GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
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
		/// <exception cref="GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
		public GorgonTextureUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int mipStart = 0, int arrayStart = 0,
																			 int arrayCount = 1)
		{
			return OnGetUnorderedAccessView(format, mipStart, arrayStart, arrayCount);
		}

        /// <summary>
        /// Function to convert a pixel space coordinate into a texel space coordinate.
        /// </summary>
        /// <param name="x">The horizontal location to convert.</param>
        /// <param name="y">The vertical location to convert.</param>
        /// <returns>The converted coordinate.</returns>
	    public Vector2 ToTexel(float x, float y)
	    {
            return new Vector2(x / Settings.Width, y / Settings.Height);
	    }

        /// <summary>
        /// Function to convert a texel space coordinate into a pixel space coordinate.
        /// </summary>
        /// <param name="tu">The horizontal location to convert.</param>
        /// <param name="tv">The vertical location to convert.</param>
        /// <returns>The converted coordinate.</returns>
        public Vector2 ToPixel(float tu, float tv)
        {
            return new Vector2(tu * Settings.Width, tv * Settings.Height);
        }

        /// <summary>
        /// Function to convert a pixel space coordinate into a texel space coordinate.
        /// </summary>
        /// <param name="x">The horizontal location to convert.</param>
        /// <param name="y">The vertical location to convert.</param>
        /// <param name="texel">The resulting texel space coordinate.</param>
        public void ToTexel(float x, float y, out Vector2 texel)
        {
            texel = new Vector2(x / Settings.Width, y / Settings.Height);
        }

        /// <summary>
        /// Function to convert a texel space coordinate into a pixel space coordinate.
        /// </summary>
        /// <param name="tu">The horizontal location to convert.</param>
        /// <param name="tv">The vertical location to convert.</param>
        /// <param name="pixel">The resulting pixel space coordinate.</param>
        public void ToPixel(float tu, float tv, out Vector2 pixel)
        {
            pixel = new Vector2(tu * Settings.Width, tv * Settings.Height);
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
		public Vector2 ToTexel(Vector2 pixel)
		{
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
			result = new Vector2(pixel.X / Settings.Width, pixel.Y / Settings.Height);
		}

		/// <summary>
		/// Function to convert a pixel coordinate rectangle into a texel space coordinate rectangle.
		/// </summary>
		/// <param name="pixel">The pixel coordinates to convert.</param>
		/// <param name="result">The texel space location and size of the pixel rectangle on the texture.</param>
		public void ToTexel(ref RectangleF pixel, out RectangleF result)
		{
			result = new RectangleF(pixel.X / Settings.Width,
			                        pixel.Y / Settings.Height,
			                        pixel.Width / Settings.Width,
			                        pixel.Height / Settings.Height);
		}

		/// <summary>
		/// Function to convert a texel coordinate rectangle into a pixel space coordinate rectangle.
		/// </summary>
		/// <param name="texel">The texel coordinates to convert.</param>
		/// <param name="result">The pixel space location and size of the texel rectangle on the texture.</param>
		public void ToPixel(ref RectangleF texel, out RectangleF result)
		{
			result = new RectangleF(texel.X * Settings.Width,
									texel.Y * Settings.Height,
									texel.Width * Settings.Width,
									texel.Height * Settings.Height);
		}

		/// <summary>
		/// Function to convert a pixel coordinate rectangle into a texel space coordinate rectangle.
		/// </summary>
		/// <param name="pixel">The pixel coordinates to convert.</param>
		public RectangleF ToTexel(RectangleF pixel)
		{
			return new RectangleF(pixel.X / Settings.Width,
									pixel.Y / Settings.Height,
									pixel.Width / Settings.Width,
									pixel.Height / Settings.Height);
		}

		/// <summary>
		/// Function to convert a texel coordinate rectangle into a pixel space coordinate rectangle.
		/// </summary>
		/// <param name="texel">The texel coordinates to convert.</param>
		public RectangleF ToPixel(RectangleF texel)
		{
			return new RectangleF(texel.X * Settings.Width,
			                      texel.Y * Settings.Height,
			                      texel.Width * Settings.Width,
			                      texel.Height * Settings.Height);
		}

	    /// <summary>
	    /// Function to copy a texture subresource from another texture.
	    /// </summary>
	    /// <param name="sourceTexture">Source texture to copy.</param>
	    /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destY">[Optional] Vertical offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="unsafeCopy">[Optional] <c>true</c> to disable all range checking for coorindates, <c>false</c> to clip coorindates to safe ranges.</param>
        /// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
	    /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  The <paramref name="sourceRange"/> 
	    /// coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
	    /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
	    /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
	    /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
	    /// <para>Pass NULL (Nothing in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
	    /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>The <paramref name="unsafeCopy"/> parameter is meant to provide a performance increase by skipping any checking of the destination and source coorindates passed in to the function.  When set to <c>true</c> it will 
        /// just pass the coordinates without testing and adjusting for clipping.  If your coordinates are outside of the source/destination texture range, then the behaviour will be undefined (i.e. depending on your 
        /// video driver, it may clip, or throw an exception or do nothing).  Care must be taken to ensure the coordinates fit within the source and destination if this parameter is set to <c>true</c>.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
	    /// accessing the sub resource.</para>
	    /// </remarks>
	    /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
	    /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
	    /// <para>-or-</para>
	    /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
	    /// </exception>
	    /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
	    /// </exception>
	    /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
	    public void CopySubResource(GorgonTexture2D sourceTexture,
	        Rectangle? sourceRange = null,
	        int sourceArrayIndex = 0,
	        int sourceMipLevel = 0,
	        int destX = 0,
	        int destY = 0,
	        int destArrayIndex = 0,
	        int destMipLevel = 0,
            bool unsafeCopy = false,
	        GorgonGraphics deferred = null)
	    {
	        OnCopySubResource(sourceTexture,
	            new GorgonBox
	            {
	                Front = 0,
	                Depth = 1,
	                Left = sourceRange != null ? sourceRange.Value.Left : 0,
	                Width = sourceRange != null ? sourceRange.Value.Width : Settings.Width,
	                Top = sourceRange != null ? sourceRange.Value.Top : 0,
	                Height = sourceRange != null ? sourceRange.Value.Height : Settings.Height
	            },
	            sourceArrayIndex,
	            sourceMipLevel,
	            destX,
	            destY,
	            0,
	            destArrayIndex,
	            destMipLevel,
                unsafeCopy,
                deferred);
	    }

        /// <summary>
        /// Function to copy a texture subresource from another texture.
        /// </summary>
        /// <param name="sourceTexture">Source texture to copy.</param>
        /// <param name="sourceRange">The dimensions of the source area to copy.</param>
        /// <param name="destX">Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destY">Vertical offset into the destination texture to place the copied data.</param>
        /// <param name="unsafeCopy">[Optional] <c>true</c> to disable all range checking for coorindates, <c>false</c> to clip coorindates to safe ranges.</param>
        /// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
        /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  The <paramref name="sourceRange"/> 
        /// coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
        /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
        /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
        /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
        /// <para>Pass NULL (Nothing in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>The <paramref name="unsafeCopy"/> parameter is meant to provide a performance increase by skipping any checking of the destination and source coorindates passed in to the function.  When set to <c>true</c> it will 
        /// just pass the coordinates without testing and adjusting for clipping.  If your coordinates are outside of the source/destination texture range, then the behaviour will be undefined (i.e. depending on your 
        /// video driver, it may clip, or throw an exception or do nothing).  Care must be taken to ensure the coordinates fit within the source and destination if this parameter is set to <c>true</c>.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
        /// <para>-or-</para>
        /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
        public void CopySubResource(GorgonTexture2D sourceTexture,
            Rectangle sourceRange,
            int destX,
            int destY,
            bool unsafeCopy = false,
            GorgonGraphics deferred = null)
        {
            CopySubResource(sourceTexture, sourceRange, 0, 0, destX, destY, 0, 0, unsafeCopy, deferred);
        }

        /// <summary>
        /// Function to copy a texture subresource from another texture.
        /// </summary>
        /// <param name="sourceTexture">Source texture to copy.</param>
        /// <param name="sourceArrayIndex">The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">The mip map level of the sub resource to copy.</param>
        /// <param name="destArrayIndex">The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">The mip map level of the destination sub resource to copy into.</param>
        /// <param name="unsafeCopy">[Optional] <c>true</c> to disable all range checking for coorindates, <c>false</c> to clip coorindates to safe ranges.</param>
        /// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
        /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  
        /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
        /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
        /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
        /// <para>Pass NULL (Nothing in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>The <paramref name="unsafeCopy"/> parameter is meant to provide a performance increase by skipping any checking of the destination and source coorindates passed in to the function.  When set to <c>true</c> it will 
        /// just pass the coordinates without testing and adjusting for clipping.  If your coordinates are outside of the source/destination texture range, then the behaviour will be undefined (i.e. depending on your 
        /// video driver, it may clip, or throw an exception or do nothing).  Care must be taken to ensure the coordinates fit within the source and destination if this parameter is set to <c>true</c>.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
        /// <para>-or-</para>
        /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
        public void CopySubResource(GorgonTexture2D sourceTexture,
            int sourceArrayIndex,
            int sourceMipLevel,
            int destArrayIndex,
            int destMipLevel,
            bool unsafeCopy = false,
            GorgonGraphics deferred = null)
        {
            CopySubResource(sourceTexture,
                null,
                sourceArrayIndex,
                sourceMipLevel,
                0,
                0,
                destArrayIndex,
                destMipLevel,
                unsafeCopy,
                deferred);
        }
        
        /// <summary>
        /// Function to copy data from the CPU to the texture on the GPU.
        /// </summary>
        /// <param name="buffer">A buffer containing the image data to copy.</param>
        /// <param name="destRect">A rectangle that will specify the region that will receive the data.</param>
        /// <param name="destArrayIndex">[Optional] The array index that will receive the data.</param>
        /// <param name="destMipLevel">[Optional] The mip map level that will receive the data.</param>
        /// <param name="deferred">[Optional] A deferred graphics context used to copy the data.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when the texture is dynamic or immutable.
        /// <para>-or-</para>
        /// <para>Thrown when the texture is multisampled.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="destArrayIndex"/> or the <paramref name="destMipLevel"/> is less than 0 or greater than/equal to the 
        /// number of array indices or mip levels in the texture.</exception>
        /// <remarks>
        /// Use this to copy data into a texture with a usage of staging or default.  If the <paramref name="destRect"/> values are larger than the dimensions of the texture, then the data will be clipped.
        /// <para>Passing NULL (Nothing in VB.Net) to the <paramref name="destArrayIndex"/> and/or the <paramref name="destMipLevel"/> parameters will use the first array index and/or mip map level.</para>
        /// <para>This method will not work with depth/stencil textures or with textures that have multisampling applied.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        public virtual void UpdateSubResource(GorgonImageBuffer buffer,
            Rectangle destRect,
            int destArrayIndex = 0,
            int destMipLevel = 0,
            GorgonGraphics deferred = null)
        {
            OnUpdateSubResource(buffer,
                new GorgonBox
                {
                    Front = 0,
                    Depth = 1,
                    Left = destRect.Left,
                    Width = destRect.Width,
                    Top = destRect.Top,
                    Height = destRect.Height
                },
                destArrayIndex,
                destMipLevel,
                deferred);
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
		    Settings = new GorgonTexture2DSettings
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