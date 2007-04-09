#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, January 06, 2007 2:10:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using SharpUtilities;
using SharpUtilities.Mathematics;
using SharpUtilities.Collections;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a manager for sprite objects.
	/// </summary>
	public class SpriteManager
		: Manager<Sprite>
	{
		#region Methods.
		/// <summary>
		/// Function to return a renderable from a stream.
		/// </summary>
		/// <param name="name">Renderable name or path.</param>
		/// <param name="fileSystem">A file system that contains the renderable.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The object contained within the stream.</returns>
		protected override Sprite RenderableFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, Image alternateImage)
		{
			ISerializer spriteSerializer = null;		// Sprite serializer.
			Sprite newSprite = null;					// New sprite object.
			string imageName = string.Empty;			// Image name.
			string spritePath = string.Empty;			// Path to the sprite.

			try
			{
				// Get the filename if this is a file stream.
				spritePath = name;

				// Create the sprite object.
				newSprite = new Sprite(name);
				((ISerializable)newSprite).Filename = spritePath;

				// Open the file for reading.
				if (isXML)
				{
					spriteSerializer = new XMLSerializer(newSprite, stream);
					spriteSerializer.Parameters.AddParameter<string>("FileImagePath", string.Empty);
				}
				else
					spriteSerializer = new BinarySerializer(newSprite, stream);

				if (resources != null)
					spriteSerializer.Parameters.AddParameter<ResourceManager>("ResourceManager", resources);
				if (fileSystem != null)
					spriteSerializer.Parameters.AddParameter<FileSystem>("FileSystem", fileSystem);

				// Don't close the underlying stream.
				spriteSerializer.DontCloseStream = true;

				// Set the image parameters.
				if (alternateImage != null)
					spriteSerializer.Parameters.AddParameter<Image>("Image", alternateImage);

				spriteSerializer.Deserialize();
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(newSprite.Name, typeof(Sprite), ex);
			}
			finally
			{
				if (spriteSerializer != null)
					spriteSerializer.Dispose();
				spriteSerializer = null;
			}

			return newSprite;			
		}

		/// <summary>
		/// Function to return a sprite from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite contained within the stream.</returns>
		public Sprite FromStream(Stream stream, bool isXML, Image alternateImage)
		{
			return RenderableFromStream("@SpriteObject.", null, null, stream, isXML, alternateImage);
		}

		/// <summary>
		/// Function to return a sprite from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>		
		/// <returns>The sprite contained within the stream.</returns>
		public Sprite FromStream(Stream stream, bool isXML)
		{
			return RenderableFromStream("@SpriteObject.", null, null, stream, isXML, null);
		}

		/// <summary>
		/// Function to return a binary sprite from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the sprite.</param>
		/// <returns>The sprite contained within the stream.</returns>
		public Sprite FromStream(Stream stream)
		{
			return RenderableFromStream("@SpriteObject.", null, null, stream, false, null);
		}

		/// <summary>
		/// Function to return a sprite from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="path">Path and filename of the sprite.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite contained within the file system.</returns>
		public Sprite FromFileSystem(FileSystem fileSystem, string path, Image alternateImage)
		{
			MemoryStream stream = null;		// Stream that contains the sprite data.

			try
			{
				// Extract the object data from the file system.
				stream = new MemoryStream(fileSystem.ReadFile(path));
				
				// Open the file for reading.
				if (fileSystem[path].Extension.ToLower() == ".xml")
					return RenderableFromStream(path, fileSystem, null, stream, true, alternateImage);
				else
					return RenderableFromStream(path, fileSystem, null, stream, false, alternateImage);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(path, typeof(Sprite), ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to return a sprite from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to use.</param>
		/// <param name="path">Path and filename of the sprite.</param>
		/// <returns>The sprite contained within the file system.</returns>
		public Sprite FromFileSystem(FileSystem fileSystem, string path)
		{
			return FromFileSystem(fileSystem, path, null);
		}
		
		/// <summary>
		/// Function to load a sprite.
		/// </summary>
		/// <param name="spritePath">Filename/path to the sprite.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite stored on the disk.</returns>
		public Sprite FromFile(string spritePath, Image alternateImage)
		{
			Stream spriteStream = null;				// Stream for the sprite.

			try
			{
				// Open the file for reading.
				spriteStream = File.OpenRead(spritePath);

				// Open the file for reading.
				if (Path.GetExtension(spritePath).ToLower() == ".xml")
					return RenderableFromStream(spritePath, null, null, spriteStream, true, alternateImage);
				else
					return RenderableFromStream(spritePath, null, null, spriteStream, false, alternateImage);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(spritePath, typeof(Sprite), ex);
			}
			finally
			{
				if (spriteStream != null)
					spriteStream.Dispose();
				spriteStream = null;
			}
		}

		/// <summary>
		/// Function to load a sprite.
		/// </summary>
		/// <param name="spritePath">Filename/path to the sprite.</param>
		/// <returns>The sprite stored on the disk.</returns>
		public Sprite FromFile(string spritePath)
		{
			return FromFile(spritePath, null);
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, ResourceManager resourceManager, bool isXML, Image alternateImage)
		{
			MemoryStream stream = null;						// Memory stream.
			Assembly assembly = null;						// Assembly that holds the resource manager.
			object resourceData = null;						// Resource data.

			try
			{
				// Default to the calling application resource manager.
				if (resourceManager == null)
				{
					// Extract the resource manager from the calling assembly.
					assembly = Assembly.GetEntryAssembly();
					resourceManager = new ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);
				}

				// Get object from memory stream.
				resourceData = resourceManager.GetObject(spriteName);

				// If this is a text file, then convert to a byte array.
				if (resourceData is string)
					stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(resourceData.ToString()));
				else
					stream = new MemoryStream((byte[])resourceManager.GetObject(spriteName));

				return RenderableFromStream("@SpriteResource.", null, resourceManager, stream, isXML, alternateImage);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(spriteName, typeof(Sprite), ex);
			}
			finally
			{
				if (stream != null)
					stream.Close();

				stream = null;
			}
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, ResourceManager resourceManager, bool isXML)
		{
			return FromResource(spriteName, resourceManager, isXML, null);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, ResourceManager resourceManager, Image alternateImage)
		{
			return FromResource(spriteName, resourceManager, false, alternateImage);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, ResourceManager resourceManager)
		{
			return FromResource(spriteName, resourceManager, false, null);
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, bool isXML, Image alternateImage)
		{
			return FromResource(spriteName, null, isXML, alternateImage);
		}

		/// <summary>
		/// Function to load a sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, bool isXML)
		{
			return FromResource(spriteName, null, isXML, null);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName, Image alternateImage)
		{
			return FromResource(spriteName, null, false, alternateImage);
		}

		/// <summary>
		/// Function to load a binary sprite from the embedded resources.
		/// </summary>
		/// <param name="spriteName">Name of the sprite.</param>
		/// <returns>The sprite that was loaded from the embedded resource.</returns>
		public Sprite FromResource(string spriteName)
		{
			return FromResource(spriteName, null, false, null);
		}

		/// <summary>
		/// Function to add a sprite to the manager.
		/// </summary>
		/// <param name="sprite">Sprite object to add.</param>
		public void Add(Sprite sprite)
		{
			AddObject(sprite);
		}

		/// <summary>
		/// Function to clear the sprites.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to remove a sprite by index.
		/// </summary>
		/// <param name="index">Index of the sprite.</param>
		public void Remove(int index)
		{
			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a sprite by its key name.
		/// </summary>
		/// <param name="key">Name of the sprite.</param>
		public void Remove(string key)
		{
			RemoveItem(key);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public SpriteManager()
			: base(true)
		{
		}
		#endregion
	}
}
