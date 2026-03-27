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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 11, 2025 12:27:17 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Windows.Web.AtomPub;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of synchronization to apply in a barrier before and after the resource is accessed.
/// </summary>
/// <remarks>
/// <para>
/// These values can be combined via OR'ing to provide fine grained synchronization and better performance.
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// Each sync scope bit has a limited set of compatible access types (see the specific sync bit description for details). In addition, <see cref="BarrierAccess.Common"/> can be used to imply any or all 
/// layout and sync scope compatible access bits. However, using <c>Common</c> in a barrier may result in excessive cache flushing, negatively impacting performance.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="BarrierAccess"/>
[Flags]
public enum BarrierSync
{
    /// <summary>
    /// <para>
    /// When used in a <c>Before</c> state, this indicates NO PRECEDING work must complete before executing the barrier. This MUST be paired with an <c>Access Before</c> value of 
    /// <see cref="BarrierAccess.None"/>. Additionally, no preceding barriers or accesses to the related subresource are permitted in the same command context execution scope.
    /// </para>
    /// <para>
    /// When used in an <c>After</c> state, this indicates NO SUBSEQUENT work must wait for the barrier to complete, and MUST be paired with an <c>Access After</c> value of 
    /// <see cref="BarrierAccess.None"/>. Additionally, no subsequent barriers or accesses to the related subresource are permitted in the same command context execution scope.
    /// </para>
    /// </summary>
    None = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_NONE,
    /// <summary>
    /// <para>
    /// When used in a <c>Before</c> state, this indicates <b>ALL</b> PRECEDING work must complete before executing the barrier. And when used in an <c>After</c> state, this indicates ALL SUBSEQUENT work 
    /// must wait for the barrier to complete.
    /// </para>
    /// <para>
    /// Be careful when using this value, as it could impact performance.
    /// </para>
    /// </summary>
    All = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_ALL,
    /// <summary>
    /// <para>
    /// Synchronize against the following GPU workloads:
    /// <list type="bullet">
    /// <item><description>Drawing instanced (indexed and otherwise) geometry.</description></item>
    /// <item><description>Setting a root descriptor table.</description></item>
    /// <item><description>Setting a root shader resource.</description></item>
    /// <item><description>Setting a root unordered access view.</description></item>
    /// <item><description>Setting a root constant buffer view.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This is an umbrella scope for all Drawing pipeline stages. When used in a <c>Before</c> state, it indicates ALL PRECEDING Draw work must complete before executing the barrier. And when used in an 
    /// <c>After</c> state, it indicates ALL SUBSEQUENT Draw work must wait for the barrier to complete.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VertexBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.ConstantBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.IndexBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.RenderTarget"/></description></item>
    /// <item><description><see cref="BarrierAccess.UnorderedAccess"/></description></item>
    /// <item><description><see cref="BarrierAccess.DepthStencilWrite"/></description></item>
    /// <item><description><see cref="BarrierAccess.DepthStencilRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShaderResource"/></description></item>
    /// <item><description><see cref="BarrierAccess.StreamOutput"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    Draw = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_DRAW,
    /// <summary>
    /// <para>
    /// Synchronize scope for processing index buffer input.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.IndexBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    IndexInput = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_INDEX_INPUT,
    /// <summary>
    /// <para>
    /// Synchronize scope for all vertex shading stages, including vertex, domain, hull, tessellation, geometry, amplification and mesh shading.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VertexBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.ConstantBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.UnorderedAccess"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShaderResource"/></description></item>
    /// <item><description><see cref="BarrierAccess.StreamOutput"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    VertexShading = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_VERTEX_SHADING,
    /// <summary>
    /// <para>
    /// Synchronize scope for pixel shader execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.ConstantBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.UnorderedAccess"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShadingRateSource"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShaderResource"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    PixelShading = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_PIXEL_SHADING,
    /// <summary>
    /// <para>
    /// Synchronize scope for non pixel shader execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in thi scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VertexBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.ConstantBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.UnorderedAccess"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShaderResource"/></description></item>
    /// <item><description><see cref="BarrierAccess.StreamOutput"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    NonPixelShading = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_NON_PIXEL_SHADING,
    /// <summary>
    /// <para>
    /// Synchronize scope for depth/stencil read/write operations. This includes DSV accesses during Draw* and ClearRenderTargetView.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.DepthStencilWrite"/></description></item>
    /// <item><description><see cref="BarrierAccess.DepthStencilRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    DepthStencil = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_DEPTH_STENCIL,
    /// <summary>
    /// <para>
    /// Synchronize scope for render target read/write operations. This include RTV writes during Draw* and ClearRenderTargetView.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.RenderTarget"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    RenderTarget = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_RENDER_TARGET,
    /// <summary>
    /// <para>
    /// Synchronize scope for the following GPU workloads:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description>Dispatch operations.</description></item>
    /// <item><description>Setting a <b>compute</b> root descriptor table.</description></item>
    /// <item><description>Setting a <b>compute</b> root shader resource.</description></item>
    /// <item><description>Setting a <b>compute</b> root unordered access view.</description></item>
    /// <item><description>Setting a <b>compute</b> root constant buffer view.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.RenderTarget"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    ComputeShading = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_COMPUTE_SHADING,
    /// <summary>
    /// <para>
    /// Synchronize scope for raytracing execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    RayTracing = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_RAYTRACING,
    /// <summary>
    /// <para>
    /// Synchronize scope for Copy commands.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.CopySource"/></description></item>
    /// <item><description><see cref="BarrierAccess.CopyDestination"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    Copy = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_COPY,
    /// <summary>
    /// <para>
    /// Synchronize scope for Resolve commands.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.ResolveSource"/></description></item>
    /// <item><description><see cref="BarrierAccess.ResolveDestination"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    Resolve = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_RESOLVE,
    /// <summary>
    /// <para>
    /// Synchronize scope for ExecuteIndirect execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.IndirectArgument"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    ExecuteIndirect = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_EXECUTE_INDIRECT,
    /// <summary>
    /// <para>
    /// Synchronize scope for ALL shader execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VertexBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.ConstantBuffer"/></description></item>
    /// <item><description><see cref="BarrierAccess.UnorderedAccess"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShaderResource"/></description></item>
    /// <item><description><see cref="BarrierAccess.StreamOutput"/></description></item>
    /// <item><description><see cref="BarrierAccess.ShadingRateSource"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    AllShading = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_ALL_SHADING,
    /// <summary>
    /// <para>
    /// Undocumented by Microsoft at this time.
    /// </para>
    /// </summary>
    EmitRayTracingAccelerationStructurePostBuildInformation = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_EMIT_RAYTRACING_ACCELERATION_STRUCTURE_POSTBUILD_INFO,
    /// <summary>
    /// <para>
    /// Synchronize scope for the <see cref="GorgonCommandList.ClearUnorderedAccessView(uint)"/> and <see cref="GorgonCommandList.ClearUnorderedAccessView(float)"/> methods.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.UnorderedAccess"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    ClearUnorderedAccessView = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_CLEAR_UNORDERED_ACCESS_VIEW,
    /* TODO: We do not support ray tracing or video encoding/decoding (yet).  Leave this here for now, but ignore it. */
/*    /// <summary>
    /// <para>
    /// Synchronize scope for Video Decode execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VideoDecodeRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.VideoDecodeWrite"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoDecode = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_VIDEO_DECODE,
    /// <summary>
    /// <para>
    /// Synchronize scope for Video Process execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VideoProcessRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.VideoProcessWrite"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoProcess = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_VIDEO_PROCESS,
    /// <summary>
    /// <para>
    /// Synchronize scope for Video Encode execution.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.VideoEncodeRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.VideoEncodeWrite"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoEncode = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_VIDEO_ENCODE,
    /// <summary>
    /// <para>
    /// Synchronize scope for <see cref="GorgonGraphicsCommandContext.BuildAccelerationStructure"/> work.
    /// </para>
    /// <para>
    /// Corresponding <see cref="BarrierAccess"/> (both <c>Before</c> and <c>After</c>) must have the <see cref="BarrierAccess.RayTracingAccelerationStructureWrite"/> bit set.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.RayTracingAccelerationStructureRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    BuildRayTracingAccelerationStructure = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_BUILD_RAYTRACING_ACCELERATION_STRUCTURE,
    /// <summary>
    /// <para>
    /// Synchronize scope for <see cref="GorgonGraphicsCommandContext.CopyRaytracingAccelerationStructure"/> work.
    /// </para>
    /// <para>
    /// Corresponding <see cref="BarrierAccess"/> (both <c>Before</c> and <c>After</c>) must have the <see cref="BarrierAccess.RayTracingAccelerationStructureWrite"/> bit set.
    /// </para>
    /// <para>
    /// <see cref="BarrierAccess">Access types</see> in this scope are limited to the following values:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierAccess.RayTracingAccelerationStructureRead"/></description></item>
    /// <item><description><see cref="BarrierAccess.Common"/> (See the note on the <see cref="BarrierSync"/> description.)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    CopyRayTracingAccelerationStructure = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_COPY_RAYTRACING_ACCELERATION_STRUCTURE,*/
    /// <summary>
    /// <para>
    /// Special sync bit indicating a split barrier. Used as an <c>After</c> state to indicates the start of a split barrier. The application must provide a matching barrier with this value as a 
    /// <c>Before</c> bit.
    /// </para>
    /// <para>
    /// There are no specific restrictions on access bits for this value.
    /// </para>
    /// </summary>
    Split = D3D12_BARRIER_SYNC.D3D12_BARRIER_SYNC_SPLIT,

