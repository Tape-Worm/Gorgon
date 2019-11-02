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
using Gorgon.Animation.Properties;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Animation
{
    /// <summary>
    /// The current state of the animation.
    /// </summary>
    public enum AnimationState
    {
        /// <summary>
        /// The animation is playing.
        /// </summary>
        Playing = 0,
        /// <summary>
        /// The animation is stopped.  When it starts playing again, it will restart.
        /// </summary>
        Stopped = 1,
        /// <summary>
        /// The animation is paused.  When it starts playing again, it will continue from where it left off.
        /// </summary>
        Paused = 2
    }

    /// <summary>
	/// Base class for applying animations to an object.
	/// </summary>
	/// <typeparam name="T">The type of object that this controller will use.  The type passed in must be a reference type (i.e. a class).</typeparam>
	/// <remarks>
	/// <para>
	/// A controller will update the object properties over a certain time frame (or continuously if looped) using a <see cref="IGorgonAnimation"/>.
	/// </para>
	/// <para>
	/// This controller will advance the time for an animation, and coordinate the changes from interpolation (if supported) between <see cref="IGorgonKeyFrame"/> items on a <see cref="IGorgonAnimationTrack{T}"/>.
	/// The values from the animation will then by applied to the object properties.
	/// </para>
	/// <para>
	/// Applications can force the playing animation to jump to a specific <see cref="Time"/>, or increment the time step smoothly using the <see cref="Update"/> method.
	/// </para>
	/// <para>
	/// <note type="important">
	/// Please note that this is an abstract class. Applications will provide specific controllers for specific types.
	/// </note>
	/// </para>
    /// <para>
    /// <note type="information">
    /// Because this is a base class, not all controllers will support all track types, or even components of a track key frame.
    /// </note>
    /// </para>
	/// </remarks>
	/// <seealso cref="IGorgonAnimation"/>
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
        // The time index.
        private float _time;
        // The loop count for the current animation.
        private int _loopCount;
        // The object that is to be animated.
        private T _animatedObject;
        // The current animation state.
        private AnimationState _state = AnimationState.Stopped;
        // The list of registered track names.
        private readonly List<GorgonTrackRegistration> _trackNames = new List<GorgonTrackRegistration>();
        // The list of registered track names that can be played with a given animation.
        private readonly List<GorgonTrackRegistration> _playableTracks = new List<GorgonTrackRegistration>();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of available tracks used by this controller.
        /// </summary>
        public IReadOnlyList<GorgonTrackRegistration> RegisteredTracks => _trackNames;

        /// <summary>
        /// Property to return the currently playing animation.
        /// </summary>
        public IGorgonAnimation CurrentAnimation
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current state of the animation on the controller.
        /// </summary>
        public AnimationState State
        {
            get => CurrentAnimation == null ? AnimationState.Stopped : _state;
            private set
            {
                if (CurrentAnimation == null)
                {
                    return;
                }

                _state = value;
            }
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

                _time = value;

                if (CurrentAnimation == null)
                {
                    return;
                }

                if ((CurrentAnimation.IsLooped) && (_time > CurrentAnimation.Length))
                {
                    // Loop the animation.
                    if ((CurrentAnimation.LoopCount != 0) && (_loopCount == CurrentAnimation.LoopCount))
                    {
                        return;
                    }

                    _loopCount++;
                    _time %= CurrentAnimation.Length;

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
                    State = AnimationState.Stopped;
                }

                if (_time > CurrentAnimation.Length)
                {
                    _time = CurrentAnimation.Length;
                    State = AnimationState.Stopped;
                }

                // Do not update the animation when we're in a paused state.
                if (State == AnimationState.Paused)
                {
                    return;
                }

                NotifyAnimation();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the playable track list.
        /// </summary>
        /// <param name="animation">The animation to evaluate.</param>
        private void BuildPlayableTracks(IGorgonAnimation animation)
        {
            _playableTracks.Clear();

            for (int i = 0; i < _trackNames.Count; ++i)
            {
                GorgonTrackRegistration registration = _trackNames[i];

                switch (registration.KeyType)
                {
                    case AnimationTrackKeyType.Single:
                        if (animation.SingleTracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                    case AnimationTrackKeyType.Vector2:
                        if (animation.Vector2Tracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                    case AnimationTrackKeyType.Vector3:
                        if (animation.Vector3Tracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                    case AnimationTrackKeyType.Vector4:
                        if (animation.Vector4Tracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                    case AnimationTrackKeyType.Rectangle:
                        if (animation.RectangleTracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                    case AnimationTrackKeyType.Color:
                        if (animation.ColorTracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                    case AnimationTrackKeyType.Texture2D:
                        if (animation.Texture2DTracks.ContainsKey(registration.TrackName))
                        {
                            _playableTracks.Add(registration);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Function to update the animation.
        /// </summary>
        private void NotifyAnimation()
        {
            for (int i = 0; i < _playableTracks.Count; ++i)
            {
                GorgonTrackRegistration registration = _playableTracks[i];

                switch (registration.KeyType)
                {
                    case AnimationTrackKeyType.Single:
                        if ((CurrentAnimation.SingleTracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeySingle> singleTrack))
                            && (TrackKeyProcessor.TryUpdateSingle(CurrentAnimation.Length, singleTrack, _time, out float singleValue)))
                        {
                            OnSingleValueUpdate(registration, _animatedObject, singleValue);
                        }
                        break;
                    case AnimationTrackKeyType.Vector2:
                        if ((CurrentAnimation.Vector2Tracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeyVector2> vec2DTrack))
                            && (TrackKeyProcessor.TryUpdateVector2(CurrentAnimation.Length, vec2DTrack, _time, out DX.Vector2 vec2DValue)))
                        {
                            OnVector2ValueUpdate(registration, _animatedObject, vec2DValue);
                        }
                        break;
                    case AnimationTrackKeyType.Vector3:
                        if ((CurrentAnimation.Vector3Tracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeyVector3> vec3DTrack))
                            && (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation.Length, vec3DTrack, _time, out DX.Vector3 vec3DValue)))
                        {
                            OnVector3ValueUpdate(registration, _animatedObject, vec3DValue);
                        }
                        break;
                    case AnimationTrackKeyType.Vector4:
                        if ((CurrentAnimation.Vector4Tracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeyVector4> vec4DTrack))
                            && (TrackKeyProcessor.TryUpdateVector4(CurrentAnimation.Length, vec4DTrack, _time, out DX.Vector4 vec4DValue)))
                        {
                            OnVector4ValueUpdate(registration, _animatedObject, vec4DValue);
                        }
                        break;
                    case AnimationTrackKeyType.Rectangle:
                        if ((CurrentAnimation.RectangleTracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeyRectangle> rectTrack))
                            && (TrackKeyProcessor.TryUpdateRectBounds(CurrentAnimation.Length, rectTrack, _time, out DX.RectangleF rectValue)))
                        {
                            OnRectangleUpdate(registration, _animatedObject, rectValue);
                        }
                        break;
                    case AnimationTrackKeyType.Color:
                        if ((CurrentAnimation.ColorTracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeyGorgonColor> colorTrack))
                            && (TrackKeyProcessor.TryUpdateColor(CurrentAnimation.Length, colorTrack, _time, out GorgonColor colorValue)))
                        {
                            OnColorUpdate(registration, _animatedObject, colorValue);
                        }
                        break;
                    case AnimationTrackKeyType.Texture2D:
                        if ((CurrentAnimation.Texture2DTracks.TryGetValue(registration.TrackName, out IGorgonAnimationTrack<GorgonKeyTexture2D> textureTrack))
                            && (TrackKeyProcessor.TryUpdateTexture2D(CurrentAnimation.Length, textureTrack, _time, out GorgonTexture2DView texture, out DX.RectangleF texCoords, out int texArray)))
                        {
                            OnTexture2DUpdate(registration, _animatedObject, texture, texCoords, texArray);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Function called when a single floating point value needs to be updated on the animated object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected abstract void OnSingleValueUpdate(GorgonTrackRegistration track, T animObject, float value);

        /// <summary>
        /// Function called when a 2D vector value needs to be updated on the animated object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected abstract void OnVector2ValueUpdate(GorgonTrackRegistration track, T animObject, DX.Vector2 value);

        /// <summary>
        /// Function called when a 3D vector value needs to be updated on the animated object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected abstract void OnVector3ValueUpdate(GorgonTrackRegistration track, T animObject, DX.Vector3 value);

        /// <summary>
        /// Function called when a 4D vector value needs to be updated on the animated object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected abstract void OnVector4ValueUpdate(GorgonTrackRegistration track, T animObject, DX.Vector4 value);

        /// <summary>
        /// Function called when a <see cref="GorgonColor"/> value needs to be updated on the animated object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
	    protected abstract void OnColorUpdate(GorgonTrackRegistration track, T animObject, GorgonColor value);

        /// <summary>
        /// Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected abstract void OnRectangleUpdate(GorgonTrackRegistration track, T animObject, DX.RectangleF value);

        /// <summary>
        /// Function called when a texture needs to be updated on the object.
        /// </summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected abstract void OnTexture2DUpdate(GorgonTrackRegistration track, T animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex);

        /// <summary>
        /// Function to register a track with the controller.
        /// </summary>
        /// <param name="registration">The data used to register the track.</param>
        /// <exception cref="ArgumentException">Thrown when the track <paramref name="registration"/> data is already registered.</exception>
        /// <exception cref="NotSupportedException">Thrown when the <paramref name="registration"/> <see cref="GorgonTrackRegistration.KeyType"/> parameter does not have an equivalent supported track key frame data type.</exception>
        protected void RegisterTrack(GorgonTrackRegistration registration)
        {
            registration.TrackName.ValidateObject(nameof(registration.TrackName));

            // Check for a track with the same name.
            if (_trackNames.Any(item => item.Equals(registration)))
            {
                throw new ArgumentException(Resources.GORANM_TRACK_ALREADY_EXISTS, nameof(registration));
            }

            _trackNames.Add(registration);
        }

        /// <summary>
        /// Function to update the currently playing animation time and bound properties.
        /// </summary>
        /// <param name="timingDelta">[Optional] The delta time for a frame to be rendered.</param>
        /// <remarks>        
        /// <para>
        /// If the <paramref name="timingDelta"/> is not <b>null</b>, then the value passed will represent the amount of time it takes the GPU to render a frame. This is useful for fixed timestep 
        /// animations. If it is left as <b>null</b>, then <see cref="GorgonTiming.Delta"/> on the <see cref="GorgonTiming"/> class is used for variable timestep.
        /// </para>
        /// <para>
        /// Users should call this method once per frame in order to update the current state of the playing (by calling <see cref="Play(T,IGorgonAnimation)"/> animation.  If no animation is playing,
        /// then this method will do nothing.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTiming"/>
        public void Update(float? timingDelta = null)
        {
            if (State != AnimationState.Playing)
            {
                return;
            }

            float increment = (CurrentAnimation.Speed * (timingDelta ?? GorgonTiming.Delta));

            // Push the animation time forward (or backward, depending on the Speed modifier).
            Time += increment;

            // If we've stopped the animation, then reset it.
            if (State == AnimationState.Stopped)
            {
                _loopCount = 0;
                _animatedObject = null;
                CurrentAnimation = null;
                Time = 0;
            }
        }

        /// <summary>
        /// Function to reset the currently playing animation back to the start of the animation.
        /// </summary>
	    public void Reset()
        {
            if (CurrentAnimation == null)
            {
                Time = 0;
                return;
            }

            if (CurrentAnimation.Speed >= 0)
            {
                Time = 0;
            }
            else if (CurrentAnimation.Speed < 0)
            {
                Time = CurrentAnimation.Length;
            }
        }

        /// <summary>
        /// Function to set an animation playing on an object.
        /// </summary>
        /// <param name="animatedObject">The object to apply the animation onto.</param>
        /// <param name="animation">The <see cref="IGorgonAnimation"/> to play.</param>
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

            // This animation is already playing.
            if (animation == CurrentAnimation)
            {
                return;
            }

            BuildPlayableTracks(animation);

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

            State = AnimationState.Playing;
        }

        /// <summary>
        /// Function to pause the currently executing animation.
        /// </summary>
	    public void Pause()
        {
            if ((CurrentAnimation == null) || (_animatedObject == null))
            {
                State = AnimationState.Stopped;
                return;
            }

            State = AnimationState.Paused;
        }

        /// <summary>
        /// Function to resume a paused animation.
        /// </summary>
        public void Resume()
        {
            if ((CurrentAnimation == null) || (_animatedObject == null))
            {
                State = AnimationState.Stopped;
                return;
            }

            State = AnimationState.Playing;
        }

        /// <summary>
        /// Function to stop the currently playing animation.
        /// </summary>
        public void Stop()
        {
            Reset();

            if (CurrentAnimation == null)
            {
                return;
            }

            _playableTracks.Clear();
            State = AnimationState.Stopped;
            _loopCount = 0;
            _animatedObject = null;
            CurrentAnimation = null;
        }
        #endregion
    }
}
