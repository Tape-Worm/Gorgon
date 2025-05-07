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
// Created: Thursday, May 21, 2015 1:16:41 AM
// 

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.Properties;

namespace Gorgon.Core;

/// <summary>
/// Extension methods for the <see cref="Exception"/> type
/// </summary>
public static class GorgonExceptionExtensions
{
    // Newline characters.
    private static readonly char[] _newLine = ['\n', '\r'];

    /// <summary>
    /// Function to format the stack trace output.
    /// </summary>
    /// <param name="stackTrace">Stack trace to format.</param>
    private static string FormatStackTrace(string stackTrace)
    {
        StringBuilder result = new(1024);
        StringBuilder stack = new(1024);

        if (string.IsNullOrEmpty(stackTrace))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> stackTraceSpan = stackTrace.AsSpan();

        GorgonSpanCharEnumerator lines = stackTraceSpan.Split(_newLine);

        foreach (ReadOnlySpan<char> line in lines)
        {
            int inIndex = line.LastIndexOf(") in ", StringComparison.Ordinal);
            int pathIndex = line.LastIndexOf('\\');

            if ((inIndex > -1) && (pathIndex > -1))
            {
                stack.Insert(0, line[(pathIndex + 1)..]);
                stack.Insert(0, line[..(inIndex + 5)]);
            }
            else
            {
                stack.Insert(0, line);
            }
            stack.Insert(0, '\n');
        }

        result.AppendFormat("\n{0}:", Resources.GOR_EXCEPT_STACK_TRACE);
        result.Append(stack);
        result.AppendFormat("\n<<<{0}>>>", Resources.GOR_EXCEPT_STACK_END);
        return result.ToString();
    }

    /// <summary>
    /// Function to add target site information to the exception message.
    /// </summary>
    /// <param name="resultMessage">The message to update.</param>
    /// <param name="ex">The exception being evaluated.</param>
    [RequiresUnreferencedCode("Calls System.Exception.TargetSite")]
    private static void AddTargetSite(StringBuilder resultMessage, Exception ex)
    {
        if (ex.TargetSite?.DeclaringType is not null)
        {
            resultMessage.AppendFormat("\n{0}: {1}.{2}",
                                   Resources.GOR_EXCEPT_TARGET_SITE,
                                   ex.TargetSite.DeclaringType.FullName,
                                   ex.TargetSite.Name);
        }
    }

    /// <summary>
    /// Function to retrieve the details for an exception.
    /// </summary>
    /// <param name="innerException">Exception to evaluate.</param>
    /// <returns>A string containing the details of the exception.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
                                  Justification = "Using RuntimeFeature.IsDynamicCodeSupported flag to skip the code.")]
    public static string GetDetailsFromException(this Exception innerException)
    {
        // Find all inner exceptions.
        StringBuilder errorText = new(1024);
        Exception? nextException = innerException;

        while (nextException is not null)
        {
            errorText.AppendFormat("{0}: {1}\n{2}: {3}",
                                   Resources.GOR_EXCEPT_DETAILS_MSG,
                                   nextException.Message,
                                   Resources.GOR_EXCEPT_EXCEPT_TYPE,
                                   nextException.GetType().FullName);

            if (nextException.Source is not null)
            {
                errorText.AppendFormat("\n{0}: {1}", Resources.GOR_EXCEPT_SRC, nextException.Source);
            }

            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                AddTargetSite(errorText, nextException);
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
                StringBuilder customData = new(256);

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

            string stackTrace = string.Empty;

            if (nextException.StackTrace is not null)
            {
                stackTrace = FormatStackTrace(nextException.StackTrace);
            }

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
    /// Function to catch and handle an exception.
    /// </summary>
    /// <typeparam name="T">The type of exception. This value must be or inherit from the <see cref="Exception"/> type.</typeparam>
    /// <param name="ex">Exception to pass to the handler.</param>
    /// <param name="handler">A method that is called to handle the exception.</param>
    /// <param name="log">[Optional] A logger that will capture the exception, or <b>null</b> to disable logging of this exception.</param>
    /// <remarks>
    /// <para>
    /// This is a convenience method used to catch an exception and then handle it with the supplied <paramref name="handler"/> method. The handler method must take a parameter 
    /// that has a type that is or derives from <see cref="Exception"/>.
    /// </para>
    /// </remarks>
    public static void Handle<T>(this T ex, Action<T> handler, IGorgonLog? log = null)
        where T : Exception
    {
        log?.PrintException(ex);

        // We pass the exception to the handler so that we don't capture the exception object in the closure.
        handler.Invoke(ex);
    }
}