    /// <summary>
    /// The default barrier sync state for a buffer resource.
    /// </summary>
    UploadHeapDefaultSyncState = AllShading | IndexInput | Copy | ExecuteIndirect | PixelShading | NonPixelShading
}

/// <summary>
/// Defines the type of access for the resource in the barrier.
/// </summary>
/// <remarks>
/// <para>
/// These values can be combined via OR'ing to provide fine grained access.
/// </para>
/// </remarks>
[Flags]
public enum BarrierAccess
{
    /// <summary>
    /// <para>
    /// Default initial access for all resources in a given command context execution scope. Supports any type of access compatible with current layout and resource properties, including no-more than one 
    /// write access. For buffers and textures using <see cref="BarrierLayout.Common"/>, this value supports concurrent read and write accesses.
    /// </para>
    /// <para>
    /// When used as an <c>After</c> state, this value may be used to return a resource back to common accessibility. Note, this may force unnecessary cache flushes if used incorrectly. When possible, the 
    /// <c>After</c> state should be limited to explicit access bits.
    /// </para>
    /// <para>
    /// App developers should avoid using this value as a barrier with a <c>Before</c> state. Any read-after-write or write-after-write hazards are best handled using explicit <c>After</c> state bits.
    /// </para>
    /// </summary>
    Common = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_COMMON,
    /// <summary>
    /// <para>
    /// Indicates a buffer resource is accessible as a vertex buffer in the current execution queue. Vertex buffer accesses occur only in <see cref="BarrierSync.VertexShading"/> scope. Runtime barrier 
    /// validation ensure that value is used with one or more of the following sync bits:
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VertexShading"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VertexBuffer = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VERTEX_BUFFER,
    /// <summary>
    /// <para>
    /// Indicates a buffer resource is accessible as a constant buffer in the current execution queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VertexShading"/></description></item>
    /// <item><description><see cref="BarrierSync.PixelShading"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    ConstantBuffer = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_CONSTANT_BUFFER,
    /// <summary>
    /// <para>
    /// Indicates a buffer resource is accessible as an index buffer in the current execution queue. Index buffer accesses occur only in the <see cref="BarrierSync.IndexInput"/> scope. Runtime barrier 
    /// validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.IndexInput"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    IndexBuffer = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_INDEX_BUFFER,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a render target. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// <item><description><see cref="BarrierSync.RenderTarget"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    RenderTarget = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_RENDER_TARGET,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as an unordered access resource. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VertexShading"/></description></item>
    /// <item><description><see cref="BarrierSync.PixelShading"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// <item><description><see cref="BarrierSync.ClearUnorderedAccessView"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    UnorderedAccess = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_UNORDERED_ACCESS,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a writable depth/stencil resource. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.DepthStencil"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    DepthStencilWrite = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_DEPTH_STENCIL_WRITE,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a read-only depth/stencil resource. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.DepthStencil"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    DepthStencilRead = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_DEPTH_STENCIL_READ,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a shader resource. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VertexShading"/></description></item>
    /// <item><description><see cref="BarrierSync.PixelShading"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    ShaderResource = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_SHADER_RESOURCE,
    /// <summary>
    /// <para>
    /// Indicates a buffer is accessible as a stream output target. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VertexShading"/></description></item>
    /// <item><description><see cref="BarrierSync.Draw"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    StreamOutput = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_STREAM_OUTPUT,
    /// <summary>
    /// <para>
    /// Indicates a buffer is accessible as an indirect argument buffer. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.ExecuteIndirect"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    IndirectArgument = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_INDIRECT_ARGUMENT,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a copy destination. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.Copy"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    CopyDestination = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_COPY_DEST,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a copy source. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.Copy"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    CopySource = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_COPY_SOURCE,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a MSAA resolve destination. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.Resolve"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    ResolveDestination = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_RESOLVE_DEST,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a MSAA resolve source. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.Resolve"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    ResolveSource = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_RESOLVE_SOURCE,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible as a shading rate source. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.PixelShading"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    ShadingRateSource = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_SHADING_RATE_SOURCE,
    /* TODO: We do not support ray tracing or video encoding/decoding (yet).  Leave this here for now, but ignore it. */
    /*
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for read as a raytracing acceleration structure. The resource MUST have been created using an initial state of 
    /// <see cref="TODO: D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE"/>. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// <item><description><see cref="BarrierSync.BuildRayTracingAccelerationStructure"/></description></item>
    /// <item><description><see cref="BarrierSync.CopyRayTracingAccelerationStructure"/></description></item>
    /// <item><description><see cref="BarrierSync.EmitRayTracingAccelerationStructurePostBuildInformation"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    RayTracingAccelerationStructureRead = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_RAYTRACING_ACCELERATION_STRUCTURE_READ,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for writing as a raytracing acceleration structure. The resource MUST have been created using an initial state of 
    /// <see cref="TODO: D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE"/>. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.ComputeShading"/></description></item>
    /// <item><description><see cref="BarrierSync.AllShading"/></description></item>
    /// <item><description><see cref="BarrierSync.BuildRayTracingAccelerationStructure"/></description></item>
    /// <item><description><see cref="BarrierSync.CopyRayTracingAccelerationStructure"/></description></item>
    /// <item><description><see cref="BarrierSync.EmitRayTracingAccelerationStructurePostBuildInformation"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    RayTracingAccelerationStructureWrite = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_RAYTRACING_ACCELERATION_STRUCTURE_WRITE,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for read-only access in a video decode queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VideoDecode"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoDecodeRead = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VIDEO_DECODE_READ,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for write access in a video decode queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VideoDecode"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoDecodeWrite = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VIDEO_DECODE_WRITE,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for read-only access in a video process queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VideoProcess"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoProcessRead = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VIDEO_PROCESS_READ,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for write access in a video process queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VideoProcess"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoProcessWrite = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VIDEO_PROCESS_WRITE,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for read-only access in a video encode queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VideoEncode"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoEncodeRead = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VIDEO_ENCODE_READ,
    /// <summary>
    /// <para>
    /// Indicates a resource is accessible for write access in a video encode queue. Runtime barrier validation ensures that this value is used with one or more of the following sync bits:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="BarrierSync.All"/></description></item>
    /// <item><description><see cref="BarrierSync.VideoEncode"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    VideoEncodeWrite = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_VIDEO_ENCODE_WRITE,*/
    /// <summary>
    /// <para>
    /// The resource is either not accessed before/after the barrier in the same command context execution context, or the data is no longer needed. This value is exclusive, and may not be combined with other 
    /// access bits.
    /// </para>
    /// <para>
    /// Using this value in a <c>Before</c> state with a <c>Sync Before</c> state of <see cref="BarrierSync.None"/> implies that a subresource was not accessed before the barrier in the current command context 
    /// execution scope. Likewise, using this value in a <c>After</c> state with a <c>Sync After</c> state of <see cref="BarrierSync.None"/> implies that a subresource is not accessed after the barrier in 
    /// the same scope.This is useful for initiating a layout transition as the final act on a resource before the end of a command context execution scope.
    /// </para>
    /// <para>
    /// Barriers used for aliased resource transitions can set this value on either <c>Before</c>/<c>After</c> states to indicate that aliased subresources do not share data across synchronization 
    /// boundaries. This can help avoid unnecessary cache flushes and layout transitions.
    /// </para>
    /// </summary>
    None = D3D12_BARRIER_ACCESS.D3D12_BARRIER_ACCESS_NO_ACCESS,

