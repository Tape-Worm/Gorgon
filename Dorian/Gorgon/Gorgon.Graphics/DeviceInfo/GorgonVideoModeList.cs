#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, July 25, 2011 8:16:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A list of video modes.
	/// </summary>
	public class GorgonVideoModeList
		: IList<GorgonVideoMode>
	{
		#region Variables.
		private List<GorgonVideoMode> _modes = null;				// List of video modes.
		private GorgonVideoOutput _output = null;					// Output that owns the video modes.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the video mode list.
		/// </summary>
		public void Refresh()
		{
			IList<BufferFormat> formats = Enum.GetValues(typeof(BufferFormat)) as IList<BufferFormat>;

			Gorgon.Log.Print("Retrieving video modes for output '{0}'...", Diagnostics.GorgonLoggingLevel.Simple, _output.Name);
			Gorgon.Log.Print("===================================================================", Diagnostics.GorgonLoggingLevel.Verbose);

			foreach (var format in formats)
			{				
				IList<GI.ModeDescription> modes = _output.GIOutput.GetDisplayModeList((GI.Format)format, GI.DisplayModeEnumerationFlags.Scaling | GI.DisplayModeEnumerationFlags.Interlaced);

				if (modes != null)
				{
					foreach (var mode in modes)
					{
						if (_output.VideoDevice.SupportsDisplayFormat((BufferFormat)mode.Format))
						{
							GorgonVideoMode videoMode = GorgonVideoMode.Convert(mode);
							_modes.Add(videoMode);
							Gorgon.Log.Print("Mode: {0}x{1}, Format: {2}, Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, videoMode.Width, videoMode.Height, videoMode.Format, videoMode.RefreshRateNumerator, videoMode.RefreshRateDenominator);
						}
					}
				}
			}

			Gorgon.Log.Print("===================================================================", Diagnostics.GorgonLoggingLevel.Verbose);
			Gorgon.Log.Print("Found {0} video modes for output '{1}'.", Diagnostics.GorgonLoggingLevel.Simple, _modes.Count, _output.Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoModeList"/> class.
		/// </summary>
		/// <param name="output">Output that owns the video modes.</param>
		internal GorgonVideoModeList(GorgonVideoOutput output)
		{
			if (output == null)
				throw new ArgumentNullException("output");

			_modes = new List<GorgonVideoMode>();
			_output = output;
		}
		#endregion

		#region IList<GorgonVideoMode> Members
		#region Properties.
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <returns>
		/// The element at the specified index.
		///   </returns>
		///   
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		///   </exception>
		public GorgonVideoMode this[int index]
		{
			get
			{
				return _modes[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(GorgonVideoMode item)
		{
			return _modes.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		///   </exception>
		void IList<GorgonVideoMode>.Insert(int index, GorgonVideoMode item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		///   </exception>
		void IList<GorgonVideoMode>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		#endregion
		#endregion

		#region ICollection<GorgonVideoMode> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		///   </returns>
		public int Count
		{
			get 
			{
				return _modes.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		///   </returns>
		public bool IsReadOnly
		{
			get 
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		void ICollection<GorgonVideoMode>.Add(GorgonVideoMode item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		void ICollection<GorgonVideoMode>.Clear()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(GorgonVideoMode item)
		{
			return _modes.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo(GorgonVideoMode[] array, int arrayIndex)
		{
			_modes.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		bool ICollection<GorgonVideoMode>.Remove(GorgonVideoMode item)
		{
			throw new NotSupportedException();
		}
		#endregion
		#endregion

		#region IEnumerable<GorgonVideoMode> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonVideoMode> GetEnumerator()
		{
			foreach (var mode in _modes)
				yield return mode;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _modes.GetEnumerator();
		}
		#endregion
	}
}
