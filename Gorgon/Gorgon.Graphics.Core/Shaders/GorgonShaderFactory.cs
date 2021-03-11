#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 8, 2016 11:26:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.IO;
using Gorgon.Reflection;
using SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A factory used to create various <see cref="GorgonShader"/> based types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This factory is used to compile from source code or load shaders from a stream or file. Shaders such as the <see cref="GorgonPixelShader"/> cannot be created by using the <c>new</c> keyword and 
    /// must be instantiated via this factory.
    /// </para>
    /// </remarks>
    public static class GorgonShaderFactory
    {
        #region Constants.
        /// <summary>
        /// The header chunk for a Gorgon binary shader file.
        /// </summary>
        public const string BinaryShaderFileHeader = "GORSHD20";

        /// <summary>
        /// The chunk ID for the chunk that contains metadata for the shader.
        /// </summary>
        public const string BinaryShaderMetaData = "METADATA";

        /// <summary>
        /// The chunk ID for the chunk that contains the binary shader bytecode.
        /// </summary>
        public const string BinaryShaderByteCode = "BYTECODE";
        #endregion

        #region Variables.
        // A processor used to parse shader source code for include statements.
        private static readonly ShaderProcessor _processor = new();

        // A list of available shader types.
        private static readonly (Type, ShaderType, string)[] _shaderTypes =
        {
            (typeof(GorgonVertexShader), ShaderType.Vertex, "vs_5_0"),
            (typeof(GorgonPixelShader), ShaderType.Pixel, "ps_5_0"),
            (typeof(GorgonGeometryShader), ShaderType.Geometry, "gs_5_0"),
            (typeof(GorgonHullShader), ShaderType.Hull, "hs_5_0"),
            (typeof(GorgonDomainShader), ShaderType.Domain, "ds_5_0"),
            (typeof(GorgonComputeShader), ShaderType.Compute, "cs_5_0")
        };

        // A list of factory objects used to create the actual shader objects.
        private static readonly Dictionary<ShaderType, ObjectActivator<GorgonShader>> _shaderFactory =
            new();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of <see cref="GorgonShaderInclude"/> definitions to include with compiled shaders.
        /// </summary>
        /// <remarks>
        /// Gorgon uses a special keyword in shaders to allow shader files to include other files as part of the source. This keyword is named <c>#GorgonInclude</c> and is similar to the HLSL 
        /// <c>#include</c> keyword. The difference is that this keyword allows users to include shader source from memory instead of a separate source file. This is done by assigning a name to the included 
        /// source code in the <c>#GorgonInclude</c> keyword, and adding the string containing the source to this property. When the include is loaded from a file, then it will automatically be added to 
        /// this property.
        /// </remarks>
        /// <seealso cref="GorgonShaderInclude"/>
        public static IDictionary<string, GorgonShaderInclude> Includes => _processor.CachedIncludes;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load a shader from a <see cref="Stream"/> containing a Gorgon binary shader in chunked format.
        /// </summary>
        /// <typeparam name="T">The type of shader to return. Must inherit from <see cref="GorgonShader"/>.</typeparam>
        /// <param name="graphics">The graphics interface used to create the shader.</param>
        /// <param name="stream">The stream containing the binary shader data.</param>
        /// <param name="size">The size, in bytes, of the binary shader within the stream.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if an attempt to read beyond the end of the stream is made.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1 byte.</exception>
        /// <exception cref="InvalidCastException">Thrown if the application tries to load a specific shader type as <typeparamref name="T"/>, but it is stored as another type.</exception>
        /// <returns>A new <see cref="GorgonShader"/>. The type of shader depends on the data that was written to the stream and the <typeparamref name="T"/> type parameter.</returns>
        /// <remarks>
        /// <para>
        /// Use this to load precompiled shaders from a stream. This has the advantage of not needing a lengthy recompile of the shader when initializing.
        /// </para>
        /// <para>
        /// This makes use of the Gorgon <see cref="GorgonChunkFile{T}"/> format to allow flexible storage of data. The Gorgon shader format is broken into 2 chunks, both of which are available in the 
        /// <see cref="BinaryShaderMetaData"/>, and <see cref="BinaryShaderByteCode"/> constants. The file header for the format is stored in the <see cref="BinaryShaderFileHeader"/> constant.  
        /// </para>
        /// <para>
        /// The file format is as follows:
        /// <list type="bullet">
        ///		<item>
        ///			<term><see cref="BinaryShaderFileHeader"/></term>
        ///			<description>This describes the type of file, and the version.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="BinaryShaderMetaData"/></term>
        ///			<description>Shader metadata, such as the <see cref="ShaderType"/> (<see cref="int"/>), debug flag (<see cref="bool"/>), and the entry point name (<see cref="string"/>) is stored here.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="BinaryShaderByteCode"/></term>
        ///			<description>The compiled shader byte code is stored here and is loaded as a <see cref="byte"/> array.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonChunkFile{T}"/>
        /// <seealso cref="GorgonChunkFileReader"/>
        /// <seealso cref="GorgonChunkFileWriter"/>
        public static T FromStream<T>(GorgonGraphics graphics, Stream stream, int size)
            where T : GorgonShader
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
            }

            if (stream.Position >= stream.Length)
            {
                throw new EndOfStreamException();
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            // We will store the shader as a Gorgon chunked binary format. 
            // This will break the shader into parts within the file to allow us the ability to read portions of the file.
            var chunkReader = new GorgonChunkFileReader(stream,
                                                                          new[]
                                                                          {
                                                                              BinaryShaderFileHeader.ChunkID()
                                                                          });

            ShaderBytecode byteCode = null;

            try
            {
                // When we open, the file header will be validated via the app specific ID passed to the chunk reader constructor above.
                chunkReader.Open();

                if ((!chunkReader.Chunks.Contains(BinaryShaderMetaData))
                    || (!chunkReader.Chunks.Contains(BinaryShaderByteCode)))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_NOT_GORGON_SHADER);
                }

                GorgonChunk metaDataChunk = chunkReader.Chunks[BinaryShaderMetaData];
                GorgonChunk byteCodeChunk = chunkReader.Chunks[BinaryShaderByteCode];

                // Ensure the metadata and bytecode are the correct size, or have data.
                // The metadata should have at least 2 bytes at the end for the entry point (string length + a single character).
                if ((metaDataChunk.Size < sizeof(ShaderType) + sizeof(bool) + 2)
                    || (byteCodeChunk.Size < 1))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_NOT_GORGON_SHADER);
                }

                // Get the meta data.
                GorgonBinaryReader reader = chunkReader.OpenChunk(metaDataChunk.ID);

                ShaderType shaderType = reader.ReadValue<ShaderType>();
                bool debug = reader.ReadBoolean();
                string entryPoint = reader.ReadString();

                if ((!Enum.IsDefined(typeof(ShaderType), shaderType))
                    || (string.IsNullOrWhiteSpace(entryPoint)))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_NOT_GORGON_SHADER);
                }

                chunkReader.CloseChunk();

                Type requestedType = typeof(T);

                // Validate the type for the shader.
                // Just in case we try to cast a pixel shader to a compute shader or some such nonsense.
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _shaderTypes.Length; ++i)
                {
                    (Type ShaderObjectType, ShaderType Type, string) shaderTypeXref = _shaderTypes[i];

                    if (shaderTypeXref.Type != shaderType)
                    {
                        continue;
                    }

                    if ((requestedType != shaderTypeXref.ShaderObjectType)
                        && (!shaderTypeXref.ShaderObjectType.IsSubclassOf(requestedType)))
                    {
                        throw new InvalidCastException(string.Format(Resources.GORGFX_ERR_SHADER_TYPE_MISMATCH, shaderType, requestedType.FullName));
                    }

                    break;
                }

                graphics.Log.Print($"Reading {shaderType} '{entryPoint}' from stream.", LoggingLevel.Simple);

                reader = chunkReader.OpenChunk(byteCodeChunk.ID);

                byte[] data = new byte[byteCodeChunk.Size];

                reader.Read(data, 0, byteCodeChunk.Size);

                graphics.Log.Print($"Compiling {shaderType} '{entryPoint}'.", LoggingLevel.Simple);
                byteCode = new ShaderBytecode(data);

                ObjectActivator<GorgonShader> shaderCtor = _shaderFactory[shaderType];

                var result = (T)shaderCtor(graphics.D3DDevice,
                                           entryPoint,
                                           debug,
                                           byteCode);
                result.RegisterDisposable(graphics);
                return result;
            }
            catch (DX.CompilationException cEx)
            {
                throw new GorgonException(GorgonResult.CannotCompile,
                                          string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, cEx.Message.Replace("{", "{{").Replace("}", "}}")));
            }
            finally
            {
                chunkReader.Close();
                byteCode?.Dispose();
            }
        }

        /// <summary>
        /// Function to load a shader from a file containing a Gorgon binary shader in chunked format.
        /// </summary>
        /// <typeparam name="T">The type of shader to return. Must inherit from <see cref="GorgonShader"/>.</typeparam>
        /// <param name="graphics">The graphics interface used to create the shader.</param>
        /// <param name="path">The path to the file containing the Gorgon binary shader data.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="InvalidCastException">Thrown if the application tries to load a specific shader type as <typeparamref name="T"/>, but it is stored as another type.</exception>
        /// <returns>A new <see cref="GorgonShader"/>. The type of shader depends on the data that was written to the file and the <typeparamref name="T"/> type parameter.</returns>
        /// <remarks>
        /// <para>
        /// Use this to load precompiled shaders from a file. This has the advantage of not needing a lengthy recompile of the shader when initializing.
        /// </para>
        /// <para>
        /// This makes use of the Gorgon <see cref="GorgonChunkFile{T}"/> format to allow flexible storage of data. The Gorgon shader format is broken into 2 chunks, both of which are available in the 
        /// <see cref="BinaryShaderMetaData"/>, and <see cref="BinaryShaderByteCode"/> constants. The file header for the format is stored in the <see cref="BinaryShaderFileHeader"/> constant.  
        /// </para>
        /// <para>
        /// The file format is as follows:
        /// <list type="bullet">
        ///		<item>
        ///			<term><see cref="BinaryShaderFileHeader"/></term>
        ///			<description>This describes the type of file, and the version.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="BinaryShaderMetaData"/></term>
        ///			<description>Shader metadata, such as the <see cref="ShaderType"/> (<see cref="int"/>), debug flag (<see cref="bool"/>), and the entry point name (<see cref="string"/>) is stored here.</description>
        ///		</item>
        ///		<item>
        ///			<term><see cref="BinaryShaderByteCode"/></term>
        ///			<description>The compiled shader byte code is stored here and is loaded as a <see cref="byte"/> array.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonChunkFile{T}"/>
        /// <seealso cref="GorgonChunkFileReader"/>
        /// <seealso cref="GorgonChunkFileWriter"/>
        public static T FromFile<T>(GorgonGraphics graphics, string path)
            where T : GorgonShader
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream<T>(graphics, stream, (int)stream.Length);
        }

        /// <summary>
        /// Function to compile a shader from source code.
        /// </summary>
        /// <typeparam name="T">The type of shader to return.</typeparam>
        /// <param name="graphics">The graphics interface to use.</param>
        /// <param name="sourceCode">A string containing the source code to compile.</param>
        /// <param name="entryPoint">The entry point function for the shader.</param>
        /// <param name="debug">[Optional] <b>true</b> to indicate whether the shader should be compiled with debug information; otherwise, <b>false</b> to strip out debug information.</param>
        /// <param name="macros">[Optional] A list of macros for conditional compilation.</param>
        /// <param name="sourceFileName">[Optional] The name of the file where the source code came from.</param>
        /// <returns>A byte array containing the compiled shader.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceCode"/>, <paramref name="entryPoint"/> or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourceCode"/>, <paramref name="entryPoint"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the type specified by <typeparamref name="T"/> is not a valid shader type.
        /// <para>-or-</para>
        /// <para>Thrown when there was an error during compilation of the shader.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will compile a shader from a <see cref="string"/> containing the source code for requested the shader. Applications can load a string from a text file, use an internal string or use a string 
        /// resource and pass it to this method to produce shader byte code that can then be used with the shader interfaces.
        /// </para>
        /// <para>
        /// When the <paramref name="debug"/> flag is set to <b>true</b>, optimizations are turned off for the shader, and debugging information is injected into the shader to allow for debugging with Visual 
        /// Studio's graphics debugging tools, or a tool like <a target="_blank" href="https://github.com/baldurk/renderdoc">RenderDoc</a> (highly recommended). When compiling the shader with debugging 
        /// information, it is prudent to pass the full path to the file that contains the shader source in the <paramref name="sourceFileName"/>. If the shader resides in memory (as a string or resource), 
        /// then the parameter may be omitted if the user wishes.
        /// </para>
        /// <para>
        /// For some shaders, conditional compilation allows users to create many variations of shaders from the same source code. This is not unlike the <c>#define</c> conditional code in C#. To pass a list 
        /// of these macros, just pass a list of <see cref="GorgonShaderMacro"/> objects to the <paramref name="macros"/> parameter to define which parts of the code will be compiled.
        /// </para>
        /// <para>
        /// Finally, Gorgon uses a special keyword in shaders to allow shader files to include other files as part of the source. This keyword is named <c>#GorgonInclude</c> and is similar to the HLSL 
        /// <c>#include</c> keyword. The difference is that this keyword allows users to include shader source from memory instead of a separate source file. This is done by assigning a name to the included 
        /// source code in the <c>#GorgonInclude</c> keyword, and adding the <see cref="GorgonShaderInclude"/> containing the source to the <see cref="Includes"/> property on this class. When the include 
        /// is loaded from a file, then it will automatically be added to the <see cref="Includes"/> property.
        /// </para>
        /// <para>
        /// The parameters for the <c>#GorgonInclude</c>keyword are: 
        /// <list type="bullet">
        ///		<item>
        ///			<term>Name</term>
        ///			<description>The path name of the included source. This must be unique, and assigned to the <see cref="Includes"/> property.</description>
        ///		</item>
        ///		<item>
        ///			<term>(Optional) Path</term>
        ///			<description>The path to the shader source file to include. This may be omitted if the include is assigned from memory in the <see cref="Includes"/> property.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static T Compile<T>(GorgonGraphics graphics,
                                   string sourceCode,
                                   string entryPoint,
                                   bool debug = false,
                                   IList<GorgonShaderMacro> macros = null,
                                   string sourceFileName = "(in memory)")
            where T : GorgonShader
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (sourceCode is null)
            {
                throw new ArgumentNullException(nameof(sourceCode));
            }

            if (entryPoint is null)
            {
                throw new ArgumentNullException(nameof(entryPoint));
            }

            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentEmptyException(nameof(sourceCode));
            }

            if (string.IsNullOrWhiteSpace(entryPoint))
            {
                throw new ArgumentEmptyException(nameof(entryPoint));
            }

            (Type Type, ShaderType Shader, string Profile)? shaderType = _shaderTypes.FirstOrDefault(item => item.Item1 == typeof(T));

            if (shaderType is null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, typeof(T).FullName));
            }

            // Get shader flags based on our feature set and whether we want debug info or not.
            ShaderFlags flags = debug ? ShaderFlags.Debug : ShaderFlags.OptimizationLevel3;

            // Make compatible macros for the shader compiler.
            D3D.ShaderMacro[] actualMacros = null;

            if ((macros is not null) && (macros.Count > 0))
            {
                actualMacros = macros.Select(item => item.D3DShaderMacro).ToArray();
            }

            string processedSource = _processor.Process(sourceCode);

            try
            {
                CompilationResult byteCode = ShaderBytecode.Compile(processedSource,
                                                                    entryPoint,
                                                                    shaderType.Value.Profile,
                                                                    flags,
                                                                    EffectFlags.None,
                                                                    actualMacros,
                                                                    null,
                                                                    sourceFileName);

                if ((byteCode.HasErrors) || (byteCode.Bytecode is null))
                {
                    throw new GorgonException(GorgonResult.CannotCompile,
                                              string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER,
                                                            byteCode.Message.Replace("{", "{{").Replace("}", "}}")));
                }

                ObjectActivator<GorgonShader> shaderCtor = _shaderFactory[shaderType.Value.Shader];

                var result = (T)shaderCtor(graphics,
                                           entryPoint,
                                           debug,
                                           byteCode.Bytecode);
                result.RegisterDisposable(graphics);

                return result;
            }
            catch (DX.CompilationException cEx)
            {
                throw new GorgonException(GorgonResult.CannotCompile,
                                          string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, cEx.Message.Replace("{", "{{").Replace("}", "}}")));
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes static members of the <see cref="GorgonShaderFactory" /> class.
        /// </summary>
        static GorgonShaderFactory()
        {                        
            foreach ((Type Type, ShaderType Shader, _) in _shaderTypes)
            {
                _shaderFactory[Shader] = Type.CreateActivator<GorgonShader>(typeof(GorgonGraphics),
                                                                            typeof(string),
                                                                            typeof(bool),
                                                                            typeof(ShaderBytecode));
            }
        }
        #endregion
    }
}
