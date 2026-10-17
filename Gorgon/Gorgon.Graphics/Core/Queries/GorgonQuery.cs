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
// Created: October 13, 2026 1:26:10 PM
//

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of data that a query records.
/// </summary>
/// <remarks>
/// <para>
/// Every slot in a query records the same type of data. Occlusion, binary occlusion and timestamp queries produce one 64-bit unsigned integer per slot. Pipeline statistics queries produce one 
/// <see cref="GorgonQueryPipelineStats"/> value per slot.
/// </para>
/// </remarks>
/// <seealso cref="GorgonQueryPipelineStats"/>
public enum QueryType
{
    /// <summary>
    /// The query counts the number of samples that passed the depth and stencil tests.
    /// </summary>
    Occlusion = D3D12_QUERY_TYPE.D3D12_QUERY_TYPE_OCCLUSION,
    /// <summary>
    /// The query records 1 if at least one sample passed the depth and stencil tests, or 0 if none did.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this instead of <see cref="Occlusion"/> when only visibility matters. Because it does not count samples, it does not interfere with the optimizations the GPU uses for depth and stencil testing.
    /// </para>
    /// </remarks>
    BinaryOcclusion = D3D12_QUERY_TYPE.D3D12_QUERY_TYPE_BINARY_OCCLUSION,
    /// <summary>
    /// The query records the value of the GPU's clock, in ticks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single timestamp has no meaning on its own. The difference between two timestamps is the GPU time that passed between them.
    /// </para>
    /// </remarks>
    Timestamp = D3D12_QUERY_TYPE.D3D12_QUERY_TYPE_TIMESTAMP,
    /// <summary>
    /// The query records the counts of work done by each stage of the pipeline.
    /// </summary>
    /// <seealso cref="GorgonQueryPipelineStats"/>
    PipelineStatistics = D3D12_QUERY_TYPE.D3D12_QUERY_TYPE_PIPELINE_STATISTICS
}

