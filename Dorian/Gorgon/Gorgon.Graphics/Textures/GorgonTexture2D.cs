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

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a 2D texture.
	/// </summary>
	public struct GorgonTexture2DSettings
	{
		#region Variables.
		/// <summary>
		/// Default settings for the texture.
		/// </summary>
		/// <remarks>This should be used when loading a texture from memory, a stream or a file.</remarks>
		public static readonly GorgonTexture2DSettings FromFile = new GorgonTexture2DSettings()
		{
			Width = 0,
			Height = 0,
			Format = BufferFormat.Unknown,
			MipCount = 1,
			ArrayCount = 1,
			Usage = BufferUsage.Default,
			Multisampling = new GorgonMultiSampling(1, 0)
		};

		/// <summary>
		/// Width of the texture.
		/// </summary>
		public int Width;
		/// <summary>
		/// Height of the texture.
		/// </summary>
		public int Height;
		/// <summary>
		/// Format of the texture.
		/// </summary>
		public BufferFormat Format;
		/// <summary>
		/// Number of mip map levels.
		/// </summary>
		public int MipCount;
		/// <summary>
		/// Number of textures in a texture array.
		/// </summary>
		/// <remarks>This is not used for loading files.</remarks>
		public int ArrayCount;
		/// <summary>
		/// Usage levels for the texture.
		/// </summary>
		public BufferUsage Usage;
		/// <summary>
		/// Multisampling settings for the texture.
		/// </summary>
		public GorgonMultiSampling Multisampling;
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
			settings.Multisampling = new GorgonMultiSampling(D3DTexture.Description.SampleDescription.Count, D3DTexture.Description.SampleDescription.Quality);
			settings.Usage = (BufferUsage)D3DTexture.Description.Usage;
			settings.Width = D3DTexture.Description.Width;

			Settings = settings;
		}

		/// <summary>
		/// Function to create the resource view for the texture.
		/// </summary>
		private void CreateResourceView()
		{
			Gorgon.Log.Print("Gorgon 2D texture {0}: Creating D3D11 resource view...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture.DebugName = "Gorgon 2D Texture '" + Name + "'";
			View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture);
			View.DebugName = "Gorgon 2D Texture '" + Name + "' resource view";

			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
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
		/// Function to read image data from a stream.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		/// <param name="filter">Filter to apply to the image.</param>
		/// <param name="mipFilter">Mip map filter to apply to the mip levels of the image.</param>
		protected internal void Initialize(byte[] imageData, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageLoadInformation imageInfo = new D3D.ImageLoadInformation();

			imageInfo.BindFlags = D3D.BindFlags.ShaderResource;

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
			desc.SampleDescription = GorgonMultiSampling.Convert(Settings.Multisampling);

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
			desc.SampleDescription = GorgonMultiSampling.Convert(Settings.Multisampling);

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
			desc.BindFlags = D3D.BindFlags.ShaderResource;
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
			desc.SampleDescription = GorgonMultiSampling.Convert(Settings.Multisampling);

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
		/// <param name="settings">Settings for the image.</param>
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
		/// <param name="settings">Settings for the image.</param>
		/// <param name="filter">Filter to apply to the image.</param>
		/// <param name="mipFilter">Filter to apply to the mip maps.</param>
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
				
				if (D3DTexture != null)
				{
					D3DTexture.Dispose();
					D3DTexture = null;
				}

				Settings = settings;

				Initialize(imageData, filter, mipFilter);

				int textureIndex = -1;

				// Find our texture in any shader and re-assign it.
				textureIndex = Graphics.Shaders.PixelShader.Textures.IndexOf(this);
				if (textureIndex > -1)
				{
					Graphics.Shaders.PixelShader.Textures[textureIndex] = null;
					Graphics.Shaders.PixelShader.Textures[textureIndex] = this;
				}

				// Find our texture in any shader and re-assign it.
				textureIndex = Graphics.Shaders.VertexShader.Textures.IndexOf(this);
				if (textureIndex > -1)
				{
					Graphics.Shaders.VertexShader.Textures[textureIndex] = null;
					Graphics.Shaders.VertexShader.Textures[textureIndex] = this;
				}
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
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
				stream = new MemoryStream();
				this.Save(stream, ImageFileFormat.PNG);
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
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 3D textures.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is anything other than DDS.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when attempting to save a <see cref="GorgonLibrary.Graphics.GorgonTexture2D.IsDepthStencil">depth/stencil texture</see>.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;
			long position = stream.Position;

			if (IsDepthStencil)
				throw new GorgonException(GorgonResult.CannotWrite, "Cannot save a depth/stencil buffer texture.");

			D3D.Texture2D.ToStream<D3D.Texture2D>(Graphics.Context, D3DTexture, fileFormat, stream);
			stream.Position = position;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain to get texture information from.</param>
		internal GorgonTexture2D(GorgonSwapChain swapChain)
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
		internal GorgonTexture2D(GorgonGraphics graphics, string name, GorgonTexture2DSettings settings)
			: base(graphics, name)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.Multisampling.Count < 1)
				settings.Multisampling = new GorgonMultiSampling(1, 0);

			Settings = settings;
		}
		#endregion
	}
}
