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
// Created: February 24, 2017 10:04:58 AM
// 
#endregion
using System.Collections.Generic;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts.Codecs
{
    /// <summary>
    /// Information about the BmFont.
    /// </summary>
    internal class BmFontInfo
        : GorgonFontInfo
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the line height.
        /// </summary>
        public float LineHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the list of font texture paths.
        /// </summary>
        public string[] FontTextures
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the list of glyph rectangles.
        /// </summary>
        public IDictionary<char, DX.Rectangle> GlyphRects
        {
            get;
        } = new Dictionary<char, DX.Rectangle>();

        /// <summary>
        /// Property to return the list of character advances.
        /// </summary>
        public IDictionary<char, int> CharacterAdvances
        {
            get;
        } = new Dictionary<char, int>();

        /// <summary>
        /// Property to return the list of glyph texture index relationships.
        /// </summary>
        public IDictionary<char, int> GlyphTextureIndices
        {
            get;
        } = new Dictionary<char, int>();

        /// <summary>
        /// Property to return the list of offsets for the glyphs.
        /// </summary>
        public IDictionary<char, DX.Point> GlyphOffsets
        {
            get;
        } = new Dictionary<char, DX.Point>();

        /// <summary>
        /// Property to return the list of kerning pairs for the glyphs.
        /// </summary>
        public IDictionary<GorgonKerningPair, int> KerningPairs
        {
            get;
        } = new Dictionary<GorgonKerningPair, int>();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="BmFontInfo"/> class.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="size">The size of the font, in pixels.</param>
        public BmFontInfo(string fontFamily, float size)
            : base(fontFamily, size)
        {
        }
        #endregion
    }
}
