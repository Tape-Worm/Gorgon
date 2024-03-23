
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, October 12, 2013 9:03:05 PM
// 

using Gorgon.Core;
using Gorgon.IO;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// The type of glyph brush to use when painting the glyphs for the font
/// </summary>
public enum GlyphBrushType
{
    /// <summary>
    /// A solid color.
    /// </summary>
    Solid = 0,
    /// <summary>
    /// Texture.
    /// </summary>
    Texture = 1,
    /// <summary>
    /// Hatch pattern.
    /// </summary>
    Hatched = 2,
    /// <summary>
    /// Linear gradient colors.
    /// </summary>
    LinearGradient = 3,
    /// <summary>
    /// Path gradient colors.
    /// </summary>
    PathGradient = 4
}

/// <summary>
/// A brush used to paint the glyphs when generating a font
/// </summary>
public abstract class GorgonGlyphBrush
    : IGorgonCloneable<GorgonGlyphBrush>, IEquatable<GorgonGlyphBrush>
{
    /// <summary>
    /// Property to return the type of brush.
    /// </summary>
    public abstract GlyphBrushType BrushType
    {
        get;
    }

    /// <summary>
    /// Function to convert this brush to the equivalent GDI+ brush type.
    /// </summary>
    /// <returns>The GDI+ brush type for this object.</returns>
    internal abstract Brush ToGDIBrush();

    /// <summary>
    /// Function to write out the specifics of the font brush data to a file writer.
    /// </summary>
    /// <param name="writer">The writer used to write the brush data.</param>
    internal abstract void WriteBrushData(GorgonBinaryWriter writer);

    /// <summary>
    /// Function to read back the specifics of the font brush data from a file reader.
    /// </summary>
    /// <param name="reader">The reader used to read the brush data.</param>
    internal abstract void ReadBrushData(GorgonBinaryReader reader);

    /// <summary>Function to clone an object.</summary>
    /// <returns>The cloned object.</returns>
    public abstract GorgonGlyphBrush Clone();

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
    public abstract bool Equals(GorgonGlyphBrush other);

    /// <summary>Returns a hash code for this instance.</summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>Determines whether the specified <see cref="object"/> is equal to this instance.</summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) => (obj as GorgonGlyphBrush).Equals(this);

}
