using System;
using System.Collections.Generic;

namespace Gorgon.Graphics.Pipeline
{
	/// <summary>
	/// Defines a list that is used in binding to the pipeline.
	/// </summary>
	public interface IGorgonBoundList<T>
		: IList<T>
	{
		/// <summary>
		/// Property to return the starting index begin binding at.
		/// </summary>
		int BindIndex
		{
			get;
		}

		/// <summary>
		/// Property to return the number of binding slots actually used.
		/// </summary>
		int BindCount
		{
			get;
		}

		/// <summary>
		/// Property to return whether there are items to bind in this list.
		/// </summary>
		bool IsEmpty
		{
			get;
		}

		/// <summary>
		/// Function to set multiple objects of type <typeparamref name="T"/> at once.
		/// </summary>
		/// <param name="startSlot">The starting slot to assign.</param>
		/// <param name="items">The items to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="items"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="items"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startSlot"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="startSlot"/> plus the number of <paramref name="items"/> exceeds the size of this list.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <typeparamref name="T"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		void SetRange(int startSlot, IReadOnlyList<T> items, int? count = null);
	}
}
