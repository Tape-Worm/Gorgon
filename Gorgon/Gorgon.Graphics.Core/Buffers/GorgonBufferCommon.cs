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
// Created: July 4, 2017 11:24:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Native;
using Gorgon.Reflection;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A base class for common functionality for generic, structures and raw buffers.
    /// </summary>
    public abstract class GorgonBufferCommon
        : GorgonBufferBase
    {
        #region Variables.
        // A cache of shader views for the buffer.
        private readonly Dictionary<BufferShaderViewKey, GorgonShaderResourceView> _shaderViews = new Dictionary<BufferShaderViewKey, GorgonShaderResourceView>();
        // A cache of unordered access views for the buffer.
        private readonly Dictionary<BufferShaderViewKey, GorgonUnorderedAccessView> _uavs = new Dictionary<BufferShaderViewKey, GorgonUnorderedAccessView>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return a cached shader resource view.
        /// </summary>
        /// <param name="key">The key associated with the view.</param>
        /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
        internal GorgonShaderResourceView GetView(BufferShaderViewKey key)
        {
            return _shaderViews.TryGetValue(key, out GorgonShaderResourceView view) ? view : null;
        }

        /// <summary>
        /// Function to return a cached shader resource view.
        /// </summary>
        /// <param name="key">The key associated with the view.</param>
        /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
        internal GorgonUnorderedAccessView GetUav(BufferShaderViewKey key)
        {
            return _uavs.TryGetValue(key, out GorgonUnorderedAccessView view) ? view : null;
        }

        /// <summary>
        /// Function to register the shader resource view in the cache.
        /// </summary>
        /// <param name="key">The unique key for the shader view.</param>
        /// <param name="view">The view to register.</param>
        internal void RegisterView(BufferShaderViewKey key, GorgonShaderResourceView view)
        {
            _shaderViews[key] = view;
        }

        /// <summary>
        /// Function to register an unordered access view in the cache.
        /// </summary>
        /// <param name="key">The unique key for the shader view.</param>
        /// <param name="view">The view to register.</param>
        internal void RegisterUav(BufferShaderViewKey key, GorgonUnorderedAccessView view)
        {
            _uavs[key] = view;
        }

        /// <summary>
        /// Function to retrieve the total number of elements that can be placed in the buffer.
        /// </summary>
        /// <param name="info">Information about the element format.</param>
        /// <returns>The number of elements in the buffer.</returns>
        protected int GetTotalElementCount(GorgonFormatInfo info)
        {
            if (info.IsTypeless)
            {
                return 0;
            }

            return SizeInBytes / info.SizeInBytes;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the buffer is locked when this method is called, it will automatically be unlocked and any lock pointer will be invalidated.
        /// </para>
        /// <para>
        /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
        /// </para>
        /// </remarks>
        public override void Dispose()
        {
            // Remove any cached views for this buffer.
            foreach (KeyValuePair<BufferShaderViewKey, GorgonShaderResourceView> view in _shaderViews)
            {
                view.Value.Dispose();
            }

            foreach (KeyValuePair<BufferShaderViewKey, GorgonUnorderedAccessView> view in _uavs)
            {
                view.Value.Dispose();
            }

            _shaderViews.Clear();
            _uavs.Clear();

            base.Dispose();
        }

        /// <summary>
        /// Function to update the buffer with data.
        /// </summary>
        /// <typeparam name="T">The type of data to send to the buffer. This must be a value type or primitive.</typeparam>
        /// <param name="data">The data used to populate the buffer.</param>
        /// <exception cref="ArgumentException">Thrown if the type specified by <typeparamref name="T"/> is not safe to use in a native memory operation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.</exception>
        /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to send a value type (<c>struct</c>) or primitive value to the buffer. This passes <paramref name="data"/> by value, and is suitable for small items of data like primitive 
        /// values and small structs. If larger values need to be passed, call the <see cref="Update{T}(ref T,int)"/> method.
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// When sending a value type to the buffer the data must be a primitive type with no complex members (i.e. members must be value types or primitive types). Furthermore, the value type and any 
        /// members that are value types must use the <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind"/> of <see cref="LayoutKind.Explicit"/> or <see cref="LayoutKind.Sequential"/>. This is 
        /// mandatory in order to ensure that the data gets sent to the card as-is without the .NET memory manager rearranging the members of the type. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void UpdateValue<T>(T data)
            where T : struct
        {
            Update(ref data);
        }

        /// <summary>
        /// Function to update the buffer with data.
        /// </summary>
        /// <typeparam name="T">The type of data to send to the buffer. This must be a value type or primitive.</typeparam>
        /// <param name="data">The data used to populate the buffer.</param>
        /// <param name="bufferOffset">[Optional] The number of bytes within this buffer to start writing at.</param>
        /// <exception cref="ArgumentException">Thrown if the type specified by <typeparamref name="T"/> is not safe to use in a native memory operation.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="bufferOffset"/> plus the size of the data exceeds the size of the buffer.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="GorgonResource.SizeInBytes"/> of the buffer.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="bufferOffset"/> is less than 0.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the usgae is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to send a value type (<c>struct</c>) or primitive value to the buffer. 
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a usage of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// When sending a value type to the buffer the data must be a primitive type with no complex members (i.e. members must be value types or primitive types). Furthermore, the value type and any 
        /// members that are value types must use the <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind"/> of <see cref="LayoutKind.Explicit"/> or <see cref="LayoutKind.Sequential"/>. This is 
        /// mandatory in order to ensure that the data gets sent to the card as-is without the .NET memory manager rearranging the members of the type. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Update<T>(ref T data, int bufferOffset = 0)
            where T : struct
        {
            int size = DirectAccess.SizeOf<T>();

#if DEBUG
            if ((Usage == D3D11.ResourceUsage.Dynamic) || (Usage == D3D11.ResourceUsage.Immutable))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_IMMUTABLE_OR_DYNAMIC);
            }

            if (size > SizeInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            Type type = typeof(T);

            if (!type.IsSafeForNative())
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TYPE_NOT_VALID_FOR_NATIVE, type.FullName));
            }
