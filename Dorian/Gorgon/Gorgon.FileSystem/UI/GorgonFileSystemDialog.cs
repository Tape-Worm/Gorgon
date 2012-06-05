#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Thursday, May 31, 2012 12:38:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.FileSystem;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// View modes for the file dialog.
	/// </summary>
	public enum FileDialogView
	{
		/// <summary>
		/// Details view.
		/// </summary>
		Details = 0,
		/// <summary>
		/// List view.
		/// </summary>
		List = 1,
		/// <summary>
		/// Icon view.
		/// </summary>
		Icons = 2
	}

	/// <summary>
	/// Base object for the file system dialogs.
	/// </summary>
	public abstract class GorgonFileSystemDialog
		: IDisposable
	{
		#region Variables.
		private bool _isOpen = true;									// Is an open file system dialog.
		private GorgonFileSystem _fileSystem = null;					// File system.
		private bool _disposed = false;									// Flag to indicate that the object was disposed.
		private Size _lastDimensions = Size.Empty;						// Last window dimensions.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the dialog window.
		/// </summary>
		internal formGorgonFileDialog DialogWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the file system to use.
		/// </summary>
		public GorgonFileSystem FileSystem
		{
			get
			{
				return _fileSystem;
			}
			set
			{
				if (value == null)
					return;

				if (value.RootDirectory == null)
					throw new System.IO.DirectoryNotFoundException("The file system root has not been assigned.");
			}
		}

		/// <summary>
		/// Property to set or return the view for the dialog.
		/// </summary>
		public FileDialogView View
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the text for the dialog.
		/// </summary>
		public string Text
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the file name for the dialog.
		/// </summary>
		/// <remarks>In the case of multiple selection, this is only the first item on the list.</remarks>
		public string Filename
		{
			get
			{
				return DialogWindow.FileName;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				DialogWindow.FileName = value;
			}
		}

		/// <summary>
		/// Property to return the list of file names selected.
		/// </summary>
		/// <remarks>This is only valid when <see cref="GorgonLibrary.UI.GorgonFileSystemOpenFile.AllowMultiSelect">AllowMultiSelect</see> is TRUE.  Otherwise it'll just return the first entry in the list.</remarks>
		public IList<string> Filenames
		{
			get
			{
				return DialogWindow.FileNames;
			}
		}

		/// <summary>
		/// Property to set or return the default extension.
		/// </summary>
		public string DefaultExtension
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the initial directory.
		/// </summary>
		public string InitialDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the window size.
		/// </summary>
		public Size WindowSize
		{
			get
			{
				return DialogWindow.Size;
			}
			set
			{
				DialogWindow.Size = value;
			}
		}

		/// <summary>
		/// Property to set or return the filter used for file name extensions.
		/// </summary>
		public string Filter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the filter index.
		/// </summary>
		public int FilterIndex
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to parse the filter property.
		/// </summary>
		private void GetFilters()
		{
			DialogWindow.FileExtensions.Clear();
			if (string.IsNullOrEmpty(Filter))
			{
				Filter = string.Empty;
				DialogWindow.FileExtensions.Add(new GorgonFileExtension("*.*", "All Files"));
				return;
			}

			string[] filters = Filter.Split(new char[] { '|' });

			if ((filters.Length % 2) != 0)
				throw new IndexOutOfRangeException("There should be an extension for each description.  The filter should be formatted as follows: <description>|*.ext[;*.ext]");

			for (int i = 0; i < filters.Length; i += 2)
			{
				string description = filters[i];
				string[] extensions = filters[i + 1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

				if (extensions.Length == 0)
					extensions = new string[] { "*." };

				GorgonFileExtension extension = new GorgonFileExtension();
				extension.Description = description;
				extension.Extension = extensions;
				DialogWindow.FileExtensions.Add(extension);
			}

			DialogWindow.FilterIndex = FilterIndex;
		}

		/// <summary>
		/// Function to initialize the window.
		/// </summary>
		protected abstract void InitWindow();

		/// <summary>
		/// Function to show the file open dialog.
		/// </summary>
		/// <returns>A dialog result for the dialog.</returns>
		public System.Windows.Forms.DialogResult ShowDialog()
		{
			return ShowDialog(System.Windows.Forms.Form.ActiveForm);
		}

		/// <summary>
		/// Function to show the file open dialog.
		/// </summary>
		/// <param name="owner">Owner of the window.</param>
		/// <returns>A dialog result for the dialog.</returns>
		public System.Windows.Forms.DialogResult ShowDialog(System.Windows.Forms.IWin32Window owner)
		{
			System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.None;

			DialogWindow.Text = Text;
			DialogWindow.FileSystem = FileSystem;
			DialogWindow.DefaultExtension = DefaultExtension;
			DialogWindow.InitialDirectory = InitialDirectory;
			DialogWindow.DialogView = View;

			InitWindow();
			GetFilters();

			if ((_lastDimensions.Width > 0) && (_lastDimensions.Height > 0))
				DialogWindow.Size = _lastDimensions;

			result = DialogWindow.ShowDialog(owner);

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				if (string.IsNullOrEmpty(InitialDirectory))
					InitialDirectory = DialogWindow.CurrentDirectory;
				_lastDimensions = DialogWindow.Size;
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemDialog"/> class.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <param name="isOpenDialog">TRUE if this file system is meant for an open file or FALSE for a save file dialog.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the file system has had a directory or packed file mounted.</exception>
		protected GorgonFileSystemDialog(GorgonFileSystem fileSystem, bool isOpenDialog)
		{
			GorgonDebug.AssertNull<GorgonFileSystem>(fileSystem, "fileSystem");

			if (fileSystem.RootDirectory == null)
				throw new ArgumentException("The file system has not mounted a root directory.", "fileSystem");

			_fileSystem = fileSystem;
			DialogWindow = new formGorgonFileDialog(FileSystem, isOpenDialog);
						
			DefaultExtension = string.Empty;
			FilterIndex = 0;
			View = FileDialogView.Details;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (DialogWindow != null)
						DialogWindow.Dispose();
				}

				DialogWindow = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
