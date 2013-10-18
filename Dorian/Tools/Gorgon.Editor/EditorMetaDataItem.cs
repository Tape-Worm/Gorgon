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
// Created: Thursday, October 17, 2013 4:46:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A meta data item.
	/// </summary>
	public class EditorMetaDataItem
		: GorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the list of child metadata items.
		/// </summary>
		public IDictionary<string, EditorMetaDataItem> MetaDataItems
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of properties for the meta data items.
		/// </summary>
		public IDictionary<string, string> Properties
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the value for the meta data item.
		/// </summary>
		public string Value
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function deserialize the nodes from XML.
		/// </summary>
		/// <param name="parent">Parent XML node.</param>
		internal void Deserialize(XElement parent)
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

				MetaDataItems.Add(item.Name, item);
			}
		}
		
		/// <summary>
		/// Function to serialize this item into meta data XML element.
		/// </summary>
		/// <returns>The XML element holding the meta data.</returns>
		internal XElement Serialize()
		{
			var result = new XElement(Name)
			             {
				             Value = Value ?? string.Empty
			             };

			

			// Add properties as attributes.
			foreach (var property in Properties.Where(item => !string.IsNullOrWhiteSpace(item.Key)).Select(item => new {item.Key, Value = item.Value ?? string.Empty}))
			{
				result.Add(new XAttribute(property.Key, property.Value));
			}

			// Add child nodes.
			foreach (var item in MetaDataItems.Where(item => !string.IsNullOrWhiteSpace(item.Key)))
			{
				result.Add(item.Value.Serialize());
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMetaDataItem"/> class.
		/// </summary>
		/// <param name="name">The name of the item.</param>
		public EditorMetaDataItem(string name)
			: base(name)
		{
			MetaDataItems = new Dictionary<string, EditorMetaDataItem>(new EditorMetaDataFile.MetaDataNameComparer());
			Properties = new Dictionary<string, string>(new EditorMetaDataFile.MetaDataNameComparer());
		}
		#endregion
	}
}
