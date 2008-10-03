#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:20:07 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Dialogs;
using System.Text;

namespace Dialogs
{
    /// <summary>
    /// Enumeration for error dialog icons.
    /// </summary>
    public enum ErrorDialogIcons
    {
        /// <summary>
        /// Default round error icon.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Icon for a general bug.
        /// </summary>
        Bug = 1,
        /// <summary>
        /// Same as the round error icon, except in a square.
        /// </summary>
        Box = 2,
        /// <summary>
        /// Icon for a disk error.
        /// </summary>
        Disk = 3,
        /// <summary>
        /// Icon for a data error.
        /// </summary>
        Data = 4,
        /// <summary>
        /// Icon for a hardware error.
        /// </summary>
        Hardware = 5
    }

    /// <summary>
    /// Enumeration for confirmation results.
    /// </summary>
    [Flags]
    public enum ConfirmationResult
    {
        /// <summary>
        /// No confirmation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Yes clicked.
        /// </summary>
        Yes = 1,
        /// <summary>
        /// No clicked.
        /// </summary>
        No = 2,
        /// <summary>
        /// Cancel clicked.
        /// </summary>
        Cancel = 4,
        /// <summary>
        /// To all checked.
        /// </summary>
        ToAll = 8
    }

    /// <summary>
	/// Static class representing various User Interface utilities.
	/// </summary>
	public static class UI
	{
		#region Methods.
		/// <summary>
		/// Function to format the stack trace output.
		/// </summary>
		/// <param name="stackTrace">Stack trace to format.</param>
		/// <returns>A formatted stack trace.</returns>
		private static string FormatStackTrace(string stackTrace)
		{
			StringBuilder result = new StringBuilder(8192);
			string[] lines = null;

			if (string.IsNullOrEmpty(stackTrace))
				return string.Empty;

			lines = stackTrace.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			result.Append("\nStack trace:\n");

			for (int i = lines.Length - 1; i >= 0; i--)
			{
				int inIndex = lines[i].LastIndexOf(") in ");
				int pathIndex = lines[i].LastIndexOf(@"\");

				if ((inIndex > -1) && (pathIndex > -1))
					lines[i] = lines[i].Substring(0, inIndex + 5) + lines[i].Substring(pathIndex + 1);

				result.Append(lines[i]);
				result.Append("\n");
			}

			result.Append("<<< Beginning of stack >>>");
			return result.ToString();
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static DialogResult AbortRetryBox(Form owner, string message, string caption)
		{
			return MessageBox.Show(owner, message, caption, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		public static DialogResult AbortRetryBox(Form owner, string message)
		{
			return AbortRetryBox(owner, message, "Error.");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="icon">Icon to be displayed in the error message.</param>
		/// <param name="message">Supplementary error message.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		/// <param name="caption">Caption for the error box.</param>
		public static void ErrorBox(Form owner, ErrorDialogIcons icon, string message, Exception innerException, string caption)
		{
			ErrorDialog errorDialog;		// Error dialog.
			Exception nextException;		// Next exception.

			errorDialog = new ErrorDialog();
			// If the owner form is NULL or not available, center on screen.
			if ((owner == null) || (owner.WindowState == FormWindowState.Minimized) || (!owner.Visible))
				errorDialog.StartPosition = FormStartPosition.CenterScreen;

			// Find all inner exceptions.
			nextException = innerException;
			while (nextException != null)
			{
				errorDialog.ErrorDetails += "Error message:  " + nextException.Message + "\n";
				errorDialog.ErrorDetails += "Exception type:  " + nextException.GetType().Name + "\n";
				if (nextException.Source != null)
					errorDialog.ErrorDetails += "Source:  " + nextException.Source + "\n";
				if (nextException.TargetSite != null)
					errorDialog.ErrorDetails += "Target site:  " + nextException.TargetSite.DeclaringType.FullName + "." + nextException.TargetSite.Name + "\n";
				errorDialog.ErrorDetails += FormatStackTrace(nextException.StackTrace);
				nextException = nextException.InnerException;

				if (nextException != null)
					errorDialog.ErrorDetails += "\n\nNext Exception:\n";
			}

			if (string.IsNullOrEmpty(message))
			{
				if (innerException != null)
					errorDialog.Message = innerException.Message;
				else
					errorDialog.Message = "No message given.";
			}
			else
				errorDialog.Message = message;
			errorDialog.Text = caption;
			errorDialog.ErrorIcon = icon;
			errorDialog.ShowDialog(owner);
			errorDialog.Dispose();
			errorDialog = null;
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="icon">Icon to be displayed in the error message.</param>
		/// <param name="message">Supplementary error message.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, ErrorDialogIcons icon, string message, Exception innerException)
		{
			ErrorBox(owner, icon, message, innerException, "Error.");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="icon">Icon to be displayed in the error message.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, ErrorDialogIcons icon, Exception innerException)
		{
			ErrorBox(owner, icon, null, innerException, "Error.");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Supplementary error message.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, string message, Exception innerException)
		{
			ErrorBox(owner, 0, message, innerException, "Error.");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, Exception innerException)
		{
			ErrorBox(owner, 0, null, innerException, "Error.");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="icon">Icon to be displayed in the error message.</param>
		/// <param name="description">Error description.</param>
		/// <param name="details">Details for the error.</param>
		/// <param name="caption">Caption for the error box.</param>
		public static void ErrorBox(Form owner, ErrorDialogIcons icon, string description, string details, string caption)
		{
			ErrorDialog errorDialog;		// Error dialog.

			errorDialog = new ErrorDialog();
			// If the owner form is NULL or not available, center on screen.
			if ((owner == null) || (owner.WindowState == FormWindowState.Minimized) || (!owner.Visible))
				errorDialog.StartPosition = FormStartPosition.CenterScreen;
			errorDialog.Message = description;
			errorDialog.ErrorDetails = details;
			errorDialog.Text = caption;
			errorDialog.ErrorIcon = icon;
			errorDialog.ShowDialog(owner);
			errorDialog.Dispose();
			errorDialog = null;
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="icon">Icon to be displayed in the error message.</param>
		/// <param name="description">Error description.</param>
		/// <param name="details">Details for the error.</param>
		public static void ErrorBox(Form owner, ErrorDialogIcons icon, string description, string details)
		{
			ErrorBox(owner, icon, description, details,"Error");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="icon">Icon to be displayed in the error message.</param>
		/// <param name="description">Error description.</param>		
		public static void ErrorBox(Form owner, ErrorDialogIcons icon, string description)
		{
			ErrorBox(owner, icon, description, "","Error");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>		
		/// <param name="description">Error description.</param>
		/// <param name="details">Details for the error.</param>
		public static void ErrorBox(Form owner, string description, string details)
		{
			ErrorBox(owner, ErrorDialogIcons.Default, description, details, "Error");
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="description">Error description.</param>		
		public static void ErrorBox(Form owner, string description)
		{
			ErrorBox(owner, ErrorDialogIcons.Default, description, "", "Error");
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool ErrorRetryBox(Form owner, string message, string caption)
		{
			if (MessageBox.Show(owner, message, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool ErrorRetryBox(Form owner, string message)
		{
			return ErrorRetryBox(owner, message, "Error.");
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static void InfoBox(Form owner, string message, string caption)
		{
			MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		public static void InfoBox(Form owner, string message)
		{
			InfoBox(owner, message, "Information.");
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static void WarningBox(Form owner, string message, string caption)
		{
			MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		public static void WarningBox(Form owner, string message)
		{
			WarningBox(owner, message, "Warning.");
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool WarningRetryBox(Form owner, string message, string caption)
		{
			if (MessageBox.Show(owner, message, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool WarningRetryBox(Form owner, string message)
		{
			return WarningRetryBox(owner, message, "Warning.");
		}

		/// <summary>
		/// Function to display a confirmation box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>TRUE if OK is clicked, FALSE if cancel is clicked.</returns>
		public static bool OKCancelBox(Form owner, string message, string caption)
		{
			if (MessageBox.Show(owner, message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Function to display a confirmation box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <returns>TRUE if yes is clicked, FALSE if no is clicked.</returns>
		public static bool OkCancelBox(Form owner, string message)
		{
			return OKCancelBox(owner, message, "Confirmation.");
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <param name="allowCancel">TRUE to show a Cancel button, FALSE to hide.</param>
		/// <param name="allowToAll">TRUE to show a 'To all' option, FALSE to hide.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(Form owner, string message, string caption, bool allowCancel, bool allowToAll)
		{
			ConfirmationDialog confirm = null;						// Confirmation box.			
			ConfirmationResult result = ConfirmationResult.None;	// Result.

			if (allowToAll)
				confirm = new ConfirmationDialogEx();
			else
				confirm = new ConfirmationDialog();

			if ((caption != string.Empty) && (caption != null))
				confirm.Text = caption;
			else
				confirm.Text = "Confirmation.";

			confirm.Message = message;
			confirm.ShowCancel = allowCancel;
			confirm.ShowDialog(owner);
			result = confirm.ConfirmationResult;

			confirm.Dispose();

			return result;
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <param name="allowCancel">TRUE to show a Cancel button, FALSE to hide.</param>
		/// <param name="allowToAll">TRUE to show a 'To all' option, FALSE to hide.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(string message, string caption, bool allowCancel, bool allowToAll)
		{
			return ConfirmBox(null, message, caption, allowCancel, allowToAll);
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="allowCancel">TRUE to show a Cancel button, FALSE to hide.</param>
		/// <param name="allowToAll">TRUE to show a 'To all' option, FALSE to hide.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(Form owner, string message, bool allowCancel, bool allowToAll)
		{
			return ConfirmBox(owner, message, string.Empty, allowCancel, allowToAll);
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="allowCancel">TRUE to show a Cancel button, FALSE to hide.</param>
		/// <param name="allowToAll">TRUE to show a 'To all' option, FALSE to hide.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(string message, bool allowCancel, bool allowToAll)
		{
			return ConfirmBox(null, message, string.Empty, allowCancel, allowToAll);
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <param name="allowCancel">TRUE to show a Cancel button, FALSE to hide.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(Form owner, string message, string caption, bool allowCancel)
		{
			return ConfirmBox(owner, message, caption, allowCancel, false);
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(Form owner, string message, string caption)
		{
			return ConfirmBox(owner, message, caption, false, false);
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(Form owner, string message)
		{
			return ConfirmBox(owner, message, string.Empty, false, false);
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(string message)
		{
			return ConfirmBox(null, message, string.Empty, false, false);
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static DialogResult AbortRetryBox(string message, string caption)
		{
			return AbortRetryBox(null, message, caption);
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		public static DialogResult AbortRetryBox(string message)
		{
			return AbortRetryBox(null, message, "Error.");
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool ErrorRetryBox(string message, string caption)
		{
			return ErrorRetryBox(null, message, caption);
		}

		/// <summary>
		/// Function to display an error box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool ErrorRetryBox(string message)
		{
			return ErrorRetryBox(null, message, "Error.");
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static void InfoBox(string message, string caption)
		{
			InfoBox(null, message, caption);
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		public static void InfoBox(string message)
		{
			InfoBox(null, message, "Information.");
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static void WarningBox(string message, string caption)
		{
			WarningBox(null, message, caption);
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		public static void WarningBox(string message)
		{
			WarningBox(null, message, "Warning.");
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool WarningRetryBox(string message, string caption)
		{
			return WarningRetryBox(null, message, caption);
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// 
		/// <param name="message">Message to display.</param>
		/// <returns>TRUE if Retry clicked, FALSE if cancelled.</returns>
		public static bool WarningRetryBox(string message)
		{
			return WarningRetryBox(null, message, "Warning.");
		}

		/// <summary>
		/// Function to display a confirmation box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		/// <returns>TRUE if OK is clicked, FALSE if cancel is clicked.</returns>
		public static bool OKCancelBox(string message, string caption)
		{
			return OKCancelBox(null, message, caption);
		}

		/// <summary>
		/// Function to display a confirmation box.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <returns>TRUE if yes is clicked, FALSE if no is clicked.</returns>
		public static bool OkCancelBox(string message)
		{
			return OKCancelBox(null, message, "Confirmation.");
		}
		#endregion
	}
}
