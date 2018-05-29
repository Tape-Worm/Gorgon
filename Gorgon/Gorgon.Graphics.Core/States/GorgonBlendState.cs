﻿#region MIT
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
// Created: July 30, 2016 12:21:01 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Describes how rasterized data is blended with a <see cref="GorgonRenderTargetView"/> and how render targets blend with each other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will define how rasterized data is blended with the current render target(s). The ability to disable blending, define how blending operations are performed, etc... are all done through this 
    /// state. This state also defines how blending is performed between adjacent render target(s) in the <see cref="GorgonGraphics.RenderTargets"/>. This is controlled by the 
    /// <see cref="GorgonPipelineState.IsIndependentBlendingEnabled"/> flag on the <see cref="GorgonPipelineState"/> object.
    /// </para>
    /// <para>
    /// The rasterizer state contains 5 common blend states used by applications: <see cref="Default"/> (blending enabled for the first render target, using modulated blending), <see cref="NoBlending"/> 
    /// (no blending at all), <see cref="Additive"/> (blending enabled on the first render target, using additive ops for source and dest), <see cref="Premultiplied"/> (blending enabled on the first render 
    /// target, with premultiplied blending ops for source and dest), and <see cref="Inverted"/> (blending enabled on the first render target, with inverted ops for source and dest). 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    public class GorgonBlendState
        : IEquatable<GorgonBlendState>
    {
        #region Common States.
        /// <summary>
        /// The default blending state.
        /// </summary>
        public static readonly GorgonBlendState Default;

        /// <summary>
        /// Render target 0 blending enabled, blending operations don't allow for blending.
        /// </summary>
        public static readonly GorgonBlendState NoBlending = new GorgonBlendState();

        /// <summary>
        /// Additive blending on render target 0.
        /// </summary>
        public static readonly GorgonBlendState Additive;

        /// <summary>
        /// Premultiplied alpha blending on render target 0.
        /// </summary>
        public static readonly GorgonBlendState Premultiplied;

        /// <summary>
        /// Inverse color blending on render target 0.
        /// </summary>
        public static readonly GorgonBlendState Inverted;

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether blending should be enabled for this render target.
        /// </summary>
        /// <remarks>
        /// The default value is <b>false</b>.
        /// </remarks>
        public bool IsBlendingEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the blending operation to perform.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value specifies the type how to combine the <see cref="SourceColorBlend"/> and <see cref="DestinationColorBlend"/> operation results.
        /// </para>
        /// <para>
        /// The default value is <see cref="BlendOperation.Add"/>.
        /// </para>
        /// </remarks>
        public BlendOperation ColorBlendOperation
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the blending operation to perform.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value specifies the type how to combine the <see cref="SourceAlphaBlend"/> and <see cref="DestinationAlphaBlend"/> operation results.
        /// </para>
        /// <para>
        /// The default value is <see cref="BlendOperation.Add"/>.
        /// </para>
        /// </remarks>
        public BlendOperation AlphaBlendOperation
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the source blending operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the type of operation to apply to the color (RGB) components of a pixel being blended from the source pixel data. 
        /// </para> 
        /// <para>
        /// The default value is <see cref="Blend.One"/>.
        /// </para>
        /// </remarks>
        public Blend SourceColorBlend
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the destination blending operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the type of operation to apply to the color (RGB) components of a pixel being blended with the destination pixel data. 
        /// </para> 
        /// <para>
        /// The default value is <see cref="Blend.Zero"/>.
        /// </para>
        /// </remarks>
        public Blend DestinationColorBlend
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the source blending operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the type of operation to apply to the alpha component of a pixel being blended from the source pixel data. 
        /// </para> 
        /// <para>
        /// The default value is <see cref="Blend.One"/>.
        /// </para>
        /// </remarks>
        public Blend SourceAlphaBlend
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the destination blending operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This defines the type of operation to apply to the alpha component of a pixel being blended with the destination pixel data. 
        /// </para> 
        /// <para>
        /// The default value is <see cref="Blend.Zero"/>.
        /// </para>
        /// </remarks>
        public Blend DestinationAlphaBlend
        {
            get;
            internal set;
        }


        /// <summary>
        /// Property to return the logical operation to apply when blending.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This provides extra functionality used when performing a blending operation. See <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/hh404484(v=vs.85).aspx">this link</a> for more details.
        /// </para>
        /// <para>
        /// The default value is <see cref="Core.LogicOperation.Noop"/>.
        /// </para>
        /// </remarks>
        public LogicOperation LogicOperation
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the flags used to mask which pixel component to write into.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This provides the ability to allow writes to only the specified component(s) defined in the mask. To define multiple components, combine the flags with the OR operator.
        /// </para>
        /// <para>
        /// The default value is <see cref="Core.WriteMask.All"/>.
        /// </para>
        /// </remarks>
        public WriteMask WriteMask
        {
            get;
            internal set;
        }
        #endregion

        #region Methods.
        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="state" /> parameter; otherwise, false.</returns>
        /// <param name="state">An object to compare with this object.</param>
        public bool Equals(GorgonBlendState state)
        {
            return (state == this) || ((state != null)
                                       && (WriteMask == state.WriteMask)
                                       && (AlphaBlendOperation == state.AlphaBlendOperation)
                                       && (ColorBlendOperation == state.ColorBlendOperation)
                                       && (DestinationAlphaBlend == state.DestinationAlphaBlend)
                                       && (DestinationColorBlend == state.DestinationColorBlend)
                                       && (IsBlendingEnabled == state.IsBlendingEnabled)
                                       && (LogicOperation == state.LogicOperation)
                                       && (SourceAlphaBlend == state.SourceAlphaBlend)
                                       && (SourceColorBlend == state.SourceColorBlend));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
        /// </summary>
        /// <param name="state">The state to copy.</param>
        internal GorgonBlendState(GorgonBlendState state)
        {
            WriteMask = state.WriteMask;
            AlphaBlendOperation = state.AlphaBlendOperation;
            ColorBlendOperation = state.ColorBlendOperation;
            DestinationAlphaBlend = state.DestinationAlphaBlend;
            DestinationColorBlend = state.DestinationColorBlend;
            IsBlendingEnabled = state.IsBlendingEnabled;
            LogicOperation = state.LogicOperation;
            SourceAlphaBlend = state.SourceAlphaBlend;
            SourceColorBlend = state.SourceColorBlend;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
        /// </summary>
        internal GorgonBlendState()
        {
            LogicOperation = LogicOperation.Noop;
            SourceAlphaBlend = SourceColorBlend = Blend.One;
            DestinationAlphaBlend = DestinationColorBlend = Blend.Zero;
            AlphaBlendOperation = ColorBlendOperation = BlendOperation.Add;
            WriteMask = WriteMask.All;
        }

        /// <summary>
        /// Initializes static members of the <see cref="GorgonBlendState"/> class.
        /// </summary>
        static GorgonBlendState()
        {
            // Modulated blending.
            Default = new GorgonBlendState
                      {
                          IsBlendingEnabled = true,
                          SourceColorBlend = Blend.SourceAlpha,
                          DestinationColorBlend = Blend.InverseSourceAlpha
                      };


            // Additive
            Additive = new GorgonBlendState
                       {
                           IsBlendingEnabled = true,
                           SourceColorBlend = Blend.SourceAlpha,
                           DestinationColorBlend = Blend.One
                       };

            // Premultiplied
            Premultiplied = new GorgonBlendState
                            {
                                IsBlendingEnabled = true,
                                SourceColorBlend = Blend.One,
                                DestinationColorBlend = Blend.InverseSourceAlpha
                            };

            // Inverted
            Inverted = new GorgonBlendState
                       {
                           IsBlendingEnabled = true,
                           SourceColorBlend = Blend.InverseDestinationColor,
                           DestinationColorBlend = Blend.InverseSourceColor
                       };
        }
        #endregion
    }
}
