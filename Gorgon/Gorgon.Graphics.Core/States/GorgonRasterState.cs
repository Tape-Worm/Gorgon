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
using Gorgon.Graphics.Core.Properties;
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
    /// This type is mutable until it is consumed in a <see cref="GorgonPipelineState"/>. After it is assigned, it is immutable and will throw an exception if any changes are made to the state after 
    /// it has been used.
    /// </para>
    /// <para>
    /// Because of the state locking and for performance, it is best practice to pre-create the required states ahead of time.
    /// </para>
    /// <para>
    /// The rasterizer state contains 5 common raster states used by applications: <see cref="Default"/> (backface culling, solid fill, etc...), <see cref="WireFrameNoCulling"/> (no culling, wireframe fill, 
    /// etc...), <see cref="CullFrontFace"/> (front face culling, solid fill, etc...), <see cref="NoCulling"/> (no culling, solid fill, etc...), and <see cref="ScissorTestEnabled"/> (scissor testing 
    /// enabled, back face culling, solid fill, etc...). These states are always locked, and cannot be changed, but can be used as a base for a new state by using the 
    /// <see cref="GorgonRasterState(GorgonRasterState)"/> copy constructor and then modifying as needed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="Default"/>
    /// <seealso cref="WireFrameNoCulling"/>
    /// <seealso cref="CullFrontFace"/>
    /// <seealso cref="WireFrameNoCulling"/>
    /// <seealso cref="ScissorTestEnabled"/>
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
        } = new GorgonRasterState
            {
                IsLocked = true
            };


        /// <summary>
        /// Wireframe, with no culling.
        /// </summary>
        public static GorgonRasterState WireFrameNoCulling
        {
            get;
        } = new GorgonRasterState
            {
                CullMode = D3D11.CullMode.None,
                FillMode = D3D11.FillMode.Wireframe,
                IsLocked = true
            };

        /// <summary>
        /// Front face culling.
        /// </summary>
        public static GorgonRasterState CullFrontFace
        {
            get;
        } = new GorgonRasterState
            {
                CullMode = D3D11.CullMode.Front,
                IsLocked = true
            };

        /// <summary>
        /// No culling.
        /// </summary>
        public static GorgonRasterState NoCulling
        {
            get;
        } = new GorgonRasterState
            {
                CullMode = D3D11.CullMode.None,
                IsLocked = true
            };

        /// <summary>
        /// Scissor test enabled.
        /// </summary>
        public static GorgonRasterState ScissorTestEnabled
        {
            get;
        } = new GorgonRasterState
            {
                IsScissorClippingEnabled = true,
                IsLocked = true
            };
        #endregion

        #region Variables.
        // The current triangle culling mode.
        private D3D11.CullMode _cullMode;

        // The current primitive filling mode.
        private D3D11.FillMode _fillMode;

        // Flag to indicate that front facing primitives should have their vertices ordered counter clockwise.
        private bool _isFrontCounterClockwise;

        // The current depth buffer bias.
        private int _depthBias;

        // The current slope scale depth buffer bias.
        private float _slopeScaledDepthBias;

        // The current clamping value for the depth bias.
        private float _depthBiasClamp;

        // Flag to indicate that clipping against depth is on or off.
        private bool _isDepthClippingEnabled;

        // Flag to indicate that clipping against a scissor rectangle is on or off.
        private bool _isScissorClippingEnabled;

        // Flag to indicate that multisampling is on or off.
        private bool _isMultisamplingEnabled;

        // Flag to indicate that line antialiasing is on or off.
        private bool _isAntialiasedLineEnabled;

        // The current forced UAV sampling count.
        private int _forcedUavSampleCount;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the sampler state is locked for writing.
        /// </summary>
        internal bool IsLocked
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current culling mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is used to determine if a triangle is drawn or not by the direction it's facing.
        /// </para>
        /// <para>
        /// The default value is <c>Back</c>.
        /// </para>
        /// </remarks>
        public D3D11.CullMode CullMode
        {
            get => _cullMode;
            set
            {
                CheckLocked();
                _cullMode = value;
            }
        }

        /// <summary>
        /// Property to set or return the triangle fill mode.
        /// </summary>
        /// <remarks>
        /// The default value is <c>Solid</c>.
        /// </remarks>
        public D3D11.FillMode FillMode
        {
            get => _fillMode;
            set
            {
                CheckLocked();
                _fillMode = value;
            }
        }

        /// <summary>
        /// Property to set or return whether a triangle is front or back facing.
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
            get => _isFrontCounterClockwise;
            set
            {
                CheckLocked();
                _isFrontCounterClockwise = value;
            }
        }

        /// <summary>
        /// Property to set or return the value to be added to the depth of a pixel.
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
            get => _depthBias;
            set
            {
                CheckLocked();
                _depthBias = value;
            }
        }

        /// <summary>
        /// Property to set or return the scalar used for a slope of a pixel.
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
            get => _slopeScaledDepthBias;
            set
            {
                CheckLocked();
                _slopeScaledDepthBias = value;
            }
        }

        /// <summary>
        /// Property to set or return the clamping value for the <see cref="DepthBias"/>.
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
            get => _depthBiasClamp;
            set
            {
                CheckLocked();
                _depthBiasClamp = value;
            }
        }


        /// <summary>
        /// Property to set or return whether depth/clipping is enabled or not.
        /// </summary>
        /// <remarks>
        /// The default value is <b>true</b>.
        /// </remarks>
        public bool IsDepthClippingEnabled
        {
            get => _isDepthClippingEnabled;
            set
            {
                CheckLocked();
                _isDepthClippingEnabled = value;
            }
        }

        /// <summary>
        /// Property to set or return whether scissor rectangle clipping is enabled or not.
        /// </summary>
        /// <remarks>
        /// The default value is <b>false</b>.
        /// </remarks>
        public bool IsScissorClippingEnabled
        {
            get => _isScissorClippingEnabled;
            set
            {
                CheckLocked();
                _isScissorClippingEnabled = value;
            }
        }

        /// <summary>
        /// Property to set or return whether multisampling anti-aliasing or alpha line anti-aliasing for a render target is enabled or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set this value to <b>true</b> to use quadrilateral anti-aliasing or <b>false</b> to use a alpha line algorithm. This will only apply to render targets that have a 
        /// <see cref="IGorgonTextureInfo.MultisampleInfo"/> <see cref="GorgonMultisampleInfo.Count"/> greater than 1.
        /// </para>
        /// <para>
        /// If the current <see cref="IGorgonVideoDevice"/> has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <see cref="FeatureLevelSupport.Level_10_0"/>, then setting this value to 
        /// <b>false</b> will disable anti-aliasing.
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
            get => _isMultisamplingEnabled;
            set
            {
                CheckLocked();
                _isMultisamplingEnabled = value;
            }
        }

        /// <summary>
        /// Property to set or return whether anti-aliased line drawing is enabled or not.
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
            get => _isAntialiasedLineEnabled;
            set
            {
                CheckLocked();
                _isAntialiasedLineEnabled = value;
            }
        }

        /// <summary>
        /// Property to set or return the number of samples to use when unordered access view rendering or rasterizing.
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
            get => _forcedUavSampleCount;
            set
            {
                CheckLocked();
                _forcedUavSampleCount = value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the state is locked for read-only access.
        /// </summary>
        private void CheckLocked()
        {
            if (IsLocked)
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_STATE_IMMUTABLE, GetType().Name));
            }
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
        /// <param name="info">The <see cref="GorgonRasterState"/> to copy settings from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonRasterState(GorgonRasterState info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            _isAntialiasedLineEnabled = info.IsAntialiasedLineEnabled;
            _cullMode = info.CullMode;
            _depthBias = info.DepthBias;
            _depthBiasClamp = info.DepthBiasClamp;
            _isDepthClippingEnabled = info.IsDepthClippingEnabled;
            _fillMode = info.FillMode;
            _forcedUavSampleCount = info.ForcedUavSampleCount;
            _isFrontCounterClockwise = info.IsFrontCounterClockwise;
            _isMultisamplingEnabled = info.IsMultisamplingEnabled;
            _isScissorClippingEnabled = info.IsScissorClippingEnabled;
            _slopeScaledDepthBias = info.SlopeScaledDepthBias;
            IsLocked = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRasterState"/> class.
        /// </summary>
        public GorgonRasterState()
        {
            _isScissorClippingEnabled = true;
            _cullMode = D3D11.CullMode.Back;
            _fillMode = D3D11.FillMode.Solid;
            _isDepthClippingEnabled = true;
        }
        #endregion
    }
}
