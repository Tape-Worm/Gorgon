#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, May 27, 2008 2:09:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Interface representing a container for GUI objects.
	/// </summary>
	public interface IGUIContainer
	{
		/// <summary>
		/// Property to set or return whether this control will clip its children.
		/// </summary>
		bool ClipChildren
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of objects contained within this container.
		/// </summary>
		GUIObjectCollection GUIObjects
		{
			get;
		}
	}
}
