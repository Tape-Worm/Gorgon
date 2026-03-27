// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 1, 2025 6:42:49 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for Gorgon's logging functionality.
/// </summary>
internal static class LoggingExtensions
{
    extension(IGorgonLog log)
    {
        /// <summary>
        /// Function to fail and automatically log if an HRESULT value is a failure.
        /// </summary>
        /// <param name="hr">The HRESULT error to evaluate.</param>
        /// <param name="message">The message to display in the log.</param>
        /// <param name="logLevel">The logging filter level to apply.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrintError(HRESULT hr, string message, LoggingLevel logLevel)
        {
            if ((!hr.FAILED) || (string.IsNullOrWhiteSpace(message)) || (logLevel == LoggingLevel.NoLogging))
            {
                return;
            }

            if (!message.EndsWith('.'))
            {
                message += ".";
            }

            log.PrintError($"{message} Error code: 0x{hr.Value.FormatHex()}.", logLevel);
        }

        /// <summary>
        /// Function to fail and automatically log if an HRESULT value is a failure.
        /// </summary>
        /// <param name="hr">The HRESULT error to evaluate.</param>
        /// <param name="message">The message to display in the log.</param>
        /// <param name="logLevel">The logging filter level to apply.</param>
        /// <param name="showErrorCode">[Optional] <b>true</b> to show the HRESULT error code, <b>false</b> to hide.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrintWarning(HRESULT hr, string message, LoggingLevel logLevel, bool showErrorCode = true)
        {
            if ((!hr.FAILED) || (string.IsNullOrWhiteSpace(message)) || (logLevel == LoggingLevel.NoLogging))
            {
                return;
            }

            if (showErrorCode)
            {
                log.PrintWarning($"{message}. Error code: 0x{hr.Value.FormatHex()}.", logLevel);
            }
            else
            {
                log.PrintWarning(message, logLevel);
            }
        }
    }
}
