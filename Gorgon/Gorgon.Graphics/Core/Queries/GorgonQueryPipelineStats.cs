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
// Created: October 13, 2026 1:26:10 PM
//

using System;
using System.Runtime.InteropServices;
using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The result of a single <see cref="QueryType.PipelineStatistics"/> query slot.
/// </summary>
/// <remarks>
/// <para>
/// Each value counts the work done by one stage of the pipeline between the start and end of the query.
/// </para>
/// </remarks>
/// <seealso cref="QueryType.PipelineStatistics"/>
[StructLayout(LayoutKind.Sequential)]
public readonly struct GorgonQueryPipelineStats
    : IEquatable<GorgonQueryPipelineStats>, IGorgonEquatableByRef<GorgonQueryPipelineStats>
{
    /// <summary>
    /// The number of vertices read by the input assembler.
    /// </summary>
    public readonly ulong InputAssemblerVertices;

    /// <summary>
    /// The number of primitives read by the input assembler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This number depends on the primitive topology used to draw.
    /// </para>
    /// </remarks>
    public readonly ulong InputAssemblerPrimitives;

    /// <summary>
    /// The number of times a vertex shader was invoked.
    /// </summary>
    public readonly ulong VertexShaderInvocations;

    /// <summary>
    /// The number of times a geometry shader was invoked.
    /// </summary>
    public readonly ulong GeometryShaderInvocations;

    /// <summary>
    /// The number of primitives output by a geometry shader.
    /// </summary>
    public readonly ulong GeometryShaderPrimitives;

    /// <summary>
    /// The number of primitives sent to the rasterizer.
    /// </summary>
    public readonly ulong RasterizerInvocations;

    /// <summary>
    /// The number of primitives that were rendered.
    /// </summary>
    public readonly ulong RasterizerPrimitives;

    /// <summary>
    /// The number of times a pixel shader was invoked.
    /// </summary>
    public readonly ulong PixelShaderInvocations;

    /// <summary>
    /// The number of times a hull shader was invoked.
    /// </summary>
    public readonly ulong HullShaderInvocations;

    /// <summary>
    /// The number of times a domain shader was invoked.
    /// </summary>
    public readonly ulong DomainShaderInvocations;

    /// <summary>
    /// The number of times a compute shader was invoked.
    /// </summary>
    public readonly ulong ComputeShaderInvocations;

    /// <inheritdoc/>
    public bool Equals(ref readonly GorgonQueryPipelineStats other) => (InputAssemblerVertices == other.InputAssemblerVertices)
                                                                       && (InputAssemblerPrimitives == other.InputAssemblerPrimitives)
                                                                       && (VertexShaderInvocations == other.VertexShaderInvocations)
                                                                       && (GeometryShaderInvocations == other.GeometryShaderInvocations)
                                                                       && (GeometryShaderPrimitives == other.GeometryShaderPrimitives)
                                                                       && (RasterizerInvocations == other.RasterizerInvocations)
                                                                       && (RasterizerPrimitives == other.RasterizerPrimitives)
                                                                       && (PixelShaderInvocations == other.PixelShaderInvocations)
                                                                       && (HullShaderInvocations == other.HullShaderInvocations)
                                                                       && (DomainShaderInvocations == other.DomainShaderInvocations)
                                                                       && (ComputeShaderInvocations == other.ComputeShaderInvocations);

    /// <inheritdoc/>
    public bool Equals(GorgonQueryPipelineStats other) => Equals(in other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonQueryPipelineStats stats && Equals(in stats);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(InputAssemblerVertices);
        hash.Add(InputAssemblerPrimitives);
        hash.Add(VertexShaderInvocations);
        hash.Add(GeometryShaderInvocations);
        hash.Add(GeometryShaderPrimitives);
        hash.Add(RasterizerInvocations);
        hash.Add(RasterizerPrimitives);
        hash.Add(PixelShaderInvocations);
        hash.Add(HullShaderInvocations);
        hash.Add(DomainShaderInvocations);
        hash.Add(ComputeShaderInvocations);

        return hash.ToHashCode();
    }

    /// <summary>
    /// Operator to determine if two pipeline statistics values are equal.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(in GorgonQueryPipelineStats left, in GorgonQueryPipelineStats right) => left.Equals(in right);

    /// <summary>
    /// Operator to determine if two pipeline statistics values are not equal.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(in GorgonQueryPipelineStats left, in GorgonQueryPipelineStats right) => !left.Equals(in right);
}
