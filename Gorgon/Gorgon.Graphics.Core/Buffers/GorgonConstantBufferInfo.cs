﻿
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
// Created: June 15, 2016 9:39:42 PM
// 

using System.Runtime.CompilerServices;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides information on how to set up a constant buffer
/// </summary>
/// <param name="SizeInBytes">The size of the constant buffer, in bytes.</param>
public record GorgonConstantBufferInfo(int SizeInBytes)
    : IGorgonConstantBufferInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonConstantBufferInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="IGorgonConstantBufferInfo"/> to copy settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonConstantBufferInfo(IGorgonConstantBufferInfo info)
        : this(info?.SizeInBytes ?? throw new ArgumentNullException(nameof(info)))
    {
        Name = info.Name;
        Usage = info.Usage;
    }

    /// <summary>
    /// Property to return the intended usage flags for this buffer.
    /// </summary>
    /// <remarks>
    /// This value is defaulted to <see cref="ResourceUsage.Default"/>.
    /// </remarks>
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
    } = GorgonGraphicsResource.GenerateName(GorgonConstantBuffer.NamePrefix);

    /// <summary>
    /// Function to create a <see cref="IGorgonConstantBufferInfo"/> based on the type representing a vertex.
    /// </summary>
    /// <typeparam name="T">The type of data representing a constant. This must be an unmanaged value type.</typeparam>
    /// <param name="name">[Optional] The name of this constant buffer.</param>
    /// <param name="count">[Optional] The number of items to store in the buffer.</param>
    /// <param name="usage">[Optional] The usage parameter for the vertex buffer.</param>
    /// <returns>A new <see cref="IGorgonConstantBufferInfo"/> to use when creating a <see cref="GorgonConstantBuffer"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// This method is offered as a convenience to simplify the creation of the required info for a <see cref="GorgonConstantBuffer"/>. It will automatically determine the size of the constant based on the 
    /// size of a type specified by <typeparamref name="T"/> and fill in the <see cref="SizeInBytes"/> with the correct size.
    /// </para>
    /// <para>
    /// A constant buffer must have its size rounded to the nearest multiple of 16. If the type specified by <typeparamref name="T"/> does not have a size that is a multiple of 16, then the 
    /// <see cref="SizeInBytes"/> returned will be rounded up to the nearest multiple of 16.
    /// </para>
    /// <para>
    /// This method requires that the type passed by <typeparamref name="T"/> have its members decorated with the <see cref="InputElementAttribute"/>. This is used to determine which members of the 
    /// type are to be used in determining the size of the type.
    /// </para>
    /// <para>
    /// If the <paramref name="name"/> parameter is <b>null</b> or empty, then the fully qualified name of the type specified by <typeparamref name="T"/> is used.
    /// </para>
    /// </remarks>
    public static IGorgonConstantBufferInfo CreateFromType<T>(string name = null, int count = 1, ResourceUsage usage = ResourceUsage.Default)
        where T : unmanaged
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        return new GorgonConstantBufferInfo(((count * Unsafe.SizeOf<T>()) + 15) & ~15)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GorgonGraphicsResource.GenerateName(GorgonConstantBuffer.NamePrefix) : name,
            Usage = usage
        };
    }
}