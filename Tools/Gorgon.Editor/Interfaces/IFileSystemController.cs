#region MIT.
//  
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
// ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Created: 03/18/2015 10:24 PM
// 
// 
#endregion

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The controller for the file system to synchronize between the editor file system object and the UI.
	/// </summary>
	interface IFileSystemController
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the currently active file system.
		/// </summary>
		IEditorFileSystem CurrentFileSystem
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the view based on model changes or view changes.
		/// </summary>
		void Refresh();
		#endregion
	}
}