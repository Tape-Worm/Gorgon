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

using System;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An input element for a buffer.
/// </summary>
/// <remarks>
/// <para>
/// This defines a single element within a <see cref="GorgonInputLayout"/>, and its relationship with other elements in that layout.
/// </para>
/// <para>
/// A <see cref="GorgonInputLayout"/> will use an array of these items to define individual elements for an input slot.
/// </para>
/// </remarks>
public readonly struct GorgonInputElement
    : IGorgonNamedObject, IGorgonEquatableByRef<GorgonInputElement>
{
    #region Variables.
    /// <summary>
    /// The Direct 3D 11 Input Element that is wrapped by this type.
    /// </summary>
    internal readonly D3D11.InputElement D3DInputElement;

    /// <summary>
    /// Property to return the size, in bytes, of this element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is directly influenced by the <see cref="Format"/> value.
    /// </para>
    /// </remarks>
    public readonly int SizeInBytes;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    string IGorgonNamedObject.Name => Context;

    /// <summary>
    /// Property to return the context of the element.
    /// </summary>
    /// <remarks>
    /// This is a string value that corresponds to a shader semantic.  For example, to specify a normal, the user would set this to "Normal".  With the exception of the position element (which must be 
    /// named "SV_Position"), these contexts can be any name as long as it maps to a corresponding vertex element in the shader.
    /// </remarks>
    public string Context => D3DInputElement.SemanticName;

    /// <summary>
    /// Property to return The index of the context.
    /// </summary>
    /// <remarks>
    /// This is used to differentiate between elements with the same <see cref="Context"/>. For example, to define a 2nd set of texture coordinates, use the same <see cref="Context"/> for the element 
    /// and define this value as 1 in the constructor.
    /// </remarks>
    public int Index => D3DInputElement.SemanticIndex;

    /// <summary>
    /// Property to return the format of the data.
    /// </summary>
    /// <remarks>
    /// This is used to specify the type of data for the element, and will also determine how many bytes the element will occupy.
    /// </remarks>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the offset of this element compared to other elements.
    /// </summary>
    /// <remarks>
    /// This is used to determine the order in which an element will appear after another element. For example, if the previous element has a format of <see cref="BufferFormat.R32G32B32A32_Float"/> and an offset of 0, 
    /// then this value needs to be set to 16. If this element were to use a format of <see cref="BufferFormat.R32G32_Float"/>, then the following element would have an offset of 16 + 8 (24).
    /// </remarks>
    public int Offset => D3DInputElement.AlignedByteOffset;

    /// <summary>
    /// Property to return the vertex buffer slot this element will use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiple vertex buffers can be used to identify parts of the same vertex.  This is used to minimize the amount of data being written to a vertex buffer and provide better performance.
    /// </para>
    /// <para>
    /// This value has a valid range of 0 to 15, inclusive.
    /// </para>
    /// </remarks>
    public int Slot => D3DInputElement.Slot;

    /// <summary>
    /// Property to return whether this data is instanced or per vertex.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indicates that the element should be included in instancing.
    /// </para>
    /// </remarks>
    public bool Instanced => D3DInputElement.Classification == D3D11.InputClassification.PerInstanceData;

    /// <summary>
    /// Property to return the number of instances to draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The number of instances to draw using the same per-instance data before moving to the next element.
    /// </para>
    /// <para>
    /// If the <see cref="Instanced"/> value is set to <b>false</b>, then this value will be set to 0.
    /// </para>
    /// </remarks>
    public int InstanceCount => D3DInputElement.Classification == D3D11.InputClassification.PerInstanceData ? D3DInputElement.InstanceDataStepRate : 0;
    #endregion

    #region Method.
    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    ///   <b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override bool Equals(object obj) => obj is GorgonInputElement element ? element.Equals(this) : base.Equals(obj);

    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool Equals(in GorgonInputElement left, in GorgonInputElement right) => (string.Equals(left.Context, right.Context, StringComparison.OrdinalIgnoreCase)) && (left.Format == right.Format) && (left.Index == right.Index) &&
               (left.InstanceCount == right.InstanceCount) && (left.Instanced == right.Instanced) && (left.Offset == right.Offset) &&
               (left.SizeInBytes == right.SizeInBytes) && (left.Slot == right.Slot);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(in GorgonInputElement left, in GorgonInputElement right) => Equals(in left, in right);

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(in GorgonInputElement left, in GorgonInputElement right) => !Equals(in left, in right);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Context, Index, Format, Offset, Slot, Instanced, InstanceCount, SizeInBytes);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the other parameter; otherwise, false.
    /// </returns>
    public bool Equals(GorgonInputElement other) => Equals(in this, in other);

    /// <summary>
    /// Function to compare this instance with another.
    /// </summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(in GorgonInputElement other) => Equals(in this, in other);
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonInputElement"/> class.
    /// </summary>
    /// <param name="context">The context for the element. This is used to indicate the HLSL semantic for the element.</param>
    /// <param name="format">The format and type of the element.</param>
    /// <param name="offset">The offset, in bytes, between each element.</param>
    /// <param name="index">[Optional] The index of the element. This is used when there are multiple elements with the same context. It allows the HLSL shader to differentiate between the elements.</param>
    /// <param name="slot">[Optional] The input assembler slot for the element. This is used to accomodate a vertex buffer bound at a specific slot (e.g. a vertex buffer at slot 3 would use this input element also assigned at slot 3).</param>
    /// <param name="instanced">[Optional] <b>true</b> if using instanced data, <b>false</b> if not.</param>
    /// <param name="instanceCount">[Optional] Number of instances, using the same per-instance data before moving to the next element. This value will be ignored if the instanced parameter is set to <b>false</b>.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="slot"/> parameter must be between 0 and 15 (inclusive).  A value outside of this range will cause an exception to be thrown.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="context"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> parameter is not supported.</exception> 
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or greater than 15.</exception>
    public GorgonInputElement(string context, BufferFormat format, int offset, int index = 0, int slot = 0, bool instanced = false, int instanceCount = 0)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context))
        {
            throw new ArgumentEmptyException(nameof(context));
        }

        var formatInfo = new GorgonFormatInfo(format);

        if (formatInfo.BitDepth == 0)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
        }

        if (slot is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_VALUE_OUT_OF_RANGE, slot, 16));
        }

        Format = format;
        D3DInputElement = new D3D11.InputElement(context,
                                                 index,
                                                 (Format)format,
                                                 offset,
                                                 slot,
                                                 instanced ? D3D11.InputClassification.PerInstanceData : D3D11.InputClassification.PerVertexData,
                                                 instanced ? instanceCount : 0);
        SizeInBytes = formatInfo.SizeInBytes;
    }
    #endregion
}
