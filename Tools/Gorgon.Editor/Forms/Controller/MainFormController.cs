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
// Created: Tuesday, March 17, 2015 12:12:52 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The controller for the main form of the application.
	/// </summary>
	class MainFormController 
		: IMainFormController
	{
		#region Variables.
		// Controller for the file system sub-view and editor file system model.
		private readonly IFileSystemController _fileSystemController;
		// The application log file.
		private readonly GorgonLogFile _logFile;
		// Data model for the main form.
		private readonly IMainFormModel _model;
		// The view for the main form.
		private readonly IMainFormView _view;
		// The file system service.
		private readonly IFileSystemService _fileSystemService;
		// Application settings.
		private readonly IEditorSettings _settings;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the caption for the main application window.
		/// </summary>
		private void SetWindowText()
		{
			// Update the view to reflect the new file.
			_view.SetWindowText(string.Format("{0} - {1}{2}",
												_model.WindowText,
												_model.CurrentFile.Name,
												_model.CurrentFile.HasChanged ? "*" : string.Empty));
		}

		/// <summary>
		/// Function to determine if the file system is dirty and requires saving.
		/// </summary>
		/// <returns>A confirmation result indicating whether to save the file (if changed), continue the operation, or cancel the operation.</returns>
		private ConfirmationResult ConfirmFileSave()
		{
			var result = ConfirmationResult.No;

			// TODO: Determine if the content is dirty and ask to save that.  If we cancel the content save, then pass that back immediately.
			// TODO: If we chose to save it, then we need to forward that on to the object responsible for saving content.
			// TODO: If we chose to skip saving, then move on to asking to save the file itself.

			if (_model.CurrentFile.HasChanged)
			{
				result = _view.ConfirmFileSave(_model.CurrentFile.Name);
			}

			return result;
		}

		/// <summary>
		/// Handles the Loaded event of the View control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void View_Loaded(object sender, EventArgs eventArgs)
		{
			SetWindowText();

			_view.RestoreViewSettings(_settings);
			_view.BindContentView(null);

			_fileSystemController.CurrentFileSystem = _model.CurrentFile;
		}

		/// <summary>
		/// Handles the ApplicationClose event of the View control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonCancelEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void View_ApplicationClose(object sender, GorgonCancelEventArgs e)
		{
			try
			{
				_logFile.Print("Closing main window", LoggingLevel.Verbose);

				ConfirmationResult result = ConfirmFileSave();

				e.Cancel = result == ConfirmationResult.Cancel;

				if (e.Cancel)
				{
					_logFile.Print("Closing main window cancelled.  User cancelled operation.", LoggingLevel.Verbose);
					return;
				}

				// Turn off any events bound to the view at this point since we're shutting down anyway.
				UnbindView();

				if ((result & ConfirmationResult.Yes) != ConfirmationResult.Yes)
				{
					return;
				}

				// If we have no path, then we need to get a filename.
				if (string.IsNullOrWhiteSpace(_model.CurrentFile.FullName))
				{
					string filePath = _view.SaveFileDialog();

					if (string.IsNullOrWhiteSpace(filePath))
					{
						e.Cancel = true;
						return;
					}

					_model.CurrentFile.FullName = filePath;
				}

				// TODO: Write the functionality to allow us to save the file.
				//_model.CurrentFile.Save();
				_model.CurrentFile.HasChanged = false;
			}
			catch (Exception ex)
			{
				e.Cancel = false;

				// Log any exceptions on shut down of the main form just so we have a 
				// record of things going wrong.
				GorgonException.Catch(ex);

#if DEBUG
				// We won't bother showing anything here outside of DEBUG.
				GorgonDialogs.ErrorBox(null, ex);
#endif
			}
			finally
			{
				// Persist the settings on application shut down - regardless of what happens before.
				// If this fails, just log it and continue on.
				try
				{
					_view.StoreViewSettings(_settings);
					_settings.Save();
				}
				catch (Exception ex)
				{
					GorgonException.Catch(ex);
				}
			}
		}

		/// <summary>
		/// Handles the CreateNewFile event of the View control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void View_CreateNewFile(object sender, EventArgs eventArgs)
		{
			ConfirmationResult result = ConfirmFileSave();

			if (result == ConfirmationResult.Cancel)
			{
				return;
			}

			if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
			{
				// We need to save.
			}

			try
			{
				_view.BindContentView(null);

				_model.CurrentFile = _fileSystemService.NewFile();

				_fileSystemController.CurrentFileSystem = _model.CurrentFile;
			}
			finally
			{
				_settings.LastEditorFile = string.Empty;
				SetWindowText();
			}
		}

		/// <summary>
		/// Function to retrieve the last directory of the most recently saved file.
		/// </summary>
		/// <returns>The directory path to last saved file, or an empty string if the directory was not found.</returns>
		private string GetOpenFileDirectory()
		{
			// Set up the file open dialog to point at the last place we saved a file at.
			if (string.IsNullOrWhiteSpace(_settings.LastEditorFile))
			{
				return string.Empty;
			}

			string lastDirectory = Path.GetDirectoryName(_settings.LastEditorFile);

			if (string.IsNullOrWhiteSpace(lastDirectory))
			{
				return string.Empty;
			}

			var directory = new DirectoryInfo(lastDirectory);

			try
			{
				return !directory.Exists ? string.Empty : directory.FullName;
			}
			catch (Exception ex)
			{
				_logFile.Print("Could not locate the last directory for the file '{0}'. (Error: {1})", LoggingLevel.Verbose, _settings.LastEditorFile, ex.Message);
				return string.Empty;
			}
		}

		/// <summary>
		/// Handles the LoadFile event of the View control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void View_LoadFile(object sender, EventArgs eventArgs)
		{
			ConfirmationResult result = ConfirmFileSave();

			if (result == ConfirmationResult.Cancel)
			{
				return;
			}

			if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
			{
				// We need to save.
			}

			try
			{
				string filePath = _view.LoadFileDialog(GetOpenFileDirectory(), _fileSystemService.ReadFileTypes);

				if (string.IsNullOrWhiteSpace(filePath))
				{
					return;
				}

				// Ensure that we can indeed read the file.
				if (!_fileSystemService.CanReadFile(filePath))
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_CANNOT_LOCATE_PROVIDER, filePath));
				}

				// Create default content.
				_view.BindContentView(null);

				_model.CurrentFile = _fileSystemService.LoadFile(filePath);

				// Bind to a new data model.
				_fileSystemController.CurrentFileSystem = _model.CurrentFile;

				_settings.LastEditorFile = filePath;
			}
			finally
			{
				SetWindowText();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="MainFormController"/> class.
		/// </summary>
		/// <param name="logFile">The application log file.</param>
		/// <param name="settings">The settings for the application.</param>
		/// <param name="fileSystemService">The file system service.</param>
		/// <param name="view">The view for the main form.</param>
		/// <param name="model">The data model for the main form.</param>
		/// <param name="fileSystemController">The controller for the file system sub-view and editor file system model.</param>
		public MainFormController(GorgonLogFile logFile, 
			IEditorSettings settings, 
			IFileSystemService fileSystemService, 
			IMainFormView view, 
			IMainFormModel model,
			IFileSystemController fileSystemController)
		{
			_logFile = logFile;
			_settings = settings;
			_fileSystemService = fileSystemService;
			_fileSystemController = fileSystemController;
			_view = view;
			_model = model;
		}
		#endregion

		#region IMainFormController Implementation.
		#region Properties.
		/// <summary>
		/// Property to return the current view bound to the controller.
		/// </summary>
		public IMainFormView View
		{
			get
			{
				return _view;
			}
		}

		/// <summary>
		/// Property to return the current model bound to the controller.
		/// </summary>
		public IMainFormModel Model
		{
			get
			{
				return _model;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load the previously saved file on start up.
		/// </summary>
		public void LoadPreviousFile()
		{
			if ((!_settings.AutoLoadLastFile)
				|| (string.IsNullOrWhiteSpace(_settings.LastEditorFile))
				|| (!_fileSystemService.HasFileSystemProviders))
			{
				return;
			}

			try
			{
				string filePath = Path.GetFullPath(_settings.LastEditorFile);

				// If the file no longer exists, then it's no cause for alarm.  Just leave.
				if (!File.Exists(filePath))
				{
					_logFile.Print("FileSystemService: Could not auto load the file '{0}'.  The file was not found.", LoggingLevel.Verbose, filePath);
					return;
				}

				if (!_fileSystemService.CanReadFile(filePath))
				{
					_logFile.Print("FileSystemService: Could not auto load the file '{0}'.  No suitable file system provider was found that can read this format.", LoggingLevel.Verbose, filePath);
					return;
				}

				// Load the last file and send it on to the main application.
				_model.CurrentFile = _fileSystemService.LoadFile(filePath);

				SetWindowText();
			}
			catch
			{
				// Reset the last file loaded if we can no longer load it.
				_settings.LastEditorFile = string.Empty;

				// Rethrow so we can notify the user.
				throw;
			}
		}

		/// <summary>
		/// Function to perform view binding with this controller.
		/// </summary>
		public void BindView()
		{
			_view.CreateNewFile += View_CreateNewFile;
			_view.LoadFile += View_LoadFile;
			_view.ApplicationClose += View_ApplicationClose;
			_view.Loaded += View_Loaded;
		}

		/// <summary>
		/// Function to unbind the current view from this controller.
		/// </summary>
		public void UnbindView()
		{
			_view.CreateNewFile -= View_CreateNewFile;
			_view.LoadFile -= View_LoadFile;
			_view.ApplicationClose -= View_ApplicationClose;
		}
		#endregion
		#endregion
	}
}
