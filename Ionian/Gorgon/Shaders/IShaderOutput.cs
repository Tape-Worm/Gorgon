#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, June 25, 2008 12:10:37 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface to define a shader renderer.
	/// </summary>
	/// <remarks>This defines the component of a shader that can be applied to the rendering of a scene.</remarks>
	public interface IShaderRenderer
	{
		/// <summary>
		/// Function to begin the rendering with the shader.
		/// </summary>
		void Begin();

		/// <summary>
		/// Function to render with the shader.
		/// </summary>
		void Render();

		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void End();
	}
}
