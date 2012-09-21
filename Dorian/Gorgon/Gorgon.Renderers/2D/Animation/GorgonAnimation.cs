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
// Created: Monday, September 3, 2012 7:46:31 PM
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
	/// A callback used by the renderable object to set a property value.
	/// </summary>
	/// <typeparam name="T">Type of value to set on the property.</typeparam>
	/// <param name="propertyName">Name of the property to update.</param>
	/// <param name="value">Value to assign to the property.</param>
	public delegate void Animator<T>(string propertyName, T value);

	/// <summary>
	/// Defines animations for a renderable object.
	/// </summary>
	public class GorgonAnimationCollection
        : GorgonBaseNamedObjectCollection<GorgonAnimationClip>
	{
		#region Properties.
		/// <summary>
		/// Property to return the list of animated properties on the renderable.
		/// </summary>
		internal IDictionary<string, Type> RenderableProperties
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the currently playing animation.
        /// </summary>
        public GorgonAnimationClip CurrentAnimation
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the renderable object that this animation is assigned to.
		/// </summary>
        public IRenderable Renderable
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the animation at the specified index.
        /// </summary>
        public GorgonAnimationClip this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                if (value == null)
                    return;

                SetItem(value.Name, value);
            }
        }

        /// <summary>
        /// Property to set or return an animation by name.
        /// </summary>
        public GorgonAnimationClip this[string name]
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
        /// Function to set an item by its name.
        /// </summary>
        /// <param name="name">Name of the item to set.</param>
        /// <param name="value">Value to set.</param>
        protected override void SetItem(string name, GorgonAnimationClip value)
        {
            if (CurrentAnimation == this[name])
                Stop();

            if ((this[name].Renderable != null) && (this[name].Renderable.Animations != this))
                throw new ArgumentException("The animation '" + name + "' is already in another collection!", "animation");

            this[name].Renderable = Renderable;

            base.SetItem(name, value);
        }

        /// <summary>
        /// Function to add an animation to the collection.
        /// </summary>
        /// <param name="value">Animation to add.</param>
        protected override void AddItem(GorgonAnimationClip value)
        {
            base.AddItem(value);
            if ((value.Renderable != null) && (value.Renderable.Animations != this))
                throw new ArgumentException("The animation '" + value.Name + "' is already in another collection!", "animation");
            value.Renderable = Renderable;
        }

        /// <summary>
        /// Function to remove an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        protected override void RemoveItem(GorgonAnimationClip item)
        {
            if (CurrentAnimation == item)
                Stop();
            item.Renderable = null;
            base.RemoveItem(item);
        }

        /// <summary>
        /// Function to retrieve the animated properties for the renderable.
        /// </summary>
        internal void RefreshProperties()
        {
            // Get only the properties marked with the animated attribute.
            var properties = Renderable.GetType().GetProperties();
            RenderableProperties = (from property in properties
                                    let propertyAttrib = property.GetCustomAttributes(typeof(AnimatedPropertyAttribute), false) as IList<AnimatedPropertyAttribute>
                                    where propertyAttrib != null && propertyAttrib.Count > 0
                                    select new { Name = property.Name, PropType = property.PropertyType }).ToDictionary(key => key.Name, val => val.PropType);
        }

        /// <summary>
        /// Function to set an animation playing.
        /// </summary>
        /// <param name="animation">Animation to play.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
        public void Play(GorgonAnimationClip animation)
        {
            GorgonDebug.AssertNull<GorgonAnimationClip>(animation, "animation");

#if DEBUG
            if (!Contains(animation.Name))
                throw new KeyNotFoundException("The animation '" + animation + "' was not found in this collection");
#endif

            // Stop the current animation.
            if (CurrentAnimation != null)
            {
                // TODO: Write this.
            }
                        
            // TODO: Set the animation as playing.
            CurrentAnimation = animation;
        }

        /// <summary>
        /// Function to set an animation playing.
        /// </summary>
        /// <param name="animation">Name of the animation to start playing.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the animation parameter is an empty string.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
        public void Play(string animation)
        {
            GorgonDebug.AssertParamString(animation, animation);

#if DEBUG
            if (!Contains(animation))
                throw new KeyNotFoundException("The animation '" + animation + "' was not found in this collection");
#endif            
            Play(this[animation]);
        }

        /// <summary>
        /// Function to set an animation playing.
        /// </summary>
        /// <param name="index">Index of the animation to start playing.</param>
        /// <exception cref="System.Collections.IndexOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0 or greater than (or equal to) the number of animations.</exception>
        public void Play(int index)
        {
            GorgonDebug.AssertParamRange(index, 0, Count, "index");
            Play(this[index]);
        }

        /// <summary>
        /// Function to stop the currently playing animation.
        /// </summary>
        public void Stop()
        {
            if (CurrentAnimation == null)
                return;

            // TODO: Write stop code.

            CurrentAnimation = null;
        }

        /// <summary>
        /// Function to add a list of animations to the collection.
        /// </summary>
        /// <param name="animations">Animations to add.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animations"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when an animation in the list already exists in this collection.</exception>
        public void AddRange(IEnumerable<GorgonAnimationClip> animations)
        {
            GorgonDebug.AssertNull<IEnumerable<GorgonAnimationClip>>(animations, "animations");
            
            if (animations.Count() == 0)
                return;

            AddItems(animations);
        }

        /// <summary>
        /// Function to add an animation to the collection.
        /// </summary>
        /// <param name="animation">Animation to add to the collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the animation already exists in this collection.</exception>
        public void Add(GorgonAnimationClip animation)
        {
            GorgonDebug.AssertNull<GorgonAnimationClip>(animation, "animation");

            if (Contains(animation.Name))
                throw new ArgumentException("'" + animation.Name + "' already exists in this collection.", "animation");

            AddItem(animation);
        }

        /// <summary>
        /// Function to clear the animation collection.
        /// </summary>
        public void Clear()
        {
            // Stop the current animation.
            Stop();
            
            foreach (var item in this)
                item.Renderable = null;

            ClearItems();
        }

        /// <summary>
        /// Function to remove an animation from the collection.
        /// </summary>
        /// <param name="animation">Animation to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
        public void Remove(GorgonAnimationClip animation)
        {
            GorgonDebug.AssertNull<GorgonAnimationClip>(animation, "animation");
            if (!Contains(animation))
                throw new KeyNotFoundException("The animation '" + animation.Name + "' was not found in this collection.");

            RemoveItem(animation);
        }

        /// <summary>
        /// Function to remove an animation from the collection.
        /// </summary>
        /// <param name="animation">Animation to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the animation parameter is an empty string.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
        public void Remove(string animation)
        {
            GorgonDebug.AssertParamString(animation, "animation");
            if (!Contains(animation))
                throw new KeyNotFoundException("The animation '" + animation + "' was not found in this collection.");

            RemoveItem(this[animation]);
        }

        /// <summary>
        /// Function to remove an animation from the collection.
        /// </summary>
        /// <param name="index">Index of the animation to remove.</param>
        /// <exception cref="System.Collections.IndexOutOfRange">Thrown when the <paramref name="animation"/> parameter is less than 0 or greater than (or equal to) the number of items in the collection.</exception>
        public void Remove(int index)
        {
            GorgonDebug.AssertParamRange(index, 0, Count, "index");

            RemoveItem(this[index]);
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonAnimationCollection" /> class.
		/// </summary>
        /// <param name="renderable">The renderable that holds the animations.</param>
		internal GorgonAnimationCollection(IRenderable renderable)
            : base(false)
		{
			Renderable = renderable;
			RenderableProperties = new Dictionary<string, Type>();
		}
		#endregion
	}
}
