using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for the index buffer.
	/// </summary>
	public class GorgonIndexBufferSettings
		: IShaderBufferSettings
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
		/// Property to set or return whether to allow the index buffer to create shader resource views or not.
		/// </summary>
		public bool UseShaderView
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

		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>
		/// Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>This value will always return 4 if the Use32BitIndices value is set to TRUE, 2 if set to FALSE.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
		public int ElementSize
		{
			get
			{
				return Use32BitIndices ? sizeof(uint) : sizeof(ushort);
			}
			set
			{
                throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
		public int ElementCount
		{
			get
			{
				return SizeInBytes / ((IShaderBufferSettings)this).ElementSize;
			}
			set
			{
                throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>
		/// This value will always return R32_UInt for 32 bit index buffers, and R16_UInt for 16 bit index buffers.
		/// </remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
		BufferFormat IShaderBufferSettings.ShaderViewFormat
		{
			get
			{
				return Use32BitIndices ? BufferFormat.R32_UInt : BufferFormat.R16_UInt;
			}
			set
			{
                throw new NotSupportedException();
			}
		}
	}
}