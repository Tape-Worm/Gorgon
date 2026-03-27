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
// Created: January 25, 2026 8:43:01 PM
//

using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Native;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;


/// <summary>
/// Provides a fluent interface for the <see cref="GorgonResourceCopier"/>.
/// </summary>
/// <remarks>
/// <para>
/// This interface performs copies to a <see cref="GorgonGpuBuffer_OLDE"/> or <see cref="GorgonTexture"/> by scheduling an upload of one or more write operations on the GPU copy queue. When a write is performed, a 
/// temporary buffer is allocated and the application data is copied into that buffer. When the application calls the <see cref="End"/> (or <see cref="EndAsync"/>) method, then that temporary buffer is 
/// uploaded into the buffer resource on the GPU. After this, the temporary buffer is freed.
/// </para>
/// <para>
/// When the copy to the GPU buffer resource is performed, the object will stall the CPU (this can be awaited using the <see cref="EndAsync"/> method) until the GPU is finished with its upload. Because of 
/// this, performance will suffer when the methods on this object are called in a tight loop (such as a game loop). The ideal use for this type is for asynchronous initialization of resources.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGpuResource"/>
/// <seealso cref="GorgonGpuBuffer_OLDE"/>
/// <seealso cref="GorgonTexture"/>
public interface IGorgonResourceWriter
{
    /// <summary>
    /// Function to end the batch and perform any pending uploads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method must be called after a call to the <see cref="GorgonResourceCopier"/>.<see cref="GorgonResourceCopier.BeginUpload"/> method. Failure to do so can lead to data not being sent the GPU properly.
    /// </para>
    /// <para>
    /// Data written to a <see cref="GorgonGpuBuffer_OLDE"/> may be immediate, depending on the buffer <see cref="BufferUsage"/>. For buffers that have a <see cref="BufferUsage.DynamicPerFrame"/> or 
    /// <see cref="BufferUsage.Upload"/> usage, calls to the <c>Write</c> methods on this interface will be immediately sent to the buffer. Otherwise, the GPU will copy the data from the CPU to a GPU upload 
    /// buffer, and then a copy from the upload buffer to the <see cref="BufferUsage.Default"/> buffer. This is all handled transparently by this method.
    /// </para>
    /// <para>
    /// This method will not return until the GPU is finished its upload(s).
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="BufferUsage"/>
    /// <example>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBuffer_OLDE, long)"/>
    /// </example>
    void End();

    /// <summary>
    /// Function to asynchronously end the batch and perform any pending uploads.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> for asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method takes advantage of the asynchronous nature of the GPU and CPU. Applications can use this method to let the GPU finish uploading data without blocking the CPU (the <see cref="End"/> method 
    /// blocks until the GPU is done). This can be useful in a scenario where many uploads to the GPU need to be scheduled on multiple CPU threads.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This is an advanced method. Any buffers that are being written using this uploader must <b>not</b> be used until the GPU is finished with it. Failure to do so can lead to race conditions.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// <inheritdoc cref="End" path="/remarks/para[1]"/>
    /// </para>
    /// <para>
    /// <inheritdoc cref="End" path="/remarks/para[2]"/>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="BufferUsage"/>
    /// <example>    
    /// <code lang="csharp">
    /// <![CDATA[
    /// // This example is here to give an idea how async can be used to upload a lot of data to GPU buffers.
    /// // It should not be considered production code.
    /// async Task<GorgonGpuBuffer> WriteTheDataAsync(GorgonNativeBuffer<byte>[] buffers)
    /// {
    ///     GorgonGpuBuffer[] result = new GorgonGpuBuffer[buffers.Length];
    ///     GorgonGpuBufferWriter[] writers = new GorgonGpuBufferWriter[buffers.Length];
    ///     Task[] tasks = new Task[buffers.Length];
    ///     
    ///     // Initialize.
    ///     for (int i = 0; i < buffers.Length; ++i)
    ///     {
    ///         result[i] = new GorgonGpuBuffer(_graphics, $"Buffer {i}", new GorgonGpuBufferInfo(buffers[i].Length, BufferUsage.Default));
    ///         writers[i] = new GorgonGpuBufferWriter(_graphics);
    ///         
    ///         // Start writing.
    ///         writers[i].Begin().CopyPointer<byte>(buffers[i], result[i]);
    ///         tasks[i] = writers[i].EndAsync();        
    ///     }
    ///     
    ///     // Wait for all buffers to write to the GPU.
    ///     // If this takes a long time, the CPU is free to do other work.
    ///     await Task.WhenAll(tasks);
    /// 
    ///     // Clean up.
    ///     for (int i = 0; i < writers.Length; ++i)
    ///     {
    ///         writers[i].Dispose();
    ///     }
    ///     return result;
    /// }
    /// ]]>
    /// </code>
    /// </example>
    ValueTask EndAsync();

