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
// Created: March 26, 2020 9:05:58 PM
// 
#endregion

using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The settings view model for the one bit effect.
    /// </summary>
    internal interface IFxOneBit
        : IHostedPanelViewModel
    {
        /// <summary>
        /// Property to set or return the maximum threshold to convert to white (or black if inverted).
        /// </summary>
        int MaxWhiteThreshold
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the minimum threshold to convert to white (or black if inverted).
        /// </summary>
        int MinWhiteThreshold
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the flag used to invert the black/white values.
        /// </summary>
        bool Invert
        {
            get;
            set;
        }
    }
}
