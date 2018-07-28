﻿using System.ComponentModel;

namespace Gorgon.Examples
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.SuspendLayout();
            // 
            // ContentArea
            // 
            this.ContentArea.BackgroundImage = global::Gorgon.Examples.Properties.Resources.TileBg;
            this.ContentArea.Location = new System.Drawing.Point(1, 34);
            this.ContentArea.Size = new System.Drawing.Size(1278, 765);
            // 
            // FormMain
            // 
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowBorder = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Images";
            this.Theme.ContentPanelBackground = System.Drawing.SystemColors.Control;
            this.Theme.WindowBorderActive = System.Drawing.Color.SteelBlue;
            this.Theme.WindowBorderInactive = System.Drawing.SystemColors.ControlDarkDark;
            this.ResumeLayout(false);

        }

        #endregion
    }
}

