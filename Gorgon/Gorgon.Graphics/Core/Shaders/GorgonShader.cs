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
using System.Runtime.CompilerServices;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The types of shaders available.
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
    /// A mesh shader.
    /// </summary>
    MeshShader = 7,
    /// <summary>
    /// An amplification shader.
    /// </summary>
    AmplificationShader = 8
}

/// <summary>
/// A shader used to execute instructions on the GPU.
/// </summary>
/// <remarks>
/// <para>
/// A Gorgon shader object is just a series of binary data blobs which is passed to the GPU via the <see cref="GorgonGraphicsPso"/>, <see cref="GorgonComputePso"/>, or other pipeline state objects available 
/// in Gorgon. Combinations of these shader objects across multiple pipeline state objects allow for various ways to interpret and act on data on the GPU. Without a shader, the GPU cannot even do any work 
/// and as such, shaders are a basic building block of any rendering/computation on the GPU.
/// </para>
/// <para>
/// To create a shader, it must be compiled from text using a <see cref="GorgonShaderCompiler"/>. The resulting shader is then ready to be used in a pipeline state object, or persisted to a file for later use. 
/// Applications can also load previously saved shaders by using the <see cref="Codecs.GorgonCodecShader"/> object or even a <see cref="Codecs.GorgonShaderCodecCommon">custom codec</see> that matches the 
/// application more appropriately.
/// </para>
/// <h3>Why do we need a pipeline?</h3>
/// <para>
/// The data from a compiled shader is stored in an intermediate, common format that is agnostic to the GPU. This is why it needs to be bound to a pipeline so that the GPU can compile it to its own internal, 
/// proprietary format.
/// </para>
/// <h3>Equality checking</h3>
/// <para>
/// The <see cref="GorgonShader"/> type also implements <see cref="IEquatable{T}"/>. When one shader is compared to another, their hashcodes are checked, and if they are the same, the binary data for the 
/// shader is compared to determine if the shader is the same. This is a fairly fast process, but it is not recommended to do this in tight loops. 
/// </para>
/// <para>
/// This has been implemented in this way because different shader binaries can produce the same hash code, so the hash code alone cannot be used to determine equality.
/// </para>
/// </remarks>
/// <seealso cref="GorgonShaderCompiler"/>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonComputePso"/>
public sealed class GorgonShader
    : IEquatable<GorgonShader>
{
    private readonly byte[] _data = [];
    private readonly byte[] _pdbData = [];
    private readonly byte[] _reflectionData = [];
    private readonly byte[] _hash = [];
    private readonly int _hashCode = 0;

    /// <summary>
    /// Property to return the graphics interface that is associated with this shader.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

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
    /// <remarks>
    /// <para>
    /// Gorgon supports a minimum of Shader Model 6.6 up to 6.8.
    /// </para>
    /// </remarks>
    public ShaderModel ShaderModel
    {
        get;
    }

    /// <summary>
    /// Property to return the suggested name of the PDB data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is made up of the shader <see cref="Hash"/> name and the '.pdb' extension.
    /// </para>
    /// </remarks>
    public string PdbName
    {
        get;
    } = string.Empty;

    /// <summary>
    /// Property to return the shader binary data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the intermediate compiled data for the shader. If this value is empty, then this shader is not valid.
    /// </para>
    /// </remarks>
    public ReadOnlySpan<byte> ShaderData => _data;

    /// <summary>
    /// Property to return the program database for shader debugging.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will only be populated if the shader is compiled with the <see cref="CompileFlags.Debug"/> flag.
    /// </para>
    /// </remarks>
    public ReadOnlySpan<byte> PdbData => _pdbData;

    /// <summary>
    /// Property to return any reflection information within the shader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will only be populated if the shader is compiled without the <see cref="CompileFlags.NoReflection"/> flag.
    /// </para>
    /// </remarks>
    public ReadOnlySpan<byte> ReflectionData => _reflectionData;

    /// <summary>
    /// Property to return the shader hash.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will only be populated if the shader contains any hash data.
    /// </para>
    /// </remarks>
    public ReadOnlySpan<byte> Hash => _hash;

    /// <summary>
    /// Function to generate the hash code for this shader from its binary blob data.
    /// </summary>
    /// <param name="data">The data for the shader.</param>
    /// <returns>The hash code for the shader data blob.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Collection initializer will perform a copy. We need a wrapper around the pointer instead. VS fucked up here and should not have suggested it.")]
    internal static int CreateNewShaderHashCode(ReadOnlySpan<byte> data)
    {
        HashCode hashCode = new();
        hashCode.AddBytes(data);
        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Function to compare the binary data for this shader with a binary blob and its hash code.
    /// </summary>
    /// <param name="otherHashCode">The hash code for the other binary blob.</param>
    /// <param name="otherBlob">The other binary blob to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CheckShaderBlob(int otherHashCode, ReadOnlySpan<byte> otherBlob) => (otherBlob.Length == _data.Length) && (otherHashCode == _hashCode) && (_data.SequenceEqual(otherBlob));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is GorgonShader shader ? Equals(shader) : base.Equals(obj);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _hashCode;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GorgonShader? other) => (ReferenceEquals(other, this)) || ((other is not null) && (CheckShaderBlob(other._hashCode, other._data)));

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShader"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with this shader.</param>
    /// <param name="shaderData">The compiled binary data for the shader.</param>
    /// <param name="hash">The shader hash.</param>
    /// <param name="pdbData">Symbol database for debugging shaders.</param>
    /// <param name="pdbName">The name of the PDB data.</param>
    /// <param name="reflectionData">The reflection information for the shader.</param>
    /// <param name="shaderType">The type of shader.</param>
    /// <param name="shaderModel">The shader model.</param>
    internal GorgonShader(GorgonGraphics graphics, 
        byte[] shaderData, 
        byte[]? hash,
        byte[]? pdbData, 
        string? pdbName,
        byte[]? reflectionData, 
        ShaderType shaderType, 
        ShaderModel shaderModel)
    {
        Graphics = graphics;
        _hashCode = CreateNewShaderHashCode(shaderData);
        _data = shaderData;
        _hash = hash ?? [];
        _pdbData = pdbData ?? [];
        PdbName = pdbName ?? string.Empty;
        _reflectionData = reflectionData ?? [];
        ShaderType = shaderType;    
        ShaderModel = shaderModel;        
    }
}
