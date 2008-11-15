#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Saturday, September 23, 2006 1:27:31 AM
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
            D3D9.TechniqueDescription techniqueInfo = default(D3D9.TechniqueDescription);		// Technique information.

            if (technique == null)
                throw new ArgumentNullException("technique");
            
				ShaderPass newPass = null;						// Pass.

			try
			{
				techniqueInfo = technique.Owner.D3DEffect.GetTechniqueDescription(technique.D3DEffectHandle);
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotCreate, "Error while retrieving the HLSL techniques.", ex);
			}

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

			if (techniqueInfo.Passes < 1)
				throw new GorgonException(GorgonErrors.CannotCreate, "The are no passes for the technique '" + techniqueInfo.Name + "'.");
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
