namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a structured buffer.
	/// </summary>
	/// <remarks>Structured buffers are only available on SM_5 video devices.</remarks>
	public sealed class GorgonStructuredBufferSettings
		: IShaderBufferSettings
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBufferSettings"/> class.
		/// </summary>
		public GorgonStructuredBufferSettings()
		{
			IsOutput = false;
			AllowUnorderedAccess = false;
			ElementCount = 0;
			ElementSize = 0;
			Usage = BufferUsage.Default;
		}
		#endregion

		#region IShaderBufferSettings Members
		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		/// <remarks>This value will return the Element Count multiplied by the Element Size.</remarks>
		int IBufferSettings.SizeInBytes
		{
			get
			{
				return ElementCount * ElementSize;
			}
			set
			{
			}
		}

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
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsOutput
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ElementSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>
		/// This value is only applicable on SM_5 video devices if used with a structured buffer. The value must be non-zero for all buffer types.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ElementCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>This value is not applicable to structured buffers.</remarks>
		BufferFormat IShaderBufferSettings.ShaderViewFormat
		{
			get;
			set;
		}
		#endregion
	}
}