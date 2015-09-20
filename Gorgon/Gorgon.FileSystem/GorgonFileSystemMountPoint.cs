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
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
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
		/// <param name="physicalPath">The physical path.</param>
		/// <param name="mountLocation">[Optional] The mount location.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
		/// <remarks>
		/// The <paramref name="mountLocation"/> is optional, if omitted then the phsyical file system object will be mounted under the root directory of the virtual file system. Passing <b>null</b> (<i>Nothing</i> in VB.Net) 
		/// or an empty string to this parameter will default the mount location to the root.
		/// </remarks>
		public GorgonFileSystemMountPoint(string physicalPath, string mountLocation = null)
		{
			if (physicalPath == null)
			{
				throw new ArgumentNullException(nameof(physicalPath));
			}

			if (string.IsNullOrWhiteSpace(physicalPath))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(physicalPath));
			}

			if (string.IsNullOrWhiteSpace(mountLocation))
			{
				mountLocation = "/";
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
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(GorgonFileSystemMountPoint other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
