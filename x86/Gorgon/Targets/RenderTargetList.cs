#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Tuesday, August 02, 2005 12:03:56 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// List of render targets.
	/// </summary>
	public class RenderTargetList 
		: BaseCollection<RenderTarget>
	{
		#region Properties.
		/// <summary>
		/// Property to return a render target by index.
		/// </summary>
		public RenderTarget this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a render target by name.
		/// </summary>
		public RenderTarget this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a target.
		/// </summary>
		/// <param name="target">Target to add.</param>
		internal void Add(RenderTarget target)
		{
			if (Contains(target.Name))
				throw new RenderTargetAlreadyExistsException(target.Name);

			Items.Add(target.Name, target);
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
		internal RenderTargetList()
			: base(4, true)
		{
		}
		#endregion
	}
}
