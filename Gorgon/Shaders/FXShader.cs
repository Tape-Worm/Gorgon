#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Friday, September 22, 2006 1:03:06 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// HLSL Effects framework shader object.
	/// </summary>
	public class FXShader
		: Shader, ISerializable
	{
		#region Variables.
		private bool _isResource = false;							// Flag to indicate that this object is an embedded resource.
		private ShaderTechniqueList _techniques;					// Techniques.
		private D3D9.Effect _effect = null;							// Direct 3D effect.
		private string _fileName = string.Empty;					// Filename of the shader.
		private bool _disposed = false;								// Flag to indicate whether this object is disposed already or not.
		private ShaderTechnique _currentTechnique = null;			// First valid technique.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the effect.
		/// </summary>
		internal D3D9.Effect D3DEffect
		{
		    get
		    {
		        return _effect;
		    }
		}

		/// <summary>
		/// Property to return a list of techniques.
		/// </summary>
		public ShaderTechniqueList Techniques
		{
			get
			{
				return _techniques;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the parameters for a shader.
		/// </summary>
		protected override void GetParameters()
		{
			ShaderParameter newParameter = null;			// Shader parameter.

			// If there aren't any parameters, then exit.
			if (_effect.Description.Parameters < 1)
				return;

			// Get technique handle.
			for (int i = 0; i < _effect.Description.Parameters; i++)
			{
				D3D9.EffectHandle handle = null;											// Handle.
				D3D9.ParameterDescription description = default(D3D9.ParameterDescription);	// Description.

				// Get parameter handle.
				try
				{
					handle = _effect.GetParameter(null, i);
					description = _effect.GetParameterDescription(handle);
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.CannotCreate, "Error trying to retrieve the shader parameters.",ex);
				}			

				// Get the parameter.
				newParameter = new ShaderParameter(description, handle, this, i);

				// Update collection.
				Parameters.Add(newParameter);

				// Check technique.
				foreach (ShaderTechnique technique in Techniques)
				{
					((IShaderRenderer)technique).GetDefinedConstants(); 
					if (_effect.IsParameterUsed(handle, technique.D3DEffectHandle))
						technique.Parameters.Add(new ShaderParameter(description, handle, this, i));

					foreach (ShaderPass pass in technique.Passes)
						((IShaderRenderer)pass).GetDefinedConstants();
				}
			}

			((IShaderRenderer)this).GetDefinedConstants();
		}

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			if (_effect != null)
			{
				if (_currentTechnique == null)
				{
					for (int i = 0; i < _techniques.Count; i++)
					{
						if (_techniques[i].Valid)
						{
							_currentTechnique = _techniques[i];
							break;
						}
					}

					if (_currentTechnique == null)
						throw new GorgonException(GorgonErrors.CannotUpdate, "Unable to find a valid technique for shader '" + Name + "'.");
				}

				// NOTE: Note to self, we have to set the technique BEFORE calling Begin(), or else the handles become invalidated.
				_effect.Technique = _currentTechnique.D3DEffectHandle;				
				_effect.Begin(D3D9.FX.None);
			}
		}

		/// <summary>
		/// Function called when rendering with this shader.
		/// </summary>
		/// <param name="primitiveStyle">Type of primitive to render.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		protected override void OnRender(PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount)
		{
			if ((_effect != null) && (_currentTechnique != null))
			{
				for (int i = 0; i < _currentTechnique.Passes.Count; i++)
				{
					_effect.BeginPass(i);
					Gorgon.Renderer.DrawCachedTriangles(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
					_effect.EndPass();
				}
			}
		}

		/// <summary>
		/// Function called after the rendering ends with this shader.
		/// </summary>
		protected override void OnRenderEnd()
		{
			if (_effect != null)
				_effect.End();
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (!_disposed)
				{
					if (_effect != null)
						_effect.Dispose();
				}
				_disposed = true;
			}

			// Do unmanaged clean up.
			_effect = null;
		}

		/// <summary>
		/// Function to return an arbitrary object from a stream.
		/// </summary>
		/// <param name="name">Object name or path.</param>
		/// <param name="fileSystem">A file system that contains the object.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="bytes">Number of bytes to deserialize.</param>
		/// <returns>The object contained within the stream.</returns>
		protected static FXShader ShaderFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, ShaderCompileOptions flags, bool isXML, int bytes)
		{
			FXShaderSerializer serializer = null;	// Shader serializer.
			FXShader newShader = null;			// Shader object.
			string filePath = string.Empty;		// Path to the file.

			try
			{
				filePath = name;
				name = Path.GetFileNameWithoutExtension(name);

				// Return the shader if it already exists.
				if (ShaderCache.Shaders.Contains(name))
					return ShaderCache.Shaders[name] as FXShader;

				// Create a dummy shader object.
				newShader = new FXShader(name);
				newShader._fileName = filePath;
				newShader._isResource = (resources != null);

				// Create the serializer.
				serializer = new FXShaderSerializer(newShader, stream);
				serializer.DontCloseStream = true;

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
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			if (_effect != null)
				_effect.OnLostDevice();
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			if (_effect != null)
				_effect.OnResetDevice();
		}

		/// <summary>
		/// Function to compile the shader source code.
		/// </summary>
		/// <param name="flags">Options to use for compilation.</param>
		/// <remarks>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.</remarks>
		public void CompileShader(ShaderCompileOptions flags)
		{
			string errors = string.Empty;							// Errors generated by shader compiler.
			DX.DataStream data = null;								// Effect data.
			D3D9.EffectCompiler compiler = null;					// Effect compiler.
			D3D9.ShaderFlags d3dflags = (D3D9.ShaderFlags)flags;	// Shader flags.

			if (ShaderSource == string.Empty)
				return;

			try
			{
				// Clean up the shader.
				if (_effect != null)
					_effect.Dispose();
				_effect = null;
				_techniques.Clear();
				Parameters.Clear();
				Compiled = null;
				_isResource = false;
				IsBinary = false;
				
				compiler = new D3D9.EffectCompiler(ShaderSource, null, null, d3dflags, out errors);
				data = compiler.CompileEffect(d3dflags, out errors);

				Compiled = new byte[data.Length];
				data.Read(Compiled, 0, Compiled.Length);
				data.Position = 0;

				// Create the effect.
				_effect = D3D9.Effect.FromStream(Gorgon.Screen.Device, data, null, null, null, d3dflags, null, out errors);

				// Add techniques and passes.
				_techniques.Add(this);
				GetParameters();
			}
			catch (Exception ex)
			{
				if (_effect != null)
					_effect.Dispose();

				throw GorgonException.Repackage(GorgonErrors.ShaderCompilationFailed, "The shader '" + Name + "' had compilation errors.\n\nErrors:\n" + errors, ex);
			}
			finally
			{
				if (compiler != null)
					compiler.Dispose();

				if (data != null)
					data.Dispose();

				data = null;
				compiler = null;
			}
		}

		/// <summary>
		/// Function to retrieve a shader function.
		/// </summary>
		/// <param name="function">Name of the function.</param>
		/// <param name="shaderTarget">Target for the shader function.</param>
		/// <param name="flags">Compilation options for the shader function.</param>
		/// <returns>The extracted function from the shader.</returns>
		/// <remarks>The shaderTarget parameter expects one of the following:
		/// <list type="table">
		///		<listheader><term>Target value</term><description>Description</description></listheader>
		///		<item><term>ps_major_minor</term><description>Pixel shader, where major is the major version number and minor is the minor version number (or letter).</description></item>
		///		<item><term>vs_major_minor</term><description>Vertex shader, where major is the major version number and minor is the minor version number (or letter).</description></item>
		///		<item><term>tx_1_0</term><description>Texture shader 1.0.</description></item>
		/// </list>
		/// <para>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.</para>
		/// </remarks>		
		public ShaderFunction GetShaderFunction(string function, string shaderTarget, ShaderCompileOptions flags)
		{
			string errors = string.Empty;			// List of compile errors.
			string previousDir = string.Empty;		// Previous current directory.
			D3D9.EffectCompiler compiler = null;	// Effect compiler.
			D3D9.EffectHandle functionHandle = null;// Function handle.
			D3D9.ShaderBytecode byteCode = null;	// Shader byte code.
			D3D9.ShaderFlags d3dflags;				// Shader flags.

			if (string.IsNullOrEmpty(function))
				throw new ArgumentNullException("function");

			if (string.IsNullOrEmpty(shaderTarget))
				throw new ArgumentNullException("shaderTarget");

			if (string.IsNullOrEmpty(ShaderSource))
				throw new GorgonException(GorgonErrors.ShaderCompilationFailed, "The shader '" + Name + "' has no souce to compile.");

			try
			{
				d3dflags = (D3D9.ShaderFlags)flags;
				if ((!string.IsNullOrEmpty(_fileName)) && (File.Exists(_fileName)))
				{
					previousDir = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(_fileName)));
				}

				try
				{
					compiler = new D3D9.EffectCompiler(ShaderSource, null, null, d3dflags, out errors);
					functionHandle = compiler.GetFunction(function);
					byteCode = compiler.CompileShader(functionHandle, shaderTarget, d3dflags, out errors);
				}
				catch (Exception ex)
				{
					throw GorgonException.Repackage(GorgonErrors.ShaderCompilationFailed, "The shader '" + Name + "' had compilation errors.\n\nErrors:\n" + errors, ex);
				}

				return new ShaderFunction(function, this, byteCode, shaderTarget);
			}
			finally
			{
				if ((!string.IsNullOrEmpty(previousDir)) && (Directory.Exists(previousDir)))
					Directory.SetCurrentDirectory(previousDir);

				if (compiler != null)
					compiler.Dispose();
				compiler = null;
			}
		}		

		/// <summary>
		/// Function to load a Shader from disk.
		/// </summary>
		/// <param name="filename">Filename of the Shader to load.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <returns>The loaded shader.</returns>
		public static FXShader FromFile(string filename, ShaderCompileOptions flags)
		{
			if (string.Compare(Path.GetExtension(filename), ".fxo", true) == 0)
				return FromFile(filename, flags, true);
			else
				return FromFile(filename, flags, false);
		}


		/// <summary>
		/// Function to load a Shader from disk.
		/// </summary>
		/// <param name="filename">Filename of the Shader to load.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="binary">TRUE if the file is binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static FXShader FromFile(string filename, ShaderCompileOptions flags, bool binary)
		{
			Stream stream = null;		// File stream.

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				return ShaderFromStream(filename, null, null, stream, flags, binary, (int)stream.Length);
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
		/// <param name="flags">Shader compilation flags.</param>
		/// <returns>The shader within the file system.</returns>
		public static FXShader FromFileSystem(FileSystem fileSystem, string filename, ShaderCompileOptions flags)
		{
			if (string.Compare(Path.GetExtension(filename), ".fxo", true) == 0)
				return FromFileSystem(fileSystem, filename, flags, true);
			else
				return FromFileSystem(fileSystem, filename, flags, false);
		}

		/// <summary>
		/// Function to return the shader from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="filename">File name and path of the shader.</param>
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The shader within the file system.</returns>
		public static FXShader FromFileSystem(FileSystem fileSystem, string filename, ShaderCompileOptions flags, bool binary)
		{
			Stream stream = null;		// File stream.

			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				stream = new MemoryStream(fileSystem.ReadFile(filename));
				return ShaderFromStream(filename, null, null, stream, flags, binary, (int)stream.Length);
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
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static FXShader FromResource(string name, ResourceManager resourceManager, ShaderCompileOptions flags, bool binary)
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
				return ShaderFromStream(name, null, resourceManager, stream, flags, binary, (int)stream.Length);
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
		/// <param name="flags">Flags for shader compilation.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static FXShader FromResource(string name, ShaderCompileOptions flags, bool binary)
		{
			return FromResource(name, null, flags, binary);
		}

		/// <summary>
		/// Function to load an shader from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>	
		/// <param name="flags">Shader compilation flags.</param>
		/// <param name="bytes">Size in bytes of the data.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static FXShader FromStream(string name, Stream stream, ShaderCompileOptions flags, int bytes, bool binary)
		{
			return ShaderFromStream(name, null, null, stream, flags, binary, bytes);
		}

		/// <summary>
		/// Function to save the shader to a stream.
		/// </summary>
		/// <param name="stream">Stream to send the shader into.</param>
		/// <param name="binary">TRUE to save as binary, FALSE to save as text.</param>
		public void Save(Stream stream, bool binary)
		{
			FXShaderSerializer serializer = null;		// Shader serializer.

			if (stream == null)
				throw new ArgumentNullException("stream");

			try
			{
				serializer = new FXShaderSerializer(this, stream);
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
		public void Save(string filename)
		{
			if (string.Compare(Path.GetExtension(filename), ".fxo", true) == 0)
				Save(filename, true);
			else
				Save(filename, false);
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
				if (Path.GetExtension(filename) == string.Empty)
				{
					if (!binary)
						filename += ".fx";
					else
						filename += ".fxo";
				}
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
		public void Save(FileSystem fileSystem, string filename)
		{
			if (string.Compare(Path.GetExtension(filename), ".fxo", true) == 0)
				Save(fileSystem, filename, true);
			else
				Save(fileSystem, filename, false);
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
				if (Path.GetExtension(filename) == string.Empty)
				{
					if (!binary)
						filename += ".fx";
					else
						filename += ".fxo";
				}

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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FXShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public FXShader(string name)
			: base(name)
		{
			_techniques = new ShaderTechniqueList();
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		public string Filename
		{
			get
			{
				return _fileName;
			}
		}

		/// <summary>
		/// Property to return whether this object is an embedded resource.
		/// </summary>
		public bool IsResource
		{
			get
			{
				return _isResource;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(Serializer serializer)
		{		
			bool binary = false;			// Flag for binary mode.

			if (!serializer.Parameters.Contains("Binary"))
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer parameter 'Binary' is missing.");

			binary = (bool)serializer.Parameters["Binary"];

			if ((Compiled == null) && (binary))
				throw new GorgonException(GorgonErrors.CannotSave, "Cannot save a binary shader without compiled shader code.");

			if ((!binary) && (ShaderSource == string.Empty))
				throw new GorgonException(GorgonErrors.CannotSave, "Cannot save a non-binary shader without source code.");

			// Write to the stream.			
			IsBinary = binary;
			if (binary)
				serializer.Write(string.Empty, Compiled, 0, Compiled.Length);				
			else
				serializer.Write(string.Empty, ShaderSource);
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.ReadData(Serializer serializer)
		{
			byte[] code = null;			        // Shader code.
			int size = 0;				        // Size of shader in bytes.
			MemoryStream stream = null;			// Stream containing compiled data.
			string errors = string.Empty;		// List of errors returned from the compiler.
			string previousDir = string.Empty;	// Previous directory.
			D3D9.ShaderFlags flags;				// Shader flags.

			if (serializer.Parameters.Contains("byteSize"))
				size = (int)serializer.Parameters["byteSize"];
			else
				throw new GorgonException(GorgonErrors.CannotLoad, "The serializer is missing a parameter 'byteSize'");				

			if (!serializer.Parameters.Contains("Binary"))
				throw new GorgonException(GorgonErrors.CannotLoad, "The serializer is missing a parameter 'Binary'");

			if (!serializer.Parameters.Contains("flags"))
				throw new GorgonException(GorgonErrors.CannotLoad, "The serializer is missing a parameter 'flags'");

			try
			{
				flags = (D3D9.ShaderFlags)serializer.Parameters["flags"];

				// Compile the effect.
				if ((!string.IsNullOrEmpty(_fileName)) && (File.Exists(_fileName)))
				{
					previousDir = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(Filename)));
				}

				// Read in the data.			
				code = serializer.ReadBytes(string.Empty, size);

				if ((bool)serializer.Parameters["Binary"])
				{
					// Send to a memory stream and build from a stream.
					stream = new MemoryStream(code);
					try
					{
						_effect = D3D9.Effect.FromStream(Gorgon.Screen.Device, stream, null, null, null, flags, null, out errors);
					}
					catch (Exception ex)
					{
						throw GorgonException.Repackage(GorgonErrors.CannotLoad, "The shader '" + Name + "' had loading errors.\n\nErrors:\n" + errors, ex);
					}

					// Add techniques and passes.
					_techniques.Add(this);
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
					ShaderSource = Encoding.UTF8.GetString(code);
					CompileShader((ShaderCompileOptions)flags);
				}
			}
			catch
			{
				if (_effect != null)
					_effect.Dispose();
				_effect = null;

				throw;
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
		#endregion
	}
}
