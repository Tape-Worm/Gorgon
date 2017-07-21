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

using System;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Information used to create the stencil portion of a <see cref="GorgonDepthStencilState"/>.
	/// </summary>
	public class GorgonStencilOperation 
		: IEquatable<GorgonStencilOperation>
	{
        #region Variables.
        // The depth/stencil state that owns this type.
	    private readonly GorgonDepthStencilState _depthStencilState;
        // The type of stencil comparison to perform.
	    private D3D11.Comparison _comparison;
        // The type of operation to perform when a depth test fails.
	    private D3D11.StencilOperation _depthFailOperation;
        // The type of operation to perform when a stencil test fails.
	    private D3D11.StencilOperation _failOperation;
        // The type of operation to perform when a stencil test passes.
	    private D3D11.StencilOperation _passOperation;
	    #endregion

        #region Properties.
	    /// <summary>
	    /// Property to return whether this operation state is locked or not.
	    /// </summary>
	    internal bool IsLocked => _depthStencilState.IsLocked;

	    /// <summary>
	    /// Property to set or return the comparison function to use for stencil operations.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This specifies the function to evaluate with stencil data being read/written and existing stencil data.
	    /// </para>
	    /// <para>
	    /// The default value is <c>Keep</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.Comparison Comparison
	    {
	        get => _comparison;
	        set
	        {
                CheckLocked();
	            _comparison = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the operation to perform when the depth testing function fails, but stencil testing passes.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <c>Keep</c>.
	    /// </remarks>
	    public D3D11.StencilOperation DepthFailOperation
	    {
	        get => _depthFailOperation;
	        set
	        {
                CheckLocked();
	            _depthFailOperation = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the operation to perform when the stencil testing fails.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <c>Keep</c>.
	    /// </remarks>
	    public D3D11.StencilOperation FailOperation
	    {
	        get => _failOperation;
	        set
	        {
                CheckLocked();
	            _failOperation = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the operation to perform when the stencil testing passes.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <c>Keep</c>.
	    /// </remarks>
	    public D3D11.StencilOperation PassOperation
	    {
	        get => _passOperation;
	        set
	        {
                CheckLocked();
	            _passOperation = value;
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
	            throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_STATE_IMMUTABLE, _depthStencilState.GetType().Name + "." + GetType().Name));
	        }
	    }

        /// <summary>
        /// Function to copy the constants of this <see cref="GorgonStencilOperation"/> to another one.
        /// </summary>
        /// <param name="destOp">The stencil operation that will recieve the contents of this stencil operation.</param>
	    internal void CopyTo(GorgonStencilOperation destOp)
	    {
	        destOp._comparison = Comparison;
	        destOp._depthFailOperation = DepthFailOperation;
	        destOp._failOperation = FailOperation;
	        destOp._passOperation = PassOperation;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(GorgonStencilOperation other)
        {
            return (other == this) || (other != null
                                       && Comparison == other.Comparison
                                       && DepthFailOperation == other.DepthFailOperation
                                       && FailOperation == other.FailOperation
                                       && PassOperation == other.PassOperation);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStencilOperation"/> class.
        /// </summary>
        /// <param name="parent">The parent depth/stencil state for this operation state.</param>
        /// <param name="opInfo">The operation information to copy.</param>
        internal GorgonStencilOperation(GorgonDepthStencilState parent, GorgonStencilOperation opInfo = null)
        {
            _depthStencilState = parent;

		    if (opInfo == null)
		    {
		        _comparison = D3D11.Comparison.Always;
		        _failOperation = _passOperation = _depthFailOperation = D3D11.StencilOperation.Keep;
		        return;
		    }

            opInfo.CopyTo(this);
        }
        #endregion
    }
}
