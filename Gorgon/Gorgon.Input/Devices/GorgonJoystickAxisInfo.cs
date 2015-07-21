using System;
using Gorgon.Core;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// Information about a joystick axis.
	/// </summary>
	public struct GorgonJoystickAxisInfo
		: IEquatable<GorgonJoystickAxisInfo>
	{
		#region Variables.
		/// <summary>
		/// The identifier for the axis.
		/// </summary>
		public readonly JoystickAxis Axis;
		/// <summary>
		/// The range of the axis.
		/// </summary>
		public readonly GorgonRange Range;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonJoystickAxisInfo left, ref GorgonJoystickAxisInfo right)
		{
			return left.Axis == right.Axis;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns><b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.</returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonJoystickAxisInfo)
			{
				return ((GorgonJoystickAxisInfo)obj).Equals(this);
			}
			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
			return Axis.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			return string.Format(Resources.GORINP_TOSTR_JOYSTICKAXIS, Axis, Range.Minimum, Range.Maximum);
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonJoystickAxisInfo left, GorgonJoystickAxisInfo right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to determine if two instances are not equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonJoystickAxisInfo left, GorgonJoystickAxisInfo right)
		{
			return !Equals(ref left, ref right);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickAxisInfo"/> struct.
		/// </summary>
		/// <param name="axis">The identifier for the axis.</param>
		/// <param name="range">The range of the axis.</param>
		public GorgonJoystickAxisInfo(JoystickAxis axis, GorgonRange range)
		{
			Axis = axis;
			Range = range;
		}
		#endregion

		#region IEquatable<GorgonJoystickAxisInfo> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		public bool Equals(GorgonJoystickAxisInfo other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
