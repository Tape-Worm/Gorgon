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
// Created: July 4, 2016 1:05:13 AM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of texture sampler states to apply to the pipeline.
	/// </summary>
	public sealed class GorgonSamplerStates
		: GorgonMonitoredArray<GorgonSamplerState>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed sampler states that can be bound at the same time.
		/// </summary>
		public const int MaximumSamplerStateCount = D3D11.CommonShaderStage.SamplerSlotCount;
		#endregion

		#region Variables.
		// The native resource bindings.
		private readonly D3D11.SamplerState[] _native = new D3D11.SamplerState[MaximumSamplerStateCount];
		#endregion


		#region Properties.
		/// <summary>
		/// Propetry to return the native sampler states.
		/// </summary>
		internal D3D11.SamplerState[] Native => _native;
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		/// <remarks>
		/// <para>
		/// Implementors must override this method to assign the native version of the object to bind. 
		/// </para>
		/// </remarks>
		protected override void OnItemSet(int index, GorgonSamplerState item)
		{
			_native[index] = item.D3DState;
		}

		/// <summary>
		/// Function to clear the list of native binding objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementing class must implement this in order to unassign items from the native binding object list when the <see cref="GorgonMonitoredArray{T}.Clear"/> method is called.
		/// </para>
		/// </remarks>
		protected override void OnClear()
		{
			Array.Clear(_native, 0, MaximumSamplerStateCount);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStates"/> class.
		/// </summary>
		public GorgonSamplerStates()
			: base(MaximumSamplerStateCount)
		{
		}
		#endregion
	}
}
