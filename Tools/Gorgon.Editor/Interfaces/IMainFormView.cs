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
// Created: 03/17/2015 12:24 AM
// 
// 
#endregion

using System;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The view for the main form.
	/// </summary>
	interface IMainFormView
	{
		#region Events.
		/// <summary>
		/// Event fired when the new file item is clicked.
		/// </summary>
		event EventHandler CreateNewFile;
		/// <summary>
		/// Event fired when a file has been selected to load.
		/// </summary>
		event EventHandler LoadFile;
		/// <summary>
		/// Event fired when the close button is clicked on the application window.
		/// </summary>
		event EventHandler<GorgonCancelEventArgs> ApplicationClose;
		/// <summary>
		/// Event fired when the view is loaded.
		/// </summary>
		event EventHandler Loaded;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to open the load file dialog.
		/// </summary>
		/// <param name="defaultDirectory">The default directory for the dialog.</param>
		/// <param name="fileExtensions">The applicable file extensions for the dialog.</param>
		/// <returns>The path to the selected file to load, or NULL (Nothing in VB.Net) if Cancel was selected.</returns>
		string LoadFileDialog(string defaultDirectory, string fileExtensions);

		/// <summary>
		/// Function to open the save file dialog.
		/// </summary>
		/// <returns>The path to the file to save, or NULL (Nothing in VB.Net) if Cancel was selected.</returns>
		string SaveFileDialog();

		/// <summary>
		/// Function to ask the user whether to save the file or not.
		/// </summary>
		/// <param name="fileName">Name of the file to save.</param>
		/// <returns>A confirmation result indicating whether to save the file, discard the changes, or cancel the operation.</returns>
		ConfirmationResult ConfirmFileSave(string fileName);

		/// <summary>
		/// Function to set the text for the application window.
		/// </summary>
		/// <param name="newCaption">New caption to set.</param>
		void SetWindowText(string newCaption);

		/// <summary>
		/// Function to bind the content view to the application form.
		/// </summary>
		/// <param name="view">View to bind.</param>
		void BindContentView(IContentPanel view);

		/// <summary>
		/// Function to store the view state in the settings object.
		/// </summary>
		/// <param name="settings">Settings object to update.</param>
		void StoreViewSettings(IEditorSettings settings);

		/// <summary>
		/// Function to recall the view state from the settings object.
		/// </summary>
		/// <param name="settings">Settings object to use when restoring view state.</param>
		void RestoreViewSettings(IEditorSettings settings);

		/// <summary>
		/// Function to retrieve a specific sub-view from the main view.
		/// </summary>
		/// <typeparam name="T">Type of view to retrieve.</typeparam>
		/// <returns>The view, if found, NULL (Nothing in VB.Net) if not.</returns>
		T GetView<T>();
		#endregion
	}
}