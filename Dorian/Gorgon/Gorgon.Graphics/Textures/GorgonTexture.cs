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
// Created: Monday, February 13, 2012 7:48:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using DX = SharpDX;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The base texture object for all textures.
	/// </summary>
	public abstract class GorgonTexture
		: GorgonNamedResource
	{
		#region Variables.
		private readonly IList<DX.DataStream> _lock;			// Locks for the texture.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the render target that this texture belongs to.
		/// </summary>
		public GorgonRenderTarget RenderTarget
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return whether this texture is for a render target.
		/// </summary>
		public bool IsRenderTarget
		{
			get
			{
				return RenderTarget != null;
			}
		}

		/// <summary>
		/// Property to return the information about the format for the unordered access view associated with the texture.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData UnorderedAccessViewFormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about the format for the texture.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData FormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the texture, in bytes.
		/// </summary>
		/// <remarks>This will take into account whether the texture is a packed format, or compressed.</remarks>
		public override int SizeInBytes
		{
			get
			{
				return GorgonImageData.GetSizeInBytes(Settings);
			}
		}

		/// <summary>
		/// Property to set or return the settings for the texture.
		/// </summary>
		public ITextureSettings Settings
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the resource object.
		/// </summary>
		protected override void CreateDefaultResourceView()
		{
			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
			if (Settings.ShaderView != BufferFormat.Unknown)
			{
				ShaderViewFormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.ShaderView);
			}
			else
			{
				ShaderViewFormatInformation = FormatInformation;
			}

			if (Settings.UnorderedAccessViewFormat != BufferFormat.Unknown)
			{
				UnorderedAccessViewFormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.UnorderedAccessViewFormat);
			}
			else
			{
				UnorderedAccessViewFormatInformation = null;
			}

			// Create the default view for this resource.
			if (Settings.Usage != BufferUsage.Staging)
			{
				DefaultView = new GorgonResourceView(Graphics, Name + "ResourceView");
				DefaultView.Resource = this;
				DefaultView.BuildResourceView();

				// Unbind and rebind us to the pipeline.
				Graphics.Shaders.Reseat(this);	
			}
		}

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			Gorgon.Log.Print("Gorgon texture {0}: Unbound from shaders.", Diagnostics.LoggingLevel.Verbose, Name);
			Graphics.Shaders.Unbind(this);

			Gorgon.Log.Print("Gorgon texture {0}: Destroying D3D 11 texture resource.", Diagnostics.LoggingLevel.Verbose, Name);

			// Unbind from the view.
			if (View != null)
			{
				View.Resource = null;
			}

			// Destroy any view attached to this object.
			if (DefaultView != null)
			{
				Gorgon.Log.Print("Gorgon default resource view {0}: Destroying default resource view.", Diagnostics.LoggingLevel.Verbose, DefaultView.Name);
				DefaultView.Dispose();
				DefaultView = null;
			}

			if (D3DResource != null)
			{
				GorgonRenderStatistics.TextureCount--;
				GorgonRenderStatistics.TextureSize -= SizeInBytes;
				D3DResource.Dispose();
				D3DResource = null;
			}			
		}

		/// <summary>
		/// Function to return sub resource data for a lock operation.
		/// </summary>
		/// <param name="dataStream">Stream containing the data.</param>
		/// <param name="rowPitch">The number of bytes per row of the texture.</param>
		/// <param name="slicePitch">The number of bytes per depth slice of the texture.</param>
		/// <returns>The sub resource data.</returns>
		protected abstract ISubResourceData GetLockSubResourceData(GorgonDataStream dataStream, int rowPitch, int slicePitch);

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		protected abstract void UpdateSubResourceImpl(ISubResourceData data, int subResource);

		/// <summary>
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>The new staging texture.</returns>
		protected abstract GorgonTexture GetStagingTextureImpl();

		/// <summary>
		/// Function to retrieve information about an existing texture.
		/// </summary>
		/// <returns>New settings for the texture.</returns>
		protected abstract ITextureSettings GetTextureInformation();

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The <paramref name="initialData"/> can be NULL (Nothing in VB.Net) IF the texture is not created with an Immutable usage flag.
		/// <para>To initialize the texture, create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object and fill it with image information.</para>
		/// </remarks>
		protected abstract void InitializeImpl(GorgonImageData initialData);

        /// <summary>
        /// Funtion to retrieve the binding flags for a resource.
        /// </summary>
        /// <param name="isDepth">TRUE if the texture is meant for depth/stencil.</param>
        /// <param name="isTarget">TRUE if the texture is meant for use as a render target.</param>
        /// <returns>The Direct3D binding flags.</returns>
        internal D3D.BindFlags GetBindFlags(bool isDepth, bool isTarget)
        {
            var flags = D3D.BindFlags.None;

            if (!isDepth)
            {
                if (Settings.Usage != BufferUsage.Staging)
                {
                    flags |= D3D.BindFlags.ShaderResource;
                }

                if (Settings.UnorderedAccessViewFormat != BufferFormat.Unknown)
                {
                    flags |= D3D.BindFlags.UnorderedAccess;
                }

                if (isTarget)
                {
                    flags |= D3D.BindFlags.RenderTarget;
                }
            }
            else
            {
                flags |= D3D.BindFlags.DepthStencil;
            }

            return flags;
        }

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The initial data can be a <see cref="GorgonLibrary.IO.GorgonDataStream">GorgonDataStream</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DData">GorgonTexture2DData</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture3DData">GorgonTexture3DData</see></remarks>
		internal void Initialize(GorgonImageData initialData)
		{
			try
			{
				Gorgon.Log.Print("{0} {1}: Creating D3D11 texture resource...", Diagnostics.LoggingLevel.Verbose, GetType().Name, Name);
				InitializeImpl(initialData);
				CreateDefaultResourceView();

				GorgonRenderStatistics.TextureCount++;
				GorgonRenderStatistics.TextureSize += SizeInBytes;
			}
			catch
			{
				if (D3DResource != null)
					D3DResource.Dispose();
				D3DResource = null;
				throw;
			}
		}

        /// <summary>
        /// Function to convert this texture into an array of System.Drawing.Images.
        /// </summary>
        /// <returns>A list of <see cref="System.Drawing.Image"/> image objects.</returns>
        public Image[] ToGDIImage()
        {
            return GorgonGDIImageConverter.CreateGDIImagesFromTexture(this);
        }

		/// <summary>
		/// Function to copy this texture into a staging texture.
		/// </summary>
		/// <returns>A new staging texture.</returns>
        /// <remarks>If the current video device has a feature level of SM2_a_b, and this texture is not a staging texture, then an exception will be thrown.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when a device with a feature level of SM2_a_b tries to create a staging texture from a texture that isn't already a staging texture.</exception>
		public T GetStagingTexture<T>()
			where T : GorgonTexture
		{
#if DEBUG
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (Settings.Usage != BufferUsage.Staging))
            {
                throw new NotSupportedException("Feature level SM2_a_b video devices cannot build a staging texture from a non-staging texture.");
            }
