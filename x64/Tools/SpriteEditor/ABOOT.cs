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
// Created: Wednesday, November 15, 2006 3:08:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Form for 'about' information.
	/// </summary>
	public partial class ABOOT : Form
	{
		#region Methods.
		/// <summary>
		/// Handles the LinkClicked event of the linkWeebsite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
		private void linkWeebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("http://www.tape-worm.net/");
			}
			finally
			{
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			Version assemblyVersion;								// Assembly version #.
			AssemblyCopyrightAttribute assemblyCopyright = null;	// Assembly copyright.
			AssemblyCompanyAttribute assemblyCompany = null;		// Assembly company.
			AssemblyDescriptionAttribute assemblyDescription = null;// Assembly description.

			base.OnLoad(e);

			if (!DesignMode)
			{
				Assembly fontAssembly = Assembly.GetAssembly(typeof(formMain));		// Font editor assembly.

				assemblyVersion = fontAssembly.GetName().Version;
				assemblyCopyright = (AssemblyCopyrightAttribute)fontAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
				assemblyCompany = (AssemblyCompanyAttribute)fontAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0];
				assemblyDescription = (AssemblyDescriptionAttribute)fontAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0];
				labelName.Text = assemblyDescription.Description;
				this.Text = "Aboot! " + assemblyDescription.Description;
				labelVersion.Text = "Version: " + assemblyVersion.ToString();
				labelCopyright.Text = assemblyCopyright.Copyright;
				labelWho.Text = "Developed by " + assemblyCompany.Company;
				pictureIcon.Image = Properties.Resources.GorSprite_64;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ABOOT()
		{
			InitializeComponent();
		}
		#endregion
	}
}