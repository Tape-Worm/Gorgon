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
// Created: April 10, 2019 10:25:18 AM
// 
#endregion

using Gorgon.Editor.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Base interface for a manual input view model.
    /// </summary>
    internal interface IManualInputViewModel
        : IViewModel
    {
        /// <summary>
        /// Property to set or return whether the manual input interface is active or not.
        /// </summary>
        bool IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the position of the input interface.
        /// </summary>
        DX.Point? Position
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the manual input interface is moving.
        /// </summary>
        bool IsMoving
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set or return the command used to apply the coordinates to the sprite.
        /// </summary>
        IEditorCommand<object> ApplyCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the command used to cancel the operation.
        /// </summary>
        IEditorCommand<object> CancelCommand
        {
            get;
            set;
        }
    }
}
