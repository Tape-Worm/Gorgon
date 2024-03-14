#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, March 11, 2014 9:18:52 PM
// 
#endregion

using System.Drawing.Drawing2D;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A wrap mode combo box item.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WrapModeComboItem"/> class.
/// </remarks>
/// <param name="wrapMode">The wrap mode.</param>
/// <param name="text">The text.</param>
internal struct WrapModeComboItem(GlyphBrushWrapMode wrapMode, string text)
        : IEquatable<WrapModeComboItem>
{
    #region Variables.
    // Item text.
    private readonly string _text = text;			

    /// <summary>
    /// The wrapping mode.
    /// </summary>
    public readonly GlyphBrushWrapMode WrapMode = wrapMode;
    #endregion

    #region Methods.
    /// <summary>
    /// Operator to determine if two instances are equal.
    /// </summary>
    /// <param name="left">Left instance to compare.</param>
    /// <param name="right">Right instance to compare.</param>
    /// <returns>TRUE if equal, FALSE if not.</returns>
    public static bool operator ==(WrapModeComboItem left, WrapModeComboItem right) => Equals(ref left, ref right);

    /// <summary>
    /// Operator to determine if two instances are not equal.
    /// </summary>
    /// <param name="left">Left instance to compare.</param>
    /// <param name="right">Right instance to compare.</param>
    /// <returns>TRUE if not equal, FALSE if equal.</returns>
    public static bool operator !=(WrapModeComboItem left, WrapModeComboItem right) => !Equals(ref left, ref right);

    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="left">Left instance to compare.</param>
    /// <param name="right">Right instance to compare.</param>
    /// <returns>TRUE if equal, FALSE if not.</returns>
    public static bool Equals(ref WrapModeComboItem left, ref WrapModeComboItem right) => left.WrapMode == right.WrapMode;

    /// <summary>
    /// Function to perform an explicit conversion between a Drawing wrap mode and this wrap mode item.
    /// </summary>
    /// <param name="item">Item to convert.</param>
    /// <returns>The drawing wrap mode.</returns>
    public static implicit operator WrapMode(WrapModeComboItem item) => (WrapMode)item.WrapMode;

    /// <summary>
    /// Function to perform an explicit conversion between a brush wrap mode and this wrap mode item.
    /// </summary>
    /// <param name="item">Item to convert.</param>
    /// <returns>The drawing wrap mode.</returns>
    public static implicit operator GlyphBrushWrapMode(WrapModeComboItem item) => item.WrapMode;

    /// <summary>
    /// Determines whether the specified <see cref="object"/>, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override readonly bool Equals(object obj) => obj is WrapModeComboItem item ? item.Equals(this) : base.Equals(obj);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override readonly int GetHashCode() => HashCode.Combine(WrapMode);

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override readonly string ToString() => _text;

    #endregion
    #region Constructor/Destructor.

    /// <summary>
    /// Initializes a new instance of the <see cref="WrapModeComboItem"/> struct.
    /// </summary>
    /// <param name="wrapMode">The wrap mode.</param>
    public WrapModeComboItem(GlyphBrushWrapMode wrapMode)
        : this(wrapMode, string.Empty)
    {
    }
    #endregion

    #region IEquatable<WrapModeComboItem> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(WrapModeComboItem other) => Equals(ref this, ref other);
    #endregion
}
