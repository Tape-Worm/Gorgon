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
using System.Collections;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Core.Properties;

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
		/// Function to retrieve the details for an exception.
		/// </summary>
		/// <param name="innerException">Exception to evaluate.</param>
		/// <returns>A string containing the details of the exception.</returns>
		private static string GetDetailsFromException(Exception innerException)
		{
			if (innerException == null)
			{
				return Resources.GOR_DLG_ERR_NO_MSG;
			}

			// Find all inner exceptions.
			var errorText = new StringBuilder(1024);
			Exception nextException = innerException;

			while (nextException != null)
			{
				errorText.AppendFormat("{0}: {1}\n{2}:  {3}",
									   Resources.GOR_DLG_ERR_DETAILS_MSG,
									   nextException.Message,
									   Resources.GOR_DLG_ERR_EXCEPT_TYPE,
									   nextException.GetType().FullName);

				if (nextException.Source != null)
				{
					errorText.AppendFormat("\n{0}:  {1}", Resources.GOR_DLG_ERR_SRC, nextException.Source);
				}

				if ((nextException.TargetSite != null) && (nextException.TargetSite.DeclaringType != null))
				{
					errorText.AppendFormat("\n{0}:  {1}.{2}",
										   Resources.GOR_DLG_ERR_TARGET_SITE,
										   nextException.TargetSite.DeclaringType.FullName,
										   nextException.TargetSite.Name);
				}

				var gorgonException = nextException as GorgonException;

				if (gorgonException != null)
				{
					errorText.AppendFormat("\n{0}: [{1}] {2} (0x{3})",
										   Resources.GOR_DLG_ERR_GOREXCEPT_RESULT,
										   gorgonException.ResultCode.Name,
										   gorgonException.ResultCode.Description,
										   gorgonException.ResultCode.Code.FormatHex());
				}

				IDictionary extraInfo = nextException.Data;

				// Print custom information.
				if (extraInfo.Count > 0)
				{
					var customData = new StringBuilder(256);

					foreach (DictionaryEntry item in extraInfo)
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
											   Resources.GOR_DLG_ERR_CUSTOM_INFO,
											   customData);
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

			return errorText.ToString();
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Supplementary error message.</param>
		/// <param name="caption">Caption for the error box.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		/// <param name="autoShowDetails">[Optional] TRUE to open the details pane, FALSE to leave it closed.</param>
		public static void ErrorBox(Form owner, string message, string caption, Exception innerException, bool autoShowDetails = false)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				message = innerException != null ? innerException.Message : Resources.GOR_DLG_ERR_NO_MSG;
			}

			ErrorBox(owner, message, caption, GetDetailsFromException(innerException), autoShowDetails);
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="innerException">Exception that was thrown.</param>
		public static void ErrorBox(Form owner, Exception innerException)
		{
			ErrorBox(owner, null, null, innerException);
		}

		/// <summary>
		/// Function to display the enhanced error dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="description">Error description.</param>
		/// <param name="caption">[Optional] Caption for the error box.</param>
		/// <param name="details">[Optional] Details for the error.</param>
		/// <param name="autoShowDetails">[Optional] TRUE to automatically open the details pane, FALSE to leave it closed.</param>
		/// <remarks>If the <paramref name="details"/> parameter is NULL (Nothing in VB.Net) or empty, then <paramref name="autoShowDetails"/> is ignored.</remarks>
		public static void ErrorBox(Form owner, string description, string caption = "", string details = "", bool autoShowDetails = false)
		{
			ErrorDialog errorDialog = null;

			try
			{
				if (string.IsNullOrEmpty(caption))
				{
					caption = Resources.GOR_DLG_CAPTION_ERROR;
				}

				errorDialog = new ErrorDialog
				              {
					              Message = description,
					              ErrorDetails = details,
					              Text = caption,
								  ShowDetailPanel = (autoShowDetails) && (!string.IsNullOrEmpty(details))
				              };

				errorDialog.ShowDialog(owner);

				// If the owner form is NULL or not available, center on screen.
				if ((owner == null) || (owner.WindowState == FormWindowState.Minimized) || (!owner.Visible))
				{
					errorDialog.StartPosition = FormStartPosition.CenterScreen;
				}
			}
			finally
			{
				if (errorDialog != null)
				{
					errorDialog.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to display an information box.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">[Optional] Caption for the dialog.</param>
		public static void InfoBox(Form owner, string message, string caption = "")
		{
			BaseDialog dialog = null;

			if (string.IsNullOrWhiteSpace(caption))
			{
				caption = Resources.GOR_DLG_CAPTION_INFO;
			}

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
		/// Function to display the enhanced warning dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="description">Error description.</param>
		/// <param name="caption">[Optional] Caption for the error box.</param>
		/// <param name="details">[Optional] Details for the error.</param>
		/// <param name="autoShowDetails">[Optional] TRUE to automatically open the details pane, FALSE to leave it closed.</param>
		/// <remarks>If the <paramref name="details"/> parameter is NULL (Nothing in VB.Net) or empty, then <paramref name="autoShowDetails"/> is ignored.</remarks>
		public static void WarningBox(Form owner, string description, string caption = "", string details = "", bool autoShowDetails = false)
		{
			if (string.IsNullOrEmpty(caption))
			{
				caption = Resources.GOR_DLG_CAPTION_WARNING;
			}

			WarningDialog warningDialog = null;
			try
			{
				warningDialog = new WarningDialog
				                {
					                Message = description,
					                WarningDetails = details,
					                Text = caption,
					                ShowDetailPanel = (autoShowDetails) && (!string.IsNullOrEmpty(details))
				                };

				// If the owner form is NULL or not available, center on screen.
				if ((owner == null) || (owner.WindowState == FormWindowState.Minimized) || (!owner.Visible))
				{
					warningDialog.StartPosition = FormStartPosition.CenterScreen;
				}

				warningDialog.ShowDialog(owner);
			}
			finally
			{
				if (warningDialog != null)
				{
					warningDialog.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to display a confirmation dialog.
		/// </summary>
		/// <param name="owner">Owning window of this dialog.</param>
		/// <param name="message">Message to display.</param>
		/// <param name="caption">[Optional] Caption for the dialog.</param>
		/// <param name="allowCancel">[Optional] TRUE to show a Cancel button, FALSE to hide.</param>
		/// <param name="allowToAll">[Optional] TRUE to show a 'To all' option, FALSE to hide.</param>
		/// <returns>Any member of ConfirmationResult except ConfirmationResult.None.</returns>
		public static ConfirmationResult ConfirmBox(Form owner, string message, string caption = "", bool allowCancel = false, bool allowToAll = false)
		{
			ConfirmationDialog confirm = null;
			ConfirmationResult result;

			if (string.IsNullOrEmpty(caption))
			{
				caption = Resources.GOR_DLG_CAPTION_CONFIRM;
			}

			try
			{
				confirm = allowToAll ? new ConfirmationDialogEx() : new ConfirmationDialog();

				confirm.Text = !string.IsNullOrEmpty(caption) ? caption : Resources.GOR_DLG_CAPTION_CONFIRM;

				confirm.Message = message;
				confirm.ShowCancel = allowCancel;
				confirm.ShowDialog(owner);

				result = confirm.ConfirmationResult;
			}
			finally
			{
				if (confirm != null)
				{
					confirm.Dispose();
				}
			}

			return result;
		}
		#endregion
	}
}
