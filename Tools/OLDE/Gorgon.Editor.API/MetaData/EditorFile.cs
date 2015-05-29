#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Wednesday, October 15, 2014 9:41:04 PM
// 
#endregion

using System;
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
	/// Metadata for a file contained within the file system.
	/// </summary>
	public class EditorFile
		: INamedObject, ICloneable<EditorFile>
	{
		#region Constants.
		// The attribute containing the type of plug-in used to open the file.
		private const string EditorFilePlugInTypeAttr = "PlugIn";
		// The root node for custom attributes.
		private const string EditorFileCustomNodeRoot = "Attributes";
		// The node for a custom attribute.
		private const string EditorFileCustomNode = "Attribute";
		// The attribute containing the name of a custom file attribute.
		private const string EditorCustomNameAttr = "Name";

		/// <summary>
		/// The root node for file dependencies.
		/// </summary>
		internal const string EditorDependenciesNodeRoot = "Dependencies";
		
		/// <summary>
		/// The name of the node containing the file. 
		/// </summary>
		internal const string EditorFileNode = "File";

		/// <summary>
		/// The attribute containing the path to the file in the file system. 
		/// </summary>
		internal const string EditorFilePathAttr = "FilePath";
		#endregion

		#region Variables.
		// Path to the file.
		private string _filePath;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the path to the file.
		/// </summary>
		public string FilePath
		{
			get
			{
				return _filePath;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(_filePath))
				{
					return;
				}

				_filePath = value;

				Filename = Path.GetFileName(_filePath).FormatFileName();
			}
		}

		/// <summary>
		/// Property to return the file name for this file.
		/// </summary>
		public string Filename
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the fully qualified type name of the plug-in that can open this file.
		/// </summary>
		public string PlugInType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the items that this file depends on.
		/// </summary>
		public DependencyCollection DependsOn
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the attributes for this file.
		/// </summary>
		public Dictionary<string, string> Attributes
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize this file into an XML element.
		/// </summary>
		/// <returns>An XML element containing the serialized contents of the file object.</returns>
		internal XElement Serialize()
		{
			var result = new XElement(EditorFileNode,
			                          new XAttribute(EditorFilePathAttr, FilePath),
			                          new XAttribute(EditorFilePlugInTypeAttr, PlugInType ?? string.Empty));

			if (DependsOn.Count > 0)
			{
				// Serialize any dependencies.
				result.Add(new XElement(EditorDependenciesNodeRoot, DependsOn.Serialize()));
			}

			if (Attributes.Count == 0)
			{
				return result;
			}

			// Add custom attributes.
			var root = new XElement(EditorFileCustomNodeRoot);

			foreach (KeyValuePair<string, string> attr in Attributes.Where(attr => !string.IsNullOrWhiteSpace(attr.Key)))
			{
				root.Add(new XElement(EditorFileCustomNode, new XAttribute(EditorCustomNameAttr, attr.Key), attr.Value));
			}

			result.Add(root);

			return result;
		}

		/// <summary>
		/// Function to deserialize this item from an XML node.
		/// </summary>
		/// <param name="node">Node containing the data.</param>
		/// <returns>A new editor file object.</returns>
		internal static EditorFile Deserialize(XElement node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}

			XAttribute plugInAttr = node.Attribute(EditorFilePlugInTypeAttr);
			XAttribute filePathAttr = node.Attribute(EditorFilePathAttr);

			string plugIn = string.Empty;

			if ((filePathAttr == null)
			    || (string.IsNullOrWhiteSpace(filePathAttr.Value)))
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(APIResources.GOREDIT_ERR_EDITFILE_CORRUPT, APIResources.GOREDIT_TEXT_UNKNOWN_FILE));
			}

			string filePath = filePathAttr.Value;

			if ((plugInAttr != null)
				&& (!string.IsNullOrWhiteSpace(plugInAttr.Value)))
			{
				plugIn = plugInAttr.Value;
			}

			// Create our file.
			var result = new EditorFile(filePath)
			             {
				             PlugInType = plugIn
			             };

			// Check for custom attributes.
			XElement customAttrRoot = node.Element(EditorFileCustomNodeRoot);

			if (customAttrRoot == null)
			{
				return result;
			}

			// Load custom attribute key/value pairs.
			IEnumerable<XElement> customAttrs = customAttrRoot.Elements(EditorFileCustomNode);

			foreach (XElement element in customAttrs)
			{
				XAttribute attrName = element.Attribute(EditorCustomNameAttr);

				// No name, then skip it.
				if ((attrName == null)
				    || (string.IsNullOrWhiteSpace(attrName.Value)))
				{
					continue;
				}

				result.Attributes[attrName.Value] = element.Value;
			}

			return result;
		}

		/// <summary>
		/// Function to rename this editor file.
		/// </summary>
		/// <param name="newName">New name for the editor file.</param>
		internal void Rename(string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
			{
				throw new ArgumentException(APIResources.GOREDIT_ERR_FILE_PATH_INVALID_CHARS, "newName");
			}

			FilePath = newName;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFile" /> class.
		/// </summary>
		/// <param name="filePath">The friendly name for the file.</param>
		public EditorFile(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "filePath");
			}

			_filePath = filePath;
			Filename = Path.GetFileName(_filePath).FormatFileName();
			Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			DependsOn = new DependencyCollection();
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get
			{
				return _filePath;
			}
		}
		#endregion

		#region ICloneable<EditorFile> Members
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>
		/// The cloned object.
		/// </returns>
		public EditorFile Clone()
		{
			var result = new EditorFile(_filePath)
			             {
				             DependsOn = DependsOn.Clone(),
				             PlugInType = PlugInType
			             };

			foreach (KeyValuePair<string, string> attrib in Attributes)
			{
				result.Attributes[attrib.Key] = attrib.Value;
			}

			return result;
		}
		#endregion
	}
}
