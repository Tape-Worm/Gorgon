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
using System.IO;
using System.Linq;
using Gorgon.Animation.Properties;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.IO;

namespace Gorgon.Animation
{
	/// <summary>
	/// Manages and controls animations for an object.
	/// </summary>
	/// <typeparam name="T">The type of object that this controller will use.  The type passed in must be a reference type (i.e. a class).</typeparam>
	/// <remarks>
	/// A controller will force the object to update its properties over a certain time frame (or continously if looped).  It does this by placing its animated properties into the <see cref="Gorgon.Animation.GorgonAnimation{T}.Tracks">Tracks</see> of an 
	/// <see cref="Gorgon.Animation.GorgonAnimation{T}">Animation</see>.  These tracks will take <see cref="Gorgon.Animation.GorgonAnimationTrack{T}.KeyFrames">key frames</see> which correspond to the type of the property.  For example, the Angle property
	/// of a sprite uses a floating point value, so a <see cref="Gorgon.Animation.GorgonKeySingle">GorgonKeySingle</see>, or floating point key frame should be added to the Angle track.
	/// <para>To ensure the object will animate, it should have a <see cref="Gorgon.Animation.AnimatedPropertyAttribute">AnimatedPropertyAttribute</see> applied to one of its properties.  Otherwise, no animations will play.  Currently, Gorgon's graphical objects (e.g. sprites, text, etc...)
	/// all have appropriate attributes assigned to their properties.</para>
	/// <para>A user may add a custom track by inheriting from <see cref="Gorgon.Animation.GorgonAnimationTrack{T}">GorgonAnimationTrack</see> and creating a custom key frame type that implements <see cref="Gorgon.Animation.IKeyFrame">IKeyFrame</see>, and then adding a instance of the 
	/// custom track to the animation (not the controller).</para>
	/// </remarks>
	public class GorgonAnimationController<T>
		: GorgonBaseNamedObjectCollection<GorgonAnimation<T>>
		where T : class
	{
		#region Constants.
		/// <summary>
		/// Version header for the animation.
		/// </summary>
		public const string AnimationVersion = "GORANM10";
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of animated properties for the type specified by the generic parameter.
		/// </summary>
		internal IDictionary<string, GorgonAnimatedProperty> AnimatedProperties
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of object that this controller will animate.
		/// </summary>
		public Type AnimatedObjectType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the object that is to be animated.
		/// </summary>
		public T AnimatedObject
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the currently playing animation.
		/// </summary>
		public GorgonAnimation<T> CurrentAnimation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the animation at the specified index.
		/// </summary>
		public GorgonAnimation<T> this[int index]
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
		public GorgonAnimation<T> this[string name]
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
		protected override void SetItem(string name, GorgonAnimation<T> value)
		{
			if (CurrentAnimation == this[name])
				Stop();

			// Remove the clip from the other 
		    if ((value != null) && (value.AnimationController != null) && (value.AnimationController != this) &&
		        (value.AnimationController.Contains(value)))
		    {
		        value.AnimationController.Remove(value);
		    }

		    this[name].AnimationController = this;

			base.SetItem(name, value);
		}

		/// <summary>
		/// Function to add an animation to the collection.
		/// </summary>
		/// <param name="value">Animation to add.</param>
		protected override void AddItem(GorgonAnimation<T> value)
		{
            GorgonDebug.AssertNull(value, "value");

		    if ((value.AnimationController != null) && (value.AnimationController != this) &&
		        (value.AnimationController.Contains(value)))
		    {
		        value.AnimationController.Remove(value);
                value.AnimationController = this;
		    }
		    
			base.AddItem(value);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		protected override void RemoveItem(GorgonAnimation<T> item)
		{
			if (CurrentAnimation == item)
				Stop();
			item.AnimationController = null;
			base.RemoveItem(item);
		}		

		/// <summary>
		/// Function to load an animation from a stream.
		/// </summary>
		/// <param name="stream">Stream to load from.</param>
		/// <returns>The animation in the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the stream parameter does not contain a Gorgon animation file.
		/// <para>-or-</para>
		/// <para>Thrown when the name of the animation is already present in the controller animation collection.</para>
		/// <para>-or-</para>
		/// <para>Thrown when a track type cannot be associated with a property on the object type that the controller was declared with.</para>
		/// </exception>
		/// <exception cref="System.InvalidCastException">Thrown when the animation being loaded is for a different type than the controller was declared with.</exception>
		public GorgonAnimation<T> FromStream(Stream stream)
		{
			GorgonAnimation<T> animation;
			GorgonDebug.AssertNull(stream, "stream");

			using (var chunk = new GorgonChunkReader(stream))
			{
				// Get the header.
				chunk.Begin(AnimationVersion);

				chunk.Begin("ANIMDATA");				
				// Get the type data.
				string typeString = chunk.ReadString();

			    if (typeString != AnimatedObjectType.FullName)
			    {
			        throw new InvalidCastException(string.Format(Resources.GORANM_ANIMATION_TYPE_MISMATCH, typeString,
			                                                     AnimatedObjectType.FullName));
			    }

			    // Get the name.
				string animationName = chunk.ReadString();
			    if (Contains(animationName))
			    {
			        throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, animationName),
			                                    "stream");
			    }

			    animation = new GorgonAnimation<T>(this, animationName, chunk.ReadFloat())
				    {
				        IsLooped = chunk.ReadBoolean()
				    };

			    chunk.End();

				// Get all the tracks.
				while (chunk.HasChunk("TRCKDATA"))
				{
					chunk.Begin("TRCKDATA");

					string trackName = chunk.ReadString();			// Get the name of the track.

				    if (!animation.Tracks.Contains(trackName))
				    {
				        throw new ArgumentException(
				            string.Format(Resources.GORANM_TRACK_TYPE_DOES_NOT_EXIST, trackName, AnimatedObjectType.FullName), "stream");
				    }

				    animation.Tracks[trackName].FromChunk(chunk);

					chunk.End();
				}
			}

			Add(animation);

			return animation;
		}

