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
// Created: Monday, December 19, 2011 9:43:57 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Texture addressing flags.
	/// </summary>
	public enum TextureAddressing
	{
		/// <summary>
		/// Wrap and repeat the texture at the texture boundary.
		/// </summary>
		Wrap = 1,
		/// <summary>
		/// Wrap and mirror the texture at the texture boundary.
		/// </summary>
		Mirror = 2,
		/// <summary>
		/// Clamp the texture at the texture boundary.
		/// </summary>
		Clamp = 3,
		/// <summary>
		/// Assign a border color at the texture boundary.
		/// </summary>
		Border = 4,
		/// <summary>
		/// Wrap and mirror the texture only once at the texture boundary.
		/// </summary>
		MirrorOnce = 5
	}

	/// <summary>
	/// Filtering to apply to a texture.
	/// </summary>
	[Flags()]
	public enum TextureFilter
	{
		/// <summary>
		/// Point minification filtering.
		/// </summary>
		MinPoint = 1,
		/// <summary>
		/// Point magnifcation filtering.
		/// </summary>
		MagPoint = 2,
		/// <summary>
		/// Linear minification filtering.
		/// </summary>
		MinLinear = 4,
		/// <summary>
		/// Linear magnifcation filtering.
		/// </summary>
		MagLinear = 8,
		/// <summary>
		/// Mip map point sampling.
		/// </summary>
		MipPoint = 16,
		/// <summary>
		/// Mip map linear sampling.
		/// </summary>
		MipLinear = 32,
		/// <summary>
		/// Compare the result to the comparison value.
		/// </summary>
		Comparison = 64,
		/// <summary>
		/// Linear filtering.
		/// </summary>
		Linear = MinLinear | MagLinear | MipLinear,
		/// <summary>
		/// Point filtering.
		/// </summary>
		Point = MinPoint | MagPoint | MipPoint,
		/// <summary>
		/// Point filtering with comparison.
		/// </summary>
		ComparePoint = MinPoint | MagPoint | MipPoint | Comparison,
		/// <summary>
		/// Linear filtering with comparison.
		/// </summary>
		CompareLinear = MinLinear | MagLinear | MipLinear | Comparison,
		/// <summary>
		/// 1 bit texture for text.
		/// </summary>
		Text1Bit = 32768,
		/// <summary>
		/// Anisotropic filtering.
		/// </summary>
		/// <remarks>This flag is mutually exclusive and applies to minification, magnification and mip mapping.</remarks>
		Anisotropic = 65536,
		/// <summary>
		/// Anisotropic filtering with comparison.
		/// </summary>
		/// <remarks>This flag is mutually exclusive and applies to minification, magnification and mip mapping.</remarks>
		CompareAnisotropic = 131072,
	}

	/// <summary>
	/// States for the texture samplers.
	/// </summary>
	public struct GorgonTextureSamplerStates
		: IEquatable<GorgonTextureSamplerStates>
	{
		#region Variables.
		/// <summary>
		/// Default texture sampler states.
		/// </summary>
		public static readonly GorgonTextureSamplerStates DefaultStates = new GorgonTextureSamplerStates()
		{
			TextureFilter = TextureFilter.Linear,
			HorizontalAddressing = TextureAddressing.Clamp,
			VerticalAddressing = TextureAddressing.Clamp,
			DepthAddressing = TextureAddressing.Clamp,
			MipLODBias = 0.0f,
			MaxAnisotropy = 1,
			ComparisonFunction = ComparisonOperators.Never,
			BorderColor = System.Drawing.Color.White,
			MinLOD = -3.402823466e+38f,
			MaxLOD = 3.402823466e+38f
		};


		/// <summary>
		/// Filtering to apply to the texture.
		/// </summary>
		public TextureFilter TextureFilter;
		/// <summary>
		/// Horizontal texture addressing (U).
		/// </summary>
		public TextureAddressing HorizontalAddressing;
		/// <summary>
		/// Vertical texture addressing (V).
		/// </summary>
		public TextureAddressing VerticalAddressing;
		/// <summary>
		/// Depth texture addressing (W).
		/// </summary>
		public TextureAddressing DepthAddressing;
		/// <summary>
		/// Offset from the calculated mipmap level.
		/// </summary>
		public float MipLODBias;
		/// <summary>
		/// Clamping value used when texture filtering is set to anisotropic.
		/// </summary>
		/// <remarks>This value should only be from 1 to 16.</remarks>
		public int MaxAnisotropy;
		/// <summary>
		/// Function to compare sampled data against existing data.
		/// </summary>
		public ComparisonOperators ComparisonFunction;
		/// <summary>
		/// Color used when the horizontal, vertical or depth addressing are set to border.
		/// </summary>
		public GorgonColor BorderColor;
		/// <summary>
		/// Lowest level of detail to use in the mip map range.
		/// </summary>
		public float MinLOD;
		/// <summary>
		/// Highest level of detail to use int he mip map range.
		/// </summary>
		public float MaxLOD;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two texture sampler states are equal or not.
		/// </summary>
		/// <param name="left">Left side to evaluate.</param>
		/// <param name="right">Right side to evaluate.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonTextureSamplerStates left, ref GorgonTextureSamplerStates right)
		{
			return ((left.BorderColor == right.BorderColor) && (left.ComparisonFunction == right.ComparisonFunction) && (left.DepthAddressing == right.DepthAddressing) &&
				(left.HorizontalAddressing == right.HorizontalAddressing) && (left.MaxAnisotropy == right.MaxAnisotropy) && (Math.GorgonMathUtility.EqualFloat(left.MaxLOD, right.MaxLOD)) &&
				(Math.GorgonMathUtility.EqualFloat(left.MinLOD, right.MinLOD)) && (Math.GorgonMathUtility.EqualFloat(left.MipLODBias, right.MipLODBias)) && (left.TextureFilter == right.TextureFilter) &&
				(left.VerticalAddressing == right.VerticalAddressing));
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
			if (obj is GorgonTextureSamplerStates)
				return Equals((GorgonTextureSamplerStates)obj);

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
				return 281.GenerateHash(BorderColor).GenerateHash(ComparisonFunction).GenerateHash(DepthAddressing).
					GenerateHash(HorizontalAddressing).GenerateHash(MaxAnisotropy).GenerateHash(MaxLOD).
					GenerateHash(MinLOD).GenerateHash(MipLODBias).GenerateHash(TextureFilter).GenerateHash(VerticalAddressing);
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
		public static bool operator ==(GorgonTextureSamplerStates left, GorgonTextureSamplerStates right)
		{
			return GorgonTextureSamplerStates.Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonTextureSamplerStates left, GorgonTextureSamplerStates right)
		{
			return !GorgonTextureSamplerStates.Equals(ref left, ref right);
		}
		#endregion

		#region IEquatable<GorgonTextureSamplerStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the other parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonTextureSamplerStates other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}

	/// <summary>
	/// Blending state.
	/// </summary>
	/// <remarks>This is used to control how polygons are blended in a scene.</remarks>
	public class GorgonTextureSamplerState
		: GorgonStateObjects<GorgonTextureSamplerStates>
	{
		#region Variables.
		private D3D.CommonShaderStage _shaderStage = null;		// Direct 3D shader stage.
		private D3D.SamplerState[] _states = null;				// Sampler states.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply the specified state.
		/// </summary>
		/// <param name="index">Index of the slot for the state to apply.</param>
		/// <param name="state">State to apply.</param>
		protected override void ApplyState(int index, IDisposable state)
		{
			_shaderStage.SetSampler(index, (D3D.SamplerState)state);
		}

		/// <summary>
		/// Function to convert this blend state into a Direct3D blend state.
		/// </summary>
		/// <param name="states">States being converted.</param>
		/// <returns>The Direct3D blend state.</returns>
		protected override IDisposable Convert(GorgonTextureSamplerStates states)
		{
			D3D.SamplerStateDescription desc = new D3D.SamplerStateDescription();

			desc.AddressU = (D3D.TextureAddressMode)states.HorizontalAddressing;
			desc.AddressV = (D3D.TextureAddressMode)states.VerticalAddressing;
			desc.AddressW = (D3D.TextureAddressMode)states.DepthAddressing;
			desc.BorderColor = new SharpDX.Color4(states.BorderColor.Red, states.BorderColor.Green, states.BorderColor.Blue, states.BorderColor.Alpha);
			desc.ComparisonFunction = (D3D.Comparison)states.ComparisonFunction;
			desc.MaximumAnisotropy = states.MaxAnisotropy;
			desc.MaximumLod = states.MaxLOD;
			desc.MinimumLod = states.MinLOD;
			desc.MipLodBias = states.MipLODBias;


			if (states.TextureFilter == TextureFilter.Anisotropic)
				desc.Filter = D3D.Filter.Anisotropic;
			if (states.TextureFilter == TextureFilter.CompareAnisotropic)
				desc.Filter = D3D.Filter.ComparisonAnisotropic;

			// Sort out filter states.
			// Check comparison states.
			if ((states.TextureFilter & TextureFilter.Comparison) == TextureFilter.Comparison)
			{
				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.ComparisonMinMagMipLinear;
				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.ComparisonMinMagMipPoint;

				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.ComparisonMinMagLinearMipPoint;
				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.ComparisonMinLinearMagMipPoint;
				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.ComparisonMinLinearMagPointMipLinear;

				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.ComparisonMinPointMagMipLinear;
				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.ComparisonMinMagPointMipLinear;
				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.ComparisonMinPointMagLinearMipPoint;
			}
			else
			{
				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.MinMagMipLinear;
				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.MinMagMipPoint;

				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.MinMagLinearMipPoint;
				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.MinLinearMagMipPoint;
				if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.MinLinearMagPointMipLinear;

				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.MinPointMagMipLinear;
				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
					desc.Filter = D3D.Filter.MinMagPointMipLinear;
				if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
					desc.Filter = D3D.Filter.MinPointMagLinearMipPoint;
			}

			D3D.SamplerState state = new D3D.SamplerState(Graphics.D3DDevice, desc);
			state.DebugName = "Gorgon Sampler State #" + StateCacheCount.ToString();

			return state;
		}

		/// <summary>
		/// Function to set a range of states at once.
		/// </summary>
		/// <param name="slot">Starting slot for the states.</param>
		/// <param name="states">States to set.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of state slots.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="states"/> count + the slot is greater than or equal to the number of available state slots.</para>
		/// </exception>
		public void SetRange(int slot, IEnumerable<GorgonTextureSamplerStates> states)
		{
			int count = 0;

			GorgonDebug.AssertNull<IEnumerable<GorgonTextureSamplerStates>>(states, "states");
#if DEBUG
			if ((slot < 0) || (slot >= SlotCount) || ((slot + states.Count()) >= SlotCount))
				throw new ArgumentOutOfRangeException("Cannot have more than " + SlotCount.ToString() + " slots occupied.");
#endif

			count = states.Count();
			for (int i = 0; i < count; i++)
			{
				var state = states.ElementAtOrDefault(i);
				_states[i + slot] = (D3D.SamplerState)GetState(i, state);
			}

			_shaderStage.SetSamplers(slot, count, _states);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBlendRenderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="shaderStage">D3D common shader stage.</param>
		internal GorgonTextureSamplerState(GorgonGraphics graphics, D3D.CommonShaderStage shaderStage)
			: base(graphics, D3D.CommonShaderStage.SamplerSlotCount, 4096, Int32.MaxValue)
		{
			_shaderStage = shaderStage;
			_states = new D3D.SamplerState[D3D.CommonShaderStage.SamplerSlotCount];
		}
		#endregion
	}
}
