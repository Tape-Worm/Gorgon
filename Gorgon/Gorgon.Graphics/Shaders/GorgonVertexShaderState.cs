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

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Defines what has been changed on the vertex shader state.
	/// </summary>
	[Flags]
	public enum VertexShaderStateChangedFlags
		: ulong
	{
		/// <summary>
		/// No changes.
		/// </summary>
		None = 0,
		/// <summary> 
		/// The shader itself has been changed.
		/// </summary>
		Shader = 0x1
	}

	/// <summary>
	/// Vertex shader states.
	/// </summary>
	public class GorgonVertexShaderState
	{
		#region Variables.
		// The vertex shader.
		private GorgonVertexShader _shader;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the vertex shader state as a <see cref="VertexShaderStateChangedFlags"/> value to determine state change.
		/// </summary>
		/// <remarks>
		/// This is used to determine what states have changed since the last vertex shader state state was set. This is used to reduce overhead when changing states during a frame.
		/// </remarks>
		public VertexShaderStateChangedFlags VertexShaderStateChangedFlags
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public GorgonVertexShader Shader
		{
			get
			{
				return _shader;
			}
			set
			{
				if (_shader == value)
				{
					return;
				}

				_shader = value;
				VertexShaderStateChangedFlags |= VertexShaderStateChangedFlags.Shader;
			}
		}
		#endregion
	}
}