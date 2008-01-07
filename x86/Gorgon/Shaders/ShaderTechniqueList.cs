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
// Created: Saturday, September 23, 2006 1:28:13 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;
using SharpUtilities.Collections;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a list of shader techniques.
	/// </summary>
	public class ShaderTechniqueList
		: Collection<ShaderTechnique>
	{
		#region Methods.
		/// <summary>
		/// Function to add the techniques from the shader.
		/// </summary>
		/// <param name="shader">Shader owner.</param>
		internal void Add(Shader shader)
		{			
			try
			{
				ShaderTechnique newTechnique = null;			// Technique.

				if (shader.D3DEffect.Description.Techniques < 1)
					throw new GorgonException(shader.Name + " has no techniques.");

				// Get technique handle.
				for (int i = 0;i < shader.D3DEffect.Description.Techniques;i++)
				{
					newTechnique = new ShaderTechnique(shader, i);
					// Update collection.
					if (Contains(newTechnique.Name))
						SetItem(newTechnique.Name, newTechnique);
					else
						AddItem(newTechnique.Name, newTechnique);
				}
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new ShaderNotValidException(ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ShaderTechniqueList()
			: base(4, true)
		{
		}
		#endregion
	}
}
