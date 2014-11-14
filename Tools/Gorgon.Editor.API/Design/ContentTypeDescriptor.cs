#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, May 14, 2012 3:52:01 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Linq;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Editor
{	
	/// <summary>
	/// A collection of properties.
	/// </summary>
	public class ContentTypeDescriptor
		: GorgonBaseNamedObjectList<ContentProperty>, ICustomTypeDescriptor
	{
		#region Properties.
		/// <summary>
		/// Property to return the content for this type descriptor.
		/// </summary>
		public ContentObject Content
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to return the property at the specified index.
		/// </summary>
		public ContentProperty this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a property by its name.
		/// </summary>
		public ContentProperty this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate the properties for a specific document type.
		/// </summary>
		/// <param name="type">Type of object to retrieve properties from.</param>
		public void Enumerate(Type type)			
		{
            if (!type.IsSubclassOf(typeof(ContentObject)))
            {
                throw new InvalidCastException("The type is not a content type.");
            }

			var properties = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>();

			ClearItems();

			foreach (var property in properties)
			{
				// Don't use non browsable properties.
				if (property.Attributes.Matches(BrowsableAttribute.No))
				{
					continue;
				}

				AddItem(new ContentProperty(property, Content));
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentTypeDescriptor"/> class.
		/// </summary>
		/// <param name="owner">Owner of this type descriptor.</param>
		public ContentTypeDescriptor(ContentObject owner)
			: base(true)
		{
			Content = owner;	
		}
		#endregion

		#region ICustomTypeDescriptor Members
		/// <summary>
		/// Returns a collection of custom attributes for this instance of a component.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.AttributeCollection"/> containing the attributes for this object.
		/// </returns>
		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		/// <summary>
		/// Returns the class name of this instance of a component.
		/// </summary>
		/// <returns>
		/// The class name of the object, or null if the class does not have a name.
		/// </returns>
		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		/// <summary>
		/// Returns the name of this instance of a component.
		/// </summary>
		/// <returns>
		/// The name of the object, or null if the object does not have a name.
		/// </returns>
		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		/// <summary>
		/// Returns a type converter for this instance of a component.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.TypeConverter"/> that is the converter for this object, or null if there is no <see cref="T:System.ComponentModel.TypeConverter"/> for this object.
		/// </returns>
		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		/// <summary>
		/// Returns the default event for this instance of a component.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.EventDescriptor"/> that represents the default event for this object, or null if this object does not have events.
		/// </returns>
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		/// <summary>
		/// Returns the default property for this instance of a component.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that represents the default property for this object, or null if this object does not have properties.
		/// </returns>
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		/// <summary>
		/// Returns an editor of the specified type for this instance of a component.
		/// </summary>
		/// <param name="editorBaseType">A <see cref="T:System.Type"/> that represents the editor for this object.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> of the specified type that is the editor for this object, or null if the editor cannot be found.
		/// </returns>
		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		/// <summary>
		/// Returns the events for this instance of a component using the specified attribute array as a filter.
		/// </summary>
		/// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.EventDescriptorCollection"/> that represents the filtered events for this component instance.
		/// </returns>
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		/// <summary>
		/// Returns the events for this instance of a component.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.ComponentModel.EventDescriptorCollection"/> that represents the events for this component instance.
		/// </returns>
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		/// <summary>
		/// Returns the properties for this instance of a component using the attribute array as a filter.
		/// </summary>
		/// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the filtered properties for this component instance.
		/// </returns>
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			var descriptors = this.Where(item => !item.HideProperty)
			                      .Select(propValue => new ContentPropertyDescriptor(propValue));

			return new PropertyDescriptorCollection(descriptors.Cast<PropertyDescriptor>().ToArray());
		}

		/// <summary>
		/// Returns the properties for this instance of a component.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the properties for this component instance.
		/// </returns>
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[] { });
		}

		/// <summary>
		/// Returns an object that contains the property described by the specified property descriptor.
		/// </summary>
		/// <param name="pd">A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that represents the property whose owner is to be found.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the owner of the specified property.
		/// </returns>
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
		#endregion
	}
}
