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
// Created: Wednesday, May 22, 2013 9:55:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A parameter to pass to an effect.
	/// </summary>
	/// <remarks>Some effects will require parameters be passed before they can function, or to alter their function.  Use this type to define an effect parameter.</remarks>
	public struct GorgonEffectParameter
		: IEquatable<GorgonEffectParameter>, INamedObject
	{
		#region Variables.
        /// <summary>
        /// The name of the parameter.
        /// </summary>
		public readonly string Name;

		/// <summary>
		/// The value for the parameter.
		/// </summary>
		public readonly object Value;
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_EFFECT_PARAM_TOSTR, Name, Value == null ? "(NULL)" : Value.ToString());
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Value != null ? 281.GenerateHash(Name).GenerateHash(Value) : 281.GenerateHash(Name);
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
			if (obj is GorgonEffectParameter)
			{
				return Equals((GorgonEffectParameter)obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonEffectParameter left, ref GorgonEffectParameter right)
		{
			if (!string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if ((left.Value == null) && (right.Value == null))
			{
				return true;
			}

			if ((left.Value != null) && (left.Equals(right)))
			{
				return true;
			}

			return (right.Value != null) && (right.Equals(left));
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonEffectParameter left, GorgonEffectParameter right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonEffectParameter left, GorgonEffectParameter right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Implicit conversion for KeyValuePair type.
		/// </summary>
		/// <param name="param">The parameter to convert.</param>
		/// <returns>The parameter converted to a key value pair type.</returns>
		public static implicit operator KeyValuePair<string, object>(GorgonEffectParameter param)
		{
			return new KeyValuePair<string, object>(param.Name, param.Value);
		}

		/// <summary>
		/// Implicit conversion for GorgonEffectParameter type.
		/// </summary>
		/// <param name="keyValue">The key value pair to convert.</param>
		/// <returns>The key value pair converted into a Gorgon effect parameter.</returns>
		public static implicit operator GorgonEffectParameter(KeyValuePair<string, object> keyValue)
		{
			return new GorgonEffectParameter(keyValue.Key, keyValue.Value);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEffectParameter"/> struct.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="value">The value to assign.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="parameterName"/> is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="parameterName"/> is empty.</exception>
		public GorgonEffectParameter(string parameterName, object value)
		{
			if (parameterName == null)
			{
				throw new ArgumentNullException("parameterName");
			}

			if (string.IsNullOrWhiteSpace(parameterName))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "parameterName");
			}

			Name = parameterName;
			Value = value;
		}
		#endregion
	
		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the parameter.
		/// </summary>
		string INamedObject.Name
		{
			get
			{
				return Name;
			}
		}
		#endregion

		#region IEquatable<GorgonEffectParameter> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonEffectParameter other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
