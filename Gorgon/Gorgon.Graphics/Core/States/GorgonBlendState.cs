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
/// This is the part of the <see cref="GorgonGraphicsPso"/> object that defines blending parameters for render target(s).
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
public sealed class GorgonBlendState
    : IEquatable<GorgonBlendState>
{
    /// <summary>
    /// A predefined blending state that indicates that there will be no blending.
    /// </summary>
    public static readonly GorgonBlendState NoBlending = new()
    {
        IsEnabled = false
    };

    /// <summary>
    /// A predefined blending state that performs modulated blending.
    /// </summary>
    public static readonly GorgonBlendState Default = new()
    {
        IsEnabled = true,
        SourceColorBlend = Blend.SourceAlpha,
        DestinationColorBlend = Blend.InverseSourceAlpha,
        SourceAlphaBlend = Blend.One,
        DestinationAlphaBlend = Blend.InverseSourceAlpha
    };

    /// <summary>
    /// A predefined blending state that performs premultiplied blending.
    /// </summary>
    public static readonly GorgonBlendState PremultipliedBlend = new()
    {
        IsEnabled = true,
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
        IsEnabled = true,
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
        IsEnabled = true,
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
        IsEnabled = true,
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
        IsEnabled = true,
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
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsEnabled
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return whether blending logic is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsLogicEnabled
    {
        get;
        internal set;
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
        internal set;
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
        internal set;
    } = BlendOperation.Add;

    /// <summary>
    /// Property to return the logic operation that is performed while blending.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="LogicOperation.Noop"/>.
    /// </para>
    /// </remarks>
    public LogicOperation LogicOperation
    {
        get;
        internal set;
    } = LogicOperation.Noop;

    /// <summary>
    /// Property to return the blending type to apply to the source color channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.One"/>.
    /// </para>
    /// </remarks>
    public Blend SourceColorBlend
    {
        get;
        internal set;
    } = Blend.One;

    /// <summary>
    /// Property to return the blending type to apply to the destination color channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.Zero"/>.
    /// </para>
    /// </remarks>
    public Blend DestinationColorBlend
    {
        get;
        internal set;
    } = Blend.Zero;

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
        internal set;
    } = Blend.One;

    /// <summary>
    /// Property to return the blending type to apply to the destination alpha channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="Blend.Zero"/>.
    /// </para>
    /// </remarks>
    public Blend DestinationAlphaBlend
    {
        get;
        internal set;
    } = Blend.Zero;

    /// <summary>
    /// Property to return the mask of color channels to write into on the render target.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows the application to exclude colour channels when rendering. By sending a write mask of <see cref="WriteMask.Green"/>, only the green channel will be written. Users can also combine the 
    /// flags to write to multiple channels.
    /// </para>
    /// <para>
    /// The default value is <see cref="WriteMask.All"/>.
    /// </para>
    /// </remarks>
    public WriteMask WriteMask
    {
        get;
        internal set;
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
    public override int GetHashCode()
    {
        HashCode code = new();
        code.Add(IsEnabled);
        code.Add(IsLogicEnabled);
        code.Add(LogicOperation);
        code.Add(ColorBlendOperation);
        code.Add(AlphaBlendOperation);
        code.Add(SourceColorBlend);
        code.Add(DestinationColorBlend);
        code.Add(SourceAlphaBlend);
        code.Add(DestinationAlphaBlend);
        code.Add(WriteMask);

        return code.ToHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GorgonBlendState blendState ? Equals(blendState) : base.Equals(obj);

    /// <inheritdoc/>
    public bool Equals(GorgonBlendState? other) =>
            (other == this) || (other is not null 
            && IsEnabled == other.IsEnabled
            && IsLogicEnabled == other.IsLogicEnabled
            && LogicOperation == other.LogicOperation
            && ColorBlendOperation == other.ColorBlendOperation
            && AlphaBlendOperation == other.AlphaBlendOperation
            && SourceColorBlend == other.SourceColorBlend
            && DestinationColorBlend == other.DestinationColorBlend
            && SourceAlphaBlend == other.SourceAlphaBlend
            && DestinationAlphaBlend == other.DestinationAlphaBlend
            && WriteMask == other.WriteMask);    

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
    /// </summary>
    internal GorgonBlendState()
    {
    }
}
