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
// Created: Monday, June 15, 2015 10:40:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Core;

namespace Gorgon.IO
{
	/// <summary>
	/// A reader that will read in and parse the contents of a <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink> file.
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
	/// A chunk file object will expose a collection of <see cref="IGorgonChunk"/> values, and these give the available chunks in the file and can be looked up either by the <see cref="ulong"/> value for 
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
	/// <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF) details</conceptualLink>
	/// <example>
	/// This example builds on the example provided in the <see cref="GorgonChunkFileWriter"/> example and shows how to read in the file created by that example:
	/// <code language="csharp">
	/// <![CDATA[
	///		// An application defined file header ID. Useful for identifying the contents of the file.
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
	///		// compatibility with file types.
	///		GorgonChunkFileWriter file = new GorgonChunkFileReader(myStream, new [] { FileHeader });
	/// 
	///		try
	///		{
	///			// Open the file for writing within the stream.
	///			file.Open();
	/// 
	///			// Read the chunk that contains the integers. Note that this is different than the writer example,
	///			// we're wrote these items last, and in a sequential file read, we'd have to read the values last when 
	///			// reading the file. But with this format, we can find the chunk and read it from anywhere in the file.
	///			// Alternatively, we could pass in an UInt64 value for the chunk ID instead of a string.
	///			using (GorgonBinaryReader reader = file.OpenChunk(IntChunk))
	///			{
	///				ints = new int[reader.ReadInt32()];
	///	
	///				for (int = 0; i < ints.Length; ++i)
	///				{
	///					ints[i] = reader.ReadInt32();
	///				}
	///			}			
	/// 
	///			// Read the chunk that contains strings.
	///			using (GorgonBinaryReader reader = file.OpenChunk(StringsChunk))
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
	public interface IGorgonChunkFileReader
	{
		#region Properties.
		/// <summary>
		/// Property to return the list of chunks available in the file.
		/// </summary>
		/// <remarks>
		/// Use this property to determine if a chunk exists when reading a chunk file.
		/// </remarks>
		IGorgonReadOnlyChunkCollection Chunks
		{
			get;
		}

		#endregion

		#region Methods.
		/// <summary>
		/// Function to close an open chunk.
		/// </summary>
		/// <remarks>This will close the active chunk, and reposition the stream to the end of the file header.</remarks>
		void CloseChunk();

		/// <summary>
		/// Function to open a chunk for reading.
		/// </summary>
		/// <param name="chunkId">The ID of the chunk to open.</param>
		/// <returns>A <see cref="GorgonBinaryReader" /> that will allow reading within the chunk.</returns>
		/// <remarks>
		/// <para>
		/// Use this to read data from a chunk within the file. If the <paramref name="chunkId"/> is not found, then this method will throw an exception. To mitigate this, check for the existence of a chunk in 
		/// the <see cref="IGorgonChunkFileReader.Chunks"/> collection.
		/// </para>
		/// <para>
		/// This method will provide minimal validation for the chunk in that it will only check the <paramref name="chunkId"/> to see if it matches what's in the file, beyond that, the user is responsible for 
		/// validating the data that lives within the chunk.
		/// </para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the <paramref name="chunkId" /> does not match the chunk in the file.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the <paramref name="chunkId" /> was not found in the chunk table.</exception>
		GorgonBinaryReader OpenChunk(ulong chunkId);

		/// <summary>
		/// Function to open a chunked file within the stream.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This opens a Gorgon chunk file that exists within the <see cref="GorgonChunkFile{T}.Stream"/> passed to the constructor of this object. Typically this would be in a <see cref="FileStream"/>, but any type of stream 
		/// is valid and can contain a chunk file. 
		/// </para>
		/// <para>
		/// If the this method is called and the file is already opened, then it will be closed and reopened.
		/// </para>
		/// <note type="Important">
		/// Always pair a call to <c>Open</c> with a call to <see cref="IGorgonChunkFileReader.Close"/>, otherwise the file may become corrupted or the stream may not be updated to the correct position.
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
		void Open();

		/// <summary>
		/// Function to close an open chunk file in the stream.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Always call this method when a call to <see cref="IGorgonChunkFileReader.Open"/> is made. Failure to do so could cause the file to become corrupted.
		/// </para>
		/// <para>
		/// If the file is not open, then this method will do nothing.
		/// </para>
		/// </remarks>
		void Close();

		/// <summary>
		/// Function to open a chunk, by the text representation of its ID, for reading.
		/// </summary>
		/// <param name="chunkName">The name of the chunk.</param>
		/// <returns>A <see cref="GorgonBinaryReader"/>, or <see cref="GorgonBinaryWriter"/> that will allow reading or writing within the chunk.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="chunkName"/> parameter is empty.</exception>
		/// <remarks>
		/// See the <see cref="Gorgon.IO.IGorgonChunkFileReader.OpenChunk(ulong)"/> method for more information.
		/// </remarks>
		/// <seealso cref="Gorgon.IO.IGorgonChunkFileReader.OpenChunk(ulong)"/>
		GorgonBinaryReader OpenChunk(string chunkName);
		#endregion
	}
}