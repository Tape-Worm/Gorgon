#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
using SharpUtilities;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object to encapsulate pixel and vertex shader functionality.
	/// </summary>
	public class Shader
		: NamedObject, IDisposable, IDeviceStateObject, ISerializable
	{
		#region Variables.
		private ShaderTechniqueList _techniques;					// Techniques.
		private D3D9.Effect _effect = null;							// Direct 3D effect.
		private ShaderParameterList _parameters;					// List of parameters.
		private ShaderTechnique _activeTechnique = null;			// Active technique.
		private string _fileName = string.Empty;					// Filename of the shader.
		private bool _isResource = false;							// Shader is a resource.
		private StringBuilder _shaderCode = null;					// Shader code.
		private byte[] _compiled = null;							// Compiled shader.
		private bool _isBinary;										// Flag to indicate that the shader is a binary.
		private bool _disposed = false;								// Flag to indicate whether this object is disposed already or not.
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
		/// Property to set or return whether the shader is a compiled binary or contains source.
		/// </summary>
		public bool IsCompiled
		{
			get
			{
				return ((_isBinary) || (_shaderCode.Length == 0));
			}
			set
			{
				_isBinary = value;
			}
		}

		/// <summary>
		/// Property to set or return the source code for the shader.
		/// </summary>
		public string ShaderSource
		{
			get
			{
				return _shaderCode.ToString();
			}
			set
			{
				string errors = string.Empty;				// Errors generated by shader compiler.
				DX.DataStream data = null;					// Effect data.
				D3D9.EffectCompiler compiler = null;		// Effect compiler.
				D3D9.ShaderFlags flags;						// Shader flags.

				if ((value == null) || (value == string.Empty))
					throw new ShaderNotValidException();

				try
				{
					// Add the code.
					_shaderCode.Length = 0;
					_shaderCode.Append(value);

					// Clean up the shader.
					if (_effect != null)
						_effect.Dispose();
					_effect = null;
					_techniques.Clear();
					_parameters.Clear();					
					_compiled = null;
					_isResource = false;
					_isBinary = false;


#if DEBUG
					flags = D3D9.ShaderFlags.Debug;
#else
					flags = D3D9.ShaderFlags.OptimizationLevel3;
#endif
					compiler = new D3D9.EffectCompiler(value, null, null, flags, out errors);
					data = compiler.CompileEffect(D3D9.ShaderFlags.None, out errors);
					_compiled = new byte[data.Length];
					data.Read(_compiled, 0, _compiled.Length);
					data.Position = 0;

					// Create the effect.
					_effect = D3D9.Effect.FromStream(Gorgon.Screen.Device, data, null, null, flags, null, out errors);

					// Add techniques and passes.
					_techniques.Add(this);
					_parameters.Add(this);

					// Assign the first valid technique.
					foreach (ShaderTechnique technique in _techniques)
					{
						ActiveTechnique = technique;
						if (ActiveTechnique != null)
							break;
					}
				}
				catch (Exception ex)
				{
					if (_effect != null)
						_effect.Dispose();

					throw new ShaderCompilerErrorException(_objectName, errors, ex);
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
		}

		/// <summary>
		/// Property to return whether this shader is a resource or not.
		/// </summary>
		public bool IsResource
		{
			get
			{
				return _isResource;
			}
		}

		/// <summary>
		/// Property to return the shader filename.
		/// </summary>
		public string Filename
		{
			get
			{
				return _fileName;
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

		/// <summary>
		/// Property to return the list of parameters.
		/// </summary>
		public ShaderParameterList Parameters
		{
			get
			{
				return _parameters;
			}
		}

		/// <summary>
		/// Property to set or return the active technique.
		/// </summary>
		public ShaderTechnique ActiveTechnique
		{
			get
			{
				return _activeTechnique;
			}
			set
			{
				// Validate.
				if (_effect == null)
					return;

				if (_techniques.Count == 0)
					throw new ShaderHasNoTechniquesException(_objectName);

				// Don't set the same technique.
				if (value == _activeTechnique)
					return;

				if (value != null)
				{
					// Ensure the technique is valid.
					if (_techniques[value.Name].Valid)
						_activeTechnique = value;
				}
				else
					_activeTechnique = null;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return an arbitrary object from a stream.
		/// </summary>
		/// <param name="name">Object name or path.</param>
		/// <param name="fileSystem">A file system that contains the object.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="bytes">Number of bytes to deserialize.</param>
		/// <returns>The object contained within the stream.</returns>
		protected static Shader ShaderFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, int bytes)
		{
			ShaderSerializer serializer = null;	// Shader serializer.
			Shader newShader = null;			// Shader object.
			string filePath = string.Empty;		// Path to the file.

			try
			{
				filePath = name;
				name = Path.GetFileNameWithoutExtension(name);

				// Return the shader if it already exists.
				if (ShaderCache.Shaders.Contains(name))
					return ShaderCache.Shaders[name];

				// Create a dummy shader object.
				newShader = new Shader(string.Empty, name, filePath, resources != null);

				// Create the serializer.
				serializer = new ShaderSerializer(newShader, stream);
				serializer.DontCloseStream = true;

				// Add whether we're binary or not.
				serializer.Parameters["Binary"] = isXML;

				// Add the size parameter.
				serializer.Parameters["byteSize"] = bytes;

				// Read the shader.
				serializer.Deserialize();

				// Add to the list.
				ShaderCache.Shaders.Add(newShader);
				return newShader;
			}
			catch (ShaderCompilerErrorException scEx)
			{
				// Throw compiler errors on the beginning of the chain.
				throw scEx;
			}
			catch (Exception ex)
			{
				if (newShader != null)
					newShader.Dispose();
				newShader = null;
				throw new CannotLoadException(name, typeof(Shader), ex);
			}
			finally
			{
				if (serializer != null)
					serializer.Dispose();
				serializer = null;
			}
		}

		/// <summary>
		/// Function to retrieve a D3D texture shader from this shader.
		/// </summary>
		/// <param name="function">Name of the function to use.</param>
		/// <returns>A new D3D texture shader.</returns>
		internal D3D9.ShaderBytecode GetShaderFunction(string function)
		{
			string errors = string.Empty;			// List of compile errors.
			string previousDir = string.Empty;		// Previous current directory.
			D3D9.EffectCompiler compiler = null;	// Effect compiler.
			D3D9.EffectHandle functionHandle = null;// Function handle.
			D3D9.ShaderFlags flags;					// Shader flags.

			if (string.IsNullOrEmpty(function))
				throw new ArgumentNullException("function");

			if (_shaderCode.Length == 0)
				throw new ShaderCannotCreateTextureShaderException("No shader code to compile.");

			try
			{
				// Compile the effect.
				if ((!string.IsNullOrEmpty(_fileName)) && (File.Exists(_fileName)))
				{
					previousDir = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(_fileName)));
				}

				// Get effect compiler.
#if DEBUG
				flags = D3D9.ShaderFlags.Debug;
#else
				flags = D3D9.ShaderFlags.OptimizationLevel3;
#endif
				compiler = new D3D9.EffectCompiler(_shaderCode.ToString(), null, null, flags, out errors);
				functionHandle = compiler.GetFunction(function);

				// Create code.
				return compiler.CompileShader(functionHandle, "tx_1_0", flags);
			}
			catch(Exception ex)
			{
				throw new ShaderCannotCreateTextureShaderException(errors, ex);
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
		/// <returns>The loaded shader.</returns>
		public static Shader FromFile(string filename)
		{
			if (string.Compare(Path.GetExtension(filename), ".fxb", true) == 0)
				return FromFile(filename, true);
			else
				return FromFile(filename, false);
		}

		/// <summary>
		/// Function to load a Shader from disk.
		/// </summary>
		/// <param name="filename">Filename of the Shader to load.</param>
		/// <param name="binary">TRUE if the file is binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static Shader FromFile(string filename, bool binary)
		{
			Stream stream = null;		// File stream.

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				stream = File.OpenRead(filename);
				return ShaderFromStream(filename, null, null, stream, binary, (int)stream.Length);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(filename, typeof(Shader), ex);
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
		/// <returns>The shader within the file system.</returns>
		public static Shader FromFileSystem(FileSystem fileSystem, string filename)
		{
			if (string.Compare(Path.GetExtension(filename), ".fxb", true) == 0)
				return FromFileSystem(fileSystem, filename, true);
			else
				return FromFileSystem(fileSystem, filename, false);
		}

		/// <summary>
		/// Function to return the shader from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="filename">File name and path of the shader.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The shader within the file system.</returns>
		public static Shader FromFileSystem(FileSystem fileSystem, string filename, bool binary)
		{
			Stream stream = null;		// File stream.

			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			try
			{
				stream = new MemoryStream(fileSystem.ReadFile(filename));
				return ShaderFromStream(filename, null, null, stream, binary, (int)stream.Length);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(filename, typeof(Shader), ex);
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
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static Shader FromResource(string name, ResourceManager resourceManager, bool binary)
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
				effect = resourceManager.GetObject(name).ToString();
				stream = new MemoryStream(Encoding.UTF8.GetBytes(effect));
				return ShaderFromStream(name, null, resourceManager, stream, binary, (int)stream.Length);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(name, typeof(Shader), ex);
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
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static Shader FromResource(string name, bool binary)
		{
			return FromResource(name, null, binary);
		}

		/// <summary>
		/// Function to load an shader from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>	
		/// <param name="bytes">Size in bytes of the data.</param>
		/// <param name="binary">TRUE if the shader is a compiled binary, FALSE if not.</param>
		/// <returns>The loaded shader.</returns>
		public static Shader FromStream(string name, Stream stream, int bytes, bool binary)
		{
			return ShaderFromStream(name, null, null, stream, binary, bytes);
		}

		/// <summary>
		/// Function to save the shader to a stream.
		/// </summary>
		/// <param name="stream">Stream to send the shader into.</param>
		/// <param name="binary">TRUE to save as binary, FALSE to save as text.</param>
		public void Save(Stream stream, bool binary)
		{
			ShaderSerializer serializer = null;		// Shader serializer.

			if (stream == null)
				throw new ArgumentNullException("stream");

			try
			{
				serializer = new ShaderSerializer(this, stream);
				serializer.DontCloseStream = true;

				// Save to a binary file.
				serializer.Parameters["Binary"] = binary;

				serializer.Serialize();
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(_objectName, GetType(), ex);
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
		/// Constructor.
		/// </summary>
		/// <param name="effectSource">Source code for the effect.</param>
		/// <param name="name">Name of the shader.</param>
		/// <param name="filename">Filename and path of the shader.</param>
		/// <param name="isResource">TRUE if the shader is a resource, FALSE if not.</param>
		internal Shader(string effectSource, string name, string filename, bool isResource)
			: base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(name);

			_shaderCode = new StringBuilder(4096);
			_isResource = isResource;
			_techniques = new ShaderTechniqueList();
			_parameters = new ShaderParameterList();
			if ((effectSource != null) && (effectSource != string.Empty))
				ShaderSource = effectSource;
            _fileName = filename;
			DeviceStateList.Add(this);
		}

		/// <summary>
		/// Function to create a shader.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		/// <param name="sourceCode">Code for the shader.</param>
		/// <returns>Newly created shader.</returns>
		public Shader(string name, string sourceCode)
			: this(sourceCode, name, string.Empty, false)
		{
			if (string.IsNullOrEmpty(sourceCode))
				throw new ArgumentNullException(sourceCode);

			if (ShaderCache.Shaders.Contains(name))
				throw new ShaderAlreadyExistsException(name);

			try
			{
				ShaderCache.Shaders.Add(this);
			}
			catch (Exception ex)
			{
				Dispose();
				throw new CannotCreateException(name, typeof(Shader), ex);
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					if (ShaderCache.Shaders.Contains(Name))
						ShaderCache.Shaders.Remove(Name);

					if (_effect != null)
						_effect.Dispose();

					DeviceStateList.Remove(this);
				}
				_disposed = true;
			}

			// Do unmanaged clean up.
			_effect = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			_effect.OnLostDevice();			
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public void DeviceReset()
		{
			_effect.OnResetDevice();			
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			DeviceLost();	
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		string ISerializable.Filename
		{
			get
			{
				return _fileName;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				_fileName = value;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(Serializer serializer)
		{		
			if (!serializer.Parameters.Contains("Binary"))
				throw new ShaderNotValidException();

			// Write to the stream.			
			if (((bool)serializer.Parameters["Binary"]) && (_compiled != null))
			{
				byte[] code = Encoding.UTF8.GetBytes(ShaderSource);		// Shader code.

				serializer.Write(string.Empty, _compiled, 0, _compiled.Length);
				_isBinary = true;
			}
			else
			{
				serializer.Write(string.Empty, ShaderSource);
				_isBinary = false;
			}

			
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
				throw new ShaderNotValidException();

			if (!serializer.Parameters.Contains("Binary"))
				throw new ShaderNotValidException();

			try
			{
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
					// Compile to the shader.
					_shaderCode.Length = 0;
					_compiled = code;

					// Clean up the present shader.
					if (_effect != null)
						_effect.Dispose();
					_effect = null;
					_techniques.Clear();
					_parameters.Clear();

					// Send to a memory stream and build from a stream.
					stream = new MemoryStream(code);
#if DEBUG
					flags = D3D9.ShaderFlags.Debug;
#else
					flags = D3D9.ShaderFlags.OptimizationLevel3;
#endif
					_effect = D3D9.Effect.FromStream(Gorgon.Screen.Device, stream, null, null, flags, null, out errors);

					// Add techniques and passes.
					_techniques.Add(this);
					_parameters.Add(this);

					// Assign the first valid technique.
					foreach (ShaderTechnique technique in _techniques)
					{
						ActiveTechnique = technique;
						if (ActiveTechnique != null)
							break;
					}

					_isBinary = true;
				}
				else
				{
					// Read source code and convert to a string.
					//serializer.Stream.Read(code, 0, size);
					ShaderSource = Encoding.UTF8.GetString(code);
				}
			}
			catch (ShaderCompilerErrorException scEx)
			{
				// Throw compiler errors back earlier so we can see them easier.
				throw scEx;
			}
			catch (Exception ex)
			{
				if (_effect != null)
					_effect.Dispose();
				_effect = null;

				throw new SerializerCannotDeserializeException(typeof(ISerializable).Name, "Shader", errors, ex);
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
