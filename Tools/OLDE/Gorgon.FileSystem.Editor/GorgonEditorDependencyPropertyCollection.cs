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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of dependency properties.
	/// </summary>
	public sealed class GorgonEditorDependencyPropertyCollection
		: GorgonBaseNamedObjectCollection<GorgonEditorDependencyProperty>
	{
		#region Constants.
		/// <summary>
		/// Properties XML node name.
		/// </summary>
		internal const string PropertiesNode = "Properties";
		#endregion

		#region Properties.
        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <b>true</b> if this instance is read only; otherwise, <b>false</b>.
        /// </value>
        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

		/// <summary>
		/// Property to return a dependency property by its index.
		/// </summary>
		public GorgonEditorDependencyProperty this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a dependency property by its name.
		/// </summary>
		public GorgonEditorDependencyProperty this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function to deserialize a list of properties from an XML element.
	    /// </summary>
	    /// <param name="propertiesNode">XML node containing the properties list.</param>
	    /// <returns>A new properties collection from the XML node.</returns>
	    internal static GorgonEditorDependencyPropertyCollection Deserialize(XElement propertiesNode)
	    {
	        return new GorgonEditorDependencyPropertyCollection(propertiesNode
	                                                    .Elements(GorgonEditorDependencyProperty.PropertyNode)
	                                                    .Select(GorgonEditorDependencyProperty.Deserialize)
	                                                    .ToArray());
	    }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEditorDependencyPropertyCollection"/> class.
		/// </summary>
		/// <param name="list">The list of dependencies.</param>
		internal GorgonEditorDependencyPropertyCollection(IEnumerable<GorgonEditorDependencyProperty> list)
			: base(true)
		{
		    if (list == null)
		    {
		        return;
		    }

		    foreach (GorgonEditorDependencyProperty property in list)
		    {
		        AddItem(property);
		    }
		}
		#endregion
	}
}
