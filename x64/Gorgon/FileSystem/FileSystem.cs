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
        : NamedObject, IEnumerable<FileSystemFile>, IDisposable
	{
		#region Events.
		/// <summary>
		/// File system read event.
		/// </summary>
		public event FileSystemReadWriteHandler FileRead;
		/// <summary>
		/// File system write event.
		/// </summary>
		public event FileSystemReadWriteHandler FileWrite;
		/// <summary>
		/// File system data loading event.  This is called when the file data is actually being read from the disk.
		/// </summary>
		public event FileSystemDataIOHandler DataLoad;
		/// <summary>
		/// File system data save event.  This is called when the file data is actually committed to the disk.
		/// </summary>
		public event FileSystemDataIOHandler DataSave;		
		#endregion

		#region Variables.
		private bool _isDisposed = false;									// Flag to indicate whether the object is disposed already.
		private FileSystemProvider _provider = null;						// File system provider.
		private XmlDocument _fileIndex = null;								// XML file containing the directory and file list.
		private FileSystemPath _rootPath = new FileSystemPath(null, @"\");	// File system root path object.
		private string _root = string.Empty;								// Root file path for the physical file system.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the XML document containing the file index list.
		/// </summary>
		protected XmlDocument FileIndexXML
		{
			get
			{
				return _fileIndex;
			}
		}

		/// <summary>
		/// Property to return the header for the file system.
		/// </summary>
		protected abstract string FileSystemHeader
		{
			get;
		}

		/// <summary>
		/// Property to return the file system root path.
		/// </summary>
		public FileSystemPath Paths
		{
			get
			{
				return _rootPath;
			}
		}

		/// <summary>
		/// Property to return the type of file system.
		/// </summary>
		public FileSystemProvider Provider
		{
			get
			{
				return _provider;
			}
		}

		/// <summary>
		/// Property to return the a file system file.
		/// </summary>
		/// <param name="filePath">Path of the file system file.</param>
		/// <returns>File system file.</returns>
		public FileSystemFile this[string filePath]
		{
			get
			{
				if (!FileExists(filePath))
					throw new FileSystemFileNotFoundException(filePath);

				// Get the fully qualified name.
				filePath = FileSystem.FullFileName(filePath);

				return Paths.GetFiles()[filePath];
			}
		}

		/// <summary>
		/// Property to return the number of files in the file system.
		/// </summary>
		public int FileCount
		{
			get
			{
				return Paths.GetFiles().Count;
			}
		}		

        /// <summary>
        /// Property to set or return the root path of the file system.
        /// </summary>
        public virtual string Root
        {
            get
            {
                return _root;
            }
            set
            {
				if ((value == null) || (value == string.Empty))
					throw new FileSystemRootIsInvalidException();

				// Use standard separators
				value = value.Replace("/", @"\");

				// Add the file index.
				_fileIndex = new XmlDocument();

				// Remove all entries.
				Clear();

				_root = value;
			}
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to recursively add files to the XML index.
		/// </summary>
		/// <param name="parent">Parent XML node.</param>
		/// <param name="path">Path to add.</param>
		private void AddXMLEntry(XmlNode parent, FileSystemPath path)
		{
			XmlElement fileElement = null;			// File element.
			XmlElement pathElement = null;			// Path element.
			XmlElement propertyElement = null;		// File property.
			XmlAttribute pathNameAttribute = null;	// Path name attribute.

			// Create this path node.
			pathElement = _fileIndex.CreateElement("Path");

			pathNameAttribute = _fileIndex.CreateAttribute("Name");
			pathNameAttribute.Value = path.Name;
			pathElement.Attributes.Append(pathNameAttribute);

			pathNameAttribute = _fileIndex.CreateAttribute("FullPath");
			pathNameAttribute.Value = path.FullPath;
			pathElement.Attributes.Append(pathNameAttribute);

			// If we have child paths, then recursively fill them in.
			foreach (FileSystemPath childPath in path.ChildPaths)
				AddXMLEntry(pathElement, childPath);

			// Add the files.
			foreach (FileSystemFile file in path.Files)
			{
				// Create the file element.
				fileElement = _fileIndex.CreateElement("File");

				// Add the file name.
				propertyElement = _fileIndex.CreateElement("Filename");
				propertyElement.InnerText = file.Filename;
				fileElement.AppendChild(propertyElement);

				// Add the file name extension.
				propertyElement = _fileIndex.CreateElement("Extension");
				propertyElement.InnerText = file.Extension;
				fileElement.AppendChild(propertyElement);

				// Add the file offset.
				propertyElement = _fileIndex.CreateElement("Offset");
				propertyElement.InnerText = file.Offset.ToString();
				fileElement.AppendChild(propertyElement);

				// Add the file size.
				propertyElement = _fileIndex.CreateElement("Size");
				propertyElement.InnerText = file.Size.ToString();
				fileElement.AppendChild(propertyElement);

				// Add the compressed file size.
				propertyElement = _fileIndex.CreateElement("CompressedSize");
				propertyElement.InnerText = file.CompressedSize.ToString();
				fileElement.AppendChild(propertyElement);

				// Add the file date and time.
				propertyElement = _fileIndex.CreateElement("FileDate");
				propertyElement.InnerText = file.DateTime.ToString();
				fileElement.AppendChild(propertyElement);

				// Add the encrypted flag.
				propertyElement = _fileIndex.CreateElement("Encrypted");
				propertyElement.InnerText = file.IsEncrypted.ToString();
				fileElement.AppendChild(propertyElement);

				// Add the comment flag.
				propertyElement = _fileIndex.CreateElement("Comment");
				propertyElement.InnerText = file.Comment;
				fileElement.AppendChild(propertyElement);

				pathElement.AppendChild(fileElement);
			}

			// Append the path.
			parent.AppendChild(pathElement);
		}

		/// <summary>
		/// Function to save the file index.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		protected abstract void SaveIndex(string filePath);

		/// <summary>
		/// Function to save the file data.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		/// <param name="file">File to save.</param>
		protected abstract void SaveFileData(string filePath, FileSystemFile file);

		/// <summary>
		/// Function to fire the file read event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnFileRead(object sender, FileSystemReadWriteEventArgs e)
		{
			if (FileRead != null)
				FileRead(sender, e);
		}

		/// <summary>
		/// Function to fire the file write event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnFileWrite(object sender, FileSystemReadWriteEventArgs e)
		{
			if (FileWrite != null)
				FileWrite(sender, e);
		}

		/// <summary>
		/// Function to fire the data load event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnDataLoad(object sender, FileSystemDataIOEventArgs e)
		{
			if (DataLoad != null)
				DataLoad(sender, e);
		}

		/// <summary>
		/// Function to fire the data save event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnDataSave(object sender, FileSystemDataIOEventArgs e)
		{
			if (DataSave != null)
				DataSave(sender, e);
		}

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
				throw new FileSystemHeaderInvalidException();

			if (string.Compare(nodes[0].InnerText, "gorfs1.0", true) != 0)
				throw new FileSystemHeaderInvalidException();
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected abstract byte[] DecodeData(FileSystemFile file);
		
		/// <summary>
		/// Function clear files and paths.
		/// </summary>
		protected void Clear()
		{			
			// Remove all files and paths.
			_rootPath.ChildPaths.Clear();
			_rootPath.Files.Clear();

			// Rebuild the index XML.
			RebuildIndex();		
		}

		/// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="path">Path to add the file into.</param>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file.</returns>
		protected abstract FileSystemFile EncodeData(FileSystemPath path, string filePath, byte[] data);

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to load.</param>
		protected abstract void Load(FileSystemFile file);

		/// <summary>
		/// Function called when the save function is complete.
		/// </summary>
		/// <remarks>This function is called at the end of the save function, regardless of whether the save was successful or not.</remarks>
		protected virtual void SaveFinalize()
		{
		}

		/// <summary>
		/// Function called when a save operation begins.
		/// </summary>
		/// <param name="filePath">Path to the file system location.</param>
		protected virtual void SaveInitialize(string filePath)
		{
		}

		/// <summary>
		/// Function to create a file system.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system.</param>
		/// <param name="provider">Provider for the file system.</param>
		/// <returns>Return the file system loaded from the provider.</returns>
		public static FileSystem Create(string fileSystemName, FileSystemProvider provider)
		{
			FileSystem newFileSystem = null;		// File system to be created.

			// We require a provider and name.
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (string.IsNullOrEmpty(fileSystemName))
				throw new ArgumentNullException(fileSystemName);

			// If the file system already exists, then return it.
			if (FileSystemCache.FileSystems.Contains(fileSystemName))
				return FileSystemCache.FileSystems[fileSystemName];

			// Create a file system object.
			newFileSystem = provider.CreateFileSystemInstance(fileSystemName);

			if (newFileSystem == null)
				throw new TypeLoadException("Unable to create the file system object '" + provider.Type.FullName + "'.");

			FileSystemCache.FileSystems.Add(newFileSystem);

			return newFileSystem;
		}

		/// <summary>
		/// Function to create a file system.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system.</param>
		/// <param name="provider">Name of the provider for the file system.</param>
		/// <returns>Return the file system loaded from the provider.</returns>
		public static FileSystem Create(string fileSystemName, string provider)
		{
			if (string.IsNullOrEmpty(provider))
				throw new ArgumentNullException("provider");

			if (!FileSystemProviderCache.Providers.Contains(provider))
				throw new FileSystemPlugInNotFoundException(provider);

			return Create(fileSystemName, FileSystemProviderCache.Providers[provider]);
		}

		/// <summary>
		/// Function to adjust a pathname for use with the file system.
		/// </summary>
		/// <param name="path">Path to adjust.</param>
		/// <returns>Adjusted pathname.</returns>
		public static string FullPathName(string path)
		{
			if ((path == string.Empty) || (path == null))
				path = @"\";

			// Replace alternate path separator.
			path = path.Replace("/", @"\");
			path = path.Replace(@"\\", @"\");

			// Put root separator in.
			if (path[0] != '\\')
				path = @"\" + path;

			if (!path.EndsWith(@"\"))
				path += @"\";

			return path;
		}

		/// <summary>
		/// Function to adjust a pathname for use with the file system.
		/// </summary>
		/// <param name="path">Path to adjust.</param>
		/// <returns>Adjusted pathname.</returns>
		public static string FullFileName(string path)
		{
			// Ensure that there's a file name.
			if ((path == string.Empty) || (path == null))
				throw new FileSystemFilenameInvalidException();

			// Replace alternate path separator.
			path = path.Replace("/", @"\");
			path = path.Replace(@"\\", @"\");

			// Put root separator in.
			if (path[0] != '\\')
				path = @"\" + path;

			return path;
		}

		/// <summary>
		/// Function to retrieve a file system path object.
		/// </summary>
		/// <param name="path">Path to return.</param>
		/// <returns>The file system path object.</returns>
		public FileSystemPath GetPath(string path)
		{
			FileSystemPath newPath = null;		// New path.
			string[] paths = null;				// Path list.

			if (!PathExists(path))
				throw new FileSystemPathNotFoundException(path);

			// Get the pull pathname.
			path = FullPathName(path);

			// Return the root path.
			if (path == @"\")
				return Paths;

			// Get the path list.
			paths = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

			// No paths?  Then leave.
			if (paths.Length == 0)
				throw new FileSystemPathNotFoundException(path);

			// Begin at the root.
			newPath = Paths;

			// Add each path.
			foreach (string searchPath in paths)
			{
				// Add the path if it doesn't exist.
				if (newPath.ChildPaths.Contains(searchPath))
					newPath = newPath.ChildPaths[searchPath];
				else
					throw new FileSystemPathNotFoundException(path);
			}

			return newPath;
		}

		/// <summary>
		/// Function to add a path and all files under it to the file system.
		/// </summary>
		/// <param name="path">Path to add.</param>
		/// <param name="recurse">TRUE to search sub directories, FALSE to only search the top level.</param>
		public void Mount(string path, bool recurse)
		{
			XmlNodeList nodes = null;				// XML node list.
			XmlNodeList files = null;				// XML file list.
			XmlNode fileProperty = null;			// File property.
			FileSystemPath newPath = null;			// Path that was created.
			FileSystemFile newFile = null;			// File system file.
			int fileCompressedSize = 0;				// Compressed size of the file.
			int fileSize = 0;						// Size of the file.
			long fileOffset = 0;					// Offset of the file.
			bool fileEncrypted = false;				// Flag to indicate that the file is encrypted.
			string fileType = string.Empty;			// File type.
			string filePath = string.Empty;			// Path to the file.
			DateTime fileDate = DateTime.MinValue;	// File date & time.
			string fileComment = string.Empty;		// File comment.

			if ((path == null) || (path == string.Empty))
				path = @"\";
			
			// Get the path name.
			path = FileSystem.FullPathName(path);

			if (!FileSystemPath.ValidPath(path))
				throw new FileSystemPathInvalidException(path);

			try
			{
				// Get file system paths.
				if (recurse)
					nodes = _fileIndex.SelectNodes("//Path[@FullPath[starts-with(.,'" + path + "')]]");
				else
					nodes = _fileIndex.SelectNodes("//Path[@FullPath='" + path + "']");

				if (nodes.Count == 0)
					throw new FileSystemPathNotFoundException(path);

				// Add paths.
				foreach (XmlNode pathNode in nodes)
				{
					CreatePath(pathNode.Attributes["FullPath"].Value);

					// Get file list.
					files = pathNode.SelectNodes("File");

					// Get the parent path object.
					newPath = GetPath(pathNode.Attributes["FullPath"].Value);					

					// Add each new file.
					foreach (XmlNode fileNode in files)
					{
						// Get file path.
						fileProperty = fileNode.SelectSingleNode("Filename");
						if ((fileProperty != null) && (fileProperty.InnerText != string.Empty))
							filePath = fileProperty.InnerText;
						else
							throw new GorgonException("File system corrupted.  Invalid filename in index table.");

						fileProperty = fileNode.SelectSingleNode("Extension");
						if (fileProperty != null)
							filePath += fileProperty.InnerText;

						// Get offset.
						fileProperty = fileNode.SelectSingleNode("Offset");
						if (fileProperty != null)
							fileOffset = Convert.ToInt64(fileProperty.InnerText);
						
						// Get size.
						fileProperty = fileNode.SelectSingleNode("Size");
						if (fileProperty != null)
							fileSize = Convert.ToInt32(fileProperty.InnerText);

						// Get compressed size.
						fileProperty = fileNode.SelectSingleNode("CompressedSize");
						if (fileProperty != null)
							fileCompressedSize = Convert.ToInt32(fileProperty.InnerText);

						// Get file date.
						fileProperty = fileNode.SelectSingleNode("FileDate");
						if (fileProperty != null)
							fileDate = Convert.ToDateTime(fileProperty.InnerText);

						// Get encrypted flag.
						fileProperty = fileNode.SelectSingleNode("Encrypted");
						if (fileProperty != null)
							fileEncrypted = (string.Compare(fileProperty.InnerText, "true", true) == 0);

						// Get comment.
						fileProperty = fileNode.SelectSingleNode("Comment");
						if (fileProperty != null)
							fileComment = fileProperty.InnerText;

						newFile = newPath.Files.Add(filePath, null, fileSize, fileCompressedSize, fileDate, fileEncrypted);
						newFile.Comment = fileComment;
						newFile.Offset = fileOffset;

						// Attempt to load the file data from the disk.
						Load(newFile);
					}
				}
			}
			catch (Exception ex)
			{
				throw new FileSystemPathMountException(path, ex);
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
        /// Function to unload file data from all file system files to keep memory consumption down.
        /// </summary>
        /// <param name="path">Path to remove.</param>
		/// <param name="recurse">TRUE to recursively remove data, FALSE to only remove data for the current path.</param>
		/// <remarks>Trying to save a file system with unmounted data will throw an exception.</remarks>
        public void Unmount(string path, bool recurse)
        {
			FileList removed = null;	// Removed entries.
			
			path = FileSystem.FullPathName(path);

			if (!FileSystemPath.ValidPath(path))
				throw new FileSystemPathInvalidException(path);

			if (!PathExists(path))
				throw new FileSystemPathNotFoundException(path);

			// All the files to be unmounted.
			removed = GetPath(path).GetFiles();

			// Destroy all the data.
			foreach (FileSystemFile file in removed)
			{
				if (recurse)
				{
					if (file.Owner.FullPath.ToLower().StartsWith(path.ToLower()))
						file.Data = null;
				}
				else
				{
					if (file.Owner.FullPath.ToLower() == path.ToLower())
						file.Data = null;
				}
			}
        }

		/// <summary>
		/// Function to clear all file data from the specified path.
		/// </summary>
		/// <param name="path">Path to remove.</param>
		public void Unmount(string path)
		{
			Unmount(path, false);
		}

        /// <summary>
        /// Function to clear all file data from the file system.
        /// </summary>
        public void Unmount()
        {
			Unmount(@"\", true);
        }

		/// <summary>
		/// Function to return the size of the file system in bytes.
		/// </summary>
		/// <param name="compressed">TRUE to get compressed size, FALSE to return normal size.</param>
		/// <returns>Size of the file system in bytes.</returns>
		public ulong FileSystemSize(bool compressed)
		{
			ulong count = 0;		// File count.

			// Search for the path.
			foreach (FileSystemFile file in this)
			{
				if ((!compressed) || (!file.IsCompressed))
					count += (ulong)file.Size;
				else
					count += (ulong)file.CompressedSize;
			}

			return count;
		}

		/// <summary>
		/// Function to create a path.
		/// </summary>
		/// <param name="path">File system path to create.</param>
		public void CreatePath(string path)
		{
			string[] paths = null;				// Path list.
			FileSystemPath newPath = null;		// New path.

			if ((path == string.Empty) || (path == null))
				throw new ArgumentNullException("path");

			path = FullPathName(path);

			if (!FileSystemPath.ValidPath(path))
				throw new FileSystemPathInvalidException(path);

			// Do nothing if the path exists.
			if (PathExists(path))
				return;

			// Get the path list.
			paths = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

			// No paths?  Then leave.
			if (paths.Length == 0)
				return;

			// Begin at the root.
			newPath = Paths;

			// Add each path.
			foreach (string searchPath in paths)
			{
				// Add the path if it doesn't exist.
				if (!newPath.ChildPaths.Contains(searchPath))
					newPath = newPath.ChildPaths.Add(searchPath);
				else
					newPath = newPath.ChildPaths[searchPath];
			}
		}

		/// <summary>
		/// Function to delete a path.
		/// </summary>
		/// <param name="path">File system path to delete.</param>
		public void DeletePath(string path)
		{
			// Start from the top.
			if ((path == string.Empty) || (path == null))
				path = @"\";

			path = FullPathName(path);

			if (!FileSystemPath.ValidPath(path))
				throw new FileSystemPathInvalidException(path);

			// Destroy everything under the root if we specify the root.
			if (path == @"\")
			{
				Paths.Files.Clear();
				Paths.ChildPaths.Clear();
				return;
			}

			// Get the path list.
			GetPath(path).Parent.ChildPaths.Remove(path);
		}

		/// <summary>
		/// Function to delete all paths and files under the file system.
		/// </summary>
		public void DeletePath()
		{
			Delete(@"\");
		}

		/// <summary>
		/// Function to add an array of byte data as a specific file type.
		/// </summary>
		/// <param name="filePath">Path and filename.</param>
		/// <param name="objectData">Byte array containing object data.</param>
		public void WriteFile(string filePath, byte[] objectData)
		{
			string path = null;				// File system path.
			FileSystemFile file = null;		// File.
			
			// Get the file path.
			filePath = FileSystem.FullFileName(filePath);

			if (!FileSystemFile.ValidFilename(filePath))
				throw new FileSystemFilenameInvalidException(filePath);

			try
			{
				// Remove the file if it already exists.
				if (FileExists(filePath))
					Delete(filePath);

				// Get the path.
				path = Path.GetDirectoryName(filePath);

				// Attempt to create the path.
				CreatePath(path);

				// Add the file and fire write event.
				file = EncodeData(GetPath(path), Path.GetFileName(filePath), objectData);
				OnFileWrite(this, new FileSystemReadWriteEventArgs(file));
			}
			catch (Exception ex)
			{
				throw new FileSystemWriteException(filePath, ex);
			}
		}

		/// <summary>
		/// Function to read a file and return it as an array of binary data.
		/// </summary>
		/// <param name="filePath">Path and file name of the file.</param>
		/// <returns>A byte array containing the file.</returns>
		public byte[] ReadFile(string filePath)
		{
			byte[] result = null;			// Result data.
			FileSystemFile file = null;		// File to read.

			// Get the file path.
			filePath = FileSystem.FullFileName(filePath);

			if (!FileSystemFile.ValidFilename(filePath))
				throw new FileSystemFilenameInvalidException(filePath);

			if (!FileExists(filePath))
				throw new FileSystemFileNotFoundException(filePath);

			try
			{
				// Get the data and decode.
				file = GetPath(Path.GetDirectoryName(filePath)).Files[Path.GetFileName(filePath)];
				result = DecodeData(file);

				// Fire read event.
				OnFileRead(this, new FileSystemReadWriteEventArgs(file));

				return result;
			}
			catch (Exception ex)
			{
				throw new FileSystemReadException(filePath, ex);
			}
		}

		/// <summary>
		/// Function to open a stream into a file.
		/// </summary>
		/// <param name="filePath">File to get a stream from.</param>
		/// <param name="readOnly">TRUE for read-only access, FALSE if not.</param>
		/// <returns>A new file stream.</returns>
		public Stream OpenFileStream(string filePath, bool readOnly)
		{
			byte[] result = null;			// Result data.
			FileSystemFile file = null;		// File to read.

			// Get the file path.
			filePath = FileSystem.FullFileName(filePath);

			if (!FileSystemFile.ValidFilename(filePath))
				throw new FileSystemFilenameInvalidException(filePath);

			if (!FileExists(filePath))
				throw new FileSystemFileNotFoundException(filePath);

			try
			{
				// Get the data and decode.
				file = GetPath(Path.GetDirectoryName(filePath)).Files[Path.GetFileName(filePath)];
				result = DecodeData(file);

				return new MemoryStream(result, readOnly);
			}
			catch (Exception ex)
			{
				throw new FileSystemReadException(filePath, ex);
			}
		}

		/// <summary>
		/// Function to open a stream into a file.
		/// </summary>
		/// <param name="filePath">File to get a stream from.</param>
		/// <returns>A new file stream.</returns>
		public Stream OpenFileStream(string filePath)
		{
			return OpenFileStream(filePath, false);
		}

        /// <summary>
        /// Function to remove a file from the file system.
        /// </summary>
        /// <param name="file">Path and filename of the object to delete.</param>
        public void Delete(string file)
        {
			try
			{
				// If we pass in nothing then delete all files.			
				if ((file == string.Empty) || (file == null))
				{
					Paths.Files.Clear();
					return;
				}

				file = FileSystem.FullFileName(file);
			
				// Remove all files under the path.
				if (Path.GetFileName(file) == string.Empty)
				{
					GetPath(file).Files.Clear();
					return;
				}

				if (!FileSystemFile.ValidFilename(file))
					throw new FileSystemFilenameInvalidException(file);

				// Check to see if the file exists.
				if (!FileExists(file))
					throw new FileSystemFileNotFoundException(file);

				// Remove the file.
				GetPath(Path.GetDirectoryName(file)).Files.Remove(file);
			}
			catch (Exception ex)
			{
				throw new FileSystemDeleteException(file, ex);
			}
        }

		/// <summary>
		/// Function to remove ALL file system entries from the file system.
		/// </summary>
		public void Delete()
		{
			// Remove all entries.
			foreach (FileSystemFile file in Paths.GetFiles())
				Delete(file.FullPath);
		}

		/// <summary>
		/// Function to return whether an file exists or not.
		/// </summary>
		/// <param name="filePath">Filename and path to the entry.</param>
		/// <returns>TRUE if entry exists, FALSE if not.</returns>
		public bool FileExists(string filePath)
		{
			// Get the file path.
			filePath = FileSystem.FullFileName(filePath);

			if (!FileSystemFile.ValidFilename(filePath))
				throw new FileSystemFilenameInvalidException(filePath);

			// If the path doesn't exist, then obviously the file doesn't either.
			if (!PathExists(Path.GetDirectoryName(filePath)))
				return false;

			// Check to see if the file exists in the path.
			return GetPath(Path.GetDirectoryName(filePath)).Files.Contains(filePath);
		}

		/// <summary>
		/// Function to return whether a path exists or not.
		/// </summary>
		/// <param name="path">Path to the check.</param>
		/// <returns>TRUE if the path exists, FALSE if not.</returns>
		public bool PathExists(string path)
		{
			string[] paths = null;			// Path list.
			FileSystemPath fsPath = null;	// File system path node.

			if ((path == string.Empty) || (path == null))
				throw new ArgumentNullException("path");

			// Validate the path.
			path = FullPathName(path);

			if (!FileSystemPath.ValidPath(path))
				throw new FileSystemPathInvalidException(path);

			// Root always exists.
			if (path == @"\")
				return true;

			paths = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

			// No paths?  Return false.
			if (paths.Length == 0)
				return false;

			// Go through each path.
			fsPath = Paths;
			foreach (string searchPath in paths)
			{
				if (fsPath.ChildPaths.Contains(searchPath))
					fsPath = fsPath.ChildPaths[searchPath];
				else
					return false;
			}

			return true;
		}

		/// <summary>
		/// Function to search for a specific file name within the mounted paths.
		/// </summary>
		/// <param name="fileName">Name of the file to search for.</param>
		/// <returns>The file if found, NULL if not found.</returns>
		public FileSystemFile FindFile(string fileName)
		{
			string filePath = string.Empty;		// File path.
			FileSystemPath sourcePath = null;	// Source path.

			fileName = FileSystem.FullFileName(fileName);

			if (!FileSystemFile.ValidFilename(fileName))
				throw new FileSystemFilenameInvalidException(fileName);

			// Extract the filename.
			filePath = Path.GetDirectoryName(fileName);

			// Default to root.
			sourcePath = Paths;

			// If we specify a path, then search only under that path.
			if (filePath != null)
				sourcePath = GetPath(filePath);

			fileName = Path.GetFileName(fileName);

			// Find the file.
			foreach(FileSystemFile file in sourcePath.GetFiles())
			{
				if (fileName.ToLower() == file.Filename.ToLower() + file.Extension.ToLower())
					return file;
			}

			return null;
		}

		/// <summary>
		/// Function to save the file system.
		/// </summary>
		/// <param name="filePath">Path to save the file system into.</param>
		public void Save(string filePath)
		{
			XmlElement fsElement = null;				// File system.
			long offset = 0;							// File offset.
			FileList allFiles = null;					// All files.
						
			if ((filePath == string.Empty) || (filePath == null))
				throw new ArgumentNullException("filePath");

			try
			{
				// Get all the file entries.
				allFiles = Paths.GetFiles();

				// Reset the XML index.
				RebuildIndex();

				// Get the file system node.
				fsElement = (XmlElement)_fileIndex.SelectSingleNode("//FileSystem");

				if (fsElement == null)
					throw new GorgonException("File system is corrupt.");

				// Update the file offsets.
				foreach (FileSystemFile file in allFiles)
				{
					file.Offset = offset;
					if (file.IsCompressed)
						offset += file.CompressedSize;
					else
						offset += file.Size;
				}

				// Build the path/file index.
				AddXMLEntry(fsElement, Paths);

				// Initialize the saving.
				SaveInitialize(filePath);

				// Save the index file.
				SaveIndex(filePath);

				// Now physically write the files..
				foreach (FileSystemFile file in allFiles)
				{
					if (file.Data == null)
						throw new GorgonException("File has no data.");

					SaveFileData(filePath, file);

					// Fire an event upon successful save.
					OnDataSave(this, new FileSystemDataIOEventArgs(file));
				}

				// Set the root path.
				_root = filePath;
			}
			catch (Exception ex)
			{
				throw new CannotSaveException("Cannot save the file system '" + filePath + "'", ex);
			}
			finally
			{
				// Perform clean up.
				SaveFinalize();
			}
		}
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the file system.</param>
		/// <param name="provider">Provider for this file system.</param>
        protected FileSystem(string name, FileSystemProvider provider)
            : base(name)
        {
			if (provider == null)
				throw new ArgumentNullException("provider");

			_root = string.Empty;
			_fileIndex = new XmlDocument();
			_provider = provider;
		}
        #endregion

		#region IEnumerable<FileSystemEntry> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<FileSystemFile> GetEnumerator()
		{
			foreach(FileSystemFile file in Paths.GetFiles())
				yield return file;
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
			foreach (FileSystemFile file in Paths.GetFiles())
				yield return file;
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
				if (!_isDisposed)			
					FileSystemCache.FileSystems.Remove(this.Name);

				_isDisposed = true;
			}

			// Do unmanaged clean up.
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
