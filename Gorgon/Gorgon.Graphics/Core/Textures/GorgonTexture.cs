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
// Created: January 11, 2026 11:49:16 AM
//

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A texture used to project an image onto a geometric primitive such as a triangle.
/// </summary>
/// <remarks>
/// <para>
/// Textures are used to map image data onto geometric data like triangles, meshes, etc... This texture type stores this image data in a structured format for various dimensions. Textures can be composed of 
/// multiple pieces like mip map levels, array indices and 3D data.
/// </para>
/// <para>
/// Textures can make use of views to allow shaders to read (or write) the data in the texture, or a portion of it. They can also be used to allow applications to render image data into the texture.
/// </para>
/// <para>
/// Texture data is organized using texels that follow a specific <see cref="BufferFormat"/>. The data is also organized by its dimensions:
/// <list type="bullet">
///     <item>
///         <term>1D</term>
///         <description>Acts like a linear buffer, in that it only has a width.</description>
///     </item>
///     <item>
///         <term>2D</term>
///         <description>Has data organized into a 2D grid, with a width, and a height.</description>
///     </item>
///     <item>
///         <term>3D</term>
///         <description>Has data organized into a 3D cube structure, with a width, height and depth.</description>
///     </item>
/// </list>
/// Textures can be sub-divided into data groupings like mip-map levels, which decrease in size for each level and typically store a smaller version of the image from the first level. All texture dimensions 
/// support this. For 1D and 2D textures, they can further be sub-divided into array indices. This works similarly to a standard array where each element is another image (with the same dimensions and 
/// format). 
/// </para>
/// <para>
/// Applications can configure textures to be used for purposes other than display. They can be used to store rendering output as render targets, or depth information as a depth/stencil buffer, or output  
/// from shaders.
/// </para>
/// <para>
/// When configuring a texture, there are certain combinations of usages that don't work together:
/// <list type="bullet">
///     <item>
///         <description>Render targets cannot be depth/stencil buffers or vice-versa.</description>
///     </item>
///     <item>
///         <description>Only 2D textures can be depth stencil buffers.</description>
///     </item>
///     <item>
///         <description>When specifying a texture as a render target, read/write access or depth stencil resource, the texture format must be compatible. Applications can evaluate the 
///         <see cref="GorgonGraphics.FormatSupport"/> property to determine if the format supports the desired functionality.</description>
///     </item>
///     <item>
///         <description>Depth/stencil textures cannot be used with read/write views.</description>
///     </item>
///     <item>
///         <description>Textures that can have read/write access, must have a multi-sample value of <see cref="GorgonMultisampleInfo.NoMultisampling"/> (i.e. multisampling disabled).</description>
///     </item>
///     <item>
///         <description>Textures that have multisampling enabled, cannot have a mip count larger than 1.</description>
///     </item>
///     <item>
///         <description>Textures that indicate they are to be used as a cube map must have an array count that is a multiple of 6.</description>
///     </item>
/// </list>
/// </para>
/// <para type="depth_buffer_usage">
/// <para>
/// <h3>Depth/Stencil Buffers</h3>
/// </para>
/// <para>
/// If the texture is to be used as a write-only depth/stencil buffer, then its format must be one of the following:
/// <list type="bullet">
///     <item>
///         <description><see cref="BufferFormat.D16_UNorm"/></description>
///     </item>
///     <item>
///         <description><see cref="BufferFormat.D32_Float"/></description>
///     </item>
///     <item>
///         <description><see cref="BufferFormat.D24_UNorm_S8_UInt"/></description>
///     </item>
///     <item>
///         <description><see cref="BufferFormat.D32_Float_S8X24_UInt"/></description>
///     </item>
/// </list>
/// </para>
/// <para>
/// However, if an application wishes to read the buffer in a shader as a <see cref="IGorgonTextureView{GorgonTexture}"/>, the format of the buffer must be one of the following typeless formats:
/// <list type="bullet">
///     <item>
///         <term><see cref="BufferFormat.R16_Typeless"/></term>
///         <description>Depth only.</description>
///     </item>
///     <item>
///         <term><see cref="BufferFormat.R32_Typeless"/></term>
///         <description>Depth only.</description>
///     </item>
///     <item>
///         <term><see cref="BufferFormat.R24G8_Typeless"/></term>
///         <description>Depth and stencil.</description>
///     </item>
///     <item>
///         <term><see cref="BufferFormat.R32G8X24_Typeless"/></term>
///         <description>Depth and stencil.</description>
///     </item>
/// </list>
/// </para>
/// <para type="max_dimensions">
/// Textures have a maximum size for array counts, width, height and depth depending on the <see cref="TextureType"/>.
/// <list type="table">
///     <listheader>
///         <term><see cref="TextureType"/></term>
///         <term>Maximum width</term>
///         <term>Maximum height</term>
///         <term>Maximum depth</term>
///         <term>Maximum array count.</term>
///     </listheader>
///     <item>
///         <description><see cref="TextureType.Texture1D"/></description>
///         <description><see cref="GorgonTextureCommon.Max1DTextureWidth"/></description>
///         <description>1</description>
///         <description>1</description>
///         <description><see cref="GorgonTextureCommon.Max1DArraySize"/></description>
///     </item>
///     <item>
///         <description><see cref="TextureType.Texture2D"/></description>
///         <description><see cref="GorgonTextureCommon.Max2DTextureWidth"/></description>
///         <description><see cref="GorgonTextureCommon.Max2DTextureHeight"/></description>
///         <description>1</description>
///         <description><see cref="GorgonTextureCommon.Max2DArraySize"/></description>
///     </item>
///     <item>
///         <description><see cref="TextureType.Texture3D"/></description>
///         <description><see cref="GorgonTextureCommon.Max3DTextureWidth"/></description>
///         <description><see cref="GorgonTextureCommon.Max3DTextureHeight"/></description>
///         <description><see cref="GorgonTextureCommon.Max3DTextureDepth"/></description>
///         <description>1</description>
///     </item>
/// </list>
/// </para>
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
/// </remarks>
/// <seealso cref="GorgonTextureCommon"/>
/// <seealso cref="IGorgonTextureView{GorgonTexture}"/>
/// <seealso cref="BufferFormat"/>
/// <seealso cref="TextureType"/>
public sealed unsafe class GorgonTexture
    : GorgonTextureCommon
{   
    private ComPtr<D3D12MA_Allocation> _resourceAllocation;

    private readonly Lock _viewLock = new();
    private readonly Dictionary<ViewKey, GorgonDepthStencilView> _dsvs = [];
    private readonly CpuResourceHeapPool _uploadHeaps;

    /// <summary>
    /// Function to build the flags for the resource description.
    /// </summary>
    /// <returns>The flags for the resource description.</returns>
    private D3D12_RESOURCE_FLAGS BuildFlags()
    {
        D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;

        if (HasReadWriteAccess)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        }

        if (!IsShaderResource)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;
        }

        if (IsRenderTarget)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        }
        else if (IsDepthStencil)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
        }

        if ((Graphics.Adapter.HasTightAlignmentSupport) && (!IsRenderTarget) && (!IsDepthStencil))
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT;
        }

        return flags;
    }

    /// <summary>
    /// Function to build a list of castable formats.
    /// </summary>
    /// <returns>The buffer containing the list of castable formats.</returns>
    private GorgonNativeBuffer<DXGI_FORMAT> BuildCastList()
    {
        GorgonNativeBuffer<DXGI_FORMAT> castable = [];

        if ((FormatGroups.TryGetValue(FormatInfo.SizeInBytes, out List<BufferFormat>? compatibleFormats))
            && (compatibleFormats.Count > 0))
        {
            castable = new GorgonNativeBuffer<DXGI_FORMAT>(compatibleFormats.Count);

            for (int i = 0; i < compatibleFormats.Count; ++i)
            {
                castable[i] = (DXGI_FORMAT)compatibleFormats[i];
            }
        }

        return castable;
    }

    /// <summary>
    /// Function to initialize a new texture with 0 values.
    /// </summary>
    private void InitializeTexture()
    {
        CommandQueue queue = Graphics.Queues.GraphicsQueue;

        _uploadHeaps.Allocate((ulong)SizeInBytes, Info.Alignment, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, $"Could not allocate upload memory for texture '{Name}'");

        queue.Tracker.TrackResource(this);
        queue.Tracker.TrackResource(allocation.Heap.D3DResource);

        NativeMemory.Fill(allocation.CpuPointer, (nuint)SizeInBytes, 0);

        CommandAllocator allocator = queue.AllocatorPool.Get("Texture Initialization Allocator");
        GorgonCommandList list = queue.ListPool.Get("Texture Initialization Command List", allocator);

        try
        {
            list.SetBarrier(this, BarrierSync.Copy, BarrierAccess.CopyDestination, BarrierLayout.CopyDestination, force: true);

            for (int i = 0; i < SubResources.Count; ++i)
            {
                D3D12_PLACED_SUBRESOURCE_FOOTPRINT placedFootPrint = SubResources[i].ToD3DPlacedSubResourceFootPrint(Format, allocation.Offset);
                ref readonly D3D12_SUBRESOURCE_FOOTPRINT footPrint = ref placedFootPrint.Footprint;
                D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in placedFootPrint);
                D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)D3DResource.Get(), (uint)i);
                D3D12_BOX box = new(0, 0, 0, (int)footPrint.Width, (int)footPrint.Height, (int)footPrint.Depth);

                list.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, &box);
            }

            list.D3DGraphicsCommandList.Get()->Close();

            queue.Execute(list);
            ulong fence = queue.IncrementFence();

            queue.WaitForFence(fence, GorgonGraphics.WaitFenceTimeout);
        }
        finally
        {
            _uploadHeaps.Signal();
            queue.AllocatorPool.Signal();
            queue.Tracker.Signal();
            queue.ListPool.Return(list);
        }
    }

    /// <summary>
    /// Function to create the native D3D 12 resources for the texture.
    /// </summary>
    /// <inheritdoc cref="OnCreateNative()" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if the COM resource could not be created.</exception>
    /// <exception cref="InvalidCastException">Thrown if the resource could not be casted to the correct COM type.</exception>
    private protected override ComPtr<ID3D12Resource2> OnCreateNative()
    {
        ulong alignment = (Graphics.Adapter.HasTightAlignmentSupport || MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling)) ? 0UL : D3D12.D3D12_DEFAULT_MSAA_RESOURCE_PLACEMENT_ALIGNMENT;
        using ComPtr<ID3D12Resource2> result = default;
        ComPtr<D3D12MA_Allocation> resourcePtr = default;
        D3D12_CLEAR_VALUE* clearValue = null;
        D3D12_RESOURCE_FLAGS flags = BuildFlags();

        if (IsRenderTarget)
        {
            D3D12_CLEAR_VALUE clear = new()
            {
                Format = (DXGI_FORMAT)Format,
            };
            clear.Color[0] = 0;
            clear.Color[1] = 0;
            clear.Color[2] = 0;
            clear.Color[3] = 0;
            clearValue = &clear;
        }
        else if (IsDepthStencil)
        {
            D3D12_CLEAR_VALUE clear = new()
            {
                Format = (DXGI_FORMAT)Format,
                DepthStencil = new D3D12_DEPTH_STENCIL_VALUE
                {
                    Depth = 1.0f,
                    Stencil = 0
                }
            };
            clearValue = &clear;
        }

        D3D12_RESOURCE_DESC1 desc = Type switch
        {
            TextureType.Texture1D => D3D12_RESOURCE_DESC1.Tex1D((DXGI_FORMAT)Format, (ulong)Width, (ushort)ArrayCount, (ushort)MipCount, flags),
            TextureType.Texture2D => D3D12_RESOURCE_DESC1.Tex2D((DXGI_FORMAT)Format, (ulong)Width, (uint)Height, (ushort)ArrayCount, (ushort)MipCount, (uint)MultisampleInfo.Count, (uint)MultisampleInfo.Quality, flags,
                                        alignment: alignment),
            TextureType.Texture3D => D3D12_RESOURCE_DESC1.Tex3D((DXGI_FORMAT)Format, (ulong)Width, (uint)Height, (ushort)Depth, (ushort)MipCount, flags),
            _ => throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, Type))
        };

        string textureName = $"D3D12 {Type} {Name}";

        Graphics.Log.Print($"Created D3D 12 {Type} resource object for '{Name}'.", LoggingLevel.Verbose);

        D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

        using GorgonNativeBuffer<DXGI_FORMAT> castable = BuildCastList();

        DXGI_FORMAT* castPtr = castable.Length == 0 ? null : (DXGI_FORMAT*)castable;

        Graphics.Memory.Allocator.Get()->CreateResource3(&allocDesc, &desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
                                                         clearValue, (uint)castable.Length, castPtr,
                                                         resourcePtr.GetAddressOf(), Win32.__uuidof<ID3D12Resource2>(), (void**)result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE);

        _resourceAllocation = resourcePtr;

        result.SetD3DDebugName(textureName);

        return result;
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Remove all the child views.
            foreach (GorgonResourceView view in _dsvs.Values.Where(v => !v.OwnsResource))
            {
                view.Dispose();
            }

            _dsvs.Clear();
        }

        _resourceAllocation.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Function to create a 2D render target from a swap chain.
    /// </summary>
    /// <param name="swapChain">The swap chain to retrieve the render target view fromn.</param>
    /// <param name="index">The index of the back buffer for the render target view.</param>
    /// <returns>A new render target view.</returns>
    internal static GorgonTexture FromSwapChain(GorgonSwapChain swapChain, uint index)
    {        
        ComPtr<ID3D12Resource2> d3dResource = default;

        string backBufferName = $"{swapChain.Name} D3D 12 Backbuffer Resource #{index}";

        swapChain.Graphics.Log.Print($"Retrieving D3D 12 Resource (Swap chain back buffer): {backBufferName}.", LoggingLevel.Verbose);

        swapChain.DXGISwapChain.Get()->GetBuffer(index, Win32.__uuidof<ID3D12Resource2>(), (void**)d3dResource.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_RETRIEVE_BACKBUFFER, index, swapChain.Name));

        d3dResource.SetD3DDebugName(backBufferName);

        D3D12_RESOURCE_DESC1 desc = d3dResource.Get()->GetDesc1();
        GpuResourceInfo info = GpuResourceInfo.FromD3D(in desc);

        GorgonTexture resource = new(swapChain.Graphics, $"{swapChain.Name}: Render target texture #{index}", d3dResource, new GorgonTextureInfo(TextureType.Texture2D, info.Format)
        {
            ArrayCount = 1,
            Depth = 1,
            Height = info.Texture2D.Height,
            Width = info.Texture2D.Width,
            IsCube = false,
            IsDepthStencil = false,
            IsRenderTarget = true,
            IsShaderResource = (info.Usage & GraphicsResourceUsage.ShaderResource) == GraphicsResourceUsage.ShaderResource,
            HasReadWriteAccess = (info.Usage & GraphicsResourceUsage.ReadWrite) == GraphicsResourceUsage.ReadWrite,
            MipCount = 1,
            MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
        });

        if (resource.IsShaderResource)
        {
            resource.GetTextureView();
        }

        // Get a default view for the texture.
        resource.GetRenderTargetView(resource.Format, resource.MipCount, 0, resource.ArrayCount, 0, true);

        return resource;
    }

    /// <summary>
    /// Function to retrieve a <see cref="GorgonDepthStencilView"/> to allow this texture to be used as a depth/stencil buffer.
    /// </summary>
    /// <param name="format"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='format']"/></param>
    /// <param name="access">[Optional] The access flags for the view.</param>
    /// <param name="mipLevel">[Optional] The mip map level in the texture to view.</param>
    /// <param name="arrayIndex">[Optional] The first array index to view.</param>
    /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='owned']"/></param>
    /// <returns>The <see cref="GorgonDepthStencilView"/> for the texture.</returns>
    /// <inheritdoc cref="GorgonDepthStencilView.ValidateDepthStencilView(string, GorgonBufferFormatSupport, GorgonFormatInfo, GorgonFormatInfo, bool)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows textures to be used as a depth/stencil buffer. 
    /// </para>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/remarks/para[@type='param_constraints']"/>
    /// <para>
    /// If the <paramref name="arrayCount"/> is less than 1, then this will ensure the view uses the remainder (starting from <paramref name="arrayIndex"/>) of the texture for the view.
    /// </para>
    /// <para>
    /// The <paramref name="access"/> value can be used to lock down a portion of the depth/stencil buffer.
    /// </para>
    /// <para>
    /// The texture must have been created as a depth/stencil buffer by passing the <see cref="GorgonTextureInfo"/> with its <see cref="GorgonTextureInfo.IsDepthStencil"/> set to <b>true</b> when creating 
    /// the texture. If the texture was not created as a depth/stencil buffer, then an exception will be thrown.
    /// </para>
    /// <para>
    /// The <paramref name="format"/> cannot be a typeless format, and must be supported by depth/stencil buffers. This can be found on the <see cref="GorgonBufferFormatSupport.IsDepthStencilFormat"/> 
    /// property on the <see cref="GorgonBufferFormatSupport"/> object on the <see cref="GorgonGraphics.FormatSupport"/> property. If these conditions are not met, then an exception will be thrown.
    /// </para>
    /// <para>
    /// If the texture is a write only depth/stencil texture with a depth/stencil <see cref="GorgonTextureCommon.Format"/>, then the view <paramref name="format"/> must match. 
    /// </para>
    /// <para>
    /// If the depth/stencil is meant to be read in a shader, and needs a <see cref="IGorgonTextureView{GorgonTexture}"/>, then the texture must be created with a typeless <see cref="BufferFormat"/>. The following table 
    /// indicates the formats required for reading depth/stencil textures:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Texture Format</term>
    ///         <term>Depth/Stencil View Format</term>
    ///         <term>Texture View Format (Depth)</term>
    ///         <term>Texture View Format (Stencil)</term>
    ///     </listheader>
    ///     <item>
    ///         <description><see cref="BufferFormat.R16_Typeless"/></description>
    ///         <description><see cref="BufferFormat.D16_UNorm"/></description>
    ///         <description><see cref="BufferFormat.R16_UNorm"/></description>
    ///         <description>N/A</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R32_Typeless"/></description>
    ///         <description><see cref="BufferFormat.D32_Float"/></description>
    ///         <description><see cref="BufferFormat.R32_Float"/></description>
    ///         <description>N/A</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R24G8_Typeless"/></description>
    ///         <description><see cref="BufferFormat.D24_UNorm_S8_UInt"/></description>
    ///         <description><see cref="BufferFormat.R24_UNorm_X8_Typeless"/></description>
    ///         <description><see cref="BufferFormat.X24_Typeless_G8_UInt"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R32G8X24_Typeless"/></description>
    ///         <description><see cref="BufferFormat.D32_Float_S8X24_UInt"/></description>
    ///         <description><see cref="BufferFormat.R32_Float_X8X24_Typeless"/></description>
    ///         <description><see cref="BufferFormat.X32_Typeless_G8X24_UInt"/></description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonDepthStencilView"/>
    /// <seealso cref="IGorgonTextureView{GorgonTexture}"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="GorgonTextureInfo"/>
    /// <seealso cref="BufferFormat"/>
    internal GorgonDepthStencilView GetDepthStencilView(BufferFormat format, DepthStencilViewAccess access, short mipLevel, short arrayIndex, short arrayCount, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            if (format == BufferFormat.Unknown)
            {
                format = Format;
            }

            mipLevel = mipLevel.Max(0).Min((short)(MipCount - 1));
            arrayIndex = arrayIndex.Max(0).Min((short)(ArrayCount - 1));

            if (arrayCount <= 0)
            {
                arrayCount = (short)(ArrayCount - arrayIndex);
            }
            else
            {
                arrayCount = arrayCount.Min((short)(ArrayCount - arrayIndex)).Max(1);
            }

            ViewKey key = new((short)format, (short)access, mipLevel, arrayIndex, arrayCount, 0, 0);

            if (_dsvs.TryGetValue(key, out GorgonDepthStencilView? result))
            {
                return result;
            }

            GorgonFormatInfo formatInfo = format == Format ? FormatInfo : new GorgonFormatInfo(format);
            GorgonDepthStencilView.ValidateDepthStencilView(Name, Graphics.FormatSupport[format], FormatInfo, formatInfo, IsDepthStencil);

            return _dsvs[key] = new(Graphics, Name, this, formatInfo, access, mipLevel, arrayIndex, arrayCount, owned);
        }
    }

    /// <summary>
    /// <inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/summary"/>
    /// </summary>
    /// <param name="format"><inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/param[@name='format']"/></param>
    /// <param name="access"><inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/param[@name='access']"/></param>
    /// <param name="mipLevel"><inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/param[@name='mipLevel']"/></param>
    /// <param name="arrayIndex"><inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/param[@name='arrayIndex']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/param[@name='arrayCount']"/></param>
    /// <inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/returns"/>
    /// <inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/exception"/>
    /// <inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/remarks"/>
    /// <inheritdoc cref="GetDepthStencilView(BufferFormat, DepthStencilViewAccess, short, short, short, bool)" path="/seealso"/>
    public GorgonDepthStencilView GetDepthStencilView(BufferFormat format, DepthStencilViewAccess access = DepthStencilViewAccess.None, short mipLevel = 0, short arrayIndex = 0, short arrayCount = 0) =>
        GetDepthStencilView(format, access, mipLevel, arrayIndex, arrayCount, false);

    /// <summary>
    /// Function to create a texture from a <see cref="IGorgonImage"/> object.
    /// </summary>
    /// <param name="graphics">The graphics image associated with the texture.</param>
    /// <param name="name">The name of the image.</param>
    /// <param name="image">The image used to define the texture schema, and its contents.</param>
    /// <returns>A new texture, populated with the image data.</returns>
    public static GorgonTexture FromImage(GorgonGraphics graphics, string name, IGorgonImage image)
    {
        GorgonResourceCopier copier = graphics.Queues.GlobalCopier;
        GorgonTextureInfo info = GorgonTextureInfo.FromImageInfo(image);

        GorgonTexture texture = new(graphics, name, info);
        copier.BeginUpload()
              .CopyImageToTexture(image, texture)
              .End();
        return texture;
    }

    /// <summary>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/summary"/>
    /// </summary>
    /// <param name="format"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='format']"/></param>
    /// <param name="mipLevel"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='mipLevel']"/></param>
    /// <param name="mipCount"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="arrayIndex"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='arrayIndex']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='arrayCount']"/></param>
    /// <param name="resourceMinLodClamp"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='resourceMinLodClamp']"/></param>
    /// <param name="planeIndex"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='planeIndex']"/></param>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/exception"/>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/remarks"/>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/seealso"/>
    public IGorgonTextureView<GorgonTexture> GetTextureView(BufferFormat format = BufferFormat.Unknown, short mipLevel = 0, short mipCount = 0, short arrayIndex = 0, short arrayCount = 0, float resourceMinLodClamp = 0, byte planeIndex = 0) =>
        GetTextureView<GorgonTexture>(format, mipLevel, mipCount, arrayIndex, arrayCount, resourceMinLodClamp, planeIndex, false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture"/> resource.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this texture.</param>
    /// <param name="name">The name of the texture.</param>
    /// <param name="resource">The predefined D3D12 resource that this texture is wrapping.</param>
    /// <param name="info">The texture information.</param>
    /// <remarks>
    /// <para>
    /// This is used for swap chain creation.
    /// </para>
    /// </remarks>
    internal GorgonTexture(GorgonGraphics graphics, string name, ComPtr<ID3D12Resource2> resource, GorgonTextureInfo info)
        : base(graphics, name, resource, info) => _uploadHeaps = graphics.Memory.UploadHeaps;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture"/> resource.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the texture.</param>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// When applications create a texture, they have to pass in a <see cref="GorgonTextureInfo"/> object to define the layout of the texture. Applications use this to define the number of dimensions in the 
    /// texture (<see cref="TextureType"/>) and its format (<see cref="BufferFormat"/>).
    /// </para>
    /// <para>
    /// The following is a list of dimensions and their required values:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="TextureType.Texture1D"/></term>
    ///         <description><see cref="GorgonTextureInfo.Width"/> is required, <see cref="GorgonTextureInfo.Height"/> and <see cref="GorgonTextureInfo.Depth"/> should be set to 1.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TextureType.Texture2D"/></term>
    ///         <description><see cref="GorgonTextureInfo.Width"/> and <see cref="GorgonTextureInfo.Height"/> are required, <see cref="GorgonTextureInfo.Depth"/> should be set to 1. If the texture format is compressed 
    ///         then the width and height should be a multiple of 4.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TextureType.Texture3D"/></term>
    ///         <description><see cref="GorgonTextureInfo.Width"/>, <see cref="GorgonTextureInfo.Height"/>, and <see cref="GorgonTextureInfo.Depth"/> are required. If the texture format is compressed 
    ///         then the width and height should be a multiple of 4.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureInfo.Format"/> should be a supported format. This can be determined by checking the <see cref="GorgonGraphics.FormatSupport"/> property on the <see cref="GorgonGraphics"/> 
    /// object to determine if the format is supported for a given texture type.
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureInfo.ArrayCount"/> should be set to 1 for <see cref="TextureType.Texture3D"/> textures. It only applies to 1D and 2D textures. If the 
    /// <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b>, then this value <b>must</b> be a multiple of 6.
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureInfo.MipCount"/> value should be at least 1. <see cref="GorgonTextureInfo.GetMaximumMipCount(int, int, short)"/> can be used to determine the maximum number of mip levels for 
    /// the texture.
    /// </para>
    /// <para>
    /// If the <see cref="GorgonTextureInfo.IsDepthStencil"/> value is <b>true</b>:
    /// <list type="bullet">
    ///     <item>
    ///         <description><see cref="GorgonTextureInfo.IsRenderTarget"/> must be <b>false</b>. The opposite also applies.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="GorgonTextureInfo.HasReadWriteAccess"/> must be <b>false</b>. The opposite also applies.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="GorgonTextureInfo.Type"/> must be set to <see cref="TextureType.Texture2D"/>.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// If the <see cref="GorgonTextureInfo.MultisampleInfo"/> value is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>:
    /// <list type="bullet">
    ///     <item>
    ///         <description><see cref="GorgonTextureInfo.HasReadWriteAccess"/> must not be set to <b>true</b>.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="GorgonTextureInfo.MipCount"/> must be set to 1.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <inheritdoc cref="GorgonTexture" path="/remarks/para[@type='depth_buffer_usage']"/>
    /// <para>
    /// When using the texture as a shader readable depth/stencil buffer, the <see cref="GorgonTextureInfo.IsShaderResource"/> should be set to <b>true</b>, otherwise, if it's not meant to be visible to 
    /// shaders then the <see cref="GorgonTextureInfo.IsShaderResource"/> should be set to <b>false</b>.
    /// </para>
    /// <inheritdoc cref="GorgonTexture" path="/remarks/para[@type='max_dimensions']"/>
    /// </remarks>
    /// <seealso cref="GorgonTextureCommon"/>
    /// <seealso cref="GorgonTextureInfo"/>
    /// <seealso cref="GorgonImageInfo.GetMaximumMipCount(int, int, int)"/>
    /// <seealso cref="GorgonGraphics.FormatSupport"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="TextureType"/>
    public GorgonTexture(GorgonGraphics graphics, string name, GorgonTextureInfo info)
        : base(graphics, name, info)
    {
        _uploadHeaps = graphics.Memory.UploadHeaps;
        CreateNative();
        InitializeTexture();
    }
}