    /// <summary>
    /// Function to set a barrier on a texture to enforce synchronization.
    /// </summary>
    /// <param name="texture">The texture to assign the barrier to.</param>
    /// <param name="sync">The new synchronization state for the barrier.</param>
    /// <param name="access">The new access level for the barrier.</param>
    /// <param name="layout">The new layout for the texture data.</param>
    /// <param name="subResources">[Optional] The individual sub resource on the texture to barrier.</param>
    /// <param name="discard">[Optional] <b>true</b> to force a discard operation on the resource, <b>false</b> to leave as-is.</param>
    /// <param name="force">[Optional] <b>true</b> to force the barrier to apply right away, <b>false</b> to wait until the barrier is required.</param>
    /// <returns>The fluent interface for the for the resource writer.</returns>
    /// <remarks>
    /// <para>
    /// Most operations in Gorgon automatically apply barriers for resources on the user's behalf. However, there may be times the end user may need a more optimal barrier strategy, or Gorgon cannot set the 
    /// barriers correctly. This method allows users to bypass the automatic barrier management.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para><h3>What is a barrier? What is it for?</h3></para>
    /// <para>
    /// Modern GPUs may require their resources to be in specific states (e.g. compressed, uncompressed, etc...) prior to modification, and they also run operations in parallel. To prevent hazards for 
    /// read-after-write, write-after-read, and write-after-write operations, applications can provide barriers to indicate that they intend to perform a certain action, requiring certain access types and 
    /// data layout. This allows the GPU to perform the necessary operations required to ensure the data is synchronized. 
    /// </para>
    /// <para>
    /// For more detailed information, please refer to <a target="_blank">https://microsoft.github.io/DirectX-Specs/d3d/D3D12EnhancedBarriers.html</a>.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The synchronization bits provided by the <paramref name="sync"/> parameter indicate what synchronization we require before accessing the data. Thess bits can be combined to provide multiple 
    /// synchronization types.
    /// </para>
    /// <para>
    /// The access bits provided provided by the <paramref name="access"/> parameter indicates what type of access we require before accessing the data. Since GPUs cache a lot of their write operations, 
    /// this allows the GPU to ensure that the caches are correctly flushed. Thess bits can be combined to provide multiple access types.
    /// </para>
    /// <para type="TextureBarrier">
    /// The <paramref name="layout"/> transition is for <see cref="GorgonTexture"/> objects only. This value indicates the type of read/write operation being performed so the GPU can compress or 
    /// decompress its data (depending on the GPU architecture).
    /// </para>
    /// <para type="TextureBarrier">
    /// The barrier can be applied to sub resources of a <see cref="GorgonTexture"/> by assigning a value to the <paramref name="subResources"/> parameter. If this value is omitted, then the entire resource 
    /// has the barrier applied instead of a portion of it. By assigning a <see cref="GorgonSubResourceRange"/> to this barrier, the GPU can allow work on another portion of the texture while working on 
    /// the sub resource passed to this method.
    /// </para>
    /// <para type="TextureBarrier">
    /// The <paramref name="discard"/> value indicates that the contents of the resource can be discarded/ignored when this value is <b>true</b>. This is only available for initial barriers on a 
    /// <see cref="GorgonTexture"/> resource.
    /// </para>
    /// <para>
    /// When a barrier is set on a resource, it is queued in a list of barriers and when it comes time to make use of the resource in a <see cref="GorgonCommandList"/> operation. This operation is automatic.
    /// If this value is set to <b>true</b>, then that queue is processed immediately after this barrier is set. This allows the user to control when barriers are applied. Queuing of barriers until the last 
    /// moment is a performance optimization for some GPUs.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonCommandList"/>
    IGorgonResourceWriter SetBarrier(GorgonTexture texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool discard = false, bool force = false);

