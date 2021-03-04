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
using Gorgon.Graphics.Imaging;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The factory used to create Direct 3D resources.
    /// </summary>
    internal static class ResourceFactory
    {
        /// <summary>
        /// Function to create a Direct 3D Query.
        /// </summary>
        /// <param name="device">The Direct 3D device used to create the query.</param>
        /// <param name="name">The name of the query.</param>
        /// <param name="desc">The description of the query.</param>
        /// <returns>A new Direct 3D query.</returns>
        public static D3D11.Query CreateQuery(D3D11.Device5 device, string name, in D3D11.QueryDescription desc) => new D3D11.Query(device, desc)
        {
            DebugName = name
        };

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

        /// <summary>
        /// Function to create a 3D texture resource.
        /// </summary>
        /// <param name="device">The device used to create the resource object.</param>
        /// <param name="name">The name of the texture.</param>
        /// <param name="id">The ID for the texture.</param>
        /// <param name="desc">The descriptor for the descriptor.</param>
        /// <param name="image">Image data used to initialize the texture.</param>
        /// <returns>A new texture.</returns>
        public static D3D11.Texture3D1 Create(D3D11.Device5 device, string name, long id, in D3D11.Texture3DDescription1 desc, IGorgonImage image)
        {
            if (image is null)
            {
                return new D3D11.Texture3D1(device, desc)
                {
                    DebugName = $"{name}[{id}]_ID3D11Texture3D1"
                };
            }

            // Upload the data to the texture.
            var dataBoxes = new DX.DataBox[GorgonImage.CalculateDepthSliceCount(desc.Depth, desc.MipLevels)];
            int depthLevel = desc.Depth;
            int dataBoxIndex = 0;

            for (int mipIndex = 0; mipIndex < desc.MipLevels; ++mipIndex)
            {   
                for (int depthSlice = 0; depthSlice < depthLevel; ++depthSlice)
                {
                    IGorgonImageBuffer buffer = image.Buffers[mipIndex, depthSlice];
                    dataBoxes[dataBoxIndex++] = new DX.DataBox(buffer.Data, buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);                    
                }

                depthLevel >>= 1;

                if (depthLevel < 1)
                {
                    depthLevel = 1;
                }
            }

            return new D3D11.Texture3D1(device, desc, dataBoxes)
            {
                DebugName = $"{name}[{id}]_ID3D11Texture3D1"
            };
        }

        /// <summary>
        /// Function to create a 2D texture resource.
        /// </summary>
        /// <param name="device">The device used to create the resource object.</param>
        /// <param name="name">The name of the texture.</param>
        /// <param name="id">The ID for the texture.</param>
        /// <param name="desc">The descriptor for the descriptor.</param>
        /// <param name="image">Image data used to initialize the texture.</param>
        /// <returns>A new texture.</returns>
        public static D3D11.Texture2D1 Create(D3D11.Device5 device, string name, long id, in D3D11.Texture2DDescription1 desc, IGorgonImage image)
        {
            if (image is null)
            {
                return new D3D11.Texture2D1(device, desc)
                {
                    DebugName = $"{name}[{id}]_ID3D11Texture2D1"
                };
            }

            // Upload the data to the texture.
            var dataBoxes = new DX.DataBox[GorgonImage.CalculateDepthSliceCount(1, desc.MipLevels) * desc.ArraySize];

            for (int arrayIndex = 0; arrayIndex < desc.ArraySize; ++arrayIndex)
            {
                for (int mipIndex = 0; mipIndex < desc.MipLevels; ++mipIndex)
                {
                    int boxIndex = mipIndex + (arrayIndex * desc.MipLevels);
                    IGorgonImageBuffer buffer = image.Buffers[mipIndex, arrayIndex];
                    dataBoxes[boxIndex] = new DX.DataBox(buffer.Data, buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
                }
            }

            return new D3D11.Texture2D1(device, desc, dataBoxes)
            {
                DebugName = $"{name}[{id}]_ID3D11Texture2D1"
            };
        }

        /// <summary>
        /// Function to create a 1D texture resource.
        /// </summary>
        /// <param name="device">The device used to create the resource object.</param>
        /// <param name="name">The name of the texture.</param>
        /// <param name="id">The ID for the texture.</param>
        /// <param name="desc">The descriptor for the descriptor.</param>
        /// <param name="image">Image data used to initialize the texture.</param>
        /// <returns>A new texture.</returns>
        public static D3D11.Texture1D Create(D3D11.Device5 device, string name, long id, in D3D11.Texture1DDescription desc, IGorgonImage image)
        {
            if (image is null)
            {
                return new D3D11.Texture1D(device, desc)
                {
                    DebugName = $"{name}[{id}]_ID3D11Texture1D"
                };
            }

            // Upload the data to the texture.
            var dataBoxes = new DX.DataBox[GorgonImage.CalculateDepthSliceCount(1, desc.MipLevels) * desc.ArraySize];

            for (int arrayIndex = 0; arrayIndex < desc.ArraySize; ++arrayIndex)
            {
                for (int mipIndex = 0; mipIndex < desc.MipLevels; ++mipIndex)
                {
                    int boxIndex = mipIndex + (arrayIndex * desc.MipLevels);
                    IGorgonImageBuffer buffer = image.Buffers[mipIndex, arrayIndex];
                    dataBoxes[boxIndex] = new DX.DataBox(buffer.Data, buffer.PitchInformation.RowPitch, buffer.PitchInformation.RowPitch);
                }
            }

            return new D3D11.Texture1D(device, desc, dataBoxes)
            {
                DebugName = $"{name}[{id}]_ID3D11Texture1D"
            };
        }
    }
}
