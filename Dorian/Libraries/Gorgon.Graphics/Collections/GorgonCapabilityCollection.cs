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
// Created: Tuesday, July 19, 2011 8:27:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Collections
{
	/// <summary>
	/// A driver capability item.
	/// </summary>
	public struct GorgonCapability
		: INamedObject
	{
		#region Variables.
		private string _name;			// Name of the capability.
		private string _value;			// Name of the value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the value for the capability.
		/// </summary>
		public string Value
		{
			get
			{
				return _value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCapability"/> struct.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		internal GorgonCapability(string name, string value)
		{
			GorgonUtility.AssertParamString(name, "name");
			_name = name;
			_value = value;
		}
		#endregion
	
		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the capability.
		/// </summary>
		public string  Name
		{
			get 
			{ 
				return _name;
			}
		}
		#endregion
	}

	/// <summary>
	/// A list of driver capabilities.
	/// </summary>
	public class GorgonCapabilityCollection
		: GorgonBaseNamedObjectCollection<GorgonCapability>
	{
		#region Variables.
		private IList<string> _capNames = null;			// Capability names.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of capability names.
		/// </summary>
		public IEnumerable<string> Capabilities
		{
			get
			{
				return _capNames;
			}
		}

		/// <summary>
		/// Property to return a capability by name.
		/// </summary>
		public GorgonCapability this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}

		/// <summary>
		/// Property to return a capability by its index.
		/// </summary>
		public GorgonCapability this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a list of capabilities to this list.
		/// </summary>
		/// <param name="capabilities">List of capabilities to add.</param>
		internal void AddCapabilities(IEnumerable<KeyValuePair<string, string>> capabilities)
		{
			foreach (var item in capabilities)
				AddItem(new GorgonCapability(item.Key, item.Value));
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCapabilityCollection"/> class.
		/// </summary>
		internal GorgonCapabilityCollection()
			: base(false)
		{
			_capNames = new List<string>();
		}
		#endregion
	}
}
