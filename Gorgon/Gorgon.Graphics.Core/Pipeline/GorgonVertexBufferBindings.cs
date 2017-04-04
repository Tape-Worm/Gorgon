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
// Created: July 26, 2016 10:35:58 PM
// 
#endregion


using System;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of <see cref="GorgonVertexBufferBinding"/> values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="GorgonVertexBufferBinding"/> is used to bind a vertex buffer to the GPU pipeline so that it may be used for rendering.
	/// </para>
	/// </remarks>
	public sealed class GorgonVertexBufferBindings
		: GorgonMonitoredValueTypeArray<GorgonVertexBufferBinding>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of vertex buffers allow to be bound at the same time.
		/// </summary>
		public const int MaximumVertexBufferCount = D3D11.InputAssemblerStage.VertexInputResourceSlotCount;
		#endregion

		#region Variables.
		// The native bindings.
		private readonly D3D11.VertexBufferBinding[] _nativeBindings;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the native items wrapped by this list.
		/// </summary>
		internal D3D11.VertexBufferBinding[] Native => _nativeBindings;

		/// <summary>
		/// Property to return the input layout assigned to the buffer bindings.
		/// </summary>
		/// <remarks>
		/// The input layout defines how the vertex data is arranged within the vertex buffers.
		/// </remarks>
		public GorgonInputLayout InputLayout
		{
			get;
			internal set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when an item is assigned to an index.
		/// </summary>
		/// <param name="index">The index of the item that was assigned.</param>
		/// <param name="value">The value that was assigned.</param>
		protected override void OnItemSet(int index, GorgonVertexBufferBinding value)
		{
			_nativeBindings[index] = value.ToVertexBufferBinding();
		}

		/// <summary>
		/// Function called when the array is cleared.
		/// </summary>
		protected override void OnClear()
		{
			Array.Clear(_nativeBindings, 0, _nativeBindings.Length);
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		/// <param name="inputLayout">The input layout that describes the arrangement of the vertex data within the buffers being bound.</param>
		/// <param name="size">The number of vertex buffers to store in this list.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="inputLayout"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> is larger than the <see cref="MaximumVertexBufferCount"/>.</exception>
		public GorgonVertexBufferBindings(GorgonInputLayout inputLayout, int size)
			: base(size)
		{
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}

			if (size > MaximumVertexBufferCount)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ARG_OUT_OF_RANGE, MaximumVertexBufferCount, 0));
			}
			
			InputLayout = inputLayout ?? throw new ArgumentNullException(nameof(inputLayout));
			_nativeBindings = new D3D11.VertexBufferBinding[size];
		}
		#endregion
	}
}
