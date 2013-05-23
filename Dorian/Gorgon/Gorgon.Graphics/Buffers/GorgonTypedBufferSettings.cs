using System;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a typed buffer.
	/// </summary>
	/// <typeparam name="T">Type of data stored in the buffer.</typeparam>
	public sealed class GorgonTypedBufferSettings<T>
		: IShaderBufferSettings
		where T : struct
	{
		#region Variables.
		private readonly int _elementSize;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTypedBufferSettings{T}"/> class.
		/// </summary>
		public GorgonTypedBufferSettings()
		{
			IsOutput = false;
			Usage = BufferUsage.Default;
			ElementCount = 0;
			ShaderViewFormat = BufferFormat.Unknown;
			AllowUnorderedAccess = false;
			_elementSize = DirectAccess.SizeOf<T>();
		}
		#endregion

		#region IShaderBufferSettings Members
		/// <summary>
		/// Property to set or return the size of the buffer, in bytes.
		/// </summary>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
		int IBufferSettings.SizeInBytes
		{
			get
			{
				return ElementCount * _elementSize;
			}
			set
			{
                throw new NotSupportedException();
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
		/// <remarks>This value is read-only for typed buffers and will return the size, in bytes, of the type parameter.
		/// </remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
		public int ElementSize
		{
			get
			{
				return _elementSize;
			}
			set
			{
                throw new NotSupportedException();
			}
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
		/// <remarks>If this value is set to unknown, then no default shader view will be created for the buffer.  If no shader view exists for this buffer, 
		/// then it will be unable to be bound to a shader.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat ShaderViewFormat
		{
			get;
			set;
		}
		#endregion
	}
}