    /// <summary>
    /// Function to set a barrier on a buffer to enforce synchronization.
    /// </summary>
    /// <param name="buffer">The buffer to assign the barrier to.</param>
    /// <param name="sync"><inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='sync']"/></param>
    /// <param name="access"><inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='access']"/></param>
    /// <param name="force"><inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='force']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/remarks/para[not(@type='TextureBarrier')]"/>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonCommandList"/>
    IGorgonResourceWriter SetBarrier(GorgonGpuBuffer buffer, BarrierSync sync, BarrierAccess access, bool force = false);

    /// <summary>
    /// Function to set a barrier on a buffer to enforce synchronization.
    /// </summary>
    /// <param name="buffer">The buffer to assign the barrier to.</param>
    /// <param name="sync"><inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='sync']"/></param>
    /// <param name="access"><inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='access']"/></param>
    /// <param name="force"><inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/param[@name='force']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/remarks/para[not(@type='TextureBarrier')]"/>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="GorgonCommandList"/>
    [Obsolete("For old buffers.")]
    IGorgonResourceWriter SetBarrier(GorgonGpuBuffer_OLDE buffer, BarrierSync sync, BarrierAccess access, bool force = false);

    /// <summary>
    /// Function to copy a single value into a buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">[Optional] The offset, in bytes, within the <paramref name="buffer"/> to start writing at.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonResourceCopier.ValidateRangeParams(GorgonGpuBuffer_OLDE, long, long, int)" path="/exception[not(@cref='T:Gorgon.Core.GorgonException')]"/>
    /// <exception cref="GorgonException"><para><inheritdoc cref="GorgonResourceCopier.ValidateRangeParams(GorgonGpuBuffer_OLDE, long, long, int)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/node()"/></para>
    /// <para>-or-</para>
    /// <para>Thrown if the <see cref="GorgonResourceCopier.BeginUpload"/> method was not called prior to calling this method.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method writes a single <paramref name="value"/> of the <typeparamref name="T"/> type into the <paramref name="buffer"/>. Applications can use this to write a single value type to a buffer at 
    /// the specified <paramref name="offset"/>. 
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// If the <typeparamref name="T"/> type is larger than 16 bytes in size, then the value should passed using the <see langword="in"/> parameter modifier for improved performance.
    /// </para>
    /// </note>
    /// </para>
    /// <para type="CopyCommon">
    /// If the <typeparamref name="T"/> type is a custom struct type, then that struct type should be decorated by the <see cref="StructLayoutAttribute"/> attribute with a <see cref="LayoutKind"/> of 
    /// <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> to ensure the value type fields are not moved around.
    /// </para>
    /// <para type="CopyCommon">
    /// Applications must call the <see cref="GorgonResourceCopier"/>.<see cref="GorgonResourceCopier.BeginUpload"/> method prior to calling this method, failure to do so will result in an exception being 
    /// thrown.
    /// </para>
    /// <para type="CopyCommon">
    /// Depending on the <see cref="BufferUsage"/> of the <paramref name="buffer"/>, data may not be immediately written to the buffer until the <see cref="End"/> method is called. Because of this, the 
    /// <paramref name="buffer"/> should not be used until the <c>End</c> method returns.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// [StructLayout(LayoutKind.Sequential)]
    /// struct MyType
    /// {
    ///    public Vector4 Position;
    ///    public Vector2 UV;
    /// }
    /// 
    /// MyType sourceData = new()
    /// {
    ///    Position = new Vector4(1, 0, 1, 1),
    ///    UV = new Vector2(0, 1)
    /// };
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// using GorgonGpuBufferWriter writer = new(_graphics);
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// writer.Begin()
    ///       .CopyValue(in sourceData, destBuffer)
    ///       .End();
    ///       
    /// // Use the GpuBuffer here...
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    IGorgonResourceWriter CopyValue<T>(in T value, GorgonGpuBuffer_OLDE buffer, long offset = 0) where T : unmanaged;

