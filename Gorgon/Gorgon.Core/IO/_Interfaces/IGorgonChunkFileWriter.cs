using System;
using System.IO;
using Gorgon.Core;

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
	///			// Alternatively, we could pass in an UInt64 value for the chunk ID instead of a string.
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
	public interface IGorgonChunkFileWriter
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

		/// <summary>
		/// Property to return the <see cref="GorgonStreamWrapper"/> that is being used by this chunk file reader.
		/// </summary>
		GorgonStreamWrapper Stream
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to open a chunk for reading.
		/// </summary>
		/// <param name="chunkId">The ID of the chunk to open.</param>
		/// <returns>A <see cref="GorgonBinaryWriter" /> that will allow writing within the chunk.</returns>
		/// <remarks>
		/// <para>
		/// Use this to write data to a chunk within the file. This method will add a new chunk to the chunk table represented by the <see cref="IGorgonChunkFileWriter.Chunks"/> collection. Note that the <paramref name="chunkId"/> 
		/// is not required to be unique, but must not be the same as the header for the file, or the chunk table identifier. There are constants in the <see cref="GorgonChunkFile{T}"/> type that expose these values.
		/// </para>
		/// <note type="Important">
		/// This method should always be paired with a call to <see cref="GorgonChunkFileWriter.CloseChunk"/>. Failure to do so will keep the chunk table from being updated properly, and corrupt the file.
		/// </note>
		/// </remarks>
		GorgonBinaryWriter OpenChunk(ulong chunkId);

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
		/// just that the chunk will not exist. Regardless, this method should always be called when one of the <see cref="O:Gorgon.IO.IGorgonChunkFileWriter.OpenChunk"/> are called.
		/// </para>
		/// </remarks>
		void CloseChunk();

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
		/// Always pair a call to <c>Open</c> with a call to <see cref="IGorgonChunkFileWriter.Close"/>, otherwise the file may become corrupted or the stream may not be updated to the correct position.
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
		/// Always call this method when a call to <see cref="IGorgonChunkFileWriter.Open"/> is made. Failure to do so could cause the file to become corrupted.
		/// </para>
		/// <para>
		/// If the file is not open, then this method will do nothing.
		/// </para>
		/// </remarks>
		void Close();

		/// <summary>
		/// Function to open a chunk, by the text representation of its ID, for writing.
		/// </summary>
		/// <param name="chunkName">The name of the chunk.</param>
		/// <returns>A <see cref="GorgonBinaryReader"/>, or <see cref="GorgonBinaryWriter"/> that will allow reading or writing within the chunk.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="chunkName"/> parameter is empty.</exception>
		/// <remarks>
		/// See the <see cref="Gorgon.IO.IGorgonChunkFileWriter.OpenChunk(ulong)"/> method for more information.
		/// </remarks>
		/// <seealso cref="Gorgon.IO.IGorgonChunkFileWriter.OpenChunk(ulong)"/>
		GorgonBinaryWriter OpenChunk(string chunkName);
		#endregion
	}
}