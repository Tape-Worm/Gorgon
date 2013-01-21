﻿#region MIT.
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
// Created: Saturday, January 19, 2013 4:13:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// A mount point for the virtual file system.
	/// </summary>
	public struct GorgonFileSystemMountPoint
		: IEquatable<GorgonFileSystemMountPoint>, IComparable<GorgonFileSystemMountPoint>, IComparable
	{
		#region Variables.
		private string _physicalPath;
		private string _mountLocation;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the physical location of the mount point.
		/// </summary>
		public string PhysicalPath
		{
			get
			{
				return _physicalPath;
			}
		}

		/// <summary>
		/// Property to return the virtual location of the mount point.
		/// </summary>
		public string MountLocation
		{
			get
			{
				return _mountLocation;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonFileSystemMountPoint left, ref GorgonFileSystemMountPoint right)
		{
			return (string.Compare(left._mountLocation, right._mountLocation, true) == 0)
					&& (string.Compare(left._physicalPath, right._physicalPath, true) == 0);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonFileSystemMountPoint)
			{
				return this.Equals((GorgonFileSystemMountPoint)obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Mount Point  Virtual Location: {0}, Physical Location: {1}", _mountLocation, _physicalPath);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(_physicalPath).GenerateHash(_mountLocation);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonFileSystemMountPoint left, GorgonFileSystemMountPoint right)
		{
			return GorgonFileSystemMountPoint.Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if not equal, FALSE if equal.</returns>
		public static bool operator !=(GorgonFileSystemMountPoint left, GorgonFileSystemMountPoint right)
		{
			return !GorgonFileSystemMountPoint.Equals(ref left, ref right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemMountPoint" /> struct.
		/// </summary>
		/// <param name="physicalPath">The physical path.</param>
		/// <param name="mountLocation">The mount location.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are empty.</exception>
		public GorgonFileSystemMountPoint(string physicalPath, string mountLocation)
		{
			GorgonDebug.AssertParamString(physicalPath, "physicalPath");
			GorgonDebug.AssertParamString(mountLocation, "mountLocation");

			_physicalPath = physicalPath;
			_mountLocation = mountLocation;
		}
		#endregion

		#region IEquatable<MountPoint> Members
		/// <summary>
		/// Function to determine if this instance is equal to another instance.
		/// </summary>
		/// <param name="other">The other instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(GorgonFileSystemMountPoint other)
		{
			return GorgonFileSystemMountPoint.Equals(ref this, ref other);
		}
		#endregion

		#region IComparable<MountPoint> Members
		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
		/// </returns>
		public int CompareTo(GorgonFileSystemMountPoint other)
		{
			int result = 0;

			result = string.Compare(_physicalPath, other._physicalPath, true);

			if (result == 0)
			{
				result = string.Compare(_mountLocation, other._mountLocation, true);
			}

			return result;
		}
		#endregion

		#region IComparable Members
		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
		/// </returns>
		int IComparable.CompareTo(object obj)
		{
			return CompareTo((GorgonFileSystemMountPoint)obj);
		}
		#endregion
	}
}