#endif

            Graphics.D3DDeviceContext.UpdateSubresource(ref data,
                                                        D3DResource,
                                                        0,
                                                        0,
                                                        0,
                                                        new D3D11.ResourceRegion
                                                        {
                                                            Left = bufferOffset,
                                                            Right = bufferOffset + size,
                                                            Top = 0,
                                                            Front = 0,
                                                            Back = 1,
                                                            Bottom = 1
                                                        });
        }

        /// <summary>
        /// Function to update the constant buffer data with data from native memory.
        /// </summary>
        /// <param name="data">The <see cref="GorgonPointer"/> to the native memory holding the data to copy into the buffer.</param>
        /// <param name="bufferOffset">[Optional] The number of bytes within this buffer to start writing at.</param>
        /// <param name="offset">[Optional] The offset, in bytes, to start copying from.</param>
        /// <param name="size">[Optional] The size, in bytes, to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/>, or the <paramref name="bufferOffset"/> plus the size of the data in <paramref name="data"/> exceed the size of this buffer.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="GorgonResource.SizeInBytes"/> of the buffer.
        /// <para>-or-</para>
        /// <para>The <paramref name="size"/> parameter is less than 1, or larger than the buffer size.</para>
        /// <para>-or-</para>
        /// <para>The <paramref name="offset"/> or the <paramref name="bufferOffset"/> parameter is less than 0.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the usage is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to send a blob of byte data to the buffer. This allows for fine grained control over what gets sent to the buffer. 
        /// </para>
        /// <para>
        /// Because this is using native, unmanaged, memory, special care must be taken to ensure that the application does not attempt to read/write out of bounds of that memory region. Particular care must be 
        /// taken to ensure that <paramref name="offset"/>, <paramref name="bufferOffset"/> and <paramref name="size"/> do not exceed the bounds of the memory region.
        /// </para>
        /// <para>
        /// If the <paramref name="size"/> parameter is omitted (<b>null</b>), then the entire buffer size is used minus the <paramref name="offset"/>.
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a usage of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void UpdateFromPointer(IGorgonPointer data, int bufferOffset = 0, int offset = 0, int? size = null)
        {
            data.ValidateObject(nameof(data));

            if (size == null)
            {
                size = ((int)data.Size) - offset;
            }

#if DEBUG
            if ((Usage == D3D11.ResourceUsage.Dynamic) || (Usage == D3D11.ResourceUsage.Immutable))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_IMMUTABLE_OR_DYNAMIC);
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (bufferOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferOffset));
            }

            if (offset + size > data.Size)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, size));
            }

            if (bufferOffset + size > data.Size)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, size));
            }
