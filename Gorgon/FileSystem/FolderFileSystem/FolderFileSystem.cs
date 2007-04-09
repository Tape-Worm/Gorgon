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
	[FileSystemEditorInfo("Folder File System", false, false)]
    public class FolderFileSystem
        : FileSystem
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the root path of the file system.
		/// </summary>
		public override string RootPath
		{
			get
			{
				return base.RootPath;
			}
			set
			{
				base.RootPath = value;

				try
				{
					// Append a path separator.
					if (_root != string.Empty)
					{
						if (_root[_root.Length - 1].ToString() != @"\")
							_root += @"\";
					}

					// Load the index file.
					if (!File.Exists(RootPath + "FileSystem.Index.xml"))
						throw new Exception("Cannot locate the file system index.");

					_fileIndex.Load(RootPath + "FileSystem.Index.xml");
				}
				catch (Exception ex)
				{
					throw new InvalidRootException(_root, ex);
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
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system entry.</returns>
		protected override FileSystemEntry EncodeData(string filePath, byte[] data)
		{
			FileSystemEntry entry = null;		// File system entry.

			try
			{
				// This is very straight forward, the basic folder file system doesn't
				// need to do any special encoding.
				return _entries.AddEntry(filePath, data, 0, data.Length, 0, false);
			}
			catch (Exception ex)
			{
				throw new CannotCreateException("Cannot encode " + entry.FullPath + " to the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="entry">File system entry for the object.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected override byte[] DecodeData(FileSystemEntry entry)
		{
			try
			{
				// This file system requires no conversion of data, so pass through.
				return entry.ObjectData;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + entry.FullPath + " from the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="entry">File system entry for the object.</param>
		protected override void Load(FileSystemEntry entry)
		{
			Stream stream = null;					// File stream.

			try
			{
				if (!File.Exists(RootPath + entry.FullPath))
					throw new Exception("There is no file named '" + RootPath + entry.FullPath + "' on the physical file system.");

				// Open the file.
				stream = File.Open(RootPath + entry.FullPath, FileMode.Open);
				entry.ObjectData = new byte[stream.Length];
				stream.Read(entry.ObjectData, 0, entry.ObjectData.Length);

				// Close the file.
				stream.Dispose();
				stream = null;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + entry.FullPath + " from the file system.", ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to write the file system contents to a specified path.
		/// </summary>
		/// <param name="path">Path to write the file system contents into.</param>
		public override void Save(string path)
		{
			XmlElement fsElement = null;				// File system.
			XmlElement entry = null;					// File system entry.
			XmlElement entryProperty = null;			// Entry property.
			XmlAttribute entryPath = null;				// Full relative path.
			Stream fileStream = null;					// File stream.
			string filePath = string.Empty;				// File path.
			string directory = string.Empty;			// Directory.
			
			try
			{
				if ((path == string.Empty) || (path == null))
					throw new Exception("Path is empty.");

				// Append the path separator if necessary.
				if (path[path.Length - 1].ToString() != @"\")
					path += @"\";

				path = Path.GetDirectoryName(path);

				// Reset the XML directory.
				RebuildIndex();

				// Get the file system node.
				fsElement = (XmlElement)_fileIndex.SelectSingleNode("//FileSystem");

				if (fsElement == null)
					throw new Exception("File system is corrupt.");

				// Remove deleted entries.
				foreach (FileSystemEntry fileEntry in _deleted)
				{
					filePath = path + fileEntry.FullPath;

					// If the file exists, then destroy it.
					if (File.Exists(filePath))
						File.Delete(filePath);
				}

				// Remove all empty directories.
				// This is fairly complex because we have to determine which is the lowest
				// level on the tree.  This is problematic because the list can be out of 
				// order.  We could sort the list, but that would make too much sense.
				List<FileSystemEntry> directories = new List<FileSystemEntry>();		// Directories to delete.

				while (_deleted.Count > 0)
				{
					// Record empty directories.
					foreach (FileSystemEntry fileEntry in _deleted)
					{
						filePath = path + fileEntry.FullPath;

						// If the containing directory has no files in it, then 
						// remove it.					
						if ((fileEntry.Path == @"\") || ((Directory.GetFiles(Path.GetDirectoryName(filePath)).Length == 0) && (Directory.GetDirectories(Path.GetDirectoryName(filePath)).Length == 0)))
							directories.Add(fileEntry);
					}

					// Leave the loop if we can't delete anything else.
					if (directories.Count == 0)
						break;

					// Remove the physical directories.
					foreach (FileSystemEntry fileEntry in directories)
					{
						filePath = path + fileEntry.FullPath;
						// Don't remove root, we'll do that later.
						if ((fileEntry.Path != @"\") && (Directory.Exists(Path.GetDirectoryName(filePath))))
							Directory.Delete(Path.GetDirectoryName(filePath));
						_deleted.Remove(fileEntry.FullPath);
					}

					directories.Clear();
				}

				// Remove deleted entries.
				_deleted.Clear();

				// If the path doesn't exist, then create it.
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				// Add entries.
				foreach (FileSystemEntry fileEntry in this)
				{
					// Create the actual file path.
					filePath = fileEntry.FullPath;

					if (fileEntry.ObjectData != null)
					{
						// Create full path.
						entryPath = _fileIndex.CreateAttribute("RelativePath");
						entryPath.Value = fileEntry.Path + fileEntry.Filename + fileEntry.Extension;

						entry = _fileIndex.CreateElement("Entry");
						entry.Attributes.Append(entryPath);
						fsElement.AppendChild(entry);

						// Set the properties.
						entryProperty = _fileIndex.CreateElement("Path");
						entryProperty.InnerText = fileEntry.Path;
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("Filename");
						entryProperty.InnerText = fileEntry.Filename;
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("Extension");
						entryProperty.InnerText = fileEntry.Extension;
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("FileSize");
						entryProperty.InnerText = fileEntry.FileSize.ToString();
						entry.AppendChild(entryProperty);

						// Check for directory.
						directory = Path.GetDirectoryName(path + filePath);

						// Create necessary paths.
						if (!Directory.Exists(directory))
							Directory.CreateDirectory(directory);

						fileStream = File.Open(path + filePath, FileMode.Create);

						// Write.
						fileStream.Write(fileEntry.ObjectData, 0, fileEntry.ObjectData.Length);

						// Clean up.
						fileStream.Dispose();
						fileStream = null;
					}
				}

				// Save the index file.
				if (EntryCount > 0)
					_fileIndex.Save(path + @"\FileSystem.Index.xml");
				else
				{
					// Remove the file system if there aren't any entries.
					if (File.Exists(path + @"\FileSystem.Index.xml"))
						File.Delete(path + @"\FileSystem.Index.xml");

					// Remove the file system top level.
					if (Directory.Exists(path))
					{
						// Only remove if the directory is empty.
						if ((Directory.GetFiles(path).Length == 0) && (Directory.GetDirectories(path).Length == 0))
							Directory.Delete(path);
					}
				}
			}
			catch (Exception ex)
			{
				throw new CannotSaveException("Cannot save the file system state.", ex);
			}
			finally
			{
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
        public FolderFileSystem(string name)
            : base(name, null)
        {
        }
        #endregion
    }
}
