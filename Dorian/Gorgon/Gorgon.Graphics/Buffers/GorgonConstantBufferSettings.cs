namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for the constant buffer.
	/// </summary>
	public class GorgonConstantBufferSettings
		: IBufferSettings
	{
		/// <summary>
		/// Property to set or return the usage for the buffer.
		/// </summary>
		/// <para>This value will always return Default or Dynamic depending on the CPU access flag.</para>
		BufferUsage IBufferSettings.Usage
		{
			get
			{
				return AllowCPUWrite ? BufferUsage.Dynamic : BufferUsage.Default;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>
		/// This value always returns FALSE for constant buffers.
		/// </remarks>
		bool IBufferSettings.IsOutput
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return whether to allow writing from the CPU.
		/// </summary>
		/// <remarks>
		/// The default value is FALSE.
		/// </remarks>
		public bool AllowCPUWrite
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
		/// Initializes a new instance of the <see cref="GorgonConstantBufferSettings"/> class.
		/// </summary>
		public GorgonConstantBufferSettings()
		{
			SizeInBytes = 0;
			AllowCPUWrite = false;
		}
	}
}