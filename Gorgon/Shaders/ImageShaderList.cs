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
// Created: Friday, September 22, 2006 1:05:54 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using SharpUtilities.IO;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a list of image shaders.
	/// </summary>
	public class ImageShaderList
		: BaseCollection<ImageShader>
	{
		#region Properties.
		/// <summary>
		/// Property to return a image shader by index.
		/// </summary>
		public ImageShader this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a image shader by its key.
		/// </summary>
		public ImageShader this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a image shader to the list.
		/// </summary>
		/// <param name="imageShader">ImageShader to add.</param>
		internal void Add(ImageShader imageShader)
		{
			AddItem(imageShader.Name, imageShader);
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		internal void Remove(int index)
		{
			base.RemoveItem(index);
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		internal void Remove(string key)
		{
			base.RemoveItem(key);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ImageShaderList()
			: base(16, false)
		{
		}
		#endregion
	}
}
