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
// Created: Tuesday, September 23, 2014 2:28:35 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Gorgon.Core;

namespace Gorgon.Examples
{
	/// <summary>
	/// A category for our examples.
	/// </summary>
	class Category
		: INamedObject, IDisposable
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the examples for this category.
		/// </summary>
		public IReadOnlyList<Example> Examples
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a category object from a category node.
		/// </summary>
		/// <param name="categoryNode">Node containing object data.</param>
		public static Category Read(XElement categoryNode)
		{
			XAttribute nameAttr = categoryNode.Attribute("name");

			if ((nameAttr == null)
			    || (string.IsNullOrWhiteSpace(nameAttr.Value)))
			{
				return null;
			}

			var result = new Category
			             {
				             Name = nameAttr.Value
			             };

			IEnumerable<XElement> examples = categoryNode.Elements("Example");


			result.Examples = ExampleLoader.Read(result.Name, examples);

			if ((result.Examples == null)
				|| (result.Examples.Count == 0))
			{
				return null;
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Prevents a default instance of the <see cref="Category"/> class from being created.
		/// </summary>
		private Category()
		{
		}
		#endregion


		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Examples != null)
				{
					foreach (Example example in Examples)
					{
						example.Dispose();
					}

					Examples = null;
				}
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);	
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
