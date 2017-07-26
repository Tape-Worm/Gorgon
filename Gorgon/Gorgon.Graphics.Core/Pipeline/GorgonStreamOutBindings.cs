#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 25, 2017 9:28:39 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of <see cref="GorgonStreamOutBinding"/> values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="GorgonStreamOutBinding"/> is used to bind a vertex buffer to the GPU pipeline so that it may be used for rendering.
	/// </para>
	/// </remarks>
	public sealed class GorgonStreamOutBindings
		: GorgonMonitoredValueTypeArray<GorgonStreamOutBinding>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of vertex buffers allow to be bound at the same time.
		/// </summary>
		public const int MaximumStreamOutCount = 4;
		#endregion

		#region Variables.
		// The native bindings.
		private readonly D3D11.StreamOutputBufferBinding[] _nativeBindings;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the native items wrapped by this list.
		/// </summary>
		internal D3D11.StreamOutputBufferBinding[] Native => _nativeBindings;
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function to store the native item at the given index.
	    /// </summary>
	    /// <param name="nativeItemIndex">The index of the item in the native array.</param>
	    /// <param name="value">The value containing the native item.</param>
	    protected override void OnStoreNativeItem(int nativeItemIndex, GorgonStreamOutBinding value)
	    {
            _nativeBindings[nativeItemIndex] = new D3D11.StreamOutputBufferBinding(value.Buffer?.NativeBuffer, value.Offset);
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
		/// Initializes a new instance of the <see cref="GorgonStreamOutBindings"/> class.
		/// </summary>
		public GorgonStreamOutBindings()
			: base(MaximumStreamOutCount)
		{
			_nativeBindings = new D3D11.StreamOutputBufferBinding[MaximumStreamOutCount];
		}
		#endregion
	}
}
