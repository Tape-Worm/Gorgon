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
/// Provides a fluent interface for systems that implement copying functionality.
/// </summary>
public interface IGorgonCopyMethodsFluent<T>
{
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
    T SetBarrier(GorgonTexture texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources = null, bool discard = false, bool force = false);

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
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="GorgonCommandList"/>
    T SetBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access, bool force = false);

    /// <summary>
    /// Function to copy a single value into a buffer.
    /// </summary>
    /// <typeparam name="Tv">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">[Optional] The offset, in bytes, within the <paramref name="buffer"/> to start writing at.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonResourceCopier.ValidateRangeParams(GorgonGpuBufferCommon, long, long, int)" path="/exception[not(@cref='T:Gorgon.Core.GorgonException')]"/>
    /// <exception cref="GorgonException"><para><inheritdoc cref="GorgonResourceCopier.ValidateRangeParams(GorgonGpuBufferCommon, long, long, int)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/node()"/></para>
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
    /// Depending on the <see cref="BufferUsage"/> of the <paramref name="buffer"/>, data may not be immediately written to the buffer until the <see cref="IGorgonResourceWriter.End"/> method is called. Because of this, the 
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
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="IGorgonResourceWriter.End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    T CopyValue<Tv>(in Tv value, GorgonGpuBufferCommon buffer, long offset = 0) where Tv : unmanaged;

    /// <summary>
    /// Function to copy the contents of memory pointed at by a <see cref="GorgonPtr{T}"/> into a buffer.
    /// </summary>
    /// <typeparam name="Tv">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="pointer">The pointer to memory containing the data to write into the buffer.</param>
    /// <param name="buffer"><inheritdoc cref="CopyValue" path="/param[@name='buffer']"/></param>
    /// <param name="offset"><inheritdoc cref="CopyValue" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> pointer is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method writes data into the <paramref name="buffer"/> from the <see cref="GorgonPtr{T}"/> type specified by the <paramref name="pointer"/> parameter. Applications can use this to write directly 
    /// from the contents of native/pinned memory pointed at by a <see cref="GorgonPtr{T}"/> or a <see cref="GorgonNativeBuffer{T}"/> into a <see cref="GorgonGpuBuffer"/> or <see cref="GorgonIndexBuffer"/>. 
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
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="IGorgonResourceWriter.End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    T CopyPointer<Tv>(GorgonPtr<Tv> pointer, GorgonGpuBufferCommon buffer, long offset = 0) where Tv : unmanaged;

    /// <summary>
    /// Function to copy a range of values into a buffer.
    /// </summary>
    /// <typeparam name="Tv">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="values">The span of values to write.</param>
    /// <param name="buffer"><inheritdoc cref="CopyValue" path="/param[@name='buffer']"/></param>
    /// <param name="offset"><inheritdoc cref="CopyValue" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method writes data into the <paramref name="buffer"/> from the read only span type specified by the <paramref name="values"/> parameter. Applications can use this to write directly from arrays 
    /// into a <see cref="GorgonGpuBufferCommon"/>.
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
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="IGorgonResourceWriter.End"/>
    /// <seealso cref="StructLayoutAttribute"/>
    /// <seealso cref="LayoutKind"/>
    T CopyRange<Tv>(ReadOnlySpan<Tv> values, GorgonGpuBufferCommon buffer, long offset = 0) where Tv : unmanaged;

    /// <summary>
    /// Function to copy the contents of one <see cref="GorgonGpuBufferCommon"/> to another.
    /// </summary>
    /// <param name="source">The buffer to copy from.</param>
    /// <param name="destination">The buffer to copy into.</param>
    /// <param name="sourceOffset">[Optional] The offset, in bytes, within the source buffer to start reading from.</param>
    /// <param name="destinationOffset">[Optional] The offset, in bytes, within the destination buffer to start writing into.</param>
    /// <param name="count">[Optional] The number of bytes to copy.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="sourceOffset"/>, or <paramref name="destinationOffset"/> parameters are less than 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="count"/> parameter is less than 1.</para>
    /// </exception>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="sourceOffset"/> plus the <paramref name="count"/> exceeds the size of the <paramref name="source"/> buffer.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="destinationOffset"/> plus the <paramref name="count"/> exceeds the size of the <paramref name="destination"/> buffer.</para>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method copies the entire, or partial, contents of a <see cref="GorgonGpuBufferCommon"/> to another <see cref="GorgonGpuBufferCommon"/>. 
    /// </para>
    /// <para>
    /// If the <paramref name="sourceOffset"/>, or <paramref name="destinationOffset"/> are specified, then reading and writing will begin at the specified byte offsets. If these are not specified, then the 
    /// data will be copied from and to the beginning of each buffer respectively.
    /// </para>
    /// <para>
    /// If the <paramref name="count"/> parameter is specified, the method will copy up to the number of bytes passed in. If it is omitted, then the number of bytes copied will be the smaller of the two 
    /// buffer sizes, minus their respective offsets.
    /// </para>
    /// </para>
    /// <para type="endrequired">
    /// The copy operation in this method is deferred until the application calls the <see cref="IGorgonResourceWriter.End"/> method.
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
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="IGorgonResourceWriter.End"/>
    T CopyBuffer(GorgonGpuBufferCommon source, GorgonGpuBufferCommon destination, long sourceOffset = 0, long destinationOffset = 0, long? count = null);

    /// <summary>
    /// Function to copy a <see cref="IGorgonImage"/> into a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <param name="image">The image data to copy into the texture.</param>
    /// <param name="texture">The texture that will receive the image data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="texture"/> uses multi sampling.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="texture"/> is <see cref="GorgonTextureInfo.IsDepthStencil">configured to be used as a depth/stencil texture</see>.</para>
    /// </exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="image"/> <see cref="BufferFormat">format</see> is not compatible with the format of the texture.</exception>    
    /// <seealso cref="IGorgonImage"/>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the contents of a <see cref="IGorgonImage"/> into a <see cref="GorgonTexture"/> so that applications can use image data as textures. This method copies the entire image to the 
    /// texture, if an application needs to more fine grained copying, use the <see cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/> overload.
    /// </para>
    /// <para>
    /// If the <paramref name="texture"/> dimensions, array count (1D or 2D only), or mip count are not the same as those in the <paramref name="image"/>, then the method will only copy the minimum 
    /// dimensions, array count and/or mip count. For example, if the image has 5 array indices, and the texture only has 2 array indices, this method will only copy the first two indices. This ensures we 
    /// don't have an overrun when copying. 
    /// </para>
    /// <para>
    /// If the texture <see cref="GorgonTexture.Format"/> does not match that of the <paramref name="image"/>, and the image can be converted to the format of the texture, the method will automatically do so 
    /// prior to copying into the texture. If it cannot convert the image due to an incompatible format, then an exception will be thrown.
    /// </para>
    /// <para type="Limits">
    /// This method also has the following limitations for the destination <paramref name="texture"/>.
    /// <list type="bullet">
    /// <item>
    ///     <description>Textures that are created for use as a <see cref="GorgonTextureInfo.IsDepthStencil">Depth/Stencil</see> cannot be used as a destination. An exception will be thrown if an attempt to copy 
    ///     into a depth/stencil texture is made.</description>
    /// </item>
    /// <item>
    ///     <description>Textures that are created using <see cref="GorgonTextureInfo.MultisampleInfo">Multi-sampling</see> (i.e. a multi-sample value that is not equal to 
    ///     <see cref="GorgonMultisampleInfo.NoMultisampling"/>) cannot be used as a destination. An exception will be thrown if an attempt to copy into a multi-sampled texture is made.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonTextureInfo"/>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/>
    T CopyImageToTexture(IGorgonImage image, GorgonTexture texture);

    /// <summary>
    /// Function to copy a <see cref="IGorgonImageBuffer"/> into a <see cref="GorgonTexture"/> sub resource.
    /// </summary>
    /// <param name="imageBuffer">The image data buffer to copy into the texture.</param>
    /// <param name="texture"><inheritdoc cref="CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/param[@name='texture']"/></param>
    /// <param name="destinationMipLevel">[Optional] The destination mip level on the texture to copy the image data into.</param>
    /// <param name="destinationZOrArrayIndex">[Optional] The destination depth slice on a 3D texture, or array index on a 1D or 2D texture array to copy the image data into.</param>
    /// <param name="destinationPlane">[Optional] The destination format plane on the texture to copy the image data into.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/exception"/>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the contents of an individual <see cref="IGorgonImageBuffer"/> on a <see cref="IGorgonImage"/> into a single texture sub resource. This method only copies one buffer, if the 
    /// application needs to copy the entire image instead, use the <see cref="CopyImageToTexture(IGorgonImage, GorgonTexture)"/> overload.
    /// </para>
    /// <para>
    /// If the buffer dimensions are not the same as the sub resource, then the image data will be cropped to fit as to prevent a buffer overrun. Unlike the 
    /// <see cref="CopyImageToTexture(IGorgonImage, GorgonTexture)"/> no image conversion is done with this method, and an exception will be thrown if the <see cref="BufferFormat"/>s do not match.
    /// </para>
    /// <para>
    /// If the <paramref name="destinationMipLevel"/>, <paramref name="destinationZOrArrayIndex"/>, and the <paramref name="destinationPlane"/> is not specified, then the first mip level, first array index 
    /// (or depth slice for a 3D texture), and the first format plane are used to copy.
    /// </para>
    /// <para>
    /// Like the dimensions, the <paramref name="destinationMipLevel"/>, <paramref name="destinationZOrArrayIndex"/>, and <paramref name="destinationPlane"/> parameters are clamped to a minimum value of 0, 
    /// and to the maximum mip level, array indices (or depth slices), and plane count of the destination <paramref name="texture"/>.
    /// </para>
    /// <inheritdoc cref="CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/remarks/para[@type='Limits']"/>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonTextureInfo"/>
    /// <seealso cref="IGorgonImageBuffer"/>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="CopyImageToTexture(IGorgonImage, GorgonTexture)"/>
    T CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel = 0, short destinationZOrArrayIndex = 0, byte destinationPlane = 0);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonGpuBuffer"/> into a specific sub resource of a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to copy.</param>
    /// <param name="texture">The texture to copy the data into.</param>
    /// <param name="parameters">The parameters to define what offset to copy from in the buffer, and which sub resource will receive the data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the size, in bytes, of <paramref name="buffer"/> minus the <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.SourceOffset"/> is larger than 
    /// the texture sub resource size.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.SourceOffset"/> is less than 0.</exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the data within the specified <see cref="GorgonGpuBuffer"/> into the specified sub resource for the specified <see cref="GorgonTexture"/>. This allows applications to load 
    /// arbitrary data into a texture sub resource. To fill an entire texture with the contents of a buffer, call the <see cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, long)"/> overload.
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.SourceOffset"/> parameter, must be at least the same size, or less than the size, in 
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
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.DestinationArrayIndex"/> is the array index in texture array that will receive the data. This only applies to a 
    /// <paramref name="texture"/> with a <see cref="GorgonTexture.Type"/> of <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.
    /// <note type="information">
    /// <para>
    /// You cannot copy into an individual depth slice using this method. To perform a copy on an individual depth slice, use the 
    /// <see cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/> method.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.DestinationMipLevel"/> is the mip map level that will receive the data in the <paramref name="buffer"/>. By default 
    /// this is the top mip level.
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.DestinationPlane"/> is the format plane for a <see cref="BufferFormat"/> that use multiple planes. For example, the 
    /// depth buffer format <see cref="BufferFormat.D24_UNorm_S8_UInt"/> has a 24 bit depth plane, and an 8 bit stencil plane), and a 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.DestinationPlane"/> of 0 would write into the depth portion, and a value of 1 would write into the stencil portion. 
    /// </para>
    /// <para>
    /// All destination parameters are clipped against their respective values for the <paramref name="texture"/>. This ensures that the values passed can never exceed the minimum and maximum mip levels, 
    /// array/depth count, and plane count for the <paramref name="texture"/>.
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonSubResourceInfo"/>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, long)"/>
    /// <seealso cref="TextureType"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="GorgonCopyBufferToTexture"/>
    T CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, GorgonCopyBufferToTexture parameters);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonGpuBuffer"/> into a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <param name="buffer"><inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/param[@name='buffer']"/></param>
    /// <param name="texture"><inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/param[@name='texture']"/></param>
    /// <param name="sourceOffset">[Optional] The number of bytes within the buffer to start copying from.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the size, in bytes, of <paramref name="buffer"/> minus the <paramref name="sourceOffset"/> is larger than the texture sub resource size, in bytes.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="sourceOffset"/> is less than 0.</exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the data within the specified <see cref="GorgonGpuBuffer"/> into a <see cref="GorgonTexture"/>. This allows applications to load arbitrary data into a texture. To copy data from 
    /// a buffer into an arbitrary sub resource on a texture, call the <see cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)"/> overload.
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the <paramref name="sourceOffset"/> parameter, must be at least the same size, in 
    /// bytes, as the full texture. 
    /// </para>
    /// <para>
    /// The <paramref name="buffer"/> content should also match the alignment requirements for the <paramref name="texture"/>. This means the <see cref="GorgonSubResourceInfo.RowPitch"/> should match between 
    /// the two resources. For example, if the texture has a row pitch alignment of 256 bytes, then the data in the buffer should be aligned in the same way. So, for a texture sub resource with a width of 
    /// 300, its row pitch may be aligned to 512 bytes. This means that a row in the buffer data should also be aligned to 512 bytes and <b>not</b> the width of the sub resource. This information can be 
    /// retrieved from the aforementioned <see cref="GorgonTexture.SubResources"/> list.
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonSubResourceInfo"/>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/>
    T CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, long sourceOffset = 0);

    /// <summary>
    /// Function to copy <see cref="GorgonTexture"/> sub resource into a <see cref="GorgonGpuBuffer"/>.
    /// </summary>
    /// <param name="texture">The texture containing the sub resource to copy.</param>
    /// <param name="buffer">The buffer that will receive a copy of the data.</param>
    /// <param name="parameters">The parameters used to determine what to copy..</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if the <paramref name="texture"/> sub resource size, in bytes, is too large to fit within the <paramref name="buffer"/>.</exception>    
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.DestinationOffset"/> is less than 0.</exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the subresource from the <see cref="GorgonTexture"/> into the <see cref="GorgonGpuBuffer"/>. This allows applications to load 
    /// <paramref name="texture"/> sub resource data into a <paramref name="buffer"/>. To fill a <paramref name="buffer"/> with the entire <paramref name="texture"/>, call the 
    /// <see cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, long)"/> overload.
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.DestinationOffset"/> parameter, must be at least the same size, in bytes, as the texture sub resource. To determine the size of a 
    /// sub resource use the <see cref="GorgonTexture.SubResources"/> list on the <paramref name="texture"/> to retrieve a 
    /// <see cref="GorgonSubResourceInfo"/> data structure and use the <see cref="GorgonSubResourceInfo.SizeInBytes"/> property, which can be used to determine the size of the <paramref name="buffer"/>. 
    /// </para>
    /// <para type="alignment">
    /// The <paramref name="buffer"/> content should also match the alignment requirements for the <paramref name="texture"/>. This means the <see cref="GorgonSubResourceInfo.RowPitch"/> should match between 
    /// the two resources. For example, if the texture has a row pitch alignment of 256 bytes, then the data in the buffer should be aligned in the same way. So, for a texture sub resource with a width of 
    /// 300, its row pitch may be aligned to 512 bytes. This means that a row in the buffer data should also be aligned to 512 bytes and <b>not</b> the width of the sub resource. This information can be 
    /// retrieved from the aforementioned <see cref="GorgonTexture.SubResources"/> list.
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.SourceArrayIndex"/> is the array index in texture array to copy from. This only applies to a <paramref name="texture"/> with a 
    /// <see cref="GorgonTexture.Type"/> of <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.
    /// <note type="information">
    /// <para>
    /// You cannot copy into an individual depth slice using this method. To perform a copy on an individual depth slice, use the 
    /// <see cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)"/> method.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.SourceMipLevel"/> is the mip map level to copy from the <paramref name="texture"/>. By default this is the top mip level.
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.SourcePlane"/> is the format plane for a <see cref="BufferFormat"/> that use multiple planes. For example, the 
    /// depth buffer format <see cref="BufferFormat.D24_UNorm_S8_UInt"/> has a 24 bit depth plane, and an 8 bit stencil plane), and a 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.SourcePlane"/> of 0 would write into the depth portion, and a value of 1 would write into the stencil portion. 
    /// </para>
    /// <para>
    /// All source parameters are clipped against their respective values for the <paramref name="texture"/>. This ensures that the values passed can never exceed the minimum and maximum mip levels, 
    /// array count, and plane count for the <paramref name="texture"/>.
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonSubResourceInfo"/>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, long)"/>
    /// <seealso cref="TextureType"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="GorgonCopyTextureToBuffer"/>
    T CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, GorgonCopyTextureToBuffer parameters);

    /// <summary>
    /// Function to copy a <see cref="GorgonTexture"/> into a <see cref="GorgonGpuBuffer"/>.
    /// </summary>
    /// <param name="texture">The texture to copy.</param>
    /// <param name="buffer"><inheritdoc cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/param[@name='buffer']"/></param>
    /// <param name="destinationOffset">[Optional] The offset, in bytes, within the buffer to start copying into.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/exception[@cref='T:Gorgon.Core.GorgonException']"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="destinationOffset"/> is less than 0.</exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the entire <see cref="GorgonTexture"/> into the <see cref="GorgonGpuBuffer"/>. This allows applications to load a full <paramref name="texture"/> a <paramref name="buffer"/>. 
    /// To copy only a sub resource to a <paramref name="buffer"/>, call the <see cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)"/> overload.
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the <paramref name="destinationOffset"/> parameter, must be at least the same size, 
    /// in bytes, as the <paramref name="texture"/>. Users may use the <see cref="GorgonTexture.SizeInBytes"/> property to determine how large the buffer needs to be. 
    /// </para>
    /// <inheritdoc cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)" path="/remarks/path[@type='alignment']"/>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyTextureToBuffer(GorgonTexture, GorgonGpuBuffer, GorgonCopyTextureToBuffer)"/>
    T CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, long destinationOffset = 0);

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
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.SourceRegion"/> is not empty, or the <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationX"/>, 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationY"/> or <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (3D textures only) parameters are not 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="source"/>, or <paramref name="destination"/> parameters have a <see cref="GorgonTexture.MultisampleInfo"/> not equal to <see cref="GorgonMultisampleInfo.NoMultisampling"/> and the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.SourceRegion"/> is not empty, or the <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationX"/>, 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationY"/> or <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (3D textures only) parameters are not 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="source"/> texture <see cref="GorgonTexture.Type"/> is unknown.</para>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This will copy a sub resource from one <see cref="GorgonTexture"/> to another sub resource in another <see cref="GorgonTexture"/>, or, another sub resource in the same <see cref="GorgonTexture"/>. 
    /// This method only copies a single sub resource at a time, to copy the full texture, use the <see cref="CopyTexture(GorgonTexture, GorgonTexture)"/> overload.
    /// </para>
    /// <para>
    /// When copying a region on a sub resource, and the destination sub resource does not have the same dimensions, it will be clipped against the smaller sub resource. This keeps from triggering a buffer 
    /// overflow. 
    /// </para>
    /// <para>
    /// The <see cref="GorgonCopyTextureSubResource.SourceMipLevel"/>, <see cref="GorgonCopyTextureSubResource.DestinationMipLevel"/>, <see cref="GorgonCopyTextureSubResource.SourceRegion"/> Z value (1D and 
    /// 2D textures only), <see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (1D and 2D textures only), <see cref="GorgonCopyTextureSubResource.SourcePlane"/>, and 
    /// <see cref="GorgonCopyTextureSubResource.DestinationPlane"/> values are constrained to a minimum value of 0, and limited to the maximum values for each texture.
    /// </para>
    /// <para>
    /// This method also has the following limitations for the <paramref name="source"/> and <paramref name="destination"/> texture.
    /// <list type="bullet">
    /// <item>
    ///     <description>The textures must have identical <see cref="GorgonMultisampleInfo">multi-sample</see> values.</description>
    /// </item>
    /// <item>
    ///     <description>The textures must have a format that belongs to the same <see cref="GorgonFormatInfo.Group"/>.</description>
    /// </item>
    /// <item>
    ///     <description>If either of the textures are depth/stencil textures, the <see cref="GorgonCopyTextureSubResource.SourceRegion"/> must be empty and 
    ///     <see cref="GorgonCopyTextureSubResource.DestinationX"/>, <see cref="GorgonCopyTextureSubResource.DestinationY"/>, and <see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (3D 
    ///     textures only) must be set to 0.</description>
    /// </item>
    /// <item>
    ///     <description>If either of the textures use multi-sampling, the <see cref="GorgonCopyTextureSubResource.SourceRegion"/> must be empty and 
    ///     <see cref="GorgonCopyTextureSubResource.DestinationX"/>, <see cref="GorgonCopyTextureSubResource.DestinationY"/>, and <see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (3D 
    ///     textures only) must be set to 0.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="CopyTexture(GorgonTexture, GorgonTexture)"/>
    /// <seealso cref="GorgonCopyTextureSubResource"/>
    /// <seealso cref="GorgonMultisampleInfo"/>
    T CopyTexture(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters);

    /// <summary>
    /// Function to copy an entire texture into another.
    /// </summary>
    /// <param name="source">The texture to copy.</param>
    /// <param name="destination">The texture that will receive the data.</param>
    /// <inheritdoc cref="SetBarrier(GorgonTexture, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/returns"/>
    /// <exception cref="ArgumentException"><para><inheritdoc cref="CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception[@cref='T:System.ArgumentException']/node()"/></para></exception>
    /// <exception cref="GorgonException"><para><inheritdoc cref="CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/node()"/></para>
    /// <para>-or-</para>
    /// <para>Thrown if the source texture and destination texture do not meet the restrictions for this method.</para>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the entirety of the <paramref name="source"/> texture to the <paramref name="destination"/> texture. This method is more efficient than copying individual subresources with the 
    /// <see cref="CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)"/> method.
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
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="TextureType"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)"/>
    T CopyTexture(GorgonTexture source, GorgonTexture destination);
}