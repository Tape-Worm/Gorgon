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
// Created: Tuesday, May 27, 2008 12:32:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Exception thrown when an input system is not bound to the GUI.
	/// </summary>
	public class GUIInputInvalidException
		: GorgonException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIInputInvalidException"/> class.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="inner">The inner exception that caused this exception.</param>
		public GUIInputInvalidException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GUIInputInvalidException"/> class.
		/// </summary>
		/// <param name="message">Error message.</param>
		public GUIInputInvalidException(string message)
			: this(message, null)
		{
		}
	}

	/// <summary>
	/// Exception thrown when an object of an invalid type is added to a collection or assigned as an owner.
	/// </summary>
	public class GUIInvalidTypeException
		: GorgonException 
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIInvalidTypeException"/> class.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="inner">The inner exception that caused this exception.</param>
		public GUIInvalidTypeException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GUIInvalidTypeException"/> class.
		/// </summary>
		/// <param name="message">Error message.</param>
		public GUIInvalidTypeException(string message)
			: this(message, null)
		{
		}
	}
}
