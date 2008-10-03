#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Friday, May 19, 2006 10:15:53 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;
using Drawing = System.Drawing;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a list of fonts.
	/// </summary>
	public class FontList
		: BaseCollection<Font>
	{
		#region Properties.
		/// <summary>
		/// Property to return an font by index.
		/// </summary>
		public Font this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return an font by its key.
		/// </summary>
		public Font this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a font to the list.
		/// </summary>
		/// <param name="font">Font to add.</param>
		internal void Add(Font font)
		{
			AddItem(font.Name, font);
		}

		/// <summary>
		/// Function to remove a font by name from the list.
		/// </summary>
		/// <param name="fontName">Name of the font to remove.</param>
		internal void Remove(string fontName)
		{
			RemoveItem(fontName);
		}

		/// <summary>
		/// Function to remove an font by its index.
		/// </summary>
		/// <param name="index">Index of the font to remove.</param>
		internal void Remove(int index)
		{
			RemoveItem(index);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal FontList()
			: base(16, false)
		{
		}
		#endregion
	}
}
