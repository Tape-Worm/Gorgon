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
// Created: December 29, 2025 10:18:24 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides functionality for executing commands and synchronizing the queue.
/// </summary>
internal sealed unsafe class CommandQueue : IDisposable
{
    private ComPtr<ID3D12CommandQueue> _d3dQueue;
    private ComPtr<ID3D12Fence1> _d3dFence;

    private readonly Lock _fenceLock = new();
    private readonly GorgonGraphics _graphics;
    private ulong _previousFenceValue = 0;
    private ulong _nextFenceValue = 1;

    /// <summary>
    /// Property to return the fence value for the queue.
    /// </summary>
    public ulong FenceValue
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the current fence values for the frames in flight.
    /// </summary>
    public ulong[] FrameFenceValue
    {
        get;
    }

    /// <summary>
    /// Property to return the type of command queue.
    /// </summary>
    public D3D12_COMMAND_LIST_TYPE Type
    {
        get;
    }

    /// <summary>
    /// Property to return the pool for command allocators.
    /// </summary>
    public CommandAllocatorPool AllocatorPool
    {
        get;
    }

    /// <summary>
    /// Property to return the pool for command lists.
    /// </summary>
    public CommandListPool ListPool
    {
        get;
    }


    /// <summary>
    /// Property to return a tracker used to ensure in flight resources aren't destroyed prematurely.
    /// </summary>
    public ResourceTracker Tracker
    {
        get;
    }

    /// <summary>
    /// Property to return the D3D 12 command queue.
    /// </summary>
    public ref readonly ComPtr<ID3D12CommandQueue> D3DQueue => ref _d3dQueue;

    /// <summary>
    /// Property to return the D3D 12 queue fence.
    /// </summary>
    public ref readonly ComPtr<ID3D12Fence1> D3DFence => ref _d3dFence;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _graphics.Log.Print($"Destroying {nameof(ID3D12Fence1)} on ({Type} queue)...", LoggingLevel.Verbose);
            _graphics.Log.Print($"Destroying {nameof(ID3D12CommandQueue)} ({Type})...", LoggingLevel.Verbose);            

            // Ensure that any pending in-flight resources are removed.
            Tracker.Dispose();

