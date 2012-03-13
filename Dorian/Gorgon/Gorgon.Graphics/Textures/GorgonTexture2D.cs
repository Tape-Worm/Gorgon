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
	/// Settings for a 2D texture.
	/// </summary>
	public class GorgonTexture2DSettings
		: ITextureSettings
	{
		#region Variables.
		/// <summary>
		/// Default settings for the texture.
		/// </summary>
		/// <remarks>This should be used when loading a texture from memory, a stream or a file.</remarks>
		public static readonly GorgonTexture2DSettings FromFile = new GorgonTexture2DSettings();
		#endregion 

		#region Properties.
		/// <summary>
		/// Property to set or return the size of the texture.
		/// </summary>
		public Size Size
		{
			get
			{
				return new Size(Width, Height);
			}
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DSettings"/> class.
		/// </summary>
		public GorgonTexture2DSettings()
		{
			Width = 0;
			Height = 0;
			Format = BufferFormat.Unknown;
			MipCount = 1;
			ArrayCount = 1;
			Multisampling = new GorgonMultisampling(1, 0);			
			ViewFormat = BufferFormat.Unknown;
			Usage = BufferUsage.Default;
		}
		#endregion

		#region ITextureSettings Members
		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When setting this value to TRUE, ensure that the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>This only applies to 2D textures.  All other textures will return FALSE.  The default value is FALSE.</para></remarks>
		public bool IsTextureCube
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>When loading a file, leave as 0 to use the width from the file source.</remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a file, leave as 0 to use the height from the file source.
		/// <para>This applies to 2D and 3D textures only.</para></remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a file, leave as 0 to use the width from the depth source.
		/// <para>This applies to 3D textures only.</para></remarks>
		int ITextureSettings.Depth
		{
			get
			{
				return 1;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return the format of a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// When loading a texture from a file, leave this as Unknown to get the file format from the source file.
		/// <para>This sets the format of the texture data.  If you want to change the format of a texture when being sampled in a shader, then set the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.ViewFormat">ViewFormat</see> property to anything other than Unknown.</para></remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the shader view format.
		/// </summary>
		/// <value></value>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  The default value is Unknown.</remarks>
		public BufferFormat ViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of textures there are in a texture array.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 1D and 2D textures, 3D textures always have this value set to 1.  The default value is 1.</remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip maps in a texture.
		/// </summary>
		/// <value></value>
		/// <remarks>To have the system generate mipmaps for you, set this value to 0.  The default value for this setting is 1.</remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the texture.
		/// </summary>
		/// <value></value>
		/// <remarks>This only applies to 2D textures.  The default value is a count of 1, and a quality of 0 (no multisampling).
		/// <para>Note that multisampled textures cannot have sub resources (e.g. mipmaps), so the <see cref="P:GorgonLibrary.Graphics.ITextureSettings.MipCount">MipCount</see> should be set to 1.</para>
		/// </remarks>
		public GorgonMultisampling Multisampling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage for the texture.
		/// </summary>
		/// <value></value>
		/// <remarks>The default value is Default.</remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		public bool IsPowerOfTwo
		{
			get
			{
				return ((Width == 0) || (Width & (Width - 1)) == 0) &&
						((Height == 0) || (Height & (Height - 1)) == 0);
			}
		}
		#endregion
	}

	/// <summary>
	/// A 2 dimensional texture object.
	/// </summary>
	public class GorgonTexture2D
		: GorgonTexture
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D texture object.
		/// </summary>
		internal D3D.Texture2D D3DTexture
		{
			get;
			private set;
		}

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
		public GorgonTexture2DSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the texture settings from an existing texture.
		/// </summary>
		private void RetrieveSettings()
		{
			GorgonTexture2DSettings settings = new GorgonTexture2DSettings();

			settings.ArrayCount = D3DTexture.Description.ArraySize;
			settings.Format = (BufferFormat)D3DTexture.Description.Format;
			settings.Height = D3DTexture.Description.Height;
			settings.MipCount = D3DTexture.Description.MipLevels;
			settings.Multisampling = new GorgonMultisampling(D3DTexture.Description.SampleDescription.Count, D3DTexture.Description.SampleDescription.Quality);
			settings.Usage = (BufferUsage)D3DTexture.Description.Usage;
			settings.Width = D3DTexture.Description.Width;

			Settings = settings;
		}

		/// <summary>
		/// Function to create the resource view for the texture.
		/// </summary>
		private void CreateResourceView()
		{
			if (Settings.Usage != BufferUsage.Staging)
			{
				Gorgon.Log.Print("Gorgon 2D texture {0}: Creating D3D11 resource view...", Diagnostics.LoggingLevel.Verbose, Name);
				D3DTexture.DebugName = "Gorgon 2D Texture '" + Name + "'";
				View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture);
				View.DebugName = "Gorgon 2D Texture '" + Name + "' resource view";
			}

			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
		}

		/// <summary>
		/// Function to copy a texture to this texture using a slow (but accurate) procedure.
		/// </summary>
		protected void SlowCopy(GorgonTexture2D texture)
		{
			System.IO.MemoryStream stream = null;
			byte[] imageData = null;

			try
			{
				stream = new MemoryStream();
				texture.Save(stream, ImageFileFormat.DDS);
				imageData = stream.ToArray();

				if (View != null)
				{
					View.Dispose();
					View = null;
				}

				if (D3DTexture != null)
				{
					D3DTexture.Dispose();
					D3DTexture = null;
				}

				Initialize(imageData, ImageFilters.None, ImageFilters.None);

				if (RenderTarget != null)
					RenderTarget.UpdateResourceView();

				Graphics.Shaders.Reseat(this);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (D3DTexture != null)
						D3DTexture.Dispose();

					Gorgon.Log.Print("Gorgon 2D texture {0}: D3D 11 Texture destroyed", Diagnostics.LoggingLevel.Verbose, Name);
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to copy a resource in its entirety.
		/// </summary>
		/// <param name="source">Resource to copy.</param>
		/// <param name="destination">Destination resource.</param>
		internal virtual void CopyResourceProxy(D3D.Texture2D source, D3D.Texture2D destination)
		{
			Graphics.Context.CopyResource(source, destination);
		}

		/// <summary>
		/// Function to copy a sub resource.
		/// </summary>
		/// <param name="source">The source resource.</param>
		/// <param name="destination">The destination resource.</param>
		/// <param name="srcSubresourceIndex">Index of the source subresource.</param>
		/// <param name="destSubresourceIndex">Index of the destination subresource.</param>
		/// <param name="sourceRegion">The source region to copy.</param>
		/// <param name="destinationPosition">Destination position.</param>
		internal virtual void CopySubresourceProxy(D3D.Texture2D source, D3D.Texture2D destination, int srcSubresourceIndex, int destSubresourceIndex, D3D.ResourceRegion? sourceRegion, Vector3 destinationPosition)
		{
			Graphics.Context.CopySubresourceRegion(source, srcSubresourceIndex, sourceRegion, destination, destSubresourceIndex, (int)destinationPosition.X, (int)destinationPosition.Y, (int)destinationPosition.Z);
		}

		/// <summary>
		/// Function to create the texture.
		/// </summary>
		/// <param name="graphics">Graphics interface that created this object.</param>
		/// <param name="name">Name of the object.</param>
		/// <param name="settings">Settings for the object.</param>
		/// <returns>The new 2D texture.</returns>
		internal static GorgonTexture2D CreateTexture(GorgonGraphics graphics, string name, GorgonTexture2DSettings settings)
		{
			GorgonTexture2D texture = null;

			if (graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM2_a_b)
				texture = new GorgonTexture2D(graphics, name, settings);
			else
				texture = new GorgonTexture2DSM2(graphics, name, settings);

			return texture;
		}

		/// <summary>
		/// Function to create the texture.
		/// </summary>
		/// <param name="swapChain">Swap chain to retrieve texture from.</param>
		/// <returns>The texture for the swap chain.</returns>
		internal static GorgonTexture2D CreateTexture(GorgonSwapChain swapChain)
		{
			GorgonTexture2D texture = null;

			if (swapChain.Graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM2_a_b)
				texture = new GorgonTexture2D(swapChain);
			else
				texture = new GorgonTexture2DSM2(swapChain);

			return texture;
		}

		/// <summary>
		/// Function to read image data from an array of bytes.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		/// <param name="filter">Filter to apply to the image.</param>
		/// <param name="mipFilter">Mip map filter to apply to the mip levels of the image.</param>
		protected internal void Initialize(byte[] imageData, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageLoadInformation imageInfo = new D3D.ImageLoadInformation();

			if (Settings.Usage != BufferUsage.Staging)
				imageInfo.BindFlags = D3D.BindFlags.ShaderResource;
			else
				imageInfo.BindFlags = D3D.BindFlags.None;

			if (RenderTarget != null)
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
			imageInfo.Depth = 0;
			imageInfo.Filter = (D3D.FilterFlags)filter;
			imageInfo.FirstMipLevel = 0;
			imageInfo.Format = (SharpDX.DXGI.Format)Settings.Format;
			imageInfo.Height = Settings.Height;
			imageInfo.Width = Settings.Width;
			imageInfo.MipFilter = (D3D.FilterFlags)mipFilter;
			imageInfo.MipLevels = Settings.MipCount;
			imageInfo.OptionFlags = D3D.ResourceOptionFlags.None;
			imageInfo.Usage = (D3D.ResourceUsage)Settings.Usage;
			imageInfo.Width = Settings.Width;

			Gorgon.Log.Print("Gorgon 2D texture {0}: Loading D3D11 2D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = D3D.Texture2D.FromMemory<D3D.Texture2D>(Graphics.D3DDevice, imageData, imageInfo);

			RetrieveSettings();
			CreateResourceView();
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

			Gorgon.Log.Print("Gorgon 2D texture {0}: Creating 2D render target texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = new D3D.Texture2D(Graphics.D3DDevice, desc);
			
			CreateResourceView();
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

			Gorgon.Log.Print("Gorgon 2D texture {0}: Creating 2D depth/stencil texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = new D3D.Texture2D(Graphics.D3DDevice, desc);
			IsDepthStencil = true;
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="data">Data used to populate the texture.</param>
		protected internal void Initialize(GorgonTexture2DData? data)
		{
			D3D.Texture2DDescription desc = new D3D.Texture2DDescription();
			DX.DataRectangle[] dataRects = null;

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
			switch(Settings.Usage)
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

			Gorgon.Log.Print("Gorgon 2D texture {0}: Creating D3D11 2D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			if (data != null)
			{
				dataRects = new DX.DataRectangle[data.Value.Data.Length];
				for (int i = 0; i < dataRects.Length; i++)
				{
					dataRects[i].DataPointer = data.Value.Data[i].PositionPointer;
					dataRects[i].Pitch = data.Value.Pitch[i];
				}

				D3DTexture = new D3D.Texture2D(Graphics.D3DDevice, desc, dataRects);
			}
			else
				D3DTexture = new D3D.Texture2D(Graphics.D3DDevice, desc);

			CreateResourceView();
		}

		/// <summary>
		/// Function to copy a GDI bitmap to this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <remarks>Use this to copy data from a GDI+ bitmap into the texture.
		/// <para>This overload will preserve the <see cref="P:GorgonLibrary.Graphics.GorgonTexture2D.Settings">settings</see> of the texture and make the bitmap conform to those settings.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public void Copy(Image image)
		{
			Copy(image, Settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to copy a GDI bitmap to this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <param name="settings">Settings to apply to the current texture.</param>
		/// <remarks>Use this to copy data from a GDI+ bitmap into the texture.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public void Copy(Image image, GorgonTexture2DSettings settings)
		{
			Copy(image, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to copy a GDI bitmap to this image.
		/// </summary>
		/// <param name="image">Image to copy.</param>
		/// <param name="settings">Settings to apply to the current texture.</param>
		/// <param name="filter">Filter to apply to the copied image.</param>
		/// <param name="mipFilter">Filter to apply to the mip maps for the copied image.</param>
		/// <remarks>Use this to copy data from a GDI+ bitmap into the texture.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public void Copy(Image image, GorgonTexture2DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageInformation? info = null;
			System.IO.MemoryStream stream = null;
			byte[] imageData = null;
			
			try
			{				
				if ((image.Width <= 0) || (image.Width >= Graphics.Textures.MaxWidth))
					throw new ArgumentException("The texture width must be at least 1 pixel, or less than " + Graphics.Textures.MaxWidth.ToString() + " pixels.", "image");

				if ((image.Height <= 0) || (image.Height >= Graphics.Textures.MaxHeight))
					throw new ArgumentException("The texture height must be at least 1 pixel, or less than " + Graphics.Textures.MaxHeight.ToString() + " pixels.", "image");

				stream = new MemoryStream();
				image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				imageData = stream.ToArray();

				// Get the file information.
				info = D3D.ImageInformation.FromMemory(imageData);

				// Assign defaults.
				if (info != null)
				{
					// Only load 2D textures.
					if (info.Value.ResourceDimension != D3D.ResourceDimension.Texture2D)
						throw new ArgumentException("The specified texture is not a 2D texture.", "stream");

					if (settings.Format == BufferFormat.Unknown)
						settings.Format = (BufferFormat)info.Value.Format;
					if (settings.Width < 1)
						settings.Width = info.Value.Width;
					if (settings.Height < 1)
						settings.Height = info.Value.Height;
					if (settings.MipCount == 0)
						settings.MipCount = 1;
				}
				settings.ArrayCount = 1;

				// Validate the texture settings.
				Graphics.Textures.ValidateTexture2D(ref settings, true);

				if (View != null)
				{
					View.Dispose();
					View = null;
				}
				
				if (D3DTexture != null)
				{
					D3DTexture.Dispose();
					D3DTexture = null;
				}

				Settings = settings;

				Initialize(imageData, filter, mipFilter);

				if (RenderTarget != null)
					RenderTarget.UpdateResourceView();

				Graphics.Shaders.Reseat(this);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
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
		/// <para>Pass NULL (Nothing in VB.Net) to the sourceRegion parameter to copy the entire sub resource.</para>
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
		public void CopySubresource(GorgonTexture2D texture, int subResource, int destSubResource, Rectangle? sourceRegion, Vector2 destination)
		{
			GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count) || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
				throw new ArgumentException("Cannot copy textures with different multisampling parameters.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((string.Compare(texture.FormatInformation.Group, FormatInformation.Group, true) != 0) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((this == texture) && (subResource == destSubResource))
				throw new ArgumentException("Cannot copy to and from the same sub resource on the same texture.");

			// If we have multisampling enabled, then copy the entire sub resource.
			if ((Settings.Multisampling.Count > 1) || (Settings.Multisampling.Quality > 0) || (sourceRegion == null))
				CopySubresourceProxy(texture.D3DTexture, D3DTexture, subResource, destSubResource, null, Vector3.Zero);
			else
				CopySubresourceProxy(texture.D3DTexture, D3DTexture, subResource, destSubResource, new D3D.ResourceRegion()
				{
					Back = 1,
					Front = 0,
					Top = sourceRegion.Value.Top,
					Left = sourceRegion.Value.Left,
					Right = sourceRegion.Value.Right,
					Bottom = sourceRegion.Value.Bottom
				}, new Vector3(destination, 0.0f));
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
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubresource(GorgonTexture2D texture, Rectangle sourceRegion, Vector2 destination)
		{
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");

			CopySubresource(texture, 0, 0, sourceRegion, destination);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters and the sourceRegion and destination parameters will be ignored.  The same is true for Depth/Stencil buffer textures.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void CopySubresource(GorgonTexture2D texture)
		{
			if (texture == this)
				throw new ArgumentException("The source texture and this texture are the same.  Cannot copy.", "texture");

			CopySubresource(texture, 0, 0, null, Vector2.Zero);
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
		public void CopySubresource(GorgonTexture2D texture, int subResource, int destSubResource)
		{
			GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

			CopySubresource(texture, subResource, destSubResource, null, Vector2.Zero);
		}

		/// <summary>
		/// Function to copy a another texture into this texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <remarks>
		/// This overload will copy the -entire- texture, including mipmaps, array levels, etc...  Use <see cref="M:GorgonLibrary.Graphics.GorgonTexture2D.CopySubresource(GorgonTexture2D, int, int, System.Drawing.Rectangle, SlimMath.Vector2)">CopySubresource</see> to copy a portion of the texture.
		/// <para>This method will -not- perform stretching, filtering or clipping.</para>
		/// <para>The <paramref name="texture"/> dimensions must be have the same dimensions as this texture.  If they do not, an exception will be thrown.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texure sizes are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// </exception>
		public void Copy(GorgonTexture2D texture)
		{
			GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

			if (Settings.Usage == BufferUsage.Immutable)
				throw new InvalidOperationException("Cannot copy to an immutable resource.");

			if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count) || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
				throw new InvalidOperationException("Cannot copy textures with different multisampling parameters.");

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) && ((string.Compare(texture.FormatInformation.Group, FormatInformation.Group, true) != 0) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
				throw new ArgumentException("Cannot copy because these formats: '" + texture.Settings.Format.ToString() + "' and '" + Settings.Format.ToString() + "', cannot be converted.", "texture");

			if ((texture.Settings.Width != Settings.Width) || (texture.Settings.Width != Settings.Height))
				throw new ArgumentException("The texture sizes do not match.", "texture");
			
			// If we have multisampling enabled, then copy the entire sub resource.
			Graphics.Context.CopyResource(texture.D3DTexture, D3DTexture);
		}

		/// <summary>
		/// Function to save this image to a GDI bitmap.
		/// </summary>
		/// <remarks>Use this to copy data from this texture into a GDI+ bitmap.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public Image ToGDIBitmap()
		{
			MemoryStream stream = null;

			try
			{
				// If this format is not compatible, then throw an exception.
				if (Settings.Format != BufferFormat.R8G8B8A8)
					throw new GorgonException(GorgonResult.CannotCreate, "To convert to GDI the format must be one of the R8G8B8A8 group members.");

				stream = new MemoryStream();
				Save(stream, ImageFileFormat.PNG);
				stream.Position = 0;

				return Image.FromStream(stream);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}			
		}


		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">
		/// Thrown when the format is anything other than DDS for a volume (3D) texture.
		///   <para>-or-</para>
		///   <para>Thrown when the format is anything other than DDS.</para>
		///   </exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;

			if (IsDepthStencil)
				throw new GorgonException(GorgonResult.CannotWrite, "Cannot save a depth/stencil buffer texture.");

			// We can only save to 32 bit RGBA uint normalized formats if we're not using DDS, so we have to convert.
			if ((format != ImageFileFormat.DDS) && (Settings.Format != BufferFormat.R8G8B8A8))
				throw new ArgumentException("Cannot save the format '" + Settings.Format.ToString() + "' to a " + format.ToString() + " file.  DDS is the only file format that may be used with that texture format.");
					
			D3D.Texture2D.ToStream<D3D.Texture2D>(Graphics.Context, D3DTexture, fileFormat, stream);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain to get texture information from.</param>
		protected GorgonTexture2D(GorgonSwapChain swapChain)
			: base(swapChain.Graphics, swapChain.Name + "_Internal_Texture_" + Guid.NewGuid().ToString())
		{
			D3DTexture = D3D.Texture2D.FromSwapChain<D3D.Texture2D>(swapChain.GISwapChain, 0);
			D3DTexture.DebugName = "Gorgon swap chain texture '" + Name + "'";

			RetrieveSettings();

			RenderTarget = swapChain;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonTexture2D(GorgonGraphics graphics, string name, GorgonTexture2DSettings settings)
			: base(graphics, name)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.Multisampling.Count < 1)
				settings.Multisampling = new GorgonMultisampling(1, 0);

			Settings = settings;
		}
		#endregion
	}
}
