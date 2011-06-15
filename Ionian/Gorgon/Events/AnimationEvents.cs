#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Monday, December 11, 2006 1:40:41 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Event handler used when the time has been updated for an animation frame.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event parameters.</param>
    public delegate void AnimationAdvanceHandler(object sender, AnimationAdvanceEventArgs e);

    /// <summary>
    /// Event arguments for animation frame advancement.
    /// </summary>
    public class AnimationAdvanceEventArgs
        : EventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the time delta between this and the last frame.
        /// </summary>
		public float Delta
		{
			get;
			private set;
		}

		/// <summary>Current animation time.</summary>
		public float CurrentFrameTime
		{
			get;
			set;
		}
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="currentTime">Current frame time.</param>
        /// <param name="delta">Frame time delta.</param>
        public AnimationAdvanceEventArgs(float currentTime, float delta)
        {
            CurrentFrameTime = currentTime;
            Delta = delta;
        }
        #endregion
    }

	/// <summary>
	/// Event handler used when the time has been updated for an animation frame.
	/// </summary>
	/// <param name="sender">Sender of the event.</param>
	/// <param name="e">Event parameters.</param>
	/// <remarks>When a property is marked as animated, and its type is unknown to the animation system (e.g. a custom object or value type) we can still animate that property
	/// if we define the track type.  This event allows us to pass back a new instance of the track type to the animation and will allow the user to customize how to handle that
	/// particular type within an animation.</remarks>
	public delegate void AnimationTrackDefineHandler(object sender, AnimationTrackDefineEventArgs e);

	/// <summary>
	/// Event arguments for animation track definitions.
	/// </summary>
	/// <remarks>When a property is marked as animated, and its type is unknown to the animation system (e.g. a custom object or value type) we can still animate that property
	/// if we define the track type.  This event allows us to pass back a new instance of the track type to the animation and will allow the user to customize how to handle that
	/// particular type within an animation.</remarks>
	public class AnimationTrackDefineEventArgs
	{
		#region Variables.
		private Track _newTrack = null;						// Track type.
		private Type _trackDataType = null;					// Data type for the track.
		private string _propertyName = string.Empty;		// Name of the property being animated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the track type.
		/// </summary>
		public Track Track
		{
			get
			{
				return _newTrack;
			}
			set
			{
				_newTrack = value;
			}
		}

		/// <summary>
		/// Property to return the data type for the track.
		/// </summary>
		public Type DataType
		{
			get
			{
				return _trackDataType;
			}
		}

		/// <summary>
		/// Property to return the name of the animated property.
		/// </summary>
		public string PropertyName
		{
			get
			{
				return _propertyName;
			}
		}
		#endregion

		#region Constructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationTrackDefineEventArgs"/> class.
		/// </summary>
		/// <param name="propertyName">Name of the property being animated.</param>
		/// <param name="dataType">Type of the data represented by the track.</param>
		public AnimationTrackDefineEventArgs(string propertyName, Type dataType)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException("propertyName");
			if (dataType == null)
				throw new ArgumentNullException("dataType");
			_trackDataType = dataType;
			_propertyName = propertyName;
		}
		#endregion
	}
}
