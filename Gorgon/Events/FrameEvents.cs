#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Monday, July 11, 2005 11:52:31 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Event delegate for frame events.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void FrameEventHandler(object sender, FrameEventArgs e);

	/// <summary>
	/// Arguments for the frame begin/end events.
	/// </summary>
	public class FrameEventArgs : EventArgs
	{
		#region Variables.
		private Viewport _currentView = null;				// Currently active view port.
		private RenderTarget _currentTarget = null;			// Currently active render target.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the currently active viewport.
		/// </summary>
		public Viewport CurrentView
		{
			get
			{
				return _currentView;
			}
		}

		/// <summary>
		/// Property to return the currently active render target.
		/// </summary>
		public RenderTarget CurrentTarget
		{
			get
			{
				return _currentTarget;
			}
		}
		#endregion

        #region Methods.
		/// <summary>
		/// Function to set the currently active viewport and render target.
		/// </summary>
		/// <param name="activeView">Active viewport.</param>
		/// <param name="activeTarget">Active render target.</param>
		internal void SetTargets(Viewport activeView, RenderTarget activeTarget)
		{
			_currentView = activeView;
			_currentTarget = activeTarget;
		}
        #endregion

        #region Constructor.
        /// <summary>
		/// Constructor.
		/// </summary>
		public FrameEventArgs()
		{
		}
		#endregion
	}
}
