#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 4, 2020 9:58:33 PM
// 
#endregion

using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A view model for the keyframe value editor.
    /// </summary>
    internal interface IKeyValueEditor
        : IHostedPanelViewModel
    {
        /// <summary>
        /// Property to return the title for the editor.
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// Property to set or return the track being edited.
        /// </summary>
        TrackKeySelection Track
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the value for the key frame.
        /// </summary>
        Vector4 Value
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the working sprite to update.
        /// </summary>
        GorgonSprite WorkingSprite
        {
            get;
            set;
        }
    }
}
