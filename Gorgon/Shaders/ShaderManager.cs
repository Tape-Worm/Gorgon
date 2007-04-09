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
// Created: Friday, September 22, 2006 1:05:54 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using SharpUtilities.IO;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics.Shaders
{
	/// <summary>
	/// Object representing a manager for shaders.
	/// </summary>
	public class ShaderManager
		: Manager<Shader>, IDisposable
    {
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
		protected override Shader ObjectFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, int bytes)
		{
			ShaderSerializer serializer = null;	// Shader serializer.
			Shader newShader = null;			// Shader object.
			string filePath = string.Empty;		// Path to the file.

			try
			{
				filePath = name;
				name = Path.GetFileNameWithoutExtension(name);

				// Return the shader if it already exists.
				if (Contains(name))
					return this[name];

				// Create a dummy shader object.
				newShader = new Shader(string.Empty, name, filePath, resources != null);

				// Create the serializer.
				serializer = new ShaderSerializer(newShader, stream);
				serializer.DontCloseStream = true;

				// Add the size parameter.
				serializer.Parameters.AddParameter<int>("byteSize", bytes);				

				// Read the shader.
				serializer.Deserialize();

				// Add to the list.
				AddObject(newShader);

				return newShader;
			}
			catch (SharpException sEx)
			{
				throw sEx;
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
		/// Function to clear the collection.
		/// </summary>
		internal void Clear()
		{
			foreach (Shader shader in this)
				shader.Dispose();

			ClearItems();
		}

		/// <summary>
		/// Function to add a shader to the manager.
		/// </summary>
		/// <param name="shader">Shader to add.</param>
		internal void Add(Shader shader)
		{
			AddObject(shader);
		}
				
		/// <summary>
        /// Function to return whether a key exists in the collection or not.
        /// </summary>
        /// <param name="key">Key of the object in the collection.</param>
        /// <returns>TRUE if the object exists, FALSE if not.</returns>
        public override bool Contains(string key)
        {
            return base.Contains(key.ToLower());
        }

		/// <summary>
		/// Function to create a shader.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		/// <param name="sourceCode">Code for the shader.</param>
		/// <returns>Newly created shader.</returns>
		public Shader Create(string name, string sourceCode)
		{
			Shader newShader = null;		// Shader object.

			try
			{
				if (Contains(name))
					throw new DuplicateObjectException(name);

				newShader = new Shader(sourceCode, name, string.Empty, false);

				AddObject(newShader);

				return newShader;
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				if (newShader != null)
					newShader.Dispose();
				newShader = null;

				throw new CannotCreateException(name, typeof(Shader), ex);
			}
		}

		/// <summary>
		/// Function to load a Shader from disk.
		/// </summary>
		/// <param name="filename">Filename of the Shader to load.</param>
		/// <returns>The loaded shader.</returns>
		public Shader FromFile(string filename)
		{
			Stream stream = null;		// File stream.

			try
			{
				stream = File.OpenRead(filename);
				return ObjectFromStream(filename, null, null, stream, false, (int)stream.Length);
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
		public Shader FromFileSystem(FileSystem fileSystem, string filename)
		{
			Stream stream = null;		// File stream.

			try
			{
				stream = new MemoryStream(fileSystem.ReadFile(filename));

				return ObjectFromStream(filename, null, null, stream, false, (int)stream.Length);
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
		/// <returns>The loaded shader.</returns>
		public Shader FromResource(string name, ResourceManager resourceManager)
		{
			Stream stream = null;			// Stream
			Assembly assembly = null;		// Assembly object.
			string effect = string.Empty;	// Effect code.

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
				return ObjectFromStream(name, null, resourceManager, stream, false, (int)stream.Length);
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
		/// <returns>The loaded shader.</returns>
		public Shader FromResource(string name)
		{
			return FromResource(name, null);
		}

		/// <summary>
		/// Function to load an shader from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>	
		/// <param name="bytes">Size in bytes of the data.</param>
		/// <returns>The loaded shader.</returns>
		public Shader FromStream(string name, Stream stream, int bytes)
		{
			return ObjectFromStream(name, null, null, stream, false, bytes);
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		public void Remove(int index)
		{
			this[index].Dispose();
			base.RemoveItem(index);
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		public void Remove(string key)
		{
			this[key].Dispose();
			base.RemoveItem(key);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ShaderManager()
			: base(false)
		{
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
				Clear();
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
	}
}
