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
        #region Variables.
        private float _frameDelta = 0.0f;           // Delta between last time and current time.

        /// <summary>Current animation time.</summary>
        public float CurrentFrameTime;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the time delta between this and the last frame.
        /// </summary>
        public float Delta
        {
            get
            {
                return _frameDelta;
            }
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
            _frameDelta = delta;
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
