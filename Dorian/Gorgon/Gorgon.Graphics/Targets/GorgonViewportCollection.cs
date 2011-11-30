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
// Created: Tuesday, November 22, 2011 9:45:10 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of viewports.
	/// </summary>
	/// <remarks>This collection will -always- have 1 element in it at index 0.</remarks>
	public class GorgonViewportCollection
		: IList<GorgonViewport>, INotifier
	{
		#region Variables.
		private List<GorgonViewport> _viewPorts = null;		// A list of view ports.
		private D3D.Viewport[] _viewportArray = null;		// A cached array of D3D viewports.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the collection as an array.
		/// </summary>
		/// <returns>An array of viewports.</returns>
		internal D3D.Viewport[] Convert()
		{
			if (HasChanged)
			{
				var enabledViewports = from viewport in this
									   where viewport.IsEnabled
									   select viewport;

				_viewportArray = new D3D.Viewport[enabledViewports.Count()];

				for (int i = 0; i < _viewportArray.Length; i++)
					_viewportArray[i] = enabledViewports.ElementAt(i).Convert();
			}
			return _viewportArray;
		}

		/// <summary>
		/// Function to insert a range of viewports at a specified index.
		/// </summary>
		/// <param name="index">Index to insert at.</param>
		/// <param name="viewPorts">List of viewports to insert.</param>
		public void InsertRange(int index, IEnumerable<GorgonViewport> viewPorts)
		{
			_viewPorts.InsertRange(index, viewPorts);
			HasChanged = true;
		}

		/// <summary>
		/// Function to remove an item at the specified index.
		/// </summary>
		/// <param name="index">Index of the item to remove.</param>
		public void Remove(int index)
		{			
			_viewPorts.RemoveAt(index);
			if (_viewPorts.Count == 0)
				_viewPorts.Add(new GorgonViewport());
			HasChanged = true;
		}

		/// <summary>
		/// Function to add a list of viewports to the collection.
		/// </summary>
		/// <param name="viewPorts">Viewports to add.</param>
		public void AddRange(IEnumerable<GorgonViewport> viewPorts)
		{
			_viewPorts.AddRange(viewPorts);
			HasChanged = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonViewportCollection"/> class.
		/// </summary>
		internal GorgonViewportCollection()
		{
			_viewPorts = new List<GorgonViewport>();
			_viewPorts.Add(new GorgonViewport());
		}
		#endregion

		#region IList<GorgonViewport> Members
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
		public GorgonViewport this[int index]
		{
			get
			{
				return _viewPorts[index];
			}
			set
			{
				_viewPorts[index] = value;
				HasChanged = true;
			}
		}
		#endregion

		#region Methods.
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
		void IList<GorgonViewport>.RemoveAt(int index)
		{
			Remove(index);			
		}

		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(GorgonViewport item)
		{
			return _viewPorts.IndexOf(item);
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
		public void Insert(int index, GorgonViewport item)
		{
			_viewPorts.Insert(index, item);
			HasChanged = true;
		}
		#endregion
		#endregion

		#region ICollection<GorgonViewport> Members
		#region Properties.
		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		///   </returns>
		bool ICollection<GorgonViewport>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

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
				return _viewPorts.Count;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		void ICollection<GorgonViewport>.CopyTo(GorgonViewport[] array, int arrayIndex)
		{
			_viewPorts.CopyTo(array, arrayIndex);
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
		public bool Remove(GorgonViewport item)
		{
			HasChanged = true;
			if (Count == 1)
			{
				_viewPorts[0] = new GorgonViewport();
				return true;
			}
			return _viewPorts.Remove(item);
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		public void Add(GorgonViewport item)
		{
			_viewPorts.Add(item);
			HasChanged = true;
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		public void Clear()
		{
			_viewPorts.Clear();
			this.Add(new GorgonViewport());
			HasChanged = true;
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(GorgonViewport item)
		{
			return _viewPorts.Contains(item);
		}
		#endregion
		#endregion

		#region IEnumerable<GorgonViewport> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonViewport> GetEnumerator()
		{
			return _viewPorts.GetEnumerator();
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
			return _viewPorts.GetEnumerator();
		}
		#endregion

		#region INotifier Members
		/// <summary>
		/// Property to set or return whether an object has been updated.
		/// </summary>
		public bool HasChanged
		{
			get;
			set;
		}
		#endregion
	}
}
