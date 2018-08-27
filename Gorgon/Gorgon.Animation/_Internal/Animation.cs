#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 15, 2018 11:01:36 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Animation
{
    /// <summary>
    /// A base class for a <see cref="IGorgonAnimation"/> implementation.
    /// </summary>
    public class Animation
        : GorgonNamedObject, IGorgonAnimation
    {
        #region Variables.
        // Number of loops for the animation.
        private int _loopCount;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the editable track used to update positioning of an object.
        /// </summary>
        protected internal IGorgonTrack<GorgonKeyVector3> PositionTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the editable track used to update the rotation of an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This track is read/write and should only be used by a corresponding <see cref="IGorgonTrackKeyBuilder{T}"/>. Any other usage is not supported and will have unintended side effects.
        /// </para>
        /// <para>
        /// The rotation track is made up of <see cref="GorgonKeyVector3"/> key frame types where the X, Y and Z values represent the x axis, y axis and z axis of rotation. All values are in degrees.
        /// </para>
        /// <para>
        /// Note that not all controller types will use every axis when rotating. 
        /// </para>
        /// <para>
        /// </para>
        /// </remarks>
        protected internal IGorgonTrack<GorgonKeyVector3> RotationTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the editable track used to update the scale of an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This track is read/write and should only be used by a corresponding <see cref="IGorgonTrackKeyBuilder{T}"/>. Any other usage is not supported and will have unintended side effects.
        /// </para>
        /// </remarks>
        protected internal IGorgonTrack<GorgonKeyVector3> ScaleTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the editable track used to update the color of an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This track is read/write and should only be used by a corresponding <see cref="IGorgonTrackKeyBuilder{T}"/>. Any other usage is not supported and will have unintended side effects.
        /// </para>
        /// </remarks>
        protected internal IGorgonTrack<GorgonKeyGorgonColor> ColorTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the editable track used for rectangular boundaries of an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This track is read/write and should only be used by a corresponding <see cref="IGorgonTrackKeyBuilder{T}"/>. Any other usage is not supported and will have unintended side effects.
        /// </para>
        /// </remarks>
        protected internal IGorgonTrack<GorgonKeyRectangle> RectBoundsTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the editable track used for updating a 2D texture on an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This track is read/write and should only be used by a corresponding <see cref="IGorgonTrackKeyBuilder{T}"/>. Any other usage is not supported and will have unintended side effects.
        /// </para>
        /// </remarks>
        protected internal IGorgonTrack<GorgonKeyTexture2D> Texture2DTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the number of times to loop an animation.
        /// </summary>
        public int LoopCount
        {
            get => _loopCount;
            set => _loopCount = value.Max(0);
        }

        /// <summary>
        /// Property to set or return the speed of the animation.
        /// </summary>
        /// <remarks>Setting this value to a negative value will make the animation play backwards.</remarks>
        public float Speed
        {
            get;
            set;
        } = 1.0f;

        /// <summary>
		/// Property to set or return the length of the animation (in seconds).
		/// </summary>
		public float Length
        {
            get;
        }

		/// <summary>
		/// Property to set or return whether this animation should be looping or not.
		/// </summary>
		public bool IsLooped
		{
			get;
			set;
		}

        /// <summary>
        /// Property to return the track used to update positioning of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyVector3> IGorgonAnimation.PositionTrack => PositionTrack;

        /// <summary>
        /// Property to return the track used to update the rotation of an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The rotation track is made up of <see cref="GorgonKeyVector3"/> key frame types where the X, Y and Z values represent the x axis, y axis and z axis of rotation. All values are in degrees.
        /// </para>
        /// <para>
        /// Note that not all controller types will use every axis when rotating. 
        /// </para>
        /// </remarks>
        IGorgonTrack<GorgonKeyVector3> IGorgonAnimation.RotationTrack => RotationTrack;

        /// <summary>
        /// Property to return the track used to update the scale of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyVector3> IGorgonAnimation.ScaleTrack => ScaleTrack;

        /// <summary>
        /// Property to return the track used to update the color of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyGorgonColor> IGorgonAnimation.ColorTrack => ColorTrack;

        /// <summary>
        /// Property to return the track used for rectangular boundaries of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyRectangle> IGorgonAnimation.RectBoundsTrack => RectBoundsTrack;

        /// <summary>
        /// Property to return the track used for updating a 2D texture on an object.
        /// </summary>
        IGorgonTrack<GorgonKeyTexture2D> IGorgonAnimation.Texture2DTrack => Texture2DTrack;
        #endregion

        #region Methods.
        /*/// <summary>
		/// Function to update each track in the animation.
		/// </summary>
	    public void Update()
		{
		    // Notify each track to update their animation to the current time.
			foreach (IGorgonAnimationTrack track in Tracks)
			{
			    if ((track.KeyFrames.Count <= 0) 
                    || ((track.TimeUpdatedCallback != null) 
                    && (!track.TimeUpdatedCallback(track, Time))))
			    {
			        continue;
			    }

				IGorgonKeyFrame key = track.GetKeyAtTime(_time);
				track.ApplyKey(key);
			}
		}*/

        // TODO: This goes into a codec.
        /*
		/// <summary>
		/// Function to save the animation to a stream.
		/// </summary>
		/// <param name="stream">Stream to write the animation into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
		/// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read-only.</exception>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GORANM_ERR_STREAM_READ_ONLY);
			}

			IGorgonChunkFileWriter animFile = new GorgonChunkFileWriter(stream, GorgonAnimationController.AnimationVersion.ChunkID());

			try
			{
				animFile.Open();

				GorgonBinaryWriter writer = animFile.OpenChunk(GorgonAnimationController.AnimationChunk.ChunkID());
				writer.Write(AnimationController.AnimatedObjectType.FullName);
				writer.Write(Name);
				writer.Write(Length);
				writer.Write(IsLooped);

				animFile.CloseChunk();

				// Put out the tracks with the most keys first.
				var activeTracks = from GorgonAnimationTrack track in Tracks
								   where track.KeyFrames.Count > 0
								   select track;
				
				foreach (GorgonAnimationTrack track in activeTracks)
				{
					track.ToChunk(animFile);
				}
			}
			finally
			{
				animFile.Close();
			}
		}

		/// <summary>
		/// Function to save the animation to a file.
		/// </summary>
		/// <param name="fileName">Path and file name of the file to write.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b>.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.</exception>
		public void Save(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(Resources.GORANM_PARAMETER_MUST_NOT_BE_EMPTY, "fileName");
			}

		    using (FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
		    {
		        Save(stream);
		    }
		}
        */
        /*
		/// <summary>
		/// Function to reset the animation state.
		/// </summary>
		public void Reset()
		{
			_time = 0;
			_looped = 0;
			Update();
		}*/
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Animation" /> class.
        /// </summary>
        /// <param name="name">The name of the track.</param>
        /// <param name="length">The length of the animation, in seconds.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public Animation(string name, float length)
            : base(name) => Length = length.Max(0);
        #endregion
    }
}