		/// <summary>
		/// Function to load an animation from a file.
		/// </summary>
		/// <param name="fileName">Path and file name for the animation file.</param>
		/// <returns>The loaded animation from the file.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the stream parameter does not contain a Gorgon animation file.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the name of the animation is already present in the controller animation collection.</para>
		/// <para>-or-</para>
		/// <para>Thrown when a track type cannot be associated with a property on the object type that the controller was declared with.</para>
		/// </exception>
		/// <exception cref="System.InvalidCastException">Thrown when the animation being loaded is for a different type than the controller was declared with.</exception>
		public GorgonAnimation<T> FromFile(string fileName)
		{
			GorgonDebug.AssertParamString(fileName, "fileName");

		    using (FileStream file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		    {
		        return FromStream(file);
		    }
		}

		/// <summary>
		/// Function to update the currently playing animation time and bound properties.
		/// </summary>
		/// <remarks>This will update the animation time using the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.Delta">Delta</see> time.  Note that the animation time is not affected by <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.ScaledDelta">ScaledDelta</see>.</remarks>
		public void Update()
		{
		    if ((Count == 0) || (CurrentAnimation == null))
				return;

			float lastTime = CurrentAnimation.Time;
			float increment = (CurrentAnimation.Speed * GorgonTiming.Delta);

			// Push the animation time forward (or backward, depending on the Speed modifier).
			CurrentAnimation.Time += increment;

			// If we're not looping, put the animation into a stopped state.
			lastTime += increment;
			
			if ((CurrentAnimation.IsLooped) || ((lastTime < CurrentAnimation.Length) && (lastTime > 0)))
			{
				return;
			}

			Stop();
		}
		
		/// <summary>
		/// Function to set an animation playing.
		/// </summary>
		/// <param name="animatedObject">The object to apply the animation onto.</param>
		/// <param name="animation">Animation to play.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> or <paramref name="animatedObject"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		public void Play(T animatedObject, GorgonAnimation<T> animation)
		{
			GorgonDebug.AssertNull(animation, "animation");
			GorgonDebug.AssertNull(animatedObject, "animatedObject");

#if DEBUG
		    if (!Contains(animation))
		    {
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animation.Name));
		    }
#endif

			// This animation is already playing.
		    if (animation == CurrentAnimation)
		    {
		        return;
		    }

		    // Stop the current animation.
		    if (CurrentAnimation != null)
		    {
		        Stop();
		    }

		    AnimatedObject = animatedObject;
			CurrentAnimation = animation;

