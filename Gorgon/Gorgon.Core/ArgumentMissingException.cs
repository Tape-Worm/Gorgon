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
// Created: September 17, 2018 8:19:33 AM
// 
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Gorgon.Properties;

namespace Gorgon
{
    /// <summary>
    /// Exception to be thrown when an argument is missing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ideal use case for this exception is when a method expects a structure of data with specific members and a required member is not initialized (e.g. <b>null</b>).
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ArgumentMissingException
        : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMissingException"/> class.
        /// </summary>
        /// <param name="memberName">The name of the member that is missing.</param>
        /// <param name="parameterName">The name of the parameter that is in error.</param>
        public ArgumentMissingException(string memberName, string parameterName)
            : base(string.Format(Resources.GOR_ERR_ARGUMENT_MISSING, memberName, parameterName))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMissingException"/> class.
        /// </summary>
        /// <param name="memberName">The name of the member that is missing.</param>
        /// <param name="parameterName">The name of the parameter that is in error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <b>null</b> reference if no inner exception is specified. </param>
        public ArgumentMissingException(string memberName, string parameterName, Exception innerException)
            : base(string.Format(Resources.GOR_ERR_ARGUMENT_MISSING, memberName, parameterName), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMissingException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ArgumentMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
