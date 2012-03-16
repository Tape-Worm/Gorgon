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
// Created: Thursday, March 15, 2012 7:34:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A box structure with width, height and depth.
	/// </summary>
	public struct GorgonBox
	{
		/// <summary>
		/// Horizontal position.
		/// </summary>
		public int X;
		/// <summary>
		/// Vertical position
		/// </summary>
		public int Y;
		/// <summary>
		/// Depth position.
		/// </summary>
		public int Z;
		/// <summary>
		/// Width of the box.
		/// </summary>
		public int Width;
		/// <summary>
		/// Height of the box.
		/// </summary>
		public int Height;
		/// <summary>
		/// Depth of the box.
		/// </summary>
		public int Depth;

		/// <summary>
		/// Property to retrieve the resource region.
		/// </summary>
		/// <returns>The D3D resource region.</returns>
		internal D3D.ResourceRegion Convert
		{
			get
			{
				return new D3D.ResourceRegion()
				{
					Front = Front,
					Back = Back,
					Top = Top,
					Left = Left,
					Right = Right,
					Bottom = Bottom
				};
			}
		}

		/// <summary>
		/// Property to set or return the left value for the box.
		/// </summary>
		public int Left
		{
			get
			{
				return X;
			}
			set
			{
				X = value;
			}
		}

		/// <summary>
		/// Property to set or return the top value for the box.
		/// </summary>
		public int Top
		{
			get
			{
				return Y;
			}
			set
			{
				Y = value;
			}
		}

		/// <summary>
		/// Property to set or return the front value for the box.
		/// </summary>
		public int Front
		{
			get
			{
				return Z;
			}
			set
			{
				Z = value;
			}
		}

		/// <summary>
		/// Property to return the right value of the box.
		/// </summary>
		public int Right
		{
			get
			{
				return X + Width;
			}
		}

		/// <summary>
		/// Property to return the bottom value of the box.
		/// </summary>
		public int Bottom
		{
			get
			{
				return Y + Height;
			}
		}

		/// <summary>
		/// Property to return the back value of the box.
		/// </summary>
		public int Back
		{
			get
			{
				return Z + Depth;
			}
		}		
	}
}
