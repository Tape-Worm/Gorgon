
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 2:26:21 PM
// 

using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// Base class for a Gorgon chunked formatted data readers/writers
/// </summary>
/// <typeparam name="T">The type of read/writer to use when deserializing or serializing the data.</typeparam>
/// <remarks>
/// <para>
/// This allows access to a file format that uses the concept of grouping sections of an object together into a grouping called a chunk. This chunk will hold binary data associated with an object allows 
/// the developer to read/write only the pieces of the object that are absolutely necessary while skipping optional chunks
/// </para>
/// <para>
/// A more detailed explanation of the chunk file format can be found in the <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink> topic
/// </para>
/// <para>
/// A chunk file object will expose a collection of <see cref="GorgonChunk"/> values, and these give the available chunks in the file and can be looked up either by the <see cref="ulong"/> value for 
/// the chunk ID, or an 8 character <see cref="string"/> that represents the chunk (this is recommended for readability). This allows an application to do validation on the chunk file to ensure that 
/// its format is correct. It also allows an application to discard chunks it doesn't care about or are optional. This allows for some level of versioning between chunk file formats
/// </para>
/// <para>
/// Chunks can be accessed in any order, not just the order in which they were written. This allows an application to only take the pieces they require from the file, and leave the rest. It also allows 
/// for optional chunks that can be skipped if not present, and read/written when they are
/// </para>
/// <note type="tip">
/// <para>
/// Gorgon uses the chunked file format for its own file serializing/deserializing of its objects that support persistence. 
/// </para>
/// </note>
/// </remarks>
/// <seealso cref="GorgonChunkFileReader"/>
/// <seealso cref="GorgonChunkFileWriter"/>
/// <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF) details</conceptualLink>
public abstract class GorgonChunkFile<T>
{
    /// <summary>
    /// The header ID for the 1.0 version of the chunk file format. (GCFF0100)
    /// </summary>
    public const ulong FileFormatHeaderIDv0100 = 0x3030313046464347;
    /// <summary>
    /// The chunk table chunk ID (CHUNKTBL)
    /// </summary>
    public const ulong ChunkTableID = 0x4C42544B4E554843;

    /// <summary>
    /// Property to return an editable list of chunks.
    /// </summary>
    internal GorgonChunkCollection ChunkList
    {
        get;
    }

    /// <summary>
    /// Property to return the <see cref="GorgonStreamWrapper"/> that contains the chunked file.
    /// </summary>
    public GorgonStreamWrapper Stream
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not the file is open.
    /// </summary>
    public bool IsOpen
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the list of chunks available in the file.
    /// </summary>
    /// <remarks>
    /// Use this property to determine if a chunk exists when reading a chunk file.
    /// </remarks>
    public IGorgonReadOnlyChunkCollection Chunks => ChunkList;

    /// <summary>
    /// Function to perform validation against the requested chunk ID and the list of reserved values.
    /// </summary>
    /// <param name="chunkId">Chunk ID to evaluate.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="chunkId"/> is the same as one of the reserved chunk IDs.</exception>
    protected void ValidateChunkID(ulong chunkId)
    {
        switch (chunkId)
        {
            case ChunkTableID:
            case FileFormatHeaderIDv0100:
                throw new ArgumentException(string.Format(Resources.GOR_ERR_CHUNK_RESERVED, chunkId.FormatHex()), nameof(chunkId));
        }
    }

    /// <summary>
    /// Function called when a chunk file is closing.
    /// </summary>
    /// <returns>The total number of bytes read or written.</returns>
    protected abstract long OnClose();

    /// <summary>
    /// Function called when a chunk file is opening.
    /// </summary>
    protected abstract void OnOpen();

