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
using System.Reflection;
using System.Text;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Native;
using Shaders = SharpDX.D3DCompiler;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Version for the shaders.
	/// </summary>
	public enum ShaderVersion
	{
		/// <summary>
		/// Shader model 5.
		/// </summary>
		Version5 = 0,
		/// <summary>
		/// Shader model 4.
		/// </summary>
		Version4 = 1,
		/// <summary>
		/// Shader model 4, profile 1.
		/// </summary>
		Version4_1 = 2,
		/// <summary>
		/// Shader model 2, vertex shader profile a, pixel shader profile b.
		/// </summary>
		Version2A_B = 3
	}

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
		private readonly GorgonGraphics _graphics;		// Graphics interface.
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
        /// Property to return the current geometry shader states.
        /// </summary>
        /// <remarks>On video devices with a feature level of SM2_a_b, this property will return NULL (<i>Nothing</i> in VB.Net).  This is because devices 
        /// require a feature level of SM4 or better to use geometry shaders.</remarks>
        public GorgonGeometryShaderState GeometryShader
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current compute shader states.
        /// </summary>
        /// <remarks>On video devices with a feature level less than SM5, this property will return NULL (<i>Nothing</i> in VB.Net).  This is because devices 
        /// require a feature level of SM5 or better to use compute shaders.</remarks>
        public GorgonComputeShaderState ComputeShader
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the current hull shader states.
		/// </summary>
		/// <remarks>On video devices with a feature level less than SM5, this property will return NULL (<i>Nothing</i> in VB.Net).  This is because devices 
		/// require a feature level of SM5 or better to use hull shaders.</remarks>
		public GorgonHullShaderState HullShader
		{
			get;
		}

		/// <summary>
		/// Property to return the current domain shader states.
		/// </summary>
		/// <remarks>On video devices with a feature level less than SM5, this property will return NULL (<i>Nothing</i> in VB.Net).  This is because devices 
		/// require a feature level of SM5 or better to use domain shaders.</remarks>
		public GorgonDomainShaderState DomainShader
		{
			get;
		}

		/// <summary>
		/// Property to return a list of include files for the shaders.
		/// </summary>
		public IGorgonNamedObjectDictionary<GorgonShaderInclude> IncludeFiles
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to reset the shader states.
        /// </summary>
	    internal void Reset()
	    {
            VertexShader.Reset();
            PixelShader.Reset();

            if (GeometryShader != null)
            {
                GeometryShader.Reset();
            }

            if (ComputeShader != null)
            {
                ComputeShader.Reset();
            }

            if (HullShader != null)
            {
                HullShader.Reset();
            }

            if (DomainShader != null)
            {
                DomainShader.Reset();
            }
	    }


	    /// <summary>
		/// Function clean up any resources within this interface.
		/// </summary>
		internal void CleanUp()
		{
			if (PixelShader != null)
			{
				PixelShader.CleanUp();
			}

			if (VertexShader != null)
			{
				VertexShader.CleanUp();
			}

            if (GeometryShader != null)
            {
                GeometryShader.CleanUp();
            }

            if (ComputeShader != null)
            {
                ComputeShader.CleanUp();
            }

			if (HullShader != null)
			{
				HullShader.CleanUp();
			}

			if (DomainShader != null)
			{
				DomainShader.CleanUp();
			}

		    ComputeShader = null;
		    GeometryShader = null;
			PixelShader = null;
			VertexShader = null;
		}

		/// <summary>
		/// Function to re-seat a shader after it's been altered.
		/// </summary>
		/// <param name="shader">Shader to re-seat.</param>
		internal void Reseat(GorgonShader shader)
		{
		    switch (shader.ShaderType)
		    {
                case ShaderType.Vertex:
                    if (VertexShader.Current == shader)
                    {
                        VertexShader.Current = null;
                        VertexShader.Current = (GorgonVertexShader)shader;
                    }
		            break;
                case ShaderType.Pixel:
                    if (PixelShader.Current == shader)
                    {
                        PixelShader.Current = null;
                        PixelShader.Current = (GorgonPixelShader)shader;
                    }
		            break;
                case ShaderType.Geometry:
                    if ((GeometryShader != null) && (GeometryShader.Current == shader))
                    {
                        GeometryShader.Current = null;
                        GeometryShader.Current = (GorgonGeometryShader)shader;
                    }
		            break;
                case ShaderType.Compute:
                    if ((ComputeShader != null) && (ComputeShader.Current == shader))
                    {
                        ComputeShader.Current = null;
                        ComputeShader.Current = (GorgonComputeShader)shader;
                    }
		            break;
				case ShaderType.Hull:
					if ((HullShader != null) && (HullShader.Current == shader))
					{
						HullShader.Current = null;
						HullShader.Current = (GorgonHullShader)shader;
					}
				    break;
				case ShaderType.Domain:
					if ((DomainShader != null) && (DomainShader.Current == shader))
					{
						DomainShader.Current = null;
						DomainShader.Current = (GorgonDomainShader)shader;
					}
				    break;
		    }

            // If we have multiple contexts, then we need to unbind from those as well.
            if ((_graphics.IsDeferred) || (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.Sm5))
            {
                return;
            }

            foreach (var context in _graphics.GetTrackedObjectsOfType<GorgonGraphics>())
            {
                context.Shaders.Reseat(shader);
            }
		}

		/// <summary>
		/// Function to unbind a shader view.
		/// </summary>
		/// <param name="view">View to unbind.</param>
		internal void Unbind(GorgonShaderView view)
		{
			PixelShader.Resources.Unbind(view);
			VertexShader.Resources.Unbind(view);
		    if (GeometryShader != null)
		    {
                GeometryShader.Resources.Unbind(view);
		    }
            if (ComputeShader != null)
            {
                ComputeShader.Resources.Unbind(view);
            }
			if (HullShader != null)
			{
				HullShader.Resources.Unbind(view);
			}
			if (DomainShader != null)
			{
				DomainShader.Resources.Unbind(view);
			}

            // If we have multiple contexts, then we need to unbind from those as well.
            if ((_graphics.IsDeferred) || (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.Sm5))
		    {
		        return;
		    }

		    foreach (var context in _graphics.GetTrackedObjectsOfType<GorgonGraphics>())
		    {
		        context.Shaders.Unbind(view);
		    }
		}
        
        /// <summary>
        /// Function to unbind a shader resource from all shaders.
        /// </summary>
        /// <param name="resource">Shader resource to unbind.</param>
        internal void UnbindResource(GorgonResource resource)
        {
            PixelShader.Resources.UnbindResource(resource);
            VertexShader.Resources.UnbindResource(resource);
            if (GeometryShader != null)
            {
                GeometryShader.Resources.UnbindResource(resource);
            }
            if (ComputeShader != null)
            {
                ComputeShader.Resources.UnbindResource(resource);
                ComputeShader.UnorderedAccessViews.UnbindResource(resource);
            }
			if (HullShader != null)
			{
				HullShader.Resources.UnbindResource(resource);
			}
			if (DomainShader != null)
			{
				DomainShader.Resources.UnbindResource(resource);
			}

            // If we have multiple contexts, then we need to unbind from those as well.
            if ((_graphics.IsDeferred) || (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.Sm5))
            {
                return;
            }

            foreach (var context in _graphics.GetTrackedObjectsOfType<GorgonGraphics>())
            {
                context.Shaders.UnbindResource(resource);
            }
        }

        /// <summary>
        /// Function to unbind UAVs bound to the compute shader.
        /// </summary>
        /// <param name="view">View to unbind.</param>
        internal void Unbind(GorgonUnorderedAccessView view)
        {
            if (ComputeShader == null)
            {
                return;
            }

            ComputeShader.UnorderedAccessViews.Unbind(view);

            // If we have multiple contexts, then we need to unbind from those as well.
            if ((_graphics.IsDeferred) || (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.Sm5))
            {
                return;
            }

            foreach (var context in _graphics.GetTrackedObjectsOfType<GorgonGraphics>())
            {
                context.Shaders.Unbind(view);
            }
        }

		/// <summary>
		/// Function to unbind a constant buffer from all shaders.
		/// </summary>
		/// <param name="constantBuffer">The constant buffer to unbind.</param>
		internal void UnbindConstantBuffer(GorgonConstantBuffer constantBuffer)
		{
			PixelShader.ConstantBuffers.Unbind(constantBuffer);
			VertexShader.ConstantBuffers.Unbind(constantBuffer);
            if (GeometryShader != null)
            {
                GeometryShader.ConstantBuffers.Unbind(constantBuffer);
            }
            if (ComputeShader != null)
            {
                ComputeShader.ConstantBuffers.Unbind(constantBuffer);
            }
			if (HullShader != null)
			{
				HullShader.ConstantBuffers.Unbind(constantBuffer);
			}
			if (DomainShader != null)
			{
				DomainShader.ConstantBuffers.Unbind(constantBuffer);
			}

            // If we have multiple contexts, then we need to unbind from those as well.
            if ((_graphics.IsDeferred) || (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.Sm5))
            {
                return;
            }

            foreach (var context in _graphics.GetTrackedObjectsOfType<GorgonGraphics>())
            {
                context.Shaders.UnbindConstantBuffer(constantBuffer);
            }
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
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the parameter list does not contain a required parameter.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the graphics context is deferred.</exception>
		public T CreateEffect<T>(string name, params GorgonEffectParameter[] parameters)
			where T : GorgonEffect
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
            }

			// Create the effect.
			var effect = (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] {_graphics, name}, null);

            effect.InitializeEffect(parameters);

			_graphics.AddTrackedObject(effect);

			return effect;
		}

        /// <summary>
		/// Function to load a shader from a byte array.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="Gorgon.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="shaderData">Array of bytes containing the shader data.</param>
		/// <param name="macros">[Optional] A list of conditional compilation macros to apply to the shader.</param>
        /// <param name="isDebug">[Optional] <b>true</b> to apply debug information, <b>false</b> to exclude it.</param>
        /// <returns>The new shader loaded from the data stream.</returns>
		/// <remarks>The <paramref name="isDebug"/> parameter is only applicable to source code shaders.
		/// <para>If the <paramref name="macros"/> parameter is not NULL (<i>Nothing</i> in VB.Net), then a list of conditional compilation macro #define symbols will be sent to the shader.  This 
		/// is handy when you wish to exclude parts of a shader upon compilation.  Please note that this parameter is only used if the data in memory contains source code to compile.</para>
		/// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shaderData"/>, <paramref name="name"/> or <paramref name="entryPoint"/> parameters are NULL (<i>Nothing</i> in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or entryPoint parameters are empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the shaderData parameter length is less than or equal to 0.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
        /// <exception cref="GorgonException">Thrown when the shader could not be created.</exception>
