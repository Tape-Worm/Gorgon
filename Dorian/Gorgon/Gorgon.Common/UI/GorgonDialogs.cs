#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, June 18, 2011 4:20:00 PM
// 
#endregion

using System;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Properties;

namespace GorgonLibrary.UI
{
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
	public static class GorgonDialogs
	{
		#region Methods.
		/// <summary>
		/// Function to format the stack trace output.
		/// </summary>
		/// <param name="stackTrace">Stack trace to format.</param>
		/// <returns>A formatted stack trace.</returns>
		private static string FormatStackTrace(string stackTrace)
		{
			var result = new StringBuilder(8192);

		    if (string.IsNullOrEmpty(stackTrace))
				return string.Empty;

			string[] lines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			result.Append("\nStack trace:\n");

			for (int i = lines.Length - 1; i >= 0; i--)
			{
				int inIndex = lines[i].LastIndexOf(") in ", StringComparison.Ordinal);
				int pathIndex = lines[i].LastIndexOf(@"\", StringComparison.Ordinal);

			    if ((inIndex > -1) && (pathIndex > -1))
			    {
			        lines[i] = lines[i].Substring(0, inIndex + 5) + lines[i].Substring(pathIndex + 1);
			    }

			    result.Append(lines[i]);
				result.Append("\n");
			}

			result.AppendFormat("<<<{0}>>>", Resources.GOR_DLG_ERR_STACK_END);
			return result.ToString();
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Supplementary error message.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		/// <param name="caption">Caption for the error box.</param>
		public static void ErrorBox(Form owner, string message, Exception innerException, string caption)
		{
			var errorText = new StringBuilder(1024);

		    var errorDialog = new ErrorDialog();
			// If the owner form is NULL or not available, center on screen.
			if ((owner == null) || (owner.WindowState == FormWindowState.Minimized) || (!owner.Visible))
			{
				errorDialog.StartPosition = FormStartPosition.CenterScreen;
			}

			// Find all inner exceptions.
			Exception nextException = innerException;

			while (nextException != null)
			{
				errorText.AppendFormat("{0}:  {1}\n{2}:  {3}", Resources.GOR_DLG_ERR_DETAILS_MSG, nextException.Message,
				                       Resources.GOR_DLG_ERR_EXCEPT_TYPE, nextException.GetType().FullName);

			    if (nextException.Source != null)
			    {
				    errorText.AppendFormat("{0}:  {1}\n", Resources.GOR_DLG_ERR_SRC, nextException.Source);
			    }

			    if ((nextException.TargetSite != null) && (nextException.TargetSite.DeclaringType != null))
			    {
				    errorText.AppendFormat("{0}:  {1}.{2}\n", Resources.GOR_DLG_ERR_TARGET_SITE,
				                           nextException.TargetSite.DeclaringType.FullName, nextException.TargetSite.Name);
			    }

				var gorgonException = nextException as GorgonException;

				if (gorgonException != null)
				{
					errorText.AppendFormat("{0}: [{1}] {2} (0x{3})", Resources.GOR_DLG_ERR_GOREXCEPT_RESULT,
					                       gorgonException.ResultCode.Name,
					                       gorgonException.ResultCode.Description, gorgonException.ResultCode.Code.FormatHex());
				}

			    System.Collections.IDictionary extraInfo = nextException.Data;

				// Print custom information.
				if (extraInfo.Count > 0)
				{
					var customData = new StringBuilder(256);

					foreach (System.Collections.DictionaryEntry item in extraInfo)
					{
						if (customData.Length > 0)
						{
							customData.Append("\n");
						}

					    if (item.Value != null)
					    {
						    customData.AppendFormat("{0}: {1}", item.Key, item.Value);
					    }
					}

				    if (customData.Length > 0)
				    {
					    errorText.AppendFormat("\n{0}:\n-------------------\n{1}\n-------------------\n",
					                           Resources.GOR_DLG_ERR_CUSTOM_INFO, customData);
				    }
				}

				string stackTrace = FormatStackTrace(nextException.StackTrace);

				if (!string.IsNullOrEmpty(stackTrace))
				{
					errorText.AppendFormat("{0}\n", stackTrace);
				}

				nextException = nextException.InnerException;

				if (nextException != null)
				{
					errorText.AppendFormat("\n{0}:\n===============\n", Resources.GOR_DLG_ERR_NEXT_EXCEPTION);
				}
			}

			errorDialog.ErrorDetails = errorText.ToString();

		    if (string.IsNullOrEmpty(message))
		    {
		        errorDialog.Message = innerException != null ? innerException.Message : Resources.GOR_DLG_ERR_NO_MSG;
		    }
		    else
		    {
		        errorDialog.Message = message;
		    }

		    errorDialog.Text = caption;
			errorDialog.ShowDialog(owner);
			errorDialog.Dispose();
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Supplementary error message.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, string message, Exception innerException)
		{
			ErrorBox(owner, message, innerException, Resources.GOR_DLG_CAPTION_ERROR);
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, Exception innerException)
		{
			ErrorBox(owner, null, innerException, Resources.GOR_DLG_CAPTION_ERROR);
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="description">Error description.</param>
		/// <param name="details">Details for the error.</param>
        /// <param name="showDetail">TRUE to open the window with the detail panel open, FALSE to open it with the detail panel closed.</param>
		/// <param name="caption">Caption for the error box.</param>
		public static void ErrorBox(Form owner, string description, string details, bool showDetail, string caption)
		{
		    var errorDialog = new ErrorDialog();
			// If the owner form is NULL or not available, center on screen.
		    if ((owner == null) || (owner.WindowState == FormWindowState.Minimized) || (!owner.Visible))
		    {
		        errorDialog.StartPosition = FormStartPosition.CenterScreen;
		    }

		    errorDialog.Message = description;
			errorDialog.ErrorDetails = details;
			errorDialog.Text = caption;
            errorDialog.ShowDetailPanel = showDetail;
			errorDialog.ShowDialog(owner);
			errorDialog.Dispose();
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="description">Error description.</param>
		/// <param name="details">Details for the error.</param>
        /// <param name="showDetail">TRUE to open the window with the detail panel open, FALSE to open it with the detail panel closed.</param>
		public static void ErrorBox(Form owner, string description, string details, bool showDetail)
		{
			ErrorBox(owner, description, details, showDetail, Resources.GOR_DLG_CAPTION_ERROR);
		}

        /// <summary>
        /// Function to display the enhanced error dialog.
        /// </summary>
        /// <param name="owner">Owning window of this dialog.</param>
        /// <param name="description">Error description.</param>
        /// <param name="details">Details for the error.</param>
        public static void ErrorBox(Form owner, string description, string details)
        {
            ErrorBox(owner, description, details, false, Resources.GOR_DLG_CAPTION_ERROR);
        }

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="description">Error description.</param>		
		public static void ErrorBox(Form owner, string description)
		{
			ErrorBox(owner, description, string.Empty, false, Resources.GOR_DLG_CAPTION_ERROR);
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static void InfoBox(Form owner, string message, string caption)
		{
			BaseDialog dialog = null;

			try
			{
				dialog = new BaseDialog
				    {
				        Icon = Resources.GorgonInfo,
				        DialogImage = Resources.Info_48x48,
				        Message = message,
				        ButtonAction = DialogResult.OK
				    };

			    if (owner != null)
			    {
			        dialog.MessageHeight = Screen.FromControl(owner).WorkingArea.Height/2;
			    }
			    else
			    {
			        dialog.MessageHeight = Screen.FromControl(dialog).WorkingArea.Height/2;
			    }

			    dialog.Text = !string.IsNullOrEmpty(caption) ? caption : Resources.GOR_DLG_CAPTION_INFO;

			    dialog.ShowDialog(owner);
			}
			finally
			{
			    if (dialog != null)
			    {
			        dialog.Dispose();
			    }
			}
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		public static void InfoBox(Form owner, string message)
		{
            InfoBox(owner, message, Resources.GOR_DLG_CAPTION_INFO);
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">Caption for the dialog.</param>
		public static void WarningBox(Form owner, string message, string caption)
		{
			BaseDialog dialog = null;

		    try
		    {
		        dialog = new BaseDialog
		            {
		                Icon = Resources.GorgonWarning,
		                DialogImage = Resources.Warning_48x48,
		                Message = message,
		                ButtonAction = DialogResult.OK
		            };

		        if (owner != null)
		        {
		            dialog.MessageHeight = Screen.FromControl(owner).WorkingArea.Height/2;
		        }
		        else
		        {
		            dialog.MessageHeight = Screen.FromControl(dialog).WorkingArea.Height/2;
		        }

		        dialog.Text = !string.IsNullOrEmpty(caption) ? caption : Resources.GOR_DLG_CAPTION_WARNING;

		        dialog.ShowDialog(owner);
		    }
		    finally
		    {
		        if (dialog != null)
		        {
		            dialog.Dispose();
		        }
		    }
		}

		/// <summary>
		/// Function to display a warning box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		public static void WarningBox(Form owner, string message)
		{
            WarningBox(owner, message, Resources.GOR_DLG_CAPTION_WARNING);
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
		    ConfirmationDialog confirm = allowToAll ? new ConfirmationDialogEx() : new ConfirmationDialog();

		    confirm.Text = !string.IsNullOrEmpty(caption) ? caption : Resources.GOR_DLG_CAPTION_CONFIRM;

		    confirm.Message = message;
			confirm.ShowCancel = allowCancel;
			confirm.ShowDialog(owner);

			ConfirmationResult result = confirm.ConfirmationResult;

			confirm.Dispose();

			return result;
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
		#endregion
	}
}
