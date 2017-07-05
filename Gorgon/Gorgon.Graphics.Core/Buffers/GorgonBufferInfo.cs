using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides the necessary information required to set up a generic unstructured buffer.
    /// </summary>
    public class GorgonBufferInfo
        : IGorgonBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the intended usage for binding to the GPU.
        /// </summary>
        public D3D11.ResourceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </para>
        /// <para>
        /// This value should also be a multiple of 16.
        /// </para>
        /// </remarks>
        public int SizeInBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        /// <remarks>
        /// The type of binding should be used to determine what type of view to apply to the buffer when accessing it from shaders. This will also help determine how data will be interpreted.
        /// </remarks>
        public BufferBinding Binding
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        /// <param name="bufferInfo">The buffer information to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bufferInfo"/> parameter is <b>null</b>.</exception>
        public GorgonBufferInfo(IGorgonBufferInfo bufferInfo)
        {
            if (bufferInfo == null)
            {
                throw new ArgumentNullException(nameof(bufferInfo));
            }

            Usage = bufferInfo.Usage;
            SizeInBytes = bufferInfo.SizeInBytes;
            Binding = bufferInfo.Binding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        public GorgonBufferInfo()
        {
            Usage = D3D11.ResourceUsage.Default;
            Binding = BufferBinding.Shader;
        }
        #endregion
    }
}
