#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 9, 2016 8:30:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.Core.Collections
{
	/// <summary>
	/// Provides extension functions for array types.
	/// </summary>
	public static class GorgonArrayExtensions
	{
		/// <summary>
		/// Function to provide an in-place quicksort of an array.
		/// </summary>
		/// <typeparam name="T">The type of array element. Elements must implement <see cref="IComparable{T}"/>.</typeparam>
		/// <param name="array">The array to quick sort.</param>
		/// <param name="arrayCount">The number of elements in the array to sort.</param>
		public static void QuickSort<T>(this T[] array, int arrayCount)
			where T : IComparable<T>
		{
			if ((array == null) || (array.Length < 2) || (arrayCount < 2))
			{
				return;
			}

			arrayCount = arrayCount.Min(array.Length);

			// Optimization to short circuit on small arrays.
			if (arrayCount == 2)
			{
				T value = array[0];

				if (value.CompareTo(array[1]) != 1)
				{
					return;
				}

				// Swap the two elements for a small array.
				array[0] = array[1];
				array[1] = value;
				return;
			}

			Array.Sort(array, 0, arrayCount);
		}
	}
}
