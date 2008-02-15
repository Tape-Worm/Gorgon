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
// Created: Saturday, November 04, 2006 11:36:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing a standard disk based filesystem.
    /// </summary>
	[FileSystemInfo("Folder File System", false, false, "GORPACK1.GorgonFolderSystem")]
    public class FolderFileSystem
        : FileSystem
	{
		#region Properties.
		/// <summary>
		/// Property to return the header for the file system.
		/// </summary>
		/// <value></value>
		protected override string FileSystemHeader
		{
			get 
			{
				// We don't need one for this.
				return "GORPACK1.GorgonFolderSystem";
			}
		}

		/// <summary>
		/// Property to set or return the root path of the file system.
		/// </summary>
		public override string Root
		{
			get
			{
				return base.Root;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				// Append a path separator.
				if (!value.EndsWith(@"\"))
					value += @"\";
				
				base.Root = value;

				try
				{
					// Load the index file.
					if (!File.Exists(Root + "FileSystem.Index.xml"))
						throw new GorgonException("Cannot locate the file system index.");


					FileIndexXML.Load(Root + "FileSystem.Index.xml");
				}
				catch (Exception ex)
				{
					throw new FileSystemRootIsInvalidException(Root, ex);
				}

				// Validate the index XML.
				ValidateIndexXML();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="path">Path to add the file into.</param>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system file.</returns>
		protected override FileSystemFile EncodeData(FileSystemPath path, string filePath, byte[] data)
		{
			FileSystemFile file = null;			// New file system file.

			try
			{
				// Add the file.
				file = path.Files.Add(filePath, data, data.Length, 0, DateTime.Now, false);

				// This is very straight forward, the basic folder file system doesn't
				// need to do any special encoding.
				return file;
			}
			catch (Exception ex)
			{
				throw new CannotCreateException("Cannot encode " + file.FullPath + " to the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected override byte[] DecodeData(FileSystemFile file)
		{
			try
			{
				// This file system requires no conversion of data, so pass through.
				return file.Data;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + file.FullPath + " from the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to load.</param>
		protected override void Load(FileSystemFile file)
		{
			Stream stream = null;					// File stream.

			try
			{
				if (!File.Exists(Root + file.FullPath))
					throw new GorgonException("There is no file named '" + Root + file.FullPath + "' on the physical file system.");

				// Open the file.
				stream = File.Open(Root + file.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				file.Data = new byte[stream.Length];
				stream.Read(file.Data, 0, file.Data.Length);

				// Close the file.
				stream.Dispose();
				stream = null;

				// Fire the event.
				OnDataLoad(this, new FileSystemDataIOEventArgs(file));
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + file.FullPath + " from the file system.", ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save the file index.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		protected override void SaveIndex(string filePath)
		{
			BinaryWriter writer = null;		// Writer for header.
			Stream outStream = null;		// File stream for header.

			if ((filePath == string.Empty) || (filePath == null))
				throw new ArgumentNullException("filePath");

			// Append the path separator if necessary.
			if (filePath.EndsWith(@"\"))
				filePath = Path.GetDirectoryName(filePath);

			if (filePath == null)
				throw new FileSystemPathInvalidException();

			// Write out the header ID.
			try
			{
				outStream = File.Open(filePath + @"\Header.folderSystem", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				writer = new BinaryWriter(outStream, Encoding.UTF8);
				writer.Write(FileSystemHeader);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (outStream != null)
					outStream.Dispose();
				if (writer != null)
					writer.Close();

				outStream = null;
				writer = null;
			}

			FileIndexXML.Save(filePath + @"\FileSystem.Index.xml");
		}

		/// <summary>
		/// Function to save the file data.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		/// <param name="file">File to save.</param>
		protected override void SaveFileData(string filePath, FileSystemFile file)
		{
			Stream fileStream = null;		// File stream to the file.
			string path = string.Empty;		// File path.

			try
			{
				if ((filePath == string.Empty) || (filePath == null))
					throw new ArgumentNullException("filePath");

				if (filePath.EndsWith(@"\"))
					filePath = Path.GetDirectoryName(filePath);

				if (filePath == null)
					throw new FileSystemPathInvalidException();

				// Get the path to the file.
				path = filePath + Path.GetDirectoryName(file.FullPath);

				// Create the directory if needed.
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				fileStream = File.Open(filePath + file.FullPath, FileMode.Create, FileAccess.Write, FileShare.None);

				// Write.
				fileStream.Write(file.Data, 0, file.Data.Length);
			}
			catch
			{
				throw;
			}
			finally
			{
				// Clean up.
				if (fileStream != null)
					fileStream.Dispose();
				fileStream = null;
			}
		}
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this file system.</param>
		/// <param name="provider">File system provider.</param>
        public FolderFileSystem(string name, FileSystemProvider provider)
            : base(name, provider)
        {
        }
        #endregion
    }
}