			// Update to the first frame.
			CurrentAnimation.UpdateObject();
		}

		/// <summary>
		/// Function to set an animation playing.
		/// </summary>
		/// <param name="animatedObject">The object to apply the animation onto.</param>
		/// <param name="animation">Name of the animation to start playing.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> or <paramref name="animatedObject"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the animation parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		public void Play(T animatedObject, string animation)
		{
			GorgonDebug.AssertParamString(animation, animation);

#if DEBUG
            if (!Contains(animation))
            {
                throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animation));
            }
#endif            
			Play(animatedObject, this[animation]);
		}

		/// <summary>
		/// Function to set an animation playing.
		/// </summary>
		/// <param name="animatedObject">The object to apply the animation onto.</param>
		/// <param name="index">Index of the animation to start playing.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animatedObject"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.IndexOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0 or greater than (or equal to) the number of animations.</exception>
		public void Play(T animatedObject, int index)
		{
			GorgonDebug.AssertParamRange(index, 0, Count, "index");
			Play(animatedObject, this[index]);
		}

		/// <summary>
		/// Function to stop the currently playing animation.
		/// </summary>
		public void Stop()
		{
			if (CurrentAnimation == null)
			{
				return;
			}

			AnimatedObject = null;
			CurrentAnimation = null;
		}

		/// <summary>
		/// Function to add a list of animations to the collection.
		/// </summary>
		/// <param name="animations">Animations to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animations"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when an animation in the list already exists in this collection.</exception>
		public void AddRange(IEnumerable<GorgonAnimation<T>> animations)
		{
            var animationList = animations.ToArray();

			GorgonDebug.AssertNull(animationList, "animations");

		    if (animationList.Length == 0)
		    {
		        return;
		    }

		    AddItems(animationList);
		}

		/// <summary>
		/// Function to add an animation to the collection.
		/// </summary>
		/// <param name="animation">Animation to add to the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the animation already exists in this collection.</exception>
		public void Add(GorgonAnimation<T> animation)
		{
			GorgonDebug.AssertNull(animation, "animation");

#if DEBUG
		    if (Contains(animation.Name))
		    {
                throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, animation.Name), "animation");
		    }
#endif

		    AddItem(animation);
		}

		/// <summary>
		/// Function to add an animation to the collection.
		/// </summary>
		/// <param name="name">Name of the animation to add.</param>
		/// <param name="length">Length of the animation, in seconds.</param>
		/// <returns>The newly created animation.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the animation already exists in this collection.</para></exception>
		public GorgonAnimation<T> Add(string name, float length)
		{
		    GorgonDebug.AssertParamString(name, "name");

#if DEBUG
		    if (Contains(name))
		    {
		        throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, name), "name");
		    }
#endif

		    var result = new GorgonAnimation<T>(this, name, length);
			AddItem(result);

			return result;
		}

		/// <summary>
		/// Function to clear the animation collection.
		/// </summary>
		public void Clear()
		{
			// Stop the current animation.
			Stop();
			
			foreach (var item in this)
				item.AnimationController = null;

			ClearItems();
		}

		/// <summary>
		/// Function to remove an animation from the collection.
		/// </summary>
		/// <param name="animation">Animation to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
		public void Remove(GorgonAnimation<T> animation)
		{
			GorgonDebug.AssertNull(animation, "animation");
#if DEBUG
		    if (!Contains(animation))
		    {
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animation.Name));
		    }
#endif

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
#if DEBUG
		    if (!Contains(animation))
		    {
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animation));
		    }
#endif

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
		/// Initializes a new instance of the <see cref="GorgonAnimationController{T}" /> class.
		/// </summary>
		public GorgonAnimationController()
			: base(false)
		{
			AnimatedObjectType = typeof(T);

			AnimatedProperties = (from property in AnimatedObjectType.GetProperties()
								  let attribs = property.GetCustomAttributes(typeof(AnimatedPropertyAttribute), true) as IList<AnimatedPropertyAttribute>
								  where attribs != null && attribs.Count == 1
								  select new GorgonAnimatedProperty(
                                      string.IsNullOrEmpty(attribs[0].DisplayName) ? property.Name : attribs[0].DisplayName, 
                                      attribs[0].DataType ?? property.PropertyType, property))
                                        .ToDictionary(key => key.Property.Name, value => value);
		}
		#endregion
	}
}
