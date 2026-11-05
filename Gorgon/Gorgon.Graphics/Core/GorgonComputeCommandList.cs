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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of commands to send to the GPU for compute operations.
/// </summary>
/// <remarks>
/// <para>
/// A compute command list records dispatches, execute calls, clears of read/write views, copies, and timestamps, and sends them to the compute queue when it is submitted. Command lists are returned by 
/// <see cref="GorgonComputeEngine.GetCommandList"/>, recorded, and then passed to <see cref="GorgonComputeEngine.Submit(GorgonComputeCommandList)"/>. Once a command list is submitted, it stops recording 
/// (see <see cref="IsRecording"/>) and cannot be used again. A new command list is retrieved for the next batch of work.
/// </para>
/// <para>
/// Work on the compute queue runs alongside the work on the graphics and copy queues. Ordering between the queues is the application's responsibility: use 
/// <see cref="GorgonComputeEngine.WaitForGraphics"/> or <see cref="GorgonComputeEngine.WaitForCopy"/> to make the compute queue wait for work on another queue, and 
/// <see cref="GorgonGraphics.WaitForCompute"/> to make the graphics queue wait for compute work.
/// </para>
/// <para>
/// The constants written to a command list are used by every dispatch and execute call recorded after them, until they are changed. Most methods return the command list, so calls can be chained 
/// (e.g. <c>list.WriteConstant(0, value).CopyValue(data, buffer)</c>).
/// </para>
/// <para>
/// Gorgon adds the barriers needed by the commands automatically, based on the resources that each command uses. The 
/// <see cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)"/> and <see cref="Reset(ReadOnlySpan{IGorgonTextureView{GorgonTextureCommon}})"/> 
/// methods are available for cases that need explicit control.
/// </para>
/// <para>
/// Compute work can also be recorded on a <see cref="GorgonCommandList"/>, where it runs on the graphics queue in order with the draws. A compute command list is used to run that work on its own queue.
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// A command list is <b>not</b> thread safe. Multiple command lists can be recorded on separate threads, as long as each command list is only used by one thread.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonComputeEngine"/>
/// <seealso cref="GorgonDispatchCall"/>
/// <seealso cref="GorgonComputeExecuteCall"/>
public sealed unsafe class GorgonComputeCommandList
    : IGorgonNamedObject, IGorgonCopyMethodsFluent<GorgonComputeCommandList>, IGorgonBarrierManagement<GorgonComputeCommandList>
{
    private string _name = string.Empty;

    /// <summary>
    /// Property to return the parent command list for the comptue command list.
    /// </summary>
    internal GorgonCommandList? ParentList
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the queue assigned to the command list.
    /// </summary>
    internal CommandQueue? Queue => ParentList?.Queue;

    /// <summary>
    /// Property to return whether the command list is recording commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A command list records commands from the time it is returned by <see cref="GorgonComputeEngine.GetCommandList"/> until it is submitted. When this value is <b>false</b>, the command list cannot be used.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputeEngine"/>
    public bool IsRecording => ParentList?.IsRecording ?? false;

    /// <summary>
    /// Property to return the compute engine interface associated with this command list.
    /// </summary>
    public GorgonComputeEngine ComputeEngine
    {
        get;
    }

    /// <inheritdoc/>
    public string Name
    {
        get => _name;
        private set
        {
            string name = GorgonGraphicsFactory.GenerateName(value, nameof(GorgonCommandList));

            if (string.Equals(name, _name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _name = name;
        }
    }

    /// <summary>
    /// Function to begin recordinf of the command list.
    /// </summary>
    /// <param name="currentFrame">The current frame for our in-flight frame values.</param>
    /// <param name="rootSignature">The global root signature for the application.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BeginRecording(int currentFrame, ref readonly ComPtr<ID3D12RootSignature> rootSignature)
    {
        Debug.Assert(ParentList is not null, $"The compute command list '{Name}' cannot record.");

        ParentList?.BeginRecording(currentFrame, in rootSignature);
    }

    /// <summary>
    /// Function called when a list is pulled from the pool.
    /// </summary>
    /// <param name="newName">The new name for the list.</param>
    /// <param name="parent">The parent command list.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResetState(string newName, GorgonCommandList? parent)
    {
        Name = newName;
        ParentList = parent;
    }

    /// <inheritdoc cref="GorgonCommandList.ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ClearReadWriteView(GorgonRawBufferRwView view, Vector128<int> values)
    {
        ParentList?.ClearReadWriteView(view, values);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.ClearReadWriteView(GorgonTypedBufferRwView, Vector128{int})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ClearReadWriteView(GorgonTypedBufferRwView view, Vector128<int> values)
    {
        ParentList?.ClearReadWriteView(view, values);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.ClearReadWriteView(GorgonTypedBufferRwView, Vector4)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ClearReadWriteView(GorgonTypedBufferRwView view, Vector4 values)
    {
        ParentList?.ClearReadWriteView(view, values);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.ClearReadWriteView(GorgonTextureRwView, Vector128{int}, IReadOnlyList{GorgonRectangle}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ClearReadWriteView(GorgonTextureRwView view, Vector128<int> values, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        ParentList?.ClearReadWriteView(view, values, clearRects);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.ClearReadWriteView(GorgonTextureRwView, Vector4, IReadOnlyList{GorgonRectangle}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ClearReadWriteView(GorgonTextureRwView view, Vector4 values, IReadOnlyList<GorgonRectangle>? clearRects = null)
    {
        ParentList?.ClearReadWriteView(view, values, clearRects);
        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList Reset(ReadOnlySpan<IGorgonTextureView<GorgonTextureCommon>> textures)
    {
        ParentList?.Reset(textures);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyValue{T}(in T, GorgonGpuBufferCommon, long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyValue<T>(in T value, GorgonGpuBufferCommon buffer, long offset = 0) where T : unmanaged
    {
        ParentList?.CopyValue(in value, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyImageToTexture(IGorgonImage, GorgonTexture)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyImageToTexture(IGorgonImage image, GorgonTexture texture)
    {
        ParentList?.CopyImageToTexture(image, texture);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyPointer{T}(GorgonPtr{T}, GorgonGpuBufferCommon, long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyPointer<T>(GorgonPtr<T> pointer, GorgonGpuBufferCommon buffer, long offset = 0) where T : unmanaged
    {
        ParentList?.CopyPointer(pointer, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyRange{T}(ReadOnlySpan{T}, GorgonGpuBufferCommon, long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyRange<T>(ReadOnlySpan<T> values, GorgonGpuBufferCommon buffer, long offset = 0) where T : unmanaged
    {
        ParentList?.CopyRange(values, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyBuffer(GorgonGpuBufferCommon source, GorgonGpuBufferCommon destination, long sourceOffset = 0, long destinationOffset = 0, long? count = null)
    {
        ParentList?.CopyBuffer(source, destination, sourceOffset, destinationOffset, count);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel = 0, short destinationZOrArrayIndex = 0, byte destinationPlane = 0)
    {
        ParentList?.CopyImageToTexture(imageBuffer, texture, destinationMipLevel, destinationZOrArrayIndex, destinationPlane);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyImageToTexture(IGorgonImageBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, short)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonVirtualTexture texture, GorgonVirtualTextureHandle handle, short destinationDepthSlice = 0)
    {
        ParentList?.CopyImageToTexture(imageBuffer, texture, handle, destinationDepthSlice);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, GorgonCopyBufferToTexture parameters)
    {
        ParentList?.CopyBufferToTexture(buffer, texture, parameters);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, GorgonCopyTextureToBuffer parameters)
    {
        ParentList?.CopyTextureToBuffer(texture, buffer, parameters);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyTexture(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters)
    {
        ParentList?.CopyTexture(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyTexture(GorgonTexture, GorgonTexture)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyTexture(GorgonTexture source, GorgonTexture destination)
    {
        ParentList?.CopyTexture(source, destination);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyTextureToVirtual(GorgonTexture source, GorgonVirtualTexture destination, ref readonly GorgonCopyTextureToVirtual parameters)
    {
        ParentList?.CopyTextureToVirtual(source, destination, in parameters);
        return this;
    }


    /// <inheritdoc cref="GorgonCommandList.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyVirtualToTexture(GorgonVirtualTexture source, GorgonTexture destination, ref readonly GorgonCopyVirtualToTexture parameters)
    {
        ParentList?.CopyVirtualToTexture(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyVirtualToVirtual(GorgonVirtualTexture source, GorgonVirtualTexture destination, ref readonly GorgonCopyVirtualToVirtual parameters)
    {
        ParentList?.CopyVirtualToVirtual(source, destination, in parameters);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.CopyBufferToVirtual(GorgonGpuBuffer, GorgonVirtualTexture, GorgonVirtualTextureHandle, long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList CopyBufferToVirtual(GorgonGpuBuffer buffer, GorgonVirtualTexture texture, GorgonVirtualTextureHandle destinationHandle, long sourceOffset = 0)
    {
        ParentList?.CopyBufferToVirtual(buffer, texture, destinationHandle, sourceOffset);
        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList SetBarrier(GorgonTextureCommon texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool force = false)
    {
        ParentList?.SetBarrier(texture, sync, access, layout, subResources, force);
        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList SetBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access, bool force = false)
    {
        ParentList?.SetBarrier(buffer, sync, access, force);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/summary"/>
    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/typeparam"/>
    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/param"/>
    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/returns"/>
    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/exception"/>
    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/remarks"/>
    /// <inheritdoc cref="GorgonCommandList.WriteConstant{T}(int, in T)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList WriteConstant<T>(int index, in T data)
        where T : unmanaged
    {
        ParentList?.WriteConstant(index, data);
        return this;
    }

    /// <summary>
    /// Function to write a timestamp into a query record.
    /// </summary>
    /// <param name="query">The query that holds the record.</param>
    /// <param name="recordIndex">The index of the record that receives the timestamp.</param>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="recordIndex"/> is less than 0, or greater than or equal to the <see cref="GorgonQuery.MaximumRecordCount"/>.</exception>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the <see cref="GorgonQuery.QueryType"/> of the <paramref name="query"/> is not <see cref="QueryType.Timestamp"/>.
    /// </para>
    /// <para>
    /// Thrown if the record belongs to another command list.
    /// </para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// The compute queue only supports <see cref="QueryType.Timestamp"/> queries. The timestamp is written when the GPU reaches this command. To convert timestamps into time, use the frequency returned by 
    /// <see cref="GorgonComputeEngine.QueryTimestampFrequency"/>, because the compute queue may run at a different frequency than the graphics queue.
    /// </para>
    /// <para>
    /// From this call, the record belongs to this command list until its data has been delivered, either to the <see cref="GorgonQuery.ReadResults"/> buffer after this command list is submitted and the GPU 
    /// has finished with it, or to a buffer through <see cref="ResolveQueryToGpuBuffer"/>. If the record has already been written on this command list, the previous timestamp is discarded.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonQuery"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ExecuteQuery(GorgonQuery query, int recordIndex)
    {
        if (query.QueryType != QueryType.Timestamp)
        {
            throw new GorgonException(GorgonResult.CannotExecute, Resources.GORGFX_ERR_COMPUTE_QUERY_NOT_TIMESTAMP);
        }

        ParentList?.BeginQuery(query, recordIndex);
        ParentList?.EndQuery(query, recordIndex);
        return this;
    }

    /// <summary>
    /// Function to copy the data for a range of query records into a GPU buffer.
    /// </summary>
    /// <param name="query">The query that holds the records.</param>
    /// <param name="recordIndex">The index of the first record to copy.</param>
    /// <param name="recordCount">The number of records to copy.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="destinationOffset">[Optional] The offset, in bytes, within the <paramref name="buffer"/> to start writing at.</param>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>Thrown if the <paramref name="recordIndex"/>, or the <paramref name="destinationOffset"/> is less than 0.</para>
    /// <para>Thrown if the <paramref name="recordCount"/> is less than 1.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <para>Thrown if the <paramref name="recordIndex"/> plus the <paramref name="recordCount"/> is greater than the <see cref="GorgonQuery.MaximumRecordCount"/>.</para>
    /// <para>Thrown if the data for the records does not fit within the <paramref name="buffer"/> at the <paramref name="destinationOffset"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this method when the timestamps are needed on the GPU, for example to be read by a shader. The records are written one after another, starting at the <paramref name="destinationOffset"/>, and each 
    /// record is a 64-bit unsigned integer (8 bytes). The <paramref name="destinationOffset"/> must be a multiple of 8 bytes.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Every record in the range must have been written with <see cref="ExecuteQuery"/> on this command list, and not already copied with this method. If a record in the range was not written on this 
    /// command list, this method does nothing. No data is copied, and the <paramref name="buffer"/> keeps whatever data it held before. A warning is written to the log naming the query, the
    /// <paramref name="buffer"/>, and the index of the record that was not written. If the <paramref name="buffer"/> does not contain the expected data, check the log for this warning.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Records copied with this method are not written to the <see cref="GorgonQuery.ReadResults"/> buffer, and they are released as soon as this method is called, so they do not affect the 
    /// <see cref="GorgonQuery.IsReady"/> property. A released record can be written again right away, on this command list or another. The GPU runs command lists in the order they are submitted, so a 
    /// command list that writes one of these records again must be submitted after this one, otherwise the data copied into the <paramref name="buffer"/> is undefined.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonQuery"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList ResolveQueryToGpuBuffer(GorgonQuery query, int recordIndex, int recordCount, GorgonGpuBuffer buffer, int destinationOffset = 0)
    {
        ParentList?.ResolveQueryToGpuBuffer(query, recordIndex, recordCount, buffer, destinationOffset);
        return this;
    }

    /// <summary>
    /// Function to set a buffer value that decides whether the GPU skips the commands recorded after this call.
    /// </summary>
    /// <param name="buffer">The buffer that holds the predication value, or <b>null</b> to turn predication off.</param>
    /// <param name="offset">[Optional] The offset, in bytes, of the predication value within the <paramref name="buffer"/>.</param>
    /// <param name="operation">[Optional] The operation that decides, from the predication value, whether commands are skipped.</param>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the size of the predication value (8 bytes) exceeds the size of the <paramref name="buffer"/>.</exception>
    /// <remarks>
    /// <para>
    /// Predication lets the GPU decide, rather than the CPU, whether commands are executed. The predication value is a 64-bit unsigned integer stored in the <paramref name="buffer"/> at the
    /// <paramref name="offset"/>, which must be a multiple of 8 bytes. If the <paramref name="offset"/> is left at its default, the value is read from the start of the <paramref name="buffer"/>. The
    /// <paramref name="operation"/> compares the value against 0 to decide whether commands are skipped. If the <paramref name="operation"/> is left at its default, commands are skipped when the value is 0.
    /// </para>
    /// <para>
    /// Any buffer can be used to hold the predication value, including one written by a shader, or one that received the result of an occlusion query recorded on a <see cref="GorgonCommandList"/>.
    /// </para>
    /// <para>
    /// The GPU reads the predication value when it reaches this command. Changing the contents of the <paramref name="buffer"/> afterward has no effect on commands that follow, until this method is called 
    /// again.
    /// </para>
    /// <para>
    /// Predication stays active on this command list until this method is called with a <paramref name="buffer"/> of <b>null</b>, or the command list is submitted. Every command list starts with predication 
    /// turned off.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// While predication is active, every dispatch, execute, clear, copy and write recorded on this command list is skipped when the predication value says so, not just dispatches. Turn predication off 
    /// before recording commands that must always run. Timestamps and query resolves are never skipped.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following code only runs a dispatch if a shader wrote a non-zero value into the predication buffer.
    /// </para>
    /// <code language="csharp">
    /// <![CDATA[
    /// computeCommandList.Dispatch(writeFlagDispatchCall);   // Writes 0 or 1 into predicationBuffer.
    ///
    /// computeCommandList.SetPredication(predicationBuffer, 0, PredicationOperation.SkipIfZero);
    /// computeCommandList.Dispatch(expensiveDispatchCall);
    /// computeCommandList.SetPredication(null);
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="PredicationOperation"/>
    public GorgonComputeCommandList SetPredication(GorgonGpuBufferCommon? buffer, long offset = 0, PredicationOperation operation = PredicationOperation.SkipIfZero)
    {
        ParentList?.SetPredication(buffer, offset, operation);
        return this;
    }

    /// <summary>
    /// Function to copy the data in a <see cref="GorgonGpuUploadMemory"/> into a buffer.
    /// </summary>
    /// <param name="upload">The transient GPU memory containing the data to copy.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="offset">[Optional] The offset, in bytes, within the <paramref name="buffer"/> to start writing into.</param>
    /// <inheritdoc cref="ClearReadWriteView(GorgonRawBufferRwView, Vector128{int})" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the size of the <paramref name="upload"/> memory, in bytes, exceeds the size of the 
    /// <paramref name="buffer"/>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="upload"/> memory is no longer available for use.</exception>
    /// <remarks>
    /// <para>
    /// Upload memory is written directly by the application (see <see cref="GorgonGpuUploadMemory(GorgonComputeEngine, long)"/>), so large amounts of data can be sent to a buffer without copying it into 
    /// an intermediate array first. This method copies the whole <paramref name="upload"/> memory into the <paramref name="buffer"/>. To copy a portion of it, pass a slice of the memory (see 
    /// <see cref="GorgonGpuUploadMemory.Slice(long, long?)"/>).
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='list_gpu']"/>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following example writes a large block of data directly into upload memory, and then copies it into a buffer that the compute shader reads through its view handle.
    /// </para>
    /// <code language="csharp">
    /// <![CDATA[
    /// GorgonComputeCommandList list = compute.GetCommandList();
    ///
    /// int particleSize = Unsafe.SizeOf<Particle>();
    /// GorgonGpuUploadMemory upload = new(compute, particleCount * particleSize);
    ///
    /// // Generate the particles straight into the upload memory, without building an array first.
    /// for (int i = 0; i < particleCount; ++i)
    /// {
    ///     upload.Write(GenerateParticle(i), i * particleSize);   // The offset is in bytes.
    /// }
    ///
    /// // Copy the upload memory into the particle buffer. The copy runs on the GPU when the command list is submitted,
    /// // before the dispatches recorded after it.
    /// list.UploadGpuMemoryToBuffer(in upload, particleBuffer);
    ///
    /// list.Dispatch(dispatchCall);
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuUploadMemory"/>
    /// <seealso cref="GorgonGpuUploadMemory(GorgonComputeEngine, long)"/>
    public GorgonComputeCommandList UploadGpuMemoryToBuffer(ref readonly GorgonGpuUploadMemory upload, GorgonGpuBufferCommon buffer, long offset = 0)
    {
        ParentList?.UploadGpuMemoryToBuffer(in upload, buffer, offset);
        return this;
    }

    /// <inheritdoc cref="GorgonCommandList.Dispatch(GorgonDispatchCall)" path="/summary"/>
    /// <inheritdoc cref="GorgonCommandList.Dispatch(GorgonDispatchCall)" path="/param"/>
    /// <inheritdoc cref="GorgonCommandList.Dispatch(GorgonDispatchCall)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonCommandList.Dispatch(GorgonDispatchCall)" path="/remarks/para[@type='dispatch']"/>
    /// </remarks>
    /// <inheritdoc cref="GorgonCommandList.Dispatch(GorgonDispatchCall)" path="/seealso"/>
    public void Dispatch(GorgonDispatchCall dispatchCall) => ParentList?.Dispatch(dispatchCall);

    /// <inheritdoc cref="GorgonCommandList.Execute(GorgonComputeExecuteCall)" path="/summary"/>
    /// <inheritdoc cref="GorgonCommandList.Execute(GorgonComputeExecuteCall)" path="/param"/>
    /// <inheritdoc cref="GorgonCommandList.Execute(GorgonComputeExecuteCall)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonCommandList.Execute(GorgonComputeExecuteCall)" path="/remarks/para[@type='execute']"/>
    /// </remarks>
    /// <inheritdoc cref="GorgonCommandList.Execute(GorgonComputeExecuteCall)" path="/seealso"/>
    public void Execute(GorgonComputeExecuteCall executeCall) => ParentList?.Execute(executeCall);

    /// <summary>
    /// Initializes a new instance of a <see cref="GorgonComputeCommandList"/> class.
    /// </summary>
    /// <param name="computeEngine">The compute engine interface that owns this object.</param>
    /// <param name="name">The name of the command list.</param>
    /// <param name="parent">The parent command list.</param>
    internal GorgonComputeCommandList(GorgonComputeEngine computeEngine, string name, GorgonCommandList parent)
    {
        ParentList = parent;
        ComputeEngine = computeEngine;
        Name = name;
    }
}