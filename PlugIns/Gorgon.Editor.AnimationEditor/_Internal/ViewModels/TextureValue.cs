
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 16, 2020 3:36:38 PM
// 

using Gorgon.Core;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Content;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A value for a texture key frame
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="TextureValue"/> struct.</remarks>
/// <param name="texture">The texture used when rendering the key.</param>
/// <param name="textureFile">The file for the texture.</param>
/// <param name="arrayIndex">The array index to use on the texture.</param>
/// <param name="textureCoordinates">The texture coordinates to use.</param>
internal readonly struct TextureValue(GorgonTexture2DView texture, IContentFile textureFile, int arrayIndex, GorgonRectangleF textureCoordinates)
        : IGorgonEquatableByRef<TextureValue>
{
    /// <summary>
    /// The texture to use when rendering the key.
    /// </summary>
    public readonly GorgonTexture2DView Texture = texture;

    /// <summary>
    /// The array index to use on the texture.
    /// </summary>
    public readonly int ArrayIndex = arrayIndex;

    /// <summary>
    /// The texture coordinates to use.
    /// </summary>
    public readonly GorgonRectangleF TextureCoordinates = textureCoordinates;

    /// <summary>
    /// The file for the texture.
    /// </summary>
    public readonly IContentFile TextureFile = textureFile;

    /// <summary>Returns a hash code for this instance.</summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode() => HashCode.Combine(ArrayIndex, TextureCoordinates, Texture);

    /// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public override string ToString() => Resources.GORANM_TOSTR_TEXTURE_KEYFRAME;

    /// <summary>
    /// Function to determine if two values are equal.
    /// </summary>
    /// <param name="left">The left side value to compare.</param>
    /// <param name="right">The right side value to compare.</param>
    /// <returns><b>true</b> if the values are equal, <b>false</b> if not.</returns>
    public static bool Equals(ref readonly TextureValue left, ref readonly TextureValue right) => (left.Texture == right.Texture)
            && (left.ArrayIndex == right.ArrayIndex)
            && (left.TextureCoordinates.Left.EqualsEpsilon(right.TextureCoordinates.Left))
            && (left.TextureCoordinates.Top.EqualsEpsilon(right.TextureCoordinates.Top))
            && (left.TextureCoordinates.Right.EqualsEpsilon(right.TextureCoordinates.Right))
            && (left.TextureCoordinates.Bottom.EqualsEpsilon(right.TextureCoordinates.Bottom))
            && (left.TextureFile == right.TextureFile);

    /// <summary>Determines whether the specified <see cref="object"/> is equal to this instance.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) => obj is TextureValue val ? Equals(in val, in this) : base.Equals(obj);

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
    /// </returns>
    bool IEquatable<TextureValue>.Equals(TextureValue other) => Equals(in other, in this);

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
    /// </returns>
    public bool Equals(ref readonly TextureValue other) => Equals(in other, in this);

}
