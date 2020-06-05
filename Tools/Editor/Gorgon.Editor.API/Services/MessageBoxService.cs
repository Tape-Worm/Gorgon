#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 31, 2018 12:31:55 AM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.UI;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to display messages in message box dialogs.
    /// </summary>
    public class MessageBoxService
        : IMessageDisplayService
    {
        // The log for the application.
        private readonly IGorgonLog _log;

        /// <summary>
        /// Function to retrieve the parent form for the message box.
        /// </summary>
        /// <returns>The form to use as the owner.</returns>
        private static Form GetParentForm() => Form.ActiveForm ?? (Application.OpenForms.Count > 1 ? Application.OpenForms[Application.OpenForms.Count - 1] : GorgonApplication.MainForm);

        /// <summary>
        /// Function to show a confirmation message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">[Optional] A caption for the message.</param>
        /// <param name="toAll">[Optional] <b>true</b> to allow an application to apply the result to all items, <b>false</b> to only allow an application to apply the result to a single item.</param>
        /// <param name="allowCancel">[Optional] <b>true</b> to allow cancellation support, <b>false</b> to only allow accept or deny functionality.</param>
        /// <returns>The response value for the message.</returns>
        public MessageResponse ShowConfirmation(string message, string caption = null, bool toAll = false, bool allowCancel = false)
        {
            ConfirmationResult confirmResult = GorgonDialogs.ConfirmBox(GetParentForm(), message, caption, allowCancel, toAll);

            switch (confirmResult)
            {
                case ConfirmationResult.None:
                    return MessageResponse.None;
                case ConfirmationResult.Cancel:
                    return MessageResponse.Cancel;
            }

            if (((confirmResult & ConfirmationResult.ToAll) == ConfirmationResult.ToAll) && (toAll))
            {
                if ((confirmResult & ConfirmationResult.Yes) == ConfirmationResult.Yes)
                {
                    return MessageResponse.YesToAll;
                }

                if ((confirmResult & ConfirmationResult.No) == ConfirmationResult.No)
                {
                    return MessageResponse.NoToAll;
                }
            }

            switch (confirmResult)
            {
                case ConfirmationResult.Yes:
                    return MessageResponse.Yes;
                default:
                    return MessageResponse.No;
            }
        }

        /// <summary>
        /// Function to show an error message based on an exception.
        /// </summary>
        /// <param name="ex">The exception to display.</param>
        /// <param name="message">[Optional] A custom message to display with the error.</param>
        /// <param name="caption">[Optional] A caption for the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ex"/> parameter is <b>null</b>.</exception>
        public void ShowError(Exception ex, string message = null, string caption = null)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            _log.Print($"ERROR: {(string.IsNullOrWhiteSpace(message) ? ex.Message : message)}", LoggingLevel.Verbose);
            _log.LogException(ex);

            GorgonDialogs.ErrorBox(GetParentForm(), string.IsNullOrWhiteSpace(message) ? ex.Message : message, caption, ex);
        }

        /// <summary>
        /// Function to show an error message with optional detail information.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">[Optional] A caption for the message.</param>
        /// <param name="details">[Optional] Additional information for the error.</param>
        public void ShowError(string message, string caption = null, string details = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _log.Print($"ERROR: {message}", LoggingLevel.Intermediate);
            }

            GorgonDialogs.ErrorBox(GetParentForm(), message, caption, details);
        }

        /// <summary>
        /// Function to show an informational message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">[Optional] A caption for the message.</param>
        public void ShowInfo(string message, string caption = null) => GorgonDialogs.InfoBox(GetParentForm(), message, caption);

        /// <summary>
        /// Function to show a warning message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">[Optional] A caption for the message.</param>
        /// <param name="details">[Optional] Additional information for the error.</param>
        public void ShowWarning(string message, string caption = null, string details = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _log.Print($"WARNING: {message}", LoggingLevel.Simple);
            }

            GorgonDialogs.WarningBox(GetParentForm(), message, caption, details);
        }


        /// <summary>Initializes a new instance of the <see cref="MessageBoxService"/> class.</summary>
        /// <param name="log">The log.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="log"/> parameter is <b>null</b>.</exception>
        public MessageBoxService(IGorgonLog log) => _log = log ?? throw new ArgumentNullException(nameof(log));
    }
}
