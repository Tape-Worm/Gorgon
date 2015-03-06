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
		private string _windowText;
		// The content manager interface.
		private readonly IEditorContentManager _contentManager;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
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
			// TODO: Write file system management.

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
					previousPanel.Dispose();
				}

				if (newControl == null)
				{
					return;
				}

				e.Panel.CurrentTheme = Theme;

				panelContentHost.Controls.Add(newControl);

				if (e.Panel.CaptionVisible)
				{
					e.Panel.Padding = new Padding(0, 5, 0, 0);
				}

				e.Panel.Dock = DockStyle.Fill;

				// Initialize rendering if necessary.
				if (e.Panel.UsesRenderer)
				{
					e.Panel.Renderer.CreateResources(e.Panel.RenderControl);
				}

				// If we don't have properties, then we'll have to unhook from the properties grid.
				// And we'll also have to hide it.
				//pageProperties.Enabled = e.Content.HasProperties;
				tabPages.SelectedTab = e.Content.HasProperties ? pageProperties : pageFiles;
			}
			catch
			{
				// If the default panel was not loaded, then put it back.
				_contentManager.CreateContent();

				// Send the exception back so the root event handler will pick it up.
				throw;
			}
		}

		/// <summary>
		/// Function to update the text on this window in a thread-safe manner.
		/// </summary>
		/// <param name="newText">New text to set.</param>
		private void SetWindowText(string newText)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => SetWindowText(newText)));
				return;
			}

			Text = string.Format("{0} - {1}", _windowText, string.IsNullOrWhiteSpace(newText) ? Resources.GOREDIT_TEXT_UNTITLED : newText);
		}

		/// <summary>
		/// Function called to allow sub-classed windows to apply the theme to controls that are not necessarily themeable.
		/// </summary>
		protected override void ApplyTheme()
		{
			tabPages.BackgroundColor = Theme.ContentPanelBackground;
			tabPages.BorderColor = Theme.WindowBackground;
			tabPages.TabBorderColor = Theme.WindowBackground;
			tabPages.TabGradient.ColorEnd = Theme.WindowBackground;
			tabPages.TabGradient.ColorStart = Theme.WindowBackground;
			tabPages.TabGradient.TabPageSelectedTextColor = Theme.HilightBackColor;
			tabPages.TabGradient.TabPageTextColor = Theme.ForeColor;

			propertyGrid.BackColor = Theme.ContentPanelBackground;
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
			propertyGrid.ViewBackColor = Theme.ContentPanelBackground;
			propertyGrid.ViewBorderColor = Theme.ContentPanelBackground;
			propertyGrid.ViewForeColor = Theme.ForeColor;
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
				// Load in the default content here.
				_contentManager.CreateContent();

				SetWindowText(null);

				if (_settings.FormState != FormWindowState.Minimized)
				{
					WindowState = _settings.FormState;
				}

				Location = _settings.WindowDimensions.Location;
				Size = _settings.WindowDimensions.Size;

				// If this window can't be placed on a monitor, then shift it to the primary, 
				// otherwise it'll end up off-screen and it'll just be annoying to try and 
				// bring it back.
				if (!Screen.AllScreens.Any(item => item.Bounds.Contains(Location)))
				{
					Location = Screen.PrimaryScreen.Bounds.Location;
				}
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
					_settings.WindowDimensions = WindowState != FormWindowState.Normal ? RestoreBounds : DesktopBounds;
					_settings.Save();
				}
				catch (Exception ex)
				{
					GorgonException.Catch(ex);
				}
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
		/// <param name="contentManager">The content manager interface.</param>
		[DefaultConstructor]
		public FormMain(IEditorSettings settings, GorgonLogFile log, IEditorContentManager contentManager)
			: this()
		{
			_logFile = log;
			_settings = settings;
			_windowText = Text;
			_contentManager = contentManager;

			_contentManager.ContentCreated += ContentManager_ContentCreatedEvent;
		}
		#endregion
	}
}
