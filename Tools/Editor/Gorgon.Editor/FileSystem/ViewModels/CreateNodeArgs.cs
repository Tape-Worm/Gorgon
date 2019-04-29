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
// Created: September 8, 2018 3:21:07 PM
// 
#endregion

using System.ComponentModel;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Arguments for the <see cref="IFileExplorerVm.CreateNodeCommand"/>.
    /// </summary>
    internal class CreateNodeArgs
        : CancelEventArgs
    {
        /// <summary>
        /// Property to return the parent node that of the node being edited.
        /// </summary>
        public IFileExplorerNodeVm ParentNode
        {
            get;
        }

		/// <summary>
        /// Property to return the name of the new node.
        /// </summary>
		public string NewNodeName
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="CreateNodeArgs"/> class.</summary>
        /// <param name="parent">The parent of the new node.</param>
        public CreateNodeArgs(IFileExplorerNodeVm parent) => ParentNode = parent;

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.CreateNodeArgs"/> class.</summary>
        /// <param name="parent">The parent of the new node.</param>
        public CreateNodeArgs(IFileExplorerNodeVm parent, string newNodeName)
            : this(parent) => NewNodeName = newNodeName;
    }
}
