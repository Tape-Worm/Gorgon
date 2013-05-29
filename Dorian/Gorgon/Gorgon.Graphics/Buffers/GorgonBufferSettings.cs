using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a generic buffer.
	/// </summary>
	public class GorgonBufferSettings
		: IBufferSettings2, IBufferSettings
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
		    AllowUnorderedAccessViews = false;
		    AllowShaderViews = false;
		    AllowRenderTarget = false;
            DefaultShaderViewFormat = BufferFormat.Unknown;
		    AllowRawViews = false;
		    AllowIndirectArguments = false;
		}

        /// <summary>
        /// Property to set or return whether to allow unordered access to the buffer.
        /// </summary>
        /// <remarks>
        /// This value must be set to FALSE if <see cref="IsOutput" /> is set to TRUE.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowUnorderedAccessViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to allow shader resource views for this buffer.
        /// </summary>
        /// <remarks>
        /// This value does not apply to constant buffers.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowShaderViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to allow this buffer to be used as a render target.
        /// </summary>
        /// <remarks>
        /// This will allow the buffer to hold render target data.  As of right now, Gorgon does not have a way to bind a render target to the buffer objects.
        /// <para>This value does not apply to constant or structured buffers.</para>
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowRenderTarget
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format for the default shader view.
        /// </summary>
        /// <remarks>
        /// Setting this value to any other value than Unknown will create a default shader view for the buffer that will encompass the entire buffer with the specified format.
        /// <para>If <see cref="AllowRawViews" /> is set to TRUE, then this value should be set to one of: R32_Uint, R32_Int, R32_Float.</para>
        /// <para>This value does not apply to constant or structured buffers.</para>
        /// <para>The default value is Unknown.</para>
        /// </remarks>
        public BufferFormat DefaultShaderViewFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether a buffer will allow raw views.
        /// </summary>
        /// <remarks>
        /// This value must be set to FALSE if <see cref="AllowShaderViews" /> or <see cref="AllowUnorderedAccessViews" /> is set to FALSE.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowRawViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the buffer will be used as an indirect argument buffer.
        /// </summary>
        /// <remarks>
        /// This value does not apply to structured buffers or constant buffers.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowIndirectArguments
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size, in bytes, of an individual item in a structured buffer.
        /// </summary>
        /// <remarks>This value is only applicable to a structured buffer.  It will always return 0.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
        int IBufferSettings2.StructureSize
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