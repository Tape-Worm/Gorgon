#region MIT.
// 
// Gorgon.
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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, March 18, 2015 10:23:40 PM
// 
#endregion

using Gorgon.Diagnostics;

namespace Gorgon.Editor
{
	/// <summary>
	/// The controller used to negotiate between the data model (the file system) and the tree view.
	/// </summary>
	class FileSystemTreeController
		: IFileSystemController
	{
		#region Variables.
		// The application log file.
		private GorgonLogFile _logFile;
		// The view bound to the controller.
		private readonly IFileSystemView _view;
		// The model bound to the controller.
		private IEditorFileSystem _model;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Handles the GetNodeState event of the View control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GetNodeStateEventArgs"/> instance containing the event data.</param>
		private void View_GetNodeState(object sender, GetNodeStateEventArgs e)
		{
			switch (e.NodeType)
			{
				case NodeType.Root:
					e.HasChanges = _model.HasChanged;
					break;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemTreeController"/> class.
		/// </summary>
		/// <param name="logFile">The application log file.</param>
		/// <param name="view">The view for the file system.</param>
		public FileSystemTreeController(GorgonLogFile logFile, IFileSystemView view)
		{
			_logFile = logFile;
			_view = view;
			_view.GetNodeState += View_GetNodeState;
		}
		#endregion

		#region IFileSystemTreeController
		#region Properties.
		/// <summary>
		/// Property to set or return the currently active file system.
		/// </summary>
		public IEditorFileSystem CurrentFileSystem
		{
			get
			{
				return _model;
			}
			set
			{
				_model = value;
				Refresh();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the view based on model changes or view changes.
		/// </summary>
		public void Refresh()
		{
			if (_view == null)
			{
				return;
			}

			// Function to clear all items from the view.
			_view.Clear();

			if (_model == null)
			{
				return;
			}

			_view.AddFileSystemRoot(_model.Name);
		}
		#endregion
		#endregion
	}
}
