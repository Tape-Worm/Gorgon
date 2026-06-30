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
// Created: June 15, 2026 9:54:37 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to create a <see cref="GorgonBlendState"/> object.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonBlendState"/> to pass to a <see cref="GorgonGraphicsPsoBuilder"/>. This will define how blending is performed with the rendered primitive and 
/// the current render target(s).
/// </para>
/// <para>
/// A <see cref="GorgonBlendState"/> is an immutable object, it can only be created through this builder factory type.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonGraphicsPsoBuilder"/>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonBlendState"/>
public sealed class GorgonBlendStateBuilder
    : IGorgonFluentBuilder<GorgonBlendStateBuilder, GorgonBlendState, IGorgonAllocator<GorgonBlendState>>
{
    /// <summary>
    /// The default allocator for generating blend states.
    /// </summary>
    private class DefaultAllocator
        : IGorgonAllocator<GorgonBlendState>
    {
        /// <inheritdoc/>
        public GorgonBlendState Allocate(Action<GorgonBlendState>? initializer = null)
        {
            GorgonBlendState result = new();
            initializer?.Invoke(result);
            return result;
        }
    }

    private readonly GorgonBlendState _worker = new();
    private readonly DefaultAllocator _allocator = new();

    /// <summary>
    /// Function to copy a blend state into another.
    /// </summary>
    /// <param name="source">The blend state to copy from.</param>
    /// <param name="destination">The blend state to copy into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy(GorgonBlendState source, GorgonBlendState destination)
    {
        destination.AlphaBlendOperation = source.AlphaBlendOperation;
        destination.ColorBlendOperation = source.ColorBlendOperation;
        destination.DestinationAlphaBlend = source.DestinationAlphaBlend;
        destination.DestinationColorBlend = source.DestinationColorBlend;
        destination.IsEnabled = source.IsEnabled;
        destination.IsLogicEnabled = source.IsLogicEnabled;
        destination.LogicOperation = source.LogicOperation;
        destination.SourceAlphaBlend = source.SourceAlphaBlend;
        destination.SourceColorBlend = source.SourceColorBlend;
        destination.WriteMask = source.WriteMask;
    }

    /// <summary>
    /// Function to enable or disable the blend state.
    /// </summary>
    /// <param name="enabled"><b>true</b> if the blend state is enabled, <b>false</b> if not.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <inheritdoc cref="GorgonBlendState.IsEnabled" path="/remarks"/>
    public GorgonBlendStateBuilder Enable(bool enabled)
    {
        _worker.IsEnabled = enabled;
        return this;    
    }

    /// <summary>
    /// Function to enable or disable logic operations.
    /// </summary>    
    /// <inheritdoc cref="Enable(bool)" path="/param"/>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.IsLogicEnabled" path="/remarks"/>
    public GorgonBlendStateBuilder LogicOperationsEnable(bool enabled)
    {
        _worker.IsLogicEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Function to set the color blending operation.
    /// </summary>
    /// <param name="op">The operation to apply.</param>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.ColorBlendOperation" path="/remarks"/>
    public GorgonBlendStateBuilder ColorBlendOperation(BlendOperation op)
    {
        _worker.ColorBlendOperation = op;
        return this;
    }

    /// <summary>
    /// Function to set the alpha blending operation.
    /// </summary>
    /// <inheritdoc cref="ColorBlendOperation(BlendOperation)" path="/param"/>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.AlphaBlendOperation" path="/remarks"/>
    public GorgonBlendStateBuilder AlphaBlendOperation(BlendOperation op)
    {
        _worker.AlphaBlendOperation = op;
        return this;
    }

    /// <summary>
    /// Function to set the logic operation.
    /// </summary>
    /// <inheritdoc cref="ColorBlendOperation(BlendOperation)" path="/param"/>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// This value is only used when the <see cref="LogicOperationsEnable(bool)"/> method has been called with a value of <b>true</b>.
    /// </para>
    /// <inheritdoc cref="GorgonBlendState.LogicOperation" path="/remarks/para"/>
    /// </remarks>
    public GorgonBlendStateBuilder LogicOperation(LogicOperation op)
    {
        _worker.LogicOperation = op;
        return this;
    }

    /// <summary>
    /// Function to set the source color blending type.
    /// </summary>
    /// <param name="blend">The blending type to apply.</param>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.SourceColorBlend" path="/remarks"/>
    public GorgonBlendStateBuilder SourceColorBlend(Blend blend)
    {
        _worker.SourceColorBlend = blend;
        return this;
    }

    /// <summary>
    /// Function to set the source alpha blending type.
    /// </summary>
    /// <inheritdoc cref="SourceColorBlend(Blend)" path="/param"/>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.SourceAlphaBlend" path="/remarks"/>
    public GorgonBlendStateBuilder SourceAlphaBlend(Blend blend)
    {
        _worker.SourceAlphaBlend = blend;
        return this;
    }

    /// <summary>
    /// Function to set the destination color blending type.
    /// </summary>
    /// <inheritdoc cref="SourceColorBlend(Blend)" path="/param"/>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.DestinationColorBlend" path="/remarks"/>
    public GorgonBlendStateBuilder DestinationColorBlend(Blend blend)
    {
        _worker.DestinationColorBlend = blend;
        return this;
    }

    /// <summary>
    /// Function to set the destination alpha blending type.
    /// </summary>
    /// <inheritdoc cref="SourceColorBlend(Blend)" path="/param"/>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.DestinationAlphaBlend" path="/remarks"/>
    public GorgonBlendStateBuilder DestinationAlphaBlend(Blend blend)
    {
        _worker.DestinationAlphaBlend = blend;
        return this;
    }

    /// <summary>
    /// Function to set the write mask for the color channels on the output.
    /// </summary>
    /// <param name="writeMask">The mask that will be applied.</param>
    /// <inheritdoc cref="Enable(bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonBlendState.WriteMask" path="/remarks"/>
    public GorgonBlendStateBuilder WriteMask(WriteMask writeMask)
    {
        _worker.WriteMask = writeMask;
        return this;
    }

    /// <inheritdoc/>
    public GorgonBlendState Build(IGorgonAllocator<GorgonBlendState>? allocator = null)
    {
        allocator ??= _allocator;
        GorgonBlendState result = allocator.Allocate(b => Copy(_worker, b));
        return result;
    }

    /// <inheritdoc/>
    public GorgonBlendStateBuilder ResetTo(GorgonBlendState builderObject)
    {
        Copy(builderObject, _worker);
        return this;
    }

    /// <inheritdoc/>
    public GorgonBlendStateBuilder Clear()
    {
        Copy(GorgonBlendState.Default, _worker);        
        return this;
    }
}
