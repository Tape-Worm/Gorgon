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
// Created: September 27, 2018 10:58:50 PM
// 
#endregion


using System;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Defines functionality used to handle dragging and dropping.
    /// </summary>
    /// <typeparam name="T">The type of data being dragged and dropped.</typeparam>
    public interface IDragDropHandler<in T>
    {
        /// <summary>
        /// Function to determine if an object can be dropped.
        /// </summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns><b>true</b> if the data can be dropped, <b>false</b> if not.</returns>
        bool CanDrop(T dragData);

        /// <summary>
        /// Function to drop the payload for a drag drop operation.
        /// </summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        void Drop(T dragData, Action afterDrop = null);
    }
}