    /// <summary>
    /// The default access bits for an upload heap buffer resource.
    /// </summary>
    UploadHeapDefaultAccessState = VertexBuffer | IndexBuffer | ConstantBuffer | CopySource | ShaderResource | IndirectArgument
}

/// <summary>
/// Defines the type of memory layout for a resource.
/// </summary>
/// <remarks>
/// <para>
/// </para>
/// </remarks>
[Flags]
public enum BarrierLayout
{
    /// <summary>
    /// <para>
    /// Provides support for subresource layout changes where the previous layout is irrelevant or undefined. Typically, this is used for full-subresource or full-subresource Clear, Discard, and Copy 
    /// commands.
    /// </para>
    /// <para>
    /// This value, when suppplied to the <c>Before</c> AND <c>After</c> state, indicates a memory-access-only barrier. Many write operations support more than one layout (e.g. Copy operations support 
    /// <see cref="Common"/> or <see cref="CopyDestination"/>). A memory-access-only barrier can be used to flush writes to a texture without inadvertently changing the texture layout.
    /// </para>
    /// <para>
    /// Otherwise, a <c>Before</c>c OR <c>After</c> state with this value must set the corresponding <c>Access Before</c> or <c>Access After</c> value to <see cref="BarrierAccess.None"/>. A texture with an 
    /// undefined layout clearly does not have meaningful data and thus should not require preservation of data or cache flushes. Barriers used for aliasing can take advantage of this to let the GPU discard 
    /// outstanding cache writes.
    /// </para>
    /// </summary>
    None = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
    /// <summary>
    /// <para>
    /// Subresources with this layout are readable in any queue type without requiring a layout change. They are also writable as a copy dest in any queue type.
    /// </para>
    /// <para>
    /// Before calling <see cref="GorgonSwapChain.Present"/> on a <see cref="GorgonSwapChain"/>, the current back buffer layout must be set to this value using a <see cref="GorgonTextureBarrier"/>.
    /// </para>
    /// </summary>
    Common = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMMON,
    /// <summary>
    /// <para>
    /// Provides support for any read-only access (e.g. <see cref="BarrierAccess.ShaderResource"/> , <see cref="BarrierAccess.CopySource"/>). Should only be used for textures that require multiple, 
    /// concurrent read accesses since this may not be as optimal as a more specific read layout.
    /// </para>
    /// </summary>
    GenericRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_GENERIC_READ,
    /// <summary>
    /// <para>
    /// The resource is used as a render target. A subresource must be in this state when it is rendered to, or when it is cleared with <see cref="GorgonCommandList.ClearRenderTarget(GorgonTextureRenderTargetView, GorgonColor)"/>.
    /// </para>
    /// <para>
    /// This is a write-only state. To read from a render target as a shader resource, the resource must be set to <see cref="ShaderResource"/>.
    /// </para>
    /// </summary>
    RenderTarget = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_RENDER_TARGET,
    /// <summary>
    /// <para>
    /// The resource is used for unordered access. A subresource must be in this state when it is accessed by the GPU via an unordered access view. A subresource must also be in this state when it is cleared 
    /// with <see cref="GorgonCommandList.ClearUnorderedAccessView"/>. This is a read/write state.
    /// </para>
    /// </summary>
    UnorderedAccess = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNORDERED_ACCESS,
    /// <summary>
    /// <para>
    /// This state is mutually exclusive with other states. You should use it for <see cref="GorgonCommandList.ClearDepthStencilView"/> when the flags indicate a given subresource should be cleared 
    /// (otherwise the subresource state doesn't matter), or when using it in a writable depth stencil view when the PSO has depth write enabled.
    /// </para>
    /// </summary>
    DepthStencilWrite = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DEPTH_STENCIL_WRITE,
    /// <summary>
    /// <para>
    /// This is a state that can be combined with other states. It should be used when the subresource is in a read-only depth stencil view, or when depth write on a PSO is disabled. It can be combined with 
    /// other read states, such that the resource can be used for the depth or stencil test, and accessed by a shader within the same draw call. Using it when depth will be written by a draw call or clear 
    /// command is invalid.
    /// </para>
    /// </summary>
    DepthStencilRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DEPTH_STENCIL_READ,
    /// <summary>
    /// <para>
    /// The resource is used with a shader. A subresource must be in this state before being read by the shader via a shader resource view. This is a read-only state.
    /// </para>
    /// </summary>
    ShaderResource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_SHADER_RESOURCE,
    /// <summary>
    /// <para>
    /// The resource is used as the source in a copy operation. Subresources must be in this state when they are used as the source of copy operation, or a blt operation. This is a read-only state.
    /// </para>
    /// </summary>
    CopySource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COPY_SOURCE,
    /// <summary>
    /// <para>
    /// The resource is used as the destination in a copy operation. Subresources must be in this state when they are used as the destination of copy operation, or a blt operation. This is a write-only state.
    /// </para>
    /// </summary>
    CopyDestination = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COPY_DEST,
    /// <summary>
    /// <para>
    /// The resource is used as the source in a MSAA resolve operation.
    /// </para>
    /// </summary>
    ResolveSource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_RESOLVE_SOURCE,
    /// <summary>
    /// <para>
    /// The resource is used as the destination in a MSAA resolve operation.
    /// </para>
    /// </summary>
    ResolveDestination = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_RESOLVE_DEST,
    /// <summary>
    /// <para>
    /// This indicates that the resource is a screen-space shading-rate image for variable-rate shading (VRS). For more info, see 
    /// <a target="_blank" href="https://learn.microsoft.com/en-us/windows/win32/direct3d12/vrs">Variable-rate shading (VRS)</a>.
    /// </para>
    /// </summary>
    ShadingRateSource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_SHADING_RATE_SOURCE,
    /// <summary>
    /// <para>
    /// The resource is used as a source in a decode operation. Examples include reading the compressed bitstream and reading from decode references,
    /// </para>
    /// </summary>
    VideoDecodeRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_VIDEO_DECODE_READ,
    /// <summary>
    /// <para>
    /// The resource is used as a destination in the decode operation. This state is used for decode output and histograms.
    /// </para>
    /// </summary>
    VideoDecodeWrite = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_VIDEO_DECODE_WRITE,
    /// <summary>
    /// <para>
    /// The resource is used to read video data during video processing; that is, the resource is used as the source in a processing operation such as video encoding (compression).
    /// </para>
    /// </summary>
    VideoProcessRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_VIDEO_PROCESS_READ,
    /// <summary>
    /// <para>
    /// The resource is used to write video data during video processing; that is, the resource is used as the destination in a processing operation such as video encoding (compression).
    /// </para>
    /// </summary>
    VideoProcessWrite = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_VIDEO_PROCESS_WRITE,
    /// <summary>
    /// <para>
    /// The resource is used as the source in an encode operation. This state is used for the input and reference of motion estimation.
    /// </para>
    /// </summary>
    VideoEncodeRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_VIDEO_ENCODE_READ,
    /// <summary>
    /// <para>
    /// This resource is used as the destination in an encode operation. This state is used for the destination texture of a resolve motion vector heap operation.
    /// </para>
    /// </summary>
    VideoEncodeWrite = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_VIDEO_ENCODE_WRITE,
    /// <summary>
    /// <para>
    /// Supports common (barrier free) usage on graphics queues only. May be more optimal than the more general <see cref="Common"/>. Can only be used in barriers on graphics queues.
    /// </para>
    /// <para>
    /// Note that this cannot be used for Presentation on a <see cref="GorgonSwapChain"/>.
    /// </para>
    /// </summary>
    GraphicsCommon = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DIRECT_QUEUE_COMMON,
    /// <summary>
    /// <para>
    /// Same as <see cref="GenericRead"/> except with optimizations specific for graphics queues. Can only be used in barriers on graphics queues.
    /// </para>
    /// <para>
    /// In addition,this value includes support for read-only depth, shading-rate source, and resolve source accesses on direct queues.
    /// </para>
    /// </summary>
    GraphicsGenericRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DIRECT_QUEUE_GENERIC_READ,
    /// <summary>
    /// <para>
    /// Same as <see cref="UnorderedAccess"/> except with optimizations specific for graphics queues. Can only be used in barriers on graphics queues.
    /// </para>
    /// </summary>
    GraphicsUnorderedAccess = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DIRECT_QUEUE_UNORDERED_ACCESS,
    /// <summary>
    /// <para>
    /// Same as <see cref="ShaderResource"/> except with optimizations specific for graphics queues. Can only be used in barriers on graphics queues.
    /// </para>
    /// </summary>
    GraphicsShaderResource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DIRECT_QUEUE_SHADER_RESOURCE,
    /// <summary>
    /// <para>
    /// Same as <see cref="CopySource"/> except with optimizations specific for graphics queues. Can only be used in barriers on graphics queues.
    /// </para>
    /// </summary>
    GraphicsCopySource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DIRECT_QUEUE_COPY_SOURCE,
    /// <summary>
    /// <para>
    /// Same as <see cref="CopyDestination"/> except with optimizations specific for graphics queues. Can only be used in barriers on graphics queues.
    /// </para>
    /// <para>
    /// This flag may prevent costly, and unnecessary decompression on some layout transitions on resources with next access in a graphics queue.
    /// </para>
    /// </summary>
    GraphicsCopyDestination = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_DIRECT_QUEUE_COPY_DEST,
    /// <summary>
    /// <para>
    /// Supports common (barrier free) usage on compute queues only. May be more optimal than the more general <see cref="Common"/>. Can only be used in barriers on compute queues.
    /// </para>
    /// </summary>
    ComputeCommon = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMPUTE_QUEUE_COMMON,
    /// <summary>
    /// <para>
    /// Same as <see cref="GenericRead"/> except with optimizations specific for compute queues. Can only be used in barriers on compute queues.
    /// </para>
    /// </summary>
    ComputeGenericRead = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMPUTE_QUEUE_GENERIC_READ,
    /// <summary>
    /// <para>
    /// Same as <see cref="UnorderedAccess"/> except with optimizations specific for compute queues. Can only be used in barriers on compute queues.
    /// </para>
    /// </summary>
    ComputeUnorderedAccess = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMPUTE_QUEUE_UNORDERED_ACCESS,
    /// <summary>
    /// <para>
    /// Same as <see cref="ShaderResource"/> except with optimizations specific for compute queues. Can only be used in barriers on compute queues.
    /// </para>
    /// </summary>
    ComputeShaderResource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMPUTE_QUEUE_SHADER_RESOURCE,
    /// <summary>
    /// <para>
    /// Same as <see cref="CopySource"/> except with optimizations specific for compute queues. Can only be used in barriers on compute queues.
    /// </para>
    /// </summary>
    ComputeCopySource = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMPUTE_QUEUE_COPY_SOURCE,
    /// <summary>
    /// <para>
    /// Same as <see cref="CopyDestination"/> except with optimizations specific for compute queues. Can only be used in barriers on compute queues.
    /// </para>
    /// </summary>
    ComputeCopyDestination = D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_COMPUTE_QUEUE_COPY_DEST,
}

