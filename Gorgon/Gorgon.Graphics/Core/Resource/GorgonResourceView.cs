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
// Created: January 3, 2026 12:18:17 PM
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A base class for all views of a <see cref="GorgonGpuResource"/>.
/// </summary>
/// <seealso cref="GorgonGpuResource"/>
public abstract class GorgonResourceView
    : IGorgonNamedObject, IDisposable
{
    private int _disposed;

    /// <summary>
    /// Property to return whether the resource is owned by this view.
    /// </summary>
    internal bool OwnsResource
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the D3D CPU descriptor handle for the view.
    /// </summary>
    internal D3D12_CPU_DESCRIPTOR_HANDLE D3DCpuHandle
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the D3D GPU descriptor handle for the view.
    /// </summary>
    internal D3D12_GPU_DESCRIPTOR_HANDLE D3DGpuHandle
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the graphics interface associated with this view.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <inheritdoc/>
    public string Name
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the resource for this view.
    /// </summary>
    public GorgonGpuResource Resource
    {
        get;        
    }

    /// <summary>
    /// Property to return the resource type that is backing this view.
    /// </summary>
    public GraphicsResourceType ResourceType => Resource.Info.ResourceType;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            D3DCpuHandle = D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT;
            D3DGpuHandle = D3D12_GPU_DESCRIPTOR_HANDLE.DEFAULT;

            if (OwnsResource)
            {
                Graphics.Log.Print($"Destroying resource '{Resource.Name}' because it is owned by this view.", LoggingLevel.Intermediate);
                Resource.Dispose();
            }            
        }
    }

    /// <summary>
    /// Function to create the handles required for the view.
    /// </summary>
    /// <returns>A tuple containing the D3D CPU descriptor handle, and, optionally, the D3D12 GPU descriptor handle.</returns>
    private protected virtual (D3D12_CPU_DESCRIPTOR_HANDLE CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE GpuHandle) OnCreateViewHandles() => (D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT, D3D12_GPU_DESCRIPTOR_HANDLE.DEFAULT);

    /// <summary>
    /// Function to assign the descriptor handles to the associated properties.
    /// </summary>
    /// <param name="cpuHandle">The CPU descriptor handle.</param>
    /// <param name="gpuHandle">The GPU descriptor handle.</param>
    /// <remarks>
    /// <para>
    /// Implementors MUST call this method after descriptor handle creation.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected void SetHandles(D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle)
    {
        D3DCpuHandle = cpuHandle;
        D3DGpuHandle = gpuHandle;
    }

    /// <summary>
    /// Function to create the native view handle(s) and memory allocation information.
    /// </summary>
    [Obsolete("This is the old way, get rid of it!")]
    protected void CreateNative() => (D3DCpuHandle, D3DGpuHandle) = OnCreateViewHandles();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonResourceView"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this view.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="resource">The resource for the view.</param>
    /// <param name="owned"><b>true</b> if the resource is owned by this view, <b>false</b> if not.</param>
    private protected GorgonResourceView(GorgonGraphics graphics, string name, GorgonGpuResource resource, bool owned)
    {
        Graphics = graphics;
        Name = GorgonGraphicsFactory.GenerateName(name, GetType().Name);
        OwnsResource = owned;
        Resource = resource;
    }
}