#if DEBUG
        public T FromMemory<T>(string name, string entryPoint, byte[] shaderData, IList<GorgonShaderMacro> macros = null, bool isDebug = true)
#else
        public T FromMemory<T>(string name, string entryPoint, byte[] shaderData, IList<GorgonShaderMacro> macros = null, bool isDebug = false)
#endif
 where T : GorgonShader
        {
            if (shaderData == null)
            {
                throw new ArgumentNullException(nameof(shaderData));
            }

            using (IGorgonPointer pointer = new GorgonPointerPinned<byte>(shaderData))
            {
                return FromStream<T>(name, entryPoint, new GorgonDataStream(pointer), shaderData.Length, macros, isDebug);
            }
        }

		/// <summary>
		/// Function to load a shader from a stream of data.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="Gorgon.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="stream">Stream to load the shader from.</param>
		/// <param name="size">Size of the shader, in bytes.</param>
		/// <param name="macros">[Optional] A list of conditional compilation macros to apply to the shader.</param>
        /// <param name="isDebug">[Optional] <b>true</b> to apply debug information, <b>false</b> to exclude it.</param>
        /// <returns>The new shader loaded from the data stream.</returns>
		/// <remarks>The <paramref name="isDebug"/> parameter is only applicable to source code shaders.
		/// <para>If the <paramref name="macros"/> parameter is not NULL (<i>Nothing</i> in VB.Net), then a list of conditional compilation macro #define symbols will be sent to the shader.  This 
		/// is handy when you wish to exclude parts of a shader upon compilation.  Please note that this parameter is only used if the data in the stream contains source code to compile.</para>
		/// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/>, <paramref name="name"/> or <paramref name="entryPoint"/> parameters are NULL (<i>Nothing</i> in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or entryPoint parameters are empty.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than or equal to 0.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
        /// <exception cref="GorgonException">Thrown when the shader could not be created.</exception>