#endif
			return (T)GetStagingTextureImpl();
		}

        /// <summary>
        /// Function to save the texture to a byte array.
        /// </summary>
        /// <param name="codec">Codec used to encode the stream data.</param>
        /// <returns>A byte array containing the texture data.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>		
        /// <exception cref="System.IO.IOException">Thrown when there is an error when attempting to encode the image data.</exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current video device has a feature level of SM2_a_b and the texture is not a staging texture or the texture is not 2D.</exception>
        /// <remarks>This will persist the contents of the texture into an array of bytes.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
        /// number of built-in codecs.  Currently, Gorgon supports the following formats:
        /// <list type="bullet">
        ///		<item>
        ///			<description>DDS</description>
        ///		</item>
        ///		<item>
        ///			<description>TGA</description>
        ///		</item>
        ///		<item>
        ///			<description>PNG (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>BMP (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>JPG (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>WMP (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>TIF (WIC)</description>
        ///		</item>
        /// </list>
        /// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
        /// then it is required in order to read/save the files in those formats.</para>
        /// <para>Note that devices with a feature level of SM2_a_b cannot save textures that don't have a usage of staging.  Also, these devices will only save 2D staging textures.  Attempting to 
        /// save 3D and 1D textures will throw an exception.</para>
        /// </remarks>
        public byte[] Save(IO.GorgonImageCodec codec)
        {
            using (var memoryStream = new MemoryStream())
            {
                Save(memoryStream, codec);
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

		/// <summary>
		/// Function to save the texture to a stream with the specified codec.
		/// </summary>
		/// <param name="stream">Stream that will contain the texture information.</param>
		/// <param name="codec">Codec used to encode the stream data.</param>        
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>		
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current video device has a feature level of SM2_a_b and the texture is not a staging texture or the texture is not 2D.</exception>
		/// <remarks>This will persist the contents of the texture into a stream.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
        /// <para>Note that devices with a feature level of SM2_a_b cannot save textures that don't have a usage of staging.  Also, these devices will only save 2D staging textures.  Attempting to 
        /// save 3D and 1D textures will throw an exception.</para>
		/// </remarks>
		public void Save(Stream stream, IO.GorgonImageCodec codec)
		{
            if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
            {
                if (Settings.Usage != BufferUsage.Staging)
                {
                    throw new NotSupportedException("Feature level SM2_a_b video devices cannot save a non-staging texture.");
                }

                if (Settings.ImageType != ImageType.Image2D)
                {
                    throw new NotSupportedException("Feature level SM2_a_B video devices can only save 2D staging textures.");
                }
            }

            using (var textureData = GorgonImageData.CreateFromTexture(this))
			{
				textureData.Save(stream, codec);
			}
		}

		/// <summary>
		/// Function to save the image data to a file with the specified codec.
		/// </summary>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="codec">Codec used to encode the file data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> or the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current video device has a feature level of SM2_a_b and the texture is not a staging texture or the texture is not 2D.</exception>
		/// <remarks>This will persist the contents of the image data object into a stream.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
        /// <para>Note that devices with a feature level of SM2_a_b cannot save textures that don't have a usage of staging.  Also, these devices will only save 2D staging textures.  Attempting to 
        /// save 3D and 1D textures will throw an exception.</para>
        /// </remarks>
		public void Save(string filePath, GorgonImageCodec codec)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "fileName");
			}

            if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
            {
                if (Settings.Usage != BufferUsage.Staging)
                {
                    throw new NotSupportedException("Feature level SM2_a_b video devices cannot save a non-staging texture.");
                }

                if (Settings.ImageType != ImageType.Image2D)
                {
                    throw new NotSupportedException("Feature level SM2_a_B video devices can only save 2D staging textures.");
                }
            }

            using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
				Save(stream, codec);
			}
		}

		/// <summary>
		/// Function to copy another texture into this texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <remarks>
		/// This overload will copy the -entire- texture, including mipmaps, array levels, etc...  Use <see cref="M:GorgonLibrary.Graphics.GorgonTexture.CopySubResource(GorgonTexture2D, int, int, System.Drawing.Rectangle, SlimMath.Vector2)">CopySubResource</see> to copy a portion of the texture.
		/// <para>This method will -not- perform stretching, filtering or clipping.</para>
		/// <para>The <paramref name="texture"/> dimensions must be have the same dimensions as this texture.  If they do not, an exception will be thrown.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture sizes are not the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a 3D texture and is in CPU accessible memory and the video device is a SM2_a_b device.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b, and the source texture 
        /// is not a staging texture and the this texture is a staging texture, or if the textures are 1D textures and neither texture is a staging texture.</exception>
		public void Copy(GorgonTexture texture)
		{
			GorgonDebug.AssertNull<GorgonTexture>(texture, "texture");

#if DEBUG            
            if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
            {
                if ((texture.Settings.Usage != BufferUsage.Staging) && (Settings.Usage == BufferUsage.Staging))
                {
                    throw new NotSupportedException("Feature level SM2_a_b video devices cannot copy non staging texture data into a staging texture.");
                }

                if ((texture.Settings.ImageType == ImageType.Image1D) && (Settings.Usage != BufferUsage.Staging) && (texture.Settings.Usage != BufferUsage.Staging))
                {
                    throw new NotSupportedException("Feature level SM2_a_b video devices cannot copy 1D texture data in GPU memory.");
                }
            }

            if (texture.GetType() != this.GetType())
                throw new ArgumentException("The texure '" + texture.Name + "' is of type '" + texture.GetType().FullName + "' and cannot be copied to or from the type '" + this.GetType().FullName + "'.", "texture");

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count) || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
				throw new InvalidOperationException("Cannot copy textures with different multisampling parameters.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((texture.FormatInformation.Group != FormatInformation.Group) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((texture.Settings.Width != Settings.Width) || (texture.Settings.Height != Settings.Height))
				throw new ArgumentException("The texture sizes do not match.", "texture");

			// Ensure that the SM2_a_b devices don't try and copy between CPU and GPU accessible memory.
			if ((this is GorgonTexture3D) && (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (this.Settings.Usage == BufferUsage.Staging))
				throw new InvalidOperationException("This 3D texture is CPU accessible and cannot be copied.");
#endif

			Graphics.Context.CopyResource(texture.D3DResource, D3DResource);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <remarks>Use this to copy data to this texture.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has multisampling applied.</para>
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a depth/stencil buffer texture.</para>
		/// </exception>
		public void UpdateSubResource(ISubResourceData data, int subResource)
		{
#if DEBUG
			if ((Settings.Usage == BufferUsage.Dynamic) || (Settings.Usage == BufferUsage.Immutable))
				throw new InvalidOperationException("Cannot update a texture that is Dynamic or Immutable");

			if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0))
				throw new InvalidOperationException("Cannot update a texture that is multisampled.");
#endif
			UpdateSubResourceImpl(data, subResource);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <remarks>Use this to copy data to this texture.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has multisampling applied.</para>
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a depth/stencil buffer texture.</para>
		/// </exception>
		public void UpdateSubResource(ISubResourceData data)
		{
			UpdateSubResource(data, 0);
		}

		/// <summary>
		/// Function to return whether a texture sub resource is locked or not.
		/// </summary>
		/// <param name="subResource">Sub resource to check.</param>
		/// <returns>TRUE if it's locked, FALSE if not.</returns>
		public bool IsLocked(int subResource)
		{
			if (subResource >= _lock.Count)
				return false;

			return _lock[subResource] != null;
		}

		/// <summary>
		/// Function to lock the texture for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used to lock.</param>
		/// <remarks>When locking a texture, the entire texture sub resource is locked and returned.  There is no setting to return a portion of the texture subresource.
		/// <para>This overload locks the first sub resource (index 0) only.</para>
		/// <para>This method is only available to textures created with a staging or dynamic usage setting.  Otherwise an exception will be raised.</para>
		/// <para>The NoOverwrite flag is not valid with texture locking and will be ignored.</para>
		/// <para>If the texture is not a staging texture and Read is specified, then an exception will be raised.</para>
		/// <para>Discard is only applied to dynamic textures.  If the texture is not dynamic, then an exception will be raised.</para>
		/// </remarks>
		/// <returns>The locked data stream and information about the lock.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the texture is not a dynamic or staging texture.
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the texture sub resource is already locked.</exception>
		public T Lock<T>(BufferLockFlags lockFlags)
			where T : ISubResourceData
		{
			return Lock<T>(0, lockFlags);
		}

		/// <summary>
		/// Function to lock a CPU accessible texture sub resource for reading/writing.
		/// </summary>
		/// <param name="subResource">Sub resource to lock.</param>
		/// <param name="lockFlags">Flags used to lock.</param>
		/// <returns>A stream used to write to the texture.</returns>
		/// <remarks>When locking a texture, the entire texture sub resource is locked and returned.  There is no setting to return a portion of the texture subresource.
		/// <para>This method is only available to textures created with a staging or dynamic usage setting.  Otherwise an exception will be raised.</para>
		/// <para>The NoOverwrite flag is not valid with texture locking and will be ignored.</para>
		/// <para>If the texture is not a staging texture and Read is specified, then an exception will be raised.</para>
		/// <para>Discard is only applied to dynamic textures.  If the texture is not dynamic, then an exception will be raised.</para>
		/// </remarks>
		/// <returns>The locked data stream and information about the lock.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the texture is not a dynamic or staging texture.
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the texture sub resource is already locked.</exception>
		/// <typeparam name="T">The type of locking data.  This must be one of <see cref="GorgonLibrary.Graphics.GorgonTexture1DData">GorgonTexture1DData</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DData">GorgonTexture2DData</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture3DData">GorgonTexture3DData</see></typeparam>
		public T Lock<T>(int subResource, BufferLockFlags lockFlags)
			where T : ISubResourceData
		{
			var mapMode = D3D.MapMode.Write;
			DX.DataStream lockStream = null;
			ISubResourceData data = default(ISubResourceData);

			if (IsLocked(subResource))
				throw new InvalidOperationException("This texture is already locked.");

			// NoOverwrite is not valid for textures, so just remove it.
			if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
				lockFlags &= ~BufferLockFlags.NoOverwrite;

#if DEBUG
			if ((Settings.Usage != BufferUsage.Staging) && (Settings.Usage != BufferUsage.Dynamic))
				throw new ArgumentException("Only dynamic or staging textures may be locked.", "lockFlags");

			if ((Settings.Usage != BufferUsage.Staging) && (((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read) || (lockFlags == BufferLockFlags.Write)))
				throw new ArgumentException("Cannot use Read or Write (without Discard) unless the texture is a staging texture.", "lockFlags");

			if (((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard) && (Settings.Usage != BufferUsage.Dynamic))
				throw new ArgumentException("Cannot use discard unless the texture has dynamic usage.", "lockFlags");
#endif

			if (((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read) && (lockFlags & BufferLockFlags.Write) == BufferLockFlags.Write)
				mapMode = D3D.MapMode.ReadWrite;
			else
			{
				if ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read)
					mapMode = D3D.MapMode.Read;
				if ((lockFlags & BufferLockFlags.Write) == BufferLockFlags.Write)
					mapMode = D3D.MapMode.Write;
			}

			if ((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
				mapMode = D3D.MapMode.WriteDiscard;

			DX.DataBox box = Graphics.Context.MapSubresource(D3DResource, subResource, mapMode, D3D.MapFlags.None, out lockStream);

			if (subResource >= _lock.Count)
				_lock.Add(lockStream);
			else
				_lock[subResource] = lockStream;

			data = GetLockSubResourceData(new GorgonDataStream(lockStream.DataPointer, (int)lockStream.Length), box.RowPitch, box.SlicePitch); 

			return (T)data;
		}

		/// <summary>
		/// Function to unlock a locked texture.
		/// </summary>
		public void Unlock()
		{
			Unlock(0);
		}

		/// <summary>
		/// Function to unlock a locked texture sub resource.
		/// </summary>
		public void Unlock(int subResource)
		{
			if (!IsLocked(subResource))
				return;

			Graphics.Context.UnmapSubresource(D3DResource, subResource);
			_lock[subResource].Dispose();
			_lock[subResource] = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns the texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		protected GorgonTexture(GorgonGraphics graphics, string name, ITextureSettings settings)
			: base(graphics, name)
		{
			_lock = new List<DX.DataStream>(16);
			Settings = settings;
		}
		#endregion
	}
}
