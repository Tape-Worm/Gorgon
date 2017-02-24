﻿#region MIT
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
// Created: June 15, 2016 9:39:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using Gorgon.Reflection;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Provides information on how to set up a constant buffer.
	/// </summary>
	public class GorgonConstantBufferInfo
		: IGorgonConstantBufferInfo
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the intended usage flags for this texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <c>Default</c>.
		/// </remarks>
		public D3D11.ResourceUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of bytes to allocate for the buffer.
		/// </summary>
		/// <remarks>
		/// <note type="important">
		/// <para>
		/// <para>
		/// A <see cref="Core.BufferType.Constant"/> buffer, must set the size to be a multiple of 16. Constant buffer alignment rules require that they be sized to the nearest 16 bytes.
		/// </para>
		/// <para>
		/// If the buffer is not sized to a multiple of 16, Gorgon will attempt to adjust the size to fit the alignment requirement.
		/// </para>
		/// </para>
		/// </note>
		/// </remarks>
		public int SizeInBytes
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a <see cref="IGorgonConstantBufferInfo"/> based on the type representing a vertex.
		/// </summary>
		/// <typeparam name="T">The type of data representing a constant. This must be a value type.</typeparam>
		/// <param name="count">[Optional] The number of items to store in the buffer.</param>
		/// <param name="usage">[Optional] The usage parameter for the vertex buffer.</param>
		/// <returns>A new <see cref="IGorgonConstantBufferInfo"/> to use when creating a <see cref="GorgonConstantBuffer"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is less than 1.</exception>
		/// <exception cref="GorgonException">Thrown when the type specified by <typeparamref name="T"/> is not safe for use with native functions (see <see cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>).
		/// <para>-or-</para>
		/// <para>Thrown when the type specified by <typeparamref name="T"/> does not contain any public members.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This method is offered as a convenience to simplify the creation of the required info for a <see cref="GorgonConstantBuffer"/>. It will automatically determine the size of the constant based on the 
		/// size of a type specified by <typeparamref name="T"/> and fill in the <see cref="SizeInBytes"/> with the correct size.
		/// </para>
		/// <para>
		/// A constant buffer must have its size rounded to the nearest multiple of 16. If the type specified by <typeparamref name="T"/> does not have a size that is a multiple of 16, then the 
		/// <see cref="SizeInBytes"/> returned will be rounded up to the nearest multiple of 16.
		/// </para>
		/// <para>
		/// This method requires that the type passed by <typeparamref name="T"/> have its members decorated with the <see cref="InputElementAttribute"/>. This is used to determine which members of the 
		/// type are to be used in determining the size of the type.
		/// </para>
		/// </remarks>
		/// <seealso cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>
		/// <seealso cref="GorgonReflectionExtensions.IsSafeForNative(Type)"/>
		/// <seealso cref="GorgonReflectionExtensions.IsSafeForNative(Type,out IReadOnlyList{FieldInfo})"/>
		public static IGorgonConstantBufferInfo CreateFromType<T>(int count = 1, D3D11.ResourceUsage usage = D3D11.ResourceUsage.Default)
			where T : struct
		{
			if (count < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			Type dataType = typeof(T);
			IReadOnlyList<FieldInfo> badFields;

			if (dataType.IsSafeForNative(out badFields))
			{
				return new GorgonConstantBufferInfo
				       {
					       Usage = usage,
					       SizeInBytes = ((count * DirectAccess.SizeOf<T>()) + 15) & ~15
				       };
			}

			if (badFields.Count == 0)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TYPE_NO_FIELDS, dataType.FullName));
			}

			throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TYPE_NOT_VALID_FOR_NATIVE, dataType.FullName));
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBufferInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonConstantBufferInfo"/> to copy settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonConstantBufferInfo(IGorgonConstantBufferInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			SizeInBytes = info.SizeInBytes;
			Usage = info.Usage;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBufferInfo"/> class.
		/// </summary>
		public GorgonConstantBufferInfo()
		{
			Usage = D3D11.ResourceUsage.Default;
		}
		#endregion
	}
}
