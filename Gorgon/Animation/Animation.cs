#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, November 20, 2006 1:04:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary.Internal;
using GorgonLibrary.Timing;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics.Animations
{
    /// <summary>
    /// Object representing an animation.
    /// </summary>
    public class Animation
        : NamedObject, ISerializable
    {
        #region Variables.
        private IAnimatable _owner = null;					// Object that owns this animation.        
        private float _length = 1000;                       // Length of the track in milliseconds.
        private bool _loop = false;                         // Flag to indicate that this animation should loop.
        private float _currentTime = 0;                     // Current time.
        private bool _enabled;                              // Flag to indicate whether the animation is enabled or not.
        private bool _isStopped;                            // Flag to indicate whether the animation has stopped or not.
        
        /// <summary>Transformation keys</summary>        
        protected TrackTransformList _transforms = null;
        /// <summary>Color keys</summary>
        protected TrackColorList _colors = null;
		/// <summary>Frame switch list.</summary>
		protected TrackFrameList _frames = null;
        #endregion

        #region Events.
        /// <summary>
        /// Event fired when an animation has stopped.
        /// </summary>
        public event EventHandler AnimationStopped;
        /// <summary>
        /// Event fired when an animation has started.
        /// </summary>
        public event EventHandler AnimationStarted;
        /// <summary>
        /// Event fired when the animation current time has advanced.
        /// </summary>
        public event AnimationAdvanceHandler AnimationAdvanced;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the animation should be looping or not.
        /// </summary>
        public bool Looped
        {
            get
            {
                return _loop;
            }
            set
            {
                _loop = value;
            }
        }

        /// <summary>
        /// Property to set or return whether the animation is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (!value)
                    Stop();

                _enabled = value;
            }
        }

        /// <summary>
        /// Property to return the owner of this animation.
        /// </summary>
        public IAnimatable Owner
        {
            get
            {
                return _owner;
            }
        }

        /// <summary>
        /// Property to return the transformation track list for the animation.
        /// </summary>
        public TrackTransformList TransformTracks
        {
            get
            {
                return _transforms;
            }
        }

        /// <summary>
        /// Property to return the color track list for the animation.
        /// </summary>
        public TrackColorList ColorTracks
        {
            get
            {
                return _colors;
            }
        }

		/// <summary>
		/// Property to return the frame track list for the animation.
		/// </summary>
		public TrackFrameList FrameTracks
		{
			get
			{
				return _frames;
			}
		}

        /// <summary>
        /// Property to set or return the length of the animation.
        /// </summary>
        public float Length
        {
            get
            {
                return _length;
            }
            set
            {
                if (value <= 0)
                    value = float.MaxValue;
                _length = value;
            }
        }

        /// <summary>
        /// Property to return whether an animation has stopped or not.
        /// </summary>
        public bool IsStopped
        {
            get
            {
                return _isStopped;
            }
        }

        /// <summary>
        /// Property to set or return the current time.
        /// </summary>
        public float CurrentTime
        {
            get
            {
                return _currentTime;
            }
            set
            {
                // Do nothing if stopped.
                if (_isStopped)
                    return;

                if (AnimationAdvanced != null)
                {
                    // Animation event arguments.
                    AnimationAdvanceEventArgs eventArgs = new AnimationAdvanceEventArgs(value, value - _currentTime);

                    AnimationAdvanced(this, eventArgs);
                    _currentTime = eventArgs.CurrentFrameTime;
                }
                else
                    _currentTime = value;

                if (_loop)
                {
                    // Reset to beginning.
                    _currentTime = _currentTime % _length;
                    if (_currentTime < 0)
                        _currentTime += _length;
                }
                else
                {
                    // Clamp to beginning or end.
                    if (_currentTime <= 0)
                    {
                        _currentTime = 0;
                        if (_currentTime < 0)
                            Stop();
                    }
                    if (_currentTime >= _length)
                    {
                        _currentTime = _length;
                        Stop();
                    }
                }                
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to apply the animation to this owning object.
        /// </summary>
        internal void ApplyAnimation()
        {
            // If not enabled, do not apply.
            if ((!_enabled) || (_isStopped))
                return;

            // Apply transformation tracks.
            for (int i = 0; i < _transforms.Count; i++)
            {
                IKey<KeyTransform> key = null;      // Key to apply.

                // Get interpolated key.
                key = _transforms[i][_currentTime];
                key.UpdateLayerObject(_owner);
            }

            // Apply color tracks.
            for (int i = 0; i < _colors.Count; i++)
            {
                IKey<KeyColor> key = null;      // Key to apply.

                // Get interpolated key.
                key = _colors[i][_currentTime];
                key.UpdateLayerObject(_owner);
            }

			// Apply frame tracks.
			for (int i = 0; i < _frames.Count; i++)
			{
				IKey<KeyFrame> key = null;		// Key to apply.

				// Get interpolated key.
				key = _frames[i][_currentTime];
				key.UpdateLayerObject(_owner);
			}
        }

        /// <summary>
        /// Function to advance the animation by a specific time in milliseconds.
        /// </summary>
        /// <param name="time">Time in milliseconds.</param>
        public void Advance(float time)
        {
            CurrentTime += time;
        }

        /// <summary>
        /// Function to advance the animation by the timing data provided.
        /// </summary>
        /// <param name="data">Timing data to use.</param>
        public void Advance(TimingData data)
        {
            Advance((float)data.FrameDrawTime);
        }

        /// <summary>
		/// Function to reset the animation.
		/// </summary>
		public void Reset()
        {
            _currentTime = 0;
        }

        /// <summary>
        /// Function to start the animation.
        /// </summary>
        public void Go()
        {
            if (_enabled)
            {
                _isStopped = false;

                if (AnimationStarted != null)
                    AnimationStarted(this, EventArgs.Empty);
            }            
        }

        /// <summary>
        /// Function to stop an animation.
        /// </summary>
        public void Stop()
        {

            _isStopped = true;

            if (AnimationStopped != null)
                AnimationStopped(this, EventArgs.Empty);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the animation.</param>
        /// <param name="owner">Owner of this animation.</param>
        /// <param name="length">Length of the animation in milliseconds.</param>
        internal Animation(string name, IAnimatable owner, float length)
            : base(name)
        {
            _owner = owner;
            _transforms = new TrackTransformList(this);
            _colors = new TrackColorList(this);
			_frames = new TrackFrameList(this);
            _enabled = true;
            _isStopped = false;
            Length = length;
        }
        #endregion

        #region ISerializable Members
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		/// <value></value>
		string ISerializable.Filename
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}
	
		/// <summary>
        /// Function to persist the data into the serializer stream.
        /// </summary>
        /// <param name="serializer">Serializer that's calling this function.</param>
        void ISerializable.WriteData(ISerializer serializer)
        {
            serializer.WriteGroupBegin("Animation");

            // Write animation data.
            serializer.Write<string>("Name", _objectName);
            serializer.Write<float>("Length", _length);
            serializer.Write<bool>("Looping", _loop);
            serializer.Write<bool>("Enabled", _enabled);
            serializer.Write<int>("TransformTracks", _transforms.Count);

            if (_transforms.Count > 0)
            {
                // Write the transformation tracks.
                foreach (TrackTransform track in _transforms)
                {
                    serializer.WriteGroupBegin("Track");
                    serializer.Write<string>("Name", track.Name);                    
                    serializer.Write<int>("KeyCount", track.KeyCount);

                    // Write keys.
                    if (track.KeyCount > 0)
                    {
                        KeyTransform key = null;        // Transformation key. 

                        for (int i = 0; i < track.KeyCount; i++)
                        {
                            serializer.WriteGroupBegin("Key");
                            key = track.GetAssignedKey(i);
                            serializer.Write<float>("TimeIndex", key.Time);
                            serializer.Write<int>("InterpolationMode", (int)key.InterpolationMode);
                            serializer.Write<float>("PositionX", key.Position.X);
                            serializer.Write<float>("PositionY", key.Position.Y);
                            serializer.Write<float>("ScaleX", key.Scale.X);
                            serializer.Write<float>("ScaleY", key.Scale.Y);
                            serializer.Write<float>("Rotation", key.Rotation);
                            serializer.Write<float>("AxisX", key.Axis.X);
                            serializer.Write<float>("AxisY", key.Axis.Y);
                            serializer.Write<float>("SizeX", key.Size.X);
                            serializer.Write<float>("SizeY", key.Size.Y);
                            serializer.Write<float>("OffsetX", key.ImageOffset.X);
                            serializer.Write<float>("OffsetY", key.ImageOffset.Y);
                            serializer.WriteGroupEnd();
                        }
                    }
                    serializer.WriteGroupEnd();
                }                
            }

            serializer.Write<int>("ColorTracks", _colors.Count);
            if (_colors.Count > 0)
            {
                // Write the transformation tracks.
                foreach (TrackColor track in _colors)
                {
                    serializer.WriteGroupBegin("Track");
                    serializer.Write<string>("Name", track.Name);                    
                    serializer.Write<int>("KeyCount", track.KeyCount);

                    // Write keys.
                    if (track.KeyCount > 0)
                    {
                        KeyColor key = null;        // Color key. 

                        for (int i = 0; i < track.KeyCount; i++)
                        {
                            serializer.WriteGroupBegin("Key");
                            key = track.GetAssignedKey(i);
                            serializer.Write<float>("TimeIndex", key.Time);
                            serializer.Write<int>("InterpolationMode", (int)key.InterpolationMode);
                            serializer.Write<int>("Color", key.Color.ToArgb());
                            serializer.Write<int>("AlphaMaskValue", key.AlphaMaskValue);
                            serializer.WriteGroupEnd();
                        }
                    }
                    serializer.WriteGroupEnd();
                }                
            }

            serializer.Write<int>("FrameTracks", _frames.Count);
            if (_frames.Count > 0)
            {
                // Write the transformation tracks.
                foreach (TrackFrame track in _frames)
                {
                    serializer.WriteGroupBegin("Track");
                    serializer.Write<string>("Name", track.Name);                    
                    serializer.Write<int>("KeyCount", track.KeyCount);

                    // Write keys.
                    if (track.KeyCount > 0)
                    {
                        KeyFrame key = null;        // Frame switch key. 

                        for (int i = 0; i < track.KeyCount; i++)
                        {
                            serializer.WriteGroupBegin("Key");
                            key = track.GetAssignedKey(i);
                            serializer.Write<float>("TimeIndex", key.Time);
                            serializer.Write<int>("InterpolationMode", (int)key.InterpolationMode);
                            serializer.Write<string>("ImageName", key.Frame.Image.Name);
                            serializer.Write<float>("ImageOffsetX", key.Frame.Offset.X);
                            serializer.Write<float>("ImageOffsetY", key.Frame.Offset.Y);
                            serializer.Write<float>("SizeX", key.Frame.Size.X);
                            serializer.Write<float>("SizeY", key.Frame.Size.Y);
                            serializer.WriteGroupEnd();
                        }
                    }
                    serializer.WriteGroupEnd();
                }                
            }

            serializer.WriteGroupEnd();
        }

        /// <summary>
        /// Function to retrieve data from the serializer stream.
        /// </summary>
        /// <param name="serializer">Serializer that's calling this function.</param>
        void ISerializable.ReadData(ISerializer serializer)
        {
            // Write animation data.
            _objectName = serializer.Read<string>("Name");
            _length = serializer.Read<float>("Length");
            _loop = serializer.Read<bool>("Looping");
            _enabled = serializer.Read<bool>("Enabled");

            // Track count.
            int trackCount = 0;
            string trackName = string.Empty;
            int keyCount = 0;

            trackCount = serializer.Read<int>("TransformTracks");            
            if (trackCount > 0)
            {
                // Write the transformation tracks.
                for (int i = 0; i < trackCount; i++)
                {
                    TrackTransform track = null;        // Transform track.

                    trackName = serializer.Read<string>("Name");        
                    keyCount = serializer.Read<int>("KeyCount");
                    
                    track = _transforms.Create(trackName);
                                        
                    // Write keys.
                    if (keyCount > 0)
                    {
                        KeyTransform key = null;        // Transformation key. 

                        for (int j = 0; j < keyCount; j++)
                        {
                            // Create a key.
                            key = new KeyTransform(null, 0.0f);                                                        

                            key.Time = serializer.Read<float>("TimeIndex");
                            key.InterpolationMode = (InterpolationMode)serializer.Read<int>("InterpolationMode");
                            key.Position = new Vector2D(serializer.Read<float>("PositionX"), serializer.Read<float>("PositionY"));
                            key.Scale = new Vector2D(serializer.Read<float>("ScaleX"), serializer.Read<float>("ScaleY"));
                            key.Rotation = serializer.Read<float>("Rotation");
                            key.Axis = new Vector2D(serializer.Read<float>("AxisX"), serializer.Read<float>("AxisY"));
                            key.Size = new Vector2D(serializer.Read<float>("SizeX"), serializer.Read<float>("SizeY"));
                            key.ImageOffset = new Vector2D(serializer.Read<float>("OffsetX"), serializer.Read<float>("OffsetY"));

                            track.AddKey(key);
                        }
                    }
                }
            }

            trackCount = serializer.Read<int>("ColorTracks");
            if (trackCount > 0)
            {
                // Write the color tracks.
                for (int i = 0; i < trackCount; i++)
                {
                    TrackColor track = null;        // Color track.

                    trackName = serializer.Read<string>("Name");
                    keyCount = serializer.Read<int>("KeyCount");

                    track = _colors.Create(trackName);

                    // Write keys.
                    if (keyCount > 0)
                    {
                        KeyColor key = null;        // Color key. 

                        for (int j = 0; j < keyCount; j++)
                        {
                            // Create a key.
                            key = new KeyColor(null, 0.0f);                            

                            key.Time = serializer.Read<float>("TimeIndex");
                            key.InterpolationMode = (InterpolationMode)serializer.Read<int>("InterpolationMode");
                            key.Color = Color.FromArgb(serializer.Read<int>("Color"));
                            key.AlphaMaskValue = serializer.Read<int>("AlphaMaskValue");

                            track.AddKey(key);
                        }
                    }
                }
            }

            trackCount = serializer.Read<int>("FrameTracks");
            if (trackCount > 0)
            {
                // Write the transformation tracks.
                for (int i = 0; i < trackCount; i++)
                {
                    TrackFrame track = null;    // Frame switch track.

                    trackName = serializer.Read<string>("Name");
                    keyCount = serializer.Read<int>("KeyCount");

                    track = _frames.Create(trackName);

                    // Write keys.
                    if (keyCount > 0)
                    {
                        KeyFrame key = null;        // Frame switch key. 

                        for (int j = 0; j < keyCount; j++)
                        {
                            Image imageFrame = null;            // Image used for frame.
                            string imageName = string.Empty;    // Image name.

                            // Create key.
                            key = new KeyFrame(null, 0.0f);
                            key.Time = serializer.Read<float>("TimeIndex");
                            key.InterpolationMode = (InterpolationMode)serializer.Read<int>("InterpolationMode");

                            // Get the current image frame.
                            imageFrame = Owner.Image;
                            imageName = serializer.Read<string>("ImageName");

                            if (((imageFrame != null) && (imageName != imageFrame.Name) && (Gorgon.ImageManager.Contains(imageName))) || ((imageFrame == null) && (Gorgon.ImageManager.Contains(imageName))))
                                imageFrame = Gorgon.ImageManager[imageName];

                            // No image?  No keyframe.
                            if (imageFrame != null)
                            {
                                key.Frame = new Frame(imageFrame, new Vector2D(serializer.Read<float>("ImageOffsetX"), serializer.Read<float>("ImageOffsetY")),
                                    new Vector2D(serializer.Read<float>("SizeX"), serializer.Read<float>("SizeY")));

                                track.AddKey(key);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
