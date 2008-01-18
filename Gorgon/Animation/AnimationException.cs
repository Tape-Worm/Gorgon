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
// Created: Monday, December 08, 2006 08:02:13 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// No keys found in the track.
    /// </summary>
    public class AnimationTrackHasNoKeysException
        : GorgonException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ex">Exception source.</param>
        public AnimationTrackHasNoKeysException(Exception ex)
			: base("There are no keys in this track.", ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
		public AnimationTrackHasNoKeysException()
            : this(null)
        {
        }
        #endregion
    }

    /// <summary>
    /// Key already assigned exception.
    /// </summary>
    public class AnimationKeyAssignedException
        : GorgonException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type of key.</param>
        /// <param name="ex">Exception source.</param>
        public AnimationKeyAssignedException(Type type, Exception ex)
            : base("Key " + type.Name + " has already been assigned to a track.", ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type of key.</param>
        public AnimationKeyAssignedException(Type type)
            : this(type, null)
        {
        }
        #endregion
    }

	/// <summary>
	/// Animation key not found.
	/// </summary>
	public class AnimationKeyNotFoundException
		: GorgonException
	{
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="time">Time position of the key.</param>
        /// <param name="ex">Exception source.</param>
        public AnimationKeyNotFoundException(string time, Exception ex)
            : base("There is no key at time index " + time + ".", ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="time">Time position of the key.</param>
		public AnimationKeyNotFoundException(string time)
            : this(time, null)
        {
        }
        #endregion
	}

	/// <summary>
	/// Track is not the expected type.
	/// </summary>
	public class AnimationTypeMismatchException
		: GorgonException
	{
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
		/// <param name="itemType">Type of object that threw the exception.</param>
        /// <param name="trackName">Name of the track that is mismatched.</param>
		/// <param name="expectedType">Type that we expected.</param>
		/// <param name="currentType">Type that we actually got.</param>
        /// <param name="ex">Exception source.</param>
		public AnimationTypeMismatchException(string itemType, string trackName, string expectedType, string currentType, Exception ex)
            : base("The " + itemType + " '" + trackName + "' is not a valid type.\nExpected '" + expectedType + "', got '" + currentType + "'.", ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
		/// <param name="itemType">Type of object that threw the exception.</param>
		/// <param name="trackName">Name of the track that is mismatched.</param>
		/// <param name="expectedType">Type that we expected.</param>
		/// <param name="currentType">Type that we actually got.</param>
		public AnimationTypeMismatchException(string itemType, string trackName, string expectedType, string currentType)
            : this(itemType, trackName, expectedType, currentType, null)
        {
        }
        #endregion
	}
}
