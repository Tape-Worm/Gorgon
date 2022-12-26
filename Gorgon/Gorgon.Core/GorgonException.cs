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
// Created: Tuesday, June 14, 2011 8:56:44 PM
// 
#endregion

using System;
using System.Runtime.Serialization;

namespace Gorgon.Core;

/// <summary>
/// A custom exception that allows the passing of a <see cref="GorgonResult"/> code.
/// </summary>
[Serializable]
public class GorgonException
    : Exception
{
    #region Properties.
    /// <summary>
    /// Property to return the exception result code.
    /// </summary>
    public GorgonResult ResultCode
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is a null reference (<i>Nothing</i> in Visual Basic). </exception>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue("ResultCode", ResultCode, typeof(GorgonResult));
    }
    #endregion

    #region Constructor/Destructor.
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
    /// Initializes a new instance of the <see cref="GorgonException" /> class with serialized data.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Serialization context.</param>
    protected GorgonException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        if (info.FullTypeName == typeof(GorgonResult).FullName)
        {
            ResultCode = (GorgonResult)info.GetValue("ResultCode", typeof(GorgonResult));
        }
        else
        {
            ResultCode = new GorgonResult("Exception", info.GetInt32("HResult"), info.GetString("Message"));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="message">Message data to append to the error.</param>
    /// <param name="inner">The inner exception.</param>
    public GorgonException(GorgonResult result, string message, Exception inner)
        : base(result.Description + (!string.IsNullOrEmpty(message) ? "\n" + message : string.Empty), inner) => ResultCode = result;

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
    /// <param name="inner">The inner exception.</param>
    public GorgonException(GorgonResult result, Exception inner)
        : this(result, null, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonException"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    public GorgonException(GorgonResult result)
        : this(result, null, null)
    {
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public GorgonException() => ResultCode = new GorgonResult("GorgonException", int.MinValue, string.Empty);
    #endregion
}
