// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: November 17, 2023 5:05:48 PM
//

namespace Gorgon.Core;

/// <summary>
/// A custom exception that allows the passing of a <see cref="GorgonResult"/> code.
/// </summary>
public class GorgonException
    : Exception
{
    /// <summary>
    /// Property to return the exception result code.
    /// </summary>
    public GorgonResult ResultCode
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="errorMessage">Error message to display.</param>
    /// <param name="innerException">Inner exception to pass through.</param>
    public GorgonException(string errorMessage, Exception innerException)
        : base(errorMessage, innerException) => ResultCode = new GorgonResult("GorgonException", HResult, errorMessage);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="errorMessage">Error message to display.</param>
    public GorgonException(string errorMessage)
        : base(errorMessage) => ResultCode = new GorgonResult("GorgonException", HResult, errorMessage);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="message">Message data to append to the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public GorgonException(GorgonResult result, string message, Exception? innerException = null)
        : base(result.Description + (!string.IsNullOrEmpty(message) ? "\n" + message : string.Empty), innerException) => ResultCode = result;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="message">Message data to append to the error.</param>
    public GorgonException(GorgonResult result, string message)
        : base(result.Description + (!string.IsNullOrEmpty(message) ? "\n" + message : string.Empty)) => ResultCode = result;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="innerException">The inner exception.</param>
    public GorgonException(GorgonResult result, Exception innerException)
        : this(result, string.Empty, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    public GorgonException(GorgonResult result)
        : this(result, string.Empty, null)
    {
    }
}
