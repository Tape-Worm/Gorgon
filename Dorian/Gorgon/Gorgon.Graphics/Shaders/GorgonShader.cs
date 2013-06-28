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
using System.Text;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using D3D = SharpDX.Direct3D11;
using Shaders = SharpDX.D3DCompiler;

namespace GorgonLibrary.Graphics
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
	/// The base shader object.
	/// </summary>
	public abstract class GorgonShader
		: GorgonNamedObject, INotifier, IDisposable
	{
		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
		private string _source;										// Shader source code.
		private ShaderVersion _version = ShaderVersion.Version2A_B;	// Shader model version.
		private bool _isDebug;										// Flag to indicate that debug information is included.
		private Shaders.ShaderBytecode _byteCode;					// Byte code for the shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the shader byte code.
		/// </summary>
		internal Shaders.ShaderBytecode D3DByteCode
		{
			get
			{
				return _byteCode;
			}
			set
			{
				if (_byteCode != value)
				{
					_byteCode = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to return whether the shader is a binary object or not.
		/// </summary>
		public bool IsBinary
		{
			get
			{
				return (string.IsNullOrEmpty(SourceCode)) && (D3DByteCode != null);
			}
		}

		/// <summary>
		/// Property to set or return whether to include debug information in the shader or not.
		/// </summary>
		/// <remarks>
		/// This property has no effect when the shader is a <see cref="P:GorgonLibrary.Graphics.GorgonShader.IsBinary">binary shader</see> (i.e. no source code).
		/// <para>After changing this property, use the <see cref="M:GorgonLibrary.Graphics.GorgonShader.Compile">Compile</see> method to update the shader.</para>
		/// </remarks>
		public bool IsDebug
		{
			get
			{
				return _isDebug;
			}
			set
			{
				if (_isDebug == value)
				{
					return;
				}

				_isDebug = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to return the type of shader.
		/// </summary>
		public ShaderType ShaderType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the entry point method.
		/// </summary>
		public string EntryPoint
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the graphics interface that created this shader.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the shader model version number for this shader.
		/// </summary>
		/// <remarks>It is not recommended to set this value manually.  Gorgon will attempt to find the best version for the supported feature level.
		/// <para>After changing this property, use the <see cref="M:GorgonLibrary.Graphics.GorgonShader.Compile">Compile</see> method to update the shader.</para>
		/// </remarks>
		public ShaderVersion Version
		{
			get
			{
				return _version;
			}
			set
			{
				if (_version != value)
				{
					_version = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the source code for the shader.
		/// </summary>
		/// <remarks>This value will be empty or NULL (Nothing in VB.Net) if the shader has no source code (i.e. it's loaded from a binary shader).
		/// <para>After changing this property, use the <see cref="M:GorgonLibrary.Graphics.GorgonShader.Compile">Compile</see> method to update the shader.</para>
		/// </remarks>
		public string SourceCode
		{
			get
			{
				return _source;
			}
			set
			{
				if (_source != value)
				{
					_source = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the errors generated by the shader.
		/// </summary>
		public string Errors
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the Direct3D shader version.
		/// </summary>
		/// <returns>The Direct3D shader version.</returns>
		private string GetD3DVersion()
		{			
			string prefix;
			string version;

		    if (((ShaderType == GorgonLibrary.Graphics.ShaderType.Compute)
		         || (ShaderType == GorgonLibrary.Graphics.ShaderType.Domain)
		         || (ShaderType == GorgonLibrary.Graphics.ShaderType.Hull))
		        && (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5))
		    {
		        throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
		    }

		    if ((ShaderType == GorgonLibrary.Graphics.ShaderType.Geometry)
		        && ((Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4)))
		    {
                throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
		    }

		    switch (ShaderType)
			{
				case ShaderType.Pixel:
					prefix = "ps";
					break;
				case ShaderType.Compute:
					prefix = "cs";
					break;
				case ShaderType.Geometry:
					prefix = "gs";
					break;
				case ShaderType.Domain:
					prefix = "ds";
					break;
				case ShaderType.Hull:
					prefix = "hs";
					break;
				case ShaderType.Vertex:
					prefix = "vs";
					break;
				default:
					throw new NotSupportedException(string.Format(Resources.GORGFX_SHADER_UNKNOWN_TYPE, ShaderType));
			}

			switch (_version)
			{
				case ShaderVersion.Version5:
					version = "5_0";
					break;
				case ShaderVersion.Version4_1:
					version = "4_1";
					break;
				case ShaderVersion.Version4:
					version = "4_0";
					break;
				case ShaderVersion.Version2A_B:
					version = "4_0_level_9_3";
					break;
				default:
					throw new NotSupportedException(string.Format(Resources.GORGFX_SHADER_UNKNOWN_TYPE, ShaderType));
			}

			return prefix + "_" + version;
		}
		
		/// <summary>
		/// Function to compile the shader.
		/// </summary>
		/// <param name="includeDebugInfo">TRUE to include debug information, FALSE to exclude it.</param>
		/// <returns>The compiled shader byte code.</returns>
		private Shaders.ShaderBytecode CompileFromSource(bool includeDebugInfo)
		{
			var flags = Shaders.ShaderFlags.OptimizationLevel3;
			string parsedCode;

			try
			{
				parsedCode = Graphics.Shaders.IncludeFiles.ProcessSource(SourceCode);
								
				IsDebug = includeDebugInfo;

				if (IsDebug)
					flags = Shaders.ShaderFlags.Debug;

				if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
				{
					flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
				}

				return Shaders.ShaderBytecode.Compile(parsedCode, EntryPoint, GetD3DVersion(), flags, Shaders.EffectFlags.None, null, null);
			}
			catch (SharpDX.CompilationException cex)
			{
				Errors = cex.Message;
				throw GorgonException.Catch(cex);
			}
		}

		/// <summary>
		/// Function to create the shader.
		/// </summary>
		/// <param name="byteCode">Byte code for the shader.</param>
		protected abstract void CreateShader(Shaders.ShaderBytecode byteCode);

		/// <summary>
		/// Function to load a shader from preexisting byte code.
		/// </summary>
		internal void Initialize()
		{
		    if (D3DByteCode != null)
		    {
		        CreateShader(D3DByteCode);
		    }

		    Graphics.Shaders.Reseat(this);
			HasChanged = false;
		}

		/// <summary>
		/// Function to compile the shader.
		/// </summary>
		/// <remarks>Whenever a shader is changed (i.e. its <see cref="GorgonLibrary.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is modified), this method should be called to build the shader.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when the shader is not supported by the current supported feature level for the video hardware.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the shader fails to compile.</exception>
		public void Compile()
		{
		    if (!HasChanged)
		    {
		        return;
		    }

		    if (D3DByteCode != null)
			{
				D3DByteCode.Dispose();
				D3DByteCode = null;
			}

		    if (!string.IsNullOrEmpty(SourceCode))
		    {
		        D3DByteCode = CompileFromSource(IsDebug);
		    }

		    Initialize();
		}

        /// <summary>
        /// Function to save the shader to a stream.
        /// </summary>
        /// <param name="binary">TRUE to save the binary version of the shader, FALSE to save the source.</param>
        /// <param name="saveDebug">TRUE to save the debug information, FALSE to exclude it.</param>
        /// <returns>An array of bytes.</returns>
        /// <remarks>The <paramref name="saveDebug"/> parameter is only applicable when the <paramref name="binary"/> parameter is set to TRUE.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when the shader is being saved as source code and the <see cref="GorgonLibrary.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is NULL (Nothing in VB.Net) or empty.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the shader fails to compile.</exception>
        public byte[] Save(bool binary, bool saveDebug)
        {
            using (var memoryStream = new MemoryStream())
            {
                Save(memoryStream, binary, saveDebug);
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

		/// <summary>
		/// Function to save the shader to a stream.
		/// </summary>
		/// <param name="stream">Stream to write into.</param>
		/// <param name="binary">[Optional] TRUE to save the binary version of the shader, FALSE to save the source.</param>
		/// <param name="saveDebug">[Optional] TRUE to save the debug information, FALSE to exclude it.</param>
		/// <remarks>The <paramref name="saveDebug"/> parameter is only applicable when the <paramref name="binary"/> parameter is set to TRUE.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the shader is being saved as source code and the <see cref="GorgonLibrary.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is NULL (Nothing in VB.Net) or empty.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the shader fails to compile.</exception>
		public void Save(Stream stream, bool binary = false, bool saveDebug = false)
		{
			Shaders.ShaderBytecode compiledShader = null;
			GorgonDebug.AssertNull<Stream>(stream, "stream");

			if ((!binary) && (string.IsNullOrEmpty(SourceCode)))
				throw new ArgumentException("Cannot save the shader source, no source code was found for the shader.", "binary");

			if (!binary)
			{
				byte[] shaderSource = Encoding.UTF8.GetBytes(SourceCode);
				stream.Write(shaderSource, 0, shaderSource.Length);
			}
			else
			{
				try
				{
					compiledShader = CompileFromSource(saveDebug);
					byte[] header = Encoding.UTF8.GetBytes(GorgonShaderBinding.BinaryShaderHeader);
					stream.Write(header, 0, header.Length);
					compiledShader.Save(stream);
				}
				finally
				{
					if (compiledShader != null)
						compiledShader.Dispose();
					compiledShader = null;
				}
			}
		}

		/// <summary>
		/// Function to save the shader to a file.
		/// </summary>
		/// <param name="fileName">File name and path for the shader file.</param>
		/// <param name="binary">[Optional] TRUE if saving as a binary version of the shader, FALSE if not.</param>
		/// <param name="saveDebug">[Optional] TRUE to save debug information with the shader, FALSE to exclude it.</param>
		/// <remarks>The <paramref name="saveDebug"/> parameter is only applicable when the <paramref name="binary"/> parameter is set to TRUE.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown is the fileName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the shader is being saved as source code and the <see cref="GorgonLibrary.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is NULL (Nothing in VB.Net) or empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the shader fails to compile.</exception>
		public void Save(string fileName, bool binary = false, bool saveDebug = false)
		{
			FileStream stream = null;

			GorgonDebug.AssertParamString(fileName, "fileName");

			try
			{
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream, binary, saveDebug);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShader"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that created this shader.</param>
		/// <param name="name">The name of the shader.</param>
		/// <param name="type">Type of the shader.</param>
		/// <param name="entryPoint">The entry point method for the shader.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonShader(GorgonGraphics graphics, string name, ShaderType type, string entryPoint)
			: base(name)
		{
			Graphics = graphics;

#if DEBUG
			IsDebug = true;
#else
			IsDebug = false;
#endif

			ShaderType = type;
			EntryPoint = entryPoint;

			// Determine the version by the supported feature level.
			switch (Graphics.VideoDevice.SupportedFeatureLevel)
			{
				case DeviceFeatureLevel.SM5:
					Version = ShaderVersion.Version5;
					break;
				case DeviceFeatureLevel.SM4_1:
					Version = ShaderVersion.Version4_1;
					break;
				case DeviceFeatureLevel.SM4:
					Version = ShaderVersion.Version4;
					break;
				default:
					Version = ShaderVersion.Version2A_B;
					break;
			}
		}
		#endregion

		#region INotifier Members
		/// <summary>
		/// Property to set or return whether an object has been updated.
		/// </summary>
		public bool HasChanged
		{
			get;
			set;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
		    if (_disposed)
		    {
		        return;
		    }

		    if (disposing)
		    {
		        if (D3DByteCode != null)
		            D3DByteCode.Dispose();

		        Graphics.RemoveTrackedObject(this);
		    }

		    D3DByteCode = null;
		    _disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
