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
// Created: Saturday, September 23, 2006 1:27:55 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a technique within a shader.
	/// </summary>
	public class ShaderTechnique
		: NamedObject, IShaderRenderer
	{
		#region Variables.
		private D3D9.EffectHandle _effectHandle;		// Handle to the technique.
		private ShaderPassList _passes;					// List of passes.
		private FXShader _owner;							// Shader that owns the technique.
		private ShaderParameterList _parameters;		// Shader parameters.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the effect handle for the technique.
		/// </summary>
		internal D3D9.EffectHandle D3DEffectHandle
		{
			get
			{
				return _effectHandle;
			}
		}

		/// <summary>
		/// Property to return the parameters for the technique.
		/// </summary>
		public ShaderParameterList Parameters
		{
			get
			{
				return _parameters;
			}
		}

		/// <summary>
		/// Property to return the shader that owns this technique.
		/// </summary>
		public FXShader Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the list of passes for the technique.
		/// </summary>
		public ShaderPassList Passes
		{
			get
			{
				return _passes;
			}
		}

		/// <summary>
		/// Property to return whether this technique is valid or not.
		/// </summary>
		public bool Valid
		{
			get
			{
				return _owner.D3DEffect.ValidateTechnique(_effectHandle);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the D3D technique information.
		/// </summary>
		/// <param name="index">Index of the technique.</param>
		private void GetTechnique(int index)
		{
			try
			{
				// Get the handle.
				_effectHandle = _owner.D3DEffect.GetTechnique(index);
				Name = _owner.D3DEffect.GetTechniqueDescription(_effectHandle).Name;
				_passes.Add(this);
			}
			catch (Exception ex)
			{
				GorgonException.Repackage(GorgonErrors.CannotReadData, "Error trying to retrieve the technique information.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shader">Shader that owns this technique.</param>
		/// <param name="index">Index of the technique.</param>
		internal ShaderTechnique(FXShader shader, int index)
			: base("NO_NAME")
		{
			_owner = shader;
			_effectHandle = null;
			_passes = new ShaderPassList();
			_parameters = new ShaderParameterList();
			GetTechnique(index);
		}
		#endregion

		#region IShaderRenderer Members
		/// <summary>
		/// Function to begin the rendering with the shader.
		/// </summary>
		void IShaderRenderer.Begin()
		{
			if (_owner.Parameters.Contains("_projectionMatrix"))
				_owner.Parameters["_projectionMatrix"].SetValue(Gorgon.CurrentClippingViewport.ProjectionMatrix);

			_owner.D3DEffect.Technique = _effectHandle; 
			_owner.D3DEffect.Begin(D3D9.FX.None);
		}


		/// <summary>
		/// Function to render with the shader.
		/// </summary>
		void IShaderRenderer.Render()
		{
			// Begin rendering each pass.
			for (int i = 0; i < _passes.Count; i++)
			{
				_owner.D3DEffect.BeginPass(i);
				Gorgon.Renderer.DrawCachedTriangles();
				_owner.D3DEffect.EndPass();
			}
		}

		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void IShaderRenderer.End()
		{
			_owner.D3DEffect.End();
		}
		#endregion
	}
}

