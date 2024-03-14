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
// Created: July 29, 2016 7:31:42 PM
// 
#endregion

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information used to create the stencil portion of a <see cref="GorgonDepthStencilState"/>.
/// </summary>
public class GorgonStencilOperation
    : IEquatable<GorgonStencilOperation>
{
    #region Properties.
    /// <summary>
    /// Property to set or return the comparison function to use for stencil operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This specifies the function to evaluate with stencil data being read/written and existing stencil data.
    /// </para>
    /// <para>
    /// The default value is <see cref="Comparison.Always"/>.
    /// </para>
    /// </remarks>
    public Comparison Comparison
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to set or return the operation to perform when the depth testing function fails, but stencil testing passes.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="StencilOperation.Keep"/>.
    /// </remarks>
    public StencilOperation DepthFailOperation
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to set or return the operation to perform when the stencil testing fails.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="StencilOperation.Keep"/>.
    /// </remarks>
    public StencilOperation FailOperation
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to set or return the operation to perform when the stencil testing passes.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="StencilOperation.Keep"/>.
    /// </remarks>
    public StencilOperation PassOperation
    {
        get;
        internal set;
    }
    #endregion

    #region Methods.
    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(GorgonStencilOperation other) => (other == this) || ((other is not null)
                                   && (Comparison == other.Comparison)
                                   && (DepthFailOperation == other.DepthFailOperation)
                                   && (FailOperation == other.FailOperation)
                                   && (PassOperation == other.PassOperation));

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) => Equals(obj as GorgonStencilOperation);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Comparison, DepthFailOperation, FailOperation, PassOperation);
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStencilOperation"/> class.
    /// </summary>
    /// <param name="state">The state to copy.</param>
    internal GorgonStencilOperation(GorgonStencilOperation state)
    {
        Comparison = state.Comparison;
        DepthFailOperation = state.DepthFailOperation;
        FailOperation = state.FailOperation;
        PassOperation = state.PassOperation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStencilOperation"/> class.
    /// </summary>
    internal GorgonStencilOperation()
    {
        Comparison = Comparison.Always;
        DepthFailOperation = StencilOperation.Keep;
        FailOperation = StencilOperation.Keep;
        PassOperation = StencilOperation.Keep;
    }
    #endregion
}
