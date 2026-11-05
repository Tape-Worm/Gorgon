// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 26, 2026 2:43:54 PM
//

using System;
using Gorgon.Core;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pipeline state object for the compute pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The compute pipeline state object (PSO) defines the state of the compute pipeline used by a <see cref="GorgonDispatchCall"/> or a <see cref="GorgonComputeExecuteCall"/>. Unlike the 
/// <see cref="GorgonGraphicsPso"/>, the only state it holds is the compute shader.
/// </para>
/// <para>
/// Pipeline state objects perform final compilation of shaders, so they can take a small amount of time to compile. The best practice is to create the pipeline state objects up front instead of during 
/// rendering to minimize the cost.
/// </para>
/// <para>
/// Pipeline state objects are immutable and can only be created by calling <see cref="GorgonComputePsoFactory.CreateOrGetPso(string, GorgonShader)"/> on the <see cref="GorgonComputePsoFactory"/>.
/// </para>
/// <para>
/// The <see cref="GorgonComputePsoFactory"/> controls the lifetime of the pipeline state object. It will remain usable until the factory cache is flushed, which destroys the underlying state for every 
/// object in the cache. If the application is still holding on to a pipeline state object after that happens, it can no longer be used, and its <see cref="IsCompiled"/> property will return 
/// <b>false</b>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonComputePsoFactory"/>
public sealed class GorgonComputePso
    : IGorgonNamedObject, IEquatable<GorgonComputePso>
{
    private ComPtr<ID3D12PipelineState> _d3dPso;

    private byte[] _psoBlob = [];

    /// <summary>
    /// Property to return the underlying Direct3D PSO object.
    /// </summary>
    internal ref ComPtr<ID3D12PipelineState> D3DPso => ref _d3dPso;

    /// <inheritdoc cref="GorgonGraphicsPso.Graphics"/>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsPso.PsoCachedBlob"/>
    public ReadOnlySpan<byte> PsoCachedBlob
    {
        get => _psoBlob;
        internal set => _psoBlob = (value.Length == 0 ? [] : value.ToArray());
    }

    /// <inheritdoc cref="GorgonGraphicsPso.IsCompiled"/>
    public bool IsCompiled => !_d3dPso.IsNull;

    /// <summary>
    /// Property to return the name for this pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is used by the <see cref="GorgonComputePsoFactory"/> as a unique identifier for caching purposes. This allows users to reuse PSOs without having to recreate them.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputePsoFactory"/>
    public string Name
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the compute shader for the pipeline state object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The compute shader is executed for each thread group dispatched by a <see cref="GorgonDispatchCall"/> or a <see cref="GorgonComputeExecuteCall"/>.
    /// </para>
    /// </remarks>
    public GorgonShader ComputeShader
    {
        get;
    }

    /// <summary>
    /// Function to set the COM pointer for the pipeline state object.
    /// </summary>
    /// <param name="ptr">The COM pointer to the pipeline state object.</param>
    internal void SetComPtr(ComPtr<ID3D12PipelineState> ptr) => _d3dPso = ptr;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonComputePso pso ? Equals(pso) : base.Equals(obj);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();

        hash.Add(Graphics);
        hash.Add(Name, StringComparer.OrdinalIgnoreCase);
        hash.Add(ComputeShader);

        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(GorgonComputePso? other) => (this == other) || (other is not null
        && other.Graphics == Graphics
        && string.Equals(other.Name, Name, StringComparison.OrdinalIgnoreCase)
        && other.ComputeShader == ComputeShader);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonComputePso"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with the PSO.</param>
    /// <param name="name">The unique name for the PSO.</param>
    /// <param name="computeShader">The compute shader for the PSO.</param>
    internal GorgonComputePso(GorgonGraphics graphics, string name, GorgonShader computeShader)
    {
        Graphics = graphics;
        Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonComputePso));
        ComputeShader = computeShader;
    }
}