#if DEBUG
        public T FromStream<T>(string name, string entryPoint, Stream stream, int size, IList<GorgonShaderMacro> macros = null, bool isDebug = true)
#else
        public T FromStream<T>(string name, string entryPoint, Stream stream, int size, IList<GorgonShaderMacro> macros = null, bool isDebug = false)
#endif
 where T : GorgonShader
		{
			GorgonShader shader;
			byte[] shaderData;

            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }
            
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (entryPoint == null)
            {
                throw new ArgumentNullException(nameof(entryPoint));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (string.IsNullOrWhiteSpace("name"))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
            }

            if (string.IsNullOrWhiteSpace("entryPoint"))
            {
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(entryPoint));
            }           
			
			long streamPosition = stream.Position;

			// Check for the binary header.  If we have it, load the file as a binary file.
			// Otherwise load it as source code.
			var header = new byte[Encoding.UTF8.GetBytes(BinaryShaderHeader).Length];
			bool isBinary = (string.Equals(Encoding.UTF8.GetString(header), stream.ReadString(), StringComparison.OrdinalIgnoreCase));
			if (isBinary)
			{
				shaderData = new byte[size - BinaryShaderHeader.Length];
			}
			else
			{
				stream.Position = streamPosition;
				shaderData = new byte[size];
			}

			stream.Read(shaderData, 0, shaderData.Length);

			if (isBinary)
			{
				shader = CreateShader<T>(name, entryPoint, string.Empty, null, isDebug);
				shader.D3DByteCode = new Shaders.ShaderBytecode(shaderData);
				shader.Initialize();
			}
			else
			{
				string sourceCode = Encoding.UTF8.GetString(shaderData);
				shader = CreateShader<T>(name, entryPoint, sourceCode, macros, isDebug);
			}

			return (T)shader;
		}

		/// <summary>
		/// Function to load a shader from a file.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="Gorgon.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader object.</param>
		/// <param name="entryPoint">Entry point method to call in the shader.</param>
		/// <param name="fileName">File name and path to the shader file.</param>
		/// <param name="macros">[Optional] A list of conditional compilation macros to apply to the shader.</param>
        /// <param name="isDebug">[Optional] <b>true</b> to apply debug information, <b>false</b> to exclude it.</param>
        /// <returns>The new shader loaded from the file.</returns>
		/// <para>If the <paramref name="macros"/> parameter is not NULL (<i>Nothing</i> in VB.Net), then a list of conditional compilation macro #define symbols will be sent to the shader.  This 
		/// is handy when you wish to exclude parts of a shader upon compilation.  Please note that this parameter is only used if the data in the file contains source code to compile.</para>
		/// <remarks>The <paramref name="isDebug"/> parameter is only applicable to source code shaders.
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="entryPoint"/> or <paramref name="fileName"/> parameters are NULL (<i>Nothing</i> in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name, entryPoint or fileName parameters are empty.</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
        /// <exception cref="GorgonException">Thrown when the shader could not be created.</exception>
#if DEBUG
        public T FromFile<T>(string name, string entryPoint, string fileName, IList<GorgonShaderMacro> macros = null, bool isDebug = true)
#else
        public T FromFile<T>(string name, string entryPoint, string fileName, IList<GorgonShaderMacro> macros = null, bool isDebug = false)
#endif
 where T : GorgonShader
		{
			FileStream stream = null;

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(fileName));
            }

			try
			{
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                return FromStream<T>(name, entryPoint, stream, (int)stream.Length, macros, isDebug);
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to create a shader.
		/// </summary>
		/// <typeparam name="T">The shader type.  Must be inherited from <see cref="Gorgon.Graphics.GorgonShader">GorgonShader</see>.</typeparam>
		/// <param name="name">Name of the shader.</param>
		/// <param name="entryPoint">Name of the function serves as the entry point to the shader program.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <param name="macros">[Optional] A list of conditional compilation macros to apply to the shader.</param>
		/// <param name="debug">[Optional] <b>true</b> to include debug information, <b>false</b> to exclude.</param>
		/// <returns>A new vertex shader.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="entryPoint"/> parameters are empty strings.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the name or entryPoint parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.TypeInitializationException">Thrown when the type of shader is unrecognized.</exception>
		/// <exception cref="GorgonException">Thrown when the shader could not be created.</exception>
		/// <exception cref="System.NotSupportedException">Thrown when T is a <see cref="Gorgon.Graphics.GorgonOutputGeometryShader">GorgonOutputGeometryShader</see>.</exception>
        /// <remarks>This method will create one of the 6 shader types (<see cref="Gorgon.Graphics.GorgonVertexShader">vertex</see>, <see cref="Gorgon.Graphics.GorgonPixelShader">pixel</see>, 
        /// <see cref="Gorgon.Graphics.GorgonGeometryShader">geometry</see>, <see cref="Gorgon.Graphics.GorgonComputeShader">compute</see>, 
        /// <see cref="Gorgon.Graphics.GorgonHullShader">hull</see> and <see cref="Gorgon.Graphics.GorgonDomainShader">domain</see>).  
        /// Shaders are small programs that can be run on the GPU and are invoked 
		/// when processing data on the GPU.  
        /// <para>A vertex shader is used when processing a single vertex.<para> </para>
        /// A pixel (aka fragment) shader is used when processing pixels when rasterizing
        /// A geometry shader is unique in that it processes full primtivies (triangles, lines, points) but it may also be used to create or remove geometry.<para> </para>
        /// A compute shader allows the GPU to be used for general computations.<para> </para>
        /// A hull and domain shader are used in tessellation of geometry.
        /// </para>
        /// <para>For devices that have a feature level of SM2_a_b, only Vertex and Pixel shaders are supported, all other types will generate an exception upon creation.</para>
        /// <para>Compute, Hull, and Domain shaders require a device with a feature level of SM5 or better.  Creation of these shaders on devices that do not have a feature level of SM5 or better will 
        /// generate an exception.</para>
        /// <para>If the DEBUG version of Gorgon is being used, then the <paramref name="debug"/> flag will be defaulted to <b>true</b>, if the RELEASE version is used, then it will be defaulted to <b>false</b>.</para>
        /// <para>If the <paramref name="macros"/> parameter is not NULL (<i>Nothing</i> in VB.Net), then a list of conditional compilation macro #define symbols will be sent to the shader.  This 
        /// is handy when you wish to exclude parts of a shader upon compilation.  Please note that this parameter is only used if the <paramref name="sourceCode"/> parameter is not NULL or empty.</para>
		/// <para>Do not use this method to create a <see cref="Gorgon.Graphics.GorgonOutputGeometryShader">GorgonOutputGeometryShader</see> shader, use the <see cref="CreateShader(string, string, string, IList{GorgonStreamOutputElement}, IList{int}, int, IList{GorgonShaderMacro}, bool)">overload</see> 
		/// instead.</para>
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
#if DEBUG
		public T CreateShader<T>(string name, string entryPoint, string sourceCode, IList<GorgonShaderMacro> macros = null, bool debug = true)
#else
        public T CreateShader<T>(string name, string entryPoint, string sourceCode, IList<GorgonShaderMacro> macros = null, bool debug = false)
#endif
 where T : GorgonShader
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }
            
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
			
			if (entryPoint == null)
			{
				throw new ArgumentNullException(nameof(entryPoint));
			}

			if (string.IsNullOrWhiteSpace(entryPoint))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(entryPoint));
			}
			
			if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (typeof(T) == typeof(GorgonOutputGeometryShader))
			{
				throw new NotSupportedException(Resources.GORGFX_SHADER_NO_SO);
			}

			var shader = (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[]
				{
					_graphics, name, entryPoint
				}, null, null);

			if (shader == null)
			{
				throw new TypeInitializationException(typeof(T).FullName, null);
			}

			shader.IsDebug = debug;

			if (!string.IsNullOrWhiteSpace(sourceCode))
			{
				shader.SourceCode = sourceCode;
				shader.Compile(macros);
			}

			_graphics.AddTrackedObject(shader);

			return shader;
		}

	    /// <summary>
		/// Function to create a geometry shader that can stream out data into a buffer.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		/// <param name="entryPoint">Name of the function serves as the entry point to the shader program.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <param name="streamOutputElements">A list of layout elements to indicate how to organize the data in the buffer.</param>
		/// <param name="bufferStrides">The size, in bytes, of an element in the buffer.</param>
        /// <param name="rasterizeStream">[Optional] The stream to send on to the rasterizer.</param>
		/// <param name="macros">[Optional] A list of conditional compilation macros to apply to the shader.</param>
        /// <param name="debug">[Optional] <b>true</b> to include debug information, <b>false</b> to exclude.</param>
		/// <returns>A new geometry shader that can stream output to a buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, the <paramref name="entryPoint"/>, <paramref name="bufferStrides"/> or the <paramref name="streamOutputElements"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/>, the <paramref name="entryPoint"/>, <paramref name="bufferStrides"/> or the <paramref name="streamOutputElements"/> parameters are empty.</exception>
		/// <exception cref="GorgonException">Thrown when the shader could not be created.</exception>
		/// <remarks>Create this shader when you need to stream the output of the shader pipeline into a buffer along with (or instead of) the rasterizer.  If the <paramref name="rasterizeStream"/> value is less than 0 or greater than 3 then 
		/// it is assumed that the stream does not get rasterized. 
		/// <para>The <paramref name="bufferStrides"/> is the size of an element inside of a buffer.  This value must not be larger than 2048 bytes, and must be at least the same size as the total of all the component counts per stream
		///  in the stream output element multiplied by 4.  If this value is NULL (<i>Nothing</i> in VB.Net), then the strides for the buffers will be automatically calculated.</para>
		/// <para>
		/// Up to four stream out buffers may be used at the same time to receive data.  Because of this, the <paramref name="bufferStrides"/> parameter should contain no more than 4 values.
		/// </para>
		/// <para>If the <paramref name="macros"/> parameter is not NULL (<i>Nothing</i> in VB.Net), then a list of conditional compilation macro #define symbols will be sent to the shader.  This 
		/// is handy when you wish to exclude parts of a shader upon compilation.  Please note that this parameter is only used if the <paramref name="sourceCode"/> parameter is not NULL or empty.</para>
		/// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
