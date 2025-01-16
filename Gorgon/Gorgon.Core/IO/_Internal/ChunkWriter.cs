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
// Created: February 9, 2024 2:35:14 PM
//

using Gorgon.Native;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// Internal implementation for a <see cref="IGorgonChunkWriter"/>.
/// </summary>
/// <param name="chunkID">The ID of the chunk being written.</param>
/// <param name="parentStream">The parent stream containing the chunk file.</param>
/// <param name="offset">The offset of the chunk (include header bytes) from the beginning of chunk data.</param>
/// <param name="closeAction">The action to execute when the writer closes.</param>
internal class ChunkWriter(ulong chunkID, Stream parentStream, ulong offset, Action<GorgonChunk> closeAction)
        : IGorgonChunkWriter
{
    // The binary writer used to send data to the chunk.
    private readonly BinaryWriter _writer = new(new GorgonSubStream(parentStream), Encoding.UTF8);
    // The current position of the chunk starting from the header of the file.
    private readonly ulong _chunkPosition = offset;
    // Flag to indicate that the serialization method is being used.
    private bool _inSerialize;
    // The action to execute when the writer closes.
    private Action<GorgonChunk>? _closeAction = closeAction;
    // Flag to indicate that the write is open.
    private bool _isOpen = true;

    /// <inheritdoc/>
    public ulong ID
    {
        get;
    } = chunkID;

    /// <summary>
    /// Function to determine if we're in a serialization process.
    /// </summary>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    private void CheckIfSerializing()
    {
        if (!_inSerialize)
        {
            return;
        }

        throw new IOException(Resources.GOR_ERR_CHUNK_WRITER_SERIALIZING);
    }

    /// <inheritdoc/>
    public void WriteBool(bool value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteByte(byte value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteInt16(short value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteInt32(int value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteInt64(long value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteUInt16(ushort value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteUInt32(uint value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteUInt64(ulong value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteHalf(Half value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteSingle(float value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteDouble(double value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteDecimal(decimal value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteChar(char value)
    {
        CheckIfSerializing();

        _writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteString(string value, Encoding? encoding = null)
    {
        CheckIfSerializing();

        _writer.BaseStream.WriteString(value, encoding);
    }

    /// <inheritdoc/>
    public void WriteSpan<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        CheckIfSerializing();

        _writer.WriteRange(data);
    }

    /// <inheritdoc/>
    public void WriteArray<T>(T[] data, int index = 0, int? count = null) where T : unmanaged
    {
        CheckIfSerializing();

        _writer.WriteRange(new ReadOnlySpan<T>(data, index, count ?? data.Length));
    }

    /// <inheritdoc/>
    public void WritePointer<T>(GorgonPtr<T> pointer) where T : unmanaged
    {
        CheckIfSerializing();

        _writer.WriteFromPointer(pointer);
    }

    /// <inheritdoc/>
    public void WriteValue<T>(ref readonly T value) where T : unmanaged
    {
        CheckIfSerializing();

        _writer.WriteValue(in value);
    }

    /// <inheritdoc/>
    public void WriteValue<T>(T value) where T : unmanaged
    {
        CheckIfSerializing();

        _writer.WriteValue(value);
    }

    /// <inheritdoc/>
    public void Serialize<T>(T objectToSerialize, Action<T, Stream> serializeCallback)
    {
        CheckIfSerializing();

        GorgonSubStream? subStream = null;
        _inSerialize = true;

        try
        {
            subStream = new(_writer.BaseStream);

            // If we have a failure in our callback, then we can overwrite whatever was written before.
            serializeCallback(objectToSerialize, subStream);

            // Move our position ahead.
            _writer.BaseStream.Position += subStream.Length;
        }
        finally
        {
            // This isn't strictly necessary, but it's good to be consistent.
            subStream?.Dispose();
            _inSerialize = false;
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
            CheckIfSerializing();

            GorgonChunk chunk = new(ID, (int)_writer.BaseStream.Length, _chunkPosition);

            // Finalize and flush the contents of the buffer.
            _writer.Flush();
            _writer.Close();

            // Notify the owner of this writer that we're done.
            _closeAction?.Invoke(chunk);
        }
        finally
        {
            _closeAction = null;
            _isOpen = false;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Close();
}
