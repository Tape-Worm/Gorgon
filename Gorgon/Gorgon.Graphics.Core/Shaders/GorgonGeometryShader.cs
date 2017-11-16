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
// Created: Thursday, December 15, 2011 12:49:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using D3DCompiler = SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A shader that can be used to produce new geometry.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A geometry shader is a program that is used to build new geometry on the GPU. This allows for effects like point sprites.
	/// </para>
	/// <para>
	/// In Gorgon, shaders can be compiled from a string containing source code via the <see cref="GorgonShaderFactory"/>, or loaded from a <see cref="Stream"/> or file for quicker access. The 
	/// <see cref="GorgonShaderFactory"/> is required to compile or read shaders, they cannot be created via the <c>new</c> keyword.
	/// </para>
	/// </remarks>
	public sealed class GorgonGeometryShader
		: GorgonShader
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the Direct3D geometry shader.
		/// </summary>
		internal D3D.GeometryShader NativeShader
		{
			get;
		    private set;
		}

        /// <summary>
        /// Property to return the <see cref="GorgonStreamOutLayout"/> for this geometry shader.
        /// </summary>
        /// <remarks>
        /// If this geometry shader is capable of performing stream output, then this value will be non <b>null</b>. Otherwise, it will not be capable of supporting stream output.
        /// </remarks>
	    public GorgonStreamOutLayout StreamOutLayout
	    {
	        get;
            private set;
        }

		/// <summary>
		/// Property to return the type of shader.
		/// </summary>
		public override ShaderType ShaderType => ShaderType.Geometry;
		#endregion

		#region Methods.
        /// <summary>
        /// Function to convert this geometry shader to use a stream output.
        /// </summary>
        /// <param name="streamOutLayout">The stream output layout for the shader.</param>
        /// <param name="strides">[Optional] A list of strides that define the size of an element for each buffer.</param>
        /// <returns>A new <see cref="GorgonGeometryShader"/> that is capable of stream output.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="streamOutLayout"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// A base geometry shader must be converted to use stream output if an application wants to send data from the shader into a buffer.
        /// </para>
        /// <para>
        /// If the <paramref name="strides"/> parameter is supplied, then it will be limited to 4 items at most, any more than that and the list will be truncated.
        /// </para>
        /// </remarks>
	    public GorgonGeometryShader ToStreamOut(GorgonStreamOutLayout streamOutLayout, IEnumerable<int> strides = null)
	    {
	        if (streamOutLayout == null)
	        {
	            throw new ArgumentNullException(nameof(streamOutLayout));
	        }

	        int[] strideList = strides?.Take(4).ToArray() ?? new int[0];
            // Clone the byte code just in case we decide to destroy the original.
	        D3DCompiler.ShaderBytecode byteCode = new D3DCompiler.ShaderBytecode(D3DByteCode.Data);

	        D3D.GeometryShader shader = new D3D.GeometryShader(VideoDevice.D3DDevice(), byteCode, streamOutLayout.Native, strideList, 0)
	                     {
	                         DebugName = $"{Name} (SO) D3D11GeometryShader"
	                     };
	        return new GorgonGeometryShader(VideoDevice, Name + " (SO)", IsDebug, byteCode)
	               {
	                   NativeShader = shader,
	                   StreamOutLayout = streamOutLayout
	               };
	    }

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
		/// Initializes a new instance of the <see cref="GorgonGeometryShader" /> class.
		/// </summary>
		/// <param name="videoDevice">The video adapter used to create the shader.</param>
		/// <param name="name">The name for this shader.</param>
		/// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
		/// <param name="byteCode">The byte code for the shader.</param>
		internal GorgonGeometryShader(IGorgonVideoAdapter videoDevice, string name, bool isDebug, D3DCompiler.ShaderBytecode byteCode)
			: base(videoDevice, name, isDebug, byteCode)
		{
			NativeShader = new D3D.GeometryShader(videoDevice.D3DDevice(), byteCode)
			            {
				            DebugName = name + " D3D11GeometryShader"
			            };
		}
		#endregion
	}
}
