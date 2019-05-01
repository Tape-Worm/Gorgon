#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, cluding without limitation the rights
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
// Created: Sunday, September 22, 2013 8:28:38 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// An extension and description for a file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type allows for easy manipulation of file extensions and their descriptions when populating a <see cref="OpenFileDialog"/>, or <see cref="SaveFileDialog"/> extension list.
    /// </para>
    /// <para>
    /// The file extensions can be compared to each other to determine uniqueness. When comparing file extensions, the comparison is done with a case-insensitive comparer.
    /// </para>
    /// </remarks>
    public readonly struct GorgonFileExtension
        : IEquatable<GorgonFileExtension>, IComparable<GorgonFileExtension>, IEquatable<string>, IComparable<string>, IGorgonNamedObject
    {
        #region Variables.
        /// <summary>
        /// The file extension without the leading period.
        /// </summary>
        public readonly string Extension;

        /// <summary>
        /// The description of the file type.
        /// </summary>
        public readonly string Description;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the name of the object.
        /// </summary>
        string IGorgonNamedObject.Name => Extension;

        /// <summary>
        /// Property to return whether the extension is empty or not.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Extension);
        #endregion

        #region Operators.
        /// <summary>
        /// Operator to return whether 2 instances are equal.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonFileExtension left, GorgonFileExtension right) => Equals(left, right);

        /// <summary>
        /// Operator to return whether 2 instances are not equal.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonFileExtension left, GorgonFileExtension right) => !Equals(left, right);

        /// <summary>
        /// Operator to return whether one instance is less or equal to the other.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if less than or equal, <b>false</b> if not.</returns>
        public static bool operator <=(GorgonFileExtension left, GorgonFileExtension right)
        {
            if (Equals(left, right))
            {
                return true;
            }

            return string.Compare(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase) == -1;
        }

        /// <summary>
        /// Operator to return whether one instance is greater than or equal to the other.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if greater or equal, <b>false</b> if not.</returns>
        public static bool operator >=(GorgonFileExtension left, GorgonFileExtension right)
        {
            if (Equals(left, right))
            {
                return true;
            }

            return string.Compare(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase) == 1;
        }

        /// <summary>
        /// Operator to return whether one instance is less than the other.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if less than, <b>false</b> if not.</returns>
        public static bool operator <(GorgonFileExtension left, GorgonFileExtension right) => string.Compare(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase) == -1;

        /// <summary>
        /// Operator to return whether one instance is greater than the other.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if greater than, <b>false</b> if not.</returns>
        public static bool operator >(GorgonFileExtension left, GorgonFileExtension right) => string.Compare(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase) == 1;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return if instances are equal.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(GorgonFileExtension left, GorgonFileExtension right) => string.Equals(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the specified <see cref="object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj) => obj is GorgonFileExtension ext ? ext.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => 281.GenerateHash(Extension.ToUpperInvariant());

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GOR_TOSTR_FILE_EXTENSION, Description, Extension);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GorgonFileExtension other) => Equals(this, other);

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        public int CompareTo(GorgonFileExtension other) => string.Compare(Extension, other.Extension, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(string other)
        {
            if (string.IsNullOrWhiteSpace(other))
            {
                return false;
            }

            if (other.StartsWith(".", StringComparison.Ordinal))
            {
                other = other.Substring(1);
            }

            return string.Equals(Extension, other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public int CompareTo(string other)
        {
            if (string.IsNullOrWhiteSpace(other))
            {
                return -1;
            }

            if (other.StartsWith(".", StringComparison.Ordinal))
            {
                other = other.Substring(1);
            }

            return string.Compare(Extension, other, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileExtension"/> struct.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="description">The description.</param>
        public GorgonFileExtension(string extension, string description)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = extension.Substring(1);
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentEmptyException(nameof(extension));
            }

            Extension = extension;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileExtension"/> struct.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public GorgonFileExtension(string extension)
            : this(extension, string.Empty)
        {
        }
        #endregion
    }
}
