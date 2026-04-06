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
// Created: January 3, 2026 2:11:32 AM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The type of data stored in the graphics resource.
/// </summary>
public enum GraphicsResourceType
{
    /// <summary>
    /// The data type is unknown. Typically this means invalid data.
    /// </summary>
    Unknown = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_UNKNOWN,
    /// <summary>
    /// The data is a series of bytes.
    /// </summary>
    Buffer = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_BUFFER,
    /// <summary>
    /// The data is 1 dimensional texture data.
    /// </summary>
    Texture1D = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE1D,
    /// <summary>
    /// The data is 2 dimensional texture data.
    /// </summary>
    Texture2D = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D,
    /// <summary>
    /// The data is 3 dimensional texture data.
    /// </summary>
    Texture3D = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE3D
}

/// <summary>
/// The intended usage flags for the resource.
/// </summary>
[Flags]
public enum GraphicsResourceUsage
{
    /// <summary>
    /// No defined intended usage.
    /// </summary>
    None = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE,
    /// <summary>
    /// Resource is used as a render target.
    /// </summary>
    RenderTarget = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET,
    /// <summary>
    /// Resource is used as a depth/stencil buffer.
    /// </summary>
    DepthStencil = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL,
    /// <summary>
    /// Resource allows unordered access views.
    /// </summary>
    UnorderedAccess = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
    /// <summary>
    /// Resource is a ray tracing acceleration structure.
    /// </summary>
    RayTracingAccelerationStructure = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_RAYTRACING_ACCELERATION_STRUCTURE,
    /// <summary>
    /// Resource uses tight alignment.
    /// </summary>
    TightAlignment = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT,
    /// <summary>
    /// Resource is a shader resource.
    /// </summary>
    ShaderResource = 0b1000_0000_0000_0000_0000_0000
}

/// <summary>
/// A resource that contains data to be used by the GPU or CPU.
/// </summary>
/// <remarks>
/// <para>
/// TODO:...
/// </para>
/// </remarks>
public abstract unsafe class GorgonGpuResource
    : IGorgonNamedObject, IDisposable
{
    private ComPtr<ID3D12Resource2> _d3dResource;

    private static ulong _resHandleAccumulator = 0;

    private GpuResourceInfo _info;
    private readonly ulong _resHandle;
    private int _disposed;

    /// <summary>
    /// Property to return the D3D 12 resource COM pointer.
    /// </summary>
    internal ref readonly ComPtr<ID3D12Resource2> D3DResource => ref _d3dResource;

    /// <summary>
    /// Property to return information about the resource.
    /// </summary>
    internal ref readonly GpuResourceInfo Info => ref _info;

    /// <summary>
    /// Property to return the unique ID for this resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the resource belongs to the <see cref="MegaBuffer"/>, then this value <b>always</b> returns 0. 0 is a reserved ID for the mega buffer resource.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// This ID is for the lifetime of the application. It is not guaranteed to be consistent between application restarts. This makes it unusable as a external unique identifer.
    /// </note>
    /// </para>
    /// </remarks>
    internal ulong ResourceID
    {
        get
        {
            if (D3DResource.Get() == Graphics.MegaBuffer.D3DBuffer.Get())
            {
                return 0;
            }

            Debug.Assert(_resHandle != 0, "Resource ID handle is 0, this is a reserved value.");
            return _resHandle;
        }
    }

    /// <summary>
    /// Property to return whether the resource has been disposed or not.
    /// </summary>
    public bool IsDisposed => _disposed != 0 || _d3dResource.IsNull;

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the graphics instance associated with this resource.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private protected virtual void Dispose(bool disposing)
    {
        if (_d3dResource.IsNull)
        {
            return;
        }

        if (disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            Graphics.Log.Print($"Destroying Gorgon resource '{Name}'", LoggingLevel.Simple);
            Graphics.Log.Print($"Destroying D3D 12 {_info.ResourceType} resource object for '{Name}'...", LoggingLevel.Verbose);

            this.UnregisterDisposable(Graphics);
        }

        _d3dResource.Dispose();
    }

    /// <summary>
    /// Function to retrieve information about the underlying resource.
    /// </summary>
    /// <param name="resourceInfo">The data structure to populate.</param>
    private protected abstract void OnGetResourceInfo(out GpuResourceInfo resourceInfo);

    /// <summary>
    /// Function to assign the D3D 12 resource object to this resource.
    /// </summary>
    /// <param name="resource">The resource to assign.</param>
    private protected void AssignResource(ref readonly ComPtr<ID3D12Resource2> resource)
    {
        _d3dResource.Dispose();

        if (resource.IsNull)
        {
            _info = default;
            return;
        }

        // This will addref.
        _d3dResource = new ComPtr<ID3D12Resource2>(resource);

        OnGetResourceInfo(out _info);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonGpuResource() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuResource"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this resource.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="resource">The D3D 12 resource for the resource. Supplied in cases where the resource already exists prior to object creation.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="resource"/> parameter is passed in to this object and takes ownership of it. 
    /// </para>
    /// </remarks>
    private protected GorgonGpuResource(GorgonGraphics graphics, string name, ComPtr<ID3D12Resource2> resource)
    {
        Graphics = graphics;
        Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonGpuResource));

        _d3dResource = resource;

        D3D12_RESOURCE_DESC1 desc = resource.Get()->GetDesc1();
        _info = GpuResourceInfo.FromD3D(in desc);

        _resHandle = Interlocked.Increment(ref _resHandleAccumulator);
        Debug.Assert(_resHandle != 0, "Resource ID handle is 0, this is a reserved value.");
    }

    /// <summary>
    /// <inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})"/>
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <remarks>
    /// </remarks>
    private protected GorgonGpuResource(GorgonGraphics graphics, string name)
    {
        Graphics = graphics;

        this.RegisterDisposable(Graphics);

        Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonGpuResource));

        _resHandle = Interlocked.Increment(ref _resHandleAccumulator);
        Debug.Assert(_resHandle != 0, "Resource ID handle is 0, this is a reserved value.");
    }
}