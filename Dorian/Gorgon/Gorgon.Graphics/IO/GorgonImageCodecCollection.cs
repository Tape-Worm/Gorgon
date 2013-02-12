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
// Created: Thursday, February 7, 2013 9:06:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Collections;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// A list of registered image codecs.
	/// </summary>
	public class GorgonImageCodecCollection
		: GorgonBaseNamedObjectCollection<GorgonImageCodec>
	{
		#region Properties.
		/// <summary>
		/// Gets the <see cref="GorgonImageCodec" /> at the specified index.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the index is less than 0 or larger than or equal to the count of the collection.</exception>
		public GorgonImageCodec this[int index]
		{
			get
			{
				if ((index < 0) || (index >= Count))
				{
					throw new ArgumentOutOfRangeException("The index [" + index.ToString() + "] is less than 0 or larger than or equal to the count of the collection.");
				}

				return GetItem(index);
			}
		}

		/// <summary>
		/// Gets the <see cref="GorgonImageCodec" /> with the specified codec name.
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the codec could not be found in the collection.</exception>
		public GorgonImageCodec this[string codecName]
		{
			get
			{
				if (!Contains(codecName))
				{
					throw new KeyNotFoundException("No codec named '" + codecName + "' is registered.");
				}

				return GetItem(codecName);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a codec.
		/// </summary>
		/// <param name="codec">Codec to add.</param>
		internal void Add(GorgonImageCodec codec)
		{
			AddItem(codec);
		}

		/// <summary>
		/// Function to remove a codec.
		/// </summary>
		/// <param name="codec">Codec to remove.</param>
		internal void Remove(GorgonImageCodec codec)
		{
			RemoveItem(codec);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageCodecCollection" /> class.
		/// </summary>
		internal GorgonImageCodecCollection()
			: base(false)
		{
		}
		#endregion
	}
}
