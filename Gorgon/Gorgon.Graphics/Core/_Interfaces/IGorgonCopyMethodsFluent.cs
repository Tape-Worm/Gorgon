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
/// <typeparam name="T">The type of object to return as the fluent interface.</typeparam>
public interface IGorgonCopyMethodsFluent<T>
{
    /// <summary>
    /// Function to copy a single value into a buffer.
    /// </summary>
    /// <typeparam name="Tv">The type of data to write to the GPU buffer. Must be an unmanaged compatible struct type.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">[Optional] The offset, in bytes, within the <paramref name="buffer"/> to start writing at.</param>
    /// <returns>The fluent interface for the for the resource writer.</returns>
    /// <inheritdoc cref="GorgonResourceCopier.ValidateRangeParams(GorgonGpuBufferCommon, long, long, int)" path="/exception[not(@cref='T:Gorgon.Core.GorgonException')]"/>
    /// <exception cref="GorgonException"><para><inheritdoc cref="GorgonResourceCopier.ValidateRangeParams(GorgonGpuBufferCommon, long, long, int)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/node()"/></para>
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
    /// </remarks>
    /// <example>
    /// <code language="csharp">
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
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
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
    /// <code language="csharp">
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
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method writes data into the <paramref name="buffer"/> from the read only span type specified by the <paramref name="values"/> parameter. Applications can use this to write directly from arrays 
    /// into a <see cref="GorgonGpuBufferCommon"/>.
    /// </para>
    /// <inheritdoc cref="CopyValue" path="/remarks/para[@type='CopyCommon']"/>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
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
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="sourceOffset"/>, or <paramref name="destinationOffset"/> parameters are less than 0.</para>
    /// <para>Thrown if the <paramref name="count"/> parameter is less than 1.</para>
    /// </exception>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="sourceOffset"/> plus the <paramref name="count"/> exceeds the size of the <paramref name="source"/> buffer.</para>
    /// <para>Thrown if the <paramref name="destinationOffset"/> plus the <paramref name="count"/> exceeds the size of the <paramref name="destination"/> buffer.</para>
    /// </exception>
    /// <inheritdoc cref="CopyValue{T}(in T, GorgonGpuBufferCommon, long)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[2]"/>
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
    /// <code language="csharp">    
    /// <![CDATA[
    /// byte[] sourceData = new byte[1024];
    /// 
    /// // Code to write data to the sourceData array goes here...
    /// 
    /// using GorgonGpuBuffer destBuffer = new(_graphics, "Destination Buffer", new GorgonBufferInfo(256);
    /// using GorgonGpuBuffer srcBuffer = new(_graphics, "Source Buffer", new GorgonBufferInfo(256);
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
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="texture"/> uses multisampling.</para>
    /// <para>Thrown if the <paramref name="texture"/> is <see cref="GorgonTextureInfo.IsDepthStencil">configured to be used as a depth/stencil texture</see>.</para>
    /// </exception>
    /// <exception cref="GorgonException">
    /// <para>
    /// Thrown if the <paramref name="image"/> <see cref="BufferFormat">format</see> is not compatible with the format of the texture.
    /// </para>
    /// <inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/>
    /// </exception>    
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
    /// If the texture <see cref="GorgonTextureCommon.Format"/> does not match that of the <paramref name="image"/>, and the image can be converted to the format of the texture, the method will automatically do so 
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
    ///     <description>Textures that are created using <see cref="GorgonTextureInfo.MultisampleInfo">Multisampling</see> (i.e. a multi-sample value that is not equal to 
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
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <inheritdoc cref="CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/exception"/>
    /// <exception cref="GorgonException"><inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/></exception>
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
    /// Function to copy a <see cref="IGorgonImageBuffer"/> into a <see cref="GorgonVirtualTexture"/> sub resource.
    /// </summary>
    /// <param name="imageBuffer"><inheritdoc cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/param[@name='imageBuffer']"/></param>
    /// <param name="texture">The virtual texture that will receive the data from the image buffer.</param>
    /// <param name="handle">The allocation handle returned from the texture to indicate where the data should be stored.</param>
    /// <param name="destinationDepthSlice">[Optional] For 3D textures only. Indicates which depth slice will receive the buffer data.</param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="handle"/> refers to a handle that has not been allocated on the <paramref name="texture"/>.</para></exception>
    /// <exception cref="GorgonException"><inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/></exception>
    /// <inheritdoc cref="CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/exception[@cref='T:Gorgon.Core.GorgonException']"/>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the contents of an individual <see cref="IGorgonImageBuffer"/> on a <see cref="IGorgonImage"/> into a single virtual texture sub resource allocation, which is represent by the 
    /// <paramref name="handle"/> parameter. Use the <see cref="GorgonVirtualTexture.TryAllocate(ref readonly GorgonBoxF, out GorgonVirtualTextureHandle, short, short)"/> method to receive this handle before copying.
    /// </para>
    /// <inheritdoc cref="CopyImageToTexture(IGorgonImageBuffer, GorgonTexture, short, short, byte)" path="/remarks/para/para[2]"/>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonVirtualTexture"/>
    T CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonVirtualTexture texture, GorgonVirtualTextureHandle handle, short destinationDepthSlice = 0);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonGpuBuffer"/> into a specific sub resource of a <see cref="GorgonTexture"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to copy.</param>
    /// <param name="texture">The texture to copy the data into.</param>
    /// <param name="parameters">The parameters to define what offset to copy from in the buffer, and which sub resource will receive the data.</param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="ArgumentException">Thrown if the size, in bytes, of <paramref name="buffer"/> minus the <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.SourceOffset"/> is larger than 
    /// the texture sub resource size.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.SourceOffset"/> is less than 0.</exception>
    /// <exception cref="GorgonException"><inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/></exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the data within the specified <paramref name="buffer"/> into the specified sub resource for the specified <paramref name="texture"/>. This allows applications to load arbitrary 
    /// data into a texture sub resource. 
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.SourceOffset"/> parameter, must be at least the same size, or less than the size, in 
    /// bytes, as the texture sub resource. To determine the size of a sub resource use the <see cref="GorgonTextureCommon.SubResources"/> list on the <paramref name="texture"/> to retrieve a 
    /// <see cref="GorgonSubResourceInfo"/> data structure and use the <see cref="GorgonSubResourceInfo.SizeInBytes"/> property, which can be used to determine the size of the <paramref name="buffer"/>. 
    /// </para>
    /// <para type='alignment'>
    /// <para>
    /// The <paramref name="buffer"/> content should also match the alignment requirements for the <paramref name="texture"/>. This means the <see cref="GorgonSubResourceInfo.RowPitch"/> should match between 
    /// the two resources. For example, if the texture has a row pitch alignment of 256 bytes, then the data in the buffer should be aligned in the same way. So, for a texture sub resource with a width of 
    /// 300, its row pitch may be aligned to 512 bytes. This means that a row in the buffer data should also be aligned to 512 bytes and <b>not</b> the width of the sub resource. This information can be 
    /// retrieved from the aforementioned <see cref="GorgonTextureCommon.SubResources"/> list.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The <paramref name="buffer"/> has no width, height or depth information for the sub resource, nor does it have any information about the format of the pixel data. Thus it is the responsibility of 
    /// the user to ensure that the texture and buffer data are compatible.
    /// </para>
    /// </note>
    /// </para>
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyBufferToTexture.DestinationArrayIndex"/> is the array index in texture array that will receive the data. This only applies to a 
    /// <paramref name="texture"/> with a <see cref="GorgonTextureCommon.Type"/> of <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.
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
    /// <seealso cref="TextureType"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="GorgonCopyBufferToTexture"/>
    T CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, GorgonCopyBufferToTexture parameters);

    /// <summary>
    /// Function to copy <see cref="GorgonTexture"/> sub resource into a <see cref="GorgonGpuBuffer"/>.
    /// </summary>
    /// <param name="texture">The texture containing the sub resource to copy.</param>
    /// <param name="buffer">The buffer that will receive a copy of the data.</param>
    /// <param name="parameters">The parameters used to determine what to copy..</param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="GorgonException">Thrown if the <paramref name="texture"/> sub resource size, in bytes, is too large to fit within the <paramref name="buffer"/>.</exception>    
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.DestinationOffset"/> is less than 0.</exception>
    /// <exception cref="GorgonException"><inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/></exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the subresource from the <paramref name="texture"/> into the <paramref name="buffer"/>. 
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.DestinationOffset"/> parameter, must be at least the same size, in bytes, as the texture sub resource. To determine the size of a 
    /// sub resource use the <see cref="GorgonTextureCommon.SubResources"/> list on the <paramref name="texture"/> to retrieve a 
    /// <see cref="GorgonSubResourceInfo"/> data structure and use the <see cref="GorgonSubResourceInfo.SizeInBytes"/> property, which can be used to determine the size of the <paramref name="buffer"/>. 
    /// </para>
    /// <para type="alignment">
    /// <para>
    /// The <paramref name="buffer"/> content should also match the alignment requirements for the <paramref name="texture"/>. This means the <see cref="GorgonSubResourceInfo.RowPitch"/> should match between 
    /// the two resources. For example, if the texture has a row pitch alignment of 256 bytes, then the data in the buffer should be aligned in the same way. So, for a texture sub resource with a width of 
    /// 300, its row pitch may be aligned to 512 bytes. This means that a row in the buffer data should also be aligned to 512 bytes and <b>not</b> the width of the sub resource. This information can be 
    /// retrieved from the aforementioned <see cref="GorgonTextureCommon.SubResources"/> list.
    /// </para>
    /// </para>
    /// <para>
    /// The <paramref name="parameters"/>.<see cref="GorgonCopyTextureToBuffer.SourceArrayIndex"/> is the array index in texture array to copy from. This only applies to a <paramref name="texture"/> with a 
    /// <see cref="GorgonTextureCommon.Type"/> of <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.
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
    /// <seealso cref="TextureType"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="GorgonCopyTextureToBuffer"/>
    T CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, GorgonCopyTextureToBuffer parameters);

    /// <summary>
    /// Function to copy a texture subresource into another texture subresource.
    /// </summary>
    /// <param name="source">The texture with the data to copy.</param>
    /// <param name="destination">The texture that will receive data to copy.</param>
    /// <param name="parameters">The parameters used to define which sub resource to copy, and which sub resource will receive the copied data.</param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="GorgonException"><para>Thrown if the <paramref name="source"/> and <paramref name="destination"/> have a <see cref="BufferFormat"/> that does not belong to the same <see cref="GorgonFormatInfo.Group"/>.</para>
    /// <para>Thrown if the <paramref name="source"/> and <paramref name="destination"/> do not have matching <see cref="GorgonTextureCommon.MultisampleInfo"/> values.</para>
    /// <inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/>
    /// </exception>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="source"/>, or <paramref name="destination"/> parameters have a <see cref="GorgonTextureCommon.IsDepthStencil"/> value of <b>true</b> and the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.SourceRegion"/> is not empty, or the <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationX"/>, 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationY"/> or <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (3D textures only) parameters are not 0.</para>
    /// <para>Thrown if the <paramref name="source"/>, or <paramref name="destination"/> parameters have a <see cref="GorgonTextureCommon.MultisampleInfo"/> not equal to <see cref="GorgonMultisampleInfo.NoMultisampling"/> and the 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.SourceRegion"/> is not empty, or the <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationX"/>, 
    /// <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationY"/> or <paramref name="parameters"/>.<see cref="GorgonCopyTextureSubResource.DestinationZOrArrayIndex"/> (3D textures only) parameters are not 0.</para>
    /// <para>Thrown if the <paramref name="source"/> texture <see cref="GorgonTextureCommon.Type"/> is unknown.</para>
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
    ///     <description>If either of the textures use multisampling, the <see cref="GorgonCopyTextureSubResource.SourceRegion"/> must be empty and 
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
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="ArgumentException"><para><inheritdoc cref="CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception[@cref='T:System.ArgumentException']/node()"/></para></exception>
    /// <exception cref="GorgonException"><para><inheritdoc cref="CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/node()"/></para>
    /// <para>Thrown if the source texture and destination texture do not meet the restrictions for this method.</para>
    /// <inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/>
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
    ///     <description>Have the same <see cref="GorgonTextureCommon.Type"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTextureCommon.Width"/>, <see cref="GorgonTextureCommon.Height"/>, <see cref="GorgonTextureCommon.Depth"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTextureCommon.MipCount"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTextureCommon.ArrayCount"/> if the textures are <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/>.</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same texture <see cref="GorgonTextureCommon.Format"/> that is within the same texture group (see the <see cref="GorgonFormatInfo.Group"/> property of the 
    ///     <see cref="GorgonFormatInfo"/> on the <see cref="GorgonTextureCommon.FormatInfo"/> property on the texture).</description>
    ///     </item>
    ///     <item>
    ///     <description>Have the same <see cref="GorgonTextureCommon.MultisampleInfo"/> values.</description>
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

    /// <summary>
    /// Function to copy a <see cref="GorgonTexture"/> sub resource to a <see cref="GorgonVirtualTexture"/> handle.
    /// </summary>
    /// <param name="source">The texture to copy data from.</param>
    /// <param name="destination">The virtual texture that will receive the data.</param>
    /// <param name="parameters">The parameters that define where to copy the information on the destination, and where on the source to retrieve information from.</param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para>Thrown if the <paramref name="destination"/> handle is <see cref="GorgonVirtualTextureHandle.Null"/>, or was not allocated from the <paramref name="destination"/> texture.</para>
    /// <para>Thrown if the <paramref name="source"/> format is not in the same <see cref="GorgonFormatInfo.Group"/> as the <paramref name="destination"/> format.</para>
    /// <para>Thrown if the <paramref name="source"/> is a multi-sampled texture, or is a depth/stencil texture.</para>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method copies a texture sub resource into a virtual texture handle region allocated on the <paramref name="destination"/>. Copies made with this method will clip against the boundaries on the 
    /// handle allocation and <paramref name="source"/> texture sub resource. 
    /// </para>
    /// <para type="destinfo">
    /// <h3>Destination Considerations</h3>
    /// <para>
    /// When copying to a virtual texture, the <see cref="GorgonCopyTextureToVirtual"/>.<see cref="GorgonCopyTextureToVirtual.DestinationHandle"/> must have been allocated from the 
    /// <paramref name="destination"/> virtual texture. Furthermore, the texture allocation handle encompasses a specific sub resource (i.e. mip level, and/or 1D/2D texture array index), and a physical 
    /// region on the virtual texture. These values can be retrieved from the virtual texture by calling <see cref="GorgonVirtualTexture.TryGetAllocatedTileRegion(GorgonVirtualTextureHandle, out GorgonBox)"/>, 
    /// and <see cref="GorgonVirtualTexture.TryGetSubResources(GorgonVirtualTextureHandle, out short, out short)"/>.
    /// </para>
    /// <para type="vtcommon">
    /// <note type="information">
    /// <para>
    /// The tile region returned from <see cref="GorgonVirtualTexture.TryGetAllocatedTileRegion(GorgonVirtualTextureHandle, out GorgonBox)"/> is in tiles, which is the unit used when allocating data on a 
    /// virtual texture. Applications can convert tiles to Texture coordinates <c>0.0f..1.0f</c> by calling <see cref="GorgonVirtualTexture.FromTiles(ref readonly GorgonBox, out GorgonBoxF, short)"/> and to 
    /// convert to pixels call <see cref="GorgonTextureCommon.ToPixelBox(ref readonly GorgonBoxF, out GorgonBox, short)"/> (or one of the overloads).
    /// </para>
    /// <para>
    /// When to pixels, you will notice that the region coordinates do not exactly match your allocation region, this is because the allocation is done in tiles, and the tile size may require that the 
    /// coorindates be larger than what was passed in. 
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// While the allocation may have been made with absolute coordinates on the destination texture (e.g. starting at 2048x2048), the copy method expects the 
    /// <see cref="GorgonCopyTextureToVirtual.DestinationX"/>, <see cref="GorgonCopyTextureToVirtual.DestinationX"/> or <see cref="GorgonCopyTextureToVirtual.DestinationZ"/> values on the 
    /// <paramref name="parameters"/> to start from 0. In the example given, our allocation coordinates were given starting at <c>2048x2048</c>, this means that when we pass a value of <c>DestinationX = 0</c> 
    /// and <c>DestinationY = 0</c> (Z is ignored here), then the data will be written to <c>2048x2048</c> on the <paramref name="destination"/>, this makes it easier to treat an allocation as a separate set 
    /// of image data.
    /// </para>
    /// <img src="/images/VirtualTexture.png"/>
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonCopyTextureToVirtual"/>
    /// <seealso cref="GorgonVirtualTextureHandle"/>
    /// <seealso cref="GorgonFormatInfo"/>
    T CopyTextureToVirtual(GorgonTexture source, GorgonVirtualTexture destination, ref readonly GorgonCopyTextureToVirtual parameters);

    /// <summary>
    /// Function to copy a <see cref="GorgonVirtualTexture"/> handle to a <see cref="GorgonTexture"/> sub resource.
    /// </summary>
    /// <param name="source">The virutal texture to copy data from.</param>
    /// <param name="destination">The texture that will receive the data.</param>
    /// <param name="parameters"><inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/param[@name='parameters']"/></param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <para>Thrown if the <paramref name="source"/> handle is <see cref="GorgonVirtualTextureHandle.Null"/>, or was not allocated from the <paramref name="source"/> texture.</para>
    /// <inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[2]"/>
    /// <para>Thrown if the <paramref name="destination"/> is a multi-sampled texture, or is a depth/stencil texture.</para>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method copies the contents of a virtual texture handle region allocated on the <paramref name="source"/> to a sub resource on the <paramref name="destination"/>. Copies made with this method 
    /// will clip against the boundaries on the handle allocation and <paramref name="destination"/> texture sub resource. 
    /// </para>
    /// <para type="srcinfo">
    /// <para>
    /// <h3>Source Considerations</h3>
    /// <para>
    /// When copying from a virtual texture, the <see cref="GorgonCopyVirtualToTexture"/>.<see cref="GorgonCopyVirtualToTexture.SourceHandle"/> must have been allocated from the 
    /// <paramref name="source"/> virtual texture. Furthermore, the texture allocation handle encompasses a specific sub resource (i.e. mip level, and/or 1D/2D texture array index), and a physical 
    /// region on the virtual texture. These values can be retrieved from the virtual texture by calling <see cref="GorgonVirtualTexture.TryGetAllocatedTileRegion(GorgonVirtualTextureHandle, out GorgonBox)"/>, 
    /// and <see cref="GorgonVirtualTexture.TryGetSubResources(GorgonVirtualTextureHandle, out short, out short)"/>.
    /// </para>
    /// <inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/remarks/para/para/para[@type='vtcommon']"/>
    /// <para>
    /// While the allocation may have been made with absolute coordinates on the texture (e.g. starting at 2048x2048), the copy method expects the <see cref="GorgonCopyVirtualToTexture.SourceRegion"/> values 
    /// on the <paramref name="parameters"/> to start from 0. In the example given, our allocation coordinates were given starting at <c>2048x2048</c>, this means that when we pass a value of <c>X = 0</c> 
    /// and <c>Y = 0</c> (Z is ignored here), then the data will be read from <c>2048x2048</c> on the <paramref name="source"/>, this makes it easier to treat an allocation as a separate set of image data.
    /// </para>
    /// </para>
    /// <img src="/images/VirtualTexture.png"/>    
    /// </para>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonCopyVirtualToTexture"/>
    /// <seealso cref="GorgonVirtualTextureHandle"/>
    /// <seealso cref="GorgonFormatInfo"/>
    T CopyVirtualToTexture(GorgonVirtualTexture source, GorgonTexture destination, ref readonly GorgonCopyVirtualToTexture parameters);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonVirtualTexture"/> handle to another <see cref="GorgonVirtualTexture"/> handle.
    /// </summary>
    /// <param name="source"><inheritdoc cref="CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/param[@name='source']"/></param>
    /// <param name="destination"><inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/param[@name='destination']"/></param>
    /// <param name="parameters"><inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/param[@name='parameters']"/></param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[1]"/>
    /// <inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[1]"/>
    /// <para>Thrown if the <paramref name="source"/> and <paramref name="destination"/> have the same handle.</para>
    /// <inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/exception[@cref='T:Gorgon.Core.GorgonException']/para[2]"/>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the contents of an allocated virtual texture handle region to another. Copies made with this method will clip against the regions defined in the <paramref name="source"/> and 
    /// <paramref name="destination"/> handles. This method also allows the <paramref name="source"/> and <paramref name="destination"/> to be the same texture, as long as the handles are different, allowing 
    /// applications to copy between different sub resources on the same virtual texture.
    /// </para>
    /// <inheritdoc cref="CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)" path="/remarks/para/para[@type='srcinfo']/para"/>
    /// <inheritdoc cref="CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)" path="/remarks/para/para[@type='destinfo']"/>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonCopyVirtualToVirtual"/>
    /// <seealso cref="GorgonVirtualTextureHandle"/>
    /// <seealso cref="GorgonFormatInfo"/>
    T CopyVirtualToVirtual(GorgonVirtualTexture source, GorgonVirtualTexture destination, ref readonly GorgonCopyVirtualToVirtual parameters);

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonGpuBuffer"/> to a <see cref="GorgonVirtualTexture"/> handle.
    /// </summary>
    /// <param name="buffer"><inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/param[@name='buffer']"/></param>
    /// <param name="texture"><inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/param[@name='texture']"/></param>
    /// <param name="destinationHandle">The handle allocated from the texture.</param>
    /// <param name="sourceOffset">The offset, in bytes, within the buffer to start reading from.</param>
    /// <inheritdoc cref="CopyValue{Tv}(in Tv, GorgonGpuBufferCommon, long)" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="sourceOffset"/> is less than 0.</para></exception>
    /// <exception cref="ArgumentException"><para>Thrown if the size, in bytes, of <paramref name="buffer"/> minus the <paramref name="sourceOffset"/> is larger than the texture sub resource size.</para></exception>
    /// <exception cref="GorgonException">
    /// <para type="common">
    /// <para>Thrown if the <paramref name="destinationHandle"/> is <see cref="GorgonVirtualTextureHandle.Null"/>, or the handle is not from the <paramref name="texture"/>.</para>
    /// <inheritdoc cref="GorgonCommandList.SetBarrier(GorgonTextureCommon, BarrierSync, BarrierAccess, BarrierLayout, GorgonSubResourceRange?, bool, bool)" path="/exception/para[@type='barrier_issue']"/>
    /// </para>
    /// </exception>
    /// <remarks>
    /// <para type="common">
    /// <para>
    /// This method will copy the data within the specified <paramref name="buffer"/> into the specified sub resource allocated into the <paramref name="destinationHandle"/> on the 
    /// <paramref name="texture"/>. This allows applications to load or stream arbitrary data into an allocated region on a virtual texture.
    /// </para>
    /// <para>
    /// For the copy to succeed, the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the <paramref name="buffer"/> minus the <paramref name="sourceOffset"/> parameter, must be at least the same size, or 
    /// less than the size, in bytes, as the texture sub resource. To determine the size of a sub resource use the <see cref="GorgonTextureCommon.SubResources"/> list on the <paramref name="texture"/> to 
    /// retrieve a <see cref="GorgonSubResourceInfo"/> data structure and use the <see cref="GorgonSubResourceInfo.SizeInBytes"/> property, which can be used to determine the size of the 
    /// <paramref name="buffer"/>. Because this information is associated with the <paramref name="destinationHandle"/>, applications must first call the 
    /// <see cref="GorgonVirtualTexture.TryGetSubResources(GorgonVirtualTextureHandle, out short, out short)"/> method to retrieve the mip level, and if applicable, the array index of the sub resource.
    /// </para>
    /// <inheritdoc cref="CopyBufferToTexture(GorgonGpuBuffer, GorgonTexture, GorgonCopyBufferToTexture)" path="/remarks/para/para[@type='alignment']"/>
    /// </para>
    /// <inheritdoc cref="CopyBuffer(GorgonGpuBufferCommon, GorgonGpuBufferCommon, long, long, long?)" path="/remarks/para[@type='endrequired']"/>
    /// </remarks>
    /// <seealso cref="GorgonSubResourceInfo"/>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="TextureType"/>
    /// <seealso cref="BufferFormat"/>
    T CopyBufferToVirtual(GorgonGpuBuffer buffer, GorgonVirtualTexture texture, GorgonVirtualTextureHandle destinationHandle, long sourceOffset = 0);
}