    /// <summary>
    /// Function to copy the contents of memory pointed at by a <see cref="GorgonPtr{T}"/> into a buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="pointer">The pointer to memory containing the data to write into the buffer.</param>
    /// <param name="buffer"><inheritdoc cref="CopyValue" path="/param[@name='buffer']"/></param>
    /// <param name="offset"><inheritdoc cref="CopyValue" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> pointer is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBuffer_OLDE, long)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method writes data into the <paramref name="buffer"/> from the <see cref="GorgonPtr{T}"/> type specified by the <paramref name="pointer"/> parameter. Applications can use this to write directly 
    /// from the contents of native/pinned memory pointed at by a <see cref="GorgonPtr{T}"/> or a <see cref="GorgonNativeBuffer{T}"/> into a <see cref="GorgonGpuBuffer_OLDE"/>. 
    /// </para>
    /// <inheritdoc cref="CopyValue" path="/remarks/para[@type='CopyCommon']"/>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// using GorgonNativeBuffer<byte> sourceData = new(1024);
    /// 
    /// // Code to write to the sourceData buffer goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// using GorgonGpuBufferWriter writer = new(_graphics);
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// writer.Begin()
    ///       .CopyPointer<byte>(sourceData, destBuffer)
    ///       .End();
    ///       
    /// // Use the GpuBuffer here...
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonPtr{T}"/>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    IGorgonResourceWriter CopyPointer<T>(GorgonPtr<T> pointer, GorgonGpuBuffer_OLDE buffer, long offset = 0) where T : unmanaged;

    /// <summary>
    /// Function to copy the contents of memory pointed at by a <see cref="GorgonPtr{T}"/> into a buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="pointer">The pointer to memory containing the data to write into the buffer.</param>
    /// <param name="buffer"><inheritdoc cref="CopyValue" path="/param[@name='buffer']"/></param>
    /// <param name="offset"><inheritdoc cref="CopyValue" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> pointer is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBuffer_OLDE, long)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method writes data into the <paramref name="buffer"/> from the <see cref="GorgonPtr{T}"/> type specified by the <paramref name="pointer"/> parameter. Applications can use this to write directly 
    /// from the contents of native/pinned memory pointed at by a <see cref="GorgonPtr{T}"/> or a <see cref="GorgonNativeBuffer{T}"/> into a <see cref="GorgonGpuBuffer_OLDE"/>. 
    /// </para>
    /// <inheritdoc cref="CopyValue" path="/remarks/para[@type='CopyCommon']"/>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// using GorgonNativeBuffer<byte> sourceData = new(1024);
    /// 
    /// // Code to write to the sourceData buffer goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// using GorgonGpuBufferWriter writer = new(_graphics);
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// writer.Begin()
    ///       .CopyPointer<byte>(sourceData, destBuffer)
    ///       .End();
    ///       
    /// // Use the GpuBuffer here...
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonPtr{T}"/>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    IGorgonResourceWriter CopyPointer<T>(GorgonPtr<T> pointer, GorgonGpuBuffer buffer, long offset = 0) where T : unmanaged;

    /// <summary>
    /// Function to copy a range of values into a buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="values">The span of values to write.</param>
    /// <param name="buffer"><inheritdoc cref="CopyValue" path="/param[@name='buffer']"/></param>
    /// <param name="offset"><inheritdoc cref="CopyValue" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBuffer_OLDE, long)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method writes data into the <paramref name="buffer"/> from the read only span type specified by the <paramref name="values"/> parameter. Applications can use this to write directly from arrays 
    /// into a <see cref="GorgonGpuBuffer_OLDE"/>.
    /// </para>
    /// <inheritdoc cref="CopyValue" path="/remarks/para[@type='CopyCommon']"/>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// byte[] sourceData = new byte[1024];
    /// 
    /// // Code to write data to the sourceData array goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = ... code to create the GPU buffer...
    /// using GorgonGpuBufferWriter writer = new(_graphics);
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// writer.Begin()
    ///       .CopyRange<byte>(sourceData.ToSpan(), destBuffer)
    ///       .End();
    ///       
    /// // Use the GpuBuffer here...
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    IGorgonResourceWriter CopyRange<T>(ReadOnlySpan<T> values, GorgonGpuBuffer_OLDE buffer, long offset = 0) where T : unmanaged;

