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
// Created: Tuesday, November 21, 2006 12:08:22 AM
// 
#endregion

using System;
using System.Drawing;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a frame switch key.
	/// </summary>
	public class KeyFrame
		: Key
	{
		#region Variables.
		private Frame _frame;								// Frame for the animation.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the interpolation mode.
		/// </summary>
		public override InterpolationMode InterpolationMode
		{
			get
			{
				return InterpolationMode.None;
			}
			set
			{				
			}
		}

        /// <summary>
        /// Property to set or return the frame.
        /// </summary>
        public Frame Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                if (Owner != null)
					Owner.NeedsUpdate = true;
            }
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		protected internal override void Apply(int prevKeyIndex, Key previousKey, Key nextKey)
		{
			// Not available.
		}

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		protected internal override void UpdateLayerObject(Renderable layerObject)
		{
			if (layerObject.Image != _frame.Image)
				layerObject.Image = _frame.Image;
			if (layerObject.ImageOffset != _frame.Offset)
				layerObject.ImageOffset = _frame.Offset;
			if (layerObject.Size != _frame.Size)
				layerObject.Size = _frame.Size;
		}

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		public override Key Clone()
		{
			KeyFrame newKey = null;         // Cloned key.

			newKey = new KeyFrame(null, Time);
			newKey.InterpolationMode = InterpolationMode;
			newKey.Frame = _frame;

			return newKey;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owner of this key.</param>
		/// <param name="timePosition">Position in time for this keyframe.</param>
		public KeyFrame(TrackFrame owner, float timePosition)
			: base(owner, timePosition)
		{
			InterpolationMode = InterpolationMode.None;

			// Get the size from the animation owner.
			if (Owner != null)
				_frame = new Frame(Owner.Owner.Owner.Image, Owner.Owner.Owner.ImageOffset, Owner.Owner.Owner.Size);
		}
		#endregion
	}
}
