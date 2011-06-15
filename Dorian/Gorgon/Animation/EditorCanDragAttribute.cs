﻿#region MIT.
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
// Created: Saturday, May 03, 2008 7:15:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Type of drag to perform.
	/// </summary>
	public enum EditorDragType
	{
		/// <summary>
		/// No dragging.
		/// </summary>
		None = 0,
		/// <summary>
		/// Sprite can be dragged.
		/// </summary>
		Sprite = 1,
		/// <summary>
		/// Axis can be dragged.
		/// </summary>
		Axis = 2
	}

	/// <summary>
	/// Attribute for sprite editor to determine min/max.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class EditorCanDragAttribute
		: Attribute
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether we're dragging the sprite or not.
		/// </summary>
		public EditorDragType DragType
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorCanDragAttribute"/> class.
		/// </summary>
		/// <param name="dragType">Type of drag operation.</param>
		public EditorCanDragAttribute(EditorDragType dragType)
		{
			DragType = dragType;
		}
		#endregion
	}
}
