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
// Created: March 14, 2019 11:39:34 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for sprite content.
    /// </summary>
    internal interface ISpriteContent
        : IEditorContent
    {
        /// <summary>
        /// Property to return the texture associated with the sprite.
        /// </summary>
        GorgonTexture2DView Texture
        {
            get;
        }

        /// <summary>
        /// Property to set or return the texture coordinates used by the sprite.
        /// </summary>
        DX.RectangleF TextureCoordinates
        {
            get;
            set;
        }
    }
}
