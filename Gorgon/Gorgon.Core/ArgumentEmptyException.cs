#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: April 26, 2017 12:46:08 PM
// 
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Gorgon.Properties;

namespace Gorgon.Core
{
    /// <summary>
    /// An exception that should be thrown when a non-null parameter (e.g. <see cref="string"/>) requires a value, and does not.  
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ArgumentEmptyException
        : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentEmptyException"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the exception.</param>
        public ArgumentEmptyException(string parameterName)
            : base(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, parameterName)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentEmptyException"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the exception.</param>
        /// <param name="innerException">The inner exception for this exception.</param>
        public ArgumentEmptyException(string parameterName, Exception innerException)
            : base(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, parameterName, innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentEmptyException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ArgumentEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
