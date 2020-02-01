#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 27, 2016 7:02:11 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Describes how primitive data (i.e. triangles, lines, etc...) are rasterized by the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will define how a triangle, line, point, etc... is rasterized by the GPU when rendering. Clipping, vertex ordering, culling, etc... are all affected by this state.
    /// </para>
    /// <para>
    /// The rasterizer state contains 4 common raster states used by applications: <see cref="Default"/> (backface culling, solid fill, etc...), <see cref="WireFrameNoCulling"/> (no culling, wireframe fill, 
    /// etc...), <see cref="CullFrontFace"/> (front face culling, solid fill, etc...), and <see cref="NoCulling"/> (no culling, solid fill, etc...).
    /// </para>
    /// <para>
    /// A raster state is an immutable object, and as such can only be created by using a <see cref="GorgonRasterStateBuilder"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonRasterStateBuilder"/>
    public class GorgonRasterState
        : IEquatable<GorgonRasterState>
    {
        #region Common States
        /// <summary>
        /// The default raster state.
        /// </summary>
        public static GorgonRasterState Default
        {
            get;
        } = new GorgonRasterState();

        /// <summary>
        /// The default raster state, with scissor rectangles enabled.
        /// </summary>
        public static GorgonRasterState ScissorRectanglesEnabled
        {
            get;
        } =new GorgonRasterState
        {
            ScissorRectsEnabled = true
        };

        /// <summary>
        /// Wireframe, with no culling.
        /// </summary>
        public static GorgonRasterState WireFrameNoCulling
        {
            get;
        } = new GorgonRasterState
        {
            CullMode = CullingMode.None,
            FillMode = FillMode.Wireframe
        };

        /// <summary>
        /// Front face culling.
        /// </summary>
        public static GorgonRasterState CullFrontFace
        {
            get;
        } = new GorgonRasterState
        {
            CullMode = CullingMode.Front
        };

        /// <summary>
        /// No culling.
        /// </summary>
        public static GorgonRasterState NoCulling
        {
            get;
        } = new GorgonRasterState
        {
            CullMode = CullingMode.None
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the current culling mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is used to determine if a triangle is drawn or not by the direction it's facing.
        /// </para>
        /// <para>
        /// The default value is <see cref="CullingMode.Back"/>.
        /// </para>
        /// </remarks>
        public CullingMode CullMode
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the triangle fill mode.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="Core.FillMode.Solid"/>.
        /// </remarks>
        public FillMode FillMode
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whether conservative rasterization should be used or not.
        /// </summary>
        public bool UseConservativeRasterization
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whether a triangle is front or back facing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value determines if a triangle is front or back facing by using the winding order of its vertices.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IsFrontCounterClockwise
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the value to be added to the depth of a pixel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is used to help z-fighting for co-planar polygons. This is often caused by a lack of precision for the depth in the view volume and the depth/stencil buffer. By adding a small offset 
        /// via this property, it can make a polygon appear to be in front of or behind another polygon even though they actually share the same (or very nearly the same depth value).
        /// </para>
        /// <para>
        /// The default value is 0.
        /// </para>
        /// </remarks>
        public int DepthBias
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the scalar used for a slope of a pixel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used in conjunction with the <see cref="DepthBias"/> to help overcome issues (typically "acne" for shadow maps) when rendering coplanar polygons. It is used to adjust the depth value 
        /// based on a slope.
        /// </para>
        /// <para>
        /// The default value is 0.0f.
        /// </para>
        /// </remarks>
        public float SlopeScaledDepthBias
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the clamping value for the <see cref="DepthBias"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The maximum <see cref="DepthBias"/> for a pixel.
        /// </para>
        /// <para>
        /// The default value is 0.0f.
        /// </para>
        /// </remarks>
        /// <seealso cref="DepthBias"/>
        public float DepthBiasClamp
        {
            get;
            internal set;
        }


        /// <summary>
        /// Property to return whether depth/clipping is enabled or not.
        /// </summary>
        /// <remarks>
        /// The default value is <b>true</b>.
        /// </remarks>
        public bool IsDepthClippingEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whether multisampling anti-aliasing or alpha line anti-aliasing for a render target is enabled or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set this value to <b>true</b> to use quadrilateral anti-aliasing or <b>false</b> to use a alpha line algorithm. This will only apply to render targets that have a 
        /// <see cref="GorgonRenderTarget2DView.MultisampleInfo"/> <see cref="GorgonMultisampleInfo.Count"/> greater than 1.
        /// </para>
        /// <para>
        /// This value overrides the value set in <see cref="IsAntialiasedLineEnabled"/>.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IsAntialiasedLineEnabled"/>
        public bool IsMultisamplingEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whether anti-aliased line drawing is enabled or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This only applies when drawing lines and <see cref="IsMultisamplingEnabled"/> is set to <b>false</b>.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IsMultisamplingEnabled"/>
        public bool IsAntialiasedLineEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the number of samples to use when unordered access view rendering or rasterizing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This forces the number of samples to use when rendering unordered access view data. The valid values are 0, 1, 2, 4, 8 and a value of 16 indicates that the sample count is not forced.
        /// </para>
        /// <para>
        /// The default value is 0.
        /// </para>
        /// <para>
        /// <note type="note">
        /// <para>
        /// If you want to render with sample count set to 1 or greater, you must follow these guidelines:
        /// <list type="bullet">
        ///		<item> 
        ///			<description>Don't bind depth-stencil views.</description>
        ///		</item>
        ///		<item>
        ///			<description>Disable depth testing.</description>
        ///		</item>
        ///		<item>
        ///			<description>Ensure the shader doesn't output depth.</description>
        ///		</item>
        ///		<item>
        ///			<description>If you have any render-target views bound and this value is greater than 1, ensure that every render target has only a single sample.</description>
        ///		</item>
        ///		<item>
        ///			<description>Don't operate the shader at sample frequency.</description>
        ///		</item> 
        /// </list>
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public int ForcedReadWriteViewSampleCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whwther scissor rectangles are enabled or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Scissor rectangles are used to clip pixels on a region defined by a scissor rectangle region. These rectangles are set by calling <see cref="GorgonGraphics.SetScissorRect"/> 
        /// or <see cref="GorgonGraphics.SetScissorRects"/> on the <see cref="GorgonGraphics"/> interface.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// If no scissor rectangle is set on the <see cref="GorgonGraphics"/> interface, and this is set to true, nothing will be rendered.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonGraphics"/>
        public bool ScissorRectsEnabled
        {
            get;
            internal set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the Direct 3D 11 rasterizer state object.
        /// </summary>
        /// <param name="device">The direct 3D device.</param>
        /// <returns>The D3D 11 rasterizer state object.</returns>
        internal D3D11.RasterizerState2 GetD3D11RasterState(D3D11.Device5 device)
        {
            var desc = new D3D11.RasterizerStateDescription2
            {
                CullMode = (D3D11.CullMode)CullMode,
                ConservativeRasterizationMode =
                               UseConservativeRasterization ? D3D11.ConservativeRasterizationMode.On : D3D11.ConservativeRasterizationMode.Off,
                FillMode = (D3D11.FillMode)FillMode,
                DepthBias = DepthBias,
                DepthBiasClamp = DepthBiasClamp,
                IsAntialiasedLineEnabled = IsAntialiasedLineEnabled,
                IsFrontCounterClockwise = IsFrontCounterClockwise,
                SlopeScaledDepthBias = SlopeScaledDepthBias,
                ForcedSampleCount = ForcedReadWriteViewSampleCount,
                IsScissorEnabled = ScissorRectsEnabled,
                IsDepthClipEnabled = IsDepthClippingEnabled,
                IsMultisampleEnabled = IsMultisamplingEnabled
            };

            return new D3D11.RasterizerState2(device, desc)
            {
                DebugName = nameof(GorgonRasterState)
            };
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="state" /> parameter; otherwise, false.</returns>
        /// <param name="state">An object to compare with this object.</param>
        public bool Equals(GorgonRasterState state) => (this == state) || ((state != null)
                                       && (IsAntialiasedLineEnabled == state.IsAntialiasedLineEnabled)
                                       && (CullMode == state.CullMode)
                                       && (DepthBias == state.DepthBias)
                                       && (DepthBiasClamp.EqualsEpsilon(state.DepthBiasClamp))
                                       && (IsDepthClippingEnabled == state.IsDepthClippingEnabled)
                                       && (FillMode == state.FillMode)
                                       && (ForcedReadWriteViewSampleCount == state.ForcedReadWriteViewSampleCount)
                                       && (IsFrontCounterClockwise == state.IsFrontCounterClockwise)
                                       && (IsMultisamplingEnabled == state.IsMultisamplingEnabled)
                                       && (ScissorRectsEnabled == state.ScissorRectsEnabled)
                                       && (SlopeScaledDepthBias.EqualsEpsilon(state.SlopeScaledDepthBias))
                                       && (UseConservativeRasterization == state.UseConservativeRasterization));

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) => Equals(obj as GorgonRasterState);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => 281.GenerateHash(IsAntialiasedLineEnabled)
            .GenerateHash(CullMode)
            .GenerateHash(DepthBias)
            .GenerateHash(DepthBiasClamp)
            .GenerateHash(IsDepthClippingEnabled)
            .GenerateHash(FillMode)
            .GenerateHash(ForcedReadWriteViewSampleCount)
            .GenerateHash(IsFrontCounterClockwise)
            .GenerateHash(IsMultisamplingEnabled)
            .GenerateHash(ScissorRectsEnabled)
            .GenerateHash(SlopeScaledDepthBias)
            .GenerateHash(UseConservativeRasterization);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRasterState"/> class.
        /// </summary>
        /// <param name="state">The state to copy.</param>
        internal GorgonRasterState(GorgonRasterState state)
        {
            IsAntialiasedLineEnabled = state.IsAntialiasedLineEnabled;
            CullMode = state.CullMode;
            DepthBias = state.DepthBias;
            DepthBiasClamp = state.DepthBiasClamp;
            IsDepthClippingEnabled = state.IsDepthClippingEnabled;
            FillMode = state.FillMode;
            ForcedReadWriteViewSampleCount = state.ForcedReadWriteViewSampleCount;
            IsFrontCounterClockwise = state.IsFrontCounterClockwise;
            IsMultisamplingEnabled = state.IsMultisamplingEnabled;
            SlopeScaledDepthBias = state.SlopeScaledDepthBias;
            UseConservativeRasterization = state.UseConservativeRasterization;
            ScissorRectsEnabled = state.ScissorRectsEnabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRasterState"/> class.
        /// </summary>
        internal GorgonRasterState()
        {
            CullMode = CullingMode.Back;
            FillMode = FillMode.Solid;
            IsDepthClippingEnabled = true;
        }
        #endregion
    }
}
