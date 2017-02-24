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
// Created: Monday, June 10, 2013 8:56:42 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Flags to indicate which part of the shader state has changed.
	/// </summary>
	[Flags]
	public enum ShaderStateChangeFlags
	{
		/// <summary>
		/// Nothing has changed.
		/// </summary>
		None = 0,
		/// <summary>
		/// The shader has changed.
		/// </summary>
		Shader = 0x1,
		/// <summary>
		/// The shader constants have changed.
		/// </summary>
		Constants = 0x2,
		/// <summary>
		/// The shader resource views have changed.
		/// </summary>
		ShaderResourceViews = 0x4,
		/// <summary>
		/// The texture sampler states have changed.
		/// </summary>
		SamplerStates = 0x8
	}

	/// <summary>
	/// Pixel shader states.
	/// </summary>
	public class GorgonPixelShaderState
    {
		#region Variables.
		// The vertex shader.
		// The constant buffers for the pixel shader.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public GorgonPixelShader Shader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the constant buffers for the pixel shader.
		/// </summary>
		public GorgonConstantBuffers ConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to set or return the shader resource views for resources on this pixel shader.
		/// </summary>
		public GorgonShaderResourceViews ResourceViews
		{
			get;
		}

		/// <summary>
		/// Property to set or return the texture sampler states for texture resources on this pixel shader.
		/// </summary>
		public GorgonSamplerStates SamplerStates
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the pixel shader state from another set of states.
		/// </summary>
		/// <param name="state">The state to copy.</param>
		internal void CopyStates(GorgonPixelShaderState state)
		{
			if (state == null)
			{
				Reset();
				return;
			}

			SamplerStates.Clear();
			ConstantBuffers.Clear();
			ResourceViews.Clear();
			SamplerStates.CopyFrom(state.SamplerStates);
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
			SamplerStates.Clear();
		}

		/// <summary>
		/// Function to retrieve the changes for this shader state.
		/// </summary>
		/// <param name="other">The other instance to compare with this instance.</param>
		/// <returns>A <see cref="ShaderStateChangeFlags"/> value representing which parts of the state have changed.</returns>
		public ShaderStateChangeFlags GetChanges(GorgonPixelShaderState other)
		{
			var result = ShaderStateChangeFlags.None;

			if (other == null)
			{
				return ShaderStateChangeFlags.Constants | ShaderStateChangeFlags.Shader | ShaderStateChangeFlags.ShaderResourceViews | ShaderStateChangeFlags.SamplerStates;
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

			if (!GorgonSamplerStates.Equals(SamplerStates, other.SamplerStates))
			{
				result |= ShaderStateChangeFlags.SamplerStates;
			}

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPixelShaderState"/> class.
		/// </summary>
		public GorgonPixelShaderState()
		{
			ConstantBuffers = new GorgonConstantBuffers();
			ResourceViews = new GorgonShaderResourceViews();
			SamplerStates = new GorgonSamplerStates();
		}
		#endregion
	}
}