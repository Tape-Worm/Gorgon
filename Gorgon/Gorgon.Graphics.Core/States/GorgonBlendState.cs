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
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    #region Enums.
    /// <summary>
    /// Defines the type of logical operations to perform while blending a render target.
    /// </summary>
    [Flags]
    public enum WriteMask
    {
        /// <summary>The red channel will be written.</summary>
        Red = D3D11.ColorWriteMaskFlags.Red,
        /// <summary>The green channel will be written.</summary>
        Green = D3D11.ColorWriteMaskFlags.Green,
        /// <summary>The blue channel will be written.</summary>
        Blue = D3D11.ColorWriteMaskFlags.Blue,
        /// <summary>The alpha channel will be written.</summary>
        Alpha = D3D11.ColorWriteMaskFlags.Alpha,
        /// <summary>All channels will be written.</summary>
        All = D3D11.ColorWriteMaskFlags.All
    }

    /// <summary>
    /// Defines the type of logical operations to perform while blending a render target.
    /// </summary>
    public enum LogicOperation
    {
        /// <summary>
        /// <para>
        /// Clears the render target.
        /// </para>
        /// </summary>
        Clear = D3D11.LogicOperation.Clear,
        /// <summary>
        /// <para>
        /// Sets the render target.
        /// </para>
        /// </summary>
        Set = D3D11.LogicOperation.Set,
        /// <summary>
        /// <para>
        /// Copys the render target.
        /// </para>
        /// </summary>
        Copy = D3D11.LogicOperation.Copy,
        /// <summary>
        /// <para>
        /// Performs an inverted-copy of the render target.
        /// </para>
        /// </summary>
        CopyInverted = D3D11.LogicOperation.CopyInverted,
        /// <summary>
        /// <para>
        /// No operation is performed on the render target.
        /// </para>
        /// </summary>
        Noop = D3D11.LogicOperation.Noop,
        /// <summary>
        /// <para>
        /// Inverts the render target.
        /// </para>
        /// </summary>
        Invert = D3D11.LogicOperation.Invert,
        /// <summary>
        /// <para>
        /// Performs a logical AND operation on the render target.
        /// </para>
        /// </summary>
        And = D3D11.LogicOperation.And,
        /// <summary>
        /// <para>
        /// Performs a logical NAND operation on the render target.
        /// </para>
        /// </summary>
        Nand = D3D11.LogicOperation.Nand,
        /// <summary>
        /// <para>
        /// Performs a logical OR operation on the render target.
        /// </para>
        /// </summary>
        Or = D3D11.LogicOperation.Or,
        /// <summary>
        /// <para>
        /// Performs a logical NOR operation on the render target.
        /// </para>
        /// </summary>
        Nor = D3D11.LogicOperation.Nor,
        /// <summary>
        /// <para>
        /// Performs a logical XOR operation on the render target.
        /// </para>
        /// </summary>
        Xor = D3D11.LogicOperation.Xor,
        /// <summary>
        /// <para>
        /// Performs a logical equal operation on the render target.
        /// </para>
        /// </summary>
        Equiv = D3D11.LogicOperation.Equiv,
        /// <summary>
        /// <para>
        /// Performs a logical AND and reverse operation on the render target.
        /// </para>
        /// </summary>
        AndReverse = D3D11.LogicOperation.AndReverse,
        /// <summary>
        /// <para>
        /// Performs a logical AND and invert operation on the render target.
        /// </para>
        /// </summary>
        AndInverted = D3D11.LogicOperation.AndInverted,
        /// <summary>
        /// <para>
        /// Performs a logical OR and reverse operation on the render target.
        /// </para>
        /// </summary>
        OrReverse = D3D11.LogicOperation.OrReverse,
        /// <summary>
        /// <para>
        /// Performs a logical OR and invert operation on the render target.
        /// </para>
        /// </summary>
        OrInverted = D3D11.LogicOperation.OrInverted
    }

    /// <summary>
    /// Defines the type of operation to perform while blending colors.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum Blend
    {
        /// <summary>
        /// <para>
        /// The blend factor is (0, 0, 0, 0). No pre-blend operation.
        /// </para>
        /// </summary>
        Zero = D3D11.BlendOption.Zero,
        /// <summary>
        /// <para>
        /// The blend factor is (1, 1, 1, 1). No pre-blend operation.
        /// </para>
        /// </summary>
        One = D3D11.BlendOption.One,
        /// <summary>
        /// <para>
        /// The blend factor is (Rₛ, Gₛ, Bₛ, Aₛ), that is color data (RGB) from a pixel shader. No pre-blend operation.
        /// </para>
        /// </summary>
        SourceColor = D3D11.BlendOption.SourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is (1 - Rₛ, 1 - Gₛ, 1 - Bₛ, 1 - Aₛ), that is color data (RGB) from a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB.
        /// </para>
        /// </summary>
        InverseSourceColor = D3D11.BlendOption.InverseSourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is (Aₛ, Aₛ, Aₛ, Aₛ), that is alpha data (A) from a pixel shader. No pre-blend operation.
        /// </para>
        /// </summary>
        SourceAlpha = D3D11.BlendOption.SourceAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is ( 1 - Aₛ, 1 - Aₛ, 1 - Aₛ, 1 - Aₛ), that is alpha data (A) from a pixel shader. The pre-blend operation inverts the data, generating 1 - A.
        /// </para>
        /// </summary>
        InverseSourceAlpha = D3D11.BlendOption.InverseSourceAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is (A A A A), that is alpha data from a render target. No pre-blend operation.
        /// </para>
        /// </summary>
        DestinationAlpha = D3D11.BlendOption.DestinationAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is (1 - A 1 - A 1 - A 1 - A), that is alpha data from a render target. The pre-blend operation inverts the data, generating 1 - A.
        /// </para>
        /// </summary>
        InverseDestinationAlpha = D3D11.BlendOption.InverseDestinationAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is (R, G, B, A), that is color data from a render target. No pre-blend operation.
        /// </para>
        /// </summary>
        DestinationColor = D3D11.BlendOption.DestinationColor,
        /// <summary>
        /// <para>
        /// The blend factor is (1 - R, 1 - G, 1 - B, 1 - A), that is color data from a render target. The pre-blend operation inverts the data, generating 1 - RGB.
        /// </para>
        /// </summary>
        InverseDestinationColor = D3D11.BlendOption.InverseDestinationColor,
        /// <summary>
        /// <para>
        /// The blend factor is (f, f, f, 1); where f = min(Aₛ, 1
        /// </para>
        /// <para>
        /// - A). The pre-blend operation clamps the data to 1 or less.
        /// </para>
        /// </summary>
        SourceAlphaSaturate = D3D11.BlendOption.SourceAlphaSaturate,
        /// <summary>
        /// <para>
        /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState. No pre-blend operation.
        /// </para>
        /// </summary>
        BlendFactor = D3D11.BlendOption.BlendFactor,
        /// <summary>
        /// <para>
        /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState. The pre-blend operation inverts the blend factor, generating 1 - blend_factor.
        /// </para>
        /// </summary>
        InverseBlendFactor = D3D11.BlendOption.InverseBlendFactor,
        /// <summary>
        /// <para>
        /// The blend factor is data sources both as color data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        SecondarySourceColor = D3D11.BlendOption.SecondarySourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is data sources both as color data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        InverseSecondarySourceColor = D3D11.BlendOption.InverseSecondarySourceColor,
        /// <summary>
        /// <para>
        /// The blend factor is data sources as alpha data output by a pixel shader. There is no pre-blend operation. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        SecondarySourceAlpha = D3D11.BlendOption.SecondarySourceAlpha,
        /// <summary>
        /// <para>
        /// The blend factor is data sources as alpha data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - A. This blend factor supports dual-source color blending.
        /// </para>
        /// </summary>
        InverseSecondarySourceAlpha = D3D11.BlendOption.InverseSecondarySourceAlpha
    }

    /// <summary>
    /// Defines the type of operation to perform while blending colors.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum BlendOperation
    {
        /// <summary>
        /// <para>
        /// Add source 1 and source 2.
        /// </para>
        /// </summary>
        Add = D3D11.BlendOperation.Add,
        /// <summary>
        /// <para>
        /// Subtract source 1 from source 2.
        /// </para>
        /// </summary>
        Subtract = D3D11.BlendOperation.Subtract,
        /// <summary>
        /// <para>
        /// Subtract source 2 from source 1.
        /// </para>
        /// </summary>
        ReverseSubtract = D3D11.BlendOperation.ReverseSubtract,
        /// <summary>
        /// <para>
        /// Find the minimum of source 1 and source 2.
        /// </para>
        /// </summary>
        Minimum = D3D11.BlendOperation.Minimum,
        /// <summary>
        /// <para>
        /// Find the maximum of source 1 and source 2.
        /// </para>
        /// </summary>
        Maximum = D3D11.BlendOperation.Maximum
    }
    #endregion

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
        // The blend state ID.
	    private static long _stateID;
        // Flag to indicate whether or not blending is enabled for this state.
	    private bool _isBlendingEnabled;
        // Flag to indicate that a logical operator should be applied when blending.
	    private bool _isLogicalOperationEnabled;
        // The blend operation used on color channels.
	    private BlendOperation _colorBlendOperation;
        // The blend operation used on the alpha channel.
	    private BlendOperation _alphaBlendOperation;
        // The source blending op on color channels.
	    private Blend _sourceColorBlend;
        // The destination blending op on color channels.
	    private Blend _destinationColorBlend;
        // The source blending op on the alpha channel.
	    private Blend _sourceAlphaBlend;
        // The destination blending op on the alpha channel.
	    private Blend _destinationAlphaBlend;
        // The logical operation to perform when blending.
	    private LogicOperation _logicOperation;
        // The mask used to enable/disable channels when writing blended data.
	    private WriteMask _writeMask;
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
        /// Property to return the state ID.
        /// </summary>
	    public long ID
	    {
	        get;
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
	    /// The default value is <see cref="BlendOperation.Add"/>.
	    /// </para>
	    /// </remarks>
	    public BlendOperation ColorBlendOperation
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
	    /// The default value is <see cref="BlendOperation.Add"/>.
	    /// </para>
	    /// </remarks>
	    public BlendOperation AlphaBlendOperation
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
	    /// The default value is <see cref="Blend.One"/>.
	    /// </para>
	    /// </remarks>
	    public Blend SourceColorBlend
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
	    /// The default value is <see cref="Blend.Zero"/>.
	    /// </para>
	    /// </remarks>
	    public Blend DestinationColorBlend
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
	    /// The default value is <see cref="Blend.One"/>.
	    /// </para>
	    /// </remarks>
	    public Blend SourceAlphaBlend
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
	    /// The default value is <see cref="Blend.Zero"/>.
	    /// </para>
	    /// </remarks>
	    public Blend DestinationAlphaBlend
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
	    /// The default value is <see cref="Core.LogicOperation.Noop"/>.
	    /// </para>
	    /// </remarks>
	    public LogicOperation LogicOperation
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
	    /// The default value is <see cref="Core.WriteMask.All"/>.
	    /// </para>
	    /// </remarks>
	    public WriteMask WriteMask
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

        /// <summary>
        /// Function to copy this <see cref="GorgonBlendState"/> into another <see cref="GorgonBlendState"/>.
        /// </summary>
        /// <param name="destState">The state that will receive the contents of this state.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destState"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destState"/> is locked and used by a draw call.</exception>
        public void CopyTo(GorgonBlendState destState)
	    {
	        if (destState == null)
	        {
	            throw new ArgumentNullException(nameof(destState));
	        }

	        destState.CheckLocked();

	        destState._writeMask = _writeMask;
	        destState._alphaBlendOperation = _alphaBlendOperation;
	        destState._colorBlendOperation = _colorBlendOperation;
	        destState._destinationAlphaBlend = _destinationAlphaBlend;
	        destState._destinationColorBlend = _destinationColorBlend;
	        destState._isBlendingEnabled = _isBlendingEnabled;
	        destState._isLogicalOperationEnabled = _isLogicalOperationEnabled;
	        destState._logicOperation = _logicOperation;
	        destState._sourceAlphaBlend = _sourceAlphaBlend;
	        destState._sourceColorBlend = _sourceColorBlend;
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

		    ID = Interlocked.Increment(ref _stateID);
            info.CopyTo(this);
		    IsLocked = false;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
        /// </summary>
        public GorgonBlendState()
        {
            ID = Interlocked.Increment(ref _stateID);
			_logicOperation = LogicOperation.Noop;
			_sourceAlphaBlend = _sourceColorBlend = Blend.One;
			_destinationAlphaBlend = _destinationColorBlend = Blend.Zero;
			_alphaBlendOperation = _colorBlendOperation = BlendOperation.Add;
			_writeMask = WriteMask.All;
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
		                  DestinationColorBlend = Blend.InverseSourceAlpha,
		                  IsLocked = true
		              };


			// Additive
			Additive = new GorgonBlendState
			{
				IsBlendingEnabled = true,
				SourceColorBlend = Blend.SourceAlpha,
				DestinationColorBlend = Blend.One,
			    IsLocked = true
            };

			// Premultiplied
			Premultiplied = new GorgonBlendState
			{
				IsBlendingEnabled = true,
				SourceColorBlend = Blend.One,
				DestinationColorBlend = Blend.InverseSourceAlpha,
			    IsLocked = true
            };

			// Inverted
			Inverted = new GorgonBlendState
			{
				IsBlendingEnabled = true,
				SourceColorBlend = Blend.InverseDestinationColor,
				DestinationColorBlend = Blend.InverseSourceColor,
			    IsLocked = true
            };
		}
		#endregion
	}
}
