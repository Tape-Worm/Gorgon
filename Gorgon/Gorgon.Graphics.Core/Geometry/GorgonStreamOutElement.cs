#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Wednesday, November 23, 2011 9:54:30 AM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A stream out element for a stream out buffer.
/// </summary>
/// <remarks>
/// <para>
/// This defines a single element within a <see cref="GorgonStreamOutLayout"/>, and its relationship with other elements in that layout.
/// </para>
/// <para>
/// A <see cref="GorgonStreamOutLayout"/> will use an array of these items to define individual elements for a stream out slot.
/// </para>
/// </remarks>
public readonly struct GorgonStreamOutElement
    : IGorgonNamedObject, IGorgonEquatableByRef<GorgonStreamOutElement>
{
    #region Variables.
    /// <summary>
    /// The Direct 3D 11 Input Element that is wrapped by this type.
    /// </summary>
    internal readonly D3D11.StreamOutputElement NativeElement;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    string IGorgonNamedObject.Name => Context;

    /// <summary>
    /// Property to return the stream number to use.
    /// </summary>
        public int StreamIndex => NativeElement.Stream;

    /// <summary>
    /// Property to return the context of the element.
    /// </summary>
    /// <remarks>
    /// This is a string value that corresponds to a shader semantic.  For example, to specify a normal, the user would set this to "Normal".  With the exception of the position element (which must be 
    /// named "SV_Position"), these contexts can be any name as long as it maps to a corresponding vertex element in the shader.
    /// </remarks>
    public string Context => NativeElement.SemanticName;

    /// <summary>
    /// Property to return The index of the context.
    /// </summary>
    /// <remarks>
    /// This is used to differentiate between elements with the same <see cref="Context"/>. For example, to define a 2nd set of texture coordinates, use the same <see cref="Context"/> for the element 
    /// and define this value as 1 in the constructor.
    /// </remarks>
    public int Index => NativeElement.SemanticIndex;

    /// <summary>
    /// Property to return the component of the entry to begin writing out to.
    /// </summary>
    /// <remarks>
    /// Valid values are 0 to 3. 
    /// </remarks>
    public byte StartComponent => NativeElement.StartComponent;

    /// <summary>
    /// Property to return the number of components of the entry to write out to.
    /// </summary>
    /// <remarks>
    /// Valid values are 1 to 4. 
    /// </remarks>
    public byte ComponentCount => NativeElement.ComponentCount;

    /// <summary>
    /// Property to return the associated stream output buffer that is bound to the pipeline
    /// </summary>
    /// <remarks>
    /// The valid range for OutputSlot is 0 to 3.
    /// </remarks>
    public int Slot => NativeElement.OutputSlot;
    #endregion

    #region Method.
    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    ///   <b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override bool Equals(object obj) => obj is GorgonStreamOutElement element ? element.Equals(obj) : base.Equals(obj);

    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool Equals(in GorgonStreamOutElement left, in GorgonStreamOutElement right) => (string.Equals(left.Context, right.Context, StringComparison.OrdinalIgnoreCase)) && (left.Index == right.Index) &&
               (left.StartComponent == right.StartComponent) && (left.ComponentCount == right.ComponentCount) && (left.StreamIndex == right.StreamIndex) &&
               (left.Slot == right.Slot);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(in GorgonStreamOutElement left, in GorgonStreamOutElement right) => Equals(in left, in right);

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(in GorgonStreamOutElement left, in GorgonStreamOutElement right) => !Equals(in left, in right);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Context, Index, StreamIndex, StartComponent, Slot, ComponentCount);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the other parameter; otherwise, false.
    /// </returns>
    public bool Equals(GorgonStreamOutElement other) => Equals(in this, in other);

    /// <summary>
    /// Function to compare this instance with another.
    /// </summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(in GorgonStreamOutElement other) => Equals(in this, in other);
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutElement"/> class.
    /// </summary>
    /// <param name="context">The context for the element. This is used to indicate the HLSL semantic for the element.</param>
    /// <param name="start">The component to start writing out to.</param>
    /// <param name="count">The number of components to write out.</param>
    /// <param name="slot">The associated stream output buffer that is bound to the pipeline.</param>
    /// <param name="index">[Optional] The index of the element. This is used when there are multiple elements with the same context. It allows the HLSL shader to differentiate between the elements.</param>
    /// <param name="stream">[Optional] The stream number to use.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="start"/> and <paramref name="slot"/> values must be between 0 - 3, and the <paramref name="count"/> parameter must be between 1 - 4. If they are not, then an exception will be 
    /// raised.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="context"/> parameter is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="start"/> or <paramref name="slot"/> parameter is less than 0 or greater than 3.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="count"/> parameter is less than 1 or greater than 4.</para>
    /// </exception>
    public GorgonStreamOutElement(string context, byte start, byte count, byte slot, int index = 0, int stream = 0)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context))
        {
            throw new ArgumentEmptyException(nameof(context));
        }

        if (start > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_VALUE_OUT_OF_RANGE, start, 4));
        }

        if (count is > 4 or < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_VALUE_OUT_OF_RANGE_COUNT, count, 4));
        }

        if (slot > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_VALUE_OUT_OF_RANGE, slot, 4));
        }

        NativeElement = new D3D11.StreamOutputElement(stream.Max(0), context, index, start, count, slot);
    }
    #endregion
}
