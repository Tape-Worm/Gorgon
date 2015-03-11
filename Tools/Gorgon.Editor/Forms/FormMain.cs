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
// Created: Tuesday, February 10, 2015 11:14:19 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.UI;
using StructureMap;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Primary application window.
	/// </summary>
	sealed partial class FormMain 
		: FlatForm
	{
		#region Variables.
		// Settings for the editor.
		private readonly IEditorSettings _settings;
		// The log file for the application.
		private readonly GorgonLogFile _logFile;
		// The window text.
		private readonly string _windowText;
		// The content manager interface.
		private readonly IEditorContentManager _contentManager;
		// The file system service.
		private readonly IFileSystemService _fileSystemService;
		#endregion

		#region Properties.

		#endregion

		#region Validation Methods.
		/// <summary>
		/// Function to validate the file menu.
		/// </summary>
		private void ValidateFileMenu()
		{
			itemOpen.Enabled = _fileSystemService.HasFileSystemProviders;
		}

		/// <summary>
		/// Function to validate the controls on this form.
		/// </summary>
		private void ValidateControls()
		{
			ValidateFileMenu();
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the itemOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemOpen_Click(object sender, EventArgs e)
		{
			try
			{
				// Set up the file open dialog to point at the last place we saved a file at.
				if (!string.IsNullOrWhiteSpace(_settings.LastEditorFile))
				{
					string lastDirectory = Path.GetDirectoryName(_settings.LastEditorFile);

					if (string.IsNullOrWhiteSpace(lastDirectory))
					{
						return;
					}

					var directory = new DirectoryInfo(lastDirectory);

					if (directory.Exists)
					{
						dialogOpenFile.InitialDirectory = directory.FullName;
					}
				}

				dialogOpenFile.FileName = string.Empty;
				dialogOpenFile.Filter = _fileSystemService.ReadFileTypes;
				if (dialogOpenFile.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				// Ensure that we can indeed read the file.
				if (!_fileSystemService.CanReadFile(dialogOpenFile.FileName))
				{
					GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_ERR_CANNOT_LOCATE_PROVIDER, dialogOpenFile.SafeFileName));
					return;
				}

				// Unload the current file if necessary.
				UnloadData();

				_fileSystemService.LoadFile(dialogOpenFile.FileName);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));

				// Reload the default content if we had something catastrophic happen.
				// Let's hope it never executes this.
				_contentManager.CreateContent();
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the itemNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemNew_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				UnloadData();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursor.Current;
			}
		}

		/// <summary>
		/// Handles the FileUpdated event of the FileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void FileSystem_FileUpdated(object sender, EventArgs eventArgs)
		{
			SetWindowText();
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the labelUnCollapse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelUnCollapse_Click(object sender, EventArgs e)
		{
			labelUnCollapse.Visible = splitMain.Panel2Collapsed = false;
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelUnCollapse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelUnCollapse_MouseEnter(object sender, EventArgs e)
		{
			labelUnCollapse.BackColor = Theme.HilightBackColor;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelUnCollapse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelUnCollapse_MouseLeave(object sender, EventArgs e)
		{
			labelUnCollapse.BackColor = Theme.WindowBackground;
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the splitMain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void splitMain_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			labelUnCollapse.Visible = splitMain.Panel2Collapsed = true;
		}

		/// <summary>
		/// Handles the Click event of the exitToolStripMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				Close();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to unload the file system data and the content data with save confirmation.
		/// </summary>
		/// <returns>TRUE if the data was unloaded, FALSE if cancelled by the user.</returns>
		private bool UnloadData()
		{
			// First ensure that we want to save the current content.
			if (!_contentManager.CloseContent())
			{
				return false;
			}
			
			// Next, confirm whether to save the currently open file system.
			// TODO: Write file system management "has changes" functionality and also validate against a flag to indicate whether we have any file system writers loaded or not.

			ConfirmationResult result = ConfirmationResult.None;

			// If we have changes, then ask if we want to save them.
			if (_fileSystemService.HasChanges)
			{
				var lastCursor = Cursor.Current;

				result = GorgonDialogs.ConfirmBox(this,
				                                  string.Format(Resources.GOREDIT_DLG_FILE_NOT_SAVED, _fileSystemService.CurrentFile),
				                                  null,
				                                  true);
				Cursor.Current = lastCursor;
			}

			switch (result)
			{
				case ConfirmationResult.Cancel:
					return false;
				case ConfirmationResult.Yes:
					if (string.IsNullOrWhiteSpace(_fileSystemService.CurrentFilePath))
					{
						// TODO: We need to "Save as".
					}
					else
					{
						// TODO: We need to just "Save".
					}
					break;
			}

			_fileSystemService.UnloadCurrentFile();

			return true;
		}

		/// <summary>
		/// Handles the ContentCreatedEvent event of the ContentManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Editor.ContentCreatedEventArgs"/> instance containing the event data.</param>
		private void ContentManager_ContentCreatedEvent(object sender, ContentCreatedEventArgs e)
		{
			try
			{
				Control newControl = e.Panel as Control;
				IContentPanel previousPanel = null;
				
				if (panelContentHost.Controls.Count > 0)
				{
					previousPanel = panelContentHost.Controls[0] as IContentPanel;
				}

				if (previousPanel != null)
				{
					_logFile.Print("ContentService: Removing previous content panel.", LoggingLevel.Verbose);
					previousPanel.Dispose();
				}

				if (newControl == null)
				{
					_logFile.Print("ContentService: NO CONTENT SPECIFIED!!!", LoggingLevel.Simple);
					return;
				}

				_logFile.Print("ContentService: Setting theme for content panel '{0}'.", LoggingLevel.Verbose, newControl.Name);
				e.Panel.CurrentTheme = Theme;

				_logFile.Print("ContentService: Adding content panel '{0}'.", LoggingLevel.Verbose, newControl.Name);
				panelContentHost.Controls.Add(newControl);

				if (e.Panel.CaptionVisible)
				{
					e.Panel.Padding = new Padding(0, 5, 0, 0);
				}

				e.Panel.Dock = DockStyle.Fill;

				// Initialize rendering if necessary.
				if (e.Panel.UsesRenderer)
				{
					_logFile.Print("ContentService: Content panel '{0}' supports renderer: True.", LoggingLevel.Verbose, newControl.Name);
					_logFile.Print("ContentService: Creating renderer for '{0}'.", LoggingLevel.Verbose, newControl.Name);
					e.Panel.Renderer.CreateResources(e.Panel.RenderControl);
				}

				// If we don't have properties, then we'll have to unhook from the properties grid.
				// And we'll also have to hide it.
				_logFile.Print("ContentService: Content '{0}' supports properties: {1}.", LoggingLevel.Verbose, e.Content.Name, e.Content.HasProperties);
				pageProperties.Enabled = e.Content.HasProperties;
				tabPages.SelectedTab = e.Content.HasProperties ? pageProperties : pageFiles;
			}
			catch
			{
				// If the default panel was not loaded, then put it back.
				try
				{
					_contentManager.CreateContent();
				}
				catch(Exception ex)
				{
					_logFile.Print("ContentService: Failure loading the default content after exception.", LoggingLevel.Simple);
					GorgonException.Catch(ex);
				}

				// Send the exception back so the root event handler will pick it up.
				throw;
			}
		}

		/// <summary>
		/// Function to update the text on this window in a thread-safe manner.
		/// </summary>
		private void SetWindowText()
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(SetWindowText));
				return;
			}

			Text = string.Format("{0} - {1}{2}",
			                     _windowText,
			                     string.IsNullOrWhiteSpace(_fileSystemService.CurrentFile) ? Resources.GOREDIT_TEXT_UNTITLED : _fileSystemService.CurrentFile, 
								 _fileSystemService.HasChanges ? "*" : string.Empty);  // TODO: Replace this with a "HasChanges" flag.
		}

		/// <summary>
		/// Function called to allow sub-classed windows to apply the theme to controls that are not necessarily themeable.
		/// </summary>
		protected override void ApplyTheme()
		{
			splitMain.BackColor = Theme.WindowBackground;
			splitMain.Panel2.BackColor = Theme.ContentPanelBackground;
			splitMain.Panel1.BackColor = Theme.ContentPanelBackground;
			
			tabPages.BackgroundColor = Theme.ContentPanelBackground;
			tabPages.BorderColor = Theme.WindowBackground;
			tabPages.TabBorderColor = Theme.WindowBackground;
			tabPages.TabGradient.ColorEnd = Theme.WindowBackground;
			tabPages.TabGradient.ColorStart = Theme.WindowBackground;
			tabPages.TabGradient.TabPageSelectedTextColor = Theme.HilightBackColor;
			tabPages.TabGradient.TabPageTextColor = Theme.ForeColor;

			propertyGrid.BackColor = ((EditorTheme)Theme).PropertyPanelBackgroundColor;
			propertyGrid.CategoryForeColor = Theme.ForeColor;
			propertyGrid.CommandsActiveLinkColor = Theme.HilightForeColor;
			propertyGrid.CommandsBackColor = propertyGrid.BackColor;
			propertyGrid.CommandsBorderColor = Theme.WindowBackground;
			propertyGrid.CommandsDisabledLinkColor = Theme.DisabledColor;
			propertyGrid.CommandsForeColor = Theme.ForeColor;
			propertyGrid.CommandsLinkColor = Theme.HilightBackColor;
			propertyGrid.DisabledItemForeColor = Theme.DisabledColor;
			propertyGrid.HelpBackColor = Theme.WindowBackground;
			propertyGrid.HelpBorderColor = Theme.WindowBackground;
			propertyGrid.HelpForeColor = Theme.ForeColor;
			propertyGrid.LineColor = Theme.WindowBackground;
			propertyGrid.SelectedItemWithFocusBackColor = Theme.HilightBackColor;
			propertyGrid.SelectedItemWithFocusForeColor = Theme.HilightForeColor;
			propertyGrid.ViewBackColor = propertyGrid.BackColor;
			propertyGrid.ViewBorderColor = propertyGrid.BackColor;
			propertyGrid.ViewForeColor = Theme.ForeColor;
			propertyGrid.CategorySplitterColor = Theme.WindowBackground;

			labelUnCollapse.BackColor = Theme.WindowBackground;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			Rectangle windowRect;

			base.OnLoad(e);

			try
			{
				// Load in the default content here.
				_contentManager.CreateContent();

				SetWindowText();

				windowRect = _settings.WindowDimensions;

				// Find the screen that contains our window.
				var currentScreen = Screen.AllScreens.FirstOrDefault(item => item.Bounds.Contains(windowRect.Location));

				if (currentScreen == null)
				{
					currentScreen = Screen.PrimaryScreen;
					windowRect.Location = currentScreen.Bounds.Location;
				}

				// Ensure that the window fits within the screen.
				windowRect.Intersect(currentScreen.WorkingArea);

				Location = windowRect.Location;
				Size = windowRect.Size;

				// Set the current state of the window.
				if (_settings.FormState != FormWindowState.Minimized)
				{
					WindowState = _settings.FormState;
				}

				if (_settings.SplitPosition > splitMain.Panel1MinSize)
				{
					splitMain.SplitterDistance = _settings.SplitPosition;
				}

				labelUnCollapse.Visible = splitMain.Panel2Collapsed = !_settings.PropertiesVisible;

				_fileSystemService.FileUnloaded += FileSystem_FileUpdated;
				_fileSystemService.FileLoaded += FileSystem_FileUpdated;

				ValidateControls();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Gorgon.Quit();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			Cursor.Current = Cursors.WaitCursor;

			_logFile.Print("Closing main window '{0}'", LoggingLevel.Verbose, Text);

			try
			{
				// Shut down the content manager.
				if (!UnloadData())
				{
					e.Cancel = true;
					_logFile.Print("Closing main window cancelled.  User cancelled operation.", LoggingLevel.Verbose, Text);
					return;
				}

				_fileSystemService.FileLoaded -= FileSystem_FileUpdated;
				_fileSystemService.FileSaved -= FileSystem_FileUpdated;
				_contentManager.ContentCreated -= ContentManager_ContentCreatedEvent;
				_contentManager.Dispose();
			}
			catch (Exception ex)
			{
				e.Cancel = false;

				// Log any exceptions on shut down of the main form just so we have a 
				// record of things going wrong.
				GorgonException.Catch(ex);

#if DEBUG
				// We won't bother showing anything here outside of DEBUG.
				GorgonDialogs.ErrorBox(this, ex);
#endif
			}
			finally
			{
				// Persist the settings on application shut down - regardless of what happens before.
				// If this fails, just log it and continue on.
				try
				{
					_settings.FormState = WindowState != FormWindowState.Minimized ? WindowState : FormWindowState.Normal;
					_settings.WindowDimensions = WindowState != FormWindowState.Normal ? RestoreBounds : DesktopBounds;
					_settings.SplitPosition = splitMain.SplitterDistance;
					_settings.PropertiesVisible = !splitMain.Panel2Collapsed;
					_settings.Save();
				}
				catch (Exception ex)
				{
					GorgonException.Catch(ex);
				}

				Cursor.Current = Cursors.Default;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormMain"/> class.
		/// </summary>
		public FormMain()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FormMain"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="log">The log file for the application.</param>
		/// <param name="fileSystemService">The file system service.</param>
		/// <param name="contentManager">The content manager interface.</param>
		[DefaultConstructor]
		public FormMain(GorgonLogFile log, IEditorSettings settings, IFileSystemService fileSystemService, IEditorContentManager contentManager)
			: this()
		{
			_logFile = log;
			_settings = settings;
			_windowText = Text;
			_contentManager = contentManager;
			_fileSystemService = fileSystemService;

			_contentManager.ContentCreated += ContentManager_ContentCreatedEvent;
		}
		#endregion
	}
}
