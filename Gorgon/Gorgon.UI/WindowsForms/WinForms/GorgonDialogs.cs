
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Saturday, June 18, 2011 4:20:00 PM
// 

using Gorgon.Core;
using Gorgon.UI.WindowsForms.Properties;

namespace Gorgon.UI.WindowsForms;

/// <summary>
/// Class used to display various dialog types with enhanced abilities over that of the standard <see cref="MessageBox"/> class
/// </summary>
public static class GorgonDialogs
{
    /// <summary>
    /// Function to display the enhanced error dialog.
    /// </summary>
    /// <param name="owner">The owning window of this dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <param name="caption">The caption for the error box.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <remarks>
    /// <para>
    /// This will display an enhanced error dialog with a details button that will have <paramref name="exception"/> information in the details pane.
    /// </para>
    /// <para>
    /// If the <paramref name="message"/> parameter is <b>null</b> or an empty string, then the <see cref="Exception.Message"/> property is used to display the error message.
    /// </para>
    /// </remarks>
    public static void Error(IWin32Window? owner, Exception exception, string message = "", string caption = "")
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = exception.Message ?? Resources.GOR_EXCEPT_NO_MSG;
        }

        Error(owner, message, caption, exception.GetDetailsFromException());
    }

    /// <summary>
    /// Function to display the enhanced error dialog.
    /// </summary>
    /// <param name="owner">The owning window of this dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <param name="caption">[Optional] The caption for the error box.</param>
    /// <param name="details">[Optional] The detailed information about the error.</param>
    /// <remarks>
    /// <para>
    /// This will display an enhanced error dialog with a details button. This button will expand the window to show the <paramref name="details"/> passed to the method. If the <paramref name="details"/> 
    /// are <b>null</b>, or empty, then the details button will not show.
    /// </para>
    /// </remarks>
    public static void Error(IWin32Window? owner, string message, string caption = "", string details = "")
    {
        FormDialog? errorDialog = null;

        try
        {
            FormDialogButtons buttons = FormDialogButtons.OK;

            if (!string.IsNullOrWhiteSpace(details))
            {
                buttons |= FormDialogButtons.Details;
            }

            errorDialog = new FormDialog
            {
                DialogIcon = Resources.Error_48x48,
                Message = message,
                Details = details,
                Buttons = buttons,
                Text = string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_ERROR : caption
            };

            errorDialog.ShowDialog(owner);
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
    public static void Information(IWin32Window? owner, string message, string caption = "")
    {
        FormDialog? errorDialog = null;

        try
        {
            errorDialog = new FormDialog
            {
                DialogIcon = Resources.Info_48x48,
                Message = message,
                Details = string.Empty,
                Buttons = FormDialogButtons.OK,
                Text = string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_INFO : caption
            };

            errorDialog.ShowDialog(owner);
        }
        finally
        {
            errorDialog?.Dispose();
        }
    }

    /// <summary>
    /// Function to display the enhanced warning dialog.
    /// </summary>
    /// <param name="owner">The owning window of this dialog.</param>
    /// <param name="message">The warning message to display.</param>
    /// <param name="caption">[Optional] The caption for the warning box.</param>
    /// <param name="details">[Optional] The details for the warning.</param>
    /// <remarks>
    /// <para>
    /// This will show an enhanced warning dialog with a details button. This button will expand the window to show the <paramref name="details"/> passed to the method. If the <paramref name="details"/> 
    /// are <b>null</b>, or empty, then the details button will not show.
    /// </para>
    /// </remarks>
    public static void Warning(IWin32Window? owner, string message, string caption = "", string details = "")
    {
        FormDialog? errorDialog = null;

        try
        {
            FormDialogButtons buttons = FormDialogButtons.OK;

            if (!string.IsNullOrWhiteSpace(details))
            {
                buttons |= FormDialogButtons.Details;
            }

            errorDialog = new FormDialog
            {
                DialogIcon = Resources.Warning_48x48,
                Message = message,
                Details = details,
                Buttons = buttons,
                Text = string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_WARNING : caption
            };

            errorDialog.ShowDialog(owner);
        }
        finally
        {
            errorDialog?.Dispose();
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
    public static ConfirmationResult Confirm(IWin32Window? owner, string message, string caption = "", bool allowCancel = false, bool allowToAll = false)
    {
        FormDialog? confirm = null;
        ConfirmationResult result;

        try
        {
            FormDialogButtons buttons = FormDialogButtons.Yes | FormDialogButtons.No;

            if (allowCancel)
            {
                buttons |= FormDialogButtons.Cancel;
            }

            if (allowToAll)
            {
                buttons |= FormDialogButtons.ToAll;
            }

            confirm = new FormDialog
            {
                Message = message,
                Details = string.Empty,
                DialogIcon = Resources.Confirm_48x48,
                Buttons = buttons,
                Text = string.IsNullOrWhiteSpace(caption) ? Resources.GOR_TEXT_CONFIRM : caption
            };

            DialogResult dialogResult = confirm.ShowDialog(owner);

            result = dialogResult switch
            {
                DialogResult.Yes when !confirm.ToAll => ConfirmationResult.Yes,
                DialogResult.Yes when confirm.ToAll => ConfirmationResult.Yes | ConfirmationResult.ToAll,
                DialogResult.No when !confirm.ToAll => ConfirmationResult.No,
                DialogResult.No when confirm.ToAll => ConfirmationResult.No | ConfirmationResult.ToAll,
                DialogResult.Cancel => ConfirmationResult.Cancel,
                _ => ConfirmationResult.None
            };
        }
        finally
        {
            confirm?.Dispose();
        }

        return result;
    }
}
