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
// Created: Monday, May 14, 2012 5:49:34 PM
// 
#endregion

using System;
using System.ComponentModel;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Content property descriptor.
	/// </summary>
	public class ContentPropertyDescriptor
		: PropertyDescriptor
	{
		#region Properties.
		/// <summary>
		/// Property to return the property associated with this descriptor.
		/// </summary>
		public ContentProperty ContentProperty
		{
			get;
			private set;
		}

		/// <summary>
		/// When overridden in a derived class, gets the type of the component this property is bound to.
		/// </summary>
		/// <returns>A <see cref="T:System.Type"/> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"/> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"/> methods are invoked, the object specified might be an instance of this type.</returns>
		public override Type ComponentType
		{
			get 
			{
				return typeof(ContentProperty);
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether this property is read-only.
		/// </summary>
		/// <returns>true if the property is read-only; otherwise, false.</returns>
		public override bool IsReadOnly
		{
			get 
			{
				return Attributes.Matches(ReadOnlyAttribute.Yes);
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets the type of the property.
		/// </summary>
		/// <returns>A <see cref="T:System.Type"/> that represents the type of the property.</returns>
		public override Type PropertyType
		{
			get 
			{
				return ContentProperty.PropertyType;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// When overridden in a derived class, returns whether resetting an object changes its value.
		/// </summary>
		/// <param name="component">The component to test for reset capability.</param>
		/// <returns>
		/// true if resetting the component changes its value; otherwise, false.
		/// </returns>
		public override bool CanResetValue(object component)
		{
			if ((!ContentProperty.HasDefaultValue) || (ContentProperty.IsReadOnly))
				return false;

			return !Equals(GetValue(component), ContentProperty.DefaultValue);
		}

		/// <summary>
		/// When overridden in a derived class, resets the value for this property of the component to the default value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be reset to the default value.</param>
		public override void ResetValue(object component)
		{
			if (CanResetValue(component))
			{
				SetValue(component, ContentProperty.DefaultValue);
			}
		}

		/// <summary>
		/// When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
		/// </summary>
		/// <param name="component">The component with the property to be examined for persistence.</param>
		/// <returns>
		/// true if the property should be persisted; otherwise, false.
		/// </returns>
		public override bool ShouldSerializeValue(object component)
		{
			if (!ContentProperty.HasDefaultValue)
				return false;

			return !Equals(GetValue(component), ContentProperty.DefaultValue);
		}

		/// <summary>
		/// When overridden in a derived class, sets the value of the component to a different value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be set.</param>
		/// <param name="value">The new value.</param>
		public override void SetValue(object component, object value)
		{
			if (ContentProperty.IsReadOnly)
				return;

			ContentProperty.SetValue(value);
		}

		/// <summary>
		/// When overridden in a derived class, gets the current value of the property on a component.
		/// </summary>
		/// <param name="component">The component with the property for which to retrieve the value.</param>
		/// <returns>
		/// The value of a property for a given component.
		/// </returns>
		public override object GetValue(object component)
		{
			return ContentProperty.GetValue<object>();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentPropertyDescriptor"/> class.
		/// </summary>
		/// <param name="property">The property to bind to the descriptor..</param>
		public ContentPropertyDescriptor(ContentProperty property)
			: base(property.Name, property.RetrieveAttributes())
		{
			ContentProperty = property;
		}
		#endregion
	}
}
