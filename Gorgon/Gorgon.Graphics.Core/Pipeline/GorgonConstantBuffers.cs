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
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
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
		: GorgonResourceBindingList<GorgonConstantBuffer>
	{
		#region Constants.
		/// <summary>
		/// The maximum size for a constant buffer binding list.
		/// </summary>
		public const int MaximumConstantBufferCount = D3D11.CommonShaderStage.ConstantBufferApiSlotCount;
		#endregion

		#region Variables.
		// The native Direct3D 11 constant buffer list.
		private D3D11.Buffer[] _nativeBuffers;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D11 constant buffers.
		/// </summary>
		internal D3D11.Buffer[] NativeBuffers => _nativeBuffers;
		#endregion

		#region Methods.
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
			Array.Resize(ref _nativeBuffers, newSize);
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
			Array.Clear(_nativeBuffers, 0, _nativeBuffers.Length);
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		protected override void OnSetNativeItem(int index, GorgonConstantBuffer item)
		{
			_nativeBuffers[index] = item.D3DBuffer;
		}

		/// <summary>
		/// Function to set multiple <see cref="GorgonConstantBuffer"/> objects at once.
		/// </summary>
		/// <param name="buffers">The buffers to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="buffers"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffers"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the number of <paramref name="buffers"/> exceeds the size of this list.</exception>
		/// <exception cref="GorgonException">Thrown when the list is locked for writing.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <see cref="GorgonConstantBuffer"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		public void SetRange(IReadOnlyList<GorgonConstantBuffer> buffers, int? count = null)
		{
			if (count == null)
			{
				count = buffers?.Count ?? 0;
			}

#if DEBUG
			if (IsLocked)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_ERR_BINDING_LIST_LOCKED);
			}

			if (count > (buffers?.Count ?? 0))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, buffers?.Count));
			}

			if (count > MaximumConstantBufferCount)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, MaximumConstantBufferCount));
			}
#endif
			if (count != Count)
			{
				Resize(count.Value);
				Array.Resize(ref _nativeBuffers, count.Value);
			}

			if ((buffers == null) || (count == 0))
			{
				Clear();
				return;
			}

			for (int i = 0; i < count.Value; ++i)
			{
				this[i] = buffers[i];
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		/// <param name="buffers">The list of <see cref="GorgonConstantBuffer"/> objects to copy into this list.</param>
		/// <exception cref="GorgonException">Thrown when the same <see cref="GorgonConstantBuffer"/> is bound to more than one slot in the <paramref name="buffers"/> list.</exception>
		public GorgonConstantBuffers(IEnumerable<GorgonConstantBuffer> buffers)
			: this()
		{
			SetRange(buffers?.ToArray());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		/// <param name="size">[Optional] The number of constant buffers for this list.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// If the <paramref name="size"/> parameter is larger than the maximum allowed number of constant buffers, then the size will be adjusted to that maximum instead of the amount requested. See the 
		/// <seealso cref="MaximumConstantBufferCount"/> constant to determine what the maximum is.
		/// </para>
		/// <para>
		/// If the size is omitted, then room is made for 1 constant buffer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For the sake of performance, Exceptions thrown by this constructor will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonConstantBuffers(int size = 1)
			: base(size, MaximumConstantBufferCount)
		{
			_nativeBuffers = new D3D11.Buffer[size];
		}
		#endregion
	}
}