/// <summary>
/// Defines a sub section of a resource to be used in the barrier operation.
/// </summary>
/// <param name="firstMipLevel"><inheritdoc cref="FirstMipLevel" path="/summary"/></param>
/// <param name="mipLevelCount"><inheritdoc cref="MipLevelCount" path="/summary"/></param>
/// <param name="firstArrayIndex"><inheritdoc cref="FirstArrayIndex" path="/summary"/></param>
/// <param name="arrayCount"><inheritdoc cref="ArrayCount" path="/summary"/></param>
/// <param name="firstPlane"><inheritdoc cref="FirstPlane" path="/summary"/></param>
/// <param name="planeCount"><inheritdoc cref="PlaneCount" path="/summary"/></param>
public readonly struct GorgonSubResourceRange(short firstMipLevel, short mipLevelCount, short firstArrayIndex, short arrayCount, byte firstPlane, byte planeCount)
    : IEquatable<GorgonSubResourceRange>
{
    /// <summary>
    /// An empty sub resource range.
    /// </summary>
    public static readonly GorgonSubResourceRange Empty = new(-1, -1, -1, -1, 0, 0);

    /// <summary>
    /// The full resource.
    /// </summary>
    public static readonly GorgonSubResourceRange All = new(-1, 0, 0, 0, 0, 0);

    /// <summary>
    /// The index of the first mip map level.
    /// </summary>
    public readonly short FirstMipLevel = firstMipLevel;

    /// <summary>
    /// The first index in the texture array.
    /// </summary>
    public readonly short FirstArrayIndex = firstArrayIndex;

    /// <summary>
    /// The number of mip map levels in the range.
    /// </summary>
    public readonly short MipLevelCount = mipLevelCount;

    /// <summary>
    /// The number of array slices or depth slices in the range.
    /// </summary>
    public readonly short ArrayCount = arrayCount;

    /// <summary>
    /// The first plane slice.
    /// </summary>
    public readonly byte FirstPlane = firstPlane;

    /// <summary>
    /// The number of plane slices.
    /// </summary>
    public readonly byte PlaneCount = planeCount;

    /// <summary>
    /// Function to convert this type into a D3D barrier sub resource range.
    /// </summary>
    /// <returns>The D3D 12 barrier sub resource range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly D3D12_BARRIER_SUBRESOURCE_RANGE ToD3DBarrierSubResourceRange() => new((uint)FirstMipLevel, (uint)MipLevelCount,
                                                            (uint)FirstArrayIndex, (uint)ArrayCount,
                                                            FirstPlane, PlaneCount);

    /// <inheritdoc/>
    public readonly bool Equals(GorgonSubResourceRange other) => FirstMipLevel == other.FirstMipLevel
        && MipLevelCount == other.MipLevelCount
        && FirstArrayIndex == other.FirstArrayIndex
        && ArrayCount == other.ArrayCount
        && FirstPlane == other.FirstPlane
        && PlaneCount == other.PlaneCount;

    /// <inheritdoc cref="Equals(GorgonSubResourceRange)"/>
    public readonly bool Equals(ref readonly GorgonSubResourceRange other) => FirstMipLevel == other.FirstMipLevel
        && MipLevelCount == other.MipLevelCount
        && FirstArrayIndex == other.FirstArrayIndex
        && ArrayCount == other.ArrayCount
        && FirstPlane == other.FirstPlane
        && PlaneCount == other.PlaneCount;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonSubResourceRange range && Equals(in range);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(FirstMipLevel, MipLevelCount, FirstArrayIndex, ArrayCount, FirstPlane, PlaneCount);

    /// <summary>
    /// Operator to test for equality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(in GorgonSubResourceRange left, in GorgonSubResourceRange right) => left.Equals(in right);

    /// <summary>
    /// Operator to test for inequality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(in GorgonSubResourceRange left, in GorgonSubResourceRange right) => !left.Equals(in right);
}

