#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, December 15, 2011 9:29:56 AM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.IO;
using SharpDX.D3DCompiler;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Shader types.
    /// </summary>
    public enum ShaderType
    {
        /// <summary>
        /// Vertex shader.
        /// </summary>
        Vertex = 0,
        /// <summary>
        /// Pixel shader.
        /// </summary>
        Pixel = 1,
        /// <summary>
        /// Geometry shader.
        /// </summary>
        Geometry = 2,
        /// <summary>
        /// Compute shader.
        /// </summary>
        Compute = 3,
        /// <summary>
        /// Domain shader.
        /// </summary>
        Domain = 4,
        /// <summary>
        /// Hull shader.
        /// </summary>
        Hull = 5
    }

    /// <summary>
    /// The base shader object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shaders are used to modify data on the GPU via a program uploaded to the GPU. This allows for a wide range of effects on GPU data. For example, a gaussian blur algorithm can be written using a pixel 
    /// shader to blur an image.
    /// </para>
    /// <para>
    /// In Gorgon, shaders can be compiled from a string containing source code via the <see cref="GorgonShaderFactory"/>, or loaded from a <see cref="Stream"/> or file for quicker access. The 
    /// <see cref="GorgonShaderFactory"/> is required to compile or read shaders, they cannot be created via the <c>new</c> keyword.
    /// </para>
    /// <para>
    /// All shaders in Gorgon will inherit from this type.
    /// </para>
    /// </remarks>
    public abstract class GorgonShader
        : GorgonNamedObject, IDisposable, IGorgonGraphicsObject
    {
        #region Variables.
        // The ID for the shaders.
        private static long _shaderID;
        // Flag to indicate whether the shader is compiled for debug.
        private bool _isDebug;
        // The D3D byte code for the shader.
        private ShaderBytecode _byteCode;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the shader byte code.
        /// </summary>
        internal ShaderBytecode D3DByteCode => _byteCode;

        /// <summary>
        /// Property to return the ID for the shader.
        /// </summary>
	    public long ID
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether to include debug information in the shader or not.
        /// </summary>
        public bool IsDebug => _isDebug;

        /// <summary>
        /// Property to return the type of shader.
        /// </summary>
        public abstract ShaderType ShaderType
        {
            get;
        }

        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to persist the shader data to a stream as a <see cref="GorgonChunkFile{T}"/>.
        /// </summary>
        /// <param name="stream">The stream to write the data into.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the stream is read only.</exception>
        /// <remarks>
        /// <para>
        /// This will write the shader data as <see cref="GorgonChunkFile{T}"/> formatted data into the supplied <paramref name="stream"/>. Shaders may take some time to compile, saving them to a binary 
        /// format in a stream will help cut down on the time it takes to initialize an application.
        /// </para>
        /// <para>
        /// This makes use of the Gorgon <see cref="GorgonChunkFile{T}"/> format to allow flexible storage of data. The Gorgon shader format is broken into 2 chunks, both of which are available in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderMetaData"/>, and <see cref="GorgonShaderFactory.BinaryShaderByteCode"/> constants. The file header for the format is stored in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderFileHeader"/> constant.  
        /// </para>
        /// <para>
        /// The file format is as follows:
        /// <list type="bullet">
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderFileHeader"/></term>
        ///			<description>This describes the type of file, and the version.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderMetaData"/></term>
        ///			<description>Shader metadata, such as the <see cref="Core.ShaderType"/> (<see cref="int"/>), debug flag (<see cref="bool"/>), and the entry point name (<see cref="string"/>) is stored here.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderByteCode"/></term>
        ///			<description>The compiled shader byte code is stored here and is loaded as a <see cref="byte"/> array.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonChunkFile{T}"/>
        /// <seealso cref="GorgonChunkFileReader"/>
        /// <seealso cref="GorgonChunkFileWriter"/>
        [Obsolete("Use Save(Stream) instead.")]
        public void SaveToStream(Stream stream) => Save(stream);

        /// <summary>
        /// Function to persist the shader data to a file as a <see cref="GorgonChunkFile{T}"/>.
        /// </summary>
        /// <param name="path">The path to the file where the shader data will eb written.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will write the shader data as <see cref="GorgonChunkFile{T}"/> formatted data into the file. Shaders may take some time to compile, saving them to a binary format in a stream will help cut 
        /// down on the time it takes to initialize an application.
        /// </para>
        /// <para>
        /// This makes use of the Gorgon <see cref="GorgonChunkFile{T}"/> format to allow flexible storage of data. The Gorgon shader format is broken into 2 chunks, both of which are available in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderMetaData"/>, and <see cref="GorgonShaderFactory.BinaryShaderByteCode"/> constants. The file header for the format is stored in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderFileHeader"/> constant.  
        /// </para>
        /// <para>
        /// The file format is as follows:
        /// <list type="bullet">
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderFileHeader"/></term>
        ///			<description>This describes the type of file, and the version.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderMetaData"/></term>
        ///			<description>Shader metadata, such as the <see cref="Core.ShaderType"/> (<see cref="int"/>), debug flag (<see cref="bool"/>), and the entry point name (<see cref="string"/>) is stored here.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderByteCode"/></term>
        ///			<description>The compiled shader byte code is stored here and is loaded as a <see cref="byte"/> array.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonChunkFile{T}"/>
        /// <seealso cref="GorgonChunkFileReader"/>
        /// <seealso cref="GorgonChunkFileWriter"/>
        [Obsolete("Use Save(string) instead.")]
        public void SaveToFile(string path) => Save(path);

        /// <summary>
        /// Function to persist the shader data to a stream as a <see cref="GorgonChunkFile{T}"/>.
        /// </summary>
        /// <param name="stream">The stream to write the data into.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the stream is read only.</exception>
        /// <remarks>
        /// <para>
        /// This will write the shader data as <see cref="GorgonChunkFile{T}"/> formatted data into the supplied <paramref name="stream"/>. Shaders may take some time to compile, saving them to a binary 
        /// format in a stream will help cut down on the time it takes to initialize an application.
        /// </para>
        /// <para>
        /// This makes use of the Gorgon <see cref="GorgonChunkFile{T}"/> format to allow flexible storage of data. The Gorgon shader format is broken into 2 chunks, both of which are available in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderMetaData"/>, and <see cref="GorgonShaderFactory.BinaryShaderByteCode"/> constants. The file header for the format is stored in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderFileHeader"/> constant.  
        /// </para>
        /// <para>
        /// The file format is as follows:
        /// <list type="bullet">
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderFileHeader"/></term>
        ///			<description>This describes the type of file, and the version.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderMetaData"/></term>
        ///			<description>Shader metadata, such as the <see cref="Core.ShaderType"/> (<see cref="int"/>), debug flag (<see cref="bool"/>), and the entry point name (<see cref="string"/>) is stored here.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderByteCode"/></term>
        ///			<description>The compiled shader byte code is stored here and is loaded as a <see cref="byte"/> array.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonChunkFile{T}"/>
        /// <seealso cref="GorgonChunkFileReader"/>
        /// <seealso cref="GorgonChunkFileWriter"/>
        public void Save(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY, nameof(stream));
            }

            var chunkFile = new GorgonChunkFileWriter(stream, GorgonShaderFactory.BinaryShaderFileHeader.ChunkID());

            try
            {
                GorgonBinaryWriter writer = chunkFile.OpenChunk(GorgonShaderFactory.BinaryShaderMetaData);
                ShaderType shaderType = ShaderType;
                writer.WriteValue(ref shaderType);
                writer.WriteValue(ref _isDebug);
                writer.Write(Name);
                chunkFile.CloseChunk();

                writer = chunkFile.OpenChunk(GorgonShaderFactory.BinaryShaderByteCode);
                writer.Write(D3DByteCode.Data);
            }
            finally
            {
                chunkFile.CloseChunk();
            }
        }

        /// <summary>
        /// Function to persist the shader data to a file as a <see cref="GorgonChunkFile{T}"/>.
        /// </summary>
        /// <param name="path">The path to the file where the shader data will eb written.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will write the shader data as <see cref="GorgonChunkFile{T}"/> formatted data into the file. Shaders may take some time to compile, saving them to a binary format in a stream will help cut 
        /// down on the time it takes to initialize an application.
        /// </para>
        /// <para>
        /// This makes use of the Gorgon <see cref="GorgonChunkFile{T}"/> format to allow flexible storage of data. The Gorgon shader format is broken into 2 chunks, both of which are available in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderMetaData"/>, and <see cref="GorgonShaderFactory.BinaryShaderByteCode"/> constants. The file header for the format is stored in the 
        /// <see cref="GorgonShaderFactory.BinaryShaderFileHeader"/> constant.  
        /// </para>
        /// <para>
        /// The file format is as follows:
        /// <list type="bullet">
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderFileHeader"/></term>
        ///			<description>This describes the type of file, and the version.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderMetaData"/></term>
        ///			<description>Shader metadata, such as the <see cref="Core.ShaderType"/> (<see cref="int"/>), debug flag (<see cref="bool"/>), and the entry point name (<see cref="string"/>) is stored here.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="GorgonShaderFactory.BinaryShaderByteCode"/></term>
        ///			<description>The compiled shader byte code is stored here and is loaded as a <see cref="byte"/> array.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonChunkFile{T}"/>
        /// <seealso cref="GorgonChunkFileReader"/>
        /// <seealso cref="GorgonChunkFileWriter"/>
        public void Save(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            ShaderBytecode byteCode = Interlocked.Exchange(ref _byteCode, null);

            if (byteCode == null)
            {
                this.UnregisterDisposable(Graphics);
                GC.SuppressFinalize(this);
                return;
            }

            Graphics.Log.Print("Destroying shader byte code.", LoggingLevel.Verbose);
            byteCode.Dispose();

            this.UnregisterDisposable(Graphics);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShader"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this object.</param>
        /// <param name="name">The name for this shader.</param>
        /// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
        /// <param name="byteCode">The byte code for the shader.</param>
        internal GorgonShader(GorgonGraphics graphics, string name, bool isDebug, ShaderBytecode byteCode)
            : base(name)
        {
            Graphics = graphics;
            ID = Interlocked.Increment(ref _shaderID);
            _isDebug = isDebug;
            _byteCode = byteCode;
        }
        #endregion
    }
}
