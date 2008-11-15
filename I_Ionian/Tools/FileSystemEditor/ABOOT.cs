#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Wednesday, April 25, 2007 3:29:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GorgonLibrary.FileSystems.Tools
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
				pictureIcon.Image = Properties.Resources.AppIconImage;
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