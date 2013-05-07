using System;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	public partial class formSplash : Form
	{
		/// <summary>
		/// Function to update the version text.
		/// </summary>
		/// <param name="text">Text to display.</param>
		public void UpdateVersion(string text)
		{
			labelVersion.Text = "Version: " + GetType().Assembly.GetName().Version.ToString() + " ";
			if (!string.IsNullOrEmpty(text))
				labelVersion.Text += "- " + text;

			labelVersion.Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateVersion("Initializing...");
		}

		public formSplash()
		{
			InitializeComponent();
		}
	}
}
