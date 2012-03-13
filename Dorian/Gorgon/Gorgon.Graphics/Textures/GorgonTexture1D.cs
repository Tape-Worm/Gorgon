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
		/// Function to read image data from an array of bytes.
		/// </summary>
		/// <param name="imageData">Array of bytes holding the image data.</param>
		/// <param name="imageInfo">Information to pass to the image loading method.</param>
		protected override void InitializeImpl(byte[] imageData, D3D.ImageLoadInformation imageInfo)
		{
			// We don't need depth or height for this guy.
			imageInfo.Depth = 0;
			imageInfo.Height = 0;
						
			D3DTexture = D3D.Texture1D.FromMemory<D3D.Texture1D>(Graphics.D3DDevice, imageData, imageInfo);
		}

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		protected override void InitializeImpl(IEnumerable<ISubResourceData> initialData)
		{
			D3D.Texture1DDescription desc = new D3D.Texture1DDescription();

			desc.ArraySize = Settings.ArrayCount;
			desc.Format = (SharpDX.DXGI.Format)Settings.Format;
			desc.Width = Settings.Width;
			desc.MipLevels = Settings.MipCount;
			
			if (Settings.Usage != BufferUsage.Staging)
				desc.BindFlags = D3D.BindFlags.ShaderResource;
			else
				desc.BindFlags = D3D.BindFlags.None;

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

			if ((initialData != null) && (initialData.Count() > 0))
			{
				DX.DataStream[] streams = GorgonTexture1DData.Convert(initialData);
				D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc, streams);

				// Clean up.
				foreach (var stream in streams)
					stream.Dispose();
			}
			else
				D3DTexture = new D3D.Texture1D(Graphics.D3DDevice, desc);
		}

		/// <summary>
		/// Function to save the texture data to a stream.
		/// </summary>
		/// <param name="stream">Stream to write.</param>
		/// <param name="format">Image format to use.</param>
		/// <remarks>A 1D dimensional texture can only be saved to DDS format.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the format is not DDS.</exception>
		public override void Save(System.IO.Stream stream, ImageFileFormat format)
		{
			if (format != ImageFileFormat.DDS)
				throw new ArgumentException("The file format for a 1D texture can only be DDS.", "format");

			D3D.Resource.ToStream<D3D.Texture1D>(Graphics.Context, (D3D.Texture1D)D3DTexture, D3D.ImageFileFormat.Dds, stream);
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
