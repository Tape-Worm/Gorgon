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
using System.ComponentModel;
using System.Reflection;
using GorgonLibrary.Internal;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Animation states.
	/// </summary>
	/// <remarks>
	/// These states describe if an animation can have its time advanced or not.  
	/// </remarks>
	public enum AnimationState
	{
		/// <summary>Animation is currently in a play state and is ready to have its time position advanced.</summary>
		Playing = 0,
		/// <summary>Animation is currently in a stopped state and changes to its time position will be ignored.</summary>
		Stopped = 1
	}

	/// <summary>
	/// An animation object will contain animation data that can be applied to a animation capable object.
	/// </summary>
	/// <remarks>
	/// Animations can be used to move a sprite, change its color over time, or even flip between individual frames.
	/// <para>
	/// An animation is composed of several tracks, which represent a specific type of animation.  Currently there are 3 tracks:
	/// <list type="table">
	/// 		<listheader>
	/// 			<term>Type</term>
	/// 			<description> Description</description>
	/// 		</listheader>
	/// 		<item>
	/// 			<term>Color</term>
	/// 			<description> Used to animate the color and alpha related properties of an animated object.</description>
	/// 		</item>
	/// 		<item>
	/// 			<term>Frame</term>
	/// 			<description> Used to flip between frames of animation.</description>
	/// 		</item>
	/// 		<item>
	/// 			<term>Transformation</term>
	/// 			<description> Used to apply move, scale or rotate transformations to the animated object.</description>
	/// 		</item>
	/// 	</list>
	/// </para>
	/// </remarks>
    public class Animation
        : NamedObject, ISerializable, ICloneable<Animation>
    {
        #region Variables.
        private object _owner = null;								// Object that owns this animation.        
        private float _length = 1000;								// Length of the track in milliseconds.
        private bool _loop;											// Flag to indicate that this animation should loop.
        private float _currentTime;									// Current time.
        private bool _enabled;										// Flag to indicate whether the animation is enabled or not.
		private AnimationState _state = AnimationState.Playing;		// State of action for the animation.
		private TrackTransform _transforms = null;					// Transformation track.
		private TrackColor _colors = null;							// Color track.
		private TrackFrame _frames = null;							// Frame track.
		private TrackCollection _tracks = null;						// Tracks.
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
		/// Property to set or return the name.
		/// </summary>
		public new string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
                base.SetName(value);
			}
		}

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
					AnimationState = AnimationState.Stopped;

                _enabled = value;
            }
        }

        /// <summary>
        /// Property to return the owner of this animation.
        /// </summary>
        public object Owner
        {
            get
            {
                return _owner;
            }
        }

        /// <summary>
        /// Property to return the transformation track for the animation.
        /// </summary>
        public TrackTransform TransformationTrack
        {
            get
            {
                return _transforms;
            }
        }

        /// <summary>
        /// Property to return the color track for the animation.
        /// </summary>
        public TrackColor ColorTrack
        {
            get
            {
                return _colors;
            }
        }

		/// <summary>
		/// Property to return the frame track for the animation.
		/// </summary>
		public TrackFrame FrameTrack
		{
			get
			{
				return _frames;
			}
		}

		/// <summary>
		/// Property to return the collection of tracks for this animation.
		/// </summary>
		/// <remarks>Each track corresponds to an animation property on the owner object.<para>Custom tracks can be added to this collection as well.</para></remarks>
		public TrackCollection Tracks
		{
			get
			{
				return _tracks;
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
        /// Property to set or return the state of the animation.
        /// </summary>
        public AnimationState AnimationState
        {
            get
            {
                return _state;
            }
			set
			{
				_state = value;

				if (value == AnimationState.Playing)
				{
					if (_enabled)
					{
						if (AnimationStarted != null)
							AnimationStarted(this, EventArgs.Empty);
					}
					else
						_state = AnimationState.Stopped;
				}
				else
				{
					if (AnimationStopped != null)
						AnimationStopped(this, EventArgs.Empty);
				}
			}
        }

		/// <summary>
		/// Property to return the total number of keys in the animation.
		/// </summary>
		public int TotalKeyCount
		{
			get
			{
				return _transforms.KeyCount + _colors.KeyCount + _frames.KeyCount;
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
                if (_state == AnimationState.Stopped)
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
							AnimationState = AnimationState.Stopped;
                    }
                    if (_currentTime >= _length)
                    {
                        _currentTime = _length;
						AnimationState = AnimationState.Stopped;
                    }
                }                
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to set the owner for this animation.
		/// </summary>
		/// <param name="owner">Owner for the animation.</param>
		protected internal void SetOwner(object owner)
		{
			Attribute[] attributes = null;						// List of attributes for the target object.
			PropertyInfo[] properties = null;					// List of properties on the renderable.

			if (owner == null)
				throw new ArgumentNullException(owner);

			properties = owner.GetType().GetProperties();

			if (properties.Length == 0)
				throw new AnimationOwnerInvalidException(owner.Name);

			_tracks.Clear();

			// Find all the properties that are animated and make tracks for those properties.
			foreach (PropertyInfo property in properties)
			{
				attributes = property.GetCustomAttributes(typeof(AnimatedAttribute), true);

				foreach (AnimatedAttribute attribute in attributes)
				{
					switch (attribute.DataType.Name.ToLower())
					{
						case "vector2d":
							_tracks.Add(new TrackVector2D(this, property));
							break;
						default:
							// If we don't know the type that's being added, then don't bother.
							break;
					}
				}
			}

			if (_tracks.Count == 0)
				throw new AnimationOwnerInvalidException(owner.Name);
			
			_owner = owner;
		}

        /// <summary>
        /// Function to apply the animation to this owning object.
        /// </summary>
        internal void ApplyAnimation()
        {
            // If not enabled, do not apply.
			if ((!_enabled) || (_state == AnimationState.Stopped))
                return;

			// Apply transformation tracks.
			if (_transforms.KeyCount > 0)
			{
				Key key = null;      // Key to apply.

				// Get interpolated key.
				key = _transforms[_currentTime];
				key.UpdateLayerObject(_owner);
			}

			// Apply color tracks.
			if (_colors.KeyCount > 0)
			{
				Key key = null;      // Key to apply.

				// Get interpolated key.
				key = _colors[_currentTime];
				key.UpdateLayerObject(_owner);
			}

			// Apply frame tracks.
			if (_frames.KeyCount > 0)
			{
				Key key = null;		// Key to apply.

				// Get interpolated key.
				key = _frames[_currentTime];
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

			// Apply transformation tracks.
			if (_transforms.KeyCount > 0)
			{
				Key key = null;      // Key to apply.

				// Get interpolated key.
				key = _transforms[0];
				key.UpdateLayerObject(_owner);
			}

			// Apply color tracks.
			if (_colors.KeyCount > 0)
			{
				Key key = null;      // Key to apply.

				// Get interpolated key.
				key = _colors[0];
				key.UpdateLayerObject(_owner);
			}

			// Apply frame tracks.
			if (_frames.KeyCount > 0)
			{
				Key key = null;		// Key to apply.

				// Get interpolated key.
				key = _frames[0];
				key.UpdateLayerObject(_owner);
			}
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the animation.</param>
        /// <param name="length">Length of the animation in milliseconds.</param>
        protected Animation(string name, float length)
            : base(name)
        {
			_owner = null;
			_tracks = new TrackCollection();
			_transforms = new TrackTransform(this);
			_colors = new TrackColor(this);
			_frames = new TrackFrame(this);
            _enabled = true;
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
        void ISerializable.WriteData(Serializer serializer)
        {
            serializer.WriteGroupBegin("Animation");

            // Write animation data.
            serializer.Write("Name", Name);
            serializer.Write("Length", _length);
            serializer.Write("Looping", _loop);
            serializer.Write("Enabled", _enabled);
            serializer.Write("TransformKeyCount", _transforms.KeyCount);

			// Write keys.
			if (_transforms.KeyCount > 0)
			{
				serializer.WriteGroupBegin("TransformationTrack");
				KeyTransform key = null;        // Transformation key. 

				for (int i = 0; i < _transforms.KeyCount; i++)
				{
					serializer.WriteGroupBegin("Key");
					key = _transforms.GetKeyAtIndex(i) as KeyTransform;
					serializer.Write("TimeIndex", key.Time);
					serializer.Write("InterpolationMode", (int)key.InterpolationMode);
					serializer.Write("PositionX", key.Position.X);
					serializer.Write("PositionY", key.Position.Y);
					serializer.Write("ScaleX", key.Scale.X);
					serializer.Write("ScaleY", key.Scale.Y);
					serializer.Write("Rotation", key.Rotation);
					serializer.Write("AxisX", key.Axis.X);
					serializer.Write("AxisY", key.Axis.Y);
					serializer.Write("SizeX", key.Size.X);
					serializer.Write("SizeY", key.Size.Y);
					serializer.Write("OffsetX", key.ImageOffset.X);
					serializer.Write("OffsetY", key.ImageOffset.Y);
					serializer.WriteGroupEnd();
				}

				serializer.WriteGroupEnd();
			}			

			serializer.Write("ColorKeyCount", _colors.KeyCount);
			// Write keys.
			if (_colors.KeyCount > 0)
			{
				serializer.WriteGroupBegin("ColorTrack");
				KeyColor key = null;        // Color key. 

				for (int i = 0; i < _colors.KeyCount; i++)
				{
					serializer.WriteGroupBegin("Key");
					key = _colors.GetKeyAtIndex(i) as KeyColor;
					serializer.Write("TimeIndex", key.Time);
					serializer.Write("InterpolationMode", (int)key.InterpolationMode);
					serializer.Write("Color", key.Color.ToArgb());
					serializer.Write("AlphaMaskValue", key.AlphaMaskValue);
					serializer.WriteGroupEnd();
				}

				serializer.WriteGroupEnd();
			}			

			serializer.Write("FrameKeyCount", _frames.KeyCount);
			// Write keys.
			if (_frames.KeyCount > 0)
			{
				serializer.WriteGroupBegin("FrameTrack");
				KeyFrame key = null;        // Frame switch key. 

				for (int i = 0; i < _frames.KeyCount; i++)
				{
					serializer.WriteGroupBegin("Key");
					key = _frames.GetKeyAtIndex(i) as KeyFrame;
					serializer.Write("TimeIndex", key.Time);
					serializer.Write("InterpolationMode", (int)key.InterpolationMode);
					serializer.Write("ImageName", key.Frame.Image.Name);
					serializer.Write("ImageOffsetX", key.Frame.Offset.X);
					serializer.Write("ImageOffsetY", key.Frame.Offset.Y);
					serializer.Write("SizeX", key.Frame.Size.X);
					serializer.Write("SizeY", key.Frame.Size.Y);
					serializer.WriteGroupEnd();
				}

				serializer.WriteGroupEnd();
			}			
            serializer.WriteGroupEnd();
        }

        /// <summary>
        /// Function to retrieve data from the serializer stream.
        /// </summary>
        /// <param name="serializer">Serializer that's calling this function.</param>
        void ISerializable.ReadData(Serializer serializer)
        {
            // Write animation data.
            Name = serializer.ReadString("Name");
            _length = serializer.ReadSingle("Length");
            _loop = serializer.ReadBool("Looping");
            _enabled = serializer.ReadBool("Enabled");

            // Track count.
            int keyCount = 0;

			// Remove keys.
			_transforms.ClearKeys();
			_colors.ClearKeys();
			_frames.ClearKeys();

			keyCount = serializer.ReadInt32("TransformKeyCount");

            // Write keys.
            if (keyCount > 0)
            {
                KeyTransform key = null;        // Transformation key. 

                for (int j = 0; j < keyCount; j++)
                {
                    // Create a key.
                    key = new KeyTransform(null, 0.0f);                                                        

                    key.Time = serializer.ReadSingle("TimeIndex");
                    key.InterpolationMode = (InterpolationMode)serializer.ReadInt32("InterpolationMode");
                    key.Position = new Vector2D(serializer.ReadSingle("PositionX"), serializer.ReadSingle("PositionY"));
                    key.Scale = new Vector2D(serializer.ReadSingle("ScaleX"), serializer.ReadSingle("ScaleY"));
                    key.Rotation = serializer.ReadSingle("Rotation");
                    key.Axis = new Vector2D(serializer.ReadSingle("AxisX"), serializer.ReadSingle("AxisY"));
                    key.Size = new Vector2D(serializer.ReadSingle("SizeX"), serializer.ReadSingle("SizeY"));
                    key.ImageOffset = new Vector2D(serializer.ReadSingle("OffsetX"), serializer.ReadSingle("OffsetY"));

					_transforms.AddKey(key);
                }
            }

			keyCount = serializer.ReadInt32("ColorKeyCount");

			// Write keys.
			if (keyCount > 0)
			{
				KeyColor key = null;        // Color key. 

				for (int j = 0; j < keyCount; j++)
				{
					// Create a key.
					key = new KeyColor(null, 0.0f);                            

					key.Time = serializer.ReadSingle("TimeIndex");
					key.InterpolationMode = (InterpolationMode)serializer.ReadInt32("InterpolationMode");
					key.Color = Color.FromArgb(serializer.ReadInt32("Color"));
					key.AlphaMaskValue = serializer.ReadInt32("AlphaMaskValue");

					_colors.AddKey(key);
				}
			}

            keyCount = serializer.ReadInt32("FrameKeyCount");
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
                    key.Time = serializer.ReadSingle("TimeIndex");
                    key.InterpolationMode = (InterpolationMode)serializer.ReadInt32("InterpolationMode");

                    // Get the current image frame.
                    imageFrame = Owner.Image;
                    imageName = serializer.ReadString("ImageName");

					if (((imageFrame != null) && (imageName != imageFrame.Name) && (ImageCache.Images.Contains(imageName))) || ((imageFrame == null) && (ImageCache.Images.Contains(imageName))))
						imageFrame = ImageCache.Images[imageName];

                    // No image?  No keyframe.
                    if (imageFrame != null)
                    {
                        key.Frame = new Frame(imageFrame, new Vector2D(serializer.ReadSingle("ImageOffsetX"), serializer.ReadSingle("ImageOffsetY")),
                            new Vector2D(serializer.ReadSingle("SizeX"), serializer.ReadSingle("SizeY")));

                        _frames.AddKey(key);
                    }
                }
			}
        }
        #endregion

		#region ICloneable<T> Members
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public Animation Clone()
		{
			Animation clone = new Animation(Name, _length);

			clone.SetOwner(_owner);
			clone._currentTime = _currentTime;
			clone._enabled = _enabled;
			clone._state = _state;
			clone._loop = _loop;

			// Add transforms.
			foreach (KeyTransform key in _transforms)
				clone.TransformationTrack.AddKey(key.Clone());

			// Add colors.
			foreach (KeyColor key in _colors)
				clone.ColorTrack.AddKey(key.Clone());

			// Add frames.
			foreach (KeyFrame key in _frames)
				clone.FrameTrack.AddKey(key.Clone());

			return clone;
		}
		#endregion
	}
}
