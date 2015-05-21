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
// Created: Sunday, March 18, 2012 11:32:16 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Collections;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A collection of shader include files.
	/// </summary>
	public class GorgonShaderIncludeCollection
		: GorgonBaseNamedObjectDictionary<GorgonShaderInclude>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return an include file in the collection.
		/// </summary>
		public GorgonShaderInclude this[string includeName]
		{
			get
			{
				return GetItem(includeName);
			}
			set
			{
			    if (includeName == null)
			    {
			        return;
			    }

			    if (string.IsNullOrWhiteSpace(includeName))
			    {
			        return;
			    }

			    if (!Contains(value.Name))
				{
					AddItem(value);
					return;
				}

				SetItem(value.Name, value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a list of shader includes to the collection.
		/// </summary>
		/// <param name="includes">Include files to add.</param>
		public void AddRange(IEnumerable<GorgonShaderInclude> includes)
		{
			AddItems(includes);
		}

		/// <summary>
		/// Function to add a new include file to the collection.
		/// </summary>
		/// <param name="includeFile">Include file to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="includeFile"/> parameter contains a NULL (Nothing in VB.Net) file name or NULL source code.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the includeFile parameter has an empty file name or source code.
		/// <para>-or-</para>
		/// <para>Thrown when the include file name already exists in this collection.</para>
		/// </exception>
		public void Add(GorgonShaderInclude includeFile)
		{
			AddItem(includeFile);
		}

		/// <summary>
		/// Function to add a new include file to the collection.
		/// </summary>
		/// <param name="includeName">Filename for the include file.</param>
		/// <param name="includeSourceCode">Source code for the include file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="includeName"/> or the <paramref name="includeSourceCode"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the includeFileName or the includeSource parameters are empty.
		/// <para>-or-</para>
		/// <para>Thrown when the includeFileName already exists in this collection.</para>
		/// </exception>
		public void Add(string includeName, string includeSourceCode)
		{
			AddItem(new GorgonShaderInclude(includeName, includeSourceCode));
		}

		/// <summary>
		/// Function to remove an include file from the collection.
		/// </summary>
		/// <param name="includeName">File name of the include file to remove.</param>
		public void Remove(string includeName)
		{
			RemoveItem(includeName);
		}

		/// <summary>
		/// Function to remove an include file from the collection.
		/// </summary>
		/// <param name="include">Include file to remove.</param>
		public void Remove(GorgonShaderInclude include)
		{
			RemoveItem(include.Name);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderIncludeCollection"/> class.
		/// </summary>
		internal GorgonShaderIncludeCollection()
			: base(false)
		{
		}
		#endregion
	}
}
