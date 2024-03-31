// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: March 28, 2024 4:00:28 PM
//

using Gorgon.Native;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// Internal implementation for a <see cref="IGorgonChunkReader"/>.
/// </summary>
/// <param name="chunk">The chunk that is currently being read.</param>
/// <param name="parentStream">The parent stream containing the chunk file.</param>
/// <param name="onClosed">Method called when the chunk is closed.</param>
internal class ChunkReader(GorgonChunk chunk, Stream parentStream, Action onClosed)
    : IGorgonChunkReader
{
    // The binary reader used to read data from the chunk.
    private readonly BinaryReader _reader = new(new GorgonSubStream(parentStream, 0, chunk.Size, allowWrite: false), Encoding.UTF8);
    // The current chunk.
    private GorgonChunk _chunk = chunk;
    // The method to call when the chunk is closed.
    private Action? _closed = onClosed;
    // Flag to indicate that the serialization method is being used.
    private bool _inDeserialize;
    // Flag to indicate that the reader is open.
    private bool _isOpen = true;

    /// <inheritdoc/>
    public ref readonly GorgonChunk ChunkInfo => ref _chunk;

    /// <summary>
    /// Function to determine if we're in a deserialization process.
    /// </summary>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize{T}"/> method is executing.</exception>
    private void CheckIfDeserializing()
    {
        if (!_inDeserialize)
        {
            return;
        }

        throw new IOException(Resources.GOR_ERR_CHUNK_WRITER_SERIALIZING);
    }

    /// <inhertidoc/>
    public void ReadArray<T>(T[] data, int index = 0, int? count = null)
        where T : unmanaged
    {
        CheckIfDeserializing();

        _reader.ReadRange(new Span<T>(data, index, count ?? data.Length));
    }

    /// <inheritdoc/>
    public bool ReadBool()
    {
        CheckIfDeserializing();

        return _reader.ReadBoolean();
    }

    /// <inheritdoc/>
    public byte ReadByte()
    {
        CheckIfDeserializing();

        return _reader.ReadByte();
    }

    /// <inheritdoc/>
    public char ReadChar()
    {
        CheckIfDeserializing();

        return _reader.ReadChar();
    }

    /// <inheritdoc/>
    public decimal ReadDecimal()
    {
        CheckIfDeserializing();

        return _reader.ReadDecimal();
    }

    /// <inheritdoc/>
    public double ReadDouble()
    {
        CheckIfDeserializing();

        return _reader.ReadDouble();
    }

    /// <inheritdoc/>
    public Half ReadHalf()
    {
        CheckIfDeserializing();

        return _reader.ReadHalf();
    }

    /// <inheritdoc/>
    public short ReadInt16()
    {
        CheckIfDeserializing();

        return _reader.ReadInt16();
    }

    /// <inheritdoc/>
    public int ReadInt32()
    {
        CheckIfDeserializing();

        return _reader.ReadInt32();
    }

    /// <inheritdoc/>
    public long ReadInt64()
    {
        CheckIfDeserializing();

        return _reader.ReadInt64();
    }

    /// <inheritdoc/>
    public void ReadPointer<T>(GorgonPtr<T> pointer) where T : unmanaged
    {
        CheckIfDeserializing();

        _reader.ReadToPointer(pointer);
    }

    /// <inheritdoc/>
    public float ReadSingle()
    {
        CheckIfDeserializing();

        return _reader.ReadSingle();
    }

    /// <inheritdoc/>
    public void ReadSpan<T>(Span<T> data) where T : unmanaged
    {
        CheckIfDeserializing();

        _reader.ReadRange(data);
    }

    /// <inheritdoc/>
    public string ReadString(Encoding? encoding = null)
    {
        CheckIfDeserializing();

        return _reader.ReadString();
    }

    /// <inheritdoc/>
    public ushort ReadUInt16()
    {
        CheckIfDeserializing();

        return _reader.ReadUInt16();
    }

    /// <inheritdoc/>
    public uint ReadUInt32()
    {
        CheckIfDeserializing();

        return _reader.ReadUInt32();
    }

    /// <inheritdoc/>
    public ulong ReadUInt64()
    {
        CheckIfDeserializing();

        return _reader.ReadUInt64();
    }

    /// <inheritdoc/>
    public void ReadValue<T>(out T value) where T : unmanaged
    {
        CheckIfDeserializing();

        value = _reader.ReadValue<T>();
    }

    /// <inheritdoc/>
    public T ReadValue<T>() where T : unmanaged
    {
        CheckIfDeserializing();

        return _reader.ReadValue<T>();
    }

    /// <inheritdoc/>
    public int Skip(int bytes)
    {
        CheckIfDeserializing();

        if (bytes + _reader.BaseStream.Position > _reader.BaseStream.Length)
        {
            bytes = (int)(_reader.BaseStream.Length - _reader.BaseStream.Position);
        }

        if (bytes <= 0)
        {
            return 0;
        }

        _reader.BaseStream.Position += bytes;

        return bytes;
    }

    /// <inheritdoc/>
    public T Deserialize<T>(Func<Stream, T> deserializeCallback)
    {
        CheckIfDeserializing();

        GorgonSubStream? subStream = null;
        _inDeserialize = true;

        try
        {
            subStream = new(_reader.BaseStream, allowWrite: false);

            // If we have a failure in our callback, then we can overwrite whatever was written before.
            T value = deserializeCallback(subStream);

            // Move our position ahead.
            _reader.BaseStream.Position += subStream.Length;

            return value;
        }
        finally
        {
            // This isn't strictly necessary, but it's good to be consistent.
            subStream?.Dispose();
            _inDeserialize = false;
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (!_isOpen)
        {
            return;
        }

        try
        {
            CheckIfDeserializing();

            // Shut down the reader.
            _reader.Dispose();

            // Notify the owner of this reader that we're done.
            _closed?.Invoke();
        }
        finally
        {
            _closed = null;
            _chunk = default;
            _isOpen = false;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Close();
}
