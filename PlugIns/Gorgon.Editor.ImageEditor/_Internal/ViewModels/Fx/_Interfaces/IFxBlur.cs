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
// Created: February 22, 2020 5:41:00 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor.Fx
{
    /// <summary>
    /// The view model for the fx blur settings panel.
    /// </summary>
    internal interface IFxBlur
        : IHostedPanelViewModel
    {
        /// <summary>
        /// Property to set or return the amount of blur to apply.
        /// </summary>
        int BlurAmount
        {
            get;
            set;
        }
    }
}
