#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, July 02, 2011 12:12:59 PM
// 
#endregion

using System;
using Gorgon.Collections;

namespace Gorgon.Input
{
	/// <summary>
	/// A collection of custom HID properties.
	/// </summary>
	public class GorgonCustomHIDPropertyCollection
		: GorgonBaseNamedObjectCollection<GorgonCustomHIDProperty>
	{
		#region Properties.
		/// <summary>
		/// Function to return the <see cref="Gorgon.Input.GorgonCustomHIDProperty"/> at the specified index.
		/// </summary>
		public GorgonCustomHIDProperty this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Function to return the <see cref="Gorgon.Input.GorgonCustomHIDProperty"/> with the specified name.
		/// </summary>
		public GorgonCustomHIDProperty this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a property to the list.
		/// </summary>
		/// <param name="property">Property to add.</param>
		internal void Add(GorgonCustomHIDProperty property)
		{
		    if (property == null)
		    {
		        throw new ArgumentNullException("property");
		    }

		    AddItem(property);
		}

		/// <summary>
		/// Function to clear the properties.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCustomHIDPropertyCollection"/> class.
		/// </summary>
		internal GorgonCustomHIDPropertyCollection()
			: base(false)
		{
		}
		#endregion
	}
}
