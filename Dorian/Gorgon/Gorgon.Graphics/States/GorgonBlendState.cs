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
using GorgonLibrary.Diagnostics;

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

	#region Value Types.
	/// <summary>
	/// Blending state for an individual render target.
	/// </summary>
	public struct GorgonRenderTargetBlendState
		: IEquatableByRef<GorgonRenderTargetBlendState>
	{
		#region Variables.
		/// <summary>
		/// Default render target blending states.
		/// </summary>
		public static readonly GorgonRenderTargetBlendState DefaultStates = new GorgonRenderTargetBlendState()
		{
			IsBlendingEnabled = false,
			AlphaOperation = BlendOperation.Add,
			BlendingOperation = BlendOperation.Add,
			SourceBlend = BlendType.One,
			DestinationBlend = BlendType.Zero,
			SourceAlphaBlend = BlendType.One,
			DestinationAlphaBlend = BlendType.Zero,
			WriteMask = ColorWriteMaskFlags.All
		};
		#endregion

		#region Properties.
		/// <summary>
		/// Is blending enabled for this render target or not.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsBlendingEnabled;

		/// <summary>
		/// The alpha blending operation to perform.
		/// </summary>
		/// <remarks>This defines how the source and destination alpha channels will blend together.
		/// <para>The default value is Add.</para>
		/// </remarks>
		public BlendOperation AlphaOperation;

		/// <summary>
		/// The blending operation to perform.
		/// </summary>
		/// <remarks>This defines how the source and destination color channels will blend together.
		/// <para>The default value is Add.</para>
		/// </remarks>
		public BlendOperation BlendingOperation;

		/// <summary>
		/// The color blending type for the source.
		/// </summary>
		/// <remarks>This defines the operation to perform on color data.
		/// <para>The default value is One.</para>
		/// </remarks>
		public BlendType SourceBlend;

		/// <summary>
		/// The color blending type for the destination.
		/// </summary>
		/// <remarks>This defines the operation to perform on color data.
		/// <para>The default value is Zero.</para>
		/// </remarks>
		public BlendType DestinationBlend;

		/// <summary>
		/// The alpha blending type for the source.
		/// </summary>
		/// <remarks>This defines the operation to perform on alpha data.
		/// <para>The default value is One.</para>
		/// </remarks>
		public BlendType SourceAlphaBlend;

		/// <summary>
		/// The alpha blending type for the destination.
		/// </summary>
		/// <remarks>This defines the operation to perform on alpha data.
		/// <para>The default value is Zero.</para>
		/// </remarks>
		public BlendType DestinationAlphaBlend;

		/// <summary>
		/// The channels to use when blending.
		/// </summary>
		/// <remarks>The default value is All.</remarks>
		public ColorWriteMaskFlags WriteMask;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert this render target blend state to a D3D blend description.
		/// </summary>
		/// <returns>The D3D render target blend description.</returns>
		internal D3D.RenderTargetBlendDescription Convert()
		{
			D3D.RenderTargetBlendDescription desc = new D3D.RenderTargetBlendDescription();

			desc.AlphaBlendOperation = (D3D.BlendOperation)AlphaOperation;
			desc.BlendOperation = (D3D.BlendOperation)BlendingOperation;
			desc.IsBlendEnabled = IsBlendingEnabled;
			desc.DestinationAlphaBlend = (D3D.BlendOption)DestinationAlphaBlend;
			desc.DestinationBlend = (D3D.BlendOption)DestinationBlend;
			desc.RenderTargetWriteMask = (D3D.ColorWriteMaskFlags)WriteMask;
			desc.SourceAlphaBlend = (D3D.BlendOption)SourceAlphaBlend;
			desc.SourceBlend = (D3D.BlendOption)SourceBlend;

			return desc;
		}

		/// <summary>
		/// Function to compare two render target blend states for equality.
		/// </summary>
		/// <param name="left">Left render target state to compare.</param>
		/// <param name="right">Right render target state to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonRenderTargetBlendState left, ref GorgonRenderTargetBlendState right)
		{
			return ((left.AlphaOperation == right.AlphaOperation) && (left.BlendingOperation == right.BlendingOperation) && (left.DestinationAlphaBlend == right.DestinationAlphaBlend) &&
					(left.DestinationBlend == right.DestinationBlend) && (left.IsBlendingEnabled == right.IsBlendingEnabled) && (left.SourceAlphaBlend == right.SourceAlphaBlend) &&
					(left.SourceBlend == right.SourceBlend) && (left.WriteMask == right.WriteMask));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonRenderTargetBlendState)
				return Equals((GorgonRenderTargetBlendState)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return 281.GenerateHash(AlphaOperation).
						GenerateHash(BlendingOperation).
						GenerateHash(DestinationBlend).
						GenerateHash(DestinationAlphaBlend).
						GenerateHash(IsBlendingEnabled).
						GenerateHash(SourceAlphaBlend).
						GenerateHash(SourceBlend).
						GenerateHash(WriteMask);
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonRenderTargetBlendState left, GorgonRenderTargetBlendState right)
		{
			return GorgonRenderTargetBlendState.Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonRenderTargetBlendState left, GorgonRenderTargetBlendState right)
		{
			return !GorgonRenderTargetBlendState.Equals(ref left, ref right);
		}
		#endregion

		#region IEquatable<BlendStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonRenderTargetBlendState other)
		{
			return GorgonRenderTargetBlendState.Equals(ref this, ref other);
		}
		#endregion

		#region IEquatableByRef<GorgonRenderTargetBlendState> Members
		/// <summary>
		/// Function to compare this instance with another.
		/// </summary>
		/// <param name="other">The other instance to use for comparison.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(ref GorgonRenderTargetBlendState other)
		{
			return GorgonRenderTargetBlendState.Equals(ref this, ref other);			
		}
		#endregion
	}

	/// <summary>
	/// Blending states used to determine how to blend pixels together.
	/// </summary>
	public struct GorgonBlendStates
		: IEquatableByRef<GorgonBlendStates>
	{
		#region Variables.		
		/// <summary>
		/// Default blending states.
		/// </summary>
		public static readonly GorgonBlendStates DefaultStates = new GorgonBlendStates()
		{
			IsAlphaCoverageEnabled = false,
			IsIndependentBlendEnabled = false,
			RenderTarget0 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget1 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget2 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget3 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget4 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget5 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget6 = GorgonRenderTargetBlendState.DefaultStates,
			RenderTarget7 = GorgonRenderTargetBlendState.DefaultStates
		};

		/// <summary>
		/// Is alpha-to-coverage is enabled or not.
		/// </summary>
		/// <remarks>This is a multisample techique that smooths out the transparent edges of polygons with alpha blending.
		/// <para>Please note that this is only available for devices that have a feature level of SM_4 and above.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool IsAlphaCoverageEnabled;

		/// <summary>
		/// Is independent blending is enabled or not.
		/// </summary>
		/// <remarks>When this value is TRUE This allows for each render target to have its own blending settings.  When it is set to FALSE, it will only use the blending settings of the 
		/// first target in the array.
		/// <para>The default value is FALSE.</para>
		/// </remarks>		
		public bool IsIndependentBlendEnabled;

		/// <summary>
		/// Blend states for render target 0.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget0;

		/// <summary>
		/// Blend states for render target 1.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget1;

		/// <summary>
		/// Blend states for render target 2.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget2;

		/// <summary>
		/// Blend states for render target 3.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget3;

		/// <summary>
		/// Blend states for render target 4.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget4;

		/// <summary>
		/// Blend states for render target 5.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget5;

		/// <summary>
		/// Blend states for render target 6.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget6;

		/// <summary>
		/// Blend states for render target 7.
		/// </summary>
		public GorgonRenderTargetBlendState RenderTarget7;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare and return whether two belnd states are equal or not.
		/// </summary>
		/// <param name="left">Left blend state to compare.</param>
		/// <param name="right">Right blend state to compare.</param>
		/// <returns></returns>
		public static bool Equals(ref GorgonBlendStates left, ref GorgonBlendStates right)
		{
			return ((left.IsAlphaCoverageEnabled == right.IsAlphaCoverageEnabled) && (left.IsIndependentBlendEnabled == right.IsIndependentBlendEnabled) &&
					(GorgonRenderTargetBlendState.Equals(ref left.RenderTarget0, ref right.RenderTarget0)) && (GorgonRenderTargetBlendState.Equals(ref left.RenderTarget1, ref right.RenderTarget1)) &&
					(GorgonRenderTargetBlendState.Equals(ref left.RenderTarget2, ref right.RenderTarget2)) && (GorgonRenderTargetBlendState.Equals(ref left.RenderTarget3, ref right.RenderTarget3)) &&
					(GorgonRenderTargetBlendState.Equals(ref left.RenderTarget4, ref right.RenderTarget4)) && (GorgonRenderTargetBlendState.Equals(ref left.RenderTarget5, ref right.RenderTarget5)) &&
					(GorgonRenderTargetBlendState.Equals(ref left.RenderTarget6, ref right.RenderTarget6)) && (GorgonRenderTargetBlendState.Equals(ref left.RenderTarget7, ref right.RenderTarget7)));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonBlendStates)
				return Equals((GorgonBlendStates)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				// Pick an arbitrary prime.
				return 281.GenerateHash(IsAlphaCoverageEnabled).
							GenerateHash(IsIndependentBlendEnabled).
							GenerateHash(RenderTarget0).
							GenerateHash(RenderTarget1).
							GenerateHash(RenderTarget2).
							GenerateHash(RenderTarget3).
							GenerateHash(RenderTarget4).
							GenerateHash(RenderTarget5).
							GenerateHash(RenderTarget6).
							GenerateHash(RenderTarget7);
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonBlendStates left, GorgonBlendStates right)
		{
			return GorgonBlendStates.Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonBlendStates left, GorgonBlendStates right)
		{
			return !GorgonBlendStates.Equals(ref left, ref right);
		}
		#endregion

		#region IEquatable<BlendStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonBlendStates other)
		{
			return GorgonBlendStates.Equals(ref this, ref other);
		}
		#endregion

		#region IEquatableByRef<GorgonBlendStates> Members
		/// <summary>
		/// Function to compare this instance with another.
		/// </summary>
		/// <param name="other">The other instance to use for comparison.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(ref GorgonBlendStates other)
		{
			return GorgonBlendStates.Equals(ref this, ref other);		
		}
		#endregion
	}
	#endregion
		
	/// <summary>
	/// The blending render state.
	/// </summary>
	/// <remarks>This is used to control how polygons are blended in a scene.</remarks>
	public class GorgonBlendRenderState
		: GorgonState<GorgonBlendStates>
	{
		#region Variables.
		private GorgonColor _blendFactor = new GorgonColor(0.0f, 0.0f, 0.0f, 0.0f);		// Blend factor.
		private uint _sampleMask = 0xFFFFFFFF;											// Sample mask.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the blending factor.
		/// </summary>
		/// <remarks>This is used when the source/destination blending type is set to use the blending factor.</remarks>
		public GorgonColor BlendFactor
		{
			get
			{
#if DEBUG
				if (Graphics.Context == null)
					throw new InvalidOperationException("A valid context could not be found.");
#endif
				return _blendFactor;
			}
			set
			{
				if (_blendFactor != value)
				{
					_blendFactor = value;
					Graphics.Context.OutputMerger.BlendFactor = value.SharpDXColor4;
				}
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
#if DEBUG
				if (Graphics.Context == null)
					throw new InvalidOperationException("A valid context could not be found.");
#endif
				return _sampleMask;
			}
			set
			{
				if (_sampleMask != value)
				{				
					_sampleMask = value;
					Graphics.Context.OutputMerger.BlendSampleMask = (int)_sampleMask;
				}
			}
		}
		#endregion

		#region Methods.		
		/// <summary>
		/// Function to apply the state to the current rendering context.
		/// </summary>
		/// <param name="stateObject">State to apply.</param>
		internal override void ApplyState(D3D.DeviceChild stateObject)
		{
			Graphics.Context.OutputMerger.BlendState = (D3D.BlendState)stateObject;
		}

		/// <summary>
		/// Function to retrieve the D3D state object.
		/// </summary>
		/// <param name="stateType">The state type information.</param>
		/// <returns>The D3D state object.</returns>
		internal override D3D.DeviceChild GetStateObject(ref GorgonBlendStates stateType)
		{
			D3D.BlendStateDescription desc = new D3D.BlendStateDescription();

			if ((desc.AlphaToCoverageEnable) && (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b))
				throw new GorgonException(GorgonResult.CannotBind, "Cannot bind the blending state.  Alpha to coverage is only available on SM 4.x or better video devices.");

			desc.AlphaToCoverageEnable = stateType.IsAlphaCoverageEnabled;
			desc.IndependentBlendEnable = stateType.IsIndependentBlendEnabled;

			// Copy render targets.
			desc.RenderTarget[0] = stateType.RenderTarget0.Convert();
			desc.RenderTarget[1] = stateType.RenderTarget1.Convert();
			desc.RenderTarget[2] = stateType.RenderTarget2.Convert();
			desc.RenderTarget[3] = stateType.RenderTarget3.Convert();
			desc.RenderTarget[4] = stateType.RenderTarget4.Convert();
			desc.RenderTarget[5] = stateType.RenderTarget5.Convert();
			desc.RenderTarget[6] = stateType.RenderTarget6.Convert();
			desc.RenderTarget[7] = stateType.RenderTarget7.Convert();

			D3D.BlendState state = new D3D.BlendState(Graphics.D3DDevice, desc);
			state.DebugName = "Gorgon Blend State #" + StateCacheCount.ToString();

			return state;
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBlendRenderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonBlendRenderState(GorgonGraphics graphics)
			: base(graphics)
		{
			States = GorgonBlendStates.DefaultStates;
		}
		#endregion
	}
}

