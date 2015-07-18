using System;
using Gorgon.Core;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// A code and textual mapping for a character on the keyboard.
	/// </summary>
	public struct GorgonKeyCharMap
		: IEquatable<GorgonKeyCharMap>
	{
		#region Variables.
		/// <summary>
		/// Key that the character represents.
		/// </summary>
		public readonly KeyboardKey Key;
		/// <summary>
		/// Character representation of the <see cref="KeyboardKey"/>.
		/// </summary>
		public readonly char Character;
		/// <summary>
		/// Character representation when applying a shift modifier (e.g. the Shift key is held down).
		/// </summary>
		public readonly char Shifted;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Key);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is GorgonKeyCharMap)
			{
				return ((GorgonKeyCharMap)obj).Key == Key;
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Function to determine if 2 instances are equal.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonKeyCharMap left, ref GorgonKeyCharMap right)
		{
			return left.Key == right.Key;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return String.Format(Resources.GORINP_TOSTR_KEYCHARMAP, Key, Character, Shifted);
		}

		/// <summary>
		/// Operator to compare 2 instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonKeyCharMap left, GorgonKeyCharMap right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to compare 2 instances for inequality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonKeyCharMap left, GorgonKeyCharMap right)
		{
			return !Equals(ref left, ref right);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="key">Key that the character represents.</param>
		/// <param name="character">Character representation.</param>
		/// <param name="shifted">Character representation with shift modifier.</param>
		public GorgonKeyCharMap(KeyboardKey key, char character, char shifted)
		{
			Key = key;
			Character = character;
			Shifted = shifted;
		}
		#endregion

		#region IEquatable<KeyCharMap> Members
		/// <inheritdoc/>
		public bool Equals(GorgonKeyCharMap other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}