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
// Created: Thursday, January 14, 2016 7:14:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using DXGI = SharpDX.DXGI;
using D3D12 = SharpDX.Direct3D12;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A 2D texture resource.
	/// </summary>
	public class GorgonTexture2D
		: IDisposable
	{
		#region Variables.
		// The resource holding the texture data.
		private D3D12.Resource _resource;
		// A handle for a render target view.
		private D3D12.CpuDescriptorHandle _rtvHandle;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D resource behind this texture.
		/// </summary>
		internal D3D12.Resource D3DResource => _resource;

		/// <summary>
		/// Property to return the render target view handle for the texture.
		/// </summary>
		internal D3D12.CpuDescriptorHandle RTVHandle => _rtvHandle;

		/// <summary>
		/// Property to return a <see cref="GorgonTexture2DInfo"/> used to describe this 2D texture.
		/// </summary>
		public GorgonTexture2DInfo Info
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_resource?.Dispose();
			_resource = null;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D" /> class.
		/// </summary>
		/// <param name="swapChain">The swap chain containing the texture information.</param>
		/// <param name="bufferIndex">The index of the swap chain buffer that we are retrieving.</param>
		internal GorgonTexture2D(GorgonSwapChain swapChain, int bufferIndex)
		{
			_resource = swapChain.DXGISwapChain.GetBackBuffer<D3D12.Resource>(bufferIndex);
			_resource.Name = string.Format($"{swapChain.Name} Backbuffer {bufferIndex}");
			Info = new GorgonTexture2DInfo
			{
				Format = (BufferFormat)_resource.Description.Format,
				Width = _resource.Description.Width,
				Height = _resource.Description.Height
			};

			_rtvHandle = swapChain.Graphics.RenderTargetViewAllocator.Allocate(1);
			swapChain.Graphics.D3DDevice.CreateRenderTargetView(_resource, null, _rtvHandle);
		}
		#endregion
	}
}
