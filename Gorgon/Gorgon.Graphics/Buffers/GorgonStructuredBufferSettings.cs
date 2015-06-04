using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for a structured buffer.
	/// </summary>
	/// <remarks>
	/// Structured buffers are only available on SM_5 video devices.
	/// </remarks>
	public sealed class GorgonStructuredBufferSettings
		: IBufferSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBufferSettings"/> class.
		/// </summary>
		public GorgonStructuredBufferSettings()
		{
			AllowUnorderedAccessViews = false;
			CreateDefaultShaderView = true;
			AllowShaderViews = true;
			Usage = BufferUsage.Default;
			SizeInBytes = 0;
			StructureSize = 0;
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
		/// <remarks>Unordered access views require a video device with SM5 capabilities.
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		public bool AllowUnorderedAccessViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow shader resource views for this buffer.
		/// </summary>
		/// <remarks>The default value is <b>true</b>.</remarks>
		public bool AllowShaderViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to create a default shader view for this buffer.
		/// </summary>
		/// <remarks>The default value is <b>true</b>.</remarks>
		public bool CreateDefaultShaderView
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format for the default shader view.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
		/// <remarks>This value does not apply to structured buffers will always return Unknown.</remarks>
		BufferFormat IBufferSettings.DefaultShaderViewFormat
		{
			get
			{
				return BufferFormat.Unknown;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return whether a buffer will allow raw views.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
		/// <remarks>This value does not apply to structured buffers will always return <b>false</b>.</remarks>
		bool IBufferSettings.AllowRawViews
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return whether the buffer will be used as an indirect argument buffer.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
		/// <remarks>This value does not apply to structured buffers will always return <b>false</b>.</remarks>
		bool IBufferSettings.AllowIndirectArguments
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
		/// <remarks>This value does not apply to structured buffers will always return <b>false</b>.</remarks>
		bool IBufferSettings.IsOutput
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the size of the buffer, in bytes.
		/// </summary>
		/// <remarks>The default value is 0.</remarks>
		public int SizeInBytes
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size, in bytes, of an individual item in a structured buffer.
		/// </summary>
		/// <remarks>This value must be between 1 and 2048 and be a multiple of 4.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int StructureSize
		{
			get;
			set;
		}
	}
}