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
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// A dependency for content files.
    /// </summary>
	public class Dependency
		: INamedObject
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
		/// Property to return the path of the file that is being depended upon.
		/// </summary>
		public string Path
		{
			get;
			private set;
		}

		/// <summary>
		/// Property set or return the object that was created because of this dependency.
		/// </summary>
		public object DependencyObject
		{
			get;
			set;
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
				new XAttribute(DependencyTypeAttr, Type), new XElement(DependencyPathNode, Path));

			result.Add(Properties.Serialize());

			return result;
		}

		/// <summary>
		/// Function to deserialize a dependency from an XML node.
		/// </summary>
		/// <param name="element">Element containing the serialized dependency.</param>
		/// <returns>A dependency deserialized from the XML node.</returns>
		internal static Dependency Deserialize(XElement element)
		{
			XAttribute typeAttr = element.Attribute(DependencyTypeAttr);
			XElement pathNode = element.Element(DependencyPathNode);

			if ((typeAttr == null)
				|| (pathNode == null)
				|| (string.IsNullOrWhiteSpace(typeAttr.Value))
			    || (string.IsNullOrWhiteSpace(pathNode.Value)))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_DEPENDENCY_CORRUPT);
			}

			var result = new Dependency(pathNode.Value, typeAttr.Value);

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
		/// <param name="name">The name of the dependency.</param>
		/// <param name="type">The type of dependency.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="type"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is an empty string.</para>
		/// </exception>
		public Dependency(string name, string type)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (string.IsNullOrWhiteSpace(type))
			{
				throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "type");
			}

			Path = name;
			Properties = new DependencyPropertyCollection();
			Type = type;
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
				return Path;
			}
		}
		#endregion
	}
}
