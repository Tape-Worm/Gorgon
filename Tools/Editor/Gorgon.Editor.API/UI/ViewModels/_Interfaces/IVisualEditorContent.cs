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
// Created: February 8, 2020 7:55:01 PM
// 
#endregion

using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// The view model for a visual content editor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When implementing a view model for a <see cref="VisualContentBaseControl"/>, the class must implement this interface to represent content data.
    /// </para>
    /// </remarks>
    public interface IVisualEditorContent
        : IEditorContent, IUndoHandler
    {
        /// <summary>
        /// Property to set or return the currently active hosted panel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property holds the view model for the currently active hosted panel which can be used for parameters for an operation on the content. Setting this value will bring the panel up on the UI and 
        /// setting it to <b>null</b> will remove it.
        /// </para>
        /// </remarks>
        IHostedPanelViewModel CurrentPanel
        {
            get;
            set;
        }
    }
}
