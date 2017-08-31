﻿#region MIT
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, September 10, 2015 10:51:19 PM
// 
#endregion

using System;
using Gorgon.Core;

namespace Gorgon.Input
{
	/// <summary>
	/// A key used to identify a device in a dictionary.
	/// </summary>
	internal struct DeviceKey
		: IEquatable<DeviceKey>
	{
		/// <summary>
		/// The type of device.
		/// </summary>
		public RawInputType DeviceType;
		/// <summary>
		/// The handle for the device.
		/// </summary>
		public IntPtr DeviceHandle;

		/// <summary>
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>true if equal, false if not.</returns>
		public static bool Equals(ref DeviceKey left, ref DeviceKey right)
		{
			return left.DeviceType == right.DeviceType && left.DeviceHandle == right.DeviceHandle;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (obj is DeviceKey devKey)
			{
				return devKey.Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyMemberInGetHashCode
			return 281.GenerateHash(DeviceType).GenerateHash(DeviceHandle);
			// ReSharper enable NonReadonlyMemberInGetHashCode
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(DeviceKey other)
		{
			return Equals(ref this, ref other);
		}
	}
}
