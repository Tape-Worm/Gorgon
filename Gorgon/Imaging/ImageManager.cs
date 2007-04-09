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
// Created: Friday, May 19, 2006 10:15:53 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;
using Drawing = System.Drawing;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a manager for images.
	/// </summary>
	public class ImageManager
		: Manager<Image>, IDisposable
	{
		#region Methods.
		/// <summary>
		/// Function to return an image from a stream.
		/// </summary>
		/// <param name="name">Image name or path.</param>
		/// <param name="fileSystem">A file system that contains the image.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="bytes">Size in bytes of the image.</param>
		/// <param name="format">Format of the image buffer.</param>
		/// <param name="colorKey">Color key to use for transparency.</param>
		/// <returns>The object contained within the stream.</returns>
		protected override Image ImageFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, int width, int height, int bytes, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Image newImage = null;						// New image.
			ImageSerializer serializer = null;			// Serializer.
			string imagePath = string.Empty;			// Path to the image.

			try
			{
				// Get the path.
				imagePath = name;

				// Get the image name.
				name = Path.GetFileNameWithoutExtension(name);

				// If the image already exists, then return it.
				if (Contains(name))
					return this[name];

				// Create the Image.
				newImage = new Image(name, ImageType.Normal, 1, 1, ImageBufferFormats.BufferUnknown, resources != null, false);
				((ISerializable)newImage).Filename = imagePath;

				// Create image serializer.
				serializer = new ImageSerializer(newImage, stream);

				// Add parameters.
				serializer.Parameters.AddParameter<int>("byteSize", bytes);
				serializer.Parameters.AddParameter<int>("Width", width);
				serializer.Parameters.AddParameter<int>("Height", height);
				serializer.Parameters.AddParameter<ImageBufferFormats>("Format", format);
				serializer.Parameters.AddParameter<Drawing.Color>("ColorKey", colorKey);
				serializer.Deserialize();

				AddObject(newImage);

				return newImage;
			}
			catch (Exception ex)
			{
				if (newImage != null)
					newImage.Dispose();
				newImage = null;
				throw new CannotLoadException(name, typeof(Image), ex);
			}
			finally
			{
				if (serializer != null)
					serializer.Dispose();
				serializer = null;
			}
		}

		/// <summary>
		/// Function to clear the list.
		/// </summary>
		internal void Clear()
		{
			foreach (Image image in this)
				image.Dispose();
			ClearItems();
		}

		/// <summary>
		/// Function to add an image to the manager.
		/// </summary>
		/// <param name="image">Image to add.</param>
		internal void Add(Image image)
		{
			AddObject(image);
		}

		/// <summary>
		/// Function to load a Image from the disk.
		/// </summary>
		/// <param name="filename">Name and path of the file to load.</param>
		/// <returns>The loaded image.</returns>
		public Image FromFile(string filename)
		{
			return FromFile(filename, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to load a Image from the disk.
		/// </summary>
		/// <param name="filename">Name and path of the file to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The loaded image.</returns>
		public Image FromFile(string filename, ImageBufferFormats format)
		{
			return FromFile(filename, 0, 0, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load a Image from the disk.
		/// </summary>
		/// <param name="filename">Name and path of the file to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The loaded image.</returns>
		public Image FromFile(string filename, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromFile(filename, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to load a Image from disk.
		/// </summary>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The loaded image.</returns>
		public Image FromFile(string filename, int width, int height, ImageBufferFormats format)
		{
			return FromFile(filename, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load a Image from disk.
		/// </summary>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The loaded image.</returns>
		public Image FromFile(string filename, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream stream = null;			// File stream.

			try
			{
				// Open the stream.
				stream = File.OpenRead(filename);
				return ImageFromStream(filename, null, null,stream, false, width, height, -1, format, colorKey);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The image contained within the file system.</returns>
		public Image FromFileSystem(FileSystem fileSystem, string filename, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream stream = null;			// File stream.

			try
			{				
				// Open the stream.
				stream = new MemoryStream(fileSystem.ReadFile(filename));

				return ImageFromStream(filename, fileSystem, null, stream, false, width, height, -1, format, colorKey);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="width">Requested width of the Image.</param>
		/// <param name="height">Requested height of the Image.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The image contained within the file system.</returns>
		public Image FromFileSystem(FileSystem fileSystem, string filename, int width, int height, ImageBufferFormats format)
		{
			return FromFileSystem(fileSystem, filename, width, height, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <param name="colorKey">Color to use as transparent.</param>
		/// <returns>The image contained within the file system.</returns>
		public Image FromFileSystem(FileSystem fileSystem, string filename, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromFileSystem(fileSystem, filename, format, colorKey);
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <param name="format">Requested format of the Image.</param>
		/// <returns>The image contained within the file system.</returns>
		public Image FromFileSystem(FileSystem fileSystem, string filename, ImageBufferFormats format)
		{
			return FromFileSystem(fileSystem, filename, 0, 0, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load a Image from a file system.
		/// </summary>
		/// <param name="fileSystem">File system that contains the image.</param>
		/// <param name="filename">Filename of the Image to load.</param>
		/// <returns>The image contained within the file system.</returns>
		public Image FromFileSystem(FileSystem fileSystem, string filename)
		{
			return FromFileSystem(fileSystem, filename, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromResource(name, null, width, height, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, int width, int height, ImageBufferFormats format)
		{
			return FromResource(name, null, width, height, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromResource(name, null, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ImageBufferFormats format)
		{
			return FromResource(name, null, 0, 0, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name)
		{
			return FromResource(name, null, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ResourceManager resourceManager, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Stream memStream = null;			// Memory stream for the resource.
			Assembly assembly = null;			// Assembly object.
			Drawing.Bitmap imageData = null;	// Image data.

			try
			{
				// Default to the calling application resource manager.
				if (resourceManager == null)
				{
					// Extract the resource manager from the calling assembly.
					assembly = Assembly.GetEntryAssembly();
					resourceManager = new ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);
				}

				// Get the resource memory stream.
				imageData = (Drawing.Bitmap)resourceManager.GetObject(name);
				memStream = new MemoryStream();
				imageData.Save(memStream, Drawing.Imaging.ImageFormat.Png);
				memStream.Position = 0;
				return ImageFromStream(name, null, resourceManager, memStream, false, width, height, -1, format, colorKey);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (memStream != null)
					memStream.Dispose();
				memStream = null;
			}			
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ResourceManager resourceManager, int width, int height, ImageBufferFormats format)
		{
			return FromResource(name, resourceManager, width, height, format, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ResourceManager resourceManager, ImageBufferFormats format)
		{
			return FromResource(name, resourceManager, 0, 0, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ResourceManager resourceManager, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromResource(name, resourceManager, 0, 0, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from resources.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="resourceManager">Resource manager to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromResource(string name, ResourceManager resourceManager)
		{
			return FromResource(name, resourceManager, 0, 0, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromBitmap(string name, Drawing.Bitmap image, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			Image newImage = null;			// New Image.

			try
			{
				if (!Contains(name))
					newImage = CreateImage(name, width, height, format);					
				else
					newImage = this[name];

				// Copy the GDI image into our image.
				newImage.Copy(image, width, height, format, colorKey);
			}
			catch
			{
				if (newImage != null)
					newImage.Dispose();
				throw;
			}

			return newImage;			
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromBitmap(string name, Drawing.Bitmap image, int width, int height, ImageBufferFormats format)
		{
			return FromBitmap(name, image, width, height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color used for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromBitmap(string name, Drawing.Bitmap image, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return FromBitmap(name, image, image.Width, image.Height, ImageBufferFormats.BufferUnknown, colorKey);
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromBitmap(string name, Drawing.Bitmap image,ImageBufferFormats format)
		{
			return FromBitmap(name, image, image.Width, image.Height, format, Drawing.Color.FromArgb(0, 0, 0, 0));
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="image">Image to retrieve data from.</param>
		/// <returns>The loaded image.</returns>
		public Image FromBitmap(string name, Drawing.Bitmap image)
		{
			return FromBitmap(name, image, image.Width, image.Height, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0,0,0,0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>		
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int bytes, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return ImageFromStream(name, null, null, stream, false, width, height, bytes, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <param name="colorKey">Color to use for transparency.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int width, int height, ImageBufferFormats format, Drawing.Color colorKey)
		{
			return ImageFromStream(name, null, null, stream, false, width, height, -1, format, colorKey);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int bytes, int width, int height, ImageBufferFormats format)
		{
			return ImageFromStream(name, null, null, stream, false, width, height, bytes, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int width, int height, ImageBufferFormats format)
		{
			return ImageFromStream(name, null, null, stream, false, width, height, -1, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int bytes, int width, int height)
		{
			return ImageFromStream(name, null, null, stream, false, width, height, bytes, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="format">Format of the image.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, ImageBufferFormats format)
		{
			return ImageFromStream(name, null, null, stream, false, 0, 0, -1, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="width">New width of the image, 0 for original size.</param>
		/// <param name="height">New height of the image, 0 for original size.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int width, int height)
		{
			return ImageFromStream(name, null, null, stream, false, width, height, -1, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <param name="format">Format to use.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int bytes, ImageBufferFormats format)
		{
			return ImageFromStream(name, null, null, stream, false, 0, 0, bytes, format, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the resource.</param>
		/// <param name="stream">Stream to use for loading.</param>		
		/// <param name="bytes">Number of bytes for the image.</param>
		/// <returns>The loaded image.</returns>
		public Image FromStream(string name, Stream stream, int bytes)
		{
			return ImageFromStream(name, null, null, stream, false, 0, 0, bytes, ImageBufferFormats.BufferUnknown, Drawing.Color.FromArgb(0));
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="dynamic">TRUE if a dynamic image, FALSE if not.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		/// <param name="format">Format of the Image.</param>
		/// <returns>Newly created image.</returns>
		public Image CreateImage(string name, bool dynamic, int width, int height, ImageBufferFormats format)
		{
			Image newImage = null;		// New image.

			try
			{
				if (Contains(name))
					throw new ImageAlreadyLoadedException(name, null);

				newImage = new Image(name, dynamic ? ImageType.Dynamic : ImageType.Normal, width, height, format, false, true);

				AddObject(newImage);
			}
			catch
			{
				if (newImage != null)
					newImage.Dispose();
				newImage = null;

				throw;
			}

			return newImage;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		/// <param name="format">Format of the Image.</param>
		/// <returns>Newly created image.</returns>
		public Image CreateImage(string name, int width, int height, ImageBufferFormats format)			
		{
			return CreateImage(name, false, width, height, format);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="dynamic">TRUE if a dynamic image, FALSE if not.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>
		/// <returns>Newly created image.</returns>
		public Image CreateImage(string name, bool dynamic, int width, int height)
		{
			return CreateImage(name, dynamic, width, height, ImageBufferFormats.BufferUnknown);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the Image.</param>
		/// <param name="width">Width of the Image.</param>
		/// <param name="height">Height of the Image.</param>		
		/// <returns>Newly created image.</returns>
		public Image CreateImage(string name, int width, int height)
		{
			return CreateImage(name, false, width, height, ImageBufferFormats.BufferUnknown);
		}

		/// <summary>
		/// Function to remove an image from the list by name.
		/// </summary>
		/// <param name="imageName">Name of the image to remove.</param>
		public void Remove(string imageName)
		{			
			this[imageName].Dispose();
			RemoveItem(imageName);
		}

		/// <summary>
		/// Function to remove an image from the list by index.
		/// </summary>
		/// <param name="index">Index of the image to remove.</param>
		public void Remove(int index)
		{
			this[index].Dispose();
			RemoveItem(index);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ImageManager()
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
