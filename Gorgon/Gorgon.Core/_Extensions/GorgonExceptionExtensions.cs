// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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

using Gorgon.Diagnostics;

namespace Gorgon.Core;

/// <summary>
/// Extension methods for the <see cref="Exception"/> type
/// </summary>
public static class GorgonExceptionExtensions
{
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
    public static void Handle<T>(this T ex, Action<T> handler, IGorgonLog log = null)
        where T : Exception
    {
        if (ex is null)
        {
            return;
        }

        log?.LogException(ex);

        // We pass the exception to the handler so that we don't capture the exception object in the closure.
        handler?.Invoke(ex);
    }
}
