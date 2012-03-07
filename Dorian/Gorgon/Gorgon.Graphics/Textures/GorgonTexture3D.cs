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
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a 3D texture.
	/// </summary>
	public struct GorgonTexture3DSettings
	{
		#region Variables.
		/// <summary>
		/// Default settings for the texture.
		/// </summary>
		/// <remarks>This should be used when loading a texture from memory, a stream or a file.</remarks>
		public static readonly GorgonTexture3DSettings FromFile = new GorgonTexture3DSettings()
		{
			Width = 0,
			Height = 0,
			Depth = 0,
			MipCount = 1,
			Format = BufferFormat.Unknown,
			Usage = BufferUsage.Default,
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
		/// Depth of the texture.
		/// </summary>
		public int Depth;
		/// <summary>
		/// Number of mip levels.
		/// </summary>
		public int MipCount;
		/// <summary>
		/// Format of the texture.
		/// </summary>
		public BufferFormat Format;
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
				return ((Width == 0) || (Width & (Width - 1)) == 0) && 
						((Height == 0) || (Height & (Height - 1)) == 0) &&
						((Depth == 0) || (Depth & (Depth - 1)) == 0);
			}
		}
		#endregion
	}

	/// <summary>
	/// A 3 dimensional texture object.
	/// </summary>
	public class GorgonTexture3D
		: GorgonTexture
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D texture object.
		/// </summary>
		internal D3D.Texture3D D3DTexture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public GorgonTexture3DSettings Settings
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
			GorgonTexture3DSettings settings = new GorgonTexture3DSettings();

			settings.Depth = D3DTexture.Description.Depth;
			settings.Format = (BufferFormat)D3DTexture.Description.Format;
			settings.Height = D3DTexture.Description.Height;
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
			Gorgon.Log.Print("Gorgon 3D texture {0}: Creating D3D11 resource view...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture.DebugName = "Gorgon 3D Texture '" + Name + "'";
			View = new D3D.ShaderResourceView(Graphics.D3DDevice, D3DTexture);
			View.DebugName = "Gorgon 3D Texture '" + Name + "' resource view";

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

					Gorgon.Log.Print("Gorgon 3D texture {0}: D3D 11 Texture destroyed", Diagnostics.LoggingLevel.Verbose, Name);
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

			Gorgon.Log.Print("Gorgon 3D texture {0}: Loading D3D11 3D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			D3DTexture = D3D.Texture3D.FromMemory<D3D.Texture3D>(Graphics.D3DDevice, imageData, imageInfo);

			RetrieveSettings();
			CreateResourceView();
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="data">Data used to populate the texture.</param>
		protected internal void Initialize(GorgonTexture3DData? data)
		{
			D3D.Texture3DDescription desc = new D3D.Texture3DDescription();
			DX.DataBox[] dataRects = null;

			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.Height = Settings.Height;
			desc.Depth = Settings.Depth;
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

			Gorgon.Log.Print("Gorgon 3D texture {0}: Creating D3D11 3D texture...", Diagnostics.LoggingLevel.Verbose, Name);
			if (data != null)
			{
				dataRects = new DX.DataBox[data.Value.Data.Length];
				for (int i = 0; i < dataRects.Length; i++)
				{
					dataRects[i].DataPointer = data.Value.Data[i].PositionPointer;
					dataRects[i].RowPitch = data.Value.Pitch[i];
					dataRects[i].SlicePitch = data.Value.Slice[i];
				}

				D3DTexture = new D3D.Texture3D(Graphics.D3DDevice, desc, dataRects);
			}
			else
				D3DTexture = new D3D.Texture3D(Graphics.D3DDevice, desc);

			CreateResourceView();
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>The <paramref name="format"/> parameter must be set to DDS when saving 3D textures.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is anything other than DDS.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			D3D.ImageFileFormat fileFormat = (D3D.ImageFileFormat)format;
			long position = stream.Position;

			if (format == ImageFileFormat.DDS)
				throw new ArgumentException("Volume textures can only be saved to DDS format.", "format");
			
			D3D.Texture3D.ToStream<D3D.Texture3D>(Graphics.Context, D3DTexture, fileFormat, stream);
			stream.Position = position;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonTexture3D(GorgonGraphics graphics, string name, GorgonTexture3DSettings settings)
			: base(graphics, name)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			Settings = settings;
		}
		#endregion
	}
}
