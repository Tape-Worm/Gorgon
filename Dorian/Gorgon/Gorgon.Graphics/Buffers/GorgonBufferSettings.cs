namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a generic buffer.
	/// </summary>
	public class GorgonBufferSettings
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
		/// Initializes a new instance of the <see cref="GorgonBufferSettings"/> class.
		/// </summary>
		public GorgonBufferSettings()
		{
			SizeInBytes = 0;
			IsOutput = false;
			Usage = BufferUsage.Default;
		}
	}
}