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
// Created: Monday, November 20, 2006 1:35:28 AM
// 
#endregion

using System;
using System.Collections.Generic;
using SharpUtilities.Collections;

namespace GorgonLibrary.Graphics.Animations
{
    /// <summary>
    /// Object representing a list of tracks for an animation.
    /// </summary>
    /// <typeparam name="T">Type of key for the track.</typeparam>
    public abstract class TrackList<T>
        : Collection<T>
		where T : class
    {
        #region Variables.
        private Animation _owner = null;            // Owning animation object.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the owner of the track list.
        /// </summary>
        public Animation Owner
        {
            get
            {
                return _owner;
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to create a track.
		/// </summary>
		/// <param name="name">Name of the track.</param>
		/// <returns>A new track.</returns>
		public abstract T Create(string name);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owning animation object.</param>
        internal TrackList(Animation owner)
        {
            _owner = owner;            
        }
        #endregion
    }
}
