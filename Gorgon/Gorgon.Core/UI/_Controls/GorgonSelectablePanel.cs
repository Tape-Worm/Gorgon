#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, October 29, 2013 10:06:31 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core.Properties;
using Gorgon.Design;
using Gorgon.Native;

namespace Gorgon.UI
{
	/// <summary>
	/// A <see cref="Panel"/> that can receive keyboard focus.
	/// </summary>
	[ToolboxItem(true), ToolboxBitmap(typeof(GorgonApplication), "Resources.GorgonSelectablePanel.bmp")]
	public class GorgonSelectablePanel
		: Panel
	{
		#region Variables.
		// Show focus flag.
		private bool _showFocus = true;
		// Flag to indicate that the panel is resizing.
		private bool _resizing;	
		#endregion

		#region Properties.
		/// <summary>
		/// Gets or sets a value indicating whether the user can give the focus to this control using the TAB key.
		/// </summary>
		/// <returns>true if the user can give the focus to the control using the TAB key; otherwise, false. The default is false.</returns>
		///   <PermissionSet>
		///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
		///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   </PermissionSet>
		[Browsable(true), LocalDescription(typeof(Resources), "PROP_TABSTOP_DESC"), LocalCategory(typeof(Resources), "PROP_CATEGORY_DESIGN"), DefaultValue(true)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to show the focus rectangle when the control is focused.
		/// </summary>
		[Browsable(true), LocalDescription(typeof(Resources), "PROP_TABSTOP_DESC"), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), DefaultValue(true)]
		public bool ShowFocus
		{
			get
			{
				return _showFocus;
			}
			set
			{
				if (_showFocus == value)
				{
					return;
				}

				_showFocus = value;
				Invalidate();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Enter" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnEnter(EventArgs e)
		{
			Invalidate();
			base.OnEnter(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLeave(EventArgs e)
		{
			Invalidate();
			base.OnLeave(e);
		}

		/// <summary>
		/// Handles the <see cref="E:Resize" /> event.
		/// </summary>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			try
			{
				_resizing = true;
				Invalidate();
				base.OnResize(e);
			}
			finally
			{
				_resizing = false;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (((!Focused) && (!_resizing)) || (!ShowFocus))
			{
				return;
			}

			Rectangle newRect = ClientRectangle;
			newRect.Inflate(-2, -2);
			ControlPaint.DrawFocusRectangle(e.Graphics, newRect);
		}

	    /// <summary>
	    /// Function to process window messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
	    protected override void WndProc(ref Message m)
	    {
		    WindowMessages message = (WindowMessages)m.Msg;

		    switch (message)
		    {
				case WindowMessages.LeftButtonDown:
				case WindowMessages.RightButtonDown:
				case WindowMessages.MiddleButtonDown:
				    if (!Focused)
				    {
					    Focus();
				    }

				    break;
		    }

            // Turn off mouse wheel scrolling if auto scroll is on.
	        if ((message == WindowMessages.MouseWheel) && (AutoScroll) && (AutoScrollMinSize.Height > 0))
	        {
	            return;
	        }

	        base.WndProc(ref m);
	    }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSelectablePanel"/> class.
		/// </summary>
		public GorgonSelectablePanel()
		{
			SetStyle(ControlStyles.Selectable, true);
			TabStop = true;
		}
		#endregion
	}
}
