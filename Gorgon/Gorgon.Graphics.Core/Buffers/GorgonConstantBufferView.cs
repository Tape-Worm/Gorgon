using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A view for a <see cref="GorgonConstantBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view will allow applications to access a <see cref="GorgonConstantBuffer"/> that may be larger than the 4096 float4 limit. This is performed by offering a "window" into the constant
    /// buffer data that can be used to tell a shader which portion of the buffer to access at any given time.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonConstantBuffer"/>
    public sealed class GorgonConstantBufferView
        : IGorgonGraphicsObject, IGorgonConstantBufferInfo, IDisposable, IEquatable<GorgonConstantBufferView>
    {
        #region Variables.
        // Flag to indicate that the view owns the buffer resource.
        private bool _ownsBuffer;
        // The buffer associated with the view.
        private GorgonConstantBuffer _buffer;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the buffer associated with this view.
        /// </summary>
        public GorgonConstantBuffer Buffer => _buffer;

        /// <summary>
        /// Property to return the size of a single element in the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// An element is a single float4 value (4 floating point values).
        /// </remarks>
        public int ElementSize => 16;

        /// <summary>
        /// Property to return the intended usage flags for this texture.
        /// </summary>
        public ResourceUsage Usage => Buffer?.Usage ?? ResourceUsage.None;

        /// <summary>
        /// Property to return the number of bytes to allocate for the buffer.
        /// </summary>
        int IGorgonConstantBufferInfo.SizeInBytes => Buffer?.SizeInBytes ?? 0;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        string IGorgonNamedObject.Name => Buffer?.Name ?? string.Empty;

        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics => Buffer?.Graphics;

        /// <summary>
        /// Property to return the total number of elements in the buffer.
        /// </summary>
        /// <remarks>
        /// An element is a single float4 value inside of the buffer. 
        /// </remarks>
        public int TotalElementCount
        {
            get;
        }

        /// <summary>
        /// Property to return the index of the first element in the buffer to view.
        /// </summary>
        /// <remarks>
        /// An element is a single float4 value inside of the buffer. 
        /// </remarks>
        public int StartElement
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of elements to view.
        /// </summary>
        /// <remarks>
        /// An element is a single float4 value inside of the buffer. 
        /// </remarks>
        public int ElementCount
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to change the view "window" into the buffer.
        /// </summary>
        /// <param name="startElement">The index of the first element in the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <remarks>
        /// <para>
        /// This will adjust the <see cref="StartElement"/>, and <see cref="ElementCount"/> to allow an application to change the "window" of data in the constant buffer. This allows applications to update
        /// the range of the view for a shader.
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter must be between 0 and the <seealso cref="TotalElementCount"/>-1.  If it is not it will be constrained to those values to ensure there is no out of
        /// bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or equal to or less than 0), then the remainder of the buffer is mapped to the view up to 4096 elements. If it is provided, then the
        /// number of elements will be mapped to the view, up to a maximum of 4096 elements.  If the value exceeds 4096, then it will be constrained to 4096.
        /// </para>
        /// </remarks>
        public void AdjustView(int startElement, int elementCount = 0)
        {
            StartElement = startElement.Min(TotalElementCount - 1).Max(0);

            if (elementCount <= 0)
            {
                ElementCount = TotalElementCount - StartElement;
            }

            ElementCount = ElementCount.Min(4096).Max(1);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_ownsBuffer)
            {
                return;
            }

            GorgonConstantBuffer buffer = Interlocked.Exchange(ref _buffer, null);
            buffer?.Dispose();
            _ownsBuffer = false;
        }

        /// <summary>
        /// Function to create a constant buffer and an associated view.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="info">The information about the texture.</param>
        /// <param name="startElement">[Optional] The index of the first element within the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of elements in the buffer to view.</param>
        /// <returns>A new <see cref="GorgonConstantBufferView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonConstantBuffer"/> and a <see cref="GorgonConstantBufferView"/> as a single object that can be used to pass constant information
        /// to a shader.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonConstantBuffer"/> created by this method is linked to the <see cref="GorgonConstantBufferView"/> returned, disposal of either one will dispose of the other on your
        /// behalf. If the user created a <see cref="GorgonConstantBufferView"/> from the <see cref="GorgonConstantBuffer.GetView"/> method on the <see cref="GorgonConstantBuffer"/>, then it's assumed the
        /// user knows what they are doing and will handle the disposal of the buffer and view on their own.
        /// </para>
        /// <para>
        /// If provided, the <paramref name="startElement"/> parameter must be between 0 and the total number of elements in the buffer.  If it is not it will be constrained to those values to ensure there 
        /// is no out of bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or equal to or less than 0), then the remainder of the buffer is mapped to the view up to 4096 elements. If it is provided, then the 
        /// number of elements will be mapped to the view, up to a maximum of 4096 elements.  If the value exceeds 4096, then it will be constrained to 4096.
        /// </para>
        /// <para>
        /// A constant buffer element is a single float4 value (4 floating point values). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonConstantBuffer"/>
        public static GorgonConstantBufferView CreateConstantBuffer(GorgonGraphics graphics, IGorgonConstantBufferInfo info, int startElement = 0, int elementCount = 0)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var buffer = new GorgonConstantBuffer(graphics, info);
            GorgonConstantBufferView view = buffer.GetView(startElement, elementCount);
            view._ownsBuffer = true;
            return view;
        }

        /// <summary>
        /// Function to create a constant buffer and an associated view, initialized with the specified value.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the buffer. Must be an unmanaged value type.</typeparam>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="value">The value to store in the buffer.</param>
        /// <param name="name">[Optional] The name of the buffer.</param>
        /// <param name="usage">[Optional] The intended usage of the buffer.</param>
        /// <param name="startElement">[Optional] The index of the first element within the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of elements in the buffer to view.</param>
        /// <returns>A new <see cref="GorgonConstantBufferView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonConstantBuffer"/> and a <see cref="GorgonConstantBufferView"/> as a single object that can be used to pass constant information
        /// to a shader. The buffer created will match the size of the type specified by <typeparamref name="T"/> (adjusted to the nearest 16 bytes) and it will also upload the <paramref name="value"/> 
        /// specified into the buffer.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonConstantBuffer"/> created by this method is linked to the <see cref="GorgonConstantBufferView"/> returned, disposal of either one will dispose of the other on your
        /// behalf. If the user created a <see cref="GorgonConstantBufferView"/> from the <see cref="GorgonConstantBuffer.GetView"/> method on the <see cref="GorgonConstantBuffer"/>, then it's assumed the
        /// user knows what they are doing and will handle the disposal of the buffer and view on their own.
        /// </para>
        /// <para>
        /// If provided, the <paramref name="startElement"/> parameter must be between 0 and the total number of elements in the buffer.  If it is not it will be constrained to those values to ensure there 
        /// is no out of bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or equal to or less than 0), then the remainder of the buffer is mapped to the view up to 4096 elements. If it is provided, then the 
        /// number of elements will be mapped to the view, up to a maximum of 4096 elements.  If the value exceeds 4096, then it will be constrained to 4096.
        /// </para>
        /// <para>
        /// The <paramref name="usage"/> parameter defines where the GPU should place the resource for best performance.
        /// </para>
        /// <para>
        /// A constant buffer element is a single float4 value (4 floating point values). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonConstantBuffer"/>
        public static GorgonConstantBufferView CreateConstantBuffer<T>(GorgonGraphics graphics, ref T value, string name = null, ResourceUsage usage = ResourceUsage.Default, int startElement = 0, int elementCount = 0)
            where T : unmanaged
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            var buffer = new GorgonConstantBuffer(graphics, new GorgonConstantBufferInfo(name)
                                                            {
                                                                Usage = usage,
                                                                SizeInBytes = Unsafe.SizeOf<T>()
                                                            });
            buffer.SetData(ref value);
            GorgonConstantBufferView view = buffer.GetView(startElement, elementCount);
            view._ownsBuffer = true;
            return view;
        }

        /// <summary>
        /// Function to create a constant buffer and an associated view, initialized with the specified set of values.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the buffer. Must be an unmanaged value type.</typeparam>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="value">The array of values to store in the buffer.</param>
        /// <param name="name">[Optional] The name of the buffer.</param>
        /// <param name="usage">[Optional] The intended usage of the buffer.</param>
        /// <param name="startElement">[Optional] The index of the first element within the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of elements in the buffer to view.</param>
        /// <returns>A new <see cref="GorgonConstantBufferView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonConstantBuffer"/> and a <see cref="GorgonConstantBufferView"/> as a single object that can be used to pass constant information
        /// to a shader. The buffer created will match the size of the type specified by <typeparamref name="T"/> (adjusted to the nearest 16 bytes), multiplied by the length of the array and it will also
        /// upload the <paramref name="value"/> array specified into the buffer.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonConstantBuffer"/> created by this method is linked to the <see cref="GorgonConstantBufferView"/> returned, disposal of either one will dispose of the other on your
        /// behalf. If the user created a <see cref="GorgonConstantBufferView"/> from the <see cref="GorgonConstantBuffer.GetView"/> method on the <see cref="GorgonConstantBuffer"/>, then it's assumed the
        /// user knows what they are doing and will handle the disposal of the buffer and view on their own.
        /// </para>
        /// <para>
        /// If provided, the <paramref name="startElement"/> parameter must be between 0 and the total number of elements in the buffer.  If it is not it will be constrained to those values to ensure there 
        /// is no out of bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or equal to or less than 0), then the remainder of the buffer is mapped to the view up to 4096 elements. If it is provided, then the 
        /// number of elements will be mapped to the view, up to a maximum of 4096 elements.  If the value exceeds 4096, then it will be constrained to 4096.
        /// </para>
        /// <para>
        /// The <paramref name="usage"/> parameter defines where the GPU should place the resource for best performance.
        /// </para>
        /// <para>
        /// A constant buffer element is a single float4 value (4 floating point values). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonConstantBuffer"/>
        public static GorgonConstantBufferView CreateConstantBuffer<T>(GorgonGraphics graphics, T[] value, string name = null, ResourceUsage usage = ResourceUsage.Default, int startElement = 0, int elementCount = 0)
            where T : unmanaged
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var buffer = new GorgonConstantBuffer(graphics, new GorgonConstantBufferInfo(name)
                                                            {
                                                                Usage = usage,
                                                                SizeInBytes = Unsafe.SizeOf<T>() * value.Length
                                                            });
            buffer.SetData(value);
            GorgonConstantBufferView view = buffer.GetView(startElement, elementCount);
            view._ownsBuffer = true;
            return view;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(GorgonConstantBufferView other)
        {
            return this == other;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConstantBufferView"/> class.
        /// </summary>
        /// <param name="buffer">The constant buffer to view.</param>
        /// <param name="startElement">The index of the first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements to view.</param>
        /// <param name="totalElementCount">The total element count for the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        internal GorgonConstantBufferView(GorgonConstantBuffer buffer, int startElement, int elementCount, int totalElementCount)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            StartElement = startElement;
            ElementCount = elementCount;
            TotalElementCount = totalElementCount;
        }
        #endregion
    }
}
