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
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using DX = SharpDX;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The base texture object for all textures.
	/// </summary>
	public abstract class GorgonTexture
		: GorgonNamedResource
	{
		#region Variables.
		private int _size = 0;								// Size of the texture, in bytes.
		private GorgonTexture1D _texture1D = null;			// 1D representation of this texture.
		private GorgonTexture2D _texture2D = null;			// 2D representation of this texture.
		private GorgonTexture3D _texture3D = null;			// 3D representation of this texture.
		private IList<DX.DataStream> _lock = null;			// Locks for the texture.
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
		/// Property to return information about format for the shader resource view.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData ViewFormatInformation
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
				if (_size == 0)
					_size = GetTextureSize();

				return _size;
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
		/// Function to retrieve the size of the texture, in bytes.
		/// </summary>
		/// <returns>The size of the texture in bytes.</returns>
		private int GetTextureSize()
		{
			int width = 1;
			int height = 1;
			int depth = 1;
			int mipCount = 1;
			int arrayCount = 1;		// This is always 1 for a 3D texture.
			int result = 0;
			int bytes = 0;
			bool isCompressed = false;

			if (FormatInformation == null)
			{
				FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);

				if (ViewFormatInformation == null)
				{
					if (Settings.ViewFormat == BufferFormat.Unknown)
						ViewFormatInformation = FormatInformation;
					else
						ViewFormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.ViewFormat);
				}
			}

			bytes = FormatInformation.SizeInBytes;
			isCompressed = FormatInformation.IsCompressed;

			if (bytes == 0)
				return 0;

			if (_texture1D != null)
			{
				width = _texture1D.Settings.Width;
				mipCount = _texture1D.Settings.MipCount;
				arrayCount = _texture1D.Settings.ArrayCount;
			}

			if (_texture2D != null)
			{
				width = _texture2D.Settings.Width;
				height = _texture2D.Settings.Height;
				mipCount = _texture2D.Settings.MipCount;
				arrayCount = _texture2D.Settings.ArrayCount;
			}

			if (_texture3D != null)
			{
				width = _texture3D.Settings.Width;
				height = _texture3D.Settings.Height;
				depth = _texture3D.Settings.Depth;
				mipCount = _texture3D.Settings.MipCount;
			}

			for (int arrayIndex = 0; arrayIndex < arrayCount; arrayIndex++)
			{
				int mipWidth = width;
				int mipHeight = height;

				for (int mipIndex = 0; mipIndex < mipCount; mipIndex++)
				{
					int slicePitch = 0;

					if (isCompressed)
						slicePitch = 1.Max(((mipHeight + 3) / 4)) * (1.Max(((mipWidth + 3) / 4)) * bytes);
					else
						slicePitch = (mipWidth * bytes) * mipHeight;
						
					for (int slice = 0; slice < depth; slice++)
						result += slicePitch;

					if (mipWidth > 1)
						mipWidth >>= 1;
					if (mipHeight > 1)
						mipHeight >>= 1;
					if (depth > 1)
						depth >>= 1;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to create the resource object.
		/// </summary>
		protected override void CreateDefaultResourceView()
		{
			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
			if (Settings.ViewFormat != BufferFormat.Unknown)
			{
				ViewFormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
			}
			else
			{
				ViewFormatInformation = FormatInformation;
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
		/// Function to retrieve information about an existing texture.
		/// </summary>
		/// <returns>New settings for the texture.</returns>
		protected ITextureSettings GetTextureInformation()
		{
			ITextureSettings newSettings = null;
			BufferFormat viewFormat = BufferFormat.Unknown;

			if (Settings != null)
				viewFormat = Settings.ViewFormat;

			if (_texture1D != null)
			{
				D3D.Texture1DDescription desc = ((D3D.Texture1D)_texture1D.D3DResource).Description;
				newSettings = new GorgonTexture1DSettings();
				newSettings.Width = desc.Width;
				newSettings.Height = 1;
				newSettings.ArrayCount = desc.ArraySize;
				newSettings.Depth = 1;
				newSettings.Format = (BufferFormat)desc.Format;
				newSettings.MipCount = desc.MipLevels;
				newSettings.Usage = (BufferUsage)desc.Usage;
				newSettings.ViewFormat = BufferFormat.Unknown;
				newSettings.Multisampling = new GorgonMultisampling(1, 0);
			}

			if (_texture2D != null)
			{
				D3D.Texture2DDescription desc = ((D3D.Texture2D)_texture2D.D3DResource).Description;
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
			}

			if (_texture3D != null)
			{
				D3D.Texture3DDescription desc = ((D3D.Texture3D)_texture3D.D3DResource).Description;
				newSettings = new GorgonTexture3DSettings();
				newSettings.Width = desc.Width;
				newSettings.Height = desc.Height;
				newSettings.ArrayCount = 1;
				newSettings.Depth = desc.Depth;
				newSettings.Format = (BufferFormat)desc.Format;
				newSettings.MipCount = desc.MipLevels;
				newSettings.Usage = (BufferUsage)desc.Usage;
				newSettings.ViewFormat = BufferFormat.Unknown;
				newSettings.Multisampling = new GorgonMultisampling(1, 0);
			}

			// Preserve any custom view format.
			newSettings.ViewFormat = viewFormat;

			return newSettings;
		}

		/// <summary>
		/// Function to read image data from an array of bytes.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		/// <param name="imageInfo">Information to pass to the image loading method.</param>
		protected abstract void InitializeImpl(byte[] imageData, D3D.ImageLoadInformation imageInfo);

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The initial data can be a <see cref="GorgonLibrary.GorgonDataStream">GorgonDataStream</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DData">GorgonTexture2DData</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture3DData">GorgonTexture3DData</see></remarks>
		protected abstract void InitializeImpl(IEnumerable<ISubResourceData> initialData);

		/// <summary>
		/// Function to copy a resource in its entirety.
		/// </summary>
		/// <param name="source">Resource to copy.</param>
		/// <param name="destination">Destination resource.</param>
		internal virtual void CopyResourceProxy(GorgonTexture source, GorgonTexture destination)
		{
			Graphics.Context.CopyResource(source.D3DResource, destination.D3DResource);
		}

		/// <summary>
		/// Function to copy a sub resource.
		/// </summary>
		/// <param name="source">The source resource.</param>
		/// <param name="destination">The destination resource.</param>
		/// <param name="srcSubResourceIndex">Index of the source subresource.</param>
		/// <param name="destSubResourceIndex">Index of the destination subresource.</param>
		/// <param name="sourceRegion">The source region to copy.</param>
		/// <param name="x">Destination horizontal coordindate.</param>
		/// <param name="y">Destination vertical coordinate.</param>
		/// <param name="z">Destination depth coordinate.</param>
		internal virtual void CopySubResourceProxy(GorgonTexture source, GorgonTexture destination, int srcSubResourceIndex, int destSubResourceIndex, D3D.ResourceRegion? sourceRegion, int x, int y, int z)
		{
			Graphics.Context.CopySubresourceRegion(source.D3DResource, srcSubResourceIndex, sourceRegion, destination.D3DResource, destSubResourceIndex, x, y, z);
		}

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The initial data can be a <see cref="GorgonLibrary.GorgonDataStream">GorgonDataStream</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DData">GorgonTexture2DData</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture3DData">GorgonTexture3DData</see></remarks>
		internal void Initialize(IEnumerable<ISubResourceData> initialData)
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
		/// Function to read image data from an array of bytes.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		internal void InitializeFileData(byte[] imageData)
		{
			try
			{
				D3D.ImageLoadInformation imageInfo = new D3D.ImageLoadInformation();

				if (Settings.Usage != BufferUsage.Staging)
					imageInfo.BindFlags = D3D.BindFlags.ShaderResource;
				else
					imageInfo.BindFlags = D3D.BindFlags.None;

				// Rebind as a render target.
				if ((_texture2D != null) && (_texture2D.IsRenderTarget))
					imageInfo.BindFlags |= D3D.BindFlags.RenderTarget;

				switch (Settings.Usage)
				{
					case BufferUsage.Staging:
						imageInfo.CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write;
						break;
					case BufferUsage.Dynamic:
						imageInfo.CpuAccessFlags = D3D.CpuAccessFlags.Write;
						break;
					default:
						imageInfo.CpuAccessFlags = D3D.CpuAccessFlags.None;
						break;
				}

				imageInfo.Depth = Settings.Depth;
				imageInfo.Filter = (D3D.FilterFlags)Settings.FileFilter;
				imageInfo.FirstMipLevel = 0;
				imageInfo.Format = (SharpDX.DXGI.Format)Settings.Format;
				imageInfo.Width = Settings.Width;
				imageInfo.Height = Settings.Height;
				imageInfo.MipFilter = (D3D.FilterFlags)Settings.FileMipFilter;
				imageInfo.MipLevels = Settings.MipCount;
				imageInfo.OptionFlags = D3D.ResourceOptionFlags.None;
				imageInfo.Usage = (D3D.ResourceUsage)Settings.Usage;

				Gorgon.Log.Print("{0} {1}: Loading D3D11 texture resource...", Diagnostics.LoggingLevel.Verbose, GetType().Name, Name);
				InitializeImpl(imageData, imageInfo);

				Settings = GetTextureInformation();
				CreateDefaultResourceView();

				GorgonRenderStatistics.TextureCount++;
				GorgonRenderStatistics.TextureSize += SizeInBytes;
			}
			catch
			{
				if (D3DResource != null)
				{
					D3DResource.Dispose();
					D3DResource = null;
				}
				throw;
			}
		}

		/// <summary>
		/// Function to save the texture data to an array of bytes.
		/// </summary>
		/// <param name="format">Image format to use.</param>
		/// <returns>An array of bytes containing the image data.</returns>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 1D or 3D textures.
		/// <para>If the texture format is not compatiable with a file format, then an exception will be raised.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is anything other than DDS for a volume (3D) or 1D texture.
		/// <para>-or-</para>
		/// <para>Thrown when the file cannot be saved with the requested file <paramref name="format"/>.</para>
		/// </exception>
		public byte[] Save(ImageFileFormat format)
		{
			MemoryStream stream = new MemoryStream();
			Save(stream, format);
			stream.Position = 0;
			return stream.ToArray();
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 1D or 3D textures.
		/// <para>If the texture format is not compatiable with a file format, then an exception will be raised.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">
		/// Thrown when the format is anything other than DDS for a volume (3D) or 1D texture.
		/// <para>-or-</para>
		/// <para>Thrown when the format is anything other than DDS.</para>
		/// </exception>
		public abstract void Save(System.IO.Stream stream, ImageFileFormat format);

		/// <summary>
		/// Function to save the texture data to a file.
		/// </summary>
		/// <param name="fileName">Name of the file to save into.</param>
		/// <param name="format">Image format to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the format is anything other than DDS for a volume (3D) or 1D texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the file cannot be saved with the requested file <paramref name="format"/>.</para>
		/// </exception>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 1D or 3D textures.
		/// <para>If the texture format is not compatiable with a file format, then an exception will be raised.</para>
		/// </remarks>
		public void Save(string fileName, ImageFileFormat format)
		{
			GorgonDebug.AssertParamString(fileName, "fileName");

			FileStream stream = null;

			try
			{
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream, format);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
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
		public void Copy(GorgonTexture texture)
		{
			GorgonDebug.AssertNull<GorgonTexture>(texture, "texture");

#if DEBUG
			if (texture.GetType() != this.GetType())
				throw new ArgumentException("The texure '" + texture.Name + "' is of type '" + texture.GetType().FullName + "' and cannot be copied to or from the type '" + this.GetType().FullName + "'.", "texture");

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count) || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
				throw new InvalidOperationException("Cannot copy textures with different multisampling parameters.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((string.Compare(texture.FormatInformation.Group, FormatInformation.Group, true) != 0) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((texture.Settings.Width != Settings.Width) || (texture.Settings.Height != Settings.Height))
				throw new ArgumentException("The texture sizes do not match.", "texture");

			// Ensure that the SM2_a_b devices don't try and copy between CPU and GPU accessible memory.
			if ((this is GorgonTexture3D) && (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (this.Settings.Usage == BufferUsage.Staging))
				throw new InvalidOperationException("This 3D texture is CPU accessible and cannot be copied.");
#endif

			// If we have multisampling enabled, then copy the entire sub resource.
			Graphics.Context.CopyResource(texture.D3DResource, D3DResource);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <remarks>Use this to copy data to this texture.  If the texture is non CPU accessible texture then an exception is raised.</remarks>
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

			if ((_texture2D != null) && (_texture2D.IsDepthStencil))
				throw new InvalidOperationException("Cannot update a texture used as a depth/stencil buffer.");
#endif

			SharpDX.DataBox box = new SharpDX.DataBox()
			{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			D3D.ResourceRegion region = new D3D.ResourceRegion();

			if (_texture1D != null)
			{
				region.Front = 0;
				region.Back = 1;
				region.Left = 0;
				region.Right = Settings.Width;
				region.Top = 0;
				region.Bottom = 1;
			}
			else if (_texture2D != null)
			{
				region.Front = 0;
				region.Back = 1;
				region.Left = 0;
				region.Right = Settings.Width;
				region.Top = 0;
				region.Bottom = Settings.Height;
			} 
			else if (_texture3D != null)
			{
				region.Front = 0;
				region.Back = Settings.Depth;
				region.Left = 0;
				region.Right = Settings.Width;
				region.Top = 0;
				region.Bottom = Settings.Height;
			}

			Graphics.Context.UpdateSubresource(box, D3DResource, subResource, region);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <remarks>Use this to copy data to this texture.  If the texture is non CPU accessible texture then an exception is raised.</remarks>
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
			D3D.MapMode mapMode = D3D.MapMode.Write;
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

			if (_texture1D != null)
				data = new GorgonTexture1DData(new GorgonDataStream(lockStream.DataPointer, (int)lockStream.Length));
			else if (_texture2D != null)
				data = new GorgonTexture2DData(new GorgonDataStream(lockStream.DataPointer, (int)lockStream.Length), box.RowPitch);
			else if (_texture3D != null)
				data = new GorgonTexture3DData(new GorgonDataStream(lockStream.DataPointer, (int)lockStream.Length), box.RowPitch, box.SlicePitch);

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
			_texture1D = this as GorgonTexture1D;
			_texture2D = this as GorgonTexture2D;
			_texture3D = this as GorgonTexture3D;			
		}
		#endregion
	}
}
