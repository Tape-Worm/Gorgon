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
// Created: Friday, October 24, 2014 12:33:48 AM
// 
#endregion

using System.IO;
using System.Windows.Forms;

namespace Gorgon.Editor
{
	/// <summary>
	/// Drag and drop file data.
	/// </summary>
	public class DragFile
		: DragData
	{
		#region Properties.
		/// <summary>
		/// Property to return the file node being dragged.
		/// </summary>
		internal TreeNodeFile FileNode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the editor file being dragged.
		/// </summary>
		public EditorFile EditorFile
		{
			get
			{
				return FileNode.EditorFile;
			}
		}
		#endregion

        #region Methods.
        /// <summary>
        /// Function to open a stream to the file.
        /// </summary>
        /// <returns>A read-only stream to the file, or NULL (<i>Nothing</i> in VB.Net) if no file is attached to this drag node.</returns>
	    public Stream OpenFile()
	    {
            if ((FileNode == null)
                || (FileNode.File == null))
            {
                return null;
            }

            return FileNode.File.OpenStream(false);
	    }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="DragFile"/> class.
		/// </summary>
		/// <param name="file">The file node being dragged.</param>
		/// <param name="mouseButton">The mouse button used to drag.</param>
		internal DragFile(TreeNodeFile file, MouseButtons mouseButton)
			: base(mouseButton)
		{
			FileNode = file;
		}
		#endregion
	}
}
