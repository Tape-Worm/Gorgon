namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a typed buffer.
	/// </summary>
	/// <remarks>Raw buffers are only available on SM5 video devices.</remarks>
	public sealed class GorgonRawBufferSettings
		: IShaderBufferSettings
    {
        #region Constructor/Destructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawBufferSettings"/> class.
		/// </summary>
		public GorgonRawBufferSettings()
		{
			IsOutput = false;
			Usage = BufferUsage.Default;
			AllowUnorderedAccess = false;
		}
		#endregion

		#region IShaderBufferSettings Members
		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		public int SizeInBytes
		{
			get;
			set;
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
		/// <remarks>This value always returns 4.</remarks>
		public int ElementSize
		{
			get
			{
				return 4;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>This value returns the number of 4 byte values in the buffer (SizeInBytes / 4).</remarks>
		public int ElementCount
		{
			get
			{
			    return SizeInBytes / 4;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>This value will always return R32.</remarks>
		BufferFormat IShaderBufferSettings.ShaderViewFormat
		{
			get
			{
				return BufferFormat.R32;
			}
			set
			{
			}
		}
		#endregion
	}
}