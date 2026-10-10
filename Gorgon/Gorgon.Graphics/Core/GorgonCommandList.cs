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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
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
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
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
/// A list of commands to send to the GPU.
/// </summary>
/// <remarks>
/// <para>
/// A command list records commands for the GPU, such as draws, clears and copies, and sends them to the GPU when it is submitted. Command lists are returned by <see cref="GorgonGraphics.GetCommandList"/>, 
/// recorded, and then passed to <see cref="GorgonGraphics.Submit(GorgonCommandList)"/> at the end of the frame. Once a command list is submitted, it stops recording (see <see cref="IsRecording"/>) and cannot 
/// be used again. A new command list is retrieved for the next frame.
/// </para>
/// <para>
/// The render targets, depth/stencil, viewports, scissor rectangles and constants set on a command list are used by every draw and execute call recorded after them, until they are changed. Most methods return 
/// the command list, so calls can be chained (e.g. <c>list.SetRenderTarget(target).SetViewport(viewport)</c>).
/// </para>
/// <para>
/// Gorgon adds the barriers needed by the commands automatically, based on the resources that each command uses. The 
/// <see cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)"/> and <see cref="Reset(ReadOnlySpan{IGorgonTextureView{GorgonTextureCommon}})"/> 
/// methods are available for cases that need explicit control.
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// A command list is <b>not</b> thread safe. Multiple command lists can be recorded on separate threads, as long as each command list is only used by one thread.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonDrawCall"/>
/// <seealso cref="GorgonExecuteCall"/>
public sealed unsafe class GorgonCommandList
    : IGorgonNamedObject, IGorgonCopyMethodsFluent<GorgonCommandList>
{
    private ComPtr<ID3D12GraphicsCommandList10> _list;
    private ComPtr<ID3D12CommandList> _baseList;

    private CommandAllocator? _commandAllocator;
    private string _name;
    private readonly GorgonResourceCopier _resourceCopier;
    private readonly IGorgonResourceWriter _resourceWriter;
    private readonly CpuBufferAllocation[] _constantWriteData = new CpuBufferAllocation[GorgonGraphics.MaxRootCbvCount];
    private D3D12_CPU_DESCRIPTOR_HANDLE _depthStencilView;
    private bool _depthStencilChanged;
    private readonly GorgonRenderTargetView[] _renderTargetViews = new GorgonRenderTargetView[GorgonVideoAdapterInfo.MaxRenderTargetCount];
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
    private ushort _dirtyRootConstants;

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
    /// Property to return the manager for the barriers used by this command list.
    /// </summary>
    internal BarrierManager BarrierManager
    {
        get;
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
    /// Property to return the render targets assigned to this command list.
    /// </summary>
    /// <seealso cref="SetRenderTargets(ReadOnlySpan{GorgonRenderTargetView}, GorgonDepthStencilView?)"/>
    public ReadOnlySpan<GorgonRenderTargetView> RenderTargets => _renderTargetViews.AsSpan(0, (int)_rtvsCount);

    /// <summary>
    /// Property to return the viewports assigned to this command list.
    /// </summary>
    /// <seealso cref="SetViewports(ReadOnlySpan{GorgonViewport})"/>
    public ReadOnlySpan<GorgonViewport> Viewports => _viewports.AsSpan(0, (int)_viewportCount);

    /// <summary>
    /// Property to return the scissor rectangles assigned to this command list.
    /// </summary>
    /// <seealso cref="SetScissorRectangles(ReadOnlySpan{GorgonRectangle})"/>
    public ReadOnlySpan<GorgonRectangle> ScissorRectangles => _scissors.AsSpan(0, (int)_scissorCount);

    /// <summary>
    /// Property to return whether the command list is recording commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A command list records commands from the time it is returned by <see cref="GorgonGraphics.GetCommandList"/> until it is submitted. When this value is <b>false</b>, the command list cannot be used.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    public bool IsRecording
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the depth/stencil view assigned to this command list.
    /// </summary>
    /// <seealso cref="SetRenderTargets(ReadOnlySpan{GorgonRenderTargetView}, GorgonDepthStencilView?)"/>
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

        if (indexBuffer.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(GorgonIndexBuffer), string.Format(Resources.GORGFX_ERR_BUFFER_DISPOSED, indexBuffer.Name));
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
        int dirty = _dirtyRootConstants;

        while (dirty != 0)
        {
            int index = BitOperations.TrailingZeroCount(dirty);
            ref CpuBufferAllocation allocation = ref _constantWriteData[index];

            _list.Get()->SetGraphicsRootConstantBufferView((uint)index, allocation.GpuAddress);

            dirty &= (dirty - 1);
        }

        _dirtyRootConstants = 0;
    }

    /// <summary>
    /// Function to set the barriers for textures on a draw call.
    /// </summary>
    /// <param name="textures">The textures on the draw call.</param>
    private void ApplyTextureBarriers(List<GorgonUsedTexture> textures)
    {
        if (textures.Count == 0)
        {
            return;
        }

        for (int i = 0; i < textures.Count; ++i)
        {
            (GorgonTextureCommon? texture, ShaderStage shader, TextureUsage usage) = textures[i];

            if (texture is null)
            {
                continue;
            }

            if (texture.IsDisposed)
            {
                throw new ObjectDisposedException(texture.GetType().Name, string.Format(Resources.GORGFX_ERR_BUFFER_DISPOSED, texture.Name));
            }

            Queue.Tracker.TrackResource(texture);

            BarrierSync sync = shader.ToSync();

            BarrierAccess access = usage switch
            {
                TextureUsage.ReadWrite => BarrierAccess.ReadWrite,
                _ => BarrierAccess.ShaderResource,
            };

            BarrierLayout layout = usage switch
            {
                TextureUsage.ReadWrite => BarrierLayout.ReadWrite,
                _ => BarrierLayout.ShaderResource,
            };

            SetBarrier(texture, sync, access, layout, textures[i].SubResources);
        }
    }

    /// <summary>
    /// Function to apply barriers to buffers used in a draw call.
    /// </summary>
    private void ApplyBufferBarriers(List<GorgonUsedBuffer> buffers)
    {
        // ExecuteIndirect does not use a shader stage, it's its own thing.
        static BarrierSync GetSyncForBuffer(BufferUsage usage, ShaderStage stage) => usage != BufferUsage.IndirectArguments ? stage.ToSync() : BarrierSync.ExecuteIndirect;
        static BarrierAccess GetAccessForBuffer(BufferUsage usage) => usage switch
        {
            BufferUsage.ConstantBuffer => BarrierAccess.ConstantBuffer,
            BufferUsage.ReadWrite => BarrierAccess.ReadWrite,
            BufferUsage.ReadOnlyAndReadWrite => BarrierAccess.ReadWrite | BarrierAccess.ShaderResource,
            BufferUsage.IndirectArguments => BarrierAccess.IndirectArgument,
            _ => BarrierAccess.ShaderResource,
        };

        // We need to validate that the buffers that are reading do not overlap their data regions with buffers that are writing.
        // Since we are using the MegaBuffer approach, this is a potential hazard. But, we also need to ensure that we're not trying to 
        // make a single non-UAV buffer (user facing, not mega buffer) with read access and write access simultaneously (e.g. SRV | COPY_DEST).
        static int Intersects(GorgonGpuBufferCommon buffer, int bufferIndex, BarrierSync sync, BarrierAccess access, IList<GorgonUsedBuffer> buffers)
        {
            ulong bufferStart = buffer.ResourceOffset;
            ulong bufferEnd = (ulong)buffer.SizeInBytes + buffer.ResourceOffset;

            for (int i = 0; i < buffers.Count; ++i)
            {
                if (bufferIndex == i)
                {
                    continue;
                }

                (GorgonGpuBufferCommon? otherBuffer, ShaderStage otherStage, BufferUsage otherUsage) = buffers[i];

                if (otherBuffer is null)
                {
                    continue;
                }

                BarrierSync otherSync = GetSyncForBuffer(otherUsage, otherStage);
                BarrierAccess otherAccess = GetAccessForBuffer(otherUsage);

                ulong otherEnd = (ulong)otherBuffer.SizeInBytes + otherBuffer.ResourceOffset;

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

        for (int i = 0; i < buffers.Count; ++i)
        {
            (GorgonGpuBufferCommon? buffer, ShaderStage stage, BufferUsage usage) = buffers[i];

            if (buffer is null)
            {
                continue;
            }

            BarrierSync sync = GetSyncForBuffer(usage, stage);
            BarrierAccess access = GetAccessForBuffer(usage);

            if (buffer.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(GorgonGpuBufferCommon), string.Format(Resources.GORGFX_ERR_BUFFER_DISPOSED, buffer.Name));
            }

            if (!buffer.IsMegaBufferResource)
            {
                // If this buffer is not from the mega buffer, then just set its barrier as-is.
                BarrierManager.AddBarrier(buffer, sync, access);
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

            BarrierManager.AddBarrier(buffer, megaSync, megaAccess);
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
        Debug.Assert(!pso.D3DPso.IsNull, $"The pipeline state object '{pso.Name}' no longer has a pointer to a native pipeline state object.");
        ObjectDisposedException.ThrowIf(pso.D3DPso.IsNull, pso);

        Queue.Tracker.TrackResource(pso.D3DPso);

        _list.Get()->SetPipelineState((PID3D12PipelineState)pso.D3DPso.Get());
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

        if (!depthStencil.FormatInfo.HasStencil)
        {
            flags &= ~D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_STENCIL;
        }

        // Nothing to clear.
        if (flags == 0)
        {
            return this;
        }

        Queue.Tracker.TrackResource(depthStencil.Texture);
        // Every plane of the view is covered: clearing a depth/stencil format writes the stencil plane too.
        GorgonSubResourceRange range = new(depthStencil.MipLevel, 1, depthStencil.ArrayIndex, depthStencil.ArrayCount, 0, Graphics.FormatSupport[depthStencil.Format].PlaneCount);

        SetBarrier(depthStencil.Texture, BarrierSync.DepthStencil, BarrierAccess.DepthStencilWrite, BarrierLayout.DepthStencilWrite, range, force: true);

        clearRects ??= [];

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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ValidateDrawCall(GorgonDrawCallCommon drawCall)
    {
        if (drawCall.Pso is null)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_DRAW_NEEDS_PSO, nameof(drawCall));
        }

        if (drawCall.InstanceCount < 1)
        {
            Graphics.Log.PrintWarning($"The draw call requires at least 1 instance value. The number of instances are {drawCall.InstanceCount}. Nothing will be drawn.", LoggingLevel.Simple);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Function to validate the state of an execute call before it is executed.
    /// </summary>
    /// <param name="executeCall">The execute call to validate.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the commands buffer is disposed.</exception>
    /// <exception cref="ArgumentException">Thrown if the range of commands to execute, or the counter index used for GPU rendering, is out of range.</exception>
    private static void ValidateExecuteCall(GorgonExecuteCallCommon executeCall)
    {
        if (executeCall.Commands.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(GorgonExecuteBuffer), string.Format(Resources.GORGFX_ERR_BUFFER_DISPOSED, executeCall.Commands.Name));
        }

        if ((executeCall.CommandExecutionStartIndex < 0) || (executeCall.CommandExecutionCount < 0)
            || ((executeCall.CommandExecutionStartIndex + executeCall.CommandExecutionCount) > executeCall.CommandCount))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_EXECUTE_RANGE_INVALID, executeCall.CommandExecutionStartIndex, executeCall.CommandExecutionCount, executeCall.CommandCount),
                                        nameof(executeCall));
        }

        if (executeCall.Usage != ExecuteUsage.GpuRendering)
        {
            return;
        }

        if ((executeCall.CounterIndex < 0) || (executeCall.CounterIndex >= executeCall.CounterCount))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_EXECUTE_COUNTER_INDEX_INVALID, executeCall.CounterIndex, executeCall.CounterCount), nameof(executeCall));
        }
    }

    /// <summary>
    /// Function to clear a buffer read/write view with the specified integer values.
    /// </summary>
    /// <param name="view">The buffer read/write view to clear.</param>
    /// <param name="values">The values to fill the view with.</param>
    /// <returns>The fluent interface for the command list.</returns>
    private GorgonCommandList ClearBufferReadWriteView(GorgonShaderBufferRwView view, Vector128<int> values)
    {
        SetBarrier(view.Buffer, BarrierSync.ClearReadWriteView, BarrierAccess.ReadWrite, force: true);

        Queue.Tracker.TrackResource(view.Buffer);

        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.Descriptors.GpuViewDescriptors.D3DGpuHandle;
        gpuHandle.Offset(view.GetViewHandle(), Graphics.Descriptors.GpuViewDescriptors.DescriptorSize);

        _list.Get()->ClearUnorderedAccessViewUint(gpuHandle, view.CpuAllocation.CpuHandle, (PID3D12Resource2)view.Buffer.D3DResource.Get(), (uint*)&values, 0, null);

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

        // Wait for the next frame to become available.
        Queue.WaitForFence(Queue.FrameFenceValue[currentFrame], Timeout.Infinite);

        // Signal any resources that are awaiting destruction.
        Queue.Tracker.Signal();

        // Any previous barriers on this command list should be voided.
        BarrierManager.Clear();

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
    /// Function to commit and finalize any pending barriers.
    /// </summary>
    internal void CommitBarriers()
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
            BarrierManager.Submit(this);
        }

        BarrierManager.CopyCurrentToGlobal(Queue.Type);
    }

    /// <summary>
    /// Function to close the list for recording.
    /// </summary>
    internal void Close()
    {
        _list.Get()->Close()
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_CANNOT_CLOSE_COMMAND_LIST, Name));

        IsRecording = false;
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
        BarrierManager.Clear();
        _currentIndexBuffer = null;
        _dirtyRootConstants = 0;
    }

    /// <summary>
    /// Function to release any disposable objects or native objects.
    /// </summary>
    internal void Dispose()
    {
        BarrierManager.Clear();
        _resourceCopier.Dispose();

        Graphics.Log.Print($"Destroying {nameof(GorgonCommandList)} '{Name}'...", LoggingLevel.Intermediate);
        Graphics.Log.Print($"Destroying D3D 12 {Queue.Type} '{Name}'", LoggingLevel.Verbose);

        _baseList.Dispose();
        _list.Dispose();
    }

    /// <summary>
    /// Function to resolve a multisampled texture into a single-sample texture.
    /// </summary>
    /// <param name="msaaTexture">The multisampled texture to read the sample data from.</param>
    /// <param name="output">The single-sample texture that will receive the resolved data.</param>
    /// <param name="sourceArrayIndex">[Optional] The array index on the multisampled texture to read from.</param>
    /// <param name="destinationArrayIndex">[Optional] The array index on the output texture to write into.</param>
    /// <param name="destinationMipLevel">[Optional] The mip level on the output texture to write into.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para>Thrown if the <paramref name="msaaTexture"/> is not multisampled.</para>
    /// <para>Thrown if the <paramref name="output"/> texture is multisampled.</para>
    /// <para>Thrown if the <paramref name="msaaTexture"/> and <paramref name="output"/> textures do not have the same format.</para>
    /// <para>Thrown if the format of the <paramref name="msaaTexture"/> is not supported for resolving (e.g. if the format is typeless).</para>
    /// <para>Thrown if the <paramref name="output"/> texture is not a 2D texture.</para>
    /// <para>Thrown if the sub resources being resolved do not have the same width and height.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// A multisampled texture stores several samples per pixel, and because of this it cannot be filtered when read by a shader. This method averages those samples down into a single value per pixel and 
    /// writes the result into the <paramref name="output"/> texture, which can then be used like any other texture. 
    /// </para>
    /// <para>
    /// Both textures must use the same format, and that format must be resolvable by the video adapter. This can be checked ahead of time with the 
    /// <see cref="GorgonBufferFormatSupport.CanResolveMultisample"/> property for the format.
    /// </para>
    /// <para>
    /// A single sub resource is resolved per call. The <paramref name="sourceArrayIndex"/> parameter selects the array index to read from, and the <paramref name="destinationArrayIndex"/> and 
    /// <paramref name="destinationMipLevel"/> parameters select the sub resource to write into. These values are clipped to the range available on their respective textures, so out of range values will not 
    /// cause an error. The sub resources must have the same width and height because no scaling or filtering is performed during the resolve. The data is always read from the first mip level of the 
    /// <paramref name="msaaTexture"/>, because a multisampled texture only has one mip level.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonMultisampleInfo"/>
    /// <seealso cref="GorgonBufferFormatSupport"/>
    public GorgonCommandList ResolveMsaaTexture(GorgonTextureCommon msaaTexture, GorgonTextureCommon output, short sourceArrayIndex = 0, short destinationArrayIndex = 0, short destinationMipLevel = 0)
    {
        if (msaaTexture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_MSAA_NOT_MULTISAMPLED, msaaTexture.Name));
        }

        if (!output.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_MSAA_TEXTURE_NOT_SUPPORTED, output.Name));
        }

        if (msaaTexture.Format != output.Format)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_MSAA_TEXTURE_FORMATS_NOT_SAME, msaaTexture.Name, msaaTexture.Format, output.Name, output.Format));
        }

        if (!Graphics.FormatSupport[msaaTexture.Format].CanResolveMultisample)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_MSAA_TEXTURE_FORMAT_NOT_RESOLVABLE, msaaTexture.Name, msaaTexture.Format));
        }

        if (msaaTexture.FormatInfo.IsTypeless)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_FORMAT_INVALID, msaaTexture.Format));
        }

        if (output.Type != TextureType.Texture2D)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_MSAA_DEST_NOT_2D, output.Name));
        }

        int destSubResWidth = output.GetMipWidth(destinationMipLevel);
        int destSubResHeight = output.GetMipHeight(destinationMipLevel);

        if ((msaaTexture.Width != destSubResWidth) || (msaaTexture.Height != destSubResHeight))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_MSAA_RESOLVE_SIZE_MISMATCH, msaaTexture.Name, msaaTexture.Width, msaaTexture.Height, output.Name, destSubResWidth, destSubResHeight));
        }

        // The GetSubResourceIndex method clips the input values to their minimums and maximums (minus one).
        // So we don't need to do that here.
        int sourceSubResourceIndex = msaaTexture.GetSubResourceIndex(0, sourceArrayIndex);
        int destSubResourceIndex = output.GetSubResourceIndex(destinationMipLevel, destinationArrayIndex);

        SetBarrier(msaaTexture, BarrierSync.Resolve, BarrierAccess.ResolveSource, BarrierLayout.ResolveSource);
        SetBarrier(output, BarrierSync.Resolve, BarrierAccess.ResolveDestination, BarrierLayout.ResolveDestination, force: true);

        _list.Get()->ResolveSubresource((PID3D12Resource2)output.D3DResource.Get(), (uint)destSubResourceIndex, (PID3D12Resource2)msaaTexture.D3DResource.Get(), (uint)sourceSubResourceIndex, (DXGI_FORMAT)msaaTexture.Format);

        return this;
    }

    /// <summary>
    /// Function to draw primitives using the vertices described by a draw call.
    /// </summary>
    /// <param name="drawCall">The draw call that describes what to draw, and the state used to draw it.</param>
    /// <remarks>
    /// <para type="draw">
    /// The draw uses the pipeline state object, resources and dynamic pipeline values on the <paramref name="drawCall"/>, along with the render targets, depth/stencil, viewports, scissor rectangles and 
    /// constants set on this command list. Gorgon adds the barriers needed for the resources assigned to the draw call (see <see cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})"/> 
    /// and <see cref="GorgonGraphicsCallCommon.AssignTextures(ReadOnlySpan{GorgonUsedTexture})"/>) before the draw.
    /// </para>
    /// <para type="draw">
    /// Gorgon does not bind vertex buffers. The vertex shader reads its vertex data from buffers through their view handles, using the 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_VertexID</a> and 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_InstanceID</a> system values to find the data for the vertex and 
    /// instance being processed.
    /// </para>
    /// <para>
    /// This draws <see cref="GorgonDrawCall.VertexCount"/> vertices for each of the <see cref="GorgonDrawCallCommon.InstanceCount"/> instances. The 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_VertexID</a> and 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_InstanceID</a> values in the vertex shader start at 0, and do not 
    /// include the <see cref="GorgonDrawCall.StartVertex"/> or the <see cref="GorgonDrawCallCommon.StartInstance"/>. A vertex shader that needs those values reads them through the 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartVertexLocation</a> and 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartInstanceLocation</a> system values (shader model 6.8, see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsExtendedCommandInfo"/>), or receives them through a constant. Negative start values are treated as 0.
    /// </para>
    /// <para>
    /// If the vertex count or the instance count is less than 1, nothing is drawn, and a warning is written to the log.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="Draw(GorgonIndexedDrawCall)"/>
    public void Draw(GorgonDrawCall drawCall)
    {
        if (!ValidateDrawCall(drawCall))
        {
            return;
        }

        if (drawCall.VertexCount < 1)
        {
            Graphics.Log.PrintWarning($"The draw call requires at least 1 vertex in its vertex count. The number of vertices are {drawCall.VertexCount}. Nothing will be drawn.", LoggingLevel.Simple);
            return;
        }

        // Apply pending barrier calls.
        ApplyBufferBarriers(drawCall.UsedBuffers);
        ApplyTextureBarriers(drawCall.UsedTextures);
        ApplyConstantWrites();
        ApplyRenderTargets();
        ApplyPso(drawCall.Pso);
        ApplyViewSetup();

        BarrierManager.Submit(this);

        _list.Get()->DrawInstanced((uint)drawCall.VertexCount, (uint)drawCall.InstanceCount, (uint)drawCall.StartVertex.Max(0), (uint)drawCall.StartInstance.Max(0));
    }

    /// <summary>
    /// Function to draw primitives using the indices described by an indexed draw call.
    /// </summary>
    /// <param name="drawCall">The indexed draw call that describes what to draw, and the state used to draw it.</param>
    /// <remarks>
    /// <inheritdoc cref="Draw(GorgonDrawCall)" path="/remarks/para[@type='draw']"/>
    /// <para>
    /// This draws <see cref="GorgonIndexedDrawCall.IndexCount"/> indices from the <see cref="GorgonIndexedDrawCall.IndexBuffer"/>, starting at the <see cref="GorgonIndexedDrawCall.StartIndex"/>, for each of 
    /// the <see cref="GorgonDrawCallCommon.InstanceCount"/> instances. The 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_VertexID</a> value in the vertex shader is the index read from the 
    /// index buffer, and does not include the <see cref="GorgonIndexedDrawCall.BaseVertex"/>. The 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_InstanceID</a> value starts at 0, and does not include the 
    /// <see cref="GorgonDrawCallCommon.StartInstance"/>. A vertex shader that needs the base vertex or the start instance reads them through the 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartVertexLocation</a> and 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartInstanceLocation</a> system values (shader model 6.8, see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsExtendedCommandInfo"/>), or receives them through a constant. Negative start index and start instance values are treated as 0.
    /// </para>
    /// <para>
    /// If the instance count is less than 1, nothing is drawn, and a warning is written to the log.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonIndexedDrawCall"/>
    /// <seealso cref="Draw(GorgonDrawCall)"/>
    public void Draw(GorgonIndexedDrawCall drawCall)
    {
        if (!ValidateDrawCall(drawCall))
        {
            return;
        }

        if (drawCall.IndexBuffer is null)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_DRAW_INDEXED_NEEDS_INDEXBUFFER, nameof(drawCall));
        }

        if (drawCall.InstanceCount < 1)
        {
            Graphics.Log.PrintWarning($"The draw indexed call requires at least 1 instance value. The number of instances are {drawCall.InstanceCount}. Nothing will be drawn.", LoggingLevel.Simple);
            return;
        }

        // Apply pending barrier calls.
        ApplyBufferBarriers(drawCall.UsedBuffers);
        ApplyTextureBarriers(drawCall.UsedTextures);
        ApplyIndexBuffer(drawCall.IndexBuffer);
        ApplyConstantWrites();
        ApplyRenderTargets();
        ApplyPso(drawCall.Pso);
        ApplyViewSetup();

        BarrierManager.Submit(this);

        _list.Get()->DrawIndexedInstanced((uint)drawCall.IndexCount, (uint)drawCall.InstanceCount, (uint)drawCall.StartIndex.Max(0), drawCall.BaseVertex, (uint)drawCall.StartInstance.Max(0));
    }

    /// <summary>
    /// Function to execute the commands in an execute call.
    /// </summary>
    /// <param name="executeCall">The execute call containing the commands, and the state used to execute them.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the <see cref="GorgonExecuteCallCommon.Commands"/> buffer is disposed.</exception>
    /// <exception cref="ArgumentException"><para>
    /// Thrown if the range of commands set by the <see cref="GorgonExecuteCallCommon.CommandExecutionStartIndex"/> and <see cref="GorgonExecuteCallCommon.CommandExecutionCount"/> is outside of the commands in 
    /// the <see cref="GorgonExecuteCallCommon.Commands"/> buffer.
    /// </para>
    /// <para>
    /// Thrown if the <see cref="GorgonExecuteCallCommon.Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>, and the <see cref="GorgonExecuteCallCommon.CounterIndex"/> is outside of the counters in the 
    /// <see cref="GorgonExecuteCallCommon.Commands"/> buffer.
    /// </para>
    /// </exception>
    /// <remarks>
    /// <para type="execute">
    /// This runs the <see cref="GorgonExecuteCallCommon.CommandExecutionCount"/> commands in the <see cref="GorgonExecuteCallCommon.Commands"/> buffer, starting at the 
    /// <see cref="GorgonExecuteCallCommon.CommandExecutionStartIndex"/>. When the <see cref="GorgonExecuteCallCommon.Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>, the number of commands run is the 
    /// lesser of the <see cref="GorgonExecuteCallCommon.CommandExecutionCount"/>, and the value of the counter selected by the <see cref="GorgonExecuteCallCommon.CounterIndex"/>.
    /// </para>
    /// <para type="execute">
    /// Like a draw call, every command uses the state on the execute call, along with the render targets, viewports, scissor rectangles and constants set on this command list.
    /// </para>
    /// <para>
    /// Every command in the buffer must contain its root constants, followed by all of the draw arguments. See <see cref="GorgonExecuteCall"/> for the layout of a command, and examples of writing 
    /// commands on the CPU and on the GPU.
    /// </para>
    /// <para type="execute">
    /// <note type="important">
    /// <para>
    /// Once the call is executed, the root constants set by its commands are reset to 0. Draws recorded after the call that read those constants will receive 0.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonExecuteCall"/>
    /// <seealso cref="GorgonExecuteBuffer"/>
    public void Execute(GorgonExecuteCall executeCall)
    {
        ValidateExecuteCall(executeCall);

        ID3D12Resource* countBuffer = null;
        ulong countOffset = 0;

        if (executeCall.Usage == ExecuteUsage.GpuRendering)
        {
            countBuffer = (PID3D12Resource2)executeCall.Commands.D3DResource.Get();
            countOffset = executeCall.Commands.CountersOffset + ((ulong)executeCall.CounterIndex * sizeof(uint));
        }

        Queue.Tracker.TrackResource(executeCall.Commands);
        SetBarrier(executeCall.Commands, BarrierSync.ExecuteIndirect, BarrierAccess.IndirectArgument);

        // Apply pending barrier calls.
        ApplyBufferBarriers(executeCall.UsedBuffers);
        ApplyTextureBarriers(executeCall.UsedTextures);
        ApplyConstantWrites();
        ApplyRenderTargets();
        ApplyPso(executeCall.Pso);
        ApplyViewSetup();        

        BarrierManager.Submit(this);

        _list.Get()->ExecuteIndirect(executeCall.D3DCommandSignature.Get(), (uint)executeCall.CommandExecutionCount, (PID3D12Resource2)executeCall.Commands.D3DResource.Get(), 
                                    ((ulong)executeCall.CommandExecutionStartIndex * (uint)executeCall.CommandSizeInBytes) + executeCall.Commands.ResourceOffset, 
                                    countBuffer, countOffset);
    }

    /// <summary>
    /// Function to execute the commands in an indexed execute call.
    /// </summary>
    /// <param name="executeCall">The indexed execute call containing the commands, and the state used to execute them.</param>
    /// <inheritdoc cref="Execute(GorgonExecuteCall)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="Execute(GorgonExecuteCall)" path="/remarks/para[@type='execute']"/>
    /// <para>
    /// Every command in the buffer must contain its root constants, followed by all of the indexed draw arguments. See <see cref="GorgonIndexedExecuteCall"/> for the layout of a command, and 
    /// <see cref="GorgonExecuteCall"/> for examples of writing commands on the CPU and on the GPU. Every command reads its indices from the <see cref="GorgonIndexedExecuteCall.IndexBuffer"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonIndexedExecuteCall"/>
    /// <seealso cref="GorgonExecuteBuffer"/>
    public void Execute(GorgonIndexedExecuteCall executeCall)
    {
        ValidateExecuteCall(executeCall);

        if (executeCall.IndexBuffer is null)
        {
            throw new ArgumentException(Resources.GORGFX_ERR_DRAW_INDEXED_NEEDS_INDEXBUFFER, nameof(executeCall));
        }

        ID3D12Resource* countBuffer = null;
        ulong countOffset = 0;

        if (executeCall.Usage == ExecuteUsage.GpuRendering)
        {
            countBuffer = (PID3D12Resource2)executeCall.Commands.D3DResource.Get();
            countOffset = executeCall.Commands.CountersOffset + ((ulong)executeCall.CounterIndex * sizeof(uint));
        }

        Queue.Tracker.TrackResource(executeCall.Commands);
        SetBarrier(executeCall.Commands, BarrierSync.ExecuteIndirect, BarrierAccess.IndirectArgument);

        // Apply pending barrier calls.
        ApplyBufferBarriers(executeCall.UsedBuffers);
        ApplyTextureBarriers(executeCall.UsedTextures);
        ApplyIndexBuffer(executeCall.IndexBuffer);
        ApplyConstantWrites();
        ApplyRenderTargets();
        ApplyPso(executeCall.Pso);
        ApplyViewSetup();

        BarrierManager.Submit(this);

        _list.Get()->ExecuteIndirect(executeCall.D3DCommandSignature.Get(), (uint)executeCall.CommandExecutionCount, (PID3D12Resource2)executeCall.Commands.D3DResource.Get(),
                                    ((ulong)executeCall.CommandExecutionStartIndex * (uint)executeCall.CommandSizeInBytes) + executeCall.Commands.ResourceOffset,
                                    countBuffer, countOffset);
    }

    /// <summary>
    /// Function to add a swap chain to present when this command list is submitted.
    /// </summary>
    /// <param name="swapChain">The swap chain to present.</param>
    /// <param name="interval">[Optional] The number of vertical blank periods to wait for before presenting.</param>
    /// <returns>The command list as a fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// When the command list is submitted with <see cref="GorgonGraphics.Submit(GorgonCommandList)"/>, each swap chain added with this method is presented, which shows the image rendered into its back buffer 
    /// on the display.
    /// </para>
    /// <para>
    /// The <paramref name="interval"/> synchronizes the presentation with the vertical blank of the display. A value of 0 presents immediately (when the swap chain is windowed and the video adapter supports 
    /// it, tearing is allowed), and a value of 1 to 4 waits for at least that many vertical blank periods. Values outside of 0 to 4 are clipped to that range.
    /// </para>
    /// <para>
    /// Adding a swap chain that has already been added to this command list only changes its <paramref name="interval"/>.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="interval"/> parameter is 0.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonSwapChain"/>
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
    /// Function to clear the back buffer of a swap chain with a specified color.
    /// </summary>
    /// <param name="swapChain">The swap chain to clear.</param>
    /// <param name="color">The color to fill the swap chain back buffer with.</param>
    /// <param name="clearRects">[Optional] Defines a list of regions to clear on the back buffer.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This clears the render target view returned by the <see cref="GorgonSwapChain.Target"/> property of the <paramref name="swapChain"/>.
    /// </para>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='texture_rects']"/>
    /// <para>
    /// The rectangles are in pixels of the back buffer.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonSwapChain"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearRenderTarget(GorgonSwapChain swapChain, GorgonColor color, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        Queue.Tracker.TrackResource(swapChain.DXGISwapChain);
        ClearRenderTarget(swapChain.Target, color, clearRects);
        return this;
    }

    /// <summary>
    /// Function to clear a render target view with a specified color.
    /// </summary>
    /// <param name="renderTarget">The render target view to clear.</param>
    /// <param name="color">The color to fill the render target with.</param>
    /// <param name="clearRects">[Optional] Defines a list of regions to clear on the render target.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para>
    /// Only the mip level and array indices covered by the <paramref name="renderTarget"/> view are cleared. The <paramref name="color"/> is converted to the format of the view.
    /// </para>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='texture_rects']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='rect_coordinates']"/>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRenderTargetView"/>
    public GorgonCommandList ClearRenderTarget(GorgonRenderTargetView renderTarget, GorgonColor color, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        GorgonSubResourceRange range = new(renderTarget.MipLevel, 1, renderTarget.ArrayIndex, renderTarget.ArrayCount, renderTarget.PlaneIndex, 1);

        SetBarrier(renderTarget.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, range, force: true);

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
    /// Function to clear the depth and stencil values of a depth/stencil view.
    /// </summary>
    /// <param name="depthStencil">The depth/stencil view to clear.</param>
    /// <param name="depthValue">The depth value to write.</param>
    /// <param name="stencilValue">The stencil value to write.</param>
    /// <param name="clearRects">[Optional] Defines a list of regions to clear on the depth/stencil view.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para type="ds_range">
    /// Only the mip level and array indices covered by the <paramref name="depthStencil"/> view are cleared.
    /// </para>
    /// <para type="depth_range">
    /// The depth value is clamped to the range of 0 to 1.
    /// </para>
    /// <para>
    /// If the format of the view does not have a stencil component, only the depth values are cleared, and the <paramref name="stencilValue"/> is ignored.
    /// </para>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='texture_rects']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='rect_coordinates']"/>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDepthStencilView"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearDepthStencil(GorgonDepthStencilView depthStencil, float depthValue, byte stencilValue, IReadOnlyList<GorgonRectangle>? clearRects = null) =>
        ClearDepthStencil(depthStencil, depthValue, stencilValue, clearRects, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH | D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_STENCIL);

    /// <summary>
    /// Function to clear the depth values of a depth/stencil view, without changing the stencil values.
    /// </summary>
    /// <param name="depthStencil"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='depthStencil']"/></param>
    /// <param name="depthValue"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='depthValue']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='ds_range']"/>
    /// <inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='depth_range']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='texture_rects']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='rect_coordinates']"/>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDepthStencilView"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearDepth(GorgonDepthStencilView depthStencil, float depthValue, IReadOnlyList<GorgonRectangle>? clearRects = null) =>
        ClearDepthStencil(depthStencil, depthValue, 0, clearRects, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH);

    /// <summary>
    /// Function to clear the stencil values of a depth/stencil view, without changing the depth values.
    /// </summary>
    /// <param name="depthStencil"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='depthStencil']"/></param>
    /// <param name="stencilValue"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='stencilValue']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="ClearDepthStencil(GorgonDepthStencilView, float, byte, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='ds_range']"/>
    /// <para>
    /// If the format of the view does not have a stencil component, nothing is cleared.
    /// </para>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='texture_rects']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='rect_coordinates']"/>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDepthStencilView"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearStencil(GorgonDepthStencilView depthStencil, byte stencilValue, IReadOnlyList<GorgonRectangle>? clearRects = null) =>
        ClearDepthStencil(depthStencil, 1.0f, stencilValue, clearRects, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_STENCIL);

    /// <summary>
    /// Function to clear a raw buffer read/write view with the specified integer values.
    /// </summary>
    /// <param name="view">The raw buffer read/write view to clear.</param>
    /// <param name="values">The values to fill the view with, one per component of the view format.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para type="buffer_clear">
    /// The integer values are written as their 32-bit patterns, so negative values are stored as their unsigned (two's complement) equivalents.
    /// </para>
    /// <para type="raw_float">
    /// Raw views can only be cleared with integer values. To fill a raw view with a floating point value, pass its bit pattern (e.g. <see cref="BitConverter.SingleToInt32Bits(float)"/>).
    /// </para>
    /// <para type="structured">
    /// Structured read/write views (<see cref="GorgonStructuredBufferRwView"/>) cannot be cleared. To clear a structured buffer, clear a <see cref="GorgonRawBufferRwView"/> or 
    /// <see cref="GorgonTypedBufferRwView"/> of the same buffer.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearReadWriteView(GorgonRawBufferRwView view, Vector128<int> values) => ClearBufferReadWriteView(view, values);

    /// <summary>
    /// Function to clear a typed buffer read/write view with the specified integer values.
    /// </summary>
    /// <param name="view">The typed buffer read/write view to clear.</param>
    /// <param name="values"><inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/param[@name='values']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/remarks"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList ClearReadWriteView(GorgonTypedBufferRwView view, Vector128<int> values) => ClearBufferReadWriteView(view, values);

    /// <summary>
    /// Function to clear a typed buffer read/write view with the specified floating point values.
    /// </summary>
    /// <param name="view"><inheritdoc cref="ClearReadWriteView(GorgonTypedBufferRwView, Vector128{int})" path="/param[@name='view']"/></param>
    /// <param name="values"><inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/param[@name='values']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para type="float_conversion">
    /// The values are converted to the format of the view. For integer formats, the values are converted to whole numbers (e.g. 1.5 is stored as 1). Use the integer overload to write exact bit patterns.
    /// </para>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/remarks/para[@type='structured']"/>
    /// </remarks>
    public GorgonCommandList ClearReadWriteView(GorgonTypedBufferRwView view, Vector4 values)
    {
        SetBarrier(view.Buffer, BarrierSync.ClearReadWriteView, BarrierAccess.ReadWrite, force: true);

        Queue.Tracker.TrackResource(view.Buffer);

        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.Descriptors.GpuViewDescriptors.D3DGpuHandle;
        gpuHandle.Offset(view.GetViewHandle(), Graphics.Descriptors.GpuViewDescriptors.DescriptorSize);

        _list.Get()->ClearUnorderedAccessViewFloat(gpuHandle, view.CpuAllocation.CpuHandle, (PID3D12Resource2)view.Buffer.D3DResource.Get(), (float*)&values, 0, null);

        return this;
    }

    /// <summary>
    /// Function to clear a texture read/write view with the specified integer values.
    /// </summary>
    /// <param name="view">The texture read/write view to clear.</param>
    /// <param name="values"><inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/param[@name='values']"/></param>
    /// <param name="clearRects">[Optional] Defines a list of regions to clear on the texture.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/remarks/para[@type='buffer_clear']"/>
    /// <para type="texture_rects">
    /// If the <paramref name="clearRects"/> parameter is <b>null</b>, or empty, then the entire view is cleared.
    /// </para>
    /// <para type="rect_coordinates">
    /// The rectangles are in pixels of the mip level being viewed. When the view covers more than one array index, the rectangles are cleared on every array index in the view.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    public GorgonCommandList ClearReadWriteView(GorgonTextureRwView view, Vector128<int> values, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        GorgonSubResourceRange range = new(view.MipLevel, 1, view.ArrayIndex, view.ArrayCount, view.PlaneIndex, 1);

        SetBarrier(view.Texture, BarrierSync.ClearReadWriteView, BarrierAccess.ReadWrite, BarrierLayout.ReadWrite, range, force: true);

        clearRects ??= [];

        Queue.Tracker.TrackResource(view.Texture);

        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.Descriptors.GpuViewDescriptors.D3DGpuHandle;
        gpuHandle.Offset(view.GetViewHandle(), Graphics.Descriptors.GpuViewDescriptors.DescriptorSize);

        if (clearRects.Count == 0)
        {
            _list.Get()->ClearUnorderedAccessViewUint(gpuHandle, view.CpuAllocation.CpuHandle, (PID3D12Resource2)view.Texture.D3DResource.Get(), (uint*)(&values), 0, null);
            return this;
        }

        RECT* rects = stackalloc RECT[clearRects.Count];

        for (int i = 0; i < clearRects.Count; ++i)
        {
            GorgonRectangle rect = clearRects[i];
            rects[i] = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        _list.Get()->ClearUnorderedAccessViewUint(gpuHandle, view.CpuAllocation.CpuHandle, (PID3D12Resource2)view.Texture.D3DResource.Get(), (uint*)(&values), (uint)clearRects.Count, rects);

        return this;
    }

    /// <summary>
    /// Function to clear a texture read/write view with the specified floating point values.
    /// </summary>
    /// <param name="view"><inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='view']"/></param>
    /// <param name="values"><inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/param[@name='values']"/></param>
    /// <param name="clearRects"><inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/param[@name='clearRects']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTypedBufferRwView, Vector4)" path="/remarks/para[@type='float_conversion']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='texture_rects']"/>
    /// <inheritdoc cref="ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)" path="/remarks/para[@type='rect_coordinates']"/>
    /// <para>
    /// The default value for the <paramref name="clearRects"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    public GorgonCommandList ClearReadWriteView(GorgonTextureRwView view, Vector4 values, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        GorgonSubResourceRange range = new(view.MipLevel, 1, view.ArrayIndex, view.ArrayCount, view.PlaneIndex, 1);

        SetBarrier(view.Texture, BarrierSync.ClearReadWriteView, BarrierAccess.ReadWrite, BarrierLayout.ReadWrite, range, force: true);

        clearRects ??= [];

        Queue.Tracker.TrackResource(view.Texture);

        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.Descriptors.GpuViewDescriptors.D3DGpuHandle;
        gpuHandle.Offset(view.GetViewHandle(), Graphics.Descriptors.GpuViewDescriptors.DescriptorSize);

        if (clearRects.Count == 0)
        {
            _list.Get()->ClearUnorderedAccessViewFloat(gpuHandle, view.CpuAllocation.CpuHandle, (PID3D12Resource2)view.Texture.D3DResource.Get(), (float*)(&values), 0, null);
            return this;
        }

        RECT* rects = stackalloc RECT[clearRects.Count];

        for (int i = 0; i < clearRects.Count; ++i)
        {
            GorgonRectangle rect = clearRects[i];
            rects[i] = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        _list.Get()->ClearUnorderedAccessViewFloat(gpuHandle, view.CpuAllocation.CpuHandle, (PID3D12Resource2)view.Texture.D3DResource.Get(), (float*)(&values), (uint)clearRects.Count, rects);

        return this;
    }

    /// <summary>
    /// Function to reset the usage for the specified textures.
    /// </summary>
    /// <param name="textures">The list of textures that need their usage state reset.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/>
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
    /// The whole texture behind each view is reset, not only the sub resources that the view covers.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Any texture that has been reset cannot be used again by any command list passed to the same <see cref="GorgonGraphics.Submit(ReadOnlySpan{GorgonCommandList})"/> call, including the command list
    /// that reset it. When the texture has been reset, we are telling the GPU that there is no intention to use the texture again until those command lists have been submitted. Use the texture in a
    /// command list for a later submit, or on the object it was reset for.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The reset operation <b>must</b> be called on a command list from the correct object. So, if the resource was last used by a command list from a <see cref="GorgonGraphics"/> object, then only a command 
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
    /// <typeparam name="T"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/typeparam[@name='Tv']"/></typeparam>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/remarks/para"/>
    /// <para type="list_staged">
    /// The source data is captured when this method is called, so it can be changed or released as soon as the method returns. The copy itself is recorded on this command list, and the GPU performs it when 
    /// the command list is submitted, in order with the other commands recorded on the command list.
    /// </para>
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
    /// GorgonCommandList list = _graphics.GetCommandList();
    ///
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
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/remarks/para[@type='list_staged']"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList CopyImageToTexture(IGorgonImage image, GorgonTexture texture)
    {
        _resourceWriter.CopyImageToTexture(image, texture);
        return this;
    }

    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/summary"/>
    /// <typeparam name="T"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/typeparam[@name='Tv']"/></typeparam>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)" path="/remarks/para"/>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/remarks/para[@type='list_staged']"/>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// <![CDATA[
    /// using GorgonNativeBuffer<byte> sourceData = new(1024);
    /// 
    /// // Code to write to the sourceData buffer goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// GorgonCommandList list = _graphics.GetCommandList();
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
    /// <typeparam name="T"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/typeparam[@name='Tv']"/></typeparam>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/param"/>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)" path="/remarks/para"/>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/remarks/para[@type='list_staged']"/>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// <![CDATA[
    /// byte[] sourceData = new byte[1024];
    /// 
    /// // Code to write data to the sourceData array goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// GorgonCommandList list = _graphics.GetCommandList();
    ///
    /// list.CopyRange<byte>(sourceData, destBuffer);
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
    /// <para type="list_gpu">
    /// The copy is recorded on this command list, and the GPU performs it when the command list is submitted, in order with the other commands recorded on the command list.
    /// </para>
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
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/remarks/para[@type='list_staged']"/>
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
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/remarks/para[@type='list_staged']"/>
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
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/exception[@cref='T:System.ArgumentOutOfRangeException']/para"/></exception>
    /// <exception cref="ArgumentException"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/exception[@cref='T:System.ArgumentException']/para"/></exception>
    /// <exception cref="GorgonException"><inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[@type='common']"/></exception>
    /// <remarks>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{GorgonCommandList}.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
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
    /// <param name="subResources">[Optional] The sub resources on the texture to apply the barrier to, or <b>null</b> to apply it to the whole texture.</param>
    /// <param name="discard">[Optional] <b>true</b> to discard the current contents of the texture, <b>false</b> to keep them.</param>
    /// <param name="force">[Optional] <b>true</b> to apply the queued barriers right away, <b>false</b> to wait until they are needed.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para type="barrier_issue">Thrown if a resource was previously used in a command list on one of the Gorgon root objects (<see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/>, or 
    /// <see cref="GorgonComputeEngine"/>) and was left in a usage state that is incompatible with the root object that is currently trying to use the resource.</para>
    /// <para type="barrier_issue">Thrown if a resource was previously reset on a command list, and then used again before the command list was submitted.</para>
    /// </exception>
    /// <remarks>
    /// <para type="barrier">
    /// Most operations in Gorgon apply the barriers for their resources automatically. However, there may be times when an application needs a more optimal barrier strategy, or Gorgon cannot set the barriers 
    /// correctly. This method allows applications to set barriers themselves.
    /// </para>
    /// <para type="barrier">
    /// <note type="information">
    /// <para><h3>What is a barrier? What is it for?</h3></para>
    /// <para>
    /// Modern GPUs may require their resources to be in specific states (e.g. compressed, uncompressed, etc...) before they are used, and they also run operations in parallel. To prevent hazards for 
    /// read-after-write, write-after-read, and write-after-write operations, applications provide barriers to indicate that they intend to perform a certain action, requiring certain access types and data 
    /// layout. This allows the GPU to perform the operations required to ensure the data is synchronized.
    /// </para>
    /// <para>
    /// For more detailed information, please refer to the <a href="https://microsoft.github.io/DirectX-Specs/d3d/D3D12EnhancedBarriers.html" target="_blank">Direct3D 12 Enhanced Barriers specification</a>.
    /// </para>
    /// </note>
    /// </para>
    /// <para type="barrier">
    /// The synchronization bits in the <paramref name="sync"/> parameter indicate the work that must finish, or wait, around the barrier. These bits can be combined to provide multiple synchronization types.
    /// </para>
    /// <para type="barrier">
    /// The access bits in the <paramref name="access"/> parameter indicate how the resource will be accessed after the barrier. Since GPUs cache many of their write operations, this allows the GPU to ensure 
    /// that the caches are correctly flushed. These bits can be combined to provide multiple access types.
    /// </para>
    /// <para type="TextureBarrier">
    /// The <paramref name="layout"/> transition is for <see cref="GorgonTextureCommon"/> objects only. This value indicates the type of read/write operation being performed so the GPU can compress or 
    /// decompress its data (depending on the GPU architecture).
    /// </para>
    /// <para type="TextureBarrier">
    /// The barrier can be applied to sub resources of a <see cref="GorgonTextureCommon"/> by assigning a value to the <paramref name="subResources"/> parameter. If this value is omitted, then the barrier is 
    /// applied to the entire texture. Applying the barrier to a range of sub resources lets the GPU work on another portion of the texture while working on the sub resources passed to this method.
    /// </para>
    /// <para type="TextureBarrier">
    /// When the <paramref name="discard"/> parameter is <b>true</b>, the contents of the texture are discarded. This is only available for the first barrier on a <see cref="GorgonTextureCommon"/> resource.
    /// </para>
    /// <para type="barrier">
    /// Barriers are queued, and applied right before the next operation on the command list that needs them (e.g. a draw, clear or copy). When the <paramref name="force"/> parameter is <b>true</b>, the queued 
    /// barriers are applied immediately. Queuing the barriers until they are needed lets them be applied together, which is more efficient on some GPUs.
    /// </para>
    /// <para>
    /// The default values are <b>null</b> for the <paramref name="subResources"/>, <b>false</b> for the <paramref name="discard"/>, and <b>false</b> for the <paramref name="force"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="Reset(ReadOnlySpan{IGorgonTextureView{GorgonTextureCommon}})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList SetBarrier(GorgonTextureCommon texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool discard = false, bool force = false)
    {
        BarrierManager.AddBarrier(this, texture, sync, access, layout, subResources, discard);

        if ((force) && (!BarrierManager.IsEmpty))
        {
            BarrierManager.Submit(this);
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
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/remarks/para[@type='barrier']"/>
    /// <para>
    /// The default value for the <paramref name="force"/> parameter is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList SetBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access, bool force = false)
    {
        BarrierManager.AddBarrier(buffer, sync, access);

        if ((force) && (!BarrierManager.IsEmpty))
        {
            BarrierManager.Submit(this);
        }

        return this;
    }

    /// <summary>
    /// Function to write a constant value for shaders.
    /// </summary>
    /// <typeparam name="T">The type of data, must be an unmanaged value type.</typeparam>
    /// <param name="index">The constant slot to use.</param>
    /// <param name="data">The data to write to the constant slot.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> value is less than 0, or greater than or equal to the <see cref="GorgonGraphics.MaxRootCbvCount"/> 
    /// value.</exception>
    /// <remarks>
    /// <para>
    /// This writes constant data to one of the constant slots used by the shaders. The <paramref name="index"/> is the register of the slot in the shader, for example, index 0 is <c>ConstantBuffer&lt;T&gt; cb 
    /// : register(b0)</c>, index 1 is <c>ConstantBuffer&lt;T&gt; cb : register(b1)</c>, and so on, up to a maximum of <see cref="GorgonGraphics.MaxRootCbvCount"/> slots.
    /// </para>
    /// <para>
    /// The data is copied when this method is called, and is used by every draw and execute call recorded after it on this command list, until the slot is written again. The data is not kept between command 
    /// lists, so each command list that needs it must write it again. This makes the method well suited to data that changes often (e.g. every frame, or every draw). For constant data that rarely changes, use 
    /// a <see cref="GorgonConstantBufferView"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following example writes the camera and object data for a draw into constant slots 0 and 1.
    /// </para>
    /// The HLSL code:
    /// <code>
    /// <![CDATA[
    /// struct CameraData
    /// {
    ///     float4x4 ViewProjection;
    /// };
    ///
    /// struct ObjectData
    /// {
    ///     float4x4 World;
    ///     float4 Color;
    /// };
    ///
    /// ConstantBuffer<CameraData> _camera : register(b0);     // Slot 0.
    /// ConstantBuffer<ObjectData> _object : register(b1);     // Slot 1.
    /// ]]>
    /// </code>
    /// The C# code:
    /// <code language="csharp">
    /// <![CDATA[
    /// // These mirror the HLSL structures.
    /// [StructLayout(LayoutKind.Sequential)]
    /// struct CameraData
    /// {
    ///     public Matrix4x4 ViewProjection;
    /// }
    ///
    /// [StructLayout(LayoutKind.Sequential)]
    /// struct ObjectData
    /// {
    ///     public Matrix4x4 World;
    ///     public Vector4 Color;
    /// }
    ///
    /// GorgonCommandList list = graphics.GetCommandList();
    ///
    /// // The camera data is shared by every draw that follows.
    /// list.WriteConstant(0, new CameraData { ViewProjection = viewProjection });
    ///
    /// foreach (SceneObject sceneObject in sceneObjects)
    /// {
    ///     // The object data is written again before each draw, and each draw sees the data written before it.
    ///     list.WriteConstant(1, new ObjectData { World = sceneObject.World, Color = sceneObject.Color });
    ///     list.Draw(sceneObject.DrawCall);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGraphics.MaxRootCbvCount"/>
    /// <seealso cref="GorgonConstantBufferView"/>
    public GorgonCommandList WriteConstant<T>(int index, in T data)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, GorgonGraphics.MaxRootCbvCount);

        nuint typeSize = (nuint)Unsafe.SizeOf<T>();

        _uploadHeaps.Allocate(typeSize, D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, "Allocation for constant write is not valid.");

        Queue.Tracker.TrackResource(allocation.Heap.D3DResource);

        fixed (T* src = &data)
        {
            NativeMemory.Copy(src, allocation.CpuPointer, typeSize);
        }

        _constantWriteData[index] = allocation;

        // Record the dirty state.
        _dirtyRootConstants |= (ushort)(1 << index);

        return this;
    }

    /// <summary>
    /// Function to copy the data in a <see cref="GorgonGpuUploadMemory"/> into a buffer.
    /// </summary>
    /// <param name="upload">The transient GPU memory containing the data to copy.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="offset">[Optional] The offset, in bytes, within the <paramref name="buffer"/> to start writing into.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the size of the <paramref name="upload"/> memory, in bytes, exceeds the size of the 
    /// <paramref name="buffer"/>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="upload"/> memory is no longer available for use.</exception>
    /// <remarks>
    /// <para>
    /// Upload memory is written directly by the application (see <see cref="GorgonGpuUploadMemory(GorgonGraphics, long)"/>), so large amounts of data can be sent to a buffer without copying it into an intermediate array first. This 
    /// method copies the whole <paramref name="upload"/> memory into the <paramref name="buffer"/>. To copy a portion of it, pass a slice of the memory (see 
    /// <see cref="GorgonGpuUploadMemory.Slice(long, long?)"/>).
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
    /// <para>
    /// The default value for the <paramref name="offset"/> parameter is 0.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following example writes a large block of vertex data directly into upload memory, and then copies it into a buffer that the vertex shader reads through its view handle.
    /// </para>
    /// <code language="csharp">
    /// <![CDATA[
    /// GorgonCommandList list = graphics.GetCommandList();
    ///
    /// int vertexSize = Unsafe.SizeOf<Vertex>();
    /// GorgonGpuUploadMemory upload = new(graphics, vertexCount * vertexSize);
    ///
    /// // Generate the vertices straight into the upload memory, without building an array first.
    /// for (int i = 0; i < vertexCount; ++i)
    /// {
    ///     upload.Write(GenerateVertex(i), i * vertexSize);   // The offset is in bytes.
    /// }
    ///
    /// // Copy the upload memory into the vertex buffer. The copy runs on the GPU when the command list is submitted,
    /// // before the draws recorded after it.
    /// list.UploadGpuMemoryToBuffer(in upload, vertexBuffer);
    ///
    /// list.Draw(drawCall);
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuUploadMemory"/>
    /// <seealso cref="GorgonGpuUploadMemory(GorgonGraphics, long)"/>
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown if there are more than 16 <paramref name="viewports"/>.</exception>
    /// <remarks>
    /// <para type="viewport">
    /// A viewport maps the output of the vertex processing stages to an area of the render target, along with a depth range. The viewports are used by every draw and execute call recorded after this call, 
    /// until they are changed.
    /// </para>
    /// <para>
    /// This replaces all of the viewports assigned to the command list. Up to 16 viewports can be assigned, and a shader selects the viewport to use for a primitive with the 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_ViewportArrayIndex</a> system value. When a shader does not output 
    /// that value, the first viewport is used. An empty span removes all of the viewports.
    /// </para>
    /// </remarks>
    /// <seealso cref="Viewports"/>
    /// <seealso cref="GorgonViewport"/>
    public GorgonCommandList SetViewports(ReadOnlySpan<GorgonViewport> viewports)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(viewports.Length, _viewports.Length, nameof(viewports));

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
    /// <inheritdoc cref="SetViewports(ReadOnlySpan{GorgonViewport})" path="/remarks/para[@type='viewport']"/>
    /// <para>
    /// This replaces all of the viewports assigned to the command list with the <paramref name="viewport"/>. If the <paramref name="viewport"/> is empty, all of the viewports are removed.
    /// </para>
    /// </remarks>
    /// <seealso cref="Viewports"/>
    /// <seealso cref="GorgonViewport"/>
    public GorgonCommandList SetViewport(in GorgonViewport viewport)
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown if there are more than 16 <paramref name="scissors"/>.</exception>
    /// <remarks>
    /// <para type="scissor">
    /// A scissor rectangle clips rendering to an area, in pixels, of the render target. Pixels outside of the rectangle are discarded. Direct3D 12 has no setting to turn the scissor test off, so a scissor 
    /// rectangle that covers the area being rendered (usually the same area as the viewport) must be set. The scissor rectangles are used by every draw and execute call recorded after this call, until they 
    /// are changed.
    /// </para>
    /// <para>
    /// This replaces all of the scissor rectangles assigned to the command list. Up to 16 scissor rectangles can be assigned, and each scissor rectangle is used with the viewport at the same index (selected 
    /// with the <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_ViewportArrayIndex</a> system value). An empty span 
    /// removes all of the scissor rectangles.
    /// </para>
    /// </remarks>
    /// <seealso cref="ScissorRectangles"/>
    /// <seealso cref="SetViewports(ReadOnlySpan{GorgonViewport})"/>
    public GorgonCommandList SetScissorRectangles(ReadOnlySpan<GorgonRectangle> scissors)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(scissors.Length, _scissors.Length, nameof(scissors));

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
    /// <inheritdoc cref="SetScissorRectangles(ReadOnlySpan{GorgonRectangle})" path="/remarks/para[@type='scissor']"/>
    /// <para>
    /// This replaces all of the scissor rectangles assigned to the command list with the <paramref name="scissor"/> rectangle. If the <paramref name="scissor"/> rectangle is empty, all of the scissor 
    /// rectangles are removed.
    /// </para>
    /// </remarks>
    /// <seealso cref="ScissorRectangles"/>
    /// <seealso cref="SetViewport(in GorgonViewport)"/>
    public GorgonCommandList SetScissorRectangle(in GorgonRectangle scissor)
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
    /// Function to set the back buffer of a swap chain as the render target to use when rendering.
    /// </summary>
    /// <param name="swapChain">The swap chain to render into, or <b>null</b> to remove all of the render targets.</param>
    /// <param name="depthStencil"><inheritdoc cref="SetRenderTarget(GorgonRenderTargetView?, GorgonDepthStencilView?)" path="/param[@name='depthStencil']"/></param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This assigns the render target view returned by the <see cref="GorgonSwapChain.Target"/> property of the <paramref name="swapChain"/>, and works the same way as 
    /// <see cref="SetRenderTarget(GorgonRenderTargetView?, GorgonDepthStencilView?)"/>.
    /// </para>
    /// <inheritdoc cref="SetRenderTarget(GorgonRenderTargetView?, GorgonDepthStencilView?)" path="/remarks/para[@type='rt']"/>
    /// <para>
    /// The default value for the <paramref name="depthStencil"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonSwapChain"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList SetRenderTarget(GorgonSwapChain? swapChain, GorgonDepthStencilView? depthStencil = null) => SetRenderTarget(swapChain?.Target, depthStencil);

    /// <summary>
    /// Function to set a single render target view to use when rendering.
    /// </summary>
    /// <param name="renderTarget">The render target to assign, or <b>null</b> to remove all of the render targets.</param>
    /// <param name="depthStencil">[Optional] The depth/stencil view to assign, or <b>null</b> to remove the depth/stencil view.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This replaces all of the render targets assigned to the command list with the <paramref name="renderTarget"/>, which receives the 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target</a> output of the pixel shader.
    /// </para>
    /// <para type="rt">
    /// The render targets and the depth/stencil view are used by every draw and execute call recorded after this call, until they are changed. The formats of the views must match the output formats of the 
    /// pipeline state object used to draw (see <see cref="GorgonGraphicsPso.OutputFormats"/>).
    /// </para>
    /// <para>
    /// The default value for the <paramref name="depthStencil"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="RenderTargets"/>
    /// <seealso cref="DepthStencil"/>
    public GorgonCommandList SetRenderTarget(GorgonRenderTargetView? renderTarget, GorgonDepthStencilView? depthStencil = null)
    {
        if (_rtvsCount > 1)
        {
            Array.Clear(_d3dRtvs);
        }
        Array.Clear(_renderTargetViews, 0, (int)_rtvsCount);

        if (renderTarget is not null)
        {
            Queue.Tracker.TrackResource(renderTarget.Resource);

            GorgonSubResourceRange range = new(renderTarget.MipLevel, 1, renderTarget.ArrayIndex, renderTarget.ArrayCount, renderTarget.PlaneIndex, 1);
            SetBarrier(renderTarget.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, range);
        }

        if (renderTarget is not null)
        {
            _renderTargetViews[0] = renderTarget;
        }
        _d3dRtvs[0] = renderTarget?.GetCpuHandle() ?? D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
        _rtvsCount = renderTarget is null ? 0 : 1u;
        _rtvsChanged = true;

        if (depthStencil is not null)
        {
            Queue.Tracker.TrackResource(depthStencil.Resource);

            GorgonSubResourceRange range = new(depthStencil.MipLevel, 1, depthStencil.ArrayIndex, depthStencil.ArrayCount, 0, Graphics.FormatSupport[depthStencil.Format].PlaneCount);
            SetBarrier(depthStencil.Texture, BarrierSync.DepthStencil, BarrierAccess.DepthStencilWrite, BarrierLayout.DepthStencilWrite, range);
        }

        DepthStencil = depthStencil;
        _depthStencilView = depthStencil?.GetCpuHandle() ?? D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
        _depthStencilChanged = true;

        return this;
    }

    /// <summary>
    /// Function to set the render target views to use when rendering.
    /// </summary>
    /// <param name="renderTargets">The render targets to assign.</param>
    /// <param name="depthStencil">[Optional] The depth/stencil view to assign, or <b>null</b> to remove the depth/stencil view.</param>
    /// <inheritdoc cref="AddPresenter" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if there are more <paramref name="renderTargets"/> than the <see cref="GorgonVideoAdapterInfo.MaxRenderTargetCount"/>.</exception>
    /// <remarks>
    /// <para>
    /// This replaces all of the render targets assigned to the command list. The render target at each index receives the pixel shader output with the matching 
    /// <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_Target</a> index (e.g. index 1 receives <c>SV_Target1</c>). An 
    /// empty span removes all of the render targets.
    /// </para>
    /// <inheritdoc cref="SetRenderTarget(GorgonRenderTargetView?, GorgonDepthStencilView?)" path="/remarks/para[@type='rt']"/>
    /// <para>
    /// The default value for the <paramref name="depthStencil"/> parameter is <b>null</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="RenderTargets"/>
    /// <seealso cref="DepthStencil"/>
    public GorgonCommandList SetRenderTargets(ReadOnlySpan<GorgonRenderTargetView> renderTargets, GorgonDepthStencilView? depthStencil = null)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(renderTargets.Length, _d3dRtvs.Length, nameof(renderTargets));

        Array.Clear(_d3dRtvs);
        Array.Clear(_renderTargetViews);
        _rtvsCount = 0;

        for (int i = 0; i < renderTargets.Length.Min(_renderTargetViews.Length); ++i)
        {
            GorgonRenderTargetView view = _renderTargetViews[i] = renderTargets[i];

            Queue.Tracker.TrackResource(view.Resource);
            _d3dRtvs[i] = view.GetCpuHandle();

            GorgonSubResourceRange range = new(view.MipLevel, 1, view.ArrayIndex, view.ArrayCount, view.PlaneIndex, 1);
            SetBarrier(view.Texture, BarrierSync.RenderTarget, BarrierAccess.RenderTarget, BarrierLayout.RenderTarget, range);
        }

        if (depthStencil is not null)
        {
            Queue.Tracker.TrackResource(depthStencil.Resource);

            GorgonSubResourceRange range = new(depthStencil.MipLevel, 1, depthStencil.ArrayIndex, depthStencil.ArrayCount, 0, Graphics.FormatSupport[depthStencil.Format].PlaneCount);
            SetBarrier(depthStencil.Texture, BarrierSync.DepthStencil, BarrierAccess.DepthStencilWrite, BarrierLayout.DepthStencilWrite, range);
        }

        DepthStencil = depthStencil;
        _depthStencilView = depthStencil?.GetCpuHandle() ?? D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
        _depthStencilChanged = true;

        _rtvsCount = (uint)renderTargets.Length;
        _rtvsChanged = true;

        return this;
    }

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
        BarrierManager = new BarrierManager(graphics);
        _name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonCommandList));

        _list = CreateNative();

        // Keep a reference to the base command list type for assignment in the list execution code.
        _list.As(ref _baseList);

        _resourceWriter = _resourceCopier = new GorgonResourceCopier(Graphics, this);
    }
}