#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: December 12, 2018 11:14:05 AM
// 
#endregion

using System;

namespace Gorgon.UI
{
    /// <summary>
    /// Event arguments for the <see cref="GorgonSearchBox.Search"/> event.
    /// </summary>
    public class GorgonSearchEventArgs
        : EventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the text to search for.
        /// </summary>
        public string SearchText
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.UI.GorgonSearchEventArgs"/> class.</summary>
        /// <param name="searchText">  The text to search for.</param>
        public GorgonSearchEventArgs(string searchText) => SearchText = searchText ?? string.Empty;
        #endregion
    }
}