    /// <summary>
    /// Function to copy the contents of one <see cref="GorgonGpuBuffer_OLDE"/> to another.
    /// </summary>
    /// <param name="source">The buffer to copy from.</param>
    /// <param name="destination">The buffer to copy into.</param>
    /// <param name="sourceOffset">[Optional] The offset, in bytes, within the source buffer to start reading from.</param>
    /// <param name="destinationOffset">[Optional] The offset, in bytes, within the destination buffer to start writing into.</param>
    /// <param name="count">[Optional] The number of bytes to copy.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if the <paramref name="source"/> buffer has a <see cref="BufferUsage"/> of <see cref="BufferUsage.Download"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="sourceOffset"/>, or <paramref name="destinationOffset"/> parameters are less than 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="count"/> parameter is less than 1.</para>
    /// </exception>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="sourceOffset"/> plus the <paramref name="count"/> exceeds the size of the <paramref name="source"/> buffer.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="destinationOffset"/> plus the <paramref name="count"/> exceeds the size of the <paramref name="destination"/> buffer.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method copies the entire, or partial, contents of a <see cref="GorgonGpuBuffer_OLDE"/> to another <see cref="GorgonGpuBuffer_OLDE"/>. 
    /// </para>
    /// <para>
    /// If the <paramref name="sourceOffset"/>, or <paramref name="destinationOffset"/> are specified, then reading and writing will begin at the specified byte offsets. If these are not specified, then the 
    /// data will be copied from and to the beginning of each buffer respectively.
    /// </para>
    /// <para>
    /// If the <paramref name="count"/> parameter is specified, the method will copy up to the number of bytes passed in. If it is omitted, then the number of bytes copied will be the smaller of the two 
    /// buffer sizes, minus their respective offsets.
    /// </para>
    /// <para>
    /// The copy operation in this method is deferred until the application calls the <see cref="End"/> method.
    /// </para>
    /// <para>
    /// The <paramref name="source"/> buffer has a <see cref="GorgonGpuBuffer_OLDE.Usage"/> of <see cref="BufferUsage.Download"/>, an exception will be thrown because download buffers cannot be read by the GPU.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="csharp">    
    /// <![CDATA[
    /// byte[] sourceData = new byte[1024];
    /// 
    /// // Code to write data to the sourceData array goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = new(_graphics, "Destination Buffer", new GorgonBufferInfo(256, BufferUsage.Default);
    /// using GorgonGpuBuffer srcBuffer = new(_graphics, "Source Buffer", new GorgonBufferInfo(256, BufferUsage.Upload);
    /// using GorgonGpuBufferWriter writer = new(_graphics);
    /// 
    /// 
    /// // sourceData is a GorgonNativeBuffer<byte> which implicitly converts to GorgonPtr<byte>.
    /// writer.Begin()
    ///       // Populate data in the upload GPU buffer.
    ///       .CopyRange<byte>(sourceData.ToSpan(), srcBuffer)
    ///       // Copy that buffer to the destination default buffer.
    ///       .CopyBuffer(srcBuffer, destBuffer);
    ///       // Copy 24 bytes at offset 16 in the srcBuffer starting at the 4th byte in the destBuffer.
    ///       .CopyBuffer(srcBuffer, destBuffer, 16, 4, 24);
    ///       .End();
    ///       
    /// // Use the GpuBuffer(s) here...
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="End"/>
    IGorgonResourceWriter CopyBuffer(GorgonGpuBuffer_OLDE source, GorgonGpuBuffer_OLDE destination, long sourceOffset = 0, long destinationOffset = 0, long? count = null);

    /// <summary>
    /// Function to copy a <see cref="IGorgonImage"/> into a texture.
    /// </summary>
    /// <param name="image">The image data to copy into the texture.</param>
    /// <param name="texture">The texture that will receive the image data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <seealso cref="IGorgonImage"/>
    IGorgonResourceWriter CopyImageToTexture(IGorgonImage image, GorgonTexture texture);

