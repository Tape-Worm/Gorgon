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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 6, 2026 9:00:00 AM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functions to create and release the descriptors for read/write (unordered access) views.
/// </summary>
/// <remarks>
/// <para>
/// Read/write views need two descriptors. The view is created in a heap that is not visible to shaders because <c>ClearUnorderedAccessView*</c> requires a CPU handle from such a heap. That 
/// descriptor is then copied into the shader visible heap so shaders can access the view.
/// </para>
/// </remarks>
internal static unsafe class ReadWriteViewDescriptors
{
    /// <summary>
    /// Function to create the descriptors for a read/write view.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns the descriptor heaps.</param>
    /// <param name="resource">The resource to view.</param>
    /// <param name="counterResource">The resource holding the counter for the view, or <b>null</b> if there is no counter.</param>
    /// <param name="desc">The description of the view.</param>
    /// <param name="cpuAllocation">The allocation in the heap that is not visible to shaders. Any existing allocation is released first.</param>
    /// <param name="gpuAllocation">The allocation in the shader visible heap. Any existing allocation is released first.</param>
    public static void Allocate(GorgonGraphics graphics, ID3D12Resource* resource, ID3D12Resource* counterResource, D3D12_UNORDERED_ACCESS_VIEW_DESC desc, 
                                ref CpuDescriptorAllocation cpuAllocation, ref GpuDescriptorAllocation gpuAllocation)
    {
        Free(graphics, ref cpuAllocation, ref gpuAllocation);

        GpuDescriptorHeap gpuHeap = graphics.Descriptors.GpuViewDescriptors;

        graphics.Descriptors.CbvSrvUavDescriptors.Allocate(1, out cpuAllocation);
        graphics.D3DDevice.Get()->CreateUnorderedAccessView(resource, counterResource, &desc, cpuAllocation.CpuHandle);

        gpuHeap.Allocate(1, out gpuAllocation);

        D3D12_CPU_DESCRIPTOR_HANDLE gpuHeapCpuHandle = gpuHeap.D3DCpuHandle;
        gpuHeapCpuHandle.Offset(gpuAllocation.Offset, gpuHeap.DescriptorSize);

        graphics.D3DDevice.Get()->CopyDescriptorsSimple(1, gpuHeapCpuHandle, cpuAllocation.CpuHandle, D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
    }

    /// <summary>
    /// Function to release the descriptors for a read/write view.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns the descriptor heaps.</param>
    /// <param name="cpuAllocation">The allocation in the heap that is not visible to shaders.</param>
    /// <param name="gpuAllocation">The allocation in the shader visible heap.</param>
    public static void Free(GorgonGraphics graphics, ref CpuDescriptorAllocation cpuAllocation, ref GpuDescriptorAllocation gpuAllocation)
    {
        if (!gpuAllocation.Equals(GpuDescriptorAllocation.Null))
        {
            graphics.Descriptors.GpuViewDescriptors.Free(ref gpuAllocation);
        }

        if (!cpuAllocation.Equals(CpuDescriptorAllocation.Null))
        {
            cpuAllocation.Heap?.Free(ref cpuAllocation);
        }
    }
}
