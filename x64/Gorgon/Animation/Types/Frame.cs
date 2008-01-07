#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Friday, November 24, 2006 2:29:31 PM
// 
#endregion

using System;
using SharpUtilities.Mathematics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Value type containing data for a frame switch.
	/// </summary>
	public struct Frame
	{
		#region Variables.
		/// <summary>
		/// Offset of the frame into the bound image.
		/// </summary>
		public Vector2D Offset;

		/// <summary>
		/// Property to return the size of the frame.
		/// </summary>
		public Vector2D Size;

		/// <summary>
		/// Property to return the image that the frame is located on.
		/// </summary>
		public Image Image;
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="image">Image the frame is located on.</param>
		/// <param name="offset">Offset of the frame within the bound image.</param>
		/// <param name="size">Size of the frame.</param>		
		public Frame(Image image, Vector2D offset, Vector2D size)
		{
			Image = image;
			Offset = offset;
			Size = size;
		}
		#endregion
	}
}