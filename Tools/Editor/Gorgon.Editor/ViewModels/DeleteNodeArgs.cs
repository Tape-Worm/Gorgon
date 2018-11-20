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
// Created: November 18, 2018 12:04:42 AM
// 
#endregion

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The arguments to pass to the <see cref="IFileExplorerVm.DeleteNodeCommand"/>.
    /// </summary>
    internal class DeleteNodeArgs
    {
        /// <summary>
        /// Property to set or return whether to suppress the confirmation prompt when deleting.
        /// </summary>
        public bool SuppressConfirm
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether or not the recycle the item being deleted.
        /// </summary>
        public bool Recycle
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Property to return the parent node that of the node being edited.
        /// </summary>
        public IFileExplorerNodeVm ParentNode
        {
            get;
        }

        /// <summary>
        /// Property to return the path to the node that is being edited.
        /// </summary>
        public string NodePath
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.DeletNodeArgs"/> class.</summary>
        /// <param name="parent">The parent of the new node.</param>
        /// <param name="path">The path to the node to delete.</param>
        public DeleteNodeArgs(IFileExplorerNodeVm parent, string path)
        {
            ParentNode = parent;
            NodePath = path;
        }
    }
}
