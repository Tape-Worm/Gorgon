// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: March 22, 2026 3:02:19 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Diagnostics;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Base object for shader resource based views.
/// </summary>
public unsafe abstract class GorgonShaderBufferView
    : GorgonResourceView
{
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;

    /// <summary>
    /// Property to return the descriptor allocation for this view.
    /// </summary>
    private protected ref readonly GpuDescriptorAllocation Allocation => ref _allocation;

    /// <summary>
    /// Property to return the buffer used by this view.
    /// </summary>
    public GorgonGpuBuffer Buffer
    {
        get;
    }

    /// <summary>
    /// Property to return the size, in bytes, of the buffer to view.
    /// </summary>
    public long Size
    {
        get;
    }

    /// <summary>
    /// Property to return the offset, in bytes, within the buffer that the view starts at.
    /// </summary>
    public long Offset
    {
        get;
    }

    /// <summary>
    /// Property to return the number of elements in the buffer view.
    /// </summary>
    public int ElementCount
    {
        get;
    }

    /// <summary>
    /// Property to return the first index within the buffer to start the view at.
    /// </summary>
    public long StartElementIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the size, in bytes, of a single element in the view.
    /// </summary>
    public int ElementSize
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the shader resource view description.
    /// </summary>
    /// <returns>The shader resource description used to create the descriptor.</returns>
    private protected abstract D3D12_SHADER_RESOURCE_VIEW_DESC GetDesc();

    /// <summary>
    /// Function to allocate a view descriptor from the descriptor heap.
    /// </summary>
    private protected void AllocateDescriptors()
    {
        if (!_allocation.Equals(GpuDescriptorAllocation.Null))
        {
            Graphics.GpuViewDescriptors.Free(ref _allocation);
        }

        D3D12_SHADER_RESOURCE_VIEW_DESC desc = GetDesc();

        Graphics.GpuViewDescriptors.Allocate(1, out _allocation);        

        D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = Graphics.GpuViewDescriptors.D3DCpuHandle;
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.GpuViewDescriptors.D3DGpuHandle;

        cpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);
        gpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);

        Graphics.D3DDevice.Get()->CreateShaderResourceView((PID3D12Resource2)Buffer.D3DResource.Get(), &desc, cpuHandle);

        SetHandles(cpuHandle, gpuHandle);
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_allocation.Equals(GpuDescriptorAllocation.Null))
            {
                Graphics.Log.Print($"Freeing descriptor handle allocation for '{Name}'.", LoggingLevel.Verbose);
                Graphics.GpuViewDescriptors.Free(ref _allocation);
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected override void OnReset() => AllocateDescriptors();

    /// <summary>
    /// Function to retrieve the handle of the view, which is used to pass to a shader for resource heap indexing.
    /// </summary>
    /// <returns>The handle of the view.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetViewHandle()
    {
        if (_allocation.Equals(in GpuDescriptorAllocation.Null))
        {
            AllocateDescriptors();
        }

        return _allocation.Offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderBufferView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of elements in the buffer to view.</param>
    /// <param name="elementSize">The size of the element.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonShaderBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, long startIndex, int elementCount, int elementSize , bool owned)
        : base(graphics, name, buffer, owned)
    {
        Buffer = buffer;
        StartElementIndex = startIndex;
        ElementCount = elementCount;
        ElementSize = elementSize;
        Offset = startIndex * elementSize;
        Size = elementCount * elementSize;
    }
}
