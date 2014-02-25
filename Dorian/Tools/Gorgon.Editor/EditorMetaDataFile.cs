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
	public class EditorMetaDataFile
		: INamedObject
	{
		#region Constants.
		private const string MetaDataRootName = "Gorgon.Editor.MetaData";           // Name of the root node in the meta data.

		/// <summary>
		/// Meta data group for content file dependencies.
		/// </summary>
		public const string ContentDependencyFiles = "ContentFileDependencies";
		#endregion

		#region Classes.
		/// <summary>
		/// Comparer used to find the name of a meta data item.
		/// </summary>
		internal class MetaDataNameComparer
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
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of meta data items for the meta data.
		/// </summary>
		public EditorMetaDataItemCollection MetaDataItems
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the full path to the meta data file.
		/// </summary>
		public string Path
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the meta data file should show up in the directory tree or file lists.
		/// </summary>
		public bool IsVisible
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function deserialize the nodes from XML.
		/// </summary>
		/// <param name="parent">Parent XML node.</param>
		private void Deserialize(XElement parent)
		{
			foreach (XElement element in parent.Elements())
			{
				var item = new EditorMetaDataItem(element.Name.LocalName);
				var attributes = element.Attributes();

				foreach (var attribute in attributes)
				{
					item.Properties.Add(attribute.Name.LocalName, attribute.Value);
				}

				item.Deserialize(element);

				MetaDataItems.Add(item);
			}
		}

		/// <summary>
		/// Function to load in the meta data.
		/// </summary>
		/// <remarks>Use this method to retrieve any stored meta data.  If no meta data exists, then this function will do nothing.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the meta data is corrupted.</exception>
		public void Load()
		{
			_metaDataFile = ScratchArea.ScratchFiles.GetFile(Path);

			// If the file doesn't exist yet, then move on.
			if (_metaDataFile == null)
			{
				_metaData = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(MetaDataRootName));
				return;
			}

			// Otherwise, load it up and parse it.
			using (Stream stream = _metaDataFile.OpenStream(false))
			{
				_metaData = XDocument.Load(stream);
			}

			// Validate the file.
			XElement rootNode = _metaData.Element(MetaDataRootName);

			if (rootNode == null)
			{
				_metaData = null;
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_METADATA_CORRUPT, Name));
			}

			Deserialize(rootNode);

			if (!IsVisible)
			{
				ScratchArea.AddBlockedFile(_metaDataFile);
			}
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
				return;
			}

			// Clear the current document.
			root.RemoveAll();

			// Rebuild it.
			foreach (var item in MetaDataItems)
			{
				root.Add(item.Serialize());
			}

			_metaDataFile = ScratchArea.ScratchFiles.WriteFile(Path, null);
			using (Stream stream = _metaDataFile.OpenStream(true))
			{
				_metaData.Save(stream);
			}

			// Add to the block list if this file should not show up.
			if (!IsVisible)
			{
				ScratchArea.AddBlockedFile(_metaDataFile);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMetaDataFile"/> class.
		/// </summary>
		/// <param name="name">The name of the file.</param>
		/// <param name="directoryPath">[Optional] A file system directory path that will contain the meta data file.</param>
		/// <param name="isVisible">[Optional] TRUE if the meta data file should show up in the directory tree or other file lists, FALSE if not.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		public EditorMetaDataFile(string name, string directoryPath = @"/", bool isVisible = false)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			MetaDataItems = new EditorMetaDataItemCollection();

			Name = name.FormatFileName();

			// If we provided an object link, locate the object.
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				Path = "/" + Name;
			}
			else
			{
				Path = directoryPath.FormatDirectory('/') + Name;
			}

			_metaData = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(MetaDataRootName));

			IsVisible = isVisible;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion
	}
}
