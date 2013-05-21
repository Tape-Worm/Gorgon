namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for the index buffer.
	/// </summary>
	public class GorgonIndexBufferSettings
		: IBufferSettings
	{
		/// <summary>
		/// Property to set or return the usage for the buffer.
		/// </summary>
		/// <para>The default value is Default.</para>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>
		/// The default value is FALSE.
		/// </remarks>
		public bool IsOutput
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the buffer, in bytes.
		/// </summary>
		/// <remarks>
		/// This value must be a multiple of 16.
		/// </remarks>
		public int SizeInBytes
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the indices are 32 bits in size or not.
		/// </summary>
		/// <remarks>Pass TRUE to use 32 bit indices, FALSE to use 16 bit indices.
		/// <para>The default value is TRUE.</para>
		/// </remarks>
		public bool Use32BitIndices
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonIndexBufferSettings"/> class.
		/// </summary>
		public GorgonIndexBufferSettings()
		{
			SizeInBytes = 0;
			IsOutput = false;
			Use32BitIndices = true;
			Usage = BufferUsage.Default;
		}
	}
}