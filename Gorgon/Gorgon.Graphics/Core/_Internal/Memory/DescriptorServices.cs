// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: July 6, 2025 3:49:07 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// The descriptor related services.
/// </summary>
/// <param name="GpuSamplerDescriptors">The sampler descriptors.</param>
/// <param name="GpuViewDescriptors">The cbv/srv/uav descriptors.</param>
/// <param name="RtvDescriptors">The render target view descriptors.</param>
/// <param name="DsvDescriptors">The depth/stencil view descriptors.</param>
internal sealed record class DescriptorServices(GpuDescriptorHeap GpuSamplerDescriptors, GpuDescriptorHeap GpuViewDescriptors, CpuDescriptorHeapPool RtvDescriptors, CpuDescriptorHeapPool DsvDescriptors)
    : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        GpuSamplerDescriptors.Dispose();
        GpuViewDescriptors.Dispose();
        RtvDescriptors.Dispose(); 
        DsvDescriptors.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to perform garbage collection on the descriptors.
    /// </summary>
    public void GarbageCollect()
    {
        RtvDescriptors.GarbageCollect();
        DsvDescriptors.GarbageCollect();
    }    
}
