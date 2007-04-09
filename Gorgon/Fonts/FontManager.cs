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
// Created: Sunday, July 23, 2006 9:56:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.Graphics;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics.Fonts
{
	/// <summary>
	/// Object representing a manager for the fonts.
	/// </summary>
	public class FontManager
		: Manager<Font>
	{
		#region Variables.
		private static Font _defaultFont = null;			// Default font.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default font.
		/// </summary>
		public static Font DefaultFont
		{
			get
			{
				if (_defaultFont == null)
				{
					FontManager fontList = new FontManager();		// Create a font manager to load the font.

					// Load the default font.
					_defaultFont = fontList.FromResource("_GorgonFont_Arial_9_Bold", Properties.Resources.ResourceManager);
				}

				return _defaultFont;
			}
		}
		#endregion

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
		protected override Font RenderableFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, Image alternateImage)
		{
			ISerializer fontSerializer = null;		// Font serializer.
			Font newFont = null;					// New font object.
			string imageName = string.Empty;		// Image name.
			string fontPath = string.Empty;			// Path to the font.

			try
			{
				// Get the filename if this is a file stream.
				fontPath = name;

				// Create the font object.
				newFont = new Font("EmptyFont");
				((ISerializable)newFont).Filename = fontPath;

				// Open the file for reading.
				if (isXML)
				{
					fontSerializer = new XMLSerializer(newFont, stream);
					fontSerializer.Parameters.AddParameter<string>("FileImagePath", string.Empty);
				}
				else
					fontSerializer = new BinarySerializer(newFont, stream);

				if (resources != null)
					fontSerializer.Parameters.AddParameter<ResourceManager>("ResourceManager", resources);
				if (fileSystem != null)
					fontSerializer.Parameters.AddParameter<FileSystem>("FileSystem", fileSystem);

				// Don't close the underlying stream.
				fontSerializer.DontCloseStream = true;

				// Set the image parameters.
				if (alternateImage != null)
					fontSerializer.Parameters.AddParameter<Image>("Image", alternateImage);

				fontSerializer.Deserialize();
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(newFont.Name, typeof(Font), ex);
			}
			finally
			{
				if (fontSerializer != null)
					fontSerializer.Dispose();
				fontSerializer = null;
			}

			return newFont;
		}

		/// <summary>
		/// Function to return a font from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the font.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font contained within the stream.</returns>
		public Font FromStream(Stream stream, bool isXML, Image alternateImage)
		{
			return RenderableFromStream("@FontObject.", null, null, stream, isXML, alternateImage);
		}

		/// <summary>
		/// Function to return a font from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the font.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>		
		/// <returns>The font contained within the stream.</returns>
		public Font FromStream(Stream stream, bool isXML)
		{
			return RenderableFromStream("@FontObject.", null, null, stream, isXML, null);
		}

		/// <summary>
		/// Function to return a binary font from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the font.</param>
		/// <returns>The font contained within the stream.</returns>
		public Font FromStream(Stream stream)
		{
			return RenderableFromStream("@FontObject.", null, null, stream, false, null);
		}

		/// <summary>
		/// Function to load a font from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to load the font from.</param>
		/// <param name="fontPath">Filename/path to the font.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font stored in the file system.</returns>
		public Font FromFileSystem(FileSystem fileSystem, string fontPath, Image alternateImage)
		{
			Stream fontStream = null;				// Stream for the font.

			try
			{
				// Open the file for reading.
				fontStream = new MemoryStream(fileSystem.ReadFile(fontPath));

				// Open the file for reading.
				if (Path.GetExtension(fontPath).ToLower() == ".xml")
					return RenderableFromStream(fontPath, fileSystem, null, fontStream, true, alternateImage);
				else
					return RenderableFromStream(fontPath, fileSystem, null, fontStream, false, alternateImage);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(fontPath, typeof(Font), ex);
			}
			finally
			{
				if (fontStream != null)
					fontStream.Close();
				fontStream = null;
			}
		}

		/// <summary>
		/// Function to load a font from a file system.
		/// </summary>
		/// <param name="fileSystem">File system to load the font from.</param>
		/// <param name="fontPath">Filename/path to the font.</param>
		/// <returns>The font stored in the file system.</returns>
		public Font FromFileSystem(FileSystem fileSystem, string fontPath)
		{
			return FromFileSystem(fileSystem, fontPath, null);
		}
		
		/// <summary>
		/// Function to load a font.
		/// </summary>
		/// <param name="fontPath">Filename/path to the font.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font stored on the disk.</returns>
		public Font FromFile(string fontPath, Image alternateImage)
		{
			Stream fontStream = null;				// Stream for the font.

			try
			{
				// Open the file for reading.
				fontStream = File.OpenRead(fontPath);

				// Open the file for reading.
				if (Path.GetExtension(fontPath).ToLower() == ".xml")
					return RenderableFromStream(fontPath, null, null, fontStream, true, alternateImage);
				else
					return RenderableFromStream(fontPath, null, null, fontStream, false, alternateImage);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(fontPath, typeof(Font), ex);
			}
			finally
			{
				if (fontStream != null)
					fontStream.Close();
				fontStream = null;
			}			
		}

		/// <summary>
		/// Function to load a font.
		/// </summary>
		/// <param name="fontPath">Filename/path to the font.</param>
		/// <returns>The font stored on the disk.</returns>
		public Font FromFile(string fontPath)
		{
			return FromFile(fontPath, null);
		}

		/// <summary>
		/// Function to load a font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, ResourceManager resourceManager, bool isXML, Image alternateImage)
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
				resourceData = resourceManager.GetObject(fontName);

				// If this is a text file, then convert to a byte array.
				if (resourceData is string)
					stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(resourceData.ToString()));
				else
					stream = new MemoryStream((byte[])resourceManager.GetObject(fontName));

				return RenderableFromStream("@FontResource.", null, resourceManager, stream, isXML, alternateImage);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(fontName, typeof(Font), ex);
			}
			finally
			{
				if (stream != null)
					stream.Close();

				stream = null;
			}
		}

		/// <summary>
		/// Function to load a font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, ResourceManager resourceManager, bool isXML)
		{
			return FromResource(fontName, resourceManager, isXML, null);
		}

		/// <summary>
		/// Function to load a binary font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, ResourceManager resourceManager, Image alternateImage)
		{
			return FromResource(fontName, resourceManager, false, alternateImage);
		}

		/// <summary>
		/// Function to load a binary font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, ResourceManager resourceManager)
		{
			return FromResource(fontName, resourceManager, false, null);
		}

		/// <summary>
		/// Function to load a font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, bool isXML, Image alternateImage)
		{
			return FromResource(fontName, null, isXML, alternateImage);
		}

		/// <summary>
		/// Function to load a font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="isXML">TRUE if the file is in XML format, FALSE if binary.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, bool isXML)
		{
			return FromResource(fontName, null, isXML, null);
		}

		/// <summary>
		/// Function to load a binary font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName, Image alternateImage)
		{
			return FromResource(fontName, null, false, alternateImage);
		}

		/// <summary>
		/// Function to load a binary font from the embedded resources.
		/// </summary>
		/// <param name="fontName">Name of the font.</param>
		/// <returns>The font that was loaded from the embedded resource.</returns>
		public Font FromResource(string fontName)
		{
			return FromResource(fontName, null, false, null);
		}

		/// <summary>
		/// Function to add a font to the manager.
		/// </summary>
		/// <param name="font">Font to add.</param>
		public void Add(Font font)
		{
			AddObject(font);
		}

		/// <summary>
		/// Function to clear the fonts.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to remove a font by index.
		/// </summary>
		/// <param name="index">Index of the font.</param>
		public void Remove(int index)
		{
			this[index].FontImage = null;
			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a font by its key name.
		/// </summary>
		/// <param name="key">Name of the font.</param>
		public void Remove(string key)
		{
			this[key].FontImage = null;
			RemoveItem(key);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public FontManager()
			: base(true)
		{
		}
		#endregion
	}
}
