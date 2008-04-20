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
// Created: Saturday, September 23, 2006 2:29:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a list of shader values.
	/// </summary>
	public class ShaderParameterList
		: Collection<ShaderParameter>
	{
		#region Methods.
		/// <summary>
		/// Function to add a parameter to the list.
		/// </summary>
		/// <param name="parameter">Parameter to add.</param>
		internal void Add(ShaderParameter parameter)
		{
			// Update collection.
			if (Contains(parameter.Name))
				SetItem(parameter.Name, parameter);
			else
				AddItem(parameter.Name, parameter);
		}

		/// <summary>
		/// Function to add parameters.
		/// </summary>
		/// <param name="shader">Shader that owns these parameters.</param>
		internal void Add(Shader shader)
		{
			try
			{
				ShaderParameter newParameter = null;			// Shader parameter.

				// If there aren't any parameters, then exit.
				if (shader.D3DEffect.Description.Parameters < 1)
					return;
				
				// Get technique handle.
				for (int i = 0; i < shader.D3DEffect.Description.Parameters; i++)
				{
					D3D9.EffectHandle handle;					// Handle.
					D3D9.ParameterDescription description;		// Description.

					// Get parameter handle.
					handle = shader.D3DEffect.GetParameter(null, i);
					description = shader.D3DEffect.GetParameterDescription(handle);

					// Get the parameter.
					newParameter = new ShaderParameter(description, handle, shader, i);

					// Update collection.
					Add(newParameter);

					// Check technique.
					foreach (ShaderTechnique technique in shader.Techniques)
					{
						if (shader.D3DEffect.IsParameterUsed(handle, technique.D3DEffectHandle))
							technique.Parameters.Add(new ShaderParameter(description, handle, shader, i));
					}
				}
			}
			catch (Exception ex)
			{
				throw new ShaderCannotGetParametersException(shader.Name, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ShaderParameterList()
		{
		}
		#endregion
	}
}
