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
// Created: July 9, 2016 3:47:30 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of constant buffers used for shaders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is used to pass a set of <see cref="GorgonConstantBuffer"/> objects to a <see cref="GorgonPipelineResources"/> object. This allows an application to set a single or multiple constant buffers at 
	/// the same time and thus provides a performance boost over setting them individually. 
	/// </para>
	/// </remarks>
	public sealed class GorgonConstantBuffers
		: GorgonMonitoredArray<GorgonConstantBuffer>
	{
		#region Constants.
		/// <summary>
		/// The maximum size for a constant buffer binding list.
		/// </summary>
		public const int MaximumConstantBufferCount = D3D11.CommonShaderStage.ConstantBufferApiSlotCount;
		#endregion

		#region Variables.
		// The native binding.
		private readonly D3D11.Buffer[] _native = new D3D11.Buffer[MaximumConstantBufferCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the list is in a dirty state or not.
		/// </summary>
		internal D3D11.Buffer[] Native => _native;
		#endregion

		#region Methods.
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
			Array.Clear(_native, 0, MaximumConstantBufferCount);
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		protected override void OnItemSet(int index, GorgonConstantBuffer item)
		{
			_native[index] = item.D3DBuffer;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		public GorgonConstantBuffers()
			: base(MaximumConstantBufferCount)
		{
		}
		#endregion
	}
}
