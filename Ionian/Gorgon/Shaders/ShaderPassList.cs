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
