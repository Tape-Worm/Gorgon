#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, March 9, 2013 3:36:32 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Design;
using Gorgon.Editor.Properties;
using Gorgon.Input;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// A base interface for content display in the main interface.
	/// </summary>
	public partial class ContentPanel : UserControl
	{
		#region Variables.
		private bool _captionVisible = true;
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the content has been closed from the interface.
		/// </summary>
		[Browsable(false)]
		internal event EventHandler<CancelEventArgs> ContentClosed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the raw input interface.
		/// </summary>
		[Browsable(false)]
		protected GorgonInputFactory RawInput
		{
			get;
            private set;
		}

		/// <summary>
		/// Property to return the background color for the panel.
		/// </summary>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor
		{
			get
			{
				return DarkFormsRenderer.DarkBackground;
			}
			set
			{
				// Do nothing.
			}
		}

		/// <summary>
		/// Property to return the foreground color for the panel.
		/// </summary>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor
		{
			get
			{
				return DarkFormsRenderer.ForeColor;
			}
			set
			{
				// Do nothing.
			}
		}

		/// <summary>
		/// Property to return the display panel.
		/// </summary>
        [Browsable(false)]
		public Panel PanelDisplay
		{
			get
			{
				return _panelContentDisplay;
			}
		}

		/// <summary>
		/// Property to set or return the content object to be manipulated in this interface.
		/// </summary>
		[Browsable(false)]
		public ContentObject Content
		{
            get;
			private set;
		}

	    /// <summary>
	    /// Property to set or return the text caption for this control.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_APPEARANCE")]
	    [LocalDescription(typeof(APIResources), "PROP_TEXT_CONTENT_PANEL_DESC")]
	    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	    public new virtual string Text
	    {
	        get
	        {
	            return base.Text;
	        }
	        set
	        {
	            base.Text = value;
	            UpdateCaption();
	        }
	    }

	    /// <summary>
	    /// Property to set or return whether the caption for the content panel is visible or not.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(APIResources), "PROP_CATEGORY_APPEARANCE")]
	    [LocalDescription(typeof(APIResources), "PROP_TEXT_CAPTION_VISIBLE_DESC")]
	    [DefaultValue(true)]
	    public bool CaptionVisible
	    {
	        get
	        {
	            return _captionVisible;
	        }
	        set
	        {
	            _captionVisible = value;
	            panelCaption.Visible = _captionVisible;
	        }
	    }

	    #endregion

		#region Methods.
		/// <summary>
		/// Handles the MouseEnter event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClose_MouseEnter(object sender, EventArgs e)
		{
			labelClose.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClose_MouseLeave(object sender, EventArgs e)
		{
			labelClose.ForeColor = Color.Silver;
		}

		/// <summary>
		/// Handles the MouseMove event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void labelClose_MouseMove(object sender, MouseEventArgs e)
		{
			labelClose.ForeColor = Color.White;
		}

		/// <summary>
		/// Handles the Click event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClose_Click(object sender, EventArgs e)
		{
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                OnCloseClicked();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
		}

		/// <summary>
		/// Function to perform localization on the control text properties.
		/// </summary>
		protected virtual void LocalizeControls()
		{
			
		}
		
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			if (!DesignMode)
			{
				LocalizeControls();
			}
			base.OnLoad(e);
		}

		/// <summary>
		/// Function called when the close button is clicked.
		/// </summary>
		private void OnCloseClicked()
		{
			if (Content == null)
			{
				return;
			}

			labelClose.Enabled = false;

			try
			{
				var args = new CancelEventArgs();

				OnContentClosing(args);

				if (args.Cancel)
				{
					return;
				}

				if (ContentClosed != null)
				{
					args = new CancelEventArgs();
					ContentClosed(this, args);

					if (args.Cancel)
					{
						return;
					}
				}

				ContentManagement.LoadDefaultContentPane();
			}
			finally
			{
				if (!labelClose.IsDisposed)
				{
					labelClose.Enabled = true;
				}
			}
		}

		/// <summary>
		/// Function called when the content is closing.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		/// <remarks>Override this method to perform custom closing functionality for content.</remarks>
		protected virtual void OnContentClosing(CancelEventArgs e)
		{
			
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected internal virtual void OnContentPropertyChanged(string propertyName, object value)
		{
			UpdateCaption();
		}

        /// <summary>
        /// Function called when the settings for the editor or content plug-in have changed.
        /// </summary>
        /// <remarks>Plug-in implementors should implement this method to facilitate the updating of the UI when a plug-in setting has changed.  This 
        /// only applies to plug-ins that implement <see cref="Gorgon.Editor.IPlugInSettingsUI"/>.</remarks>
        protected internal virtual void OnEditorSettingsChanged()
        {
        }

		/// <summary>
		/// Function called when the content data has been persisted.
		/// </summary>
		internal void ContentPersisted()
		{
			RefreshContent();
		}

		/// <summary>
		/// Function to update the caption label.
		/// </summary>
		internal void UpdateCaption()
		{
			if (Content == null)
			{
				labelCaption.Text = string.Format("{0} - {1}", base.Text, APIResources.GOREDIT_TEXT_UNTITLED);
				return;
			}

			labelCaption.Text = string.Format("{0} - {1}{2}", base.Text, Content.Name, Content.HasChanges ? "*" : string.Empty);
		}

		/// <summary>
        /// Function called when the content has changed.
        /// </summary>
        public virtual void RefreshContent()
		{
			UpdateCaption();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentPanel"/> class.
		/// </summary>
		public ContentPanel()
		{
			InitializeComponent();			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentPanel"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="input">Input interface to use.</param>
		public ContentPanel(ContentObject content, GorgonInputFactory input = null)
			: this()
		{
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}

			Content = content;
			UpdateCaption();
		    RawInput = input;
		}
		#endregion
	}
}
