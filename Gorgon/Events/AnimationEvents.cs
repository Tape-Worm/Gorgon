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

namespace GorgonLibrary.Graphics.Animations
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
}
