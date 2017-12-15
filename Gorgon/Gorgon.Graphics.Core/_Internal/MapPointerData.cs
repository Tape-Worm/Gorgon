#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: December 15, 2017 12:17:00 PM
// 
#endregion

using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Native;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A special pointer type for mapped buffer data.
    /// </summary>
    internal unsafe class MapPointerData
        : GorgonPointerBase
    {
        #region Variables.
        // The graphics interface that is associated with the resource.
        private readonly GorgonGraphics _graphics;
        // The resource that is locked.
        private readonly D3D11.Resource _resource;
        // The mapping mode for locking.
        private readonly D3D11.MapMode _mapMode;
        // The sub resource index to use.
        private readonly int _subResource;
        // The offset within the buffer data to read form.
        private readonly int _offset;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to call when the <see cref="IGorgonPointer"/> needs to deallocate memory or release handles.
        /// </summary>
        protected override void Cleanup()
        {
            if (DataPointer == null)
            {
                return;
            }
            
            DataPointer = null;
            Size = 0;
            _graphics.D3DDeviceContext.UnmapSubresource(_resource, _subResource);
        }

        /// <summary>
        /// Function to map the data from the GPU into the CPU.
        /// </summary>
        public void Lock()
        {
            if (DataPointer != null)
            {
                return;
            }

            DX.DataBox box = _graphics.D3DDeviceContext.MapSubresource(_resource, _subResource, _mapMode, D3D11.MapFlags.None);
            DataPointer = (byte*)(box.DataPointer + _offset);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPointerData"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to map the data.</param>
        /// <param name="resource">The resource to map.</param>
        /// <param name="mapMode">The map mode.</param>
        /// <param name="subResource">The sub resource index to map.</param>
        /// <param name="offset">Offset, in bytes, within the buffer to start reading from.</param>
        /// <param name="size">The number of bytes to read.</param>
        public MapPointerData(GorgonGraphics graphics, D3D11.Resource resource, D3D11.MapMode mapMode, int subResource, int offset, int size)
        {
            DataPointer = null;
            _graphics = graphics;
            _resource = resource;
            _mapMode = mapMode;
            _subResource = subResource;
            _offset = offset;
            Size = size;
        }
        #endregion
    }
}
