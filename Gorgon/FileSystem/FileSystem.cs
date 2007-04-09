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
// Created: Thursday, November 02, 2006 9:17:23 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Abstract object representing a virtual filesystem.
	/// </summary>
	public abstract class FileSystem
        : NamedObject, IPlugIn, IEnumerable<FileSystemEntry>
	{
        #region Variables.
		/// <summary>File system entries.</summary>
        protected FileSystemEntryManager _entries = null;
		
		/// <summary>Deleted file system entries.</summary>
		protected FileSystemEntryManager _deleted = null;

        /// <summary>Plug-in entry point.</summary>
        protected PlugInEntryPoint _plugInEntry = null;

        /// <summary>Root path for the filesystem.</summary>
        protected string _root = string.Empty;

		/// <summary>XML file containing the directory and file list.</summary>
		protected XmlDocument _fileIndex = null;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the a file system entry.
		/// </summary>
		/// <param name="path">Path of the file system entry.</param>
		/// <returns>Object at the entry.</returns>
		public FileSystemEntry this[string path]
		{
			get
			{
				if (!_entries.Contains(path))
					throw new FileSystemEntryDoesNotExistException(path, null);

				return _entries[path];
			}
		}

		/// <summary>
		/// Property to return the number of file system entries in the file system.
		/// </summary>
		public int EntryCount
		{
			get
			{
				return _entries.Count;
			}
		}		

        /// <summary>
        /// Property to set or return the root path of the file system.
        /// </summary>
        public virtual string RootPath
        {
            get
            {
                return _root;
            }
            set
            {
				if ((value == null) || (value == string.Empty))
					throw new InvalidRootException("NULL", null);

				// Use standard separators
				value = value.Replace("/", @"\");

				// Add the file index.
				_fileIndex = new XmlDocument();

				// Remove all entries.
				ClearEntries();

				_root = value;
			}
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to reset the directory index XML.
		/// </summary>
		protected void RebuildIndex()
		{
			XmlProcessingInstruction xmlHeader = null;	// XML processing instruction.
			XmlComment comment = null;					// Comment node.
			XmlElement fsElement = null;				// File system.
			XmlElement header = null;					// Header.

			// Set up index file.
			_fileIndex.RemoveAll();

			// Create processing instruction.
			xmlHeader = _fileIndex.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\"");
			_fileIndex.AppendChild(xmlHeader);

			// Add comment.
			comment = _fileIndex.CreateComment("This is an index of files for the file system.  Do NOT modify by hand.");
			_fileIndex.AppendChild(comment);

			// Begin outer file system element.
			fsElement = _fileIndex.CreateElement("FileSystem");
			_fileIndex.AppendChild(fsElement);

			// Create header node.
			header = _fileIndex.CreateElement("Header");
			header.InnerText = "GORFS1.0";
			fsElement.AppendChild(header);
		}

		/// <summary>
		/// Function to validate the index XML file.
		/// </summary>
		protected void ValidateIndexXML()
		{
			XmlNodeList nodes = null;		// Xml node list.

			nodes = _fileIndex.SelectNodes("//Header");

			// Validate the index file.
			if ((nodes == null) || (nodes.Count == 0))
				throw new Exception("Invalid file system.  No header.");

			if (nodes[0].InnerText.ToLower() != "gorfs1.0")
				throw new Exception("Invalid file system.  Not a Gorgon file system.");
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="entry">File system entry for the object.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected abstract byte[] DecodeData(FileSystemEntry entry);
		
		/// <summary>
		/// Function clear the entry list.
		/// </summary>
		protected void ClearEntries()
		{
			// Remove all entries.
			_entries.Clear();
			_deleted.Clear();

			// Rebuild the index XML.
			RebuildIndex();		
		}

		/// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system entry.</returns>
		protected abstract FileSystemEntry EncodeData(string filePath, byte[] data);

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="entry">File system entry for the object.</param>
		protected abstract void Load(FileSystemEntry entry);
		
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		internal void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Function to set the file system name.
		/// </summary>
		/// <param name="name">Name of the file system.</param>
		internal void SetName(string name)
		{
			if ((name == null) || (name == string.Empty))
				throw new InvalidNameException();

			_objectName = name;
		}

		/// <summary>
		/// Function to adjust a pathname for use with the file system.
		/// </summary>
		/// <param name="path">Path to adjust.</param>
		/// <returns>Adjusted pathname.</returns>
		public static string FullPathName(string path)
		{
			// Replace alternate path separator.
			path = path.Replace("/", @"\");
			path = path.Replace(@"\\", @"\");

			if ((path == string.Empty) || (path == null))
				path = @"\";

			// Put root separator in.
			if (path[0] != '\\')
				path = @"\" + path;

			if (path.Length > 1)
			{
				// Append a separator.
				if (path[path.Length - 1] != '\\')
					path += @"\";
			}

			return path;
		}

		/// <summary>
		/// Function to validate a filename and path.
		/// </summary>
		/// <param name="path">Path to validate.</param>
		/// <param name="pathOnly">TRUE if we only want to validate the path, FALSE to validate everything.</param>
		/// <returns>TRUE if valid, FALSE if not.</returns>
		public static bool ValidateFilename(string path, bool pathOnly)
		{
			path = path.Replace("/", @"\");
			path = path.Replace(@"\\", @"\");

			// Check for a file name.
			if ((!pathOnly) && (Path.GetFileName(path) == string.Empty))
				return false;

			// Validate filename.
			if ((Path.GetFileName(path) != string.Empty) && (Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()) > -1))
				return false;

			// Validate file path.
			if ((Path.GetDirectoryName(path) != null) && (Path.GetDirectoryName(path) != string.Empty) && (Path.GetDirectoryName(path).IndexOfAny(Path.GetInvalidPathChars()) > -1))
				return false;

			return true;
		}

		/// <summary>
		/// Function to adjust a pathname for use with the file system.
		/// </summary>
		/// <param name="path">Path to adjust.</param>
		/// <returns>Adjusted pathname.</returns>
		public static string FullFileName(string path)
		{
			// Ensure that there's a file name.
			if (Path.GetFileName(path) == string.Empty)
				throw new InvalidFilenameException(null);

			// Replace alternate path separator.
			path = path.Replace("/", @"\");
			path = path.Replace(@"\\", @"\");

			// Put root separator in.
			if (path[0] != '\\')
				path = @"\" + path;

			return path;
		}

		/// <summary>
		/// Function to add a path and all files under it to the file system.
		/// </summary>
		/// <param name="path">Path to add.</param>
		/// <param name="recurse">TRUE to search sub directories, FALSE to only search the top level.</param>
		public void Mount(string path, bool recurse)
		{
			XmlNodeList nodes = null;				// XML node list.
			XmlNode entryProperties = null;			// Properties for the entry.
			FileSystemEntry entry = null;			// File system entry.
			int fileCompressedSize = 0;				// Compressed size of the file.
			int fileSize = 0;						// Size of the file.
			int fileOffset = 0;						// Offset of the file.
			bool fileEncrypted = false;				// Flag to indicate that the file is encrypted.
			string fileType = string.Empty;			// File type.
			string filePath = string.Empty;			// Path to the file.
			Stream stream = null;					// File stream.
			
			// Get the path name.
			path = FileSystem.FullPathName(path);

			if (!FileSystem.ValidateFilename(path, true))
				throw new InvalidPathException(path, null);

			try
			{
				// Get file system entries.
				if (recurse)
					nodes = _fileIndex.SelectNodes("//Entry[Path[starts-with(.,'" + path + "')]]");
				else
					nodes = _fileIndex.SelectNodes("//Entry[Path='" + path + "']");

				// Add each entry.
				foreach (XmlNode entryNode in nodes)
				{
					// Get relative path.
					if (entryNode.Attributes["RelativePath"] == null)
						throw new Exception("Invalid directory node '" + entryNode.Name + "'.");

					filePath = entryNode.Attributes["RelativePath"].Value;

					// Get file size property.
					entryProperties = entryNode.SelectSingleNode("FileSize");

					if (entryProperties != null)
						fileSize = Convert.ToInt32(entryProperties.InnerText);
					else
						throw new Exception("Invalid directory node '" + entryNode.Name + "'.\nFile size is invalid.");
					
					// Get compressed file size property.
					entryProperties = entryNode.SelectSingleNode("CompressedFileSize");

					if (entryProperties != null)
						fileCompressedSize = Convert.ToInt32(entryProperties.InnerText);

					// Get file offset property.
					entryProperties = entryNode.SelectSingleNode("FileOffset");

					if (entryProperties != null)
						fileOffset = Convert.ToInt32(entryProperties.InnerText);

					// Get file offset property.
					entryProperties = entryNode.SelectSingleNode("Encrypted");

					if (entryProperties != null)
						fileEncrypted = (entryProperties.InnerText.ToLower() == "true") ? true : false;

					// Add the entry.
					entry = _entries.AddEntry(entryNode.Attributes["RelativePath"].InnerText, null, fileOffset, fileSize, fileCompressedSize, fileEncrypted);

					// Load the binary data for the entry.
					Load(entry);

					// If this matches a deleted item, remove it.
					if (_deleted.Contains(entry.FullPath))
						_deleted.Remove(entry.FullPath);
				}
			}
			catch (Exception ex)
			{
				throw new CannotMountFilePathException("Cannot mount the path " + path + ".", ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to mount all directories in the file system.
		/// </summary>
		public void Mount()
		{
			Mount(@"\", true);
		}

        /// <summary>
        /// Function to add a path and all files under it to the file system.
        /// </summary>
        /// <param name="path">Path to add.</param>
        public void Mount(string path)
        {
            Mount(path, false);
        }

        /// <summary>
        /// Function to remove a path from the file system.
        /// </summary>
        /// <param name="path">Path to remove.</param>
		/// <param name="recurse">TRUE to remove all paths under the path, FALSE to only remove the path.</param>
        public void Unmount(string path, bool recurse)
        {
			string entryPath = string.Empty;		// Entry path.
			List<string> removed = null;			// Removed entries.

			path = FileSystem.FullPathName(path);

			if (!FileSystem.ValidateFilename(path, true))
				throw new InvalidPathException(path, null);

			removed = new List<string>();

			// Remove entries.
			foreach (FileSystemEntry entry in this)
			{
				entryPath = entry.Path.ToLower();
	
				// We need to add the deleted files to a list
				// because if we delete them within the loop
				// the collection will be modified and thus
				// mess up the enumerator.
				if (!recurse)
				{
					if (entryPath == path)
						removed.Add(entry.FullPath);						
				}
				else
				{
					if (entryPath.StartsWith(path))
						removed.Add(entry.FullPath);
				}
			}

			// Perform delete.
			for (int i = 0; i < removed.Count; i++)
				Delete(removed[i]);
        }

		/// <summary>
		/// Function to remove a path from the file system.
		/// </summary>
		/// <param name="path">Path to remove.</param>
		public void Unmount(string path)
		{
			Unmount(path, true);
		}

        /// <summary>
        /// Function to clear all the paths from the file system.
        /// </summary>
        public void Unmount()
        {
			Unmount(@"\", true);
        }

		/// <summary>
		/// Function to copy the data and entries from a source file system.
		/// </summary>
		/// <param name="sourceFileSystem">File system to copy.</param>
		public void CopyFileSystem(FileSystem sourceFileSystem)
		{
			try
			{
				// Remove all entries and reset the root.
				ClearEntries();
				_root = string.Empty;

				// Add the entries and the data.
				foreach (FileSystemEntry entry in sourceFileSystem)
				{
					// If the object isn't loaded, then load it.
					if (entry.ObjectData != null)
						WriteFile(entry.FullPath, sourceFileSystem.DecodeData(entry));
					else
						throw new Exception("Unable to copy '" + entry.FullPath + "', no object was created.");
				}
			}
			catch (Exception ex)
			{
				throw new CannotCopyFileSystemException(sourceFileSystem, ex);
			}
		}

		/// <summary>
		/// Function to move a file into a new path.
		/// </summary>
		/// <param name="entry">File to move.</param>
		/// <param name="newPath">Path to move into.</param>
		public void MoveEntry(FileSystemEntry entry, string newPath)
		{
			string oldPath = string.Empty;		// Old path.

			if (entry == null)
				throw new ArgumentNullException("entry");

			// Fix path.
			newPath = FileSystem.FullPathName(newPath);
			oldPath = entry.FullPath;

			// Validate the paths.
			if (!FileSystem.ValidateFilename(newPath, true))
				throw new InvalidPathException(newPath, null);
			if (!FileSystem.ValidateFilename(oldPath, false))
				throw new InvalidPathException(oldPath, null);

			if (oldPath.ToLower() == newPath.ToLower())
				throw new CannotMoveException(oldPath, newPath, "Source and destination files are the same.", null);

			try
			{
				// Add to the new path.
				WriteFile(newPath + entry.Filename + entry.Extension, DecodeData(entry));

				// Remove from the old.
				Delete(oldPath);
			}
			catch (Exception ex)
			{
				throw new CannotMoveException(oldPath, newPath, string.Empty, ex);
			}
		}

		/// <summary>
		/// Function to move a path into a new path.
		/// </summary>
		/// <param name="oldPath">Path to move from.</param>
		/// <param name="newPath">Path to move into.</param>
		public void MovePath(string oldPath, string newPath)
		{
			string entryPath = string.Empty;			// Entry path.
			List<FileSystemEntry> newEntries;			// List of new entries.

			// Fix path.
			newPath = FileSystem.FullPathName(newPath);
			oldPath = FileSystem.FullPathName(oldPath);

			// Validate the paths.
			if (!FileSystem.ValidateFilename(newPath, true))
				throw new InvalidPathException(newPath, null);
			if (!FileSystem.ValidateFilename(oldPath, true))
				throw new InvalidPathException(oldPath, null);

			if (oldPath == newPath)
				throw new CannotMoveException(oldPath, newPath, "Paths are the same.", null);

			try
			{
				// Entries to create.
				newEntries = new List<FileSystemEntry>();

				// Get moved entries.
				foreach (FileSystemEntry oldEntry in _entries)
				{
					entryPath = oldEntry.Path;
					if (entryPath.ToLower().StartsWith(oldPath.ToLower()))
						newEntries.Add(oldEntry);
				}

				// Add new entries.
				for (int i = 0; i < newEntries.Count; i++)
				{
					// Extract the old name and prepend the new name.

					entryPath = newPath + newEntries[i].Path.Substring(oldPath.Length);
					WriteFile(entryPath, DecodeData(newEntries[i]));

					// Remove the previous entries.
					Delete(newEntries[i].FullPath);
				}	
			}
			catch(Exception ex)
			{
				throw new CannotMoveException(oldPath, newPath, string.Empty, ex);
			}
		}

		/// <summary>
		/// Function to add an array of byte data as a specific file type.
		/// </summary>
		/// <param name="filePath">Path and filename.</param>
		/// <param name="objectData">Byte array containing object data.</param>
		public void WriteFile(string filePath, byte[] objectData)
		{
			// Get the file path.
			filePath = FileSystem.FullFileName(filePath);

			// Validate the filename.
			if (!ValidateFilename(filePath, false))
				throw new InvalidFilenameException(null);

			try
			{
				// Add the entry.
				EncodeData(filePath, objectData);
			}
			catch (Exception ex)
			{
				throw new CannotAddFileSystemEntryException(filePath, ex);
			}
		}

		/// <summary>
		/// Function to read a file and return it as an array of binary data.
		/// </summary>
		/// <param name="filePath">Path and file name of the file.</param>
		/// <returns>A byte array containing the file.</returns>
		public byte[] ReadFile(string filePath)
		{
			// Get the file path.
			filePath = FileSystem.FullFileName(filePath);

			// Validate the filename.
			if (!ValidateFilename(filePath, false))
				throw new InvalidFilenameException(null);

			if (!_entries.Contains(filePath))
				throw new FileSystemEntryDoesNotExistException(filePath, null);

			try
			{
				return DecodeData(_entries[filePath]);
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load file system data from '" + filePath + "'.", ex);
			}
		}

		/// <summary>
        /// Function to write the file system contents to a specified path.
        /// </summary>
        /// <param name="path">Path to write the file system contents into.</param>
        public abstract void Save(string path);

        /// <summary>
        /// Function to remove a file entry from the file system.
        /// </summary>
        /// <param name="filePath">Path and filename of the object to delete.</param>
        public void Delete(string filePath)
        {
			FileSystemEntry entry = null;		// File system entry.

			try
			{
				// Get the file path.
				filePath = FileSystem.FullFileName(filePath);

				// Validate the filename.
				if (!ValidateFilename(filePath, false))
					throw new InvalidFilenameException(null);

				// Get previous entry.
				entry = _entries[filePath];

				// Add to deleted list if it's not already there.
				if (!_deleted.Contains(entry.FullPath))
					_deleted.AddEntry(entry.FullPath, entry.ObjectData, entry.Offset, entry.FileSize, entry.CompressedFileSize, entry.Encrypted);

				// Remove from active list.
				_entries.Remove(filePath);
			}
			catch (Exception ex)
			{
				throw new CannotDeleteEntryException(filePath, ex);
			}
        }

		/// <summary>
		/// Function to remove ALL file system entries from the file system.
		/// </summary>
		public void Delete()
		{
			// Remove all entries.
			foreach (FileSystemEntry entry in this)
				Delete(entry.FullPath);
		}

		/// <summary>
		/// Function to return whether an file entry exists or not.
		/// </summary>
		/// <param name="path">Path to the entry.</param>
		/// <returns>TRUE if entry exists, FALSE if not.</returns>
		public bool FileExists(string path)
		{
			// Get the file path.
			path = FileSystem.FullFileName(path);

			// Validate the filename.
			if (!ValidateFilename(path, false))
				throw new InvalidFilenameException(null);

			return _entries.Contains(path);
		}

		/// <summary>
		/// Function to search for a specific file name within the mounted paths.
		/// </summary>
		/// <param name="fileName">Name of the file to search for.</param>
		/// <returns>The file system entry of the file if found, NULL if not found.</returns>
		public FileSystemEntry FindFile(string fileName)
		{
			if (!ValidateFilename(fileName, false))
				throw new InvalidFilenameException(null);

			// Find the entry.
			foreach(FileSystemEntry entry in this)
			{
				if (fileName.ToLower() == entry.Filename.ToLower() + entry.Extension.ToLower())
					return entry;
			}

			return null;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the file system.</param>
		/// <param name="entryPoint">Plug-in entry point</param>
        protected FileSystem(string name, PlugInEntryPoint entryPoint)
            : base(name)
        {
            _entries = new FileSystemEntryManager(this);
            _plugInEntry = entryPoint;
			_root = string.Empty;
			_fileIndex = new XmlDocument();
			_deleted = new FileSystemEntryManager(this);

			// Add ourselves to the file system list.
			Gorgon.FileSystems.Add(this);
		}
        #endregion

        #region IPlugIn Members
        /// <summary>
        /// Property to return the plug-in entry associated with this object.
        /// </summary>
        PlugInEntryPoint IPlugIn.PlugInEntryPoint
        {
            get 
            {
                return _plugInEntry;
            }
        }
        #endregion

		#region IEnumerable<FileSystemEntry> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<FileSystemEntry> GetEnumerator()
		{
			return _entries.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _entries.GetEnumerator();
		}
		#endregion
	}
}
