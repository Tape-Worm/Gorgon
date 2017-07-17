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
// Created: July 30, 2016 12:21:01 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Describes how rasterized data is blended with a <see cref="GorgonRenderTargetView"/> and how render targets blend with each other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will define how rasterized data is blended with the current render target(s). The ability to disable blending, define how blending operations are performed, etc... are all done through this 
    /// state. This state also defines how blending is performed between adjacent render target(s) in the <see cref="GorgonGraphics.RenderTargets"/>. This is controlled by the 
    /// <see cref="IGorgonPipelineStateInfo.IsIndependentBlendingEnabled"/> flag on the <see cref="IGorgonPipelineStateInfo"/> object.
    /// </para>
    /// <para>
    /// This type is mutable until it is consumed in a <see cref="GorgonPipelineState"/>. After it is assigned, it is immutable and will throw an exception if any changes are made to the state after 
    /// it has been used.
    /// </para>
    /// <para>
    /// Because of the state locking and for performance, it is best practice to pre-create the required states ahead of time.
    /// </para>
    /// <para>
    /// The rasterizer state contains 5 common blend states used by applications: <see cref="Default"/> (blending enabled for the first render target, using modulated blending), <see cref="NoBlending"/> 
    /// (no blending at all), <see cref="Additive"/> (blending enabled on the first render target, using additive ops for source and dest), <see cref="Premultiplied"/> (blending enabled on the first render 
    /// target, with premultiplied blending ops for source and dest), and <see cref="Inverted"/> (blending enabled on the first render target, with inverted ops for source and dest). These states are always 
    /// locked, and cannot be changed, but can be used as a base for a new state by using the <see cref="GorgonBlendState(GorgonBlendState)"/> copy constructor and then modifying as needed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonGraphics.RenderTargets"/>
    /// <seealso cref="IGorgonPipelineStateInfo"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="Default"/>
    /// <seealso cref="NoBlending"/>
    /// <seealso cref="Additive"/>
    /// <seealso cref="Premultiplied"/>
    /// <seealso cref="Inverted"/>
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
		public static readonly GorgonBlendState NoBlending = new GorgonBlendState
		                                                                     {
		                                                                         IsLocked = true
		                                                                     };

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

        #region Variables.
        // Flag to indicate whether or not blending is enabled for this state.
	    private bool _isBlendingEnabled;
        // Flag to indicate that a logical operator should be applied when blending.
	    private bool _isLogicalOperationEnabled;
        // The blend operation used on color channels.
	    private D3D11.BlendOperation _colorBlendOperation;
        // The blend operation used on the alpha channel.
	    private D3D11.BlendOperation _alphaBlendOperation;
        // The source blending op on color channels.
	    private D3D11.BlendOption _sourceColorBlend;
        // The destination blending op on color channels.
	    private D3D11.BlendOption _destinationColorBlend;
        // The source blending op on the alpha channel.
	    private D3D11.BlendOption _sourceAlphaBlend;
        // The destination blending op on the alpha channel.
	    private D3D11.BlendOption _destinationAlphaBlend;
        // The logical operation to perform when blending.
	    private D3D11.LogicOperation _logicOperation;
        // The mask used to enable/disable channels when writing blended data.
	    private D3D11.ColorWriteMaskFlags _writeMask;
	    #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the state is locked or not.
        /// </summary>
	    internal bool IsLocked
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to set or return whether blending should be enabled for this render target.
        /// </summary>
        /// <remarks>
        /// The default value is <b>false</b>.
        /// </remarks>
        public bool IsBlendingEnabled
        {
            get => _isBlendingEnabled;
            set
            {
                CheckLocked();
                _isBlendingEnabled = value;
            }
		}

	    /// <summary>
	    /// Property to set or return whether the logical operation for this blend state is enabled or not.
	    /// </summary>
	    /// <remarks>
	    /// The default value is <b>false</b>.
	    /// </remarks>
	    public bool IsLogicalOperationEnabled
	    {
	        get => _isLogicalOperationEnabled;
	        set
	        {
                CheckLocked();
	            _isLogicalOperationEnabled = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the blending operation to perform.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This value specifies the type how to combine the <see cref="SourceColorBlend"/> and <see cref="DestinationColorBlend"/> operation results.
	    /// </para>
	    /// <para>
	    /// The default value is <c>Add</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.BlendOperation ColorBlendOperation
	    {
	        get => _colorBlendOperation;
	        set
	        {
	            CheckLocked();
                _colorBlendOperation = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the blending operation to perform.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This value specifies the type how to combine the <see cref="SourceAlphaBlend"/> and <see cref="DestinationAlphaBlend"/> operation results.
	    /// </para>
	    /// <para>
	    /// The default value is <c>Add</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.BlendOperation AlphaBlendOperation
	    {
	        get => _alphaBlendOperation;
	        set
	        {
	            CheckLocked();
                _alphaBlendOperation = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the source blending operation.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This defines the type of operation to apply to the color (RGB) components of a pixel being blended from the source pixel data. 
	    /// </para> 
	    /// <para>
	    /// The default value is <c>One</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.BlendOption SourceColorBlend
	    {
	        get => _sourceColorBlend;
	        set
	        {
	            CheckLocked();
                _sourceColorBlend = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the destination blending operation.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This defines the type of operation to apply to the color (RGB) components of a pixel being blended with the destination pixel data. 
	    /// </para> 
	    /// <para>
	    /// The default value is <c>Zero</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.BlendOption DestinationColorBlend
	    {
	        get => _destinationColorBlend;
	        set
	        {
	            CheckLocked();
                _destinationColorBlend = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the source blending operation.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This defines the type of operation to apply to the alpha component of a pixel being blended from the source pixel data. 
	    /// </para> 
	    /// <para>
	    /// The default value is <c>One</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.BlendOption SourceAlphaBlend
	    {
	        get => _sourceAlphaBlend;
	        set
	        {
	            CheckLocked();
                _sourceAlphaBlend = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the destination blending operation.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This defines the type of operation to apply to the alpha component of a pixel being blended with the destination pixel data. 
	    /// </para> 
	    /// <para>
	    /// The default value is <c>Zero</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.BlendOption DestinationAlphaBlend
	    {
	        get => _destinationAlphaBlend;
	        set
	        {
	            CheckLocked();
                _destinationAlphaBlend = value;
	        }
	    }


	    /// <summary>
	    /// Property to set or return the logical operation to apply when blending.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This provides extra functionality used when performing a blending operation. See <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/hh404484(v=vs.85).aspx">this link</a> for more details.
	    /// </para>
	    /// <para>
	    /// The default value is <c>Noop</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.LogicOperation LogicOperation
	    {
	        get => _logicOperation;
	        set
	        {
	            CheckLocked();
                _logicOperation = value;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the flags used to mask which pixel component to write into.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// This provides the ability to allow writes to only the specified component(s) defined in the mask. To define multiple components, combine the flags with the OR operator.
	    /// </para>
	    /// <para>
	    /// The default value is <c>All</c>.
	    /// </para>
	    /// </remarks>
	    public D3D11.ColorWriteMaskFlags WriteMask
	    {
	        get => _writeMask;
	        set
	        {
	            CheckLocked();
                _writeMask = value;
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
        public bool Equals(GorgonBlendState info)
		{
		    return (info == this) || (info != null
		                              && WriteMask == info.WriteMask
		                              && AlphaBlendOperation == info.AlphaBlendOperation
		                              && ColorBlendOperation == info.ColorBlendOperation
		                              && DestinationAlphaBlend == info.DestinationAlphaBlend
		                              && DestinationColorBlend == info.DestinationColorBlend
		                              && IsBlendingEnabled == info.IsBlendingEnabled
		                              && IsLogicalOperationEnabled == info.IsLogicalOperationEnabled
		                              && LogicOperation == info.LogicOperation
		                              && SourceAlphaBlend == info.SourceAlphaBlend
		                              && SourceColorBlend == info.SourceColorBlend);
		}
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
        /// </summary>
        /// <param name="info">A <see cref="GorgonBlendState"/> to copy settings from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonBlendState(GorgonBlendState info)
		{
		    if (info == null)
		    {
		        throw new ArgumentNullException(nameof(info));
		    }

		    _writeMask = info.WriteMask;
		    _alphaBlendOperation = info.AlphaBlendOperation;
		    _colorBlendOperation = info.ColorBlendOperation;
		    _destinationAlphaBlend = info.DestinationAlphaBlend;
		    _destinationColorBlend = info.DestinationColorBlend;
		    _isBlendingEnabled = info.IsBlendingEnabled;
		    _isLogicalOperationEnabled = info.IsLogicalOperationEnabled;
		    _logicOperation = info.LogicOperation;
		    _sourceAlphaBlend = info.SourceAlphaBlend;
		    _sourceColorBlend = info.SourceColorBlend;
		    IsLocked = false;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
        /// </summary>
        public GorgonBlendState()
		{
			_logicOperation = D3D11.LogicOperation.Noop;
			_sourceAlphaBlend = _sourceColorBlend = D3D11.BlendOption.One;
			_destinationAlphaBlend = _destinationColorBlend = D3D11.BlendOption.Zero;
			_alphaBlendOperation = _colorBlendOperation = D3D11.BlendOperation.Add;
			_writeMask = D3D11.ColorWriteMaskFlags.All;
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
		                  SourceColorBlend = D3D11.BlendOption.SourceAlpha,
		                  DestinationColorBlend = D3D11.BlendOption.InverseSourceAlpha,
		                  IsLocked = true
		              };


			// Additive
			Additive = new GorgonBlendState
			{
				IsBlendingEnabled = true,
				SourceColorBlend = D3D11.BlendOption.SourceAlpha,
				DestinationColorBlend = D3D11.BlendOption.One,
			    IsLocked = true
            };

			// Premultiplied
			Premultiplied = new GorgonBlendState
			{
				IsBlendingEnabled = true,
				SourceColorBlend = D3D11.BlendOption.One,
				DestinationColorBlend = D3D11.BlendOption.InverseSourceAlpha,
			    IsLocked = true
            };

			// Inverted
			Inverted = new GorgonBlendState
			{
				IsBlendingEnabled = true,
				SourceColorBlend = D3D11.BlendOption.InverseDestinationColor,
				DestinationColorBlend = D3D11.BlendOption.InverseSourceColor,
			    IsLocked = true
            };
		}
		#endregion
	}
}
