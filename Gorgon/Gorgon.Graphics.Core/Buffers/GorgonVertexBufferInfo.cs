
// 
// Gorgon
// Copyright (C) 2016 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 25, 2016 12:40:16 AM
// 

using System.Reflection;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Reflection;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides the necessary information required to set up a vertex buffer
/// </summary>
/// <param name="SizeInBytes">The size of the buffer, in bytes.</param>
/// <remarks>
/// <para>
/// When creating the vertex buffer, the <paramref cref="SizeInBytes"/> parameter represents the number of bytes for a single vertex multiplied by the number of vertices
/// </para>
/// </remarks>
public record GorgonVertexBufferInfo(int SizeInBytes)
    : IGorgonVertexBufferInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVertexBufferInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="IGorgonVertexBufferInfo"/> to copy settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonVertexBufferInfo(IGorgonVertexBufferInfo info)
        : this(info?.SizeInBytes ?? throw new ArgumentNullException(nameof(info)))
    {
        Name = info.Name;
        Usage = info.Usage;
        Binding = info.Binding;
    }

    /// <summary>
    /// Property to return the binding used to bind this buffer to the GPU.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="VertexIndexBufferBinding.None"/>.
    /// </remarks>
    public VertexIndexBufferBinding Binding
    {
        get;
        init;
    } = VertexIndexBufferBinding.None;

    /// <summary>
    /// Property to return the intended usage for binding to the GPU.
    /// </summary>
    public ResourceUsage Usage
    {
        get;
        init;
    } = ResourceUsage.Default;

    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = GorgonGraphicsResource.GenerateName(GorgonVertexBuffer.NamePrefix);

    /// <summary>
    /// Function to create a <see cref="GorgonVertexBufferInfo"/> based on the type representing a vertex.
    /// </summary>
    /// <typeparam name="T">The type of data representing a vertex. This must be an unmanaged value type.</typeparam>
    /// <param name="count">The number of vertices to store in the buffer.</param>
    /// <param name="usage">[Optional] The usage parameter for the vertex buffer.</param>
    /// <returns>A new <see cref="GorgonVertexBufferInfo"/> to use when creating a <see cref="GorgonVertexBuffer"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is less than 1.</exception>
    /// <exception cref="GorgonException">Thrown when the type specified by <typeparamref name="T"/> is not safe for use with native functions (see <see cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>).
    /// <para>-or-</para>
    /// <para>Thrown when the type specified by <typeparamref name="T"/> does not contain any public members.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is offered as a convenience to simplify the creation of the required info for a <see cref="GorgonVertexBuffer"/>. It will automatically determine the size of a vertex based on the size 
    /// of a type specified by <typeparamref name="T"/> and fill in the <see cref="SizeInBytes"/> with the correct size.
    /// </para>
    /// <para>
    /// This method requires that the type passed by <typeparamref name="T"/> have its members decorated with the <see cref="InputElementAttribute"/>. This is used to determine which members of the 
    /// type are to be used in determining the size of the type.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>
    /// <seealso cref="GorgonReflectionExtensions.IsSafeForNative(Type)"/>
    /// <seealso cref="GorgonReflectionExtensions.IsSafeForNative(Type,out IReadOnlyList{FieldInfo})"/>
    public static GorgonVertexBufferInfo CreateFromType<T>(int count, ResourceUsage usage = ResourceUsage.Default)
        where T : unmanaged
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        List<(FieldInfo, InputElementAttribute)> fields = GorgonInputLayout.GetFieldInfoList(typeof(T));

        return fields.Count == 0
            ? throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VERTEX_NO_FIELDS, typeof(T).FullName))
            : new GorgonVertexBufferInfo(count * Unsafe.SizeOf<T>())
            {
                Usage = usage
            };
    }

    /// <summary>
    /// Function to create a <see cref="GorgonVertexBufferInfo"/> based on a <see cref="GorgonInputLayout"/> and the intended slot for vertex data.
    /// </summary>
    /// <param name="layout">The <see cref="GorgonInputLayout"/> to evaluate.</param>
    /// <param name="count">The number of vertices to store in the buffer.</param>
    /// <param name="slot">The intended slot to use for the vertex data.</param>
    /// <param name="usage">[Optional] The usage parameter for the vertex buffer.</param>
    /// <returns>A new <see cref="GorgonVertexBufferInfo"/> to use when creating a <see cref="GorgonVertexBuffer"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is less than 1.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="slot"/> is not present in the <paramref name="layout"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is offered as a convenience to simplify the creation of the required info for a <see cref="GorgonVertexBuffer"/>. It will automatically determine the size of a vertex based on the size 
    /// of the specified <paramref name="slot"/> in the <paramref name="layout"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInputLayout"/>
    public static GorgonVertexBufferInfo CreateFromInputLayout(GorgonInputLayout layout, int slot, int count, ResourceUsage usage = ResourceUsage.Default)
    {
        if (layout is null)
        {
            throw new ArgumentNullException(nameof(layout));
        }

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        int sizeInBytes = layout.GetSlotSize(slot);

        return sizeInBytes < 1
            ? throw new ArgumentOutOfRangeException(nameof(slot))
            : new GorgonVertexBufferInfo(sizeInBytes * count)
            {
                Usage = usage
            };
    }

}