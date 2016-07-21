#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 13, 2016 8:42:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A texture used to project an image onto a graphic primitive such as a triangle.
	/// </summary>
	public class GorgonTexture
		: GorgonResource
	{
		#region Variables.
		// The logging interface used for debug logging.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the information about the format of the texture.
		/// </summary>
		public GorgonFormatInfo FormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType 
		{
			get
			{
				switch (Info.TextureType)
				{
					case TextureType.Texture1D:
						return ResourceType.Texture1D;
					case TextureType.Texture2D:
						return ResourceType.Texture2D;
					case TextureType.Texture3D:
						return ResourceType.Texture3D;
					default:
						return ResourceType.Unknown;
				}
			}
		}

		/// <summary>
		/// Property to return the default shader view for this texture.
		/// </summary>
		public GorgonTextureShaderView DefaultView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the <see cref="GorgonTextureInfo"/> used to create this texture.
		/// </summary>
		public GorgonTextureInfo Info
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize a 2D image.
		/// </summary>
		/// <param name="image">The image data used to populate the texture.</param>
		private void Initialize2D(IGorgonImage image)
		{
			// TODO: Validate the settings.
			D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

			if (Info.Usage == D3D11.ResourceUsage.Staging)
			{
				cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
			}

			if (Info.Usage == D3D11.ResourceUsage.Dynamic)
			{
				cpuFlags = D3D11.CpuAccessFlags.Write;
			}

			var desc = new D3D11.Texture2DDescription
			           {
				           Format = Info.Format,
				           Width = Info.Width,
				           Height = Info.Height,
				           ArraySize = Info.ArrayCount,
				           Usage = Info.Usage,
				           BindFlags = (D3D11.BindFlags)Info.Binding,
				           CpuAccessFlags = cpuFlags,
				           OptionFlags = Info.IsCubeMap ? D3D11.ResourceOptionFlags.TextureCube : D3D11.ResourceOptionFlags.None,
						   SampleDescription = Info.MultiSampleInfo.ToSampleDesc(),
						   MipLevels = Info.MipLevels
			           };

			if (image == null)
			{
				D3DResource = new D3D11.Texture2D(Graphics.VideoDevice.D3DDevice, desc);
				DefaultView = new GorgonTextureShaderView(this, Info.Format, 0, Info.MipLevels, 0, Info.ArrayCount);
				return;
			}

			// Upload the data to the texture.
			DX.DataBox[] dataBoxes = new DX.DataBox[Info.ArrayCount * Info.MipLevels];

			int boxIndex = 0;
			for (int arrayIndex = 0; arrayIndex < Info.ArrayCount; ++arrayIndex)
			{
				for (int mipIndex = 0; mipIndex < Info.MipLevels; ++mipIndex)
				{
					IGorgonImageBuffer buffer = image.Buffers[mipIndex, arrayIndex];
					dataBoxes[boxIndex++] = new DX.DataBox(new IntPtr(buffer.Data.Address), buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
				}
			}

			D3DResource = new D3D11.Texture2D(Graphics.VideoDevice.D3DDevice, desc, dataBoxes);
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="image">The image used to initialize the texture.</param>
		private void Initialize(IGorgonImage image)
		{
			FormatInformation = new GorgonFormatInfo(Info.Format);

			switch (Info.TextureType)
			{
				case TextureType.Texture1D:
					break;
				case TextureType.Texture2D:
					Initialize2D(image);
					break;
				case TextureType.Texture3D:
					break;
			}

			DefaultView = new GorgonTextureShaderView(this);
		}
		
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			DefaultView?.Dispose();

			if (D3DResource == null)
			{
				return;
			}

			_log.Print($"{Info.TextureType} '{Name}': Destroying D3D11 Texture.", LoggingLevel.Simple);

			base.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain that holds the back buffers to retrieve.</param>
		/// <param name="index">The index of the back buffer to retrieve.</param>
		/// <param name="log">The log used for debug output.</param>
		/// <remarks>
		/// <para>
		/// This constructor is used internally to create a render target texture from a swap chain.
		/// </para>
		/// </remarks>
		internal GorgonTexture(GorgonSwapChain swapChain, int index, IGorgonLog log)
			: base(swapChain.Graphics, $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.")
		{
			_log = log;

			_log.Print($"Swap Chain '{swapChain.Name}': Creating texture from back buffer index {index}.", LoggingLevel.Simple);
			
			D3D11.Texture2D texture;
			
			// Get the resource from the swap chain.
			D3DResource = texture = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(swapChain.GISwapChain, index);
			D3DResource.DebugName = $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.";

			// Get the info from the back buffer texture.
			Info = new GorgonTextureInfo
			       {
				       Format = texture.Description.Format,
				       Width = texture.Description.Width,
				       Height = texture.Description.Height,
				       TextureType = TextureType.Texture2D,
				       Usage = texture.Description.Usage,
				       ArrayCount = texture.Description.ArraySize,
				       MipLevels = texture.Description.MipLevels,
				       Depth = 0,
				       IsCubeMap = false,
				       MultiSampleInfo = GorgonMultiSampleInfo.NoMultiSampling,
				       Binding = (TextureBinding)texture.Description.BindFlags
			       };

			FormatInformation = new GorgonFormatInfo(Info.Format);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="name">The name of the texture.</param>
		/// <param name="graphics">The graphics interface used to create this texture.</param>
		/// <param name="image">The image to copy into the texture.</param>
		/// <param name="info">The information used to define how the texture should be created.</param>
		/// <param name="log">The log interface used for debugging.</param>
		/// <remarks>
		/// <para>
		/// This constructor is used when converting an image to a texture.
		/// </para>
		/// </remarks>
		internal GorgonTexture(string name, GorgonGraphics graphics, IGorgonImage image, GorgonImageToTextureInfo info, IGorgonLog log)
			: base(graphics, name)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;

			TextureType type;

			switch (image.Info.ImageType)
			{
				case ImageType.Image1D:
					type = TextureType.Texture1D;
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					type = TextureType.Texture2D;
					break;
				case ImageType.Image3D:
					type = TextureType.Texture3D;
					break;
				default:
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_IMAGE_TYPE_UNSUPPORTED, image.Info.ImageType), nameof(image));
			}

			Info = new GorgonTextureInfo
			       {
				       Format = image.Info.Format,
				       Width = image.Info.Width,
				       Height = image.Info.Height,
				       TextureType = type,
				       Usage = info.Usage,
				       ArrayCount = image.Info.ArrayCount,
				       Binding = info.Binding,
				       Depth = image.Info.Depth,
				       IsCubeMap = image.Info.ImageType == ImageType.ImageCube,
				       MipLevels = image.Info.MipCount,
				       MultiSampleInfo = info.MultiSampleInfo
			       };

			Initialize(image);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="name">The name of the texture.</param>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> interface that created this texture.</param>
		/// <param name="textureInfo">A <see cref="GorgonTextureInfo"/> object describing the properties of this texture.</param>
		/// <param name="log">[Optional] The logging interface used for debugging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="graphics"/>, or the <paramref name="textureInfo"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="GorgonTextureInfo.Usage"/> is set to <c>Immutable</c>.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture could not be created due to misconfiguration.</exception>
		/// <remarks>
		/// <para>
		/// This constructor creates an empty texture. Data may be uploaded to the texture at a later time if its <see cref="GorgonTextureInfo.Usage"/> is not set to <c>Immutable</c>. If the 
		/// <see cref="GorgonTextureInfo.Usage"/> is set to <c>immutable</c> with this constructor, then an exception will be thrown. To use an immutable texture, use the 
		/// <see cref="GorgonImageTextureExtensions.ToTexture"/> extension method on the <see cref="IGorgonImage"/> type.
		/// </para>
		/// </remarks>
		public GorgonTexture(string name, GorgonGraphics graphics, GorgonTextureInfo textureInfo, IGorgonLog log = null)
			: base(graphics, name)
		{
			if (textureInfo == null)
			{
				throw new ArgumentNullException(nameof(textureInfo));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			
			Info = new GorgonTextureInfo
			       {
				       Format = textureInfo.Format,
				       Width = textureInfo.Width,
				       Height = textureInfo.Height,
				       TextureType = textureInfo.TextureType,
				       Usage = textureInfo.Usage,
				       ArrayCount = textureInfo.ArrayCount,
				       Binding = textureInfo.Binding,
				       Depth = textureInfo.Depth,
				       IsCubeMap = textureInfo.IsCubeMap,
				       MipLevels = textureInfo.MipLevels,
				       MultiSampleInfo = textureInfo.MultiSampleInfo
			       };

			Initialize(null);
		}
		#endregion
	}
}
