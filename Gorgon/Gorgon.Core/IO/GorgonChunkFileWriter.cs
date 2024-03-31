
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
// Created: Sunday, June 14, 2015 10:40:50 PM
// 

using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// A writer that will lay out and write the contents of a <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink> file
/// </summary>
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
/// <example>
/// The following is an example of how to write out the contents of an object into a chunked file format:
/// <code language="csharp">
/// <![CDATA[
///		// An application defined file header ID. Useful for identifying the contents of the file
///		const ulong FileHeader = 0xBAADBEEFBAADF00D;	
/// 
///		const string StringsChunk = "STRNGLST";
///		const string IntChunk = "INTGRLST"; 
/// 
///		string[] strings = { "Cow", "Pig", "Dog", "Cat", "Slagathor" };
///		int[] ints { 1, 2, 9, 100, 122, 129, 882, 82, 62, 42 };
/// 
///		Stream myStream = File.Open("<<Path to your file>>", FileMode.Create, FileAccess.Write, FileShare.None);
///		GorgonChunkFileWriter file = new GorgonChunkFileWriter(myStream, FileHeader);
/// 
///		try
///		{
///			// Open the file for writing within the stream
///			file.Open();
/// 
///			// Write the chunk that will contain strings
///			// Alternatively, we could pass in an ulong value for the chunk ID instead of a string
///			using (IGorgonChunkWriter writer = file.OpenChunk(StringsChunk))
///			{
///				writer.Write(strings.Length);
///				for (int = 0; i < strings.Length; ++i)
///				{
///					writer.Write(strings[i]);
///				}
///			}			
/// 
///			// Write the chunk that will contain integers
///			using (IGorgonChunkWriter writer = file.OpenChunk(IntChunk))
///			{
///				writer.Write(ints.Length);
///				for (int i = 0; i < ints.Length; ++i)
///				{
///					writer.Write(ints[i]);
///				}
///			}
///		}
///		finally
///		{
///			// Ensure that we close the file, otherwise it'll be corrupt because the 
///			// chunk table will not be persisted
///			file.Close();
/// 
///			if (myStream is not null)
///			{
///				myStream.Dispose();
///			}
///		}
/// ]]>
/// </code>
/// </example>
/// <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF) details</conceptualLink>
public sealed class GorgonChunkFileWriter
    : GorgonChunkFile
{
    // The application specific header ID.
    private readonly ulong _appHeaderId;
    // The position of the place holder position for deferred data.
    private ulong _placeHolderStartPosition;
    // The active chunk writer.
    private ChunkWriter? _activeWriter;

    /// <summary>
    /// Function to write the header information for the chunk file.
    /// </summary>
    protected override void OnOpen()
    {
        using BinaryWriter writer = new(Stream, Encoding.UTF8, true);
        writer.Write(FileFormatHeaderIDv0100);
        writer.Write(_appHeaderId);

        // Write these as placeholders, we'll be back to fill it when we close the file.
        _placeHolderStartPosition = (ulong)Stream.Position;
        // The total length of the file.
        writer.Write((long)0);
        // The offset of the chunk table within the stream.
        writer.Write((long)0);
    }

    /// <summary>
    /// Function called when a chunk file is closing.
    /// </summary>
    /// <returns>The total number of bytes written to the stream.</returns>
    protected override long OnClose()
    {
        // Force the last chunk to close.
        _activeWriter?.Close();

        // Write out the file footer and chunk table.
        using (BinaryWriter writer = new(Stream, Encoding.UTF8, true))
        {
            long tableOffset = Stream.Position;

            writer.Write(ChunkTableID);
            writer.Write(Chunks.Count);

            foreach (GorgonChunk chunk in Chunks)
            {
                writer.Write(chunk.ID);
                writer.Write(chunk.Size);
                writer.Write(chunk.FileOffset);
            }

            Stream.Position = (long)_placeHolderStartPosition;
            writer.Write(Stream.Length);
            writer.Write(tableOffset);
        }

        return Stream.Length;
    }

    /// <summary>
    /// Function to open a new chunk for writing.
    /// </summary>
    /// <param name="chunkId">The ID of the chunk to create.</param>
    /// <returns>A <see cref="IGorgonChunkWriter" /> that will allow writing within the chunk.</returns>
    /// <exception cref="IOException">Thrown if the chunk was opened without calling <see cref="GorgonChunkFile.Open"/> first.
    /// <para>-or-</para>
    /// <para>Thrown if another <see cref="IGorgonChunkWriter"/> is open elsewhere.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this to write data to a chunk within the file. This method will add a new chunk to the chunk table represented by the <see cref="GorgonChunkFile.Chunks"/> collection. Note that the <paramref name="chunkId"/> 
    /// is not required to be unique (but should be for best results), but must not be the same as the header for the file, or the chunk table identifier. There are constants in the <see cref="GorgonChunkFile"/> type 
    /// that expose these values.
    /// </para>
    /// <para>
    /// Only one chunk may be open for writing at a time. If a chunk is already open, then an exception will be thrown.
    /// </para>
    /// <note type="Important">
    /// The <see cref="IGorgonChunkWriter"/> implements <see cref="IDisposable"/>. Ensure the dispose method is called when finished with the chunk, otherwise file corruption may occur.
    /// </note>
    /// </remarks>
    /// <seealso cref="IGorgonChunkWriter"/>
    public IGorgonChunkWriter OpenChunk(string chunkId) => OpenChunk(chunkId.ChunkID());

    /// <summary>
    /// Function to open a new chunk for writing.
    /// </summary>
    /// <param name="chunkId">The ID of the chunk to create.</param>
    /// <returns>A <see cref="IGorgonChunkWriter" /> that will allow writing within the chunk.</returns>
    /// <exception cref="IOException">Thrown if the chunk was opened without calling <see cref="GorgonChunkFile.Open"/> first.
    /// <para>-or-</para>
    /// <para>Thrown if another <see cref="IGorgonChunkWriter"/> is open elsewhere.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this to write data to a chunk within the file. This method will add a new chunk to the chunk table represented by the <see cref="GorgonChunkFile.Chunks"/> collection. Note that the <paramref name="chunkId"/> 
    /// is not required to be unique (but should be for best results), but must not be the same as the header for the file, or the chunk table identifier. There are constants in the <see cref="GorgonChunkFile"/> type 
    /// that expose these values.
    /// </para>
    /// <para>
    /// Only one chunk may be open for writing at a time. If a chunk is already open, then an exception will be thrown.
    /// </para>
    /// <note type="Important">
    /// The <see cref="IGorgonChunkWriter"/> implements <see cref="IDisposable"/>. Ensure the dispose method is called when finished with the chunk, otherwise file corruption may occur.
    /// </note>
    /// </remarks>
    /// <seealso cref="IGorgonChunkWriter"/>
    public IGorgonChunkWriter OpenChunk(ulong chunkId)
    {
        if (!IsOpen)
        {
            throw new IOException(Resources.GOR_ERR_CHUNK_FILE_NOT_OPEN);
        }

        ValidateChunkID(chunkId);

        if (_activeWriter is not null)
        {
            throw new IOException(string.Format(Resources.GOR_ERR_CHUNK_ALREADY_OPEN, chunkId.FormatHex()));
        }

        // Function called when the chunk is closed.
        void OnChunkClosed(GorgonChunk chunk)
        {
            if (chunk.ID != 0)
            {
                ChunkList.Add(chunk);
            }

            _activeWriter = null;

            // Because we're using sub streams, we must ensure that we continue writing at the end of the stream.
            Stream.Position = Stream.Length;
        }

        ulong position = (ulong)Stream.Position - (_placeHolderStartPosition + 16);

        using (BinaryWriter writer = new(Stream, Encoding.UTF8, true))
        {
            writer.Write(chunkId);
        }

        position += sizeof(ulong);

        _activeWriter = new ChunkWriter(chunkId, Stream, position, OnChunkClosed);

        return _activeWriter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonChunkFileWriter"/> class.
    /// </summary>
    /// <param name="stream">The stream that contains the chunk file to write.</param>
    /// <param name="appHeaderId">An application specific header ID to write to the file for validation.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream" /> is has its <see cref="Stream.CanSeek" /> property set to <b>false</b> 
    /// <para>-or-</para>
    /// <para>
    /// Thrown when the <paramref name="stream"/> is read-only.
    /// </para>
    /// </exception>    
    /// <remarks>
    /// <para>
    /// The <paramref name="stream"/> passed to this method requires that the <see cref="Stream.CanSeek"/> property returns a value of <b>true</b>.
    /// </para>
    /// <para>
    /// The <paramref name="appHeaderId"/> provides an application specific header ID that can be used to identify the contents of the file. When the chunk file is written this app ID can be used to verify 
    /// whether the file can be read by the application or not.
    /// </para>
    /// </remarks>
    public GorgonChunkFileWriter(Stream stream, ulong appHeaderId)
        : base(stream)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(stream));
        }

        _appHeaderId = appHeaderId;
    }
}
