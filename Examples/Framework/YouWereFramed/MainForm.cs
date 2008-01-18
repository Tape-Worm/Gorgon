#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, October 26, 2007 11:55:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Mathematics;
using SharpUtilities.Utility;
using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Variables.
		private Random _rnd = new Random();					// Random number generator.
		private TextSprite _text = null;					// Text sprite.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			_text.Draw();
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseMovement(MouseInputEventArgs e)
		{
			base.OnMouseMovement(e);

			_text.Text = "Mouse X: " + e.Position.X.ToString("0.0") + " Y: " + e.Position.Y.ToString("0.0");
			_text.Position = e.Position + new Vector2D(6, 20);
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected override void OnDeviceLost()
		{
			base.OnDeviceLost();
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			_text = new TextSprite("MyText", "Framework O' fun.", FrameworkFont, Drawing.Color.Black);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\GorgonSettings.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}