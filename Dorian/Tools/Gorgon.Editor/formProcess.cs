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
// Created: Sunday, March 24, 2013 4:14:10 PM
// 
#endregion

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A dialog used to indicate a time consuming action is taking place.
	/// </summary>
	public partial class formProcess 
		: ZuneForm
	{
		#region Variables.
		private bool _forceClose = false;			// Flag to indicate that we should force the window to shut down.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the task to execute.
		/// </summary>
		public Task Task
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Shown" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected async override void OnShown(EventArgs e)
		{
			try
			{
				_forceClose = false;

				if (Task != null)
				{
					await Task;
				}
			}
			finally
			{
				_forceClose = true;
				DialogResult = System.Windows.Forms.DialogResult.OK;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			e.Cancel = ((e.CloseReason == CloseReason.UserClosing) && (!buttonCancel.Enabled) && (!_forceClose));
		}

		/// <summary>
		/// Function to set the progress meter value.
		/// </summary>
		/// <param name="value">Value to set.</param>
		public void SetProgress(int value)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(() =>
					{
						SetProgress(value);
					}));
			}
			else
			{
				// If the window shows up and closes right away, this method may still be called.
				// And since there's no handle, InvokeRequired won't tell us the truth, so check 
				// to see if we have a handle as well.
				if ((IsHandleCreated) && (!IsDisposed))
				{
					if ((value < progressMeter.Minimum) || (value > progressMeter.Maximum))
					{
						return;
					}

					progressMeter.Value = value;
				}
			}
		}

		/// <summary>
		/// Function to update the status text.
		/// </summary>
		/// <param name="text">Text to put into the status label.</param>
		public void UpdateStatusText(string text)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(() =>
					{
						UpdateStatusText(text);
					}));
			}
			else
			{
				// If the window shows up and closes right away, this method may still be called.
				// And since there's no handle, InvokeRequired won't tell us the truth, so check 
				// to see if we have a handle as well.
				if ((IsHandleCreated) && (!IsDisposed))
				{
					if (string.IsNullOrWhiteSpace(text))
					{
						return;
					}

					labelStatus.Text = text;
				}
			}
		}

		/// <summary>
		/// Function enable or disable the cancellation functionality.
		/// </summary>
		/// <param name="cancel">TRUE to enable cancellation, FALSE to disable.</param>
		public void EnableCancel(bool cancel)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(() =>
					{
						EnableCancel(cancel);
					}));
			}
			else
			{
				if ((IsHandleCreated) && (!IsDisposed))
				{
					buttonCancel.Enabled = cancel;				
				}				
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formProcess"/> class.
		/// </summary>
		public formProcess()
		{
			InitializeComponent();
		}
		#endregion
	}
}
