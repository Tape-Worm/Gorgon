
namespace Gorgon.Core.Extensions
{
	/// <summary>
	/// An extension to provide hash code generation for dictionaries.
	/// </summary>
	public static class GorgonHashGenerationExtension
	{
		/// <summary>
		/// Function to build upon the hash code from a value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="previousHash">The hash code of the previous value.</param>
		/// <param name="item">New item to add to the hash code.</param>
		/// <returns>The hash code.</returns>
		public static int GenerateHash<T>(this int previousHash, T item)
		{
			unchecked
			{
				return 397 * previousHash + item.GetHashCode();		// 397 is our magic prime number.
			}
		}
	}
}
