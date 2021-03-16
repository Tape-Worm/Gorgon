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
using System.Threading;
using Gorgon.Diagnostics;
using SharpDX.D3DCompiler;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A shader that operates on a single vertex at a time on the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A vertex shader is a program that is used to modify the attributes of a single vertex on the GPU. This allows for effects like transforming vertices, skinning, etc....
    /// </para>
    /// <para>
    /// In Gorgon, shaders can be compiled from a string containing source code via the <see cref="GorgonShaderFactory"/>, or loaded from a <see cref="Stream"/> or file for quicker access. The 
    /// <see cref="GorgonShaderFactory"/> is required to compile or read shaders, they cannot be created via the <c>new</c> keyword.
    /// </para>
    /// </remarks>
    public sealed class GorgonVertexShader
        : GorgonShader
    {
        #region Variables.
        // The D3D 11 vertex shader.
        private D3D11.VertexShader _nativeShader;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D vertex shader.
        /// </summary>
        internal D3D11.VertexShader NativeShader => _nativeShader;

        /// <summary>
        /// Property to return the type of shader.
        /// </summary>
        public override ShaderType ShaderType => ShaderType.Vertex;
        #endregion

        #region Methods.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            D3D11.VertexShader shader = Interlocked.Exchange(ref _nativeShader, null);

            if (shader is not null)
            {
                Graphics.Log.Print($"Destroying {ShaderType} '{Name}' ({ID})", LoggingLevel.Verbose);

                shader.Dispose();
            }

            base.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonVertexShader" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this object.</param>
        /// <param name="name">The name for this shader.</param>
        /// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
        /// <param name="byteCode">The byte code for the shader..</param>
        internal GorgonVertexShader(GorgonGraphics graphics, string name, bool isDebug, ShaderBytecode byteCode)
            : base(graphics, name, isDebug, byteCode)
        {
            graphics.Log.Print($"Creating {ShaderType} '{name}' ({ID})", LoggingLevel.Verbose);
            _nativeShader = new D3D11.VertexShader(graphics.D3DDevice, byteCode)
            {
                DebugName = name + " D3D11VertexShader"
            };
        }
        #endregion
    }
}