/// <summary>
/// A query used to record information about the work done by the GPU, such as visibility, timing and pipeline statistics.
/// </summary>
/// <remarks>
/// <para>
/// A query holds a fixed number of records, set by the <see cref="MaximumRecordCount"/> property, and identified by their index. Every record holds the same type of data, which is defined by the 
/// <see cref="QueryType"/> property.
/// </para>
/// <para>
/// To record data, call <see cref="GorgonCommandList.BeginQuery"/> with a record index, record the commands to measure, and then call <see cref="GorgonCommandList.EndQuery"/> with the same index on the same 
/// command list. Every record that is begun on a command list must be ended before that command list is submitted.
/// </para>
/// <para>
/// When the command list is submitted with <see cref="GorgonGraphics.Submit(GorgonCommandList)"/>, the ended records are copied into the <see cref="ReadResults"/> buffer after the GPU has finished with them. 
/// The <see cref="IsReady"/> property indicates when that data can be read. If the data is needed on the GPU instead, the <see cref="GorgonCommandList.ResolveQueryToGpuBuffer"/> method copies the records into 
/// a <see cref="GorgonGpuBuffer"/>.
/// </para>
/// <para>
/// Records can be recorded on multiple command lists at the same time, including command lists on other threads, as long as each command list uses different record indices. A record belongs to the command 
/// list that began it until its data has been delivered, either to the <see cref="ReadResults"/> buffer or to a buffer through <see cref="GorgonCommandList.ResolveQueryToGpuBuffer"/>. Until then, no other 
/// command list can begin that record. Once the data has been delivered, the record can be used again.
/// </para>
/// <para>
/// Calls that do not make sense for the current state of a record, such as ending a record that was never begun, are ignored rather than treated as errors. The query does not attempt to correct for this, and 
/// the data for the affected records is undefined.
/// </para>
/// </remarks>
/// <example>
/// <para>
/// The following code measures how long the GPU takes to process a set of commands.
/// </para>
/// <code language="csharp">
/// <![CDATA[
/// GorgonQuery timer = new(graphics, "Frame timer", QueryType.Timestamp, 2);
///
/// GorgonCommandList commandList = graphics.GetCommandList();
/// commandList.BeginQuery(timer, 0)
///            .EndQuery(timer, 0);
///
/// // Record the commands to measure here.
///
/// commandList.BeginQuery(timer, 1)
///            .EndQuery(timer, 1);
///
/// graphics.Submit(commandList);
///
/// // On a later frame:
/// if (timer.IsReady)
/// {
///     ulong start = timer.ReadResults.AsRef<ulong>(0);
///     ulong end = timer.ReadResults.AsRef<ulong>(sizeof(ulong));
///     double milliseconds = (end - start) * 1000.0 / graphics.QueryTimestampFrequency;
/// }
/// ]]>
/// </code>
/// </example>
/// <seealso cref="QueryType"/>
/// <seealso cref="GorgonQueryPipelineStats"/>
/// <seealso cref="GorgonCommandList"/>
public unsafe sealed class GorgonQuery
    : IGorgonNamedObject, IDisposable
{
    private ComPtr<ID3D12QueryHeap> _queryHeap;
    private ComPtr<D3D12MA_Allocation> _bufferAllocation;
    private ComPtr<ID3D12Resource2> _readbackBuffer;

    private readonly GorgonCommandList?[] _slots;
    private int _allocatedSlots = -1;
    private byte* _readbackPtr;

    /// <summary>
    /// Property to return the D3D COM pointer for the query heap.
    /// </summary>
    internal ref readonly ComPtr<ID3D12QueryHeap> D3DQueryHeap => ref _queryHeap;

    /// <summary>
    /// Property to return the D3D COM pointer for the read back buffer.
    /// </summary>
    internal ref readonly ComPtr<ID3D12Resource2> D3DReadbackBuffer => ref _readbackBuffer;

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the graphics interface associated with this query.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the data from the query that can be read by the CPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The data for a record starts at its index multiplied by the size of a record, in bytes. For <see cref="QueryType.Occlusion"/>, <see cref="QueryType.BinaryOcclusion"/> and 
    /// <see cref="QueryType.Timestamp"/> queries, each record is a 64-bit unsigned integer. For <see cref="QueryType.PipelineStatistics"/> queries, each record is a <see cref="GorgonQueryPipelineStats"/> 
    /// value.
    /// </para>
    /// <para>
    /// Records are copied into this buffer after the command list that ended them is submitted, and the GPU has finished with them. Records that were never recorded, or that were copied to a GPU buffer with 
    /// <see cref="GorgonCommandList.ResolveQueryToGpuBuffer"/>, keep whatever data was in the buffer before.
    /// </para>
    /// <para>
    /// This buffer is owned by the query, and is no longer valid once the query is disposed.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Only read from this buffer when the <see cref="IsReady"/> property is <b>true</b>. Before then, the data may be left over from a previous use of the query, or may be in the process of being written by 
    /// the GPU.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="IsReady"/>
    public GorgonNativeBuffer<byte> ReadResults
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the maximum number of records the query can hold.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <see cref="QueryType.Occlusion"/>, <see cref="QueryType.BinaryOcclusion"/> and <see cref="QueryType.Timestamp"/> queries, each record is 8 bytes. For <see cref="QueryType.PipelineStatistics"/> 
    /// queries, each record is the size of a <see cref="GorgonQueryPipelineStats"/> value. The size of the <see cref="ReadResults"/> buffer is this value multiplied by the size of a record.
    /// </para>
    /// </remarks>
    public int MaximumRecordCount
    {
        get;
    }

    /// <summary>
    /// Property to return whether the data for the query can be read from the <see cref="ReadResults"/> property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value applies to the query as a whole. It is <b>true</b> only when no record in the query belongs to a command list, which means every record that was recorded has had its data delivered. It is 
    /// <b>false</b> for a query that has never been used, and it becomes <b>false</b> again as soon as any record is begun with <see cref="GorgonCommandList.BeginQuery"/>.
    /// </para>
    /// <para>
    /// This value is updated when a frame is submitted with <see cref="GorgonGraphics.Submit(GorgonCommandList)"/>. Data is delivered on the first submission after the GPU has finished with it, so the query 
    /// will be ready one or more frames after the records were submitted. Calling <see cref="GorgonGraphics.WaitForGpu"/> does not update this value on its own.
    /// </para>
    /// <para>
    /// Records copied to a GPU buffer with <see cref="GorgonCommandList.ResolveQueryToGpuBuffer"/> are released when that method is called, and do not keep this value <b>false</b>. Their data is not written 
    /// to the <see cref="ReadResults"/> buffer.
    /// </para>
    /// <para>
    /// Because beginning any record makes the query busy, read the results before recording into the query again. A query that is recorded into every frame may never become ready. To measure every frame, use 
    /// a separate query for each frame in flight (see <see cref="GorgonGraphics.InFlightFrameCount"/>).
    /// </para>
    /// </remarks>
    /// <seealso cref="ReadResults"/>
    public bool IsReady => Volatile.Read(in _allocatedSlots) == 0;

    /// <summary>
    /// Property to return the type of records in this query.
    /// </summary>
    public QueryType QueryType
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);

            // Ensure the GPU is done its work before dispose any query heaps.
            Graphics.WaitForGpu();

            ReadResults.Dispose();

            Graphics.Log.Print($"Destroying {nameof(ID3D12QueryHeap)} '{Name}'.", LoggingLevel.Verbose);
        }

        _bufferAllocation.Dispose();
        _readbackBuffer.Dispose();
        _readbackPtr = null;
        _queryHeap.Dispose();
    }

    /// <summary>
    /// Function to create the native COM pointer for the query heap.
    /// </summary>
    /// <returns>The pointer to the readback buffer.</returns>
    private GorgonPtr<byte> CreateNative()
    {
        string queryName = $"Gorgon D3D 12 Query Heap '{Name}'";
        string readbackName = $"Gorgon D3D 12 Query Readback Buffer '{Name}'";
        ComPtr<ID3D12QueryHeap> heap = default;
        ComPtr<D3D12MA_Allocation> allocation = default;
        ComPtr<ID3D12Resource2> readback = default;

        D3D12_QUERY_HEAP_DESC desc = new()
        {
            Count = (uint)MaximumRecordCount,
            Type = QueryType switch
            {
                QueryType.BinaryOcclusion or QueryType.Occlusion => D3D12_QUERY_HEAP_TYPE.D3D12_QUERY_HEAP_TYPE_OCCLUSION,
                QueryType.PipelineStatistics => D3D12_QUERY_HEAP_TYPE.D3D12_QUERY_HEAP_TYPE_PIPELINE_STATISTICS,
                _ => D3D12_QUERY_HEAP_TYPE.D3D12_QUERY_HEAP_TYPE_TIMESTAMP
            }
        };

        Graphics.Log.Print($"Creating {nameof(ID3D12QueryHeap)} '{Name}'...", LoggingLevel.Verbose);

        Graphics.D3DDevice.Get()->CreateQueryHeap(&desc, Win32.__uuidof<ID3D12QueryHeap>(), (void**)heap.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_QUERY);

        _queryHeap = heap;

        heap.SetD3DDebugName(queryName);

        D3D12_RESOURCE_FLAGS flags = Graphics.Adapter.HasTightAlignmentSupport ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;
        D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK);
        D3D12_RESOURCE_DESC1 resDesc = D3D12_RESOURCE_DESC1.Buffer((ulong)MaximumRecordCount * (ulong)(QueryType == QueryType.PipelineStatistics ? sizeof(GorgonQueryPipelineStats) : sizeof(ulong)), flags);
        Graphics.Memory.Allocator.Get()->CreateResource3(&allocDesc, &resDesc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED, null, 0, null, allocation.GetAddressOf(), Win32.__uuidof<ID3D12Resource2>(), (void**)readback.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_QUERY);

        _bufferAllocation = allocation;
        _readbackBuffer = readback;
        _readbackBuffer.SetD3DDebugName(readbackName);
        byte* readbackPtr = null;
        _readbackBuffer.Get()->Map(0, null, (void**)&readbackPtr)
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_QUERY);
        _readbackPtr = readbackPtr;

        return new GorgonPtr<byte>(readbackPtr, (long)resDesc.Width);            
    }

    /// <summary>
    /// Function to reserve a range of query records.
    /// </summary>
    /// <param name="commandList">The command list to assign to the record.</param>
    /// <param name="index">The index of the record to allocate.</param>
    /// <returns><b>true</b> if the allocation succeeded, <b>false</b> if it failed because the command list already owns the index.</returns>
    internal bool Allocate(GorgonCommandList commandList, int index)
    {        
        GorgonCommandList? owner = Interlocked.CompareExchange(ref _slots[index], commandList, null);

        if (owner == commandList)
        {
            return false;
        }

        if (owner is not null)
        {
            throw new GorgonException(GorgonResult.AlreadyInitialized, string.Format(Resources.GORGFX_ERR_QUERY_CANNOT_ALLOCATE, index, owner.Name));
        }

        Interlocked.CompareExchange(ref _allocatedSlots, 0, -1);
        Interlocked.Increment(ref _allocatedSlots);

        return true;
    }

    /// <summary>
    /// Function to free a range of query records.
    /// </summary>
    /// <param name="index">The index of the record to allocate.</param>
    internal void Free(int index)
    {
        if (Interlocked.Exchange(ref _slots[index], null) is not null)
        {
            Interlocked.Decrement(ref _allocatedSlots);                
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// <note type="important">
    /// <para>
    /// This method waits for the GPU to finish all of its work before the query is destroyed, which stalls the CPU until the GPU is idle. Avoid disposing of queries inside of the render loop.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        Graphics.Log.Print($"Destroying {nameof(GorgonQuery)} '{Name}'.", LoggingLevel.Simple);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonQuery() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonQuery"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this query.</param>
    /// <param name="name">The name of the query.</param>
    /// <param name="queryType">The type of query.</param>
    /// <param name="maximumRecordCount">The number of query records that can be held.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="maximumRecordCount"/> is less than 1.</exception>
    /// <exception cref="GorgonException">Thrown if the query could not be created.</exception>
    public GorgonQuery(GorgonGraphics graphics, string name, QueryType queryType, int maximumRecordCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumRecordCount, 1);        

        Graphics = graphics;       

        Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonQuery));
        QueryType = queryType;
        MaximumRecordCount = maximumRecordCount;
        _slots = new GorgonCommandList?[MaximumRecordCount];

        Graphics.Log.Print($"Creating {nameof(GorgonQuery)} '{Name}' with a maximum of {MaximumRecordCount} records...", LoggingLevel.Simple);

        ReadResults = new GorgonNativeBuffer<byte>(CreateNative());       

        this.RegisterDisposable(Graphics);
    }
}