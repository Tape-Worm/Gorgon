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
    /// This type of view will allow applications to access a <see cref="GorgonConstantBuffer"/> that may be larger than the 4096 constant limit. This is performed by offering a "window" into the constant
    /// buffer data that can be used to tell a shader which portion of the buffer to access at any given time.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Due to the nature of constant buffers on GPU hardware, these views are not aligned to a constant (which is a float4, or 16 bytes), but rather aligned to 16 constants (16 float4 values, or 256 
    /// bytes). This requires that your buffer be set up to be a multiple of 256 bytes in its <see cref="IGorgonConstantBufferInfo.SizeInBytes"/>.  This makes each element in the view the same as 16 float4
    /// values (or 256 bytes). That means when an offset of 2, and a count of 4 is set in the view, it is actually at an offset of 32 float4 values (512 bytes), and covers a range of 64 float4 values
    /// (1024 bytes). Because of this, care should be taken to ensure the buffer matches this alignment if constant buffer offsets/counts are to be used in your application.
    /// </para>
    /// <para>
    /// If no offsetting into the buffer is required, then the above information is not applicable.
    /// </para>
    /// </note>
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
        /// Property to set or return whether the view constant range has been adjusted
        /// </summary>
        internal bool ViewAdjusted
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Property to return the buffer associated with this view.
        /// </summary>
        public GorgonConstantBuffer Buffer => _buffer;

        /// <summary>
        /// Property to return the size of a single element in the buffer, in bytes.
        /// </summary>
        public int ElementSize => 256;

        /// <summary>
        /// Property to return the size of a single float4 constant in the buffer, in bytes.
        /// </summary>
        public int ConstantSize => 16;

        /// <summary>
        /// Property to return the intended usage flags for this texture.
        /// </summary>
        public ResourceUsage Usage => Buffer?.Usage ?? ResourceUsage.None;

        /// <summary>
        /// Property to return the number of bytes to allocate for the buffer.
        /// </summary>
        public int SizeInBytes => Buffer?.SizeInBytes ?? 0;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        string IGorgonNamedObject.Name => Buffer?.Name ?? string.Empty;

        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics => Buffer?.Graphics;

        /// <summary>
        /// Property to return the total number of constants in the buffer.
        /// </summary>
        /// <remarks>
        /// A constant is a single float4 value (16 bytes). 
        /// </remarks>
        public int TotalConstantCount => Buffer?.TotalConstantCount ?? 0;

        /// <summary>
        /// Property to return the total number of elements in the buffer.
        /// </summary>
        /// <returns>
        /// An element is equal to 16 constants, or 256 bytes.
        /// </returns>
        /// <seealso cref="ElementSize"/>
        public int TotalElementCount
        {
            get;
        }

        /// <summary>
        /// Property to return the index of the first element in the buffer to view.
        /// </summary>
        /// <remarks>
        /// An element refers to a group fo 16 constants (where a constant is a single float4, or 16 bytes).  If the start element is set to 2, then the offset in the buffer will be 512 bytes 
        /// (or 32 constants).
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
        /// An element refers to a group fo 16 constants (where a constant is a single float4, or 16 bytes).  If the element count is set to 4, then the view will cover 1024 bytes (or 64 constants).
        /// </remarks>
        public int ElementCount
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to change the view element range in the associated constant buffer.
        /// </summary>
        /// <param name="firstElement">The index of the first element in the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of constants to view.</param>
        /// <remarks>
        /// <para>
        /// This will adjust the <see cref="StartElement"/>, and <see cref="ElementCount"/> to allow an application to change the area in the buffer being viewed by a shader. This allows applications to 
        /// move the range of viewed constants around on demand.
        /// </para>
        /// <para>
        /// The <paramref name="firstElement"/> parameter must be between 0 and <seealso cref="TotalElementCount"/> - 1.  If it is not it will be constrained to those values to ensure there is no out of
        /// bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or less than 1), then the remainder of the buffer is mapped to the view up to 256 elements (4096 constants, or 65536 bytes). If it
        /// is provided, then the number of elements will be mapped to the view, up to a maximum of 256 elements.  If the value exceeds 256, then it will be constrained to 256.
        /// </para>
        /// </remarks>
        public void AdjustView(int firstElement, int elementCount = 0)
        {
            StartElement = firstElement.Min(TotalElementCount - 1).Max(0);

            if (elementCount <= 0)
            {
                elementCount = TotalConstantCount - StartElement;
            }

            ElementCount = elementCount.Min(256).Max(1);

            ViewAdjusted = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GorgonConstantBuffer buffer = Interlocked.Exchange(ref _buffer, null);

            if ((buffer is null) || (!_ownsBuffer))
            {
                return;
            }
            
            buffer.Dispose();
            _ownsBuffer = false;
        }

        /// <summary>
        /// Function to create a constant buffer and an associated view.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="info">The information about the texture.</param>
        /// <param name="startConstant">[Optional] The index of the first constant within the buffer to view.</param>
        /// <param name="constantCount">[Optional] The number of constants in the buffer to view.</param>
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
        /// If provided, the <paramref name="startConstant"/> parameter must be between 0 and the total number of constants in the buffer.  If it is not it will be constrained to those values to ensure there 
        /// is no out of bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="constantCount"/> parameter is omitted (or equal to or less than 0), then the remainder of the buffer is mapped to the view up to 4096 constants. If it is provided, then the 
        /// number of constants will be mapped to the view, up to a maximum of 4096 constants.  If the value exceeds 4096, then it will be constrained to 4096.
        /// </para>
        /// <para>
        /// A constant buffer constant is a single float4 value (4 floating point values). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonConstantBuffer"/>
        public static GorgonConstantBufferView CreateConstantBuffer(GorgonGraphics graphics, GorgonConstantBufferInfo info, int startConstant = 0, int constantCount = 0)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var buffer = new GorgonConstantBuffer(graphics, info);
            GorgonConstantBufferView view = buffer.GetView(startConstant, constantCount);
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
        /// <param name="firstElement">[Optional] The index of the first constant within the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of constants in the buffer to view.</param>
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
        /// The <paramref name="firstElement"/> parameter must be between 0 and <seealso cref="TotalElementCount"/> - 1.  If it is not it will be constrained to those values to ensure there is no out of
        /// bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or less than 1), then the remainder of the buffer is mapped to the view up to 256 elements (4096 constants, or 65536 bytes). If it
        /// is provided, then the number of elements will be mapped to the view, up to a maximum of 256 elements.  If the value exceeds 256, then it will be constrained to 256.
        /// </para>
        /// <para>
        /// The <paramref name="usage"/> parameter defines where the GPU should place the resource for best performance.
        /// </para>
        /// <para>
        /// A constant buffer constant is a single float4 value (4 floating point values). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonConstantBuffer"/>
        public static GorgonConstantBufferView CreateConstantBuffer<T>(GorgonGraphics graphics, in T value, string name = null, ResourceUsage usage = ResourceUsage.Default, int firstElement = 0, int elementCount = 0)
            where T : unmanaged
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            var buffer = new GorgonConstantBuffer(graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<T>())
            {
                Name = name,
                Usage = usage                
            });
            buffer.SetData(in value);
            GorgonConstantBufferView view = buffer.GetView(firstElement, elementCount);
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
        /// <param name="firstElement">[Optional] The index of the first constant within the buffer to view.</param>
        /// <param name="elementCount">[Optional] The number of constants in the buffer to view.</param>
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
        /// The <paramref name="firstElement"/> parameter must be between 0 and <seealso cref="TotalElementCount"/> - 1.  If it is not it will be constrained to those values to ensure there is no out of
        /// bounds access to the buffer.  
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> parameter is omitted (or less than 1), then the remainder of the buffer is mapped to the view up to 256 elements (4096 constants, or 65536 bytes). If it
        /// is provided, then the number of elements will be mapped to the view, up to a maximum of 256 elements.  If the value exceeds 256, then it will be constrained to 256.
        /// </para>
        /// <para>
        /// The <paramref name="usage"/> parameter defines where the GPU should place the resource for best performance.
        /// </para>
        /// <para>
        /// A constant buffer constant is a single float4 value (4 floating point values). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonConstantBuffer"/>
        public static GorgonConstantBufferView CreateConstantBuffer<T>(GorgonGraphics graphics, ReadOnlySpan<T> value, string name = null, ResourceUsage usage = ResourceUsage.Default, int firstElement = 0, int elementCount = 0)
            where T : unmanaged
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            var buffer = new GorgonConstantBuffer(graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<T>() * value.Length)
            {
                Name = name,
                Usage = usage                
            });
            buffer.SetData(value);
            GorgonConstantBufferView view = buffer.GetView(firstElement, elementCount);
            view._ownsBuffer = true;
            return view;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(GorgonConstantBufferView other) => (ReferenceEquals(this, other)) && (!ViewAdjusted);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) => (obj is GorgonConstantBufferView cbv) && cbv.Equals(this);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => base.GetHashCode().GenerateHash(ViewAdjusted);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConstantBufferView"/> class.
        /// </summary>
        /// <param name="buffer">The constant buffer to view.</param>
        /// <param name="firstElement">The index of the first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements to view.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        internal GorgonConstantBufferView(GorgonConstantBuffer buffer, int firstElement, int elementCount)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            TotalElementCount = (int)(buffer.SizeInBytes / 256.0f).FastFloor().Min(1);
            AdjustView(firstElement, elementCount);
        }
        #endregion
    }
}
