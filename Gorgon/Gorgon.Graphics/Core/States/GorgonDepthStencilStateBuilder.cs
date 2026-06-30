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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 17, 2026 4:31:42 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to create a <see cref="GorgonDepthStencilState"/> object.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonDepthStencilState"/> to pass to a <see cref="GorgonGraphicsPsoBuilder"/>. This will define how rasterized primitive data is clipped against a 
/// depth/stencil buffer. Depth reading, writing, and stencil operations are affected by this state.
/// </para>
/// <para>
/// A <see cref="GorgonDepthStencilState"/> is an immutable object, it can only be created through this builder factory type.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonGraphicsPsoBuilder"/>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonDepthStencilState"/>
public class GorgonDepthStencilStateBuilder
    : IGorgonFluentBuilder<GorgonDepthStencilStateBuilder, GorgonDepthStencilState, IGorgonAllocator<GorgonDepthStencilState>>
{
    /// <summary>
    /// A default allocator for depth/stencil objects.
    /// </summary>
    private class DefaultAllocator
        : IGorgonAllocator<GorgonDepthStencilState>
    {
        /// <inheritdoc/>
        public GorgonDepthStencilState Allocate(Action<GorgonDepthStencilState>? initializer = null)
        {
            GorgonDepthStencilState result = new();
            initializer?.Invoke(result);
            return result;
        }
    }

    private readonly DefaultAllocator _allocator = new();
    private readonly GorgonDepthStencilState _worker = new();

    /// <summary>
    /// Function to copy a depth/stencil state to another.
    /// </summary>
    /// <param name="source">The depth/stencil state to copy from.</param>
    /// <param name="destination">The depth/stencil state to copy into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy(GorgonDepthStencilState source, GorgonDepthStencilState destination)
    {
        destination.BackFaceStencilOperation.DepthFailOperation = source.BackFaceStencilOperation.DepthFailOperation;
        destination.BackFaceStencilOperation.FailOperation = source.BackFaceStencilOperation.FailOperation;
        destination.BackFaceStencilOperation.PassOperation = source.BackFaceStencilOperation.PassOperation;
        destination.BackFaceStencilOperation.ReadMask = source.BackFaceStencilOperation.ReadMask;
        destination.BackFaceStencilOperation.StencilFunction = source.BackFaceStencilOperation.StencilFunction;
        destination.BackFaceStencilOperation.WriteMask = source.BackFaceStencilOperation.WriteMask;
        destination.FrontFaceStencilOperation.DepthFailOperation = source.FrontFaceStencilOperation.DepthFailOperation;
        destination.FrontFaceStencilOperation.FailOperation = source.FrontFaceStencilOperation.FailOperation;
        destination.FrontFaceStencilOperation.PassOperation = source.FrontFaceStencilOperation.PassOperation;
        destination.FrontFaceStencilOperation.ReadMask = source.FrontFaceStencilOperation.ReadMask;
        destination.FrontFaceStencilOperation.StencilFunction = source.FrontFaceStencilOperation.StencilFunction;
        destination.FrontFaceStencilOperation.WriteMask = source.FrontFaceStencilOperation.WriteMask;

        destination.DepthFunction = source.DepthFunction;
        destination.IsDepthBoundsTestingEnabled = source.IsDepthBoundsTestingEnabled;
        destination.IsDepthEnabled = source.IsDepthEnabled;
        destination.IsDepthWriteEnabled = source.IsDepthWriteEnabled;
        destination.IsStencilEnabled = source.IsStencilEnabled;
    }

    /// <summary>
    /// Function to set the comparison type for a stencil operation.
    /// </summary>
    /// <param name="face">The face direction for the operation.</param>
    /// <param name="comparison">The comparison type.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <inheritdoc cref="GorgonStencilOperation.StencilFunction" path="/remarks"/>
    public GorgonDepthStencilStateBuilder StencilComparison(StencilFace face, ComparisonFunction comparison)
    {
        switch (face)
        {
            case StencilFace.Back:
                _worker.BackFaceStencilOperation.StencilFunction = comparison;
                break;
            case StencilFace.Front:
                _worker.FrontFaceStencilOperation.StencilFunction = comparison;
                break;
        }

        return this;
    }

    /// <summary>
    /// Function to set the operation(s) for the stencil <see cref="StencilComparison"/> result.
    /// </summary>
    /// <param name="face"><inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/param[@name='face']"/></param>
    /// <param name="passStencilOp">[Optional] The stencil operation if the comparison passes.</param>
    /// <param name="failStencilOp">[Optional] The stencil operation if the comparison fails.</param>
    /// <param name="depthFailOp">[Optional] The stencil operation if the depth comparison fails.</param>
    /// <inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// The default values are <see cref="StencilOperation.Keep"/> for all 3 operations.
    /// </para>
    /// </remarks>
    public GorgonDepthStencilStateBuilder StencilOperation(StencilFace face, StencilOperation passStencilOp = Core.StencilOperation.Keep, StencilOperation failStencilOp = Core.StencilOperation.Keep, StencilOperation depthFailOp = Core.StencilOperation.Keep)
    {
        switch (face)
        {
            case StencilFace.Back:
                _worker.BackFaceStencilOperation.PassOperation = passStencilOp;
                _worker.BackFaceStencilOperation.FailOperation = failStencilOp;
                _worker.BackFaceStencilOperation.DepthFailOperation = depthFailOp;
                break;
            case StencilFace.Front:
                _worker.FrontFaceStencilOperation.PassOperation = passStencilOp;
                _worker.FrontFaceStencilOperation.FailOperation = failStencilOp;
                _worker.FrontFaceStencilOperation.DepthFailOperation = depthFailOp;
                break;
        }

        return this;
    }

    /// <summary>
    /// Function to set the read and write mask for the given face.
    /// </summary>
    /// <param name="face"><inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/param[@name='face']"/></param>
    /// <param name="read">The read mask for the specified face.</param>
    /// <param name="write">The write mask for the specified face.</param>
    /// <inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This sets a stencil read and write mask for the given <paramref name="face"/>. If the video adapter supports it, the stencil read/write mask value can be different for each face. Check the 
    /// <see cref="GorgonVideoAdapterInfo.SupportsIndependentFrontAndBackStencilRef"/> value to determine if the adapter can support this functionality. If it cannot, the pipeline state object will fail to 
    /// build.
    /// </para>
    /// <h2>For Read</h2>
    /// <inheritdoc cref="GorgonStencilOperation.ReadMask" path="/remarks/para"/>
    /// <h2>For Write</h2>
    /// <inheritdoc cref="GorgonStencilOperation.WriteMask" path="/remarks/para"/>
    /// </remarks>
    public GorgonDepthStencilStateBuilder StencilMask(StencilFace face, byte read, byte write)
    {
        switch (face)
        {
            case StencilFace.Back:
                _worker.BackFaceStencilOperation.ReadMask = read;
                _worker.BackFaceStencilOperation.WriteMask = write;
                break;
            case StencilFace.Front:
                _worker.FrontFaceStencilOperation.ReadMask = read;
                _worker.FrontFaceStencilOperation.WriteMask = write;
                break;
        }

        return this;
    }

    /// <summary>
    /// Function to set the depth test comparison function.
    /// </summary>
    /// <param name="depthCompare">The comparison function to use when testing depth values.</param>
    /// <inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/returns"/>
    /// <inheritdoc cref="GorgonDepthStencilState.DepthFunction" path="/remarks"/>
    public GorgonDepthStencilStateBuilder DepthFunction(ComparisonFunction depthCompare)
    {
        _worker.DepthFunction = depthCompare;
        return this;
    }

    /// <summary>
    /// Function to enable depth testing.
    /// </summary>
    /// <param name="enabled"><b>true</b> to enable, <b>false</b> to disable.</param>
    /// <inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/returns"/>
    /// <inheritdoc cref="GorgonDepthStencilState.IsDepthEnabled" path="/remarks"/>
    public GorgonDepthStencilStateBuilder DepthEnable(bool enabled)
    {
        _worker.IsDepthEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Function to enable depth writing.
    /// </summary>
    /// <inheritdoc cref="DepthEnable(bool)" path="/param"/>
    /// <inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/returns"/>
    /// <inheritdoc cref="GorgonDepthStencilState.IsDepthWriteEnabled" path="/remarks"/>
    public GorgonDepthStencilStateBuilder DepthWriteEnable(bool enabled)
    {
        _worker.IsDepthWriteEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Function to enable or disable stencil testing.
    /// </summary>
    /// <inheritdoc cref="DepthEnable(bool)" path="/param"/>
    /// <inheritdoc cref="StencilComparison(StencilFace, ComparisonFunction)" path="/returns"/>
    /// <inheritdoc cref="GorgonDepthStencilState.IsStencilEnabled" path="/remarks"/>
    public GorgonDepthStencilStateBuilder StencilEnable(bool enabled)
    {
        _worker.IsStencilEnabled = enabled;
        return this;
    }

    /// <inheritdoc/>
    public GorgonDepthStencilState Build(IGorgonAllocator<GorgonDepthStencilState>? allocator = null)
    {
        allocator ??= _allocator;
        GorgonDepthStencilState result = allocator.Allocate(ds => Copy(_worker, ds));
        return result;
    }

    /// <inheritdoc/>
    public GorgonDepthStencilStateBuilder ResetTo(GorgonDepthStencilState builderObject)
    {
        Copy(builderObject, _worker);
        return this;
    }

    /// <inheritdoc/>
    public GorgonDepthStencilStateBuilder Clear()
    {
        Copy(GorgonDepthStencilState.Default, _worker);
        return this;
    }
}