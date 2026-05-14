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
// Created: January 26, 2026 11:20:28 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a view that interprets the data in a <see cref="GorgonGpuBuffer"/> as shader constant values.
/// </summary>
/// <remarks>
/// <para>
/// Constant buffers are used to send data into the shader that changes periodically over the application lifetime. This allows an application to modify the behaviour of a shader during a frame.
/// </para>
/// <para>
/// Constant views should only be used by an application to store constant data that changes infrequently, meaning once a frame or less. Otherwise, applications should use one of the 
/// <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> methods on the command list.
/// </para>
/// <para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='constant_alignment']"/>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferView.GetViewHandle()"/>
public unsafe sealed class GorgonConstantBufferView
    : GorgonResourceView
{
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;
    private readonly uint _allocationSize;
    private readonly GpuDescriptorHeap _descriptors;

    /// <summary>
    /// The alignment, in bytes, required for constant buffer data.
    /// </summary>
    public const int AlignmentRequirement = D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT;

    /// <summary>
    /// Property to return the buffer used by this view.
    /// </summary>
    public GorgonGpuBuffer Buffer
    {
        get;
    }

    /// <summary>
    /// Function to allocate a view descriptor from the descriptor heap.
    /// </summary>
    private void AllocateDescriptors()
    {
        if (!_allocation.Equals(GpuDescriptorAllocation.Null))
        {
            _descriptors.Free(ref _allocation);
        }

        _descriptors.Allocate(1, out _allocation);

        D3D12_CONSTANT_BUFFER_VIEW_DESC view = new()
        {
            BufferLocation = Buffer.D3DResource.Get()->GetGPUVirtualAddress() + Buffer.ResourceOffset,
            SizeInBytes = _allocationSize
        };

        D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = _descriptors.D3DCpuHandle;

        cpuHandle.Offset(_allocation.Offset, _descriptors.DescriptorSize);

        Graphics.D3DDevice.Get()->CreateConstantBufferView(&view, cpuHandle);

        D3DCpuHandle = cpuHandle;
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_allocation.Equals(GpuDescriptorAllocation.Null))
            {
                Graphics.Log.Print($"Freeing descriptor handle allocation for '{Name}'.", LoggingLevel.Verbose);
                _descriptors.Free(ref _allocation);
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Function to perform validations on the constant buffer view.
    /// </summary>
    /// <param name="name">The name of the buffer.</param>
    /// <param name="alignment">The alignment, in bytes, of the buffer.</param>
    /// <param name="sizeInBytes">The total size, in bytes, of the buffer.</param>
    /// <param name="resourceOffset">The resource offset, in bytes, of the buffer within its mega buffer host.</param>
    /// <exception cref="GorgonException"><para>Thrown if the view could not be created because the buffer is smaller than the <see cref="AlignmentRequirement"/> size (256 bytes).</para>
    /// <para>Thrown if the buffer was not aligned to the <see cref="AlignmentRequirement"/> (256 bytes) upon creation.</para>
    /// </exception>
    internal static void ValidateConstantView(string name, int alignment, ulong sizeInBytes, ulong resourceOffset)
    {
        if (sizeInBytes < AlignmentRequirement)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, sizeInBytes, nameof(GorgonConstantBufferView), AlignmentRequirement));
        }

        if ((alignment != AlignmentRequirement) && ((resourceOffset % AlignmentRequirement) != 0))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, AlignmentRequirement, nameof(GorgonConstantBufferView)));
        }
    }

    /// <summary>
    /// Function to create a constant buffer view and associated buffer.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with the view and buffer.</param>
    /// <param name="name">The name of the buffer and view.</param>
    /// <param name="sizeInBytes">The size of the buffer, in bytes.</param>
    /// <param name="allowReadWriteAccess">[Optional] <b>true</b> to allow read and write access to the buffer in the shader, <b>false</b> to only allow read-only access.</param>
    /// <returns>The <see cref="GorgonConstantBufferView"/> and associated <see cref="GorgonGpuBuffer"/>.</returns>
    /// <exception cref="GorgonException"><inheritdoc cref="GorgonGpuBuffer.ValidateInfo()" path="/exception/para[2]"/></exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method used to create a <see cref="GorgonConstantBufferView"/> and its associated <see cref="GorgonGpuBuffer"/> in a single call. This takes some of the tedium out of creating 
    /// constant buffers.
    /// </para>
    /// <para>
    /// Buffers created with this method are guaranteed to be aligned to the <see cref="AlignmentRequirement"/> (256 bytes). 
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// Buffers created with this method will be disposed when the view is disposed. There is no need to dispose of the buffer directly when created by this method.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    public static GorgonConstantBufferView CreateConstantBuffer(GorgonGraphics graphics, string name, long sizeInBytes, bool allowReadWriteAccess = false)
    {
        GorgonGpuBufferInfo bufferInfo = new(sizeInBytes)
        {
            Alignment = AlignmentRequirement,
            HasReadWriteAccess = allowReadWriteAccess
        };

        GorgonGpuBuffer buffer = new(graphics, name, bufferInfo);

        try
        {
            return buffer.GetConstantBufferView(true);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Function to retrieve the handle of the view, which is used to pass to a shader for resource heap indexing.
    /// </summary>
    /// <returns>The handle of the view.</returns>
    /// <remarks>
    /// <para>
    /// This handle is meant to be passed directly into a shader via a direct constant value by the <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> method on the <see cref="GorgonCommandList"/>, 
    /// or using the value inside of another <see cref="GorgonConstantBufferView"/> (although this is not recommended). This helps facilitate Gorgon's bindless system and allows direct access to buffers and 
    /// textures in the shader via the <a href="https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_DynamicResources.html#resourcedescriptorheap-and-samplerdescriptorheap">
    /// <c>ResourceDescriptorHeap</c></a> HLSL intrinsic. 
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following code is an example of how to use Gorgon's bindless system to pass in views to a shader. The same pattern applies to all resource views, be that a <see cref="IGorgonTextureView{T}"/>, 
    /// <see cref="GorgonStructuredBufferView"/>, <see cref="GorgonConstantBufferView"/>, etc... The only view types this does not apply to is the <see cref="GorgonRenderTargetView"/> and 
    /// <see cref="GorgonDepthStencilView"/> as they are assigned on the command list directly.
    /// </para>
    /// The HLSL code:
    /// <code>
    /// <![CDATA[
    /// struct RenderingData
    /// {
    ///    int ConstantBufferHandle;
    ///    int TextureHandle;
    ///    // Other handle types...
    /// };
    ///
    /// struct ConstantData
    /// {
    ///    // Your fields here.
    /// };
    /// 
    /// ConstantBuffer<RenderingData> _renderData : register(b0);
    /// 
    /// float4 OneOfTheShaders()
    /// {
    ///   ConstantBuffer<ConstantData> data = ResourceDescriptorHeap[_renderData.ConstantBufferHandle];
    ///   Texture2D texture = ResourceDescriptorHeap[_renderData.TextureHandle];
    ///   
    ///   // Do stuff here.
    /// }
    /// ]]>
    /// </code>
    /// The C# code:
    /// <code language="csharp">
    /// <![CDATA[
    /// // This mirrors our HLSL structure.
    /// [StructLayout(LayoutKind.Sequential)]
    /// public struct RenderData
    /// {
    ///    int ConstantBufferHandle;
    ///    int TextureHandle;
    ///    // Other handle types...
    /// }
    /// 
    /// // This function sends our rendering info to the shader as needed.
    /// public void SendToShader(GorgonCommandList list, GorgonConstantBufferView cbv, GorgonTextureView tv)
    /// {
    ///    RenderData data = new()
    ///    {
    ///       ConstantBufferHandle = cbv.GetViewHandle(),  // This is how we send it.
    ///       TextureHandle = tv.GetViewHandle()
    ///    };
    ///    
    ///    // Write the data to buffer slot 0.
    ///    list.WriteConstant<RenderData>(0, in data);
    /// }    
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonRenderTargetView"/>
    /// <seealso cref="GorgonDepthStencilView"/>
    /// <seealso cref="GorgonStructuredBufferView"/>
    /// <seealso cref="IGorgonTextureView{T}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetViewHandle()
    {
        if (_allocation.Equals(in GpuDescriptorAllocation.Null))
        {
            AllocateDescriptors();
        }

        return _allocation.Offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonConstantBufferView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer">The buffer to view as a constant buffer.</param>
    /// <param name="allocationSize">The size of the buffer allocation (not the buffer size), in bytes.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonConstantBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, uint allocationSize, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonConstantBufferView))} - Constant Buffer View", buffer, owned)
    {
        Graphics.Log.Print($"Creating constant buffer view for buffer '{Name}'...", LoggingLevel.Simple);

        _descriptors = graphics.Descriptors.GpuViewDescriptors;
        _allocationSize = allocationSize;
        Buffer = buffer;

        AllocateDescriptors();
    }
}