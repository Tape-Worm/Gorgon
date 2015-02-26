#region MIT.
// 
// Gorgon.
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
// Created: Wednesday, February 25, 2015 11:43:29 PM
// 
#endregion

using System;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Disabled plug-in.
	/// </summary>
	struct DisabledPlugIn
		: IEquatable<DisabledPlugIn>, INamedObject
	{
		#region Variables.
		/// <summary>
		/// Plug-in that was disabled.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// Path to the plug-in.
		/// </summary>
		public readonly string Path;
		/// <summary>
		/// Reason for disabling the plug-in.
		/// </summary>
		public readonly string Reason;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref DisabledPlugIn left, ref DisabledPlugIn right)
		{
			return (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)
				   && string.Equals(left.Path, right.Path, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Operator to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(DisabledPlugIn left, DisabledPlugIn right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to determine if two instances are not equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if not equal, FALSE if equal.</returns>
		public static bool operator !=(DisabledPlugIn left, DisabledPlugIn right)
		{
			return !Equals(ref left, ref right);
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
			if (obj is DisabledPlugIn)
			{
				return ((DisabledPlugIn)obj).Equals(this);
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
			return 281.GenerateHash(Name).GenerateHash(Path);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DisabledPlugIn"/> struct.
		/// </summary>
		/// <param name="plugInName">The plug in name.</param>
		/// <param name="path">The path to the plug-in.</param>
		/// <param name="reason">The reason.</param>
		public DisabledPlugIn(string plugInName, string path, string reason)
		{
			Name = plugInName;
			Path = path;
		    
			if (reason == null)
		    {
		        reason = string.Empty;
		    }

			Reason = reason;
		}
		#endregion

		#region IEquatable<DisabledPlugIn> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(DisabledPlugIn other)
		{
			return Equals(ref this, ref other);
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the disabled plug-in.
		/// </summary>
		string INamedObject.Name
		{
			get
			{
				return Name;
			}
		}
		#endregion
	}
}
