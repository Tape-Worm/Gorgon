#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, October 17, 2013 4:18:05 PM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor
{
	/// <summary>
	/// A file for meta data in the file system used by the editor. 
	/// </summary>
	/// <remarks>Meta data allows us to attach various types of information that wouldn't normally contain it.  It can be used to show relationships between 
	/// objects, or point to an icon for a file.</remarks>
	static class EditorMetaDataFile
	{
		#region Constants.
		private const string MetaDataFile = ".gorgon.editor.metadata";				// Metadata file name.
		private const string MetaDataRootName = "Gorgon.Editor.MetaData";           // Name of the root node in the meta data.
		// Node containing the list of editor files.
		private const string EditorFilesNode = "EditorFiles";
		private const string WriterPlugInNode = "WriterPlugIn";						// Node name for the writer plug-in used by this file.
		private const string TypeNameAttr = "TypeName";								// Fully qualified type name for the plug-in interface.
		#endregion

		#region Variables.
		private static GorgonFileSystemFileEntry _metaDataFile;					// The file in the file system that is holding our meta data.
		private static XDocument _metaData;										// The meta data XML.
		private static readonly string _path;										// Path to the metadata file.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the fully qualified type name of the writer plug-in.
		/// </summary>
		public static string WriterPlugInType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of editor files in the file system.
		/// </summary>
		public static EditorFileCollection Files
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add or update a node in the XML tree.
		/// </summary>
		/// <param name="parent">Parent node.</param>
		/// <param name="elementName">Name of the element to add/update.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>The node that was updated.</returns>
		private static XElement AddOrUpdateNode(XElement parent, string elementName, string value = "")
		{
			XElement node = parent.Element(elementName);

			if (node == null)
			{
				node = new XElement(elementName);
				parent.Add(node);
			}

			node.Value = value ?? string.Empty;

			return node;
		}

		/// <summary>
		/// Function to add or update an attribute on a node.
		/// </summary>
		/// <param name="parent">Parent node to update.</param>
		/// <param name="attributeName">Name of the attribute to add/update.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>The attribute that was updated.</returns>
		private static XAttribute AddOrUpdateAttribute(XElement parent, string attributeName, string value)
		{
			XAttribute attribute = parent.Attribute(attributeName);

			if (attribute == null)
			{
				attribute = new XAttribute(attributeName, value);
				parent.Add(attribute);

				return attribute;
			}

			attribute.Value = value;
			return attribute;
		}

		/// <summary>
		/// Function to retrieve the writer settings.
		/// </summary>
		/// <param name="parent">Parent node of the settings.</param>
		private static void GetWriterSettings(XElement parent)
		{
			if (parent == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, APIResources.GOREDIT_ERR_METADATA_CORRUPT);
			}

			XElement writerPlugInElement = parent.Element(WriterPlugInNode);

			if (writerPlugInElement == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, APIResources.GOREDIT_ERR_METADATA_CORRUPT);
			}

			XAttribute writerPlugInTypeAttr = writerPlugInElement.Attribute(TypeNameAttr);

			if (writerPlugInTypeAttr == null)
			{
				return;
			}
			
			WriterPlugInType = writerPlugInTypeAttr.Value;
		}

		/// <summary>
		/// Function to retrieve the list of files for the editor.
		/// </summary>
		/// <param name="parent">Parent node.</param>
		private static void GetFiles(XElement parent)
		{
			if (parent == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, APIResources.GOREDIT_ERR_METADATA_CORRUPT);
			}

			XElement filesNode = parent.Element(EditorFilesNode);

			if (filesNode == null)
			{
				return;
			}

			Files = EditorFileCollection.Deserialize(filesNode.Elements(EditorFile.EditorFileNode));
		}

		/// <summary>
		/// Function to reset the meta data back to its initial state.
		/// </summary>
		public static void Reset()
		{
			_metaData = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
									  new XElement(MetaDataRootName,
												   new XElement(WriterPlugInNode)));

			WriterPlugInType = string.Empty;
			Files = new EditorFileCollection();
		}

		/// <summary>
		/// Function to load in the meta data.
		/// </summary>
		/// <remarks>Use this method to retrieve any stored meta data.  If no meta data exists, then this function will do nothing.</remarks>
		/// <exception cref="GorgonException">Thrown when the meta data is corrupted.</exception>
		public static void Load()
		{
			_metaDataFile = ScratchArea.ScratchFiles.GetFile(_path);

			// If the file doesn't exist yet, then move on.
			if (_metaDataFile == null)
			{
				Reset();
				return;
			}

			// Otherwise, load it up and parse it.
			using (Stream stream = _metaDataFile.OpenStream(false))
			{
				_metaData = XDocument.Load(stream);
			}

			// Validate the file.
			XElement rootNode = _metaData.Element(MetaDataRootName);

			GetWriterSettings(rootNode);

			GetFiles(rootNode);

			ScratchArea.AddBlockedFile(_metaDataFile);
		}

		/// <summary>
		/// Function to store the meta data.
		/// </summary>
		/// <remarks>Use this to store the meta data in the file system.</remarks>
		public static void Save()
		{
			if (_metaData == null)
			{
				return;
			}

			XElement root = _metaData.Element(MetaDataRootName);

			if (root == null)
			{
				root = new XElement(MetaDataRootName);
				_metaData.Add(root);
			}

			// Add or update the elements to the XML document.
			XElement writerPlugInElement = AddOrUpdateNode(root, WriterPlugInNode);
			XAttribute writerTypeAttr = AddOrUpdateAttribute(writerPlugInElement, TypeNameAttr, WriterPlugInType);
			
			if (string.IsNullOrWhiteSpace(WriterPlugInType))
			{
				writerTypeAttr.Remove();
			}

			// Add files.
			if (Files.Count > 0)
			{
				XElement fileList = AddOrUpdateNode(root, EditorFilesNode);
				fileList.Add(Files.Serialize());
			}

			_metaDataFile = ScratchArea.ScratchFiles.WriteFile(_path, null);
			using (Stream stream = _metaDataFile.OpenStream(true))
			{
				_metaData.Save(stream);
			}

			// Add to the block list if this file should not show up.
			ScratchArea.AddBlockedFile(_metaDataFile);
		}

		/// <summary>
		/// Function to determine if the specified file is linked to another.
		/// </summary>
		/// <param name="file">File to check.</param>
		/// <returns><b>true</b> if a link is found, <b>false</b> if not.</returns>
		public static bool HasFileLinks(EditorFile file)
		{
			return Files.Any(item => item.DependsOn.Contains(file));
		}

		/// <summary>
		/// Function to determine if the directory or its sub-directories contain files that are linked.
		/// </summary>
		/// <param name="directory">Directory to evaluate.</param>
		/// <returns><b>true</b> if the directory or sub directory contains file links, <b>false</b> if not.</returns>
		public static bool HasFileLinks(GorgonFileSystemDirectory directory)
		{
			if ((directory.Directories.Count > 0) && (directory.Directories.Any(HasFileLinks)))
			{
				return true;
			}

			return directory.Files.Any(item =>
			                           {
				                           EditorFile file;

				                           return Files.TryGetValue(item.FullPath, out file) && HasFileLinks(file);
			                           });
		}

		/// <summary>
		/// Function to return the files that are linked to the specified file.
		/// </summary>
		/// <param name="file">The file to look up.</param>
		/// <returns>A list of files that are linked to the specified file.</returns>
		public static IList<EditorFile> GetFileLinks(EditorFile file)
		{
			return Files
				.Where(item => item.DependsOn.Contains(file))
				.ToArray();
		}

		/// <summary>
		/// Function to returnthe files that are linked within the specified directory.
		/// </summary>
		/// <param name="directory">The directory to look up.</param>
		/// <returns>A list of files within the directory (or sub directories) that are linked to other files.</returns>
		public static IList<EditorFile> GetFileLinks(GorgonFileSystemDirectory directory)
		{
			var ownerFiles = new List<EditorFile>();

			if (directory.Directories.Count > 0)
			{
				foreach (IList<EditorFile> subFiles in
					directory.Directories.Select(GetFileLinks).Where(subFiles => subFiles.Count > 0))
				{
					ownerFiles.AddRange(subFiles);
				}
			}

			IEnumerable<EditorFile> editorFiles = from fileEntry in directory.Files
			                                      where Files.Contains(fileEntry.FullPath)
			                                      select Files[fileEntry.FullPath];
			

			foreach (IList<EditorFile> files in editorFiles.Select(GetFileLinks).Where(files => files.Count > 0))
			{
				ownerFiles.AddRange(files);
			}

			return ownerFiles;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMetaDataFile"/> class.
		/// </summary>
		static EditorMetaDataFile()
		{
			_path = "/" + MetaDataFile;

			Reset();
		}
		#endregion
	}
}
