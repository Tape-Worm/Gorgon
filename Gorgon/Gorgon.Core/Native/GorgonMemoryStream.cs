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
// Created: June 1, 2025 6:42:47 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Native;

/// <summary>
/// A stream that wraps a <see cref="GorgonNativeBuffer{T}"/> or <see cref="GorgonPtr{T}"/> type.
/// </summary>
/// <typeparam name="T">The type of data in the stream.</typeparam>
public class GorgonMemoryStream<T>
    : Stream
    where T : unmanaged
{
    // The pointer wrapped by the stream.
    private readonly GorgonPtr<T> _pointer;
    // The current position.
    private long _position;

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => true;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override long Length => _pointer.SizeInBytes;

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    public override void Flush()
    {
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        GorgonPtr<byte> srcPtr = (GorgonPtr<byte>)_pointer;

        count = count.Min((int)(srcPtr.SizeInBytes - _position));

        Unsafe.CopyBlock(ref buffer[offset], ref (srcPtr + _position).Value, (uint)count);

        _position += count;

        return count;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset.Max(0).Min(_pointer.SizeInBytes);
                break;
            case SeekOrigin.End:
                _position = (Length + offset).Max(0).Min(_pointer.SizeInBytes);
                break;
            case SeekOrigin.Current:
                _position = (_position + offset).Max(0).Min(_pointer.SizeInBytes);
                break;
        }

        return _position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        GorgonPtr<byte> destPtr = (GorgonPtr<byte>)_pointer;

        if (count > (int)(destPtr.SizeInBytes - _position))
        {
            throw new OverflowException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        Unsafe.CopyBlock(ref (destPtr + _position).Value, ref buffer[offset], (uint)count);

        _position += count;
    }

    /// <summary>
    /// Initializes a new instance of a <see cref="GorgonMemoryStream{T}"/> class.
    /// </summary>
    /// <param name="buffer">The native buffer to wrap the stream in.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if there is no data covered by the buffer.</exception>
    public GorgonMemoryStream(GorgonNativeBuffer<T> buffer)
        : this((GorgonPtr<T>)buffer)
    {
    }

    /// <summary>
    /// Initializes a new instance of a <see cref="GorgonMemoryStream{T}"/> class.
    /// </summary>
    /// <param name="ptr">The pointer to wrap the stream in.</param>
    /// <exception cref="NullReferenceException">Thrown if the <paramref name="ptr"/> is equal to <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown if there is no data covered by the pointer.</exception>
    public GorgonMemoryStream(GorgonPtr<T> ptr)
    {
        if (ptr == GorgonPtr<T>.NullPtr)
        {
            throw new NullReferenceException();
        }

        ArgumentEmptyException.ThrowIfEmpty(ptr);

        _pointer = ptr;
    }
}
