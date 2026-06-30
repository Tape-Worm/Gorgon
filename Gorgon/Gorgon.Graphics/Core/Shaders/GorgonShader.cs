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

using System.Diagnostics.CodeAnalysis;
using Gorgon.Native;

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
/// A shader used to execute instructions on the GPU.
/// </summary>
/// <remarks>
/// <para>
/// Applications use shaders to do everything from generating primitives, modifying vertices, to coloring a pixel on the screen using the GPU. These programs are what give GPUs their power when rendering 
/// interesting and creative scenes.
/// </para>
/// </remarks>
public sealed class GorgonShader
    : IDisposable
{
    /// <summary>
    /// A dummy shader used to indicate that no shader has been applied.
    /// </summary>
    internal static readonly GorgonShader NullShader = new();

    private readonly GorgonNativeBuffer<byte> _data = [];
    private readonly GorgonNativeBuffer<byte> _pdbData = [];
    private readonly GorgonNativeBuffer<byte> _reflectionData = [];
    private readonly GorgonNativeBuffer<byte> _hash = [];    

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
    [MaybeNull()]
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
    } = string.Empty;

    /// <summary>
    /// Property to retrieve the shader binary data.
    /// </summary>
    public GorgonPtr<byte> ShaderData => _data;

    /// <summary>
    /// Property to return the program database for shader debugging.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will only be populated if the shader is compiled using debug mode.
    /// </para>
    /// </remarks>
    public GorgonPtr<byte> PdbData => _pdbData;

    /// <summary>
    /// Property to return any reflection information within the shader.
    /// </summary>
    public GorgonPtr<byte> ReflectionData => _reflectionData;

    /// <summary>
    /// Property to return the shader hash.
    /// </summary>
    public GorgonPtr<byte> Hash => _hash;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);

            _data.Dispose();
            _pdbData.Dispose();
            _reflectionData.Dispose();
            _hash.Dispose();
        }        
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
    internal GorgonShader(GorgonGraphics graphics, GorgonNativeBuffer<byte> shaderData, GorgonNativeBuffer<byte> hash, GorgonNativeBuffer<byte> pdbData, string pdbName, GorgonNativeBuffer<byte> reflectionData, ShaderType shaderType, ShaderModel shaderModel)
    {
        Graphics = graphics;
        _data = shaderData;
        _hash = hash;
        _pdbData = pdbData;
        PdbName = pdbName;
        _reflectionData = reflectionData;
        ShaderType = shaderType;
        ShaderModel = shaderModel;

        this.RegisterDisposable(graphics);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShader"/> class.
    /// </summary>
    private GorgonShader()
    {
    }
}
