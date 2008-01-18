#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, September 04, 2007 12:29:42 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for drop-in panels.
	/// </summary>
	public interface IDropIn
	{
		/// <summary>
		/// Property to set or return the current key frame time.
		/// </summary>
		float CurrentTime
		{
			get;
			set;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		void CleanUp();
	}
}
