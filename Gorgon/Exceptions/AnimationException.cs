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

namespace GorgonLibrary
{
    /// <summary>
    /// Animation error codes.
    /// </summary>
    public enum AnimationErrors
    {
        /// <summary>No keys found in the track.</summary>
        NoKeysFound = 0x7FFF0001,
        /// <summary>Key has already been assigned to a track.</summary>
        KeyAlreadyAssigned = 0x7FFF0002
    }

    /// <summary>
    /// Base exception for animations.
    /// </summary>
    public abstract class AnimationException
        : GorgonException
    {
        #region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Exception source.</param>
		public AnimationException(string message, AnimationErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
        #endregion
    }

    /// <summary>
    /// No keys found in track exception.
    /// </summary>
    public class NoKeysInTrackException
        : AnimationException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Exception source.</param>
        public NoKeysInTrackException(string message, Exception ex)
            : base(message, AnimationErrors.NoKeysFound, ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ex">Exception source.</param>
        public NoKeysInTrackException(Exception ex)
            : this("There are no keys in this track.", ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Key already assigned exception.
    /// </summary>
    public class KeyAlreadyAssignedException
        : AnimationException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Exception source.</param>
        public KeyAlreadyAssignedException(string message, Exception ex)
            : base(message, AnimationErrors.KeyAlreadyAssigned, ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type of key.</param>
        /// <param name="ex">Exception source.</param>
        public KeyAlreadyAssignedException(Type type, Exception ex)
            : this("Key " + type.Name + " has already been assigned to a track.", ex)
        {
        }
        #endregion
    }
}
