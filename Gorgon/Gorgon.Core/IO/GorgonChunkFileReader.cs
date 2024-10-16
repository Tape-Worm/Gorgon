﻿// 
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
// Created: Monday, June 15, 2015 8:57:39 PM
// 

using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// A reader that will read in and parse the contents of a <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink> file
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
/// <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF) details</conceptualLink>
/// <example>
/// This example builds on the example provided in the <see cref="GorgonChunkFileWriter"/> example and shows how to read in the file created by that example:
/// <code language="csharp">
/// <![CDATA[
///		// An application defined file header ID. Useful for identifying the contents of the file
///		const ulong FileHeader = 0xBAADBEEFBAADF00D;	
/// 
///		const string StringsChunk = "STRNGLST";
///		const string IntChunk = "INTGRLST"; 
/// 
///		string[] strings;
///		int[] ints;
/// 
///		Stream myStream = File.Open("<<Path to your file>>", FileMode.Open, FileAccess.Read, FileShare.Read);
/// 
///		// Notice that we're passing in an array of file header ID values. This allows us to allow the formatter to 
///		// read the file with multiple versions of the header ID. This gives us an ability to provide backwards 
///		// compatibility with file types
///		GorgonChunkFileReader file = new GorgonChunkFileReader(myStream, new [] { FileHeader });
/// 
///		try
///		{
///			// Open the file for writing within the stream
///			file.Open();
/// 
///			// Read the chunk that contains the integers. Note that this is different than the writer example,
///			// we wrote these items last, and in a sequential file read, we'd have to read the values last when 
///			// reading the file. But with this format, we can find the chunk and read it from anywhere in the file
///			// Alternatively, we could pass in an ulong value for the chunk ID instead of a string
///			using (IGorgonChunkReader reader = file.OpenChunk(IntChunk))
///			{
///				ints = new int[reader.ReadInt32()];
///	
///				for (int = 0; i < ints.Length; ++i)
///				{
///					ints[i] = reader.ReadInt32();
///				}
///			}			
/// 
///			// Read the chunk that contains strings
///			using (IGorgonChunkReader reader = file.OpenChunk(StringsChunk))
///			{
///				strings = new string[reader.ReadInt32()];
/// 
///				for (int i = 0; i < strings.Length; ++i)
///				{
///					strings[i] = reader.ReadString();
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
public sealed class GorgonChunkFileReader
    : GorgonChunkFile
{
    // The list of allowable application specific Id values.
    private readonly HashSet<ulong> _appSpecificIds;
    // The currently active chunk.
    private GorgonChunk _activeChunk;
    // The reader for the active chunk.
    private ChunkReader? _activeReader;
    // The byte marker for the end of the file header.
    private long _headerEnd;
    // The size of the file, in bytes.
    private long _fileSize;

    /// <summary>
    /// Function to read in the chunk table from the file.
    /// </summary>
    /// <param name="reader">The reader for the stream.</param>
    /// <param name="offset">Offset of the chunk table from the file start.</param>
    private void ReadChunkTable(BinaryReader reader, long offset)
    {
        long prevPosition = Stream.Position;

        try
        {
            Stream.Position = offset;

            ulong chunkID = reader.ReadUInt64();

            if (chunkID != ChunkTableID)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR_ERR_CHUNK_FILE_TABLE_CHUNK_INVALID);
            }

            int count = reader.ReadInt32();

            if (count < 0)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GOR_ERR_CHUNK_FILE_TABLE_INVALID_COUNT);
            }

            // Retrieve the chunk table.
            for (int i = 0; i < count; ++i)
            {
                ChunkList.Add(new GorgonChunk(reader.ReadUInt64(), reader.ReadInt32(), reader.ReadUInt64()));
            }
        }
        finally
        {
            Stream.Position = prevPosition;
        }
    }

    /// <summary>
    /// Function to read in the header information from the chunk file and validate it.
    /// </summary>
    /// <exception cref="GorgonException">Thrown when the chunked file format header ID does not match.
    /// <para>-or-</para>
    /// <para>Thrown when application specific header ID in the file was not found in the list passed to the constructor.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the chunk file table offset is less than or equal to the size of the header.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the file size recorded in the header is less than the size of the header.</para>
    /// </exception>
    protected override void OnOpen()
    {
        using BinaryReader reader = new(Stream, Encoding.UTF8, true);
        ulong headerID = reader.ReadUInt64();

        if (headerID != FileFormatHeaderIDv0100)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_CHUNK_FILE_HEADER_MISMATCH, headerID));
        }

        ulong appHeaderID = reader.ReadUInt64();

        if (!_appSpecificIds.Contains(appHeaderID))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_CHUNK_FILE_HEADER_MISMATCH, appHeaderID));
        }

        // Get the size of the file, in bytes.
        _fileSize = reader.ReadInt64();

        // The offset of the chunk table in the file.
        long tablePosition = reader.ReadInt64();

        // Record the end of the header.
        _headerEnd = Stream.Position;

        // Ensure our file size is somewhat realistic.
        if (_fileSize < _headerEnd)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_CHUNK_FILE_TABLE_OFFSET_INVALID, _fileSize.FormatMemory()));
        }

        // Ensure that our table position is not less than our header position.
        if ((tablePosition <= _headerEnd)
            || (tablePosition >= _fileSize))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GOR_ERR_CHUNK_FILE_TABLE_OFFSET_INVALID);
        }

        ReadChunkTable(reader, tablePosition);
    }

    /// <summary>
    /// Function called when a chunk file is closing.
    /// </summary>
    /// <returns>The total number of bytes read from the stream.</returns>
    protected override long OnClose()
    {
        _activeReader?.Dispose();
        _activeReader = null;
        return _fileSize;
    }

    /// <summary>
    /// Function to determine if the data in the stream is a chunk file.
    /// </summary>
    /// <param name="stream">The stream containing the data.</param>
    /// <param name="appIDs">A list of application specific IDs to check for.</param>
    /// <returns><b>true</b> if the stream data contains a chunk file, <b>false</b> if not.</returns>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only.</exception>
    /// <remarks>
    /// <para>
    /// This method will restore the <paramref name="stream"/> position when finished, so further calls to stream functions will work as though the stream position has not advanced.
    /// </para>
    /// </remarks>
    public static bool IsReadable(Stream stream, IEnumerable<ulong> appIDs)
    {
        if (!stream.CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        if (!stream.CanSeek)
        {
            return false;
        }

        BinaryReader? reader = null;
        long pos = 0;

        try
        {
            pos = stream.Position;

            if (stream.Length < 16)
            {
                return false;
            }

            reader = new BinaryReader(stream, Encoding.UTF8, true);

            ulong headerID = reader.ReadUInt64();

            if (FileFormatHeaderIDv0100 != headerID)
            {
                return false;
            }

            if (reader.BaseStream.Position + 8 > reader.BaseStream.Length)
            {
                return false;
            }

            ulong appHeaderID = reader.ReadUInt64();

            if (!appIDs.Contains(appHeaderID))
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            reader?.Dispose();
            stream.Position = pos;
        }
    }

    /// <summary>
    /// Function to open a chunk for reading.
    /// </summary>
    /// <param name="chunkId">The ID of the chunk to open.</param>
    /// <returns>A <see cref="IGorgonChunkReader" /> that will allow reading within the chunk.</returns>
    /// <remarks>
    /// <para>
    /// Use this to read data from a chunk within the file. If the <paramref name="chunkId"/> is not found, then this method will throw an exception. To mitigate this, check for the existence of a chunk in 
    /// the <see cref="GorgonChunkFile.Chunks"/> collection.
    /// </para>
    /// <para>
    /// This method will provide minimal validation for the chunk in that it will only check the <paramref name="chunkId"/> to see if it matches what's in the file, beyond that, the user is responsible for 
    /// validating the data that lives within the chunk.
    /// </para>
    /// </remarks>
    /// <exception cref="IOException">Thrown if the chunk was opened without calling <see cref="GorgonChunkFile.Open"/> first.
    /// <para>-or-</para>
    /// <para>Thrown if another <see cref="IGorgonChunkReader"/> is open elsewhere.</para>
    /// </exception>
    /// <exception cref="GorgonException">Thrown when the <paramref name="chunkId" /> does not match the chunk in the file.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="chunkId" /> was not found in the chunk table.</exception>
    public IGorgonChunkReader OpenChunk(string chunkId) => OpenChunk(chunkId.ChunkID());

    /// <summary>
    /// Function to open a chunk for reading.
    /// </summary>
    /// <param name="chunkId">The ID of the chunk to open.</param>
    /// <returns>A <see cref="IGorgonChunkReader" /> that will allow reading within the chunk.</returns>
    /// <remarks>
    /// <para>
    /// Use this to read data from a chunk within the file. If the <paramref name="chunkId"/> is not found, then this method will throw an exception. To mitigate this, check for the existence of a chunk in 
    /// the <see cref="GorgonChunkFile.Chunks"/> collection.
    /// </para>
    /// <para>
    /// This method will provide minimal validation for the chunk in that it will only check the <paramref name="chunkId"/> to see if it matches what's in the file, beyond that, the user is responsible for 
    /// validating the data that lives within the chunk.
    /// </para>
    /// </remarks>
    /// <exception cref="IOException">Thrown if the chunk was opened without calling <see cref="GorgonChunkFile.Open"/> first.
    /// <para>-or-</para>
    /// <para>Thrown if another <see cref="IGorgonChunkReader"/> is open elsewhere.</para>
    /// </exception>
    /// <exception cref="GorgonException">Thrown when the <paramref name="chunkId" /> does not match the chunk in the file.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="chunkId" /> was not found in the chunk table.</exception>
    public IGorgonChunkReader OpenChunk(ulong chunkId)
    {
        if (!IsOpen)
        {
            throw new IOException(Resources.GOR_ERR_CHUNK_FILE_NOT_OPEN);
        }

        if ((_activeChunk.ID != 0) || (_activeReader is not null))
        {
            throw new IOException(string.Format(Resources.GOR_ERR_CHUNK_ALREADY_OPEN, _activeChunk.ID.FormatHex()));
        }

        // Function called when the chunk is closed.
        void OnChunkClosed()
        {
            _activeChunk = default;
            Stream.Position = _headerEnd;
            _activeReader = null;
        }

        ValidateChunkID(chunkId);

        GorgonChunk chunk = Chunks[chunkId];

        if (chunk.Equals(in GorgonChunk.EmptyChunk))
        {
            throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_CHUNK_NOT_FOUND, chunkId.FormatHex()));
        }

        // Adjust our stream position.
        Stream.Position = ((long)chunk.FileOffset + _headerEnd) - sizeof(ulong);

        using (BinaryReader reader = new(Stream, Encoding.UTF8, true))
        {
            ulong id = reader.ReadUInt64();

            if (id != chunk.ID)
            {
                throw new GorgonException(GorgonResult.CannotRead,
                                          string.Format(Resources.GOR_ERR_CHUNK_FILE_CHUNK_MISMATCH,
                                                        chunk.FileOffset.FormatHex(),
                                                        chunk.ID.FormatHex()));
            }
        }

        // Validate the chunk ID at the offset.
        _activeReader = new ChunkReader(chunk, Stream, OnChunkClosed);
        _activeChunk = chunk;

        return _activeReader;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonChunkFileReader"/> class.
    /// </summary>
    /// <param name="stream">The stream containing the chunk file to read.</param>
    /// <param name="appSpecificIds">The allowable application specific ids for file validation.</param>
    /// <remarks>
    /// </remarks>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="appSpecificIds"/> contains no values.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="stream"/> is write-only</para>
    /// </exception>
    /// <exception cref="EndOfStreamException">Thrown when the <paramref name="stream"/> is at its end.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="stream"/> passed to this method requires that the <see cref="Stream.CanSeek"/> property returns a value of <b>true</b>.
    /// </para>
    /// <para>
    /// The <paramref name="appSpecificIds"/> parameter is an <see cref="IEnumerable{T}"/> because there may be multiple versions of the file that an application might wish to read. By providing a list 
    /// of which versions are supported through the application specific IDs, the reader can determine if the file type is readable or not.
    /// </para>
    /// </remarks>
    public GorgonChunkFileReader(Stream stream, IEnumerable<ulong> appSpecificIds)
        : base(stream)
    {
        if (Stream.Length == Stream.Position)
        {
            throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
        }

        _appSpecificIds = new HashSet<ulong>(appSpecificIds.Distinct().OrderByDescending(item => item));

        ArgumentEmptyException.ThrowIfNullOrEmpty(_appSpecificIds);
    }
}
