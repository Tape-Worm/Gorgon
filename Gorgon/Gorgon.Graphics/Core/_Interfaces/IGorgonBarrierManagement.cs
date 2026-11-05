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
// Created: November 3, 2026 9:16:41 PM
//

using System;
using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality for applying barriers for resource synchronization.
/// </summary>
/// <typeparam name="T">The type of object to return as the fluent interface.</typeparam>
internal interface IGorgonBarrierManagement<T>
{
    /// <summary>
    /// Function to set a barrier on a texture to enforce synchronization.
    /// </summary>
    /// <param name="texture">The texture to assign the barrier to.</param>
    /// <param name="sync">The new synchronization state for the barrier.</param>
    /// <param name="access">The new access level for the barrier.</param>
    /// <param name="layout">The new layout for the texture data.</param>
    /// <param name="subResources">[Optional] The sub resources on the texture to apply the barrier to, or <b>null</b> to apply it to the whole texture.</param>
    /// <param name="force">[Optional] <b>true</b> to apply the queued barriers right away, <b>false</b> to wait until they are needed.</param>
    /// <returns>The fluent interface for the method.</returns>
    /// <exception cref="GorgonException">
    /// <para type="barrier_issue">Thrown if a resource was previously used in a command list on one of the Gorgon root objects (<see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/>, or 
    /// <see cref="GorgonComputeEngine"/>) and was left in a usage state that is incompatible with the root object that is currently trying to use the resource.</para>
    /// <para type="barrier_issue">Thrown if debugging is enabled, and a resource was previously reset on a command list, and then used again before the command list was submitted.</para>
    /// </exception>
    /// <remarks>
    /// <para type="barrier">
    /// Most operations in Gorgon apply the barriers for their resources automatically. However, there may be times when an application needs a more optimal barrier strategy, or Gorgon cannot set the barriers 
    /// correctly. This method allows applications to set barriers themselves.
    /// </para>
    /// <para type="barrier">
    /// <note type="information">
    /// <para><h3>What is a barrier? What is it for?</h3></para>
    /// <para>
    /// Modern GPUs may require their resources to be in specific states (e.g. compressed, uncompressed, etc...) before they are used, and they also run operations in parallel. To prevent hazards for 
    /// read-after-write, write-after-read, and write-after-write operations, applications provide barriers to indicate that they intend to perform a certain action, requiring certain access types and data 
    /// layout. This allows the GPU to perform the operations required to ensure the data is synchronized.
    /// </para>
    /// <para>
    /// For more detailed information, please refer to the <a href="https://microsoft.github.io/DirectX-Specs/d3d/D3D12EnhancedBarriers.html" target="_blank">Direct3D 12 Enhanced Barriers specification</a>.
    /// </para>
    /// </note>
    /// </para>
    /// <para type="barrier">
    /// The synchronization bits in the <paramref name="sync"/> parameter indicate the work that must finish, or wait, around the barrier. These bits can be combined to provide multiple synchronization types.
    /// </para>
    /// <para type="barrier">
    /// The access bits in the <paramref name="access"/> parameter indicate how the resource will be accessed after the barrier. Since GPUs cache many of their write operations, this allows the GPU to ensure 
    /// that the caches are correctly flushed. These bits can be combined to provide multiple access types.
    /// </para>
    /// <para type="TextureBarrier">
    /// The <paramref name="layout"/> transition is for <see cref="GorgonTextureCommon"/> objects only. This value indicates the type of read/write operation being performed so the GPU can compress or 
    /// decompress its data (depending on the GPU architecture).
    /// </para>
    /// <para type="TextureBarrier">
    /// The barrier can be applied to sub resources of a <see cref="GorgonTextureCommon"/> by assigning a value to the <paramref name="subResources"/> parameter. If this value is omitted, then the barrier is 
    /// applied to the entire texture. Applying the barrier to a range of sub resources lets the GPU work on another portion of the texture while working on the sub resources passed to this method.
    /// </para>
    /// <para type="barrier">
    /// Barriers are queued, and applied right before the next operation on the command list that needs them (e.g. a draw, clear or copy). When the <paramref name="force"/> parameter is <b>true</b>, the queued 
    /// barriers are applied immediately. Queuing the barriers until they are needed lets them be applied together, which is more efficient on some GPUs.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="Reset(ReadOnlySpan{IGorgonTextureView{GorgonTextureCommon}})"/>
    T SetBarrier(GorgonTextureCommon texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool force = false);

    /// <summary>
    /// Function to set a barrier on a buffer to enforce synchronization.
    /// </summary>
    /// <param name="buffer">The buffer to assign the barrier to.</param>
    /// <param name="sync"><inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/param[@name='sync']"/></param>
    /// <param name="access"><inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/param[@name='access']"/></param>
    /// <param name="force"><inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/param[@name='force']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/remarks/para[@type='barrier']"/>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    T SetBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access, bool force = false);

    /// <summary>
    /// Function to reset the usage for the specified textures.
    /// </summary>
    /// <param name="textures">The views of the textures that need their usage state reset.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool)" path="/exception/para[@type='barrier_issue']"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// When a texture is used in a command list on the <see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/> (directly), or the <see cref="GorgonComputeEngine"/> objects, it will be put into a 
    /// state that provides optimal usage for the GPU. 
    /// </para>
    /// <para>
    /// Because of this, the state of the object will need to be reset before using it on a different <see cref="GorgonGraphics"/>, <see cref="GorgonResourceCopier"/>, or the 
    /// <see cref="GorgonComputeEngine"/> objects, otherwise an exception will be thrown when it is used without resetting it to a generalized state.
    /// </para>
    /// <para>
    /// This is required because the GPU can have metadata on a resource that can be used to help with rendering, or other operations. Unfortunately that metadata makes the resource unusable for other 
    /// operations like copying, or use as a read/write resource. So, the resource needs to be put back into a general state before it can be consumed in those operations.
    /// </para>
    /// <para>
    /// The whole texture behind each view is reset, not only the sub resources that the view covers.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Any texture that has been reset cannot be used again by any command list passed to the same <see cref="GorgonGraphics.Submit(ReadOnlySpan{GorgonCommandList})"/> call, including the command list 
    /// that reset it. When the texture has been reset, we are telling the GPU that there is no intention to use the texture again until those command lists have been submitted. Use the texture in a 
    /// command list for a later submit, or on the object it was reset for.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The reset operation <b>must</b> be called on a command list from the correct object. So, if the resource was last used by a command list from a <see cref="GorgonGraphics"/> object, then only a command 
    /// list from that object can reset the resource, otherwise an exception will be thrown.
    /// </para>    
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="GorgonComputeEngine"/>
    T Reset(ReadOnlySpan<IGorgonTextureView<GorgonTextureCommon>> textures);
}
