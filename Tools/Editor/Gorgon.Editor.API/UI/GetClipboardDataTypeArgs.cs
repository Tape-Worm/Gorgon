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
// Created: January 14, 2020 10:56:56 PM
// 
#endregion

using System;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Arguments for the <see cref="IClipboardHandler.GetClipboardDataTypeCommand"/>.
    /// </summary>
    public class GetClipboardDataTypeArgs
    {
        /// <summary>
        /// Property to return whether the clipboard has any data.
        /// </summary>
        public bool HasData => DataType is not null;

        /// <summary>
        /// Property to set or return the type of data on the clipboard.
        /// </summary>
        /// <remarks>
        /// This value will be <b>null</b> if <see cref="HasData"/> is <b>false</b>.
        /// </remarks>
        public Type DataType
        {
            get;
            set;
        }
    }
}