#endif

            Graphics.D3DDeviceContext.UpdateSubresource(new DX.DataBox
                                                        {
                                                            DataPointer = new IntPtr(data.Address + offset),
                                                            SlicePitch = 0,
                                                            RowPitch = size.Value
                                                        },
                                                        D3DResource,
                                                        0,
                                                        new D3D11.ResourceRegion
                                                        {
                                                            Left = bufferOffset,
                                                            Right = bufferOffset + size.Value,
                                                            Top = 0,
                                                            Front = 0,
                                                            Back = 1,
                                                            Bottom = 1
                                                        });
        }

        /// <summary>
        /// Function to update the buffer with data.
        /// </summary>
        /// <param name="data">The array of data to populate the buffer with.</param>
        /// <param name="bufferOffset">[Optional] The number of bytes within this buffer to start writing at.</param>
        /// <param name="startIndex">[Optional] The offset within the array to start copying from.</param>
        /// <param name="count">[Optional] The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the type specified by <typeparamref name="T"/> is not safe to use in a native memory operation.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="startIndex"/>, or the <paramref name="bufferOffset"/> plus the <paramref name="count"/> exceeds the number of elements in the <paramref name="data"/> parameter.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter, multiplied by the number of items to copy is larger than the total <see cref="GorgonResource.SizeInBytes"/> of the buffer.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="startIndex"/>, or the <paramref name="bufferOffset"/> is less than 0, or the <paramref name="count"/> is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the size of the type <typeparamref name="T"/> multiplied by the count (minus the offset) is larger than the buffer size.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the usgae is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to send an array of value types (<c>struct</c>) or primitive values to the buffer. 
        /// </para>
        /// <para>
        /// When sending a value type to the buffer the data must be a primitive type with no complex members (i.e. members must be value types or primitive types). Furthermore, the value type and any 
        /// members that are value types must use the <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind"/> of <see cref="LayoutKind.Explicit"/> or <see cref="LayoutKind.Sequential"/>. This is 
        /// mandatory in order to ensure that the data gets sent to the card as-is without the .NET memory manager rearranging the members of the type. 
        /// </para>
        /// <para>
        /// If the <paramref name="count"/> parameter is omitted (<b>null</b>), then the length of the <paramref name="data"/> parameter is used minus the <paramref name="startIndex"/>.
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a usage of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Update<T>(T[] data, int bufferOffset = 0, int startIndex = 0, int? count = null)
            where T : struct
        {
            data.ValidateObject(nameof(data));

            int size = DirectAccess.SizeOf<T>();

            if (count == null)
            {
                count = data.Length - startIndex;
            }

#if DEBUG
            if ((Usage == D3D11.ResourceUsage.Dynamic) || (Usage == D3D11.ResourceUsage.Immutable))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_IMMUTABLE_OR_DYNAMIC);
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (bufferOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferOffset));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if ((startIndex + count) > data.Length)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, startIndex, count));
            }

            if ((bufferOffset + count * size) > SizeInBytes)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, startIndex, count));
            }
#endif
            Graphics.D3DDeviceContext.UpdateSubresource(data,
                                                        D3DResource,
                                                        0,
                                                        0,
                                                        0,
                                                        new D3D11.ResourceRegion
                                                        {
                                                            Left = bufferOffset,
                                                            Right = bufferOffset + (size * count.Value),
                                                            Top = 0,
                                                            Front = 0,
                                                            Back = 1,
                                                            Bottom = 1
                                                        });
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferCommon"/> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics" /> object used to create and manipulate the buffer.</param>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="log">The log interface used for debug logging.</param>
        protected GorgonBufferCommon(GorgonGraphics graphics, string name, IGorgonLog log)
            : base(graphics, name, log)
        {
            
        }
        #endregion
    }
}
