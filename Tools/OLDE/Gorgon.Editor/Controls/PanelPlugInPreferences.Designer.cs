#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, April 15, 2014 10:14:26 PM
// 
#endregion

using System.Collections.Generic;

namespace GorgonLibrary.Editor
{
    partial class PanelPlugInPreferences
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                if (_settingPanels != null)
                {
                    foreach (KeyValuePair<EditorPlugIn, PreferencePanel> panel in _settingPanels)
                    {
                        panel.Value.Dispose();
                    }

                    _settingPanels = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitPluginPrefs = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listPlugIns = new System.Windows.Forms.ListBox();
            this.labelPlugIns = new System.Windows.Forms.Label();
            this.panelPlugInPrefs = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitPluginPrefs)).BeginInit();
            this.splitPluginPrefs.Panel1.SuspendLayout();
            this.splitPluginPrefs.Panel2.SuspendLayout();
            this.splitPluginPrefs.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitPluginPrefs
            // 
            this.splitPluginPrefs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitPluginPrefs.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitPluginPrefs.IsSplitterFixed = true;
            this.splitPluginPrefs.Location = new System.Drawing.Point(3, 3);
            this.splitPluginPrefs.Name = "splitPluginPrefs";
            // 
            // splitPluginPrefs.Panel1
            // 
            this.splitPluginPrefs.Panel1.Controls.Add(this.panel1);
            this.splitPluginPrefs.Panel1.Controls.Add(this.labelPlugIns);
            // 
            // splitPluginPrefs.Panel2
            // 
            this.splitPluginPrefs.Panel2.Controls.Add(this.panelPlugInPrefs);
            this.splitPluginPrefs.Size = new System.Drawing.Size(585, 436);
            this.splitPluginPrefs.SplitterDistance = 205;
            this.splitPluginPrefs.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listPlugIns);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 18);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(205, 418);
            this.panel1.TabIndex = 1;
            // 
            // listPlugIns
            // 
            this.listPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listPlugIns.FormattingEnabled = true;
            this.listPlugIns.IntegralHeight = false;
            this.listPlugIns.ItemHeight = 15;
            this.listPlugIns.Location = new System.Drawing.Point(0, 0);
            this.listPlugIns.Name = "listPlugIns";
            this.listPlugIns.Size = new System.Drawing.Size(205, 418);
            this.listPlugIns.TabIndex = 1;
            this.listPlugIns.SelectedIndexChanged += new System.EventHandler(this.listPlugIns_SelectedIndexChanged);
            // 
            // labelPlugIns
            // 
            this.labelPlugIns.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelPlugIns.Location = new System.Drawing.Point(0, 0);
            this.labelPlugIns.Name = "labelPlugIns";
            this.labelPlugIns.Size = new System.Drawing.Size(205, 18);
            this.labelPlugIns.TabIndex = 0;
            this.labelPlugIns.Text = "plugins";
            // 
            // panelPlugInPrefs
            // 
            this.panelPlugInPrefs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPlugInPrefs.Location = new System.Drawing.Point(0, 0);
            this.panelPlugInPrefs.Name = "panelPlugInPrefs";
            this.panelPlugInPrefs.Size = new System.Drawing.Size(376, 436);
            this.panelPlugInPrefs.TabIndex = 1;
            // 
            // PanelPlugInPreferences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitPluginPrefs);
            this.Name = "PanelPlugInPreferences";
            this.splitPluginPrefs.Panel1.ResumeLayout(false);
            this.splitPluginPrefs.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPluginPrefs)).EndInit();
            this.splitPluginPrefs.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitPluginPrefs;
        private System.Windows.Forms.ListBox listPlugIns;
        private System.Windows.Forms.Label labelPlugIns;
        private System.Windows.Forms.Panel panelPlugInPrefs;
        private System.Windows.Forms.Panel panel1;
    }
}
