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
// Created: Saturday, September 23, 2006 1:27:31 AM
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
	/// Object representing a list of passes within a shader technique.
	/// </summary>
	public class ShaderPassList
		: Collection<ShaderPass>
	{
		#region Methods.
		/// <summary>
		/// Function to add the passes for the technique.
		/// </summary>
		/// <param name="technique">Technique to use.</param>
		internal void Add(ShaderTechnique technique)
		{
            D3D9.TechniqueDescription techniqueInfo;		// Technique information.

            if (technique == null)
                throw new ArgumentNullException("technique");
            
            try
			{
				ShaderPass newPass = null;						// Pass.

				techniqueInfo = technique.Owner.D3DEffect.GetTechniqueDescription(technique.D3DEffectHandle);

				// Get technique handle.
				for (int i = 0; i < techniqueInfo.Passes; i++)
				{
					newPass = new ShaderPass(technique, i);

					// Update collection.
					if (Contains(newPass.Name))
						SetItem(newPass.Name, newPass);
					else
						AddItem(newPass.Name, newPass);
				}
			}
			catch (Exception ex)
			{
				throw new ShaderCannotGetTechniquesException(technique.Owner.Name, ex);
			}

            if (techniqueInfo.Passes < 1)
                throw new ShaderCannotGetPassesException(technique);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ShaderPassList()
			: base(2, true)
		{
		}
		#endregion
	}
}
