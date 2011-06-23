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
// Created: Monday, May 01, 2006 4:21:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;


namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Object to repesent a list of plugins.
	/// </summary>
	public class PlugInList
		: BaseCollection<PlugInEntryPoint>
	{
		#region Properties.
		/// <summary>
		/// Property to return a plug-in by its index.
		/// </summary>
		public PlugInEntryPoint this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a plug-in by its name.
		/// </summary>
		public PlugInEntryPoint this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove all the plug-ins.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to add a plug-in to the list.
		/// </summary>
		/// <param name="plugIn">Plug-in to add.</param>
		internal void Add(PlugInEntryPoint plugIn)
		{
			if (plugIn == null)
				throw new ArgumentNullException("plugIn");

			AddItem(plugIn.Name, plugIn);
		}

		/// <summary>
		/// Function to remove a plug-in.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in to remove.</param>
		internal void Remove(string plugInName)
		{
			RemoveItem(plugInName);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal PlugInList()
            : base(4, true)
		{
		}
		#endregion
	}
}
