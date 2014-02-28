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
// Created: Thursday, February 27, 2014 11:13:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A collection of dependencies.
	/// </summary>
	public class DependencyCollection
		: GorgonBaseNamedObjectCollection<Dependency>
	{
		#region Properties.
		/// <summary>
		/// Property to return a dependency by index.
		/// </summary>
		public Dependency this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to set or return a dependency by name.
		/// </summary>
		/// <remarks>Setting a dependency to NULL (Nothing in VB.Net) will remove it from the collection.</remarks>
		public Dependency this[string name]
		{
			get
			{
				return GetItem(name);
			}
			set
			{
				if (value == null)
				{
					if (Contains(name))
					{
						RemoveItem(name);
					}

					return;
				}

				if (Contains(name))
				{
					// If the dependency exists, but has a different name, then remove the old and re-add it.
					if (!string.Equals(name, value.Path, StringComparison.OrdinalIgnoreCase))
					{
						RemoveItem(name);
						AddItem(value);
						return;
					}

					SetItem(name, value);
					return;
				}

				AddItem(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize the collection into an XML node.
		/// </summary>
		/// <returns>The XML nodes containing the serialized collection.</returns>
		internal IEnumerable<XElement> Serialize()
		{
			return this.Select(dependency => dependency.Serialize()).ToList();
		}

		/// <summary>
		/// Function to deserialize a dependency collection from an XML node.
		/// </summary>
		/// <param name="dependenciesNode">The list of nodes that contain the dependencies.</param>
		/// <returns>The deserialized dependencies.</returns>
		internal static DependencyCollection Deserialize(IEnumerable<XElement> dependenciesNode)
		{
			var result = new DependencyCollection();

			foreach (XElement dependencyNode in dependenciesNode)
			{
				Dependency dependency = Dependency.Deserialize(dependencyNode);
				result[dependency.Path] = dependency;
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyCollection"/> class.
		/// </summary>
		internal DependencyCollection()
			: base(false)
		{
			
		}
		#endregion
	}
}
