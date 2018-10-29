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
// Created: September 28, 2018 11:10:20 PM
// 
#endregion

using System;
using ComponentFactory.Krypton.Ribbon;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// The interface used to merge krypton ribbons.
    /// </summary>
    public interface IRibbonMerger
    {
        /// <summary>
        /// Function to merge a ribbon with the target ribbon
        /// </summary>
        /// <param name="ribbon">The ribbon to merge.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ribbon"/> parameter is <b>null</b>.</exception>
        void Merge(KryptonRibbon ribbon);

        /// <summary>
        /// Function to unmerge the specified ribbon from the target ribbon.
        /// </summary>
        /// <param name="ribbon">The ribbon to unmerge.</param>
        void Unmerge(KryptonRibbon ribbon);
    }
}
