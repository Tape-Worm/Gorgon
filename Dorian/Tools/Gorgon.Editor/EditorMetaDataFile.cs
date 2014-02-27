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

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A file for meta data in the file system used by the editor. 
	/// </summary>
	/// <remarks>Meta data allows us to attach various types of information that wouldn't normally contain it.  It can be used to show relationships between 
	/// objects, or point to an icon for a file.</remarks>
	class EditorMetaDataFile
	{
		#region Constants.
		private const string MetaDataFile = ".gorgon.editor.metadata";				// Metadata file name.
		private const string MetaDataRootName = "Gorgon.Editor.MetaData";           // Name of the root node in the meta data.

		/// <summary>
		/// Meta data group for content file dependencies.
		/// </summary>
		public const string ContentDependencyFiles = "ContentFileDependencies";

		private const string WriterPlugInNode = "WriterPlugIn";						// Node name for the writer plug-in used by this file.
		private const string TypeNameAttr = "TypeName";								// Fully qualified type name for the plug-in interface.
		private const string FileNode = "File";										// Name of the file node.
		private const string NameAttr = "Name";										// Name attribute name.
		private const string DependsOnNode = "DependsOn";							// Depends on node.
		#endregion

		#region Classes.
		/// <summary>
		/// Comparer used to find the name of an item.
		/// </summary>
		internal class NameComparer
			: IEqualityComparer<string>
		{
			#region IEqualityComparer<EditorMetaDataItem> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <paramref name="x" /> to compare.</param>
			/// <param name="y">The second object of type <paramref name="y" /> to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			/// <exception cref="System.NotImplementedException"></exception>
			public bool Equals(string x, string y)
			{
				if ((x == null) && (y == null))
				{
					return true;
				}

				if (((x != null) && (y == null))
					|| (x == null))
				{
					return false;
				}

				return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The object.</param>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public int GetHashCode(string obj)
			{
				return 281.GenerateHash(obj);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private GorgonFileSystemFileEntry _metaDataFile;					// The file in the file system that is holding our meta data.
		private XDocument _metaData;										// The meta data XML.
		private readonly string _path;										// Path to the metadata file.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the fully qualified type name of the writer plug-in.
		/// </summary>
		public string WriterPlugInType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of files and associated dependencies.
		/// </summary>
		public IDictionary<string, ICollection<string>> Dependencies
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
		private void GetWriterSettings(XElement parent)
		{
			if (parent == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_METADATA_CORRUPT);
			}

			XElement writerPlugInElement = parent.Element(WriterPlugInNode);

			if (writerPlugInElement == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_METADATA_CORRUPT);
			}

			XAttribute writerPlugInTypeAttr = writerPlugInElement.Attribute(TypeNameAttr);

			if (writerPlugInTypeAttr == null)
			{
				return;
			}
			
			WriterPlugInType = writerPlugInTypeAttr.Value;
		}

		/// <summary>
		/// Function to retrieve the dependency list.
		/// </summary>
		/// <param name="parent">Parent node.</param>
		private void GetDependencies(XElement parent)
		{
			if (parent == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_METADATA_CORRUPT);
			}

			XElement dependencyNode = parent.Element(ContentDependencyFiles);

			if (dependencyNode == null)
			{
				return;
			}

			IEnumerable<XElement> dependencyFiles = dependencyNode.Elements(FileNode);

			Dependencies.Clear();

			// Fill in the list.
			foreach (XElement file in dependencyFiles)
			{
				XAttribute filePath = file.Attribute(NameAttr);

				if ((filePath == null)
					|| (string.IsNullOrWhiteSpace(filePath.Value)))
				{
					continue;
				}

				var fileList = new HashSet<string>(new NameComparer());

				IEnumerable<XElement> dependencies = file.Elements(DependsOnNode);

				foreach (XElement dependencyFile in dependencies)
				{
					if ((string.IsNullOrWhiteSpace(dependencyFile.Value)) 
						|| (fileList.Contains(dependencyFile.Value)))
					{
						continue;
					}

					fileList.Add(dependencyFile.Value);
				}

				if (fileList.Count == 0)
				{
					continue;
				}

				Dependencies.Add(filePath.Value, fileList);
			}
		}

		/// <summary>
		/// Function to reset the meta data back to its initial state.
		/// </summary>
		public void Reset()
		{
			_metaData = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
									  new XElement(MetaDataRootName,
												   new XElement(WriterPlugInNode),
												   new XElement(ContentDependencyFiles)));

			WriterPlugInType = string.Empty;
			Dependencies.Clear();
		}

		/// <summary>
		/// Function to load in the meta data.
		/// </summary>
		/// <remarks>Use this method to retrieve any stored meta data.  If no meta data exists, then this function will do nothing.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the meta data is corrupted.</exception>
		public void Load()
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

			GetDependencies(rootNode);

			ScratchArea.AddBlockedFile(_metaDataFile);
		}

		/// <summary>
		/// Function to store the meta data.
		/// </summary>
		/// <remarks>Use this to store the meta data in the file system.</remarks>
		public void Save()
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

			XElement dependencyList = AddOrUpdateNode(root, ContentDependencyFiles);

			foreach (KeyValuePair<string, ICollection<string>> files in Dependencies)
			{
				if ((string.IsNullOrWhiteSpace(files.Key))
					|| (files.Value == null)
					|| (files.Value.Count == 0))
				{
					continue;
				}

				XElement file = AddOrUpdateNode(dependencyList, FileNode);
				AddOrUpdateAttribute(file, NameAttr, files.Key);

				// Add file list.
				foreach (string dependency in files.Value)
				{
					file.Add(new XElement(DependsOnNode, dependency));
				}
			}

			_metaDataFile = ScratchArea.ScratchFiles.WriteFile(_path, null);
			using (Stream stream = _metaDataFile.OpenStream(true))
			{
				_metaData.Save(stream);
			}

			// Add to the block list if this file should not show up.
			ScratchArea.AddBlockedFile(_metaDataFile);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMetaDataFile"/> class.
		/// </summary>
		public EditorMetaDataFile()
		{
			_path = "/" + MetaDataFile;
			Dependencies = new Dictionary<string, ICollection<string>>(new NameComparer());

			Reset();
		}
		#endregion
	}
}