/// <summary>
/// Defines a barrier for buffer resource data.
/// </summary>
public readonly struct GorgonBufferBarrier
    : IEquatable<GorgonBufferBarrier>
{
    /// <summary>
    /// The resource to block with the barrier.
    /// </summary>
    internal readonly ComPtr<ID3D12Resource2> Resource;

    /// <summary>
    /// An empty instance of a buffer barrier
    /// </summary>
    public static readonly GorgonBufferBarrier Empty = new();

    /// <summary>
    /// The type of synchronization to apply to the resource after the barrier has executed.
    /// </summary>
    public readonly BarrierSync Sync;

    /// <summary>
    /// The type of access to apply to the resource after the barrier has executed.
    /// </summary>
    public readonly BarrierAccess Access;

    /// <inheritdoc/>
    public unsafe readonly bool Equals(GorgonBufferBarrier other) => Resource.Get() == other.Resource.Get()
        && Sync == other.Sync
        && Access == other.Access;

    /// <summary>
    /// Function to convert this value into D3D 12 buffer barrier type.
    /// </summary>
    /// <param name="beforeSync">The previous sync bit.</param>
    /// <param name="beforeAccess">The previous access bit.</param>
    /// <returns>The D3D 12 buffer barrier.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe readonly D3D12_BUFFER_BARRIER ToD3DBufferBarrier(BarrierSync beforeSync, BarrierAccess beforeAccess) => new((D3D12_BARRIER_SYNC)beforeSync, (D3D12_BARRIER_SYNC)Sync,
                            (D3D12_BARRIER_ACCESS)beforeAccess, (D3D12_BARRIER_ACCESS)Access,
                            (PID3D12Resource2)Resource.Get());

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonBufferBarrier barrier && Equals(barrier);

    /// <summary>
    /// Operator to test for equality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(in GorgonBufferBarrier left, in GorgonBufferBarrier right) => left.Equals(right);

    /// <summary>
    /// Operator to test for inequality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(in GorgonBufferBarrier left, in GorgonBufferBarrier right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Resource, Sync, Access);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBufferBarrier"/> value type.
    /// </summary>
    /// <param name="resource"><inheritdoc cref="Resource" path="/summary"/></param>
    /// <param name="sync"><inheritdoc cref="Sync" path="/summary"/></param>
    /// <param name="access"><inheritdoc cref="Access" path="/summary"/></param>
    internal GorgonBufferBarrier(ComPtr<ID3D12Resource2> resource, BarrierSync sync, BarrierAccess access)        
    {
        Resource = resource;
        Sync = sync;
        Access = access;
    }

    /// <inheritdoc cref="GorgonBufferBarrier(ComPtr{ID3D12Resource2}, BarrierSync, BarrierAccess)"/>
    public GorgonBufferBarrier(GorgonGpuResource resource, BarrierSync sync, BarrierAccess access)
        : this(resource.D3DResource, sync, access)
    {
    }
}

