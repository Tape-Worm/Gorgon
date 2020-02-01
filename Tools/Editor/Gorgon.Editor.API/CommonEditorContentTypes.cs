#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 10, 2019 9:32:06 AM
// 
#endregion


namespace Gorgon.Editor
{
    /// <summary>
    /// Defines type names for the most common content types used in Gorgon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are the most common types of content data used by Gorgon as-is.  Users can specify their own types for the own data.  However, if a new content editor plug in replaces the stock editors, then 
    /// the plug in author should use these type names to ensure compatibility between their plug in and the other stock plug ins for Gorgon.
    /// </para>
    /// </remarks>
    public static class CommonEditorContentTypes
    {
        /// <summary>
        /// The common type name for sprite data.
        /// </summary>
        public const string SpriteType = "Sprite";

        /// <summary>
        /// The common type name for image data.
        /// </summary>
        public const string ImageType = "Image";

        /// <summary>
        /// The common type name for animation data.
        /// </summary>
        public const string AnimationType = "Animation";

        /// <summary>
        /// The common type name for font data.
        /// </summary>
        public const string FontType = "Font";
    }
}
