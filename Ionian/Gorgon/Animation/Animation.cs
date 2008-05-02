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
		private TrackCollection _tracks = null;						// Tracks.
		private int _frameRate = 30;								// Frame rate (for information purposes only).
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
		/// <summary>
		/// Event fired when the animation needs to define the type of a track.
		/// </summary>
		public event AnimationTrackDefineHandler AnimationTrackDefinition;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to set or return the frame rate.
		/// </summary>
		/// <remarks>This is just metadata and is only used for display purposes.  Its primary use is in the sprite editor.</remarks>
		public int FrameRate
		{
			get
			{
				return _frameRate;
			}
			set
			{
				if (value < 1)
					value = 1;
				_frameRate = value;
			}
		}

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
		/// Function called when the animation needs to define an unknown track type.
		/// </summary>
		/// <param name="sender">Object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnAnimationTrackDefine(object sender, AnimationTrackDefineEventArgs e)
		{
			if (AnimationTrackDefinition != null)
				AnimationTrackDefinition(sender, e);
		}

		/// <summary>
		/// Function to set the owner for this animation.
		/// </summary>
		/// <param name="owner">Owner for the animation.</param>
		protected internal void SetOwner(object owner)
		{
			Attribute[] attributes = null;						// List of attributes for the target object.
			PropertyInfo[] properties = null;					// List of properties on the renderable.
			Track track = null;									// Track to add.
			AnimationTrackDefineEventArgs e;					// Event arguments.

			if (owner == null)
				throw new ArgumentNullException("owner");

			properties = owner.GetType().GetProperties();

			if (properties.Length == 0)
				throw new AnimationOwnerInvalidException(owner.GetType().Name);

			_tracks.Clear();
			
			// Find all the properties that are animated and make tracks for those properties.
			foreach (PropertyInfo property in properties)
			{
				// Note: this does not work.  Apparently it doesn't care about inherited attributes.  This is extremely dumb.
				//attributes = property.GetCustomAttributes(typeof(AnimatedAttribute), true) as Attribute[];
				attributes = AnimatedAttribute.GetCustomAttributes(property, typeof(AnimatedAttribute), true);

				foreach (AnimatedAttribute attribute in attributes)
				{
					switch (attribute.DataType.FullName.ToLower())
					{
						case "gorgonlibrary.vector2d":
							track = new TrackVector2D(property);
							break;
						case "system.single":
							track = new TrackFloat(property);
							break;
						case "system.int32":
							track = new TrackInt32(property);
							break;
						case "system.byte":
							track = new TrackByte(property);
							break;
						case "system.drawing.color":
							track = new TrackColor(property);
							break;
						case "gorgonlibrary.graphics.image":
							track = new TrackImage(property);
							break;
						default:
							e = new AnimationTrackDefineEventArgs(property.Name, attribute.DataType);
							OnAnimationTrackDefine(this, e);
							track = e.Track;
							if (track != null)
								track.BoundProperty = property;
							break;
					}

					if (track != null)
					{
						track.SetAnimationOwner(this);
						track.InterpolationMode = attribute.InterpolationMode;
						_tracks.Add(track);
					}
				}
			}

			if (_tracks.Count == 0)
				throw new AnimationOwnerInvalidException(owner.GetType().Name);
			
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

			foreach (Track track in _tracks)
			{
				if (track.KeyCount > 0)
					track[_currentTime].Update();
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

			foreach (Track track in _tracks)
			{
				if (track.KeyCount > 0)
					track[0].Update();
			}
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the animation.</param>
        /// <param name="length">Length of the animation in milliseconds.</param>
        public Animation(string name, float length)
            : base(name)
        {
			_owner = null;
			_tracks = new TrackCollection();
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
		/// Function to read in the old version 1 file format.
		/// </summary>
		/// <param name="serializer">Serializer to use to read the format.</param>
		internal void ReadVersion1Animation(Serializer serializer)
		{
			float keyTime = 0.0f;						// Key time.
			Vector2D vector = Vector2D.Zero;			// Key vector.
			float floatValue = 0;						// Key floating point value.
			int intValue = 0;							// Key integer value.

            // Write animation data.
            Name = serializer.ReadString("Name");
            _length = serializer.ReadSingle("Length");
            _loop = serializer.ReadBool("Looping");
            _enabled = serializer.ReadBool("Enabled");

            // Track count.
            int keyCount = 0;

			keyCount = serializer.ReadInt32("TransformKeyCount");

            // Write keys.
            if (keyCount > 0)
            {
                for (int j = 0; j < keyCount; j++)
                {
                    // Create a key.
					keyTime = serializer.ReadSingle("TimeIndex");
					serializer.ReadInt32("InterpolationMode");		// Skip this, it's handled by the track now - note that animations may not react properly.
                    vector = new Vector2D(serializer.ReadSingle("PositionX"), serializer.ReadSingle("PositionY"));

					if (_tracks.Contains("Position"))
						_tracks["Position"].AddKey(new KeyVector2D(keyTime, vector));

					vector = new Vector2D(serializer.ReadSingle("ScaleX"), serializer.ReadSingle("ScaleY"));

					if (_tracks.Contains("Scale"))
						_tracks["Scale"].AddKey(new KeyVector2D(keyTime, vector));

					floatValue = serializer.ReadSingle("Rotation");

					if (_tracks.Contains("Rotation"))
						_tracks["Rotation"].AddKey(new KeyFloat(keyTime, floatValue));

					vector = new Vector2D(serializer.ReadSingle("AxisX"), serializer.ReadSingle("AxisY"));

					if (_tracks.Contains("Axis"))
						_tracks["Axis"].AddKey(new KeyVector2D(keyTime, vector));

					vector = new Vector2D(serializer.ReadSingle("SizeX"), serializer.ReadSingle("SizeY"));
					
					if (_tracks.Contains("Size"))
						_tracks["Size"].AddKey(new KeyVector2D(keyTime, vector));
					
					vector = new Vector2D(serializer.ReadSingle("OffsetX"), serializer.ReadSingle("OffsetY"));

					if (_tracks.Contains("ImageOffset"))
						_tracks["ImageOffset"].AddKey(new KeyVector2D(keyTime, vector));					
                }
            }

			keyCount = serializer.ReadInt32("ColorKeyCount");

			// Write keys.
			if (keyCount > 0)
			{
				for (int j = 0; j < keyCount; j++)
				{
					keyTime = serializer.ReadSingle("TimeIndex");
					serializer.ReadInt32("InterpolationMode");

					intValue = serializer.ReadInt32("Color");
					if (_tracks.Contains("Color"))
						_tracks["Color"].AddKey(new KeyColor(keyTime, Color.FromArgb(intValue)));
					intValue = serializer.ReadInt32("AlphaMaskValue");
					if (_tracks.Contains("AlphaMaskValue"))
						_tracks["AlphaMaskValue"].AddKey(new KeyInt32(keyTime, intValue));
				}
			}

            keyCount = serializer.ReadInt32("FrameKeyCount");
            // Write keys.
            if (keyCount > 0)
            {
                for (int j = 0; j < keyCount; j++)
                {
					KeyImage newKey = null;				// Key image.
					string imageName = string.Empty;    // Image name.

					keyTime = serializer.ReadSingle("TimeIndex");
					serializer.ReadInt32("InterpolationMode");

					imageName = serializer.ReadString("ImageName");

					if ((_tracks.Contains("Image")) && (!string.IsNullOrEmpty(imageName)) && (ImageCache.Images.Contains(imageName)))
					{
						newKey = new KeyImage(keyTime, ImageCache.Images[imageName]);
						newKey.ImageOffset = new Vector2D(serializer.ReadSingle("ImageOffsetX"), serializer.ReadSingle("ImageOffsetY"));
						newKey.ImageSize = new Vector2D(serializer.ReadSingle("SizeX"), serializer.ReadSingle("SizeY"));
						_tracks["Image"].AddKey(newKey);
					}
                }
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
			serializer.Write("Header", "GORANM11");		// Stupid choice on my part, I should have versioned the animations.
            serializer.Write("Name", Name);
            serializer.Write("Length", _length);
            serializer.Write("Looping", _loop);
            serializer.Write("Enabled", _enabled);
			serializer.Write("FPS", _frameRate);
			serializer.Write("TrackCount", _tracks.Count);

			// Write tracks.
			foreach (Track track in _tracks)
			{
				serializer.WriteGroupBegin("Track");
				serializer.Write("Name", track.Name);
				serializer.Write("KeyCount", track.KeyCount);
				if (track.KeyCount > 0)
				{	
					serializer.Write("InterpolationMode", (int)track.InterpolationMode);					

					// Write keys for the track.
					for (int i = 0; i < track.KeyCount;i++)
						track.GetKeyAtIndex(i).WriteData(serializer);		// Keyframe to write.					
				}
				serializer.WriteGroupEnd();
			}
        }

        /// <summary>
        /// Function to retrieve data from the serializer stream.
        /// </summary>
        /// <param name="serializer">Serializer that's calling this function.</param>
        void ISerializable.ReadData(Serializer serializer)
        {
			string header = string.Empty;			// Header.
			string trackName = string.Empty;		// Name of the track.
			int keyCount = 0;						// Number of keys for the track.
			int trackCount = 0;						// Number of tracks.
			KeyFrame newKey = null;					// Key frame.

			header = serializer.ReadString("Header");
			if (string.Compare(header, "GORANM11", true) != 0)
				throw new InvalidOperationException("The animation data is not in a known format.");
            Name = serializer.ReadString("Name");
            _length = serializer.ReadSingle("Length");
            _loop = serializer.ReadBool("Looping");
            _enabled = serializer.ReadBool("Enabled");
			_frameRate = serializer.ReadInt32("FPS");
			trackCount = serializer.ReadInt32("TrackCount");

			// Loop through existing tracks.
			for (int i = 0; i < trackCount; i++)
			{
				trackName = serializer.ReadString("Name");
				keyCount = serializer.ReadInt32("KeyCount");

				// If the animation contains a track by this name, load its data.
				if ((_tracks.Contains(trackName)) && (keyCount > 0))
				{
					_tracks[trackName].InterpolationMode = (InterpolationMode)serializer.ReadInt32("InterpolationMode");					
					for (int j = 0; j < keyCount; j++)
					{
						// Get the new key.
						newKey = _tracks[trackName].CreateKey();
						newKey.ReadData(serializer);
						_tracks[trackName].AddKey(newKey);
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
			foreach (Track track in _tracks)
			{
				if (clone.Tracks.Contains(track.Name))
				{
					foreach (KeyFrame key in track)
						clone.Tracks[track.Name].AddKey(key.Clone());
				}
			}

			return clone;
		}
		#endregion
	}
}
