﻿
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: July 25, 2017 8:41:37 PM
// 


using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A binding to allow the streaming of data from the GPU to an arbitrary buffer
/// </summary>
/// <remarks>
/// <para>
/// This type of binding allows the GPU to send data to one of the available buffer types (except the <see cref="GorgonConstantBuffer"/> type). Data can be generated by a shader and pushed into a buffer 
/// attached to the binding for later use
/// </para>
/// </remarks>
/// <seealso cref="GorgonBuffer"/>
/// <seealso cref="GorgonVertexBuffer"/>
/// <seealso cref="GorgonIndexBuffer"/>
public readonly struct GorgonStreamOutBinding
    : IGorgonEquatableByRef<GorgonStreamOutBinding>
{

    /// <summary>
    /// An empty binding.
    /// </summary>
    public static readonly GorgonStreamOutBinding Empty;

    /// <summary>
    /// The buffer used for the binding.
    /// </summary>
    public readonly GorgonBufferCommon Buffer;

    /// <summary>
    /// The offset within the buffer to use, in bytes.
    /// </summary>
    /// <remarks>
    /// A value of -1 will cause the buffer to append data after the last location written in a previous stream out operation.
    /// </remarks>
    public readonly int Offset;



    /// <summary>
    /// Function to determine if a buffer type is valid or not.
    /// </summary>
    /// <param name="buffer">The buffer to validate.</param>
    private static void IsValidBufferType(GorgonBufferCommon buffer)
    {
        switch (buffer)
        {
            case null:
                return;
            case GorgonConstantBuffer _:
                // Deny constant buffers entirely.
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_CONSTANT_NO_SO);
            case GorgonIndexBuffer indexBuffer when ((indexBuffer.Binding & VertexIndexBufferBinding.StreamOut) != VertexIndexBufferBinding.StreamOut):
            case GorgonVertexBuffer vertexBuffer when ((vertexBuffer.Binding & VertexIndexBufferBinding.StreamOut) != VertexIndexBufferBinding.StreamOut):
            case GorgonBuffer genericBuffer when ((genericBuffer.Binding & BufferBinding.StreamOut) == BufferBinding.StreamOut):
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TYPE_MISSING_SO, buffer.Name));
        }
    }

    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(GorgonStreamOutBinding other) => Equals(in this, in other);

    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(ref readonly GorgonStreamOutBinding other) => Equals(in this, in other);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GORGFX_TOSTR_SO_BINDING, Offset, (Buffer?.Native is null) ? "(NULL)" : Buffer.Name);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Offset, Buffer);

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// 	<b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override bool Equals(object obj) => obj is GorgonStreamOutBinding streamOut ? streamOut.Equals(this) : base.Equals(obj);

    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool Equals(ref readonly GorgonStreamOutBinding left, in GorgonStreamOutBinding right) => ((left.Buffer == right.Buffer) && (left.Offset == right.Offset));

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(in GorgonStreamOutBinding left, in GorgonStreamOutBinding right) => Equals(in left, in right);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(in GorgonStreamOutBinding left, in GorgonStreamOutBinding right) => !Equals(in left, in right);



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutBinding"/> struct.
    /// </summary>
    /// <param name="buffer">The buffer to bind.</param>
    /// <param name="offset">[Optional] The offset within the buffer to use.</param>
    /// <exception cref="GorgonException">Thrown when the <paramref name="buffer"/> is not a stream out buffer, or is a constant buffer.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="buffer"/> passed did not have a stream out binding set on creation, then an exception will be thrown.
    /// </para>
    /// </remarks>
    public GorgonStreamOutBinding(GorgonBufferCommon buffer, int offset = 0)
    {
        IsValidBufferType(buffer);

        Buffer = buffer;
        Offset = offset;
    }

}
