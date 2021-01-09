#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 29, 2020 4:51:04 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The factory used to create Direct 3D buffers.
    /// </summary>
    internal static class BufferFactory
    {
        /// <summary>
        /// Function to create a Direct 3D buffer.
        /// </summary>
        /// <param name="device">The Direct 3D device used to create the buffer.</param>
        /// <param name="name">The name of the buffer.</param>
        /// <param name="desc">The description of the buffer.</param>
        /// <param name="initialData">A span containing the initial data for the buffer.</param>
        /// <returns>A new Direct 3D buffer.</returns>
        public static D3D11.Buffer Create<T>(D3D11.Device5 device, string name, in D3D11.BufferDescription desc, ReadOnlySpan<T> initialData)
            where T : unmanaged
        {
            D3D11.Buffer result = null;

            if ((initialData != null) && (initialData.Length > 0))
            {
                unsafe
                {
                    fixed (T* dataPtr = &initialData[0])
                    {
                        result = new D3D11.Buffer(device, new IntPtr(dataPtr), desc)
                        {
                            DebugName = $"{name}_ID3D11Buffer"
                        };
                    }
                }
            }
            else
            {
                result = new D3D11.Buffer(device, desc)
                {
                    DebugName = $"{name}_ID3D11Buffer"
                };
            }

            return result;
        }
    }
}
