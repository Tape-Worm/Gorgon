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
// Created: October 28, 2026 5:39:24 PM
//

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using Gorgon.Timing;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The compute engine, used to send compute work to the GPU on its own queue.
/// </summary>
/// <remarks>
/// <para>
/// The compute engine is to the compute queue what the <see cref="GorgonGraphics"/> interface is to the graphics queue. It returns <see cref="GorgonComputeCommandList"/> objects from 
/// <see cref="GetCommandList"/>, and sends them to the compute queue with <see cref="Submit(GorgonComputeCommandList)"/>. Work on the compute queue can run on the GPU at the same time as the work on the 
/// graphics and copy queues.
/// </para>
/// <para>
/// The compute engine keeps its own frame count, separate from the one used by the <see cref="GorgonGraphics"/> interface. Each call to <see cref="Submit(GorgonComputeCommandList)"/> ends a frame on the 
/// compute queue. Like the graphics queue, the number of frames that can be in flight is set by the <see cref="GorgonGraphics.InFlightFrameCount"/> property.
/// </para>
/// <para>
/// Resources, pipeline state objects, and views are shared with the <see cref="GorgonGraphics"/> interface it is associated with, so a buffer written by a dispatch on the compute queue can be read by a 
/// draw on the graphics queue. Gorgon adds the barriers needed on each queue, but the order of the work between the queues is up to the application. Use <see cref="WaitForGraphics"/> and 
/// <see cref="WaitForCopy"/> to make the compute queue wait for the other queues, and <see cref="GorgonGraphics.WaitForCompute"/> or <see cref="GorgonResourceCopier.WaitForCompute"/> to make the other 
/// queues wait for the compute queue.
/// </para>
/// </remarks>
/// <seealso cref="GorgonComputeCommandList"/>
/// <seealso cref="GorgonComputePso"/>
/// <seealso cref="GorgonDispatchCall"/>
public unsafe class GorgonComputeEngine
    : IDisposable
{
    private ComPtr<ID3D12RootSignature> _rootSignature;

    private int _isDisposed;
    private int _currentFrame;
    private readonly ComputeCommandListPool _commandListPool;
    private readonly ConcurrentBag<ComPtr<ID3D12CommandList>> _commands = [];

    /// <inheritdoc cref="GorgonGraphics.Descriptors"/>
    internal DescriptorServices Descriptors => Graphics.Descriptors;

    /// <inheritdoc cref="GorgonGraphics.Memory"/>
    internal MemoryServices Memory => Graphics.Memory;

    /// <inheritdoc cref="GorgonGraphics.Queues"/>
    internal QueueServices Queues => Graphics.Queues;

    /// <inheritdoc cref="GorgonGraphics.IsDisposed"/>
    public bool IsDisposed => _isDisposed != 0;

    /// <inheritdoc cref="GorgonGraphics.IsInDebugMode"/>
    public bool IsInDebugMode => Graphics.IsInDebugMode;

    /// <inheritdoc cref="GorgonGraphics.FormatSupport"/>
    public IReadOnlyDictionary<BufferFormat, GorgonBufferFormatSupport> FormatSupport => Graphics.FormatSupport;

    /// <summary>
    /// Property to return the frequency, in ticks per second, of the timestamps recorded by queries on command lists on the compute queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="QueryType.Timestamp"/> query records the GPU clock in ticks. To convert the difference between two timestamps into seconds, divide it by this value.
    /// </para>
    /// <para>
    /// This value applies to command lists returned by <see cref="GetCommandList"/>. The compute queue can run at a different frequency than the graphics queue, so timestamps recorded on the compute queue 
    /// should only be compared with other timestamps from the compute queue. For command lists from the graphics interface, use the <see cref="GorgonGraphics.QueryTimestampFrequency"/> property.
    /// </para>
    /// <para>
    /// If the frequency cannot be retrieved from the video adapter, this value is 0.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonQuery"/>
    public long QueryTimestampFrequency
    {
        get
        {
            ulong frequency = 0;
            return Queues.ComputeQueue.D3DQueue.Get()->GetTimestampFrequency(&frequency).FAILED ? 0 : (long)frequency;
        }
    }

    /// <summary>
    /// Property to return the graphics interface associated with this compute engine instance.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphics.Log"/>
    public IGorgonLog Log => Graphics.Log;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 1)
        {
            return;
        }

        if (disposing)
        {
            Log.Print("Waiting for GPU to finish work...", LoggingLevel.Intermediate);

            GorgonTimer sw = new();

            // Ensure the frame is ended.
            FinalizeFrame();
            
            // Wait for the GPU to finish whatever it's doing.
            WaitForGpu();
            Log.Print($"GPU finished work in {sw.Elapsed}.", LoggingLevel.Intermediate);

            Log.Print("Shutting down the compute engine interface...", LoggingLevel.Simple);
        }

        _rootSignature.Dispose();
    }

    /// <summary>
    /// Function to finalize the currently active frame.
    /// </summary>
    private void FinalizeFrame()
    {
        int finishedFrame = _currentFrame;
        int nextFrame = Interlocked.Increment(ref _currentFrame);
        nextFrame %= Graphics.InFlightFrameCount;

        ulong gfxNextFence = Queues.ComputeQueue.IncrementFence();

        Interlocked.Exchange(ref Queues.ComputeQueue.FrameFenceValue[finishedFrame], gfxNextFence);
        Interlocked.Exchange(ref _currentFrame, nextFrame);

        Queues.ComputeQueue.CollectOutstandingLists();

        // Every command list for this frame has been submitted, so the fence we just signaled covers everything allocated during the frame.
        Memory.MegaBuffer.Signal();
        Memory.TextureTilePool.Signal();
        Queues.ComputeQueue.AllocatorPool.Signal();
        Queues.ComputeQueue.QueryTracker.Signal();        
        Queues.ComputeQueue.UploadHeaps.Signal();
        Queues.ComputeQueue.DownloadHeaps.Signal();
        Descriptors.GpuViewDescriptors.Signal();

        SpinWait waiter = new();

        while (!_commands.IsEmpty)
        {
            if (!_commands.TryTake(out ComPtr<ID3D12CommandList> command))
            {
                waiter.SpinOnce();
                continue;
            }

            command.Dispose();
        }
    }

    /// <summary>
    /// Function to submit a single command list at the end of a frame.
    /// </summary>
    /// <param name="commandList">The command list to execute.</param>
    private void SubmitCommandList(GorgonComputeCommandList commandList)
    {
        if (commandList.ComputeEngine != this)
        {
            Log.PrintWarning($"The command list {commandList.Name} was not created by the same graphics object. It will be skipped. Only submit command lists created by the same graphics object.", LoggingLevel.Simple);
            return;
        }

        // This is an error, and ultimately the fault of the user and needs to be corrected ASAP. Hence why we log as an error.
        if (commandList.ParentList is null)
        {
            Log.PrintError($"The command list {commandList.Name} has already been submitted and cannot be submitted again. It will be skipped.", LoggingLevel.Simple);
            return;
        }

        try
        {
            Queues.ComputeQueue.Execute(commandList.ParentList);
        }
        finally
        {
            _commandListPool.Return(commandList);
        }
    }

    /// <summary>
    /// Function to submit multiple command lists at the end of a frame.
    /// </summary>
    /// <param name="commandLists">The list of command lists to execute.</param>
    private void SubmitCommandLists(ReadOnlySpan<GorgonComputeCommandList> commandLists)
    {
        int commandCount = 0;
        ArrayPool<GorgonCommandList> pool = GorgonArrayPools<GorgonCommandList>.GetBestPool(commandLists.Length);
        GorgonCommandList[] commands = pool.Rent(commandLists.Length);
        ReadOnlySpan<GorgonCommandList> finalCommandList = default;

        try
        {
            for (int i = 0; i < commandLists.Length; ++i)
            {
                GorgonComputeCommandList commandList = commandLists[i];

                if (commandList.ComputeEngine != this)
                {
                    Log.PrintWarning($"The command list '{commandList.Name}' at index {i} was not created by the same graphics object. It will be skipped. Only submit command lists created by the same graphics object.", LoggingLevel.Simple);
                    continue;
                }

                // This is an error, and ultimately the fault of the user and needs to be corrected ASAP. Hence why we log as an error.
                if (commandList.ParentList is null)
                {
                    Log.PrintError($"The command list {commandList.Name} has already been submitted and cannot be submitted again. It will be skipped.", LoggingLevel.Simple);
                    return;
                }

                commandList.ParentList.TrackResource(commandList.ParentList.D3DGraphicsCommandList);
                commands[commandCount++] = commandList.ParentList;
            }

            if (commandCount == 0)
            {
                Log.PrintWarning("There are no command lists passed to this method that can be executed.", LoggingLevel.Simple);
                return;
            }

            finalCommandList = commands.AsSpan(0, commandCount);
            Queues.ComputeQueue.Execute(finalCommandList);
        }
        finally
        {
            // Return the actual list to the command queue for reuse later.
            if (!finalCommandList.IsEmpty)
            {
                for (int i = 0; i < finalCommandList.Length; ++i)
                {
                    Queues.ComputeQueue.ListPool.Return(finalCommandList[i]);
                }
            }

            // Return our wrapper list so we can reuse.
            if (!commandLists.IsEmpty)            
            {
                for (int i = 0; i < commandLists.Length; ++i)
                {
                    _commandListPool.Return(commandLists[i]);
                }
            }

            pool.Return(commands, true);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to create a new compute command list.
    /// </summary>
    /// <param name="commandListName">[Optional] The name to apply to the command list returned by this method.</param>
    /// <returns>A new <see cref="GorgonComputeCommandList"/> for sending commands to the compute queue.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to create command lists to send commands to the compute queue. Multiple command lists can be created for use in multiple threads, which helps reduce the cost of recording the work.
    /// </para>
    /// <para>
    /// The optional <paramref name="commandListName"/> parameter is used to help identify the command list in debugging scenarios, so while it is optional, it is recommended to set this value as it will 
    /// assist in locating which command list is causing an issue.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// While Gorgon is capable of using multiple threads to record commands, a <see cref="GorgonComputeCommandList"/> is <b>NOT</b> thread safe. Only use a single command list per thread.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputeCommandList"/>
    public GorgonComputeCommandList GetCommandList(string? commandListName = null)
    {
        commandListName = GorgonGraphicsFactory.GenerateName(commandListName ?? string.Empty, nameof(GorgonCommandList));
        GorgonComputeCommandList list = _commandListPool.Get(commandListName);

        Debug.Assert(list.ParentList is not null, $"The parent command list for '{commandListName}' is NULL.");

        list.BeginRecording(_currentFrame, in _rootSignature);
        _commands.Add(new ComPtr<ID3D12CommandList>(list.ParentList.D3DCommandList));

        return list;
    }

    /// <summary>
    /// Function to end the current frame, move to the next frame, and clean up per frame resources.
    /// </summary>
    /// <param name="commandList">The command list to execute at the end of the frame.</param>
    /// <inheritdoc cref="Submit(ReadOnlySpan{GorgonComputeCommandList})" path="/exception"/>
    /// <inheritdoc cref="Submit(ReadOnlySpan{GorgonComputeCommandList})" path="/remarks"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Submit(GorgonComputeCommandList commandList)
    {
        SubmitCommandList(commandList);

        FinalizeFrame();
    }

    /// <summary>
    /// Function to end the current frame, move to the next frame, and clean up per frame resources.
    /// </summary>
    /// <param name="commandLists">The command lists to execute at the end of the frame.</param>
    /// <exception cref="AggregateException">Thrown if one or more command lists could not be closed after an error occurred during submission. The first inner exception is the original error.</exception>
    /// <remarks>
    /// <para>
    /// This method submits the command lists to the GPU in a single batch, and then ends the current frame. Every command list obtained from <see cref="GetCommandList"/> during the frame must be 
    /// passed to this method before the next frame begins.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// This method must be called from a single thread. Command lists can be recorded on multiple threads, but they must all be submitted from the same thread. Direct3D 12 serializes submissions to a 
    /// command queue in the order they arrive, so submitting from multiple threads provides no benefit, and makes the execution order of the command lists depend on thread timing.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Submit(ReadOnlySpan<GorgonComputeCommandList> commandLists)
    {
        if (commandLists.Length != 0)
        {
            SubmitCommandLists(commandLists);
        }
        else
        {
            Log.PrintWarning("There are no command lists passed to this method.", LoggingLevel.Intermediate);
        }

        FinalizeFrame();
    }

    /// <summary>
    /// Function to make the compute queue wait for the copy queue if it's in the process of copying data.
    /// </summary>
    /// <exception cref="GorgonException">Thrown if there was a failure during the wait operation.</exception>
    /// <remarks>
    /// <para>
    /// This method is meant to make the compute queue wait for the copy queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the copy queue is busy updating a buffer 
    /// required by the compute queue, this will allow the compute queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// To make use of the copy queue, developers can use the <see cref="GorgonResourceCopier"/> functionality.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// This method is different from the <see cref="WaitForGpu"/> method in that it causes the compute queue to wait for the copy queue to finish its work on the GPU, while the <see cref="WaitForGpu"/> method 
    /// stalls the CPU until the GPU has finished its work.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="WaitForGpu"/>
    /// <seealso cref="WaitForGraphics"/>
    /// <seealso cref="GorgonResourceCopier"/>
    public void WaitForCopy()
    {
        ulong copyQueueFence = Queues.CopyQueue.FenceValue;
        Queues.ComputeQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Queues.CopyQueue.D3DFence.Get(), copyQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, Resources.GORGFX_ERR_WAIT_FAILED, nameof(Queues.ComputeQueue), nameof(Queues.CopyQueue));
    }

    /// <summary>
    /// Function to make the compute queue wait for the graphics queue if it's in the process of working with data.
    /// </summary>
    /// <inheritdoc cref="WaitForCopy" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method is meant to make the compute queue wait for the graphics queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the graphics queue is busy rendering to a 
    /// texture required by the compute queue, this will allow the compute queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// This method is different from the <see cref="WaitForGpu"/> method in that it causes the compute queue to wait for the graphics queue to finish its work on the GPU, while the <see cref="WaitForGpu"/> 
    /// method stalls the CPU until the GPU has finished its work.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="WaitForGpu"/>
    /// <seealso cref="WaitForCopy"/>
    /// <seealso cref="GorgonGraphics"/>
    public void WaitForGraphics()
    {
        ulong graphicsQueueFence = Queues.GraphicsQueue.FenceValue;
        Queues.ComputeQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Queues.GraphicsQueue.D3DFence.Get(), graphicsQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, Resources.GORGFX_ERR_WAIT_FAILED, nameof(Queues.ComputeQueue), nameof(Queues.GraphicsQueue));
    }

    /// <inheritdoc cref="GorgonGraphics.WaitForGpu"/>
    public void WaitForGpu()
    {
        Queues.ComputeQueue.WaitForGpu();
        Queues.GraphicsQueue.WaitForGpu();
        Queues.CopyQueue.WaitForGpu();
    }

    /// <inheritdoc cref="GorgonGraphics.GarbageCollect(bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GarbageCollect(bool forceDotNetGc = false)
    {
        WaitForGpu();

        Queues.GarbageCollect();
        Memory.GarbageCollect();
        Descriptors.GarbageCollect();

        if (!forceDotNetGc)
        {
            return;
        }

        GC.Collect(2, GCCollectionMode.Aggressive, true, true);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonComputeEngine() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonComputeEngine"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with the compute engine.</param>
    public GorgonComputeEngine(GorgonGraphics graphics)
    {
        Graphics = graphics;
        _commandListPool = new(this);
        _rootSignature = new ComPtr<ID3D12RootSignature>(graphics.D3DRootSignature);
    }    
}