            ListPool.Dispose();
            AllocatorPool.Dispose();
        }

        _d3dFence.Dispose();
        _d3dQueue.Dispose();
    }

    /// <summary>
    /// Function to create a D3D fence object.
    /// </summary>
    /// <param name="device">The D3D device responsible for creating the object.</param>
    /// <returns>The COM pointer to the D3D fence object.</returns>
    /// <exception cref="GorgonException">Thrown if the D3D fence could not be created.</exception>
    private ComPtr<ID3D12Fence1> CreateFence(ref readonly ComPtr<ID3D12Device14> device)
    {
        ComPtr<ID3D12Fence1> result = default;       

        string fenceName = $"Gorgon D3D12 fence ({Type} queue)";

        _graphics.Log.Print($"Creating {nameof(ID3D12Fence1)} on {Type} queue...", LoggingLevel.Verbose);

        device.Get()->CreateFence(0, D3D12_FENCE_FLAGS.D3D12_FENCE_FLAG_NONE, Win32.__uuidof<ID3D12Fence1>(), (void**)result.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_FENCE, Type));

        result.Get()->SetD3DDebugName(fenceName);

        return result;
    }

    /// <summary>
    /// Function to create the native Direct3D command queue object.
    /// </summary>
    /// <param name="device">The D3D device responsible for creating the object.</param>
    /// <returns>The COM pointer to the D3D command queue object.</returns>
    /// <exception cref="GorgonException">Thrown if the D3D command queue could not be created.</exception>
    private ComPtr<ID3D12CommandQueue> CreateNative(ref readonly ComPtr<ID3D12Device14> device)
    {
        ComPtr<ID3D12CommandQueue> result = default;
        
        string commandQueueName = $"Gorgon D3D12 command queue ({Type})";

        _graphics.Log.Print($"Creating {nameof(ID3D12CommandQueue)} ({Type})...", LoggingLevel.Verbose);

        D3D12_COMMAND_QUEUE_DESC desc = new()
        {
            Flags = D3D12_COMMAND_QUEUE_FLAGS.D3D12_COMMAND_QUEUE_FLAG_NONE,
            Priority = (int)D3D12_COMMAND_QUEUE_PRIORITY.D3D12_COMMAND_QUEUE_PRIORITY_NORMAL,
            Type = Type
        };

        device.Get()->CreateCommandQueue(&desc, Win32.__uuidof<ID3D12CommandQueue>(), (void**)result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_COMMAND_QUEUE, Type));

        result.Get()->SetD3DDebugName(commandQueueName);

        return result;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to determine if the current fence has been signalled.
    /// </summary>
    /// <param name="fenceValue">The fence value to test.</param>
    /// <returns><b>true</b> if work is completely for the frame, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFenceComplete(ulong fenceValue)
    {
        if (fenceValue > _previousFenceValue)
        {
            _previousFenceValue = _d3dFence.Get()->GetCompletedValue().Max(_previousFenceValue);
        }

        return fenceValue <= _previousFenceValue;
    }

    /// <summary>
    /// Function to pause the CPU if there is work pending on the GPU.
    /// </summary>
    /// <param name="fenceValue">The fence value to wait for.</param>
    /// <param name="timeout">The timeout, in milliseconds, for the CPU wait event.</param>
    public void WaitForFence(ulong fenceValue, int timeout)
    {
        if (IsFenceComplete(fenceValue))
        {
            return;
        }

        HRESULT err = _d3dFence.Get()->SetEventOnCompletion(fenceValue, HANDLE.NULL);

        if (err.FAILED)
        {
            _graphics.Log.PrintError(err, "There was an error assigning the event.", LoggingLevel.Simple);
            return;
        }

        _previousFenceValue = fenceValue;
    }

    /// <summary>
    /// Function to execute command lists on the queue.
    /// </summary>
    /// <param name="commandList">The command lists to execute on the queue.</param>
    /// <returns>The current fence value.</returns>
    public void Execute(ReadOnlySpan<GorgonCommandList> commandList)
    {
        using (_fenceLock.EnterScope())
        {
            if (commandList.Length == 0)
            {
                return;
            }

            if (commandList.Length == 1)
            {
                Execute(commandList[0]);
                return;
            }

            // Commit all reserved buffer memory.
            _graphics.MegaBuffer.Commit(this);

            ID3D12CommandList** commands = stackalloc ID3D12CommandList*[commandList.Length];

            for (int i = 0; i < commandList.Length; ++i)
            {
                commands[i] = commandList[i].D3DCommandList.Get();
            }

            _d3dQueue.Get()->ExecuteCommandLists((uint)commandList.Length, commands);
        }
    }

    /// <summary>
    /// Function to execute a command list on the queue.
    /// </summary>
    /// <param name="commandList">The command list to execute on the queue.</param>    
    /// <returns>The current fence value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(GorgonCommandList commandList)
    {
        using (_fenceLock.EnterScope())
        {
            // Commit all reserved buffer memory.
            _graphics.MegaBuffer.Commit(this);

            ID3D12CommandList** commands = stackalloc ID3D12CommandList*[1]
            {
                commandList.D3DCommandList.Get()
            };

            _d3dQueue.Get()->ExecuteCommandLists(1, commands);
        }
    }

    /// <summary>
    /// Function to increment the fence for the queue.
    /// </summary>
    /// <returns>The current fence value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong IncrementFence()
    {
        using (_fenceLock.EnterScope())
        {
            FenceValue = _nextFenceValue;

            HRESULT err = _d3dQueue.Get()->Signal((PID3D12Fence1)_d3dFence.Get(), _nextFenceValue);
            Debug.Assert(err.SUCCEEDED, $"Could not set the fence value 0x{err.Value.FormatHex()}.");
            
            return _nextFenceValue++;
        }
    }

    /// <summary>
    /// Function to pause the CPU while waiting for the GPU to finish whatever it's executing.
    /// </summary>
    /// <param name="timeout">The timeout, in milliseconds, for the CPU wait event.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WaitForGpu(int timeout) => WaitForFence(IncrementFence(), timeout);

    /// <summary>
    /// Function to purge any unused heaps.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GarbageCollect()
    {
        Tracker.Signal();
        ListPool.GarbageCollect();
        AllocatorPool.GarbageCollect();
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~CommandQueue() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandQueue"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object associated with this queue.</param>
    /// <param name="type">The type of commands that this queue will execute.</param>
    public CommandQueue(GorgonGraphics graphics,  D3D12_COMMAND_LIST_TYPE type)
    {
        _graphics = graphics;
        FrameFenceValue = new ulong[_graphics.InFlightFrameCount];
        Type = type;

        AllocatorPool = new CommandAllocatorPool(graphics, this);
        ListPool = new CommandListPool(graphics, this);
        Tracker = new ResourceTracker(this);

        _d3dQueue = CreateNative(in graphics.D3DDevice);
        _d3dFence = CreateFence(in graphics.D3DDevice);
    }
}
