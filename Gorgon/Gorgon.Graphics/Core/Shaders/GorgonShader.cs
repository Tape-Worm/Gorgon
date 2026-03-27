// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: September 29, 2025 12:47:46 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// The types of shaders avaiable.
/// </summary>
public enum ShaderType
{
    /// <summary>
    /// No shader type.
    /// </summary>
    None = 0,
    /// <summary>
    /// A vertex shader.
    /// </summary>
    VertexShader = 1,
    /// <summary>
    /// A pixel shader.
    /// </summary>
    PixelShader = 2,
    /// <summary>
    /// A geometry shader.
    /// </summary>
    GeometryShader = 3,
    /// <summary>
    /// A hull shader.
    /// </summary>
    HullShader = 4,
    /// <summary>
    /// A domain shader.
    /// </summary>
    DomainShader = 5,
    /// <summary>
    /// A compute shader.
    /// </summary>
    ComputeShader = 6,
    /// <summary>
    /// A mesh shader (shader model 6.5 or better).
    /// </summary>
    MeshShader = 7,
    /// <summary>
    /// An amplification shader (shader model 6.5 or better).
    /// </summary>
    AmplificationShader = 8
}

/// <summary>
/// The base type for all shaders.
/// </summary>
public sealed class GorgonShader
{
    private readonly byte[] _data;
    private readonly byte[] _pdbData;
    private readonly byte[] _reflectionData;
    private readonly byte[] _hash;

    /// <summary>
    /// Property to return the type of shader.
    /// </summary>
    public ShaderType ShaderType
    {
        get;
    }

    /// <summary>
    /// Property to return the shader model.
    /// </summary>
    public ShaderModel ShaderModel
    {
        get;
    }

    /// <summary>
    /// Property to return the graphics interface that owns this shader.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the suggested name of the PDB data.
    /// </summary>
    /// <remarks>
    /// This is made up of the shader hash name and the '.pdb' extension.
    /// </remarks>
    public string PdbName
    {
        get;
    } 

    /// <summary>
    /// Property to retrieve the shader binary data.
    /// </summary>
    public ReadOnlySpan<byte> ShaderData => _data;

    /// <summary>
    /// Property to return the program database for shader debugging.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will only be populated if the shader is compiled using debug mode.
    /// </para>
    /// </remarks>
    public ReadOnlySpan<byte> PdbData => _pdbData;

    /// <summary>
    /// Property to return any reflection information within the shader.
    /// </summary>
    public ReadOnlySpan<byte> ReflectionData => _reflectionData;

    /// <summary>
    /// Property to return the shader hash.
    /// </summary>
    public ReadOnlySpan<byte> Hash => _hash;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShader"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns this object.</param>
    /// <param name="shaderData">The compiled binary data for the shader.</param>
    /// <param name="hash">The shader hash.</param>
    /// <param name="pdbData">Symbol database for debugging shaders.</param>
    /// <param name="pdbName">The name of the PDB data.</param>
    /// <param name="reflectionData">The reflection information for the shader.</param>
    /// <param name="shaderType">The type of shader.</param>
    /// <param name="shaderModel">The shader model.</param>
    internal GorgonShader(GorgonGraphics graphics, byte[] shaderData, byte[] hash, byte[] pdbData, string pdbName, byte[] reflectionData, ShaderType shaderType, ShaderModel shaderModel)
    {
        Graphics = graphics;
        _data = shaderData;
        _hash = hash;
        _pdbData = pdbData;
        PdbName = pdbName;
        _reflectionData = reflectionData;
        ShaderType = shaderType;
        ShaderModel = shaderModel;
    }
}
