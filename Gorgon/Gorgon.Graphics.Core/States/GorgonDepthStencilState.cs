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
// Created: July 28, 2016 11:49:51 PM
// 
#endregion

using System;
using Gorgon.Core;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Describes how rasterized primitive data is clipped against a depth/stencil buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will define how rasterized primitive data is clipped against a depth/stencil buffer. Depth reading, writing, and stencil operations are affected by this state.
    /// </para>
    /// <para>
    /// The depth/stencil state contains 6 common depth/stencil states used by applications: <see cref="Default"/> (disabled depth/stencil), <see cref="DepthStencilEnabled"/> (both depth and stencil 
    /// enabled, full write enabled), <see cref="DepthStencilEnabledNoWrite"/> (both depth and stencil enabled, no writing allowed), <see cref="DepthEnabledNoWrite"/> (depth enabled, no writing allowed), 
    /// <see cref="DepthEnabled"/> (depth enabled, stencil disabled), and <see cref="StencilEnabled"/> (stencil enabled, depth disabled). 
    /// </para>
    /// <para>
    /// A depth/stencil state is an immutable object, and as such can only be created by using a <see cref="GorgonDepthStencilStateBuilder"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonDepthStencilStateBuilder"/>
    public class GorgonDepthStencilState
        : IEquatable<GorgonDepthStencilState>
    {
        #region Common States.
        /// <summary>
        /// The default depth/stencil state.
        /// </summary>
        public static readonly GorgonDepthStencilState Default = new();

        /// <summary>
        /// Depth/stencil enabled.
        /// </summary>
        public static readonly GorgonDepthStencilState DepthStencilEnabled = new()
        {
            IsDepthEnabled = true,
            IsStencilEnabled = true,
            IsDepthWriteEnabled = true
        };

        /// <summary>
        /// Depth/stencil enabled, depth write disbled.
        /// </summary>
        public static readonly GorgonDepthStencilState DepthStencilEnabledNoWrite = new()
        {
            IsDepthEnabled = true,
            IsStencilEnabled = true,
            IsDepthWriteEnabled = false
        };

        /// <summary>
        /// Depth only enabled.
        /// </summary>
        public static readonly GorgonDepthStencilState DepthEnabled = new()
        {
            IsDepthEnabled = true,
            IsDepthWriteEnabled = true
        };

        /// <summary>
        /// Depth only enabled, depth write disabled.
        /// </summary>
        public static readonly GorgonDepthStencilState DepthEnabledNoWrite = new()
        {
            IsDepthEnabled = true,
            IsDepthWriteEnabled = false
        };

        /// <summary>
        /// Depth/stencil enabled. With a comparison of less than or equal for the depth buffer.
        /// </summary>
        /// <remarks>
        /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
        /// </remarks>
        public static readonly GorgonDepthStencilState DepthLessEqualStencilEnabled = new()
        {
            IsDepthEnabled = true,
            IsStencilEnabled = true,
            IsDepthWriteEnabled = true,
            DepthComparison = Comparison.LessEqual
        };

        /// <summary>
        /// Depth/stencil enabled, depth write disbled. With a comparison of less than or equal for the depth buffer.
        /// </summary>
        /// <remarks>
        /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
        /// </remarks>
        public static readonly GorgonDepthStencilState DepthLessEqualStencilEnabledNoWrite = new()
        {
            IsDepthEnabled = true,
            IsStencilEnabled = true,
            IsDepthWriteEnabled = false,
            DepthComparison = Comparison.LessEqual
        };

        /// <summary>
        /// Depth only enabled. With a comparison of less than or equal for the depth buffer.
        /// </summary>
        /// <remarks>
        /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
        /// </remarks>
        public static readonly GorgonDepthStencilState DepthLessEqualEnabled = new()
        {
            IsDepthEnabled = true,
            IsDepthWriteEnabled = true,
            DepthComparison = Comparison.LessEqual
        };

        /// <summary>
        /// Depth only enabled, depth write disabled. With a comparison of less than or equal for the depth buffer.
        /// </summary>
        /// <remarks>
        /// This value is suitable for 2D operations because sprites don't have any depth, and not having an equals operator will cause overlapping sprites to overwrite instead of merge together.
        /// </remarks>
        public static readonly GorgonDepthStencilState DepthLessEqualEnabledNoWrite = new()
        {
            IsDepthEnabled = true,
            IsDepthWriteEnabled = false,
            DepthComparison = Comparison.LessEqual
        };

        /// <summary>
        /// Stencil only enabled.
        /// </summary>
        public static readonly GorgonDepthStencilState StencilEnabled = new()
        {
            IsStencilEnabled = true,
            IsDepthWriteEnabled = true
        };

        /// <summary>
        /// Depth/stencil enabled, with a greater than or equal comparer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is useful for reversing a depth distribution across the depth buffer to provide better accuracy. See 
        /// <a href="https://developer.nvidia.com/content/depth-precision-visualized">https://developer.nvidia.com/content/depth-precision-visualized</a> for more information.
        /// </para>
        /// </remarks>
        public static readonly GorgonDepthStencilState DepthStencilEnabledGreaterEqual = new()
        {
            IsDepthWriteEnabled = true,
            IsDepthEnabled = true,
            IsStencilEnabled = true,
            DepthComparison = Comparison.GreaterEqual
        };

        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the depth comparison function.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this property to determine whether a depth value will be written into the buffer if the function specified evaluates to true using the data being written and existing data.
        /// </para>
        /// <para>
        /// The default value is <see cref="Comparison.Less"/>.
        /// </para>
        /// </remarks>
        public Comparison DepthComparison
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to set or return whether to enable writing to the depth buffer or not.
        /// </summary>
        /// <remarks>
        /// The default value is <c>true</c>.
        /// </remarks>
        public bool IsDepthWriteEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to set or return whether the depth buffer is enabled or not.
        /// </summary>
        /// <remarks>
        /// The default value is <b>false</b>.
        /// </remarks>
        public bool IsDepthEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to set or return whether the stencil buffer is enabled or not.
        /// </summary>
        /// <remarks>
        /// The default value is <b>false</b>.
        /// </remarks>
        public bool IsStencilEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to set or return the read mask for stencil operations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this to filter out specific values from the stencil buffer during a read operation.
        /// </para>
        /// <para>
        /// The default value is <c>0xff</c>.
        /// </para>
        /// </remarks>
        public byte StencilReadMask
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to set or return the write mask for stencil operations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this to filter out specific values from the stencil buffer during a write operation.
        /// </para>
        /// <para>
        /// The default value is <c>0xff</c>.
        /// </para>
        /// </remarks>
        public byte StencilWriteMask
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the setup information for stencil operations on front facing polygons.
        /// </summary>
        public GorgonStencilOperation FrontFaceStencilOp
        {
            get;
        }

        /// <summary>
        /// Property to return the setup information for stencil operations on back facing polygons.
        /// </summary>
        public GorgonStencilOperation BackFaceStencilOp
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build the Direct3D 11 depth/stencil state.
        /// </summary>
        /// <param name="device">The device object used to create the state.</param>
        /// <returns>The D3D11 depth/stencil state.</returns>
        internal D3D11.DepthStencilState GetD3D11DepthStencilState(D3D11.Device5 device)
        {
            var desc = new D3D11.DepthStencilStateDescription
            {
                BackFace = new D3D11.DepthStencilOperationDescription
                {
                    DepthFailOperation = (D3D11.StencilOperation)BackFaceStencilOp.DepthFailOperation,
                    Comparison = (D3D11.Comparison)BackFaceStencilOp.Comparison,
                    PassOperation = (D3D11.StencilOperation)BackFaceStencilOp.PassOperation,
                    FailOperation = (D3D11.StencilOperation)BackFaceStencilOp.FailOperation
                },
                FrontFace = new D3D11.DepthStencilOperationDescription
                {
                    DepthFailOperation = (D3D11.StencilOperation)FrontFaceStencilOp.DepthFailOperation,
                    Comparison = (D3D11.Comparison)FrontFaceStencilOp.Comparison,
                    PassOperation = (D3D11.StencilOperation)FrontFaceStencilOp.PassOperation,
                    FailOperation = (D3D11.StencilOperation)FrontFaceStencilOp.FailOperation
                },
                DepthComparison = (D3D11.Comparison)DepthComparison,
                DepthWriteMask = IsDepthWriteEnabled ? D3D11.DepthWriteMask.All : D3D11.DepthWriteMask.Zero,
                IsDepthEnabled = IsDepthEnabled,
                IsStencilEnabled = IsStencilEnabled,
                StencilReadMask = StencilReadMask,
                StencilWriteMask = StencilWriteMask
            };

            return new D3D11.DepthStencilState(device, desc)
            {
                DebugName = nameof(GorgonDepthStencilState)
            };
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="info" /> parameter; otherwise, false.</returns>
        /// <param name="info">An object to compare with this object.</param>
        public bool Equals(GorgonDepthStencilState info) => (info == this) || ((info is not null)
                                      && (BackFaceStencilOp.Equals(info.BackFaceStencilOp))
                                      && (FrontFaceStencilOp.Equals(info.FrontFaceStencilOp))
                                      && (DepthComparison == info.DepthComparison)
                                      && (IsDepthEnabled == info.IsDepthEnabled)
                                      && (IsDepthWriteEnabled == info.IsDepthWriteEnabled)
                                      && (IsStencilEnabled == info.IsStencilEnabled)
                                      && (StencilReadMask == info.StencilReadMask)
                                      && (StencilWriteMask == info.StencilWriteMask));

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) => Equals(obj as GorgonDepthStencilState);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => 281.GenerateHash(BackFaceStencilOp)
            .GenerateHash(FrontFaceStencilOp)
            .GenerateHash(DepthComparison)
            .GenerateHash(IsDepthEnabled)
            .GenerateHash(IsDepthWriteEnabled)
            .GenerateHash(IsStencilEnabled)
            .GenerateHash(StencilReadMask)
            .GenerateHash(StencilWriteMask);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDepthStencilState"/> class.
        /// </summary>
        /// <param name="state">A <see cref="GorgonDepthStencilState"/> to copy the settings from.</param>
        internal GorgonDepthStencilState(GorgonDepthStencilState state)
        {
            BackFaceStencilOp = new GorgonStencilOperation(state.BackFaceStencilOp);
            FrontFaceStencilOp = new GorgonStencilOperation(state.FrontFaceStencilOp);
            DepthComparison = state.DepthComparison;
            IsDepthWriteEnabled = state.IsDepthWriteEnabled;
            IsDepthEnabled = state.IsDepthEnabled;
            IsStencilEnabled = state.IsStencilEnabled;
            StencilReadMask = state.StencilReadMask;
            StencilWriteMask = state.StencilWriteMask;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDepthStencilState"/> class.
        /// </summary>
        internal GorgonDepthStencilState()
        {
            IsDepthWriteEnabled = true;
            DepthComparison = Comparison.Less;
            StencilReadMask = 0xff;
            StencilWriteMask = 0xff;
            BackFaceStencilOp = new GorgonStencilOperation();
            FrontFaceStencilOp = new GorgonStencilOperation();
        }
        #endregion
    }
}