/// <summary>
/// Defines a barrier for texture resource data.
/// </summary>
public readonly struct GorgonTextureBarrier
    : IEquatable<GorgonTextureBarrier>
{
    /// <summary>
    /// An empty instance of a texture barrier.
    /// </summary>
    public static readonly GorgonTextureBarrier Empty = new();

    /// <summary>
    /// The resource to block with the barrier.
    /// </summary>
    internal readonly ComPtr<ID3D12Resource2> Resource;

    /// <summary>
    /// The type of synchronization to apply to the resource after the barrier has executed.
    /// </summary>
    public readonly BarrierSync Sync;

    /// <summary>
    /// The type of access to apply to the resource after the barrier has executed.
    /// </summary>
    public readonly BarrierAccess Access;

    /// <summary>
    /// The layout of the texture data to apply tn the resource after the barrier has executed.
    /// </summary>
    public readonly BarrierLayout Layout;

    /// <summary>
    /// Property to set or return whether to initialize compression metadata for an aliased source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This parameter can only be used when the <see cref="Layout"/> is set to <see cref="BarrierLayout.None"/>.
    /// </para>
    /// </remarks>
    public bool Discard
    {
        readonly get;
        init;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe readonly bool Equals(GorgonTextureBarrier other) => Resource.Get() == other.Resource.Get()
        && Sync == other.Sync
        && Access == other.Access
        && Layout == other.Layout
        && Discard == other.Discard;

    /// <summary>
    /// Function to convert this value into D3D 12 texture barrier type.
    /// </summary>
    /// <param name="beforeSync">The previous sync bit.</param>
    /// <param name="beforeAccess">The previous access bit.</param>
    /// <param name="beforeTextureLayout">The previous texture layout state.</param>
    /// <param name="subResource">The sub resource to apply the barrier to.</param>
    /// <returns>The D3D 12 texture barrier.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the texture associated with this barrier has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly unsafe D3D12_TEXTURE_BARRIER ToD3DTextureBarrier(BarrierSync beforeSync, BarrierAccess beforeAccess, BarrierLayout beforeTextureLayout, ref readonly D3D12_BARRIER_SUBRESOURCE_RANGE subResource) =>
        new((D3D12_BARRIER_SYNC)beforeSync, (D3D12_BARRIER_SYNC)Sync,
                            (D3D12_BARRIER_ACCESS)beforeAccess, (D3D12_BARRIER_ACCESS)Access,
                            (D3D12_BARRIER_LAYOUT)beforeTextureLayout, (D3D12_BARRIER_LAYOUT)Layout,
                            (PID3D12Resource2)Resource.Get(),
                            in subResource,
                            Discard ? D3D12_TEXTURE_BARRIER_FLAGS.D3D12_TEXTURE_BARRIER_FLAG_DISCARD : D3D12_TEXTURE_BARRIER_FLAGS.D3D12_TEXTURE_BARRIER_FLAG_NONE);

    /// <inheritodc/>
    public override bool Equals(object? obj) => obj is GorgonTextureBarrier barrier && Equals(barrier);

    /// <summary>
    /// Operator to test for equality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(in GorgonTextureBarrier left, in GorgonTextureBarrier right) => left.Equals(right);

    /// <summary>
    /// Operator to test for inequality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(in GorgonTextureBarrier left, in GorgonTextureBarrier right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Resource, Sync, Access, Layout, Discard);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureBarrier"/> value type.
    /// </summary>
    /// <param name="resource"><inheritdoc cref="Resource" path="/summary"/></param>
    /// <param name="sync"><inheritdoc cref="Sync" path="/summary"/></param>
    /// <param name="access"><inheritdoc cref="Access" path="/summary"/></param>
    /// <param name="layout"><inheritdoc cref="Layout" path="/summary"/></param>
    internal GorgonTextureBarrier(ComPtr<ID3D12Resource2> resource, BarrierSync sync, BarrierAccess access, BarrierLayout layout)
    {
        Resource = resource;
        Sync = sync;
        Access = access;
        Layout = layout;
    }

    /// <inheritdoc cref="GorgonTextureBarrier(ComPtr{ID3D12Resource2}, BarrierSync, BarrierAccess, BarrierLayout)"/>
    public GorgonTextureBarrier(GorgonGpuResource resource, BarrierSync sync, BarrierAccess access, BarrierLayout layout)
        : this(resource.D3DResource, sync, access, layout)
    {
    }
}