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
// Created: October 5, 2026 8:32:27 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Diagnostics;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Base object to deliver common functionality and information for read/write buffer views.
/// </summary>
/// <remarks>
/// <para>
/// A read/write view allows shaders to read from, and write to, a buffer. The buffer must be created with read/write access (see <see cref="GorgonCommonBufferInfo.HasReadWriteAccess"/>), otherwise the 
/// view cannot be created.
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
/// </remarks>
public unsafe abstract class GorgonShaderBufferRwView
    : GorgonResourceView
{
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;
    private CpuDescriptorAllocation _cpuAllocation = CpuDescriptorAllocation.Null;

    /// <summary>
    /// Property to return the descriptor allocation for this view.
    /// </summary>
    private protected ref readonly GpuDescriptorAllocation Allocation => ref _allocation;

    /// <summary>
    /// Property to return the descriptor allocation for this view in the heap that is not visible to shaders.
    /// </summary>
    internal ref readonly CpuDescriptorAllocation CpuAllocation => ref _cpuAllocation;

    /// <summary>
    /// Property to return the buffer used by this view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is either a <see cref="GorgonGpuBuffer"/>, or a <see cref="GorgonIndexBuffer"/>.
    /// </para>
    /// </remarks>
    public GorgonGpuBufferCommon Buffer
    {
        get;
    }

    /// <inheritdoc cref="GorgonShaderBufferView.Size"/>
    public long Size
    {
        get;
    }

    /// <inheritdoc cref="GorgonShaderBufferView.Offset"/>
    public long Offset
    {
        get;
    }

    /// <inheritdoc cref="GorgonShaderBufferView.ElementCount"/>
    public int ElementCount
    {
        get;
    }

    /// <inheritdoc cref="GorgonShaderBufferView.StartElementIndex"/>
    public long StartElementIndex
    {
        get;
    }

    /// <inheritdoc cref="GorgonShaderBufferView.ElementSize"/>
    public int ElementSize
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the unordered access view description.
    /// </summary>
    /// <returns>The unordered access view description used to create the descriptor.</returns>
    private protected abstract D3D12_UNORDERED_ACCESS_VIEW_DESC GetDesc();

    /// <summary>
    /// Function to allocate a view descriptor from the descriptor heap.
    /// </summary>
    /// <param name="structuredCounterBuffer">[Optional] An extra buffer used for append/consume counters, or increment/decrement counts. Only applies to structured buffers.</param>
    private protected void AllocateDescriptors(GorgonGpuBuffer? structuredCounterBuffer = null)
    {
        D3D12_UNORDERED_ACCESS_VIEW_DESC desc = GetDesc();

        ID3D12Resource* counterBufPtr = structuredCounterBuffer is not null ? (PID3D12Resource2)structuredCounterBuffer.D3DResource.Get() : null;

        if ((desc.Buffer.StructureByteStride == 0) && (structuredCounterBuffer is not null))
        {
            Graphics.Log.PrintWarning($"Only structured buffers can use counter buffers. The buffer '{structuredCounterBuffer.Name}' will not be used with this view.", LoggingLevel.Intermediate);
            counterBufPtr = null;
        }

        ReadWriteViewDescriptors.Allocate(Graphics, (PID3D12Resource2)Buffer.D3DResource.Get(), counterBufPtr, desc, ref _cpuAllocation, ref _allocation);

        D3DCpuHandle = _cpuAllocation.CpuHandle;
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);

            Graphics.Log.Print($"Freeing descriptor handle allocations for '{Name}'.", LoggingLevel.Verbose);
            ReadWriteViewDescriptors.Free(Graphics, ref _cpuAllocation, ref _allocation);
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Function called when descriptors need to be reallocated for any reason.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some buffer types need extra functionality when allocating buffers (such as append/consume structured buffers), this allows those types to replace the default allocation call with the appropriate 
    /// call.
    /// </para>
    /// </remarks>
    private protected virtual void OnAllocateDescriptors() => AllocateDescriptors();

    /// <inheritdoc cref="GorgonConstantBufferView.GetViewHandle()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetViewHandle()
    {
        if (_allocation.Equals(in GpuDescriptorAllocation.Null))
        {
            OnAllocateDescriptors();
        }

        return _allocation.Offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderBufferRwView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of elements in the buffer to view.</param>
    /// <param name="elementSize">The size of the element.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonShaderBufferRwView(GorgonGraphics graphics, string name, GorgonGpuBufferCommon buffer, long startIndex, int elementCount, int elementSize, bool owned)
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
