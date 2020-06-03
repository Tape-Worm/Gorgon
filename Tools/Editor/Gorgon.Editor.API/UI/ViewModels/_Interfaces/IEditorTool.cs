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
// Created: May 25, 2020 2:53:02 PM
// 
#endregion

using Gorgon.Editor.UI;

namespace Gorgon.Editor.Tools
{
    /// <summary>
    /// The base view model for a tool plug in UI.
    /// </summary>
    public interface IEditorTool
        : IViewModel
    {
        /// <summary>
        /// Property to return the command used when the tool UI is about to be closed.
        /// </summary>
        /// <remarks>
        /// This command will be executed when the user shuts down the tool via some UI element (e.g. the 'X' in a window). 
        /// </remarks>
        IEditorAsyncCommand<CloseToolArgs> CloseToolCommand
        {
            get;
        }
    }
}
