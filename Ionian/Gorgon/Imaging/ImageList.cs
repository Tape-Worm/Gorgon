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
	/// Object representing a list of images.
	/// </summary>
	public class ImageList
		: BaseCollection<Image>
	{
		#region Properties.
		/// <summary>
		/// Property to return an image by index.
		/// </summary>
		public Image this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return an image by its key.
		/// </summary>
		public Image this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an image to the list.
		/// </summary>
		/// <param name="image">Image to add.</param>
		internal void Add(Image image)
		{
			AddItem(image.Name, image);
		}

		/// <summary>
		/// Function to remove an image by name from the list.
		/// </summary>
		/// <param name="imageName">Name of the image to remove.</param>
		internal void Remove(string imageName)
		{
			RemoveItem(imageName);
		}

		/// <summary>
		/// Function to remove an image by its index.
		/// </summary>
		/// <param name="index">Index of the image to remove.</param>
		internal void Remove(int index)
		{
			RemoveItem(index);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ImageList()
			: base(16, false)
		{
		}
		#endregion
	}
}
