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
// Created: June 15, 2026 12:32:07 AM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Describes a state for color and alpha blending operations.
/// </summary>
/// <remarks>
/// <para>
/// This defines how blending is performed between rendered geometry and the current render target view. Independent blend states can be set for each render target view bound, but only if the 
/// <see cref="GorgonGraphicsPso.IsIndependentBlendingEnabled"/> is <b>true</b>. Otherwise, only the first render target view blend state is used.
/// </para>
/// <para>
/// The blend state contains several common blend states used by applications as static members of the class. Use these instead of defining your own so there's less garbage for the GC to pick up.
/// </para>
/// <para>
/// The default constructor sets up the <see cref="GorgonBlendState"/> with the same parameters as the <see cref="Default"/> property.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
public sealed record class GorgonBlendState()
    : IPsoState<GorgonBlendState>
{
    /// <summary>
    /// A predefined blending state that indicates that there will be no blending.
    /// </summary>
    public static readonly GorgonBlendState NoBlending = new()
    {
        IsEnabled = false,
        SourceColorBlend = Blend.One,
        DestinationColorBlend = Blend.Zero,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.Zero
    };

    /// <summary>
    /// A predefined blending state that performs modulated blending.
    /// </summary>
    public static readonly GorgonBlendState Default = new();

    /// <summary>
    /// A predefined blending state that performs premultiplied blending.
    /// </summary>
    public static readonly GorgonBlendState PremultipliedBlend = new()
    {
        SourceColorBlend = Blend.One,
        DestinationColorBlend = Blend.InverseSourceAlpha,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.InverseSourceAlpha
    };

    /// <summary>
    /// A predefined blending state that performs additive blending.
    /// </summary>
    public static readonly GorgonBlendState AdditiveBlend = new()
    {
        SourceColorBlend = Blend.SourceAlpha,
        DestinationColorBlend = Blend.One,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.InverseSourceAlpha
    };

    /// <summary>
    /// A predefined blending state that performs modulated blending on the color channels only.
    /// </summary>
    public static readonly GorgonBlendState ModulatedColorBlend = new()
    {
        SourceColorBlend = Blend.SourceAlpha,
        DestinationColorBlend = Blend.InverseSourceAlpha,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.Zero
    };

    /// <summary>
    /// A predefined blending state that performs premultiplied blending on the color channels only.
    /// </summary>
    public static readonly GorgonBlendState PremultipliedColorBlend = new()
    {
        SourceColorBlend = Blend.One,
        DestinationColorBlend = Blend.InverseSourceAlpha,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.Zero
    };

    /// <summary>
    /// A predefined blending state that performs additive blending on the color channels only.
    /// </summary>
    public static readonly GorgonBlendState AdditiveColorBlend = new()
    {
        SourceColorBlend = Blend.SourceAlpha,
        DestinationColorBlend = Blend.One,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.Zero
    };

    /// <summary>
    /// Property to return whether blending is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, the <see cref="IsLogicEnabled"/> property must be <b>false</b>. Blending and logic operations cannot be enabled at the same time.
    /// </para>
    /// <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    public bool IsEnabled
    {
        get;
        init;
    } = true;

    /// <summary>
    /// Property to return whether logic operations are enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, the <see cref="LogicOperation"/> is applied to the value returned from the pixel shader and the existing contents of the render target, instead of blending.
    /// </para>
    /// <para>
    /// When this value is <b>true</b>, the <see cref="IsEnabled"/> property must be <b>false</b>. Logic operations are only used from the blend state in the first render target slot, and cannot be used when 
    /// <see cref="GorgonGraphicsPso.IsIndependentBlendingEnabled"/> is <b>true</b>. The same logic operation is applied to all of the render targets.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="LogicOperation"/>
    public bool IsLogicEnabled
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the blending operation to apply to the color channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="BlendOperation.Add"/>.
    /// </para>
    /// </remarks>
    public BlendOperation ColorBlendOperation
    {
        get;
        init;
    } = BlendOperation.Add;

    /// <summary>
    /// Property to return the blending operation to apply to the alpha channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="BlendOperation.Add"/>.
    /// </para>
    /// </remarks>
    public BlendOperation AlphaBlendOperation
    {
        get;
        init;
    } = BlendOperation.Add;

    /// <summary>
    /// Property to return the logic operation that is performed when logic operations are enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is only used when <see cref="IsLogicEnabled"/> is <b>true</b>.
    /// </para>
    /// <para>
    /// The default value is <see cref="LogicOperation.Noop"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="IsLogicEnabled"/>
    public LogicOperation LogicOperation
    {
        get;
        init;
    } = LogicOperation.Noop;

    /// <summary>
    /// Property to return the blending type to apply to the source color channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.SourceAlpha"/>.
    /// </para>
    /// </remarks>
    public Blend SourceColorBlend
    {
        get;
        init;
    } = Blend.SourceAlpha;

    /// <summary>
    /// Property to return the blending type to apply to the destination color channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.InverseSourceAlpha"/>.
    /// </para>
    /// </remarks>
    public Blend DestinationColorBlend
    {
        get;
        init;
    } = Blend.InverseSourceAlpha;

    /// <summary>
    /// Property to return the blending type to apply to the source alpha channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.One"/>.
    /// </para>
    /// </remarks>
    public Blend SourceAlphaBlend
    {
        get;
        init;
    } = Blend.One;

    /// <summary>
    /// Property to return the blending type to apply to the destination alpha channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.InverseSourceAlpha"/>.
    /// </para>
    /// </remarks>
    public Blend DestinationAlphaBlend
    {
        get;
        init;
    } = Blend.InverseSourceAlpha;

    /// <summary>
    /// Property to return the mask of color channels to write into on the render target.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows the application to exclude color channels when rendering. For example, by using a write mask of <see cref="WriteMask.Green"/>, only the green channel will be written. Users can also 
    /// combine the flags to write to multiple channels.
    /// </para>
    /// <para>
    /// The default value is <see cref="WriteMask.All"/>.
    /// </para>
    /// </remarks>
    public WriteMask WriteMask
    {
        get;
        init;
    } = WriteMask.All;

    /// <summary>
    /// Function to return the D3D blending description.
    /// </summary>
    /// <returns>The D3D 12 blending description.</returns>
    internal D3D12_RENDER_TARGET_BLEND_DESC GetDesc() =>
        new()
        {
            BlendEnable = IsEnabled,
            LogicOpEnable = IsLogicEnabled,
            LogicOp = (D3D12_LOGIC_OP)LogicOperation,
            BlendOp = (D3D12_BLEND_OP)ColorBlendOperation,
            BlendOpAlpha = (D3D12_BLEND_OP)AlphaBlendOperation,
            SrcBlend = (D3D12_BLEND)SourceColorBlend,
            DestBlend = (D3D12_BLEND)DestinationColorBlend,
            SrcBlendAlpha = (D3D12_BLEND)SourceAlphaBlend,
            DestBlendAlpha = (D3D12_BLEND)DestinationAlphaBlend,
            RenderTargetWriteMask = (byte)WriteMask
        };

    /// <inheritdoc/>
    public static GorgonBlendState GetDefinedState(GorgonBlendState state)
    {
        if (state == Default)
        {
            return Default;
        }

        if (state == NoBlending)
        {
            return NoBlending;
        }

        if (state == PremultipliedBlend)
        {
            return PremultipliedBlend;
        }

        if (state == AdditiveBlend)
        {
            return AdditiveBlend;
        }

        if (state == ModulatedColorBlend)
        {
            return ModulatedColorBlend;
        }

        if (state == PremultipliedColorBlend)
        {
            return PremultipliedColorBlend;
        }

        if (state == AdditiveColorBlend)
        {
            return AdditiveColorBlend;
        }

        return state;
    }
}