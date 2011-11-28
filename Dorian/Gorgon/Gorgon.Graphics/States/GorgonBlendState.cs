#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, November 28, 2011 6:08:01 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Blending operations.
	/// </summary>
	public enum BlendOperation
	{
		///<summary>
		///Add source 1 and source 2.
		///</summary>
		Add = 1,
		///<summary>
		///Subtract source 1 from source 2.
		///</summary>
		Subtract = 2,
		///<summary>
		///Subtract source 2 from source 1.
		///</summary>
		ReverseSubtract = 3,
		///<summary>
		///Find the minimum of source 1 and source 2.
		///</summary>
		Minimum = 4,
		///<summary>
		///Find the maximum of source 1 and source 2.
		///</summary>
		Maximum = 5
	}

	/// <summary>
	/// Blending operation types for alpha/color channels.
	/// </summary>
	public enum BlendType
	{
		/// <summary>
		/// The data source is the color black (0, 0, 0, 0). No pre-blend operation.
		/// </summary>
		Zero = 1,
		/// <summary>
		/// The data source is the color white (1, 1, 1, 1). No pre-blend operation.
		/// </summary>
		One = 2,
		/// <summary>
		/// The data source is color data (RGB) from a pixel shader. No pre-blend operation
		/// </summary>
		SourceColor = 3,
		/// <summary>
		/// The data source is color data (RGB) from a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB.
		/// </summary>
		InverseSourceColor = 4,
		/// <summary>
		/// The data source is alpha data (A) from a pixel shader. No pre-blend operation.
		/// </summary>
		SourceAlpha = 5,
		/// <summary>
		/// The data source is alpha data (A) from a pixel shader. The pre-blend operation inverts the data, generating 1 - A.
		/// </summary>
		InverseSourceAlpha = 6,
		/// <summary>
		/// The data source is alpha data from a rendertarget. No pre-blend operation.
		/// </summary>
		DestinationAlpha = 7,
		/// <summary>
		/// The data source is alpha data from a rendertarget. The pre-blend operation inverts the data, generating 1 - A.
		/// </summary>
		InverseDestinationAlpha = 8,
		/// <summary>
		/// The data source is color data from a rendertarget. No pre-blend operation.
		/// </summary>
		DestinationColor = 9,
		/// <summary>
		/// The data source is color data from a rendertarget. The pre-blend operation inverts the data, generating 1 - RGB.
		/// </summary>
		InverseDestinationColor = 10,
		/// <summary>
		/// The data source is alpha data from a pixel shader. The pre-blend operation clamps the data to 1 or less.
		/// </summary>
		SourceAlphaSaturate = 11,
		/// <summary>
		/// The data source is the blend factor set with the blend state object. No pre-blend operation.
		/// </summary>
		BlendFactor = 14,
		/// <summary>
		/// The data source is the blend factor set with blend state object. The pre-blend operation inverts the blend factor, generating 1 - blend_factor.
		/// </summary>
		InverseBlendFactor = 15,
		/// <summary>
		/// The data sources are both color data output by a pixel shader. There is no pre-blend operation. This options supports dual-source color blending.
		/// </summary>
		SecondarySourceColor = 16,
		/// <summary>
		/// The data sources are both color data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - RGB. This options supports dual-source color blending.
		/// </summary>
		InverseSecondarySourceColor = 17,
		/// <summary>
		/// The data sources are alpha data output by a pixel shader. There is no pre-blend operation. This options supports dual-source color blending.
		/// </summary>
		SecondarySourceAlpha = 18,
		/// <summary>
		/// The data sources are alpha data output by a pixel shader. The pre-blend operation inverts the data, generating 1 - A. This options supports dual-source color blending.
		/// </summary>
		InverseSecondarySourceAlpha = 19
	}

	/// <summary>
	/// Flags to identify a specific channel(s) to perform the blending operations on.
	/// </summary>
	[Flags]
	public enum ColorWriteMaskFlags
	{
		/// <summary>
		/// Use the red channel.
		/// </summary>
		Red = 1,
		/// <summary>
		/// Use the green channel.
		/// </summary>
		Green = 2,
		/// <summary>
		/// Use the blue channel.
		/// </summary>
		Blue = 4,
		/// <summary>
		/// Use the alpha channel.
		/// </summary>
		Alpha = 8,
		/// <summary>
		/// Use all channels.
		/// </summary>
		All = 15,
	}

	/// <summary>
	/// Blending state.
	/// </summary>
	/// <remarks>This is used to control how polygons are blended in a scene.</remarks>
	public class GorgonBlendState
		: GorgonStateObject<D3D.BlendState>
	{
		#region Classes.
		/// <summary>
		/// Blending state for an individual render target.
		/// </summary>
		public class RenderTargetBlendState
		{
			#region Variables.
			private GorgonBlendState _blend = null;
			private bool _isBlendingEnabled = false;
			private BlendOperation _blendOperation = BlendOperation.Add;
			private BlendOperation _alphaOperation = BlendOperation.Add;
			private BlendType _sourceBlend = BlendType.One;
			private BlendType _destinationBlend = BlendType.Zero;
			private BlendType _sourceAlphaBlend = BlendType.One;
			private BlendType _destinationAlphaBlend = BlendType.Zero;
			private ColorWriteMaskFlags _writeMask = ColorWriteMaskFlags.All;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return whether blending should be enabled for this render target or not.
			/// </summary>
			/// <remarks>The default value is FALSE.</remarks>
			public bool IsBlendingEnabled
			{
				get
				{
					return _isBlendingEnabled;
				}
				set
				{
					if (_isBlendingEnabled != value)
					{
						_isBlendingEnabled = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the alpha blending operation to perform.
			/// </summary>
			/// <remarks>This defines how the source and destination alpha channels will blend together.
			/// <para>The default value is Add.</para>
			/// </remarks>
			public BlendOperation AlphaOperation
			{
				get
				{
					return _alphaOperation;
				}
				set
				{
					if (_alphaOperation != value)
					{
						_alphaOperation = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the blending operation to perform.
			/// </summary>
			/// <remarks>This defines how the source and destination color channels will blend together.
			/// <para>The default value is Add.</para>
			/// </remarks>
			public BlendOperation BlendingOperation
			{
				get
				{
					return _blendOperation;
				}
				set
				{
					if (_blendOperation != value)
					{
						_blendOperation = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the color blending type for the source.
			/// </summary>
			/// <remarks>This defines the operation to perform on color data.
			/// <para>The default value is One.</para>
			/// </remarks>
			public BlendType SourceBlend
			{
				get
				{
					return _sourceBlend;
				}
				set
				{
					if (_sourceBlend != value)
					{
						_sourceBlend = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the color blending type for the destination.
			/// </summary>
			/// <remarks>This defines the operation to perform on color data.
			/// <para>The default value is Zero.</para>
			/// </remarks>
			public BlendType DestinationBlend
			{
				get
				{
					return _destinationBlend;
				}
				set
				{
					if (_destinationBlend != value)
					{
						_destinationBlend = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the alpha blending type for the source.
			/// </summary>
			/// <remarks>This defines the operation to perform on alpha data.
			/// <para>The default value is One.</para>
			/// </remarks>
			public BlendType SourceAlphaBlend
			{
				get
				{
					return _sourceBlend;
				}
				set
				{
					if (_sourceAlphaBlend != value)
					{
						_sourceAlphaBlend = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the alpha blending type for the destination.
			/// </summary>
			/// <remarks>This defines the operation to perform on alpha data.
			/// <para>The default value is Zero.</para>
			/// </remarks>
			public BlendType DestinationAlphaBlend
			{
				get
				{
					return _destinationBlend;
				}
				set
				{
					if (_destinationAlphaBlend != value)
					{
						_destinationAlphaBlend = value;
						_blend.HasChanged = true;
					}
				}
			}

			/// <summary>
			/// Property to set or return the channels to use when blending.
			/// </summary>
			/// <remarks>The default value is All.</remarks>
			public ColorWriteMaskFlags WriteMask
			{
				get
				{
					return _writeMask;
				}
				set
				{
					if (_writeMask != value)
					{
						_writeMask = value;
						_blend.HasChanged = true;
					}
				}
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="RenderTargetBlendState"/> struct.
			/// </summary>
			/// <param name="state">The state object that owns this value.</param>
			internal RenderTargetBlendState(GorgonBlendState state)
			{
				_blend = state;
			}
			#endregion			
		}
		#endregion

		#region Variables.
		private bool _disposed = false;												// Flag to indicate that the object was disposed of.
		private bool _alphaCoverageEnabled = false;									// Flag to indicate that alpha coverage was enabled.
		private bool _independentBlend = false;										// Flag to indicate that independent blending is enabled.
		private ReadOnlyCollection<RenderTargetBlendState> _targetStates = null;	// Render target states.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether alpha-to-coverage is enabled or not.
		/// </summary>
		/// <remarks>This is a multisample techique that smooths out the transparent edges of polygons with alpha blending.
		/// <para>Please note that this is only available for devices that have a feature level of SM_4 and above.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool IsAlphaCoverageEnabled
		{
			get
			{
				return _alphaCoverageEnabled;
			}
			set
			{
				if ((_alphaCoverageEnabled != value) && 
					(((Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM5) == DeviceFeatureLevel.SM5) || 
					((Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM5) == DeviceFeatureLevel.SM4) ||
					((Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM5) == DeviceFeatureLevel.SM4_1)))
				{
					_alphaCoverageEnabled = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether independent blending is enabled or not.
		/// </summary>
		/// <remarks>When this value is TRUE This allows for each render target to have its own blending settings.  When it is set to FALSE, it will only use the blending settings of the 
		/// first target in the array.
		/// <para>The default value is FALSE.</para>
		/// </remarks>		
		public bool IsIndependentBlendEnabled
		{
			get
			{
				return _independentBlend;
			}
			set
			{
				if (_independentBlend != value)
				{
					HasChanged = true;
					_independentBlend = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return the blending factor.
		/// </summary>
		/// <remarks>This is used when the source/destination blending type is set to use the blending factor.</remarks>
		public GorgonColor BlendFactor
		{
			get
			{
				return new GorgonColor(Graphics.Context.OutputMerger.BlendFactor.Alpha, Graphics.Context.OutputMerger.BlendFactor.Red, Graphics.Context.OutputMerger.BlendFactor.Green, Graphics.Context.OutputMerger.BlendFactor.Blue);
			}
			set
			{
				Graphics.Context.OutputMerger.BlendFactor = new SharpDX.Color4(value.Red, value.Green, value.Blue, value.Alpha);
			}
		}

		/// <summary>
		/// Property to set or return the blending sample mask.
		/// </summary>
		/// <remarks>A sample mask determines which samples get updated in all the active render targets. The mapping of bits in a sample mask to samples in a multisample render target is the responsibility of an individual application. A sample mask is always applied; it is independent of whether multisampling is enabled, and does not depend on whether an application uses multisample render targets.</remarks>
		public uint BlendSampleMask
		{
			get
			{
				return (uint)Graphics.Context.OutputMerger.BlendSampleMask;
			}
			set
			{
				Graphics.Context.OutputMerger.BlendSampleMask = (int)value;
			}
		}

		/// <summary>
		/// Property to return a list of individual render target blending states.
		/// </summary>
		public ReadOnlyCollection<RenderTargetBlendState> RenderTarget
		{
			get
			{
				return _targetStates;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (Graphics.BlendingState == this)
						Graphics.BlendingState = null;
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to convert this blend state into a Direct3D blend state.
		/// </summary>
		/// <returns>The Direct3D blend state.</returns>
		protected internal override D3D.BlendState Convert()
		{			
			if (HasChanged)
			{
				D3D.BlendStateDescription desc = new D3D.BlendStateDescription();

				if (State != null)
					State.Dispose();
				State = null;

				desc.AlphaToCoverageEnable = _alphaCoverageEnabled;
				desc.IndependentBlendEnable = _independentBlend;

				for (int i = 0; i < desc.RenderTarget.Length; i++)
				{
					desc.RenderTarget[i].AlphaBlendOperation = (D3D.BlendOperation)RenderTarget[i].AlphaOperation;
					desc.RenderTarget[i].BlendOperation = (D3D.BlendOperation)RenderTarget[i].BlendingOperation;
					desc.RenderTarget[i].IsBlendEnabled = RenderTarget[i].IsBlendingEnabled;
					desc.RenderTarget[i].DestinationAlphaBlend = (D3D.BlendOption)RenderTarget[i].DestinationAlphaBlend;
					desc.RenderTarget[i].DestinationBlend = (D3D.BlendOption)RenderTarget[i].DestinationBlend;
					desc.RenderTarget[i].RenderTargetWriteMask = (D3D.ColorWriteMaskFlags)RenderTarget[i].WriteMask;
					desc.RenderTarget[i].SourceAlphaBlend = (D3D.BlendOption)RenderTarget[i].SourceAlphaBlend;
					desc.RenderTarget[i].SourceBlend = (D3D.BlendOption)RenderTarget[i].SourceBlend;
				}

				State = new D3D.BlendState(Graphics.VideoDevice.D3DDevice, desc);

				HasChanged = false;
			}

			return State;
		}
		#endregion

		#region Method.
		/// <summary>
		/// Function to apply any changes immediately if this state is the current state.
		/// </summary>
		protected override void ApplyImmediate()
		{
			if ((Graphics != null) && (Graphics.BlendingState == this))
				Graphics.Context.OutputMerger.BlendState = Convert();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBlendState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonBlendState(GorgonGraphics graphics)
			: base(graphics)
		{
			RenderTargetBlendState[] states = new RenderTargetBlendState[8];

			for (int i = 0; i < states.Length; i++)
				states[i] = new RenderTargetBlendState(this);

			_targetStates = new ReadOnlyCollection<RenderTargetBlendState>(states);
		}
		#endregion
	}
}

