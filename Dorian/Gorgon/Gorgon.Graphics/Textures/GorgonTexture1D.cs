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
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a 1D texture.
	/// </summary>
	public struct GorgonTexture1DSettings
	{
		#region Variables.
		/// <summary>
		/// Default settings for the texture.
		/// </summary>
		/// <remarks>This should be used when loading a texture from memory, a stream or a file.</remarks>
		public static readonly GorgonTexture1DSettings FromFile = new GorgonTexture1DSettings()
		{
			Width = 0,
			Format = BufferFormat.Unknown,
			MipCount = 1,
			ArrayCount = 1,
			Usage = BufferUsage.Default
		};

		/// <summary>
		/// Width of the texture.
		/// </summary>
		public int Width;
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
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		public bool IsPowerOfTwo
		{
			get
			{
				return ((Width == 0) || (Width & (Width - 1)) == 0);
			}
		}
		#endregion
	}

	/// <summary>
	/// A 1 dimension texture object.
	/// </summary>
	public class GorgonTexture1D
		: GorgonTexture
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D texture object.
		/// </summary>
		internal D3D.Texture1D D3DTexture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public GorgonTexture1DSettings Settings
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
			GorgonTexture1DSettings settings = new GorgonTexture1DSettings();

			settings.ArrayCount = D3DTexture.Description.ArraySize;
			settings.Format = (BufferFormat)D3DTexture.Description.Format;
			settings.MipCount = D3DTexture.Description.MipLevels;
			settings.Usage = (BufferUsage)D3DTexture.Description.Usage;
			settings.Width = D3DTexture.Description.Width;

			Settings = settings;
		}

		/// <summary>
		/// Function to create the resource view for the texture.
		/// </summary>
		private void CreateResourceView()
		{
			Gorgon.Log.Print("Gorgon 1D texture {0}: Creating D3D11 resource view...", Diagnostics.LoggingLevel.Verbose, Name);

			D3DTexture.DebugName = "Gorgon 1D Texture '" + Name + "'";
			View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture);
			View.DebugName = "Gorgon 1D Texture '" + Name + "' resource view";

			FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
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
			imageInfo.Width = Settings.Width;
			imageInfo.MipFilter = (D3D.FilterFlags)mipFilter;
			imageInfo.MipLevels = Settings.MipCount;
			imageInfo.OptionFlags = D3D.ResourceOptionFlags.None;
			imageInfo.Usage = (D3D.ResourceUsage)Settings.Usage;
			imageInfo.Width = Settings.Width;

			Gorgon.Log.Print("Gorgon 2D texture {0}: Loading D3D11 1D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = D3D.Texture1D.FromMemory<D3D.Texture1D>(Graphics.D3DDevice, imageData, imageInfo);
			
			RetrieveSettings();
			CreateResourceView();
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
					Gorgon.Log.Print("Gorgon 1D texture {0}: D3D 11 Texture destroyed", Diagnostics.LoggingLevel.Verbose, Name);
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="data">Data used to populate the texture.</param>
		protected internal void Initialize(GorgonDataStream data)
		{
			D3D.Texture1DDescription desc = new D3D.Texture1DDescription();

			desc.ArraySize = Settings.ArrayCount;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.MipLevels = Settings.MipCount;
			desc.BindFlags = D3D.BindFlags.ShaderResource;
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

			Gorgon.Log.Print("Gorgon 2D texture {0}: Creating D3D11 1D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			if (data != null)
			{
				using (DX.DataStream stream = new DX.DataStream(data.PositionPointer, data.Length - data.Position, true, true))
					D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc, stream);
			}
			else
				D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc);

			CreateResourceView();			
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;
			long position = stream.Position;
			D3D.Texture1D.ToStream<D3D.Texture1D>(Graphics.Context, D3DTexture, fileFormat, stream);
			stream.Position = position;
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
		internal GorgonTexture1D(GorgonGraphics graphics, string name, GorgonTexture1DSettings settings)
			: base(graphics, name)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			Settings = settings;
		}
		#endregion
	}
}
