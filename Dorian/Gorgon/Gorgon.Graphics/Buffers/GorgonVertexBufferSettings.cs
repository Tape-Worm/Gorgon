using System;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Settings for a vertex buffer.
    /// </summary>
    public class GorgonVertexBufferSettings
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
        /// Initializes a new instance of the <see cref="GorgonVertexBufferSettings"/> class.
        /// </summary>
        public GorgonVertexBufferSettings()
        {
            SizeInBytes = 0;
            IsOutput = false;
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
        /// <remarks>This value will always return -1 because element size does not apply to vertex buffers.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
        int IShaderBufferSettings.ElementSize
        {
            get
            {
                return -1;
            }
            set 
            { 
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Property to set or return the number of an elements in a structured buffer or typed buffer.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        /// <remarks>This value wil always return -1 because element count does not apply to vertex buffers.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value is made.</exception>
        int IShaderBufferSettings.ElementCount
        {
            get
            {
                return -1;
            }
            set 
            { 
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Property to set or return the format of the view used when binding a vertex buffer to a shader.
        /// </summary>
        /// <remarks>Set this value to something other than Unknown if you wish to provide access to the resource from a shader and build a default shader view for the vertex buffer.
        /// <para>The default value is Unknown.</para>
        /// </remarks>
        public BufferFormat ShaderViewFormat
        {
            get;
            set;
        }
    }
}