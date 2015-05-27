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
// Created: Tuesday, September 23, 2014 2:35:19 AM
// 
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gorgon.Core.Collections.Specialized;
using Gorgon.Examples.Properties;

namespace Gorgon.Examples
{
	/// <summary>
	/// Loads categories from the manifest XML.
	/// </summary>
	static class CategoryLoader
	{
		/// <summary>
		/// Function to read the categories and examples from the embedded example list.
		/// </summary>
		/// <returns>A new category collection.</returns>
		public static IReadOnlyList<Category> Read()
		{
			var result = new List<Category>();
			var document = XDocument.Parse(Resources.Examples);

			XElement root = document.Element("Examples");

			if (root == null)
			{
				return result;
			}

			IEnumerable<XElement> categories = root.Elements("Category");

			result.AddRange(categories.Select(Category.Read));

			return result;
		}
	}
}
