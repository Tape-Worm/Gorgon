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
// Created: Monday, April 30, 2012 6:28:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Context for the file menu.
	/// </summary>
	public enum FileMenuContext
	{
		/// <summary>
		/// No context.
		/// </summary>
		None = 0,
		/// <summary>
		/// New menu item.
		/// </summary>
		New = 1,
		/// <summary>
		/// Open menu item.
		/// </summary>
		Open = 2,
		/// <summary>
		/// Save menu item.
		/// </summary>
		Save = 3
	}

	/// <summary>
	/// Type of editor.
	/// </summary>
	public enum EditorType
	{
		/// <summary>
		/// No editor.
		/// </summary>
		None = 0,
		/// <summary>
		/// Font editor.
		/// </summary>
		Font = 1
	}

	/// <summary>
	/// Main application object.
	/// </summary>
	public partial class formMain 
		: Form
	{
		#region Variables.
		private FileMenuContext _context = FileMenuContext.None;			// File menu context.
		private EditorType _editorType = EditorType.None;					// Editor type.
		private string _projectName = string.Empty;							// Name of the current project.
		private bool _needsSave = false;									// Flag to indicate that we have unsaved changes.
		private Document _defaultDocument = null;							// Default document.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			buttonSave.Enabled = _editorType != EditorType.None;

			if (string.IsNullOrEmpty(_projectName))
				Text = "Gorgon Editor - Untitled";
			else
				Text = "Gorgon Editor - " + _projectName;

			if (_needsSave)
				Text += "*";
		}

		/// <summary>
		/// Handles the ContextMenuShown event of the tabDocuments control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KRBTabControl.KRBTabControl.ContextMenuShownEventArgs"/> instance containing the event data.</param>
		private void tabDocuments_ContextMenuShown(object sender, KRBTabControl.KRBTabControl.ContextMenuShownEventArgs e)
		{
			if (tabDocuments.Controls.Count < 2)
			{
				var item = (from items in e.ContextMenu.Items.Cast<ToolStripMenuItem>()
							where string.Compare(items.Text, "available tab pages", true) == 0
							select items).FirstOrDefault();

				item.Visible = false;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if (_defaultDocument != null)
					_defaultDocument.Dispose();

				if (this.WindowState != FormWindowState.Minimized)
					Program.Settings.FormState = this.WindowState;
				if (this.WindowState != FormWindowState.Normal)
					Program.Settings.WindowDimensions = this.RestoreBounds;
				else
					Program.Settings.WindowDimensions = this.DesktopBounds;

				Program.Settings.Save();
			}
#if DEBUG
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
#else
			catch
			{
				// Eat this exception if in release.
#endif
			}
		}

		/// <summary>
		/// Function to open the file menu.
		/// </summary>
		/// <param name="context">Context to use.</param>
		private void OpenFileMenu(FileMenuContext context)
		{
			// Toggle it off.
			if ((context == _context) && (_context != FileMenuContext.None))
				context = FileMenuContext.None;

			if (_context != FileMenuContext.None)
			{
				if (context == FileMenuContext.None)
					splitMain.Panel1Collapsed = true;

				buttonOpen.BackColor = buttonSave.BackColor = buttonNew.BackColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
			}

			if (context == FileMenuContext.None)
			{
				_context = FileMenuContext.None;
				return;
			}

			switch (context)
			{
				case FileMenuContext.New:
					buttonNew.BackColor = Color.DodgerBlue;
					break;
				case FileMenuContext.Open:
					buttonOpen.BackColor = Color.DodgerBlue;
					break;
				case FileMenuContext.Save:
					buttonSave.BackColor = Color.DodgerBlue;
					break;
			}

			splitMain.Panel1Collapsed = false;

			_context = context;
		}

		/// <summary>
		/// Handles the Click event of the buttonNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNew_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileMenu(FileMenuContext.New);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOpen_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileMenu(FileMenuContext.Open);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileMenu(FileMenuContext.Save);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function for idle time.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>TRUE to continue, FALSE to exit.</returns>
		private bool Idle(GorgonFrameRate timing)
		{
			if (Program.CurrentDocument == null)
				return true;

			Program.CurrentDocument.RenderMethod(timing);

			return true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{				
				this.splitMain.Panel1Collapsed = true;

				// Adjust main window.
				Visible = true;
				this.DesktopBounds = Program.Settings.WindowDimensions;

				// If this window can't be placed on a monitor, then shift it to the primary.
				if (Screen.AllScreens.Count(item => item.Bounds.Contains(this.Location)) == 0)
					this.Location = Screen.PrimaryScreen.Bounds.Location;

				this.WindowState = Program.Settings.FormState;
				this.tabDocuments.Focus();

				// Create the default document.
				_defaultDocument = new Document("Gorgon Editor", false);
				tabDocuments.TabPages.Add(_defaultDocument.Tab);
				_defaultDocument.InitializeRendering();

				Gorgon.ApplicationIdleLoopMethod = Idle;

				Program.CurrentDocument = _defaultDocument;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain"/> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
		}
		#endregion

		/// <summary>
		/// Handles the Click event of the buttonFontAction control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonFontAction_Click(object sender, EventArgs e)
		{
			formNewFont newFont = null;

			try
			{
				switch(_context)
				{
					case FileMenuContext.New:
						newFont = new formNewFont();
						if (newFont.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
						{
						}
						break;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (newFont != null)
					newFont.Dispose();

				OpenFileMenu(FileMenuContext.None);
			}
		}
	}
}
