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

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to wrap a D3D 12 command allocator.
/// </summary>
internal unsafe class CommandAllocator
    : IGorgonNamedObject, IDisposable
{
    private ComPtr<ID3D12CommandAllocator> _allocator;

    private readonly GorgonGraphics _graphics;
    private string _name;

    /// <summary>
    /// Property to return the D3D command allocator. 
    /// </summary>
    internal ref readonly ComPtr<ID3D12CommandAllocator> D3DAllocator => ref _allocator;

    /// <summary>
    /// Property to return the type of command list this allocator should be used with.
    /// </summary>
    public D3D12_COMMAND_LIST_TYPE Type
    {
        get;
    }

    /// <summary>
    /// Property to set or return the fence value associated with the allocator.
    /// </summary>
    public ulong Fence
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the command allocator is associated with an open command list.
    /// </summary>
    public bool HasCommandList
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the name of the command allocator.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _allocator.SetD3DDebugName($"Gorgon D3D12 Command Allocator '{_name}' ({Type})");
        }
    }

    /// <inheritdoc cref="GorgonGraphics.Dispose(bool)"/>
    private void Dispose(bool _) => _allocator.Dispose();

    /// <inheritdoc/>
    public void Dispose()
    {
        _graphics.Log.Print($"Destroying {nameof(ID3D12CommandAllocator)} '{Name} Command Allocator' ({Type})...", LoggingLevel.Verbose);

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~CommandAllocator() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of a <see cref="CommandAllocator"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns this object.</param>
    /// <param name="name">The name of the command allocator.</param>
    /// <param name="type">The type of command list the allocator will be used with.</param>
    public CommandAllocator(GorgonGraphics graphics, string name, D3D12_COMMAND_LIST_TYPE type)
    {
        string debugName = $"Gorgon D3D12 Command Allocator '{name}' ({type})";
        _graphics = graphics;
        Type = type;
        _name = name;

        _graphics.Log.Print($"Creating {nameof(ID3D12CommandAllocator)} '{_name} Command Allocator' ({type})", LoggingLevel.Verbose);

        ComPtr<ID3D12CommandAllocator> allocator = default;
        _graphics.D3DDevice.Get()->CreateCommandAllocator(type, Win32.__uuidof<ID3D12CommandAllocator>(), (void**)allocator.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_COMMAND_ALLOCATOR);

        allocator.SetD3DDebugName(debugName);

        _allocator = allocator;
    }
}
