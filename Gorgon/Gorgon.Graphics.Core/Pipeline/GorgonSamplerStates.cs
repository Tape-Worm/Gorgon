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
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of texture sampler states to apply to the pipeline.
	/// </summary>
	public sealed class GorgonSamplerStates
		: GorgonResourceBindingList<GorgonSamplerState>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed sampler states that can be bound at the same time.
		/// </summary>
		public const int MaximumSamplerStateCount = D3D11.CommonShaderStage.SamplerSlotCount;
		#endregion		
		
		#region Variables.
		// Actual direct 3D sampler states to bind.
		private D3D11.SamplerState[] _nativeStates;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of actual Direct 3D 11 states.
		/// </summary>
		internal D3D11.SamplerState[] NativeStates => _nativeStates;
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
		protected override void OnSetNativeItem(int index, GorgonSamplerState item)
		{
			_nativeStates[index] = item?.D3DState;
		}

		/// <summary>
		/// Function to clear the list of native binding objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementing class must implement this in order to unassign items from the native binding object list when the <see cref="GorgonResourceBindingList{T}.Clear"/> method is called.
		/// </para>
		/// </remarks>
		protected override void OnClearNativeItems()
		{
			Array.Clear(_nativeStates, 0, _nativeStates.Length);
		}

		/// <summary>
		/// Function to resize the native binding object list if needed.
		/// </summary>
		/// <param name="newSize">The new size for the list.</param>
		/// <remarks>
		/// <para>
		/// This method must be overridden by the implementing class so that the native list is resized along with this list after calling <see cref="GorgonResourceBindingList{T}.Resize"/>.
		/// </para>
		/// </remarks>
		protected override void OnResizeNativeList(int newSize)
		{
			Array.Resize(ref _nativeStates, newSize);
		}

		/// <summary>
		/// Function to set multiple objects of type <see cref="GorgonSamplerState"/> at once.
		/// </summary>
		/// <param name="startSlot">The starting slot to assign.</param>
		/// <param name="samplerStates">The samplers to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="samplerStates"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="samplerStates"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startSlot"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="startSlot"/> plus the number of <paramref name="samplerStates"/> exceeds the size of this list.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <see cref="GorgonSamplerState"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		public void SetRange(int startSlot, IReadOnlyList<GorgonSamplerState> samplerStates, int? count = null)
		{
			if (count == null)
			{
				count = samplerStates?.Count ?? 0;
			}

#if DEBUG
			if (IsLocked)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_ERR_BINDING_LIST_LOCKED);
			}

			if (startSlot < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startSlot));
			}

			if (count.Value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (count + startSlot > MaximumSamplerStateCount)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, startSlot, count.Value, MaximumSamplerStateCount));
			}

			if (count > (samplerStates?.Count ?? 0))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, samplerStates?.Count), nameof(count));
			}
#endif
			// Resize accordingly if we have a mismatch.
			if (count.Value != Count)
			{
				Resize(count.Value);
			}

			if ((samplerStates == null) || (count == 0))
			{
				Clear();
				return;
			}

			for (int i = 0; i < count.Value; ++i)
			{
				this[i + startSlot] = samplerStates[i];
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStates"/> class.
		/// </summary>
		/// <param name="size">The number of shader resources to hold in this list.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// If the <paramref name="size"/> parameter is larger than the maximum allowed number of sampler states, then the size will be adjusted to that maximum instead of the amount requested. See the 
		/// <seealso cref="MaximumSamplerStateCount"/> constant to determine what the maximum is.
		/// </para>
		/// <para>
		/// If the size is omitted, then room is made for 1 sampler state.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For the sake of performance, Exceptions thrown by this constructor will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerStates(int size = 1)
			: base(size, MaximumSamplerStateCount)
		{
			_nativeStates = new D3D11.SamplerState[size];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStates"/> class.
		/// </summary>
		/// <param name="samplerStates">The shader resource views to assign.</param>
		/// <param name="startSlot">[Optional] The starting slot to use for the sampler states.</param>
		/// <exception cref="ArgumentException">Thrown if the number of <paramref name="samplerStates"/> exceeds the <see cref="MaximumSamplerStateCount"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="startSlot"/> is larger than or equal to the number of items in <paramref name="samplerStates"/>, or larger than or equal to the <seealso cref="MaximumSamplerStateCount"/>.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This overload will set many sampler states at once.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For the sake of performance, Exceptions thrown by this constructor will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerStates(IEnumerable<GorgonSamplerState> samplerStates, int startSlot = 0)
			: this()
		{
			SetRange(startSlot, samplerStates.ToArray());
		}
		#endregion
	}
}
