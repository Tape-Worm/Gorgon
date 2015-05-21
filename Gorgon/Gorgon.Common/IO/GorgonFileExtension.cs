#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, September 22, 2013 8:28:38 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// An extension and description for a file.
	/// </summary>
	/// <remarks>This type is useful when building a filter list for a file dialog.</remarks>
	public struct GorgonFileExtension
		: IEquatable<GorgonFileExtension>, IComparable<GorgonFileExtension>, IEquatable<string>, IComparable<string>, INamedObject
	{
		#region Variables.
		/// <summary>
		/// Extension (without the leading .) for the file system writer plug-in file.
		/// </summary>
		public readonly string Extension;

		/// <summary>
		/// Friendly human-readable description of the file type.
		/// </summary>
		public readonly string Description;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the extension is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return string.IsNullOrWhiteSpace(Extension);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Operator to return whether 2 instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonFileExtension left, GorgonFileExtension right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to return whether 2 instances are not equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if not equal, FALSE if equal.</returns>
		public static bool operator !=(GorgonFileExtension left, GorgonFileExtension right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to return whether one instance is less or equal to the other.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if less than or equal, FALSE if not.</returns>
		public static bool operator <=(GorgonFileExtension left, GorgonFileExtension right)
		{
			if (Equals(ref left, ref right))
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
		/// <returns>TRUE if greater or equal, FALSE if not.</returns>
		public static bool operator >=(GorgonFileExtension left, GorgonFileExtension right)
		{
			if (Equals(ref left, ref right))
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
		/// <returns>TRUE if less than, FALSE if not.</returns>
		public static bool operator <(GorgonFileExtension left, GorgonFileExtension right)
		{
			return string.Compare(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase) == -1;
		}

		/// <summary>
		/// Operator to return whether one instance is greater than the other.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if greater than, FALSE if not.</returns>
		public static bool operator >(GorgonFileExtension left, GorgonFileExtension right)
		{
			return string.Compare(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase) == 1;
		}

		/// <summary>
		/// Function to return if instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonFileExtension left, ref GorgonFileExtension right)
		{
			return string.Equals(left.Extension, right.Extension, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonFileExtension)
			{
				return ((GorgonFileExtension)obj).Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Extension.ToUpperInvariant());
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_FILE_EXTENSION_TOSTR, Extension, Description);
		}

		/// <summary>
		/// Function to return this extension as a formatted value suitable for a windows file dialog extension description.
		/// </summary>
		/// <returns>The extension formatted as "Description (*.ext)".</returns>
		public string ToDialogDescription()
		{
			return string.Format(Resources.GOR_FILE_EXTENSION_DLG_FORMAT, Description, Extension);
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
				throw new ArgumentNullException("extension");
			}

			if (extension.StartsWith(".", StringComparison.Ordinal))
			{
				extension = extension.Substring(1);
			}

			if (string.IsNullOrWhiteSpace(extension))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "extension");
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

		#region IEquatable<FileExtension> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonFileExtension other)
		{
			return Equals(ref this, ref other);
		}
		#endregion

		#region IComparable<FileExtension> Members
		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
		/// </returns>
		public int CompareTo(GorgonFileExtension other)
		{
			return string.Compare(Extension, other.Extension, StringComparison.OrdinalIgnoreCase);
		}
		#endregion

		#region IEquatable<string> Members
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
		#endregion

		#region IComparable<string> Members
		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
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

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the object.
		/// </summary>
		string INamedObject.Name
		{
			get
			{
				return Extension;
			}
		}
		#endregion
	}
}
