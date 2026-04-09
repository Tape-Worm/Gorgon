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

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a fluent interface for the <see cref="GorgonResourceCopier"/>.
/// </summary>
/// <remarks>
/// <para>
/// This interface performs copies to a <see cref="GorgonGpuBuffer"/> or <see cref="GorgonTexture"/> by scheduling an upload of one or more write operations on the GPU copy queue. When a write is performed, a 
/// temporary buffer is allocated and the application data is copied into that buffer. When the application calls the <see cref="End"/> (or <see cref="EndAsync"/>) method, then that temporary buffer is 
/// uploaded into the buffer resource on the GPU. After this, the temporary buffer is freed.
/// </para>
/// <para>
/// When the copy to the GPU buffer resource is performed, the object will stall the CPU (this can be awaited using the <see cref="EndAsync"/> method) until the GPU is finished with its upload. Because of 
/// this, performance will suffer when the methods on this object are called in a tight loop (such as a game loop). The ideal use for this type is for asynchronous initialization of resources.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGpuResource"/>
/// <seealso cref="GorgonGpuBuffer"/>
/// <seealso cref="GorgonTexture"/>
public interface IGorgonResourceWriter
    : IGorgonCopyMethodsFluent<IGorgonResourceWriter>
{
    /// <summary>
    /// Function to end the batch and perform any pending uploads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method must be called after a call to the <see cref="GorgonResourceCopier"/>.<see cref="GorgonResourceCopier.BeginUpload"/> method. Failure to do so can lead to data not being sent the GPU properly.
    /// </para>
    /// <para>
    /// This method will not return until the GPU is finished its upload(s).
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <example>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)"/>
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
    /// </remarks>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <example>    
    /// <code language="csharp">
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
    ///         result[i] = new GorgonGpuBuffer(_graphics, $"Buffer {i}", new GorgonGpuBufferInfo(buffers[i].Length));
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
    Task EndAsync();
}