#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Sunday, June 14, 2015 10:40:50 PM
// 
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// A writer that will lay out and write the contents of a <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink> file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows access to a file format that uses the concept of grouping sections of an object together into a grouping called a chunk. This chunk will hold binary data associated with an object allows 
    /// the developer to read/write only the pieces of the object that are absolutely necessary while skipping optional chunks.
    /// </para>
    /// <para>
    /// A more detailed explanation of the chunk file format can be found in the <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink> topic.
    /// </para>
    /// <para>
    /// A chunk file object will expose a collection of <see cref="GorgonChunk"/> values, and these give the available chunks in the file and can be looked up either by the <see cref="ulong"/> value for 
    /// the chunk ID, or an 8 character <see cref="string"/> that represents the chunk (this is recommended for readability). This allows an application to do validation on the chunk file to ensure that 
    /// its format is correct. It also allows an application to discard chunks it doesn't care about or are optional. This allows for some level of versioning between chunk file formats.
    /// </para>
    /// <para>
    /// Chunks can be accessed in any order, not just the order in which they were written. This allows an application to only take the pieces they require from the file, and leave the rest. It also allows 
    /// for optional chunks that can be skipped if not present, and read/written when they are.
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
    ///		// An application defined file header ID. Useful for identifying the contents of the file.
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
    ///			// Open the file for writing within the stream.
    ///			file.Open();
    /// 
    ///			// Write the chunk that will contain strings.
    ///			// Alternatively, we could pass in an ulong value for the chunk ID instead of a string.
    ///			using (GorgonBinaryWriter writer = file.OpenChunk(StringsChunk))
    ///			{
    ///				writer.Write(strings.Length);
    ///				for (int = 0; i < strings.Length; ++i)
    ///				{
    ///					writer.Write(strings[i]);
    ///				}
    ///			}			
    /// 
    ///			// Write the chunk that will contain integers.
    ///			using (GorgonBinaryWriter writer = file.OpenChunk(IntChunk))
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
    ///			// chunk table will not be persisted.
    ///			file.Close();
    /// 
    ///			if (myStream != null)
    ///			{
    ///				myStream.Dispose();
    ///			}
    ///		}
    /// ]]>
    /// </code>
    /// </example>
    /// <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF) details</conceptualLink>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"
        , Justification = "This is not correct. GorgonBinaryWriter does not close its underlying stream, thus Dispose does not need to be called.")]
    public sealed class GorgonChunkFileWriter
        : GorgonChunkFile<GorgonBinaryWriter>
    {
        #region Variables.
        // The application specific header ID.
        private readonly ulong _appHeaderId;
        // The position of the place holder position for deferred data.
        private long _placeHolderStartPosition;
        // The position within the stream where the header ends.
        private long _headerEnd;
        // The active chunk that we're writing into.
        private GorgonChunk _activeChunk;
        // The active chunk writer.
        private GorgonBinaryWriter _activeWriter;
        #endregion

        #region Methods.
        /// <summary>
        /// This method is not available for this type.
        /// </summary>
        /// <exception cref="NotSupportedException">Reading is not supported by this type.</exception>
        protected override void ReadHeaderValidate() => throw new NotSupportedException();

        /// <summary>
        /// Function to write the header information for the chunk file.
        /// </summary>
        protected override void WriteHeader()
        {
            using (var writer = new GorgonBinaryWriter(Stream, true))
            {
                writer.Write(FileFormatHeaderIDv0100);
                writer.Write(_appHeaderId);

                // Write these as placeholders, we'll be back to fill it when we close the file.
                _placeHolderStartPosition = Stream.Position;
                writer.Write((long)0);
                writer.Write((long)0);

                // Record where the header has ended.
                _headerEnd = Stream.Position;
            }
        }

        /// <summary>
        /// Function called when a chunk file is closing.
        /// </summary>
        /// <returns>The total number of bytes written to the stream.</returns>
        protected override long OnClose()
        {
            // Force the last chunk to close.
            if (_activeChunk.ID != 0)
            {
                CloseChunk();
            }

            // Write out the file footer and chunk table.
            using (var writer = new GorgonBinaryWriter(Stream, true))
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

                Stream.Position = _placeHolderStartPosition;
                writer.Write(Stream.Length);
                writer.Write(tableOffset);
            }

            return Stream.Length;
        }

        /// <summary>
        /// Function to open a chunk for reading.
        /// </summary>
        /// <param name="chunkId">The ID of the chunk to open.</param>
        /// <returns>A <see cref="GorgonBinaryWriter" /> that will allow writing within the chunk.</returns>
        /// <remarks>
        /// <para>
        /// Use this to write data to a chunk within the file. This method will add a new chunk to the chunk table represented by the <see cref="GorgonChunkFile{T}.Chunks"/> collection. Note that the <paramref name="chunkId"/> 
        /// is not required to be unique, but must not be the same as the header for the file, or the chunk table identifier. There are constants in the <see cref="GorgonChunkFile{T}"/> type that expose these values.
        /// </para>
        /// <note type="Important">
        /// This method should always be paired with a call to <see cref="GorgonChunkFile{T}.CloseChunk"/>. Failure to do so will keep the chunk table from being updated properly, and corrupt the file.
        /// </note>
        /// </remarks>
        /// <exception cref="IOException">Thrown if the chunk was opened without calling <see cref="GorgonChunkFile{T}.Open"/> first.</exception>
        public override GorgonBinaryWriter OpenChunk(ulong chunkId)
        {
            if (!IsOpen)
            {
                throw new IOException(Resources.GOR_ERR_CHUNK_FILE_NOT_OPEN);
            }

            ValidateChunkID(chunkId);

            if (_activeChunk.ID != 0)
            {
                if (_activeChunk.ID == chunkId)
                {
                    return _activeWriter;
                }

                CloseChunk();
            }

            // Size is 0 for now, we'll update it later.
            _activeChunk = new GorgonChunk(chunkId, 0, (ulong)(Stream.Position - _headerEnd + sizeof(long)));

            using (var chunkIDWriter = new GorgonBinaryWriter(Stream, true))
            {
                chunkIDWriter.Write(chunkId);
            }

            _activeWriter = new GorgonBinaryWriter(new GorgonStreamWrapper(Stream, 0, 0), true);

            return _activeWriter;
        }

        /// <summary>
        /// Function to close an open chunk.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will close the active chunk, and add it to the chunk table list. It will reposition the stream pointer for the stream passed to the constructor of this object to the next position for 
        /// a chunk, or the end of the chunk data.
        /// </para>
        /// <para>
        /// If this method is not called, then the chunk will not be added to the chunk table in the file and the file will lose that chunk. This, however, does not mean the file is necessarily corrupt, 
        /// just that the chunk will not exist. Regardless, this method should always be called when one of the <see cref="O:Gorgon.IO.GorgonChunkFile`1.OpenChunk"/> are called.
        /// </para>
        /// </remarks>
        public override void CloseChunk()
        {
            if (_activeChunk.ID == 0)
            {
                return;
            }

            // Move the stream forward by the amount written.
            _activeChunk = new GorgonChunk(_activeChunk.ID, (int)(_activeWriter.BaseStream.Length), _activeChunk.FileOffset);
            Stream.Position += _activeChunk.Size;

            ChunkList.Add(_activeChunk);

            _activeChunk = default;
            _activeWriter.Dispose();
            _activeWriter = null;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonChunkFileWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream that contains the chunk file to write.</param>
        /// <param name="appHeaderId">An application specific header ID to write to the file for validation.</param>
        /// <remarks>
        /// The <paramref name="stream"/> passed to this method requires that the <see cref="Stream.CanSeek"/> property returns a value of <b>true</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="stream" /> is has its <see cref="Stream.CanSeek" /> property set to <b>false</b>.
        /// <para>-or-</para>
        /// <para>
        /// Thrown when the <paramref name="stream"/> is read-only.
        /// </para>
        /// </exception>
        /// <exception cref="IOException"></exception>
        public GorgonChunkFileWriter(Stream stream, ulong appHeaderId)
            : base(stream)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(stream));
            }

            _appHeaderId = appHeaderId;
            Mode = ChunkFileMode.Write;
        }
        #endregion
    }
}
