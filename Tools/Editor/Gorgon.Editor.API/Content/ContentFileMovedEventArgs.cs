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
// Created: November 11, 2018 1:08:24 PM
// 
#endregion

using System;
using Gorgon.Core;

namespace Gorgon.Editor.Content
{
    /// <summary>
    /// Event parameters for the <see cref="IContentFile.Moved"/> event.
    /// </summary>
    public class ContentFileMovedEventArgs
        : EventArgs
    {
        /// <summary>
        /// Property to return the new file object.
        /// </summary>
        public IContentFile NewFile
        {
            get;
        }

        /// <summary>Initializes a new instance of the ContentFileMovedEventArgs class.</summary>
        /// <param name="newFile">The new path after moving.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newFile" /> parameter is <strong>null</strong>.</exception>
        public ContentFileMovedEventArgs(IContentFile newFile) => NewFile = newFile ?? throw new ArgumentNullException(nameof(newFile));
    }
}
