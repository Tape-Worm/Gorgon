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
// Created: Saturday, January 19, 2013 4:13:55 PM
// 
#endregion

using System;
using Gorgon.Core.Extensions;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A mount point for the virtual file system.
	/// </summary>
	public struct GorgonFileSystemMountPoint
		: IEquatable<GorgonFileSystemMountPoint>
	{
		#region Variables.
		/// <summary>
		/// The physical location of the mount point.
		/// </summary>
		public readonly string PhysicalPath;
		/// <summary>
		/// The virtual location of the mount point.
		/// </summary>
		public readonly string MountLocation;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public static bool Equals(ref GorgonFileSystemMountPoint left, ref GorgonFileSystemMountPoint right)
		{
			return (string.Equals(left.MountLocation, right.MountLocation, StringComparison.OrdinalIgnoreCase))
					&& (string.Equals(left.PhysicalPath, right.PhysicalPath, StringComparison.OrdinalIgnoreCase));
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
				return Equals((GorgonFileSystemMountPoint)obj);
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
			return string.Format(Resources.GORFS_MOUNTPOINT_TOSTRING, MountLocation, PhysicalPath);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(PhysicalPath).GenerateHash(MountLocation);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public static bool operator ==(GorgonFileSystemMountPoint left, GorgonFileSystemMountPoint right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><c>true</c> if not equal, <c>false</c> if equal.</returns>
		public static bool operator !=(GorgonFileSystemMountPoint left, GorgonFileSystemMountPoint right)
		{
			return !Equals(ref left, ref right);
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
			if (physicalPath == null)
			{
				throw new ArgumentNullException("physicalPath");
			}

			if (string.IsNullOrWhiteSpace(physicalPath))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, "physicalPath");
			}

			if (mountLocation == null)
			{
				throw new ArgumentNullException("mountLocation");
			}

			if (string.IsNullOrWhiteSpace(mountLocation))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, "mountLocation");
			}
			
			PhysicalPath = physicalPath;
			MountLocation = mountLocation;
		}
		#endregion

		#region IEquatable<MountPoint> Members
		/// <summary>
		/// Function to determine if this instance is equal to another instance.
		/// </summary>
		/// <param name="other">The other instance to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public bool Equals(GorgonFileSystemMountPoint other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
