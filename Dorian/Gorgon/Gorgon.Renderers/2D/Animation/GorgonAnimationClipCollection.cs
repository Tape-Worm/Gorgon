#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, September 3, 2012 8:30:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A collection of animation clips.
	/// </summary>
	public class GorgonAnimationClipCollection
		: GorgonBaseNamedObjectDictionary<GorgonAnimationClip>
	{
		#region Variables.
		private GorgonAnimation _owner = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return an animation clip by name.
		/// </summary>
		public GorgonAnimationClip this[string name]
		{
			get
			{
				return GetItem(name);
			}
			set
			{
				if (string.IsNullOrEmpty(name))
					return;

				if (value == null)
				{
					if (Contains(name))
						RemoveItem(name);
					else
						throw new KeyNotFoundException("The animation clip '" + name + "' does not exist in this collection.");

					return;
				}

				if (Contains(name))
					SetItem(name, value);
				else
					AddItem(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an item to the collection.
		/// </summary>
		/// <param name="value">Item to add.</param>
		protected override void AddItem(GorgonAnimationClip value)
		{
			GorgonDebug.AssertNull<GorgonAnimationClip>(value, "value");

			value.Animation = _owner;
			base.AddItem(value);
		}

		/// <summary>
		/// Function to add a list of items to the collection.
		/// </summary>
		/// <param name="items">The items.</param>
		protected override void AddItems(IEnumerable<GorgonAnimationClip> items)
		{
			GorgonDebug.AssertNull<IEnumerable<GorgonAnimationClip>>(items, "items");

			foreach (var item in items)
				item.Animation = _owner;

			base.AddItems(items);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">The item.</param>
		protected override void RemoveItem(GorgonAnimationClip item)
		{
			item.Animation = null;
			base.RemoveItem(item);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="name">The name.</param>
		protected override void RemoveItem(string name)
		{
			this[name].Animation = null;
			base.RemoveItem(name);
		}

		/// <summary>
		/// Function to assign an item in the collection.
		/// </summary>
		/// <param name="name">The name of the item.</param>
		/// <param name="value">The value for the item.</param>
		protected override void SetItem(string name, GorgonAnimationClip value)
		{
			this[name].Animation = null;
			value.Animation = _owner;
			base.SetItem(name, value);
		}

		/// <summary>
		/// Function to add an animation clip to the collection.
		/// </summary>
		/// <param name="clip">Clip to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="clip"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name of the clip is already present in this collection.</exception>
		public void Add(GorgonAnimationClip clip)
		{
			GorgonDebug.AssertNull<GorgonAnimationClip>(clip, "clip");

			AddItem(clip);
		}

		/// <summary>
		/// Function to remove all animation clips.
		/// </summary>
		public void Clear()
		{
			foreach (var item in this)
				item.Animation = null;
			ClearItems();
		}
		
		/// <summary>
		/// Function to remove an animation clip by its name.
		/// </summary>
		/// <param name="name">Name of the clip to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the name could not be found in the collection.</exception>
		public void Remove(string name)
		{
			GorgonDebug.AssertParamString(name, "name");
#if DEBUG
			if (!Contains(name))
				throw new KeyNotFoundException("The animation clip '" + name + "' does not exist.");
#endif

			RemoveItem(name);
		}

		/// <summary>
		/// Function to remove an animation clip.
		/// </summary>
		/// <param name="clip">Animation clip to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="clip"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void Remove(GorgonAnimationClip clip)
		{
			GorgonDebug.AssertNull<GorgonAnimationClip>(clip, "clip");

			RemoveItem(clip);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationClipCollection" /> class.
		/// </summary>
		/// <param name="animation">The animation that owns this collection.</param>
		internal GorgonAnimationClipCollection(GorgonAnimation animation)			
			: base(false)
		{
			_owner = animation;
		}
		#endregion
	}
}
