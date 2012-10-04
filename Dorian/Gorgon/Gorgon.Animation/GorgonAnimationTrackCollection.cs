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
// Created: Monday, September 3, 2012 8:25:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// A collection of animation tracks.
	/// </summary>
	/// 
	public class GorgonAnimationTrackCollection
		: GorgonBaseNamedObjectDictionary<GorgonAnimationTrack>
    {
        #region Variables.
        private GorgonAnimation _animation = null;          // Animation that owns this collection.
        #endregion

        #region Properties.
        /// <summary>
		/// Property to set or return the specified track from the collection.
		/// </summary>
		public GorgonAnimationTrack this[string name]
		{
			get
			{
				return GetItem(name);
			}
            set
            {
                if (value == null)
                {
                    if (Contains(name))
                        RemoveItem(name);

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
        /// Function to add an item to a collection.
        /// </summary>
        /// <param name="value">Item to add.</param>
        protected override void AddItem(GorgonAnimationTrack value)
        {
            base.AddItem(value);
            value.Animation = _animation;
        }

        /// <summary>
        /// Function to set an item in the collection.
        /// </summary>
        /// <param name="name">Name of the item to set.</param>
        /// <param name="value">Value to set.</param>
        protected override void SetItem(string name, GorgonAnimationTrack value)
        {
            base.SetItem(name, value);
            value.Animation = _animation;
        }

        /// <summary>
        /// Function to remove an item from a collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        protected override void RemoveItem(GorgonAnimationTrack item)
        {
            item.Animation = null;
            base.RemoveItem(item);
        }

        /// <summary>
        /// Function to remove an item by its name from a collection.
        /// </summary>
        /// <param name="name">Name of the item to remove.</param>
        protected override void RemoveItem(string name)
        {
            this[name].Animation = null;
            base.RemoveItem(name);
        }

        /// <summary>
        /// Function to clear the items from a collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var item in this)
                item.Animation = null;
            base.ClearItems();
        }

		/// <summary>
		/// Function to add a track to the collection.
		/// </summary>
		/// <param name="track">Track to add.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="track"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the track parameter already exists in the collection.</exception>
		public void Add(GorgonAnimationTrack track)
		{
            GorgonDebug.AssertNull<GorgonAnimationTrack>(track, "track");

            if (Contains(track.Name))
                throw new ArgumentException("The track '" + track.Name + "' already exists in this collection.", "track");

			AddItem(track);
		}

        /// <summary>
        /// Function to remove a track from the collection.
        /// </summary>
        /// <param name="track">Track to remove from the collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="track"/> parameter is NULL (Nothing in VB.Net).</exception>
        public void Remove(GorgonAnimationTrack track)
        {
            RemoveItem(track);
        }

        /// <summary>
        /// Function to remove a track by its name.
        /// </summary>
        /// <param name="name">Name of the track to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the track could not be found in the collection.</exception>
        public void Remove(string name)
        {
            GorgonDebug.AssertParamString(name, "name");
            if (!Contains(name))
                throw new KeyNotFoundException("The track '" + name + "' could not be found in the collection.");

            RemoveItem(name);
        }

		/// <summary>
		/// Function to remove all the tracks from the collection.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationTrackCollection" /> class.
		/// </summary>
        /// <param name="animation">The animation that owns this collection.</param>
		internal GorgonAnimationTrackCollection(GorgonAnimation animation)
			: base(false)
		{
            _animation = animation;
		}
		#endregion
	}
}
