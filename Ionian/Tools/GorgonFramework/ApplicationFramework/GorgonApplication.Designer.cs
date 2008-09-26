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
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
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