    /// <summary>
    /// Function to open a chunked file within the stream.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This opens a Gorgon chunk file that exists within the <see cref="Stream"/> passed to the constructor of this object. Typically this would be in a <see cref="FileStream"/>, but any type of stream 
    /// is valid and can contain a chunk file. 
    /// </para>
    /// <para>
    /// If the this method is called and the file is already opened, then it will be closed and reopened.
    /// </para>
    /// <note type="Important">
    /// Always pair a call to <c>Open</c> with a call to <see cref="Close"/>, otherwise the file may become corrupted or the stream may not be updated to the correct position.
    /// </note>
    /// <note type="Important">
    /// When this file is opened for <i>reading</i>, then validation is performed on the file header to ensure that it is a genuine GCFF file. If it is not, then an exception will be thrown 
    /// detailing what's wrong with the header.
    /// </note>
    /// </remarks>
    /// <exception cref="GorgonException">Thrown when the chunk file format header ID does not match when reading.
    /// <para>-or-</para>
    /// <para>Thrown when application specific header ID in the file was not found in the list passed to the constructor when reading.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the chunk file table offset is less than or equal to the size of the header when reading.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the file size recorded in the header is less than the size of the header when reading.</para>
    /// </exception>
    public void Open()
    {
        if (IsOpen)
        {
            Close();
        }

        // Reset the stream position back to the beginning of the file.
        // This is a stream wrapper, so it'll be relative to the actual stream position.
        Stream.Position = 0;

        ChunkList.Clear();

        OnOpen();

        IsOpen = true;
    }

    /// <summary>
    /// Function to close an open chunk.
    /// </summary>
    /// <remarks>
    /// This method should always be called when one of the <see cref="GorgonChunkFile{T}.OpenChunk(ulong)"/> methods are called. Failure to do so may cause file corruption.
    /// </remarks>
    public abstract void CloseChunk();

    /// <summary>
    /// Function to open a chunk for reading or writing.
    /// </summary>
    /// <param name="chunkId">The ID of the chunk to open.</param>
    /// <returns>A <see cref="GorgonBinaryReader"/>, or <see cref="GorgonBinaryWriter"/> that will allow reading or writing within the chunk.</returns>
    /// <remarks>
    /// See the <see cref="GorgonChunkFileReader.OpenChunk(ulong)"/>, or the <see cref="GorgonChunkFileWriter.OpenChunk(ulong)"/> methods for detailed information on what this method does during reading/writing 
    /// contexts.
    /// </remarks>
    /// <seealso cref="GorgonChunkFileReader.OpenChunk(ulong)"/>
    /// <seealso cref="GorgonChunkFileWriter.OpenChunk(ulong)"/>
    public abstract T OpenChunk(ulong chunkId);

    /// <summary>
    /// Function to close an open chunk file in the stream.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Always call this method when a call to <see cref="Open"/> is made. Failure to do so could cause the file to become corrupted.
    /// </para>
    /// <para>
    /// If the file is not open, then this method will do nothing.
    /// </para>
    /// </remarks>
    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        // Advance the parent stream position to the end of the file.
        Stream.ParentStream.Position += OnClose();
    }

    /// <summary>
    /// Function to open a chunk, by the text representation of its ID, for reading or writing.
    /// </summary>
    /// <param name="chunkName">The name of the chunk.</param>
    /// <returns>A <see cref="GorgonBinaryReader"/>, or <see cref="GorgonBinaryWriter"/> that will allow reading or writing within the chunk.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="chunkName"/> parameter is empty.</exception>
    /// <remarks>
    /// See the <see cref="GorgonChunkFileReader.OpenChunk(ulong)"/>, or the <see cref="GorgonChunkFileWriter.OpenChunk(ulong)"/> method for more information.
    /// </remarks>
    /// <seealso cref="GorgonChunkFileReader.OpenChunk(ulong)"/>
    /// <seealso cref="GorgonChunkFileWriter.OpenChunk(ulong)"/>
    public T OpenChunk(string chunkName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(chunkName);

        return OpenChunk(chunkName.ChunkID());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonChunkFile{T}"/> class.
    /// </summary>
    /// <param name="stream">The stream that contains the chunk file to read or write.</param>
    /// <remarks>
    /// The <paramref name="stream"/> passed to this method requires that the <see cref="Stream.CanSeek"/> property returns a value of <b>true</b>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is has its <see cref="Stream.CanSeek"/> property set to <b>false</b>.</exception>
    protected GorgonChunkFile(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_NOT_SEEKABLE, nameof(stream));
        }

        ChunkList = [];
        Stream = new GorgonStreamWrapper(stream);
    }
}
