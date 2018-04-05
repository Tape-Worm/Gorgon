#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, June 10, 2013 8:56:47 PM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// Vertex shader states.
	/// </summary>
	public class GorgonVertexShaderState
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public GorgonVertexShader Shader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the constant buffers for the vertex shader.
		/// </summary>
		public GorgonConstantBuffers ConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to set or return the shader resource views for resources on this vertex shader.
		/// </summary>
		public GorgonShaderResourceViews ResourceViews
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the pixel shader state from another set of states.
		/// </summary>
		/// <param name="state">The state to copy.</param>
		internal void CopyStates(GorgonVertexShaderState state)
		{
			if (state == null)
			{
				Reset();
				return;
			}

			ConstantBuffers.Clear();
			ResourceViews.Clear();
			ConstantBuffers.CopyFrom(state.ConstantBuffers);
			ResourceViews.CopyFrom(state.ResourceViews);
		}

		/// <summary>
		/// Function to reset the state object back to its original state.
		/// </summary>
		public void Reset()
		{
			Shader = null;

			ConstantBuffers.Clear();
			ResourceViews.Clear();
		}

		/// <summary>
		/// Function to retrieve the changes for this shader state.
		/// </summary>
		/// <param name="other">The other instance to compare with this instance.</param>
		/// <returns>A <see cref="ShaderStateChangeFlags"/> value representing which parts of the state have changed.</returns>
		public ShaderStateChangeFlags GetChanges(GorgonVertexShaderState other)
		{
			var result = ShaderStateChangeFlags.None;

			if (other == null)
			{
				return ShaderStateChangeFlags.Constants | ShaderStateChangeFlags.Shader | ShaderStateChangeFlags.ShaderResourceViews;
			}

			if (other.Shader != Shader)
			{
				result |= ShaderStateChangeFlags.Shader;
			}

			if (!GorgonConstantBuffers.Equals(ConstantBuffers, other.ConstantBuffers))
			{
				result |= ShaderStateChangeFlags.Constants;
			}

			if (!GorgonShaderResourceViews.Equals(ResourceViews, other.ResourceViews))
			{
				result |= ShaderStateChangeFlags.ShaderResourceViews;
			}

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexShaderState"/> class.
		/// </summary>
		public GorgonVertexShaderState()
		{
			ConstantBuffers = new GorgonConstantBuffers();
			ResourceViews = new GorgonShaderResourceViews();
		}
		#endregion
	}
}