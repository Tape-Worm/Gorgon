#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Monday, June 30, 2008 11:55:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Base shader for single vertex/pixel shaders (i.e. non-FX).
	/// </summary>
	public abstract class BaseShader<T>
		: Shader, ISerializable
		where T : Shader, ISerializable
	{
		#region Variables.
		private bool _disposed = false;												// Flag to indicate that this object has been disposed.
		private ShaderFunction _function = null;									// Shader function containing the byte code.
		private SortedDictionary<string, D3D9.EffectHandle> _samplers = null;		// Texture samplers.
		private string _fileName = string.Empty;									// Filename and path of the pixel shader.
		private bool _isResource = false;											// Flag to indicate that this object is an embedded resource.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the samplers bound to the shader.
		/// </summary>
		protected SortedDictionary<string, D3D9.EffectHandle> Samplers
		{
			get
			{
				return _samplers;
			}
		}

		/// <summary>
		/// Property to set or return the shader function.
		/// </summary>
		protected ShaderFunction Function
		{
			get
			{
				return _function;
			}
			set
			{
				_function = value;
			}
		}

		/// <summary>
		/// Property to return whether the shader has been compiled yet or not.
		/// </summary>
		/// <value></value>
		public override bool IsCompiled
		{
			get
			{
				return _function != null;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the shader target profile.
		/// </summary>
		/// <param name="target">Version of the profile.</param>
		/// <returns>The shader target profile.</returns>
		protected abstract string ShaderProfile(Version target);

		/// <summary>
		/// Function to return an arbitrary object from a stream.
		/// </summary>
		/// <param name="name">Object name or path.</param>
		/// <param name="function">Entry point function name.</param>
		/// <param name="target">Target shader profile.</param>
		/// <param name="fileSystem">A file system that contains the object.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="bytes">Number of bytes to deserialize.</param>
		/// <returns>The object contained within the stream.</returns>
		protected static BaseShader<T> ShaderFromStream(string name, string function, Version target, FileSystem fileSystem, ResourceManager resources, Stream stream, ShaderCompileOptions flags, bool isXML, int bytes)
		{
			ShaderSerializer<T> serializer = null;	// Shader serializer.
			BaseShader<T> newShader = null;			// Shader object.
			string filePath = string.Empty;			// Path to the file.

			try
			{
				filePath = name;
				name = Path.GetFileNameWithoutExtension(name);

				// Return the shader if it already exists.
				if (ShaderCache.Shaders.Contains(name))
					return ShaderCache.Shaders[name] as BaseShader<T>;

				// Create a dummy shader object.
				newShader = typeof(T).Assembly.CreateInstance(typeof(T).FullName, false, System.Reflection.BindingFlags.CreateInstance, null, new object[] { name }, null, null) as BaseShader<T>;

				// Create the serializer.
				serializer = new ShaderSerializer<T>(newShader, stream);
				serializer.DontCloseStream = true;

				serializer.Parameters["Filename"] = filePath;
				serializer.Parameters["IsResource"] = (resources != null);
				serializer.Parameters["Function"] = function;
				serializer.Parameters["Target"] = target;

				// Add whether we're binary or not.
				serializer.Parameters["Binary"] = isXML;

				// Add the size parameter.
				serializer.Parameters["byteSize"] = bytes;

				// Get compilation options.
				serializer.Parameters["flags"] = (D3D9.ShaderFlags)flags;

				// Read the shader.
				serializer.Deserialize();

				return newShader;
			}
			catch 
			{
				if (newShader != null)
					newShader.Dispose();
				newShader = null;
				throw;
			}
			finally
			{
				if (serializer != null)
					serializer.Dispose();
				serializer = null;
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!_disposed)
			{

				if (disposing)
				{
					DestroyShader();
					if (_function != null)
						_function.Dispose();
				}

				_function = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Function to retrieve the parameters for a shader.
		/// </summary>
		protected override void GetParameters()
		{
			ShaderParameterType type;				// Parameter type.
			D3D9.EffectHandle handle = null;		// Parameter handle.
			D3D9.ConstantDescription desc;			// Constant description.			

			// Retrieve the samplers first.
			_samplers.Clear();
			Parameters.Clear();
			type = ShaderParameterType.Unknown;

			
			// Get parameters.
			if (_function.ByteCode.ConstantTable == null)
				return;

			for (int i = 0; i < _function.ByteCode.ConstantTable.Description.Constants; i++)
			{
				handle = _function.ByteCode.ConstantTable.GetConstant(null, i);

				if (handle != null)
				{
					desc = _function.ByteCode.ConstantTable.GetConstantDescription(handle);

					switch (desc.Type)
					{
						case SlimDX.Direct3D9.ParameterType.Bool:
							type = ShaderParameterType.Boolean;
							break;
						case SlimDX.Direct3D9.ParameterType.Float:
							if (desc.Class == SlimDX.Direct3D9.ParameterClass.Vector)
								type = ShaderParameterType.Vector4D;
							else
								type = ShaderParameterType.Float;
							break;
						case SlimDX.Direct3D9.ParameterType.Int:
							type = ShaderParameterType.Integer;
							break;
						case SlimDX.Direct3D9.ParameterType.Sampler:
						case SlimDX.Direct3D9.ParameterType.Sampler2D:
							// Assign to the name.
							_samplers[desc.Name] = handle;
							type = ShaderParameterType.Image;
							break;
						default:
							type = ShaderParameterType.Unknown;
							break;
					}

					Parameters.Add(new ConstantShaderParameter(desc.Name, handle, _function, type));
				}
			}

			((IShaderRenderer)this).GetDefinedConstants();
		}

		/// <summary>
		/// Function called when the shader is serialized.
		/// </summary>
		/// <param name="serializer">Serializer used to write the data.</param>
		protected virtual void OnWriteData(Serializer serializer)
		{
			bool binary = false;			// Flag for binary mode.
			byte[] psData = null;			// Pixel shader data.

			if (!serializer.Parameters.Contains("Binary"))
				throw new GorgonException(GorgonErrors.CannotWriteData, "Missing binary parameter for serialization.");

			binary = (bool)serializer.Parameters["Binary"];

			if ((!IsCompiled) && (binary))
				throw new GorgonException(GorgonErrors.CannotWriteData, "Cannot write a binary shader that was not compiled.");

			if ((!binary) && (ShaderSource == string.Empty))
				throw new GorgonException(GorgonErrors.CannotWriteData, "Cannot write a source code shader without source code.");
						
			// Write to the stream.			
			IsBinary = binary;			
			if (binary)
			{
				psData = new byte[Function.ByteCode.Data.Length];
				Function.ByteCode.Data.Read(psData, 0, psData.Length);				
				serializer.Write(string.Empty, psData, 0, psData.Length);
			}
			else
				serializer.Write(string.Empty, ShaderSource);
		}

		/// <summary>
		/// Function called when the shader is deserialized.
		/// </summary>
		/// <param name="serializer">Deserializer used to read the data.</param>
		protected virtual void OnReadData(GorgonLibrary.Serialization.Serializer serializer)
		{
			byte[] code = null;			        // Shader code.
			int size = 0;				        // Size of shader in bytes.
			MemoryStream stream = null;			// Stream containing compiled data.
			string errors = string.Empty;		// List of errors returned from the compiler.
			string previousDir = string.Empty;	// Previous directory.
			D3D9.ShaderFlags flags;				// Shader flags.
			string function = string.Empty;		// Function name.
			Version target;						// Profile target.

			if (!serializer.Parameters.Contains("Filename"))
				Filename = string.Empty;
			else
				Filename = serializer.Parameters["Filename"].ToString();

			if (!serializer.Parameters.Contains("IsResource"))
				IsResource = false;
			else
				IsResource = (bool)serializer.Parameters["IsResource"];

			if (!serializer.Parameters.Contains("Function"))
				throw new GorgonException(GorgonErrors.CannotLoad, "Missing serialization parameter 'Function'");
			else
				function = serializer.Parameters["Function"].ToString();

			if (!serializer.Parameters.Contains("Target"))
				throw new GorgonException(GorgonErrors.CannotLoad, "Missing serialization parameter 'Target'");
			else
				target = serializer.Parameters["Target"] as Version;

			if (serializer.Parameters.Contains("byteSize"))
				size = (int)serializer.Parameters["byteSize"];
			else
				throw new GorgonException(GorgonErrors.CannotLoad, "Missing serialization parameter 'byteSize'");

			if (!serializer.Parameters.Contains("Binary"))
				throw new GorgonException(GorgonErrors.CannotLoad, "Missing serialization parameter 'Binary'");

			if (!serializer.Parameters.Contains("flags"))
				throw new GorgonException(GorgonErrors.CannotLoad, "Missing serialization parameter 'flags'");

			try
			{
				flags = (D3D9.ShaderFlags)serializer.Parameters["flags"];

				// Compile the effect.
				if ((!string.IsNullOrEmpty(_fileName)) && (File.Exists(_fileName)))
				{
					previousDir = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(Filename)));
				}


				if ((bool)serializer.Parameters["Binary"])
				{
					// Read in the data.			
					code = serializer.ReadBytes(string.Empty, size);

					Function = new ShaderFunction(function, this, new D3D9.ShaderBytecode(code), ShaderProfile(target));
					CreateShader();
					GetParameters();

					// Compile to the shader.					
					IsBinary = true;
					ShaderSource = string.Empty;
					Compiled = code;
				}
				else
				{
					// Read source code and convert to a string.
					//serializer.Stream.Read(code, 0, size);
					ShaderSource = serializer.ReadString(string.Empty);
					CompileShaderImplementation(function, target, (ShaderCompileOptions)flags);
				}
			}
			finally
			{
				if ((!string.IsNullOrEmpty(previousDir)) && (Directory.Exists(previousDir)))
					Directory.SetCurrentDirectory(previousDir);
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function called to create the actual shader object.
		/// </summary>
		protected abstract void CreateShader();

		/// <summary>
		/// Function called to destroy the shader object.
		/// </summary>
		protected abstract void DestroyShader();

		/// <summary>
		/// Function to load a Shader from disk.
		/// </summary>
		/// <param name="filename">Filename of the Shader to load.</param>
		/// <param name="entryFunction">Name of the entry point function.</param>
		/// <param name="profile">Shader profile to use.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="binary">TRUE if the file is binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		/// <remarks>
		/// <para>The target parameter can be vs_1_1, vs_2_0, vs_2_a, vs_2_b, vs_2_sw, vs_3_0 or vs_3_sw for <see cref="GorgonLibrary.Graphics.VertexShader">vertex shaders</see> or ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, or ps_3_sw for <see cref="GorgonLibrary.Graphics.PixelShader">pixel shaders</see>.</para>
		/// </remarks>
		public static T FromFile(string filename, string entryFunction, Version profile, ShaderCompileOptions flags, bool binary)
		{
			Stream stream = null;		// File stream.

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			if (profile == null)
				throw new ArgumentNullException("profile");

			try
			{
				stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				return ShaderFromStream(filename, entryFunction, profile, null, null, stream, flags, binary, (int)stream.Length) as T;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to return the shader from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="filename">File name and path of the shader.</param>
		/// <param name="entryFunction">Name of the entry point function.</param>
		/// <param name="profile">Shader profile to use.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The shader within the file system.</returns>
		/// <remarks>
		/// <para>The target parameter can be vs_1_1, vs_2_0, vs_2_a, vs_2_b, vs_2_sw, vs_3_0 or vs_3_sw for <see cref="GorgonLibrary.Graphics.VertexShader">vertex shaders</see> or ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, or ps_3_sw for <see cref="GorgonLibrary.Graphics.PixelShader">pixel shaders</see>.</para>
		/// </remarks>
		public static T FromFileSystem(FileSystem fileSystem, string filename, string entryFunction, Version profile, ShaderCompileOptions flags, bool binary)
		{
			Stream stream = null;		// File stream.

			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				stream = new MemoryStream(fileSystem.ReadFile(filename));
				return ShaderFromStream(filename, entryFunction, profile, null, null, stream, flags, binary, (int)stream.Length) as T;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to load an shader from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="entryFunction">Name of the entry point function.</param>
		/// <param name="profile">Shader profile to use.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		/// <remarks>
		/// <para>The target parameter can be vs_1_1, vs_2_0, vs_2_a, vs_2_b, vs_2_sw, vs_3_0 or vs_3_sw for <see cref="GorgonLibrary.Graphics.VertexShader">vertex shaders</see> or ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, or ps_3_sw for <see cref="GorgonLibrary.Graphics.PixelShader">pixel shaders</see>.</para>
		/// </remarks>
		public static T FromResource(string name, ResourceManager resourceManager, string entryFunction, Version profile, ShaderCompileOptions flags, bool binary)
		{
			Stream stream = null;			// Stream
			Assembly assembly = null;		// Assembly object.
			string effect = string.Empty;	// Effect code.

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			try
			{
				// Default to the calling application resource manager.
				if (resourceManager == null)
				{
					// Extract the resource manager from the calling assembly.
					assembly = Assembly.GetEntryAssembly();
					resourceManager = new ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);
				}

				// Open a stream to the resource object.				
				if (!binary)
				{
					effect = resourceManager.GetObject(name).ToString();
					stream = new MemoryStream(Encoding.UTF8.GetBytes(effect));
				}
				else
					stream = new MemoryStream((byte[])resourceManager.GetObject(name));
				return ShaderFromStream(name, entryFunction, profile, null, resourceManager, stream, flags, binary, (int)stream.Length) as T;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to load an shader from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="entryFunction">Name of the entry point function.</param>
		/// <param name="profile">Shader profile to use.</param>
		/// <param name="flags">Flags for shader compilation.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		/// <remarks>
		/// <para>The target parameter can be vs_1_1, vs_2_0, vs_2_a, vs_2_b, vs_2_sw, vs_3_0 or vs_3_sw for <see cref="GorgonLibrary.Graphics.VertexShader">vertex shaders</see> or ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, or ps_3_sw for <see cref="GorgonLibrary.Graphics.PixelShader">pixel shaders</see>.</para>
		/// </remarks>
		public static T FromResource(string name, string entryFunction, Version profile, ShaderCompileOptions flags, bool binary)
		{
			return FromResource(name, null, entryFunction, profile, flags, binary);
		}

		/// <summary>
		/// Function to load an shader from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>	
		/// <param name="entryFunction">Name of the entry point function.</param>
		/// <param name="profile">Shader profile to use.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="bytes">Size in bytes of the data.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		/// <remarks>
		/// <para>The target parameter can be vs_1_1, vs_2_0, vs_2_a, vs_2_b, vs_2_sw, vs_3_0 or vs_3_sw for <see cref="GorgonLibrary.Graphics.VertexShader">vertex shaders</see> or ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, or ps_3_sw for <see cref="GorgonLibrary.Graphics.PixelShader">pixel shaders</see>.</para>
		/// </remarks>
		public static T FromStream(string name, string entryFunction, Version profile, Stream stream, ShaderCompileOptions flags, int bytes, bool binary)
		{
			return ShaderFromStream(name, entryFunction, profile, null, null, stream, flags, binary, bytes) as T;
		}

		/// <summary>
		/// Function to save the shader to a stream.
		/// </summary>
		/// <param name="stream">Stream to send the shader into.</param>
		/// <param name="binary">TRUE to save as binary, FALSE to save as text.</param>
		public void Save(Stream stream, bool binary)
		{
			ShaderSerializer<T> serializer = null;		// Shader serializer.

			if (stream == null)
				throw new ArgumentNullException("stream");

			try
			{
				serializer = new ShaderSerializer<T>(this, stream);
				serializer.DontCloseStream = true;

				// Save to a binary file.
				serializer.Parameters["Binary"] = binary;

				serializer.Serialize();
			}
			finally
			{
				if (serializer != null)
					serializer.Dispose();
				serializer = null;
			}
		}

		/// <summary>
		/// Function to save the shader to a file.
		/// </summary>
		/// <param name="filename">Filename/path of the shader.</param>
		/// <param name="binary">TRUE to save as binary, FALSE to save as text.</param>
		public void Save(string filename, bool binary)
		{
			Stream stream = null;		// File stream.

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				stream = File.Open(filename, FileMode.Create, FileAccess.Write);
				Save(stream, binary);

				_isResource = false;
				_fileName = filename;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save the shader to a file system file.
		/// </summary>
		/// <param name="fileSystem">Filesystem to save the shader into.</param>
		/// <param name="filename">Name of the file.</param>
		/// <param name="binary">TRUE to save as binary, FALSE to save as text.</param>
		public void Save(FileSystem fileSystem, string filename, bool binary)
		{
			MemoryStream stream = null;		// File stream.
			byte[] data = null;				// Binary shader data.

			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				// Save to a memory stream.
				stream = new MemoryStream();
				Save(stream, binary);

				// Get the binary data.
				stream.Position = 0;
				data = stream.ToArray();

				// Save to the file system.
				fileSystem.WriteFile(filename, data);

				_isResource = false;
				_fileName = filename;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}
		
		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			// Do nothing.
		}

		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			// Do nothing.
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public override void ForceRelease()
		{
			DestroyShader();
		}

		/// <summary>
		/// Function to bind a function as the entry point to this shader.
		/// </summary>
		/// <param name="function">Function to bind.</param>
		/// <remarks>The function target must be one of vs_2_0, vs_2_a, vs_2_b, vs_3_0, ps_2_0, ps_2_a, ps_2_b, or ps_3_0.</remarks>
		public void SetFunction(ShaderFunction function)
		{
			if (Function != null)
				Function.Dispose();
			Function = function;
			DestroyShader();

			IsBinary = false;
			IsResource = false;
			Filename = string.Empty;
			ShaderSource = string.Empty;
			Parameters.Clear();

			if (function == null)
				return;

			IsBinary = true;
			function.Shader = this;
			CreateShader();
			GetParameters();
		}

		/// <summary>
		/// Function to compile the shader source code.
		/// </summary>
		/// <param name="functionName">Name of the function to compile.</param>
		/// <param name="target">Shader target version.</param>
		/// <param name="flags">Options to use for compilation.</param>
		/// <remarks>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.
		/// <para>The target parameter can be vs_1_1, vs_2_0, vs_2_a, vs_2_b, vs_2_sw, vs_3_0 or vs_3_sw for <see cref="GorgonLibrary.Graphics.VertexShader">vertex shaders</see> or ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, or ps_3_sw for <see cref="GorgonLibrary.Graphics.PixelShader">pixel shaders</see>.</para>
		/// </remarks>
		protected void CompileShaderImplementation(string functionName, Version target, ShaderCompileOptions flags)
		{
			string errors = string.Empty;							// Errors generated by shader compiler.
			string previousDir = string.Empty;						// Previous current directory.
			DX.DataStream data = null;								// Effect data.
			D3D9.EffectHandle functionHandle = null;				// Handle to the function.
			D3D9.EffectCompiler compiler = null;					// Effect compiler.
			D3D9.ShaderFlags d3dflags = (D3D9.ShaderFlags)flags;	// Shader flags.

			if (string.IsNullOrEmpty(functionName))
				throw new ArgumentNullException("functionName");

			if (target == null)
				throw new ArgumentNullException("target");

			if (string.IsNullOrEmpty(ShaderSource))
				throw new GorgonException(GorgonErrors.ShaderCompilationFailed, "The shader '" + Name + "' has no source code");

			try
			{
				if ((!string.IsNullOrEmpty(Filename)) && (File.Exists(Filename)))
				{
					previousDir = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(Filename)));
				}

				// Clean up the shader.
				DestroyShader();
				if (_function != null)
					_function.Dispose();
				_function = null;

				_isResource = false;
				IsBinary = false;

				try
				{
					compiler = new D3D9.EffectCompiler(ShaderSource, null, null, d3dflags, out errors);
					functionHandle = compiler.GetFunction(functionName);
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.ShaderCompilationFailed, "The shader '" + Name + "' had compilation errors.\n\nErrors:\n" + errors, ex);
				}
				if (functionHandle == null)
					throw new GorgonException(GorgonErrors.ShaderCompilationFailed, "The shader '" + Name + "' does not contain the function '" + functionName + "'.");
				_function = new ShaderFunction(functionName, this, compiler.CompileShader(functionHandle, ShaderProfile(target), d3dflags, out errors), ShaderProfile(target));
				CreateShader();
				GetParameters();
			}
			finally
			{
				if ((!string.IsNullOrEmpty(previousDir)) && (Directory.Exists(previousDir)))
					Directory.SetCurrentDirectory(previousDir);

				if (compiler != null)
					compiler.Dispose();

				if (data != null)
					data.Dispose();

				data = null;
				compiler = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseShader&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <param name="function">Function to bind to the shader as an entry point.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		protected BaseShader(string name, ShaderFunction function)
			: base(name)
		{
			_samplers = new SortedDictionary<string, D3D9.EffectHandle>();

			if (function != null)
				SetFunction(function);
		}
		#endregion
		
		#region ISerializable Members
		#region Properties.
		/// <summary>
		/// Property to return the filename of the serializable object.
		/// </summary>
		/// <value></value>
		public string Filename
		{
			get 
			{
				return _fileName;
			}
			protected set
			{
				if (value == null)
					value = string.Empty;
				_fileName = value;
			}
		}

		/// <summary>
		/// Property to return whether this object is an embedded resource.
		/// </summary>
		/// <value></value>
		public bool IsResource
		{
			get 
			{
				return _isResource;
			}
			protected set
			{
				_isResource = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(Serializer serializer)
		{
			OnWriteData(serializer);
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.ReadData(Serializer serializer)
		{
			OnReadData(serializer);
		}
		#endregion
		#endregion
	}
}
