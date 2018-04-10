#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 8, 2018 12:52:17 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A factory used to create SharpDX objects.
    /// </summary>
    class SharpDxObjectFactory
    {
        #region Variables.
        // The graphics interface that will register the new objects.
        private readonly GorgonGraphics _graphics;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to ensure that the settings passed to the swap chain are valid.
        /// </summary>
        /// <param name="info">Information about the swap chain.</param>
        private void ValidateSwapChainInfo(GorgonSwapChainInfo info)
        {
            if (!_graphics.FormatSupport.TryGetValue(info.Format, out IGorgonFormatSupportInfo formatInfo))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, info.Format));
            }

            if (!formatInfo.IsDisplayFormat)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, info.Format));
            }

            // Constrain sizes.
            info.Width = info.Width.Min(_graphics.VideoAdapter.MaxTextureWidth).Max(1);
            info.Height = info.Height.Min(_graphics.VideoAdapter.MaxTextureHeight).Max(1);

            if (info.DepthStencilFormat == BufferFormat.Unknown)
            {
                return;
            }

            if (!formatInfo.IsDepthBufferFormat)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DEPTHSTENCIL_FORMAT_INVALID, info.DepthStencilFormat));
            }
        }

        /// <summary>
        /// Function to create a D3D11 2D Texture from a swap chain back buffer.
        /// </summary>
        /// <param name="swapChain">The swap chain to evaluate.</param>
        /// <param name="index">The index of the back buffer.</param>
        /// <returns>A new D3D11 2D Texture.</returns>
        public D3D11.Texture2D CreateSwapChainTexture(DXGI.SwapChain4 swapChain, int index)
        {
            return D3D11.Resource.FromSwapChain<D3D11.Texture2D>(swapChain, index);
        }

        public D3D11.RenderTargetView1 CreateRenderTargetView(D3D11.Device5 device, D3D11.Texture2D texture, D3D11.RenderTargetViewDescription1? desc = null)
        {
            // TODO: Needs to use and convert from gorgon render target view information?
            return desc == null ? new D3D11.RenderTargetView1(device, texture) : new D3D11.RenderTargetView1(device, texture, desc.Value);
        }

        public DXGI.SwapChain4 CreateSwapChain(Control control, GorgonSwapChainInfo info)
        {
            ValidateSwapChainInfo(info);

            DXGI.SwapChainDescription1 desc = info.ToSwapChainDesc();

            DXGI.SwapChain4 result;
            using (DXGI.SwapChain1 dxgiSwapChain = new DXGI.SwapChain1(_graphics.DXGIFactory, _graphics.D3DDevice, control.Handle, ref desc)
                                                   {
                                                       DebugName = $"{info.Name}_DXGISwapChain4"
                                                   })
            {
                result = dxgiSwapChain.QueryInterface<DXGI.SwapChain4>();
            }

            _graphics.DXGIFactory.MakeWindowAssociation(control.Handle, DXGI.WindowAssociationFlags.IgnoreAll);

            return result;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="SharpDxObjectFactory"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this factory.</param>
        public SharpDxObjectFactory(GorgonGraphics graphics)
        {
            _graphics = graphics;
        }
        #endregion
    }
}
