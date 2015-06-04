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

using Gorgon.Core;
using GI = SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
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
		    var settings1D = new GorgonTexture1DSettings
			{
				ArrayCount = Settings.ArrayCount,
				Format = Settings.Format,
				Width = Settings.Width,
				MipCount = Settings.MipCount,
				Usage = BufferUsage.Staging
			};

			GorgonTexture staging = Graphics.Textures.CreateTexture(Name + ".Staging", settings1D);
			staging.Copy(this);

			return staging;
		}

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>
		/// The <paramref name="initialData" /> can be NULL (<i>Nothing</i> in VB.Net) IF the texture is not created with an Immutable usage flag.
		/// <para>To initialize the texture, create a new <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> object and fill it with image information.</para>
		/// </remarks>
		protected override void OnInitialize(GorgonImageData initialData)
		{
			var desc = new D3D.Texture1DDescription
				{
					ArraySize = Settings.ArrayCount,
					BindFlags = GetBindFlags(false, false),
					Format = (GI.Format)Settings.Format,
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

			if ((initialData != null) && (initialData.Buffers.Count > 0))
			{
				D3DResource = new D3D.Texture1D(Graphics.D3DDevice, desc, initialData.Buffers.DataBoxes);
			}
			else
			{
				D3DResource = new D3D.Texture1D(Graphics.D3DDevice, desc);
			}
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
	    /// <para>If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), then the immediate context is used.  Use a deferred context to allow multiple threads to lock the 
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
		/// Function to retrieve an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="mipStart">[Optional] First mip map level to map to the view.</param>
		/// <param name="arrayStart">[Optional] The first array index to map to the view.</param>
		/// <param name="arrayCount">[Optional] The number of array indices to map to the view.</param>
		/// <returns>A new unordered access view for the texture.</returns>
		/// <remarks>Use this to retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// <para>Textures that have a usage of staging cannot create unordered views.</para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the usage for this texture is set to Staging.
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
		public GorgonTextureUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int mipStart = 0, int arrayStart = 0,
		                                                                     int arrayCount = 1)
		{
			return OnGetUnorderedAccessView(format, mipStart, arrayStart, arrayCount);
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
		public float ToTexel(float pixel)
		{
			return pixel / Settings.Width;
		}

        /// <summary>
        /// Function to copy a texture subresource from another texture.
        /// </summary>
        /// <param name="sourceTexture">Source texture to copy.</param>
        /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="unsafeCopy">[Optional] <b>true</b> to disable all range checking for coorindates, <b>false</b> to clip coorindates to safe ranges.</param>
        /// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
        /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  The <paramref name="sourceRange"/> 
        /// coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
        /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
        /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
        /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
        /// <para>Pass NULL (<i>Nothing</i> in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>The <paramref name="unsafeCopy"/> parameter is meant to provide a performance increase by skipping any checking of the destination and source coorindates passed in to the function.  When set to <b>true</b> it will 
        /// just pass the coordinates without testing and adjusting for clipping.  If your coordinates are outside of the source/destination texture range, then the behaviour will be undefined (i.e. depending on your 
        /// video driver, it may clip, or throw an exception or do nothing).  Care must be taken to ensure the coordinates fit within the source and destination if this parameter is set to <b>true</b>.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
        /// <para>-or-</para>
        /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
        public void CopySubResource(GorgonTexture1D sourceTexture, GorgonRange? sourceRange = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destArrayIndex = 0, int destMipLevel = 0, bool unsafeCopy = false, GorgonGraphics deferred = null)
        {
            OnCopySubResource(sourceTexture,
                new GorgonBox
                {
                    Front = 0,
                    Left = sourceRange != null ? sourceRange.Value.Minimum : 0,
                    Top = 0,
                    Depth = 1,
                    Width = sourceRange != null ? sourceRange.Value.Maximum : Settings.Width,
                    Height = 1
                },
                sourceArrayIndex,
                sourceMipLevel,
                destX,
                0,
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
        /// <param name="unsafeCopy">[Optional] <b>true</b> to disable all range checking for coorindates, <b>false</b> to clip coorindates to safe ranges.</param>
        /// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
        /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  The <paramref name="sourceRange"/> 
        /// coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
        /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
        /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
        /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
        /// <para>Pass NULL (<i>Nothing</i> in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>The <paramref name="unsafeCopy"/> parameter is meant to provide a performance increase by skipping any checking of the destination and source coorindates passed in to the function.  When set to <b>true</b> it will 
        /// just pass the coordinates without testing and adjusting for clipping.  If your coordinates are outside of the source/destination texture range, then the behaviour will be undefined (i.e. depending on your 
        /// video driver, it may clip, or throw an exception or do nothing).  Care must be taken to ensure the coordinates fit within the source and destination if this parameter is set to <b>true</b>.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
        /// <para>-or-</para>
        /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
        public void CopySubResource(GorgonTexture1D sourceTexture, GorgonRange sourceRange, int destX = 0, bool unsafeCopy = false, GorgonGraphics deferred = null)
        {
            CopySubResource(sourceTexture, sourceRange, 0, 0, destX, 0, 0, unsafeCopy, deferred);
        }

        /// <summary>
        /// Function to copy a texture subresource from another texture.
        /// </summary>
        /// <param name="sourceTexture">Source texture to copy.</param>
        /// <param name="sourceArrayIndex">The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">The mip map level of the sub resource to copy.</param>
        /// <param name="destArrayIndex">The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">The mip map level of the destination sub resource to copy into.</param>
        /// <param name="unsafeCopy">[Optional] <b>true</b> to disable all range checking for coorindates, <b>false</b> to clip coorindates to safe ranges.</param>
        /// <param name="deferred">[Optional] The deferred context to use when copying the sub resource.</param>
        /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  
        /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
        /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
        /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
        /// <para>Pass NULL (<i>Nothing</i> in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>The <paramref name="unsafeCopy"/> parameter is meant to provide a performance increase by skipping any checking of the destination and source coorindates passed in to the function.  When set to <b>true</b> it will 
        /// just pass the coordinates without testing and adjusting for clipping.  If your coordinates are outside of the source/destination texture range, then the behaviour will be undefined (i.e. depending on your 
        /// video driver, it may clip, or throw an exception or do nothing).  Care must be taken to ensure the coordinates fit within the source and destination if this parameter is set to <b>true</b>.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
        /// <para>-or-</para>
        /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
        public void CopySubResource(GorgonTexture1D sourceTexture, int sourceArrayIndex, int sourceMipLevel, int destArrayIndex, int destMipLevel, bool unsafeCopy = false, GorgonGraphics deferred = null)
        {
            CopySubResource(sourceTexture, null, sourceArrayIndex, 0, 0, destArrayIndex, destMipLevel, unsafeCopy, deferred);
        }

        /// <summary>
        /// Function to copy data from the CPU to the texture on the GPU.
        /// </summary>
        /// <param name="buffer">A buffer containing the image data to copy.</param>
        /// <param name="destRange">A start and end range value that will specify the region that will receive the data.</param>
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
        /// Use this to copy data into a texture with a usage of staging or default.  If the <paramref name="destRange"/> values are larger than the dimensions of the texture, then the data will be clipped.
        /// <para>Passing NULL (<i>Nothing</i> in VB.Net) to the <paramref name="destArrayIndex"/> and/or the <paramref name="destMipLevel"/> parameters will use the first array index and/or mip map level.</para>
        /// <para>This method will not work with depth/stencil textures or with textures that have multisampling applied.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        public virtual void UpdateSubResource(GorgonImageBuffer buffer,
            GorgonRange destRange,
            int destArrayIndex = 0,
            int destMipLevel = 0,
            GorgonGraphics deferred = null)
        {
            OnUpdateSubResource(buffer,
                new GorgonBox
                {
                    Front = 0,
                    Depth = 1,
                    Left = destRange.Minimum,
                    Width = destRange.Maximum,
                    Top = 0,
                    Height = 1
                },
                destArrayIndex,
                destMipLevel,
                deferred);
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture1D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonTexture1D(GorgonGraphics graphics, string name, ITextureSettings settings)
			: base(graphics, name, settings)
		{
            Settings = new GorgonTexture1DSettings
            {
                Width = settings.Width,
                AllowUnorderedAccessViews = settings.AllowUnorderedAccessViews,
                ArrayCount = settings.ArrayCount,
                Format = settings.Format,
                MipCount = settings.MipCount,
                ShaderViewFormat = settings.ShaderViewFormat,
                Usage = settings.Usage
            };
        }
		#endregion
	}
}
