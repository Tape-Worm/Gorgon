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
using System.Threading;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines how a triangle primitive should be rendered.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum FillMode
    {
        /// <summary>
        /// <para>
        /// Draw lines connecting the vertices. Adjacent vertices are not drawn.
        /// </para>
        /// </summary>
        Wireframe = D3D11.FillMode.Wireframe,
        /// <summary>
        /// <para>
        /// Fill the triangles formed by the vertices. Adjacent vertices are not drawn.
        /// </para>
        /// </summary>
        Solid = D3D11.FillMode.Solid
    }

    /// <summary>
    /// Defines how a triangle primitive should be culled from rendering.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum CullingMode
    {
        /// <summary>
        /// <para>
        /// Always draw all triangles.
        /// </para>
        /// </summary>
        None = D3D11.CullMode.None,
        /// <summary>
        /// <para>
        /// Do not draw triangles that are front-facing.
        /// </para>
        /// </summary>
        Front = D3D11.CullMode.Front,
        /// <summary>
        /// <para>
        /// Do not draw triangles that are back-facing.
        /// </para>
        /// </summary>
        Back = D3D11.CullMode.Back
    }

    /// <summary>
    /// Describes how primitive data (i.e. triangles, lines, etc...) are rasterized by the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will define how a triangle, line, point, etc... is rasterized by the GPU when rendering. Clipping, vertex ordering, culling, etc... are all affected by this state.
    /// </para>
    /// <para>
    /// This type is mutable until it is consumed in a <see cref="GorgonPipelineState"/>. After it is assigned, it is immutable and will throw an exception if any changes are made to the state after 
    /// it has been used.
    /// </para>
    /// <para>
    /// The rasterizer state contains 5 common raster states used by applications: <see cref="Default"/> (backface culling, solid fill, etc...), <see cref="WireFrameNoCulling"/> (no culling, wireframe fill, 
    /// etc...), <see cref="CullFrontFace"/> (front face culling, solid fill, etc...), <see cref="NoCulling"/> (no culling, solid fill, etc...), and <see cref="ScissorTestEnabled"/> (scissor testing 
    /// enabled, back face culling, solid fill, etc...).
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPipelineState"/>
    public class GorgonRasterState
        : IEquatable<GorgonRasterState>
    {
        #region Common States
        /// <summary>
        /// Property to return the default raster state.
        /// </summary>
        public static GorgonRasterState Default
        {
            get;
        } = new GorgonRasterState();
        
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

        /// <summary>
        /// Scissor test enabled.
        /// </summary>
        public static GorgonRasterState ScissorTestEnabled
        {
            get;
        } = new GorgonRasterState
            {
                IsScissorClippingEnabled = true
            };
        #endregion

        #region Variables.
        // The state ID.
        private static long _stateID;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the state ID.
        /// </summary>
        public long ID
        {
            get;
        }

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
        /// Property to return whether scissor rectangle clipping is enabled or not.
        /// </summary>
        /// <remarks>
        /// The default value is <b>false</b>.
        /// </remarks>
        public bool IsScissorClippingEnabled
        {
            // TODO: Remove this flag, and replace it with an array of scissor rectangles.
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
        public int ForcedUavSampleCount
        {
            get;
            internal set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy the contents of this <see cref="GorgonRasterState"/> into another one.
        /// </summary>
        /// <param name="destState">The state that will receive the contents of this state.</param>
        internal void CopyTo(GorgonRasterState destState)
        {
            destState.IsDepthClippingEnabled = IsDepthClippingEnabled;
            destState.IsAntialiasedLineEnabled = IsAntialiasedLineEnabled;
            destState.CullMode = CullMode;
            destState.DepthBias = DepthBias;
            destState.DepthBiasClamp = DepthBiasClamp;
            destState.FillMode = FillMode;
            destState.ForcedUavSampleCount = ForcedUavSampleCount;
            destState.IsFrontCounterClockwise = IsFrontCounterClockwise;
            destState.IsMultisamplingEnabled = IsMultisamplingEnabled;
            destState.IsScissorClippingEnabled = IsScissorClippingEnabled;
            destState.SlopeScaledDepthBias = SlopeScaledDepthBias;
            destState.UseConservativeRasterization = UseConservativeRasterization;
        }

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
                           ForcedSampleCount = ForcedUavSampleCount,
                           IsScissorEnabled = IsScissorClippingEnabled,
                           IsDepthClipEnabled = IsDepthClippingEnabled,
                           IsMultisampleEnabled = IsMultisamplingEnabled
                       };

            return new D3D11.RasterizerState2(device, desc)
                   {
                       DebugName = nameof(GorgonRasterState)
                   };
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="info" /> parameter; otherwise, false.</returns>
        /// <param name="info">An object to compare with this object.</param>
        public bool Equals(GorgonRasterState info)
        {
            return (this == info) || (info != null
                                      && IsAntialiasedLineEnabled == info.IsAntialiasedLineEnabled
                                      && CullMode == info.CullMode
                                      && DepthBias == info.DepthBias
                                      && DepthBiasClamp.EqualsEpsilon(info.DepthBiasClamp)
                                      && IsDepthClippingEnabled == info.IsDepthClippingEnabled
                                      && FillMode == info.FillMode
                                      && ForcedUavSampleCount == info.ForcedUavSampleCount
                                      && IsFrontCounterClockwise == info.IsFrontCounterClockwise
                                      && IsMultisamplingEnabled == info.IsMultisamplingEnabled
                                      && IsScissorClippingEnabled == info.IsScissorClippingEnabled
                                      && SlopeScaledDepthBias.EqualsEpsilon(info.SlopeScaledDepthBias));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRasterState"/> class.
        /// </summary>
        internal GorgonRasterState()
        {
            ID = Interlocked.Increment(ref _stateID);
            IsScissorClippingEnabled = false;
            CullMode = CullingMode.Back;
            FillMode = FillMode.Solid;
            IsDepthClippingEnabled = true;
        }
        #endregion
    }
}
