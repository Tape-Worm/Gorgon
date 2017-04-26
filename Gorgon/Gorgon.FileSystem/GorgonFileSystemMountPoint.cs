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
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

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
		/// Flag to indicate whether the mount point is a fake mount point or not (i.e. it has no real physical location).
		/// </summary>
		internal readonly bool IsFakeMount;

		/// <summary>
		/// The provider for this mount point.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Nothing can change this object outside of the API.")]
		public readonly IGorgonFileSystemProvider Provider;
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
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonFileSystemMountPoint left, ref GorgonFileSystemMountPoint right)
		{
			return (left.Provider == right.Provider)
					&& (left.IsFakeMount == right.IsFakeMount)
					&& (string.Equals(left.MountLocation, right.MountLocation, StringComparison.OrdinalIgnoreCase))
					&& (string.Equals(left.PhysicalPath, right.PhysicalPath, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
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
			return string.Format(Resources.GORFS_TOSTR_MOUNTPOINT, MountLocation, PhysicalPath);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			if ((Provider == null) || (PhysicalPath == null) || (MountLocation == null))
			{
				return 0;
			}

			return 281.GenerateHash(Provider).GenerateHash(PhysicalPath).GenerateHash(MountLocation);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonFileSystemMountPoint left, GorgonFileSystemMountPoint right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonFileSystemMountPoint left, GorgonFileSystemMountPoint right)
		{
			return !Equals(ref left, ref right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemMountPoint" /> struct.
		/// </summary>
		/// <param name="provider">The provider for this mount point.</param>
		/// <param name="physicalPath">The physical path.</param>
		/// <param name="mountLocation">[Optional] The mount location.</param>
		/// <param name="isFakeMountPoint">[Optional] <b>true</b> if the mount point doesn't use a real physical location, or <b>false</b> if it does.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/>, <paramref name="provider"/>, or <paramref name="mountLocation"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/>, or the <paramref name="mountLocation"/> parameter is empty.</exception>
		internal GorgonFileSystemMountPoint(IGorgonFileSystemProvider provider, string physicalPath, string mountLocation, bool isFakeMountPoint = false)
		{
			if (physicalPath == null)
			{
				throw new ArgumentNullException(nameof(physicalPath));
			}

			if (mountLocation == null)
			{
				throw new ArgumentNullException(nameof(mountLocation));
			}

			if (string.IsNullOrWhiteSpace(physicalPath))
			{
				throw new ArgumentEmptyException(nameof(physicalPath));
			}

			if (string.IsNullOrWhiteSpace(mountLocation))
			{
				throw new ArgumentEmptyException(nameof(mountLocation));
			}

			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			PhysicalPath = physicalPath;
			MountLocation = mountLocation;
			IsFakeMount = isFakeMountPoint;
		}
		#endregion

		#region IEquatable<MountPoint> Members
		/// <summary>
		/// Function to determine if this instance is equal to another instance.
		/// </summary>
		/// <param name="other">The other instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(GorgonFileSystemMountPoint other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
