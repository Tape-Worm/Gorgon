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
// Created: August 16, 2016 3:47:42 PM
// 
#endregion

using System;
using Gorgon.IO.Properties;

namespace Gorgon.IO;

/// <summary>
/// A name and description for an image codec within a <see cref="GorgonSpriteCodecPlugIn"/>.
/// </summary>
public struct GorgonSpriteCodecDescription
    : IEquatable<GorgonSpriteCodecDescription>
{
    #region Variables.
    /// <summary>
    /// The name of the plug in.  This will be the same as its fully qualified type name.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// A friendly description used for display.
    /// </summary>
    public string Description;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to determine if two instances are equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
    public static bool Equals(GorgonSpriteCodecDescription left, GorgonSpriteCodecDescription right) => string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns the fully qualified type name of this instance.</summary>
    /// <returns>A <see cref="string" /> containing a fully qualified type name.</returns>
    public override string ToString() => string.Format(Resources.GOR2DIO_TOSTR_SPRITE_CODEC_PLUGIN_DESC, Name, Description);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
    /// <param name="obj">The object to compare with the current instance. </param>
    public override bool Equals(object obj) => obj is GorgonSpriteCodecDescription codecDesc ? codecDesc.Equals(this) : base.Equals(obj);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => Name?.GetHashCode() ?? 0;

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Equals(GorgonSpriteCodecDescription other) => Equals(this, other);

    /// <summary>
    /// Operator to determine if two instances are equal or not.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonSpriteCodecDescription left, GorgonSpriteCodecDescription right) => Equals(left, right);

    /// <summary>
    /// Operator to determine if two instances are not equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if the two instances are not equal, <b>false</b> if they are.</returns>
    public static bool operator !=(GorgonSpriteCodecDescription left, GorgonSpriteCodecDescription right) => !Equals(left, right);
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSpriteCodecDescription"/> struct.
    /// </summary>
    /// <param name="type">The type of codec object.</param>
    public GorgonSpriteCodecDescription(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        Name = type.FullName;
        Description = string.Empty;
    }
    #endregion
}
