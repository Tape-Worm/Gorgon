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
// Created: Thursday, February 27, 2014 10:32:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GorgonLibrary.Collections;
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A collection of dependency properties.
	/// </summary>
	public class DependencyPropertyCollection
		: GorgonBaseNamedObjectCollection<DependencyProperty>, ICloneable<DependencyPropertyCollection>
	{
		#region Constants.
		/// <summary>
		/// Properties XML node name.
		/// </summary>
		internal const string PropertiesNode = "Properties";
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a dependency property by its index.
		/// </summary>
		public DependencyProperty this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a dependency property by its name.
		/// </summary>
		public DependencyProperty this[string name]
		{
			get
			{
				return GetItem(name);
			}
			set
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException(APIResources.GOREDIT_ERR_DEPENDENCY_PROP_NO_NAME);
				}

				if (Contains(name))
				{
					SetItem(name, value);
				}
				else
				{
					AddItem(value);
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize this list into an XML element.
		/// </summary>
		/// <returns>The XML element containing the serialized list.</returns>
		internal XElement Serialize()
		{
			var result = new XElement(PropertiesNode);

			foreach (DependencyProperty property in this)
			{
				result.Add(property.Serialize());
			}

			return result;
		}

		/// <summary>
		/// Function to deserialize a list of properties from an XML element.
		/// </summary>
		/// <param name="propertiesNode">XML node containing the properties list.</param>
		/// <returns>A new properties collection from the XML node.</returns>
		internal static DependencyPropertyCollection Deserialize(XElement propertiesNode)
		{
			var result = new DependencyPropertyCollection();

			IEnumerable<XElement> propertyNodes = propertiesNode.Elements(DependencyProperty.PropertyNode);

			foreach (XElement propertyNode in propertyNodes)
			{
				result.Add(DependencyProperty.Deserialize(propertyNode));
			}

			return result;
		}

		/// <summary>
		/// Function to add a list of properties to the collection.
		/// </summary>
		/// <param name="properties">Properties to add to the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="properties"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void AddRange(IEnumerable<DependencyProperty> properties)
		{
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}

			foreach (DependencyProperty property in properties)
			{
				Add(property);
			}
		}

		/// <summary>
		/// Function to add a new dependency property.
		/// </summary>
		/// <param name="property">The property to add.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="property"/> parameter already exists in this collection.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="property"/> parameter has no name.</para>
		/// </exception>
		public void Add(DependencyProperty property)
		{
			if (string.IsNullOrWhiteSpace(property.Name))
			{
				throw new ArgumentException(APIResources.GOREDIT_ERR_DEPENDENCY_PROP_NO_NAME);
			}

			if (Contains(property.Name))
			{
				throw new ArgumentException(string.Format(APIResources.GOREDIT_ERR_DEPENDENCY_PROP_EXISTS, property.Name));
			}

			AddItem(property);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to remove a dependency property by its name.
		/// </summary>
		/// <param name="name">Name of the property to remove.</param>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the property does not exist in the collection.</exception>
		public void Remove(string name)
		{
			if (!Contains(name))
			{
				throw new KeyNotFoundException(string.Format(APIResources.GOREDIT_ERR_DEPENDENCY_PROP_DOES_NOT_EXIST, name));
			}

			RemoveItem(name);
		}

		/// <summary>
		/// Function to remove a dependency property by its index.
		/// </summary>
		/// <param name="index">Index of the property to remove.</param>
		public void Remove(int index)
		{
			if ((index < 0) || (index >= Count))
			{
				throw new ArgumentOutOfRangeException("index");
			}

			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a dependency property from the collection.
		/// </summary>
		/// <param name="property">Property to remove from the collection.</param>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the property does not exist in the collection.</exception>
		public void Remove(DependencyProperty property)
		{
			if (!Contains(property))
			{
				throw new KeyNotFoundException(string.Format(APIResources.GOREDIT_ERR_DEPENDENCY_PROP_DOES_NOT_EXIST, property.Name));
			}

			RemoveItem(property);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyPropertyCollection"/> class.
		/// </summary>
		internal DependencyPropertyCollection()
			: base(true)
		{
			
		}
		#endregion

		#region ICloneable<DependencyPropertyCollection> Members
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>
		/// The cloned object.
		/// </returns>
		public DependencyPropertyCollection Clone()
		{
			var result = new DependencyPropertyCollection();

			foreach (var property in this)
			{
				result.Add(property.Clone());
			}

			return result;
		}
		#endregion
	}
}
