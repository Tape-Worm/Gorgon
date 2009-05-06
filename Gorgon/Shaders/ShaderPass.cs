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
// Created: Saturday, September 23, 2006 1:27:12 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a pass within the shader.
	/// </summary>
	public class ShaderPass
		: NamedObject, IShaderRenderer
	{
		#region Variables.
		private D3D9.EffectHandle _effectHandle;		// Handle to the pass.
		private ShaderTechnique _owner;					// Technique that owns this pass.
		private int _passIndex = 0;						// Index of this pass.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the owning technique for this pass.
		/// </summary>
		public ShaderTechnique Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the index of the shader pass.
		/// </summary>
		public int PassIndex
		{
			get
			{
				return _passIndex;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get pass information.
		/// </summary>
		/// <param name="index">Index of the pass.</param>
		private void GetPass(int index)
		{
			try
			{
				// Get handle and name.
				_passIndex = index;
				_effectHandle = _owner.Owner.D3DEffect.GetPass(_owner.D3DEffectHandle, index);
				Name = _owner.Owner.D3DEffect.GetPassDescription(_effectHandle).Name;
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotReadData, "Could not retrieve the shader passes.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Owning technique.</param>
		/// <param name="index">Index of the pass.</param>
		internal ShaderPass(ShaderTechnique technique, int index)
			: base("NO_NAME")
		{
			_owner = technique;
			_effectHandle = null;
			GetPass(index);
		}
		#endregion

		#region IShaderRenderer Members
		/// <summary>
		/// Function to begin the rendering with the shader.
		/// </summary>
		void IShaderRenderer.Begin()
		{
			if (_owner.Owner.Parameters.Contains("_projectionMatrix"))
				_owner.Owner.Parameters["_projectionMatrix"].SetValue(Gorgon.CurrentClippingViewport.ProjectionMatrix);

			_owner.Owner.D3DEffect.Technique = _owner.D3DEffectHandle;
			_owner.Owner.D3DEffect.Begin(D3D9.FX.None);
		}

		/// <summary>
		/// Function to render with the shader.
		/// </summary>
		/// <param name="primitiveStyle">Type of primitive to render.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		void IShaderRenderer.Render(PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount)
		{
			_owner.Owner.D3DEffect.BeginPass(_passIndex);
			Gorgon.Renderer.DrawCachedTriangles(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
			_owner.Owner.D3DEffect.EndPass();
		}

		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void IShaderRenderer.End()
		{
			
			_owner.Owner.D3DEffect.End();
		}
		#endregion
	}
}
