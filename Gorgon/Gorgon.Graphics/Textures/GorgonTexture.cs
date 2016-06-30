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
		/// Property to return the size, in bytes, of the resource.
		/// </summary>
		public override int SizeInBytes
		{
			get
			{
				// Temporary.
				return 0;
			}
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
		/// Function to initialize the texture.
		/// </summary>
		private void Initialize()
		{
			// TODO: Validation for parameters.
		}

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			// Not used.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
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
		internal GorgonTexture(GorgonSwapChain swapChain, int index, IGorgonLog log)
			: base(swapChain.VideoDevice, $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.")
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
				       Usage = (BufferUsage)texture.Description.Usage,
				       ArrayCount = texture.Description.ArraySize,
				       MipLevels = texture.Description.MipLevels,
				       Depth = 0,
				       IsCubeMap = false,
				       MultiSampleInfo = GorgonMultiSampleInfo.NoMultiSampling,
				       Binding = (TextureBinding)texture.Description.BindFlags
			       };
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="videoDevice">The graphics.</param>
		/// <param name="textureInfo">The texture information.</param>
		/// <param name="log">The log.</param>
		public GorgonTexture(string name, IGorgonVideoDevice videoDevice, GorgonTextureInfo textureInfo, IGorgonLog log = null)
			: base(videoDevice, name)
		{
			if (videoDevice == null)
			{
				throw new ArgumentNullException(nameof(videoDevice));
			}

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

			Initialize();
		}
		#endregion
	}
}
