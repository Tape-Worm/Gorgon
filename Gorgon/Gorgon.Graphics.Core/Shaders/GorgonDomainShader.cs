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

using Gorgon.Diagnostics;
using SharpDX.D3DCompiler;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A shader that can be used in the tesselation of geometry.
/// </summary>
/// <remarks>
/// <para>
/// A domain shader is a program that calculates a vertex position of a subdivided point in an output patch. This shader is run once per tessellator stage output point and has read-only access to the 
/// tessellator stage output UV coordinates, the hull shader output patch, and the hull shader output patch constants.
/// </para>
/// <para>
/// In Gorgon, shaders can be compiled from a string containing source code via the <see cref="GorgonShaderFactory"/>, or loaded from a <see cref="Stream"/> or file for quicker access. The 
/// <see cref="GorgonShaderFactory"/> is required to compile or read shaders, they cannot be created via the <c>new</c> keyword.
/// </para>
/// </remarks>
public sealed class GorgonDomainShader
    : GorgonShader
{
    #region Variables.
    // The D3D 11 Domain Shader.
    private D3D11.DomainShader _shader;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the Direct3D domain shader.
    /// </summary>
    internal D3D11.DomainShader NativeShader => _shader;

    /// <summary>
    /// Property to return the type of shader.
    /// </summary>
    public override ShaderType ShaderType => ShaderType.Domain;
    #endregion

    #region Methods.
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        D3D11.DomainShader shader = Interlocked.Exchange(ref _shader, null);

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
    /// Initializes a new instance of the <see cref="GorgonDomainShader" /> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns this object.</param>
    /// <param name="name">The name for this shader.</param>
    /// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
    /// <param name="byteCode">The byte code for the shader.</param>
    internal GorgonDomainShader(GorgonGraphics graphics, string name, bool isDebug, ShaderBytecode byteCode)
        : base(graphics, name, isDebug, byteCode) => _shader = new D3D11.DomainShader(graphics.D3DDevice, byteCode)
        {
            DebugName = name + "_ID3D11DomainShader"
        };
    #endregion
}
