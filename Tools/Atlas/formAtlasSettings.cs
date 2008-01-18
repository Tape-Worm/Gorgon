#region LGPL.
// 
// Atlas.
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
// Created: Tuesday, November 13, 2007 1:33:20 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Atlas
{
	/// <summary>
	/// Interface to set the atlas settings.
	/// </summary>
	public partial class formAtlasSettings : Form
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the pixel padding around images.
		/// </summary>
		public int PixelPadding
		{
			get
			{
				return (int)numericPadding.Value;
			}
			set
			{
				numericPadding.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the atlas width.
		/// </summary>
		public int AtlasWidth
		{
			get
			{
				return (int)numericWidth.Value;
			}
			set
			{
				numericWidth.Value = value;
			}
		}

		/// <summary>
		/// Property to set or return the height of the atlas.
		/// </summary>
		public int AtlasHeight
		{
			get
			{
				return (int)numericHeight.Value;
			}
			set
			{
				numericHeight.Value = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AtlasSettings"/> class.
		/// </summary>
		public formAtlasSettings()
		{
			InitializeComponent();
		}
		#endregion
	}
}