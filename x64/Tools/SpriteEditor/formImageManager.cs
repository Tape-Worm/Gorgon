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
// Created: Tuesday, May 08, 2007 2:00:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GorgonLibrary.Graphics.Tools.Controls;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for image previewing.
	/// </summary>
	public partial class formImageManager
		: Form
	{
		#region Variables.
		private ImageManager _imageManager = null;			// Image manager.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the image manager interface.
		/// </summary>
		public ImageManager ImageManager
		{
			get
			{
				return _imageManager;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formImageManager()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Reference to form main.</param>
		public formImageManager(formMain owner)
		{
			InitializeComponent();

			// Get main form.
			_imageManager = new ImageManager();
			_imageManager.Dock = DockStyle.Fill;
			Controls.Add(_imageManager);
			_imageManager.BindToMainForm(owner, null);
			_imageManager.RefreshList();
		}
		#endregion
	}
}