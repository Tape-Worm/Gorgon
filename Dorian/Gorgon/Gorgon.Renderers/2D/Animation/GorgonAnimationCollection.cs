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
using System.Reflection;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections;


namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Defines animations for a renderable object.
	/// </summary>
	public class GorgonAnimationCollection
		: GorgonBaseNamedObjectCollection<GorgonAnimation>
	{
		#region Value Types.
		/// <summary>
		/// Animated property type.
		/// </summary>
		public struct AnimatedProperty
		{
			/// <summary>
			/// Property information.
			/// </summary>
			public PropertyInfo Property;
			/// <summary>
			/// Display name for the property.
			/// </summary>
			public string DisplayName;
			/// <summary>
			/// Type of data in the property.
			/// </summary>
			public Type DataType;
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of tracks that can be animated on the object.
		/// </summary>
		internal IDictionary<PropertyInfo, AnimatedProperty> AnimatedProperties
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the currently playing animation.
		/// </summary>
		public GorgonAnimation CurrentAnimation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the object that this animation is assigned to.
		/// </summary>
		public IAnimated Owner
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the animation at the specified index.
		/// </summary>
		public GorgonAnimation this[int index]
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
		public GorgonAnimation this[string name]
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
		protected override void SetItem(string name, GorgonAnimation value)
		{
			if (CurrentAnimation == this[name])
				Stop();

			// Remove the clip from the other 
			if ((value != null) && (value.Owner != null) && (value.Owner.Animations != this) && (value.Owner.Animations.Contains(value)))
				value.Owner.Animations.Remove(value);

			this[name].Owner = Owner;

			base.SetItem(name, value);
		}

		/// <summary>
		/// Function to add an animation to the collection.
		/// </summary>
		/// <param name="value">Animation to add.</param>
		protected override void AddItem(GorgonAnimation value)
		{
			if ((value != null) && (value.Owner != null) && (value.Owner.Animations != this) && (value.Owner.Animations.Contains(value)))
				value.Owner.Animations.Remove(value);

			value.Owner = Owner;
			base.AddItem(value);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		protected override void RemoveItem(GorgonAnimation item)
		{
			if (CurrentAnimation == item)
				Stop();
			item.Owner = null;
			base.RemoveItem(item);
		}

		/// <summary>
		/// Function to retrieve a list of the animated properties.
		/// </summary>
		internal void GetAnimatedProperties()
		{
			var properties = (from property in Owner.GetType().GetProperties()
							 let attribs = property.GetCustomAttributes(typeof(AnimatedPropertyAttribute), true) as IList<AnimatedPropertyAttribute>
							 where attribs != null && attribs.Count == 1
							 select new { Property = property, PropertyAttribute = attribs[0] });

			// Fill our list.
			foreach (var property in properties)
			{
				AnimatedProperty newProperty = new AnimatedProperty();
				newProperty.Property = property.Property;
				newProperty.DisplayName = string.IsNullOrEmpty(property.PropertyAttribute.DisplayName) ? property.Property.Name : property.PropertyAttribute.DisplayName;
				newProperty.DataType = property.PropertyAttribute.DataType == null ? property.Property.PropertyType : property.PropertyAttribute.DataType;
				AnimatedProperties[property.Property] = newProperty;
			}
		}

		/// <summary>
		/// Function to update the currently playing animation time and bound properties.
		/// </summary>
		/// <remarks>This will update the animation time using the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.Delta">Delta</see> time.  Note that the animation time is not affected by <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.ScaledDelta">ScaledDelta</see>.</remarks>
		public void Update()
		{
			float increment = 0;
			float lastTime = 0;

			if ((Count == 0) || (CurrentAnimation == null))
				return;

			lastTime = CurrentAnimation.Time;
			increment = (CurrentAnimation.Speed * GorgonTiming.Delta) * 1000.0f;       // We modify this value by 1000 because delta time is in seconds, and our animation uses milliseconds.

			// Push the animation time forward (or backward, depending on the Speed modifier).
			CurrentAnimation.Time += increment;

			// Update the bound properties.
			CurrentAnimation.UpdateOwner();

			// If we're not looping, put the animation into a stopped state.
			if ((!CurrentAnimation.IsLooped) && ((lastTime + increment) > CurrentAnimation.Length))
				Stop();
		}
		
		/// <summary>
		/// Function to set an animation playing.
		/// </summary>
		/// <param name="animation">Animation to play.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		public void Play(GorgonAnimation animation)
		{
			GorgonDebug.AssertNull<GorgonAnimation>(animation, "animation");

#if DEBUG
			if (!Contains(animation))
				throw new KeyNotFoundException("The animation '" + animation.Name + "' was not found in this collection");
#endif

			// This animation is already playing.
			if (animation == CurrentAnimation)
				return;

			// Stop the current animation.
			if (CurrentAnimation != null)
				Stop();

			CurrentAnimation = animation;

			// Update to the first frame.
			Update();
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
		/// <exception cref="System.IndexOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0 or greater than (or equal to) the number of animations.</exception>
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
			CurrentAnimation = null;
		}

		/// <summary>
		/// Function to add a list of animations to the collection.
		/// </summary>
		/// <param name="animations">Animations to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animations"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when an animation in the list already exists in this collection.</exception>
		public void AddRange(IEnumerable<GorgonAnimation> animations)
		{
			GorgonDebug.AssertNull<IEnumerable<GorgonAnimation>>(animations, "animations");
			
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
		public void Add(GorgonAnimation animation)
		{
			GorgonDebug.AssertNull<GorgonAnimation>(animation, "animation");

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
				item.Owner = null;

			ClearItems();
		}

		/// <summary>
		/// Function to remove an animation from the collection.
		/// </summary>
		/// <param name="animation">Animation to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
		public void Remove(GorgonAnimation animation)
		{
			GorgonDebug.AssertNull<GorgonAnimation>(animation, "animation");
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
		/// <exception cref="System.IndexOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0 or greater than (or equal to) the number of items in the collection.</exception>
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
		/// <param name="owner">The object that holds the animations.</param>
		internal GorgonAnimationCollection(IAnimated owner)
			: base(false)
		{
			Owner = owner;
			AnimatedProperties = new Dictionary<PropertyInfo, AnimatedProperty>();
		}
		#endregion
	}
}
