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
using System.IO;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonChunkFileWriter"/>
	public sealed class GorgonChunkFileWriter
		: GorgonChunkFile<GorgonBinaryWriter>, IGorgonChunkFileWriter
	{
		#region Constants.
		// The size of a Int64 type.
		private const int Int64Size = sizeof(Int64);
		#endregion

		#region Variables.
		// The application specific header ID.
		private readonly UInt64 _appHeaderId;
		// The position of the place holder position for deferred data.
		private Int64 _placeHolderStartPosition;
		// The position within the stream where the header ends.
		private Int64 _headerEnd;
		// The active chunk that we're writing into.
		private GorgonChunk _activeChunk;
		// The active chunk writer.
		private GorgonBinaryWriter _activeWriter;
		#endregion

		#region Methods.
		/// <summary>
		/// This method is not available for this type.
		/// </summary>
		/// <exception cref="NotSupportedException">Writing is not supported by this type.</exception>
		protected override void ReadHeaderValidate()
		{
			throw new NotSupportedException();
		}
		
		/// <inheritdoc/>
		protected override void WriteHeader()
		{
			using (GorgonBinaryWriter writer = new GorgonBinaryWriter(Stream, true))
			{
				writer.Write(FileFormatHeaderIDv0100);
				writer.Write(_appHeaderId);

				// Write these as placeholders, we'll be back to fill it when we close the file.
				_placeHolderStartPosition = Stream.Position;
				writer.Write((Int64)0);
				writer.Write((Int64)0);

				// Record where the header has ended.
				_headerEnd = Stream.Position;
			}
		}

		/// <summary>
		/// Function called when a chunk file is closing.
		/// </summary>
		/// <returns>The total number of bytes written to the stream.</returns>
		protected override Int64 OnClose()
		{
			// Force the last chunk to close.
			if (_activeChunk != null)
			{
				CloseChunk();
			}

			// Write out the file footer and chunk table.
			using (var writer = new GorgonBinaryWriter(Stream, true))
			{
				Int64 tableOffset = Stream.Position;

				writer.Write(ChunkTableID);
				writer.Write(Chunks.Count);

				foreach (IGorgonChunk chunk in Chunks)
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

		/// <inheritdoc cref="IGorgonChunkFileWriter.OpenChunk(ulong)"/>
		public override GorgonBinaryWriter OpenChunk(ulong chunkId)
		{
			ValidateChunkID(chunkId);

			if (_activeChunk != null)
			{
				if (_activeChunk.ID == chunkId)
				{
					return _activeWriter;
				}

				CloseChunk();
			}

			_activeChunk = new GorgonChunk
			                        {
										ID = chunkId,
										// Size is set to 0 for now, we don't know how big it'll be until we're done writing.
										Size = 0,
										FileOffset = Stream.Position - _headerEnd + Int64Size
			                        };

			using (var chunkIDWriter = new GorgonBinaryWriter(Stream, true))
			{
				chunkIDWriter.Write(chunkId);
			}

			_activeWriter = new GorgonBinaryWriter(new GorgonStreamWrapper(Stream, 0, 0), true);

			return _activeWriter;
		}

		/// <inheritdoc cref="IGorgonChunkFileWriter.CloseChunk"/>
		public override void CloseChunk()
		{
			if (_activeChunk == null)
			{
				return;
			}

			// Move the stream forward by the amount written.
			_activeChunk.Size = (int)(_activeWriter.BaseStream.Length);
			Stream.Position += _activeChunk.Size;
			
			ChunkList.Add(_activeChunk);

			_activeChunk = null;
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
		/// The <paramref name="stream"/> passed to this method requires that the <see cref="System.IO.Stream.CanSeek"/> property returns a value of <b>true</b>.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream" /> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="stream" /> is has its <see cref="Stream.CanSeek" /> property set to <b>false</b>.
		/// <para>-or-</para>
		/// <para>
		/// Thrown when the <paramref name="stream"/> is read-only.
		/// </para>
		/// </exception>
		/// <exception cref="IOException"></exception>
		public GorgonChunkFileWriter(Stream stream, UInt64 appHeaderId)
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
