#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 27, 2017 12:42:58 PM
// 
#endregion

using System.IO;
using D3D11 = SharpDX.Direct3D11;
using D3DCompiler = SharpDX.D3DCompiler;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A shader that can be used in the tesselation of geometry, or the creation of patch geometry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A hull shader is a program that will take control points for a surface and convert to control points for a patch. This data can then be passed to a tessellation stage and domain stage 
    /// to be used in the tessellation of geometry.
    /// </para>
    /// <para>
    /// In Gorgon, shaders can be compiled from a string containing source code via the <see cref="GorgonShaderFactory"/>, or loaded from a <see cref="Stream"/> or file for quicker access. The 
    /// <see cref="GorgonShaderFactory"/> is required to compile or read shaders, they cannot be created via the <c>new</c> keyword.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// A hull shader requires a video device with a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public class GorgonHullShader
        : GorgonShader
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the Direct3D hull shader.
        /// </summary>
        internal D3D11.HullShader NativeShader
        {
            get;
        }

        /// <summary>
        /// Property to return the type of shader.
        /// </summary>
        public override ShaderType ShaderType => ShaderType.Hull;
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
        /// Initializes a new instance of the <see cref="GorgonHullShader" /> class.
        /// </summary>
        /// <param name="videoDevice">The video device used to create the shader.</param>
        /// <param name="name">The name for this shader.</param>
        /// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
        /// <param name="byteCode">The byte code for the shader.</param>
        internal GorgonHullShader(IGorgonVideoDevice videoDevice, string name, bool isDebug, D3DCompiler.ShaderBytecode byteCode)
            : base(videoDevice, name, isDebug, byteCode)
        {
            NativeShader = new D3D11.HullShader(videoDevice.D3DDevice(), byteCode)
                           {
                               DebugName = name + " D3D11HullShader"
                           };
        }
        #endregion
    }
}
