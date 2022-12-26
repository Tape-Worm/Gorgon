#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 13, 2018 4:59:08 PM
// 
#endregion

using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers;

/// <summary>
/// Defines the state to pass to a call to the <see cref="Gorgon2D.Begin"/> method.
/// </summary>
public sealed class Gorgon2DBatchState
{
    /// <summary>
    /// A pre-defined batch state that turns off blending.
    /// </summary>
    public static readonly Gorgon2DBatchState NoBlend = new()
    {
        BlendState = GorgonBlendState.NoBlending
    };

    /// <summary>
    /// A pre-defined batch state that sets up modulated blending for color values, and using the source alpha channel to overwrite the destination alpha channel.
    /// </summary>
    public static readonly Gorgon2DBatchState ModulatedAlphaOverwrite = new()
    {
        BlendState = GorgonBlendState.ModulatedAlphaOverwrite
    };

    /// <summary>
    /// A pre-defined batch state that sets up additive blending.
    /// </summary>
    public static readonly Gorgon2DBatchState AdditiveBlend = new()
    {
        BlendState = GorgonBlendState.Additive
    };

    /// <summary>
    /// A pre-defined batch state that sets up additive blending for color values, and using the source alpha channel to overwrite the destination alpha channel.
    /// </summary>
    public static readonly Gorgon2DBatchState AdditiveAlphaOverwrite = new()
    {
        BlendState = GorgonBlendState.AdditiveAlphaOverwrite
    };

    /// <summary>
    /// A pre-defined batch state that sets up soft additive blending.
    /// </summary>
    public static readonly Gorgon2DBatchState SoftAdditiveBlend = new()
    {
        BlendState = GorgonBlendState.SoftAdditive
    };

    /// <summary>
    /// A pre-defined batch state that sets up premultiplied blending.
    /// </summary>
    public static readonly Gorgon2DBatchState PremultipliedBlend = new()
    {
        BlendState = GorgonBlendState.Premultiplied
    };

    /// <summary>
    /// A pre-defined batch state that sets up premultiplied blendingfor color values, and using the source alpha channel to overwrite the destination alpha channel.
    /// </summary>
    public static readonly Gorgon2DBatchState PremultipliedBlendAlphaOverwrite = new()
    {
        BlendState = GorgonBlendState.PremultipliedAlphaOverwrite
    };

    /// <summary>
    /// A pre-defined batch state that sets up wirefame mode with no culling.
    /// </summary>
    public static readonly Gorgon2DBatchState WireFrameNoCulling = new()
    {
        RasterState = GorgonRasterState.WireFrameNoCulling
    };

    /// <summary>
    /// A pre-defined batch state that enables depth testing/writing and no blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthEnabledNoBlend = new()
    {
        BlendState = GorgonBlendState.NoBlending,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabled
    };

    /// <summary>
    /// A pre-defined batch state that enables depth testing/writing and modulated blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthEnabled = new()
    {
        BlendState = GorgonBlendState.Default,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabled
    };

    /// <summary>
    /// A pre-defined batch state that enables depth testing/writing and additive blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthEnabledAdditiveBlend = new()
    {
        BlendState = GorgonBlendState.Additive,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabled
    };

    /// <summary>
    /// A pre-defined batch state that enables depth testing/writing and premultiplied blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthEnabledPremultipliedBlend = new()
    {
        BlendState = GorgonBlendState.Premultiplied,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabled
    };

    /// <summary>
    /// A pre-defined batch state that sets an inverted blending mode.
    /// </summary>
    public static readonly Gorgon2DBatchState InvertedBlend = new()
    {
        BlendState = GorgonBlendState.Inverted
    };

    /// <summary>
    /// A pre-defined batch state that enables depth read testing and no blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthReadNoBlend = new()
    {
        BlendState = GorgonBlendState.NoBlending,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabledNoWrite
    };

    /// <summary>
    /// A pre-defined batch state that enables depth read testing and modulated blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthRead = new()
    {
        BlendState = GorgonBlendState.Default,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabledNoWrite
    };

    /// <summary>
    /// A pre-defined batch state that enables depth read testing and additive blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthReadAdditiveBlend = new()
    {
        BlendState = GorgonBlendState.Additive,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabledNoWrite
    };

    /// <summary>
    /// A pre-defined batch state that enables depth read testing and premultiplied blending.
    /// </summary>
    public static readonly Gorgon2DBatchState DepthReadPremultipliedBlend = new()
    {
        BlendState = GorgonBlendState.Premultiplied,
        DepthStencilState = GorgonDepthStencilState.DepthLessEqualEnabledNoWrite
    };

    /// <summary>
    /// A pre-defined batch state that sets scissor rectangle clipping.
    /// </summary>
    public static readonly Gorgon2DBatchState ScissorClipping = new()
    {
        RasterState = GorgonRasterState.ScissorRectanglesEnabled
    };

    /// <summary>
    /// Property to return the current blending state to apply.
    /// </summary>
    public GorgonBlendState BlendState
    {
        get;
        internal set;
    } = GorgonBlendState.Default;

    /// <summary>
    /// Property to return the current raster state to apply.
    /// </summary>
    public GorgonRasterState RasterState
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the current depth/stencil state to apply.
    /// </summary>
    public GorgonDepthStencilState DepthStencilState
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the current pixel shader to use.
    /// </summary>
    public Gorgon2DShaderState<GorgonPixelShader> PixelShaderState
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the current vertex shader to use.
    /// </summary>
    public Gorgon2DShaderState<GorgonVertexShader> VertexShaderState
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to set or return the blending factor used to modulate with the pixel shader, current render target or both.
    /// </summary>
    public GorgonColor BlendFactor
    {
        get;
        set;
    } = GorgonColor.White;

    /// <summary>
    /// Property to set or return the mask used to define which samples get updated in the active render target(s).
    /// </summary>
    public int BlendSampleMask
    {
        get;
        set;
    } = int.MinValue;

    /// <summary>
    /// Property to set or return the stencil reference value used when performing a stencil test.
    /// </summary>
    public int StencilReference
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Gorgon2DBatchState"/> class.
    /// </summary>
    internal Gorgon2DBatchState()
    {
        // We should not be able to create this outside of the assembly.
    }
}
