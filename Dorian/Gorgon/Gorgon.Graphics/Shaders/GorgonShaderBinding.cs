#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, January 31, 2012 8:21:21 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using GorgonLibrary.Native;
using Shaders = SharpDX.D3DCompiler;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Used to manage shader bindings and shader buffers.
	/// </summary>
	public sealed class GorgonShaderBinding
	{
		#region Constants.
		/// <summary>
		/// Header for Gorgon binary shaders.
		/// </summary>
		internal const string BinaryShaderHeader = "GORBINSHD2.0";
		#endregion

		#region Variables.
		private GorgonGraphics _graphics;		// Graphics interface.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current vertex shader states.
		/// </summary>
		public GorgonVertexShaderState VertexShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current vertex shader states.
		/// </summary>
		public GorgonPixelShaderState PixelShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of include files for the shaders.
		/// </summary>
		public GorgonShaderIncludeCollection IncludeFiles
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function clean up any resources within this interface.
		/// </summary>
		internal void CleanUp()
		{
			if (PixelShader != null)
				PixelShader.Dispose();
			if (VertexShader != null)
				VertexShader.Dispose();

			PixelShader = null;
			VertexShader = null;
		}

		/// <summary>
		/// Function to re-seat a shader after it's been altered.
		/// </summary>
		/// <param name="shader">Shader to re-seat.</param>
		internal void Reseat(GorgonShader shader)
		{
			if (shader is GorgonPixelShader)
			{
				if (PixelShader.Current == shader)
				{
					PixelShader.Current = null;
					PixelShader.Current = (GorgonPixelShader)shader;
				}
			}

			if (shader is GorgonVertexShader)
			{
				if (VertexShader.Current == shader)
				{
					VertexShader.Current = null;
					VertexShader.Current = (GorgonVertexShader)shader;
				}
			}
		}

		/// <summary>
		/// Function to re-seat a shader resource after it's been altered.
		/// </summary>
		/// <param name="resource">Shader resource to re-seat.</param>
		internal void Reseat(GorgonResource resource)
		{
			PixelShader.Resources.ReSeat(resource);
			VertexShader.Resources.ReSeat(resource);
		}

		/// <summary>
		/// Function to unbind a shader resource from all shaders.
		/// </summary>
		/// <param name="resource">Shader resource to unbind.</param>
		internal void Unbind(GorgonResource resource)
		{
			PixelShader.Resources.Unbind(resource);
			VertexShader.Resources.Unbind(resource);
		}

		/// <summary>
		/// Function to create an effect object.
		/// </summary>
		/// <typeparam name="T">Type of effect to create.</typeparam>
		/// <param name="name">Name of the effect.</param>
		/// <param name="parameters">Parameters to pass to the shader.</param>
		/// <returns>The new effect object.</returns>
		/// <remarks>Effects are used to simplify rendering with multiple passes when using a shader, similar to the old Direct 3D effects framework.
		/// <para>The <paramref name="parameters"/> parameter is optional, however some effects may require a specific set of parameters passed upon creation. 
		/// This is dependent on the effect and may thrown an exception if a parameter is missing.  Parameter names are case sensitive.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the parameter list does not contain a required parameter.</para>
		/// </exception>
		public T CreateEffect<T>(string name, params KeyValuePair<string, object>[] parameters)
			where T : GorgonEffect
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not empty.", "name");
            }

			var effect = (T)Activator.CreateInstance(typeof(T), new object[] {_graphics, name});

			if ((effect.RequiredParameters.Count > 0) && ((parameters == null) || (parameters.Length == 0)))
			{
				throw new ArgumentException("There are required parameters for the effect, but none were passed to the effect.", "parameters");
			}

			if ((parameters != null) && (parameters.Length > 0))
			{
				// Only get parameters where the key name has a value.
				var validParameters = parameters.Where(item => !string.IsNullOrWhiteSpace(item.Key));

				// Check for predefined required parameters from the effect.
				foreach (var effectParam in effect.RequiredParameters)
				{					
					if (!validParameters.Any(item => item.Key == effectParam))
					{
						throw new ArgumentException("The required parameter '" + effectParam + "' was not found in the parameter list.", "parameters");
					}
				}

				// Add/update the parameters.
				foreach (var param in validParameters)
				{
					effect.Parameters[param.Key] = param.Value;
				}
			}

			// Initialize our effect parameters.
			effect.InitializeEffectParameters();

			_graphics.AddTrackedObject(effect);

			return effect;
		}

		/// <summary>
		/// Function to create an effect object.
		/// </summary>
		/// <typeparam name="T">Type of effect to create.</typeparam>
		/// <param name="name">Name of the effect.</param>
		/// <returns>The new effect object.</returns>
		/// <remarks>Effects are used to simplify rendering with multiple passes when using a shader, similar to the old Direct 3D effects framework.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public T CreateEffect<T>(string name)
			where T : GorgonEffect
		{
			return CreateEffect<T>(name, null);
		}

		/// <summary>
		/// Function to create a constant buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer(int size, bool allowCPUWrite)
		{
			return CreateConstantBuffer(size, allowCPUWrite, null);
		}

		/// <summary>
		/// Function to create a constant buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <param name="stream">Stream used to initialize the buffer.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer(int size, bool allowCPUWrite, GorgonDataStream stream)
		{			
			var buffer = new GorgonConstantBuffer(_graphics, size, allowCPUWrite);
			buffer.Initialize(stream);
			_graphics.AddTrackedObject(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create a constant buffer and initializes it with data.
		/// </summary>
		/// <typeparam name="T">Type of data to pass to the constant buffer.</typeparam>
		/// <param name="value">Value to write to the buffer</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer<T>(T value, bool allowCPUWrite)
			where T : struct
		{
			using (GorgonDataStream stream = GorgonDataStream.ValueToStream<T>(value))
			{
				var buffer = new GorgonConstantBuffer(_graphics, (int)stream.Length, allowCPUWrite);
				buffer.Initialize(stream);

				_graphics.AddTrackedObject(buffer);
				return buffer;
			}
		}

		/// <summary>
		/// Function to create a structured buffer and initialize it with data.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new structured buffer.</returns>
		/// <typeparam name="T">Type of data to write.  Must be a value type.</typeparam>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonStructuredBuffer CreateStructuredBuffer<T>(T[] value, bool allowCPUWrite)
			where T : struct
		{
			GorgonDebug.AssertNull<T[]>(value, "value");
						
			using (GorgonDataStream stream = GorgonDataStream.ArrayToStream<T>(value))
			{
				return CreateStructuredBuffer(value.Count(), DirectAccess.SizeOf<T>(), allowCPUWrite);
			}
		}

		/// <summary>
		/// Function to create a structured buffer and initialize it with data.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new structured buffer.</returns>
		/// <typeparam name="T">Type of data to write.  Must be a value type.</typeparam>
		public GorgonStructuredBuffer CreateStructuredBuffer<T>(T value, bool allowCPUWrite)
			where T : struct
		{
			using (GorgonDataStream stream = GorgonDataStream.ValueToStream<T>(value))
			{
				return CreateStructuredBuffer(1, DirectAccess.SizeOf<T>(), allowCPUWrite);
			}			
		}

		/// <summary>
		/// Function to create a structured buffer and initialize it with data.
		/// </summary>
		/// <param name="elementCount">Number of elements that the buffer will contain.</param>
		/// <param name="elementSize">Size of an element, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to have write access to the buffer, FALSE to disallow.</param>
		/// <returns>A new structured buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="elementCount"/> or <paramref name="elementSize"/> parameters are not greater than 0.
		/// </exception>
		public GorgonStructuredBuffer CreateStructuredBuffer(int elementCount, int elementSize, bool allowCPUWrite)
		{
			return CreateStructuredBuffer(elementCount, elementSize, allowCPUWrite, null);
		}

		/// <summary>
		/// Function to create a structured buffer and initialize it with data.
		/// </summary>
		/// <param name="elementCount">Number of elements that the buffer will contain.</param>
		/// <param name="elementSize">Size of an element, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to have write access to the buffer, FALSE to disallow.</param>
		/// <param name="stream">Stream containing the data used to initialize the buffer.</param>
		/// <returns>A new structured buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="elementCount"/> or <paramref name="elementSize"/> parameters are not greater than 0.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when an attempt to create a structured buffer is made on a video device that does not support SM5 or better.</exception>
		public GorgonStructuredBuffer CreateStructuredBuffer(int elementCount, int elementSize, bool allowCPUWrite, GorgonDataStream stream)
		{
			GorgonStructuredBuffer result = null;

#if DEBUG
			if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
				throw new InvalidOperationException("Structured buffers are only available for video devices that support SM5 or better.");

			if (elementCount <= 0)
				throw new ArgumentException("The element count must be greater than 0.", "settings");

			if (elementSize <= 0)
				throw new ArgumentException("The element size must be greater than 0.", "settings");
#endif

			result = new GorgonStructuredBuffer(_graphics, elementCount, elementSize, false, allowCPUWrite);
			result.Initialize(stream);

			_graphics.AddTrackedObject(result);

			return result;
		}

        /// <summary>
        /// Function to load a shader from a byte array.
        /// </summary>
        /// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
        /// <param name="name">Name of the shader object.</param>
        /// <param name="entryPoint">Entry point method to call in the shader.</param>
        /// <param name="shaderData">Array of bytes containing the shader data.</param>
        /// <returns>The new shader loaded from the data stream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shaderData"/>, <paramref name="name"/> or <paramref name="entryPoint"/> parameters are NULL (Nothing in VB.Net).
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the name or entryPoint parameters are empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the shaderData parameter length is less than or equal to 0.</exception>
        /// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
        public T FromMemory<T>(string name, string entryPoint, byte[] shaderData)
            where T : GorgonShader
        {
#if DEBUG
            return FromMemory<T>(name, entryPoint, shaderData, true);
#else
            return FromMemory<T>(name, entryPoint, shaderData, false);
#endif
        }
        
        /// <summary>
		/// Function to load a shader from a byte array.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="shaderData">Array of bytes containing the shader data.</param>
        /// <param name="isDebug">TRUE to apply debug information, FALSE to exclude it.</param>
        /// <returns>The new shader loaded from the data stream.</returns>
		/// <remarks>The <paramref name="isDebug"/> parameter is only applicable to source code shaders.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shaderData"/>, <paramref name="name"/> or <paramref name="entryPoint"/> parameters are NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or entryPoint parameters are empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the shaderData parameter length is less than or equal to 0.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
        public T FromMemory<T>(string name, string entryPoint, byte[] shaderData, bool isDebug)
            where T : GorgonShader
        {
            if (shaderData == null)
            {
                throw new ArgumentNullException("shaderData");
            }

            using (var memoryStream = new GorgonDataStream(shaderData))
            {
                return FromStream<T>(name, entryPoint, memoryStream, shaderData.Length, isDebug);
            }
        }

		/// <summary>
		/// Function to load a shader from a stream of data.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="stream">Stream to load the shader from.</param>
		/// <param name="size">Size of the shader, in bytes.</param>
		/// <returns>The new shader loaded from the data stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/>, <paramref name="name"/> or <paramref name="entryPoint"/> parameters are NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or entryPoint parameters are empty.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than or equal to 0.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
		public T FromStream<T>(string name, string entryPoint, Stream stream, int size)
			where T : GorgonShader
		{
#if DEBUG
			return FromStream<T>(name, entryPoint, stream, size, true);
#else
			return FromStream<T>(name, entryPoint, stream, size, false);
#endif
		}

		/// <summary>
		/// Function to load a shader from a stream of data.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="stream">Stream to load the shader from.</param>
		/// <param name="size">Size of the shader, in bytes.</param>
        /// <param name="isDebug">TRUE to apply debug information, FALSE to exclude it.</param>
        /// <returns>The new shader loaded from the data stream.</returns>
		/// <remarks>The <paramref name="isDebug"/> parameter is only applicable to source code shaders.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/>, <paramref name="name"/> or <paramref name="entryPoint"/> parameters are NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or entryPoint parameters are empty.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than or equal to 0.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
        public T FromStream<T>(string name, string entryPoint, Stream stream, int size, bool isDebug)
			where T : GorgonShader
		{
			bool isBinary = false;
			GorgonShader shader = null;
			string sourceCode = string.Empty;
			byte[] shaderData = null;
			byte[] header = null;
			long streamPosition = 0;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }

            if (stream.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("size", "The parameter must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace("name"))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (string.IsNullOrWhiteSpace("entryPoint"))
            {
                throw new ArgumentException("The parameter must not be empty.", "entryPoint");
            }           
			
			streamPosition = stream.Position;

			// Check for the binary header.  If we have it, load the file as a binary file.
			// Otherwise load it as source code.
			header = new byte[Encoding.UTF8.GetBytes(BinaryShaderHeader).Length];
			stream.Read(header, 0, header.Length);
			isBinary = (string.Compare(Encoding.UTF8.GetString(header), BinaryShaderHeader, true) == 0);
			if (isBinary)
				shaderData = new byte[size - BinaryShaderHeader.Length];
			else
			{
				stream.Position = streamPosition;
				shaderData = new byte[size];
			}

			stream.Read(shaderData, 0, shaderData.Length);

			if (isBinary)
			{
				shader = CreateShader<T>(name, entryPoint, string.Empty, isDebug);
				shader.D3DByteCode = new Shaders.ShaderBytecode(shaderData);
				shader.LoadShader();
			}
			else
			{
				sourceCode = Encoding.UTF8.GetString(shaderData);
				shader = CreateShader<T>(name, entryPoint, sourceCode, isDebug);
			}

			return (T)shader;
		}

		/// <summary>
		/// Function to load a shader from a file.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="fileName">File name and path to the shader file.</param>
		/// <returns>The new shader loaded from the file.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="entryPoint"/> or <paramref name="fileName"/> parameters are NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name, entryPoint or fileName parameters are empty.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
		public T FromFile<T>(string name, string entryPoint, string fileName)
			where T : GorgonShader
		{
#if DEBUG
			return FromFile<T>(name, entryPoint, fileName, true);
#else
			return FromFile<T>(name, entryPoint, fileName, false);
#endif
		}

		/// <summary>
		/// Function to load a shader from a file.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="fileName">File name and path to the shader file.</param>
        /// <param name="isDebug">TRUE to apply debug information, FALSE to exclude it.</param>
        /// <returns>The new shader loaded from the file.</returns>
		/// <remarks>The <paramref name="isDebug"/> parameter is only applicable to source code shaders.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="entryPoint"/> or <paramref name="fileName"/> parameters are NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name, entryPoint or fileName parameters are empty.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
        public T FromFile<T>(string name, string entryPoint, string fileName, bool isDebug)
			where T : GorgonShader
		{
			FileStream stream = null;

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("The parameter must not be empty.", "fileName");
            }

			try
			{
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                return FromStream<T>(name, entryPoint, stream, (int)stream.Length, isDebug);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to create a shader.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="GorgonLibrary.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader.</param>
		/// <param name="entryPoint">Entry point for the shader.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <param name="debug">TRUE to include debug information, FALSE to exclude.</param>
		/// <returns>A new vertex shader.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="entryPoint"/> parameters are empty strings.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the name or entryPoint parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there are compile errors in the shader.</exception>
		public T CreateShader<T>(string name, string entryPoint, string sourceCode, bool debug)
			where T : GorgonShader
		{
			GorgonShader shader = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

			if (typeof(T) == typeof(GorgonVertexShader))
				shader = new GorgonVertexShader(_graphics, name, entryPoint);

			if (typeof(T) == typeof(GorgonPixelShader))
				shader = new GorgonPixelShader(_graphics, name, entryPoint);

			if (shader == null)
				throw new TypeInitializationException(typeof(T).FullName, null);

			shader.IsDebug = debug;
			if (!string.IsNullOrEmpty(sourceCode))
			{
				shader.SourceCode = sourceCode;
				shader.Compile();
			}
			_graphics.AddTrackedObject(shader);

			return (T)shader;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderBinding"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		internal GorgonShaderBinding(GorgonGraphics graphics)
		{
			IncludeFiles = new GorgonShaderIncludeCollection();
			VertexShader = new GorgonVertexShaderState(graphics);
			PixelShader = new GorgonPixelShaderState(graphics);
			_graphics = graphics;
		}
		#endregion
	}
}
