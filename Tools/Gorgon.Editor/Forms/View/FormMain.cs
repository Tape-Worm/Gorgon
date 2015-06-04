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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.UI;
using StructureMap;

namespace Gorgon.Editor
{
	/// <summary>
	/// Primary application window.
	/// </summary>
	sealed partial class FormMain 
		: FlatForm, IMainFormView
	{
		#region Variables.
		// The log file for the application.
		private readonly GorgonLogFile _logFile;
		// The content manager interface.
		private readonly IEditorContentManager _contentManager;
		// The default content model.
		private IContentModel _defaultContent;
		// The current content.
		private IContentModel _currentContent;
		#endregion

		#region Validation Methods.
		/// <summary>
		/// Function to validate the properties tab depending on the current content.
		/// </summary>
		private void ValidatePropertiesTab()
		{
			pageProperties.Enabled = _currentContent != null && _currentContent.Content.HasProperties;
		}

		/// <summary>
		/// Function to validate the file menu.
		/// </summary>
		private void ValidateFileMenu()
		{
			// TODO: Fix this, put validation in the controller.
			itemOpen.Enabled = true;//_fileSystemService.HasFileSystemProviders;
		}

		/// <summary>
		/// Function to validate the controls on this form.
		/// </summary>
		private void ValidateControls()
		{
			ValidateFileMenu();
			ValidatePropertiesTab();
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
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (LoadFile != null)
				{
					LoadFile(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), true);

				if (CreateNewFile != null)
				{
					CreateNewFile(this, EventArgs.Empty);
				}
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
				if (CreateNewFile != null)
				{
					CreateNewFile(this, EventArgs.Empty);
				}

			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), true);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursor.Current;
			}
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

		/*/// <summary>
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
					LoadDefaultContent();
				}
				catch(Exception ex)
				{
					_logFile.Print("ContentService: Failure loading the default content after exception.", LoggingLevel.Simple);
					GorgonApplication.Log.LogException(ex);
				}

				// Send the exception back so the root event handler will pick it up.
				throw;
			}
		}*/

		/// <summary>
		/// Function called to allow sub-classed windows to apply the theme to controls that are not necessarily themeable.
		/// </summary>
		protected override void ApplyTheme()
		{
			var editorTheme = (EditorTheme)Theme;

			splitMain.BackColor = editorTheme.WindowBackground;
			splitMain.Panel2.BackColor = editorTheme.ContentPanelBackground;
			splitMain.Panel1.BackColor = editorTheme.ContentPanelBackground;
			
			tabPages.BackgroundColor = editorTheme.ContentPanelBackground;
			tabPages.BorderColor = editorTheme.WindowBackground;
			tabPages.TabBorderColor = editorTheme.WindowBackground;
			tabPages.TabGradient.ColorEnd = editorTheme.WindowBackground;
			tabPages.TabGradient.ColorStart = editorTheme.WindowBackground;
			tabPages.TabGradient.TabPageSelectedTextColor = editorTheme.HilightBackColor;
			tabPages.TabGradient.TabPageTextColor = editorTheme.ForeColor;

			propertyGrid.BackColor = editorTheme.PropertyPanelBackgroundColor;
			propertyGrid.CategoryForeColor = editorTheme.ForeColor;
			propertyGrid.CommandsActiveLinkColor = editorTheme.HilightForeColor;
			propertyGrid.CommandsBackColor = propertyGrid.BackColor;
			propertyGrid.CommandsBorderColor = editorTheme.WindowBackground;
			propertyGrid.CommandsDisabledLinkColor = editorTheme.DisabledColor;
			propertyGrid.CommandsForeColor = editorTheme.ForeColor;
			propertyGrid.CommandsLinkColor = editorTheme.HilightBackColor;
			propertyGrid.DisabledItemForeColor = editorTheme.DisabledColor;
			propertyGrid.HelpBackColor = editorTheme.WindowBackground;
			propertyGrid.HelpBorderColor = editorTheme.WindowBackground;
			propertyGrid.HelpForeColor = editorTheme.ForeColor;
			propertyGrid.LineColor = editorTheme.WindowBackground;
			propertyGrid.SelectedItemWithFocusBackColor = editorTheme.HilightBackColor;
			propertyGrid.SelectedItemWithFocusForeColor = editorTheme.HilightForeColor;
			propertyGrid.ViewBackColor = propertyGrid.BackColor;
			propertyGrid.ViewBorderColor = propertyGrid.BackColor;
			propertyGrid.ViewForeColor = editorTheme.ForeColor;
			propertyGrid.CategorySplitterColor = editorTheme.WindowBackground;

			labelUnCollapse.BackColor = editorTheme.WindowBackground;

			treeFileSystem.Theme = editorTheme;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				tabPages.SelectedTab = pageFiles;

				// Load in the default content here.
				_defaultContent = _contentManager.CreateContent();

				if (Loaded != null)
				{
					Loaded(this, EventArgs.Empty);
				}
				
				ValidateControls();
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), true);
				GorgonApplication.Quit();
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

			try
			{
				var args = new GorgonCancelEventArgs(false);

				if (ApplicationClose != null)
				{
					ApplicationClose(this, args);
				}

				// Shut down the content manager.
				e.Cancel = args.Cancel;
				if (args.Cancel)
				{
					return;
				}
				
				_contentManager.Dispose();
			}
			finally
			{
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
		/// <param name="log">The log file for the application.</param>
		/// <param name="contentManager">The content manager interface.</param>
		[DefaultConstructor]
		public FormMain(GorgonLogFile log, IEditorContentManager contentManager)
			: this()
		{
			_logFile = log;
			_contentManager = contentManager;
		}
		#endregion

		#region IMainFormView Members
		#region Events.
		/// <summary>
		/// Event fired when the new file item is clicked.
		/// </summary>
		public event EventHandler CreateNewFile;

		/// <summary>
		/// Event fired when a file has been selected to load.
		/// </summary>
		public event EventHandler LoadFile;

		/// <summary>
		/// Event fired when the close button is clicked on the application window.
		/// </summary>
		public event EventHandler<GorgonCancelEventArgs> ApplicationClose;

		/// <summary>
		/// Event fired when the view is loaded.
		/// </summary>
		public event EventHandler Loaded;
		#endregion

		#region Methods
		/// <summary>
		/// Function to open the load file dialog.
		/// </summary>
		/// <param name="defaultDirectory">The default directory for the dialog.</param>
		/// <param name="fileExtensions">The applicable file extensions for the dialog.</param>
		/// <returns>
		/// The path to the selected file to load, or NULL (<i>Nothing</i> in VB.Net) if Cancel was selected.
		/// </returns>
		public string LoadFileDialog(string defaultDirectory, string fileExtensions)
		{
			var lastCursor = Cursor.Current;

			try
			{
				Cursor.Current = Cursors.Default;

				dialogOpenFile.InitialDirectory = defaultDirectory;
				dialogOpenFile.FileName = string.Empty;
				dialogOpenFile.Filter = fileExtensions;

				return dialogOpenFile.ShowDialog(this) != DialogResult.OK ? null : dialogOpenFile.FileName;
			}
			finally
			{
				Cursor.Current = lastCursor;
			}
		}

		/// <summary>
		/// Function to open the save file dialog.
		/// </summary>
		/// <returns>
		/// The path to the file to save, or NULL (<i>Nothing</i> in VB.Net) if Cancel was selected.
		/// </returns>
		public string SaveFileDialog()
		{
			// TODO: Implement this.
			return null;
		}

		/// <summary>
		/// Function to ask the user whether to save the file or not.
		/// </summary>
		/// <param name="fileName">Name of the file to save.</param>
		/// <returns>
		/// A confirmation result indicating whether to save the file, discard the changes, or cancel the operation.
		/// </returns>
		public ConfirmationResult ConfirmFileSave(string fileName)
		{
			var lastCursor = Cursor.Current;

			try
			{
				Cursor.Current = Cursors.Default;

				return GorgonDialogs.ConfirmBox(this,
				                                string.Format(Resources.GOREDIT_DLG_FILE_NOT_SAVED, fileName),
				                                null,
				                                true);
			}
			finally
			{
				Cursor.Current = lastCursor;
			}
		}

		/// <summary>
		/// Function to set the text for the application window.
		/// </summary>
		/// <param name="newCaption">New caption to set.</param>
		public void SetWindowText(string newCaption)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => SetWindowText(newCaption)));
				return;
			}

			Text = newCaption;
		}

		/// <summary>
		/// Function to load the content view into the main interface.
		/// </summary>
		/// <param name="view">View to load.</param>
		public void BindContentView(IContentPanel view)
		{
			// TODO: Set up a controller for the content view/model that is accessed through the left split panel child panel.
			if (view == null)
			{
				view = _defaultContent.View;
			}

			// If we already have the default content loaded, then leave.
			if (_currentContent != null)
			{
				_logFile.Print("Unloading current content '{0}'.", LoggingLevel.Verbose, _currentContent.Content.Name);

				_currentContent.Dispose();
				_currentContent = null;
			}
			else
			{
				// We already have the default panel loaded, leave.
				if ((panelContentHost.Controls.Count > 0)
					&& (panelContentHost.Controls[0] == view))
				{
					return;
				}
			}

			var viewControl = view as Control;

			Debug.Assert(viewControl != null, "View is not a win forms control.  Cannot load into main window.");

			_logFile.Print("Setting theme for content view.", LoggingLevel.Verbose);
			view.CurrentTheme = Theme;

			_logFile.Print("Injecting content view into main view.", LoggingLevel.Verbose);
			panelContentHost.Controls.Add(viewControl);

			if (view.CaptionVisible)
			{
				view.Padding = new Padding(0, 5, 0, 0);
			}

			view.Dock = DockStyle.Fill;

			// Initialize rendering if necessary.
			_logFile.Print("Content view supports renderer: {0}.", LoggingLevel.Verbose, view.UsesRenderer);
			if (!view.UsesRenderer)
			{
				return;
			}

			_logFile.Print("Creating renderer for view.", LoggingLevel.Verbose);
			view.Renderer.CreateResources(view.RenderControl);

			// Begin rendering.
			view.Renderer.StartRendering();

			/*// If we don't have properties, then we'll have to unhook from the properties grid.
			// And we'll also have to hide it.
			_logFile.Print("Content supports properties: {1}.", LoggingLevel.Verbose, view.HasProperties);
			tabPages.SelectedTab = e.Content.HasProperties ? pageProperties : pageFiles;*/
		}

		/// <summary>
		/// Function to store the view state in the settings object.
		/// </summary>
		/// <param name="settings">Settings object to update.</param>
		public void StoreViewSettings(IEditorSettings settings)
		{
			settings.FormState = WindowState != FormWindowState.Minimized ? WindowState : FormWindowState.Normal;
			settings.WindowDimensions = WindowState != FormWindowState.Normal ? RestoreBounds : DesktopBounds;
			settings.SplitPosition = splitMain.SplitterDistance;
			settings.PropertiesVisible = !splitMain.Panel2Collapsed;
		}

		/// <summary>
		/// Function to recall the view state from the settings object.
		/// </summary>
		/// <param name="settings">Settings object to use when restoring view state.</param>
		public void RestoreViewSettings(IEditorSettings settings)
		{
			Rectangle windowRect = settings.WindowDimensions;

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
			if (settings.FormState != FormWindowState.Minimized)
			{
				WindowState = settings.FormState;
			}

			if (settings.SplitPosition > splitMain.Panel1MinSize)
			{
				splitMain.SplitterDistance = settings.SplitPosition;
			}

			labelUnCollapse.Visible = splitMain.Panel2Collapsed = !settings.PropertiesVisible;
		}

		/// <summary>
		/// Function to retrieve a specific sub-view from the main view.
		/// </summary>
		/// <typeparam name="T">Type of view to retrieve.</typeparam>
		/// <returns>
		/// The view, if found, NULL (<i>Nothing</i> in VB.Net) if not.
		/// </returns>
		public T GetView<T>()
		{
			if (typeof(T) != typeof(IFileSystemView))
			{
				return default(T);
			}

			IFileSystemView fsView = treeFileSystem;
			return (T)fsView;
		}
		#endregion
		#endregion
	}
}
