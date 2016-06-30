using System;

namespace Gorgon.Graphics
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
		/// This value always returns <b>false</b> for constant buffers.
		/// </remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
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
            Usage = BufferUsage.Default;
		}

        /// <summary>
        /// Property to set or return whether to allow unordered access to the buffer.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
        /// <remarks>This value does not apply to constant buffers and will always return <b>false</b>.</remarks>
        bool IBufferSettings.AllowUnorderedAccessViews
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
        /// Property to set or return whether to allow shader resource views for this buffer.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
        /// <remarks>This value does not apply to constant buffers and will always return <b>false</b>.</remarks>
        bool IBufferSettings.AllowShaderViews
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
        /// Property to set or return the format for the default shader view.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
        /// <remarks>This value does not apply to constant buffers and will always return Unknown.</remarks>
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
        /// <remarks>This value does not apply to constant buffers and will always return <b>false</b>.</remarks>
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
        /// <remarks>
        /// This value does not apply to constant buffers and will always return <b>false</b>.
        /// </remarks>
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
        /// Property to set or return the size, in bytes, of an individual item in a structured buffer.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
        /// <remarks>
        /// This value is only applicable to a structured buffer.  This will always return 0.
        /// </remarks>
        int IBufferSettings.StructureSize
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}