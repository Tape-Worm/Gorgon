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
// Created: October 20, 2025 5:57:46 PM
//

using System.Buffers;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides an interface to send commands to the GPU.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something...
/// </para>
/// </remarks>
public unsafe sealed class GorgonCommandList
    : IGorgonNamedObject, IDisposable, IGorgonCopyMethodsFluent<GorgonCommandList>
{
    private ComPtr<ID3D12GraphicsCommandList10> _list;
    private ComPtr<ID3D12CommandList> _baseList;

    private CommandAllocator? _commandAllocator;
    private string _name;
    private readonly GorgonResourceCopier _resourceCopier;
    private readonly IGorgonResourceWriter _resourceWriter;
    private readonly BarrierManager _barrierManager;

    private readonly CpuBufferAllocation [] _constantWriteData = new CpuBufferAllocation[GorgonGraphics.MaxRootConstantCount];
    private GorgonIndexBuffer? _indexBuffer;
    private bool _indexBufferChanged;
    private readonly List<GorgonGpuBuffer> _dynamicBuffers = new(32);
    private readonly List<(GorgonGpuBuffer Buffer, BarrierSync Sync, BarrierAccess Access)> _usedBuffers = new(32);
    
    /// <summary>
    /// Property to set or return the allocator associated with the command list.
    /// </summary>
    internal CommandAllocator? Allocator
    {
        get => _commandAllocator;
        private set
        {
            _commandAllocator?.HasCommandList = false;
            _commandAllocator = value;
            _commandAllocator?.HasCommandList = true;            
        }
    }

    /// <summary>
    /// Property to return the D3D command list. 
    /// </summary>
    internal ref readonly ComPtr<ID3D12GraphicsCommandList10> D3DGraphicsCommandList => ref _list;

    /// <summary>
    /// Property to return the D3D command list. 
    /// </summary>
    internal ref readonly ComPtr<ID3D12CommandList> D3DCommandList => ref _baseList;

    /// <summary>
    /// Property to return the queue assigned to the command list.
    /// </summary>
    internal CommandQueue Queue
    {
        get;
    }

    /// <summary>
    /// Property to return the list of swap chains to use as presenters.
    /// </summary>
    internal List<(GorgonSwapChain SwapChain, int PresentInterval)> Presenters
    {
        get;
    } = [];

    /// <summary>
    /// Property to return the graphics interface associated with this command list.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;        
    }

    /// <inheritdoc/>
    public string Name
    {
        get => _name;
        private set
        {
            _name = GorgonGraphicsFactory.GenerateName(value, nameof(GorgonCommandList));

            if (!D3DGraphicsCommandList.IsNull)
            {
                D3DGraphicsCommandList.SetD3DDebugName($"D3D12 {Queue.Type} '{Name}'");
            }
        }
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _barrierManager.Clear();
            _resourceCopier.Dispose();

            Graphics.Log.Print($"Destroying {nameof(GorgonCommandList)} '{Name}'...", LoggingLevel.Intermediate);
            Graphics.Log.Print($"Destroying D3D 12 {Queue.Type} '{Name}'", LoggingLevel.Verbose);
        }
        _baseList.Dispose();
        _list.Dispose();
    }

    /// <summary>
    /// Function to create the native D3D 12 backing command list object.
    /// </summary>
    /// <returns>The COM pointer to the native command list object.</returns>
    private ComPtr<ID3D12GraphicsCommandList10> CreateNative()
    {
        ComPtr<ID3D12GraphicsCommandList10> result = default;

        Graphics.Log.Print($"Creating {nameof(GorgonCommandList)} '{Name}'...", LoggingLevel.Intermediate);

        Graphics.D3DDevice.Get()->CreateCommandList1(0, Queue.Type, D3D12_COMMAND_LIST_FLAGS.D3D12_COMMAND_LIST_FLAG_NONE, Win32.__uuidof<ID3D12GraphicsCommandList10>(), (void**)result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_COMMAND_LIST);

        result.SetD3DDebugName($"D3D12 {Queue.Type} '{Name}'");

        return result;
    }

    /// <summary>
    /// Function to assign an index buffer to the command list.
    /// </summary>
    private void ApplyIndexBuffer()
    {
        if (!_indexBufferChanged)
        {
            return;
        }

        if (_indexBuffer is null)
        {
            _list.Get()->IASetIndexBuffer(null);
            return;
        }

        D3D12_INDEX_BUFFER_VIEW view = new()
        {
            BufferLocation = _indexBuffer.D3DResource.Get()->GetGPUVirtualAddress(),
            Format = _indexBuffer.Use32BitIndices ? DXGI_FORMAT.DXGI_FORMAT_R32_UINT : DXGI_FORMAT.DXGI_FORMAT_R16_UINT,
            SizeInBytes = (uint)_indexBuffer.SizeInBytes
        };

        _list.Get()->IASetIndexBuffer(&view);
    }

    /// <summary>
    /// Function to apply constant value writes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyConstantWrites()
    {
        for (uint i = 0; i < _constantWriteData.Length; ++i)
        {
            ref CpuBufferAllocation allocation = ref _constantWriteData[i];

            _list.Get()->SetGraphicsRootConstantBufferView(i, allocation.GpuAddress);
        }
    }

    /// <summary>
    /// Function to upload dynamic resource data to the mega buffer.
    /// </summary>
    private void UploadDynamicBuffersToMegaBuffer()
    {
        GorgonGpuBuffer[] buffers = ArrayPool<GorgonGpuBuffer>.Shared.Rent(_dynamicBuffers.Count);

        try
        {
            int count = 0;

            for (int i = 0; i < _dynamicBuffers.Count; ++i)
            {
                GorgonGpuBuffer buffer = _dynamicBuffers[i];

                if (!buffer.NeedsDataUpload)
                {
                    continue;
                }

                SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination);
                buffers[count++] = buffer;
            }

            if (count == 0)
            {
                return;
            }

            _barrierManager.Submit(this);

            for (int i = 0; i < count; ++i)
            {
                buffers[i].FlushDynamicBuffer(this, false);
            }
        }
        finally
        {
            ArrayPool<GorgonGpuBuffer>.Shared.Return(buffers);
        }
    }

    /// <summary>
    /// Function to prepare buffers for use by the system by establishing barriers.
    /// </summary>
    private void PrepareBufferBarriers()
    {
        // We need to validate that the buffers that are reading do not overlap their data regions with buffers that are writing.
        // Since we are using the MegaBuffer approach, this is a potential hazard. But, we also need to ensure that we're not trying to 
        // make a single non-UAV buffer (user facing, not mega buffer) with read access and write access simultaneously (e.g. SRV | COPY_DEST).
        int Intersects(GorgonGpuBuffer buffer, int bufferIndex, BarrierSync sync, BarrierAccess access)
        {
            ulong bufferStart = buffer.ResourceOffset;
            ulong bufferEnd = (ulong)buffer.SizeInBytes + buffer.ResourceOffset;

            for (int i = 0; i < _usedBuffers.Count; ++i)
            {
                (GorgonGpuBuffer otherBuffer, BarrierSync otherSync, BarrierAccess otherAccess) = _usedBuffers[i];

                if (bufferIndex == i)
                {
                    continue;
                }
                
                ulong otherEnd = (ulong)otherBuffer.SizeInBytes +  otherBuffer.ResourceOffset;

                if ((otherBuffer.ResourceOffset >= bufferStart) && (otherBuffer.ResourceOffset <= bufferEnd)
                    || (otherEnd >= bufferStart) && (otherEnd <= bufferEnd))
                {
                    // If they aren't changing the barrier, then we don't care.
                    if ((otherSync != sync) || (otherAccess != access))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        BarrierSync megaSync = BarrierSync.None;
        BarrierAccess megaAccess = BarrierAccess.Common;

        for (int i = 0; i < _usedBuffers.Count; ++i)
        {
            (GorgonGpuBuffer buffer, BarrierSync sync, BarrierAccess access) = _usedBuffers[i];

            if (buffer.D3DResource.Get() != Graphics.MegaBuffer.D3DBuffer.Get())
            {
                // If this buffer is not from the mega buffer, then just set its barrier as-is.
                _barrierManager.AddBarrier(buffer, sync, access);
                continue;
            }

            // Multiple buffers cannot be used with different states in the same memory range.
            // We'll check here to see if this buffer intersects any other buffers, and if it 
            // does, we'll throw an exception.
            if (Graphics.IsInDebugMode)
            {
                int collisionIndex = Intersects(buffer, i, sync, access);

                if (collisionIndex != -1)
                {
                    throw new GorgonException(GorgonResult.CannotExecute, string.Format(Resources.GORGFX_ERR_BUFFERS_AND_BARRIER_OVERLAP, buffer, sync, access, 
                                                                                        _usedBuffers[collisionIndex].Buffer, 
                                                                                        _usedBuffers[collisionIndex].Sync, 
                                                                                        _usedBuffers[collisionIndex].Access));
                }                
            }

            // Otherwise, combine the access/sync patterns to we can use the mega buffer resource 
            // for whatever we have in mind.
            megaSync |= sync;
            megaAccess |= access;

            _barrierManager.AddBarrier(buffer, megaSync, megaAccess);
        }
    }

    /// <summary>
    /// Function to prepare the index buffer for use by the system by establishing its barriers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PrepareIndexBuffer()
    {
        if ((_indexBuffer is null) || (!_indexBufferChanged))
        {
            return;
        }

        if ((_indexBuffer.Usage == BufferUsage.DynamicPerFrame) && (_indexBuffer.NeedsDataUpload))
        {
            _indexBuffer.FlushDynamicBuffer(this, true);
        }

        SetBarrier(_indexBuffer, BarrierSync.IndexInput, BarrierAccess.IndexBuffer);
    }

    /// <summary>
    /// Function to apply the individual resources prior to a command.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyResources()
    {
        if (_dynamicBuffers.Count > 0)
        {
            UploadDynamicBuffersToMegaBuffer();
        }        

        PrepareBufferBarriers();
        PrepareIndexBuffer();

        _barrierManager.Submit(this);

        ApplyIndexBuffer();

        ApplyConstantWrites();

        _indexBufferChanged = false;
    }

    /// <summary>
    /// Function to present the swap chains added to the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Present()
    {
        for (int i = 0; i < Presenters.Count; ++i)
        {
            (GorgonSwapChain swapChain, int interval) = Presenters[i];
            swapChain.Present(interval);
        }
    }

    /// <summary>
    /// Function to begin recordinf of the command list.
    /// </summary>
    /// <param name="currentFrame">The current frame for our in-flight frame values.</param>
    internal void BeginRecording(int currentFrame)
    {
        Graphics.MegaBuffer.Signal();
        Graphics.GraphicsQueue.AllocatorPool.Signal();

        Graphics.UploadHeaps.Signal();
        Graphics.DownloadHeaps.Signal();
        Graphics.GpuSamplerDescriptors.Signal();
        Graphics.GpuViewDescriptors.Signal();

        // Signal any resources that are awaiting destruction.
        Queue.Tracker.Signal();

        // Wait for the next frame to become available.
        Queue.WaitForFence(Queue.FrameFenceValue[currentFrame], Timeout.Infinite);

        // Any previous barriers on this command list should be voided.
        _barrierManager.Clear();

        AllocateGpuDescriptorHeaps();        
    }

    /// <summary>
    /// Function to close the list for recording.
    /// </summary>
    internal void Close()
    {
        if (Presenters.Count != 0)
        {
            // Transition any presenters back to the common state before moving on.
            for (int i = 0; i < Presenters.Count; ++i)
            {
                SetBarrier(Presenters[i].SwapChain.Target.Texture, BarrierSync.None, BarrierAccess.None, BarrierLayout.Common, force: i == Presenters.Count - 1);
            }
        }
        else
        {
            _barrierManager.Submit(this);
        }

        _list.Get()->Close()
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_CANNOT_CLOSE_COMMAND_LIST, Name));

        _barrierManager.CopyCurrentToGlobal();
        Array.Clear(_constantWriteData);
        _dynamicBuffers.Clear();
        _usedBuffers.Clear();
    }

    /// <summary>
    /// Function called when a list is pulled from the pool.
    /// </summary>
    /// <param name="newName">The new name for the list.</param>
    /// <param name="allocator">The allocator used by the list.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResetState(string newName, CommandAllocator? allocator)
    {
        Name = newName;
        Allocator = allocator;
        Presenters.Clear();
        _barrierManager.Clear();
    }

    /// <summary>
    /// TBD
    /// </summary>
    public void Draw(int indexCount, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
    {
        // NOTE TO ME: DO NOT return the fluent interface.
        _list.Get()->SetGraphicsRootSignature(Graphics.RootSig.Get());
        _list.Get()->SetPipelineState(Graphics.Pso.Get());

        ApplyResources();

        DoViewSetup();
        SetupDescriptors();

        // This needs to come from the PSO.
        _list.Get()->IASetPrimitiveTopology((D3D_PRIMITIVE_TOPOLOGY)PrimitiveType.TriangleList);

        if (Presenters.Count > 0)
        {
            D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = Presenters[0].SwapChain.Target.D3DCpuHandle;
            _list.Get()->OMSetRenderTargets(1, &rtvHandle, false, null);
        }

        _list.Get()->DrawIndexedInstanced((uint)indexCount, (uint)instanceCount, (uint)startIndexLocation, baseVertexLocation, (uint)startInstanceLocation);

        _indexBuffer = null;
        _indexBufferChanged = true;
        _dynamicBuffers.Clear();
        _usedBuffers.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to assign a swap chain as a presenter for this command list.
    /// </summary>
    /// <param name="swapChain">The swap chain to assign as a presenter.</param>
    /// <param name="interval">[Optional] The presentation interval to use when presenting.</param>
    /// <returns>The command list as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="interval"/> parameter must be a value between 0 and 4.
    /// </para>
    /// </remarks>
    public GorgonCommandList AddPresenter(GorgonSwapChain swapChain, int interval = 0)
    {
        interval = interval.Max(0).Min(4);

        for (int i = 0; i < Presenters.Count; ++i)
        {
            if (Presenters[i].SwapChain == swapChain)
            {
                // We don't need to track again because if this item is already in the list, then 
                // it's already being tracked.
                Presenters[i] = (swapChain, interval);
                return this;
            }            
        }

        Presenters.Add((swapChain, interval));
        Queue.Tracker.TrackResource(swapChain.DXGISwapChain);
        Queue.Tracker.TrackResource(swapChain.Target.Texture.D3DResource);

        return this;
    }

    /// <summary>
    /// Function to clear a swap chain with a specified color.
    /// </summary>
    /// <param name="swapChain">The swap chain to clear.</param>
    /// <param name="color">The color to fill the swap chain back buffer with.</param>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearSwapChain(GorgonSwapChain swapChain, GorgonColor color)
    {
        Queue.Tracker.TrackResource(swapChain.DXGISwapChain);
        ClearRenderTarget(swapChain.Target, color);
        return this;
    }

    /// <summary>
    /// Function to clear a render target view with a specified color.
    /// </summary>
    /// <param name="renderTarget">The texture render target view to clear.</param>
    /// <param name="color"><inheritdoc cref="ClearSwapChain(GorgonSwapChain, GorgonColor)" path="/param[@name='color']"/></param>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearRenderTarget(GorgonTextureRenderTargetView renderTarget, GorgonColor color)
    {
        SetBarrier(renderTarget.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, force: true);

        float* r = &color.Red;
        Queue.Tracker.TrackResource(renderTarget.Texture);
        _list.Get()->ClearRenderTargetView(renderTarget.D3DCpuHandle, r, 0, null);

        return this;
    }

    /// <summary>
    /// This is temporary, just enough to get us up and running.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="sync"></param>
    /// <param name="access"></param>
    /// <returns></returns>
    public GorgonCommandList Use(GorgonGpuBuffer buffer, BarrierSync sync, BarrierAccess access)
    {
        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            _dynamicBuffers.Add(buffer);
        }

        _usedBuffers.Add((buffer, sync, access));

        Queue.Tracker.TrackResource(buffer);

        return this;
    }

    /// <summary>
    /// Function to assign an index buffer to render.
    /// </summary>
    /// <param name="buffer">The index buffer to assign, or <b>null</b> to unbind an existing index buffer.</param>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <remarks>
    /// <para>
    /// TODO: Fill me in.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonIndexBuffer"/>
    /// <seealso cref="BufferUsage"/>
    public GorgonCommandList Use(GorgonIndexBuffer? buffer)
    {
        if (buffer is null)
        {
            _indexBuffer = null;
            _indexBufferChanged = true;
            return this;
        }
                        
        Queue.Tracker.TrackResource(buffer);
        _indexBuffer = buffer;
        _indexBufferChanged = true;

        return this;
    }

    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public GorgonCommandList SetTextureTemp(GorgonTextureView? texture)
    {
        if (texture is not null)
        {
            SetBarrier(texture.Texture, BarrierSync.Draw, BarrierAccess.ShaderResource, BarrierLayout.ShaderResource);
        }

        _textureView = texture;
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/exception[not(@cref='T:Gorgon.Core.GorgonException')]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[1]"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para[2]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para[3]"/>
    /// </remarks>    
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// [StructLayout(LayoutKind.Sequential)]
    /// struct MyType
    /// {
    ///    public Vector4 Position;
    ///    public Vector2 UV;
    /// }
    /// 
    /// MyType sourceData = new()
    /// {
    ///    Position = new Vector4(1, 0, 1, 1),
    ///    UV = new Vector2(0, 1)
    /// };
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// 
    /// GorgonCommandList list = _graphics.BeginFrame();
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// list.CopyValue(in sourceData, destBuffer);
    ///       
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyValue<T>(in T value, GorgonGpuBufferCommon buffer, long offset = 0) where T : unmanaged
    {
        _resourceWriter.CopyValue(value, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/exception"/>    
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/remarks"/>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyImageToTexture(IGorgonImage image, GorgonTexture texture)
    {
        _resourceWriter.CopyImageToTexture(image, texture);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[2]"/>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// using GorgonNativeBuffer<byte> sourceData = new(1024);
    /// 
    /// // Code to write to the sourceData buffer goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// GorgonCommandList list = _graphics.BeginFrame();
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// list.CopyPointer<byte>(sourceData, destBuffer);
    ///       
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonPtr{T}"/>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyPointer<T>(GorgonPtr<T> pointer, GorgonGpuBufferCommon buffer, long offset = 0) where T : unmanaged
    {
        _resourceWriter.CopyPointer(pointer, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[2]"/>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// byte[] sourceData = new byte[1024];
    /// 
    /// // Code to write data to the sourceData array goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// GoronCommandList list = _graphics.BeginFrame();
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// list.CopyRange<byte>(sourceData.ToSpan(), destBuffer);
    ///       
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyRange<T>(ReadOnlySpan<T> values, GorgonGpuBufferCommon buffer, long offset = 0) where T : unmanaged
    {
        _resourceWriter.CopyRange(values, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyBuffer(GorgonGpuBufferCommon source, GorgonGpuBufferCommon destination, long sourceOffset = 0, long destinationOffset = 0, long? count = null)
    {
        _resourceWriter.CopyBuffer(source, destination, sourceOffset, destinationOffset, count);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel = 0, short destinationZOrArrayIndex = 0, byte destinationPlane = 0)
    {
        _resourceWriter.CopyImageToTexture(imageBuffer, texture, destinationMipLevel, destinationZOrArrayIndex, destinationPlane);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, GorgonCopyBufferToTexture parameters)
    {
        _resourceWriter.CopyBufferToTexture(buffer, texture, parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, long)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, long sourceOffset = 0)
    {
        _resourceWriter.CopyBufferToTexture(buffer, texture, sourceOffset);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, GorgonCopyTextureToBuffer parameters)
    {
        _resourceWriter.CopyTextureToBuffer(texture, buffer, parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, long)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, long destinationOffset = 0)
    {
        _resourceWriter.CopyTextureToBuffer(texture, buffer, destinationOffset);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTexture(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters)
    {
        _resourceWriter.CopyTexture(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTexture(GorgonTexture source, GorgonTexture destination)
    {
        _resourceWriter.CopyTexture(source, destination);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/remarks"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList SetBarrier(GorgonTexture texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool discard = false, bool force = false)
    {
        _barrierManager.AddBarrier(texture, sync, access, layout, subResources, discard);

        if ((force) && (!_barrierManager.IsEmpty))
        {
            _barrierManager.Submit(this);
        }

        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.SetBarrier(GorgonGpuBufferCommon, BarrierSync, BarrierAccess, bool)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.SetBarrier(GorgonGpuBufferCommon, BarrierSync, BarrierAccess, bool)" path="/param"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.SetBarrier(GorgonGpuBufferCommon, BarrierSync, BarrierAccess, bool)" path="/remarks"/>
    /// <returns><inheritdoc cref="AddPresenter" path="/returns"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList SetBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access, bool force = false)
    {
        _barrierManager.AddBarrier(buffer, sync, access);

        if ((force) && (!_barrierManager.IsEmpty))
        {
            _barrierManager.Submit(this);
        }

        return this;
    }

    /// <summary>
    /// Function to write a single constant value for shaders.
    /// </summary>
    /// <typeparam name="T">The type of data, must be an unmanaged value type.</typeparam>
    /// <param name="index">The constant slot to use.</param>
    /// <param name="data">The data to write to the constant slot.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> value is less than 0, or greater than or equal to the <see cref="GorgonGraphics.MaxRootConstantCount"/> value.</exception>
    /// <remarks>
    /// <para>
    /// This allows writing per-frame constant data to the constant slots in the shaders. The slot to use is indicated by the <paramref name="index"/> and correlates to the shader slot, for example if a 
    /// constant at index 0 correlates to a slot of <c>ConstantBuffer&lt;T&gt; cb : register(b0)</c>, index 1 correlates to a slot of <c>ConstantBuffer&lt;T&gt; cb : register(b1)</c> and so on, up until 
    /// a maximum of <see cref="GorgonGraphics.MaxRootConstantCount"/> on the <see cref="GorgonGraphics"/> object. 
    /// </para>
    /// <para>
    /// Because this data is per-frame, anything written with this method needs to be written again after the start of the next frame. This means that this method is meant for highly volatile data in the 
    /// shader, and is the most performant way to send volatile data to a shader. If persistent constant data is required, developers should use a <see cref="GorgonConstantBufferView"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics.MaxRootConstantCount"/>
    /// <seealso cref="GorgonConstantBufferView"/>
    public GorgonCommandList WriteConstant<T>(int index, in T data)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, GorgonGraphics.MaxRootConstantCount);

        nuint typeSize = (nuint)Unsafe.SizeOf<T>();

        Graphics.UploadHeaps.Allocate(typeSize, D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);

        Debug.Assert(allocation.IsAvailable, "Allocation for constant write is not valid.");

        fixed (T* src = &data)
        {
            NativeMemory.Copy(src, allocation.CpuPointer, typeSize);
        }

        _constantWriteData[index] = allocation;

        return this;
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonCommandList() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of a <see cref="GorgonCommandList"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns this object.</param>
    /// <param name="name">The name of the command list.</param>
    /// <param name="allocator">The command allocator associated with this command list.</param>
    /// <param name="queue">The command queue that created this list.</param>
    internal GorgonCommandList(GorgonGraphics graphics, string name, CommandAllocator allocator, CommandQueue queue)
    {
        Graphics = graphics;
        Queue = queue;
        Allocator = allocator;
        _barrierManager = new BarrierManager(graphics);
        _name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonCommandList));

        _list = CreateNative();

        // Keep a reference to the base command list type for assignment in the list execution code.
        _list.As(ref _baseList);

        _resourceWriter = _resourceCopier = new GorgonResourceCopier(Graphics, this);
    }

    #region Temporary junk to get rendering going.
    private static GpuDescriptorAllocation _samplerDescriptor = GpuDescriptorAllocation.Null;
    private GorgonTextureView? _textureView;
    private static bool _descriptorsSet;

    private void DoViewSetup()
    {
        // TODO: This ain't right.
        //if (_swapChain is not null)
        {
            //D3D12_CPU_DESCRIPTOR_HANDLE rtv = _swapChain.Target.D3DCpuHandle;
            //_list.Get()->OMSetRenderTargets(1, &rtv, false, null);

            int width = 1280;
            int height = 720;
            D3D12_VIEWPORT vp = new(0, 0, width, height, 0, 1);
            RECT rect = new(0, 0, width, height);

            _list.Get()->RSSetViewports(1, &vp);
            _list.Get()->RSSetScissorRects(1, &rect);
        }
    }

    private void SetupDescriptors()
    {
        if (_descriptorsSet)
        {
            return;
        }

        _descriptorsSet = true;

        D3D12_GPU_DESCRIPTOR_HANDLE gpuSampleHandle = Graphics.GpuSamplerDescriptors.D3DGpuHandle;

        if (_samplerDescriptor.IsNull)
        {
            Graphics.GpuSamplerDescriptors.Allocate(1, out _samplerDescriptor);

            D3D12_SAMPLER_DESC sampleDesc = new()
            {
                AddressU = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                AddressV = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                AddressW = D3D12_TEXTURE_ADDRESS_MODE.D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                Filter = D3D12_FILTER.D3D12_FILTER_MIN_MAG_MIP_POINT,
                MaxLOD = float.MaxValue,
                MaxAnisotropy = 16
            };

            D3D12_CPU_DESCRIPTOR_HANDLE cpuSampleHandle = Graphics.GpuSamplerDescriptors.D3DCpuHandle;

            cpuSampleHandle.Offset(_samplerDescriptor.Offset, Graphics.GpuSamplerDescriptors.DescriptorSize);

            Graphics.D3DDevice.Get()->CreateSampler(&sampleDesc, cpuSampleHandle);
        }

        if (_textureView is not null)
        {
            _list.Get()->SetGraphicsRootDescriptorTable(16, _textureView.D3DGpuHandle);
        }
        _list.Get()->SetGraphicsRootDescriptorTable(17, gpuSampleHandle);        
    }

    /// <summary>
    /// Function to resize or create the descriptor heaps for buffers/textures.
    /// </summary>
    private void AllocateGpuDescriptorHeaps()
    {
        if (Queue != Graphics.GraphicsQueue)
        {
            return;
        }

        _descriptorsSet = false;        

        ID3D12DescriptorHeap** heaps = stackalloc ID3D12DescriptorHeap*[2]
        {
            Graphics.GpuViewDescriptors.D3DHeap.Get(),
            Graphics.GpuSamplerDescriptors.D3DHeap.Get()
        };

        _list.Get()->SetDescriptorHeaps(2, heaps);
    }
    #endregion
}
