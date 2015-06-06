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
// Created: Thursday, February 27, 2014 10:12:33 PM
// 
#endregion

using System;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
    /// <summary>
    /// A dependency for content files.
    /// </summary>
	public class Dependency
		: IGorgonNamedObject, IGorgonCloneable<Dependency>
    {
		#region Constants.
		private const string DependencyTypeAttr = "Type";			// Type attribute for the dependency type.
		private const string DependencyPathNode = "Path";			// Path node for the dependency path.

		/// <summary>
		/// Dependency node name.
		/// </summary>
		internal const string DependencyNode = "DependsOn";
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of properties associated with this dependency.
		/// </summary>
		public DependencyPropertyCollection Properties
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of dependency.
		/// </summary>
		public string Type
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the file that has the dependency.
		/// </summary>
	    public EditorFile EditorFile
	    {
		    get;
		    internal set;
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize this dependency into an XML node.
		/// </summary>
		/// <returns>The XML node containing the serialized dependency.</returns>
		internal XElement Serialize()
		{
			var result = new XElement(DependencyNode,
			                          new XAttribute(DependencyTypeAttr, Type),
			                          new XElement(DependencyPathNode, EditorFile.FilePath));

			result.Add(Properties.Serialize());

			return result;
		}

		/// <summary>
		/// Function to deserialize a dependency from an XML node.
		/// </summary>
		/// <param name="files">Available files to evaluate.</param>
		/// <param name="element">Element containing the serialized dependency.</param>
		/// <returns>A dependency deserialized from the XML node.</returns>
		internal static Dependency Deserialize(EditorFileCollection files, XElement element)
		{
			XAttribute typeAttr = element.Attribute(DependencyTypeAttr);
			XElement pathNode = element.Element(DependencyPathNode);

			if ((typeAttr == null)
				|| (pathNode == null)
				|| (string.IsNullOrWhiteSpace(typeAttr.Value))
			    || (string.IsNullOrWhiteSpace(pathNode.Value)))
			{
				throw new GorgonException(GorgonResult.CannotRead, APIResources.GOREDIT_ERR_DEPENDENCY_CORRUPT);
			}

			EditorFile file;

			if (!files.TryGetValue(pathNode.Value, out file))
			{
				return null;
			}

			var result = new Dependency(file, typeAttr.Value);

			XElement propertiesNode = element.Element(DependencyPropertyCollection.PropertiesNode);

			if (propertiesNode != null)
			{
				result.Properties = DependencyPropertyCollection.Deserialize(propertiesNode);
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Dependency"/> class.
		/// </summary>
		/// <param name="file">The file that has the dependency.</param>
		/// <param name="type">The type of dependency.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="type"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="type"/> parameter is an empty string.</exception>
		public Dependency(EditorFile file, string type)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (string.IsNullOrWhiteSpace(type))
			{
				throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "type");
			}

			Properties = new DependencyPropertyCollection();
			Type = type;
			EditorFile = file;
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string IGorgonNamedObject.Name
		{
			get
			{
				return EditorFile.FilePath;
			}
		}
		#endregion

		#region ICloneable<Dependency> Members
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>
		/// The cloned object.
		/// </returns>
		public Dependency Clone()
		{
			return new Dependency(EditorFile, Type)
			       {
				       Properties = Properties.Clone()
			       };
		}
		#endregion
	}
}
