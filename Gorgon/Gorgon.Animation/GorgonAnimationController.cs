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
using DX = SharpDX;
using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Animation
{
    /// <summary>
	/// Manages and controls animations for an object.
	/// </summary>
	/// <typeparam name="T">The type of object that this controller will use.  The type passed in must be a reference type (i.e. a class).</typeparam>
	/// <remarks>
	/// <para>
	/// A controller will update the object properties over a certain time frame (or continuously if looped).
	/// </para>
	/// <para>
	/// This is done by placing its animated properties into the <see cref="Animation.Tracks"/> of an <see cref="Animation"/>. These tracks will take
	/// <see cref="GorgonAnimationTrack{T}.KeyFrames"/> which correspond to the type of the property being animated.  For example, the Angle property of a sprite uses a floating point value, so a
	/// <see cref="GorgonKeyFloat"/> should be added to the Angle track.
	/// </para>
	/// <para>
	/// To ensure the object will animate, it should have a <see cref="PropertyTagAttribute"/> applied to at least one of its properties, specifying a tag of
	/// <see cref="GorgonReservedPropertyTags.GorgonAnimation"/>.  Otherwise, no animations will play.
	/// </para>
	/// <para>
	/// A user may add a custom track by inheriting from <see cref="GorgonAnimationTrack{T}"/> and creating a custom key frame type that implements <see cref="IGorgonKeyFrame"/>, and then adding a instance of
	/// the  custom track to the animation (not the controller).
	/// </para>
	/// </remarks>
	/// <seealso cref="PropertyTagAttribute"/>
	/// <seealso cref="GorgonReservedPropertyTags"/>
	/// <seealso cref="Animation{T}"/>
	/// <seealso cref="GorgonKeyFloat"/>
	public abstract class GorgonAnimationController<T>
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

        #region Variables.
        // The ID for the controller.
	    private readonly Guid _id = Guid.NewGuid();
        // The animations available to the controller.
	    private readonly AnimationCollection _animations;
        // The time index.
	    private float _time;
        // The loop count for the current animation.
        private int _loopCount;
	    // The object that is to be animated.
	    private T _animatedObject;
        #endregion

		#region Properties.
	    /// <summary>
	    /// Property to return the list of animations available to this controller.
	    /// </summary>
	    public IGorgonAnimationCollection Animations => _animations;

		/// <summary>
		/// Property to return the currently playing animation.
		/// </summary>
		public IGorgonAnimation CurrentAnimation
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to set or return the current time index.
        /// </summary>
	    public float Time
        {
            get => _time;
            set
            {
                if (_time.EqualsEpsilon(value))
                {
                    return;
                }

                _time = value.Max(0);

                if (CurrentAnimation == null)
                {
                    return;
                }

                if (((CurrentAnimation.IsLooped) && (_time > CurrentAnimation.Length)) || (value < 0))
                {
                    // Loop the animation.
                    if ((CurrentAnimation.LoopCount != 0) && (_loopCount == CurrentAnimation.LoopCount))
                    {
                        return;
                    }

                    _loopCount++;
                    _time = _time % CurrentAnimation.Length;

                    if (CurrentAnimation.Speed < 0)
                    {
                        _time += CurrentAnimation.Length;
                    }

                    NotifyAnimation();
                    return;
                }

                if (_time < 0)
                {
                    _time = 0;
                }

                if (_time > CurrentAnimation.Length)
                {
                    _time = CurrentAnimation.Length;
                }

                NotifyAnimation();
            }
	    }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to update the animation.
        /// </summary>
	    private void NotifyAnimation()
	    {
            // Update each track.
	        if (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation, CurrentAnimation.PositionTrack, _time, out DX.Vector3 posValue))
	        {
                OnPositionUpdate(_animatedObject, posValue);
	        }

	        if (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation, CurrentAnimation.ScaleTrack, _time, out DX.Vector3 scaleValue))
	        {
                OnScaleUpdate(_animatedObject, scaleValue);
	        }

	        if (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation, CurrentAnimation.RotationTrack, _time, out DX.Vector3 rotValue))
	        {
	            OnRotationUpdate(_animatedObject, rotValue);
	        }

	        if (TrackKeyProcessor.TryUpdateColor(CurrentAnimation, CurrentAnimation.ColorTrack, _time, out GorgonColor colorValue))
	        {
                OnColorUpdate(_animatedObject, colorValue);
	        }

	        if (TrackKeyProcessor.TryUpdateRectBounds(CurrentAnimation, CurrentAnimation.RectBoundsTrack, _time, out DX.RectangleF rectValue))
	        {
	            OnRectBoundsUpdate(_animatedObject, rectValue);
	        }

	        if (TrackKeyProcessor.TryUpdateTexture2D(CurrentAnimation,
	                                                 _time,
                                                     CurrentAnimation.Texture2DTrack,
	                                                 out GorgonTexture2DView texture,
	                                                 out DX.RectangleF textureCoordinates,
	                                                 out int textureArrayIndex))
	        {
                OnTexture2DUpdate(_animatedObject, texture, textureCoordinates, textureArrayIndex);
	        }
	    }

        // TODO: Put into codec.
        /*
		/// <summary>
		/// Function to load an animation from a stream.
		/// </summary>
		/// <param name="stream">Stream to load from.</param>
		/// <returns>The animation in the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
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
				throw new ArgumentNullException(nameof(stream));
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
												nameof(stream));
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b>.</exception>
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
				throw new ArgumentNullException(nameof(fileName));
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, nameof(fileName));
			}

		    using (FileStream file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		    {
		        return FromStream(file);
		    }
		}
        */

        /// <summary>
        /// Function to handle when an animation is removed or the animations list is cleared.
        /// </summary>
        /// <param name="animation">The animation to stop, or <b>null</b> if the collection was cleared.</param>
	    private void AnimationRemovedCleared(IGorgonAnimation animation)
	    {
            // If null is passed, then any playing animation should stop.
	        if ((animation == null) || (animation == CurrentAnimation))
	        {
                Stop();
	        }
	    }

	    /// <summary>
	    /// Function called when a rectangle boundary needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="bounds">The new bounds.</param>
	    protected abstract void OnRectBoundsUpdate(T animObject, DX.RectangleF bounds);

	    /// <summary>
	    /// Function called when a texture needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="texture">The texture to switch to.</param>
	    /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
	    /// <param name="textureArrayIndex">The texture array index.</param>
	    protected abstract void OnTexture2DUpdate(T animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex);

        /// <summary>
        /// Function called when a position needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="position">The new position.</param>
	    protected abstract void OnPositionUpdate(T animObject, DX.Vector3 position);

	    /// <summary>
	    /// Function called when a scale needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="scale">The new scale.</param>
	    protected abstract void OnScaleUpdate(T animObject, DX.Vector3 scale);

	    /// <summary>
	    /// Function called when the angle of rotation needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="rotation">The new angle of rotation, in degrees, on the x, y and z axes.</param>
	    protected abstract void OnRotationUpdate(T animObject, DX.Vector3 rotation);

        /// <summary>
        /// Function called when the color needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="color">The new color.</param>
	    protected abstract void OnColorUpdate(T animObject, GorgonColor color);

		/// <summary>
		/// Function to update the currently playing animation time and bound properties.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will update the animation time using the <see cref="GorgonTiming.Delta">Delta</see> time.  Note that the animation time is not affected by
		/// <see cref="GorgonTiming.ScaledDelta">ScaledDelta</see>.
		/// </para>
		/// <para>
		/// Users should call this method once per frame in order to update the current state of the playing (by calling <see cref="Play(T,IGorgonAnimation)"/> animation.  If no animation is playing,
		/// then this method will do nothing.
		/// </para>
		/// </remarks>
		/// <seealso cref="GorgonTiming"/>
		public void Update()
		{
		    if ((Animations.Count == 0) || (CurrentAnimation == null))
		    {
		        return;
		    }

		    float lastTime = Time;
			float increment = (CurrentAnimation.Speed * GorgonTiming.Delta);

			// Push the animation time forward (or backward, depending on the Speed modifier).
		    Time += increment;
		    lastTime += increment;

		    // If we're not looping, put the animation into a stopped state.
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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/> or <paramref name="animatedObject"/> parameters are <b>null</b>.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		/// <remarks>
		/// <para>
		/// Applications should call this method to start an animation for an object. Otherwise, no animation will play when <see cref="Update"/> is called.
		/// </para>
		/// </remarks>
		public void Play(T animatedObject, IGorgonAnimation animation)
		{
			if (animation == null)
			{
				throw new ArgumentNullException(nameof(animation));
			}

            if (!Animations.Contains(animation))
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

		    if (Time > animation.Length)
		    {
		        Time = animation.Length;
		    }

		    if (animation.Speed < 0)
		    {
		        Time = animation.Length - Time;
		    }
            
		    _animatedObject = animatedObject ?? throw new ArgumentNullException(nameof(animatedObject));
			CurrentAnimation = animation;
		    _loopCount = 0;

			// Update to the first frame.
		    NotifyAnimation();
		}

		/// <summary>
		/// Function to set an animation playing.
		/// </summary>
		/// <param name="animatedObject">The object to apply the animation onto.</param>
		/// <param name="animationName">Name of the animation to start playing.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="animationName"/> or <paramref name="animatedObject"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the animation parameter is an empty string.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		/// <remarks>
		/// <para>
		/// Applications should call this method to start an animation for an object. Otherwise, no animation will play when <see cref="Update"/> is called.
		/// </para>
		/// </remarks>
		public void Play(T animatedObject, string animationName)
		{
			if (animationName == null)
			{
				throw new ArgumentNullException(nameof(animationName));
			}

			if (string.IsNullOrWhiteSpace(animationName))
			{
				throw new ArgumentEmptyException(nameof(animationName));
			}

			if (animatedObject == null)
			{
				throw new ArgumentNullException(nameof(animatedObject));
			}

			if (!Animations.Contains(animationName))
            {
                throw new KeyNotFoundException(string.Format(Resources.GORANM_ANIMATION_DOES_NOT_EXIST, animationName));
            }

		    IGorgonAnimation animation = Animations[animationName];
      
			Play(animatedObject, animation);
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

		    _loopCount = 0;
			_animatedObject = null;
			CurrentAnimation = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimationController{T}" /> class.
		/// </summary>
		protected GorgonAnimationController()
		{
            _animations = new AnimationCollection(AnimationRemovedCleared);
		}
		#endregion
	}
}
