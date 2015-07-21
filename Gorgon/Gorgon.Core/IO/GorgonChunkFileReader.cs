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
// Created: Monday, June 15, 2015 8:57:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonChunkFileReader"/>
	public sealed class GorgonChunkFileReader
		: GorgonChunkFile<GorgonBinaryReader>, IGorgonChunkFileReader
	{
		#region Variables.
		// The list of allowable application specific Id values.
		private readonly HashSet<UInt64> _appSpecificIds;
		// The currently active chunk.
		private IGorgonChunk _activeChunk;
		// The reader for the active chunk.
		private GorgonBinaryReader _activeReader;
		// The byte marker for the end of the file header.
		private Int64 _headerEnd;
		// The size of the file, in bytes.
		private Int64 _fileSize;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read in the chunk table from the file.
		/// </summary>
		/// <param name="reader">The reader for the stream.</param>
		/// <param name="offset">Offset of the chunk table from the file start.</param>
		private void ReadChunkTable(GorgonBinaryReader reader, Int64 offset)
		{
			Int64 prevPosition = Stream.Position;

			try
			{
				Stream.Position = offset;

				UInt64 chunkID = reader.ReadUInt64();

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
					ChunkList.Add(new GorgonChunk
					{
						ID = reader.ReadUInt64(),
						Size = reader.ReadInt32(),
						FileOffset = reader.ReadInt64()
					});
				}
			}
			finally
			{
				Stream.Position = prevPosition;
			}
		}

		/// <summary>
		/// This method is not available for this type.
		/// </summary>
		/// <exception cref="NotSupportedException">Writing is not supported by this type.</exception>
		protected override void WriteHeader()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		/// <exception cref="GorgonException">Thrown when the chunked file format header ID does not match.
		/// <para>-or-</para>
		/// <para>Thrown when application specific header ID in the file was not found in the list passed to the constructor.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the chunk file table offset is less than or equal to the size of the header.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the file size recorded in the header is less than the size of the header.</para>
		/// </exception>
		protected override void ReadHeaderValidate()
		{
			using (GorgonBinaryReader reader = new GorgonBinaryReader(Stream, true))
			{
				UInt64 headerID = reader.ReadUInt64();

				if (headerID != FileFormatHeaderIDv0100)
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_CHUNK_FILE_HEADER_MISMATCH, headerID));
				}

				UInt64 appHeaderID = reader.ReadUInt64();

				if (!_appSpecificIds.Contains(appHeaderID))
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_CHUNK_FILE_HEADER_MISMATCH, appHeaderID));
				}

				// Get the size of the file, in bytes.
				_fileSize = reader.ReadInt64();

				// The offset of the chunk table in the file.
				Int64 tablePosition = reader.ReadInt64();

				// Record the end of the header.
				_headerEnd = Stream.Position;

				// Ensure our file size is somewhat realistic.
				if (_fileSize < _headerEnd)
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR_ERR_CHUNK_FILE_TABLE_OFFSET_INVALID, _fileSize.FormatMemory()));
				}

				// Ensure that our table position is not less than our header position.
				if (tablePosition <= _headerEnd)
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GOR_ERR_CHUNK_FILE_TABLE_OFFSET_INVALID);
				}

				ReadChunkTable(reader, tablePosition);
			}
		}

		/// <inheritdoc/>
		/// <returns>The total number of bytes read from the stream.</returns>
		protected override Int64 OnClose()
		{
			CloseChunk();
			return _fileSize;
		}

		/// <inheritdoc cref="IGorgonChunkFileReader.CloseChunk"/>
		public override void CloseChunk()
		{
			if (_activeChunk == null)
			{
				return;
			}

			Stream.Position = _headerEnd;
			_activeChunk = null;
			_activeReader.Dispose();
			_activeReader = null;
		}

		/// <inheritdoc cref="IGorgonChunkFileReader.OpenChunk(ulong)"/>
		public override GorgonBinaryReader OpenChunk(ulong chunkId)
		{
			ValidateChunkID(chunkId);

			IGorgonChunk chunk = Chunks[chunkId];

			if (chunk == null)
			{
				throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_CHUNK_NOT_FOUND, chunkId.FormatHex()));	
			}

			if (_activeChunk != null)
			{
				if (_activeChunk.ID == chunkId)
				{
					return _activeReader;
				}

				CloseChunk();
			}

			_activeChunk = chunk;
			GorgonBinaryReader reader = null;

			// Validate the chunk ID at the offset.
			try
			{
				reader = new GorgonBinaryReader(Stream, true);
				Stream.Position = (_activeChunk.FileOffset + _headerEnd) - sizeof(Int64);

				UInt64 fileChunkId = reader.ReadUInt64();

				if (fileChunkId != _activeChunk.ID)
				{
					throw new GorgonException(GorgonResult.CannotRead,
					                          string.Format(Resources.GOR_ERR_CHUNK_FILE_CHUNK_MISMATCH,
					                                        _activeChunk.FileOffset.FormatHex(),
					                                        _activeChunk.ID.FormatHex()));
				}
			}
			finally
			{
				reader?.Dispose();
			}

			_activeReader = new GorgonBinaryReader(new GorgonStreamWrapper(Stream, 0, _activeChunk.Size), true);

			return _activeReader;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonChunkFileReader"/> class.
		/// </summary>
		/// <param name="stream">The stream containing the chunk file to read.</param>
		/// <param name="appSpecificIds">The allowable application specific ids for file validation.</param>
		/// <remarks>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="appSpecificIds"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="appSpecificIds"/> contains no values.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="stream"/> is write-only</para>
		/// </exception>
		/// <exception cref="EndOfStreamException">Thrown when the <paramref name="stream"/> is at its end.</exception>
		/// <remarks>
		/// <para>
		/// The <paramref name="stream"/> passed to this method requires that the <see cref="System.IO.Stream.CanSeek"/> property returns a value of <b>true</b>.
		/// </para>
		/// <para>
		/// The <paramref name="appSpecificIds"/> parameter is an <see cref="IEnumerable{T}"/> because there may be multiple versions of the file that an application might wish to read. By providing a list 
		/// of which versions are supported through the application specific IDs, the reader can determine if the file type is readable or not.
		/// </para>
		/// </remarks>
		public GorgonChunkFileReader(Stream stream, IEnumerable<UInt64> appSpecificIds)
			: base(stream)
		{
			if (Stream.Length == Stream.Position)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}

			if (!Stream.CanRead)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_WRITEONLY, nameof(stream));
			}

			if (appSpecificIds == null)
			{
				throw new ArgumentNullException(nameof(appSpecificIds));
			}

			_appSpecificIds = new HashSet<ulong>(appSpecificIds.Distinct().OrderByDescending(item => item));

			if (_appSpecificIds.Count == 0)
			{
				throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(appSpecificIds));
			}

			Mode = ChunkFileMode.Read;
		}
		#endregion
	}
}
