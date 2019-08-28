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
// Created: Tuesday, June 14, 2011 8:57:48 PM
// 
#endregion

using System;
using Gorgon.Properties;

namespace Gorgon.Core
{
    /// <summary>
    /// An error code message that is usually sent along with a <see cref="GorgonException"/>.
    /// </summary>
    public readonly struct GorgonResult
        : IGorgonNamedObject, IGorgonEquatableByRef<GorgonResult>
    {
        #region Predefined error codes.
        // Base error code.
        private const int ErrorBase = 0x7FF000;

        /// <summary>
        /// Initialization is required before continuing this operation.
        /// </summary>
        public static GorgonResult NotInitialized => new GorgonResult(nameof(NotInitialized), ErrorBase + 1, Resources.GOR_RESULT_DESC_NOT_INITIALIZED);

        /// <summary>
        /// Initialization was already performed.
        /// </summary>
        public static GorgonResult AlreadyInitialized => new GorgonResult(nameof(AlreadyInitialized), ErrorBase + 1, Resources.GOR_RESULT_DESC_NOT_INITIALIZED);

        /// <summary>
        /// There was an error during creation.
        /// </summary>
        public static GorgonResult CannotCreate => new GorgonResult(nameof(CannotCreate), ErrorBase + 2, Resources.GOR_RESULT_DESC_CANNOT_CREATE);

        /// <summary>
        /// There was an error while writing.
        /// </summary>
        public static GorgonResult CannotWrite => new GorgonResult(nameof(CannotWrite), ErrorBase + 0xa, Resources.GOR_RESULT_DESC_CANNOT_WRITE);

        /// <summary>
        /// There was not enough memory to complete the operation.
        /// </summary>
        public static GorgonResult OutOfMemory => new GorgonResult(nameof(OutOfMemory), ErrorBase + 0xb, Resources.GOR_RESULT_OUT_OF_MEMORY);

        /// <summary>
        /// Access is denied.
        /// </summary>
        public static GorgonResult AccessDenied => new GorgonResult(nameof(AccessDenied), ErrorBase + 3, Resources.GOR_RESULT_DESC_ACCESS_DENIED);

        /// <summary>
        /// There was an error interfacing with the driver.
        /// </summary>
        public static GorgonResult DriverError => new GorgonResult(nameof(DriverError), ErrorBase + 4, Resources.GOR_RESULT_DESC_DRIVER_ERROR);

        /// <summary>
        /// There was an error while reading.
        /// </summary>
        public static GorgonResult CannotRead => new GorgonResult(nameof(CannotRead), ErrorBase + 5, Resources.GOR_RESULT_DESC_CANNOT_READ);

        /// <summary>
        /// There was an error during binding.
        /// </summary>
        public static GorgonResult CannotBind => new GorgonResult(nameof(CannotBind), ErrorBase + 6, Resources.GOR_RESULT_DESC_CANNOT_BIND);

        /// <summary>
        /// There was an error during the enumeration process.
        /// </summary>
        public static GorgonResult CannotEnumerate => new GorgonResult(nameof(CannotEnumerate), ErrorBase + 7, Resources.GOR_RESULT_DESC_CANNOT_ENUMERATE);

        /// <summary>
        /// The requested format is not supported.
        /// </summary>
        public static GorgonResult FormatNotSupported => new GorgonResult(nameof(FormatNotSupported), ErrorBase + 8, Resources.GOR_RESULT_DESC_FORMAT_NOT_SUPPORTED);

        /// <summary>
        /// The file format is not supported.
        /// </summary>
        public static GorgonResult InvalidFileFormat => new GorgonResult(nameof(InvalidFileFormat), ErrorBase + 9, Resources.GOR_RESULT_FILE_FORMAT_NOT_SUPPORTED);

        /// <summary>
        /// Cannot make this call across threads.
        /// </summary>
        public static GorgonResult CrossThreadCall => new GorgonResult(nameof(CrossThreadCall), ErrorBase + 10, Resources.GOR_RESULT_CANNOT_CALL_CROSS_THREAD);

        /// <summary>
        /// Cannot compile the source code.
        /// </summary>
        public static GorgonResult CannotCompile => new GorgonResult(nameof(CannotCompile), ErrorBase + 11, Resources.GOR_RESULT_CANNOT_COMPILE);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the error message to be sent along with the <see cref="GorgonException"/>.
        /// </summary>
        public string Description
        {
            get;
        }

        /// <summary>
        /// Property to set or return the error code to be sent along with the <see cref="GorgonException"/>.
        /// </summary>
        public int Code
        {
            get;
        }

        /// <summary>
        /// Property to return the name of the error.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GorgonResult other) => Equals(in this, in other);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(in GorgonResult other) => Equals(in this, in other);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to compare two instances for equality.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(in GorgonResult left, in GorgonResult right) => ((left.Code == right.Code) && (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)));

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => obj is GorgonResult result ? result.Equals(this) : false;

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString() => string.Format(Resources.GOR_TOSTR_GORGONRESULT, Name, Description, Code.FormatHex());

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => 281.GenerateHash(Code.GenerateHash(Name));
        #endregion

        #region Operators.
        /// <summary>
        /// Operator to test for equality.
        /// </summary>
        /// <param name="left">The left item to test.</param>
        /// <param name="right">The right item to test.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonResult left, GorgonResult right) => ((left.Code == right.Code) && (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)));

        /// <summary>
        /// Operator to test for inequality.
        /// </summary>
        /// <param name="left">The left item to test.</param>
        /// <param name="right">The right item to test.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if the items are equal.</returns>
        public static bool operator !=(GorgonResult left, GorgonResult right) => ((left.Code != right.Code) || (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)));
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonResult"/> struct.
        /// </summary>
        /// <param name="name">The name of the error.</param>
        /// <param name="code">The numeric code assigned to the error.</param>
        /// <param name="description">The full description of the error.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> or <paramref name="description"/> parameter is <b>null</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> or <paramref name="description"/> parameter is an empty string.</exception>
        public GorgonResult(string name, int code, string description)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentEmptyException(nameof(description));
            }

            Name = name;
            Description = description;
            Code = code;
        }
        #endregion

        #region IEquatable<GorgonResult> Members
        #endregion
    }
}
