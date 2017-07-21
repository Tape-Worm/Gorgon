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
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
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
    /// This type is mutable until it is consumed in a <see cref="GorgonPipelineState"/>. After it is assigned, it is immutable and will throw an exception if any changes are made to the state after 
    /// it has been used.
    /// </para>
    /// <para>
    /// Because of the state locking and for performance, it is best practice to pre-create the required states ahead of time.
    /// </para>
    /// <para>
    /// The depth/stencil state contains 6 common depth/stencil states used by applications: <see cref="Default"/> (disabled depth/stencil), <see cref="DepthStencilEnabled"/> (both depth and stencil 
    /// enabled, full write enabled), <see cref="DepthStencilEnabledNoWrite"/> (both depth and stencil enabled, no writing allowed), <see cref="DepthEnabledNoWrite"/> (depth enabled, no writing allowed), 
    /// <see cref="DepthEnabled"/> (depth enabled, stencil disabled), and <see cref="StencilEnabled"/> (stencil enabled, depth disabled). These states are always locked, and cannot be changed, but can be 
    /// used as a base for a new state by using the <see cref="GorgonDepthStencilState(GorgonDepthStencilState)"/> copy constructor and then modifying as needed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="Default"/>
    /// <seealso cref="DepthStencilEnabled"/>
    /// <seealso cref="DepthStencilEnabledNoWrite"/>
    /// <seealso cref="DepthEnabledNoWrite"/>
    /// <seealso cref="DepthEnabled"/>
    /// <seealso cref="StencilEnabled"/>
	public class GorgonDepthStencilState 
		: IEquatable<GorgonDepthStencilState>
	{
		#region Common States.
		/// <summary>
		/// The default depth/stencil state.
		/// </summary>
		public static readonly GorgonDepthStencilState Default = new GorgonDepthStencilState
		                                                         {
		                                                             IsLocked = true
		                                                         };

	    /// <summary>
	    /// Depth/stencil enabled.
	    /// </summary>
	    public static readonly GorgonDepthStencilState DepthStencilEnabled = new GorgonDepthStencilState
	                                                                         {
	                                                                             IsDepthEnabled = true,
	                                                                             IsStencilEnabled = true,
	                                                                             IsDepthWriteEnabled = true,
	                                                                             IsLocked = true
	                                                                         };

	    /// <summary>
	    /// Depth/stencil enabled, depth write disbled.
	    /// </summary>
	    public static readonly GorgonDepthStencilState DepthStencilEnabledNoWrite = new GorgonDepthStencilState
	                                                                                {
	                                                                                    IsDepthEnabled = true,
	                                                                                    IsStencilEnabled = true,
	                                                                                    IsDepthWriteEnabled = false,
	                                                                                    IsLocked = true
	                                                                                };

	    /// <summary>
	    /// Depth only enabled.
	    /// </summary>
	    public static readonly GorgonDepthStencilState DepthEnabled = new GorgonDepthStencilState
	                                                                  {
	                                                                      IsDepthEnabled = true,
	                                                                      IsDepthWriteEnabled = true,
	                                                                      IsLocked = true
	                                                                  };

	    /// <summary>
	    /// Depth only enabled, depth write disabled.
	    /// </summary>
	    public static readonly GorgonDepthStencilState DepthEnabledNoWrite = new GorgonDepthStencilState
	                                                                         {
	                                                                             IsDepthEnabled = true,
	                                                                             IsDepthWriteEnabled = false,
	                                                                             IsLocked = true
	                                                                         };

	    /// <summary>
	    /// Stencil only enabled.
	    /// </summary>
	    public static readonly GorgonDepthStencilState StencilEnabled = new GorgonDepthStencilState
	                                                                    {
	                                                                        IsStencilEnabled = true,
	                                                                        IsDepthWriteEnabled = true,
	                                                                        IsLocked = true
	                                                                    };
        #endregion

        #region Variables.
	    // The state ID.
	    private static long _stateID;
        // The type of comparison to perform for depth testing.
        private D3D11.Comparison _depthComparison;
        // Flag to indicate that depth writing is enabled.
	    private bool _isDepthWriteEnabled;
        // Flag to indicate that the depth buffer is enabled.
	    private bool _isDepthEnabled;
        // Flag to indicate that the stencil buffer is enabled.
	    private bool _isStencilEnabled;
        // The stencil reading mask.
	    private byte _stencilReadMask;
        // The stencil writing mask.
	    private byte _stencilWriteMask;
	    #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether this state is locked or not.
        /// </summary>
        internal bool IsLocked
	    {
	        get;
	        set;
	    }
        
	    /// <summary>
	    /// Property to return the state ID.
	    /// </summary>
	    public long ID
	    {
	        get;
	    }

        /// <summary>
        /// Property to set or return the depth comparison function.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this property to determine whether a depth value will be written into the buffer if the function specified evaluates to true using the data being written and existing data.
        /// </para>
        /// <para>
        /// The default value is <c>Less</c>.
        /// </para>
        /// </remarks>
        public D3D11.Comparison DepthComparison
		{
		    get => _depthComparison;
		    set
		    {
		        CheckLocked();
		        _depthComparison = value;
		    }
		}

	    /// <summary>
	    /// Property to set or return whether to enable writing to the depth buffer or not.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <c>true</c>.
	    /// </remarks>
	    public bool IsDepthWriteEnabled
	    {
	        get => _isDepthWriteEnabled;
	        set
	        {
                CheckLocked();
	            _isDepthWriteEnabled = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return whether the depth buffer is enabled or not.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <b>false</b>.
	    /// </remarks>
	    public bool IsDepthEnabled
	    {
	        get => _isDepthEnabled;
	        set
	        {
	            CheckLocked();
                _isDepthEnabled = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return whether the stencil buffer is enabled or not.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <b>false</b>.
	    /// </remarks>
	    public bool IsStencilEnabled
	    {
	        get => _isStencilEnabled;
	        set
	        {
	            CheckLocked();
                _isStencilEnabled = value;
	        }
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
	        get => _stencilReadMask;
	        set
	        {
	            CheckLocked();
                _stencilReadMask = value;
	        }
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
	        get => _stencilWriteMask;
	        set
	        {
	            CheckLocked();
                _stencilWriteMask = value;
	        }
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
	    /// Function to determine if the state is locked for read-only access.
	    /// </summary>
	    private void CheckLocked()
	    {
	        if (IsLocked)
	        {
	            throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_STATE_IMMUTABLE, GetType().Name));
	        }
	    }

        /// <summary>
        /// Function to copy this <see cref="GorgonDepthStencilState"/> into another <see cref="GorgonDepthStencilState"/>.
        /// </summary>
        /// <param name="destState">The state that will receive the contents of this state.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destState"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destState"/> is locked and used by a draw call.</exception>
	    public void CopyTo(GorgonDepthStencilState destState)
	    {
	        if (destState == null)
	        {
	            throw new ArgumentNullException(nameof(destState));
	        }

            destState.CheckLocked();

	        BackFaceStencilOp.CopyTo(destState.BackFaceStencilOp);
	        FrontFaceStencilOp.CopyTo(destState.FrontFaceStencilOp);
	        destState._depthComparison = DepthComparison;
	        destState._isDepthWriteEnabled = IsDepthWriteEnabled;
	        destState._isDepthEnabled = IsDepthEnabled;
	        destState._isStencilEnabled = IsStencilEnabled;
	        destState._stencilReadMask = StencilReadMask;
	        destState._stencilWriteMask = StencilWriteMask;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="info" /> parameter; otherwise, false.</returns>
        /// <param name="info">An object to compare with this object.</param>
        public bool Equals(GorgonDepthStencilState info)
		{
		    return (info == this) || (info != null
		                              && BackFaceStencilOp.Equals(info.BackFaceStencilOp)
		                              && FrontFaceStencilOp.Equals(info.FrontFaceStencilOp)
		                              && DepthComparison == info.DepthComparison
		                              && IsDepthEnabled == info.IsDepthEnabled
		                              && IsDepthWriteEnabled == info.IsDepthWriteEnabled
		                              && IsStencilEnabled == info.IsStencilEnabled
		                              && StencilReadMask == info.StencilReadMask
		                              && StencilWriteMask == info.StencilWriteMask);
		}
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDepthStencilState"/> class.
        /// </summary>
        /// <param name="info">A <see cref="GorgonDepthStencilState"/> to copy the settings from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonDepthStencilState(GorgonDepthStencilState info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

		    ID = Interlocked.Increment(ref _stateID);
		    BackFaceStencilOp = new GorgonStencilOperation(this);
		    FrontFaceStencilOp = new GorgonStencilOperation(this);
            
            info.CopyTo(this);
		    IsLocked = false;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDepthStencilState"/> class.
        /// </summary>
        public GorgonDepthStencilState()
        {
            ID = Interlocked.Increment(ref _stateID);
			_isDepthWriteEnabled = true;
			_depthComparison = D3D11.Comparison.Less;
			_stencilReadMask = 0xff;
			_stencilWriteMask = 0xff;
			BackFaceStencilOp = new GorgonStencilOperation(this);
			FrontFaceStencilOp = new GorgonStencilOperation(this);
		}
		#endregion
	}
}
