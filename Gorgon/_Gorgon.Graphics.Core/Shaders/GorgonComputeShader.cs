#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, December 15, 2011 9:43:36 AM
// 
#endregion

using System.IO;
using D3DCompiler = SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A shader that performs mathematical and other operations in parallel using the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A compute shader can be used to perform complex mathematical functions, or other algorithms in parallel using the GPU. This allows the application to leverage the speed of the GPU to perform number 
    /// crunching that the CPU may not be able to handle in a timely manner.
    /// </para>
    /// <para>
    /// In Gorgon, shaders can be compiled from a string containing source code via the <see cref="GorgonShaderFactory"/>, or loaded from a <see cref="Stream"/> or file for quicker access. The 
    /// <see cref="GorgonShaderFactory"/> is required to compile or read shaders, they cannot be created via the <c>new</c> keyword.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// A compute shader requires a video adapter with a <see cref="IGorgonVideoAdapter.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
    /// </para>
    /// </note>
    /// </para>
	/// </remarks>
    public sealed class GorgonComputeShader
		: GorgonShader
	{
		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D compute shader.
		/// </summary>
		internal D3D.ComputeShader NativeShader
		{
			get;
		}

		/// <summary>
		/// Property to return the type of shader.
		/// </summary>
		public override ShaderType ShaderType => ShaderType.Compute;
		#endregion

		#region Methods.
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			NativeShader?.Dispose();
			base.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
	    /// <summary>
	    /// Initializes a new instance of the <see cref="GorgonVertexShader" /> class.
	    /// </summary>
	    /// <param name="videoDevice">The video adapter used to create the shader.</param>
	    /// <param name="name">The name for this shader.</param>
	    /// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
	    /// <param name="byteCode">The byte code for the shader..</param>
	    internal GorgonComputeShader(IGorgonVideoAdapter videoDevice, string name, bool isDebug, D3DCompiler.ShaderBytecode byteCode)
	        : base(videoDevice, name, isDebug, byteCode)
	    {
	        NativeShader = new D3D.ComputeShader(videoDevice.D3DDevice(), byteCode)
	                       {
	                           DebugName = name + " D3D11ComputeShader"
	                       };
	    }
	    #endregion
	}
}