#if DEBUG
		public GorgonOutputGeometryShader CreateShader(string name, string entryPoint, string sourceCode, 
		                                               IList<GorgonStreamOutputElement> streamOutputElements, IList<int> bufferStrides = null, int rasterizeStream = -1, IList<GorgonShaderMacro> macros = null, bool debug = true)
#else
		public GorgonOutputGeometryShader CreateShader(string name, string entryPoint, string sourceCode,
		                                               IList<GorgonStreamOutputElement> streamOutputElements, IList<int> bufferStrides = null, int rasterizeStream = -1, IList<GorgonShaderMacro> macros = null, bool debug = false)
#endif
		{
	        bool autoStride = bufferStrides == null;

            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }
            
            if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (streamOutputElements == null)
			{
				throw new ArgumentException("streamOutputElements");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (streamOutputElements.Count == 0)
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(streamOutputElements));
			}

            if (autoStride)
            {
                bufferStrides = new int[streamOutputElements.Max(item => item.Stream)];
            }
			
			// Validate the buffer strides.
			for (int stream = 0; stream < bufferStrides.Count; stream++)
			{
				int componentSize = (streamOutputElements.Where(item => item.Stream == stream)
				                                        .Sum(component => component.ComponentCount * 4));

                if (autoStride)
                {
                    bufferStrides[stream] = componentSize;
                }

				if ((bufferStrides[stream] % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_SO_BUFFER_NOT_MULTIPLE_OF_4, stream));
				}

				if (componentSize > bufferStrides[stream])
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_SO_STRIDE_TOO_SMALL, stream, bufferStrides[stream], componentSize));
				}

				if (bufferStrides[stream] >= 2048)
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_SO_STRIDE_TOO_LARGE, stream));
				}
			}
			
			var result = new GorgonOutputGeometryShader(_graphics, name, entryPoint, rasterizeStream, streamOutputElements, bufferStrides)
				{
					IsDebug = debug
				};

			if (!string.IsNullOrWhiteSpace(sourceCode))
			{
				result.SourceCode = sourceCode;
				result.Compile(macros);
			}

			_graphics.AddTrackedObject(result);

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderBinding"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		internal GorgonShaderBinding(GorgonGraphics graphics)
		{
			IncludeFiles = new GorgonNamedObjectDictionary<GorgonShaderInclude>(false);
			VertexShader = new GorgonVertexShaderState(graphics);
			PixelShader = new GorgonPixelShaderState(graphics);
		    GeometryShader = new GorgonGeometryShaderState(graphics);

			if (graphics.VideoDevice.SupportedFeatureLevel > DeviceFeatureLevel.Sm41)
            {
                ComputeShader = new GorgonComputeShaderState(graphics);
				HullShader = new GorgonHullShaderState(graphics);
				DomainShader = new GorgonDomainShaderState(graphics);
            }
		    _graphics = graphics;
		}
		#endregion
	}
}