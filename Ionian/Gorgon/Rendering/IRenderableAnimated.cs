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
// Created: Saturday, January 06, 2007 2:18:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface for animated renderable objects.
	/// </summary>
	public interface IRenderableAnimated
	{
		#region Properties.
		/// <summary>
		/// Property to set or return animations for the object.
		/// </summary>
		AnimationList Animations
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply animations to the object.
		/// </summary>
		void ApplyAnimations();
		#endregion
	}
}
