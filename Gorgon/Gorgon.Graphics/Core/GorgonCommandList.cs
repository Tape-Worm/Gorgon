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
using TerraFX.Interop.WinRT;
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
public sealed unsafe class GorgonCommandList
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
    private D3D12_CPU_DESCRIPTOR_HANDLE _depthStencilView;
    private bool _depthStencilChanged;
    private readonly GorgonRenderTargetView?[] _renderTargetViews = new GorgonRenderTargetView[GorgonVideoAdapterInfo.MaxRenderTargetCount];
    private readonly D3D12_CPU_DESCRIPTOR_HANDLE[] _d3dRtvs = new D3D12_CPU_DESCRIPTOR_HANDLE[GorgonVideoAdapterInfo.MaxRenderTargetCount];
    private uint _rtvsCount;
    private bool _rtvsChanged;
    private readonly GorgonViewport[] _viewports = new GorgonViewport[D3D12.D3D12_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE];
    private readonly D3D12_VIEWPORT[] _d3dViewports = new D3D12_VIEWPORT[D3D12.D3D12_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE];
    private uint _viewportCount;
    private bool _viewportsChanged;
    private readonly GorgonRectangle[] _scissors = new GorgonRectangle[D3D12.D3D12_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE];
    private readonly RECT[] _d3dScissors = new RECT[D3D12.D3D12_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE];
    private uint _scissorCount;
    private bool _scissorsChanged;    
    private readonly GpuDescriptorHeap _samplerDescriptors;
    private readonly GpuDescriptorHeap _viewDescriptors;
    private readonly MegaBufferPool _megaBuffer;
    private readonly VirtualTextureTilePool _textureTilePool;
    private readonly CpuResourceHeapPool _uploadHeaps;
    private GorgonIndexBuffer? _currentIndexBuffer;
    private Blitter? _blitter;   

    /// <summary>
    /// Property to set or return the allocator associated with the command list.
    /// </summary>
    internal CommandAllocator? Allocator
    {
        get => _commandAllocator;
        private set
        {            
            if (ReferenceEquals(_commandAllocator, value))
            {
                return;
            }

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
    /// Property to return the list of render targets bound to this command list.
    /// </summary>
    public ReadOnlySpan<GorgonRenderTargetView?> RenderTargets => _renderTargetViews.AsSpan(0, (int)_rtvsCount);

    /// <summary>
    /// Property to return the list of viewports bound to this command list.
    /// </summary>
    public ReadOnlySpan<GorgonViewport> Viewports => _viewports.AsSpan(0, (int)_viewportCount);

    /// <summary>
    /// Property to return the list of scissor rectangles bound to this command list.
    /// </summary>
    public ReadOnlySpan<GorgonRectangle> ScissorRectangles => _scissors.AsSpan(0, (int)_scissorCount);

    /// <summary>
    /// Property to return the depth/stencil buffer assigned to this command list.
    /// </summary>
    public GorgonDepthStencilView? DepthStencil
    {
        get;
        private set;
    }

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
            _blitter?.Dispose();
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
    /// <param name="indexBuffer">The index buffer to apply.</param>
    private void ApplyIndexBuffer(GorgonIndexBuffer? indexBuffer)
    {
        if (_currentIndexBuffer == indexBuffer)
        {
            return;
        }

        if (indexBuffer is null)
        {
            _list.Get()->IASetIndexBuffer(null);
            _currentIndexBuffer = null;
            return;
        }

        Queue.Tracker.TrackResource(indexBuffer);
        SetBarrier(indexBuffer, BarrierSync.IndexInput, BarrierAccess.IndexBuffer);

        D3D12_INDEX_BUFFER_VIEW view = new()
        {
            BufferLocation = indexBuffer.D3DResource.Get()->GetGPUVirtualAddress(),
            Format = indexBuffer.Use32BitIndices ? DXGI_FORMAT.DXGI_FORMAT_R32_UINT : DXGI_FORMAT.DXGI_FORMAT_R16_UINT,
            SizeInBytes = (uint)indexBuffer.SizeInBytes
        };

        _list.Get()->IASetIndexBuffer(&view);
        _currentIndexBuffer = indexBuffer;
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
    /// Function to set the barriers for textures on a draw call.
    /// </summary>
    /// <param name="textures">The textures on the draw call.</param>
    private void ApplyTextureBarriers(ReadOnlySpan<(IGorgonTextureView<GorgonTextureCommon> Texture, ShaderStage Shader, TextureUsage Usage)> textures)
    {
        if (textures.Length == 0)
        {
            return;
        }

        for (int i = 0; i < textures.Length; ++i)
        {
            (IGorgonTextureView<GorgonTextureCommon> textureView, ShaderStage shader, TextureUsage usage) = textures[i];
            GorgonTextureCommon texture = textureView.Texture;

            Queue.Tracker.TrackResource(texture);

            // ExecuteIndirect does not use a shader stage, it's its own thing.
            BarrierSync sync = shader.ToSync();

            BarrierAccess access = usage switch
            {
                TextureUsage.Writeable => BarrierAccess.ReadWrite,
                _ => BarrierAccess.ShaderResource,
            };

            BarrierLayout layout = usage switch
            {
                TextureUsage.Writeable => BarrierLayout.ReadWrite,
                _ => BarrierLayout.ShaderResource,
            };

            SetBarrier(texture, sync, access, layout);
        }
    }

    /// <summary>
    /// Function to apply barriers to buffers used in a draw call.
    /// </summary>
    private void ApplyBufferBarriers(ReadOnlySpan<(GorgonGpuBuffer Buffer, ShaderStage Shader, BufferUsage Usage)> buffers)
    {
        static BarrierSync GetSyncForBuffer(BufferUsage usage, ShaderStage stage) => usage != BufferUsage.IndirectArguments ? stage.ToSync() : BarrierSync.ExecuteIndirect;
        static BarrierAccess GetAccessForBuffer(BufferUsage usage) => usage switch
        {
            BufferUsage.ConstantBuffer => BarrierAccess.ConstantBuffer,
            BufferUsage.Writeable => BarrierAccess.ReadWrite,
            BufferUsage.ReadWrite => BarrierAccess.ReadWrite | BarrierAccess.ShaderResource,
            BufferUsage.IndirectArguments => BarrierAccess.IndirectArgument,
            _ => BarrierAccess.ShaderResource,
        };

        // We need to validate that the buffers that are reading do not overlap their data regions with buffers that are writing.
        // Since we are using the MegaBuffer approach, this is a potential hazard. But, we also need to ensure that we're not trying to 
        // make a single non-UAV buffer (user facing, not mega buffer) with read access and write access simultaneously (e.g. SRV | COPY_DEST).
        static int Intersects(GorgonGpuBuffer buffer, int bufferIndex, BarrierSync sync, BarrierAccess access, ReadOnlySpan<(GorgonGpuBuffer Buffer, ShaderStage Shader, BufferUsage Usage)> buffers)
        {
            ulong bufferStart = buffer.ResourceOffset;
            ulong bufferEnd = (ulong)buffer.SizeInBytes + buffer.ResourceOffset;

            for (int i = 0; i < buffers.Length; ++i)
            {
                if (bufferIndex == i)
                {
                    continue;
                }

                (GorgonGpuBuffer otherBuffer, ShaderStage otherStage, BufferUsage otherUsage) = buffers[i];
                BarrierSync otherSync = GetSyncForBuffer(otherUsage, otherStage);
                BarrierAccess otherAccess = GetAccessForBuffer(otherUsage);
                
                ulong otherEnd = (ulong)otherBuffer.SizeInBytes +  otherBuffer.ResourceOffset;

                if ((bufferStart < otherEnd) && (otherBuffer.ResourceOffset < bufferEnd))
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

        for (int i = 0; i < buffers.Length; ++i)
        {
            (GorgonGpuBuffer buffer, ShaderStage stage, BufferUsage usage) = buffers[i];
            BarrierSync sync = GetSyncForBuffer(usage, stage);
            BarrierAccess access = GetAccessForBuffer(usage);

            if (!buffer.IsMegaBufferResource)
            {
                // If this buffer is not from the mega buffer, then just set its barrier as-is.
                _barrierManager.AddBarrier(buffer, sync, access);
                Queue.Tracker.TrackResource(buffer);
                continue;
            }

            // Multiple buffers cannot be used with different states in the same memory range.
            // We'll check here to see if this buffer intersects any other buffers, and if it 
            // does, we'll throw an exception.
            if (Graphics.IsInDebugMode)
            {
                int collisionIndex = Intersects(buffer, i, sync, access, buffers);

                if (collisionIndex != -1)
                {
                    throw new GorgonException(GorgonResult.CannotExecute, string.Format(Resources.GORGFX_ERR_BUFFERS_AND_BARRIER_OVERLAP, buffer, sync, access, 
                                                                                        buffers[collisionIndex].Buffer, 
                                                                                        GetSyncForBuffer(buffers[collisionIndex].Usage, buffers[collisionIndex].Shader), 
                                                                                        GetAccessForBuffer(buffers[collisionIndex].Usage)));
                }                
            }

            // Otherwise, combine the access/sync patterns to we can use the mega buffer resource 
            // for whatever we have in mind.
            megaSync |= sync;
            megaAccess |= access;

            _barrierManager.AddBarrier(buffer, megaSync, megaAccess);
            Queue.Tracker.TrackResource(buffer);
        }        
    }

    /// <summary>
    /// Function to assign any pending render target views.
    /// </summary>
    private void ApplyRenderTargets()
    {
        if ((!_rtvsChanged) && (!_depthStencilChanged))
        {
            return;
        }

        D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = _depthStencilView;

        if (_rtvsCount != 0)
        {
            fixed (D3D12_CPU_DESCRIPTOR_HANDLE* rtvHandlePtr = &_d3dRtvs[0])
            {
                _list.Get()->OMSetRenderTargets(_rtvsCount, rtvHandlePtr, false, dsvHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT ? &dsvHandle : null);
            }
        }
        else
        {
            _list.Get()->OMSetRenderTargets(0, null, false, dsvHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT ? &dsvHandle : null);
        }

        _rtvsChanged = false;
    }

    /// <summary>
    /// Function to apply the PSO for the command list.
    /// </summary>
    /// <param name="pso">The pipeline state to apply.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyPso(GorgonGraphicsPso pso)
    {
        if (pso.D3DPso.IsNull)
        {
            pso.UpdateD3DPso();
        }

        Queue.Tracker.TrackResource(pso.D3DPso);

        _list.Get()->SetPipelineState((PID3D12PipelineState1)pso.D3DPso.Get());
        _list.Get()->IASetPrimitiveTopology((D3D_PRIMITIVE_TOPOLOGY)pso.PrimitiveType);
    }

    /// <summary>
    /// Function to apply the viewports and scissor rectangles.
    /// </summary>
    private void ApplyViewSetup()
    {
        if (_viewportsChanged)
        {
            if (_viewportCount != 0)
            {
                fixed (D3D12_VIEWPORT* vpPtr = &_d3dViewports[0])
                {
                    _list.Get()->RSSetViewports(_viewportCount, vpPtr);
                }
            }
            else
            {
                _list.Get()->RSSetViewports(0, null);
            }

            _viewportsChanged = false;
        }

        if (!_scissorsChanged)
        {
            return;
        }

        if (_scissorCount != 0)
        {
            fixed (RECT* scissorPtr = &_d3dScissors[0])
            {
                _list.Get()->RSSetScissorRects(_scissorCount, scissorPtr);
            }
        }
        else
        {
            _list.Get()->RSSetScissorRects(0, null);
        }

        _scissorsChanged = false;
    }

    /// <summary>
    /// Function to clear a depth/stencil to the specified values.
    /// </summary>
    /// <param name="depthStencil">The depth/stencil view containing the texture to clear.</param>
    /// <param name="depthValue">The depth value to write.</param>
    /// <param name="stencilValue">The stencil value to write.</param>
    /// <param name="clearRects">[Optional] Defines a list of regions to clear on the depth/stencil texture.</param>
    /// <param name="flags">The flags to use for clearing the buffer.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    private GorgonCommandList ClearDepthStencil(GorgonDepthStencilView depthStencil, float depthValue, byte stencilValue, IReadOnlyList<GorgonRectangle>? clearRects, D3D12_CLEAR_FLAGS flags)
    {
        if (!depthStencil.FormatInfo.HasDepth)
        {
            return this;
        }

        Queue.Tracker.TrackResource(depthStencil.Texture);
        SetBarrier(depthStencil.Texture, BarrierSync.DepthStencil, BarrierAccess.DepthStencilWrite, BarrierLayout.DepthStencilWrite, force: true);

        clearRects ??= [];

        if (!depthStencil.Texture.FormatInfo.HasStencil)
        {
            flags &= ~D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_STENCIL;
        }

        if (clearRects.Count == 0)
        {
            _list.Get()->ClearDepthStencilView(depthStencil.GetCpuHandle(), flags, depthValue, stencilValue, 0, null);
        }
        else
        {
            RECT* rects = stackalloc RECT[clearRects.Count];

            for (int i = 0; i < clearRects.Count; ++i)
            {
                GorgonRectangle rect = clearRects[i];
                rects[i] = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
            _list.Get()->ClearDepthStencilView(depthStencil.GetCpuHandle(), flags, depthValue, stencilValue, (uint)clearRects.Count, rects);
        }

        return this;
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
            swapChain.Present((uint)interval);
        }
    }

    /// <summary>
    /// Function to begin recordinf of the command list.
    /// </summary>
    /// <param name="currentFrame">The current frame for our in-flight frame values.</param>
    /// <param name="rootSignature">The global root signature for the application.</param>
    internal void BeginRecording(int currentFrame, ref readonly ComPtr<ID3D12RootSignature> rootSignature)
    {
        ResetState(Name, Allocator);

        _megaBuffer.Signal();
        _textureTilePool.Signal();
        Queue.AllocatorPool.Signal();

        _uploadHeaps.Signal();
        _samplerDescriptors.Signal();
        _viewDescriptors.Signal();

        // Wait for the next frame to become available.
        Queue.WaitForFence(Queue.FrameFenceValue[currentFrame], Timeout.Infinite);

        // Signal any resources that are awaiting destruction.
        Queue.Tracker.Signal();

        // Any previous barriers on this command list should be voided.
        _barrierManager.Clear();

        // Ensure we have our heaps set prior to the root signature.
        ID3D12DescriptorHeap** heaps = stackalloc ID3D12DescriptorHeap*[2]
        {
            _samplerDescriptors.D3DHeap.Get(),
            _viewDescriptors.D3DHeap.Get()
        };

        _list.Get()->SetDescriptorHeaps(2, heaps);
        _list.Get()->SetGraphicsRootSignature(rootSignature.Get());
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

        _barrierManager.CopyCurrentToGlobal(Queue.Type);
        Array.Clear(_constantWriteData);
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
        _currentIndexBuffer = null;
    }

    /// <summary>
    /// TBD
    /// </summary>
    /// <param name="drawCall"></param>
    public void Draw(GorgonDrawCall drawCall)
    {   
        // Apply pending barrier calls.
        ApplyBufferBarriers(drawCall.UsedBuffers);
        ApplyTextureBarriers(drawCall.UsedTextures);
        ApplyIndexBuffer(drawCall.IndexBuffer);
        ApplyConstantWrites();
        ApplyRenderTargets();
        ApplyPso(drawCall.Pso);
        ApplyViewSetup();

        _barrierManager.Submit(this);

        _list.Get()->DrawIndexedInstanced((uint)drawCall.IndexCount, (uint)drawCall.InstanceCount, (uint)drawCall.StartIndex, drawCall.BaseVertex, (uint)drawCall.StartInstance);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
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
                if (interval == Presenters[i].PresentInterval)
                {
                    return this;
                }

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
    /// <param name="clearRects">[Optional] Defines a list of regions to clear on the render target.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearSwapChain(GorgonSwapChain swapChain, GorgonColor color, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        Queue.Tracker.TrackResource(swapChain.DXGISwapChain);
        ClearRenderTarget(swapChain.Target, color, clearRects);
        return this;
    }

    /// <summary>
    /// Function to clear a render target view with a specified color.
    /// </summary>
    /// <param name="renderTarget">The render target view texture to clear.</param>
    /// <param name="color"><inheritdoc cref="ClearSwapChain(GorgonSwapChain, GorgonColor, IReadOnlyList{GorgonRectangle})" path="/param[@name='color']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearSwapChain(GorgonSwapChain, GorgonColor, IReadOnlyList{GorgonRectangle})" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    public GorgonCommandList ClearRenderTarget(GorgonRenderTargetView renderTarget, GorgonColor color, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        SetBarrier(renderTarget.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, force: true);

        clearRects ??= [];

        float* r = &color.Red;
        Queue.Tracker.TrackResource(renderTarget.Texture);

        if (clearRects.Count == 0)
        {
            _list.Get()->ClearRenderTargetView(renderTarget.GetCpuHandle(), r, 0, null);

            return this;
        }

        RECT* rects = stackalloc RECT[clearRects.Count];

        for (int i = 0; i < clearRects.Count; ++i)
        {
            GorgonRectangle rect = clearRects[i];
            rects[i] = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        _list.Get()->ClearRenderTargetView(renderTarget.GetCpuHandle(), r, (uint)clearRects.Count, rects);

        return this;
    }

    /// <summary>
    /// <inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?, D3D12_CLEAR_FLAGS)" path="/summary"/>
    /// </summary>
    /// <param name="depthStencil"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?, D3D12_CLEAR_FLAGS)" path="/param[@name='depthStencil']"/></param>
    /// <param name="depthValue"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?, D3D12_CLEAR_FLAGS)" path="/param[@name='depthValue']"/></param>
    /// <param name="stencilValue"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?, D3D12_CLEAR_FLAGS)" path="/param[@name='stencilValue']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?, D3D12_CLEAR_FLAGS)" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearDepthStencil(GorgonDepthStencilView depthStencil, float depthValue, byte stencilValue, IReadOnlyList<GorgonRectangle>? clearRects = null) =>
        ClearDepthStencil(depthStencil, depthValue, stencilValue, clearRects, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH | D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_STENCIL);

    /// <summary>
    /// Function to clear a depth buffer to the specified values.
    /// </summary>
    /// <param name="depthStencil">The depth/stencil view containing the depth buffer to clear.</param>
    /// <param name="depthValue"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='depthValue']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearDepth(GorgonDepthStencilView depthStencil, float depthValue, IReadOnlyList<GorgonRectangle>? clearRects = null) => 
        ClearDepthStencil(depthStencil, depthValue, 0, clearRects, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH);

    /// <summary>
    /// Function to clear a stencil buffer to the specified values.
    /// </summary>
    /// <param name="depthStencil">The depth/stencil view containing the stencil buffer to clear.</param>
    /// <param name="stencilValue"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='stencilValue']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearStencil(GorgonDepthStencilView depthStencil, byte stencilValue, IReadOnlyList<GorgonRectangle>? clearRects = null) =>
        ClearDepthStencil(depthStencil, 1.0f, stencilValue, clearRects, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_STENCIL);

    /// <summary>
    /// Function to reset the stream out counter for the specified stream out view.
    /// </summary>
    /// <param name="view">The stream out view to update.</param>
    /// <param name="count">[Optional] The initial value to set for the counter.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This is used to reset the stream out buffer's counter to an initial count value. Applications should call this when reusing stream out and want to start over or at a specific count.
    /// </para>
    /// </remarks>
    public GorgonCommandList ResetStreamOutCounter(GorgonStreamOutView view, long count = 0) => CopyValue(count, view.CounterBuffer);

    /// <summary>
    /// Function to reset the usage for the specified textures.
    /// </summary>
    /// <param name="textures">The list of textures that need their usage state reset.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/barrier_issue"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// When a texture is used in a command list on the <see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/> (directly), or the <see cref="GorgonComputeEngine"/> objects, it will be put into a 
    /// state that provides optimal usage for the GPU. 
    /// </para>
    /// <para>
    /// Because of this, the state of the object will need to be reset before using it on a different <see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/>, or the 
    /// <see cref="GorgonComputeEngine"/> objects, otherwise an exception will be thrown when it is used without resetting it to a generalized state.
    /// </para>
    /// <para>
    /// This is required because the GPU can have metadata on a resource that can be used to help with rendering, or other operations. Unfortunately that metadata, makes the resource unusable for other 
    /// operations like copying, or use as a read/write resource. So, the resource needs to be put back into a general state before it can be consumed in those operations.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Any texture that has been reset cannot be used again in the same command list. When the texture has been reset, we are telling the GPU that there is no intention to use the texture again until it has 
    /// been submitted to the <see cref="GorgonGraphics.Submit(GorgonCommandList)"/> method.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The reset operation <b>must</b> be called on a command list from the correct object. So, if the rsource was last used by a command list from a <see cref="GorgonGraphics"/> object, then only a command 
    /// list from that object can reset the resource, otherwise an exception will be thrown.
    /// </para>    
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="GorgonComputeEngine"/>
    public GorgonCommandList Reset(ReadOnlySpan<IGorgonTextureView<GorgonTextureCommon>> textures)
    {
        if (textures.Length == 0)
        {
            return this;
        }

        for (int i = 0; i < textures.Length; ++i)
        {
            GorgonTextureCommon texture = textures[i].Texture;
            Queue.Tracker.TrackResource(texture);
            SetBarrier(texture, BarrierSync.None, BarrierAccess.None, BarrierLayout.Common);
        }

        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/exception[not(@cref='T:Gorgon.Core.GorgonException')]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[1]"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para[2]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para[3]"/>
    /// </remarks>    
    /// <example>
    /// <code language="csharp">
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/exception"/>    
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/remarks/para[@type='common']"/>    
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyImageToTexture(IGorgonImage image, GorgonTexture texture)
    {
        _resourceWriter.CopyImageToTexture(image, texture);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[2]"/>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/remarks/para[2]"/>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel = 0, short destinationZOrArrayIndex = 0, byte destinationPlane = 0)
    {
        _resourceWriter.CopyImageToTexture(imageBuffer, texture, destinationMipLevel, destinationZOrArrayIndex, destinationPlane);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, short)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, short)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, short)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyImageToTexture(IGorgonImageBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, short)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonVirtualTexture texture, GorgonVirtualTextureHandle handle, short destinationDepthSlice = 0)
    {
        _resourceWriter.CopyImageToTexture(imageBuffer, texture, handle, destinationDepthSlice);
        return this;
    }


    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, GorgonCopyBufferToTexture parameters)
    {
        _resourceWriter.CopyBufferToTexture(buffer, texture, parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, GorgonCopyTextureToBuffer parameters)
    {
        _resourceWriter.CopyTextureToBuffer(texture, buffer, parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTexture(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters)
    {
        _resourceWriter.CopyTexture(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTexture(GorgonTexture, GorgonTexture)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTexture(GorgonTexture source, GorgonTexture destination)
    {
        _resourceWriter.CopyTexture(source, destination);
        return this;
    }


    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyTextureToVirtual(GorgonTexture source, GorgonVirtualTexture destination, ref readonly GorgonCopyTextureToVirtual parameters)
    {
        _resourceWriter.CopyTextureToVirtual(source, destination, in parameters);
        return this;
    }


    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyVirtualToTexture(GorgonVirtualTexture source, GorgonTexture destination, ref readonly GorgonCopyVirtualToTexture parameters)
    {
        _resourceWriter.CopyVirtualToTexture(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/param"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/exception"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyVirtualToVirtual(GorgonVirtualTexture source, GorgonVirtualTexture destination, ref readonly GorgonCopyVirtualToVirtual parameters)
    {
        _resourceWriter.CopyVirtualToVirtual(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/summary"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/param"/>
    /// <exception cref="ArgumentOutOfRangeException"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/exception[@cref='T:System.ArgumentOutOfRangeException']/para"/></exception>
    /// <exception cref="ArgumentException"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/exception[@cref='T:System.ArgumentException']/para"/></exception>
    /// <exception cref="GorgonException"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[@type='common']"/></exception>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyBufferToVirtual(GorgonGpuBuffer buffer, GorgonVirtualTexture texture, GorgonVirtualTextureHandle destinationHandle, long sourceOffset = 0)
    {
        _resourceWriter.CopyBufferToVirtual(buffer, texture, destinationHandle, sourceOffset);
        return this;
    }

    /// <summary>
    /// Function to set a barrier on a texture to enforce synchronization.
    /// </summary>
    /// <param name="texture">The texture to assign the barrier to.</param>
    /// <param name="sync">The new synchronization state for the barrier.</param>
    /// <param name="access">The new access level for the barrier.</param>
    /// <param name="layout">The new layout for the texture data.</param>
    /// <param name="subResources">[Optional] The individual sub resource on the texture to barrier.</param>
    /// <param name="discard">[Optional] <b>true</b> to force a discard operation on the resource, <b>false</b> to leave as-is.</param>
    /// <param name="force">[Optional] <b>true</b> to force the barrier to apply right away, <b>false</b> to wait until the barrier is required.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para type="barrier_issue">Thrown if a resource was previously used in a command list on one of the Gorgon root objects (<see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/>, or 
    /// <see cref="GorgonComputeEngine"/>) and was left in a usage state that is incompatible with the root object that is currently trying to use the resource.
    /// </para>
    /// <para type="barrier_issue">Thrown if a resource was previously reset on a command list, and then used again before the command list was submitted.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Most operations in Gorgon automatically apply barriers for resources on the user's behalf. However, there may be times the end user may need a more optimal barrier strategy, or Gorgon cannot set the 
    /// barriers correctly. This method allows users to bypass the automatic barrier management.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para><h3>What is a barrier? What is it for?</h3></para>
    /// <para>
    /// Modern GPUs may require their resources to be in specific states (e.g. compressed, uncompressed, etc...) prior to modification, and they also run operations in parallel. To prevent hazards for 
    /// read-after-write, write-after-read, and write-after-write operations, applications can provide barriers to indicate that they intend to perform a certain action, requiring certain access types and 
    /// data layout. This allows the GPU to perform the necessary operations required to ensure the data is synchronized. 
    /// </para>
    /// <para>
    /// For more detailed information, please refer to <a target="_blank">https://microsoft.github.io/DirectX-Specs/d3d/D3D12EnhancedBarriers.html</a>.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The synchronization bits provided by the <paramref name="sync"/> parameter indicate what synchronization we require before accessing the data. Thess bits can be combined to provide multiple 
    /// synchronization types.
    /// </para>
    /// <para>
    /// The access bits provided provided by the <paramref name="access"/> parameter indicates what type of access we require before accessing the data. Since GPUs cache a lot of their write operations, 
    /// this allows the GPU to ensure that the caches are correctly flushed. Thess bits can be combined to provide multiple access types.
    /// </para>
    /// <para type="TextureBarrier">
    /// The <paramref name="layout"/> transition is for <see cref="GorgonTextureCommon"/> objects only. This value indicates the type of read/write operation being performed so the GPU can compress or 
    /// decompress its data (depending on the GPU architecture).
    /// </para>
    /// <para type="TextureBarrier">
    /// The barrier can be applied to sub resources of a <see cref="GorgonTextureCommon"/> by assigning a value to the <paramref name="subResources"/> parameter. If this value is omitted, then the entire resource 
    /// has the barrier applied instead of a portion of it. By assigning a <see cref="GorgonSubResourceRange"/> to this barrier, the GPU can allow work on another portion of the texture while working on 
    /// the sub resource passed to this method.
    /// </para>
    /// <para type="TextureBarrier">
    /// The <paramref name="discard"/> value indicates that the contents of the resource can be discarded/ignored when this value is <b>true</b>. This is only available for initial barriers on a 
    /// <see cref="GorgonTextureCommon"/> resource.
    /// </para>
    /// <para>
    /// When a barrier is set on a resource, it is queued in a list of barriers and when it comes time to make use of the resource in a <see cref="GorgonCommandList"/> operation. This operation is automatic.
    /// If this value is set to <b>true</b>, then that queue is processed immediately after this barrier is set. This allows the user to control when barriers are applied. Queuing of barriers until the last 
    /// moment is a performance optimization for some GPUs.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonCommandList"/>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList SetBarrier(GorgonTextureCommon texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool discard = false, bool force = false)
    {
        _barrierManager.AddBarrier(this, texture, sync, access, layout, subResources, discard);

        if ((force) && (!_barrierManager.IsEmpty))
        {
            _barrierManager.Submit(this);
        }

        return this;
    }

    /// <summary>
    /// Function to set a barrier on a buffer to enforce synchronization.
    /// </summary>
    /// <param name="buffer">The buffer to assign the barrier to.</param>
    /// <param name="sync"><inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='sync']"/></param>
    /// <param name="access"><inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='access']"/></param>
    /// <param name="force"><inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='force']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/remarks/para[not(@type='TextureBarrier')]"/>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    /// <seealso cref="GorgonCommandList"/>    
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
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
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, GorgonGraphics.MaxRootConstantCount);

        nuint typeSize = (nuint)Unsafe.SizeOf<T>();

        _uploadHeaps.Allocate(typeSize, D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, "Allocation for constant write is not valid.");

        Queue.Tracker.TrackResource(allocation.Heap.D3DResource);

        fixed (T* src = &data)
        {
            NativeMemory.Copy(src, allocation.CpuPointer, typeSize);
        }

        _constantWriteData[index] = allocation;

        return this;
    }

    /// <summary>
    /// Function to upload data in a <see cref="GorgonGpuUploadMemory"/> to a <see cref="GorgonGpuBufferCommon"/>
    /// </summary>
    /// <param name="upload">The transient GPU memory containing the data to copy.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="offset">[Optional] The offset within the <paramref name="buffer"/> to start writing into.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the size of the <paramref name="upload"/> memory, in bytes, exceeds the size of the <paramref name="buffer"/>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="upload"/> memory is no longer available for use.</exception>
    /// <remarks>
    /// <para>
    /// TODO:
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuUploadMemory"/>
    public GorgonCommandList UploadGpuMemoryToBuffer(ref readonly GorgonGpuUploadMemory upload, GorgonGpuBufferCommon buffer, long offset = 0)
    {
        if (!upload.Allocation.IsAvailable)
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_UPLOAD_NOT_AVAILABLE);
        }

        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (upload.SizeInBytes + offset > buffer.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, offset, upload.SizeInBytes, buffer.SizeInBytes));
        }

        Queue.Tracker.TrackResource(upload.Allocation.Heap.D3DResource);
        Queue.Tracker.TrackResource(buffer);

        SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination, true);
        D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)buffer.D3DResource.Get(), buffer.ResourceOffset + (ulong)offset, (PID3D12Resource2)upload.Allocation.Heap.D3DResource.Get(), upload.Allocation.Offset, (ulong)upload.SizeInBytes);

        return this;
    }

    /// <summary>
    /// Function to set the viewports for rendering.
    /// </summary>
    /// <param name="viewports">The viewports to assign.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// TODO:
    /// </remarks>
    public GorgonCommandList SetViewports(ReadOnlySpan<GorgonViewport> viewports)
    {
        Array.Clear(_d3dViewports);
        Array.Clear(_viewports);

        for (int i = 0; i < viewports.Length.Min(_viewports.Length); ++i)
        {
            _viewports[i] = viewports[i];
            _d3dViewports[i] = viewports[i].ToD3DViewport();
        }

        _viewportCount = (uint)viewports.Length;
        _viewportsChanged = true;

        return this;
    }

    /// <summary>
    /// Function to set the viewport for rendering.
    /// </summary>
    /// <param name="viewport">The viewport to assign.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// TODO:
    /// </remarks>
    public GorgonCommandList SetViewport(GorgonViewport viewport)
    {
        if (_viewportCount > 1)
        {
            Array.Clear(_d3dViewports);
            Array.Clear(_viewports);
        }

        _viewportCount = viewport.IsEmpty ? 0 : 1u;
        _viewports[0] = viewport;
        _d3dViewports[0] = viewport.ToD3DViewport();
        _viewportsChanged = true;

        return this;
    }

    /// <summary>
    /// Function to set the scissor rectangles for clipping rendering.
    /// </summary>
    /// <param name="scissors">The scissor rectangles to assign.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// TODO:
    /// </remarks>
    public GorgonCommandList SetScissorRectangles(ReadOnlySpan<GorgonRectangle> scissors)
    {
        Array.Clear(_d3dScissors);
        Array.Clear(_scissors);

        for (int i = 0; i < scissors.Length.Min(_scissors.Length); ++i)
        {
            _scissors[i] = scissors[i];
            _d3dScissors[i] = scissors[i].ToWin32Rect();
        }

        _scissorCount = (uint)scissors.Length;
        _scissorsChanged = true;

        return this;
    }

    /// <summary>
    /// Function to set the scissor rectangle for clipping rendering.
    /// </summary>
    /// <param name="scissor">The scissor rectangle to assign.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// TODO:
    /// </remarks>
    public GorgonCommandList SetScissorRectangle(GorgonRectangle scissor)
    {
        if (_scissorCount > 1)
        {
            Array.Clear(_d3dScissors);
            Array.Clear(_scissors);
        }

        _scissorCount = scissor.IsEmpty ? 0 : 1u;
        _scissors[0] = scissor;
        _d3dScissors[0] = scissor.ToWin32Rect();
        _scissorsChanged = true;

        return this;
    }

    /// <summary>
    /// Function to set a single render target view to use when rendering.
    /// </summary>
    /// <param name="renderTarget">The render target to assign.</param>
    /// <param name="depthStencil">[Optional] The depth/stencil to assign.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// TODO:
    /// </remarks>
    public GorgonCommandList SetRenderTarget(GorgonRenderTargetView? renderTarget, GorgonDepthStencilView? depthStencil = null)
    {
        if (_rtvsCount > 1)
        {
            Array.Clear(_d3dRtvs);
            Array.Clear(_renderTargetViews);            
        }

        _renderTargetViews[0] = renderTarget;
        _d3dRtvs[0] = renderTarget?.GetCpuHandle() ?? D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;        
        _rtvsCount = renderTarget is null ? 0 : 1u;
        _rtvsChanged = true;

        DepthStencil = depthStencil;
        _depthStencilView = depthStencil?.GetCpuHandle() ?? D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
        _depthStencilChanged = true;

        if (renderTarget is not null)
        {
            GorgonSubResourceRange range = new(renderTarget.MipLevel, 1, renderTarget.ArrayIndex, renderTarget.ArrayCount, renderTarget.PlaneIndex, 1);
            SetBarrier(renderTarget.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, range);
        }

        if (depthStencil is not null)
        {
            GorgonSubResourceRange range = new(depthStencil.MipLevel, 1, depthStencil.ArrayIndex, depthStencil.ArrayCount, 0, 1);
            SetBarrier(depthStencil.Texture, BarrierSync.DepthStencil, BarrierAccess.DepthStencilWrite, BarrierLayout.DepthStencilWrite, range);
        }

        return this;
    }

    /// <summary>
    /// Function to set the render target views to use when rendering.
    /// </summary>
    /// <param name="renderTargets">The list of render targets to assign.</param>
    /// <param name="depthStencil">[Optional] The depth stencil view to assign.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// TODO:
    /// </remarks>
    public GorgonCommandList SetRenderTargets(ReadOnlySpan<GorgonRenderTargetView> renderTargets, GorgonDepthStencilView? depthStencil = null)
    {
        if (renderTargets.Length > _d3dRtvs.Length)
        {
            throw new GorgonException(GorgonResult.CannotBind, "TODO: This should be a resource string. But there's way too many rtvs.");
        }

        Array.Clear(_d3dRtvs);
        Array.Clear(_renderTargetViews);
        _rtvsCount = 0;

        for (int i = 0; i < renderTargets.Length.Min(_renderTargetViews.Length); ++i)
        {
            GorgonRenderTargetView? view = _renderTargetViews[i] = renderTargets[i];
            
            if (view is null)
            {
                _d3dRtvs[i] = D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
                continue;
            }

            Queue.Tracker.TrackResource(view.Resource);
            _d3dRtvs[i] = view.GetCpuHandle();

            GorgonSubResourceRange range = new(view.MipLevel, 1, view.ArrayIndex, view.ArrayCount, view.PlaneIndex, 1);
            SetBarrier(view.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, range);
        }

        if (depthStencil is not null)
        {
            GorgonSubResourceRange range = new(depthStencil.MipLevel, 1, depthStencil.ArrayIndex, depthStencil.ArrayCount, 0, 1);
            SetBarrier(depthStencil.Texture, BarrierSync.DepthStencil, BarrierAccess.DepthStencilWrite, BarrierLayout.DepthStencilWrite, range);
        }

        DepthStencil = depthStencil;
        _depthStencilView = depthStencil?.GetCpuHandle() ?? D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
        _depthStencilChanged = true;

        _rtvsCount = (uint)renderTargets.Length;
        _rtvsChanged = true;

        return this;
    }

    public void Blit(IGorgonTextureView<GorgonTextureCommon> texture, GorgonRectangleF destination, GorgonRectangleF? textureCoordinates = null, GorgonSampler? sampler = null, GorgonBlendState? blendState = null)
    {
        if (_viewportCount == 0)
        {
            throw new GorgonException(GorgonResult.CannotExecute, Resources.GORGFX_ERR_NO_VIEWPORTS);
        }

        if (_rtvsCount == 0)
        {
            throw new GorgonException(GorgonResult.CannotExecute, Resources.GORGFX_ERR_NO_RTVS);
        }

        _blitter ??= new Blitter(this);
        _blitter.Draw(texture, destination, textureCoordinates ?? new GorgonRectangleF(0, 0, 1, 1), sampler ?? GorgonSampler.Default(Graphics), blendState ?? GorgonBlendState.NoBlending);
    }

    /// <summary>
    /// Function to retrieve a block of transient GPU memory for uploading to a buffer.
    /// </summary>
    /// <param name="sizeInBytes">The number of bytes to allocate.</param>
    /// <returns>A new <see cref="GorgonGpuUploadMemory"/> structure.</returns>
    /// <remarks>
    /// <para>
    /// TOOD:
    /// </para>
    /// </remarks>
    public GorgonGpuUploadMemory GetGpuUploadMemory(long sizeInBytes) => new(Graphics, sizeInBytes);

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
        _megaBuffer = graphics.Memory.MegaBuffer;
        _textureTilePool = graphics.Memory.TextureTilePool;
        _uploadHeaps = graphics.Memory.UploadHeaps;
        _samplerDescriptors = Graphics.Descriptors.GpuSamplerDescriptors;
        _viewDescriptors = Graphics.Descriptors.GpuViewDescriptors;
        Queue = queue;
        Allocator = allocator;
        _barrierManager = new BarrierManager(graphics);
        _name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonCommandList));

        _list = CreateNative();

        // Keep a reference to the base command list type for assignment in the list execution code.
        _list.As(ref _baseList);

        _resourceWriter = _resourceCopier = new GorgonResourceCopier(Graphics, this);
    }
}
