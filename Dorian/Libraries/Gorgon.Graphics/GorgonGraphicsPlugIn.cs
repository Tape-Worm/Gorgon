﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, July 19, 2011 8:42:01 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The plug-in interface for a renderer object.
	/// </summary>
	public abstract class GorgonGraphicsPlugIn
		: GorgonPlugIn
	{

		#region Methods.
		/// <summary>
		/// Function to perform the actual creation of the renderer object.
		/// </summary>
		/// <returns>A new renderer object.</returns>
		/// <remarks>Implementors must use this to create the renderer object from the plug-in.</remarks>
		protected abstract GorgonRenderer CreateRenderer();

		/// <summary>
		/// Function to create a new renderer object.
		/// </summary>
		/// <returns>A new renderer object.</returns>
		internal GorgonRenderer GetRenderer()
		{
			return CreateRenderer();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGraphicsPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>Objects that implement this base class should pass in a hard coded description on the base constructor.</remarks>
		protected GorgonGraphicsPlugIn(string description)
			: base(description)
		{
		}
		#endregion
	}
}
