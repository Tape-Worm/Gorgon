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
using Gorgon.IO;
using Gorgon.Timing;

namespace Gorgon.Animation
{
	/// <summary>
	/// Manages and controls animations for an object.
	/// </summary>
	/// <typeparam name="T">The type of object that this controller will use.  The type passed in must be a reference type (i.e. a class).</typeparam>
	/// <remarks>
	/// A controller will force the object to update its properties over a certain time frame (or continuously if looped).  It does this by placing its animated properties into the <see cref="GorgonAnimation{T}.Tracks"/> of an 
	/// <see cref="GorgonAnimation{T}"/>.  These tracks will take <see cref="GorgonAnimationTrack{T}.KeyFrames"/> which correspond to the type of the property.  For example, the Angle property
	/// of a sprite uses a floating point value, so a <see cref="GorgonKeySingle"/>, or floating point key frame should be added to the Angle track.
	/// <para>To ensure the object will animate, it should have a <see cref="AnimatedPropertyAttribute"/> applied to one of its properties.  Otherwise, no animations will play.  Currently, Gorgon's graphical objects (e.g. sprites, text, etc...)
	/// all have appropriate attributes assigned to their properties.</para>
	/// <para>A user may add a custom track by inheriting from <see cref="GorgonAnimationTrack{T}"/> and creating a custom key frame type that implements <see cref="IKeyFrame"/>, and then adding a instance of the 
	/// custom track to the animation (not the controller).</para>
	/// </remarks>
	public class GorgonAnimationController<T>
		: GorgonBaseNamedObjectDictionary<GorgonAnimation<T>>
		where T : class
	{
		#region Constants.
		// Animation data chunk.
		internal const string AnimationChunk = "ANIMDATA";

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
		/// Property to set or return an animation by name.
		/// </summary>
		public GorgonAnimation<T> this[string name]
		{
			get
			{
				return Items[name];
			}
			set
			{
				if (value == null)
				{
					if (Contains(name))
					{
						Items.Remove(name);
					}
					return;
				}

				if (Contains(name))
					SetItem(name, value);
				else
					Add(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set an item by its name.
		/// </summary>
		/// <param name="name">Name of the item to set.</param>
		/// <param name="value">Value to set.</param>
		private void SetItem(string name, GorgonAnimation<T> value)
		{
			if (CurrentAnimation == this[name])
			{
				Stop();
			}

			// Remove the clip from the other 
		    if ((value != null) && (value.AnimationController != null) && (value.AnimationController != this) &&
		        (value.AnimationController.Contains(value)))
		    {
		        value.AnimationController.Remove(value);
		    }

		    this[name].AnimationController = this;

			UpdateItem(name, value);
		}

		/// <summary>
		/// Function to load an animation from a stream.
		/// </summary>
		/// <param name="stream">Stream to load from.</param>
		/// <returns>The animation in the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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

			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			IGorgonChunkFileReader animFile = new GorgonChunkFileReader(stream,
			                                                          new[]
			                                                          {
				                                                          AnimationVersion.ChunkID()
			                                                          });

			try
			{
				animFile.Open();
				GorgonBinaryReader reader = animFile.OpenChunk(AnimationChunk);

				// Ensure this type matches the data in the animation file.
				string typeString = reader.ReadString();
				if (!string.Equals(typeString, AnimatedObjectType.FullName, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidCastException(string.Format(Resources.GORANM_ANIMATION_TYPE_MISMATCH,
																 typeString,
																 AnimatedObjectType.FullName));
				}

				// Read name.
				string animationName = reader.ReadString();
				if (Contains(animationName))
				{
					throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, animationName),
												"stream");
				}

				animation = new GorgonAnimation<T>(this, animationName, reader.ReadSingle())
				            {
					            IsLooped = reader.ReadBoolean()
				            };

				animFile.CloseChunk();

				// Read tracks.
				GorgonAnimationTrack<T>.FromChunk(animFile, animation);
			}
			finally
			{
				animFile.Close();
			}

			Add(animation);

			return animation;
		}

		/// <summary>
		/// Function to load an animation from a file.
		/// </summary>
		/// <param name="fileName">Path and file name for the animation file.</param>
		/// <returns>The loaded animation from the file.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "fileName");
			}

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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> or <paramref name="animatedObject"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		public void Play(T animatedObject, GorgonAnimation<T> animation)
		{
			if (animation == null)
			{
				throw new ArgumentNullException("animation");
			}

			if (animatedObject == null)
			{
				throw new ArgumentNullException("animatedObject");
			}

		    if (!Contains(animation))
		    {
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animation.Name));
		    }

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
		/// <param name="animationName">Name of the animation to start playing.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animationName"/> or <paramref name="animatedObject"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the animation parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		public void Play(T animatedObject, string animationName)
		{
			if (animationName == null)
			{
				throw new ArgumentNullException("animationName");
			}

			if (string.IsNullOrWhiteSpace(animationName))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "animationName");
			}

			if (animatedObject == null)
			{
				throw new ArgumentNullException("animatedObject");
			}

			if (!Contains(animationName))
            {
                throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animationName));
            }
      
			Play(animatedObject, this[animationName]);
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animations"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when an animation in the list already exists in this collection.</exception>
		public void AddRange(IEnumerable<GorgonAnimation<T>> animations)
		{
			if (animations == null)
			{
				throw new ArgumentNullException("animations");
			}

			foreach (GorgonAnimation<T> item in animations)
			{
				Add(item);
			}
		}

		/// <summary>
		/// Function to add an animation to the collection.
		/// </summary>
		/// <param name="animation">Animation to add to the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the animation already exists in this collection.</exception>
		public void Add(GorgonAnimation<T> animation)
		{
			if (animation == null)
			{
				throw new ArgumentNullException("animation");
			}
			
		    if (Contains(animation.Name))
		    {
                throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, animation.Name), "animation");
		    }

			if ((animation.AnimationController != null) && (animation.AnimationController != this) &&
				(animation.AnimationController.Contains(animation)))
			{
				animation.AnimationController.Remove(animation);
				animation.AnimationController = this;
			}

			Items.Add(animation.Name, animation);
		}

		/// <summary>
		/// Function to add an animation to the collection.
		/// </summary>
		/// <param name="name">Name of the animation to add.</param>
		/// <param name="length">Length of the animation, in seconds.</param>
		/// <returns>The newly created animation.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the animation already exists in this collection.</para></exception>
		public GorgonAnimation<T> Add(string name, float length)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			if (Contains(name))
		    {
		        throw new ArgumentException(string.Format(Resources.GORANM_ANIMATION_ALREADY_EXISTS, name), "name");
		    }

		    var result = new GorgonAnimation<T>(this, name, length);

			Add(result);

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

			Items.Clear();
		}

		/// <summary>
		/// Function to remove an animation from the collection.
		/// </summary>
		/// <param name="animation">Animation to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
		public void Remove(GorgonAnimation<T> animation)
		{
			if (animation == null)
			{
				throw new ArgumentNullException("animation");
			}
			
		    if (!Contains(animation))
		    {
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animation.Name));
		    }

			if (CurrentAnimation == animation)
			{
				Stop();
			}

			animation.AnimationController = null;

		    RemoveItem(animation);
		}

		/// <summary>
		/// Function to remove an animation from the collection.
		/// </summary>
		/// <param name="animationName">Name of the animation to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="animationName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the animation parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the animation was not found in the collection.</exception>
		public void Remove(string animationName)
		{
			GorgonAnimation<T> animation;

			if (animationName == null)
			{
				throw new ArgumentNullException("animationName");
			}

			if (string.IsNullOrWhiteSpace(animationName))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "animationName");
			}

			if (!Items.TryGetValue(animationName, out animation))
			{
		        throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animationName));
		    }

			if (CurrentAnimation == animation)
			{
				Stop();
			}

			animation.AnimationController = null;

			Items.Remove(animationName);
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
			                      select new GorgonAnimatedProperty(property, attribs[0].DisplayName, attribs[0].DataType))
				.ToDictionary(key => key.Property.Name, value => value);
		}
		#endregion
	}
}
