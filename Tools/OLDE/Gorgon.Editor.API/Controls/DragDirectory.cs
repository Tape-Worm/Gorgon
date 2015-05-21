#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Friday, October 24, 2014 12:29:31 AM
// 
#endregion

using System.Windows.Forms;

namespace Gorgon.Editor
{
	/// <summary>
	/// Drag and drop directory data.
	/// </summary>
	class DragDirectory
		: DragData
	{
		#region Properties.
		/// <summary>
		/// Property to return the directory node being dragged.
		/// </summary>
		public TreeNodeDirectory DirectoryNode
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DragDirectory"/> class.
		/// </summary>
		/// <param name="directoryNode">The directory node.</param>
		/// <param name="mouseButton">The mouse button used to drag.</param>
		public DragDirectory(TreeNodeDirectory directoryNode, MouseButtons mouseButton)
			: base(mouseButton)
		{
			DirectoryNode = directoryNode;
		}
		#endregion
	}
}
