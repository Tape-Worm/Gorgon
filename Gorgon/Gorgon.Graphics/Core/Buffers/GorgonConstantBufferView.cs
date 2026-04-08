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
using Gorgon.Native;
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
            Graphics.GpuViewDescriptors.Free(ref _allocation);
        }

        Graphics.GpuViewDescriptors.Allocate(1, out _allocation);

        D3D12_CONSTANT_BUFFER_VIEW_DESC view = new()
        {
            BufferLocation = Buffer.D3DResource.Get()->GetGPUVirtualAddress() + Buffer.ResourceOffset,
            SizeInBytes = (uint)Buffer.SizeInBytes
        };

        D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = Graphics.GpuViewDescriptors.D3DCpuHandle;
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.GpuViewDescriptors.D3DGpuHandle;

        cpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);
        gpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);

        Graphics.D3DDevice.Get()->CreateConstantBufferView(&view, cpuHandle);

        SetHandles(cpuHandle, gpuHandle);
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_allocation.Equals(GpuDescriptorAllocation.Null))
            {
                Graphics.Log.Print($"Freeing descriptor handle allocation for '{Name}'.", LoggingLevel.Verbose);
                Graphics.GpuViewDescriptors.Free(ref _allocation);
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected override void OnReset() => AllocateDescriptors();

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
    internal static void ValidateConstantView(string name, int alignment, long sizeInBytes, ulong resourceOffset)
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
    /// <param name="bufferInfo">The information used to build the underlying buffer.</param>
    /// <returns>The <see cref="GorgonConstantBufferView"/> and associated <see cref="GorgonGpuBuffer"/>.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method used to create a <see cref="GorgonGpuBuffer"/> and an associated <see cref="GorgonConstantBufferView"/>. 
    /// </para>
    /// <para>
    /// The <see cref="IGorgonCommonBufferInfo.SizeInBytes"/> on the <paramref name="bufferInfo"/> parameter must be aligned to the nearest 256 bytes. If it is not, then this method will automatically create 
    /// the buffer with a size that is aligned to the nearest 256 bytes.
    /// </para>
    /// <para>
    /// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView()" path="/remarks/para/note"/>
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
    public static GorgonConstantBufferView CreateConstantBuffer(GorgonGraphics graphics, string name, GorgonGpuBufferInfo bufferInfo)
    {
        if ((bufferInfo.IsUnorderedAccess) || (bufferInfo.Alignment == 0) || ((bufferInfo.Alignment % AlignmentRequirement) != 0))
        {
            bufferInfo = bufferInfo with
            {
                IsUnorderedAccess = false,
                Alignment = AlignmentRequirement
            };
        }

        if ((bufferInfo.SizeInBytes % AlignmentRequirement) != 0)
        {
            long aligned = bufferInfo.SizeInBytes.AlignUp(AlignmentRequirement);
            graphics.Log.PrintWarning($"The constant buffer '{name}' has a size of {bufferInfo.SizeInBytes}. This is not aligned to {AlignmentRequirement} bytes. The buffer size will be adjusted to an aligned value of {aligned} bytes.", 
                LoggingLevel.Intermediate);

            bufferInfo = bufferInfo with
            {
                SizeInBytes = aligned,
                // Double check to ensure that alignment is what it should be for constant buffers.
                Alignment = AlignmentRequirement
            };
        }

        ValidateConstantView(name, bufferInfo.Alignment, bufferInfo.SizeInBytes, 0);

        GorgonGpuBuffer buffer = new(graphics, name, bufferInfo);
        return buffer.GetConstantBufferView(true);
    }

    /// <summary>
    /// Function to retrieve the handle of the view, which is used to pass to a shader for resource heap indexing.
    /// </summary>
    /// <returns>The handle of the view.</returns>
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
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonConstantBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonConstantBufferView))} - Constant Buffer View", buffer, owned)
    {
        Graphics.Log.Print($"Creating constant buffer view for buffer '{Name}'...", LoggingLevel.Simple);

        Buffer = buffer;

        AllocateDescriptors();
    }
}