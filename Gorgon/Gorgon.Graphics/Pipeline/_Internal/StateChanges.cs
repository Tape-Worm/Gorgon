#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 27, 2016 11:23:40 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Records the pipeline state changes and sub-state changes.
	/// </summary>
	struct StateChanges
		: IGorgonEquatableByRef<StateChanges>
	{
		/// <summary>
		/// The currently active flags.
		/// </summary>
		public PipelineStateChangeFlags PipelineFlags;
		/// <summary>
		/// The currently active pixel shader sub state flags.
		/// </summary>
		public ShaderStateChangeFlags PixelShaderStateFlags;
		/// <summary>
		/// The currently active vertex shader sub state flags.
		/// </summary>
		public ShaderStateChangeFlags VertexShaderStateFlags;

		/// <summary>
		/// Function to compare this instance with another.
		/// </summary>
		/// <param name="other">The other instance to use for comparison.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(StateChanges other)
		{
			return Equals(ref other);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ref StateChanges other)
		{
			return PipelineFlags == other.PipelineFlags && PixelShaderStateFlags == other.PixelShaderStateFlags && VertexShaderStateFlags == other.VertexShaderStateFlags;
		}
	}
}
