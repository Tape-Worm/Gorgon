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
// Created: Saturday, September 23, 2006 1:28:13 AM
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
		internal void Add(FXShader shader)
		{
            if (shader == null)
                throw new ArgumentNullException("shader");

			if (shader.D3DEffect.Description.Techniques < 1)
				throw new GorgonException(GorgonErrors.CannotCreate, "There are no techniues in '" + shader.Name + "'.");
                        
			ShaderTechnique newTechnique = null;			// Technique.

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
