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
// Created: Sunday, March 11, 2012 9:35:15 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A 2D texture shim for SM2 cards.
	/// </summary>
	/// <remarks>As per the 10level9 documentation:
	/// A resource that has the shader binding flag set (which we do for our textures) and no other flags cannot use CopyResource to copy data into CPU accessible memory.  Therefore, the D3DX functions will fail every time the 
	/// user tries to save a texture (the CPU needs to be able to read the data from a staging texture, which is only in CPU memory).</remarks>
	class GorgonTexture2DSM2
		: GorgonTexture2D
	{
		#region Methods.
		/// <summary>
		/// Function to destroy a shim texture.
		/// </summary>
		/// <param name="shim">Shim texture.</param>
		private void DestroyShim(D3D.Texture2D shim)
		{
			if (shim == null)
				return;

			if (shim != D3DTexture)
				shim.Dispose();
		}

		/// <summary>
		/// Function to create a shim resource.
		/// </summary>
		/// <param name="texture">Texture to create a shim for.</param>
		/// <returns>A new shim resource.</returns>
		private D3D.Texture2D CreateShim(D3D.Texture2D texture)
		{
			D3D.Texture2DDescription desc = texture.Description;
			D3D.Texture2D shim = null;

			if (desc.BindFlags != D3D.BindFlags.ShaderResource)
				return null;

			if (!Graphics.VideoDevice.SupportsRenderTargetFormat(Settings.Format, Settings.Multisampling.Count > 1 || Settings.Multisampling.Quality > 0))
				throw new GorgonException(GorgonResult.CannotWrite, "Cannot save this texture.  There are restrictions in place for SM2_a_b video devices that limit the formats that can be saved.  The format '" + Settings.Format.ToString() + "' cannot be persisted to a stream or file.");
									
			desc.BindFlags = D3D.BindFlags.RenderTarget | D3D.BindFlags.ShaderResource;
			desc.Usage = D3D.ResourceUsage.Default;
			desc.CpuAccessFlags = D3D.CpuAccessFlags.None;

			try
			{
				shim = new D3D.Texture2D(Graphics.D3DDevice, desc);
				shim.DebugName = "HACK: Fix for broken D3DX. Texture: \"" + Name + "\"";

				// Copy the texture to the temporary texture.
				Graphics.Context.CopyResource(D3DTexture, shim);

				return shim;
			}
			catch
			{
				if (shim != null)
					shim.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Function to copy a resource in its entirety.
		/// </summary>
		/// <param name="source">Resource to copy.</param>
		/// <param name="destination">Destination resource.</param>
		internal override void CopyResourceProxy(D3D.Texture2D source, D3D.Texture2D destination)
		{
			D3D.Texture2D shim = null;

			try
			{
				if (this.Settings.Usage == BufferUsage.Staging)
					shim = CreateShim(source);
				base.CopyResourceProxy(shim != null ? shim : source, destination);
			}
			finally
			{
				DestroyShim(shim);
			}
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
		internal override void CopySubresourceProxy(D3D.Texture2D source, D3D.Texture2D destination, int srcSubresourceIndex, int destSubresourceIndex, D3D.ResourceRegion? sourceRegion, SlimMath.Vector3 destinationPosition)
		{
			D3D.Texture2D shim = null;

			try
			{
				if (this.Settings.Usage == BufferUsage.Staging)
					shim = CreateShim(source);
				base.CopySubresourceProxy(shim != null ? shim : source, destination, srcSubresourceIndex, destSubresourceIndex, sourceRegion, destinationPosition);
			}
			finally
			{
				DestroyShim(shim);
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
			D3D.Texture2D shim = null;

			if (IsDepthStencil)
				throw new GorgonException(GorgonResult.CannotWrite, "Cannot save a depth/stencil buffer texture.");

			// We can only save to 32 bit RGBA uint normalized formats if we're not using DDS, so we have to convert.
			if ((format != ImageFileFormat.DDS) && (Settings.Format != BufferFormat.R8G8B8A8_UIntNormal) && (Settings.Format != BufferFormat.R8G8B8A8_UIntNormal_sRGB))
				throw new ArgumentException("Cannot save the format '" + Settings.Format.ToString() + "' to a " + format.ToString() + " file.  DDS is the only file format that may be used with that texture format.");

			try
			{
				shim = CreateShim(D3DTexture);
				D3D.Texture2D.ToStream<D3D.Texture2D>(Graphics.Context, shim != null ? shim : D3DTexture, fileFormat, stream);
			}
			finally
			{
				DestroyShim(shim);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DSM2"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">The settings for the texture.</param>
		internal GorgonTexture2DSM2(GorgonGraphics graphics, string name, GorgonTexture2DSettings settings)
			: base(graphics, name, settings)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DSM2"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain to get texture information from.</param>
		internal GorgonTexture2DSM2(GorgonSwapChain swapChain)
			: base(swapChain)
		{
		}
		#endregion
	}
}
