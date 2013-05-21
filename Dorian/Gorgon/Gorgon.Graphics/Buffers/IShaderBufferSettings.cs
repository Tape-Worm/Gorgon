namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a buffer.
	/// </summary>
	public interface IShaderBufferSettings
		: IBufferSettings
	{
		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>This value is only applicable on SM_5 video devices and must be non-zero for a structured buffer.
		/// <para>The default value is 0.</para>
		/// </remarks>
		int ElementSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>This value is only applicable on SM_5 video devices if used with a structured buffer. The value must be non-zero for all buffer types.
		/// <para>The default value is 0.</para>
		/// </remarks>
		int ElementCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>This value must be set to Unknown for structured buffers.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		BufferFormat ShaderViewFormat
		{
			get;
			set;
		}
	}
}