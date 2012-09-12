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
// Created: Monday, September 3, 2012 7:57:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A track for an animated property on a renderable object.
	/// </summary>
	public class GorgonAnimationTrack
		: GorgonNamedObject
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of data for this track.
		/// </summary>
		public Type DataType
		{
			get;
			private set;
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationTrack" /> class.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="type">Type of data in the track.</param>
		internal GorgonAnimationTrack(string name, Type type)
			: base(name)
		{
		}
		#endregion
	}
}
