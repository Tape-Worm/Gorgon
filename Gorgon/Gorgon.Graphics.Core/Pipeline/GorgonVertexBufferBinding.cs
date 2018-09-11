﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, February 01, 2012 9:40:15 AM
// 
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A binding state for a <see cref="GorgonVertexBuffer"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is used to bind a <see cref="GorgonVertexBuffer"/> to the GPU pipeline.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonVertexBuffer"/>
	public readonly struct GorgonVertexBufferBinding
		: IGorgonEquatableByRef<GorgonVertexBufferBinding>
	{
		#region Variables.
		/// <summary>
		/// Empty vertex buffer binding.
		/// </summary>
		public static readonly GorgonVertexBufferBinding Empty = new GorgonVertexBufferBinding();

		/// <summary>
		/// The vertex buffer to bind.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public readonly GorgonVertexBuffer VertexBuffer;
		/// <summary>
		/// Stride of the items within the vertex buffer, in bytes.
		/// </summary>
		public readonly int Stride;
		/// <summary>
		/// Offset within the buffer to start at, in bytes.
		/// </summary>
		public readonly int Offset;
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function to create a vertex buffer and its binding.
	    /// </summary>
	    /// <typeparam name="T">The type of data representing a vertex, must be an unmanaged value type.</typeparam>
	    /// <param name="graphics">The graphics interface that will create the buffer.</param>
	    /// <param name="info">Information about the buffer to create.</param>
	    /// <param name="initialData">[Optional] An initial set of vertex data to send to the buffer.</param>
	    /// <param name="bindingIndex">[Optional] The index, in vertices, inside the buffer where binding is to begin.</param>
	    /// <returns>A new <see cref="GorgonVertexBufferBinding"/>.</returns>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameter is <b>null</b>.</exception>
	    /// <remarks>
	    /// <para>
	    /// Use this to quickly create a vertex buffer and its binding based on a known vertex data type. 
	    /// </para>
	    /// <para>
	    /// Be aware that the <see cref="VertexBuffer"/> created by this method must be disposed manually after it is no longer of any use. 
	    /// </para>
	    /// </remarks>
	    /// <seealso cref="GorgonVertexBuffer"/>
	    public static GorgonVertexBufferBinding CreateVertexBuffer<T>(GorgonGraphics graphics, IGorgonVertexBufferInfo info, GorgonNativeBuffer<T> initialData = null, int bindingIndex = 0)
	        where T : unmanaged
	    {
	        if (graphics == null)
	        {
                throw new ArgumentNullException(nameof(graphics));
	        }

	        if (info == null)
	        {
	            throw new ArgumentNullException(nameof(info));
	        }

	        var buffer = new GorgonVertexBuffer(graphics, info, initialData?.Cast<byte>());
	        int vertexSize = Unsafe.SizeOf<T>();

	        return new GorgonVertexBufferBinding(buffer, vertexSize, bindingIndex * vertexSize);
	    }

	    /// <summary>
	    /// Function to create a vertex buffer and its binding.
	    /// </summary>
	    /// <typeparam name="T">The type of data representing a vertex, must be an unmanaged value type.</typeparam>
	    /// <param name="graphics">The graphics interface that will create the buffer.</param>
	    /// <param name="vertexCount">The total number vertices that the buffer can hold.</param>
	    /// <param name="usage">[Optional] The intended usage for the buffer.</param>
	    /// <param name="binding">[Optional] The binding options for the buffer.</param>
	    /// <param name="initialData">[Optional] An initial set of vertex data to send to the buffer.</param>
	    /// <param name="bindingIndex">[Optional] The index, in vertices, inside the buffer where binding is to begin.</param>
	    /// <param name="bufferName">[Optional] A name for the buffer.</param>
	    /// <returns>A new <see cref="GorgonVertexBufferBinding"/>.</returns>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
	    /// <remarks>
	    /// <para>
	    /// Use this to quickly create a vertex buffer and its binding based on a known vertex data type. 
	    /// </para>
	    /// <para>
	    /// Be aware that the <see cref="VertexBuffer"/> created by this method must be disposed manually after it is no longer of any use. 
	    /// </para>
	    /// </remarks>
	    /// <seealso cref="GorgonVertexBuffer"/>
	    public static GorgonVertexBufferBinding CreateVertexBuffer<T>(GorgonGraphics graphics,
	                                                                  int vertexCount,
	                                                                  ResourceUsage usage = ResourceUsage.Default,
	                                                                  VertexIndexBufferBinding binding = VertexIndexBufferBinding.None,
	                                                                  GorgonNativeBuffer<T> initialData = null,
	                                                                  int bindingIndex = 0,
	                                                                  string bufferName = null)
	        where T : unmanaged
	    {
	        if (graphics == null)
	        {
	            throw new ArgumentNullException(nameof(graphics));
	        }

	        int vertexSize = Unsafe.SizeOf<T>();
	        var buffer = new GorgonVertexBuffer(graphics,
	                                            new GorgonVertexBufferInfo(bufferName)
	                                            {
	                                                SizeInBytes = vertexCount * vertexSize,
	                                                Binding = binding,
	                                                Usage = usage
	                                            },
	                                            initialData?.Cast<byte>());


	        return new GorgonVertexBufferBinding(buffer, vertexSize, bindingIndex * vertexSize);
	    }

        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonVertexBufferBinding other) => Equals(in this, in other);

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GORGFX_TOSTR_VERTEXBUFFER_BINDING, Stride, Offset, (VertexBuffer?.Native == null) ? "(NULL)" : VertexBuffer.Name);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => VertexBuffer == null
                       ? 281.GenerateHash(Stride).GenerateHash(Offset)
                       : 281.GenerateHash(Stride).GenerateHash(Offset).GenerateHash(VertexBuffer.GetHashCode());

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
		{
		    if (obj is GorgonVertexBufferBinding binding)
		    {
		        return binding.Equals(this);
		    }

		    return base.Equals(obj);
		}

        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(in GorgonVertexBufferBinding left, in GorgonVertexBufferBinding right) => ((left.VertexBuffer == right.VertexBuffer) && (left.Offset == right.Offset) &&
                    (left.Stride == right.Stride));

        /// <summary>
        /// Function to compare this instance with another.
        /// </summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonVertexBufferBinding other) => Equals(in this, in other);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(in GorgonVertexBufferBinding left, in GorgonVertexBufferBinding right)
		{
		    return Equals(in left, in right);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(in GorgonVertexBufferBinding left, in GorgonVertexBufferBinding right)
		{
            return !Equals(in left, in right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBinding"/> struct.
		/// </summary>
		/// <param name="buffer">The buffer to bind.</param>
		/// <param name="stride">The stride of the data in the buffer, in bytes.</param>
		/// <param name="offset">[Optional] The offset within the buffer to start at, in bytes.</param>
		public GorgonVertexBufferBinding(GorgonVertexBuffer buffer, int stride, int offset = 0)
		{			
			VertexBuffer = buffer;
			Stride = stride;
			Offset = offset;
		}
		#endregion
	}
}