    /// <summary>
    /// Function to copy a <see cref="IGorgonImageBuffer"/> into a texture sub resource.
    /// </summary>
    /// <param name="imageBuffer">The image data buffer to copy into the texture.</param>
    /// <param name="texture">The texture that will receive the image data.</param>
    /// <param name="destinationMipLevel">The destination mip level on the texture to copy the image data into.</param>
    /// <param name="destinationZOrArrayIndex">[Optional] The destination depth slice on a 3D texture, or array index on a 1D or 2D texture array.</param>
    /// <param name="destinationPlane">[Optional] The destination format plane on the texture to copy the image data into.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <seealso cref="IGorgonImageBuffer"/>
    IGorgonResourceWriter CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel, short destinationZOrArrayIndex = 0, byte destinationPlane = 0);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonGpuBuffer_OLDE"/> into a specific sub resource of a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to copy.</param>
    /// <param name="texture">The texture to copy the data into.</param>
    /// <param name="parameters">The parameters to define what offset to copy from in the buffer, and which sub resource will receive the data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if the <paramref name="buffer"/> has a <see cref="GorgonGpuBuffer_OLDE.Usage"/> of <see cref="BufferUsage.Download"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the size, in bytes, of <paramref name="buffer"/> minus the <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.SourceOffset"/> is smaller than 
    /// the texture sub resource size.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.SourceOffset"/> is less than 0.</exception>
    /// <remarks>
    /// <para>
    /// This method will copy the data within the specified <see cref="GorgonGpuBuffer_OLDE"/> into the specified sub resource for the specified <see cref="GorgonTexture"/>. This allows applications to load 
    /// arbitrary data into a texture sub resource.
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBuffer_OLDE.SizeInBytes"/> of the <paramref name="buffer"/> minus the the 
    /// <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.SourceOffset"/> parameter, must be at least the same size, in 
    /// bytes, as the texture sub resource. To determine the size of a sub resource use the <see cref="GorgonTexture.SubResources"/> list on the <paramref name="texture"/> to retrieve a 
    /// <see cref="GorgonSubResourceInfo"/> data structure and use the <see cref="GorgonSubResourceInfo.SizeInBytes"/> property, which can be used to determine the size of the <paramref name="buffer"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="buffer"/> content should also match the alignment requirements for the <paramref name="texture"/>. This means the <see cref="GorgonSubResourceInfo.RowPitch"/> should match between 
    /// the two resources. For example, if the texture has a row pitch alignment of 256 bytes, then the data in the buffer should be aligned in the same way. So, for a texture sub resource with a width of 
    /// 300, its row pitch may be aligned to 512 bytes. This means that a row in the buffer data should also be aligned to 512 bytes and <b>not</b> the width of the sub resource. This information can be 
    /// retrieved from the aforementioned <see cref="GorgonTexture.SubResources"/> list.
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.DestinationArrayIndex"/> is the array index in texture array that will receive the data. This only applies to a 
    /// <paramref name="texture"/> with a <see cref="GorgonTexture.Type"/> of <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.
    /// <note type="information">
    /// <para>
    /// You cannot copy into an individual depth slice using this method. To perform a copy on an individual depth slice, use the 
    /// <see cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/> method.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.DestinationMipLevel"/> is the mip map level that will receive the data in the <paramref name="buffer"/>. By default 
    /// this is the top mip level.
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.DestinationPlane"/> is the format plane for a <see cref="BufferFormat"/> that use multiple planes. For example, the 
    /// depth buffer format <see cref="BufferFormat.D24_UNorm_S8_UInt"/> has a 24 bit depth plane, and an 8 bit stencil plane), and a 
    /// <paramref name="parameters"/>.<see cref="CopyBufferToTextureParams.DestinationPlane"/> of 0 would write into the depth portion, and a value of 1 would write into the stencil portion. 
    /// </para>
    /// <para>
    /// All destination parameters are clipped against their respective values for the <paramref name="texture"/>. This ensures that the values passed can never exceed the minimum and maximum mip levels, 
    /// array/depth count, and plane count for the <paramref name="texture"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonSubResourceInfo"/>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/>
    /// <seealso cref="TextureType"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="CopyBufferToTextureParams"/>
    IGorgonResourceWriter CopyBufferToTexture(GorgonGpuBuffer_OLDE buffer, GorgonTexture texture, CopyBufferToTextureParams parameters);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonGpuBuffer_OLDE"/> into a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <param name="buffer"><inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer_OLDE, GorgonTexture, CopyBufferToTextureParams)" path="/param[@name='buffer']"/></param>
    /// <param name="texture"><inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer_OLDE, GorgonTexture, CopyBufferToTextureParams)" path="/param[@name='texture']"/></param>
    /// <param name="sourceOffset">[Optional] The number of bytes within the buffer to start copying from.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    IGorgonResourceWriter CopyBufferToTexture(GorgonGpuBuffer_OLDE buffer, GorgonTexture texture, long sourceOffset = 0);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="buffer"></param>
    /// <param name="destinationOffset"></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    IGorgonResourceWriter CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer_OLDE buffer, long destinationOffset = 0);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="sourceZOrArrayIndex"></param>
    /// <param name="sourceMipLevel"></param>
    /// <param name="sourcePlane"></param>
    /// <param name="buffer"></param>
    /// <param name="destinationOffset"></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    IGorgonResourceWriter CopyTextureToBuffer(GorgonTexture texture, short sourceMipLevel, short sourceZOrArrayIndex, byte sourcePlane, GorgonGpuBuffer_OLDE buffer, long destinationOffset = 0);

    /// <summary>
    /// Function to copy a texture subresource into another texture subresource.
    /// </summary>
    /// <param name="source">The texture with the data to copy.</param>
    /// <param name="destination">The texture that will receive data to copy.</param>
    /// <param name="parameters">The parameters used to define which sub resource to copy, and which sub resource will receive the copied data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="GorgonException"><para>Thrown if the <paramref name="source"/> and <paramref name="destination"/> have a <see cref="BufferFormat"/> that does not belong to the same <see cref="GorgonFormatInfo.Group"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="source"/> and <paramref name="destination"/> do not have matching <see cref="GorgonTexture.MultisampleInfo"/> values.</para>
    /// </exception>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="source"/>, or <paramref name="destination"/> parameters have a <see cref="GorgonTexture.IsDepthStencil"/> value of <b>true</b> and the 
    /// <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.SourceRegion"/> is not empty, or the <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.DestinationX"/>, 
    /// <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.DestinationY"/> or <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.DestinationZOrArrayIndex"/> (3D textures only) parameters are not 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="source"/>, or <paramref name="destination"/> parameters have a <see cref="GorgonTexture.MultisampleInfo"/> not equal to <see cref="GorgonMultisampleInfo.NoMultisampling"/> and the 
    /// <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.SourceRegion"/> is not empty, or the <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.DestinationX"/>, 
    /// <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.DestinationY"/> or <paramref name="parameters"/>.<see cref="CopyTextureSubResourceParams.DestinationZOrArrayIndex"/> (3D textures only) parameters are not 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="source"/> texture <see cref="GorgonTexture.Type"/> is unknown.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// TODO:
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyTexture(GorgonTexture, GorgonTexture)"/>
    /// <seealso cref="CopyTextureSubResourceParams"/>
    IGorgonResourceWriter CopyTexture(GorgonTexture source, GorgonTexture destination, in CopyTextureSubResourceParams parameters);

    /// <summary>
    /// Function to copy an entire texture into another.
    /// </summary>
    /// <param name="source">The texture to copy.</param>
    /// <param name="destination">The texture that will receive the data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="GorgonException"><para><inheritdoc cref="CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/node()"/></para>
    /// <para>-or-</para>
    /// <para>Thrown if the source texture and destination texture do not meet the restrictions for this method.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method will copy the entirety of the <paramref name="source"/> texture to the <paramref name="destination"/> texture. This method is more efficient than copying individual subresources with the 
    /// <see cref="CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)"/> method.
    /// </para>
    /// <para>
    /// However, this functionality has several restrictions. The source and destination texture must:
    /// <list type="bullet">
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTexture.Type"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTexture.Width"/>, <see cref="GorgonTexture.Height"/>, <see cref="GorgonTexture.Depth"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTexture.MipCount"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTexture.ArrayCount"/> if the textures are <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same texture <see cref="GorgonTexture.Format"/> that is within the same texture group (see the <see cref="GorgonFormatInfo.Group"/> property of the 
    ///     <see cref="GorgonFormatInfo"/> on the <see cref="GorgonTexture.FormatInfo"/> property on the texture).</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTexture.MultisampleInfo"/> values.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// If the above restrictions are not met, an exception will be thrown.
    /// </para>
    /// </remarks>
    /// <seealso cref="TextureType"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)"/>
    IGorgonResourceWriter CopyTexture(GorgonTexture source, GorgonTexture destination);

    /// <summary>
    /// Function to make the GPU wait for the graphics queue if it's in the process of rendering data.
    /// </summary>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if there was a failure during the wait operation.</exception>
    /// <remarks>
    /// <para>
    /// This method is meant to make the copy queue wait for the graphics queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the graphics queue is busy rendering with a 
    /// texture required by the copy queue, this will allow the copy queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// The <see cref="GorgonGraphics"/> object encapsulates the graphics queue.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="WaitForCompute"/>
    IGorgonResourceWriter WaitForCopy();

    /// <summary>
    /// Function to make the GPU wait for the compute queue if it's in the process of working with data.
    /// </summary>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="WaitForCopy()" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method is meant to make the copy queue wait for the compute queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the compute queue is busy updating 
    /// a texture required by the copy queue, this will allow the copy queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// To make use of the graphics queue, developers can use the <see cref="GorgonComputeEngine"/> functionality.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputeEngine"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="WaitForCopy"/>
    IGorgonResourceWriter WaitForCompute();
}