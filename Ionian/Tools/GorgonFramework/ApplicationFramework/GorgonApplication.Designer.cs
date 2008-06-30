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
// Created: Saturday, May 12, 2007 1:23:55 AM
// 
#endregion

namespace GorgonLibrary.Framework
{
	partial class GorgonApplicationWindow 
	{
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonApplicationWindow));
			this.ConfigurationSettings = new GorgonLibrary.Framework.GorgonSettings();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationSettings)).BeginInit();
			this.SuspendLayout();
			// 
			// ConfigurationSettings
			// 
			this.ConfigurationSettings.DataSetName = "GorgonSettings";
			this.ConfigurationSettings.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
			// 
			// GorgonApplicationWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(320, 240);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "GorgonApplicationWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationSettings)).EndInit();
			this.ResumeLayout(false);

		}

		/// <summary>
		/// Configuration settings dataset.
		/// </summary>
		protected GorgonLibrary.Framework.GorgonSettings ConfigurationSettings;
	}
}
