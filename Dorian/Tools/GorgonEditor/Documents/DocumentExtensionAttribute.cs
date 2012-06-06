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
// Created: Tuesday, June 05, 2012 3:41:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Used to specify which extensions map to a document type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	class DocumentExtensionAttribute
		: Attribute
	{
		#region Properties.
		/// <summary>
		/// Property to return the extensions for the document.
		/// </summary>
		public IList<string> Extensions
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the document type.
		/// </summary>
		public Type DocumentType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the document description.
		/// </summary>
		public string DocumentDescription
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the editor can open the file or not.
		/// </summary>
		public bool CanOpen
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentExtensionAttribute"/> class.
		/// </summary>
		/// <param name="extensions">The extensions for the document.</param>
		/// <param name="documentDescription">Description of the document type.</param>
		/// <param name="type">The type of the document.</param>
		/// <param name="canOpen">TRUE if the editor can open the file, FALSE if not.</param>
		/// <remarks>The <paramref name="extensions"/> parameter can take multiple file extensions delimited by a semi-colon.  For example "zip;rar;tga" will be parsed into 3 extensions for the same type.</remarks>
		public DocumentExtensionAttribute(Type type, string extensions, string documentDescription, bool canOpen)
		{
			DocumentType = type;

			if (string.IsNullOrEmpty(extensions))
				extensions = "*";

			var list = extensions.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);

			if (list.Length == 0)
			{
				Extensions = new string[] { "*" };
				return;
			}

			for (int i = 0; i < list.Length; i++)
			{
				list[i] = list[i].ToLower();

				if (list[i].StartsWith("."))
				{
					if (list[i].Length > 1)
						list[i] = list[i].Substring(1);
				}
			}

			if (documentDescription == null)
				documentDescription = string.Empty;

			Extensions = list;
			CanOpen = canOpen;
			DocumentDescription = documentDescription;
		}
		#endregion
	}
}
