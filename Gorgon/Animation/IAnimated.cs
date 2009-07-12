#region MIT.
// 
// Gorgon.
// Copyright (C) 2009 Michael Winsor
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
// Created: Saturday, June 13, 2009 1:27:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An interface to expose methods for animated objects.
	/// </summary>
	/// <remarks>All objects that are meant to be animated using the <see cref="GorgonLibrary.Graphics.Animation">Gorgon animation</see> interface have to implement IAnimated.</remarks>
	public interface IAnimated
	{
		/// <summary>
		/// Property to return a list of animations attached to the object.
		/// </summary>
		AnimationList Animations
		{
			get;
		}

		/// <summary>
		/// Function to apply the current time in an animation to the objects animated properties.
		/// </summary>
		/// <remarks>This function will do the actual work of updating the properties (marked with the <see cref="GorgonLibrary.Graphics.AnimatedAttribute">Animated</see> attribute).  
		/// It does this by applying the values at the current interpolated (or actual depending on time) key frame to the property that's bound to the track.</remarks>
		void ApplyAnimations();
	}
}
