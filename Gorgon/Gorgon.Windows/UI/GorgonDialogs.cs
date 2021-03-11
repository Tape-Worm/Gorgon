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
using Gorgon.Windows.Properties;

namespace Gorgon.UI
{
    /// <summary>
    /// Confirmation dialog result values.
    /// </summary>
    /// <remarks>
    /// The <c>Yes</c> and <c>No</c> fields can be OR'd with the <c>ToAll</c> field to indicate "Yes to all", or "No to all".
    /// </remarks>
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
    /// Class used to display various dialog types with enhanced abilities over that of the standard <see cref="MessageBox"/> class.
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
            {
                return string.Empty;
            }

            string[] lines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            result.AppendFormat("\n{0}\n", Resources.GOR_EXCEPT_STACK_TRACE);

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                int inIndex = lines[i].LastIndexOf(") in ", StringComparison.Ordinal);
                int pathIndex = lines[i].LastIndexOf(@"\", StringComparison.Ordinal);

                if ((inIndex > -1) && (pathIndex > -1))
                {
                    lines[i] = lines[i].Substring(0, inIndex + 5) + lines[i][(pathIndex + 1)..];
                }

                result.Append(lines[i]);
                result.Append('\n');
            }

            result.AppendFormat("<<<{0}>>>", Resources.GOR_EXCEPT_STACK_END);
            return result.ToString();
        }

        /// <summary>
        /// Function to retrieve the details for an exception.
        /// </summary>
        /// <param name="innerException">Exception to evaluate.</param>
        /// <returns>A string containing the details of the exception.</returns>
        private static string GetDetailsFromException(Exception innerException)
        {
            if (innerException is null)
            {
                return Resources.GOR_EXCEPT_NO_MSG;
            }

            // Find all inner exceptions.
            var errorText = new StringBuilder(1024);
            Exception nextException = innerException;

            while (nextException is not null)
            {
                errorText.AppendFormat("{0}: {1}\n{2}:  {3}",
                                       Resources.GOR_EXCEPT_DETAILS_MSG,
                                       nextException.Message,
                                       Resources.GOR_EXCEPT_EXCEPT_TYPE,
                                       nextException.GetType().FullName);

                if (nextException.Source is not null)
                {
                    errorText.AppendFormat("\n{0}:  {1}", Resources.GOR_EXCEPT_SRC, nextException.Source);
                }

                if (nextException.TargetSite?.DeclaringType is not null)
                {
                    errorText.AppendFormat("\n{0}:  {1}.{2}",
                                           Resources.GOR_EXCEPT_TARGET_SITE,
                                           nextException.TargetSite.DeclaringType.FullName,
                                           nextException.TargetSite.Name);
                }

                if (nextException is GorgonException gorgonException)
                {
                    errorText.AppendFormat("\n{0}: [{1}] {2} (0x{3})",
                                           Resources.GOR_EXCEPT_GOREXCEPT_RESULT,
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
                            customData.Append('\n');
                        }

                        if (item.Value is not null)
                        {
                            customData.AppendFormat("{0}: {1}", item.Key, item.Value);
                        }
                    }

                    if (customData.Length > 0)
                    {
                        errorText.AppendFormat("\n{0}:\n-------------------\n{1}\n-------------------\n",
                                               Resources.GOR_EXCEPT_CUSTOM_INFO,
                                               customData);
                    }
                }

                string stackTrace = FormatStackTrace(nextException.StackTrace);

                if (!string.IsNullOrEmpty(stackTrace))
                {
                    errorText.AppendFormat("{0}\n", stackTrace);
                }

                nextException = nextException.InnerException;

                if (nextException is not null)
                {
                    errorText.AppendFormat("\n{0}:\n===============\n", Resources.GOR_EXCEPT_NEXT_EXCEPTION);
                }
            }

            return errorText.ToString();
        }

        /// <summary>
        /// Function to display the enhanced error dialog.
        /// </summary>
        /// <param name="owner">The owning window of this dialog.</param>
        /// <param name="message">The error message to display.</param>
        /// <param name="caption">The caption for the error box.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="autoShowDetails">[Optional] <b>true</b> to open the details pane when the window is made visible, <b>false</b> to leave it closed.</param>
        /// <remarks>
        /// This will display an enhanced error dialog with a details button that will have <paramref name="exception"/> information in the details pane.
        /// <para>
        /// If <paramref name="autoShowDetails"/> is set to <b>true</b>, then the details pane will automatically be shown when the window appears.
        /// </para>
        /// <para>
        /// If the <paramref name="message"/> parameter is <b>null</b> or an empty string, then the <see cref="Exception.Message"/> property is used to display the error message.
        /// </para>
        /// </remarks>
        public static void ErrorBox(IWin32Window owner, string message, string caption, Exception exception, bool autoShowDetails = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = exception?.Message ?? Resources.GOR_EXCEPT_NO_MSG;
            }

            ErrorBox(owner, message, caption, GetDetailsFromException(exception), autoShowDetails);
        }

        /// <summary>
        /// Function to display the enhanced error dialog.
        /// </summary>
        /// <param name="owner">The owning window of this dialog.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <remarks>
        /// <para>
        /// This will display an enhanced error dialog with a details button that will have <paramref name="exception"/> information in the details pane.
        /// </para>
        /// </remarks>
        public static void ErrorBox(IWin32Window owner, Exception exception) => ErrorBox(owner, null, null, exception);

        /// <summary>
        /// Function to display the enhanced error dialog.
        /// </summary>
        /// <param name="owner">The owning window of this dialog.</param>
        /// <param name="message">The error message to display.</param>
        /// <param name="caption">[Optional] The caption for the error box.</param>
        /// <param name="details">[Optional] The detailed information about the error.</param>
        /// <param name="autoShowDetails">[Optional] <b>true</b> to open the details pane when the window is made visible, <b>false</b> to leave it closed.</param>
        /// <remarks>
        /// <para>
        /// This will display an enhanced error dialog with a details button. This button will expand the window to show the <paramref name="details"/> passed to the method. If the <paramref name="details"/> 
        /// are <b>null</b>, or empty, then the details button will not show.
        /// </para>
        /// <para>
        /// If <paramref name="autoShowDetails"/> is set to <b>true</b>, then the details pane will automatically be shown when the window appears. This only applies when the <paramref name="details"/> parameter 
        /// has information passed to it.
        /// </para>
        /// </remarks>
        public static void ErrorBox(IWin32Window owner, string message, string caption = "", string details = "", bool autoShowDetails = false)
        {
            ErrorDialog errorDialog = null;

            try
            {
                errorDialog = new ErrorDialog
                {
                    Message = message,
                    ErrorDetails = details,
                    ShowDetailPanel = (autoShowDetails) && (!string.IsNullOrEmpty(details))
                };

                if (!string.IsNullOrEmpty(caption))
                {
                    errorDialog.Text = caption;
                }

                var parentForm = owner as Form;

                errorDialog.ShowDialog(parentForm);

                // If the owner form is null or not available, center on screen.
                if ((parentForm is null) || (parentForm.WindowState == FormWindowState.Minimized) || (!parentForm.Visible))
                {
                    errorDialog.StartPosition = FormStartPosition.CenterScreen;
                }
            }
            finally
            {
                errorDialog?.Dispose();
            }
        }

        /// <summary>
        /// Function to display the enhanced information dialog.
        /// </summary>
        /// <param name="owner">The owning window of this dialog.</param>
        /// <param name="message">The informational message to display.</param>
        /// <param name="caption">[Optional] The caption for the dialog.</param>
        /// <remarks>
        /// This will display an enhanced information dialog that is capable of showing more text than the standard <see cref="MessageBox"/>.
        /// </remarks>
        public static void InfoBox(IWin32Window owner, string message, string caption = "")
        {
            BaseDialog dialog = null;

            try
            {
                dialog = new BaseDialog
                {
                    DialogImage = Resources.Info_48x48,
                    Message = message,
                    ButtonAction = DialogResult.OK
                };

                if (owner is Control control)
                {
                    dialog.MessageHeight = Screen.FromControl(control).WorkingArea.Height / 2;
                }
                else
                {
                    dialog.MessageHeight = Screen.FromControl(dialog).WorkingArea.Height / 2;
                }

                dialog.Text = !string.IsNullOrEmpty(caption) ? caption : Resources.GOR_DLG_CAPTION_INFO;

                dialog.ShowDialog(owner);
            }
            finally
            {
                dialog?.Dispose();
            }
        }

        /// <summary>
        /// Function to display the enhanced warning dialog.
        /// </summary>
        /// <param name="owner">The owning window of this dialog.</param>
        /// <param name="message">The warning message to display.</param>
        /// <param name="caption">[Optional] The caption for the warning box.</param>
        /// <param name="details">[Optional] The details for the warning.</param>
        /// <param name="autoShowDetails">[Optional] <b>true</b> to open the details pane when the window is made visible, <b>false</b> to leave it closed.</param>
        /// <remarks>
        /// <para>
        /// This will show an enhanced warning dialog with a details button. This button will expand the window to show the <paramref name="details"/> passed to the method. If the <paramref name="details"/> 
        /// are <b>null</b>, or empty, then the details button will not show.
        /// </para>
        /// <para>
        /// If <paramref name="autoShowDetails"/> is set to <b>true</b>, then the details pane will automatically be shown when the window appears. This only applies when the <paramref name="details"/> parameter 
        /// has information passed to it.
        /// </para>
        /// </remarks>
        public static void WarningBox(IWin32Window owner, string message, string caption = "", string details = "", bool autoShowDetails = false)
        {
            WarningDialog warningDialog = null;
            try
            {
                warningDialog = new WarningDialog
                {
                    Message = message,
                    WarningDetails = details,
                    ShowDetailPanel = (autoShowDetails) && (!string.IsNullOrEmpty(details))
                };

                if (!string.IsNullOrEmpty(caption))
                {
                    warningDialog.Text = caption;
                }

                // If the owner form is null or not available, center on screen.
                if ((owner is not Form form) || (form.WindowState == FormWindowState.Minimized) || (!form.Visible))
                {
                    warningDialog.StartPosition = FormStartPosition.CenterScreen;
                }

                warningDialog.ShowDialog(owner);
            }
            finally
            {
                warningDialog?.Dispose();
            }
        }

        /// <summary>
        /// Function to display a confirmation dialog.
        /// </summary>
        /// <param name="owner">The owning window of this dialog.</param>
        /// <param name="message">The confirmation message to display.</param>
        /// <param name="caption">[Optional] The caption for the dialog.</param>
        /// <param name="allowCancel">[Optional] <b>true</b> to show a Cancel button, <b>false</b> to hide.</param>
        /// <param name="allowToAll">[Optional] <b>true</b> to show a To all checkbox, <b>false</b> to hide.</param>
        /// <returns>
        /// A flag from the <see cref="ConfirmationResult"/> enumeration. If the <paramref name="allowToAll"/> parameter is <b>true</b>, then this result may be combined with <see cref="ConfirmationResult.ToAll"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This will display a confirmation dialog with an optional Cancel button, and an optional "To all" checkbox.
        /// </para>
        /// <para>
        /// If the "Yes" button is clicked, this method will return the <see cref="ConfirmationResult.Yes"/> flag, or if the "No" button is clicked, this method will return the <see cref="ConfirmationResult.No"/> 
        /// flag.
        /// </para>
        /// <para>
        /// When the <paramref name="allowToAll"/> parameter is set to <b>true</b>, this method may return <see cref="ConfirmationResult.Yes"/> or <see cref="ConfirmationResult.No"/> OR'd with the 
        /// <see cref="ConfirmationResult.ToAll"/> flag when the "to all" checkbox is checked. This is meant to indicate "Yes to all", or "No to all".
        /// </para>
        /// <para>
        /// Setting the <paramref name="allowCancel"/> parameter to <b>true</b> will display a cancel button and will allow the method to return the <see cref="ConfirmationResult.Cancel"/> flag. This flag is 
        /// exclusive and is not combined with the <see cref="ConfirmationResult.ToAll"/> flag under any circumstances.
        /// </para>
        /// </remarks>
        public static ConfirmationResult ConfirmBox(IWin32Window owner, string message, string caption = "", bool allowCancel = false, bool allowToAll = false)
        {
            ConfirmationDialog confirm = null;
            ConfirmationResult result;

            try
            {
                confirm = allowToAll ? new ConfirmationDialogEx() : new ConfirmationDialog();

                if (!string.IsNullOrEmpty(caption))
                {
                    confirm.Text = caption;
                }

                confirm.Message = message;
                confirm.ShowCancel = allowCancel;
                confirm.ShowDialog(owner);

                result = confirm.ConfirmationResult;
            }
            finally
            {
                confirm?.Dispose();
            }

            return result;
        }
        #endregion
    }
}